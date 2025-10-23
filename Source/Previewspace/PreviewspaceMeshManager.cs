using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Godot;
using Godot.Collections;

public class PreviewspaceMeshManager : Node
{
	public enum ShaderEnum
	{
		DEFAULT,
		TWOSIDES,
		TRANSMISSION
	}

	public enum IlluminationEnum
	{
		ILLUMINATED,
		UNLIT,
		CHANNEL
	}

	public enum UvSelectionModeEnum
	{
		NODE,
		EDGE,
		ISLAND
	}

	public struct Triangle
	{
		public int[] Indices;

		public Vector3 Normal;

		public Vector3 Tangent;

		public Triangle(int index1, int index2, int index3, Vector3 normal, Vector3 tangent)
		{
			Indices = new int[3] { index1, index2, index3 };
			Normal = normal;
			Tangent = tangent;
		}
	}

	public class UvNode
	{
		private List<Vector2> uvsList;

		public int Group = -1;

		public int ListIndex = -1;

		public int InstanceIndex = -1;

		public Vector2 Position = -Vector2.One;

		public List<int> IndicesList;

		public List<UvNode> Neighbours = new List<UvNode>();

		public UvNode(List<Vector2> uvsList, List<int> indicesList)
		{
			this.uvsList = uvsList;
			IndicesList = indicesList;
		}

		public void ChangePosition(Vector2 position)
		{
			Position = position;
			for (int i = 0; i < IndicesList.Count; i++)
			{
				uvsList[IndicesList[i]] = Position;
			}
		}

		public void AddNeighbour(UvNode node)
		{
			if (!Neighbours.Contains(node))
			{
				Neighbours.Add(node);
			}
		}
	}

	private Workspace workspace;

	private PreviewspaceViewport previewspaceViewport;

	protected string controls = "";

	private List<Vector3> verticesList;

	private List<Vector3> normalsList;

	private List<Vector2> uvsList;

	private List<int> indicesList;

	private List<int> groupList;

	private AABB aabb;

	private List<int> trianglesList;

	private List<int> quadranglesList;

	private List<int[]> polygonsList;

	private int trianglesCount;

	private List<Vector3> collisionVerticesList;

	private List<Triangle> collisionTrianglesList;

	private AABB collisionAABB;

	private float meshScale = 1f;

	private bool isMeshLoaded;

	private string meshFile = "";

	private ArrayMesh mesh;

	private MeshInstance meshInstance;

	private ImmediateGeometry wireframeMesh;

	private ShaderEnum shader;

	private IlluminationEnum illumination;

	private float uvScale = 1f;

	private bool isEditingActivated;

	private ImmediateGeometry uvLayoutImmediateGeometry;

	private List<UvNode> uvNodesList;

	private MultiMeshInstance uvNodesMultiMeshInstance;

	private UvSelectionModeEnum uvSelectionMode;

	private bool doMoveUvNodes;

	private bool doUvNodesGridLock = true;

	private HashSet<int> selectedUvNodesHashSet = new HashSet<int>();

	private List<Vector2> selectedUvNodesOffsetsList = new List<Vector2>();

	private UvNodeCommand uvNodeCommand;

	public string Controls
	{
		get
		{
			return controls;
		}
		set
		{
			controls = value;
		}
	}

	public List<Vector3> VerticesList => verticesList;

	public List<Vector3> NormalsList => normalsList;

	public List<Vector2> UVsList => uvsList;

	public List<int> IndicesList => indicesList;

	public List<int> GroupList => groupList;

	public AABB AABB => aabb;

	public List<int> TrianglesList => trianglesList;

	public List<int> QuadranglesList => quadranglesList;

	public List<int[]> PolygonsList => polygonsList;

	public int TrianglesCount => trianglesCount;

	public List<Vector3> CollisionVerticesList => collisionVerticesList;

	public List<Triangle> CollisionTrianglesList => collisionTrianglesList;

	public AABB CollisionAABB => collisionAABB;

	public bool IsMeshLoaded => isMeshLoaded;

	public string MeshFile => meshFile;

	public Mesh Mesh => mesh;

	public bool WireframeVisible
	{
		get
		{
			return wireframeMesh.Visible;
		}
		set
		{
			wireframeMesh.Visible = value;
		}
	}

	public ShaderEnum Shader => shader;

	public IlluminationEnum Illumination
	{
		get
		{
			return illumination;
		}
		set
		{
			illumination = value;
		}
	}

	public bool IsEditingActivated => isEditingActivated;

	public bool UvLayoutVisible
	{
		get
		{
			return uvLayoutImmediateGeometry.Visible;
		}
		set
		{
			uvLayoutImmediateGeometry.Visible = value;
		}
	}

	public List<UvNode> UvNodesList => uvNodesList;

	public MultiMesh UvNodesMultiMesh => uvNodesMultiMeshInstance.Multimesh;

	public PreviewspaceMeshManager()
	{
		Register.PreviewspaceMeshManager = this;
	}

