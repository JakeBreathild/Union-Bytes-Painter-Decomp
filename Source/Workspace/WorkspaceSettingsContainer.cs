using System.IO;
using Godot;

public class WorkspaceSettingsContainer : DefaultContainer
{
	private PreviewspaceViewport previewspaceViewport;

	private Gui gui;

	[Export(PropertyHint.None, "")]
	private NodePath libraryDefaultHoverPanelNodePath;

	private DefaultHoverPanel libraryDefaultHoverPanel;

	private bool hideLibraryDefaultHoverPanel;

	[Export(PropertyHint.None, "")]
	private NodePath loadPreviewMeshFileDialogNodePath;

	private PreviewFileDialog loadPreviewMeshFileDialog;

	[Export(PropertyHint.None, "")]
	private NodePath savePreviewMeshFileDialogNodePath;

	private PreviewFileDialog savePreviewMeshFileDialog;

	[Export(PropertyHint.None, "")]
	private NodePath previewMeshScaleDefaultSliderNodePath;

	private DefaultSlider previewMeshScaleDefaultSlider;

	[Export(PropertyHint.None, "")]
	private NodePath previewUvScaleDefaultSliderNodePath;

	private DefaultSlider previewUvScaleDefaultSlider;

	[Export(PropertyHint.None, "")]
	private NodePath glowCheckButtonNodePath;

	private CheckButton glowCheckButton;

	[Export(PropertyHint.None, "")]
	private NodePath glowIntensityDefaultSliderNodePath;

	private DefaultSlider glowIntensityDefaultSlider;

	[Export(PropertyHint.None, "")]
	private NodePath emissionStrengthDefaultSliderNodePath;

	private DefaultSlider emissionStrengthDefaultSlider;

	[Export(PropertyHint.None, "")]
	private NodePath ambientLightEnergyDefaultSliderNodePath;

	private DefaultSlider ambientLightEnergyDefaultSlider;

	[Export(PropertyHint.None, "")]
	private NodePath skyContributionDefaultSliderNodePath;

	private DefaultSlider skyContributionDefaultSlider;

	[Export(PropertyHint.None, "")]
	private NodePath ambientLightColorDefaultColorPickerButtonNodePath;

	private DefaultColorPickerButton ambientLightColorDefaultColorPickerButton;

	[Export(PropertyHint.None, "")]
	private NodePath directLightCheckButtonNodePath;

	private CheckButton directLightCheckButton;

	[Export(PropertyHint.None, "")]
	private NodePath directLightEnergyDefaultSliderNodePath;

	private DefaultSlider directLightEnergyDefaultSlider;

	[Export(PropertyHint.None, "")]
	private NodePath directLightColorDefaultColorPickerButtonNodePath;

	private DefaultColorPickerButton directLightColorDefaultColorPickerButton;

	[Export(PropertyHint.None, "")]
	private NodePath directLightShadowCheckButtonNodePath;

	private CheckButton directLightShadowCheckButton;

	[Export(PropertyHint.None, "")]
	private NodePath pointLight1SettingsPanelNodePath;

	private PreviewspacePointLightSettingsPanel pointLight1SettingsPanel;

	[Export(PropertyHint.None, "")]
	private NodePath pointLight2SettingsPanelNodePath;

	private PreviewspacePointLightSettingsPanel pointLight2SettingsPanel;

	[Export(PropertyHint.None, "")]
	private NodePath pointLight3SettingsPanelNodePath;

	private PreviewspacePointLightSettingsPanel pointLight3SettingsPanel;

	[Export(PropertyHint.None, "")]
	private NodePath lightIconsCheckButtonNodePath;

	private CheckButton lightIconsCheckButton;

