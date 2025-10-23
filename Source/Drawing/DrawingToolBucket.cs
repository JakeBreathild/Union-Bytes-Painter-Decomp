using System.Collections.Generic;
using Godot;

public class DrawingToolBucket : DrawingTool
{
	public enum ModeEnum
	{
		MASK,
		MATERIAL,
		DECAL
	}

	private enum DirectionEnum
	{
		UP,
		RIGHT,
		DOWN,
		LEFT
	}

	private Data.ChannelEnum channel = Data.ChannelEnum.COLOR;

	private float tolerance = 0.05f;

	private ModeEnum mode = ModeEnum.MATERIAL;

	private Mask mask;

	private List<Mask> masksList = new List<Mask>();

	private Decal decal;

	private List<Decal> decalsList = new List<Decal>();

	private bool doHeightBlending;

	private float heightOffset;

	private float heightBlendingFactor;

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

	public float Tolerance
	{
		get
		{
			return tolerance;
		}
		set
		{
			tolerance = Mathf.Clamp(value, 0f, 1f);
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
			switch (mode)
			{
			case ModeEnum.MASK:
				Register.MaterialContainer.EnableDecal(pressed: true, settingsEnabled: false);
				Register.MaterialContainer.EnableMaterial(pressed: true);
				Register.MaterialContainer.ChangeDecalLabel("Mask");
				Register.MaterialContainer.UpdateDecalList();
				break;
			case ModeEnum.MATERIAL:
				Register.MaterialContainer.EnableDecal(pressed: false, settingsEnabled: false);
				Register.MaterialContainer.EnableMaterial(pressed: true);
				Register.MaterialContainer.ChangeDecalLabel("Preview");
				break;
			case ModeEnum.DECAL:
				Register.MaterialContainer.EnableDecal(pressed: true, settingsEnabled: true);
				Register.MaterialContainer.EnableMaterial(pressed: false);
				Register.MaterialContainer.ChangeDecalLabel("Decal");
				Register.MaterialContainer.ChangeDecalHeightOffset(heightOffset);
				Register.MaterialContainer.ToggleDecalHeightBlendSettings(enable: false);
				Register.MaterialContainer.UpdateDecalList();
				break;
			}
			Register.Gui.FillingBucketSettingsContainer.ChangeModeButtons(mode);
		}
	}

	public Mask Mask => mask;

	public List<Mask> MasksList => masksList;

	public Decal Decal => decal;

	public List<Decal> DecalsList => decalsList;

	public bool DoHeightBlending
	{
		get
		{
			return doHeightBlending;
		}
		set
		{
			doHeightBlending = value;
		}
	}

	public float HeightOffset
	{
		get
		{
			return heightOffset;
		}
		set
		{
			heightOffset = value;
		}
	}

	public float HeightBlendingFactor
	{
		get
		{
			return heightBlendingFactor;
		}
		set
		{
			heightBlendingFactor = value;
		}
	}

	public DrawingToolBucket()
	{
		drawingManager = Register.DrawingManager;
		drawingPreviewManager = Register.DrawingPreviewManager;
		workspace = Register.Workspace;
		name = "Bucket";
	}

	public void SetMask(Image image, string name)
	{
		if (image.IsEmpty())
		{
			return;
		}
		mask = new Mask();
		mask.Set(image, name, squared: false);
		if (mask.IsLoaded)
		{
			if (mask.Thumbnail != null)
			{
				ImageTexture texture = new ImageTexture();
				texture.CreateFromImage(mask.Thumbnail, 0u);
				Register.MaterialContainer.ChangeDecalPreview(texture);
			}
			if (image == Resources.DefaultMaskImage)
			{
				mode = ModeEnum.MASK;
			}
			else
			{
				Mode = ModeEnum.MASK;
			}
		}
	}

	public void LoadMask(string file)
	{
		mask = new Mask();
		mask.Load(file, squared: false);
		if (mask.IsLoaded)
		{
			if (mask.Thumbnail != null)
			{
				ImageTexture texture = new ImageTexture();
				texture.CreateFromImage(mask.Thumbnail, 0u);
				Register.MaterialContainer.ChangeDecalPreview(texture);
			}
			Mode = ModeEnum.MASK;
		}
	}

	public void AddMaskToList(Mask mask)
	{
		masksList.Add(mask);
	}

	public void AddMaskToList()
	{
		AddMaskToList(mask);
	}

