using System.IO;
using Godot;

public class StampSettingsContainer : DefaultHBoxContainer
{
	private Button loadMaskButton;

	[Export(PropertyHint.None, "")]
	private NodePath loadMaskFileDialogNodePath;

	private PreviewFileDialog loadMaskFileDialog;

	private Button loadDecalButton;

	[Export(PropertyHint.None, "")]
	private NodePath loadDecalFileDialogNodePath;

	private PreviewFileDialog loadDecalFileDialog;

	private Button maskModeButton;

	private Button decalModeButton;

	private float blendingStrengthInitialValue;

	private ToolSlider maskBlendingStrengtSlider;

	private float maskRotationInitialValue;

	private ToolSlider maskRotationSlider;

	private bool maskRandomRotationInitialValue;

	private CheckButton maskRandomRotationCheckButton;

	private bool heightBlendInitialValue;

	private CheckButton heightBlendCheckButton;

	private float heightOffsetInitialValue;

	private ToolSlider heightOffsetSlider;

	private float heightBlendingFactorInitialValue;

	private ToolSlider heightBlendingFactorSlider;

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
			loadMaskButton.Disabled = disabled;
			loadDecalButton.Disabled = disabled;
			maskRandomRotationCheckButton.Disabled = disabled;
			maskRotationSlider.Disabled = disabled;
			maskBlendingStrengtSlider.Disabled = disabled;
			heightBlendCheckButton.Disabled = disabled;
			heightOffsetSlider.Disabled = disabled;
			heightBlendingFactorSlider.Disabled = disabled;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		maskModeButton = GetNodeOrNull<Button>("Mode/HC/Mask");
		maskModeButton.Connect(Signals.Pressed, this, "MaskModeButtonPressed");
		decalModeButton = GetNodeOrNull<Button>("Mode/HC/Decal");
		decalModeButton.Connect(Signals.Pressed, this, "DecalModeButtonPressed");
		loadMaskButton = GetNodeOrNull<Button>("LoadMask/Button");
		loadMaskButton.Connect(Signals.Pressed, this, "LoadMaskFileDialogPressed");
		loadMaskFileDialog = GetNodeOrNull<PreviewFileDialog>(loadMaskFileDialogNodePath);
		loadMaskFileDialog.AddFontOverride("font", Resources.DefaultFont);
		loadMaskFileDialog.PopupExclusive = true;
		loadMaskFileDialog.WindowTitle = "Load Mask";
		loadMaskFileDialog.Connect(Signals.FileSelected, this, "LoadMask");
		loadDecalButton = GetNodeOrNull<Button>("LoadDecal/Button");
		loadDecalButton.Connect(Signals.Pressed, this, "LoadDecalFileDialogPressed");
		loadDecalFileDialog = GetNodeOrNull<PreviewFileDialog>(loadDecalFileDialogNodePath);
		loadDecalFileDialog.AddFontOverride("font", Resources.DefaultFont);
		loadDecalFileDialog.PopupExclusive = true;
		loadDecalFileDialog.WindowTitle = "Load Decal";
		loadDecalFileDialog.Connect(Signals.FileSelected, this, "LoadDecal");
		maskBlendingStrengtSlider = GetNodeOrNull<ToolSlider>("Blending");
		blendingStrengthInitialValue = maskBlendingStrengtSlider.GetValue();
		maskBlendingStrengtSlider.ValueChangedCallback = MaskBlendingStrengtChanged;
		maskRotationSlider = GetNodeOrNull<ToolSlider>("Rotation");
		maskRotationInitialValue = maskRotationSlider.GetValue();
		maskRotationSlider.ValueChangedCallback = MaskRotationChanged;
		maskRandomRotationCheckButton = GetNodeOrNull<CheckButton>("RandomRotation/CheckButton");
		maskRandomRotationInitialValue = maskRandomRotationCheckButton.Pressed;
		maskRandomRotationCheckButton.Connect(Signals.Toggled, this, "MaskRandomRotationToggled");
		heightBlendCheckButton = GetNodeOrNull<CheckButton>("HeightBlend/CheckButton");
		heightBlendInitialValue = heightBlendCheckButton.Pressed;
		heightBlendCheckButton.Connect(Signals.Toggled, this, "HeightBlendToggled");
		heightOffsetSlider = GetNodeOrNull<ToolSlider>("HeightOffset");
		heightOffsetInitialValue = heightOffsetSlider.GetValue();
		heightOffsetSlider.ValueChangedCallback = HeightOffsetChanged;
		heightBlendingFactorSlider = GetNodeOrNull<ToolSlider>("HeightBlendingFactor");
		heightBlendingFactorInitialValue = heightBlendingFactorSlider.GetValue();
		heightBlendingFactorSlider.ValueChangedCallback = HeightBlendingFactorChanged;
	}

	public override void Reset()
	{
		base.Reset();
		maskModeButton.Pressed = true;
		decalModeButton.Pressed = false;
		maskBlendingStrengtSlider.SetValue(blendingStrengthInitialValue);
		maskRotationSlider.SetValue(maskRotationInitialValue);
		maskRandomRotationCheckButton.Pressed = maskRandomRotationInitialValue;
		heightBlendCheckButton.Pressed = heightBlendInitialValue;
		heightOffsetSlider.SetValue(heightOffsetInitialValue);
		heightBlendingFactorSlider.SetValue(heightBlendingFactorInitialValue);
		base.Visible = false;
	}

	public void ChangeModeButtons(DrawingToolStamp.ModeEnum mode)
	{
		switch (mode)
		{
		case DrawingToolStamp.ModeEnum.MASK:
			maskModeButton.Pressed = true;
			decalModeButton.Pressed = false;
			break;
		case DrawingToolStamp.ModeEnum.DECAL:
			maskModeButton.Pressed = false;
			decalModeButton.Pressed = true;
			break;
		}
	}

	public void MaskModeButtonPressed()
	{
		if (Register.DrawingManager.StampTool.Mask != null)
		{
			maskModeButton.Pressed = true;
			decalModeButton.Pressed = false;
			Register.DrawingManager.StampTool.Mode = DrawingToolStamp.ModeEnum.MASK;
			ImageTexture texture = new ImageTexture();
			texture.CreateFromImage(Register.DrawingManager.StampTool.Mask.Thumbnail, 0u);
			Register.MaterialContainer.ChangeDecalLabel("Mask");
			Register.MaterialContainer.ChangeDecalPreview(texture);
		}
		else
		{
			maskModeButton.Pressed = false;
			decalModeButton.Pressed = true;
		}
	}

	public void DecalModeButtonPressed()
	{
		if (Register.DrawingManager.StampTool.Decal != null)
		{
			maskModeButton.Pressed = false;
			decalModeButton.Pressed = true;
			Register.DrawingManager.StampTool.Mode = DrawingToolStamp.ModeEnum.DECAL;
			ImageTexture texture = new ImageTexture();
			texture.CreateFromImage(Register.DrawingManager.StampTool.Decal.Thumbnail, 0u);
			Register.MaterialContainer.ChangeDecalLabel("Decal");
			Register.MaterialContainer.ChangeDecalPreview(texture);
		}
		else
		{
			maskModeButton.Pressed = true;
			decalModeButton.Pressed = false;
			LoadDecalFileDialogPressed();
		}
	}

	public void LoadMaskFileDialogPressed()
	{
		string path = Settings.MasksPath;
		if (!path.Empty())
		{
			loadMaskFileDialog.CurrentDir = path;
		}
		loadMaskFileDialog.Update();
		loadMaskFileDialog.PopupCentered();
	}

	public void LoadMask(string file)
	{
		if (file != "")
		{
			workspace.DrawingManager.StampTool.LoadMask(file);
			Settings.MasksPath = System.IO.Path.GetDirectoryName(file);
		}
	}

	public void LoadDecalFileDialogPressed()
	{
		string path = Settings.DecalsPath;
		if (!path.Empty())
		{
			loadDecalFileDialog.CurrentDir = path;
		}
		loadDecalFileDialog.Update();
		loadDecalFileDialog.PopupCentered();
	}

	public void LoadDecal(string file)
	{
		if (file != "")
		{
			workspace.DrawingManager.StampTool.LoadDecal(file);
			Settings.DecalsPath = System.IO.Path.GetDirectoryName(file);
		}
	}

	public void ToggleMaskRandomRotation(bool pressed)
	{
		maskRandomRotationCheckButton.Pressed = pressed;
	}

	public void MaskRandomRotationToggled(bool pressed)
	{
		workspace.DrawingManager.StampTool.DoRandomRotation = pressed;
	}

	public void ChangeMaskRotation(int value)
	{
		maskRotationSlider.SetValue((float)value * 90f);
	}

	public void MaskRotationChanged(float value)
	{
		workspace.DrawingManager.StampTool.Rotation = Mathf.FloorToInt(value / 90f);
	}

	public void ChangeMaskBlendingStrengt(float value)
	{
		maskRotationSlider.SetValue(value);
	}

	public void MaskBlendingStrengtChanged(float value)
	{
		workspace.DrawingManager.StampTool.BlendingStrength = maskBlendingStrengtSlider.GetValue();
	}

	public void ToggleHeightBlend(bool pressed)
	{
		heightBlendCheckButton.Pressed = pressed;
	}

	public void HeightBlendToggled(bool pressed)
	{
		workspace.DrawingManager.StampTool.DoHeightBlending = pressed;
	}

	public void ChangeHeightOffset(float value)
	{
		heightOffsetSlider.SetValue(value);
	}

	public void HeightOffsetChanged(float value)
	{
		workspace.DrawingManager.StampTool.HeightOffset = heightOffsetSlider.GetValue();
	}

	public void ChangeHeightBlendingFactor(float value)
	{
		heightBlendingFactorSlider.SetValue(value);
	}

	public void HeightBlendingFactorChanged(float value)
	{
		workspace.DrawingManager.StampTool.HeightBlendingFactor = value;
	}
}
