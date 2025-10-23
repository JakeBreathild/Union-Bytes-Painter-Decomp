using System;
using Godot;

public class LayerGroupPanel : GroupPanel
{
	private Button colorChannelEnableButton;

	private OptionButton colorBlendingModeOptionButton;

	private ColorPickerButton colorColorPickerButton;

	private bool isColorChannelEnabled = true;

	private Blender.BlendingModeEnum colorChannelBlendingMode = Blender.BlendingModeEnum.ALPHABLEND;

	private Color color = ColorExtension.Black;

	private Button roughnessChannelEnableButton;

	private OptionButton roughnessBlendingModeOptionButton;

	private AdvancedSlider roughnessAdvancedSlider;

	private bool isRoughnessChannelEnabled = true;

	private Blender.BlendingModeEnum roughnessChannelBlendingMode = Blender.BlendingModeEnum.ALPHABLEND;

	private Value roughness = Value.White;

	private Button metallicityChannelEnableButton;

	private OptionButton metallicityBlendingModeOptionButton;

	private AdvancedSlider metallicityAdvancedSlider;

	private bool isMetallicityChannelEnabled = true;

	private Blender.BlendingModeEnum metallicityChannelBlendingMode = Blender.BlendingModeEnum.ALPHABLEND;

	private Value metallicity = Value.Black;

	private Button heightChannelEnableButton;

	private OptionButton heightBlendingModeOptionButton;

	private AdvancedSlider heightAdvancedSlider;

	private bool isHeightChannelEnabled = true;

	private Blender.BlendingModeEnum heightChannelBlendingMode = Blender.BlendingModeEnum.ALPHABLEND;

	private Value height = Value.Gray;

	private Button emissionChannelEnableButton;

	private OptionButton emissionBlendingModeOptionButton;

	private ColorPickerButton emissionColorPickerButton;

	private bool isEmissionChannelEnabled;

	private Blender.BlendingModeEnum emissionChannelBlendingMode = Blender.BlendingModeEnum.ADDITION;

	private Color emission = ColorExtension.Black;

	private Action valueChangedCallback;

	public bool IsColorChannelEnabled
	{
		get
		{
			return isColorChannelEnabled;
		}
		set
		{
			isColorChannelEnabled = value;
		}
	}

