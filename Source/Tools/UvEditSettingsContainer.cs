using Godot;

public class UvEditSettingsContainer : DefaultHBoxContainer
{
	private PreviewspaceMeshManager previewMeshManager;

	private TextureButton gridLockToolButton;

	private TextureButton selectionModeNodeToolButton;

	private TextureButton selectionModeEdgeToolButton;

	private TextureButton selectionModeIslandToolButton;

	public override void _Ready()
	{
		base._Ready();
		previewMeshManager = Register.PreviewspaceMeshManager;
		gridLockToolButton = GetNodeOrNull<TextureButton>("GridLock");
		gridLockToolButton.Connect(Signals.Toggled, this, "ActivateGridLock");
		selectionModeNodeToolButton = GetNodeOrNull<TextureButton>("SelectionModeNode");
		selectionModeNodeToolButton.Connect(Signals.Pressed, this, "ActivateNodeSelection");
		selectionModeEdgeToolButton = GetNodeOrNull<TextureButton>("SelectionModeEdge");
		selectionModeEdgeToolButton.Connect(Signals.Pressed, this, "ActivateEdgeSelection");
		selectionModeIslandToolButton = GetNodeOrNull<TextureButton>("SelectionModeIsland");
		selectionModeIslandToolButton.Connect(Signals.Pressed, this, "ActivateIslandSelection");
	}

	public override void Reset()
	{
		base.Reset();
		gridLockToolButton.Pressed = true;
		selectionModeNodeToolButton.Pressed = true;
		selectionModeEdgeToolButton.Pressed = false;
		selectionModeIslandToolButton.Pressed = false;
		base.Visible = false;
	}

	public void ActivateGridLock(bool activated)
	{
		previewMeshManager.ActivateUVsNodesGridLock(activated);
	}

	public void ActivateNodeSelection()
	{
		selectionModeNodeToolButton.Pressed = true;
		selectionModeEdgeToolButton.Pressed = false;
		selectionModeIslandToolButton.Pressed = false;
		previewMeshManager.ChangeUVsSelectionMode(PreviewspaceMeshManager.UvSelectionModeEnum.NODE);
	}

	public void ActivateEdgeSelection()
	{
		selectionModeNodeToolButton.Pressed = false;
		selectionModeEdgeToolButton.Pressed = true;
		selectionModeIslandToolButton.Pressed = false;
		previewMeshManager.ChangeUVsSelectionMode(PreviewspaceMeshManager.UvSelectionModeEnum.EDGE);
	}

	public void ActivateIslandSelection()
	{
		selectionModeNodeToolButton.Pressed = false;
		selectionModeEdgeToolButton.Pressed = false;
		selectionModeIslandToolButton.Pressed = true;
		previewMeshManager.ChangeUVsSelectionMode(PreviewspaceMeshManager.UvSelectionModeEnum.ISLAND);
	}
}
