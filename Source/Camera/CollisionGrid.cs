using System.Collections.Generic;
using Godot;

public class CollisionGrid
{
	public struct Cell
	{
		public bool Initialized;

		public Data WorksheetData;

		public List<int> TrianglesIndicesList;

		public void AddTriangle(int i)
		{
			TrianglesIndicesList.Add(i);
		}

		public void Intersect(Vector3 rayOrigin, Vector3 rayNormal, ref CollisionManager.CollisionData collision, bool backfaceCulling = true)
		{
			PreviewspaceMeshManager previewMeshManager = Register.PreviewspaceMeshManager;
			collision.CollisionDetected = false;
			float tMin = float.PositiveInfinity;
			for (int j = 0; j < TrianglesIndicesList.Count; j++)
			{
				int i = TrianglesIndicesList[j];
				Vector3 n = previewMeshManager.CollisionTrianglesList[i / 3].Normal;
				float d = n.Dot(rayNormal);
				if (d == 0f || (backfaceCulling && d < 0f))
				{
					continue;
				}
				Vector3 v1 = previewMeshManager.CollisionVerticesList[previewMeshManager.IndicesList[i]];
				Vector3 p = rayOrigin - v1;
				d = 1f / d;
				float t = (0f - p.Dot(n)) * d;
				if (t > 0f && t < tMin)
				{
					Vector3 v2 = previewMeshManager.CollisionVerticesList[previewMeshManager.IndicesList[i + 1]];
					Vector3 v3 = previewMeshManager.CollisionVerticesList[previewMeshManager.IndicesList[i + 2]];
					Vector3 c = rayNormal.Cross(p);
					float s2 = (v3 - v1).Dot(c) * d;
					float s3 = (v1 - v2).Dot(c) * d;
					if (s2 > 0f && s3 > 0f && s2 + s3 < 1f)
					{
						tMin = t;
						float s4 = 1f - s2 - s3;
						collision.CollisionDetected = true;
						collision.Object = WorksheetData.Worksheet;
						collision.Position = v1 * s4 + v2 * s2 + v3 * s3;
						collision.Indices[0] = previewMeshManager.IndicesList[i];
						collision.Indices[1] = previewMeshManager.IndicesList[i + 1];
						collision.Indices[2] = previewMeshManager.IndicesList[i + 2];
						collision.UV = previewMeshManager.UVsList[collision.Indices[0]] * s4;
						collision.UV += previewMeshManager.UVsList[collision.Indices[1]] * s2;
						collision.UV += previewMeshManager.UVsList[collision.Indices[2]] * s3;
						collision.Normal = previewMeshManager.NormalsList[collision.Indices[0]] * s4;
						collision.Normal += previewMeshManager.NormalsList[collision.Indices[1]] * s2;
						collision.Normal += previewMeshManager.NormalsList[collision.Indices[2]] * s3;
						collision.Group = previewMeshManager.GroupList[collision.Indices[0]];
					}
				}
			}
		}
	}

	private readonly int[] map = new int[8] { 2, 1, 2, 1, 2, 2, 0, 0 };

	private PreviewspaceMeshManager previewMeshManager;

	private int totalNumberTriangles;

	private Vector3i resolution;

	private Vector3 cellSize;

	private Cell[,,] cellsArray;

	private AABB aabb;

	public int TotalNumberTriangles => totalNumberTriangles;

	public Vector3i Resolution => resolution;

	public Vector3 CellSize => cellSize;

	public AABB AABB => aabb;

	public CollisionGrid()
	{
		previewMeshManager = Register.PreviewspaceMeshManager;
		aabb = previewMeshManager.CollisionAABB;
		totalNumberTriangles = previewMeshManager.TrianglesCount;
		float volume = Mathf.Max(aabb.Size.x, 1f) * Mathf.Max(aabb.Size.y, 1f) * Mathf.Max(aabb.Size.z, 1f);
		float cubeRoot = Mathf.Pow((float)totalNumberTriangles / volume, 1f / 3f);
		for (int i = 0; i < 3; i++)
		{
			resolution[i] = Mathf.FloorToInt(aabb.Size[i] * cubeRoot);
			if (resolution[i] < 1)
			{
				resolution[i] = 1;
			}
			if (resolution[i] > 128)
			{
				resolution[i] = 128;
			}
		}
		cellSize = aabb.Size / resolution.ToFloat();
		cellsArray = new Cell[resolution.x, resolution.y, resolution.z];
		for (int y = 0; y < resolution.y; y++)
		{
			for (int z = 0; z < resolution.z; z++)
			{
				for (int x = 0; x < resolution.x; x++)
				{
					cellsArray[x, y, z].Initialized = false;
				}
			}
		}
		for (int j = 0; j < previewMeshManager.IndicesList.Count; j += 3)
		{
			Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
			Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
			Vector3 v1 = previewMeshManager.CollisionVerticesList[previewMeshManager.IndicesList[j]];
			Vector3 v2 = previewMeshManager.CollisionVerticesList[previewMeshManager.IndicesList[j + 1]];
			Vector3 v3 = previewMeshManager.CollisionVerticesList[previewMeshManager.IndicesList[j + 2]];
			for (int k = 0; k < 3; k++)
			{
				min[k] = Mathf.Min(min[k], v1[k]);
				min[k] = Mathf.Min(min[k], v2[k]);
				min[k] = Mathf.Min(min[k], v3[k]);
				max[k] = Mathf.Max(max[k], v1[k]);
				max[k] = Mathf.Max(max[k], v2[k]);
				max[k] = Mathf.Max(max[k], v3[k]);
			}
			min = (min - aabb.Position) / cellSize;
			max = (max - aabb.Position) / cellSize;
			int num = Mathf.Clamp(Mathf.FloorToInt(min[2]), 0, resolution[2] - 1);
			int zmax = Mathf.Clamp(Mathf.FloorToInt(max[2]), 0, resolution[2] - 1);
			int ymin = Mathf.Clamp(Mathf.FloorToInt(min[1]), 0, resolution[1] - 1);
			int ymax = Mathf.Clamp(Mathf.FloorToInt(max[1]), 0, resolution[1] - 1);
			int xmin = Mathf.Clamp(Mathf.FloorToInt(min[0]), 0, resolution[0] - 1);
			int xmax = Mathf.Clamp(Mathf.FloorToInt(max[0]), 0, resolution[0] - 1);
			for (int l = num; l <= zmax; l++)
			{
				for (int m = ymin; m <= ymax; m++)
				{
					for (int n = xmin; n <= xmax; n++)
					{
						if (!cellsArray[n, m, l].Initialized)
						{
							cellsArray[n, m, l].WorksheetData = Register.Workspace.Worksheet.Data;
							cellsArray[n, m, l].TrianglesIndicesList = new List<int>();
							cellsArray[n, m, l].Initialized = true;
						}
						cellsArray[n, m, l].AddTriangle(j);
					}
				}
			}
		}
	}

