using Godot;
using Godot.Collections;

public class WorkspaceMeshManager : Node
{
	public const float MESH_PIXEL_SIZE = 0.25f;

	private Workspace workspace;

	private Worksheet worksheet;

	private ArrayMesh mesh;

	private MeshInstance meshInstance;

	public Mesh Mesh => mesh;

	public WorkspaceMeshManager()
	{
		Register.WorkspaceMeshManager = this;
	}

	public WorkspaceMeshManager(Workspace workspace)
	{
		Register.WorkspaceMeshManager = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "WorkspaceMeshManager";
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		meshInstance = new MeshInstance();
		AddChild(meshInstance);
		meshInstance.Owner = this;
		meshInstance.Name = "WorkspaceMesh";
		meshInstance.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.WORKSPACE);
	}

	private ArrayMesh CreateMesh(float scale = 1f)
	{
		int Indices = 6;
		Vector3[] verticesArray = new Vector3[4];
		Vector3[] normalsArray = new Vector3[4];
		Vector2[] uvsArray = new Vector2[4];
		int[] indicesArray = new int[Indices];
		if (worksheet.Data.Tileable)
		{
			verticesArray[0] = new Vector3((float)(-worksheet.Data.Width) * 0.25f * scale, 0f, (float)(-worksheet.Data.Height) * 0.25f * scale);
			normalsArray[0] = Vector3.Up;
			uvsArray[0] = new Vector2(0f, 0f);
			verticesArray[1] = new Vector3(2f * (float)worksheet.Data.Width * 0.25f * scale, 0f, (float)(-worksheet.Data.Height) * 0.25f * scale);
			normalsArray[1] = Vector3.Up;
			uvsArray[1] = new Vector2(3f, 0f);
			verticesArray[2] = new Vector3(2f * (float)worksheet.Data.Width * 0.25f * scale, 0f, 2f * (float)worksheet.Data.Height * 0.25f * scale);
			normalsArray[2] = Vector3.Up;
			uvsArray[2] = new Vector2(3f, 3f);
			verticesArray[3] = new Vector3((float)(-worksheet.Data.Width) * 0.25f * scale, 0f, 2f * (float)worksheet.Data.Height * 0.25f * scale);
			normalsArray[3] = Vector3.Up;
			uvsArray[3] = new Vector2(0f, 3f);
		}
		else
		{
			verticesArray[0] = new Vector3(0f, 0f, 0f);
			normalsArray[0] = Vector3.Up;
			uvsArray[0] = new Vector2(0f, 0f);
			verticesArray[1] = new Vector3((float)worksheet.Data.Width * 0.25f * scale, 0f, 0f);
			normalsArray[1] = Vector3.Up;
			uvsArray[1] = new Vector2(1f, 0f);
			verticesArray[2] = new Vector3((float)worksheet.Data.Width * 0.25f * scale, 0f, (float)worksheet.Data.Height * 0.25f * scale);
			normalsArray[2] = Vector3.Up;
			uvsArray[2] = new Vector2(1f, 1f);
			verticesArray[3] = new Vector3(0f, 0f, (float)worksheet.Data.Height * 0.25f * scale);
			normalsArray[3] = Vector3.Up;
			uvsArray[3] = new Vector2(0f, 1f);
		}
		indicesArray[0] = 0;
		indicesArray[1] = 1;
		indicesArray[2] = 3;
		indicesArray[3] = 1;
		indicesArray[4] = 2;
		indicesArray[5] = 3;
		Array godotMeshArrayData = new Array();
		godotMeshArrayData.Resize(9);
		godotMeshArrayData[0] = verticesArray;
		godotMeshArrayData[1] = normalsArray;
		godotMeshArrayData[4] = uvsArray;
		godotMeshArrayData[8] = indicesArray;
		ArrayMesh arrayMesh = new ArrayMesh();
		arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, godotMeshArrayData, null, 97280u);
		arrayMesh.RegenNormalmaps();
		return arrayMesh;
	}

	public void UpdateMesh(Worksheet worksheet)
	{
		this.worksheet = worksheet;
		meshInstance.Mesh = (mesh = CreateMesh());
	}

	public void SetMeshMaterial(ShaderMaterial shaderMaterial)
	{
		meshInstance.MaterialOverride = shaderMaterial;
	}

	public ShaderMaterial GetMeshMaterial()
	{
		return (ShaderMaterial)meshInstance.MaterialOverride;
	}
}