	public override void _Ready()
	{
		base._Ready();
		gui = Register.Gui;
		previewspaceViewport = Register.PreviewspaceViewport;
		libraryDefaultHoverPanel = GetNodeOrNull<DefaultHoverPanel>(libraryDefaultHoverPanelNodePath);
		loadPreviewMeshFileDialog = GetNodeOrNull<PreviewFileDialog>(loadPreviewMeshFileDialogNodePath);
		loadPreviewMeshFileDialog.AddFontOverride("font", Resources.DefaultFont);
		loadPreviewMeshFileDialog.PopupExclusive = true;
		loadPreviewMeshFileDialog.WindowTitle = "Load Preview Mesh";
		loadPreviewMeshFileDialog.Connect(Signals.FileSelected, this, "LoadPreviewMesh");
		savePreviewMeshFileDialog = GetNodeOrNull<PreviewFileDialog>(savePreviewMeshFileDialogNodePath);
		savePreviewMeshFileDialog.AddFontOverride("font", Resources.DefaultFont);
		savePreviewMeshFileDialog.PopupExclusive = true;
		savePreviewMeshFileDialog.WindowTitle = "Save Preview Mesh";
		savePreviewMeshFileDialog.Connect(Signals.FileSelected, this, "SavePreviewMesh");
		previewMeshScaleDefaultSlider = GetNodeOrNull<DefaultSlider>(previewMeshScaleDefaultSliderNodePath);
		previewMeshScaleDefaultSlider.Connect(Signals.ValueChanged, this, "PreviewMeshScaleChanged");
		previewUvScaleDefaultSlider = GetNodeOrNull<DefaultSlider>(previewUvScaleDefaultSliderNodePath);
		previewUvScaleDefaultSlider.Connect(Signals.ValueChanged, this, "PreviewUvScaleChanged");
		glowCheckButton = GetNodeOrNull<CheckButton>(glowCheckButtonNodePath);
		glowCheckButton.Connect(Signals.Toggled, this, "GlowToggled");
		glowIntensityDefaultSlider = GetNodeOrNull<DefaultSlider>(glowIntensityDefaultSliderNodePath);
		glowIntensityDefaultSlider.Connect(Signals.ValueChanged, this, "GlowIntensityChanged");
		emissionStrengthDefaultSlider = GetNodeOrNull<DefaultSlider>(emissionStrengthDefaultSliderNodePath);
		emissionStrengthDefaultSlider.Connect(Signals.ValueChanged, this, "PreviewMeshEmissionStrengthChanged");
		ambientLightEnergyDefaultSlider = GetNodeOrNull<DefaultSlider>(ambientLightEnergyDefaultSliderNodePath);
		ambientLightEnergyDefaultSlider.Connect(Signals.ValueChanged, this, "AmbientLightEnergyChanged");
		skyContributionDefaultSlider = GetNodeOrNull<DefaultSlider>(skyContributionDefaultSliderNodePath);
		skyContributionDefaultSlider.Connect(Signals.ValueChanged, this, "SkyContributionChanged");
		ambientLightColorDefaultColorPickerButton = GetNodeOrNull<DefaultColorPickerButton>(ambientLightColorDefaultColorPickerButtonNodePath);
		ambientLightColorDefaultColorPickerButton.Connect(Signals.ColorChanged, this, "AmbientLightColorChanged");
		ambientLightColorDefaultColorPickerButton.Connect(Signals.Pressed, this, "ColorPickerButtonPressed");
		ambientLightColorDefaultColorPickerButton.Connect(Signals.PopupClosed, this, "ColorPickerClosed");
		directLightCheckButton = GetNodeOrNull<CheckButton>(directLightCheckButtonNodePath);
		directLightCheckButton.Connect(Signals.Toggled, this, "DirectLightToggled");
		directLightEnergyDefaultSlider = GetNodeOrNull<DefaultSlider>(directLightEnergyDefaultSliderNodePath);
		directLightEnergyDefaultSlider.Connect(Signals.ValueChanged, this, "DirectLightEnergyChanged");
		directLightColorDefaultColorPickerButton = GetNodeOrNull<DefaultColorPickerButton>(directLightColorDefaultColorPickerButtonNodePath);
		directLightColorDefaultColorPickerButton.Connect(Signals.ColorChanged, this, "DirectLightColorChanged");
		directLightColorDefaultColorPickerButton.Connect(Signals.Pressed, this, "ColorPickerButtonPressed");
		directLightColorDefaultColorPickerButton.Connect(Signals.PopupClosed, this, "ColorPickerClosed");
		directLightShadowCheckButton = GetNodeOrNull<CheckButton>(directLightShadowCheckButtonNodePath);
		directLightShadowCheckButton.Connect(Signals.Toggled, this, "DirectLightShadowToggled");
		pointLight1SettingsPanel = GetNodeOrNull<PreviewspacePointLightSettingsPanel>(pointLight1SettingsPanelNodePath);
		pointLight1SettingsPanel.Workspace = workspace;
		pointLight1SettingsPanel.LibraryDefaultHoverPanel = libraryDefaultHoverPanel;
		pointLight2SettingsPanel = GetNodeOrNull<PreviewspacePointLightSettingsPanel>(pointLight2SettingsPanelNodePath);
		pointLight2SettingsPanel.Workspace = workspace;
		pointLight2SettingsPanel.LibraryDefaultHoverPanel = libraryDefaultHoverPanel;
		pointLight3SettingsPanel = GetNodeOrNull<PreviewspacePointLightSettingsPanel>(pointLight3SettingsPanelNodePath);
		pointLight3SettingsPanel.Workspace = workspace;
		pointLight3SettingsPanel.LibraryDefaultHoverPanel = libraryDefaultHoverPanel;
		lightIconsCheckButton = GetNodeOrNull<CheckButton>(lightIconsCheckButtonNodePath);
		lightIconsCheckButton.Connect(Signals.Toggled, this, "LightIconsToggled");
	}

