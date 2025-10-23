using Godot;

public class DisplaySettingsContainer : DefaultHBoxContainer
{
	private bool gridInitialValue;

	private TextureButton gridToolButton;

	private bool maskInitialValue;

	private TextureButton maskToolButton;

	private bool mirroringInitialValue;

	private MirroringToolButton mirroringToolButton;

	private bool wireframeInitialValue;

	private TextureButton wireframeToolButton;

	private bool alignmentToolInitialValue;

	private TextureButton alignmentToolToolButton;

	private bool layerInitialValue;

	private TextureButton layerToolButton;

	public MirroringToolButton MirroringToolButton => mirroringToolButton;

	public TextureButton AlignmentToolToolButton => alignmentToolToolButton;

	public TextureButton LayerToolButton => layerToolButton;

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		gridToolButton = GetNodeOrNull<TextureButton>("Grid");
		gridInitialValue = gridToolButton.Pressed;
		gridToolButton.Connect(Signals.Toggled, this, "GridToggled");
		maskToolButton = GetNodeOrNull<TextureButton>("Mask");
		maskInitialValue = maskToolButton.Pressed;
		maskToolButton.Connect(Signals.Toggled, this, "MaskToggled");
		mirroringToolButton = GetNodeOrNull<MirroringToolButton>("Mirroring");
		mirroringInitialValue = mirroringToolButton.Pressed;
		mirroringToolButton.Connect(Signals.Toggled, this, "MirroringToggled");
		wireframeToolButton = GetNodeOrNull<TextureButton>("Wireframe");
		wireframeInitialValue = wireframeToolButton.Pressed;
		wireframeToolButton.Connect(Signals.Toggled, this, "WireframeToggled");
		alignmentToolToolButton = GetNodeOrNull<TextureButton>("AlignmentTool");
		alignmentToolInitialValue = alignmentToolToolButton.Pressed;
		alignmentToolToolButton.Connect(Signals.Toggled, this, "AlignmentToolToggled");
		layerToolButton = GetNodeOrNull<TextureButton>("Layer");
		layerInitialValue = layerToolButton.Pressed;
		layerToolButton.Connect(Signals.Toggled, this, "LayerToggled");
	}

	public override void Reset()
	{
		maskToolButton.Pressed = maskInitialValue;
		gridToolButton.Pressed = gridInitialValue;
		mirroringToolButton.Pressed = mirroringInitialValue;
		mirroringToolButton.Reset();
		wireframeToolButton.Pressed = wireframeInitialValue;
		wireframeToolButton.Disabled = true;
		alignmentToolToolButton.Pressed = alignmentToolInitialValue;
		layerToolButton.Pressed = layerInitialValue;
	}

	public void ToggleGrid(bool pressed)
	{
		gridToolButton.Pressed = pressed;
	}

	public void GridToggled(bool pressed)
	{
		workspace.ToggleGrid(pressed);
	}

	public void ToggleMask(bool pressed)
	{
		maskToolButton.Pressed = pressed;
	}

	public void MaskToggled(bool pressed)
	{
		workspace.ToggleMask(pressed);
	}

	public void ToggleMirroring(bool pressed)
	{
		mirroringToolButton.Pressed = pressed;
	}

	public void MirroringToggled(bool pressed)
	{
		workspace.ToggleMirroring(pressed);
	}

	public void DisableWireframe(bool disabled)
	{
		wireframeToolButton.Disabled = disabled;
	}

	public void ToggleWireframe(bool pressed)
	{
		wireframeToolButton.Pressed = pressed;
	}

	public void WireframeToggled(bool pressed)
	{
		workspace.ToggleWireframe(pressed);
		Register.Gui.BakeMenuButton.ToggleWireframe(pressed);
	}

	public void DisableAlignmentTool(bool disabled)
	{
		alignmentToolToolButton.Disabled = disabled;
	}

	public void ToggleAlignmentTool(bool pressed)
	{
		alignmentToolToolButton.Pressed = pressed;
	}

	public void AlignmentToolToggled(bool pressed)
	{
		Register.AlignmentTool.Visible = pressed;
	}

	public void DisableLayer(bool disabled)
	{
		layerToolButton.Disabled = disabled;
	}

	public void ToggleLayer(bool pressed)
	{
		layerToolButton.Pressed = pressed;
	}

	public void LayerToggled(bool pressed)
	{
		Register.GridManager.DoShowLayerContentAreas = pressed;
		Register.GridManager.Update(Register.Workspace.Worksheet);
	}

	public void UpdateMaskTooltip(string text, string hotkey)
	{
		maskToolButton.HintTooltip = text + " " + hotkey;
	}

	public void UpdateGridTooltip(string text, string hotkey)
	{
		gridToolButton.HintTooltip = text + " " + hotkey;
	}

	public void UpdateMirroringTooltip(string text, string hotkey)
	{
		mirroringToolButton.HintTooltip = text + " " + hotkey;
	}

	public void UpdateAlignmentToolTooltip(string text, string hotkey)
	{
		alignmentToolToolButton.HintTooltip = text + " " + hotkey;
	}

	public void UpdateWireframeTooltip(string text, string hotkey)
	{
		wireframeToolButton.HintTooltip = text + " " + hotkey;
	}

	public void UpdateLayerTooltip(string text, string hotkey)
	{
		layerToolButton.HintTooltip = text + " " + hotkey;
	}
}
