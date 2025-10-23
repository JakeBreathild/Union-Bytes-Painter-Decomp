using System.IO;
using Godot;

public class FilterWindowDialog : WindowDialog
{
	public enum FilterEnum
	{
		DEFAULT,
		CORRECTION,
		BLUR,
		FLOW
	}

	private Workspace workspace;

	private Worksheet previousWorksheet;

	private int width;

	private int height;

	private FilterEnum filter;

	private FilterEnum previousFilter;

	private Value[,] filterValueArray;

	private Color[,] filterColorArray;

	private bool isFinished;

	private bool doUpdateSource;

	private bool doUpdateFilter;

	private bool doUpdateImage;

	private bool doUpdateOutput;

	private OptionButton sourceChannelOptionButton;

	private Layer.ChannelEnum sourceChannel = Layer.ChannelEnum.ALL;

	private Value[,] sourceValueArray;

	private Value sourceDefaultValue = Value.Zero;

	private Color[,] sourceColorArray;

	private Color sourceDefaultColor = ColorExtension.Zero;

	private Image outputImage;

	private ImageTexture outputImageTexture;

	private TextureRect outputTextureRect;

	private ChannelArray outputArray;

	private Panel previewPanel;

	private TextureRect previewTextureRect;

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

	private Blender.BlendingModeEnum imageBlendingMode;

	private GroupPanel blendingGroupPanel;

	private OptionButton blendingModeOptionButton;

	private AdvancedSlider blendingStrengthAdvancedSlider;

	private Blender.BlendingModeEnum blendingMode;

	private float blendingStrength = 1f;

	private GroupPanel correctionGroupPanel;

	private Button correctionInvertButton;

	private AdvancedSlider correctionBlackAdvancedSlider;

	private AdvancedSlider correctionWhiteAdvancedSlider;

	private AdvancedSlider correctionPowerAdvancedSlider;

	private AdvancedSlider correctionSaturationAdvancedSlider;

	private AdvancedSlider correctionHueAdvancedSlider;

	private float correctionBlack;

	private float correctionWhite = 1f;

	private float correctionPower = 1f;

	private float correctionSaturation = 1f;

	private float correctionHue;

	private GroupPanel blurGroupPanel;

	private AdvancedSlider blurDistanceAdvancedSlider;

	private CheckButton blurIncludeAlphaCheckButton;

	private CheckButton blurAllChannelsCheckButton;

	private int blurDistance = 1;

	private bool blurIncludeAlpha = true;

	private bool blurAllChannels = true;

	private NoiseGroupPanel noiseGroupPanel;

	[Export(PropertyHint.None, "")]
	private NodePath flowLoadFileDialogNodePath;

	private PreviewFileDialog flowLoadFileDialog;

	private GroupPanel flowGroupPanel;

	private OptionButton flowSourceOptionButton;

	private OptionButton flowModeOptionButton;

	private Button flowLoadButton;

	private LineEdit flowPathLineEdit;

	private AdvancedSlider flowDirectionAdvancedSlider;

	private AdvancedSlider flowDistanceAdvancedSlider;

	private AdvancedSlider flowStepsAdvancedSlider;

	private CheckButton flowIncludeAlphaCheckButton;

	private CheckButton flowAllChannelsCheckButton;

	private TextureRect flowImagePreviewTextureRect;

	private Image flowImage;

	private ChannelArray<Value> flowImageArray;

	private string flowImagePath = "";

	private FlowFilter.ModeEnum flowMode = FlowFilter.ModeEnum.WARP;

	private float flowDirection;

	private int flowDistance = 1;

	private int flowSteps = 16;

	private bool flowIncludeAlpha = true;

	private bool flowAllChannels = true;

	public FilterEnum Filter
	{
		get
		{
			return filter;
		}
		set
		{
			previousFilter = filter;
			filter = value;
		}
	}

