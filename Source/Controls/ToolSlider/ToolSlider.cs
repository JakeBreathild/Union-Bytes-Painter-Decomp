using System;
using Godot;

public class ToolSlider : Panel
{
	[Export(PropertyHint.None, "")]
	private string text = "";

	[Export(PropertyHint.None, "")]
	private float valueMultiplier = 1f;

	[Export(PropertyHint.None, "")]
	private float minValue;

	[Export(PropertyHint.None, "")]
	private float maxValue = 100f;

	[Export(PropertyHint.None, "")]
	private float valueStep = 1f;

	[Export(PropertyHint.None, "")]
	private bool rounded;

	[Export(PropertyHint.None, "")]
	private string suffix = "";

	[Export(PropertyHint.None, "")]
	private float value;

	[Export(PropertyHint.None, "")]
	private Color defaultColor = new Color(0.81f, 0.81f, 0.81f);

	[Export(PropertyHint.None, "")]
	private Color disabledColor = new Color(1f, 1f, 1f, 0.35f);

	private Label textLabel;

	private SpinBox valueSpinBox;

	private Panel valueSliderPanel;

	private Slider valueSlider;

	private float timer;

	private float delay = 0.5f;

	private bool disabled;

	private Action<float> valueChangedCallback;

	public float Value
	{
		get
		{
			return value * valueMultiplier;
		}
		set
		{
			this.value = value / valueMultiplier;
			valueSpinBox.Value = this.value;
			valueSlider.Value = this.value;
		}
	}

	public bool Disabled
	{
		get
		{
			return disabled;
		}
		set
		{
			disabled = value;
			valueSpinBox.Editable = !disabled;
			valueSlider.Editable = !disabled;
			if (disabled)
			{
				textLabel.AddColorOverride("font_color", disabledColor);
			}
			else
			{
				textLabel.AddColorOverride("font_color", defaultColor);
			}
		}
	}

	public Action<float> ValueChangedCallback
	{
		set
		{
			valueChangedCallback = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		textLabel = GetChildOrNull<Label>(0);
		textLabel.Text = text;
		textLabel.RectMinSize = new Vector2(textLabel.GetFont("Default").GetStringSize(textLabel.Text).x + 8f, 0f);
		valueSpinBox = GetChildOrNull<SpinBox>(1);
		valueSpinBox.Suffix = suffix;
		valueSpinBox.MinValue = minValue;
		valueSpinBox.MaxValue = maxValue;
		valueSpinBox.Step = valueStep;
		valueSpinBox.Value = value;
		valueSpinBox.Rounded = rounded;
		valueSpinBox.MouseFilter = MouseFilterEnum.Pass;
		valueSpinBox.GetLineEdit().MouseFilter = MouseFilterEnum.Pass;
		valueSpinBox.Connect(Signals.ValueChanged, this, "ValueChangedBySpinBox");
		base.RectMinSize = new Vector2(textLabel.RectMinSize.x + valueSpinBox.RectMinSize.x + 24f, 0f);
		textLabel.RectPosition = new Vector2(4f, textLabel.RectPosition.y);
		valueSliderPanel = GetChildOrNull<Panel>(2);
		valueSliderPanel.RectPosition = new Vector2(-4f, base.RectSize.y + 6f);
		valueSliderPanel.Connect(Signals.MouseEntered, this, "MouseValueSliderPanelEntered");
		valueSliderPanel.Connect(Signals.MouseExited, this, "MouseValueSliderPanelExited");
		valueSlider = valueSliderPanel.GetChildOrNull<Slider>(0);
		valueSlider.MinValue = minValue;
		valueSlider.MaxValue = maxValue;
		valueSlider.Step = valueStep;
		valueSlider.Value = value;
		valueSlider.Rounded = rounded;
		valueSlider.MouseFilter = MouseFilterEnum.Pass;
		valueSlider.Connect(Signals.MouseEntered, this, "MouseValueSliderEntered");
		valueSlider.Connect(Signals.ValueChanged, this, "ValueChangedBySlider");
		Connect(Signals.MouseEntered, this, "MouseEntered");
		Connect(Signals.MouseExited, this, "MouseExited");
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (timer > 0f)
		{
			timer -= delta;
			if (timer <= 0f)
			{
				timer = 0f;
				valueSpinBox.GetLineEdit().ReleaseFocus();
				valueSliderPanel.Hide();
			}
		}
	}

	public void MouseEntered()
	{
		InputManager.MouseEnteredUserInterface();
		InputManager.SkipInput = true;
		timer = 0f;
		valueSliderPanel.Show();
	}

	public void MouseExited()
	{
		timer = delay;
		InputManager.MouseExitedUserInterface();
	}

	public void MouseValueSliderPanelEntered()
	{
		timer = 0f;
		InputManager.MouseEnteredUserInterface();
	}

	public void MouseValueSliderPanelExited()
	{
		InputManager.MouseExitedUserInterface();
		InputManager.SkipInput = true;
		timer = delay;
	}

	public void MouseValueSliderEntered()
	{
		timer = 0f;
	}

	public void ValueChangedBySpinBox(float value)
	{
		this.value = (float)valueSpinBox.Value;
		valueSlider.Value = this.value;
		valueChangedCallback(Value);
		valueSpinBox.ReleaseFocus();
	}

	public void ValueChangedBySlider(float value)
	{
		this.value = (float)valueSlider.Value;
		valueSpinBox.Value = this.value;
		valueChangedCallback(Value);
	}

	public void SetValue(float value)
	{
		Value = value;
	}

	public float GetValue()
	{
		return Value;
	}
}