	public PreviewspaceMeshManager(Workspace workspace)
	{
		Register.PreviewspaceMeshManager = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "PreviewMeshManager";
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		previewspaceViewport = Register.PreviewspaceViewport;
		verticesList = new List<Vector3>();
		normalsList = new List<Vector3>();
		uvsList = new List<Vector2>();
		indicesList = new List<int>();
		groupList = new List<int>();
		trianglesList = new List<int>();
		quadranglesList = new List<int>();
		polygonsList = new List<int[]>();
		collisionVerticesList = new List<Vector3>();
		collisionTrianglesList = new List<Triangle>();
		meshInstance = new MeshInstance();
		previewspaceViewport.AddChild(meshInstance);
		meshInstance.Owner = previewspaceViewport;
		meshInstance.Name = "PreviewspaceMesh";
		meshInstance.SetLayerMaskBit(0, enabled: false);
		meshInstance.SetLayerMaskBit(19, enabled: true);
		meshInstance.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW);
		wireframeMesh = new ImmediateGeometry();
		previewspaceViewport.AddChild(wireframeMesh);
		wireframeMesh.Owner = previewspaceViewport;
		wireframeMesh.Name = "WireframeMesh";
		wireframeMesh.SetLayerMaskBit(0, enabled: false);
		wireframeMesh.SetLayerMaskBit(19, enabled: true);
		wireframeMesh.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.GRID);
		uvLayoutImmediateGeometry = new ImmediateGeometry();
		AddChild(uvLayoutImmediateGeometry);
		uvLayoutImmediateGeometry.Owner = this;
		uvLayoutImmediateGeometry.Name = "UvLayout";
		Transform transform = uvLayoutImmediateGeometry.GlobalTransform;
		transform.origin.y += Settings.UvLayoutOffset;
		uvLayoutImmediateGeometry.GlobalTransform = transform;
		uvLayoutImmediateGeometry.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.GRID);
		uvNodesList = new List<UvNode>();
		uvNodesMultiMeshInstance = new MultiMeshInstance();
		workspace.AddChild(uvNodesMultiMeshInstance);
		uvNodesMultiMeshInstance.Owner = workspace;
		uvNodesMultiMeshInstance.Name = "UvNodes";
		transform = uvNodesMultiMeshInstance.GlobalTransform;
		transform.origin.y += Settings.UvNodesOffset;
		uvNodesMultiMeshInstance.GlobalTransform = transform;
		uvNodesMultiMeshInstance.Multimesh = new MultiMesh();
		PlaneMesh mesh = new PlaneMesh();
		mesh.Size = new Vector2(0.1875f, 0.1875f);
		uvNodesMultiMeshInstance.Multimesh.Mesh = mesh;
		uvNodesMultiMeshInstance.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.NODE);
		uvNodesMultiMeshInstance.Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
		uvNodesMultiMeshInstance.Multimesh.ColorFormat = MultiMesh.ColorFormatEnum.Color8bit;
		uvNodesMultiMeshInstance.Multimesh.InstanceCount = 0;
		uvNodesMultiMeshInstance.Multimesh.VisibleInstanceCount = 0;
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (!isEditingActivated || !Register.CameraManager.IsWorkspaceCameraControlsEnable())
		{
			return;
		}
		int width = workspace.Worksheet.Data.Width;
		int height = workspace.Worksheet.Data.Height;
		Vector2 pixelSize = new Vector2(1f / (float)width, 1f / (float)height);
		float maxNodeDistance = (pixelSize.x + pixelSize.y) * 0.5f;
		maxNodeDistance *= maxNodeDistance;
		maxNodeDistance *= 0.6f;
		float maxEdgeDistance = (pixelSize.x + pixelSize.y) * 0.25f;
		Vector2 collisionUVPosition = InputManager.CursorCollision.UV;
		if (Input.IsActionJustReleased("uv_node_select") && !doMoveUvNodes)
		{
			if (!Input.IsKeyPressed(16777237))
			{
				foreach (int i in selectedUvNodesHashSet)
				{
					DeselectUvNode(uvNodesList[i], removeFromList: false);
				}
				selectedUvNodesHashSet.Clear();
			}
			switch (uvSelectionMode)
			{
			case UvSelectionModeEnum.NODE:
			{
				for (int k = 0; k < uvNodesList.Count; k++)
				{
					if (collisionUVPosition.DistanceSquaredTo(uvNodesList[k].Position) < maxNodeDistance)
					{
						if (selectedUvNodesHashSet.Contains(k))
						{
							DeselectUvNode(uvNodesList[k]);
						}
						else
						{
							SelectUvNode(uvNodesList[k]);
						}
						break;
					}
				}
				break;
			}
			case UvSelectionModeEnum.EDGE:
			{
				bool breakSecondLoop = false;
				for (int l = 0; l < uvNodesList.Count; l++)
				{
					for (int ii = 0; ii < uvNodesList[l].Neighbours.Count; ii++)
					{
						if (!IsPositionOnUvEdge(collisionUVPosition, uvNodesList[l], uvNodesList[l].Neighbours[ii], maxEdgeDistance))
						{
							continue;
						}
						if (selectedUvNodesHashSet.Contains(uvNodesList[l].ListIndex) && selectedUvNodesHashSet.Contains(uvNodesList[l].Neighbours[ii].ListIndex))
						{
							DeselectUvNode(uvNodesList[l]);
							DeselectUvNode(uvNodesList[l].Neighbours[ii]);
							if (Input.IsKeyPressed(16777240))
							{
								Vector2 d = uvNodesList[l].Position - uvNodesList[l].Neighbours[ii].Position;
								Vector2 n3 = new Vector2(0f - d.y, d.x).Normalized();
								DeselectAllEdgesWithIdenticalNormals(n3, uvNodesList[l]);
								n3 = new Vector2(d.y, 0f - d.x).Normalized();
								DeselectAllEdgesWithIdenticalNormals(n3, uvNodesList[l].Neighbours[ii]);
							}
						}
						else
						{
							SelectUvNode(uvNodesList[l]);
							SelectUvNode(uvNodesList[l].Neighbours[ii]);
							if (Input.IsKeyPressed(16777240))
							{
								Vector2 d2 = uvNodesList[l].Position - uvNodesList[l].Neighbours[ii].Position;
								Vector2 n4 = new Vector2(0f - d2.y, d2.x).Normalized();
								SelectAllEdgesWithIdenticalNormals(n4, uvNodesList[l]);
								n4 = new Vector2(d2.y, 0f - d2.x).Normalized();
								SelectAllEdgesWithIdenticalNormals(n4, uvNodesList[l].Neighbours[ii]);
							}
						}
						breakSecondLoop = true;
					}
					if (breakSecondLoop)
					{
						break;
					}
				}
				break;
			}
			case UvSelectionModeEnum.ISLAND:
			{
				for (int j = 0; j < uvNodesList.Count; j++)
				{
					if (!(collisionUVPosition.DistanceSquaredTo(uvNodesList[j].Position) < maxNodeDistance))
					{
						continue;
					}
					if (selectedUvNodesHashSet.Contains(j))
					{
						DeselectUvNode(uvNodesList[j]);
						int group = uvNodesList[j].Group;
						foreach (UvNode n in uvNodesList)
						{
							if (n.Group == group)
							{
								DeselectUvNode(n);
							}
						}
						break;
					}
					SelectUvNode(uvNodesList[j]);
					int group2 = uvNodesList[j].Group;
					foreach (UvNode n2 in uvNodesList)
					{
						if (n2.Group == group2)
						{
							SelectUvNode(n2);
						}
					}
					break;
				}
				break;
			}
			}
		}
		if (Input.IsActionJustPressed("uv_node_move") && !doMoveUvNodes)
		{
			doMoveUvNodes = true;
			uvNodeCommand = (UvNodeCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.UVNODE);
			selectedUvNodesOffsetsList.Clear();
			foreach (int i2 in selectedUvNodesHashSet)
			{
				selectedUvNodesOffsetsList.Add(uvNodesList[i2].Position - collisionUVPosition);
				uvNodeCommand.AddNode(uvNodesList[i2]);
			}
			workspace.Worksheet.History.StopRecording("UV Nodes Moved [" + selectedUvNodesHashSet.Count + "]");
			uvNodeCommand = null;
		}
		if (Input.IsActionJustReleased("uv_node_move") && doMoveUvNodes)
		{
			doMoveUvNodes = false;
			CreateMesh();
		}
		if (!doMoveUvNodes)
		{
			return;
		}
		Transform transform = Transform.Identity;
		int ii2 = 0;
		bool doGridLock = doUvNodesGridLock;
		if (Input.IsKeyPressed(16777238))
		{
			doGridLock = !doGridLock;
		}
		Vector2 pixelPosition = default(Vector2);
		Vector2 position = default(Vector2);
		foreach (int i3 in selectedUvNodesHashSet)
		{
			if (doGridLock)
			{
				pixelPosition.x = Mathf.FloorToInt((selectedUvNodesOffsetsList[ii2].x + collisionUVPosition.x) * (float)width + 0.5f);
				pixelPosition.y = Mathf.FloorToInt((selectedUvNodesOffsetsList[ii2].y + collisionUVPosition.y) * (float)height + 0.5f);
			}
			else
			{
				pixelPosition.x = (selectedUvNodesOffsetsList[ii2].x + collisionUVPosition.x) * (float)width;
				pixelPosition.y = (selectedUvNodesOffsetsList[ii2].y + collisionUVPosition.y) * (float)height;
			}
			position.x = pixelPosition.x * pixelSize.x;
			position.y = pixelPosition.y * pixelSize.y;
			uvNodesList[i3].ChangePosition(position);
			transform.origin.x = position.x * (float)width * 0.25f;
			transform.origin.z = position.y * (float)height * 0.25f;
			UvNodesMultiMesh.SetInstanceTransform(uvNodesList[i3].InstanceIndex, transform);
			ii2++;
		}
		CreateUvLayout();
	}

	private void Clear()
	{
		meshScale = 1f;
		meshInstance.Scale = new Vector3(meshScale, meshScale, meshScale);
		wireframeMesh.Scale = new Vector3(meshScale, meshScale, meshScale);
		uvScale = 1f;
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "uvScale", uvScale);
		verticesList.Clear();
		normalsList.Clear();
		uvsList.Clear();
		indicesList.Clear();
		groupList.Clear();
		trianglesList.Clear();
		quadranglesList.Clear();
		polygonsList.Clear();
		aabb.Position = Vector3.Zero;
		aabb.Size = Vector3.Zero;
		aabb.End = Vector3.Zero;
		collisionVerticesList.Clear();
		collisionTrianglesList.Clear();
		collisionAABB.Position = Vector3.Zero;
		collisionAABB.Size = Vector3.Zero;
		collisionAABB.End = Vector3.Zero;
		uvNodesList.Clear();
		mesh = null;
		meshInstance.Mesh = mesh;
		wireframeMesh.Clear();
		uvLayoutImmediateGeometry.Clear();
		isMeshLoaded = false;
		meshFile = "";
	}

	public void Reset()
	{
		isEditingActivated = false;
		doMoveUvNodes = false;
		doUvNodesGridLock = true;
		uvSelectionMode = UvSelectionModeEnum.NODE;
		selectedUvNodesHashSet.Clear();
		illumination = IlluminationEnum.ILLUMINATED;
		CreateWorksheetPlane(workspace.Worksheet.Data);
	}

	public void ActivateFullShaderMaterial()
	{
		meshInstance.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW);
	}

	public void ActivateChannelShaderMaterial()
	{
		meshInstance.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW_CHANNEL);
	}

	public void ChangeShader(Data worksheetData, ShaderEnum shader)
	{
		this.shader = shader;
		switch (this.shader)
		{
		case ShaderEnum.DEFAULT:
			MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW, MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW_FULL));
			MaterialManager.SetEmissionStrength(4f);
			Register.EnvironmentManager.SetGlowIntensity(1.25f);
			ActivateFullShaderMaterial();
			break;
		case ShaderEnum.TWOSIDES:
			MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW, MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW_FULL_TWOSIDES));
			MaterialManager.SetEmissionStrength(4f);
			Register.EnvironmentManager.SetGlowIntensity(1.25f);
			ActivateFullShaderMaterial();
			break;
		case ShaderEnum.TRANSMISSION:
			MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW, MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW_FULL_TRANSMISSION));
			MaterialManager.SetEmissionStrength(0f);
			Register.EnvironmentManager.SetGlowIntensity(0f);
			ActivateFullShaderMaterial();
			break;
		}
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "colorMapTexture", worksheetData.ColorChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "normalMapTexture", worksheetData.NormalChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "roughnessMapTexture", worksheetData.RoughnessChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "metallicityMapTexture", worksheetData.MetallicityChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "emissionMapTexture", worksheetData.EmissionChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "previewMaskTexture", Register.DrawingPreviewManager.BlendingMaskImageTexture);
	}

	public float GetMeshScale()
	{
		return meshScale;
	}

	public void SetMeshScale(float scale)
	{
		meshScale = scale;
		meshInstance.Scale = new Vector3(meshScale, meshScale, meshScale);
		wireframeMesh.Scale = new Vector3(meshScale, meshScale, meshScale);
		CreateCollisionMesh();
	}

	public float GetUvScale()
	{
		return uvScale;
	}

	public void SetUvScale(float scale)
	{
		uvScale = scale;
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "uvScale", uvScale);
	}

	private void SetUvNodeGroup(UvNode uvNode, int group)
	{
		if (uvNode.Group >= 0)
		{
			return;
		}
		uvNode.Group = group;
		foreach (int i in uvNode.IndicesList)
		{
			groupList[i] = uvNode.Group;
		}
		foreach (UvNode n in uvNode.Neighbours)
		{
			SetUvNodeGroup(n, uvNode.Group);
		}
	}

	public void ActivateUVsEditing(bool activated)
	{
		isEditingActivated = activated;
		uvNodesMultiMeshInstance.Visible = activated;
	}

	public void ActivateUVsNodesGridLock(bool activated)
	{
		doUvNodesGridLock = activated;
	}

	public void ChangeUVsSelectionMode(UvSelectionModeEnum uvsSelectionMode)
	{
		uvSelectionMode = uvsSelectionMode;
	}

	public void SelectUvNode(UvNode uvNode)
	{
		if (!selectedUvNodesHashSet.Contains(uvNode.ListIndex))
		{
			selectedUvNodesHashSet.Add(uvNode.ListIndex);
			UvNodesMultiMesh.SetInstanceColor(uvNode.InstanceIndex, Settings.NodeSelectedColor);
		}
	}

	public void DeselectUvNode(UvNode uvNode, bool removeFromList = true)
	{
		if (selectedUvNodesHashSet.Contains(uvNode.ListIndex))
		{
			if ((Mathf.PosMod(uvNodesList[uvNode.ListIndex].Position.x * (float)workspace.Worksheet.Data.Width, 1f) < 0.05f || Mathf.PosMod(uvNodesList[uvNode.ListIndex].Position.x * (float)workspace.Worksheet.Data.Width, 1f) > 0.95f) && (Mathf.PosMod(uvNodesList[uvNode.ListIndex].Position.y * (float)workspace.Worksheet.Data.Height, 1f) < 0.05f || Mathf.PosMod(uvNodesList[uvNode.ListIndex].Position.y * (float)workspace.Worksheet.Data.Height, 1f) > 0.95f))
			{
				uvNodesMultiMeshInstance.Multimesh.SetInstanceColor(uvNode.InstanceIndex, Settings.NodeColor);
			}
			else
			{
				uvNodesMultiMeshInstance.Multimesh.SetInstanceColor(uvNode.InstanceIndex, Settings.NodeErrorColor);
			}
			if (removeFromList)
			{
				selectedUvNodesHashSet.Remove(uvNode.ListIndex);
			}
		}
	}

	public bool IsPositionOnUvEdge(Vector2 position, UvNode uvNode1, UvNode uvNode2, float distance)
	{
		Vector2 delta = uvNode1.Position - uvNode2.Position;
		Vector2 normal = new Vector2(delta.y, 0f - delta.x).Normalized();
		float t = ((position.x - uvNode2.Position.x) * delta.x + (position.y - uvNode2.Position.y) * delta.y) / delta.Dot(delta);
		if (0f <= t && t <= 1f && Mathf.Abs((position.x - uvNode2.Position.x) * normal.x + (position.y - uvNode2.Position.y) * normal.y) < distance)
		{
			return true;
		}
		return false;
	}

	public bool HasNodesIdenticalNormal(Vector2 normal, UvNode uvNode1, UvNode uvNode2, float tolerance = 0.01f)
	{
		Vector2 d = uvNode1.Position - uvNode2.Position;
		if (new Vector2(d.y, 0f - d.x).Normalized().Dot(normal) > 1f - tolerance)
		{
			return true;
		}
		return false;
	}

	public void SelectAllEdgesWithIdenticalNormals(Vector2 normal, UvNode uvNode)
	{
		for (int n = 0; n < uvNode.Neighbours.Count; n++)
		{
			if (HasNodesIdenticalNormal(normal, uvNode, uvNode.Neighbours[n]) && !selectedUvNodesHashSet.Contains(uvNode.Neighbours[n].ListIndex))
			{
				SelectUvNode(uvNode.Neighbours[n]);
				SelectAllEdgesWithIdenticalNormals(normal, uvNode.Neighbours[n]);
			}
		}
	}

	public void DeselectAllEdgesWithIdenticalNormals(Vector2 normal, UvNode uvNode)
	{
		for (int n = 0; n < uvNode.Neighbours.Count; n++)
		{
			if (HasNodesIdenticalNormal(normal, uvNode, uvNode.Neighbours[n]) && selectedUvNodesHashSet.Contains(uvNode.Neighbours[n].ListIndex))
			{
				DeselectUvNode(uvNode.Neighbours[n]);
				DeselectAllEdgesWithIdenticalNormals(normal, uvNode.Neighbours[n]);
			}
		}
	}

	public void CreateMesh()
	{
		trianglesCount = indicesList.Count / 3;
		Array godotMeshArrayData = new Array();
		godotMeshArrayData.Resize(9);
		godotMeshArrayData[0] = verticesList.ToArray();
		godotMeshArrayData[1] = normalsList.ToArray();
		godotMeshArrayData[4] = uvsList.ToArray();
		godotMeshArrayData[8] = indicesList.ToArray();
		mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, godotMeshArrayData, null, 2169856u);
		mesh.RegenNormalmaps();
		aabb = mesh.GetAabb();
		meshInstance.Mesh = mesh;
		Transform transform = meshInstance.Transform;
		transform.origin = -aabb.Position - aabb.Size * 0.5f;
		meshInstance.Transform = transform;
		wireframeMesh.Transform = meshInstance.Transform;
	}

	public void CreateCollisionMesh()
	{
		collisionVerticesList.Clear();
		Transform transform = meshInstance.GlobalTransform;
		for (int i = 0; i < verticesList.Count; i++)
		{
			collisionVerticesList.Add(transform.Xform(verticesList[i]));
		}
		collisionTrianglesList.Clear();
		for (int j = 0; j < indicesList.Count; j += 3)
		{
			Vector3 v0 = collisionVerticesList[indicesList[j]];
			Vector3 vector = collisionVerticesList[indicesList[j + 1]];
			Vector3 v2 = collisionVerticesList[indicesList[j + 2]];
			Vector3 n = (vector - v0).Cross(v2 - v0);
			Vector3 dv1 = vector - v0;
			Vector3 dv2 = v2 - v0;
			Vector2 du1 = uvsList[indicesList[j + 1]] - uvsList[indicesList[j]];
			Vector2 du2 = uvsList[indicesList[j + 2]] - uvsList[indicesList[j]];
			if (du1.x == 0f && du1.y == 0f)
			{
				du1.x = du2.y;
				du1.y = 0f - du2.x;
			}
			if (du2.x == 0f && du2.y == 0f)
			{
				du2.x = du1.y;
				du2.y = 0f - du1.x;
			}
			Vector3 tangent = Vector3.Right;
			float f = du2.Cross(du1);
			if (f != 0f)
			{
				f = 1f / f;
				tangent.x = f * (du2.y * dv1.x - du1.y * dv2.x);
				tangent.y = f * (du2.y * dv1.y - du1.y * dv2.y);
				tangent.z = f * (du2.y * dv1.z - du1.y * dv2.z);
			}
			collisionTrianglesList.Add(new Triangle(indicesList[j], indicesList[j + 1], indicesList[j + 2], n, -tangent));
		}
		Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int k = 0; k < collisionVerticesList.Count; k++)
		{
			min.x = Mathf.Min(min.x, collisionVerticesList[k].x);
			min.y = Mathf.Min(min.y, collisionVerticesList[k].y);
			min.z = Mathf.Min(min.z, collisionVerticesList[k].z);
			max.x = Mathf.Max(max.x, collisionVerticesList[k].x);
			max.y = Mathf.Max(max.y, collisionVerticesList[k].y);
			max.z = Mathf.Max(max.z, collisionVerticesList[k].z);
		}
		collisionAABB.Position = min;
		collisionAABB.Size = max - min;
		collisionAABB.End = max;
	}

	public void CreateUvLayout()
	{
		Vector2 pixelSize = new Vector2(1f / (float)workspace.Worksheet.Data.Width, 1f / (float)workspace.Worksheet.Data.Height);
		wireframeMesh.Clear();
		uvLayoutImmediateGeometry.Clear();
		if (isMeshLoaded)
		{
			float normalScale = 0.0001f;
			Vector3 offset = new Vector3(0f, Settings.UvNodesOffset, 0f);
			Vector2[] uvsArray = new Vector2[4];
			Vector3[] verticesArray = new Vector3[4];
			wireframeMesh.Begin(Mesh.PrimitiveType.Lines);
			wireframeMesh.SetColor(Settings.AccentColor);
			uvLayoutImmediateGeometry.Begin(Mesh.PrimitiveType.Lines);
			uvLayoutImmediateGeometry.SetColor(Settings.AccentColor);
			for (int i = 0; i < trianglesList.Count; i += 3)
			{
				uvsArray[0] = new Vector2(uvsList[trianglesList[i]] / pixelSize);
				uvsArray[1] = new Vector2(uvsList[trianglesList[i + 1]] / pixelSize);
				uvsArray[2] = new Vector2(uvsList[trianglesList[i + 2]] / pixelSize);
				verticesArray[0] = new Vector3(uvsArray[0].x * 0.25f, 0f, uvsArray[0].y * 0.25f);
				verticesArray[1] = new Vector3(uvsArray[1].x * 0.25f, 0f, uvsArray[1].y * 0.25f);
				verticesArray[2] = new Vector3(uvsArray[2].x * 0.25f, 0f, uvsArray[2].y * 0.25f);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[0]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[1]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[1]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[2]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[2]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[0]);
				verticesArray[0] = verticesList[trianglesList[i]] + normalsList[trianglesList[i]] * normalScale;
				verticesArray[1] = verticesList[trianglesList[i + 1]] + normalsList[trianglesList[i + 1]] * normalScale;
				verticesArray[2] = verticesList[trianglesList[i + 2]] + normalsList[trianglesList[i + 1]] * normalScale;
				wireframeMesh.AddVertex(verticesArray[0]);
				wireframeMesh.AddVertex(verticesArray[1]);
				wireframeMesh.AddVertex(verticesArray[1]);
				wireframeMesh.AddVertex(verticesArray[2]);
				wireframeMesh.AddVertex(verticesArray[2]);
				wireframeMesh.AddVertex(verticesArray[0]);
			}
			for (int j = 0; j < quadranglesList.Count; j += 4)
			{
				uvsArray[0] = new Vector2(uvsList[quadranglesList[j]] / pixelSize);
				uvsArray[1] = new Vector2(uvsList[quadranglesList[j + 1]] / pixelSize);
				uvsArray[2] = new Vector2(uvsList[quadranglesList[j + 2]] / pixelSize);
				uvsArray[3] = new Vector2(uvsList[quadranglesList[j + 3]] / pixelSize);
				verticesArray[0] = new Vector3(uvsArray[0].x * 0.25f, 0f, uvsArray[0].y * 0.25f);
				verticesArray[1] = new Vector3(uvsArray[1].x * 0.25f, 0f, uvsArray[1].y * 0.25f);
				verticesArray[2] = new Vector3(uvsArray[2].x * 0.25f, 0f, uvsArray[2].y * 0.25f);
				verticesArray[3] = new Vector3(uvsArray[3].x * 0.25f, 0f, uvsArray[3].y * 0.25f);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[0]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[1]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[1]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[2]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[2]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[3]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[3]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[0]);
				verticesArray[0] = verticesList[quadranglesList[j]] + normalsList[quadranglesList[j]] * normalScale;
				verticesArray[1] = verticesList[quadranglesList[j + 1]] + normalsList[quadranglesList[j + 1]] * normalScale;
				verticesArray[2] = verticesList[quadranglesList[j + 2]] + normalsList[quadranglesList[j + 2]] * normalScale;
				verticesArray[3] = verticesList[quadranglesList[j + 3]] + normalsList[quadranglesList[j + 3]] * normalScale;
				wireframeMesh.AddVertex(verticesArray[0]);
				wireframeMesh.AddVertex(verticesArray[1]);
				wireframeMesh.AddVertex(verticesArray[1]);
				wireframeMesh.AddVertex(verticesArray[2]);
				wireframeMesh.AddVertex(verticesArray[2]);
				wireframeMesh.AddVertex(verticesArray[3]);
				wireframeMesh.AddVertex(verticesArray[3]);
				wireframeMesh.AddVertex(verticesArray[0]);
			}
			for (int k = 0; k < polygonsList.Count; k++)
			{
				for (int ii = 0; ii < polygonsList[k].Length - 1; ii++)
				{
					uvsArray[0] = new Vector2(uvsList[polygonsList[k][ii]] / pixelSize);
					uvsArray[1] = new Vector2(uvsList[polygonsList[k][ii + 1]] / pixelSize);
					verticesArray[0] = new Vector3(uvsArray[0].x * 0.25f, 0f, uvsArray[0].y * 0.25f);
					verticesArray[1] = new Vector3(uvsArray[1].x * 0.25f, 0f, uvsArray[1].y * 0.25f);
					uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[0]);
					uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[1]);
					verticesArray[0] = verticesList[polygonsList[k][ii]] + normalsList[polygonsList[k][ii]] * normalScale;
					verticesArray[1] = verticesList[polygonsList[k][ii + 1]] + normalsList[polygonsList[k][ii + 1]] * normalScale;
					wireframeMesh.AddVertex(verticesArray[0]);
					wireframeMesh.AddVertex(verticesArray[1]);
				}
				uvsArray[0] = new Vector2(uvsList[polygonsList[k][polygonsList[k].Length - 1]] / pixelSize);
				uvsArray[1] = new Vector2(uvsList[polygonsList[k][0]] / pixelSize);
				verticesArray[0] = new Vector3(uvsArray[0].x * 0.25f, 0f, uvsArray[0].y * 0.25f);
				verticesArray[1] = new Vector3(uvsArray[1].x * 0.25f, 0f, uvsArray[1].y * 0.25f);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[0]);
				uvLayoutImmediateGeometry.AddVertex(offset + verticesArray[1]);
				verticesArray[0] = verticesList[polygonsList[k][polygonsList[k].Length - 1]] + normalsList[polygonsList[k][polygonsList[k].Length - 1]] * normalScale;
				verticesArray[1] = verticesList[polygonsList[k][0]] + normalsList[polygonsList[k][0]] * normalScale;
				wireframeMesh.AddVertex(verticesArray[0]);
				wireframeMesh.AddVertex(verticesArray[1]);
			}
			uvLayoutImmediateGeometry.End();
			uvLayoutImmediateGeometry.Visible = true;
			wireframeMesh.End();
			wireframeMesh.Visible = false;
		}
		else
		{
			uvLayoutImmediateGeometry.Visible = false;
			wireframeMesh.Visible = false;
		}
	}

	public void CreateUvNodes()
	{
		System.Collections.Generic.Dictionary<Vector2, List<int>> uvNodesDictionary = new System.Collections.Generic.Dictionary<Vector2, List<int>>();
		int i;
		for (i = 0; i < indicesList.Count; i++)
		{
			Vector2 uv = uvsList[indicesList[i]];
			if (!uvNodesDictionary.ContainsKey(uv))
			{
				uvNodesDictionary.Add(uv, new List<int>());
			}
			uvNodesDictionary[uv].Add(indicesList[i]);
		}
		uvNodesMultiMeshInstance.Multimesh.InstanceCount = uvNodesDictionary.Count;
		i = 0;
		Transform transform = Transform.Identity;
		uvNodesList.Clear();
		foreach (KeyValuePair<Vector2, List<int>> kvp in uvNodesDictionary)
		{
			UvNode uvNode = new UvNode(uvsList, kvp.Value);
			uvNode.ListIndex = uvNodesList.Count;
			uvNode.InstanceIndex = i;
			uvNode.Position = kvp.Key;
			uvNodesList.Add(uvNode);
			transform.origin.x = uvNode.Position.x * (float)workspace.Worksheet.Data.Width * 0.25f;
			transform.origin.z = uvNode.Position.y * (float)workspace.Worksheet.Data.Height * 0.25f;
			uvNodesMultiMeshInstance.Multimesh.SetInstanceTransform(i, transform);
			if ((Mathf.PosMod(uvNode.Position.x * (float)workspace.Worksheet.Data.Width, 1f) < 0.05f || Mathf.PosMod(uvNode.Position.x * (float)workspace.Worksheet.Data.Width, 1f) > 0.95f) && (Mathf.PosMod(uvNode.Position.y * (float)workspace.Worksheet.Data.Height, 1f) < 0.05f || Mathf.PosMod(uvNode.Position.y * (float)workspace.Worksheet.Data.Height, 1f) > 0.95f))
			{
				uvNodesMultiMeshInstance.Multimesh.SetInstanceColor(i, Settings.NodeColor);
			}
			else
			{
				uvNodesMultiMeshInstance.Multimesh.SetInstanceColor(i, Settings.NodeErrorColor);
			}
			i++;
		}
		uvNodesMultiMeshInstance.Multimesh.VisibleInstanceCount = i;
		uvNodesMultiMeshInstance.Visible = isEditingActivated;
		int[] uvNodesIndices = new int[4];
		Vector2[] uvsArray = new Vector2[4];
		int counter;
		for (i = 0; i < trianglesList.Count; i += 3)
		{
			uvsArray[0] = new Vector2(uvsList[trianglesList[i]]);
			uvsArray[1] = new Vector2(uvsList[trianglesList[i + 1]]);
			uvsArray[2] = new Vector2(uvsList[trianglesList[i + 2]]);
			counter = 0;
			for (int ii = 0; ii < uvNodesList.Count; ii++)
			{
				if (uvNodesList[ii].Position == uvsArray[0])
				{
					uvNodesIndices[0] = ii;
					counter++;
				}
				else if (uvNodesList[ii].Position == uvsArray[1])
				{
					uvNodesIndices[1] = ii;
					counter++;
				}
				else if (uvNodesList[ii].Position == uvsArray[2])
				{
					uvNodesIndices[2] = ii;
					counter++;
				}
				if (counter > 2)
				{
					break;
				}
			}
			uvNodesList[uvNodesIndices[0]].AddNeighbour(uvNodesList[uvNodesIndices[1]]);
			uvNodesList[uvNodesIndices[0]].AddNeighbour(uvNodesList[uvNodesIndices[2]]);
			uvNodesList[uvNodesIndices[1]].AddNeighbour(uvNodesList[uvNodesIndices[0]]);
			uvNodesList[uvNodesIndices[1]].AddNeighbour(uvNodesList[uvNodesIndices[2]]);
			uvNodesList[uvNodesIndices[2]].AddNeighbour(uvNodesList[uvNodesIndices[0]]);
			uvNodesList[uvNodesIndices[2]].AddNeighbour(uvNodesList[uvNodesIndices[1]]);
		}
		for (i = 0; i < quadranglesList.Count; i += 4)
		{
			uvsArray[0] = new Vector2(uvsList[quadranglesList[i]]);
			uvsArray[1] = new Vector2(uvsList[quadranglesList[i + 1]]);
			uvsArray[2] = new Vector2(uvsList[quadranglesList[i + 2]]);
			uvsArray[3] = new Vector2(uvsList[quadranglesList[i + 3]]);
			counter = 0;
			for (int j = 0; j < uvNodesList.Count; j++)
			{
				if (uvNodesList[j].Position == uvsArray[0])
				{
					uvNodesIndices[0] = j;
					counter++;
				}
				else if (uvNodesList[j].Position == uvsArray[1])
				{
					uvNodesIndices[1] = j;
					counter++;
				}
				else if (uvNodesList[j].Position == uvsArray[2])
				{
					uvNodesIndices[2] = j;
					counter++;
				}
				else if (uvNodesList[j].Position == uvsArray[3])
				{
					uvNodesIndices[3] = j;
					counter++;
				}
				if (counter > 3)
				{
					break;
				}
			}
			uvNodesList[uvNodesIndices[0]].AddNeighbour(uvNodesList[uvNodesIndices[1]]);
			uvNodesList[uvNodesIndices[0]].AddNeighbour(uvNodesList[uvNodesIndices[3]]);
			uvNodesList[uvNodesIndices[1]].AddNeighbour(uvNodesList[uvNodesIndices[0]]);
			uvNodesList[uvNodesIndices[1]].AddNeighbour(uvNodesList[uvNodesIndices[2]]);
			uvNodesList[uvNodesIndices[2]].AddNeighbour(uvNodesList[uvNodesIndices[1]]);
			uvNodesList[uvNodesIndices[2]].AddNeighbour(uvNodesList[uvNodesIndices[3]]);
			uvNodesList[uvNodesIndices[3]].AddNeighbour(uvNodesList[uvNodesIndices[0]]);
			uvNodesList[uvNodesIndices[3]].AddNeighbour(uvNodesList[uvNodesIndices[2]]);
		}
		for (i = 0; i < polygonsList.Count; i++)
		{
			for (int k = 0; k < polygonsList[i].Length - 1; k++)
			{
				uvsArray[0] = new Vector2(uvsList[polygonsList[i][k]]);
				uvsArray[1] = new Vector2(uvsList[polygonsList[i][k + 1]]);
				counter = 0;
				for (int iii = 0; iii < uvNodesList.Count; iii++)
				{
					if (uvNodesList[iii].Position == uvsArray[0])
					{
						uvNodesIndices[0] = iii;
						counter++;
					}
					else if (uvNodesList[iii].Position == uvsArray[1])
					{
						uvNodesIndices[1] = iii;
						counter++;
					}
					if (counter > 1)
					{
						break;
					}
				}
				uvNodesList[uvNodesIndices[0]].AddNeighbour(uvNodesList[uvNodesIndices[1]]);
				uvNodesList[uvNodesIndices[1]].AddNeighbour(uvNodesList[uvNodesIndices[0]]);
			}
			uvsArray[0] = new Vector2(uvsList[polygonsList[i][polygonsList[i].Length - 1]]);
			uvsArray[1] = new Vector2(uvsList[polygonsList[i][0]]);
			counter = 0;
			for (int l = 0; l < uvNodesList.Count; l++)
			{
				if (uvNodesList[l].Position == uvsArray[0])
				{
					uvNodesIndices[0] = l;
					counter++;
				}
				else if (uvNodesList[l].Position == uvsArray[1])
				{
					uvNodesIndices[1] = l;
					counter++;
				}
				if (counter > 1)
				{
					break;
				}
			}
			uvNodesList[uvNodesIndices[0]].AddNeighbour(uvNodesList[uvNodesIndices[1]]);
			uvNodesList[uvNodesIndices[1]].AddNeighbour(uvNodesList[uvNodesIndices[0]]);
		}
		counter = -1;
		foreach (UvNode n in uvNodesList)
		{
			if (n.Group < 0)
			{
				counter++;
			}
			SetUvNodeGroup(n, counter);
		}
	}

	public Mesh CreateWorksheetPlane(Data worksheetData)
	{
		Clear();
		if (worksheetData.Tileable)
		{
			verticesList.Add(new Vector3((float)(-worksheetData.Width) * 0.25f, 0f, (float)(-worksheetData.Height) * 0.25f * meshScale));
			normalsList.Add(Vector3.Up);
			uvsList.Add(new Vector2(0f, 0f));
			verticesList.Add(new Vector3(2f * (float)worksheetData.Width * 0.25f, 0f, (float)(-worksheetData.Height) * 0.25f * meshScale));
			normalsList.Add(Vector3.Up);
			uvsList.Add(new Vector2(3f, 0f));
			verticesList.Add(new Vector3(2f * (float)worksheetData.Width * 0.25f, 0f, 2f * (float)worksheetData.Height * 0.25f * meshScale));
			normalsList.Add(Vector3.Up);
			uvsList.Add(new Vector2(3f, 3f));
			verticesList.Add(new Vector3((float)(-worksheetData.Width) * 0.25f, 0f, 2f * (float)worksheetData.Height * 0.25f * meshScale));
			normalsList.Add(Vector3.Up);
			uvsList.Add(new Vector2(0f, 3f));
		}
		else
		{
			verticesList.Add(new Vector3(0f, 0f, 0f));
			normalsList.Add(Vector3.Up);
			uvsList.Add(new Vector2(0f, 0f));
			verticesList.Add(new Vector3((float)worksheetData.Width * 0.25f, 0f, 0f));
			normalsList.Add(Vector3.Up);
			uvsList.Add(new Vector2(1f, 0f));
			verticesList.Add(new Vector3((float)worksheetData.Width * 0.25f, 0f, (float)worksheetData.Height * 0.25f));
			normalsList.Add(Vector3.Up);
			uvsList.Add(new Vector2(1f, 1f));
			verticesList.Add(new Vector3(0f, 0f, (float)worksheetData.Height * 0.25f));
			normalsList.Add(Vector3.Up);
			uvsList.Add(new Vector2(0f, 1f));
		}
		indicesList.Add(0);
		groupList.Add(0);
		indicesList.Add(1);
		groupList.Add(0);
		indicesList.Add(3);
		groupList.Add(0);
		indicesList.Add(1);
		groupList.Add(0);
		indicesList.Add(2);
		groupList.Add(0);
		indicesList.Add(3);
		groupList.Add(0);
		quadranglesList.Add(0);
		quadranglesList.Add(1);
		quadranglesList.Add(2);
		quadranglesList.Add(3);
		CreateMesh();
		CreateCollisionMesh();
		CreateUvLayout();
		CreateUvNodes();
		return mesh;
	}

	public Mesh LoadMesh(string file)
	{
		Clear();
		CultureInfo cultureInfo = new CultureInfo("");
		cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
		List<Vector3> rawVerticesList = new List<Vector3>();
		List<Vector3> rawNormalsList = new List<Vector3>();
		List<Vector2> rawUVsList = new List<Vector2>();
		List<Vector3i> faceIndices = new List<Vector3i>();
		if (System.IO.File.Exists(file))
		{
			StreamReader streamReader = new StreamReader(System.IO.File.OpenRead(file));
			while (!streamReader.EndOfStream)
			{
				List<string> words = new List<string>(streamReader.ReadLine().ToLower().Split(new char[1] { ' ' }));
				words.RemoveAll((string s) => s == string.Empty);
				if (words.Count == 0)
				{
					continue;
				}
				string type = words[0];
				words.RemoveAt(0);
				switch (type)
				{
				case "v":
					rawVerticesList.Add(new Vector3(float.Parse(words[0], cultureInfo), float.Parse(words[1], cultureInfo), float.Parse(words[2], cultureInfo)));
					break;
				case "vt":
					rawUVsList.Add(new Vector2(float.Parse(words[0], cultureInfo), 1f - float.Parse(words[1], cultureInfo)));
					break;
				case "vn":
					rawNormalsList.Add(new Vector3(float.Parse(words[0], cultureInfo), float.Parse(words[1], cultureInfo), float.Parse(words[2], cultureInfo)));
					break;
				case "f":
				{
					int i = 0;
					faceIndices.Clear();
					foreach (string w in words)
					{
						if (w.Length != 0)
						{
							string[] comps = w.Split(new char[1] { '/' });
							Vector3i vertexIndices = Vector3i.Zero;
							vertexIndices[0] = int.Parse(comps[0]) - 1;
							if (comps.Length > 1 && comps[1].Length != 0)
							{
								vertexIndices[1] = int.Parse(comps[1]) - 1;
							}
							if (comps.Length > 2)
							{
								vertexIndices[2] = int.Parse(comps[2]) - 1;
							}
							faceIndices.Add(vertexIndices);
							i++;
						}
					}
					for (int v = 0; v < i; v++)
					{
						verticesList.Add(rawVerticesList[faceIndices[v][0]]);
						uvsList.Add(rawUVsList[faceIndices[v][1]]);
						normalsList.Add(rawNormalsList[faceIndices[v][2]]);
					}
					int verticesCount = verticesList.Count;
					switch (i)
					{
					case 3:
						indicesList.Add(verticesCount - 3);
						groupList.Add(0);
						indicesList.Add(verticesCount - 1);
						groupList.Add(0);
						indicesList.Add(verticesCount - 2);
						groupList.Add(0);
						trianglesList.Add(verticesCount - 3);
						trianglesList.Add(verticesCount - 2);
						trianglesList.Add(verticesCount - 1);
						break;
					case 4:
						indicesList.Add(verticesCount - 3);
						groupList.Add(0);
						indicesList.Add(verticesCount - 1);
						groupList.Add(0);
						indicesList.Add(verticesCount - 2);
						groupList.Add(0);
						indicesList.Add(verticesCount - 4);
						groupList.Add(0);
						indicesList.Add(verticesCount - 1);
						groupList.Add(0);
						indicesList.Add(verticesCount - 3);
						groupList.Add(0);
						quadranglesList.Add(verticesCount - 4);
						quadranglesList.Add(verticesCount - 3);
						quadranglesList.Add(verticesCount - 2);
						quadranglesList.Add(verticesCount - 1);
						break;
					}
					if (i > 4)
					{
						int[] polygonIndicesArray = new int[i];
						List<Vector3> polygonVerticesList = new List<Vector3>();
						for (int v2 = 0; v2 < i; v2++)
						{
							polygonVerticesList.Add(verticesList[verticesCount - i + v2]);
							polygonIndicesArray[v2] = verticesCount - i + v2;
						}
						polygonsList.Add(polygonIndicesArray);
						List<TriangulatePolygon.Triangle> polygonTrianglesList = TriangulatePolygon.TriangulateConvexPolygon(polygonVerticesList);
						for (int ii = 0; ii < polygonTrianglesList.Count; ii++)
						{
							int i2 = verticesCount - i + polygonTrianglesList[ii].v3.index;
							int i3 = verticesCount - i + polygonTrianglesList[ii].v2.index;
							int i4 = verticesCount - i + polygonTrianglesList[ii].v1.index;
							indicesList.Add(i2);
							groupList.Add(0);
							indicesList.Add(i3);
							groupList.Add(0);
							indicesList.Add(i4);
							groupList.Add(0);
						}
					}
					break;
				}
				}
			}
			streamReader.Dispose();
			isMeshLoaded = true;
			meshFile = file;
			CreateMesh();
			CreateCollisionMesh();
			CreateUvLayout();
			CreateUvNodes();
			return mesh;
		}
		Reset();
		return mesh;
	}

	public void SaveMesh(string file)
	{
	}

	public void WriteToBinaryStream(BinaryWriter binaryWriter)
	{
		if (isMeshLoaded)
		{
			binaryWriter.Write(value: true);
			binaryWriter.Write(verticesList.Count);
			for (int i = 0; i < verticesList.Count; i++)
			{
				binaryWriter.Write(verticesList[i].x);
				binaryWriter.Write(verticesList[i].y);
				binaryWriter.Write(verticesList[i].z);
			}
			binaryWriter.Write(uvsList.Count);
			for (int j = 0; j < uvsList.Count; j++)
			{
				binaryWriter.Write(uvsList[j].x);
				binaryWriter.Write(uvsList[j].y);
			}
			binaryWriter.Write(normalsList.Count);
			for (int k = 0; k < normalsList.Count; k++)
			{
				binaryWriter.Write(normalsList[k].x);
				binaryWriter.Write(normalsList[k].y);
				binaryWriter.Write(normalsList[k].z);
			}
			binaryWriter.Write(indicesList.Count);
			for (int l = 0; l < indicesList.Count; l++)
			{
				binaryWriter.Write(indicesList[l]);
			}
			binaryWriter.Write(trianglesList.Count);
			for (int m = 0; m < trianglesList.Count; m++)
			{
				binaryWriter.Write(trianglesList[m]);
			}
			binaryWriter.Write(quadranglesList.Count);
			for (int n = 0; n < quadranglesList.Count; n++)
			{
				binaryWriter.Write(quadranglesList[n]);
			}
			binaryWriter.Write(polygonsList.Count);
			for (int num = 0; num < polygonsList.Count; num++)
			{
				binaryWriter.Write(polygonsList[num].Length);
				for (int ii = 0; ii < polygonsList[num].Length; ii++)
				{
					binaryWriter.Write(polygonsList[num][ii]);
				}
			}
		}
		else
		{
			binaryWriter.Write(value: false);
		}
	}

	public Mesh ReadFromBinaryStream(BinaryReader binaryReader)
	{
		if (binaryReader.ReadBoolean())
		{
			Clear();
			int verticesCount = binaryReader.ReadInt32();
			Vector3 vertex = default(Vector3);
			for (int i = 0; i < verticesCount; i++)
			{
				vertex.x = binaryReader.ReadSingle();
				vertex.y = binaryReader.ReadSingle();
				vertex.z = binaryReader.ReadSingle();
				verticesList.Add(vertex);
			}
			int uvsCount = binaryReader.ReadInt32();
			Vector2 uv = default(Vector2);
			for (int j = 0; j < uvsCount; j++)
			{
				uv.x = binaryReader.ReadSingle();
				uv.y = binaryReader.ReadSingle();
				uvsList.Add(uv);
			}
			int normalsCount = binaryReader.ReadInt32();
			Vector3 normal = default(Vector3);
			for (int k = 0; k < normalsCount; k++)
			{
				normal.x = binaryReader.ReadSingle();
				normal.y = binaryReader.ReadSingle();
				normal.z = binaryReader.ReadSingle();
				normalsList.Add(normal);
			}
			int indicesCount = binaryReader.ReadInt32();
			for (int l = 0; l < indicesCount; l++)
			{
				indicesList.Add(binaryReader.ReadInt32());
				groupList.Add(0);
			}
			int trianglesCount = binaryReader.ReadInt32();
			for (int m = 0; m < trianglesCount; m++)
			{
				trianglesList.Add(binaryReader.ReadInt32());
			}
			int rectanglesCount = binaryReader.ReadInt32();
			for (int n = 0; n < rectanglesCount; n++)
			{
				quadranglesList.Add(binaryReader.ReadInt32());
			}
			polygonsList = new List<int[]>();
			int polygonCount = binaryReader.ReadInt32();
			for (int num = 0; num < polygonCount; num++)
			{
				int polygonIndicesCount = binaryReader.ReadInt32();
				int[] polygonIndices = new int[polygonIndicesCount];
				for (int ii = 0; ii < polygonIndicesCount; ii++)
				{
					polygonIndices[ii] = binaryReader.ReadInt32();
				}
				polygonsList.Add(polygonIndices);
			}
			isMeshLoaded = true;
			meshFile = "";
			CreateMesh();
			CreateCollisionMesh();
			CreateUvLayout();
			CreateUvNodes();
		}
		else
		{
			Reset();
		}
		return mesh;
	}
}
