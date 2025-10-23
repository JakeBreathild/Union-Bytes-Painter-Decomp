using System.IO;
using Godot;

public class BakeWindowDialog : WindowDialog
{
	public enum ModeEnum
	{
		POSITION,
		NORMAL,
		DETAILEDNORMAL,
		AMBIENTOCCLUSION,
		THICKNESS,
		LIGHT
	}

	private Workspace workspace;

	private Worksheet previousWorksheet;

	private BakeManager bakeManager;

	private DrawingManager drawingManager;

	private ulong bakeStart;

	private Worksheet worksheet;

	private int width;

	private int height;

	private float[,] bakedArray;

	private float[,] bluredArray;

	private float[,] adjustedArray;

	private bool doUpdateBake;

	private bool doUpdateBlur;

	private bool doUpdateAdjustments;

	private bool doUpdateImage;

	private bool doUpdateOutput;

	private bool isBakeingFinished;

	private bool isFinished;

	private ChannelArray<float> outputArray;

	private Image outputImage;

	private ImageTexture outputImageTexture;

	private TextureRect outputTextureRect;

	private Panel previewPanel;

	private TextureRect previewTextureRect;

	private GroupPanel sourceGroupPanel;

	private ChannelArray<Color> sourceChannelArray;

	private Image sourceImage;

	private ImageTexture sourceImageTexture;

	private TextureRect sourceTextureRect;

	private int sourceChannel = 1;

	private ItemList modesItemList;

	private string bakingModeName = "";

	private BakeManager.RayModeEnum rayMode;

	private OptionButton rayModeOptionButton;

	private SpinBox rayCountSpinBox;

	private AdvancedSlider rayOffsetAdvancedSlider;

	private AdvancedSlider rayDistanceAdvancedSlider;

	private int rayCount = 32;

	private float rayOffset = 0.025f;

	private float rayDistance = 1f;

	private AdvancedSlider normalFactorAdvancedSlider;

	private float normalFactor = 1f;

	private Panel lightPanel;

	private LineEdit lightXPositionLineEdit;

	private LineEdit lightYPositionLineEdit;

	private LineEdit lightZPositionLineEdit;

	private GroupPanel blurGroupPanel;

	private AdvancedSlider blurDistanceAdvancedSlider;

	private int blurDistance;

	private GroupPanel adjustmentsGroupPanel;

	private OptionButton sourceChannelOptionButton;

	private Button adjustmentsInverseButton;

	private AdvancedSlider adjustmentsIntensityAdvancedSlider;

	private AdvancedSlider adjustmentsBlackAdvancedSlider;

	private AdvancedSlider adjustmentsWhiteAdvancedSlider;

	private AdvancedSlider adjustmentsScaleAdvancedSlider;

	private AdvancedSlider adjustmentsPositionAdvancedSlider;

	private float adjustmentsBlack;

	private float adjustmentsWhite = 1f;

	private float adjustmentsScale = 1f;

	private float adjustmentsPosition = 0.5f;

	private float adjustmentsIntensity = 1f;

	[Export(PropertyHint.None, "")]
	private NodePath imageLoadFileDialogNodePath;

	private PreviewFileDialog imageLoadFileDialog;

	private GroupPanel imageGroupPanel;

	private Button imageLoadButton;

	private LineEdit imagePathLineEdit;

	private OptionButton imageBlendingModeOptionButton;

	private Button imageRemoveButton;

	private Image image;

	private ChannelArray<Value> imageArray;

	private Blender.BlendingModeEnum imageBlendingMode = Blender.BlendingModeEnum.MULTIPLICATION;

	private string imagePath = "";

	private GroupPanel heightBlendingGroupPanel;

	private CheckButton heightBlendingCheckButton;

	private AdvancedSlider heightBottomPowerAdvancedSlider;

	private AdvancedSlider heightBottomOffsetAdvancedSlider;

	private AdvancedSlider heightBlendingFactorAdvancedSlider;

	private CheckButton heightInverseCheckButton;

	private bool heightBlending;

	private float heightBottomPower = 1f;

	private float heightBottomOffset;

	private float heightBlendingFactor = 0.05f;

	private bool heightInverse;

	private LayerGroupPanel layerLayerGroupPanel;

	private bool selectionManagerPreviousState;

	private bool isMouseOnWindow;

	private DrawingManager.ToolEnum drawingToolBefore;

