using Godot;

public class Gui : Control
{
	[Export(PropertyHint.None, "")]
	private NodePath msgAcceptDialogNodePath;

	public static AcceptDialog MsgAcceptDialog;

	[Export(PropertyHint.None, "")]
	private NodePath workspaceNodePath;

	private Workspace workspace;

	[Export(PropertyHint.None, "")]
	private NodePath fileMenuButtonNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath filterMenuButtonNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath bakeMenuButtonNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath helpMenuButtonNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath worksheetLabelNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath versionLabelNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath informationsLabelNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath channelLabelNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath controlIconTextureRectNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath controlsRichTextLabelNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath layerPanelNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath layerControlNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath historyItemListNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath previewViewportContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath displaySettingsContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath brushSettingsContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath lineSettingsContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath fillingBucketSettingsContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath stampSettingsContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath selectSettingsContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath uvEditSettingsContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath toolsContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath materialTabContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath materialContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath channelsContainerNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath currentChannelItemListNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath libraryHoverPanelNodePath;

	public DefaultHoverPanel LibraryHoverPanel;

	[Export(PropertyHint.None, "")]
	private NodePath libraryContainerNodePath;

	public Container LibraryContainer;

	[Export(PropertyHint.None, "")]
	private NodePath settingsContainerNodePath;

	public FileMenuButton FileMenuButton { get; private set; }

	public FilterMenuButton FilterMenuButton { get; private set; }

	public BakeMenuButton BakeMenuButton { get; private set; }

	public HelpMenuButton HelpMenuButton { get; private set; }

	public Label WorksheetLabel { get; private set; }

	public Label VersionLabel { get; private set; }

	public Label InformationsLabel { get; private set; }

	public Label ChannelLabel { get; private set; }

	public TextureRect ControlIconTextureRect { get; private set; }

	public RichTextLabel ControlsRichTextLabel { get; private set; }

	public LayerPanel LayerPanel { get; private set; }

	public LayerControl LayerControl { get; private set; }

	public ItemList HistoryItemList { get; private set; }

	public PreviewspaceViewportContainer PreviewViewportContainer { get; private set; }

	public DisplaySettingsContainer DisplaySettingsContainer { get; private set; }

	public BrushSettingsContainer BrushSettingsContainer { get; private set; }

	public LineSettingsContainer LineSettingsContainer { get; private set; }

	public FillingBucketSettingsContainer FillingBucketSettingsContainer { get; private set; }

	public StampSettingsContainer StampSettingsContainer { get; private set; }

	public SelectSettingsContainer SelectSettingsContainer { get; private set; }

	public UvEditSettingsContainer UvEditSettingsContainer { get; private set; }

	public ToolsContainer ToolsContainer { get; private set; }

	public TabContainer MaterialTabContainer { get; private set; }

	public MaterialContainer MaterialContainer { get; private set; }

	public ChannelsContainer ChannelsContainer { get; private set; }

	public ChannelList CurrentChannelItemList { get; private set; }

	public WorkspaceSettingsContainer SettingsContainer { get; private set; }

	public static void MsgAcceptDialogPopupCentered(string msg, string title = "Error", Vector2? size = null)
	{
		InputManager.MouseEnteredUserInterface();
		MsgAcceptDialog.WindowTitle = title;
		MsgAcceptDialog.DialogText = msg;
		MsgAcceptDialog.PopupCentered(size);
	}

	public static void MsgAcceptDialogShow(string msg, string title = "Error")
	{
		InputManager.MouseEnteredUserInterface();
		MsgAcceptDialog.WindowTitle = title;
		MsgAcceptDialog.DialogText = msg;
		MsgAcceptDialog.Show();
	}

	public void MsgAcceptDialogHide()
	{
		MsgAcceptDialog.Hide();
		InputManager.MouseExitedUserInterface();
		InputManager.SkipInput = true;
	}

