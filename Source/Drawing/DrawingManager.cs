using System;
using System.Collections.Generic;
using Godot;

public class DrawingManager : Node
{
	public enum ToolEnum
	{
		BRUSH,
		LINE,
		RECTANGLE,
		RECTANGLE_FILLED,
		CIRCLE,
		CIRCLE_FILLED,
		BUCKET,
		STAMP,
		SMEARING,
		DISABLED
	}

	public struct Settings
	{
		public ToolEnum Tool;

		public Blender.BlendingModeEnum BlendingMode;

		public bool ColorEnabled;

		public bool ColorAlphaEnabled;

		public Color Color;

		public bool RoughnessEnabled;

		public Value Roughness;

		public bool MetallicityEnabled;

		public Value Metallicity;

		public bool HeightEnabled;

		public Value Height;

		public bool EmissionEnabled;

		public Color Emission;

		public static Settings Default
		{
			get
			{
				Settings settings = default(Settings);
				settings.Tool = ToolEnum.BRUSH;
				settings.BlendingMode = DefaultBlendingMode;
				settings.ColorEnabled = true;
				settings.ColorAlphaEnabled = true;
				settings.Color = new Color(0f, 0f, 0f);
				settings.RoughnessEnabled = true;
				settings.Roughness = Value.White;
				settings.MetallicityEnabled = true;
				settings.Metallicity = Value.Black;
				settings.HeightEnabled = true;
				settings.Height = new Value(0.5f);
				settings.EmissionEnabled = false;
				settings.Emission = new Color(0f, 0f, 0f);
				return settings;
			}
		}
	}

	public static readonly Blender.BlendingModeEnum DefaultBlendingMode = Blender.BlendingModeEnum.ALPHABLEND;

	private Workspace workspace;

	private DrawingPreviewManager drawingPreviewManager;

	private DrawingCommand drawingCommand;

	private DrawingTool currentTool;

	private DrawingToolBrush brushTool;

	private DrawingToolLine lineTool;

	private DrawingToolRectangle rectangleTool;

	private DrawingToolCircle circleTool;

	private DrawingToolBucket bucketTool;

	private DrawingToolStamp stampTool;

	private DrawingToolSmearing smearingTool;

	private ToolEnum tool;

	private bool mirroring;

	private bool verticalMirroring;

	private float verticalMirrorPosition;

	private bool horizontalMirroring;

	private float horizontalMirrorPosition;

	private bool circleMirroring;

	private float circleVerticalMirrorPosition;

	private float circleHorizontalMirrorPosition;

	private int circleMirroringCount = 8;

	private bool isDrawing;

	private Vector2i drawingStart = Vector2i.Zero;

	private Vector2i drawingEnd = Vector2i.Zero;

	private Data.ChannelEnum channel;

	private Stack<Settings> settingsStack = new Stack<Settings>();

	private Settings currentSettings = Settings.Default;

	private int settingsArrayIndex;

	private Settings[] settingsArray = new Settings[2]
	{
		Settings.Default,
		Settings.Default
	};

	public DrawingPreviewManager DrawingPreviewManager
	{
		get
		{
			return drawingPreviewManager;
		}
		set
		{
			drawingPreviewManager = value;
		}
	}

	public DrawingTool CurrentTool
	{
		get
		{
			return currentTool;
		}
		set
		{
			currentTool = value;
		}
	}

	public DrawingToolBrush BrushTool => brushTool;

	public DrawingToolLine LineTool => lineTool;

	public DrawingToolRectangle RectangleTool => rectangleTool;

	public DrawingToolCircle CircleTool => circleTool;

	public DrawingToolBucket BucketTool => bucketTool;

	public DrawingToolStamp StampTool => stampTool;

	public DrawingToolSmearing SmearingTool => smearingTool;

