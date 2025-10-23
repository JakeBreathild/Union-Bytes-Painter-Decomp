using System;
using Godot;

public class AdvancedSlider : Panel
{
	private Label nameLabel;

	private Slider slider;

	private LineEdit valueLineEdit;

	private MenuButton menuButton;

	private PopupMenu popupMenu;

	[Export(PropertyHint.None, "")]
	private string name = "";

	[Export(PropertyHint.None, "")]
	private string valueFormat = "0.0";

	[Export(PropertyHint.None, "")]
	private float value;

	[Export(PropertyHint.None, "")]
	private float step = 1f;

	[Export(PropertyHint.None, "")]
	private bool rounded;

	private float defaultValue;

	[Export(PropertyHint.None, "")]
	private bool allowOverwriteMinimumValue;

	[Export(PropertyHint.None, "")]
	private bool allowOverwriteMaximumValue;

	[Export(PropertyHint.None, "")]
	private float minValue;

	[Export(PropertyHint.None, "")]
	private float maxValue = 100f;

	[Export(PropertyHint.None, "")]
	private bool disabled;

	private Action<float> valueChangedCallback;

	[Export(PropertyHint.None, "")]
	private Color defaultColor = new Color(0.81f, 0.81f, 0.81f);

	[Export(PropertyHint.None, "")]
	private Color disabledColor = new Color(1f, 1f, 1f, 0.35f);

	public Label NameLabel => nameLabel;

	public Slider Slider => slider;

	public LineEdit ValueLineEdit => valueLineEdit;