	public void RemoveMaskFromList(int index)
	{
		masksList.RemoveAt(index);
	}

	public void RemoveMaskFromList(Mask mask)
	{
		masksList.Remove(mask);
	}

	public void RemoveMaskFromList()
	{
		masksList.Remove(mask);
	}

	public void SelectMaskFromList(int index)
	{
		if (masksList.Count > 0 && index > -1 && index < masksList.Count)
		{
			mask = masksList[index];
		}
	}

	public void SelectRandomMaskFromList()
	{
		if (masksList.Count > 0)
		{
			GD.Randomize();
			int index = (int)GD.Randi() % masksList.Count;
			SelectMaskFromList(index);
		}
	}

	public void ClearMasksList()
	{
		masksList.Clear();
	}

	public void LoadDecal(string file)
	{
		decal = new Decal();
		decal.Load(file, squared: false);
		if (decal.IsLoaded)
		{
			if (decal.Thumbnail != null)
			{
				Register.MaterialContainer.ChangeDecalPreview(decal.Thumbnail);
			}
			Mode = ModeEnum.DECAL;
		}
	}

	public void AddDecalToList(Decal decal)
	{
		decalsList.Add(decal);
	}

	public void AddDecalToList()
	{
		AddDecalToList(decal);
	}

	public void RemoveDecalFromList(int index)
	{
		decalsList.RemoveAt(index);
	}

	public void RemoveDecalFromList(Decal decal)
	{
		decalsList.Remove(decal);
	}

	public void RemoveDecalFromList()
	{
		decalsList.Remove(decal);
	}

	public void SelectDecalFromList(int index)
	{
		if (decalsList.Count > 0 && index > -1 && index < decalsList.Count)
		{
			decal = decalsList[index];
		}
	}

	public void SelectRandomDecalFromList()
	{
		if (decalsList.Count > 0)
		{
			GD.Randomize();
			int index = (int)GD.Randi() % decalsList.Count;
			SelectDecalFromList(index);
		}
	}

	public void ClearDecalsList()
	{
		decalsList.Clear();
	}

	public void DrawPixel(Worksheet worksheet, Vector2i position)
	{
		switch (mode)
		{
		case ModeEnum.MASK:
		{
			int x = position.x % mask.Width;
			int y = position.y % mask.Height;
			if (!Register.SelectionManager.Enabled)
			{
				drawingManager.DrawPixel(worksheet, position, blendingStrength * mask.MaskArray[x, y]);
			}
			else if (worksheet.Data.SelectionArray[position.x, position.y])
			{
				drawingManager.DrawPixel(worksheet, position, blendingStrength * mask.MaskArray[x, y]);
			}
			break;
		}
		case ModeEnum.MATERIAL:
			if (!Register.SelectionManager.Enabled)
			{
				drawingManager.DrawPixel(worksheet, position, blendingStrength);
			}
			else if (worksheet.Data.SelectionArray[position.x, position.y])
			{
				drawingManager.DrawPixel(worksheet, position, blendingStrength);
			}
			break;
		case ModeEnum.DECAL:
		{
			int x = position.x % decal.Width;
			int y = position.y % decal.Height;
			drawingManager.Color = decal.ColorArray[x, y];
			drawingManager.Roughness = decal.RoughnessArray[x, y];
			drawingManager.Metallicity = decal.MetallicityArray[x, y];
			drawingManager.Height = new Value(Mathf.Clamp(decal.HeightArray[x, y].v + heightOffset, 0f, 1f), decal.HeightArray[x, y].a);
			drawingManager.Emission = decal.EmissionArray[x, y];
			if (!Register.SelectionManager.Enabled)
			{
				drawingManager.DrawPixel(worksheet, position, blendingStrength * drawingManager.Color.a);
			}
			else if (worksheet.Data.SelectionArray[position.x, position.y])
			{
				drawingManager.DrawPixel(worksheet, position, blendingStrength * drawingManager.Color.a);
			}
			break;
		}
		}
	}