	private bool hasFilterChanged => filter != previousFilter;

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		sourceChannel = Layer.ChannelEnum.COLOR;
		sourceChannelOptionButton = GetNodeOrNull<OptionButton>("Output/Channel");
		sourceChannelOptionButton.AddItem("Color");
		sourceChannelOptionButton.AddItem("Roughness");
		sourceChannelOptionButton.AddItem("Metallicity");
		sourceChannelOptionButton.AddItem("Height");
		sourceChannelOptionButton.AddItem("Emission");
		sourceChannelOptionButton.Connect(Signals.ItemSelected, this, "OutputChannelChanged");
		outputTextureRect = GetNodeOrNull<TextureRect>("Output/Image");
		outputTextureRect.Connect(Signals.MouseEntered, this, "OutputTextureRectMouseEntered");
		outputTextureRect.Connect(Signals.MouseExited, this, "OutputTextureRectMouseExited");
		previewPanel = GetNodeOrNull<Panel>("Preview");
		previewTextureRect = GetNodeOrNull<TextureRect>("Preview/Image");
		GetNodeOrNull<ScrollContainer>("SC").GetVScrollbar().MouseFilter = MouseFilterEnum.Pass;
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
		blendingGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Blending");
		blendingGroupPanel.ResetButtonPressedCallback = BlendingReset;
		blendingModeOptionButton = blendingGroupPanel.GetNodeOrNull<OptionButton>("VC/Mode");
		for (int j = 0; j <= 7; j++)
		{
			blendingModeOptionButton.AddItem(Blender.BlendingModeName[j], j);
		}
		blendingModeOptionButton.Connect(Signals.ItemSelected, this, "BlendingModeChanged");
		blendingStrengthAdvancedSlider = blendingGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Strength");
		blendingStrengthAdvancedSlider.Value = blendingStrength;
		blendingStrengthAdvancedSlider.DefaultValue = blendingStrength;
		blendingStrengthAdvancedSlider.ValueChangedCallback = BlendingStrengthChanged;
		correctionGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Correction");
		correctionGroupPanel.ResetButtonPressedCallback = CorrectionReset;
		correctionInvertButton = correctionGroupPanel.GetNodeOrNull<Button>("VC/Inverse");
		correctionInvertButton.Connect(Signals.Pressed, this, "CorrectionInvertPressed");
		correctionBlackAdvancedSlider = correctionGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Black");
		correctionBlackAdvancedSlider.Value = correctionBlack;
		correctionBlackAdvancedSlider.DefaultValue = correctionBlack;
		correctionBlackAdvancedSlider.ValueChangedCallback = CorrectionBlackChanged;
		correctionWhiteAdvancedSlider = correctionGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/White");
		correctionWhiteAdvancedSlider.Value = correctionWhite;
		correctionWhiteAdvancedSlider.DefaultValue = correctionWhite;
		correctionWhiteAdvancedSlider.ValueChangedCallback = CorrectionWhiteChanged;
		correctionPowerAdvancedSlider = correctionGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Power");
		correctionPowerAdvancedSlider.Value = correctionPower;
		correctionPowerAdvancedSlider.DefaultValue = correctionPower;
		correctionPowerAdvancedSlider.ValueChangedCallback = CorrectionPowerChanged;
		correctionSaturationAdvancedSlider = correctionGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Saturation");
		correctionSaturationAdvancedSlider.Value = correctionSaturation;
		correctionSaturationAdvancedSlider.DefaultValue = correctionSaturation;
		correctionSaturationAdvancedSlider.ValueChangedCallback = CorrectionSaturationChanged;
		correctionHueAdvancedSlider = correctionGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Hue");
		correctionHueAdvancedSlider.Value = correctionHue;
		correctionHueAdvancedSlider.DefaultValue = correctionHue;
		correctionHueAdvancedSlider.ValueChangedCallback = CorrectionHueChanged;
		blurGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Blur");
		blurGroupPanel.ResetButtonPressedCallback = BlurReset;
		blurDistanceAdvancedSlider = blurGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Distance");
		blurDistanceAdvancedSlider.IntValue = blurDistance;
		blurDistanceAdvancedSlider.DefaultIntValue = blurDistance;
		blurDistanceAdvancedSlider.ValueChangedCallback = BlurDistanceChanged;
		blurIncludeAlphaCheckButton = blurGroupPanel.GetNodeOrNull<CheckButton>("VC/IncludeAlpha");
		blurIncludeAlphaCheckButton.Connect(Signals.Toggled, this, "BlurIncludeAlphaToggled");
		blurAllChannelsCheckButton = blurGroupPanel.GetNodeOrNull<CheckButton>("VC/AllChannels");
		blurAllChannelsCheckButton.Connect(Signals.Toggled, this, "BlurAllChannelsToggled");
		noiseGroupPanel = GetNodeOrNull<NoiseGroupPanel>("SC/VC/Noise");
		noiseGroupPanel.UpdateCallback = NoiseUpdate;
		flowLoadFileDialog = GetNodeOrNull<PreviewFileDialog>(flowLoadFileDialogNodePath);
		flowLoadFileDialog.Connect(Signals.FileSelected, this, "FlowFileSelected");
		flowGroupPanel = GetNodeOrNull<GroupPanel>("SC/VC/Flow");
		flowSourceOptionButton = flowGroupPanel.GetNodeOrNull<OptionButton>("VC/Source");
		flowSourceOptionButton.AddItem("Image", 0);
		flowSourceOptionButton.AddItem("Height", 1);
		flowSourceOptionButton.AddItem("Noise", 2);
		flowSourceOptionButton.Selected = 0;
		flowSourceOptionButton.GetPopup().Connect(Signals.IdPressed, this, "FlowSourceSelected");
		flowLoadButton = flowGroupPanel.GetNodeOrNull<Button>("VC/Load");
		flowLoadButton.Connect(Signals.Pressed, this, "FlowLoadPressed");
		flowPathLineEdit = flowGroupPanel.GetNodeOrNull<LineEdit>("VC/Path");
		flowImagePreviewTextureRect = flowGroupPanel.GetNodeOrNull<TextureRect>("VC/Preview");
		flowGroupPanel.ResetButtonPressedCallback = FlowReset;
		flowDirectionAdvancedSlider = flowGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Direction");
		flowModeOptionButton = flowGroupPanel.GetNodeOrNull<OptionButton>("VC/Mode");
		for (int k = 0; k <= 3; k++)
		{
			OptionButton optionButton = flowModeOptionButton;
			FlowFilter.ModeEnum modeEnum = (FlowFilter.ModeEnum)k;
			optionButton.AddItem(modeEnum.ToString(), k);
		}
		flowModeOptionButton.Selected = (int)flowMode;
		flowModeOptionButton.GetPopup().Connect(Signals.IdPressed, this, "FlowModeSelected");
		flowDirectionAdvancedSlider.Value = flowDirection;
		flowDirectionAdvancedSlider.DefaultValue = flowDirection;
		flowDirectionAdvancedSlider.ValueChangedCallback = FlowDirectionChanged;
		flowDistanceAdvancedSlider = flowGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Distance");
		flowDistanceAdvancedSlider.IntValue = flowDistance;
		flowDistanceAdvancedSlider.DefaultIntValue = flowDistance;
		flowDistanceAdvancedSlider.ValueChangedCallback = FlowDistanceChanged;
		flowStepsAdvancedSlider = flowGroupPanel.GetNodeOrNull<AdvancedSlider>("VC/Steps");
		flowStepsAdvancedSlider.IntValue = flowSteps;
		flowStepsAdvancedSlider.DefaultIntValue = flowSteps;
		flowStepsAdvancedSlider.ValueChangedCallback = FlowStepsChanged;
		flowIncludeAlphaCheckButton = flowGroupPanel.GetNodeOrNull<CheckButton>("VC/IncludeAlpha");
		flowIncludeAlphaCheckButton.Connect(Signals.Toggled, this, "FlowIncludeAlphaToggled");
		flowAllChannelsCheckButton = flowGroupPanel.GetNodeOrNull<CheckButton>("VC/AllChannels");
		flowAllChannelsCheckButton.Connect(Signals.Toggled, this, "FlowAllChannelsToggled");
		Connect(Signals.Hide, this, "Hide");
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (doUpdateSource)
		{
			UpdateSource();
		}
		if (doUpdateFilter)
		{
			UpdateFilter();
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
		InputManager.WindowShown();
		Reset();
		base.PopupCentered(size);
	}

