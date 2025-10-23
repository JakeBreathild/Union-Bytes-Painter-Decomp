using Godot;

public class WorkspaceChecker : MeshInstance
{
	public WorkspaceChecker()
	{
		Register.Checker = this;
	}

	public WorkspaceChecker(Workspace workspace)
	{
		Register.Checker = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "Checker";
	}

	public override void _Ready()
	{
		base._Ready();
		Transform transform = base.GlobalTransform;
		transform.origin.y -= Settings.CheckerOffset;
		base.GlobalTransform = transform;
		base.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.CHECKER);
	}

	public new Mesh GetMesh()
	{
		return base.Mesh;
	}

	public new void SetMesh(Mesh mesh)
	{
		base.Mesh = mesh;
	}
}