	public ToolEnum Tool
	{
		get
		{
			return tool;
		}
		set
		{
			tool = value;
			currentSettings.Tool = tool;
			switch (tool)
			{
			case ToolEnum.BRUSH:
				currentTool = brushTool;
				Register.Gui.ControlsRichTextLabel.BbcodeText = currentTool.Controls;
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.FREE;
				break;
			case ToolEnum.LINE:
				currentTool = lineTool;
				Register.Gui.LineSettingsContainer.SpacingSlider.Disabled = false;
				Register.Gui.ControlsRichTextLabel.BbcodeText = currentTool.Controls;
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.LINE;
				break;
			case ToolEnum.RECTANGLE:
				currentTool = rectangleTool;
				Register.Gui.LineSettingsContainer.SpacingSlider.Disabled = true;
				Register.Gui.ControlsRichTextLabel.BbcodeText = currentTool.Controls;
				rectangleTool.Mode = DrawingToolRectangle.ModeEnum.OUTLINED;
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.NONE;
				break;
			case ToolEnum.RECTANGLE_FILLED:
				currentTool = rectangleTool;
				Register.Gui.LineSettingsContainer.SpacingSlider.Disabled = true;
				Register.Gui.ControlsRichTextLabel.BbcodeText = currentTool.Controls;
				rectangleTool.Mode = DrawingToolRectangle.ModeEnum.FILLED;
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.NONE;
				break;
			case ToolEnum.CIRCLE:
				currentTool = circleTool;
				Register.Gui.LineSettingsContainer.SpacingSlider.Disabled = true;
				Register.Gui.ControlsRichTextLabel.BbcodeText = currentTool.Controls;
				circleTool.Mode = DrawingToolCircle.ModeEnum.OUTLINED;
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.NONE;
				break;
			case ToolEnum.CIRCLE_FILLED:
				currentTool = circleTool;
				Register.Gui.LineSettingsContainer.SpacingSlider.Disabled = true;
				Register.Gui.ControlsRichTextLabel.BbcodeText = currentTool.Controls;
				circleTool.Mode = DrawingToolCircle.ModeEnum.FILLED;
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.NONE;
				break;
			case ToolEnum.BUCKET:
				currentTool = bucketTool;
				Register.Gui.ControlsRichTextLabel.BbcodeText = currentTool.Controls;
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.NONE;
				break;
			case ToolEnum.STAMP:
				currentTool = stampTool;
				Register.Gui.ControlsRichTextLabel.BbcodeText = currentTool.Controls;
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.NONE;
				break;
			case ToolEnum.SMEARING:
				currentTool = smearingTool;
				Register.Gui.ControlsRichTextLabel.BbcodeText = currentTool.Controls;
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.NONE;
				break;
			case ToolEnum.DISABLED:
				currentTool = null;
				Register.Gui.ControlsRichTextLabel.BbcodeText = "";
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.NONE;
				break;
			default:
				currentTool = brushTool;
				Register.Gui.ControlsRichTextLabel.BbcodeText = currentTool.Controls;
				Register.CameraManager.IntersectRayMode = CameraManager.IntersectRayModeEnum.FREE;
				break;
			}
			if (currentTool != null)
			{
				if (currentTool.CircleMirroring)
				{
					Register.MirroringToolButton.HideInformation();
				}
				else
				{
					Register.MirroringToolButton.ShowInformation();
				}
			}
		}
	}

	public bool Mirroring
	{
		get
		{
			return mirroring;
		}
		set
		{
			mirroring = value;
		}
	}

	public bool VerticalMirroring
	{
		get
		{
			return verticalMirroring;
		}
		set
		{
			verticalMirroring = value;
		}
	}

	public float VerticalMirrorPosition
	{
		get
		{
			return verticalMirrorPosition;
		}
		set
		{
			verticalMirrorPosition = value;
		}
	}

	public bool HorizontalMirroring
	{
		get
		{
			return horizontalMirroring;
		}
		set
		{
			horizontalMirroring = value;
		}
	}

	public float HorizontalMirrorPosition
	{
		get
		{
			return horizontalMirrorPosition;
		}
		set
		{
			horizontalMirrorPosition = value;
		}
	}

	public bool CircleMirroring
	{
		get
		{
			return circleMirroring;
		}
		set
		{
			circleMirroring = value;
		}
	}

	public float CircleVerticalMirrorPosition
	{
		get
		{
			return circleVerticalMirrorPosition;
		}
		set
		{
			circleVerticalMirrorPosition = value;
		}
	}

	public float CircleHorizontalMirrorPosition
	{
		get
		{
			return circleHorizontalMirrorPosition;
		}
		set
		{
			circleHorizontalMirrorPosition = value;
		}
	}

	public int CircleMirroringCount
	{
		get
		{
			return circleMirroringCount;
		}
		set
		{
			circleMirroringCount = value;
		}
	}