	public new void Show()
	{
		InputManager.WindowShown();
		Reset();
		base.Show();
	}

	public new void Hide()
	{
		base.Hide();
		Register.ThreadsManager.Abort();
		InputManager.WindowHidden();
		InputManager.SkipInput = true;
	}

	public void Reset()
	{
		int lastWidth = width;
		int lastHeight = height;
		width = workspace.Worksheet.Data.Width;
		height = workspace.Worksheet.Data.Height;
		if (width != lastWidth || height != lastHeight || hasFilterChanged)
		{
			outputTextureRect.Texture = null;
			outputImage = new Image();
			outputImage.Create(width, height, useMipmaps: false, Image.Format.Rgba8);
			outputImageTexture = new ImageTexture();
			isFinished = false;
			ImageReset();
			BlendingReset();
		}
		if (previousWorksheet != workspace.Worksheet)
		{
			sourceChannel = Layer.ChannelEnum.COLOR;
			sourceChannelOptionButton.Selected = (int)sourceChannel;
			sourceColorArray = workspace.Worksheet.Data.Layer.ColorChannel.Array;
			correctionGroupPanel.Reset();
			CorrectionReset();
			blurGroupPanel.Reset();
			BlurReset();
			noiseGroupPanel.Reset();
			flowGroupPanel.Reset();
			FlowReset();
			imageGroupPanel.Reset();
			ImageReset();
			blendingGroupPanel.Reset();
			BlendingReset();
		}
		correctionGroupPanel.Visible = false;
		blurGroupPanel.Visible = false;
		flowGroupPanel.Visible = false;
		noiseGroupPanel.Visible = false;
		switch (filter)
		{
		case FilterEnum.CORRECTION:
			base.WindowTitle = Tr(FilterEnum.CORRECTION.ToString());
			correctionGroupPanel.Visible = true;
			correctionBlackAdvancedSlider.Value = correctionBlack;
			correctionWhiteAdvancedSlider.Value = correctionWhite;
			correctionPowerAdvancedSlider.Value = correctionPower;
			correctionSaturationAdvancedSlider.Value = correctionSaturation;
			correctionHueAdvancedSlider.Value = correctionHue;
			break;
		case FilterEnum.BLUR:
			base.WindowTitle = Tr(FilterEnum.BLUR.ToString());
			blurGroupPanel.Visible = true;
			blurDistanceAdvancedSlider.IntValue = blurDistance;
			blurIncludeAlphaCheckButton.Pressed = blurIncludeAlpha;
			blurAllChannelsCheckButton.Pressed = blurAllChannels;
			break;
		case FilterEnum.FLOW:
			base.WindowTitle = Tr(FilterEnum.FLOW.ToString());
			noiseGroupPanel.Visible = true;
			noiseGroupPanel.HideButtonToggled(pressed: true);
			flowGroupPanel.Visible = true;
			flowModeOptionButton.Selected = (int)flowMode;
			flowDirectionAdvancedSlider.Value = flowDirection;
			flowDistanceAdvancedSlider.IntValue = flowDistance;
			flowStepsAdvancedSlider.IntValue = flowSteps;
			flowIncludeAlphaCheckButton.Pressed = flowIncludeAlpha;
			flowAllChannelsCheckButton.Pressed = flowAllChannels;
			break;
		default:
			base.WindowTitle = Tr(FilterEnum.DEFAULT.ToString());
			break;
		}
		blendingModeOptionButton.Selected = (int)blendingMode;
		blendingStrengthAdvancedSlider.Value = blendingStrength;
		previousWorksheet = workspace.Worksheet;
		GetNodeOrNull<ScrollContainer>("SC").ScrollVertical = 0;
		doUpdateSource = true;
	}

