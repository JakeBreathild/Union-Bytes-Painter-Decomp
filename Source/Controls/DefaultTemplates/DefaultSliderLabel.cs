using Godot;

public class DefaultSliderLabel : Label
{
	[Export(PropertyHint.None, "")]
	public float ValueMultiplier = 1f;

	[Export(PropertyHint.None, "")]
	public string ValueFormat = "0.00";

	public void ValueChanged(float value)
	{
		base.Text = (value * ValueMultiplier).ToString(ValueFormat);
	}
}