	public bool IsDrawing => isDrawing;

	public Data.ChannelEnum Channel
	{
		get
		{
			return channel;
		}
		set
		{
			channel = value;
			MaterialManager.UpdateDrawingMaterials();
		}
	}

	private Stack<Settings> SettingsStack => SettingsStack;

	public Blender.BlendingModeEnum BlendingMode
	{
		get
		{
			return currentSettings.BlendingMode;
		}
		set
		{
			settingsArray[settingsArrayIndex].BlendingMode = (currentSettings.BlendingMode = value);
		}
	}

	public bool ColorEnabled
	{
		get
		{
			return currentSettings.ColorEnabled;
		}
		set
		{
			currentSettings.ColorEnabled = value;
		}
	}

	public bool ColorAlphaEnabled
	{
		get
		{
			return currentSettings.ColorAlphaEnabled;
		}
		set
		{
			currentSettings.ColorAlphaEnabled = value;
		}
	}

	public Color Color
	{
		get
		{
			return currentSettings.Color;
		}
		set
		{
			settingsArray[settingsArrayIndex].Color = (currentSettings.Color = value);
			drawingPreviewManager.ChangeColor(currentSettings.Color);
		}
	}

	public bool RoughnessEnabled
	{
		get
		{
			return currentSettings.RoughnessEnabled;
		}
		set
		{
			currentSettings.RoughnessEnabled = value;
		}
	}

	public Value Roughness
	{
		get
		{
			return currentSettings.Roughness;
		}
		set
		{
			settingsArray[settingsArrayIndex].Roughness = (currentSettings.Roughness = value);
			drawingPreviewManager.ChangeRoughness(currentSettings.Roughness);
		}
	}

	public bool MetallicityEnabled
	{
		get
		{
			return currentSettings.MetallicityEnabled;
		}
		set
		{
			currentSettings.MetallicityEnabled = value;
		}
	}

	public Value Metallicity
	{
		get
		{
			return currentSettings.Metallicity;
		}
		set
		{
			settingsArray[settingsArrayIndex].Metallicity = (currentSettings.Metallicity = value);
			drawingPreviewManager.ChangeMetallicity(currentSettings.Metallicity);
		}
	}

	public bool HeightEnabled
	{
		get
		{
			return currentSettings.HeightEnabled;
		}
		set
		{
			currentSettings.HeightEnabled = value;
		}
	}

	public Value Height
	{
		get
		{
			return currentSettings.Height;
		}
		set
		{
			settingsArray[settingsArrayIndex].Height = (currentSettings.Height = value);
			drawingPreviewManager.ChangeHeight(currentSettings.Height);
		}
	}

	public bool EmissionEnabled
	{
		get
		{
			return currentSettings.EmissionEnabled;
		}
		set
		{
			currentSettings.EmissionEnabled = value;
		}
	}

	public Color Emission
	{
		get
		{
			return currentSettings.Emission;
		}
		set
		{
			settingsArray[settingsArrayIndex].Emission = (currentSettings.Emission = value);
			drawingPreviewManager.ChangeEmission(currentSettings.Emission);
		}
	}

	public DrawingManager()
	{
		Register.DrawingManager = this;
	}

	public DrawingManager(Workspace workspace)
	{
		Register.DrawingManager = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "DrawingManager";
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		drawingPreviewManager = new DrawingPreviewManager(workspace);
		brushTool = new DrawingToolBrush();
		lineTool = new DrawingToolLine();
		rectangleTool = new DrawingToolRectangle();
		circleTool = new DrawingToolCircle();
		bucketTool = new DrawingToolBucket();
		stampTool = new DrawingToolStamp();
		smearingTool = new DrawingToolSmearing();
		currentTool = brushTool;
	}

	public void Reset()
	{
		AbortDrawing();
		Channel = Data.ChannelEnum.FULL;
		BlendingMode = DefaultBlendingMode;
		Mirroring = false;
		VerticalMirroring = true;
		HorizontalMirroring = false;
		circleMirroring = false;
		circleMirroringCount = 8;
		ColorEnabled = true;
		ColorAlphaEnabled = true;
		HeightEnabled = true;
		RoughnessEnabled = true;
		MetallicityEnabled = true;
		EmissionEnabled = false;
		settingsArrayIndex = 0;
		settingsArray[0] = Settings.Default;
		settingsArray[1] = Settings.Default;
		Color = new Color(0f, 0f, 0f);
		Roughness = Value.White;
		Metallicity = Value.Black;
		Height = new Value(0.5f);
		Emission = new Color(0f, 0f, 0f);
		Tool = ToolEnum.BRUSH;
		brushTool.Reset();
		lineTool.Reset();
		rectangleTool.Reset();
		circleTool.Reset();
		bucketTool.Reset();
		stampTool.Reset();
		smearingTool.Reset();
		drawingPreviewManager.Reset();
	}

