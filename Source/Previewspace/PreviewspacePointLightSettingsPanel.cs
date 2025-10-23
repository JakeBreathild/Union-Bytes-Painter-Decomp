using Godot;

public class PreviewspacePointLightSettingsPanel : Panel
{
	private PreviewspaceViewport previewspaceViewport;

	private DefaultHoverPanel libraryDefaultHoverPanel;

	private bool hideLibraryDefaultHoverPanel;

	[Export(PropertyHint.None, "")]
	private NodePath workspaceNodePath;

	private Workspace workspace;

	[Export(PropertyHint.None, "")]
	private int lightIndex;

	private Label label;

	private CheckButton enableCheckButton;

	private DefaultColorPickerButton colorDefaultColorPickerButton;

	private DefaultSlider energyDefaultSlider;

	private DefaultSlider rangeDefaultSlider;

	private DefaultSlider angleDefaultSlider;

	private DefaultSlider heightDefaultSlider;

	private DefaultSlider distanceDefaultSlider;

	private CheckButton rotationEnableCheckButton;

	private DefaultSlider rotationSpeedDefaultSlider;

	private Button rotationResetButton;

	public DefaultHoverPanel LibraryDefaultHoverPanel
	{
		get
		{
			return libraryDefaultHoverPanel;
		}
		set
		{
			libraryDefaultHoverPanel = value;
		}
	}

	public Workspace Workspace
	{
		get
		{
			return workspace;
		}
		set
		{
			workspace = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		if (workspaceNodePath != null)
		{
			workspace = GetNodeOrNull<Workspace>(workspaceNodePath);
		}
		previewspaceViewport = Register.PreviewspaceViewport;
		label = GetNodeOrNull<Label>("Label");
		label.Text += lightIndex + 1;
		enableCheckButton = GetNodeOrNull<CheckButton>("Enable");
		enableCheckButton.Connect(Signals.Toggled, this, "Toggled");
		colorDefaultColorPickerButton = GetNodeOrNull<DefaultColorPickerButton>("Color");
		colorDefaultColorPickerButton.Connect(Signals.ColorChanged, this, "ColorChanged");
		colorDefaultColorPickerButton.Connect(Signals.Pressed, this, "ColorPickerButtonPressed");
		colorDefaultColorPickerButton.Connect(Signals.PopupClosed, this, "ColorPickerClosed");
		energyDefaultSlider = GetNodeOrNull<DefaultSlider>("Energy");
		energyDefaultSlider.Connect(Signals.ValueChanged, this, "EnergyChanged");
		rangeDefaultSlider = GetNodeOrNull<DefaultSlider>("Range");
		rangeDefaultSlider.Connect(Signals.ValueChanged, this, "RangeChanged");
		angleDefaultSlider = GetNodeOrNull<DefaultSlider>("Angle");
		angleDefaultSlider.Connect(Signals.ValueChanged, this, "AngleChanged");
		heightDefaultSlider = GetNodeOrNull<DefaultSlider>("Height");
		heightDefaultSlider.Connect(Signals.ValueChanged, this, "HeightChanged");
		distanceDefaultSlider = GetNodeOrNull<DefaultSlider>("Distance");
		distanceDefaultSlider.Connect(Signals.ValueChanged, this, "DistanceChanged");
		rotationEnableCheckButton = GetNodeOrNull<CheckButton>("RotationEnable");
		rotationEnableCheckButton.Connect(Signals.Toggled, this, "RotationToggled");
		rotationSpeedDefaultSlider = GetNodeOrNull<DefaultSlider>("RotationSpeed");
		rotationSpeedDefaultSlider.Connect(Signals.ValueChanged, this, "RotationSpeedChanged");
		rotationResetButton = GetNodeOrNull<Button>("RotationReset");
		rotationResetButton.Connect(Signals.Pressed, this, "RotationResetPressed");
	}

	public void Reset()
	{
		enableCheckButton.Pressed = previewspaceViewport.IsLightEnabled(lightIndex);
		colorDefaultColorPickerButton.Color = previewspaceViewport.GetLightColor(lightIndex);
		energyDefaultSlider.SetValue(previewspaceViewport.GetLightEnergy(lightIndex));
		rangeDefaultSlider.SetValue(previewspaceViewport.GetLightRange(lightIndex));
		angleDefaultSlider.SetValue(previewspaceViewport.GetLightAngle(lightIndex));
		heightDefaultSlider.SetValue(previewspaceViewport.GetLightHeight(lightIndex));
		distanceDefaultSlider.SetValue(previewspaceViewport.GetLightDistance(lightIndex));
		rotationEnableCheckButton.Pressed = previewspaceViewport.IsLightRotationEnabled(lightIndex);
		rotationSpeedDefaultSlider.SetValue(previewspaceViewport.GetLightRotationSpeed(lightIndex));
	}

	public void ColorPickerButtonPressed()
	{
		hideLibraryDefaultHoverPanel = libraryDefaultHoverPanel.HideAfterMouseExited;
		libraryDefaultHoverPanel.HideAfterMouseExited = false;
		InputManager.MouseEnteredUserInterface();
	}

	public void ColorPickerClosed()
	{
		libraryDefaultHoverPanel.HideAfterMouseExited = hideLibraryDefaultHoverPanel;
		InputManager.MouseExitedUserInterface();
	}

	public void Toggled(bool pressed)
	{
		previewspaceViewport.EnableLight(lightIndex, pressed);
	}

	public void ColorChanged(Color color)
	{
		previewspaceViewport.SetLightColor(lightIndex, color);
	}

	public void EnergyChanged(float value)
	{
		previewspaceViewport.SetLightEnergy(lightIndex, energyDefaultSlider.GetValue());
	}

	public void RangeChanged(float value)
	{
		previewspaceViewport.SetLightRange(lightIndex, rangeDefaultSlider.GetValue());
	}

	public void AngleChanged(float value)
	{
		previewspaceViewport.SetLightAngle(lightIndex, angleDefaultSlider.GetValue());
	}

	public void HeightChanged(float value)
	{
		previewspaceViewport.SetLightHeight(lightIndex, heightDefaultSlider.GetValue());
	}

	public void DistanceChanged(float value)
	{
		previewspaceViewport.SetLightDistance(lightIndex, distanceDefaultSlider.GetValue());
	}

	public void RotationToggled(bool pressed)
	{
		previewspaceViewport.EnableLightRotation(lightIndex, pressed);
	}

	public void RotationSpeedChanged(float value)
	{
		previewspaceViewport.SetLightRotationSpeed(lightIndex, rotationSpeedDefaultSlider.GetValue());
	}

	public void RotationResetPressed()
	{
		previewspaceViewport.SetLightRotation(lightIndex, 0f);
	}
}
