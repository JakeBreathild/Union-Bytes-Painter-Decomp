using System.IO;
using Godot;

public class FillingBucketSettingsContainer : DefaultHBoxContainer
{
	private ChannelButton channelButton;

	private Button maskModeButton;

	private Button materialModeButton;

	private Button decalModeButton;

	private Button loadMaskButton;

	[Export(PropertyHint.None, "")]
	private NodePath loadMaskFileDialogNodePath;

	private PreviewFileDialog loadMaskFileDialog;

	private Button loadDecalButton;

	[Export(PropertyHint.None, "")]
	private NodePath loadDecalFileDialogNodePath;

	private PreviewFileDialog loadDecalFileDialog;

	private float fillingBucketToleranceInitialValue;

	private ToolSlider fillingBucketToleranceSlider;

	private float heightOffsetInitialValue;

	private ToolSlider heightOffsetSlider;

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
			maskModeButton.Disabled = disabled;
			materialModeButton.Disabled = disabled;
			decalModeButton.Disabled = disabled;
			loadMaskButton.Disabled = disabled;
			loadDecalButton.Disabled = disabled;
			heightOffsetSlider.Disabled = disabled;
			fillingBucketToleranceSlider.Disabled = disabled;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		maskModeButton = GetNodeOrNull<Button>("Mode/HC/Mask");
		maskModeButton.Connect(Signals.Pressed, this, "MaskModeButtonPressed");
		materialModeButton = GetNodeOrNull<Button>("Mode/HC/Material");
		materialModeButton.Connect(Signals.Pressed, this, "MaterialModeButtonPressed");
		decalModeButton = GetNodeOrNull<Button>("Mode/HC/Decal");
		decalModeButton.Connect(Signals.Pressed, this, "DecalModeButtonPressed");
		channelButton = GetNodeOrNull<ChannelButton>("Channel/ChannelButton");
		channelButton.ChannelSelectedCallback = ChannelChanged;
		fillingBucketToleranceSlider = GetNodeOrNull<ToolSlider>("Tolerance");
		fillingBucketToleranceInitialValue = fillingBucketToleranceSlider.GetValue();
		fillingBucketToleranceSlider.ValueChangedCallback = FillingBucketToleranceChanged;
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
		heightOffsetSlider = GetNodeOrNull<ToolSlider>("HeightOffset");
		heightOffsetInitialValue = heightOffsetSlider.GetValue();
		heightOffsetSlider.ValueChangedCallback = HeightOffsetChanged;
	}

	public override void Reset()
	{
		base.Reset();
		maskModeButton.Pressed = false;
		materialModeButton.Pressed = true;
		decalModeButton.Pressed = false;
		channelButton.SetChannel(Data.ChannelEnum.COLOR);
		fillingBucketToleranceSlider.SetValue(fillingBucketToleranceInitialValue);
		heightOffsetSlider.SetValue(heightOffsetInitialValue);
		base.Visible = false;
	}

	public void ChangeChannel(Data.ChannelEnum channel)
	{
		channelButton.SetChannel(channel);
	}

	public void ChannelChanged(Data.ChannelEnum channel)
	{
		workspace.DrawingManager.BucketTool.Channel = channel;
	}

	public void ChangeFillingBucketTolerance(float value)
	{
		fillingBucketToleranceSlider.SetValue(value);
	}

	public void FillingBucketToleranceChanged(float value)
	{
		workspace.DrawingManager.BucketTool.Tolerance = fillingBucketToleranceSlider.GetValue();
	}

	public void ChangeHeightOffset(float value)
	{
		heightOffsetSlider.SetValue(value);
	}

	public void HeightOffsetChanged(float value)
	{
		workspace.DrawingManager.BucketTool.HeightOffset = value;
	}

	public void ChangeModeButtons(DrawingToolBucket.ModeEnum mode)
	{
		switch (mode)
		{
		case DrawingToolBucket.ModeEnum.MASK:
			maskModeButton.Pressed = true;
			materialModeButton.Pressed = false;
			decalModeButton.Pressed = false;
			break;
		case DrawingToolBucket.ModeEnum.MATERIAL:
			maskModeButton.Pressed = false;
			materialModeButton.Pressed = true;
			decalModeButton.Pressed = false;
			break;
		case DrawingToolBucket.ModeEnum.DECAL:
			maskModeButton.Pressed = false;
			materialModeButton.Pressed = false;
			decalModeButton.Pressed = true;
			break;
		}
	}

	public void MaskModeButtonPressed()
	{
		if (Register.DrawingManager.BucketTool.Mask != null)
		{
			maskModeButton.Pressed = true;
			materialModeButton.Pressed = false;
			decalModeButton.Pressed = false;
			Register.DrawingManager.BucketTool.Mode = DrawingToolBucket.ModeEnum.MASK;
			ImageTexture texture = new ImageTexture();
			texture.CreateFromImage(Register.DrawingManager.BucketTool.Mask.Thumbnail, 0u);
			Register.MaterialContainer.ChangeDecalLabel("Mask");
			Register.MaterialContainer.ChangeDecalPreview(texture);
		}
		else
		{
			maskModeButton.Pressed = false;
			materialModeButton.Pressed = true;
			decalModeButton.Pressed = false;
			LoadMaskFileDialogPressed();
		}
	}

	public void MaterialModeButtonPressed()
	{
		maskModeButton.Pressed = false;
		materialModeButton.Pressed = true;
		decalModeButton.Pressed = false;
		Register.DrawingManager.BucketTool.Mode = DrawingToolBucket.ModeEnum.MATERIAL;
		Register.MaterialContainer.ChangeDecalLabel("Preview");
		Register.MaterialContainer.ChangeDecalPreview((Texture)null);
	}

	public void DecalModeButtonPressed()
	{
		if (Register.DrawingManager.BucketTool.Decal != null)
		{
			maskModeButton.Pressed = false;
			materialModeButton.Pressed = false;
			decalModeButton.Pressed = true;
			Register.DrawingManager.BucketTool.Mode = DrawingToolBucket.ModeEnum.DECAL;
			ImageTexture texture = new ImageTexture();
			texture.CreateFromImage(Register.DrawingManager.BucketTool.Decal.Thumbnail, 0u);
			Register.MaterialContainer.ChangeDecalLabel("Decal");
			Register.MaterialContainer.ChangeDecalPreview(texture);
		}
		else
		{
			maskModeButton.Pressed = false;
			materialModeButton.Pressed = true;
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

	public void LoadMask(string file)
	{
		if (file != "")
		{
			workspace.DrawingManager.BucketTool.LoadMask(file);
			Settings.MasksPath = System.IO.Path.GetDirectoryName(file);
		}
	}

	public void LoadDecal(string file)
	{
		if (file != "")
		{
			workspace.DrawingManager.BucketTool.LoadDecal(file);
			Settings.DecalsPath = System.IO.Path.GetDirectoryName(file);
		}
	}
}