	public override void Reset()
	{
		base.Reset();
		previewMeshScaleDefaultSlider.SetValue(Register.PreviewspaceMeshManager.GetMeshScale());
		previewUvScaleDefaultSlider.SetValue(Register.PreviewspaceMeshManager.GetUvScale());
		glowCheckButton.Pressed = workspace.EnvironmentManager.IsGlowEnabled();
		glowIntensityDefaultSlider.SetValue(workspace.EnvironmentManager.GetGlowIntensity());
		emissionStrengthDefaultSlider.SetValue(MaterialManager.GetEmissionStrength());
		ambientLightEnergyDefaultSlider.SetValue(workspace.EnvironmentManager.GetEnvironmentAmbientEnergy());
		skyContributionDefaultSlider.SetValue(workspace.EnvironmentManager.GetEnvironmentSkyContribution());
		ambientLightColorDefaultColorPickerButton.Color = workspace.EnvironmentManager.GetEnvironmentAmbientColor();
		directLightCheckButton.Pressed = workspace.EnvironmentManager.IsDirectionalLightEnabled();
		directLightEnergyDefaultSlider.SetValue(workspace.EnvironmentManager.GetDirectionalLightEnergy());
		directLightColorDefaultColorPickerButton.Color = workspace.EnvironmentManager.GetDirectionalLightColor();
		directLightShadowCheckButton.Pressed = workspace.EnvironmentManager.IsDirectionalLightShadowEnabled();
		pointLight1SettingsPanel.Reset();
		pointLight2SettingsPanel.Reset();
		pointLight3SettingsPanel.Reset();
		lightIconsCheckButton.Pressed = true;
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

	public void OpenLoadPreviewMeshFileDialog()
	{
		string path = Settings.ImportPath;
		if (!path.Empty())
		{
			loadPreviewMeshFileDialog.CurrentDir = path;
		}
		loadPreviewMeshFileDialog.Update();
		loadPreviewMeshFileDialog.PopupCentered();
	}

	public void LoadPreviewMesh(string file)
	{
		if (file != "")
		{
			if (System.IO.File.Exists(file) && file.Substring(file.Length - 3, 3) == "obj")
			{
				previewMeshScaleDefaultSlider.SetValue(1f);
				previewUvScaleDefaultSlider.SetValue(1f);
				Register.PreviewspaceMeshManager.LoadMesh(file);
				Register.CollisionManager.Update();
				workspace.CameraManager.ResetPreviewspaceCamera();
				workspace.BakeManager.Update(workspace.Worksheet);
				gui.DisableBakeing(disabled: false);
				gui.DisplaySettingsContainer.DisableWireframe(disabled: false);
			}
			else
			{
				Gui.MsgAcceptDialogPopupCentered("Only *.obj files are supported!");
			}
			Settings.ImportPath = System.IO.Path.GetFullPath(file);
		}
	}

	public void OpenSavePreviewMeshFileDialog()
	{
		string path = Settings.ExportPath;
		if (!path.Empty())
		{
			savePreviewMeshFileDialog.CurrentDir = path;
		}
		savePreviewMeshFileDialog.PopupCentered();
	}

	public void SavePreviewMesh(string file)
	{
		if (file != "" && file.Substring(file.Length - 3, 3) == "obj")
		{
			Register.PreviewspaceMeshManager.SaveMesh(file);
		}
	}

	public void ResetPreviewMesh()
	{
		Register.PreviewspaceMeshManager.Reset();
		Register.CollisionManager.Update();
		workspace.CameraManager.ResetPreviewspaceCamera();
		workspace.BakeManager.Update(workspace.Worksheet);
		gui.DisableBakeing(disabled: true);
		gui.DisplaySettingsContainer.DisableWireframe(disabled: true);
	}

	public void ChangePreviewMeshScale(float scale)
	{
		previewMeshScaleDefaultSlider.SetValue(scale);
	}

	public void PreviewMeshScaleChanged(float scale)
	{
		Register.PreviewspaceMeshManager.SetMeshScale(previewMeshScaleDefaultSlider.GetValue());
		Register.CollisionManager.Update();
	}

	public void ChangePreviewUvScale(float scale)
	{
		previewUvScaleDefaultSlider.SetValue(scale);
	}

	public void PreviewUvScaleChanged(float scale)
	{
		Register.PreviewspaceMeshManager.SetUvScale(previewUvScaleDefaultSlider.GetValue());
	}

	public void GlowToggled(bool pressed)
	{
		workspace.EnvironmentManager.EnableGlow(pressed);
	}

	public void GlowIntensityChanged(float intensity)
	{
		workspace.EnvironmentManager.SetGlowIntensity(glowIntensityDefaultSlider.GetValue());
	}

	public void ChangePreviewMaterialEmissionStrength(float emissionStrength)
	{
		emissionStrengthDefaultSlider.SetValue(emissionStrength);
	}

	public void PreviewMeshEmissionStrengthChanged(float emissionStrength)
	{
		MaterialManager.SetEmissionStrength(emissionStrengthDefaultSlider.GetValue());
	}

	public void ChangeAmbientLightColor(Color color)
	{
		ambientLightColorDefaultColorPickerButton.Color = color;
	}

	public void AmbientLightColorChanged(Color color)
	{
		workspace.EnvironmentManager.SetEnvironmentAmbientColor(color);
	}

	public void ChangeAmbientLightEnergy(float energy)
	{
		ambientLightEnergyDefaultSlider.SetValue(energy);
	}

	public void AmbientLightEnergyChanged(float energy)
	{
		workspace.EnvironmentManager.SetEnvironmentAmbientEnergy(ambientLightEnergyDefaultSlider.GetValue());
	}

	public void ChangeSkyContribution(float energy)
	{
		skyContributionDefaultSlider.SetValue(energy);
	}

	public void SkyContributionChanged(float energy)
	{
		workspace.EnvironmentManager.SetEnvironmentSkyContribution(skyContributionDefaultSlider.GetValue());
	}

	public void ToggleDirectLight(bool pressed)
	{
		directLightCheckButton.Pressed = pressed;
	}

	public void DirectLightToggled(bool pressed)
	{
		workspace.EnvironmentManager.EnableDirectionalLight(pressed);
	}

	public void ChangeDirectLightColor(Color color)
	{
		directLightColorDefaultColorPickerButton.Color = color;
	}

	public void DirectLightColorChanged(Color color)
	{
		workspace.EnvironmentManager.SetDirectionalLightColor(color);
	}

	public void ChangeDirectLightEnergy(float energy)
	{
		directLightEnergyDefaultSlider.SetValue(energy);
	}

	public void DirectLightEnergyChanged(float energy)
	{
		workspace.EnvironmentManager.SetDirectionalLightEnergy(directLightEnergyDefaultSlider.GetValue());
	}

	public void DirectLightShadowToggled(bool pressed)
	{
		workspace.EnvironmentManager.EnableDirectionalLightShadow(pressed);
	}

	public void LightIconsToggled(bool pressed)
	{
		previewspaceViewport.EnableLightIcons(pressed);
	}
}