	public void PushSettings()
	{
		settingsStack.Push(currentSettings);
	}

	public void PopSettings()
	{
		currentSettings = settingsStack.Pop();
		Tool = currentSettings.Tool;
		BlendingMode = currentSettings.BlendingMode;
		Color = currentSettings.Color;
		Roughness = currentSettings.Roughness;
		Metallicity = currentSettings.Metallicity;
		Height = currentSettings.Height;
		Emission = currentSettings.Emission;
	}

	public void SetSettings(int index)
	{
		settingsArrayIndex = index;
		BlendingMode = (currentSettings.BlendingMode = settingsArray[settingsArrayIndex].BlendingMode);
		Color = (currentSettings.Color = settingsArray[settingsArrayIndex].Color);
		Roughness = (currentSettings.Roughness = settingsArray[settingsArrayIndex].Roughness);
		Metallicity = (currentSettings.Metallicity = settingsArray[settingsArrayIndex].Metallicity);
		Height = (currentSettings.Height = settingsArray[settingsArrayIndex].Height);
		Emission = (currentSettings.Emission = settingsArray[settingsArrayIndex].Emission);
		Register.MaterialContainer.UpdateMaterial(settingsArrayIndex);
	}

	public void ToggleSettings()
	{
		settingsArrayIndex = ((settingsArrayIndex == 0) ? 1 : 0);
		BlendingMode = (currentSettings.BlendingMode = settingsArray[settingsArrayIndex].BlendingMode);
		Color = (currentSettings.Color = settingsArray[settingsArrayIndex].Color);
		Roughness = (currentSettings.Roughness = settingsArray[settingsArrayIndex].Roughness);
		Metallicity = (currentSettings.Metallicity = settingsArray[settingsArrayIndex].Metallicity);
		Height = (currentSettings.Height = settingsArray[settingsArrayIndex].Height);
		Emission = (currentSettings.Emission = settingsArray[settingsArrayIndex].Emission);
		Register.MaterialContainer.UpdateMaterial(settingsArrayIndex);
	}

	public void PickPixel(Worksheet worksheet, int x, int y, bool layerMode = true)
	{
		Data data = worksheet.Data;
		x = Mathf.PosMod(x, data.Width);
		y = Mathf.PosMod(y, data.Height);
		if (layerMode)
		{
			Layer layer = data.Layer;
			Color = data.PickColor(layer, x, y);
			Height = data.PickHeight(layer, x, y);
			Roughness = data.PickRoughness(layer, x, y);
			Metallicity = data.PickMetallicity(layer, x, y);
			Emission = data.PickEmission(layer, x, y);
		}
		else
		{
			Color = data.PickColor(x, y);
			Height = data.PickHeight(x, y);
			Roughness = data.PickRoughness(x, y);
			Metallicity = data.PickMetallicity(x, y);
			Emission = data.PickEmission(x, y);
		}
		MaterialManager.UpdateDrawingMaterials();
	}

	public void PickPixel(Worksheet worksheet, Vector2i position, bool layerMode = true)
	{
		PickPixel(worksheet, position.x, position.y, layerMode);
	}

	public int DrawVerticalMirrorPixel(Worksheet worksheet, int x, int y, float blendingStrength = 1f)
	{
		int newX = Mathf.RoundToInt(VerticalMirrorPosition - ((float)x - VerticalMirrorPosition)) - 1;
		if (!workspace.Worksheet.Data.Tileable && (newX < 0 || newX >= worksheet.Data.Width))
		{
			return x;
		}
		newX = Mathf.PosMod(newX, worksheet.Data.Width);
		DrawPixel(worksheet, newX, y, blendingStrength);
		return newX;
	}