	public DrawingManager.ToolEnum DrawingToolBefore
	{
		get
		{
			return drawingToolBefore;
		}
		set
		{
			drawingToolBefore = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		drawingManager = Register.DrawingManager;
		bakeManager = Register.BakeManager;
		outputTextureRect = GetNodeOrNull<TextureRect>("Output/Image");
		outputTextureRect.Connect(Signals.MouseEntered, this, "OutputTextureRectMouseEntered");
		outputTextureRect.Connect(Signals.MouseExited, this, "OutputTextureRectMouseExited");
		previewPanel = GetNodeOrNull<Panel>("Preview");
		previewTextureRect = GetNodeOrNull<TextureRect>("Preview/Image");
		GetNodeOrNull<ScrollContainer>("SC").GetVScrollbar().MouseFilter = MouseFilterEnum.Pass;
		sourceGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Source");
		sourceGroupPanel.ResetButtonPressedCallback = BakingSettingsReset;
		sourceTextureRect = sourceGroupPanel.GetNodeOrNull<TextureRect>("Preview");
		modesItemList = sourceGroupPanel.GetNodeOrNull<ItemList>("Modes");
		modesItemList.AddItem("Position");
		modesItemList.AddItem("Normal");
		modesItemList.AddItem("Detailed Normal");
		modesItemList.AddItem("Ambient Occlusion");
		modesItemList.AddItem("Thickness");
		modesItemList.AddItem("Light");
		modesItemList.Select(3);
		modesItemList.Connect(Signals.ItemSelected, this, "ModeSelected");
		rayModeOptionButton = sourceGroupPanel.GetNodeOrNull<OptionButton>("VC/Rays/Mode");
		rayModeOptionButton.AddItem("Random");
		rayModeOptionButton.AddItem("Uniform");
		rayModeOptionButton.Select((int)rayMode);
		rayCountSpinBox = sourceGroupPanel.GetNodeOrNull<SpinBox>("VC/Rays/RaysCount");
		rayCountSpinBox.GetChild(0).Connect(Signals.TextEntered, this, "RayCountTextEntered");
		rayOffsetAdvancedSlider = sourceGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Rays/Offset");
		rayDistanceAdvancedSlider = sourceGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Rays/Distance");
		rayDistanceAdvancedSlider.ValueChangedCallback = RayDistanceValueChanged;
		normalFactorAdvancedSlider = sourceGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Normal");
		lightPanel = sourceGroupPanel.GetNodeOrNull<Panel>("VC/Light");
		lightXPositionLineEdit = lightPanel.GetNodeOrNull<LineEdit>("HC/XPanel/LineEdit");
		lightXPositionLineEdit.Connect(Signals.TextEntered, this, "LightXPositionEntered");
		lightYPositionLineEdit = lightPanel.GetNodeOrNull<LineEdit>("HC/YPanel/LineEdit");
		lightYPositionLineEdit.Connect(Signals.TextEntered, this, "LightYPositionEntered");
		lightZPositionLineEdit = lightPanel.GetNodeOrNull<LineEdit>("HC/ZPanel/LineEdit");
		lightZPositionLineEdit.Connect(Signals.TextEntered, this, "LightZPositionEntered");
		blurGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Blur");
		blurGroupPanel.ResetButtonPressedCallback = BlurReset;
		blurDistanceAdvancedSlider = blurGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Distance");
		blurDistanceAdvancedSlider.IntValue = blurDistance;
		blurDistanceAdvancedSlider.DefaultIntValue = blurDistance;
		blurDistanceAdvancedSlider.ValueChangedCallback = BlurDistanceChanged;
		adjustmentsGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Adjustments");
		adjustmentsGroupPanel.ResetButtonPressedCallback = AdjustmentsReset;
		sourceChannelOptionButton = adjustmentsGroupPanel.GetNodeOrNull<OptionButton>("VC/SourceChannel");
		sourceChannelOptionButton.AddItem("X [Red]");
		sourceChannelOptionButton.AddItem("Y [Green]");
		sourceChannelOptionButton.AddItem("Z [Blue]");
		sourceChannelOptionButton.Select(sourceChannel);
		sourceChannelOptionButton.Connect(Signals.ItemSelected, this, "SourceChannelChanged");
		adjustmentsIntensityAdvancedSlider = adjustmentsGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Intensity");
		adjustmentsIntensityAdvancedSlider.Value = adjustmentsIntensity;
		adjustmentsIntensityAdvancedSlider.DefaultValue = adjustmentsIntensity;
		adjustmentsIntensityAdvancedSlider.ValueChangedCallback = AdjustmentsIntensityChanged;
		adjustmentsInverseButton = adjustmentsGroupPanel.GetNodeOrNull<Button>("VC/Inverse");
		adjustmentsInverseButton.Connect(Signals.Pressed, this, "AdjustmentsInversePressed");
		adjustmentsBlackAdvancedSlider = adjustmentsGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Black");
		adjustmentsBlackAdvancedSlider.Value = adjustmentsBlack;
		adjustmentsBlackAdvancedSlider.DefaultValue = adjustmentsBlack;
		adjustmentsBlackAdvancedSlider.ValueChangedCallback = AdjustmentsBlackChanged;
		adjustmentsWhiteAdvancedSlider = adjustmentsGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/White");
		adjustmentsWhiteAdvancedSlider.Value = adjustmentsWhite;
		adjustmentsWhiteAdvancedSlider.DefaultValue = adjustmentsWhite;
		adjustmentsWhiteAdvancedSlider.ValueChangedCallback = AdjustmentsWhiteChanged;
		adjustmentsScaleAdvancedSlider = adjustmentsGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Scale");
		adjustmentsScaleAdvancedSlider.Value = adjustmentsScale;
		adjustmentsScaleAdvancedSlider.DefaultValue = adjustmentsScale;
		adjustmentsScaleAdvancedSlider.ValueChangedCallback = AdjustmentsScaleChanged;
		adjustmentsPositionAdvancedSlider = adjustmentsGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Position");
		adjustmentsPositionAdvancedSlider.Value = adjustmentsPosition;
		adjustmentsPositionAdvancedSlider.DefaultValue = adjustmentsPosition;
		adjustmentsPositionAdvancedSlider.ValueChangedCallback = AdjustmentsPositionChanged;
		imageGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Image");
		imageGroupPanel.ResetButtonPressedCallback = ImageReset;
		imageLoadFileDialog = GetNodeOrNull<PreviewFileDialog>(imageLoadFileDialogNodePath);
		imageLoadFileDialog.Connect(Signals.FileSelected, this, "ImageFileSelected");
		imageLoadButton = imageGroupPanel.GetNodeOrNull<Button>("VC/Load");
		imageLoadButton.Connect(Signals.Pressed, this, "ImageLoadPressed");
		imagePathLineEdit = imageGroupPanel.GetNodeOrNull<LineEdit>("VC/Path");
		imageBlendingModeOptionButton = imageGroupPanel.GetNodeOrNull<OptionButton>("VC/BlendingMode");
		for (int i = 0; i <= 7; i++)
		{
			imageBlendingModeOptionButton.AddItem(Blender.BlendingModeName[i], i);
		}
		imageBlendingModeOptionButton.Selected = (int)imageBlendingMode;
		imageBlendingModeOptionButton.GetPopup().Connect(Signals.IdPressed, this, "ImageBlendingModeSelected");
		imageRemoveButton = imageGroupPanel.GetNodeOrNull<Button>("VC/Remove");
		imageRemoveButton.Connect(Signals.Pressed, this, "ImageRemovePressed");
		heightBlendingGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/HeightBlending");
		heightBlendingGroupPanel.ResetButtonPressedCallback = HeightBlendingReset;
		heightBlendingCheckButton = heightBlendingGroupPanel.GetNodeOrNull<CheckButton>("VC/Enabling");
		heightBlendingCheckButton.Connect(Signals.Toggled, this, "HeightBlendingToggled");
		heightBottomPowerAdvancedSlider = heightBlendingGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/BottomPower");
		heightBottomPowerAdvancedSlider.ValueChangedCallback = HeightBottomPowerChanged;
		heightBottomOffsetAdvancedSlider = heightBlendingGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/BottomOffset");
		heightBottomOffsetAdvancedSlider.ValueChangedCallback = HeightBottomOffsetChanged;
		heightBlendingFactorAdvancedSlider = heightBlendingGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/BlendingFactor");
		heightBlendingFactorAdvancedSlider.ValueChangedCallback = HeightBlendingFactorChanged;
		heightInverseCheckButton = heightBlendingGroupPanel.GetNodeOrNull<CheckButton>("VC/Inverse");
		heightInverseCheckButton.Connect(Signals.Toggled, this, "HeightInverseToggled");
		layerLayerGroupPanel = GetNodeOrNull<LayerGroupPanel>("SC/VC/Layer");
		Connect(Signals.MouseEntered, this, "MouseEntered");
		Connect(Signals.MouseExited, this, "MouseExited");
		Connect(Signals.Hide, this, "Hide");
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (!isMouseOnWindow)
		{
			InputManager.CheckKeyBindingList(@event, InputManager.KeyBindingsLists[2].KeyBindings);
		}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (base.Visible)
		{
			Register.CameraManager.EnableWorkspaceCameraControls(enable: false);
			if (isMouseOnWindow)
			{
				Register.CameraManager.EnablePreviewCameraControls(enable: false);
			}
			else
			{
				Register.CameraManager.EnablePreviewCameraControls(enable: true);
				InputManager.KeyBindingsLists[2].Enable = true;
				InputManager.CheckKeyBindingList(InputManager.KeyBindingsLists[2].KeyBindings);
			}
		}
		if (doUpdateBake)
		{
			UpdateBake();
		}
		if (doUpdateBlur)
		{
			UpdateBlur();
		}
		if (doUpdateAdjustments)
		{
			UpdateAdjustments();
		}
		if (doUpdateImage)
		{
			UpdateImage();
		}
		if (doUpdateOutput)
		{
			UpdateOutput();
		}
	}

	public new void PopupCentered(Vector2? size = null)
	{
		selectionManagerPreviousState = Register.SelectionManager.IsEditingActivated;
		Register.SelectionManager.IsEditingActivated = false;
		InputManager.WindowShown();
		Reset();
		base.PopupCentered(size);
	}

	public new void Show()
	{
		selectionManagerPreviousState = Register.SelectionManager.IsEditingActivated;
		Register.SelectionManager.IsEditingActivated = false;
		InputManager.WindowShown();
		Reset();
		base.Show();
	}

	public new void Hide()
	{
		base.Hide();
		bakeManager.Abort();
		switch (Register.PreviewspaceMeshManager.Illumination)
		{
		case PreviewspaceMeshManager.IlluminationEnum.ILLUMINATED:
			Register.PreviewspaceMeshManager.ActivateFullShaderMaterial();
			break;
		case PreviewspaceMeshManager.IlluminationEnum.UNLIT:
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "channelTexture", workspace.Worksheet.Data.ColorChannel.ImageTexture);
			break;
		case PreviewspaceMeshManager.IlluminationEnum.CHANNEL:
			MaterialManager.UpdateChannelMaterials();
			break;
		}
		Register.CameraManager.EnablePreviewCameraControls(enable: false);
		InputManager.WindowHidden();
		InputManager.SkipInput = true;
		Register.DrawingManager.Tool = drawingToolBefore;
		Register.SelectionManager.IsEditingActivated = selectionManagerPreviousState;
		Register.PreviewspaceCursor.ForceVisible = false;
		Register.PreviewspaceCursor.GenerateCursorGeometry(PreviewspaceCursor.CursorTypeEnum.CIRCLE);
	}

