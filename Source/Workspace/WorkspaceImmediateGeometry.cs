using Godot;

public class WorkspaceImmediateGeometry : ImmediateGeometry
{
	private Worksheet worksheet;

	private Label label;

	public Label Label => label;

	public WorkspaceImmediateGeometry()
	{
		Register.WorkspaceImmediateGeometry = this;
	}

	public WorkspaceImmediateGeometry(Workspace workspace)
	{
		Register.WorkspaceImmediateGeometry = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "WorkspaceImmediateGeometry";
	}

	public override void _Ready()
	{
		base._Ready();
		base.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.GRID);
		label = new Label();
		Register.CameraManager.WorkspaceViewport.CallDeferred("add_child", label);
		label.CallDeferred("set_owner", Register.CameraManager.WorkspaceViewport);
		label.AddColorOverride("font_color", Settings.AlignmentColor);
		label.Text = "TESTTTTTTTTT!!!!!!!!!!";
	}

	public void Update(Worksheet worksheet)
	{
		this.worksheet = worksheet;
		Clear();
		Begin(Mesh.PrimitiveType.Lines);
		SetColor(Settings.GridColor);
		if (worksheet.Data.Tileable)
		{
			new Vector3((float)(-worksheet.Data.Width) * 0.25f, Settings.GridOffset * 1.5f, (float)(-worksheet.Data.Height) * 0.25f);
		}
		else
		{
			new Vector3(0f, Settings.GridOffset * 1.5f, 0f);
		}
		End();
	}
}
