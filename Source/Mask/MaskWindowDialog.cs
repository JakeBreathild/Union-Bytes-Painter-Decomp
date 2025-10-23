using System.IO;
using Godot;

public class MaskWindowDialog : WindowDialog
{
	public enum MaskEnum
	{
		DEFAULT,
		GRADIENT,
		NOISE,
		AMBIENTOCCLUSION,
		CURVATURE
	}

	private Workspace workspace;

	private Worksheet previousWorksheet;

	private int width;

	private int height;

	private MaskEnum mask;

	private MaskEnum previousMask;

	private float[,] maskArray;

	private float[,] bluredMaskArray;

	private float[,] adjustedArray;

	private bool isFinished;

	private bool doUpdateMask;

	private bool doUpdateBlur;

	private bool doUpdateAdjustments;

	private bool doUpdateImage;

	private bool doUpdateOutput;

	private Image outputImage;

	private ImageTexture outputImageTexture;

	private TextureRect outputTextureRect;

	private ChannelArray<float> outputArray;

	private Panel previewPanel;

	private TextureRect previewTextureRect;

	private GroupPanel blurGroupPanel;

	private AdvancedSlider blurDistanceAdvancedSlider;

	private int blurDistance;

	private GroupPanel adjustmentsGroupPanel;

	private Button adjustmentsInverseButton;

	private AdvancedSlider adjustmentsBlackAdvancedSlider;

	private AdvancedSlider adjustmentsWhiteAdvancedSlider;

	private AdvancedSlider adjustmentsScaleAdvancedSlider;

	private AdvancedSlider adjustmentsPositionAdvancedSlider;

	private float adjustmentsBlack;

	private float adjustmentsWhite = 1f;

	private float adjustmentsScale = 1f;

	private float adjustmentsPosition = 0.5f;

	[Export(PropertyHint.None, "")]
	private NodePath imageLoadFileDialogNodePath;

	private PreviewFileDialog imageLoadFileDialog;

	private GroupPanel imageGroupPanel;

	private Button imageLoadButton;

	private LineEdit imagePathLineEdit;

	private OptionButton imageBlendingModeOptionButton;

	private Button imageRemoveButton;

	private Image image;

	private string imagePath = "";

	private ChannelArray<Value> imageArray;

	private Blender.BlendingModeEnum imageBlendingMode = Blender.BlendingModeEnum.MULTIPLICATION;

	private LayerGroupPanel layerLayerGroupPanel;

	private GroupPanel gradientGroupPanel;

	private OptionButton gradientOrientationOptionButton;

	private AdvancedSlider gradientPositionAdvancedSlider;

	private AdvancedSlider gradientCenterXAdvancedSlider;

	private AdvancedSlider gradientCenterYAdvancedSlider;

	private AdvancedSlider gradientScaleAdvancedSlider;

	private GradientFilter.OrientationEnum gradientOrientation = GradientFilter.OrientationEnum.Y;

	private float gradientPosition = 0.5f;

	private int gradientCenterX;

	private int gradientCenterY;

	private float gradientScale = 1f;

	private NoiseGroupPanel noiseGroupPanel;

	private GroupPanel aoGroupPanel;

	private CheckButton aoAllLayersCheckButton;

	private OptionButton aoModeOptionButton;

	private AdvancedSlider aoRaysCountAdvancedSlider;

	private AdvancedSlider aoDistanceAdvancedSlider;

	private AdvancedSlider aoBiasAdvancedSlider;

	private AdvancedSlider aoIntensityAdvancedSlider;

	private bool aoAllLayers = true;

	private AmbientOcclusionFilter.RayModeEnum aoMode;

	private int aoRaysCount = 16;

	private float aoDistance = 4f;

	private float aoBias = 0.01f;

	private float aoIntensity = 1f;

	private GroupPanel curvatureGroupPanel;

	private AdvancedSlider curvatureDistanceAdvancedSlider;

	private AdvancedSlider curvatureIntensityAdvancedSlider;

