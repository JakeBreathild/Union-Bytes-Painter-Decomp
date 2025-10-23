using Godot;

public class ProjectsSettingsWindowDialog : WindowDialog
{
	private Gui gui;

	private Workspace workspace;

	private LineEdit worksheetNameLineEdit;

	private LineEdit widthLineEdit;

	private LineEdit heightLineEdit;

	private CheckButton tileableCheckButton;

	private Button removeMeshButton;

	private DefaultSlider meshScaleDefaultSlider;

	private DefaultSlider uvScaleDefaultSlider;

	private OptionButton ShaderOptionButton;

	private DefaultSlider NormalStrengthDefaultSlider;

	private OptionButton NormalModeOptionButton;

	private TextureRect previewTextureRect;

	private ChannelArray<float> previewHeightChannelArray;

	private ChannelArray<Color> previewNormalChannelArray;

	private Image previewImage;

	private ImageTexture previewImageTexture;

	public override void _Ready()
	{
		base._Ready();
		gui = Register.Gui;
		workspace = Register.Workspace;
		string nodeGroupPath = "SC/VC/Worksheet/";
		worksheetNameLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Name");
		worksheetNameLineEdit.Connect(Signals.TextEntered, this, "WorksheetNameLineEditReleaseFocus");
		widthLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Width");
		heightLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Height");
		tileableCheckButton = GetNodeOrNull<CheckButton>(nodeGroupPath + "Tileable");
		nodeGroupPath = "SC/VC/Mesh/";
		removeMeshButton = GetNodeOrNull<Button>(nodeGroupPath + "Remove");
		meshScaleDefaultSlider = GetNodeOrNull<DefaultSlider>(nodeGroupPath + "MeshScale");
		uvScaleDefaultSlider = GetNodeOrNull<DefaultSlider>(nodeGroupPath + "UVScale");
		nodeGroupPath = "SC/VC/Shader/";
		ShaderOptionButton = GetNodeOrNull<OptionButton>(nodeGroupPath + "Shader");
		ShaderOptionButton.AddItem("Default");
		ShaderOptionButton.AddItem("Two Sides");
		ShaderOptionButton.AddItem("Emission As Transmission");
		ShaderOptionButton.Selected = 0;
		nodeGroupPath = "SC/VC/NormalMap/";
		NormalModeOptionButton = GetNodeOrNull<OptionButton>(nodeGroupPath + "Mode");
		NormalModeOptionButton.AddItem("Sobel");
		NormalModeOptionButton.AddItem("Prewitt");
		NormalModeOptionButton.AddItem("Cross");
		NormalModeOptionButton.AddItem("Godot");
		NormalModeOptionButton.Selected = (int)Channel.NormalMode;
		NormalModeOptionButton.Connect(Signals.ItemSelected, this, "NormalModeSelected");
		NormalStrengthDefaultSlider = GetNodeOrNull<DefaultSlider>(nodeGroupPath + "Strength");
		NormalStrengthDefaultSlider.Connect(Signals.ValueChanged, this, "NormalStrengthChanged");
		previewTextureRect = GetNodeOrNull<TextureRect>(nodeGroupPath + "Preview");
		previewHeightChannelArray = new ChannelArray<float>(7, 7, 0f);
		previewNormalChannelArray = new ChannelArray<Color>(previewHeightChannelArray.Width, previewHeightChannelArray.Height, new Color(0f, 0f, 0f));
		previewImage = new Image();
		previewImageTexture = new ImageTexture();
		for (int y = 2; y < 5; y++)
		{
			for (int x = 2; x < 5; x++)
			{
				previewHeightChannelArray[x, y] = 1f;
			}
		}
		UpdateNormalPreview();
		previewTextureRect.Texture = previewImageTexture;
		Connect(Signals.Hide, this, "Hide");
	}

	public void Reset()
	{
		GetNodeOrNull<ScrollContainer>("SC").ScrollVertical = 0;
		worksheetNameLineEdit.Text = workspace.Worksheet.SheetName;
		widthLineEdit.Text = workspace.Worksheet.Data.Width.ToString();
		heightLineEdit.Text = workspace.Worksheet.Data.Height.ToString();
		tileableCheckButton.Pressed = workspace.Worksheet.Data.Tileable;
		removeMeshButton.Pressed = false;
		if (Register.PreviewspaceMeshManager.IsMeshLoaded)
		{
			removeMeshButton.Disabled = false;
			meshScaleDefaultSlider.Disabled = false;
			meshScaleDefaultSlider.SetValue(Register.PreviewspaceMeshManager.GetMeshScale());
			uvScaleDefaultSlider.Disabled = false;
			uvScaleDefaultSlider.SetValue(Register.PreviewspaceMeshManager.GetUvScale());
		}
		else
		{
			removeMeshButton.Disabled = true;
			meshScaleDefaultSlider.Disabled = true;
			uvScaleDefaultSlider.Disabled = true;
		}
		ShaderOptionButton.Selected = (int)Register.PreviewspaceMeshManager.Shader;
		NormalStrengthDefaultSlider.SetValue(Channel.NormalStrength);
		NormalModeOptionButton.Selected = (int)Channel.NormalMode;
		UpdateNormalPreview();
	}

