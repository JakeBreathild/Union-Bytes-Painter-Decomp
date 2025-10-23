using System;
using Godot;

public class SelectionManager : Node
{
	public enum ToolEnum
	{
		BRUSH,
		LASSO,
		RECTANGLE,
		ISLAND,
		WAND
	}

	public enum ShapeEnum
	{
		CIRCULAR,
		SQUARE
	}

	public enum FillModeEnum
	{
		OUTLINED,
		FILLED
	}

	public enum ModeEnum
	{
		ADD,
		REMOVE,
		REPLACE
	}

	public enum WandModeEnum
	{
		FLOODFILL,
		FULLFILL
	}

	private enum DirectionEnum
	{
		UP,
		RIGHT,
		DOWN,
		LEFT
	}

	private Workspace workspace;

	private Data worksheetData;

	private BakeManager bakeManager;

	private PreviewspaceMeshManager previewspaceMeshManager;

	private DrawingManager drawingManager;

	private bool enabled;

	protected string controls = "";

	private bool isEditingActivated;

	private Image maskImage;

	private ImageTexture maskImageTexture;

	private bool doMaskUpdate;

	private Vector2i maskUpdateStart = Vector2i.Maximum;

	private Vector2i maskUpdateEnd = Vector2i.Minimum;

	private MeshInstance meshInstance;

	private bool isSelecting;

	private Vector2i drawingStart = Vector2i.Zero;

	private Vector2i drawingEnd = Vector2i.Zero;

	private Vector2i drawingPositionLastFrame = Vector2i.NegOne;

	private ModeEnum mode = ModeEnum.REPLACE;

	private ToolEnum tool = ToolEnum.RECTANGLE;

	private FillModeEnum fillMode = FillModeEnum.FILLED;

	private ShapeEnum shape;

	private WandModeEnum wandMode;

	private Data.ChannelEnum channel = Data.ChannelEnum.COLOR;

	private int size = 1;

	private bool value = true;

	private float tolerance = 0.05f;