	public int DrawHorizontalMirrorPixel(Worksheet worksheet, int x, int y, float blendingStrength = 1f)
	{
		int newY = Mathf.RoundToInt(HorizontalMirrorPosition - ((float)y - HorizontalMirrorPosition)) - 1;
		if (!workspace.Worksheet.Data.Tileable && (newY < 0 || newY >= worksheet.Data.Height))
		{
			return y;
		}
		newY = Mathf.PosMod(newY, worksheet.Data.Height);
		DrawPixel(worksheet, x, newY, blendingStrength);
		return newY;
	}

	public void DrawCircleMirroredSpot(Func<float, float, float> calculateBlendingStrengthCallback, int x, int y, int size, float blendingStrength)
	{
		float deltaX = (float)x - circleVerticalMirrorPosition;
		float deltaY = (float)y - circleHorizontalMirrorPosition;
		float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
		float step = Mathf.Pi * 2f / (float)circleMirroringCount;
		float offset = Mathf.Atan2(deltaY, deltaX);
		for (int i = 0; i < circleMirroringCount; i++)
		{
			int newX = Mathf.RoundToInt(circleVerticalMirrorPosition + Mathf.Cos(offset + step * (float)i) * distance);
			int newY = Mathf.RoundToInt(circleHorizontalMirrorPosition + Mathf.Sin(offset + step * (float)i) * distance);
			for (int yy = 0; yy < size; yy++)
			{
				for (int xx = 0; xx < size; xx++)
				{
					float falloffBlendingStrength = calculateBlendingStrengthCallback(xx, yy);
					if (falloffBlendingStrength != 0f)
					{
						drawingPreviewManager.DrawPixel(newX + xx - Mathf.CeilToInt((float)size * 0.5f) + 1, newY + yy - Mathf.CeilToInt((float)size * 0.5f) + 1, clear: false, falloffBlendingStrength * blendingStrength);
					}
				}
			}
		}
	}

	public void DrawCircleMirroredSpot(Func<float, float, Vector2i, float> calculateBlendingStrengthCallback, int x, int y, int size, float blendingStrength)
	{
		float deltaX = (float)x - circleVerticalMirrorPosition;
		float deltaY = (float)y - circleHorizontalMirrorPosition;
		float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
		float step = Mathf.Pi * 2f / (float)circleMirroringCount;
		float offset = Mathf.Atan2(deltaY, deltaX);
		for (int i = 0; i < circleMirroringCount; i++)
		{
			int newX = Mathf.RoundToInt(circleVerticalMirrorPosition + Mathf.Cos(offset + step * (float)i) * distance);
			int newY = Mathf.RoundToInt(circleHorizontalMirrorPosition + Mathf.Sin(offset + step * (float)i) * distance);
			for (int yy = 0; yy < size; yy++)
			{
				for (int xx = 0; xx < size; xx++)
				{
					float falloffBlendingStrength = calculateBlendingStrengthCallback(xx, yy, Vector2i.Zero);
					if (falloffBlendingStrength != 0f)
					{
						drawingPreviewManager.DrawPixel(newX + xx - Mathf.CeilToInt((float)size * 0.5f) + 1, newY + yy - Mathf.CeilToInt((float)size * 0.5f) + 1, clear: false, falloffBlendingStrength * blendingStrength);
					}
				}
			}
		}
	}