	public Blender.BlendingModeEnum ColorChannelBlendingMode
	{
		get
		{
			return colorChannelBlendingMode;
		}
		set
		{
			colorChannelBlendingMode = value;
		}
	}

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
		}
	}

	public bool IsRoughnessChannelEnabled
	{
		get
		{
			return isRoughnessChannelEnabled;
		}
		set
		{
			isRoughnessChannelEnabled = value;
		}
	}

	public Blender.BlendingModeEnum RoughnessChannelBlendingMode
	{
		get
		{
			return roughnessChannelBlendingMode;
		}
		set
		{
			roughnessChannelBlendingMode = value;
		}
	}

	public Value Roughness
	{
		get
		{
			return roughness;
		}
		set
		{
			roughness = value;
		}
	}

	public bool IsMetallicityChannelEnabled
	{
		get
		{
			return isMetallicityChannelEnabled;
		}
		set
		{
			isMetallicityChannelEnabled = value;
		}
	}

	public Blender.BlendingModeEnum MetallicityChannelBlendingMode
	{
		get
		{
			return metallicityChannelBlendingMode;
		}
		set
		{
			metallicityChannelBlendingMode = value;
		}
	}

	public Value Metallicity
	{
		get
		{
			return metallicity;
		}
		set
		{
			metallicity = value;
		}
	}

	public bool IsHeightChannelEnabled
	{
		get
		{
			return isHeightChannelEnabled;
		}
		set
		{
			isHeightChannelEnabled = value;
		}
	}

	public Blender.BlendingModeEnum HeightChannelBlendingMode
	{
		get
		{
			return heightChannelBlendingMode;
		}
		set
		{
			heightChannelBlendingMode = value;
		}
	}

	public Value Height
	{
		get
		{
			return height;
		}
		set
		{
			height = value;
		}
	}

	public bool IsEmissionChannelEnabled
	{
		get
		{
			return isEmissionChannelEnabled;
		}
		set
		{
			isEmissionChannelEnabled = value;
		}
	}

	public Blender.BlendingModeEnum EmissionChannelBlendingMode
	{
		get
		{
			return emissionChannelBlendingMode;
		}
		set
		{
			emissionChannelBlendingMode = value;
		}
	}

	public Color Emission
	{
		get
		{
			return emission;
		}
		set
		{
			emission = value;
		}
	}

	public Action ValueChangedCallback
	{
		get
		{
			return valueChangedCallback;
		}
		set
		{
			valueChangedCallback = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		colorChannelEnableButton = GetNodeOrNull<Button>("VC/ColorChannelHC/Enabled");
		colorChannelEnableButton.Pressed = isColorChannelEnabled;
		colorChannelEnableButton.Connect(Signals.Toggled, this, "ColorChannelEnableToggled");
		colorBlendingModeOptionButton = GetNodeOrNull<OptionButton>("VC/ColorChannelHC/BlendingMode");
		for (int i = 0; i <= 7; i++)
		{
			colorBlendingModeOptionButton.AddItem(Blender.BlendingModeName[i], i);
		}
		colorBlendingModeOptionButton.Selected = (int)colorChannelBlendingMode;
		colorBlendingModeOptionButton.GetPopup().Connect(Signals.IdPressed, this, "ColorBlendingModeSelected");
		colorColorPickerButton = GetNodeOrNull<ColorPickerButton>("VC/Color");
		colorColorPickerButton.Color = color;
		colorColorPickerButton.Connect(Signals.ColorChanged, this, "ColorChanged");
		roughnessChannelEnableButton = GetNodeOrNull<Button>("VC/RoughnessChannelHC/Enabled");
		roughnessChannelEnableButton.Pressed = isRoughnessChannelEnabled;
		roughnessChannelEnableButton.Connect(Signals.Toggled, this, "RoughnessChannelEnableToggled");
		roughnessBlendingModeOptionButton = GetNodeOrNull<OptionButton>("VC/RoughnessChannelHC/BlendingMode");
		for (int j = 0; j <= 7; j++)
		{
			roughnessBlendingModeOptionButton.AddItem(Blender.BlendingModeName[j], j);
		}
		roughnessBlendingModeOptionButton.Selected = (int)roughnessChannelBlendingMode;
		roughnessBlendingModeOptionButton.GetPopup().Connect(Signals.IdPressed, this, "RoughnessBlendingModeSelected");
		roughnessAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/Roughness");
		roughnessAdvancedSlider.Value = roughness.v;
		roughnessAdvancedSlider.DefaultValue = roughness.v;
		roughnessAdvancedSlider.ValueChangedCallback = RoughnessChanged;
		metallicityChannelEnableButton = GetNodeOrNull<Button>("VC/MetallicityChannelHC/Enabled");
		metallicityChannelEnableButton.Pressed = isMetallicityChannelEnabled;
		metallicityChannelEnableButton.Connect(Signals.Toggled, this, "MetallicityChannelEnableToggled");
		metallicityBlendingModeOptionButton = GetNodeOrNull<OptionButton>("VC/MetallicityChannelHC/BlendingMode");
		for (int k = 0; k <= 7; k++)
		{
			metallicityBlendingModeOptionButton.AddItem(Blender.BlendingModeName[k], k);
		}
		metallicityBlendingModeOptionButton.Selected = (int)metallicityChannelBlendingMode;
		metallicityBlendingModeOptionButton.GetPopup().Connect(Signals.IdPressed, this, "MetallicityBlendingModeSelected");
		metallicityAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/Metallicity");
		metallicityAdvancedSlider.Value = metallicity.v;
		metallicityAdvancedSlider.DefaultValue = metallicity.v;
		metallicityAdvancedSlider.ValueChangedCallback = MetallicityChanged;
		heightChannelEnableButton = GetNodeOrNull<Button>("VC/HeightChannelHC/Enabled");
		heightChannelEnableButton.Pressed = isHeightChannelEnabled;
		heightChannelEnableButton.Connect(Signals.Toggled, this, "HeightChannelEnableToggled");
		heightBlendingModeOptionButton = GetNodeOrNull<OptionButton>("VC/HeightChannelHC/BlendingMode");
		for (int l = 0; l <= 7; l++)
		{
			heightBlendingModeOptionButton.AddItem(Blender.BlendingModeName[l], l);
		}
		heightBlendingModeOptionButton.Selected = (int)heightChannelBlendingMode;
		heightBlendingModeOptionButton.GetPopup().Connect(Signals.IdPressed, this, "HeightBlendingModeSelected");
		heightAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/Height");
		heightAdvancedSlider.Value = height.v;
		heightAdvancedSlider.DefaultValue = height.v;
		heightAdvancedSlider.ValueChangedCallback = HeightChanged;
		emissionChannelEnableButton = GetNodeOrNull<Button>("VC/EmissionChannelHC/Enabled");
		emissionChannelEnableButton.Pressed = isEmissionChannelEnabled;
		emissionChannelEnableButton.Connect(Signals.Toggled, this, "EmissionChannelEnableToggled");
		emissionBlendingModeOptionButton = GetNodeOrNull<OptionButton>("VC/EmissionChannelHC/BlendingMode");
		for (int m = 0; m <= 7; m++)
		{
			emissionBlendingModeOptionButton.AddItem(Blender.BlendingModeName[m], m);
		}
		emissionBlendingModeOptionButton.Selected = (int)emissionChannelBlendingMode;
		emissionBlendingModeOptionButton.GetPopup().Connect(Signals.IdPressed, this, "EmissionBlendingModeSelected");
		emissionColorPickerButton = GetNodeOrNull<ColorPickerButton>("VC/Emission");
		emissionColorPickerButton.Color = emission;
		emissionColorPickerButton.Connect(Signals.ColorChanged, this, "EmissionChanged");
		base.ResetButtonPressedCallback = Reset;
	}

	public new void Reset()
	{
		base.Reset();
		colorChannelEnableButton.SetBlockSignals(enable: true);
		colorBlendingModeOptionButton.SetBlockSignals(enable: true);
		colorColorPickerButton.SetBlockSignals(enable: true);
		roughnessChannelEnableButton.SetBlockSignals(enable: true);
		roughnessBlendingModeOptionButton.SetBlockSignals(enable: true);
		roughnessAdvancedSlider.SetBlockSignals(enable: true);
		metallicityChannelEnableButton.SetBlockSignals(enable: true);
		metallicityBlendingModeOptionButton.SetBlockSignals(enable: true);
		metallicityAdvancedSlider.SetBlockSignals(enable: true);
		heightChannelEnableButton.SetBlockSignals(enable: true);
		heightBlendingModeOptionButton.SetBlockSignals(enable: true);
		heightAdvancedSlider.SetBlockSignals(enable: true);
		emissionChannelEnableButton.SetBlockSignals(enable: true);
		emissionBlendingModeOptionButton.SetBlockSignals(enable: true);
		emissionColorPickerButton.SetBlockSignals(enable: true);
		isColorChannelEnabled = true;
		colorChannelEnableButton.Pressed = isColorChannelEnabled;
		colorChannelBlendingMode = Blender.BlendingModeEnum.ALPHABLEND;
		colorBlendingModeOptionButton.Selected = (int)colorChannelBlendingMode;
		color = ColorExtension.Black;
		colorColorPickerButton.Color = color;
		isRoughnessChannelEnabled = true;
		roughnessChannelEnableButton.Pressed = isRoughnessChannelEnabled;
		roughnessChannelBlendingMode = Blender.BlendingModeEnum.ALPHABLEND;
		roughnessBlendingModeOptionButton.Selected = (int)roughnessChannelBlendingMode;
		roughnessAdvancedSlider.Reset();
		roughness.v = roughnessAdvancedSlider.Value;
		isMetallicityChannelEnabled = true;
		metallicityChannelEnableButton.Pressed = isMetallicityChannelEnabled;
		metallicityChannelBlendingMode = Blender.BlendingModeEnum.ALPHABLEND;
		metallicityBlendingModeOptionButton.Selected = (int)metallicityChannelBlendingMode;
		metallicityAdvancedSlider.Reset();
		metallicity.v = metallicityAdvancedSlider.Value;
		isHeightChannelEnabled = true;
		heightChannelEnableButton.Pressed = isHeightChannelEnabled;
		heightChannelBlendingMode = Blender.BlendingModeEnum.ALPHABLEND;
		heightBlendingModeOptionButton.Selected = (int)heightChannelBlendingMode;
		heightAdvancedSlider.Reset();
		height.v = heightAdvancedSlider.Value;
		isEmissionChannelEnabled = false;
		emissionChannelEnableButton.Pressed = isEmissionChannelEnabled;
		emissionChannelBlendingMode = Blender.BlendingModeEnum.ADDITION;
		emissionBlendingModeOptionButton.Selected = (int)emissionChannelBlendingMode;
		emission = ColorExtension.Black;
		emissionColorPickerButton.Color = emission;
		colorChannelEnableButton.SetBlockSignals(enable: false);
		colorBlendingModeOptionButton.SetBlockSignals(enable: false);
		colorColorPickerButton.SetBlockSignals(enable: false);
		roughnessChannelEnableButton.SetBlockSignals(enable: false);
		roughnessBlendingModeOptionButton.SetBlockSignals(enable: false);
		roughnessAdvancedSlider.SetBlockSignals(enable: false);
		metallicityChannelEnableButton.SetBlockSignals(enable: false);
		metallicityBlendingModeOptionButton.SetBlockSignals(enable: false);
		metallicityAdvancedSlider.SetBlockSignals(enable: false);
		heightChannelEnableButton.SetBlockSignals(enable: false);
		heightBlendingModeOptionButton.SetBlockSignals(enable: false);
		heightAdvancedSlider.SetBlockSignals(enable: false);
		emissionChannelEnableButton.SetBlockSignals(enable: false);
		emissionBlendingModeOptionButton.SetBlockSignals(enable: false);
		emissionColorPickerButton.SetBlockSignals(enable: false);
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void ColorChannelEnableToggled(bool pressed)
	{
		isColorChannelEnabled = pressed;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void ColorBlendingModeSelected(int index)
	{
		colorChannelBlendingMode = (Blender.BlendingModeEnum)index;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void ColorChanged(Color color)
	{
		this.color = color;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void RoughnessChannelEnableToggled(bool pressed)
	{
		isRoughnessChannelEnabled = pressed;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void RoughnessBlendingModeSelected(int index)
	{
		roughnessChannelBlendingMode = (Blender.BlendingModeEnum)index;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void RoughnessChanged(float value)
	{
		roughness.v = roughnessAdvancedSlider.Value;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void MetallicityChannelEnableToggled(bool pressed)
	{
		isMetallicityChannelEnabled = pressed;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void MetallicityBlendingModeSelected(int index)
	{
		metallicityChannelBlendingMode = (Blender.BlendingModeEnum)index;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void MetallicityChanged(float value)
	{
		metallicity.v = metallicityAdvancedSlider.Value;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void HeightChannelEnableToggled(bool pressed)
	{
		isHeightChannelEnabled = pressed;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void HeightBlendingModeSelected(int index)
	{
		heightChannelBlendingMode = (Blender.BlendingModeEnum)index;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void HeightChanged(float value)
	{
		height.v = heightAdvancedSlider.Value;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void EmissionChannelEnableToggled(bool pressed)
	{
		isEmissionChannelEnabled = pressed;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void EmissionBlendingModeSelected(int index)
	{
		emissionChannelBlendingMode = (Blender.BlendingModeEnum)index;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}

	public void EmissionChanged(Color emission)
	{
		this.emission = emission;
		if (valueChangedCallback != null)
		{
			valueChangedCallback();
		}
	}
}
