using Godot;

public class SelectSettingsContainer : DefaultHBoxContainer
{
	private SelectionManager selectionManager;

	private TextureButton selectionBrushToolButton;

	private TextureButton selectionLassoToolButton;

	private TextureButton selectionRectangleToolButton;

	private TextureButton selectionIslandToolButton;

	private TextureButton selectionWandToolButton;

	private Button clearButton;

	private Button invertButton;

	private int sizeInitialValue;

	private ToolSlider sizeSlider;

	private ChannelButton channelButton;

	private ToolSlider toleranceSlider;

	private CheckButton neighboringCheckButton;

	public override void _Ready()
	{
		base._Ready();
		selectionManager = Register.SelectionManager;
		selectionBrushToolButton = GetNodeOrNull<TextureButton>("Brush");
		selectionBrushToolButton.Connect(Signals.Pressed, this, "ActivateBrushSelection");
		selectionLassoToolButton = GetNodeOrNull<TextureButton>("Lasso");
		selectionLassoToolButton.Connect(Signals.Pressed, this, "ActivateLassoSelection");
		selectionRectangleToolButton = GetNodeOrNull<TextureButton>("Rectangle");
		selectionRectangleToolButton.Connect(Signals.Pressed, this, "ActivateRectangleSelection");
		selectionIslandToolButton = GetNodeOrNull<TextureButton>("Island");
		selectionIslandToolButton.Connect(Signals.Pressed, this, "ActivateIslandSelection");
		selectionWandToolButton = GetNodeOrNull<TextureButton>("Wand");
		selectionWandToolButton.Connect(Signals.Pressed, this, "ActivateWandSelection");
		clearButton = GetNodeOrNull<Button>("Clear/Button");
		clearButton.Connect(Signals.Pressed, this, "ClearPressed");
		invertButton = GetNodeOrNull<Button>("Invert/Button");
		invertButton.Connect(Signals.Pressed, this, "InvertPressed");
		sizeSlider = GetNodeOrNull<ToolSlider>("Size");
		sizeInitialValue = (int)sizeSlider.GetValue();
		sizeSlider.ValueChangedCallback = SizeChanged;
		channelButton = GetNodeOrNull<ChannelButton>("Channel/ChannelButton");
		channelButton.ChannelSelectedCallback = ChannelChanged;
		toleranceSlider = GetNodeOrNull<ToolSlider>("Tolerance");
		toleranceSlider.ValueChangedCallback = ToleranceChanged;
		neighboringCheckButton = GetNodeOrNull<CheckButton>("Neighboring/CheckButton");
		neighboringCheckButton.Connect(Signals.Toggled, this, "NeighboringToggled");
	}

	public override void Reset()
	{
		base.Reset();
		selectionBrushToolButton.Pressed = false;
		selectionLassoToolButton.Pressed = false;
		selectionRectangleToolButton.Pressed = true;
		selectionIslandToolButton.Pressed = false;
		selectionWandToolButton.Pressed = false;
		sizeSlider.SetValue(sizeInitialValue);
		sizeSlider.Disabled = true;
		channelButton.SetChannel(Data.ChannelEnum.COLOR);
		channelButton.Disabled = true;
		toleranceSlider.SetValue(0.05f);
		toleranceSlider.Disabled = true;
		neighboringCheckButton.Pressed = true;
		neighboringCheckButton.Disabled = true;
		base.Visible = false;
	}

	public void ActivateBrushSelection()
	{
		selectionBrushToolButton.Pressed = true;
		selectionLassoToolButton.Pressed = false;
		selectionRectangleToolButton.Pressed = false;
		selectionIslandToolButton.Pressed = false;
		selectionWandToolButton.Pressed = false;
		selectionManager.Tool = SelectionManager.ToolEnum.BRUSH;
		selectionManager.Shape = SelectionManager.ShapeEnum.CIRCULAR;
		selectionManager.Size = (int)sizeSlider.GetValue();
		sizeSlider.Disabled = false;
		channelButton.Disabled = true;
		toleranceSlider.Disabled = true;
		neighboringCheckButton.Disabled = true;
	}