	public void DrawCircleMirroredStroke(Func<float, float, float> calculateBlendingStrengthCallback, int x1, int y1, int x2, int y2, int size, float blendingStrength)
	{
		float startDeltaX = (float)x1 - circleVerticalMirrorPosition;
		float startDeltaY = (float)y1 - circleHorizontalMirrorPosition;
		float startDistance = Mathf.Sqrt(startDeltaX * startDeltaX + startDeltaY * startDeltaY);
		float startOffset = Mathf.Atan2(startDeltaY, startDeltaX);
		float endDeltaX = (float)x2 - circleVerticalMirrorPosition;
		float endDeltaY = (float)y2 - circleHorizontalMirrorPosition;
		float endDistance = Mathf.Sqrt(endDeltaX * endDeltaX + endDeltaY * endDeltaY);
		float endOffset = Mathf.Atan2(endDeltaY, endDeltaX);
		float step = Mathf.Pi * 2f / (float)circleMirroringCount;
		for (int i = 0; i < circleMirroringCount; i++)
		{
			int startNewX = Mathf.RoundToInt(circleVerticalMirrorPosition + Mathf.Cos(startOffset + step * (float)i) * startDistance);
			int startNewY = Mathf.RoundToInt(circleHorizontalMirrorPosition + Mathf.Sin(startOffset + step * (float)i) * startDistance);
			int endNewX = Mathf.RoundToInt(circleVerticalMirrorPosition + Mathf.Cos(endOffset + step * (float)i) * endDistance);
			int endNewY = Mathf.RoundToInt(circleHorizontalMirrorPosition + Mathf.Sin(endOffset + step * (float)i) * endDistance);
			for (int j = 0; j < size; j++)
			{
				for (int k = 0; k < size; k++)
				{
					float falloffBlendingStrength = calculateBlendingStrengthCallback(k, j);
					if (falloffBlendingStrength != 0f)
					{
						drawingPreviewManager.DrawLine(startNewX + k - Mathf.CeilToInt((float)size * 0.5f) + 1, startNewY + j - Mathf.CeilToInt((float)size * 0.5f) + 1, endNewX + k - Mathf.CeilToInt((float)size * 0.5f) + 1, endNewY + j - Mathf.CeilToInt((float)size * 0.5f) + 1, clear: false, falloffBlendingStrength * blendingStrength);
					}
				}
			}
		}
	}

	public void DrawCircleMirroredStroke(Func<float, float, Vector2i, float> calculateBlendingStrengthCallback, int x1, int y1, int x2, int y2, int size, float blendingStrength)
	{
		float startDeltaX = (float)x1 - circleVerticalMirrorPosition;
		float startDeltaY = (float)y1 - circleHorizontalMirrorPosition;
		float startDistance = Mathf.Sqrt(startDeltaX * startDeltaX + startDeltaY * startDeltaY);
		float startOffset = Mathf.Atan2(startDeltaY, startDeltaX);
		float endDeltaX = (float)x2 - circleVerticalMirrorPosition;
		float endDeltaY = (float)y2 - circleHorizontalMirrorPosition;
		float endDistance = Mathf.Sqrt(endDeltaX * endDeltaX + endDeltaY * endDeltaY);
		float endOffset = Mathf.Atan2(endDeltaY, endDeltaX);
		float step = Mathf.Pi * 2f / (float)circleMirroringCount;
		for (int i = 0; i < circleMirroringCount; i++)
		{
			int startNewX = Mathf.RoundToInt(circleVerticalMirrorPosition + Mathf.Cos(startOffset + step * (float)i) * startDistance);
			int startNewY = Mathf.RoundToInt(circleHorizontalMirrorPosition + Mathf.Sin(startOffset + step * (float)i) * startDistance);
			int endNewX = Mathf.RoundToInt(circleVerticalMirrorPosition + Mathf.Cos(endOffset + step * (float)i) * endDistance);
			int endNewY = Mathf.RoundToInt(circleHorizontalMirrorPosition + Mathf.Sin(endOffset + step * (float)i) * endDistance);
			for (int j = 0; j < size; j++)
			{
				for (int k = 0; k < size; k++)
				{
					float falloffBlendingStrength = calculateBlendingStrengthCallback(k, j, Vector2i.Zero);
					if (falloffBlendingStrength != 0f)
					{
						drawingPreviewManager.DrawLine(startNewX + k - Mathf.CeilToInt((float)size * 0.5f) + 1, startNewY + j - Mathf.CeilToInt((float)size * 0.5f) + 1, endNewX + k - Mathf.CeilToInt((float)size * 0.5f) + 1, endNewY + j - Mathf.CeilToInt((float)size * 0.5f) + 1, clear: false, falloffBlendingStrength * blendingStrength);
					}
				}
			}
		}
	}

	public void DrawPixel(Worksheet worksheet, int x, int y, float blendingStrength = 1f)
	{
		if ((worksheet.Data.Tileable || (x >= 0 && x < worksheet.Data.Width && y >= 0 && y < worksheet.Data.Height)) && (!Register.SelectionManager.Enabled || worksheet.Data.SelectionArray[x, y]))
		{
			drawingCommand.AddPixel(x, y, currentSettings.Color, currentSettings.Height, currentSettings.Roughness, currentSettings.Metallicity, currentSettings.Emission, blendingStrength);
		}
	}