	private int curvatureDistance = 1;

	private float curvatureIntensity = 1f;

	private ulong bakeStart;

	private DrawingManager.ToolEnum drawingToolBefore;

	private bool selectionManagerPreviousState;

	private bool isMouseOnWindow;

	public MaskEnum Mask
	{
		get
		{
			return mask;
		}
		set
		{
			previousMask = mask;
			mask = value;
		}
	}

	private bool hasMaskChanged => mask != previousMask;

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
		outputTextureRect = GetNodeOrNull<TextureRect>("Output/Image");
		outputTextureRect.Connect(Signals.MouseEntered, this, "OutputTextureRectMouseEntered");
		outputTextureRect.Connect(Signals.MouseExited, this, "OutputTextureRectMouseExited");
		previewPanel = GetNodeOrNull<Panel>("Preview");
		previewTextureRect = GetNodeOrNull<TextureRect>("Preview/Image");
		GetNodeOrNull<ScrollContainer>("SC").GetVScrollbar().MouseFilter = MouseFilterEnum.Pass;
		blurGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Blur");
		blurGroupPanel.ResetButtonPressedCallback = BlurReset;
		blurDistanceAdvancedSlider = blurGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Distance");
		blurDistanceAdvancedSlider.IntValue = blurDistance;
		blurDistanceAdvancedSlider.DefaultIntValue = blurDistance;
		blurDistanceAdvancedSlider.ValueChangedCallback = BlurDistanceChanged;
		adjustmentsGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Adjustments");
		adjustmentsGroupPanel.ResetButtonPressedCallback = AdjustmentsReset;
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
		layerLayerGroupPanel = GetNodeOrNull<LayerGroupPanel>("SC/VC/Layer");
		gradientGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Gradient");
		gradientGroupPanel.Visible = false;
		gradientGroupPanel.ResetButtonPressedCallback = GradientReset;
		gradientOrientationOptionButton = gradientGroupPanel.GetNodeOrNull<OptionButton>("VC/Orientation");
		gradientOrientationOptionButton.AddItem("X");
		gradientOrientationOptionButton.AddItem("Y");
		gradientOrientationOptionButton.AddItem("Radial");
		gradientOrientationOptionButton.Selected = (int)gradientOrientation;
		gradientOrientationOptionButton.Connect(Signals.ItemSelected, this, "GradientOrientationSelected");
		gradientPositionAdvancedSlider = gradientGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Position");
		gradientPositionAdvancedSlider.Value = gradientPosition;
		gradientPositionAdvancedSlider.DefaultValue = gradientPosition;
		gradientPositionAdvancedSlider.ValueChangedCallback = GradiantPositionChanged;
		gradientCenterXAdvancedSlider = gradientGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/CenterX");
		gradientCenterXAdvancedSlider.ValueChangedCallback = GradientCenterXChanged;
		gradientCenterXAdvancedSlider.Disabled = true;
		gradientCenterYAdvancedSlider = gradientGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/CenterY");
		gradientCenterYAdvancedSlider.ValueChangedCallback = GradientCenterYChanged;
		gradientCenterYAdvancedSlider.Disabled = true;
		gradientScaleAdvancedSlider = gradientGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Scale");
		gradientScaleAdvancedSlider.Value = gradientScale;
		gradientScaleAdvancedSlider.DefaultValue = gradientScale;
		gradientScaleAdvancedSlider.ValueChangedCallback = GradiantScaleChanged;
		noiseGroupPanel = GetNodeOrNull<NoiseGroupPanel>("SC/VC/Noise");
		noiseGroupPanel.UpdateCallback = UpdateMask;
		aoGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/AmbientOcclusion");
		aoGroupPanel.Visible = false;
		aoGroupPanel.ResetButtonPressedCallback = AmbientOcclusionReset;
		aoAllLayersCheckButton = aoGroupPanel.GetNodeOrNull<CheckButton>("VC/AllLayers");
		aoAllLayersCheckButton.Pressed = aoAllLayers;
		aoAllLayersCheckButton.Connect(Signals.Toggled, this, "AoAllLayersToggled");
		aoModeOptionButton = aoGroupPanel.GetNodeOrNull<OptionButton>("VC/Mode");
		aoModeOptionButton.AddItem("Random");
		aoModeOptionButton.AddItem("Uniform");
		aoModeOptionButton.Selected = (int)aoMode;
		aoModeOptionButton.Connect(Signals.ItemSelected, this, "AoModeSelected");
		aoRaysCountAdvancedSlider = aoGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/RaysCount");
		aoRaysCountAdvancedSlider.IntValue = aoRaysCount;
		aoRaysCountAdvancedSlider.DefaultIntValue = aoRaysCount;
		aoRaysCountAdvancedSlider.ValueChangedCallback = AoRaysCountChanged;
		aoDistanceAdvancedSlider = aoGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Distance");
		aoDistanceAdvancedSlider.Value = aoDistance;
		aoDistanceAdvancedSlider.DefaultValue = aoDistance;
		aoDistanceAdvancedSlider.ValueChangedCallback = AoDistanceChanged;
		aoBiasAdvancedSlider = aoGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Bias");
		aoBiasAdvancedSlider.Value = aoBias;
		aoBiasAdvancedSlider.DefaultValue = aoBias;
		aoBiasAdvancedSlider.ValueChangedCallback = AoBiasChanged;
		aoIntensityAdvancedSlider = aoGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Intensity");
		aoIntensityAdvancedSlider.Value = aoIntensity;
		aoIntensityAdvancedSlider.DefaultValue = aoIntensity;
		aoIntensityAdvancedSlider.ValueChangedCallback = AoIntensityChanged;
		curvatureGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Curvature");
		curvatureGroupPanel.Visible = false;
		curvatureGroupPanel.ResetButtonPressedCallback = CurvatureReset;
		curvatureDistanceAdvancedSlider = curvatureGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Distance");
		curvatureDistanceAdvancedSlider.IntValue = curvatureDistance;
		curvatureDistanceAdvancedSlider.DefaultIntValue = curvatureDistance;
		curvatureDistanceAdvancedSlider.ValueChangedCallback = CurvatureDistanceChanged;
		curvatureIntensityAdvancedSlider = curvatureGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Intensity");
		curvatureIntensityAdvancedSlider.Value = curvatureIntensity;
		curvatureIntensityAdvancedSlider.DefaultValue = curvatureIntensity;
		curvatureIntensityAdvancedSlider.ValueChangedCallback = CurvatureIntensityChanged;
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
		if (doUpdateMask)
		{
			UpdateMask();
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
		Register.ThreadsManager.Abort();
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
		width = workspace.Worksheet.Data.Width;
		height = workspace.Worksheet.Data.Height;
		if (width != lastWidth || height != lastHeight || hasMaskChanged)
		{
			outputTextureRect.Texture = null;
			outputImage = new Image();
			outputImage.Create(width, height, useMipmaps: false, Image.Format.Rgba8);
			outputImageTexture = new ImageTexture();
			isFinished = false;
			maskArray = null;
			bluredMaskArray = null;
			adjustedArray = null;
			previewTextureRect.Texture = null;
			gradientCenterX = width / 2;
			gradientCenterXAdvancedSlider.MaxValue = width;
			gradientCenterXAdvancedSlider.IntValue = gradientCenterX;
			gradientCenterXAdvancedSlider.DefaultIntValue = gradientCenterX;
			gradientCenterY = height / 2;
			gradientCenterYAdvancedSlider.MaxValue = height;
			gradientCenterYAdvancedSlider.IntValue = gradientCenterY;
			gradientCenterYAdvancedSlider.DefaultIntValue = gradientCenterY;
			ImageReset();
			AdjustmentsReset();
			BlurReset();
			layerLayerGroupPanel.Reset();
		}
		if (imageArray == null)
		{
			imageArray = new ChannelArray<Value>(1, 1, Value.White);
		}
		if (previousWorksheet != workspace.Worksheet)
		{
			gradientGroupPanel.Reset();
			GradientReset();
			noiseGroupPanel.Reset();
			aoGroupPanel.Reset();
			AmbientOcclusionReset();
			curvatureGroupPanel.Reset();
			CurvatureReset();
			adjustmentsGroupPanel.Reset();
			AdjustmentsReset();
			imageGroupPanel.Reset();
			ImageReset();
			blurGroupPanel.Reset();
			BlurReset();
			layerLayerGroupPanel.Reset();
		}
		gradientGroupPanel.Visible = false;
		noiseGroupPanel.Visible = false;
		aoGroupPanel.Visible = false;
		curvatureGroupPanel.Visible = false;
		switch (mask)
		{
		case MaskEnum.GRADIENT:
			base.WindowTitle = Tr(MaskEnum.GRADIENT.ToString());
			gradientGroupPanel.Visible = true;
			gradientOrientationOptionButton.Selected = (int)gradientOrientation;
			gradientPositionAdvancedSlider.Value = gradientPosition;
			gradientCenterXAdvancedSlider.IntValue = gradientCenterX;
			gradientCenterYAdvancedSlider.IntValue = gradientCenterY;
			gradientScaleAdvancedSlider.Value = gradientScale;
			break;
		case MaskEnum.NOISE:
			base.WindowTitle = Tr(MaskEnum.NOISE.ToString());
			noiseGroupPanel.Visible = true;
			noiseGroupPanel.HideButtonToggled(pressed: false);
			noiseGroupPanel.UpdateArray();
			break;
		case MaskEnum.AMBIENTOCCLUSION:
			base.WindowTitle = Tr(MaskEnum.AMBIENTOCCLUSION.ToString());
			aoGroupPanel.Visible = true;
			aoAllLayersCheckButton.Pressed = aoAllLayers;
			aoModeOptionButton.Selected = (int)aoMode;
			aoRaysCount = aoRaysCountAdvancedSlider.IntValue;
			aoDistance = aoDistanceAdvancedSlider.Value;
			aoBias = aoBiasAdvancedSlider.Value;
			aoIntensity = aoIntensityAdvancedSlider.Value;
			break;
		case MaskEnum.CURVATURE:
			base.WindowTitle = Tr(MaskEnum.CURVATURE.ToString());
			curvatureGroupPanel.Visible = true;
			curvatureDistance = curvatureDistanceAdvancedSlider.IntValue;
			curvatureIntensity = curvatureIntensityAdvancedSlider.Value;
			break;
		case MaskEnum.DEFAULT:
			base.WindowTitle = Tr(MaskEnum.DEFAULT.ToString());
			break;
		}
		adjustmentsBlackAdvancedSlider.Value = adjustmentsBlack;
		adjustmentsWhiteAdvancedSlider.Value = adjustmentsWhite;
		adjustmentsScaleAdvancedSlider.Value = adjustmentsScale;
		adjustmentsPositionAdvancedSlider.Value = adjustmentsPosition;
		blurDistanceAdvancedSlider.IntValue = blurDistance;
		Register.PreviewspaceMeshManager.ActivateChannelShaderMaterial();
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "channelTexture", outputImageTexture);
		previousWorksheet = workspace.Worksheet;
		GetNodeOrNull<ScrollContainer>("SC").ScrollVertical = 0;
		doUpdateMask = true;
	}

	private void BakingComplete(float[,] inputArray, bool lastChunk)
	{
		maskArray = inputArray;
		if (lastChunk)
		{
			doUpdateBlur = true;
			return;
		}
		bluredMaskArray = maskArray;
		doUpdateAdjustments = true;
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
		string layerName = "Mask: " + base.WindowTitle;
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
		DrawingManager drawingManager = Register.DrawingManager;
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
		Hide();
	}

	private void UpdateMask()
	{
		Register.ThreadsManager.Abort();
		isFinished = false;
		width = workspace.Worksheet.Data.Width;
		height = workspace.Worksheet.Data.Height;
		maskArray = null;
		switch (mask)
		{
		case MaskEnum.GRADIENT:
			maskArray = GradientFilter.MaskArray(width, height, gradientOrientation, gradientCenterX, gradientCenterY, gradientPosition, gradientScale);
			doUpdateBlur = true;
			break;
		case MaskEnum.NOISE:
		{
			if (maskArray == null)
			{
				maskArray = noiseGroupPanel.Array.Array;
			}
			for (int y = 0; y < noiseGroupPanel.Array.Height; y++)
			{
				for (int x = 0; x < noiseGroupPanel.Array.Width; x++)
				{
					maskArray[x, y] = noiseGroupPanel.Array[x, y];
				}
			}
			doUpdateBlur = true;
			break;
		}
		case MaskEnum.AMBIENTOCCLUSION:
			AmbientOcclusionFilter.MaskArray(BakingComplete, workspace.Worksheet, aoDistance, aoBias, aoRaysCount, aoMode, aoIntensity, aoAllLayers);
			break;
		case MaskEnum.CURVATURE:
			maskArray = CurvatureFilter.MaskArray(workspace.Worksheet.Data.NormalChannel.Array, width, height, workspace.Worksheet.Data.Tileable, curvatureDistance, curvatureIntensity);
			doUpdateBlur = true;
			break;
		}
		doUpdateMask = false;
	}

	private void UpdateBlur()
	{
		isFinished = false;
		bluredMaskArray = null;
		if (maskArray != null)
		{
			if (blurDistance > 0)
			{
				bluredMaskArray = BlurFilter.FloatArray(maskArray, workspace.Worksheet.Data.Width, workspace.Worksheet.Data.Height, workspace.Worksheet.Data.Tileable, blurDistance);
			}
			else
			{
				bluredMaskArray = maskArray;
			}
		}
		doUpdateBlur = false;
		doUpdateAdjustments = true;
	}

	private void UpdateAdjustments()
	{
		isFinished = false;
		adjustedArray = null;
		if (bluredMaskArray != null)
		{
			width = workspace.Worksheet.Data.Width;
			height = workspace.Worksheet.Data.Height;
			adjustedArray = new float[width, height];
			float range = adjustmentsWhite - adjustmentsBlack;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (!Register.SelectionManager.Enabled || workspace.Worksheet.Data.SelectionArray[x, y])
					{
						float value = bluredMaskArray[x, y];
						value = Mathf.Clamp((value - adjustmentsPosition) / adjustmentsScale + 0.5f, 0f, 1f);
						value = adjustmentsBlack + value * range;
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
			width = workspace.Worksheet.Data.Width;
			height = workspace.Worksheet.Data.Height;
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
			outputImageTexture.CreateFromImage(outputImage, 2u);
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

	public void BlurReset()
	{
		blurDistanceAdvancedSlider.Reset();
		blurDistance = blurDistanceAdvancedSlider.IntValue;
		doUpdateBlur = true;
	}

	public void BlurDistanceChanged(float value)
	{
		blurDistance = blurDistanceAdvancedSlider.IntValue;
		doUpdateBlur = true;
	}

	public void AdjustmentsReset()
	{
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

	public void GradientReset()
	{
		gradientOrientation = GradientFilter.OrientationEnum.Y;
		gradientOrientationOptionButton.Selected = (int)gradientOrientation;
		gradientPositionAdvancedSlider.Reset();
		gradientPosition = gradientPositionAdvancedSlider.Value;
		gradientCenterXAdvancedSlider.Reset();
		gradientCenterX = gradientCenterXAdvancedSlider.IntValue;
		gradientCenterYAdvancedSlider.Reset();
		gradientCenterY = gradientCenterYAdvancedSlider.IntValue;
		gradientScaleAdvancedSlider.Reset();
		gradientScale = gradientScaleAdvancedSlider.Value;
		doUpdateMask = true;
	}

	public void GradientOrientationSelected(int index)
	{
		gradientOrientation = (GradientFilter.OrientationEnum)index;
		if (gradientOrientation == GradientFilter.OrientationEnum.Radial)
		{
			gradientCenterXAdvancedSlider.Disabled = false;
			gradientCenterYAdvancedSlider.Disabled = false;
			gradientPositionAdvancedSlider.MaxValue = 4f;
		}
		else
		{
			gradientCenterXAdvancedSlider.Disabled = true;
			gradientCenterYAdvancedSlider.Disabled = true;
			gradientPositionAdvancedSlider.MaxValue = 1f;
		}
		gradientPosition = gradientPositionAdvancedSlider.Value;
		doUpdateMask = true;
	}

	public void GradiantPositionChanged(float value)
	{
		gradientPosition = gradientPositionAdvancedSlider.Value;
		doUpdateMask = true;
	}

	public void GradientCenterXChanged(float value)
	{
		gradientCenterX = gradientCenterXAdvancedSlider.IntValue;
		doUpdateMask = true;
	}

	public void GradientCenterYChanged(float value)
	{
		gradientCenterY = gradientCenterYAdvancedSlider.IntValue;
		doUpdateMask = true;
	}

	public void GradiantScaleChanged(float value)
	{
		gradientScale = gradientScaleAdvancedSlider.Value;
		doUpdateMask = true;
	}

	public void AmbientOcclusionReset()
	{
		aoAllLayers = true;
		aoAllLayersCheckButton.Pressed = aoAllLayers;
		aoMode = AmbientOcclusionFilter.RayModeEnum.RANDOM;
		aoModeOptionButton.Selected = (int)aoMode;
		aoRaysCountAdvancedSlider.Reset();
		aoRaysCount = aoRaysCountAdvancedSlider.IntValue;
		aoDistanceAdvancedSlider.Reset();
		aoDistance = aoDistanceAdvancedSlider.Value;
		aoBiasAdvancedSlider.Reset();
		aoBias = aoBiasAdvancedSlider.Value;
		aoIntensityAdvancedSlider.Reset();
		aoIntensity = aoIntensityAdvancedSlider.Value;
		doUpdateMask = true;
	}

	public void AoAllLayersToggled(bool pressed)
	{
		aoAllLayers = pressed;
		doUpdateMask = true;
	}

	public void AoModeSelected(int index)
	{
		aoMode = (AmbientOcclusionFilter.RayModeEnum)index;
		doUpdateMask = true;
	}

	public void AoRaysCountChanged(float value)
	{
		aoRaysCount = aoRaysCountAdvancedSlider.IntValue;
		doUpdateMask = true;
	}

	public void AoDistanceChanged(float value)
	{
		aoDistance = aoDistanceAdvancedSlider.Value;
		doUpdateMask = true;
	}

	public void AoBiasChanged(float value)
	{
		aoBias = aoBiasAdvancedSlider.Value;
		doUpdateMask = true;
	}

	public void AoIntensityChanged(float value)
	{
		aoIntensity = aoIntensityAdvancedSlider.Value;
		doUpdateMask = true;
	}

	public void CurvatureReset()
	{
		curvatureDistanceAdvancedSlider.Reset();
		curvatureDistance = curvatureDistanceAdvancedSlider.IntValue;
		curvatureIntensityAdvancedSlider.Reset();
		curvatureIntensity = curvatureIntensityAdvancedSlider.Value;
		doUpdateMask = true;
	}

	public void CurvatureDistanceChanged(float value)
	{
		curvatureDistance = curvatureDistanceAdvancedSlider.IntValue;
		doUpdateMask = true;
	}

	public void CurvatureIntensityChanged(float value)
	{
		curvatureIntensity = curvatureIntensityAdvancedSlider.Value;
		doUpdateMask = true;
	}
}
