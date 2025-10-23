using System.Collections.Generic;
using Godot;

public class CollisionManager : Node
{
	public enum AxisEnum
	{
		X,
		Y,
		Z
	}

	public struct CollisionData
	{
		public bool CollisionDetected;

		public object Object;

		public Vector3 Position;

		public Vector2 UV;

		public Vector3 Normal;

		public int[] Indices;

		public int Group;

		public static CollisionData Default
		{
			get
			{
				CollisionData collisionData = default(CollisionData);
				collisionData.CollisionDetected = false;
				collisionData.Object = null;
				collisionData.Position = Vector3.Zero;
				collisionData.UV = Vector2.Zero;
				collisionData.Normal = Vector3.Zero;
				collisionData.Indices = new int[3];
				collisionData.Group = 0;
				return collisionData;
			}
		}

		public void Clear()
		{
			CollisionDetected = false;
			Object = null;
			Position = Vector3.Zero;
			UV = Vector2.Zero;
			Normal = Vector3.Zero;
			Indices[0] = (Indices[1] = (Indices[2] = 0));
			Group = 0;
		}
	}

	private Workspace workspace;

	private CollisionGrid collisionGrid;

	private float stepLength = 0.016f;

	private int raysCount;

	private CollisionData collision = CollisionData.Default;

	private CollisionData collisionLastFrame = CollisionData.Default;

	private List<CollisionData> collisionsList = new List<CollisionData>();

	private bool doDebugging;

	public float StepLength
	{
		get
		{
			return stepLength;
		}
		set
		{
			stepLength = value;
		}
	}

	public int RaysCount
	{
		get
		{
			return raysCount;
		}
		set
		{
			raysCount = value;
		}
	}

	public CollisionData Collision => collision;

	public CollisionData CollisionLastFrame => collisionLastFrame;

	public List<CollisionData> CollisionsList => collisionsList;

	public CollisionManager()
	{
		Register.CollisionManager = this;
	}

	public CollisionManager(Workspace workspace)
	{
		Register.CollisionManager = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "CollisionManager";
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);
		collisionLastFrame = collision;
	}

	public void Update()
	{
		collisionGrid = new CollisionGrid();
		DebugCollisionGrid(doDebugging);
	}

	private void DebugCollisionGrid(bool enable)
	{
		if (!enable || DebugManager.ImmediateGeometry == null)
		{
			return;
		}
		DebugManager.ImmediateGeometry.Clear();
		DebugManager.ImmediateGeometry.Begin(Mesh.PrimitiveType.Points);
		for (int y = 0; y < collisionGrid.Resolution.y; y++)
		{
			for (int z = 0; z < collisionGrid.Resolution.z; z++)
			{
				for (int x = 0; x < collisionGrid.Resolution.x; x++)
				{
					if (collisionGrid.GetCell(x, y, z).Initialized)
					{
						DebugManager.ImmediateGeometry.SetColor(new Color(1f * ((float)collisionGrid.GetCell(x, y, z).TrianglesIndicesList.Count * 2f / (float)collisionGrid.TotalNumberTriangles), 0f, 0f));
					}
					else
					{
						DebugManager.ImmediateGeometry.SetColor(Settings.AccentColor);
					}
					Vector3 center = new Vector3((float)x * collisionGrid.CellSize.x, (float)y * collisionGrid.CellSize.y, (float)z * collisionGrid.CellSize.z) + collisionGrid.AABB.Position + collisionGrid.CellSize * 0.5f;
					DebugManager.ImmediateGeometry.AddVertex(center);
				}
			}
		}
		DebugManager.ImmediateGeometry.End();
		GD.Print("CollisionGrid Size: " + collisionGrid.Resolution.ToString());
	}

	public void PlaneIntersectRay(Vector3 rayOrigin, Vector3 rayNormal, AxisEnum axis = AxisEnum.Y, float planeOffset = 0f)
	{
		float t = 0f;
		Vector3 normal = Vector3.Up;
		switch (axis)
		{
		case AxisEnum.X:
			if (rayNormal.x != 0f)
			{
				t = (planeOffset - rayOrigin.x) / rayNormal.x;
				normal = Vector3.Right;
			}
			break;
		case AxisEnum.Y:
			if (rayNormal.y != 0f)
			{
				t = (planeOffset - rayOrigin.y) / rayNormal.y;
				normal = Vector3.Up;
			}
			break;
		case AxisEnum.Z:
			if (rayNormal.z != 0f)
			{
				t = (planeOffset - rayOrigin.z) / rayNormal.z;
				normal = Vector3.Back;
			}
			break;
		}
		if (t > 0f)
		{
			collision.CollisionDetected = true;
			collision.Object = null;
			collision.Position = rayOrigin + t * rayNormal;
			collision.UV = Vector2.Zero;
			collision.Normal = normal;
			collision.Indices[0] = (collision.Indices[1] = (collision.Indices[2] = 0));
			collision.Group = 0;
		}
		else
		{
			collision.Clear();
		}
	}

	public void MeshIntersectRay(Vector3 rayOrigin, Vector3 rayNormal, bool backfaceCulling = true, float distance = float.PositiveInfinity)
	{
		collision.Clear();
		if (collisionGrid != null)
		{
			collision = collisionGrid.Intersect(rayOrigin, rayNormal, backfaceCulling, distance);
		}
		raysCount++;
	}

	public void MeshIntersectRay(ref CollisionData collision, Vector3 rayOrigin, Vector3 rayNormal, bool backfaceCulling = true, float distance = float.PositiveInfinity)
	{
		collision.Clear();
		if (collisionGrid != null)
		{
			collision = collisionGrid.Intersect(rayOrigin, rayNormal, backfaceCulling, distance);
		}
		raysCount++;
	}

	public void MeshLineIntersectRays(Vector3 rayOrigin, Vector3 startRayNormal, Vector3 endRayNormal, bool clearList = true, bool backfaceCulling = true)
	{
		if (clearList)
		{
			collisionsList.Clear();
		}
		MeshIntersectRay(rayOrigin, startRayNormal, backfaceCulling);
		CollisionData startCollision = collision;
		MeshIntersectRay(rayOrigin, endRayNormal, backfaceCulling);
		CollisionData endCollision = collision;
		if (startCollision.CollisionDetected && endCollision.CollisionDetected)
		{
			float length = (endCollision.Position - startCollision.Position).Length();
			float position = stepLength;
			collisionsList.Add(startCollision);
			for (; position < length; position += stepLength)
			{
				float factor = position / length;
				Vector3 rayNormal = (startCollision.Position * (1f - factor) + endCollision.Position * factor - rayOrigin).Normalized();
				MeshIntersectRay(rayOrigin, rayNormal, backfaceCulling);
				collisionsList.Add(collision);
			}
			collisionsList.Add(endCollision);
		}
	}
}