	public void MouseEntered()
	{
		isMouseOnWindow = true;
	}

	public void MouseExited()
	{
		isMouseOnWindow = false;
	}

	public void Reset()
	{
		int lastWidth = width;
		int lastHeight = height;
		worksheet = workspace.Worksheet;
		width = worksheet.Data.Width;
		height = worksheet.Data.Height;
		if (width != lastWidth || height != lastHeight || previousWorksheet != worksheet)
		{
			sourceTextureRect.Texture = null;
			sourceImage = new Image();
			sourceImage.Create(width, height, useMipmaps: true, Image.Format.Rgba8);
			sourceImageTexture = new ImageTexture();
			sourceChannelArray = new ChannelArray<Color>(width, height);
			outputTextureRect.Texture = null;
			outputImage = new Image();
			outputImage.Create(width, height, useMipmaps: false, Image.Format.Rgba8);
			outputImageTexture = new ImageTexture();
			isBakeingFinished = false;
			isFinished = false;
			bakedArray = null;
			bluredArray = null;
			adjustedArray = null;
			previewTextureRect.Texture = null;
			ImageReset();
			AdjustmentsReset();
			BlurReset();
			BakingSettingsReset();
			HeightBlendingReset();
			layerLayerGroupPanel.Reset();
		}
		if (imageArray == null)
		{
			imageArray = new ChannelArray<Value>(1, 1, Value.White);
		}
		if (previousWorksheet != workspace.Worksheet)
		{
			sourceGroupPanel.Reset();
			BakingSettingsReset();
			adjustmentsGroupPanel.Reset();
			AdjustmentsReset();
			blurGroupPanel.Reset();
			BlurReset();
			imageGroupPanel.Reset();
			ImageReset();
			heightBlendingGroupPanel.Reset();
			HeightBlendingReset();
			layerLayerGroupPanel.Reset();
		}
		rayModeOptionButton.Select((int)rayMode);
		rayCountSpinBox.Value = rayCount;
		rayOffsetAdvancedSlider.Value = rayOffset;
		rayDistanceAdvancedSlider.Value = rayDistance;
		normalFactorAdvancedSlider.Value = normalFactor;
		Register.PreviewspaceCursor.SphereSize = rayDistance;
		Register.PreviewspaceCursor.GenerateCursorGeometry(PreviewspaceCursor.CursorTypeEnum.CROSS);
		UpdateLightPosition();
		if (modesItemList.GetSelectedItems()[0] == 5)
		{
			Register.PreviewspaceCursor.ForceHide = false;
			Register.PreviewspaceCursor.ForceVisible = true;
			lightPanel.Visible = true;
			sourceGroupPanel.RectMinSize = new Vector2(sourceGroupPanel.RectMinSize.x, 624f);
			lightXPositionLineEdit.Text = Register.PreviewspaceCursor.ForcePosition.x.ToString("0.000");
			lightYPositionLineEdit.Text = Register.PreviewspaceCursor.ForcePosition.y.ToString("0.000");
			lightZPositionLineEdit.Text = Register.PreviewspaceCursor.ForcePosition.z.ToString("0.000");
		}
		else
		{
			Register.PreviewspaceCursor.ForceHide = true;
			Register.PreviewspaceCursor.ForceVisible = false;
			lightPanel.Visible = false;
			sourceGroupPanel.RectMinSize = new Vector2(sourceGroupPanel.RectMinSize.x, 556f);
		}
		blurDistanceAdvancedSlider.IntValue = blurDistance;
		sourceChannelOptionButton.Select(sourceChannel);
		adjustmentsIntensityAdvancedSlider.Value = adjustmentsIntensity;
		adjustmentsBlackAdvancedSlider.Value = adjustmentsBlack;
		adjustmentsWhiteAdvancedSlider.Value = adjustmentsWhite;
		adjustmentsScaleAdvancedSlider.Value = adjustmentsScale;
		adjustmentsPositionAdvancedSlider.Value = adjustmentsPosition;
		heightBlendingCheckButton.Pressed = heightBlending;
		heightBottomPowerAdvancedSlider.Value = heightBottomPower;
		heightBottomOffsetAdvancedSlider.Value = heightBottomOffset;
		heightBlendingFactorAdvancedSlider.Value = heightBlendingFactor;
		heightInverseCheckButton.Pressed = heightInverse;
		Register.PreviewspaceMeshManager.ActivateChannelShaderMaterial();
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "channelTexture", outputImageTexture);
		GetNodeOrNull<ScrollContainer>("SC").ScrollVertical = 0;
		previousWorksheet = worksheet;
		doUpdateBake = false;
	}

	public void Bake()
	{
		doUpdateBake = true;
	}

	public void Apply()
	{
		if (!isFinished || outputArray == null)
		{
			return;
		}
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (Register.BakeManager.PixelAffiliations[x, y].TrianglesIndicesList == null)
				{
					outputArray[x, y] = 0f;
					if (x > 0 && Register.BakeManager.PixelAffiliations[x - 1, y].TrianglesIndicesList != null)
					{
						outputArray[x, y] = outputArray[x - 1, y];
					}
					else if (x < width - 1 && Register.BakeManager.PixelAffiliations[x + 1, y].TrianglesIndicesList != null)
					{
						outputArray[x, y] = outputArray[x + 1, y];
					}
					else if (y > 0 && Register.BakeManager.PixelAffiliations[x, y - 1].TrianglesIndicesList != null)
					{
						outputArray[x, y] = outputArray[x, y - 1];
					}
					else if (y < height - 1 && Register.BakeManager.PixelAffiliations[x, y + 1].TrianglesIndicesList != null)
					{
						outputArray[x, y] = outputArray[x, y + 1];
					}
					else if (x > 0 && y > 0 && Register.BakeManager.PixelAffiliations[x - 1, y - 1].TrianglesIndicesList != null)
					{
						outputArray[x, y] = outputArray[x - 1, y - 1];
					}
					else if (x < width - 1 && y > 0 && Register.BakeManager.PixelAffiliations[x + 1, y - 1].TrianglesIndicesList != null)
					{
						outputArray[x, y] = outputArray[x + 1, y - 1];
					}
					else if (x > 0 && y < height - 1 && Register.BakeManager.PixelAffiliations[x - 1, y + 1].TrianglesIndicesList != null)
					{
						outputArray[x, y] = outputArray[x - 1, y + 1];
					}
					else if (x < width - 1 && y < height - 1 && Register.BakeManager.PixelAffiliations[x + 1, y + 1].TrianglesIndicesList != null)
					{
						outputArray[x, y] = outputArray[x + 1, y + 1];
					}
				}
			}
		}
		string layerName = "Bake: " + bakingModeName;
		((LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER)).LayerCommandType = LayerCommand.LayerCommandTypeEnum.ADDNEW;
		workspace.Worksheet.History.StopRecording("Layer Added [" + layerName + "]");
		Layer layer = workspace.Worksheet.Data.Layer;
		layer.Name = layerName;
		layer.ColorChannel.IsVisible = layerLayerGroupPanel.IsColorChannelEnabled;
		layer.ColorChannel.BlendingMode = layerLayerGroupPanel.ColorChannelBlendingMode;
		layer.RoughnessChannel.IsVisible = layerLayerGroupPanel.IsRoughnessChannelEnabled;
		layer.RoughnessChannel.BlendingMode = layerLayerGroupPanel.RoughnessChannelBlendingMode;
		layer.MetallicityChannel.IsVisible = layerLayerGroupPanel.IsMetallicityChannelEnabled;
		layer.MetallicityChannel.BlendingMode = layerLayerGroupPanel.MetallicityChannelBlendingMode;
		layer.HeightChannel.IsVisible = layerLayerGroupPanel.IsHeightChannelEnabled;
		layer.HeightChannel.BlendingMode = layerLayerGroupPanel.HeightChannelBlendingMode;
		layer.EmissionChannel.IsVisible = layerLayerGroupPanel.IsEmissionChannelEnabled;
		layer.EmissionChannel.BlendingMode = layerLayerGroupPanel.EmissionChannelBlendingMode;
		drawingManager = Register.DrawingManager;
		drawingManager.PushSettings();
		drawingManager.Tool = DrawingManager.ToolEnum.BRUSH;
		drawingManager.BlendingMode = Blender.BlendingModeEnum.NORMAL;
		drawingManager.ColorEnabled = layerLayerGroupPanel.IsColorChannelEnabled;
		drawingManager.RoughnessEnabled = layerLayerGroupPanel.IsRoughnessChannelEnabled;
		drawingManager.MetallicityEnabled = layerLayerGroupPanel.IsMetallicityChannelEnabled;
		drawingManager.HeightEnabled = layerLayerGroupPanel.IsHeightChannelEnabled;
		drawingManager.EmissionEnabled = layerLayerGroupPanel.IsEmissionChannelEnabled;
		drawingManager.StartDrawing(workspace.Worksheet, Vector2i.NegOne);
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				if (outputArray[j, i] > 0f)
				{
					drawingManager.Color = new Color(layerLayerGroupPanel.Color, outputArray[j, i]);
					drawingManager.Roughness = new Value(layerLayerGroupPanel.Roughness.v, outputArray[j, i]);
					drawingManager.Metallicity = new Value(layerLayerGroupPanel.Metallicity.v, outputArray[j, i]);
					drawingManager.Height = new Value(layerLayerGroupPanel.Height.v, outputArray[j, i]);
					drawingManager.Emission = new Color(layerLayerGroupPanel.Emission, outputArray[j, i]);
					drawingManager.DrawPixel(workspace.Worksheet, j, i);
				}
			}
		}
		drawingManager.StopDrawing(workspace.Worksheet, Vector2i.NegOne, base.WindowTitle + " Mask [ " + workspace.Worksheet.Data.Layer.Name + "]");
		Register.DrawingManager.Tool = DrawingManager.ToolEnum.DISABLED;
		drawingManager.PopSettings();
		Register.Gui.LayerPanel.Reset();
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
		Hide();
	}

	private void UpdateBake()
	{
		Register.ThreadsManager.Abort();
		isFinished = false;
		isBakeingFinished = false;
		bakeStart = OS.GetSystemTimeMsecs();
		rayMode = (BakeManager.RayModeEnum)rayModeOptionButton.Selected;
		rayCount = Mathf.FloorToInt((float)rayCountSpinBox.Value);
		rayOffset = rayOffsetAdvancedSlider.Value;
		rayDistance = rayDistanceAdvancedSlider.Value;
		normalFactor = normalFactorAdvancedSlider.Value;
		width = workspace.Worksheet.Data.Width;
		height = workspace.Worksheet.Data.Height;
		bakedArray = null;
		switch ((ModeEnum)modesItemList.GetSelectedItems()[0])
		{
		case ModeEnum.POSITION:
			bakingModeName = "Position";
			sourceChannelArray.Array = bakeManager.BakePosition();
			GD.Print("Bake Time: " + (OS.GetSystemTimeMsecs() - bakeStart) + " ms");
			isBakeingFinished = true;
			doUpdateBlur = true;
			break;
		case ModeEnum.NORMAL:
			bakingModeName = "Normal";
			sourceChannelArray.Array = bakeManager.BakeNormal();
			GD.Print("Bake Time: " + (OS.GetSystemTimeMsecs() - bakeStart) + " ms");
			isBakeingFinished = true;
			doUpdateBlur = true;
			break;
		case ModeEnum.DETAILEDNORMAL:
			bakingModeName = "Detailed Normal";
			sourceChannelArray.Array = bakeManager.BakeDetailedNormal(normalFactor);
			GD.Print("Bake Time: " + (OS.GetSystemTimeMsecs() - bakeStart) + " ms");
			isBakeingFinished = true;
			doUpdateBlur = true;
			break;
		case ModeEnum.AMBIENTOCCLUSION:
			bakingModeName = "Ambient Occlusion";
			bakeManager.BakeAmbientOcclusion(UpdateBakingChunk, rayMode, rayCount, rayOffset, rayDistance);
			break;
		case ModeEnum.THICKNESS:
			bakingModeName = "Thickness";
			bakeManager.BakeThickness(UpdateBakingChunk, rayMode, rayCount, rayOffset, rayDistance);
			break;
		case ModeEnum.LIGHT:
			bakingModeName = "Light";
			bakeManager.BakeLight(UpdateBakingChunk, Register.PreviewspaceCursor.ForcePosition, rayDistance, normalFactor, rayOffset);
			break;
		}
		if (isBakeingFinished && sourceChannelArray != null)
		{
			bakedArray = new float[width, height];
			sourceImage.Lock();
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					sourceImage.SetPixel(x, y, sourceChannelArray[x, y]);
					switch (sourceChannel)
					{
					case 0:
						bakedArray[x, y] = sourceChannelArray[x, y].r;
						break;
					case 1:
						bakedArray[x, y] = sourceChannelArray[x, y].g;
						break;
					case 2:
						bakedArray[x, y] = sourceChannelArray[x, y].b;
						break;
					}
				}
			}
			sourceImage.Unlock();
			sourceImageTexture.CreateFromImage(sourceImage, 0u);
			sourceTextureRect.Texture = sourceImageTexture;
		}
		doUpdateBake = false;
	}

	private void UpdateBakingChunk(float[,] inputArray, bool lastChunk)
	{
		Color color = new Color(0f, 0f, 0f);
		bakedArray = inputArray;
		sourceImage.Lock();
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				color.r = (color.g = (color.b = bakedArray[x, y]));
				sourceImage.SetPixel(x, y, color);
			}
		}
		sourceImage.Unlock();
		sourceImageTexture.CreateFromImage(sourceImage, 0u);
		sourceTextureRect.Texture = sourceImageTexture;
		if (lastChunk)
		{
			isBakeingFinished = true;
			doUpdateBlur = true;
			GD.Print("Bake Time: " + (OS.GetSystemTimeMsecs() - bakeStart) + " ms");
		}
		else
		{
			bluredArray = bakedArray;
			doUpdateAdjustments = true;
		}
	}

	private void UpdateBlur()
	{
		isFinished = false;
		bluredArray = null;
		if (blurDistance > 0 && isBakeingFinished && bakedArray != null)
		{
			bluredArray = BlurFilter.FloatArray(bakedArray, width, height, worksheet.Data.Tileable, blurDistance);
		}
		else
		{
			bluredArray = bakedArray;
		}
		doUpdateBlur = false;
		doUpdateAdjustments = true;
	}

	private void UpdateAdjustments()
	{
		isFinished = false;
		adjustedArray = null;
		if (bluredArray != null)
		{
			adjustedArray = new float[width, height];
			float range = adjustmentsWhite - adjustmentsBlack;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (!Register.SelectionManager.Enabled || worksheet.Data.SelectionArray[x, y])
					{
						float value = Mathf.Clamp((1f - bluredArray[x, y]) * adjustmentsIntensity, 0f, 1f);
						value = Mathf.Clamp((value - adjustmentsPosition) / adjustmentsScale + 0.5f, 0f, 1f);
						value = adjustmentsBlack + value * range;
						if (heightBlending)
						{
							float heightBlend = Blender.HeightBlend(Mathf.Pow(workspace.Worksheet.Data.HeightChannel[x, y].v, heightBottomPower) + heightBottomOffset, 0f, value, 1f, heightBlendingFactor);
							if (heightInverse)
							{
								heightBlend = 1f - heightBlend;
							}
							value *= heightBlend;
						}
						adjustedArray[x, y] = value;
					}
					else
					{
						adjustedArray[x, y] = 0f;
					}
				}
			}
		}
		doUpdateAdjustments = false;
		doUpdateImage = true;
	}

	public void UpdateImage()
	{
		isFinished = false;
		outputArray = null;
		if (adjustedArray != null)
		{
			outputArray = new ChannelArray<float>((float[,])adjustedArray.Clone(), width, height, 0f);
			if (imageArray != null)
			{
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						if (!Register.SelectionManager.Enabled || workspace.Worksheet.Data.SelectionArray[x, y])
						{
							int modX = Mathf.PosMod(x, imageArray.Width);
							int modY = Mathf.PosMod(y, imageArray.Height);
							outputArray.Array[x, y] = Blender.Blend(outputArray.Array[x, y], imageArray[modX, modY].v, imageBlendingMode, imageArray[modX, modY].a);
						}
					}
				}
			}
		}
		doUpdateImage = false;
		doUpdateOutput = true;
	}

	private void UpdateOutput()
	{
		isFinished = false;
		if (outputArray != null)
		{
			outputArray.CreateImage(outputImage);
			outputImageTexture.CreateFromImage(outputImage, 0u);
			outputTextureRect.Texture = outputImageTexture;
			isFinished = true;
		}
		doUpdateOutput = false;
	}

	public void OutputTextureRectMouseEntered()
	{
		previewTextureRect.Texture = outputImageTexture;
		previewPanel.Visible = true;
	}

	public void OutputTextureRectMouseExited()
	{
		previewPanel.Visible = false;
	}

	public void BakingSettingsReset()
	{
		normalFactor = 1f;
		normalFactorAdvancedSlider.Value = normalFactor;
		rayMode = BakeManager.RayModeEnum.RANDOM;
		rayModeOptionButton.Select((int)rayMode);
		rayCount = 32;
		rayCountSpinBox.Value = rayCount;
		rayOffset = 0.025f;
		rayOffsetAdvancedSlider.Value = rayOffset;
		rayDistance = 1f;
		rayDistanceAdvancedSlider.Value = rayDistance;
		Register.PreviewspaceCursor.SphereSize = rayDistance;
		Register.PreviewspaceCursor.GenerateCursorGeometry(PreviewspaceCursor.CursorTypeEnum.CROSS);
		Register.PreviewspaceCursor.ForcePosition = Vector3.Zero;
		if (modesItemList.GetSelectedItems()[0] == 5)
		{
			Register.PreviewspaceCursor.ForceHide = false;
			Register.PreviewspaceCursor.ForceVisible = true;
			lightPanel.Visible = true;
			sourceGroupPanel.RectMinSize = new Vector2(sourceGroupPanel.RectMinSize.x, 624f);
		}
		else
		{
			Register.PreviewspaceCursor.ForceHide = true;
			Register.PreviewspaceCursor.ForceVisible = false;
			lightPanel.Visible = false;
			sourceGroupPanel.RectMinSize = new Vector2(sourceGroupPanel.RectMinSize.x, 556f);
		}
	}

	public void ModeSelected(int index)
	{
		if (index == 5)
		{
			Register.PreviewspaceCursor.ForceHide = false;
			Register.PreviewspaceCursor.ForceVisible = true;
			lightPanel.Visible = true;
			sourceGroupPanel.RectMinSize = new Vector2(sourceGroupPanel.RectMinSize.x, 624f);
		}
		else
		{
			Register.PreviewspaceCursor.ForceHide = true;
			Register.PreviewspaceCursor.ForceVisible = false;
			lightPanel.Visible = false;
			sourceGroupPanel.RectMinSize = new Vector2(sourceGroupPanel.RectMinSize.x, 556f);
		}
	}

	public void RayCountTextEntered(string text)
	{
		if (!text.IsValidFloat())
		{
			rayCountSpinBox.Value = rayCount;
			rayCountSpinBox.GetChild<LineEdit>(0).Text = rayCount.ToString();
		}
		rayCountSpinBox.GetChild<LineEdit>(0).ReleaseFocus();
	}

	public void RayDistanceValueChanged(float value)
	{
		rayDistance = rayDistanceAdvancedSlider.Value;
		if (Register.PreviewspaceCursor.ForceVisible)
		{
			Register.PreviewspaceCursor.SphereSize = rayDistance;
			Register.PreviewspaceCursor.GenerateCursorGeometry(PreviewspaceCursor.CursorTypeEnum.CROSS);
		}
	}

	public void UpdateLightPosition()
	{
		lightXPositionLineEdit.Text = Register.PreviewspaceCursor.ForcePosition.x.ToString("0.000");
		lightYPositionLineEdit.Text = Register.PreviewspaceCursor.ForcePosition.y.ToString("0.000");
		lightZPositionLineEdit.Text = Register.PreviewspaceCursor.ForcePosition.z.ToString("0.000");
	}

	public void LightXPositionEntered(string text)
	{
		if (text.IsValidFloat())
		{
			Register.PreviewspaceCursor.ForcePosition.x = float.Parse(text);
		}
		lightXPositionLineEdit.Text = Register.PreviewspaceCursor.ForcePosition.x.ToString("0.000");
		lightXPositionLineEdit.ReleaseFocus();
	}

	public void LightYPositionEntered(string text)
	{
		if (text.IsValidFloat())
		{
			Register.PreviewspaceCursor.ForcePosition.y = float.Parse(text);
		}
		lightYPositionLineEdit.Text = Register.PreviewspaceCursor.ForcePosition.y.ToString("0.000");
		lightYPositionLineEdit.ReleaseFocus();
	}

	public void LightZPositionEntered(string text)
	{
		if (text.IsValidFloat())
		{
			Register.PreviewspaceCursor.ForcePosition.z = float.Parse(text);
		}
		lightZPositionLineEdit.Text = Register.PreviewspaceCursor.ForcePosition.z.ToString("0.000");
		lightZPositionLineEdit.ReleaseFocus();
	}

	public void BlurReset()
	{
		blurDistance = 0;
		blurDistanceAdvancedSlider.IntValue = blurDistance;
		doUpdateBlur = true;
	}

	public void BlurDistanceChanged(float value)
	{
		blurDistance = blurDistanceAdvancedSlider.IntValue;
		doUpdateBlur = true;
	}

	public void AdjustmentsReset()
	{
		sourceChannel = 1;
		sourceChannelOptionButton.Select(sourceChannel);
		adjustmentsIntensity = 1f;
		adjustmentsIntensityAdvancedSlider.Value = adjustmentsIntensity;
		adjustmentsBlackAdvancedSlider.Reset();
		adjustmentsBlack = adjustmentsBlackAdvancedSlider.Value;
		adjustmentsWhiteAdvancedSlider.Reset();
		adjustmentsWhite = adjustmentsWhiteAdvancedSlider.Value;
		adjustmentsScaleAdvancedSlider.Reset();
		adjustmentsScale = adjustmentsScaleAdvancedSlider.Value;
		adjustmentsPositionAdvancedSlider.Reset();
		adjustmentsPosition = adjustmentsPositionAdvancedSlider.Value;
		doUpdateAdjustments = true;
	}

	public void SourceChannelChanged(int index)
	{
		sourceChannel = index;
		doUpdateBake = true;
	}

	public void AdjustmentsIntensityChanged(float value)
	{
		adjustmentsIntensity = adjustmentsIntensityAdvancedSlider.Value;
		doUpdateAdjustments = true;
	}

	public void AdjustmentsInversePressed()
	{
		float num = adjustmentsBlack;
		float num2 = adjustmentsWhite;
		adjustmentsWhite = num;
		adjustmentsBlack = num2;
		adjustmentsBlackAdvancedSlider.Value = adjustmentsBlack;
		adjustmentsWhiteAdvancedSlider.Value = adjustmentsWhite;
		doUpdateAdjustments = true;
	}

	public void AdjustmentsBlackChanged(float value)
	{
		adjustmentsBlack = adjustmentsBlackAdvancedSlider.Value;
		doUpdateAdjustments = true;
	}

	public void AdjustmentsWhiteChanged(float value)
	{
		adjustmentsWhite = adjustmentsWhiteAdvancedSlider.Value;
		doUpdateAdjustments = true;
	}

	public void AdjustmentsScaleChanged(float value)
	{
		adjustmentsScale = adjustmentsScaleAdvancedSlider.Value;
		doUpdateAdjustments = true;
	}

	public void AdjustmentsPositionChanged(float value)
	{
		adjustmentsPosition = adjustmentsPositionAdvancedSlider.Value;
		doUpdateAdjustments = true;
	}

	public void ImageReset()
	{
		imageBlendingMode = Blender.BlendingModeEnum.MULTIPLICATION;
		imageBlendingModeOptionButton.Selected = (int)imageBlendingMode;
		ImageRemovePressed();
	}

	public void ImageRemovePressed()
	{
		image = null;
		imageArray = new ChannelArray<Value>(1, 1, Value.White);
		imagePath = "";
		imagePathLineEdit.Text = imagePath;
		doUpdateImage = true;
	}

	public void ImageLoadPressed()
	{
		string path = Settings.MasksPath;
		if (!path.Empty())
		{
			imageLoadFileDialog.CurrentDir = path;
		}
		imageLoadFileDialog.SetWindowHidden = false;
		imageLoadFileDialog.PopupCentered();
	}

	public void ImageFileSelected(string file)
	{
		file = System.IO.Path.GetFullPath(file);
		if (!System.IO.File.Exists(file))
		{
			return;
		}
		string fileType = file.Substring(file.Length - 3, 3);
		if (fileType == "png" || fileType == "jpg")
		{
			imagePath = file;
			imagePathLineEdit.Text = imagePath;
			if (image == null)
			{
				image = new Image();
			}
			image.Load(file);
			imageArray = new ChannelArray<Value>(image.GetWidth(), image.GetHeight(), Value.Zero);
			image.Lock();
			for (int y = 0; y < imageArray.Height; y++)
			{
				for (int x = 0; x < imageArray.Width; x++)
				{
					Color color = image.GetPixel(x, y);
					imageArray.Array[x, y].v = color.v;
					imageArray.Array[x, y].a = color.a;
				}
			}
			image.Unlock();
			doUpdateImage = true;
		}
		Settings.MasksPath = System.IO.Path.GetDirectoryName(file);
	}

	public void ImageBlendingModeSelected(int blendingModeIndex)
	{
		imageBlendingMode = (Blender.BlendingModeEnum)blendingModeIndex;
		doUpdateImage = true;
	}

	public void HeightBlendingReset()
	{
		heightBlending = false;
		heightBlendingCheckButton.Pressed = heightBlending;
		heightBottomPower = 1f;
		heightBottomPowerAdvancedSlider.Value = heightBottomPower;
		heightBottomOffset = 0f;
		heightBottomOffsetAdvancedSlider.Value = heightBottomOffset;
		heightBlendingFactor = 0.05f;
		heightBlendingFactorAdvancedSlider.Value = heightBlendingFactor;
		heightInverse = false;
		heightInverseCheckButton.Pressed = heightInverse;
		doUpdateAdjustments = true;
	}

	public void HeightBlendingToggled(bool pressed)
	{
		heightBlending = pressed;
		doUpdateAdjustments = true;
	}

	public void HeightBottomPowerChanged(float value)
	{
		heightBottomPower = heightBottomPowerAdvancedSlider.Value;
		doUpdateAdjustments = true;
	}

	public void HeightBottomOffsetChanged(float value)
	{
		heightBottomOffset = heightBottomOffsetAdvancedSlider.Value;
		doUpdateAdjustments = true;
	}

	public void HeightBlendingFactorChanged(float value)
	{
		heightBlendingFactor = heightBlendingFactorAdvancedSlider.Value;
		doUpdateAdjustments = true;
	}

	public void HeightInverseToggled(bool pressed)
	{
		heightInverse = pressed;
		doUpdateAdjustments = true;
	}
}
