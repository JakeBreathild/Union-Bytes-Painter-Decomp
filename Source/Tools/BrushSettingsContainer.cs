using Godot;

public class BrushSettingsContainer : DefaultHBoxContainer
{
	private float blendingStrengthInitialValue;

	private ToolSlider blendingStrengthSlider;

	private int sizeInitialValue;

	private ToolSlider sizeSlider;

	private float hardnessInitialValue;

	private ToolSlider hardnessSlider;

	private TextureButton shapeCircleToolButton;

	private bool shapeCircleInitialValue;

	private TextureButton shapeSquareToolButton;

	private bool shapeSquareInitialValue;

	private Button fadingButton;

	private bool fadingInitialValue;

	private ToolSlider fadingFalloffToolSlider;

	private float fadingFalloffInitialValue;

	private ToolSlider fadingPathLengthToolSlider;

	private float fadingPathLengthInitialValue;

	private int distanceInitialValue;

	private ToolSlider distanceSlider;

	private bool disabled;

	public bool Disabled
	{
		get
		{
			return disabled;
		}
		set
		{
			disabled = value;
			blendingStrengthSlider.Disabled = disabled;
			sizeSlider.Disabled = disabled;
			hardnessSlider.Disabled = disabled;
			shapeCircleToolButton.Disabled = disabled;
			shapeSquareToolButton.Disabled = disabled;
			fadingButton.Disabled = disabled;
			fadingFalloffToolSlider.Disabled = disabled;
			fadingPathLengthToolSlider.Disabled = disabled;
			distanceSlider.Disabled = disabled;
		}
	}