	public void DrawPixel(Worksheet worksheet, Vector2i position, float blendingStrength = 1f)
	{
		DrawPixel(worksheet, position.x, position.y, blendingStrength);
	}

	public void DrawLine(Worksheet worksheet, int startX, int startY, int endX, int endY, float blendingStrength = 1f)
	{
		if (Mathf.Abs(endX - startX) >= Mathf.Abs(endY - startY) && Mathf.Abs(endX - startX) > 0)
		{
			float increase = 1f * (float)(endY - startY) / (float)(endX - startX);
			if (startX < endX)
			{
				for (int x = startX; x <= endX; x++)
				{
					int y = (int)((float)startY + (float)(x - startX) * increase);
					DrawPixel(worksheet, x, y, blendingStrength);
				}
			}
			else
			{
				for (int i = endX; i <= startX; i++)
				{
					int y2 = (int)((float)startY + (float)(i - startX) * increase);
					DrawPixel(worksheet, i, y2, blendingStrength);
				}
			}
		}
		else
		{
			if (Mathf.Abs(endY - startY) <= 0)
			{
				return;
			}
			float increase = 1f * (float)(endX - startX) / (float)(endY - startY);
			if (startY < endY)
			{
				for (int j = startY; j <= endY; j++)
				{
					int x2 = (int)((float)startX + (float)(j - startY) * increase);
					DrawPixel(worksheet, x2, j, blendingStrength);
				}
			}
			else
			{
				for (int k = endY; k <= startY; k++)
				{
					int x3 = (int)((float)startX + (float)(k - startY) * increase);
					DrawPixel(worksheet, x3, k, blendingStrength);
				}
			}
		}
	}

	public void DrawLine(Worksheet worksheet, Vector2i start, Vector2i end, float blendingStrength = 1f)
	{
		DrawLine(worksheet, start.x, start.y, end.x, end.y, blendingStrength);
	}

	public void DrawBox(Worksheet worksheet, int startX, int startY, int endX, int endY, float blendingStrength = 1f)
	{
		if (endX < startX)
		{
			int num = endX;
			endX = startX;
			startX = num;
		}
		if (endY < startY)
		{
			int num2 = endY;
			endY = startY;
			startY = num2;
		}
		for (int y = startY; y <= endY; y++)
		{
			for (int x = startX; x <= endX; x++)
			{
				DrawPixel(worksheet, x, y, blendingStrength);
			}
		}
	}

	public void DrawBox(Worksheet worksheet, Vector2i start, Vector2i end, float blendingStrength = 1f)
	{
		DrawBox(worksheet, start.x, start.y, end.x, end.y, blendingStrength);
	}