	public float Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
			slider.SetBlockSignals(enable: true);
			valueLineEdit.SetBlockSignals(enable: true);
			slider.Value = this.value;
			valueLineEdit.Text = this.value.ToString(valueFormat);
			valueLineEdit.SetBlockSignals(enable: false);
			slider.SetBlockSignals(enable: false);
		}
	}

	public int IntValue
	{
		get
		{
			return Mathf.RoundToInt(value);
		}
		set
		{
			this.value = value;
			slider.SetBlockSignals(enable: true);
			valueLineEdit.SetBlockSignals(enable: true);
			slider.Value = this.value;
			valueLineEdit.Text = this.value.ToString(valueFormat);
			valueLineEdit.SetBlockSignals(enable: false);
			slider.SetBlockSignals(enable: false);
		}
	}

	public float DefaultValue
	{
		get
		{
			return defaultValue;
		}
		set
		{
			defaultValue = value;
		}
	}

	public int DefaultIntValue
	{
		get
		{
			return Mathf.RoundToInt(defaultValue);
		}
		set
		{
			defaultValue = value;
		}
	}

	public float MinValue
	{
		get
		{
			return minValue;
		}
		set
		{
			minValue = value;
			slider.MinValue = minValue;
			if (this.value < minValue)
			{
				Value = minValue;
			}
		}
	}

	public float MaxValue
	{
		get
		{
			return maxValue;
		}
		set
		{
			maxValue = value;
			slider.MaxValue = maxValue;
			if (this.value > maxValue)
			{
				Value = maxValue;
			}
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
			slider.Editable = !disabled;
			valueLineEdit.Editable = !disabled;
			menuButton.Disabled = disabled;
			if (disabled)
			{
				nameLabel.AddColorOverride("font_color", disabledColor);
			}
			else
			{
				nameLabel.AddColorOverride("font_color", defaultColor);
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
		defaultValue = value;
		nameLabel = GetNodeOrNull<Label>("Name");
		nameLabel.Text = Tr(name);
		if (disabled)
		{
			nameLabel.AddColorOverride("font_color", disabledColor);
		}
		else
		{
			nameLabel.AddColorOverride("font_color", defaultColor);
		}
		slider = GetNodeOrNull<Slider>("Slider");
		slider.MinValue = minValue;
		slider.MaxValue = maxValue;
		slider.Step = step;
		slider.Value = value;
		slider.Rounded = rounded;
		slider.Editable = !disabled;
		slider.Connect(Signals.ValueChanged, this, "SliderValueChanged");
		valueLineEdit = GetNodeOrNull<LineEdit>("Value");
		valueLineEdit.Text = value.ToString(valueFormat);
		valueLineEdit.Editable = !disabled;
		valueLineEdit.Connect(Signals.TextEntered, this, "ValueLineEditTextEntered");
		valueLineEdit.Connect(Signals.FocusEntered, this, "ValueLineEditFocusEntered");
		valueLineEdit.Connect(Signals.FocusExited, this, "ValueLineEditFocusExited");
		menuButton = GetNodeOrNull<MenuButton>("Menu");
		menuButton.Disabled = disabled;
		popupMenu = menuButton.GetPopup();
		popupMenu.AddFontOverride("font", Resources.DefaultFont);
		popupMenu.AddItem("Default", 0);
		popupMenu.AddItem("Min", 1);
		popupMenu.AddItem("1/4", 2);
		popupMenu.AddItem("1/2", 3);
		popupMenu.AddItem("3/4", 4);
		popupMenu.AddItem("Max", 5);
		popupMenu.Connect(Signals.AboutToShow, this, "PopupShow");
		popupMenu.Connect(Signals.Hide, this, "PopupHide");
		popupMenu.Connect(Signals.IdPressed, this, "PopupMenuItemSelected");
	}

	public void Reset()
	{
		value = defaultValue;
		ChangeValue(value);
	}

	public void PopupShow()
	{
		InputManager.MouseEnteredUserInterface();
	}

	public void PopupHide()
	{
		InputManager.MouseExitedUserInterface();
		InputManager.SkipInput = true;
	}

	private void ChangeValue(float value)
	{
		slider.SetBlockSignals(enable: true);
		valueLineEdit.SetBlockSignals(enable: true);
		this.value = value;
		slider.Value = this.value;
		valueLineEdit.Text = this.value.ToString(valueFormat);
		if (valueChangedCallback != null)
		{
			valueChangedCallback(this.value);
		}
		valueLineEdit.SetBlockSignals(enable: false);
		slider.SetBlockSignals(enable: false);
	}

	public void ValueLineEditFocusEntered()
	{
		valueLineEdit.SelectAll();
	}

	public void ValueLineEditTextEntered(string text)
	{
		valueLineEdit.ReleaseFocus();
	}

	public void ValueLineEditFocusExited()
	{
		valueLineEdit.SetBlockSignals(enable: true);
		string text = valueLineEdit.Text;
		if (text.Empty())
		{
			text = "0.0";
		}
		if (text.IsValidFloat())
		{
			float value = float.Parse(text);
			if (allowOverwriteMinimumValue && value < MinValue)
			{
				MinValue = value;
			}
			if (allowOverwriteMaximumValue && value > MaxValue)
			{
				MaxValue = value;
			}
			value = Mathf.Clamp(value, MinValue, MaxValue);
			ChangeValue(value);
		}
		else
		{
			valueLineEdit.Text = this.value.ToString(valueFormat);
		}
		valueLineEdit.SetBlockSignals(enable: false);
	}

	public void SliderValueChanged(float value)
	{
		ChangeValue(value);
	}

	public void PopupMenuItemSelected(int id)
	{
		switch (id)
		{
		case 0:
			ChangeValue(DefaultValue);
			break;
		case 1:
			ChangeValue(minValue);
			break;
		case 2:
			ChangeValue(minValue + (maxValue - minValue) * 0.25f);
			break;
		case 3:
			ChangeValue(minValue + (maxValue - minValue) * 0.5f);
			break;
		case 4:
			ChangeValue(minValue + (maxValue - minValue) * 0.75f);
			break;
		case 5:
			ChangeValue(maxValue);
			break;
		}
	}
}
