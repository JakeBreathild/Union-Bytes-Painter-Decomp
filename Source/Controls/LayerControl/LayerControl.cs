using Godot;

public class LayerControl : VBoxContainer
{
	private Workspace workspace;

	[Export(PropertyHint.None, "")]
	private NodePath layerPanelNodePath;

	private LayerPanel layerPanel;

	private LineEdit nameLineEdit;

	private DefaultColorPickerButton colorPickerButton;

	private Button clearButton;

	public LayerControl()
	{
		Register.LayerControl = this;
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		layerPanel = GetNodeOrNull<LayerPanel>(layerPanelNodePath);
		nameLineEdit = GetNodeOrNull<LineEdit>("PropertiesPanel/NameLineEdit");
		nameLineEdit.Connect(Signals.FocusEntered, this, "LineEditFocusEntered");
		nameLineEdit.Connect(Signals.FocusExited, this, "LineEditFocusExited");
		nameLineEdit.Connect(Signals.TextEntered, this, "NameEntered");
		colorPickerButton = GetNodeOrNull<DefaultColorPickerButton>("PropertiesPanel/ColorPickerButton");
		colorPickerButton.Connect(Signals.ColorChanged, this, "ColorChanged");
		clearButton = GetNodeOrNull<Button>("Clear");
		clearButton.Connect(Signals.Pressed, this, "ClearPressed");
	}

	public void Reset()
	{
		Layer layer = workspace.Worksheet.Data.Layer;
		nameLineEdit.SetBlockSignals(enable: true);
		colorPickerButton.SetBlockSignals(enable: true);
		nameLineEdit.Text = layer.Name;
		colorPickerButton.Color = layer.Color;
		for (int i = 0; i < GetChildCount(); i++)
		{
			GetChildOrNull<LayerChannelControl>(i)?.Reset();
		}
		nameLineEdit.SetBlockSignals(enable: false);
		colorPickerButton.SetBlockSignals(enable: false);
	}

	public void NameEntered(string name)
	{
		if (!name.Empty())
		{
			LayerCommand obj = (LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
			obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.NAME;
			obj.Layer = workspace.Worksheet.Data.Layer;
			obj.NewText = name;
			workspace.Worksheet.History.StopRecording("Layer Renamed [" + workspace.Worksheet.Data.Layer.Name + "] to " + name);
			nameLineEdit.ReleaseFocus();
			InputManager.LockInput = false;
		}
	}

	public void LineEditFocusEntered()
	{
		InputManager.LockInput = true;
	}

	public void LineEditFocusExited()
	{
		InputManager.LockInput = false;
	}

	public void ColorChanged(Color color)
	{
		workspace.Worksheet.Data.Layer.Color = color;
		layerPanel.UpdateCurrentEntry();
	}

	public void ClearPressed()
	{
		DrawingManager drawingManager = Register.DrawingManager;
		drawingManager.PushSettings();
		drawingManager.Tool = DrawingManager.ToolEnum.BRUSH;
		drawingManager.BlendingMode = Blender.BlendingModeEnum.NORMAL;
		drawingManager.ColorEnabled = true;
		drawingManager.RoughnessEnabled = true;
		drawingManager.MetallicityEnabled = true;
		drawingManager.HeightEnabled = true;
		drawingManager.EmissionEnabled = true;
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
				drawingManager.DrawPixel(workspace.Worksheet, x, y);
			}
		}
		drawingManager.StopDrawing(workspace.Worksheet, Vector2i.NegOne, doErase: true, "Layer Cleared: " + workspace.Worksheet.Data.Layer.Name);
		workspace.Worksheet.Data.Layer.ColorChannel.IsVisible = false;
		workspace.Worksheet.Data.Layer.ColorChannel.IsNew = true;
		workspace.Worksheet.Data.Layer.RoughnessChannel.IsVisible = false;
		workspace.Worksheet.Data.Layer.RoughnessChannel.IsNew = true;
		workspace.Worksheet.Data.Layer.MetallicityChannel.IsVisible = false;
		workspace.Worksheet.Data.Layer.MetallicityChannel.IsNew = true;
		workspace.Worksheet.Data.Layer.HeightChannel.IsVisible = false;
		workspace.Worksheet.Data.Layer.HeightChannel.IsNew = true;
		workspace.Worksheet.Data.Layer.EmissionChannel.IsVisible = false;
		workspace.Worksheet.Data.Layer.EmissionChannel.IsNew = true;
		Register.DrawingManager.Tool = DrawingManager.ToolEnum.DISABLED;
		drawingManager.PopSettings();
		Reset();
	}
}