	public void ActivateLassoSelection()
	{
		selectionBrushToolButton.Pressed = false;
		selectionLassoToolButton.Pressed = true;
		selectionRectangleToolButton.Pressed = false;
		selectionIslandToolButton.Pressed = false;
		selectionWandToolButton.Pressed = false;
		selectionManager.Tool = SelectionManager.ToolEnum.LASSO;
		selectionManager.Shape = SelectionManager.ShapeEnum.CIRCULAR;
		selectionManager.Size = Mathf.Max((int)sizeSlider.GetValue(), 3);
		sizeSlider.Disabled = false;
		channelButton.Disabled = true;
		toleranceSlider.Disabled = true;
		neighboringCheckButton.Disabled = true;
	}

	public void ActivateRectangleSelection()
	{
		selectionBrushToolButton.Pressed = false;
		selectionLassoToolButton.Pressed = false;
		selectionRectangleToolButton.Pressed = true;
		selectionIslandToolButton.Pressed = false;
		selectionWandToolButton.Pressed = false;
		selectionManager.Tool = SelectionManager.ToolEnum.RECTANGLE;
		selectionManager.Shape = SelectionManager.ShapeEnum.SQUARE;
		selectionManager.Size = 1;
		sizeSlider.Disabled = true;
		channelButton.Disabled = true;
		toleranceSlider.Disabled = true;
		neighboringCheckButton.Disabled = true;
	}

	public void ActivateIslandSelection()
	{
		selectionBrushToolButton.Pressed = false;
		selectionLassoToolButton.Pressed = false;
		selectionRectangleToolButton.Pressed = false;
		selectionIslandToolButton.Pressed = true;
		selectionWandToolButton.Pressed = false;
		selectionManager.Tool = SelectionManager.ToolEnum.ISLAND;
		selectionManager.Shape = SelectionManager.ShapeEnum.CIRCULAR;
		selectionManager.Size = 1;
		sizeSlider.Disabled = true;
		channelButton.Disabled = true;
		toleranceSlider.Disabled = true;
		neighboringCheckButton.Disabled = true;
	}

	public void ActivateWandSelection()
	{
		selectionBrushToolButton.Pressed = false;
		selectionLassoToolButton.Pressed = false;
		selectionRectangleToolButton.Pressed = false;
		selectionIslandToolButton.Pressed = false;
		selectionWandToolButton.Pressed = true;
		selectionManager.Tool = SelectionManager.ToolEnum.WAND;
		selectionManager.Shape = SelectionManager.ShapeEnum.CIRCULAR;
		selectionManager.Size = 1;
		sizeSlider.Disabled = true;
		channelButton.Disabled = false;
		toleranceSlider.Disabled = false;
		neighboringCheckButton.Disabled = false;
	}

	public void ClearPressed()
	{
		selectionManager.Clear();
	}

	public void InvertPressed()
	{
		selectionManager.Invert();
	}

	public void ChangeSize(float value)
	{
		sizeSlider.SetValue(value);
	}

	public void SizeChanged(float value)
	{
		if (selectionManager.Tool == SelectionManager.ToolEnum.LASSO)
		{
			selectionManager.Size = Mathf.Max((int)value, 3);
		}
		else
		{
			selectionManager.Size = (int)value;
		}
	}

	public void ChangeChannel(Data.ChannelEnum channel)
	{
		channelButton.SetChannel(channel);
	}

	public void ChannelChanged(Data.ChannelEnum channel)
	{
		selectionManager.Channel = channel;
	}

	public void ChangeTolerance(float value)
	{
		toleranceSlider.SetValue(value);
	}

	public void ToleranceChanged(float value)
	{
		selectionManager.Tolerance = toleranceSlider.GetValue();
	}

	public void ToggleNeighboring(bool pressed)
	{
		neighboringCheckButton.Pressed = pressed;
	}

	public void NeighboringToggled(bool pressed)
	{
		if (pressed)
		{
			selectionManager.WandMode = SelectionManager.WandModeEnum.FLOODFILL;
		}
		else
		{
			selectionManager.WandMode = SelectionManager.WandModeEnum.FULLFILL;
		}
	}
}