	public new void PopupCentered(Vector2? size = null)
	{
		InputManager.WindowShown();
		Reset();
		base.PopupCentered(size);
	}

	public new void Show()
	{
		InputManager.WindowShown();
		Reset();
		base.Show();
	}

	public new void Hide()
	{
		base.Hide();
		InputManager.WindowHidden();
		InputManager.SkipInput = true;
	}

	public void WorksheetNameLineEditReleaseFocus(string name)
	{
		worksheetNameLineEdit.ReleaseFocus();
	}

	public void NormalModeSelected(int index)
	{
		UpdateNormalPreview();
	}

	public void NormalStrengthChanged(float value)
	{
		UpdateNormalPreview();
	}

	public void UpdateNormalPreview()
	{
		Channel.NormalModeEnum normalMode = Channel.NormalMode;
		float normalStrength = Channel.NormalStrength;
		Channel.NormalMode = (Channel.NormalModeEnum)NormalModeOptionButton.Selected;
		Channel.NormalStrength = NormalStrengthDefaultSlider.GetValue();
		previewNormalChannelArray.Array = Channel.ConvertHeightToNormal(previewHeightChannelArray.Array, previewHeightChannelArray.Width, previewHeightChannelArray.Height);
		previewImage.Create(previewHeightChannelArray.Width, previewHeightChannelArray.Height, useMipmaps: false, Image.Format.Rgb8);
		previewImage.Lock();
		for (int y = 0; y < previewHeightChannelArray.Height; y++)
		{
			for (int x = 0; x < previewHeightChannelArray.Width; x++)
			{
				previewImage.SetPixel(x, y, previewNormalChannelArray[x, y]);
			}
		}
		previewImage.Unlock();
		previewImageTexture.CreateFromImage(previewImage, 0u);
		Channel.NormalMode = normalMode;
		Channel.NormalStrength = normalStrength;
	}

	public void Apply()
	{
		workspace.Worksheet.SheetName = worksheetNameLineEdit.Text.Trim();
		gui.UpdateWorksheetName();
		bool updateNormalMap = false;
		if (workspace.Worksheet.Data.Tileable != tileableCheckButton.Pressed)
		{
			workspace.Worksheet.Data.Tileable = tileableCheckButton.Pressed;
			workspace.UpdateMesh();
			workspace.UpdateGrid();
			workspace.UpdateShaders();
			if (!Register.PreviewspaceMeshManager.IsMeshLoaded)
			{
				Register.PreviewspaceMeshManager.ChangeShader(workspace.Worksheet.Data, PreviewspaceMeshManager.ShaderEnum.DEFAULT);
				Register.PreviewspaceMeshManager.Reset();
				Register.CollisionManager.Update();
				workspace.CameraManager.ResetPreviewspaceCamera();
				workspace.BakeManager.Update(workspace.Worksheet);
			}
		}
		if (Register.PreviewspaceMeshManager.Shader != (PreviewspaceMeshManager.ShaderEnum)ShaderOptionButton.Selected)
		{
			Register.PreviewspaceMeshManager.ChangeShader(workspace.Worksheet.Data, (PreviewspaceMeshManager.ShaderEnum)ShaderOptionButton.Selected);
		}
		if (removeMeshButton.Pressed)
		{
			Register.PreviewspaceMeshManager.Reset();
			Register.CollisionManager.Update();
			workspace.CameraManager.ResetPreviewspaceCamera();
			workspace.BakeManager.Update(workspace.Worksheet);
			gui.DisableBakeing(disabled: true);
			gui.DisplaySettingsContainer.DisableWireframe(disabled: true);
		}
		if (Register.PreviewspaceMeshManager.GetMeshScale() != meshScaleDefaultSlider.GetValue())
		{
			Register.PreviewspaceMeshManager.SetMeshScale(meshScaleDefaultSlider.GetValue());
			Register.CollisionManager.Update();
		}
		if (Register.PreviewspaceMeshManager.GetUvScale() != uvScaleDefaultSlider.GetValue())
		{
			Register.PreviewspaceMeshManager.SetUvScale(uvScaleDefaultSlider.GetValue());
		}
		if (Channel.NormalMode != (Channel.NormalModeEnum)NormalModeOptionButton.Selected)
		{
			Channel.NormalMode = (Channel.NormalModeEnum)NormalModeOptionButton.Selected;
			updateNormalMap = true;
		}
		if (Channel.NormalStrength != NormalStrengthDefaultSlider.GetValue())
		{
			Channel.NormalStrength = NormalStrengthDefaultSlider.GetValue();
			updateNormalMap = true;
		}
		if (updateNormalMap)
		{
			workspace.Worksheet.Data.NormalChannel.CreateTexture();
		}
	}

	public void OK()
	{
		Apply();
		Hide();
	}
}