	private void ApplyValueArraySelection(Value[,] sourceFloatArray, Value[,] floatArray, int width, int height)
	{
		if (Register.SelectionManager.Enabled)
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (!workspace.Worksheet.Data.SelectionArray[x, y])
					{
						floatArray[x, y] = sourceFloatArray[x, y];
					}
				}
			}
		}
		if (imageArray == null)
		{
			return;
		}
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				if (!Register.SelectionManager.Enabled || workspace.Worksheet.Data.SelectionArray[j, i])
				{
					int modX = Mathf.PosMod(j, imageArray.Width);
					int modY = Mathf.PosMod(i, imageArray.Height);
					floatArray[j, i] = Blender.Blend(sourceFloatArray[j, i], floatArray[j, i], imageBlendingMode, imageArray[modX, modY].v * imageArray[modX, modY].a);
				}
			}
		}
	}

	private void ApplyColorArraySelection(Color[,] sourceColorArray, Color[,] colorArray, int width, int height)
	{
		if (Register.SelectionManager.Enabled)
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (!workspace.Worksheet.Data.SelectionArray[x, y])
					{
						colorArray[x, y] = sourceColorArray[x, y];
					}
				}
			}
		}
		if (imageArray == null)
		{
			return;
		}
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				if (!Register.SelectionManager.Enabled || workspace.Worksheet.Data.SelectionArray[j, i])
				{
					int modX = Mathf.PosMod(j, imageArray.Width);
					int modY = Mathf.PosMod(i, imageArray.Height);
					colorArray[j, i] = Blender.Blend(sourceColorArray[j, i], colorArray[j, i], imageBlendingMode, imageArray[modX, modY].v * imageArray[modX, modY].a);
				}
			}
		}
	}

	public void Apply()
	{
		if (!isFinished || outputArray == null)
		{
			return;
		}
		if (Filter == FilterEnum.BLUR && blurAllChannels)
		{
			FilterCommand filterCommand = (FilterCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.FILTER);
			filterCommand.ChannelType = Layer.ChannelEnum.ALL;
			filterCommand.Layer = workspace.Worksheet.Data.Layer;
			if (workspace.Worksheet.Data.Layer.ColorChannel.HasContent)
			{
				filterCommand.ColorArray = BlurFilter.ColorArray(filterCommand.Layer.ColorChannel.Array, width, height, workspace.Worksheet.Data.Tileable, blurDistance, blurIncludeAlpha);
				ApplyColorArraySelection(filterCommand.Layer.ColorChannel.Array, filterCommand.ColorArray, width, height);
			}
			if (workspace.Worksheet.Data.Layer.RoughnessChannel.HasContent)
			{
				filterCommand.ValueArray = BlurFilter.ValueArray(filterCommand.Layer.RoughnessChannel.Array, width, height, workspace.Worksheet.Data.Tileable, blurDistance, blurIncludeAlpha);
				ApplyValueArraySelection(filterCommand.Layer.RoughnessChannel.Array, filterCommand.ValueArray, width, height);
			}
			if (workspace.Worksheet.Data.Layer.MetallicityChannel.HasContent)
			{
				filterCommand.ValueArray2 = BlurFilter.ValueArray(filterCommand.Layer.MetallicityChannel.Array, width, height, workspace.Worksheet.Data.Tileable, blurDistance, blurIncludeAlpha);
				ApplyValueArraySelection(filterCommand.Layer.MetallicityChannel.Array, filterCommand.ValueArray2, width, height);
			}
			if (workspace.Worksheet.Data.Layer.HeightChannel.HasContent)
			{
				filterCommand.ValueArray3 = BlurFilter.ValueArray(filterCommand.Layer.HeightChannel.Array, width, height, workspace.Worksheet.Data.Tileable, blurDistance, blurIncludeAlpha);
				ApplyValueArraySelection(filterCommand.Layer.HeightChannel.Array, filterCommand.ValueArray3, width, height);
			}
			if (workspace.Worksheet.Data.Layer.EmissionChannel.HasContent)
			{
				filterCommand.ColorArray2 = BlurFilter.ColorArray(filterCommand.Layer.EmissionChannel.Array, width, height, workspace.Worksheet.Data.Tileable, blurDistance, blurIncludeAlpha);
				ApplyColorArraySelection(filterCommand.Layer.EmissionChannel.Array, filterCommand.ColorArray2, width, height);
			}
			workspace.Worksheet.History.StopRecording(base.WindowTitle + " Filter [" + workspace.Worksheet.Data.Layer.Name + " (All)]");
		}
		else if (Filter == FilterEnum.FLOW && flowAllChannels && flowImageArray != null)
		{
			FilterCommand filterCommand2 = (FilterCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.FILTER);
			filterCommand2.ChannelType = Layer.ChannelEnum.ALL;
			filterCommand2.Layer = workspace.Worksheet.Data.Layer;
			if (workspace.Worksheet.Data.Layer.ColorChannel.HasContent)
			{
				filterCommand2.ColorArray = FlowFilter.ColorArray(filterCommand2.Layer.ColorChannel.Array, flowImageArray, width, height, flowMode, flowDirection, flowDistance, flowSteps, flowIncludeAlpha);
				ApplyColorArraySelection(filterCommand2.Layer.ColorChannel.Array, filterCommand2.ColorArray, width, height);
			}
			if (workspace.Worksheet.Data.Layer.RoughnessChannel.HasContent)
			{
				filterCommand2.ValueArray = FlowFilter.ValueArray(filterCommand2.Layer.RoughnessChannel.Array, flowImageArray, width, height, flowMode, flowDirection, flowDistance, flowSteps, flowIncludeAlpha);
				ApplyValueArraySelection(filterCommand2.Layer.RoughnessChannel.Array, filterCommand2.ValueArray, width, height);
			}
			if (workspace.Worksheet.Data.Layer.MetallicityChannel.HasContent)
			{
				filterCommand2.ValueArray2 = FlowFilter.ValueArray(filterCommand2.Layer.MetallicityChannel.Array, flowImageArray, width, height, flowMode, flowDirection, flowDistance, flowSteps, flowIncludeAlpha);
				ApplyValueArraySelection(filterCommand2.Layer.MetallicityChannel.Array, filterCommand2.ValueArray2, width, height);
			}
			if (workspace.Worksheet.Data.Layer.HeightChannel.HasContent)
			{
				filterCommand2.ValueArray3 = FlowFilter.ValueArray(filterCommand2.Layer.HeightChannel.Array, flowImageArray, width, height, flowMode, flowDirection, flowDistance, flowSteps, flowIncludeAlpha);
				ApplyValueArraySelection(filterCommand2.Layer.HeightChannel.Array, filterCommand2.ValueArray3, width, height);
			}
			if (workspace.Worksheet.Data.Layer.EmissionChannel.HasContent)
			{
				filterCommand2.ColorArray2 = FlowFilter.ColorArray(filterCommand2.Layer.EmissionChannel.Array, flowImageArray, width, height, flowMode, flowDirection, flowDistance, flowSteps, flowIncludeAlpha);
				ApplyColorArraySelection(filterCommand2.Layer.EmissionChannel.Array, filterCommand2.ColorArray2, width, height);
			}
			workspace.Worksheet.History.StopRecording(base.WindowTitle + " Filter [" + workspace.Worksheet.Data.Layer.Name + " (All)]");
		}
		else
		{
			FilterCommand filterCommand3 = (FilterCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.FILTER);
			switch (sourceChannel)
			{
			case Layer.ChannelEnum.COLOR:
				filterCommand3.ChannelType = Layer.ChannelEnum.COLOR;
				filterCommand3.Channel = workspace.Worksheet.Data.Layer.ColorChannel;
				filterCommand3.ColorArray = outputArray.CastToColor().Array;
				break;
			case Layer.ChannelEnum.ROUGHNESS:
				filterCommand3.ChannelType = Layer.ChannelEnum.ROUGHNESS;
				filterCommand3.Channel = workspace.Worksheet.Data.Layer.RoughnessChannel;
				filterCommand3.ValueArray = outputArray.CastToValue().Array;
				break;
			case Layer.ChannelEnum.METALLICITY:
				filterCommand3.ChannelType = Layer.ChannelEnum.METALLICITY;
				filterCommand3.Channel = workspace.Worksheet.Data.Layer.MetallicityChannel;
				filterCommand3.ValueArray = outputArray.CastToValue().Array;
				break;
			case Layer.ChannelEnum.HEIGHT:
				filterCommand3.ChannelType = Layer.ChannelEnum.HEIGHT;
				filterCommand3.Channel = workspace.Worksheet.Data.Layer.HeightChannel;
				filterCommand3.ValueArray = outputArray.CastToValue().Array;
				break;
			case Layer.ChannelEnum.EMISSION:
				filterCommand3.ChannelType = Layer.ChannelEnum.EMISSION;
				filterCommand3.Channel = workspace.Worksheet.Data.Layer.EmissionChannel;
				filterCommand3.ColorArray = outputArray.CastToColor().Array;
				break;
			}
			workspace.Worksheet.History.StopRecording(base.WindowTitle + " Filter [" + workspace.Worksheet.Data.Layer.Name + " (" + Layer.ChannelName[(int)sourceChannel] + ")]");
		}
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(workspace.Worksheet);
		}
		Hide();
	}

	private void UpdateSource()
	{
		isFinished = false;
		sourceValueArray = null;
		sourceColorArray = null;
		Layer layer = workspace.Worksheet.Data.Layer;
		switch (sourceChannel)
		{
		case Layer.ChannelEnum.COLOR:
			sourceColorArray = layer.ColorChannel.Array;
			sourceDefaultColor = layer.ColorChannel.DefaultValue;
			break;
		case Layer.ChannelEnum.ROUGHNESS:
			sourceValueArray = layer.RoughnessChannel.Array;
			sourceDefaultValue = layer.RoughnessChannel.DefaultValue;
			break;
		case Layer.ChannelEnum.METALLICITY:
			sourceValueArray = layer.MetallicityChannel.Array;
			sourceDefaultValue = layer.MetallicityChannel.DefaultValue;
			break;
		case Layer.ChannelEnum.HEIGHT:
			sourceValueArray = layer.HeightChannel.Array;
			sourceDefaultValue = layer.HeightChannel.DefaultValue;
			break;
		case Layer.ChannelEnum.EMISSION:
			sourceColorArray = layer.EmissionChannel.Array;
			sourceDefaultColor = layer.EmissionChannel.DefaultValue;
			break;
		}
		doUpdateSource = false;
		doUpdateFilter = true;
	}

	public void UpdateFilter()
	{
		Register.ThreadsManager.Abort();
		isFinished = false;
		width = workspace.Worksheet.Data.Width;
		height = workspace.Worksheet.Data.Height;
		filterValueArray = null;
		filterColorArray = null;
		if (sourceValueArray != null || sourceColorArray != null)
		{
			switch (Filter)
			{
			case FilterEnum.CORRECTION:
				if (sourceColorArray != null)
				{
					filterColorArray = CorrectionFilter.ColorArray(sourceColorArray, width, height, correctionBlack, correctionWhite, correctionPower, correctionSaturation, correctionHue, blendingMode, blendingStrength);
				}
				else if (sourceValueArray != null)
				{
					filterValueArray = CorrectionFilter.ValueArray(sourceValueArray, width, height, correctionBlack, correctionWhite, correctionPower, blendingMode, blendingStrength);
				}
				break;
			case FilterEnum.BLUR:
				if (sourceColorArray != null)
				{
					filterColorArray = BlurFilter.ColorArray(sourceColorArray, width, height, workspace.Worksheet.Data.Tileable, blurDistance, blurIncludeAlpha);
				}
				else
				{
					filterValueArray = BlurFilter.ValueArray(sourceValueArray, width, height, workspace.Worksheet.Data.Tileable, blurDistance);
				}
				break;
			case FilterEnum.FLOW:
				if (sourceColorArray != null && flowImageArray != null)
				{
					filterColorArray = FlowFilter.ColorArray(sourceColorArray, flowImageArray, width, height, flowMode, flowDirection, flowDistance, flowSteps, flowIncludeAlpha);
				}
				else if (flowImageArray != null)
				{
					filterValueArray = FlowFilter.ValueArray(sourceValueArray, flowImageArray, width, height, flowMode, flowDirection, flowDistance, flowSteps, flowIncludeAlpha);
				}
				break;
			}
		}
		doUpdateFilter = false;
		doUpdateImage = true;
	}

	public void UpdateImage()
	{
		isFinished = false;
		width = workspace.Worksheet.Data.Width;
		height = workspace.Worksheet.Data.Height;
		outputArray = null;
		if (filterValueArray != null || filterColorArray != null)
		{
			if (filterColorArray != null)
			{
				ChannelArray<Color> outputColorArray = new ChannelArray<Color>((Color[,])filterColorArray.Clone(), width, height);
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						if ((Register.SelectionManager.Enabled && !workspace.Worksheet.Data.SelectionArray[x, y]) || sourceColorArray[x, y] == sourceDefaultColor)
						{
							outputColorArray[x, y] = sourceColorArray[x, y];
						}
					}
				}
				if (imageArray != null)
				{
					for (int i = 0; i < height; i++)
					{
						for (int j = 0; j < width; j++)
						{
							if (!Register.SelectionManager.Enabled || workspace.Worksheet.Data.SelectionArray[j, i])
							{
								int modX = Mathf.PosMod(j, imageArray.Width);
								int modY = Mathf.PosMod(i, imageArray.Height);
								outputColorArray.Array[j, i] = Blender.Blend(sourceColorArray[j, i], outputColorArray.Array[j, i], imageBlendingMode, imageArray[modX, modY].v * imageArray[modX, modY].a);
							}
						}
					}
				}
				outputArray = outputColorArray;
			}
			else if (filterValueArray != null)
			{
				ChannelArray<Value> outputValueArray = new ChannelArray<Value>((Value[,])filterValueArray.Clone(), width, height);
				for (int k = 0; k < height; k++)
				{
					for (int l = 0; l < width; l++)
					{
						if ((Register.SelectionManager.Enabled && !workspace.Worksheet.Data.SelectionArray[l, k]) || sourceValueArray[l, k] == sourceDefaultValue)
						{
							outputValueArray[l, k] = sourceValueArray[l, k];
						}
					}
				}
				if (imageArray != null)
				{
					for (int m = 0; m < height; m++)
					{
						for (int n = 0; n < width; n++)
						{
							if (!Register.SelectionManager.Enabled || workspace.Worksheet.Data.SelectionArray[n, m])
							{
								int modX2 = Mathf.PosMod(n, imageArray.Width);
								int modY2 = Mathf.PosMod(m, imageArray.Height);
								outputValueArray.Array[n, m] = Blender.Blend(sourceValueArray[n, m], outputValueArray.Array[n, m], imageBlendingMode, imageArray[modX2, modY2].v * imageArray[modX2, modY2].a);
							}
						}
					}
				}
				outputArray = outputValueArray;
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

	public void OutputChannelChanged(int index)
	{
		sourceChannel = (Layer.ChannelEnum)index;
		doUpdateSource = true;
	}

	public void ImageReset()
	{
		imageBlendingMode = Blender.BlendingModeEnum.NORMAL;
		imageBlendingModeOptionButton.Selected = (int)imageBlendingMode;
		ImageRemovePressed();
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

	public void ImageRemovePressed()
	{
		image = null;
		imageArray = null;
		imagePath = "";
		imagePathLineEdit.Text = imagePath;
		doUpdateImage = true;
	}

	public void ImageBlendingModeSelected(int blendingModeIndex)
	{
		imageBlendingMode = (Blender.BlendingModeEnum)blendingModeIndex;
		doUpdateImage = true;
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

	public void BlendingReset()
	{
		blendingMode = Blender.BlendingModeEnum.NORMAL;
		blendingModeOptionButton.Selected = (int)blendingMode;
		blendingStrengthAdvancedSlider.Reset();
		blendingStrength = blendingStrengthAdvancedSlider.Value;
		doUpdateOutput = true;
	}

	public void BlendingModeChanged(int index)
	{
		blendingMode = (Blender.BlendingModeEnum)index;
		doUpdateOutput = true;
	}

	public void BlendingStrengthChanged(float value)
	{
		blendingStrength = blendingStrengthAdvancedSlider.Value;
		doUpdateOutput = true;
	}

	public void CorrectionReset()
	{
		correctionBlackAdvancedSlider.Reset();
		correctionBlack = correctionBlackAdvancedSlider.Value;
		correctionWhiteAdvancedSlider.Reset();
		correctionWhite = correctionWhiteAdvancedSlider.Value;
		correctionPowerAdvancedSlider.Reset();
		correctionPower = correctionPowerAdvancedSlider.Value;
		correctionSaturationAdvancedSlider.Reset();
		correctionSaturation = correctionSaturationAdvancedSlider.Value;
		correctionHueAdvancedSlider.Reset();
		correctionHue = correctionHueAdvancedSlider.Value;
		doUpdateFilter = true;
	}

	public void CorrectionInvertPressed()
	{
		float num = correctionBlack;
		float num2 = correctionWhite;
		correctionWhite = num;
		correctionBlack = num2;
		correctionBlackAdvancedSlider.Value = correctionBlack;
		correctionWhiteAdvancedSlider.Value = correctionWhite;
		doUpdateFilter = true;
	}

	public void CorrectionBlackChanged(float value)
	{
		correctionBlack = correctionBlackAdvancedSlider.Value;
		doUpdateFilter = true;
	}

	public void CorrectionWhiteChanged(float value)
	{
		correctionWhite = correctionWhiteAdvancedSlider.Value;
		doUpdateFilter = true;
	}

	public void CorrectionPowerChanged(float value)
	{
		correctionPower = correctionPowerAdvancedSlider.Value;
		doUpdateFilter = true;
	}

	public void CorrectionSaturationChanged(float value)
	{
		correctionSaturation = correctionSaturationAdvancedSlider.Value;
		doUpdateFilter = true;
	}

	public void CorrectionHueChanged(float value)
	{
		correctionHue = correctionHueAdvancedSlider.Value;
		doUpdateFilter = true;
	}

	public void BlurReset()
	{
		blurDistanceAdvancedSlider.Reset();
		blurDistance = blurDistanceAdvancedSlider.IntValue;
		blurIncludeAlpha = true;
		blurIncludeAlphaCheckButton.Pressed = blurIncludeAlpha;
		blurAllChannels = true;
		blurAllChannelsCheckButton.Pressed = blurAllChannels;
		doUpdateFilter = true;
	}

	public void BlurDistanceChanged(float value)
	{
		blurDistance = (int)value;
		doUpdateFilter = true;
	}

	public void BlurIncludeAlphaToggled(bool pressed)
	{
		blurIncludeAlpha = pressed;
		doUpdateFilter = true;
	}

	public void BlurAllChannelsToggled(bool pressed)
	{
		blurAllChannels = pressed;
	}

	public void FlowReset()
	{
		flowImageArray = new ChannelArray<Value>(noiseGroupPanel.Array.Width, noiseGroupPanel.Array.Height, Value.Zero);
		flowSourceOptionButton.Selected = 2;
		flowPathLineEdit.Visible = false;
		flowLoadButton.Visible = false;
		flowImage = null;
		flowImageArray = null;
		flowImagePath = "";
		flowPathLineEdit.Text = flowImagePath;
		flowImagePreviewTextureRect.Texture = null;
		flowMode = FlowFilter.ModeEnum.WARP;
		flowModeOptionButton.Selected = (int)flowMode;
		flowDirectionAdvancedSlider.Reset();
		flowDirection = flowDirectionAdvancedSlider.Value;
		flowDirectionAdvancedSlider.Disabled = true;
		flowDistanceAdvancedSlider.Reset();
		flowDistance = flowDistanceAdvancedSlider.IntValue;
		flowStepsAdvancedSlider.Reset();
		flowSteps = flowStepsAdvancedSlider.IntValue;
		flowIncludeAlpha = true;
		flowIncludeAlphaCheckButton.Pressed = flowIncludeAlpha;
		flowAllChannels = true;
		flowAllChannelsCheckButton.Pressed = flowAllChannels;
		doUpdateFilter = true;
	}

	public void FlowSourceSelected(int index)
	{
		switch (index)
		{
		case 0:
			flowPathLineEdit.Visible = true;
			flowLoadButton.Visible = true;
			if (flowImagePath != "")
			{
				FlowFileSelected(flowImagePath);
				break;
			}
			flowImage = null;
			flowImageArray = null;
			flowImagePath = "";
			flowPathLineEdit.Text = flowImagePath;
			flowImagePreviewTextureRect.Texture = null;
			outputTextureRect.Texture = null;
			break;
		case 1:
		{
			flowPathLineEdit.Visible = false;
			flowLoadButton.Visible = false;
			flowImageArray = new ChannelArray<Value>(workspace.Worksheet.Data.Width, workspace.Worksheet.Data.Height, Value.Zero);
			for (int i = 0; i < flowImageArray.Height; i++)
			{
				for (int j = 0; j < flowImageArray.Width; j++)
				{
					flowImageArray.Array[j, i].v = workspace.Worksheet.Data.HeightChannel[j, i].v;
					flowImageArray.Array[j, i].a = workspace.Worksheet.Data.HeightChannel[j, i].a;
				}
			}
			flowImagePreviewTextureRect.Texture = workspace.Worksheet.Data.HeightChannel.ImageTexture;
			break;
		}
		case 2:
		{
			flowPathLineEdit.Visible = false;
			flowLoadButton.Visible = false;
			flowImageArray = new ChannelArray<Value>(noiseGroupPanel.Array.Width, noiseGroupPanel.Array.Height, Value.Zero);
			for (int y = 0; y < noiseGroupPanel.Array.Height; y++)
			{
				for (int x = 0; x < noiseGroupPanel.Array.Width; x++)
				{
					flowImageArray.Array[x, y].v = noiseGroupPanel.Array[x, y];
					flowImageArray.Array[x, y].a = 1f;
				}
			}
			flowImagePreviewTextureRect.Texture = noiseGroupPanel.Texture;
			break;
		}
		}
		doUpdateFilter = true;
	}

	public void FlowModeSelected(int index)
	{
		flowMode = (FlowFilter.ModeEnum)index;
		if (flowMode == FlowFilter.ModeEnum.WARP)
		{
			flowDirectionAdvancedSlider.Disabled = true;
		}
		else
		{
			flowDirectionAdvancedSlider.Disabled = false;
		}
		doUpdateFilter = true;
	}

	public void FlowDirectionChanged(float value)
	{
		flowDirection = value;
		doUpdateFilter = true;
	}

	public void FlowDistanceChanged(float value)
	{
		flowDistance = (int)value;
		doUpdateFilter = true;
	}

	public void FlowStepsChanged(float value)
	{
		flowSteps = (int)value;
		doUpdateFilter = true;
	}

	public void FlowIncludeAlphaToggled(bool pressed)
	{
		flowIncludeAlpha = pressed;
		doUpdateFilter = true;
	}

	public void FlowAllChannelsToggled(bool pressed)
	{
		flowAllChannels = pressed;
	}

	public void NoiseUpdate()
	{
		if (flowSourceOptionButton.Selected != 2)
		{
			return;
		}
		if (flowImageArray == null)
		{
			flowImageArray = new ChannelArray<Value>(noiseGroupPanel.Array.Width, noiseGroupPanel.Array.Height, Value.Zero);
		}
		for (int y = 0; y < noiseGroupPanel.Array.Height; y++)
		{
			for (int x = 0; x < noiseGroupPanel.Array.Width; x++)
			{
				flowImageArray.Array[x, y].v = noiseGroupPanel.Array[x, y];
				flowImageArray.Array[x, y].a = 1f;
			}
		}
		flowImagePreviewTextureRect.Texture = noiseGroupPanel.Texture;
		doUpdateFilter = true;
	}

	public void FlowLoadPressed()
	{
		string path = Settings.MasksPath;
		if (!path.Empty())
		{
			flowLoadFileDialog.CurrentDir = path;
		}
		flowLoadFileDialog.SetWindowHidden = false;
		flowLoadFileDialog.PopupCentered();
	}

	public void FlowFileSelected(string file)
	{
		file = System.IO.Path.GetFullPath(file);
		if (!System.IO.File.Exists(file))
		{
			return;
		}
		string fileType = file.Substring(file.Length - 3, 3);
		if (fileType == "png" || fileType == "jpg")
		{
			flowImagePath = file;
			flowPathLineEdit.Text = flowImagePath;
			if (flowImage == null)
			{
				flowImage = new Image();
			}
			flowImage.Load(file);
			flowImageArray = new ChannelArray<Value>(flowImage.GetWidth(), flowImage.GetHeight(), Value.Zero);
			flowImage.Lock();
			for (int y = 0; y < flowImageArray.Height; y++)
			{
				for (int x = 0; x < flowImageArray.Width; x++)
				{
					Color color = flowImage.GetPixel(x, y);
					flowImageArray.Array[x, y].v = color.v;
					flowImageArray.Array[x, y].a = color.a;
				}
			}
			flowImage.Unlock();
			flowImagePreviewTextureRect.Texture = new ImageTexture();
			((ImageTexture)flowImagePreviewTextureRect.Texture).CreateFromImage(flowImage, 2u);
			doUpdateFilter = true;
		}
		Settings.MasksPath = System.IO.Path.GetDirectoryName(file);
	}
}