	public bool[,] currentSelectionArray;

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
			meshInstance.Visible = enabled;
		}
	}

	public string Controls
	{
		get
		{
			return controls;
		}
		set
		{
			controls = value;
		}
	}

	public bool IsEditingActivated
	{
		get
		{
			return isEditingActivated;
		}
		set
		{
			isEditingActivated = value;
		}
	}

	public ImageTexture MaskImageTexture => MaskImageTexture;

	public Vector2i MaskUpdateStart => maskUpdateStart;

	public Vector2i MaskUpdateEnd => maskUpdateEnd;

	public bool IsSelecting => isSelecting;

	public Vector2i DrawingStart => drawingStart;

	public Vector2i DrawingEnd => drawingEnd;

	public Vector2i DrawingPositionLastFrame => drawingPositionLastFrame;

	public bool HasDrawingPositionChanged
	{
		get
		{
			if (drawingStart.x == drawingEnd.x)
			{
				return drawingStart.y != drawingEnd.y;
			}
			return true;
		}
	}

	public bool HasDrawingPositionChangedSinceLastFrame
	{
		get
		{
			if (drawingPositionLastFrame.x == drawingEnd.x)
			{
				return drawingPositionLastFrame.y != drawingEnd.y;
			}
			return true;
		}
	}

	public ModeEnum Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
		}
	}

	public ToolEnum Tool
	{
		get
		{
			return tool;
		}
		set
		{
			tool = value;
		}
	}

	public FillModeEnum FillMode
	{
		get
		{
			return fillMode;
		}
		set
		{
			fillMode = value;
		}
	}

	public ShapeEnum Shape
	{
		get
		{
			return shape;
		}
		set
		{
			shape = value;
		}
	}

	public WandModeEnum WandMode
	{
		get
		{
			return wandMode;
		}
		set
		{
			wandMode = value;
		}
	}

	public Data.ChannelEnum Channel
	{
		get
		{
			return channel;
		}
		set
		{
			channel = value;
		}
	}

	public int Size
	{
		get
		{
			return size;
		}
		set
		{
			size = value;
		}
	}

	public bool Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public float Tolerance
	{
		get
		{
			return tolerance;
		}
		set
		{
			tolerance = value;
		}
	}

	public SelectionManager()
	{
		Register.SelectionManager = this;
	}

	public SelectionManager(Workspace workspace)
	{
		Register.SelectionManager = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "SelectionManager";
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SELECTION, "SELECTION_CLEAR", Clear, ButtonList.Right, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SELECTION, "SELECTION_DELETE", Delete, KeyList.Delete, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SELECTION, "SELECTION_COPY", Copy, KeyList.C, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SELECTION, "SELECTION_CUT", Cut, KeyList.X, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SELECTION, "SELECTION_COPY_MERGED", CopyMerged, KeyList.C, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: true, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SELECTION, "SELECTION_CUT_MERGED", CutMerged, KeyList.X, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: true, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SELECTION, "SELECTION_INVERT", Invert, KeyList.I, KeyBinding.EventTypeEnum.JUST_PRESSED);
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		bakeManager = Register.BakeManager;
		previewspaceMeshManager = Register.PreviewspaceMeshManager;
		drawingManager = Register.DrawingManager;
		meshInstance = new MeshInstance();
		AddChild(meshInstance);
		meshInstance.Owner = this;
		meshInstance.Name = "SelectionMeshInstance";
		Transform transform = meshInstance.GlobalTransform;
		transform.origin.y += Settings.OutlineOffset;
		meshInstance.GlobalTransform = transform;
		meshInstance.Visible = enabled;
		meshInstance.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.OUTLINE);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.OUTLINE, "width", 0.2f);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.OUTLINE, "frequency", 200f);
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (doMaskUpdate)
		{
			Update();
			doMaskUpdate = false;
		}
		if (enabled)
		{
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.OUTLINE, "scale", Register.CameraManager.GetWorkspaceCameraHeight() / 24f);
		}
	}

	public void Activate(bool activated)
	{
		isEditingActivated = activated;
	}

	public void Reset()
	{
		worksheetData = workspace.Worksheet.Data;
		currentSelectionArray = new bool[worksheetData.Width, worksheetData.Height];
		maskImage = new Image();
		maskImage.Create(worksheetData.Width, worksheetData.Height, useMipmaps: false, Image.Format.R8);
		maskImageTexture = new ImageTexture();
		maskImageTexture.CreateFromImage(maskImage, 18u);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.OUTLINE, "maskTexture", maskImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.OUTLINE, "pixelSize", new Vector2(1f / (float)worksheetData.Width, 1f / (float)worksheetData.Height));
		doMaskUpdate = false;
		maskUpdateStart = Vector2i.Maximum;
		maskUpdateEnd = Vector2i.Minimum;
		enabled = false;
		meshInstance.Visible = enabled;
		tool = ToolEnum.RECTANGLE;
		shape = ShapeEnum.CIRCULAR;
		fillMode = FillModeEnum.FILLED;
		mode = ModeEnum.REPLACE;
		size = 1;
		wandMode = WandModeEnum.FLOODFILL;
		channel = Data.ChannelEnum.COLOR;
		tolerance = 0.05f;
	}

	private int SetVerticalMirrorPixel(int x, int y, bool value)
	{
		int newX = Mathf.RoundToInt(Register.DrawingManager.VerticalMirrorPosition - ((float)x - Register.DrawingManager.VerticalMirrorPosition)) - 1;
		if (!worksheetData.Tileable && (newX < 0 || newX >= worksheetData.Width))
		{
			return x;
		}
		newX = Mathf.PosMod(newX, worksheetData.Width);
		currentSelectionArray[newX, y] = value;
		UpdatePosition(newX, y);
		return newX;
	}

	private int SetHorizontalMirrorPixel(int x, int y, bool value)
	{
		int newY = Mathf.RoundToInt(Register.DrawingManager.HorizontalMirrorPosition - ((float)y - Register.DrawingManager.HorizontalMirrorPosition)) - 1;
		if (!worksheetData.Tileable && (newY < 0 || newY >= worksheetData.Height))
		{
			return y;
		}
		newY = Mathf.PosMod(newY, worksheetData.Height);
		currentSelectionArray[x, newY] = value;
		UpdatePosition(x, newY);
		return newY;
	}

	private void ColorFloodFill(Worksheet worksheet, Color[,] array, int width, int height, Vector2i seed, Color seedColor, bool value, float tolerance, DirectionEnum direction)
	{
		Vector2i position = seed;
		switch (direction)
		{
		case DirectionEnum.UP:
			position.y--;
			while (position.y >= 0 && currentSelectionArray[position.x, position.y] != value && ColorExtension.ColorDistance(array[position.x, position.y], seedColor) <= tolerance)
			{
				SetPixel(position, value);
				position.y--;
			}
			position.y++;
			while (position.y < seed.y)
			{
				if (position.x > 0 && currentSelectionArray[position.x - 1, position.y] != value)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, value, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && currentSelectionArray[position.x + 1, position.y] != value)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, value, tolerance, DirectionEnum.RIGHT);
				}
				position.y++;
			}
			break;
		case DirectionEnum.RIGHT:
			position.x++;
			while (position.x < width && currentSelectionArray[position.x, position.y] != value && ColorExtension.ColorDistance(array[position.x, position.y], seedColor) <= tolerance)
			{
				SetPixel(position, value);
				position.x++;
			}
			position.x--;
			while (position.x > seed.x)
			{
				if (position.y > 0 && currentSelectionArray[position.x, position.y - 1] != value)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, value, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && currentSelectionArray[position.x, position.y + 1] != value)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, value, tolerance, DirectionEnum.DOWN);
				}
				position.x--;
			}
			break;
		case DirectionEnum.DOWN:
			position.y++;
			while (position.y < height && currentSelectionArray[position.x, position.y] != value && ColorExtension.ColorDistance(array[position.x, position.y], seedColor) <= tolerance)
			{
				SetPixel(position, value);
				position.y++;
			}
			position.y--;
			while (position.y > seed.y)
			{
				if (position.x > 0 && currentSelectionArray[position.x - 1, position.y] != value)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, value, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && currentSelectionArray[position.x + 1, position.y] != value)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, value, tolerance, DirectionEnum.RIGHT);
				}
				position.y--;
			}
			break;
		case DirectionEnum.LEFT:
			position.x--;
			while (position.x >= 0 && currentSelectionArray[position.x, position.y] != value && ColorExtension.ColorDistance(array[position.x, position.y], seedColor) <= tolerance)
			{
				SetPixel(position, value);
				position.x--;
			}
			position.x++;
			while (position.x < seed.x)
			{
				if (position.y > 0 && currentSelectionArray[position.x, position.y - 1] != value)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, value, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && currentSelectionArray[position.x, position.y + 1] != value)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, value, tolerance, DirectionEnum.DOWN);
				}
				position.x++;
			}
			break;
		}
	}

	private void ValueFloodFill(Worksheet worksheet, Value[,] array, int width, int height, Vector2i seed, Value seedValue, bool value, float tolerance, DirectionEnum direction)
	{
		Vector2i position = seed;
		switch (direction)
		{
		case DirectionEnum.UP:
			position.y--;
			while (position.y >= 0 && currentSelectionArray[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y].v - seedValue.v) <= tolerance)
			{
				SetPixel(position, value);
				position.y--;
			}
			position.y++;
			while (position.y < seed.y)
			{
				if (position.x > 0 && currentSelectionArray[position.x - 1, position.y] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && currentSelectionArray[position.x + 1, position.y] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.RIGHT);
				}
				position.y++;
			}
			break;
		case DirectionEnum.RIGHT:
			position.x++;
			while (position.x < width && currentSelectionArray[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y].v - seedValue.v) <= tolerance)
			{
				SetPixel(position, value);
				position.x++;
			}
			position.x--;
			while (position.x > seed.x)
			{
				if (position.y > 0 && currentSelectionArray[position.x, position.y - 1] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && currentSelectionArray[position.x, position.y + 1] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.DOWN);
				}
				position.x--;
			}
			break;
		case DirectionEnum.DOWN:
			position.y++;
			while (position.y < height && currentSelectionArray[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y].v - seedValue.v) <= tolerance)
			{
				SetPixel(position, value);
				position.y++;
			}
			position.y--;
			while (position.y > seed.y)
			{
				if (position.x > 0 && currentSelectionArray[position.x - 1, position.y] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && currentSelectionArray[position.x + 1, position.y] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.RIGHT);
				}
				position.y--;
			}
			break;
		case DirectionEnum.LEFT:
			position.x--;
			while (position.x >= 0 && currentSelectionArray[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y].v - seedValue.v) <= tolerance)
			{
				SetPixel(position, value);
				position.x--;
			}
			position.x++;
			while (position.x < seed.x)
			{
				if (position.y > 0 && currentSelectionArray[position.x, position.y - 1] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && currentSelectionArray[position.x, position.y + 1] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.DOWN);
				}
				position.x++;
			}
			break;
		}
	}

	private void FloatFloodFill(Worksheet worksheet, float[,] array, int width, int height, Vector2i seed, float seedValue, bool value, float tolerance, DirectionEnum direction)
	{
		Vector2i position = seed;
		switch (direction)
		{
		case DirectionEnum.UP:
			position.y--;
			while (position.y >= 0 && currentSelectionArray[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y] - seedValue) <= tolerance)
			{
				SetPixel(position, value);
				position.y--;
			}
			position.y++;
			while (position.y < seed.y)
			{
				if (position.x > 0 && currentSelectionArray[position.x - 1, position.y] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && currentSelectionArray[position.x + 1, position.y] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.RIGHT);
				}
				position.y++;
			}
			break;
		case DirectionEnum.RIGHT:
			position.x++;
			while (position.x < width && currentSelectionArray[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y] - seedValue) <= tolerance)
			{
				SetPixel(position, value);
				position.x++;
			}
			position.x--;
			while (position.x > seed.x)
			{
				if (position.y > 0 && currentSelectionArray[position.x, position.y - 1] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && currentSelectionArray[position.x, position.y + 1] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.DOWN);
				}
				position.x--;
			}
			break;
		case DirectionEnum.DOWN:
			position.y++;
			while (position.y < height && currentSelectionArray[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y] - seedValue) <= tolerance)
			{
				SetPixel(position, value);
				position.y++;
			}
			position.y--;
			while (position.y > seed.y)
			{
				if (position.x > 0 && currentSelectionArray[position.x - 1, position.y] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && currentSelectionArray[position.x + 1, position.y] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.RIGHT);
				}
				position.y--;
			}
			break;
		case DirectionEnum.LEFT:
			position.x--;
			while (position.x >= 0 && currentSelectionArray[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y] - seedValue) <= tolerance)
			{
				SetPixel(position, value);
				position.x--;
			}
			position.x++;
			while (position.x < seed.x)
			{
				if (position.y > 0 && currentSelectionArray[position.x, position.y - 1] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && currentSelectionArray[position.x, position.y + 1] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.DOWN);
				}
				position.x++;
			}
			break;
		}
	}

	public void FloodFill(Worksheet worksheet, Data.ChannelEnum channel, Vector2i seed, bool value, float tolerance)
	{
		for (int y = 0; y < worksheetData.Height; y++)
		{
			for (int x = 0; x < worksheetData.Width; x++)
			{
				currentSelectionArray[x, y] = false;
			}
		}
		seed.x = Mathf.PosMod(seed.x, worksheet.Data.Width);
		seed.y = Mathf.PosMod(seed.y, worksheet.Data.Height);
		int width = worksheet.Data.Width;
		int height = worksheet.Data.Height;
		switch (channel)
		{
		case Data.ChannelEnum.FULL:
		case Data.ChannelEnum.COLOR:
		{
			Color[,] colorArray = worksheet.Data.ColorChannel.Array;
			Color seedColor = colorArray[seed.x, seed.y];
			for (DirectionEnum d2 = DirectionEnum.UP; d2 <= DirectionEnum.LEFT; d2++)
			{
				ColorFloodFill(worksheet, colorArray, width, height, seed, seedColor, value, tolerance, d2);
			}
			break;
		}
		case Data.ChannelEnum.EMISSION:
		{
			Color[,] colorArray = worksheet.Data.EmissionChannel.Array;
			Color seedColor = colorArray[seed.x, seed.y];
			for (DirectionEnum d4 = DirectionEnum.UP; d4 <= DirectionEnum.LEFT; d4++)
			{
				ColorFloodFill(worksheet, colorArray, width, height, seed, seedColor, value, tolerance, d4);
			}
			break;
		}
		case Data.ChannelEnum.HEIGHT:
		case Data.ChannelEnum.NORMAL:
		{
			Value[,] floatArray = worksheet.Data.HeightChannel.Array;
			Value seedValue = floatArray[seed.x, seed.y];
			for (DirectionEnum d5 = DirectionEnum.UP; d5 <= DirectionEnum.LEFT; d5++)
			{
				ValueFloodFill(worksheet, floatArray, width, height, seed, seedValue, value, tolerance, d5);
			}
			break;
		}
		case Data.ChannelEnum.ROUGHNESS:
		{
			Value[,] floatArray = worksheet.Data.RoughnessChannel.Array;
			Value seedValue = floatArray[seed.x, seed.y];
			for (DirectionEnum d3 = DirectionEnum.UP; d3 <= DirectionEnum.LEFT; d3++)
			{
				ValueFloodFill(worksheet, floatArray, width, height, seed, seedValue, value, tolerance, d3);
			}
			break;
		}
		case Data.ChannelEnum.METALLICITY:
		{
			Value[,] floatArray = worksheet.Data.MetallicityChannel.Array;
			Value seedValue = floatArray[seed.x, seed.y];
			for (DirectionEnum d = DirectionEnum.UP; d <= DirectionEnum.LEFT; d++)
			{
				ValueFloodFill(worksheet, floatArray, width, height, seed, seedValue, value, tolerance, d);
			}
			break;
		}
		}
		SetPixel(seed, value);
		if (mode == ModeEnum.REPLACE)
		{
			worksheetData.SelectionArray = (bool[,])currentSelectionArray.Clone();
		}
		else
		{
			for (int i = maskUpdateStart.y; i <= maskUpdateEnd.y; i++)
			{
				for (int j = maskUpdateStart.x; j <= maskUpdateEnd.x; j++)
				{
					switch (mode)
					{
					case ModeEnum.ADD:
						worksheetData.SelectionArray[j, i] = worksheetData.SelectionArray[j, i] || currentSelectionArray[j, i];
						break;
					case ModeEnum.REMOVE:
						if (currentSelectionArray[j, i])
						{
							worksheetData.SelectionArray[j, i] = false;
						}
						break;
					}
					currentSelectionArray[j, i] = false;
				}
			}
		}
		Update();
	}

	private void ColorFullFill(Worksheet worksheet, Color[,] array, int width, int height, Color seedColor, bool value, float tolerance)
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (currentSelectionArray[x, y] != value && ColorExtension.ColorDistance(array[x, y], seedColor) <= tolerance)
				{
					SetPixel(x, y, value);
				}
			}
		}
	}

	private void ValueFullFill(Worksheet worksheet, Value[,] array, int width, int height, Value seedValue, bool value, float tolerance)
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (currentSelectionArray[x, y] != value && Mathf.Abs(array[x, y].v - seedValue.v) <= tolerance)
				{
					SetPixel(x, y, value);
				}
			}
		}
	}

	private void FloatFullFill(Worksheet worksheet, float[,] array, int width, int height, float seedValue, bool value, float tolerance)
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (currentSelectionArray[x, y] != value && Mathf.Abs(array[x, y] - seedValue) <= tolerance)
				{
					SetPixel(x, y, value);
				}
			}
		}
	}

	public void FullFill(Worksheet worksheet, Data.ChannelEnum channel, Vector2i seed, bool value, float tolerance)
	{
		for (int y = 0; y < worksheetData.Height; y++)
		{
			for (int x = 0; x < worksheetData.Width; x++)
			{
				currentSelectionArray[x, y] = false;
			}
		}
		seed.x = Mathf.PosMod(seed.x, worksheet.Data.Width);
		seed.y = Mathf.PosMod(seed.y, worksheet.Data.Height);
		int width = worksheet.Data.Width;
		int height = worksheet.Data.Height;
		switch (channel)
		{
		case Data.ChannelEnum.FULL:
		case Data.ChannelEnum.COLOR:
		{
			Color[,] colorArray = worksheet.Data.ColorChannel.Array;
			Color seedColor = colorArray[seed.x, seed.y];
			ColorFullFill(worksheet, colorArray, width, height, seedColor, value, tolerance);
			break;
		}
		case Data.ChannelEnum.EMISSION:
		{
			Color[,] colorArray = worksheet.Data.EmissionChannel.Array;
			Color seedColor = colorArray[seed.x, seed.y];
			ColorFullFill(worksheet, colorArray, width, height, seedColor, value, tolerance);
			break;
		}
		case Data.ChannelEnum.HEIGHT:
		case Data.ChannelEnum.NORMAL:
		{
			Value[,] floatArray = worksheet.Data.HeightChannel.Array;
			Value seedValue = floatArray[seed.x, seed.y];
			ValueFullFill(worksheet, floatArray, width, height, seedValue, value, tolerance);
			break;
		}
		case Data.ChannelEnum.ROUGHNESS:
		{
			Value[,] floatArray = worksheet.Data.RoughnessChannel.Array;
			Value seedValue = floatArray[seed.x, seed.y];
			ValueFullFill(worksheet, floatArray, width, height, seedValue, value, tolerance);
			break;
		}
		case Data.ChannelEnum.METALLICITY:
		{
			Value[,] floatArray = worksheet.Data.MetallicityChannel.Array;
			Value seedValue = floatArray[seed.x, seed.y];
			ValueFullFill(worksheet, floatArray, width, height, seedValue, value, tolerance);
			break;
		}
		}
		if (mode == ModeEnum.REPLACE)
		{
			worksheetData.SelectionArray = (bool[,])currentSelectionArray.Clone();
		}
		else
		{
			for (int i = maskUpdateStart.y; i <= maskUpdateEnd.y; i++)
			{
				for (int j = maskUpdateStart.x; j <= maskUpdateEnd.x; j++)
				{
					switch (mode)
					{
					case ModeEnum.ADD:
						worksheetData.SelectionArray[j, i] = worksheetData.SelectionArray[j, i] || currentSelectionArray[j, i];
						break;
					case ModeEnum.REMOVE:
						if (currentSelectionArray[j, i])
						{
							worksheetData.SelectionArray[j, i] = false;
						}
						break;
					}
					currentSelectionArray[j, i] = false;
				}
			}
		}
		Update();
	}

	private bool CalculateSpot(float x, float y, int size, ShapeEnum shape)
	{
		float halfSize = 0.5f * (float)size;
		float hardness = 1f;
		x += 0.5f;
		y += 0.5f;
		float deltaX = Mathf.Abs(x - halfSize);
		float deltaY = Mathf.Abs(y - halfSize);
		switch (shape)
		{
		case ShapeEnum.CIRCULAR:
		{
			float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
			if (distance > halfSize)
			{
				return false;
			}
			_ = hardness * halfSize;
			return true;
		}
		case ShapeEnum.SQUARE:
			if (deltaX > halfSize || deltaY > halfSize)
			{
				return false;
			}
			return true;
		default:
			return true;
		}
	}

	public void SetPixel(int x, int y, bool value)
	{
		if (!worksheetData.Tileable && (x < 0 || x >= worksheetData.Width || y < 0 || y >= worksheetData.Height))
		{
			return;
		}
		x = Mathf.PosMod(x, worksheetData.Width);
		y = Mathf.PosMod(y, worksheetData.Height);
		currentSelectionArray[x, y] = value;
		UpdatePosition(x, y);
		if (!Register.DrawingManager.Mirroring)
		{
			return;
		}
		if (Register.DrawingManager.VerticalMirroring)
		{
			SetVerticalMirrorPixel(x, y, value);
		}
		if (Register.DrawingManager.HorizontalMirroring)
		{
			int newY = SetHorizontalMirrorPixel(x, y, value);
			if (Register.DrawingManager.VerticalMirroring)
			{
				SetVerticalMirrorPixel(x, newY, value);
			}
		}
	}

	public void SetPixel(Vector2i position, bool value)
	{
		SetPixel(position.x, position.y, value);
	}

	public void SetSpot(int x, int y, bool value)
	{
		SetSpot(new Vector2i(x, y), value);
	}

	public void SetSpot(Vector2i position, bool value)
	{
		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				int xx = position.x + x - Mathf.CeilToInt((float)size * 0.5f) + 1;
				int yy = position.y + y - Mathf.CeilToInt((float)size * 0.5f) + 1;
				if (CalculateSpot(x, y, size, shape))
				{
					SetPixel(xx, yy, value);
				}
			}
		}
	}

	public void SetStroke(int startX, int startY, int endX, int endY, bool value)
	{
		SetStroke(new Vector2i(startX, startY), new Vector2i(endX, endY), value);
	}

	public void SetStroke(Vector2i start, Vector2i end, bool value)
	{
		float halfSize = 0.5f * (float)size;
		float hardness = 1f;
		Vector2 delta = end.ToFloat() - start.ToFloat();
		Vector2 normal = new Vector2(delta.y, 0f - delta.x).Normalized();
		Vector2i lefttop = new Vector2i(Math.Min(start.x, end.x) - size / 2, Math.Min(start.y, end.y) - size / 2);
		Vector2i rightbottom = new Vector2i(Math.Max(start.x, end.x) + size / 2, Math.Max(start.y, end.y) + size / 2);
		SetSpot(start, value);
		for (int y = lefttop.y; y <= rightbottom.y; y++)
		{
			for (int x = lefttop.x; x <= rightbottom.x; x++)
			{
				float t = ((float)(x - start.x) * delta.x + (float)(y - start.y) * delta.y) / delta.Dot(delta);
				if (0f <= t && t <= 1f)
				{
					float distance = Mathf.Abs((float)(x - start.x) * normal.x + (float)(y - start.y) * normal.y);
					if (distance <= hardness * halfSize)
					{
						SetPixel(x, y, value);
					}
					else if (distance <= halfSize)
					{
						SetPixel(x, y, value);
					}
				}
			}
		}
		SetSpot(end, value);
	}

	public void SetRectangle(int startX, int startY, int endX, int endY, FillModeEnum mode, bool value)
	{
		SetRectangle(new Vector2i(startX, startY), new Vector2i(endX, endY), mode, value);
	}

	public void SetRectangle(Vector2i start, Vector2i end, FillModeEnum mode, bool value)
	{
		if (end.x < start.x)
		{
			ref int x = ref start.x;
			ref int x2 = ref end.x;
			int x3 = end.x;
			int x4 = start.x;
			x = x3;
			x2 = x4;
		}
		if (end.y < start.y)
		{
			ref int x = ref start.y;
			ref int y = ref end.y;
			int x4 = end.y;
			int x3 = start.y;
			x = x4;
			y = x3;
		}
		SetStroke(start.x, start.y, end.x, start.y, value);
		SetStroke(start.x, end.y, end.x, end.y, value);
		SetStroke(start.x, start.y, start.x, end.y, value);
		SetStroke(end.x, start.y, end.x, end.y, value);
		if (mode != FillModeEnum.FILLED)
		{
			return;
		}
		for (int i = start.y; i <= end.y; i++)
		{
			for (int j = start.x; j <= end.x; j++)
			{
				SetPixel(j, i, value);
			}
		}
	}

	public void Invert()
	{
		if (isSelecting)
		{
			return;
		}
		for (int y = 0; y < worksheetData.Height; y++)
		{
			for (int x = 0; x < worksheetData.Width; x++)
			{
				currentSelectionArray[x, y] = false;
				worksheetData.SelectionArray[x, y] = !worksheetData.SelectionArray[x, y];
			}
		}
		maskUpdateStart = new Vector2i(0, 0);
		maskUpdateEnd = new Vector2i(worksheetData.Width - 1, worksheetData.Height - 1);
		Update();
	}

	private void SelectingIsland(int group)
	{
		if (!previewspaceMeshManager.IsMeshLoaded)
		{
			return;
		}
		for (int y = 0; y < worksheetData.Height; y++)
		{
			for (int x = 0; x < worksheetData.Width; x++)
			{
				if (bakeManager.PixelAffiliations[x, y].TrianglesIndicesList == null)
				{
					continue;
				}
				foreach (int t in bakeManager.PixelAffiliations[x, y].TrianglesIndicesList)
				{
					int ti0 = previewspaceMeshManager.CollisionTrianglesList[t].Indices[0];
					if (previewspaceMeshManager.GroupList[ti0] == group)
					{
						int ti1 = previewspaceMeshManager.CollisionTrianglesList[t].Indices[1];
						int ti2 = previewspaceMeshManager.CollisionTrianglesList[t].Indices[2];
						if (bakeManager.ConservativeTriangleProbe(x, y, previewspaceMeshManager.UVsList[ti0], previewspaceMeshManager.UVsList[ti1], previewspaceMeshManager.UVsList[ti2], out var _, out var _, out var _, 0.5f))
						{
							SetPixel(x, y, value: true);
						}
					}
				}
			}
		}
		for (int i = 0; i < worksheetData.Height; i++)
		{
			for (int j = 0; j < worksheetData.Width; j++)
			{
				if (currentSelectionArray[j, i])
				{
					continue;
				}
				if (j > 0 && Register.BakeManager.PixelAffiliations[j - 1, i].TrianglesIndicesList != null)
				{
					foreach (int t2 in bakeManager.PixelAffiliations[j - 1, i].TrianglesIndicesList)
					{
						int ti3 = previewspaceMeshManager.CollisionTrianglesList[t2].Indices[0];
						if (previewspaceMeshManager.GroupList[ti3] == group)
						{
							SetPixel(j, i, value: true);
						}
					}
				}
				else if (j < worksheetData.Width - 1 && Register.BakeManager.PixelAffiliations[j + 1, i].TrianglesIndicesList != null)
				{
					foreach (int t3 in bakeManager.PixelAffiliations[j + 1, i].TrianglesIndicesList)
					{
						int ti4 = previewspaceMeshManager.CollisionTrianglesList[t3].Indices[0];
						if (previewspaceMeshManager.GroupList[ti4] == group)
						{
							SetPixel(j, i, value: true);
						}
					}
				}
				else if (i > 0 && Register.BakeManager.PixelAffiliations[j, i - 1].TrianglesIndicesList != null)
				{
					foreach (int t4 in bakeManager.PixelAffiliations[j, i - 1].TrianglesIndicesList)
					{
						int ti5 = previewspaceMeshManager.CollisionTrianglesList[t4].Indices[0];
						if (previewspaceMeshManager.GroupList[ti5] == group)
						{
							SetPixel(j, i, value: true);
						}
					}
				}
				else if (i < worksheetData.Height - 1 && Register.BakeManager.PixelAffiliations[j, i + 1].TrianglesIndicesList != null)
				{
					foreach (int t5 in bakeManager.PixelAffiliations[j, i + 1].TrianglesIndicesList)
					{
						int ti6 = previewspaceMeshManager.CollisionTrianglesList[t5].Indices[0];
						if (previewspaceMeshManager.GroupList[ti6] == group)
						{
							SetPixel(j, i, value: true);
						}
					}
				}
				else if (j > 0 && i > 0 && Register.BakeManager.PixelAffiliations[j - 1, i - 1].TrianglesIndicesList != null)
				{
					foreach (int t6 in bakeManager.PixelAffiliations[j - 1, i - 1].TrianglesIndicesList)
					{
						int ti7 = previewspaceMeshManager.CollisionTrianglesList[t6].Indices[0];
						if (previewspaceMeshManager.GroupList[ti7] == group)
						{
							SetPixel(j, i, value: true);
						}
					}
				}
				else if (j < worksheetData.Width - 1 && i > 0 && Register.BakeManager.PixelAffiliations[j + 1, i - 1].TrianglesIndicesList != null)
				{
					foreach (int t7 in bakeManager.PixelAffiliations[j + 1, i - 1].TrianglesIndicesList)
					{
						int ti8 = previewspaceMeshManager.CollisionTrianglesList[t7].Indices[0];
						if (previewspaceMeshManager.GroupList[ti8] == group)
						{
							SetPixel(j, i, value: true);
						}
					}
				}
				else if (j > 0 && i < worksheetData.Height - 1 && Register.BakeManager.PixelAffiliations[j - 1, i + 1].TrianglesIndicesList != null)
				{
					foreach (int t8 in bakeManager.PixelAffiliations[j - 1, i + 1].TrianglesIndicesList)
					{
						int ti9 = previewspaceMeshManager.CollisionTrianglesList[t8].Indices[0];
						if (previewspaceMeshManager.GroupList[ti9] == group)
						{
							SetPixel(j, i, value: true);
						}
					}
				}
				else
				{
					if (j >= worksheetData.Width - 1 || i >= worksheetData.Height - 1 || Register.BakeManager.PixelAffiliations[j + 1, i + 1].TrianglesIndicesList == null)
					{
						continue;
					}
					foreach (int t9 in bakeManager.PixelAffiliations[j + 1, i + 1].TrianglesIndicesList)
					{
						int ti10 = previewspaceMeshManager.CollisionTrianglesList[t9].Indices[0];
						if (previewspaceMeshManager.GroupList[ti10] == group)
						{
							SetPixel(j, i, value: true);
						}
					}
				}
			}
		}
	}

	public void Clear()
	{
		Color color = new Color(0f, 0f, 0f, 0f);
		maskImage.Lock();
		for (int y = 0; y < worksheetData.Height; y++)
		{
			for (int x = 0; x < worksheetData.Width; x++)
			{
				worksheetData.SelectionArray[x, y] = false;
				currentSelectionArray[x, y] = false;
				maskImage.SetPixel(x, y, color);
			}
		}
		maskImage.Unlock();
		VisualServer.TextureSetData(maskImageTexture.GetRid(), maskImage);
		maskUpdateStart = Vector2i.Maximum;
		maskUpdateEnd = Vector2i.Minimum;
		enabled = false;
		meshInstance.Visible = enabled;
	}

	public void Delete()
	{
		drawingManager = Register.DrawingManager;
		drawingManager.PushSettings();
		drawingManager.Tool = DrawingManager.ToolEnum.BRUSH;
		drawingManager.BlendingMode = Blender.BlendingModeEnum.REPLACE;
		drawingManager.Color = workspace.Worksheet.Data.Layer.ColorChannel.DefaultValue;
		drawingManager.Roughness = workspace.Worksheet.Data.Layer.RoughnessChannel.DefaultValue;
		drawingManager.Metallicity = workspace.Worksheet.Data.Layer.MetallicityChannel.DefaultValue;
		drawingManager.Height = workspace.Worksheet.Data.Layer.HeightChannel.DefaultValue;
		drawingManager.Emission = workspace.Worksheet.Data.Layer.EmissionChannel.DefaultValue;
		drawingManager.StartDrawing(workspace.Worksheet, Vector2i.NegOne);
		for (int y = 0; y < workspace.Worksheet.Data.Height; y++)
		{
			for (int x = 0; x < workspace.Worksheet.Data.Width; x++)
			{
				if (workspace.Worksheet.Data.SelectionArray[x, y])
				{
					drawingManager.DrawPixel(workspace.Worksheet, x, y);
				}
			}
		}
		drawingManager.StopDrawing(workspace.Worksheet, Vector2i.NegOne, "Selection Deleted: " + workspace.Worksheet.Data.Layer.Name);
		Register.DrawingManager.Tool = DrawingManager.ToolEnum.DISABLED;
		drawingManager.PopSettings();
		if (drawingManager.ColorEnabled)
		{
			workspace.Worksheet.Data.Layer.ColorChannel.DetectContentArea();
		}
		if (drawingManager.RoughnessEnabled)
		{
			workspace.Worksheet.Data.Layer.RoughnessChannel.DetectContentArea();
		}
		if (drawingManager.MetallicityEnabled)
		{
			workspace.Worksheet.Data.Layer.MetallicityChannel.DetectContentArea();
		}
		if (drawingManager.HeightEnabled)
		{
			workspace.Worksheet.Data.Layer.HeightChannel.DetectContentArea();
		}
		if (drawingManager.EmissionEnabled)
		{
			workspace.Worksheet.Data.Layer.EmissionChannel.DetectContentArea();
		}
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(workspace.Worksheet);
		}
		Register.LayerControl.Reset();
	}

	public void Copy(bool doCut)
	{
		drawingManager = Register.DrawingManager;
		Vector2i selectionArrayStart = Vector2i.Maximum;
		Vector2i selectionArrayEnd = Vector2i.Minimum;
		for (int y = 0; y < workspace.Worksheet.Data.Height; y++)
		{
			for (int x = 0; x < workspace.Worksheet.Data.Width; x++)
			{
				if (workspace.Worksheet.Data.SelectionArray[x, y])
				{
					if (x < selectionArrayStart.x)
					{
						selectionArrayStart.x = x;
					}
					if (x > selectionArrayEnd.x)
					{
						selectionArrayEnd.x = x;
					}
					if (y < selectionArrayStart.y)
					{
						selectionArrayStart.y = y;
					}
					if (y > selectionArrayEnd.y)
					{
						selectionArrayEnd.y = y;
					}
				}
			}
		}
		if (!(selectionArrayStart != Vector2i.Maximum))
		{
			return;
		}
		Vector2i selectionArraySize = selectionArrayEnd - selectionArrayStart + Vector2i.One;
		Decal decal = new Decal();
		decal.Name = "Copy: " + workspace.Worksheet.Data.Layer.Name + " [" + selectionArrayStart.x + "," + selectionArrayStart.y + "] -> [" + selectionArrayEnd.x + "," + selectionArrayEnd.y + "]";
		decal.Width = selectionArraySize.x;
		decal.Height = selectionArraySize.y;
		decal.ArraySize = ((decal.Width >= decal.Height) ? decal.Width : decal.Height);
		decal.Thumbnail = new Image();
		decal.Thumbnail.Create(decal.ArraySize, decal.ArraySize, useMipmaps: false, Image.Format.Rgba8);
		decal.ColorArray = new Color[decal.ArraySize, decal.ArraySize];
		decal.RoughnessArray = new Value[decal.ArraySize, decal.ArraySize];
		decal.MetallicityArray = new Value[decal.ArraySize, decal.ArraySize];
		decal.HeightArray = new Value[decal.ArraySize, decal.ArraySize];
		decal.EmissionArray = new Color[decal.ArraySize, decal.ArraySize];
		if (doCut)
		{
			drawingManager.PushSettings();
			drawingManager.Tool = DrawingManager.ToolEnum.BRUSH;
			drawingManager.BlendingMode = Blender.BlendingModeEnum.REPLACE;
			drawingManager.Color = workspace.Worksheet.Data.Layer.ColorChannel.DefaultValue;
			drawingManager.Roughness = workspace.Worksheet.Data.Layer.RoughnessChannel.DefaultValue;
			drawingManager.Metallicity = workspace.Worksheet.Data.Layer.MetallicityChannel.DefaultValue;
			drawingManager.Height = workspace.Worksheet.Data.Layer.HeightChannel.DefaultValue;
			drawingManager.Emission = workspace.Worksheet.Data.Layer.EmissionChannel.DefaultValue;
			drawingManager.ColorEnabled = true;
			drawingManager.RoughnessEnabled = true;
			drawingManager.MetallicityEnabled = true;
			drawingManager.HeightEnabled = true;
			drawingManager.EmissionEnabled = true;
			drawingManager.StartDrawing(workspace.Worksheet, Vector2i.NegOne);
		}
		decal.Thumbnail.Lock();
		for (int i = selectionArrayStart.y; i <= selectionArrayEnd.y; i++)
		{
			for (int j = selectionArrayStart.x; j <= selectionArrayEnd.x; j++)
			{
				if (workspace.Worksheet.Data.SelectionArray[j, i])
				{
					int arrayX = j - selectionArrayStart.x + (decal.ArraySize - decal.Width) / 2;
					int arrayY = i - selectionArrayStart.y + (decal.ArraySize - decal.Height) / 2;
					decal.ColorArray[arrayX, arrayY] = workspace.Worksheet.Data.Layer.ColorChannel[j, i];
					decal.RoughnessArray[arrayX, arrayY] = workspace.Worksheet.Data.Layer.RoughnessChannel[j, i];
					decal.MetallicityArray[arrayX, arrayY] = workspace.Worksheet.Data.Layer.MetallicityChannel[j, i];
					decal.HeightArray[arrayX, arrayY] = workspace.Worksheet.Data.Layer.HeightChannel[j, i];
					decal.EmissionArray[arrayX, arrayY] = workspace.Worksheet.Data.Layer.EmissionChannel[j, i];
					decal.Thumbnail.SetPixel(arrayX, arrayY, decal.ColorArray[arrayX, arrayY]);
					if (doCut)
					{
						drawingManager.DrawPixel(workspace.Worksheet, j, i);
					}
				}
			}
		}
		decal.Thumbnail.Unlock();
		decal.Thumbnail.Resize(Settings.ThumbnailSize, Settings.ThumbnailSize, Image.Interpolation.Nearest);
		if (doCut)
		{
			drawingManager.StopDrawing(workspace.Worksheet, Vector2i.NegOne, "Selection Cutted: " + workspace.Worksheet.Data.Layer.Name);
			workspace.Worksheet.Data.CombineLayersFromAllChannels();
			workspace.Worksheet.Data.Layer.ColorChannel.DetectContentArea();
			workspace.Worksheet.Data.Layer.RoughnessChannel.DetectContentArea();
			workspace.Worksheet.Data.Layer.MetallicityChannel.DetectContentArea();
			workspace.Worksheet.Data.Layer.HeightChannel.DetectContentArea();
			workspace.Worksheet.Data.Layer.EmissionChannel.DetectContentArea();
			if (Register.GridManager.DoShowLayerContentAreas)
			{
				Register.GridManager.Update(workspace.Worksheet);
			}
			Register.LayerControl.Reset();
			drawingManager.PopSettings();
		}
		decal.CreateMaskArrayFromColorArray(decal.ColorArray, squared: true);
		decal.IsLoaded = true;
		drawingManager.StampTool.Decal = decal;
		drawingManager.StampTool.Mode = DrawingToolStamp.ModeEnum.DECAL;
		Clear();
		Register.Gui.ToolsContainer.PressToolButton(7);
	}

	public void Copy()
	{
		Copy(doCut: false);
	}

	public void Cut()
	{
		Copy(doCut: true);
	}

	public void CopyMerged(bool doCut)
	{
		drawingManager = Register.DrawingManager;
		Vector2i selectionArrayStart = Vector2i.Maximum;
		Vector2i selectionArrayEnd = Vector2i.Minimum;
		for (int y = 0; y < workspace.Worksheet.Data.Height; y++)
		{
			for (int x = 0; x < workspace.Worksheet.Data.Width; x++)
			{
				if (workspace.Worksheet.Data.SelectionArray[x, y])
				{
					if (x < selectionArrayStart.x)
					{
						selectionArrayStart.x = x;
					}
					if (x > selectionArrayEnd.x)
					{
						selectionArrayEnd.x = x;
					}
					if (y < selectionArrayStart.y)
					{
						selectionArrayStart.y = y;
					}
					if (y > selectionArrayEnd.y)
					{
						selectionArrayEnd.y = y;
					}
				}
			}
		}
		if (!(selectionArrayStart != Vector2i.Maximum))
		{
			return;
		}
		Vector2i selectionArraySize = selectionArrayEnd - selectionArrayStart + Vector2i.One;
		Decal decal = new Decal();
		decal.Name = "Copy: All Layer [" + selectionArrayStart.x + "," + selectionArrayStart.y + "] -> [" + selectionArrayEnd.x + "," + selectionArrayEnd.y + "]";
		decal.Width = selectionArraySize.x;
		decal.Height = selectionArraySize.y;
		decal.ArraySize = ((decal.Width >= decal.Height) ? decal.Width : decal.Height);
		decal.Thumbnail = new Image();
		decal.Thumbnail.Create(decal.ArraySize, decal.ArraySize, useMipmaps: false, Image.Format.Rgba8);
		decal.ColorArray = new Color[decal.ArraySize, decal.ArraySize];
		decal.RoughnessArray = new Value[decal.ArraySize, decal.ArraySize];
		decal.MetallicityArray = new Value[decal.ArraySize, decal.ArraySize];
		decal.HeightArray = new Value[decal.ArraySize, decal.ArraySize];
		decal.EmissionArray = new Color[decal.ArraySize, decal.ArraySize];
		decal.Thumbnail.Lock();
		for (int i = selectionArrayStart.y; i <= selectionArrayEnd.y; i++)
		{
			for (int j = selectionArrayStart.x; j <= selectionArrayEnd.x; j++)
			{
				if (!workspace.Worksheet.Data.SelectionArray[j, i])
				{
					continue;
				}
				int arrayX = j - selectionArrayStart.x + (decal.ArraySize - decal.Width) / 2;
				int arrayY = i - selectionArrayStart.y + (decal.ArraySize - decal.Height) / 2;
				decal.ColorArray[arrayX, arrayY] = workspace.Worksheet.Data.ColorChannel[j, i];
				decal.RoughnessArray[arrayX, arrayY] = workspace.Worksheet.Data.RoughnessChannel[j, i];
				decal.MetallicityArray[arrayX, arrayY] = workspace.Worksheet.Data.MetallicityChannel[j, i];
				decal.HeightArray[arrayX, arrayY] = workspace.Worksheet.Data.HeightChannel[j, i];
				decal.EmissionArray[arrayX, arrayY] = workspace.Worksheet.Data.EmissionChannel[j, i];
				decal.Thumbnail.SetPixel(arrayX, arrayY, decal.ColorArray[arrayX, arrayY]);
				if (!doCut)
				{
					continue;
				}
				foreach (Layer layers in workspace.Worksheet.Data.LayersList)
				{
					layers.ColorChannel[j, i] = workspace.Worksheet.Data.Layer.ColorChannel.DefaultValue;
					layers.RoughnessChannel[j, i] = workspace.Worksheet.Data.Layer.RoughnessChannel.DefaultValue;
					layers.MetallicityChannel[j, i] = workspace.Worksheet.Data.Layer.MetallicityChannel.DefaultValue;
					layers.HeightChannel[j, i] = workspace.Worksheet.Data.Layer.HeightChannel.DefaultValue;
					layers.EmissionChannel[j, i] = workspace.Worksheet.Data.Layer.EmissionChannel.DefaultValue;
				}
			}
		}
		decal.Thumbnail.Unlock();
		decal.Thumbnail.Resize(Settings.ThumbnailSize, Settings.ThumbnailSize, Image.Interpolation.Nearest);
		if (doCut)
		{
			workspace.Worksheet.Data.CombineLayersFromAllChannels();
		}
		decal.CreateMaskArrayFromColorArray(decal.ColorArray, squared: true);
		decal.IsLoaded = true;
		drawingManager.StampTool.Decal = decal;
		drawingManager.StampTool.Mode = DrawingToolStamp.ModeEnum.DECAL;
		Clear();
		Register.Gui.ToolsContainer.PressToolButton(7);
	}

	public void CopyMerged()
	{
		CopyMerged(doCut: false);
	}

	public void CutMerged()
	{
		CopyMerged(doCut: true);
	}

	public void StartSelecting(Worksheet worksheet, Vector2i position)
	{
		if (isSelecting)
		{
			return;
		}
		isSelecting = true;
		drawingEnd = (drawingStart = position);
		if (tool == ToolEnum.RECTANGLE || tool == ToolEnum.LASSO)
		{
			drawingPositionLastFrame = drawingEnd;
		}
		for (int y = 0; y < worksheetData.Height; y++)
		{
			for (int x = 0; x < worksheetData.Width; x++)
			{
				currentSelectionArray[x, y] = false;
			}
		}
	}

	public void UpdateSelecting(Vector2i position)
	{
		if (!isSelecting)
		{
			return;
		}
		DrawingManager drawingManager = Register.DrawingManager;
		switch (tool)
		{
		case ToolEnum.BRUSH:
			drawingEnd = position;
			if (HasDrawingPositionChanged)
			{
				SetStroke(drawingStart, drawingEnd, value);
			}
			else if (drawingEnd != drawingPositionLastFrame)
			{
				SetSpot(drawingEnd, value);
			}
			drawingPositionLastFrame = (drawingStart = drawingEnd);
			break;
		case ToolEnum.LASSO:
			drawingEnd = position;
			if (Input.IsKeyPressed(16777238))
			{
				float angle = (float)Mathf.RoundToInt(Mathf.Rad2Deg(Mathf.Atan2(drawingEnd.y - drawingStart.y, drawingEnd.x - drawingStart.x)) / 45f) * 45f;
				float length = (drawingEnd - drawingStart).Length();
				drawingEnd.x = drawingStart.x + Mathf.FloorToInt(Mathf.Cos(Mathf.Deg2Rad(angle)) * length);
				drawingEnd.y = drawingStart.y + Mathf.FloorToInt(Mathf.Sin(Mathf.Deg2Rad(angle)) * length);
				if (drawingEnd.x < drawingStart.x)
				{
					drawingEnd.x++;
				}
				if (drawingEnd.y < drawingStart.y)
				{
					drawingEnd.y++;
				}
			}
			if (HasDrawingPositionChanged)
			{
				drawingManager.DrawingPreviewManager.ClearArea();
				int startX = drawingPositionLastFrame.x;
				int startY = drawingPositionLastFrame.y;
				int endX = drawingEnd.x;
				int endY = drawingEnd.y;
				if (Mathf.Abs(endX - startX) >= Mathf.Abs(endY - startY) && Mathf.Abs(endX - startX) > 0)
				{
					float increase = 1f * (float)(endY - startY) / (float)(endX - startX);
					if (startX < endX)
					{
						for (int k = startX; k <= endX; k++)
						{
							int y3 = (int)((float)startY + (float)(k - startX) * increase);
							SetStroke(drawingStart, new Vector2i(k, y3), value);
						}
					}
					else
					{
						for (int l = endX; l <= startX; l++)
						{
							int y4 = (int)((float)startY + (float)(l - startX) * increase);
							SetStroke(drawingStart, new Vector2i(l, y4), value);
						}
					}
				}
				else if (Mathf.Abs(endY - startY) > 0)
				{
					float increase = 1f * (float)(endX - startX) / (float)(endY - startY);
					if (startY < endY)
					{
						for (int m = startY; m <= endY; m++)
						{
							int x6 = (int)((float)startX + (float)(m - startY) * increase);
							SetStroke(drawingStart, new Vector2i(x6, m), value);
						}
					}
					else
					{
						for (int n = endY; n <= startY; n++)
						{
							int x7 = (int)((float)startX + (float)(n - startY) * increase);
							SetStroke(drawingStart, new Vector2i(x7, n), value);
						}
					}
				}
			}
			else
			{
				SetSpot(drawingEnd, value);
			}
			drawingPositionLastFrame = drawingEnd;
			break;
		case ToolEnum.RECTANGLE:
		{
			drawingEnd = position;
			Vector2i start;
			Vector2i end;
			if (HasDrawingPositionChangedSinceLastFrame)
			{
				start = drawingStart;
				end = drawingPositionLastFrame;
				if (Input.IsKeyPressed(16777238))
				{
					int originalWidth = end.x - start.x;
					int originalHeight = end.y - start.y;
					int size = Mathf.Abs((Mathf.Abs(originalWidth) >= Mathf.Abs(originalHeight)) ? originalWidth : originalHeight);
					if (originalWidth >= 0)
					{
						end.x = start.x + size;
					}
					else
					{
						end.x = start.x - size;
					}
					if (originalHeight >= 0)
					{
						end.y = start.y + size;
					}
					else
					{
						end.y = start.y - size;
					}
				}
				if (end.x < start.x)
				{
					ref int x = ref start.x;
					ref int x2 = ref end.x;
					int x3 = end.x;
					int x4 = start.x;
					x = x3;
					x2 = x4;
				}
				if (end.y < start.y)
				{
					ref int x = ref start.y;
					ref int y = ref end.y;
					int x4 = end.y;
					int x3 = start.y;
					x = x4;
					y = x3;
				}
				for (int i = start.y; i <= end.y; i++)
				{
					for (int j = start.x; j <= end.x; j++)
					{
						SetPixel(j, i, value: false);
					}
				}
			}
			start = drawingStart;
			end = drawingEnd;
			if (Input.IsKeyPressed(16777238))
			{
				int originalWidth2 = end.x - start.x;
				int originalHeight2 = end.y - start.y;
				int size2 = Mathf.Abs((Mathf.Abs(originalWidth2) >= Mathf.Abs(originalHeight2)) ? originalWidth2 : originalHeight2);
				if (originalWidth2 >= 0)
				{
					end.x = start.x + size2;
				}
				else
				{
					end.x = start.x - size2;
				}
				if (originalHeight2 >= 0)
				{
					end.y = start.y + size2;
				}
				else
				{
					end.y = start.y - size2;
				}
			}
			if (end.x < start.x)
			{
				ref int x = ref start.x;
				ref int x5 = ref end.x;
				int x3 = end.x;
				int x4 = start.x;
				x = x3;
				x5 = x4;
			}
			if (end.y < start.y)
			{
				ref int x = ref start.y;
				ref int y2 = ref end.y;
				int x4 = end.y;
				int x3 = start.y;
				x = x4;
				y2 = x3;
			}
			if (HasDrawingPositionChanged)
			{
				if (start.x != end.x && start.y != end.y)
				{
					if (HasDrawingPositionChangedSinceLastFrame)
					{
						if (end.x - start.x > 1 && end.y - start.y > 1)
						{
							SetRectangle(start, end, fillMode, value);
						}
						else if (end.x - start.x == 1)
						{
							SetStroke(start.x, start.y, start.x, end.y, value);
							SetStroke(start.x + 1, start.y, start.x + 1, end.y, value);
						}
						else
						{
							SetStroke(start.x, start.y, end.x, start.y, value);
							SetStroke(start.x, start.y + 1, end.x, start.y + 1, value);
						}
					}
				}
				else
				{
					SetStroke(start, end, value);
				}
			}
			else
			{
				SetSpot(start, value);
			}
			drawingPositionLastFrame = drawingEnd;
			break;
		}
		}
		if (InputManager.CursorSpace == CameraManager.SpaceEnum.PREVIEWSPACE && !InputManager.CursorCollision.CollisionDetected)
		{
			AbortSelecting();
		}
	}

	public void StopSelecting(Worksheet worksheet, Vector2i? position)
	{
		if (!isSelecting)
		{
			return;
		}
		if (position.HasValue)
		{
			drawingEnd = position.Value;
		}
		if (tool == ToolEnum.ISLAND)
		{
			if (previewspaceMeshManager.IsMeshLoaded)
			{
				bakeManager = Register.BakeManager;
				if (drawingEnd.x >= 0 && drawingEnd.x < worksheetData.Width && drawingEnd.y >= 0 && drawingEnd.y < worksheetData.Height && bakeManager.PixelAffiliations[drawingEnd.x, drawingEnd.y].TrianglesIndicesList != null && bakeManager.PixelAffiliations[drawingEnd.x, drawingEnd.y].TrianglesIndicesList.Count > 0)
				{
					int triangleIndex = bakeManager.PixelAffiliations[drawingEnd.x, drawingEnd.y].TrianglesIndicesList[0];
					int group = previewspaceMeshManager.GroupList[previewspaceMeshManager.CollisionTrianglesList[triangleIndex].Indices[0]];
					SelectingIsland(group);
				}
			}
		}
		else if (tool == ToolEnum.WAND)
		{
			switch (wandMode)
			{
			case WandModeEnum.FLOODFILL:
				FloodFill(worksheet, channel, drawingEnd, value: true, tolerance);
				break;
			case WandModeEnum.FULLFILL:
				FullFill(worksheet, channel, drawingEnd, value: true, tolerance);
				break;
			}
		}
		if (mode == ModeEnum.REPLACE)
		{
			worksheetData.SelectionArray = (bool[,])currentSelectionArray.Clone();
		}
		else
		{
			for (int y = maskUpdateStart.y; y <= maskUpdateEnd.y; y++)
			{
				for (int x = maskUpdateStart.x; x <= maskUpdateEnd.x; x++)
				{
					switch (mode)
					{
					case ModeEnum.ADD:
						worksheetData.SelectionArray[x, y] = worksheetData.SelectionArray[x, y] || currentSelectionArray[x, y];
						break;
					case ModeEnum.REMOVE:
						if (currentSelectionArray[x, y])
						{
							worksheetData.SelectionArray[x, y] = false;
						}
						break;
					}
					currentSelectionArray[x, y] = false;
				}
			}
		}
		isSelecting = false;
		Update();
	}

	public void AbortSelecting()
	{
		if (isSelecting)
		{
			drawingEnd = drawingStart;
			isSelecting = false;
		}
	}

	public void UpdatePosition(int x, int y)
	{
		doMaskUpdate = true;
		maskUpdateStart.SetMin(x, y);
		maskUpdateEnd.SetMax(x, y);
	}

	public void UpdateArea(Vector2i start, Vector2i end)
	{
		doMaskUpdate = true;
		maskUpdateStart = start;
		maskUpdateEnd = end;
	}

	public void Update()
	{
		Color color = new Color(0f, 0f, 0f, 0f);
		maskUpdateStart.SetMax(0, 0);
		maskUpdateEnd.SetMin(worksheetData.Width - 1, worksheetData.Height - 1);
		maskImage.Lock();
		if (mode == ModeEnum.REPLACE)
		{
			for (int y = 0; y < worksheetData.Height; y++)
			{
				for (int x = 0; x < worksheetData.Width; x++)
				{
					if (isSelecting)
					{
						color.r = (currentSelectionArray[x, y] ? 1f : 0f);
					}
					else
					{
						color.r = (worksheetData.SelectionArray[x, y] ? 1f : 0f);
					}
					maskImage.SetPixel(x, y, color);
				}
			}
		}
		else
		{
			for (int i = maskUpdateStart.y; i <= maskUpdateEnd.y; i++)
			{
				for (int j = maskUpdateStart.x; j <= maskUpdateEnd.x; j++)
				{
					bool value = worksheetData.SelectionArray[j, i];
					switch (mode)
					{
					case ModeEnum.ADD:
						value = value || currentSelectionArray[j, i];
						break;
					case ModeEnum.REMOVE:
						if (currentSelectionArray[j, i])
						{
							value = false;
						}
						break;
					}
					color.r = (value ? 1f : 0f);
					maskImage.SetPixel(j, i, color);
				}
			}
		}
		maskImage.Unlock();
		VisualServer.TextureSetData(maskImageTexture.GetRid(), maskImage);
	}

	public Mesh GetMesh()
	{
		return meshInstance.Mesh;
	}

	public void SetMesh(Mesh mesh)
	{
		meshInstance.Mesh = mesh;
	}
}