	public CollisionManager.CollisionData Intersect(Vector3 rayOrigin, Vector3 rayNormal, bool backfaceCulling = true, float distance = float.PositiveInfinity)
	{
		CollisionManager.CollisionData collision = CollisionManager.CollisionData.Default;
		Vector3 invRayNormal = Vector3.One / rayNormal;
		bool num = rayNormal.x < 0f;
		bool signY = rayNormal.y < 0f;
		bool signZ = rayNormal.z < 0f;
		float tmin = ((num ? aabb.End.x : aabb.Position.x) - rayOrigin.x) * invRayNormal.x;
		float tmax = ((num ? aabb.Position.x : aabb.End.x) - rayOrigin.x) * invRayNormal.x;
		float tymin = ((signY ? aabb.End.y : aabb.Position.y) - rayOrigin.y) * invRayNormal.y;
		float tymax = ((signY ? aabb.Position.y : aabb.End.y) - rayOrigin.y) * invRayNormal.y;
		float tzmin = ((signZ ? aabb.End.z : aabb.Position.z) - rayOrigin.z) * invRayNormal.z;
		float tzmax = ((signZ ? aabb.Position.z : aabb.End.z) - rayOrigin.z) * invRayNormal.z;
		if (tmin > tymax || tymin > tmax)
		{
			return collision;
		}
		if (tymin > tmin)
		{
			tmin = tymin;
		}
		if (tymax < tmax)
		{
			tmax = tymax;
		}
		if (tmin > tzmax || tzmin > tmax)
		{
			return collision;
		}
		if (tzmin > tmin)
		{
			tmin = tzmin;
		}
		float tHitBox = tmin;
		Vector3i exit = Vector3i.Zero;
		Vector3i step = Vector3i.Zero;
		Vector3i cell = Vector3i.Zero;
		Vector3 deltaT = Vector3.Zero;
		Vector3 nextCrossingT = Vector3.Zero;
		for (int i = 0; i < 3; i++)
		{
			float rayOrigCell = rayOrigin[i] + rayNormal[i] * tHitBox - aabb.Position[i];
			cell[i] = Mathf.Clamp(Mathf.FloorToInt(rayOrigCell / cellSize[i]), 0, resolution[i] - 1);
			if (rayNormal[i] < 0f)
			{
				deltaT[i] = (0f - cellSize[i]) * invRayNormal[i];
				nextCrossingT[i] = tHitBox + ((float)cell[i] * cellSize[i] - rayOrigCell) * invRayNormal[i];
				exit[i] = -1;
				step[i] = -1;
			}
			else
			{
				deltaT[i] = cellSize[i] * invRayNormal[i];
				nextCrossingT[i] = tHitBox + ((float)(cell[i] + 1) * cellSize[i] - rayOrigCell) * invRayNormal[i];
				exit[i] = resolution[i];
				step[i] = 1;
			}
		}
		while ((cell.ToFloat() * cellSize + cellSize * 0.5f + aabb.Position - rayOrigin).Length() <= 1.5f * distance)
		{
			if (cellsArray[cell.x, cell.y, cell.z].Initialized)
			{
				cellsArray[cell.x, cell.y, cell.z].Intersect(rayOrigin, rayNormal, ref collision, backfaceCulling);
			}
			int k = (int)((((nextCrossingT[0] < nextCrossingT[1]) ? 1u : 0u) << 2) + (((nextCrossingT[0] < nextCrossingT[2]) ? 1u : 0u) << 1)) + ((nextCrossingT[1] < nextCrossingT[2]) ? 1 : 0);
			int axis = map[k];
			if (collision.CollisionDetected)
			{
				break;
			}
			cell[axis] += step[axis];
			if (cell[axis] == exit[axis])
			{
				break;
			}
			nextCrossingT[axis] += deltaT[axis];
		}
		if (!collision.CollisionDetected)
		{
			collision.Clear();
		}
		return collision;
	}

	public Cell GetCell(int x, int y, int z)
	{
		return cellsArray[x, y, z];
	}
}
