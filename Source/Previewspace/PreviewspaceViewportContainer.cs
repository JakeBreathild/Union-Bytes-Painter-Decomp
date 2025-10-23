using Godot;

public class PreviewspaceViewportContainer : ViewportContainer
{
	private Workspace workspace;

	private PreviewspaceViewport previewspaceViewport;

	private TextureButton wireframeTextureButton;

	private TextureButton illuminatedTextureButton;

	private TextureButton unlitTextureButton;

	private TextureButton channelTextureButton;

	private TextureButton posXTextureButton;

	private TextureButton negXTextureButton;

	private TextureButton posYTextureButton;

	private TextureButton negYTextureButton;

	private TextureButton posZTextureButton;

	private TextureButton negZTextureButton;

	private TextureButton resetCameraTextureButton;

	public PreviewspaceViewport PreviewspaceViewport => previewspaceViewport;

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		previewspaceViewport = GetChildOrNull<PreviewspaceViewport>(0);
		Register.PreviewspaceViewport = previewspaceViewport;
		Connect(Signals.MouseEntered, this, "MouseEntered");
		Connect(Signals.MouseExited, this, "MouseExited");
		Connect(Signals.Resized, this, "Resized");
		wireframeTextureButton = GetNodeOrNull<TextureButton>("DisplayPanel/WireframeTextureButton");
		wireframeTextureButton.Connect(Signals.Pressed, this, "WireframePressed");
		illuminatedTextureButton = GetNodeOrNull<TextureButton>("IlluminationPanel/IlluminatedTextureButton");
		illuminatedTextureButton.Connect(Signals.Pressed, this, "IlluminatedPressed");
		unlitTextureButton = GetNodeOrNull<TextureButton>("IlluminationPanel/UnlitTextureButton");
		unlitTextureButton.Connect(Signals.Pressed, this, "UnlitPressed");
		channelTextureButton = GetNodeOrNull<TextureButton>("IlluminationPanel/ChannelTextureButton");
		channelTextureButton.Connect(Signals.Pressed, this, "ChannelPressed");
		posXTextureButton = GetNodeOrNull<TextureButton>("CameraPanel/X+TextureButton");
		posXTextureButton.Connect(Signals.Pressed, this, "PosXPressed");
		negXTextureButton = GetNodeOrNull<TextureButton>("CameraPanel/X-TextureButton");
		negXTextureButton.Connect(Signals.Pressed, this, "NegXPressed");
		posYTextureButton = GetNodeOrNull<TextureButton>("CameraPanel/Y+TextureButton");
		posYTextureButton.Connect(Signals.Pressed, this, "PosYPressed");
		negYTextureButton = GetNodeOrNull<TextureButton>("CameraPanel/Y-TextureButton");
		negYTextureButton.Connect(Signals.Pressed, this, "NegYPressed");
		posZTextureButton = GetNodeOrNull<TextureButton>("CameraPanel/Z+TextureButton");
		posZTextureButton.Connect(Signals.Pressed, this, "PosZPressed");
		negZTextureButton = GetNodeOrNull<TextureButton>("CameraPanel/Z-TextureButton");
		negZTextureButton.Connect(Signals.Pressed, this, "NegZPressed");
		resetCameraTextureButton = GetNodeOrNull<TextureButton>("CameraPanel/ResetTextureButton");
		resetCameraTextureButton.Connect(Signals.Pressed, this, "ResetPreviewspaceCameraPressed");
	}

	public void Reset()
	{
		workspace.CameraManager.ResetPreviewspaceCamera();
		wireframeTextureButton.Pressed = Register.PreviewspaceMeshManager.WireframeVisible;
		illuminatedTextureButton.Pressed = true;
		unlitTextureButton.Pressed = false;
		channelTextureButton.Pressed = false;
	}

	public void Resized()
	{
		previewspaceViewport.Size = base.RectSize;
	}

	public void DisableWireframe(bool disabled)
	{
		wireframeTextureButton.Disabled = disabled;
	}

	public void MouseEntered()
	{
		InputManager.MouseEnteredUserInterface();
		workspace.CameraManager.EnablePreviewCameraControls(enable: true);
	}

	public void MouseExited()
	{
		workspace.CameraManager.EnablePreviewCameraControls(enable: false);
		InputManager.MouseExitedUserInterface();
	}

	public void PosXPressed()
	{
		workspace.CameraManager.SetPreviewspaceCamera(0f, 90f);
	}

	public void NegXPressed()
	{
		workspace.CameraManager.SetPreviewspaceCamera(0f, 270f);
	}

	public void PosYPressed()
	{
		workspace.CameraManager.SetPreviewspaceCamera(0f, 180f);
	}

	public void NegYPressed()
	{
		workspace.CameraManager.SetPreviewspaceCamera(0f, 0f);
	}

	public void PosZPressed()
	{
		workspace.CameraManager.SetPreviewspaceCamera(90f, 180f);
	}

	public void NegZPressed()
	{
		workspace.CameraManager.SetPreviewspaceCamera(-90f, 180f);
	}

	public void ResetPreviewspaceCameraPressed()
	{
		workspace.CameraManager.ResetPreviewspaceCamera();
	}

	public void IlluminatedPressed()
	{
		illuminatedTextureButton.Pressed = true;
		unlitTextureButton.Pressed = false;
		channelTextureButton.Pressed = false;
		Register.PreviewspaceMeshManager.Illumination = PreviewspaceMeshManager.IlluminationEnum.ILLUMINATED;
		Register.PreviewspaceMeshManager.ActivateFullShaderMaterial();
		MaterialManager.UpdateDrawingMaterials();
	}

	public void UnlitPressed()
	{
		illuminatedTextureButton.Pressed = false;
		unlitTextureButton.Pressed = true;
		channelTextureButton.Pressed = false;
		Register.PreviewspaceMeshManager.Illumination = PreviewspaceMeshManager.IlluminationEnum.UNLIT;
		Register.PreviewspaceMeshManager.ActivateChannelShaderMaterial();
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "channelTexture", workspace.Worksheet.Data.ColorChannel.ImageTexture);
		MaterialManager.UpdateDrawingMaterials();
	}

	public void ChannelPressed()
	{
		illuminatedTextureButton.Pressed = false;
		unlitTextureButton.Pressed = false;
		channelTextureButton.Pressed = true;
		Register.PreviewspaceMeshManager.Illumination = PreviewspaceMeshManager.IlluminationEnum.CHANNEL;
		Register.PreviewspaceMeshManager.ActivateChannelShaderMaterial();
		MaterialManager.UpdateChannelMaterials();
		MaterialManager.UpdateDrawingMaterials();
	}

	public void WireframePressed()
	{
		TextureButton textureButton = wireframeTextureButton;
		bool pressed = (Register.PreviewspaceMeshManager.WireframeVisible = !Register.PreviewspaceMeshManager.WireframeVisible);
		textureButton.Pressed = pressed;
	}
}