	private void ColorFloodFill(Worksheet worksheet, Color[,] array, int width, int height, Vector2i seed, Color seedColor, Color color, float tolerance, DirectionEnum direction)
	{
		Vector2i position = seed;
		switch (direction)
		{
		case DirectionEnum.UP:
			position.y--;
			while (position.y >= 0 && array[position.x, position.y] != color && ColorExtension.ColorDistance(array[position.x, position.y], seedColor) <= tolerance)
			{
				array[position.x, position.y] = color;
				DrawPixel(worksheet, position);
				position.y--;
			}
			position.y++;
			while (position.y < seed.y)
			{
				if (position.x > 0 && array[position.x - 1, position.y] != color)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, color, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && array[position.x + 1, position.y] != color)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, color, tolerance, DirectionEnum.RIGHT);
				}
				position.y++;
			}
			break;
		case DirectionEnum.RIGHT:
			position.x++;
			while (position.x < width && array[position.x, position.y] != color && ColorExtension.ColorDistance(array[position.x, position.y], seedColor) <= tolerance)
			{
				array[position.x, position.y] = color;
				DrawPixel(worksheet, position);
				position.x++;
			}
			position.x--;
			while (position.x > seed.x)
			{
				if (position.y > 0 && array[position.x, position.y - 1] != color)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, color, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && array[position.x, position.y + 1] != color)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, color, tolerance, DirectionEnum.DOWN);
				}
				position.x--;
			}
			break;
		case DirectionEnum.DOWN:
			position.y++;
			while (position.y < height && array[position.x, position.y] != color && ColorExtension.ColorDistance(array[position.x, position.y], seedColor) <= tolerance)
			{
				array[position.x, position.y] = color;
				DrawPixel(worksheet, position);
				position.y++;
			}
			position.y--;
			while (position.y > seed.y)
			{
				if (position.x > 0 && array[position.x - 1, position.y] != color)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, color, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && array[position.x + 1, position.y] != color)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, color, tolerance, DirectionEnum.RIGHT);
				}
				position.y--;
			}
			break;
		case DirectionEnum.LEFT:
			position.x--;
			while (position.x >= 0 && array[position.x, position.y] != color && ColorExtension.ColorDistance(array[position.x, position.y], seedColor) <= tolerance)
			{
				array[position.x, position.y] = color;
				DrawPixel(worksheet, position);
				position.x--;
			}
			position.x++;
			while (position.x < seed.x)
			{
				if (position.y > 0 && array[position.x, position.y - 1] != color)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, color, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && array[position.x, position.y + 1] != color)
				{
					ColorFloodFill(worksheet, array, width, height, position, seedColor, color, tolerance, DirectionEnum.DOWN);
				}
				position.x++;
			}
			break;
		}
	}

	private void ValueFloodFill(Worksheet worksheet, Value[,] array, int width, int height, Vector2i seed, Value seedValue, Value value, float tolerance, DirectionEnum direction)
	{
		Vector2i position = seed;
		switch (direction)
		{
		case DirectionEnum.UP:
			position.y--;
			while (position.y >= 0 && array[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y].v - seedValue.v) <= tolerance)
			{
				array[position.x, position.y] = value;
				DrawPixel(worksheet, position);
				position.y--;
			}
			position.y++;
			while (position.y < seed.y)
			{
				if (position.x > 0 && array[position.x - 1, position.y] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && array[position.x + 1, position.y] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.RIGHT);
				}
				position.y++;
			}
			break;
		case DirectionEnum.RIGHT:
			position.x++;
			while (position.x < width && array[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y].v - seedValue.v) <= tolerance)
			{
				array[position.x, position.y] = value;
				DrawPixel(worksheet, position);
				position.x++;
			}
			position.x--;
			while (position.x > seed.x)
			{
				if (position.y > 0 && array[position.x, position.y - 1] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && array[position.x, position.y + 1] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.DOWN);
				}
				position.x--;
			}
			break;
		case DirectionEnum.DOWN:
			position.y++;
			while (position.y < height && array[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y].v - seedValue.v) <= tolerance)
			{
				array[position.x, position.y] = value;
				DrawPixel(worksheet, position);
				position.y++;
			}
			position.y--;
			while (position.y > seed.y)
			{
				if (position.x > 0 && array[position.x - 1, position.y] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && array[position.x + 1, position.y] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.RIGHT);
				}
				position.y--;
			}
			break;
		case DirectionEnum.LEFT:
			position.x--;
			while (position.x >= 0 && array[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y].v - seedValue.v) <= tolerance)
			{
				array[position.x, position.y] = value;
				DrawPixel(worksheet, position);
				position.x--;
			}
			position.x++;
			while (position.x < seed.x)
			{
				if (position.y > 0 && array[position.x, position.y - 1] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && array[position.x, position.y + 1] != value)
				{
					ValueFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.DOWN);
				}
				position.x++;
			}
			break;
		}
	}

	private void FloatFloodFill(Worksheet worksheet, float[,] array, int width, int height, Vector2i seed, float seedValue, float value, float tolerance, DirectionEnum direction)
	{
		Vector2i position = seed;
		switch (direction)
		{
		case DirectionEnum.UP:
			position.y--;
			while (position.y >= 0 && array[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y] - seedValue) <= tolerance)
			{
				array[position.x, position.y] = value;
				DrawPixel(worksheet, position);
				position.y--;
			}
			position.y++;
			while (position.y < seed.y)
			{
				if (position.x > 0 && array[position.x - 1, position.y] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && array[position.x + 1, position.y] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.RIGHT);
				}
				position.y++;
			}
			break;
		case DirectionEnum.RIGHT:
			position.x++;
			while (position.x < width && array[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y] - seedValue) <= tolerance)
			{
				array[position.x, position.y] = value;
				DrawPixel(worksheet, position);
				position.x++;
			}
			position.x--;
			while (position.x > seed.x)
			{
				if (position.y > 0 && array[position.x, position.y - 1] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && array[position.x, position.y + 1] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.DOWN);
				}
				position.x--;
			}
			break;
		case DirectionEnum.DOWN:
			position.y++;
			while (position.y < height && array[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y] - seedValue) <= tolerance)
			{
				array[position.x, position.y] = value;
				DrawPixel(worksheet, position);
				position.y++;
			}
			position.y--;
			while (position.y > seed.y)
			{
				if (position.x > 0 && array[position.x - 1, position.y] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.LEFT);
				}
				if (position.x < width - 1 && array[position.x + 1, position.y] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.RIGHT);
				}
				position.y--;
			}
			break;
		case DirectionEnum.LEFT:
			position.x--;
			while (position.x >= 0 && array[position.x, position.y] != value && Mathf.Abs(array[position.x, position.y] - seedValue) <= tolerance)
			{
				array[position.x, position.y] = value;
				DrawPixel(worksheet, position);
				position.x--;
			}
			position.x++;
			while (position.x < seed.x)
			{
				if (position.y > 0 && array[position.x, position.y - 1] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.UP);
				}
				if (position.y < height - 1 && array[position.x, position.y + 1] != value)
				{
					FloatFloodFill(worksheet, array, width, height, position, seedValue, value, tolerance, DirectionEnum.DOWN);
				}
				position.x++;
			}
			break;
		}
	}

	public void FloodFill(Worksheet worksheet, Data.ChannelEnum channel, Vector2i seed, float tolerance, bool layerMode = true)
	{
		seed.x = Mathf.PosMod(seed.x, worksheet.Data.Width);
		seed.y = Mathf.PosMod(seed.y, worksheet.Data.Height);
		int width = worksheet.Data.Width;
		int height = worksheet.Data.Height;
		if (mode == ModeEnum.DECAL)
		{
			drawingManager.Color = new Color(-1f, -1f, -1f, -1f);
			drawingManager.Roughness = new Value(-1f);
			drawingManager.Metallicity = new Value(-1f);
			drawingManager.Height = new Value(-1f);
			drawingManager.Emission = new Color(-1f, -1f, -1f, -1f);
		}
		switch (channel)
		{
		case Data.ChannelEnum.FULL:
		case Data.ChannelEnum.COLOR:
		{
			Color color = drawingManager.Color;
			Color seedColor;
			Color[,] colorArray;
			if (layerMode)
			{
				seedColor = worksheet.Data.Layer.ColorChannel.Array[seed.x, seed.y];
				colorArray = (Color[,])worksheet.Data.Layer.ColorChannel.Array.Clone();
			}
			else
			{
				seedColor = worksheet.Data.ColorChannel.Array[seed.x, seed.y];
				colorArray = (Color[,])worksheet.Data.ColorChannel.Array.Clone();
			}
			for (DirectionEnum d3 = DirectionEnum.UP; d3 <= DirectionEnum.LEFT; d3++)
			{
				ColorFloodFill(worksheet, colorArray, width, height, seed, seedColor, color, tolerance, d3);
			}
			break;
		}
		case Data.ChannelEnum.EMISSION:
		{
			Color color = drawingManager.Emission;
			Color seedColor;
			Color[,] colorArray;
			if (layerMode)
			{
				seedColor = worksheet.Data.Layer.EmissionChannel[seed.x, seed.y];
				colorArray = (Color[,])worksheet.Data.Layer.EmissionChannel.Array.Clone();
			}
			else
			{
				seedColor = worksheet.Data.EmissionChannel[seed.x, seed.y];
				colorArray = (Color[,])worksheet.Data.EmissionChannel.Array.Clone();
			}
			for (DirectionEnum d5 = DirectionEnum.UP; d5 <= DirectionEnum.LEFT; d5++)
			{
				ColorFloodFill(worksheet, colorArray, width, height, seed, seedColor, color, tolerance, d5);
			}
			break;
		}
		case Data.ChannelEnum.HEIGHT:
		case Data.ChannelEnum.NORMAL:
		{
			Value value = drawingManager.Height;
			Value seedValue;
			Value[,] valueArray;
			if (layerMode)
			{
				seedValue = worksheet.Data.Layer.HeightChannel[seed.x, seed.y];
				valueArray = (Value[,])worksheet.Data.Layer.HeightChannel.Array.Clone();
			}
			else
			{
				seedValue = worksheet.Data.HeightChannel[seed.x, seed.y];
				valueArray = (Value[,])worksheet.Data.HeightChannel.Array.Clone();
			}
			for (DirectionEnum d2 = DirectionEnum.UP; d2 <= DirectionEnum.LEFT; d2++)
			{
				ValueFloodFill(worksheet, valueArray, width, height, seed, seedValue, value, tolerance, d2);
			}
			break;
		}
		case Data.ChannelEnum.ROUGHNESS:
		{
			Value value = drawingManager.Roughness;
			Value seedValue;
			Value[,] valueArray;
			if (layerMode)
			{
				seedValue = worksheet.Data.Layer.RoughnessChannel[seed.x, seed.y];
				valueArray = (Value[,])worksheet.Data.Layer.RoughnessChannel.Array.Clone();
			}
			else
			{
				seedValue = worksheet.Data.RoughnessChannel[seed.x, seed.y];
				valueArray = (Value[,])worksheet.Data.RoughnessChannel.Array.Clone();
			}
			for (DirectionEnum d4 = DirectionEnum.UP; d4 <= DirectionEnum.LEFT; d4++)
			{
				ValueFloodFill(worksheet, valueArray, width, height, seed, seedValue, value, tolerance, d4);
			}
			break;
		}
		case Data.ChannelEnum.METALLICITY:
		{
			Value value = drawingManager.Metallicity;
			Value seedValue;
			Value[,] valueArray;
			if (layerMode)
			{
				seedValue = worksheet.Data.Layer.MetallicityChannel[seed.x, seed.y];
				valueArray = (Value[,])worksheet.Data.Layer.MetallicityChannel.Array.Clone();
			}
			else
			{
				seedValue = worksheet.Data.MetallicityChannel[seed.x, seed.y];
				valueArray = (Value[,])worksheet.Data.MetallicityChannel.Array.Clone();
			}
			for (DirectionEnum d = DirectionEnum.UP; d <= DirectionEnum.LEFT; d++)
			{
				ValueFloodFill(worksheet, valueArray, width, height, seed, seedValue, value, tolerance, d);
			}
			break;
		}
		}
		DrawPixel(worksheet, seed);
	}

	private int VerticalMirrorFloodFill(Worksheet worksheet, Data.ChannelEnum channel, int x, int y, float tolerance, bool layerMode = true)
	{
		int newX = Mathf.RoundToInt(drawingManager.VerticalMirrorPosition - ((float)x - drawingManager.VerticalMirrorPosition)) - 1;
		if (!workspace.Worksheet.Data.Tileable && (newX < 0 || newX >= worksheet.Data.Width))
		{
			return x;
		}
		newX = Mathf.PosMod(newX, worksheet.Data.Width);
		FloodFill(worksheet, channel, new Vector2i(newX, y), tolerance, layerMode);
		return newX;
	}

	private int HorizontalMirrorFloodFill(Worksheet worksheet, Data.ChannelEnum channel, int x, int y, float tolerance, bool layerMode = true)
	{
		int newY = Mathf.RoundToInt(drawingManager.HorizontalMirrorPosition - ((float)y - drawingManager.HorizontalMirrorPosition)) - 1;
		if (!workspace.Worksheet.Data.Tileable && (newY < 0 || newY >= worksheet.Data.Height))
		{
			return y;
		}
		newY = Mathf.PosMod(newY, worksheet.Data.Height);
		FloodFill(worksheet, channel, new Vector2i(x, newY), tolerance, layerMode);
		return newY;
	}

	public override void DrawPreview(Vector2i position)
	{
		switch (mode)
		{
		case ModeEnum.MASK:
		case ModeEnum.MATERIAL:
			drawingManager.DrawingPreviewManager.ChangeBlendingMode(drawingManager.BlendingMode);
			drawingManager.DrawingPreviewManager.ChangeColor(drawingManager.Color);
			drawingManager.DrawingPreviewManager.ChangeRoughness(drawingManager.Roughness);
			drawingManager.DrawingPreviewManager.ChangeMetallicity(drawingManager.Metallicity);
			drawingManager.DrawingPreviewManager.ChangeHeight(drawingManager.Height);
			drawingManager.DrawingPreviewManager.ChangeEmission(drawingManager.Emission);
			drawingPreviewManager.DrawPixel(position.x, position.y, clear: true, blendingStrength);
			break;
		case ModeEnum.DECAL:
			drawingManager.DrawingPreviewManager.ChangeBlendingMode(Blender.BlendingModeEnum.NORMAL);
			drawingManager.DrawingPreviewManager.ChangeColor(Settings.AccentColor);
			drawingManager.DrawingPreviewManager.ChangeRoughness(Value.White);
			drawingManager.DrawingPreviewManager.ChangeMetallicity(Value.Black);
			drawingManager.DrawingPreviewManager.ChangeHeight(new Value(0.5f));
			drawingManager.DrawingPreviewManager.ChangeEmission(Settings.BlackColor);
			drawingPreviewManager.DrawPixel(position.x, position.y, clear: true, blendingStrength);
			break;
		}
	}

	public override void DrawSpot(Vector2i position)
	{
	}

	public override void DrawStroke(Vector2i start, Vector2i end)
	{
	}

	public override void WhileDrawing(Vector2i position)
	{
	}

	public override void StopDrawing(Vector2i position)
	{
		drawingEnd = position;
		if (Input.IsKeyPressed(16777238) && InputManager.CursorSpace == CameraManager.SpaceEnum.PREVIEWSPACE)
		{
			drawingManager.Tool = DrawingManager.ToolEnum.BRUSH;
			drawingManager.AbortDrawing();
			Register.BakeManager.BakeOnIsland(InputManager.CursorCollision.Group);
			drawingManager.Tool = DrawingManager.ToolEnum.BUCKET;
			return;
		}
		drawingManager.PushSettings();
		if (!Input.IsKeyPressed(16777237))
		{
			FloodFill(workspace.Worksheet, channel, drawingEnd, tolerance);
		}
		else
		{
			FloodFill(workspace.Worksheet, channel, drawingEnd, tolerance, layerMode: false);
		}
		if (drawingManager.Mirroring)
		{
			if (!Input.IsKeyPressed(16777237))
			{
				if (drawingManager.VerticalMirroring)
				{
					VerticalMirrorFloodFill(workspace.Worksheet, channel, drawingEnd.x, drawingEnd.y, tolerance);
				}
				if (drawingManager.HorizontalMirroring)
				{
					int newY = HorizontalMirrorFloodFill(workspace.Worksheet, channel, drawingEnd.x, drawingEnd.y, tolerance);
					if (drawingManager.VerticalMirroring)
					{
						VerticalMirrorFloodFill(workspace.Worksheet, channel, drawingEnd.x, newY, tolerance);
					}
				}
			}
			else
			{
				if (drawingManager.VerticalMirroring)
				{
					VerticalMirrorFloodFill(workspace.Worksheet, channel, drawingEnd.x, drawingEnd.y, tolerance, layerMode: false);
				}
				if (drawingManager.HorizontalMirroring)
				{
					int newY2 = HorizontalMirrorFloodFill(workspace.Worksheet, channel, drawingEnd.x, drawingEnd.y, tolerance, layerMode: false);
					if (drawingManager.VerticalMirroring)
					{
						VerticalMirrorFloodFill(workspace.Worksheet, channel, drawingEnd.x, newY2, tolerance, layerMode: false);
					}
				}
			}
		}
		drawingManager.PopSettings();
	}

	public override void Reset()
	{
		base.Reset();
		channel = Data.ChannelEnum.COLOR;
		tolerance = 0.05f;
		mode = ModeEnum.MATERIAL;
		mask = null;
		decal = null;
		heightOffset = 0f;
	}
}