	public Gui()
	{
		Register.Gui = this;
	}

	public override void _Ready()
	{
		base._Ready();
		MsgAcceptDialog = GetNodeOrNull<AcceptDialog>(msgAcceptDialogNodePath);
		MsgAcceptDialog.Connect(Signals.Hide, this, "MsgAcceptDialogHide");
		workspace = GetNodeOrNull<Workspace>(workspaceNodePath);
		FileMenuButton = GetNodeOrNull<FileMenuButton>(fileMenuButtonNodePath);
		FileMenuButton = GetNodeOrNull<FileMenuButton>(fileMenuButtonNodePath);
		FilterMenuButton = GetNodeOrNull<FilterMenuButton>(filterMenuButtonNodePath);
		BakeMenuButton = GetNodeOrNull<BakeMenuButton>(bakeMenuButtonNodePath);
		HelpMenuButton = GetNodeOrNull<HelpMenuButton>(helpMenuButtonNodePath);
		WorksheetLabel = GetNodeOrNull<Label>(worksheetLabelNodePath);
		VersionLabel = GetNodeOrNull<Label>(versionLabelNodePath);
		VersionLabel.Text = "Demo v" + 1000.ToString("0,000") + " (" + 2024.ToString("0000") + "-" + 10.ToString("00") + "-" + 5.ToString("00") + ")";
		VersionLabel.AddColorOverride("font_color", new Color(1f, 0.2f, 0.25f));
		InformationsLabel = GetNodeOrNull<Label>(informationsLabelNodePath);
		ChannelLabel = GetNodeOrNull<Label>(channelLabelNodePath);
		ControlIconTextureRect = GetNodeOrNull<TextureRect>(controlIconTextureRectNodePath);
		ControlsRichTextLabel = GetNodeOrNull<RichTextLabel>(controlsRichTextLabelNodePath);
		LayerPanel = GetNodeOrNull<LayerPanel>(layerPanelNodePath);
		LayerControl = GetNodeOrNull<LayerControl>(layerControlNodePath);
		HistoryItemList = GetNodeOrNull<ItemList>(historyItemListNodePath);
		PreviewViewportContainer = GetNodeOrNull<PreviewspaceViewportContainer>(previewViewportContainerNodePath);
		DisplaySettingsContainer = GetNodeOrNull<DisplaySettingsContainer>(displaySettingsContainerNodePath);
		BrushSettingsContainer = GetNodeOrNull<BrushSettingsContainer>(brushSettingsContainerNodePath);
		LineSettingsContainer = GetNodeOrNull<LineSettingsContainer>(lineSettingsContainerNodePath);
		FillingBucketSettingsContainer = GetNodeOrNull<FillingBucketSettingsContainer>(fillingBucketSettingsContainerNodePath);
		StampSettingsContainer = GetNodeOrNull<StampSettingsContainer>(stampSettingsContainerNodePath);
		SelectSettingsContainer = GetNodeOrNull<SelectSettingsContainer>(selectSettingsContainerNodePath);
		UvEditSettingsContainer = GetNodeOrNull<UvEditSettingsContainer>(uvEditSettingsContainerNodePath);
		ToolsContainer = GetNodeOrNull<ToolsContainer>(toolsContainerNodePath);
		MaterialContainer = GetNodeOrNull<MaterialContainer>(materialContainerNodePath);
		MaterialTabContainer = GetNodeOrNull<TabContainer>(materialTabContainerNodePath);
		ChannelsContainer = GetNodeOrNull<ChannelsContainer>(channelsContainerNodePath);
		CurrentChannelItemList = GetNodeOrNull<ChannelList>(currentChannelItemListNodePath);
		LibraryContainer = GetNodeOrNull<Container>(libraryContainerNodePath);
		LibraryHoverPanel = GetNodeOrNull<DefaultHoverPanel>(libraryHoverPanelNodePath);
		SettingsContainer = GetNodeOrNull<WorkspaceSettingsContainer>(settingsContainerNodePath);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.GLOBAL, "SAVE_PROJECT", FileMenuButton.SaveProject, KeyList.S, KeyBinding.EventTypeEnum.JUST_RELEASED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.GLOBAL, "HELP", ToggleHelp, KeyList.F1, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.GLOBAL, "FULLSCREEN", Settings.ToggleFullscreen, KeyList.F11, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.GLOBAL, "UNDO", workspace.Undo, KeyList.Z, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.GLOBAL, "REDO", workspace.Redo, KeyList.Y, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.GLOBAL, "SWITCH_CHANNEL", SwitchChannel, KeyList.C, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.GLOBAL, "SELECT_ALL_CHANNELS", SelectAllChannels, KeyList.C, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: true, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.GLOBAL, "SELECT_CURRENT_CHANNEL", SelectCurrentChannel, KeyList.C, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.GLOBAL, "GRID", ToggleGrid, KeyList.G, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.WORKSPACE, "WORKSPACE_CAMERA_PAN", Register.CameraManager.WorkspaceToolCamera.Pan, ButtonList.Middle, KeyBinding.EventTypeEnum.PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.WORKSPACE, "WORKSPACE_CAMERA_ZOOM_IN", Register.CameraManager.WorkspaceToolCamera.ZoomIn, ButtonList.WheelUp, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.WORKSPACE, "WORKSPACE_CAMERA_ZOOM_OUT", Register.CameraManager.WorkspaceToolCamera.ZoomOut, ButtonList.WheelDown, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.PREVIEWSPACE, "PREVIEWSPACE_CAMERA_PAN", Register.CameraManager.PreviewspaceToolCamera.Pan, ButtonList.Middle, KeyBinding.EventTypeEnum.PRESSED, shift: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.PREVIEWSPACE, "PREVIEWSPACE_CAMERA_ROTATE", Register.CameraManager.PreviewspaceToolCamera.Rotate, ButtonList.Middle, KeyBinding.EventTypeEnum.PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.PREVIEWSPACE, "PREVIEWSPACE_CAMERA_ZOOM_IN", Register.CameraManager.PreviewspaceToolCamera.ZoomIn, ButtonList.WheelUp, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.PREVIEWSPACE, "PREVIEWSPACE_CAMERA_ZOOM_OUT", Register.CameraManager.PreviewspaceToolCamera.ZoomOut, ButtonList.WheelDown, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "ROTATE_ENVIRONMENT", workspace.RotateEnvironment, ButtonList.Right, KeyBinding.EventTypeEnum.PRESSED, shift: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "ABORT_DRAWING", workspace.AbortDrawing, ButtonList.Right, KeyBinding.EventTypeEnum.JUST_RELEASED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "MATERIAL_PICK_FROM_LAYER", workspace.PickMaterialFromLayer, ButtonList.Right, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "MATERIAL_PICK", workspace.PickMaterial, ButtonList.Right, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: true, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "MATERIAL_TOGGLE", Register.DrawingManager.ToggleSettings, KeyList.Space, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "MIRRORING_TOGGLE", workspace.ToggleMirroring, KeyList.N, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "MIRRORING_SET_AXIS", workspace.SetMirrorAxis, KeyList.N, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "TOOL_BRUSH", ToolsContainer.SelectBrushTool, KeyList.B, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "TOOL_LINE", ToolsContainer.SelectLineTool, KeyList.L, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "TOOL_RECTANGLE", ToolsContainer.SelectRectangleTool, KeyList.R, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "TOOL_RECTANGLE_FILLED", ToolsContainer.SelectFilledRectangleTool, KeyList.R, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "TOOL_CIRCLE", ToolsContainer.SelectCircleTool, KeyList.E, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "TOOL_CIRCLE_FILLED", ToolsContainer.SelectFilledCircleTool, KeyList.E, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "TOOL_BUCKET", ToolsContainer.SelectFillingBucketTool, KeyList.F, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "TOOL_STAMP", ToolsContainer.SelectStampTool, KeyList.S, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.FULLSPACE, "TOOL_MASK", ToolsContainer.SelectMaskTool, KeyList.M, KeyBinding.EventTypeEnum.JUST_PRESSED);
		FileMenuButton.SettingsWindowDialog.KeyBindingsContainer.Initialisation();
		UpdateKeyBindings();
	}

	public void Reset()
	{
		UpdateWorksheetName();
		InformationsLabel.Text = "";
		FileMenuButton.Reset();
		FilterMenuButton.Reset();
		BakeMenuButton.Reset();
		HelpMenuButton.Reset();
		PreviewViewportContainer.Reset();
		DisplaySettingsContainer.Reset();
		BrushSettingsContainer.Reset();
		LineSettingsContainer.Reset();
		FillingBucketSettingsContainer.Reset();
		StampSettingsContainer.Reset();
		SelectSettingsContainer.Reset();
		UvEditSettingsContainer.Reset();
		ToolsContainer.Reset();
		LayerPanel.Reset();
		MaterialContainer.Reset();
		MaterialTabContainer.CurrentTab = 0;
		ChannelsContainer.Reset();
		CurrentChannelItemList.Reset();
		SettingsContainer.Reset();
		UpdateChannelsTextures(updateAspectRatio: true);
		ChannelLabel.Text = CurrentChannelItemList.GetItemText(0);
		ControlIconTextureRect.Visible = false;
		Register.SelectionPanel.Reset(resetCounter: true);
	}

	public void UpdateKeyBindings()
	{
		DisplaySettingsContainer.UpdateGridTooltip(Tr("GRID"), InputManager.GetKeyBindingKeyButton("GRID"));
		DisplaySettingsContainer.UpdateMaskTooltip(Tr("MASK_TOGGLE"), "");
		DisplaySettingsContainer.UpdateAlignmentToolTooltip(Tr("ALIGNMENTTOOL_TOGGLE"), "");
		DisplaySettingsContainer.UpdateWireframeTooltip(Tr("WIREFRAME_TOGGLE"), "");
		DisplaySettingsContainer.UpdateMirroringTooltip(Tr("MIRRORING_TOGGLE"), InputManager.GetKeyBindingKeyButton("MIRRORING_TOGGLE"));
		ToolsContainer.UpdateTooltip(0, Tr("TOOL_BRUSH"), InputManager.GetKeyBindingKeyButton("TOOL_BRUSH"));
		ToolsContainer.UpdateTooltip(1, Tr("TOOL_LINE"), InputManager.GetKeyBindingKeyButton("TOOL_LINE"));
		ToolsContainer.UpdateTooltip(2, Tr("TOOL_RECTANGLE"), InputManager.GetKeyBindingKeyButton("TOOL_RECTANGLE"));
		ToolsContainer.UpdateTooltip(3, Tr("TOOL_RECTANGLE_FILLED"), InputManager.GetKeyBindingKeyButton("TOOL_RECTANGLE_FILLED"));
		ToolsContainer.UpdateTooltip(4, Tr("TOOL_CIRCLE"), InputManager.GetKeyBindingKeyButton("TOOL_CIRCLE"));
		ToolsContainer.UpdateTooltip(5, Tr("TOOL_CIRCLE_FILLED"), InputManager.GetKeyBindingKeyButton("TOOL_CIRCLE_FILLED"));
		ToolsContainer.UpdateTooltip(6, Tr("TOOL_BUCKET"), InputManager.GetKeyBindingKeyButton("TOOL_BUCKET"));
		ToolsContainer.UpdateTooltip(7, Tr("TOOL_STAMP"), InputManager.GetKeyBindingKeyButton("TOOL_STAMP"));
		ToolsContainer.UpdateTooltip(8, Tr("TOOL_SMEARING"), "");
		ToolsContainer.UpdateTooltip(9, Tr("TOOL_MASK"), InputManager.GetKeyBindingKeyButton("TOOL_MASK"));
		ToolsContainer.UpdateTooltip(10, Tr("TOOL_UV_EDIT"), "");
		string controlsString = InputManager.GetKeyBindingKeyButton("HELP") + " - Show\\Hide Help\n\n";
		controlsString += "[LMB] - Draw\n";
		controlsString += "[Alt]+[LMB] - Erase\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK_FROM_LAYER") + " - Pick Material\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK") + " - Pick Material (Merged)\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("BRUSH_SIZE_INCREASE") + " - Increase Size\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("BRUSH_SIZE_DECREASE") + " - Decrease Size\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("BRUSH_BLENDING_INCREASE") + " - Increase Blending\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("BRUSH_BLENDING_DECREASE") + " - Decrease Blending\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("BRUSH_SHAPE") + " - Switch Shape\n";
		Register.DrawingManager.BrushTool.Controls = controlsString;
		controlsString = InputManager.GetKeyBindingKeyButton("HELP") + " - Show\\Hide Help\n\n";
		controlsString += "[LMB] - Draw\n";
		controlsString += "[Alt]+[LMB] - Erase\n";
		controlsString += "[Ctrl] - Snap Angle In 45 Degree Steps\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK_FROM_LAYER") + " - Pick Material\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK") + " - Pick Material (Merged)\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("LINE_SIZE_INCREASE") + " - Increase Size\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("LINE_SIZE_DECREASE") + " - Decrease Size\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("LINE_BLENDING_INCREASE") + " - Increase Blending\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("LINE_BLENDING_DECREASE") + " - Decrease Blending\n";
		Register.DrawingManager.LineTool.Controls = controlsString;
		controlsString = InputManager.GetKeyBindingKeyButton("HELP") + " - Show\\Hide Help\n\n";
		controlsString += "[LMB] - Draw\n";
		controlsString += "[Alt]+[LMB] - Erase\n";
		controlsString += "[Ctrl] - Snap To Quadratic Aspect Ratio\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK_FROM_LAYER") + " - Pick Material\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK") + " - Pick Material (Merged)\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("RECTANGLE_SIZE_INCREASE") + " - Increase Size\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("RECTANGLE_SIZE_DECREASE") + " - Decrease Size\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("RECTANGLE_BLENDING_INCREASE") + " - Increase Blending\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("RECTANGLE_BLENDING_DECREASE") + " - Decrease Blending\n";
		Register.DrawingManager.RectangleTool.Controls = controlsString;
		controlsString = InputManager.GetKeyBindingKeyButton("HELP") + " - Show\\Hide Help\n\n";
		controlsString += "[LMB] - Draw\n";
		controlsString += "[Alt]+[LMB] - Erase\n";
		controlsString += "[Ctrl] - Snap To Quadratic Aspect Ratio\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK_FROM_LAYER") + " - Pick Material\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK") + " - Pick Material (Merged)\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("CIRCLE_SIZE_INCREASE") + " - Increase Size\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("CIRCLE_SIZE_DECREASE") + " - Decrease Size\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("CIRCLE_BLENDING_INCREASE") + " - Increase Blending\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("CIRCLE_BLENDING_DECREASE") + " - Decrease Blending\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("CIRCLE_GRADIENT") + " -  Switch Gradient\n";
		Register.DrawingManager.CircleTool.Controls = controlsString;
		controlsString = InputManager.GetKeyBindingKeyButton("HELP") + " - Show\\Hide Help\n\n";
		controlsString += "[LMB] - Fill\n";
		controlsString += "[Shift]+[LMB] - Fill (Merged)\n";
		controlsString += "[Ctrl]+[LMB] - Fill Island (Previewspace)\n";
		controlsString += "[Alt]+[LMB] - Erase\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK_FROM_LAYER") + " - Pick Material\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK") + " - Pick Material (Merged)\n";
		Register.DrawingManager.BucketTool.Controls = controlsString;
		controlsString = InputManager.GetKeyBindingKeyButton("HELP") + " - Show\\Hide Help\n\n";
		controlsString += "[LMB] - Draw\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK_FROM_LAYER") + " - Pick Material\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("MATERIAL_PICK") + " - Pick Material (Merged)\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("STAMP_ROTATE_LEFT") + " - Rotate Left\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("STAMP_ROTATE_RIGHT") + " - Rotate Right\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("STAMP_MIRRORING") + " - Mirroring\n";
		Register.DrawingManager.StampTool.Controls = controlsString;
		controlsString = InputManager.GetKeyBindingKeyButton("HELP") + " - Show\\Hide Help\n\n";
		controlsString += "[LMB] - Select\n";
		controlsString += "[Shift]+[LMB] - Add Selection\n";
		controlsString += "[Shift]+[LMB] - Select Multiple Islands\n";
		controlsString += "[Alt]+[LMB] - Remove Selection\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("SELECTION_CLEAR") + " - Clear Selection\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("SELECTION_DELETE") + " - Delete Selection\n";
		controlsString += "[Ctrl] - Quadratic Aspect Ratios\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("SELECTION_COPY") + " - Copy\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("SELECTION_COPY_MERGED") + " - Copy (Merged)\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("SELECTION_CUT") + " - Cut\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("SELECTION_CUT_MERGED") + " - Cut (Merged)\n";
		controlsString = controlsString + InputManager.GetKeyBindingKeyButton("SELECTION_INVERT") + " - Invert\n";
		Register.SelectionManager.Controls = controlsString;
		controlsString = InputManager.GetKeyBindingKeyButton("HELP") + " - Show\\Hide Help\n\n";
		controlsString += "[LMB] - Select Node\n";
		controlsString += "[Shift]+[LMB] - Select Multiple Edges|Islands\n";
		controlsString += "[Alt]+[LMB] - Select All Edges In A Row\n";
		controlsString += "[RMB] - Move Node\n";
		controlsString += "[Ctr] - Toggle Grid Snap \n";
		Register.PreviewspaceMeshManager.Controls = controlsString;
		if (Register.DrawingManager.Tool < DrawingManager.ToolEnum.DISABLED)
		{
			Register.DrawingManager.Tool = Register.DrawingManager.Tool;
		}
		else if (Register.SelectionManager.Enabled)
		{
			Register.Gui.ControlsRichTextLabel.BbcodeText = Register.SelectionManager.Controls;
		}
		else if (Register.PreviewspaceMeshManager.IsEditingActivated)
		{
			Register.Gui.ControlsRichTextLabel.BbcodeText = Register.PreviewspaceMeshManager.Controls;
		}
	}

	public void SetWorksheetName(string name)
	{
		WorksheetLabel.Text = name;
	}

	public void UpdateWorksheetName()
	{
		WorksheetLabel.Text = workspace.Worksheet.Data.Width + " x " + workspace.Worksheet.Data.Height + "   ";
		WorksheetLabel.Text += workspace.Worksheet.SheetName;
	}

	public void SetDrawingMaterial(Color color, Value height, Value roughness, Value metallicity, Color emission)
	{
		MaterialContainer.ChangeColor(color);
		MaterialContainer.ChangeRoughness(roughness.v);
		MaterialContainer.ChangeMetallicity(metallicity.v);
		MaterialContainer.ChangeHeight(height.v);
		MaterialContainer.ChangeEmission(emission);
	}

	public void UpdateChannelsTextures(bool updateAspectRatio)
	{
		Data worksheetData = workspace.Worksheet.Data;
		ChannelsContainer.SetChannelTexture(0, worksheetData.ColorChannel.ImageTexture, updateAspectRatio);
		ChannelsContainer.SetChannelTexture(1, worksheetData.RoughnessChannel.ImageTexture, updateAspectRatio);
		ChannelsContainer.SetChannelTexture(2, worksheetData.MetallicityChannel.ImageTexture, updateAspectRatio);
		ChannelsContainer.SetChannelTexture(3, worksheetData.HeightChannel.ImageTexture, updateAspectRatio);
		ChannelsContainer.SetChannelTexture(4, worksheetData.EmissionChannel.ImageTexture, updateAspectRatio);
	}

	public void DisableBakeing(bool disabled)
	{
		FileMenuButton.DisableBakeing(disabled);
		BakeMenuButton.DisableBakeing(disabled);
		DisplaySettingsContainer.DisableWireframe(disabled);
		PreviewViewportContainer.DisableWireframe(disabled);
		ToolsContainer.DisableBakeing(disabled);
	}

	public void ResetSettingsContainer()
	{
		SettingsContainer.Reset();
	}

	public void SetHelp(bool enable)
	{
		ControlsRichTextLabel.Visible = enable;
		Vector2 position = ControlIconTextureRect.RectPosition;
		if (ControlsRichTextLabel.Visible)
		{
			position.x = 394f;
		}
		else
		{
			position.x = 74f;
		}
		ControlIconTextureRect.RectPosition = position;
	}

	public void ToggleHelp()
	{
		ControlsRichTextLabel.Visible = !ControlsRichTextLabel.Visible;
		Vector2 position = ControlIconTextureRect.RectPosition;
		if (ControlsRichTextLabel.Visible)
		{
			position.x = 394f;
		}
		else
		{
			position.x = 74f;
		}
		ControlIconTextureRect.RectPosition = position;
	}

	public void SwitchChannel()
	{
		if (workspace.Channel == Data.ChannelEnum.EMISSION)
		{
			workspace.Channel = Data.ChannelEnum.FULL;
		}
		else
		{
			workspace.Channel++;
		}
		workspace.UpdateShaders();
		Register.DrawingManager.Channel = workspace.Channel;
		CurrentChannelItemList.Select((int)workspace.Channel);
		ChannelLabel.Text = CurrentChannelItemList.GetItemText((int)workspace.Channel);
	}

	public void SelectAllChannels()
	{
		Register.DrawingManager.ColorEnabled = true;
		Register.DrawingManager.RoughnessEnabled = true;
		Register.DrawingManager.MetallicityEnabled = true;
		Register.DrawingManager.HeightEnabled = true;
		Register.DrawingManager.EmissionEnabled = true;
		ChannelsContainer.ChangeAllChannelsSelected(enable: true);
	}

	public void SelectCurrentChannel()
	{
		if (workspace.Channel > Data.ChannelEnum.FULL)
		{
			Register.DrawingManager.ColorEnabled = false;
			Register.DrawingManager.RoughnessEnabled = false;
			Register.DrawingManager.MetallicityEnabled = false;
			Register.DrawingManager.HeightEnabled = false;
			Register.DrawingManager.EmissionEnabled = false;
			ChannelsContainer.ChangeAllChannelsSelected(enable: false);
			if (workspace.Channel >= Data.ChannelEnum.NORMAL)
			{
				workspace.ChangeDrawingChannel((int)(workspace.Channel - 2), enable: true);
				ChannelsContainer.ChangeChannelSelected((int)(workspace.Channel - 2), enable: true);
			}
			else
			{
				workspace.ChangeDrawingChannel((int)(workspace.Channel - 1), enable: true);
				ChannelsContainer.ChangeChannelSelected((int)(workspace.Channel - 1), enable: true);
			}
		}
		else
		{
			SelectAllChannels();
		}
	}

	public void ToggleGrid()
	{
		Register.GridManager.Visible = !Register.GridManager.Visible;
		DisplaySettingsContainer.ToggleGrid(Register.GridManager.Visible);
	}
}
