using Godot;

public class ViewportSplitContainer : HSplitContainer
{
	[Export(PropertyHint.None, "")]
	protected NodePath guiNodePath;

	protected Gui gui;

	[Export(PropertyHint.None, "")]
	private NodePath workspaceViewportNodePath;

	private Viewport workspaceViewport;

	private ViewportContainer workspaceViewportContainer;

	private PreviewspaceViewportContainer previewViewportContainer;

	private float splitOffsetFactor = 0.5f;

	public override void _Ready()
	{
		base._Ready();
		gui = GetNodeOrNull<Gui>(guiNodePath);
		workspaceViewport = GetNodeOrNull<Viewport>(workspaceViewportNodePath);
		workspaceViewportContainer = GetNodeOrNull<ViewportContainer>("WorkspaceViewportContainer");
		previewViewportContainer = GetNodeOrNull<PreviewspaceViewportContainer>("PreviewViewportContainer");
		base.SplitOffset = Mathf.CeilToInt(base.RectSize.x * splitOffsetFactor);
		Vector2 size = base.RectSize;
		size.x = base.SplitOffset;
		workspaceViewport.Size = size;
		workspaceViewportContainer.RectSize = size;
		size.x = base.RectSize.x - (float)base.SplitOffset;
		previewViewportContainer.RectSize = size;
		Connect(Signals.Resized, this, "Resized");
		Connect(Signals.Dragged, this, "Dragged");
		Connect(Signals.MouseEntered, this, "MouseEntered");
		Connect(Signals.MouseExited, this, "MouseExited");
	}

	public void MouseEntered()
	{
		InputManager.MouseEnteredUserInterface();
	}

	public void MouseExited()
	{
		InputManager.MouseExitedUserInterface();
	}

	public void Resized()
	{
		base.SplitOffset = Mathf.CeilToInt(base.RectSize.x * splitOffsetFactor);
		Vector2 size = base.RectSize;
		size.x = base.RectSize.x - (float)base.SplitOffset;
		workspaceViewport.Size = size;
		workspaceViewportContainer.RectSize = size;
		size.x = base.SplitOffset;
		previewViewportContainer.PreviewspaceViewport.Size = size;
		previewViewportContainer.RectSize = size;
	}

	public void Dragged(int offset)
	{
		splitOffsetFactor = Mathf.Clamp((float)offset / base.RectSize.x, 0.25f, 0.75f);
		base.SplitOffset = Mathf.CeilToInt(base.RectSize.x * splitOffsetFactor);
		Vector2 size = base.RectSize;
		size.x = base.RectSize.x - (float)base.SplitOffset;
		workspaceViewport.Size = size;
		workspaceViewportContainer.RectSize = size;
		size.x = base.SplitOffset;
		previewViewportContainer.PreviewspaceViewport.Size = size;
		previewViewportContainer.RectSize = size;
	}
}