	public void DrawCircle(Worksheet worksheet, int startX, int startY, int endX, int endY, float blendingStrength = 1f, int gradientMode = 0, bool gradientInverse = false)
	{
		if (endX < startX)
		{
			int num = endX;
			endX = startX;
			startX = num;
		}
		if (endY < startY)
		{
			int num2 = endY;
			endY = startY;
			startY = num2;
		}
		int width = endX - startX + 1;
		int height = endY - startY + 1;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				float num3 = Mathf.Abs((float)x + 0.5f - (float)width / 2f);
				float deltaY = Mathf.Abs((float)y + 0.5f - (float)height / 2f);
				float distance = Mathf.Sqrt(num3 * num3 / ((float)(width * width) * 0.25f) + deltaY * deltaY / ((float)(height * height) * 0.25f));
				if (!(distance <= 1f))
				{
					continue;
				}
				float blending = blendingStrength;
				if (!gradientInverse)
				{
					switch (gradientMode)
					{
					case 1:
						blending *= 1f - distance;
						break;
					case 2:
						blending *= 1f - distance * distance;
						break;
					case 3:
						blending *= 0.5f - 0.5f * Mathf.Cos((1f - distance) * Mathf.Pi);
						break;
					case 4:
						blending *= 0.5f - 0.5f * Mathf.Cos((1f - distance * distance) * Mathf.Pi);
						break;
					}
				}
				else
				{
					switch (gradientMode)
					{
					case 1:
						blending *= distance;
						break;
					case 2:
						blending *= distance * distance;
						break;
					case 3:
						blending *= 0.5f - 0.5f * Mathf.Cos(distance * Mathf.Pi);
						break;
					case 4:
						blending *= 0.5f - 0.5f * Mathf.Cos(distance * distance * Mathf.Pi);
						break;
					}
				}
				DrawPixel(worksheet, startX + x, startY + y, blending);
			}
		}
	}

	public void DrawCircle(Worksheet worksheet, Vector2i start, Vector2i end, float blendingStrength = 1f, int gradientMode = 0)
	{
		DrawCircle(worksheet, start.x, start.y, end.x, end.y, blendingStrength, gradientMode);
	}

	public void StartDrawing(Worksheet worksheet, Vector2i position)
	{
		if (!worksheet.Data.Layer.IsLocked && Tool != ToolEnum.DISABLED)
		{
			isDrawing = true;
			drawingEnd = (drawingStart = position);
			currentTool.StartDrawing(drawingStart);
			drawingPreviewManager.ClearArea();
			worksheet.History.StopRecording();
			drawingCommand = (DrawingCommand)worksheet.History.StartRecording(History.CommandTypeEnum.DRAWING);
			drawingCommand.CopyAllDrawingManagerSettings();
			Register.CollisionManager.CollisionsList.Clear();
		}
	}

	public void UpdateDrawing(Vector2i position)
	{
		if (Tool == ToolEnum.DISABLED)
		{
			return;
		}
		if (!isDrawing)
		{
			drawingPreviewManager.ClearArea();
			Register.Gui.InformationsLabel.Text = position.ToString();
			currentTool.DrawPreview(position);
		}
		if (isDrawing)
		{
			currentTool.WhileDrawing(position);
			if (currentTool.AreaTool)
			{
				Register.Gui.InformationsLabel.Text = drawingStart.ToString() + " > " + position.ToString() + "    |    " + (Mathf.Abs(drawingStart.x - position.x) + 1) + ", " + (Mathf.Abs(drawingStart.y - position.y) + 1);
			}
			else
			{
				Register.Gui.InformationsLabel.Text = position.ToString();
			}
			if (InputManager.CursorSpace == CameraManager.SpaceEnum.PREVIEWSPACE && !InputManager.CursorCollision.CollisionDetected)
			{
				AbortDrawing();
			}
		}
	}

	public void StopDrawing(Worksheet worksheet, Vector2i? position, string historyTextReplace = "")
	{
		if (Tool != ToolEnum.DISABLED && isDrawing)
		{
			if (position.HasValue)
			{
				drawingEnd = position.Value;
			}
			currentTool.StopDrawing(drawingEnd);
			if (historyTextReplace != "")
			{
				worksheet.History.StopRecording(historyTextReplace);
			}
			else
			{
				worksheet.History.StopRecording(currentTool.Name + " [" + worksheet.Data.Layer.Name + "]");
			}
			Register.CollisionManager.CollisionsList.Clear();
			drawingCommand = null;
			isDrawing = false;
			if (Register.GridManager.DoShowLayerContentAreas)
			{
				Register.GridManager.Update(worksheet);
			}
		}
	}

	public void StopDrawing(Worksheet worksheet, Vector2i? position, bool doErase, string historyTextReplace = "")
	{
		if (Tool != ToolEnum.DISABLED && isDrawing)
		{
			if (position.HasValue)
			{
				drawingEnd = position.Value;
			}
			currentTool.StopDrawing(drawingEnd);
			drawingCommand.DoErase = doErase;
			if (historyTextReplace != "")
			{
				worksheet.History.StopRecording(historyTextReplace);
			}
			else
			{
				worksheet.History.StopRecording(currentTool.Name + " [" + worksheet.Data.Layer.Name + "]");
			}
			Register.CollisionManager.CollisionsList.Clear();
			drawingCommand = null;
			isDrawing = false;
			if (Register.GridManager.DoShowLayerContentAreas)
			{
				Register.GridManager.Update(worksheet);
			}
		}
	}

	public void AbortDrawing()
	{
		if (Tool != ToolEnum.DISABLED && isDrawing)
		{
			Register.CollisionManager.CollisionsList.Clear();
			drawingEnd = drawingStart;
			currentTool.AbortDrawing();
			drawingPreviewManager.ClearArea();
			Register.Workspace.Worksheet.History.AbortRecording();
			drawingCommand = null;
			isDrawing = false;
		}
	}

	public void ClearPreview()
	{
		drawingPreviewManager.ClearArea();
	}
}
