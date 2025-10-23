using Godot;

public class DefaultSlider : HSlider
{
	[Export(PropertyHint.None, "")]
	private string text = "";

	[Export(PropertyHint.None, "")]
	private float valueMultiplier = 1f;

	[Export(PropertyHint.None, "")]
	private string valueFormat = "0.00";

	[Export(PropertyHint.None, "")]
	private bool allowOverwriteMinimumValue;

	[Export(PropertyHint.None, "")]
	private bool allowOverwriteMaximumValue;

	[Export(PropertyHint.None, "")]
	private Color defaultColor = new Color(0.81f, 0.81f, 0.81f);

	[Export(PropertyHint.None, "")]
	private Color disabledColor = new Color(1f, 1f, 1f, 0.35f);

	private LineEdit valueLineEdit;

	private Label nameLabel;

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
			base.Editable = !disabled;
			if (disabled)
			{
				valueLineEdit.AddColorOverride("font_color", disabledColor);
				nameLabel.AddColorOverride("font_color", disabledColor);
			}
			else
			{
				valueLineEdit.AddColorOverride("font_color", defaultColor);
				nameLabel.AddColorOverride("font_color", defaultColor);
			}
		}
	}

	public override void _Ready()
	{
		base._Ready();
		valueLineEdit = GetChild<LineEdit>(0);
		valueLineEdit.Text = (base.Value * (double)valueMultiplier).ToString(valueFormat);
		valueLineEdit.Connect(Signals.TextEntered, this, "ValueEntered");
		valueLineEdit.Connect(Signals.FocusEntered, this, "ValueFocusEntered");
		valueLineEdit.Connect(Signals.FocusExited, this, "ValueFocusExited");
		nameLabel = GetChild<Label>(1);
		nameLabel.Text = text;
		Connect(Signals.ValueChanged, this, "ValueChanged");
	}

	public void SetValue(float value)
	{
		base.Value = value / valueMultiplier;
		valueLineEdit.Text = value.ToString(valueFormat);
	}

	public new float GetValue()
	{
		return (float)base.Value * valueMultiplier;
	}

	public void ValueChanged(float value)
	{
		valueLineEdit.Text = (value * valueMultiplier).ToString(valueFormat);
	}

	public void ValueFocusEntered()
	{
		valueLineEdit.SelectAll();
	}

	public void ValueEntered(string text)
	{
		valueLineEdit.ReleaseFocus();
	}

	public void ValueFocusExited()
	{
		valueLineEdit.SetBlockSignals(enable: true);
		string text = valueLineEdit.Text;
		if (text.Empty())
		{
			text = "0.0";
		}
		if (text.IsValidFloat())
		{
			float value = float.Parse(text) / valueMultiplier;
			if (allowOverwriteMinimumValue && (double)value < base.MinValue)
			{
				base.MinValue = value;
			}
			if (allowOverwriteMaximumValue && (double)value > base.MaxValue)
			{
				base.MaxValue = value;
			}
			value = Mathf.Clamp(value, (float)base.MinValue, (float)base.MaxValue);
			base.Value = value;
		}
		valueLineEdit.Text = (base.Value * (double)valueMultiplier).ToString(valueFormat);
		valueLineEdit.SetBlockSignals(enable: false);
	}
}
