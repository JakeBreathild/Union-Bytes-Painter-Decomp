public class LineSettingsContainer : DefaultHBoxContainer
{
	private float blendingStrengthInitialValue;

	private ToolSlider blendingStrengthSlider;

	private int sizeInitialValue;

	private ToolSlider sizeSlider;

	private float hardnessInitialValue;

	private ToolSlider hardnessSlider;

	private float spacingInitialValue;

	private ToolSlider spacingSlider;

	private bool disabled;

	public ToolSlider SpacingSlider => spacingSlider;

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
			spacingSlider.Disabled = disabled;
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
		spacingSlider = GetNodeOrNull<ToolSlider>("Spacing");
		spacingInitialValue = spacingSlider.GetValue();
		spacingSlider.ValueChangedCallback = SpacingChanged;
	}

	public override void Reset()
	{
		base.Reset();
		sizeSlider.SetValue(sizeInitialValue);
		hardnessSlider.SetValue(hardnessInitialValue);
		blendingStrengthSlider.SetValue(blendingStrengthInitialValue);
		spacingSlider.SetValue(Register.CollisionManager.StepLength);
		base.Visible = false;
	}

	public void ChangeBlendingStrength(float value)
	{
		blendingStrengthSlider.SetValue(value);
	}

	public void BlendingStrengthChanged(float value)
	{
		workspace.DrawingManager.LineTool.BlendingStrength = value;
		workspace.DrawingManager.RectangleTool.BlendingStrength = value;
		workspace.DrawingManager.CircleTool.BlendingStrength = value;
	}

	public void ChangeSize(float value)
	{
		sizeSlider.SetValue(value);
	}

	public void SizeChanged(float value)
	{
		workspace.DrawingManager.LineTool.Size = (int)value;
		workspace.DrawingManager.RectangleTool.Size = (int)value;
		workspace.DrawingManager.CircleTool.Size = (int)value;
	}

	public void ChangeHardness(float value)
	{
		hardnessSlider.SetValue(value);
	}

	public void HardnessChanged(float value)
	{
		workspace.DrawingManager.LineTool.Hardness = value;
		workspace.DrawingManager.RectangleTool.Hardness = value;
		workspace.DrawingManager.CircleTool.Hardness = value;
	}

	public void ChangeSpacing(float value)
	{
		spacingSlider.SetValue(value);
	}

	public void SpacingChanged(float value)
	{
		Register.CollisionManager.StepLength = value;
	}
}