	public bool DistanceSliderVisible
	{
		get
		{
			return distanceSlider.Visible;
		}
		set
		{
			distanceSlider.Visible = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		blendingStrengthSlider = GetNodeOrNull<ToolSlider>("Blending");
		blendingStrengthInitialValue = blendingStrengthSlider.GetValue();
		blendingStrengthSlider.ValueChangedCallback = BlendingStrengthChanged;
		sizeSlider = GetNodeOrNull<ToolSlider>("Size");
		sizeInitialValue = (int)sizeSlider.GetValue();
		sizeSlider.ValueChangedCallback = SizeChanged;
		hardnessSlider = GetNodeOrNull<ToolSlider>("Hardness");
		hardnessInitialValue = hardnessSlider.GetValue();
		hardnessSlider.ValueChangedCallback = HardnessChanged;
		shapeCircleToolButton = GetNodeOrNull<TextureButton>("ShapeCircle");
		shapeCircleInitialValue = shapeCircleToolButton.Pressed;
		shapeCircleToolButton.Connect(Signals.Pressed, this, "ShapeCirclePressed");
		shapeSquareToolButton = GetNodeOrNull<TextureButton>("ShapeSquare");
		shapeSquareInitialValue = shapeSquareToolButton.Pressed;
		shapeSquareToolButton.Connect(Signals.Pressed, this, "ShapeSquarePressed");
		fadingButton = GetNodeOrNull<Button>("Fading/Button");
		fadingInitialValue = fadingButton.Pressed;
		fadingButton.Connect(Signals.Toggled, this, "FadingButtonToggled");
		fadingFalloffToolSlider = GetNodeOrNull<ToolSlider>("FadingFalloff");
		fadingFalloffInitialValue = fadingFalloffToolSlider.GetValue();
		fadingFalloffToolSlider.ValueChangedCallback = FadingFalloffChanged;
		fadingPathLengthToolSlider = GetNodeOrNull<ToolSlider>("FadingPathLength");
		fadingPathLengthInitialValue = fadingPathLengthToolSlider.GetValue();
		fadingPathLengthToolSlider.ValueChangedCallback = FadingPathLengthChanged;
		distanceSlider = GetNodeOrNull<ToolSlider>("Distance");
		distanceInitialValue = (int)distanceSlider.GetValue();
		distanceSlider.ValueChangedCallback = DistanceChanged;
	}

	public override void Reset()
	{
		base.Reset();
		sizeSlider.SetValue(sizeInitialValue);
		shapeCircleToolButton.Pressed = shapeCircleInitialValue;
		shapeSquareToolButton.Pressed = shapeSquareInitialValue;
		hardnessSlider.SetValue(hardnessInitialValue);
		blendingStrengthSlider.SetValue(blendingStrengthInitialValue);
		fadingButton.Pressed = fadingInitialValue;
		fadingFalloffToolSlider.SetValue(fadingFalloffInitialValue);
		fadingFalloffToolSlider.Visible = false;
		fadingPathLengthToolSlider.SetValue(fadingPathLengthInitialValue);
		fadingPathLengthToolSlider.Visible = false;
		distanceSlider.SetValue(distanceInitialValue);
		distanceSlider.Visible = false;
		base.Visible = true;
	}

	public void ChangeBlendingStrength(float value)
	{
		blendingStrengthSlider.SetValue(value);
	}

	public void BlendingStrengthChanged(float value)
	{
		workspace.DrawingManager.BrushTool.BlendingStrength = blendingStrengthSlider.GetValue();
		workspace.DrawingManager.SmearingTool.BlendingStrength = blendingStrengthSlider.GetValue();
	}

	public void ChangeSize(float value)
	{
		sizeSlider.SetValue(value);
	}

	public void SizeChanged(float value)
	{
		workspace.DrawingManager.BrushTool.Size = (int)value;
		workspace.DrawingManager.SmearingTool.Size = (int)value;
	}

	public void ChangeHardness(float value)
	{
		hardnessSlider.SetValue(value);
	}

	public void HardnessChanged(float value)
	{
		workspace.DrawingManager.BrushTool.Hardness = value;
		workspace.DrawingManager.SmearingTool.Hardness = value;
	}

	public void PressShapeCircle()
	{
		shapeCircleToolButton.Pressed = true;
		shapeSquareToolButton.Pressed = false;
	}

	public void ShapeCirclePressed()
	{
		shapeCircleToolButton.Pressed = true;
		shapeSquareToolButton.Pressed = false;
		workspace.DrawingManager.BrushTool.Shape = DrawingToolBrush.ShapeEnum.CIRCULAR;
		workspace.DrawingManager.SmearingTool.Shape = DrawingToolSmearing.ShapeEnum.CIRCULAR;
	}

	public void PressShapeSquare()
	{
		shapeCircleToolButton.Pressed = false;
		shapeSquareToolButton.Pressed = true;
	}

	public void ShapeSquarePressed()
	{
		shapeCircleToolButton.Pressed = false;
		shapeSquareToolButton.Pressed = true;
		workspace.DrawingManager.BrushTool.Shape = DrawingToolBrush.ShapeEnum.SQUARE;
		workspace.DrawingManager.SmearingTool.Shape = DrawingToolSmearing.ShapeEnum.SQUARE;
	}

	public void FadingButtonToggled(bool pressed)
	{
		workspace.DrawingManager.BrushTool.DoFading = pressed;
		workspace.DrawingManager.SmearingTool.DoFading = pressed;
		if (pressed)
		{
			fadingFalloffToolSlider.Visible = true;
			fadingPathLengthToolSlider.Visible = true;
		}
		else
		{
			fadingFalloffToolSlider.Visible = false;
			fadingPathLengthToolSlider.Visible = false;
		}
	}

	public void ChangeFadingFalloff(float value)
	{
		fadingFalloffToolSlider.SetValue(value);
	}

	public void FadingFalloffChanged(float value)
	{
		workspace.DrawingManager.BrushTool.FadingFalloff = fadingFalloffToolSlider.GetValue();
		workspace.DrawingManager.SmearingTool.FadingFalloff = fadingFalloffToolSlider.GetValue();
	}

	public void ChangeFadingPathLength(float value)
	{
		fadingPathLengthToolSlider.SetValue(value);
	}

	public void FadingPathLengthChanged(float value)
	{
		workspace.DrawingManager.BrushTool.FadingPathLength = fadingPathLengthToolSlider.GetValue();
		workspace.DrawingManager.SmearingTool.FadingPathLength = fadingPathLengthToolSlider.GetValue();
	}

	public void ChangeDistance(float value)
	{
		distanceSlider.SetValue(value);
	}

	public void DistanceChanged(float value)
	{
		workspace.DrawingManager.SmearingTool.BlurDistance = (int)value;
	}
}
