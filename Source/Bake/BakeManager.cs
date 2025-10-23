using System;
using System.Collections.Generic;
using Godot;

public class BakeManager : Node
{
	public enum RayModeEnum
	{
		RANDOM,
		UNIFORM
	}

	public struct PixelAffiliation
	{
		public List<int> TrianglesIndicesList;
	}

	private class BakeVector
	{
		public Vector3 Position = Vector3.Zero;

		public Vector3 Normal = Vector3.Zero;

		public float Curvature;

		public List<int> IndicesList;

		public Dictionary<Vector3, BakeVector> Neighbor = new Dictionary<Vector3, BakeVector>();
	}

	private struct BakeEdge
	{
		public Vector3 Start;

		public Vector3 End;
	}

	private ThreadsManager threadsManager;

	private PreviewspaceMeshManager previewMeshManager;

	private DrawingManager drawingManager;

	private static Worksheet worksheet = null;

	private int width;

	private int height;

	private Vector2 pixelSize = Vector2.Zero;

	private PixelAffiliation[,] pixelAffiliations;

	private static float normalFactor = 1f;

	private static RayModeEnum rayMode = RayModeEnum.RANDOM;

	private static int rayCount = 32;

	private static float rayOffset = 0.025f;

	private static float rayDistance = 1.5f;

	private static float intensity = 1f;

	private static Vector3 lightPosition = Vector3.Zero;

	private static float lightRange = 5f;

	public PixelAffiliation[,] PixelAffiliations => pixelAffiliations;

	public BakeManager()
	{
		Register.BakeManager = this;
	}

	public BakeManager(Workspace workspace)
	{
		Register.BakeManager = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "BakeManager";
	}

	public override void _Ready()
	{
		base._Ready();
		previewMeshManager = Register.PreviewspaceMeshManager;
		drawingManager = Register.DrawingManager;
		threadsManager = Register.ThreadsManager;
	}

	public void Update(Worksheet worksheet)
	{
		BakeManager.worksheet = worksheet;
		width = BakeManager.worksheet.Data.Width;
		height = BakeManager.worksheet.Data.Height;
		pixelSize = new Vector2(1f / (float)width, 1f / (float)height);
		threadsManager.Update(worksheet);
		UpdatePixelAffiliations();
	}

	public void Abort()
	{
		threadsManager.Abort();
	}

	public bool ConservativeTriangleProbe(int x, int y, Vector2 uv1, Vector2 uv2, Vector2 uv3, out float l1, out float l2, out float l3)
	{
		Vector2 d13 = uv1 - uv3;
		Vector2 d21 = uv2 - uv1;
		Vector2 d32 = uv3 - uv2;
		Vector2 uv4 = default(Vector2);
		uv4.x = (float)x * pixelSize.x;
		uv4.y = (float)y * pixelSize.y;
		float det = 1f / (d32.x * d13.y - d32.y * d13.x);
		l1 = det * (d32.x * (uv4.y + 0.5f * pixelSize.y - uv3.y) - d32.y * (uv4.x + 0.5f * pixelSize.x - uv3.x));
		l2 = det * (d13.x * (uv4.y + 0.5f * pixelSize.y - uv3.y) - d13.y * (uv4.x + 0.5f * pixelSize.x - uv3.x));
		l3 = 1f - l1 - l2;
		if (l1 >= 0f && l2 >= 0f && l3 >= 0f)
		{
			return true;
		}
		float t = ((d13.y == 0f) ? (-1f) : ((uv4.y - uv3.y) / d13.y));
		float s = uv3.x - uv4.x + t * d13.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d13.y == 0f) ? (-1f) : ((uv4.y + pixelSize.y - uv3.y) / d13.y));
		s = uv3.x - uv4.x + t * d13.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d13.x == 0f) ? (-1f) : ((uv4.x - uv3.x) / d13.x));
		s = uv3.y - uv4.y + t * d13.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		t = ((d13.x == 0f) ? (-1f) : ((uv4.x + pixelSize.x - uv3.x) / d13.x));
		s = uv3.y - uv4.y + t * d13.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		t = ((d21.y == 0f) ? (-1f) : ((uv4.y - uv1.y) / d21.y));
		s = uv1.x - uv4.x + t * d21.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d21.y == 0f) ? (-1f) : ((uv4.y + pixelSize.y - uv1.y) / d21.y));
		s = uv1.x - uv4.x + t * d21.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d21.x == 0f) ? (-1f) : ((uv4.x - uv1.x) / d21.x));
		s = uv1.y - uv4.y + t * d21.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		t = ((d21.x == 0f) ? (-1f) : ((uv4.x + pixelSize.x - uv1.x) / d21.x));
		s = uv1.y - uv4.y + t * d21.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		t = ((d32.y == 0f) ? (-1f) : ((uv4.y - uv2.y) / d32.y));
		s = uv2.x - uv4.x + t * d32.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d32.y == 0f) ? (-1f) : ((uv4.y + pixelSize.y - uv2.y) / d32.y));
		s = uv2.x - uv4.x + t * d32.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d32.x == 0f) ? (-1f) : ((uv4.x - uv2.x) / d32.x));
		s = uv2.y - uv4.y + t * d32.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		t = ((d32.x == 0f) ? (-1f) : ((uv4.x + pixelSize.x - uv2.x) / d32.x));
		s = uv2.y - uv4.y + t * d32.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		return false;
	}

	public bool ConservativeTriangleProbe(int x, int y, Vector2 uv1, Vector2 uv2, Vector2 uv3, out float l1, out float l2, out float l3, float margin)
	{
		Vector2 d13 = uv1 - uv3;
		Vector2 d21 = uv2 - uv1;
		Vector2 d32 = uv3 - uv2;
		Vector2 n13 = d13.Normalized();
		Vector2 n21 = d21.Normalized();
		Vector2 n32 = d32.Normalized();
		uv1 += margin * pixelSize * (-n21 + n13) / Mathf.Sqrt(1f - n21.Dot(n13) * n21.Dot(n13));
		uv2 += margin * pixelSize * (-n32 + n21) / Mathf.Sqrt(1f - n32.Dot(n21) * n32.Dot(n21));
		uv3 += margin * pixelSize * (-n13 + n32) / Mathf.Sqrt(1f - n13.Dot(n32) * n13.Dot(n32));
		d13 = uv1 - uv3;
		d21 = uv2 - uv1;
		d32 = uv3 - uv2;
		Vector2 uv4 = default(Vector2);
		uv4.x = (float)x * pixelSize.x;
		uv4.y = (float)y * pixelSize.y;
		float det = 1f / (d32.x * d13.y - d32.y * d13.x);
		l1 = det * (d32.x * (uv4.y + 0.5f * pixelSize.y - uv3.y) - d32.y * (uv4.x + 0.5f * pixelSize.x - uv3.x));
		l2 = det * (d13.x * (uv4.y + 0.5f * pixelSize.y - uv3.y) - d13.y * (uv4.x + 0.5f * pixelSize.x - uv3.x));
		l3 = 1f - l1 - l2;
		if (l1 >= 0f && l2 >= 0f && l3 >= 0f)
		{
			return true;
		}
		float t = ((d13.y == 0f) ? (-1f) : ((uv4.y - uv3.y) / d13.y));
		float s = uv3.x - uv4.x + t * d13.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d13.y == 0f) ? (-1f) : ((uv4.y + pixelSize.y - uv3.y) / d13.y));
		s = uv3.x - uv4.x + t * d13.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d13.x == 0f) ? (-1f) : ((uv4.x - uv3.x) / d13.x));
		s = uv3.y - uv4.y + t * d13.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		t = ((d13.x == 0f) ? (-1f) : ((uv4.x + pixelSize.x - uv3.x) / d13.x));
		s = uv3.y - uv4.y + t * d13.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		t = ((d21.y == 0f) ? (-1f) : ((uv4.y - uv1.y) / d21.y));
		s = uv1.x - uv4.x + t * d21.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d21.y == 0f) ? (-1f) : ((uv4.y + pixelSize.y - uv1.y) / d21.y));
		s = uv1.x - uv4.x + t * d21.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d21.x == 0f) ? (-1f) : ((uv4.x - uv1.x) / d21.x));
		s = uv1.y - uv4.y + t * d21.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		t = ((d21.x == 0f) ? (-1f) : ((uv4.x + pixelSize.x - uv1.x) / d21.x));
		s = uv1.y - uv4.y + t * d21.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		t = ((d32.y == 0f) ? (-1f) : ((uv4.y - uv2.y) / d32.y));
		s = uv2.x - uv4.x + t * d32.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d32.y == 0f) ? (-1f) : ((uv4.y + pixelSize.y - uv2.y) / d32.y));
		s = uv2.x - uv4.x + t * d32.x;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.x)
		{
			return true;
		}
		t = ((d32.x == 0f) ? (-1f) : ((uv4.x - uv2.x) / d32.x));
		s = uv2.y - uv4.y + t * d32.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		t = ((d32.x == 0f) ? (-1f) : ((uv4.x + pixelSize.x - uv2.x) / d32.x));
		s = uv2.y - uv4.y + t * d32.y;
		if (t >= 0f && t <= 1f && s >= 0f && s <= pixelSize.y)
		{
			return true;
		}
		return false;
	}

	public void UpdatePixelAffiliations()
	{
		pixelAffiliations = new PixelAffiliation[width, height];
		Vector2[] uvs = new Vector2[3];
		Vector2i min = default(Vector2i);
		Vector2i max = default(Vector2i);
		for (int i = 0; i < previewMeshManager.CollisionTrianglesList.Count; i++)
		{
			uvs[0] = previewMeshManager.UVsList[previewMeshManager.CollisionTrianglesList[i].Indices[0]];
			uvs[1] = previewMeshManager.UVsList[previewMeshManager.CollisionTrianglesList[i].Indices[1]];
			uvs[2] = previewMeshManager.UVsList[previewMeshManager.CollisionTrianglesList[i].Indices[2]];
			min.x = Mathf.Max(Mathf.FloorToInt(Mathf.Min(Mathf.Min(uvs[0].x, uvs[1].x), uvs[2].x) * (float)width), 0);
			min.y = Mathf.Max(Mathf.FloorToInt(Mathf.Min(Mathf.Min(uvs[0].y, uvs[1].y), uvs[2].y) * (float)height), 0);
			max.x = Mathf.Min(Mathf.FloorToInt(Mathf.Max(Mathf.Max(uvs[0].x, uvs[1].x), uvs[2].x) * (float)width), width - 1);
			max.y = Mathf.Min(Mathf.FloorToInt(Mathf.Max(Mathf.Max(uvs[0].y, uvs[1].y), uvs[2].y) * (float)height), height - 1);
			for (int y = min.y; y <= max.y; y++)
			{
				for (int x = min.x; x <= max.x; x++)
				{
					if (ConservativeTriangleProbe(x, y, uvs[0], uvs[1], uvs[2], out var _, out var _, out var _, 0f))
					{
						if (pixelAffiliations[x, y].TrianglesIndicesList == null)
						{
							pixelAffiliations[x, y].TrianglesIndicesList = new List<int>();
						}
						pixelAffiliations[x, y].TrianglesIndicesList.Add(i);
					}
				}
			}
		}
	}

	public void BakeWireframe()
	{
		if (!previewMeshManager.IsMeshLoaded)
		{
			return;
		}
		string layerName = "Bake: Wireframe";
		((LayerCommand)worksheet.History.StartRecording(History.CommandTypeEnum.LAYER)).LayerCommandType = LayerCommand.LayerCommandTypeEnum.ADDNEW;
		worksheet.History.StopRecording("Layer Added [" + layerName + "]");
		Layer layer = worksheet.Data.Layer;
		layer.Name = layerName;
		layer.RoughnessChannel.IsVisible = false;
		layer.MetallicityChannel.IsVisible = false;
		layer.HeightChannel.IsVisible = false;
		layer.EmissionChannel.IsVisible = false;
		Vector2i[] uvsArray = new Vector2i[4];
		drawingManager.PushSettings();
		drawingManager.Color = Settings.AccentColor;
		drawingManager.ColorEnabled = true;
		drawingManager.ColorAlphaEnabled = true;
		drawingManager.RoughnessEnabled = false;
		drawingManager.MetallicityEnabled = false;
		drawingManager.HeightEnabled = false;
		drawingManager.EmissionEnabled = false;
		drawingManager.StartDrawing(worksheet, Vector2i.NegOne);
		for (int i = 0; i < previewMeshManager.TrianglesList.Count; i += 3)
		{
			uvsArray[0] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.TrianglesList[i]] / pixelSize);
			uvsArray[1] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.TrianglesList[i + 1]] / pixelSize);
			uvsArray[2] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.TrianglesList[i + 2]] / pixelSize);
			drawingManager.DrawLine(worksheet, uvsArray[0], uvsArray[1]);
			drawingManager.DrawLine(worksheet, uvsArray[1], uvsArray[2]);
			drawingManager.DrawLine(worksheet, uvsArray[2], uvsArray[0]);
		}
		for (int j = 0; j < previewMeshManager.QuadranglesList.Count; j += 4)
		{
			uvsArray[0] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.QuadranglesList[j]] / pixelSize);
			uvsArray[1] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.QuadranglesList[j + 1]] / pixelSize);
			uvsArray[2] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.QuadranglesList[j + 2]] / pixelSize);
			uvsArray[3] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.QuadranglesList[j + 3]] / pixelSize);
			drawingManager.DrawLine(worksheet, uvsArray[0], uvsArray[1]);
			drawingManager.DrawLine(worksheet, uvsArray[1], uvsArray[2]);
			drawingManager.DrawLine(worksheet, uvsArray[2], uvsArray[3]);
			drawingManager.DrawLine(worksheet, uvsArray[3], uvsArray[0]);
		}
		for (int k = 0; k < previewMeshManager.PolygonsList.Count; k++)
		{
			for (int ii = 0; ii < previewMeshManager.PolygonsList[k].Length - 1; ii++)
			{
				uvsArray[0] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.PolygonsList[k][ii]] / pixelSize);
				uvsArray[1] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.PolygonsList[k][ii + 1]] / pixelSize);
				drawingManager.DrawLine(worksheet, uvsArray[0], uvsArray[1]);
			}
			uvsArray[0] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.PolygonsList[k][previewMeshManager.PolygonsList[k].Length - 1]] / pixelSize);
			uvsArray[1] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.PolygonsList[k][0]] / pixelSize);
			drawingManager.DrawLine(worksheet, uvsArray[0], uvsArray[1]);
		}
		drawingManager.StopDrawing(worksheet, Vector2i.NegOne, "Wireframe Bake [" + worksheet.Data.Layer.Name + "]");
		drawingManager.PopSettings();
	}

	private Color[,] BakeFaces(int x, int y, int width, int height)
	{
		Color[,] outputArray = new Color[this.width, this.height];
		if (previewMeshManager.IsMeshLoaded)
		{
			int startX = Mathf.Max(0, x);
			int num = Mathf.Max(0, y);
			int endX = Mathf.Min(x + width, this.width - 1);
			int endY = Mathf.Min(y + height, this.height - 1);
			for (int yy = num; yy <= endY; yy++)
			{
				for (int xx = startX; xx <= endX; xx++)
				{
					if (pixelAffiliations[xx, yy].TrianglesIndicesList == null)
					{
						continue;
					}
					foreach (int t in pixelAffiliations[xx, yy].TrianglesIndicesList)
					{
						int ti0 = previewMeshManager.CollisionTrianglesList[t].Indices[0];
						int ti1 = previewMeshManager.CollisionTrianglesList[t].Indices[1];
						int ti2 = previewMeshManager.CollisionTrianglesList[t].Indices[2];
						if (ConservativeTriangleProbe(xx, yy, previewMeshManager.UVsList[ti0], previewMeshManager.UVsList[ti1], previewMeshManager.UVsList[ti2], out var _, out var _, out var _))
						{
							Color color = (outputArray[xx, yy] = Color.FromHsv(Mathf.Sin(1f * (float)t) * 0.5f + 0.5f, 1f, 0.5f + Mathf.Cos(1f * (float)t - 45416f) * 0.25f + 0.25f));
							if (xx > 0 && pixelAffiliations[xx - 1, yy].TrianglesIndicesList == null)
							{
								outputArray[xx - 1, yy] = color;
							}
							if (xx < width - 1 && pixelAffiliations[xx + 1, yy].TrianglesIndicesList == null)
							{
								outputArray[xx + 1, yy] = color;
							}
							if (yy > 0 && pixelAffiliations[xx, yy - 1].TrianglesIndicesList == null)
							{
								outputArray[xx, yy - 1] = color;
							}
							if (yy < height - 1 && pixelAffiliations[xx, yy + 1].TrianglesIndicesList == null)
							{
								outputArray[xx, yy + 1] = color;
							}
							if (xx > 0 && yy > 0 && pixelAffiliations[xx - 1, yy - 1].TrianglesIndicesList == null)
							{
								outputArray[xx - 1, yy - 1] = color;
							}
							if (xx < width - 1 && yy > 0 && pixelAffiliations[xx + 1, yy - 1].TrianglesIndicesList == null)
							{
								outputArray[xx + 1, yy - 1] = color;
							}
							if (xx > 0 && yy < height - 1 && pixelAffiliations[xx - 1, yy + 1].TrianglesIndicesList == null)
							{
								outputArray[xx - 1, yy + 1] = color;
							}
							if (xx < width - 1 && yy < height - 1 && pixelAffiliations[xx + 1, yy + 1].TrianglesIndicesList == null)
							{
								outputArray[xx + 1, yy + 1] = color;
							}
						}
					}
				}
			}
			for (int i = 0; i < this.height; i++)
			{
				for (int j = 0; j < this.width; j++)
				{
					if (outputArray[j, i].a == 0f)
					{
						outputArray[j, i] = Settings.BlankColor;
					}
				}
			}
		}
		return outputArray;
	}

	public Color[,] BakeFaces()
	{
		return BakeFaces(0, 0, width, height);
	}

	private Color[,] BakeIslands(int x, int y, int width, int height)
	{
		Color[,] outputArray = new Color[this.width, this.height];
		if (previewMeshManager.IsMeshLoaded)
		{
			int startX = Mathf.Max(0, x);
			int num = Mathf.Max(0, y);
			int endX = Mathf.Min(x + width, this.width - 1);
			int endY = Mathf.Min(y + height, this.height - 1);
			for (int yy = num; yy <= endY; yy++)
			{
				for (int xx = startX; xx <= endX; xx++)
				{
					if (pixelAffiliations[xx, yy].TrianglesIndicesList == null)
					{
						continue;
					}
					foreach (int t in pixelAffiliations[xx, yy].TrianglesIndicesList)
					{
						int ti0 = previewMeshManager.CollisionTrianglesList[t].Indices[0];
						int ti1 = previewMeshManager.CollisionTrianglesList[t].Indices[1];
						int ti2 = previewMeshManager.CollisionTrianglesList[t].Indices[2];
						if (ConservativeTriangleProbe(xx, yy, previewMeshManager.UVsList[ti0], previewMeshManager.UVsList[ti1], previewMeshManager.UVsList[ti2], out var _, out var _, out var _, 0f))
						{
							Color color = (outputArray[xx, yy] = Color.FromHsv(Mathf.Sin(1f * (float)previewMeshManager.GroupList[ti0]) * 0.5f + 0.5f, 1f, 0.5f + Mathf.Cos(1f * (float)previewMeshManager.GroupList[ti0] - 45416f) * 0.25f + 0.25f));
							if (xx > 0 && pixelAffiliations[xx - 1, yy].TrianglesIndicesList == null)
							{
								outputArray[xx - 1, yy] = color;
							}
							if (xx < width - 1 && pixelAffiliations[xx + 1, yy].TrianglesIndicesList == null)
							{
								outputArray[xx + 1, yy] = color;
							}
							if (yy > 0 && pixelAffiliations[xx, yy - 1].TrianglesIndicesList == null)
							{
								outputArray[xx, yy - 1] = color;
							}
							if (yy < height - 1 && pixelAffiliations[xx, yy + 1].TrianglesIndicesList == null)
							{
								outputArray[xx, yy + 1] = color;
							}
							if (xx > 0 && yy > 0 && pixelAffiliations[xx - 1, yy - 1].TrianglesIndicesList == null)
							{
								outputArray[xx - 1, yy - 1] = color;
							}
							if (xx < width - 1 && yy > 0 && pixelAffiliations[xx + 1, yy - 1].TrianglesIndicesList == null)
							{
								outputArray[xx + 1, yy - 1] = color;
							}
							if (xx > 0 && yy < height - 1 && pixelAffiliations[xx - 1, yy + 1].TrianglesIndicesList == null)
							{
								outputArray[xx - 1, yy + 1] = color;
							}
							if (xx < width - 1 && yy < height - 1 && pixelAffiliations[xx + 1, yy + 1].TrianglesIndicesList == null)
							{
								outputArray[xx + 1, yy + 1] = color;
							}
						}
					}
				}
			}
			for (int i = 0; i < this.height; i++)
			{
				for (int j = 0; j < this.width; j++)
				{
					if (outputArray[j, i].a == 0f)
					{
						outputArray[j, i] = Settings.BlankColor;
					}
				}
			}
		}
		return outputArray;
	}

	public Color[,] BakeIslands()
	{
		return BakeIslands(0, 0, width, height);
	}

	private void BakeOnIsland(int x, int y, int width, int height, int group)
	{
		if (!previewMeshManager.IsMeshLoaded)
		{
			return;
		}
		drawingManager.StartDrawing(worksheet, Vector2i.NegOne);
		int startX = Mathf.Max(0, x);
		int num = Mathf.Max(0, y);
		int endX = Mathf.Min(x + width, this.width - 1);
		int endY = Mathf.Min(y + height, this.height - 1);
		for (int yy = num; yy <= endY; yy++)
		{
			for (int xx = startX; xx <= endX; xx++)
			{
				if (pixelAffiliations[xx, yy].TrianglesIndicesList == null)
				{
					continue;
				}
				foreach (int t in pixelAffiliations[xx, yy].TrianglesIndicesList)
				{
					int ti0 = previewMeshManager.CollisionTrianglesList[t].Indices[0];
					if (previewMeshManager.GroupList[ti0] != group)
					{
						continue;
					}
					int ti1 = previewMeshManager.CollisionTrianglesList[t].Indices[1];
					int ti2 = previewMeshManager.CollisionTrianglesList[t].Indices[2];
					if (ConservativeTriangleProbe(xx, yy, previewMeshManager.UVsList[ti0], previewMeshManager.UVsList[ti1], previewMeshManager.UVsList[ti2], out var _, out var _, out var _, 0f))
					{
						Register.DrawingManager.BucketTool.DrawPixel(worksheet, new Vector2i(xx, yy));
						if (xx > 0 && pixelAffiliations[xx - 1, yy].TrianglesIndicesList == null)
						{
							Register.DrawingManager.BucketTool.DrawPixel(worksheet, new Vector2i(xx - 1, yy));
						}
						if (xx < width - 1 && pixelAffiliations[xx + 1, yy].TrianglesIndicesList == null)
						{
							Register.DrawingManager.BucketTool.DrawPixel(worksheet, new Vector2i(xx + 1, yy));
						}
						if (yy > 0 && pixelAffiliations[xx, yy - 1].TrianglesIndicesList == null)
						{
							Register.DrawingManager.BucketTool.DrawPixel(worksheet, new Vector2i(xx, yy - 1));
						}
						if (yy < height - 1 && pixelAffiliations[xx, yy + 1].TrianglesIndicesList == null)
						{
							Register.DrawingManager.BucketTool.DrawPixel(worksheet, new Vector2i(xx, yy + 1));
						}
						if (xx > 0 && yy > 0 && pixelAffiliations[xx - 1, yy - 1].TrianglesIndicesList == null)
						{
							Register.DrawingManager.BucketTool.DrawPixel(worksheet, new Vector2i(xx - 1, yy - 1));
						}
						if (xx < width - 1 && yy > 0 && pixelAffiliations[xx + 1, yy - 1].TrianglesIndicesList == null)
						{
							Register.DrawingManager.BucketTool.DrawPixel(worksheet, new Vector2i(xx + 1, yy - 1));
						}
						if (xx > 0 && yy < height - 1 && pixelAffiliations[xx - 1, yy + 1].TrianglesIndicesList == null)
						{
							Register.DrawingManager.BucketTool.DrawPixel(worksheet, new Vector2i(xx - 1, yy + 1));
						}
						if (xx < width - 1 && yy < height - 1 && pixelAffiliations[xx + 1, yy + 1].TrianglesIndicesList == null)
						{
							Register.DrawingManager.BucketTool.DrawPixel(worksheet, new Vector2i(xx + 1, yy + 1));
						}
					}
				}
			}
		}
		drawingManager.StopDrawing(worksheet, Vector2i.NegOne, "Bucket [Island (" + group + ")]");
	}

	public void BakeOnIsland(int group)
	{
		BakeOnIsland(0, 0, width, height, group);
	}

	private Color[,] BakePosition(int x, int y, int width, int height)
	{
		Color[,] outputArray = new Color[this.width, this.height];
		if (previewMeshManager.IsMeshLoaded)
		{
			Vector3[] vertices = new Vector3[3];
			int startX = Mathf.Max(0, x);
			int num = Mathf.Max(0, y);
			int endX = Mathf.Min(x + width, this.width - 1);
			int endY = Mathf.Min(y + height, this.height - 1);
			for (int yy = num; yy <= endY; yy++)
			{
				for (int xx = startX; xx <= endX; xx++)
				{
					if (pixelAffiliations[xx, yy].TrianglesIndicesList == null)
					{
						continue;
					}
					foreach (int t in pixelAffiliations[xx, yy].TrianglesIndicesList)
					{
						int ti0 = previewMeshManager.CollisionTrianglesList[t].Indices[0];
						int ti1 = previewMeshManager.CollisionTrianglesList[t].Indices[1];
						int ti2 = previewMeshManager.CollisionTrianglesList[t].Indices[2];
						if (ConservativeTriangleProbe(xx, yy, previewMeshManager.UVsList[ti0], previewMeshManager.UVsList[ti1], previewMeshManager.UVsList[ti2], out var l0, out var l1, out var l2))
						{
							vertices[0] = previewMeshManager.CollisionVerticesList[ti0];
							vertices[1] = previewMeshManager.CollisionVerticesList[ti1];
							vertices[2] = previewMeshManager.CollisionVerticesList[ti2];
							Vector3 vector = vertices[0] * l0 + vertices[1] * l1 + vertices[2] * l2;
							float r = (vector.x - previewMeshManager.CollisionAABB.Position.x) / previewMeshManager.CollisionAABB.Size.x;
							float g = (vector.y - previewMeshManager.CollisionAABB.Position.y) / previewMeshManager.CollisionAABB.Size.y;
							float b = (vector.z - previewMeshManager.CollisionAABB.Position.z) / previewMeshManager.CollisionAABB.Size.z;
							outputArray[xx, yy] = new Color(r, g, b);
						}
					}
				}
			}
			for (int i = 0; i < this.height; i++)
			{
				for (int j = 0; j < this.width; j++)
				{
					if (outputArray[j, i].a == 0f)
					{
						outputArray[j, i] = Settings.BlankColor;
					}
				}
			}
		}
		return outputArray;
	}

	public Color[,] BakePosition()
	{
		return BakePosition(0, 0, width, height);
	}

	private Color[,] BakeNormal(int x, int y, int width, int height)
	{
		Color[,] outputArray = new Color[this.width, this.height];
		if (previewMeshManager.IsMeshLoaded)
		{
			Vector3[] normals = new Vector3[3];
			int startX = Mathf.Max(0, x);
			int num = Mathf.Max(0, y);
			int endX = Mathf.Min(x + width, this.width - 1);
			int endY = Mathf.Min(y + height, this.height - 1);
			for (int yy = num; yy <= endY; yy++)
			{
				for (int xx = startX; xx <= endX; xx++)
				{
					if (pixelAffiliations[xx, yy].TrianglesIndicesList == null)
					{
						continue;
					}
					foreach (int t in pixelAffiliations[xx, yy].TrianglesIndicesList)
					{
						int ti0 = previewMeshManager.CollisionTrianglesList[t].Indices[0];
						int ti1 = previewMeshManager.CollisionTrianglesList[t].Indices[1];
						int ti2 = previewMeshManager.CollisionTrianglesList[t].Indices[2];
						if (ConservativeTriangleProbe(xx, yy, previewMeshManager.UVsList[ti0], previewMeshManager.UVsList[ti1], previewMeshManager.UVsList[ti2], out var l0, out var l1, out var l2))
						{
							normals[0] = previewMeshManager.NormalsList[ti0];
							normals[1] = previewMeshManager.NormalsList[ti1];
							normals[2] = previewMeshManager.NormalsList[ti2];
							Vector3 worldNormal = (normals[0] * l0 + normals[1] * l1 + normals[2] * l2).Normalized();
							float r = 0.5f + worldNormal.x * 0.5f;
							float g = 0.5f + worldNormal.y * 0.5f;
							float b = 0.5f + worldNormal.z * 0.5f;
							outputArray[xx, yy] = new Color(r, g, b);
						}
					}
				}
			}
			for (int i = 0; i < this.height; i++)
			{
				for (int j = 0; j < this.width; j++)
				{
					if (outputArray[j, i].a == 0f)
					{
						outputArray[j, i] = Settings.BlankColor;
					}
				}
			}
		}
		return outputArray;
	}

	public Color[,] BakeNormal()
	{
		return BakeNormal(0, 0, width, height);
	}

	private Color[,] BakeDetailedNormal(int x, int y, int width, int height)
	{
		Color[,] outputArray = new Color[this.width, this.height];
		if (previewMeshManager.IsMeshLoaded)
		{
			Basis basis = Basis.Identity;
			Vector3[] vertices = new Vector3[3];
			Vector3[] normals = new Vector3[3];
			Vector2[] uvs = new Vector2[3];
			int startX = Mathf.Max(0, x);
			int num = Mathf.Max(0, y);
			int endX = Mathf.Min(x + width, this.width - 1);
			int endY = Mathf.Min(y + height, this.height - 1);
			for (int yy = num; yy <= endY; yy++)
			{
				for (int xx = startX; xx <= endX; xx++)
				{
					if (pixelAffiliations[xx, yy].TrianglesIndicesList == null)
					{
						continue;
					}
					foreach (int t in pixelAffiliations[xx, yy].TrianglesIndicesList)
					{
						int ti0 = previewMeshManager.CollisionTrianglesList[t].Indices[0];
						int ti1 = previewMeshManager.CollisionTrianglesList[t].Indices[1];
						int ti2 = previewMeshManager.CollisionTrianglesList[t].Indices[2];
						uvs[0] = previewMeshManager.UVsList[ti0];
						uvs[1] = previewMeshManager.UVsList[ti1];
						uvs[2] = previewMeshManager.UVsList[ti2];
						if (ConservativeTriangleProbe(xx, yy, uvs[0], uvs[1], uvs[2], out var l0, out var l1, out var l2))
						{
							vertices[0] = previewMeshManager.CollisionVerticesList[ti0];
							vertices[1] = previewMeshManager.CollisionVerticesList[ti1];
							vertices[2] = previewMeshManager.CollisionVerticesList[ti2];
							normals[0] = previewMeshManager.NormalsList[ti0];
							normals[1] = previewMeshManager.NormalsList[ti1];
							normals[2] = previewMeshManager.NormalsList[ti2];
							Vector3 tangent = previewMeshManager.CollisionTrianglesList[t].Tangent;
							Vector3 localNormal = new Vector3(worksheet.Data.NormalChannel[xx, yy].r, worksheet.Data.NormalChannel[xx, yy].g, worksheet.Data.NormalChannel[xx, yy].b) * 2f - Vector3.One;
							localNormal.x *= normalFactor;
							localNormal.y *= normalFactor;
							Vector3 worldNormal = (normals[0] * l0 + normals[1] * l1 + normals[2] * l2).Normalized();
							basis.x = tangent;
							basis.y = worldNormal.Cross(tangent);
							basis.z = worldNormal;
							worldNormal = basis.Xform(localNormal.Normalized()).Normalized();
							float r = 0.5f + worldNormal.x * 0.5f;
							float g = 0.5f + worldNormal.y * 0.5f;
							float b = 0.5f + worldNormal.z * 0.5f;
							outputArray[xx, yy] = new Color(r, g, b);
						}
					}
				}
			}
			for (int i = 0; i < this.height; i++)
			{
				for (int j = 0; j < this.width; j++)
				{
					if (outputArray[j, i].a == 0f)
					{
						outputArray[j, i] = Settings.BlankColor;
					}
				}
			}
		}
		return outputArray;
	}

	public Color[,] BakeDetailedNormal(float normalFactor)
	{
		BakeManager.normalFactor = normalFactor;
		return BakeDetailedNormal(0, 0, width, height);
	}

	private static float[,] BakeAmbientOcclusion(int x, int y, int width, int height)
	{
		BakeManager bakeManager = Register.BakeManager;
		PreviewspaceMeshManager previewMeshManager = Register.PreviewspaceMeshManager;
		int rayCount = BakeManager.rayCount;
		float rayOffset = BakeManager.rayOffset;
		float rayDistance = BakeManager.rayDistance;
		float[,] outputArray = new float[width + 1, height + 1];
		if (previewMeshManager.IsMeshLoaded)
		{
			CollisionManager.CollisionData collision = CollisionManager.CollisionData.Default;
			Vector3[] vertices = new Vector3[3];
			Vector2[] uvs = new Vector2[3];
			int startX = Mathf.Max(0, x);
			int startY = Mathf.Max(0, y);
			int endX = Mathf.Min(x + width, bakeManager.width - 1);
			int endY = Mathf.Min(y + height, bakeManager.height - 1);
			for (int yy = startY; yy <= endY; yy++)
			{
				for (int xx = startX; xx <= endX; xx++)
				{
					outputArray[xx - startX, yy - startY] = 1f;
					if (bakeManager.pixelAffiliations[xx, yy].TrianglesIndicesList == null)
					{
						continue;
					}
					foreach (int t in bakeManager.pixelAffiliations[xx, yy].TrianglesIndicesList)
					{
						int ti0 = previewMeshManager.CollisionTrianglesList[t].Indices[0];
						int ti1 = previewMeshManager.CollisionTrianglesList[t].Indices[1];
						int ti2 = previewMeshManager.CollisionTrianglesList[t].Indices[2];
						uvs[0] = previewMeshManager.UVsList[ti0];
						uvs[1] = previewMeshManager.UVsList[ti1];
						uvs[2] = previewMeshManager.UVsList[ti2];
						if (!bakeManager.ConservativeTriangleProbe(xx, yy, uvs[0], uvs[1], uvs[2], out var l0, out var l1, out var l2))
						{
							continue;
						}
						vertices[0] = previewMeshManager.CollisionVerticesList[ti0];
						vertices[1] = previewMeshManager.CollisionVerticesList[ti1];
						vertices[2] = previewMeshManager.CollisionVerticesList[ti2];
						Vector3 position = vertices[0] * l0 + vertices[1] * l1 + vertices[2] * l2;
						Vector3 worldNormal = -previewMeshManager.CollisionTrianglesList[t].Normal.Normalized();
						float length = 0f;
						for (int ray = 0; ray < rayCount; ray++)
						{
							Vector3 rayNormal = ((rayMode != RayModeEnum.UNIFORM) ? Vector3Extension.RandomDirection(worldNormal) : Vector3Extension.UniformDirection(worldNormal, ray, rayCount));
							Register.CollisionManager.MeshIntersectRay(ref collision, position + worldNormal * rayOffset, rayNormal, backfaceCulling: false);
							if (collision.CollisionDetected)
							{
								float currentLength = (collision.Position - (position + worldNormal * rayOffset)).Length();
								length = ((!(currentLength < rayDistance)) ? (length + rayDistance) : (length + currentLength));
							}
							else
							{
								length += rayDistance;
							}
						}
						outputArray[xx - startX, yy - startY] = Mathf.Clamp(length / (float)rayCount / rayDistance, 0f, 1f);
					}
				}
			}
		}
		return outputArray;
	}

	public void BakeAmbientOcclusion(Action<float[,], bool> threadCompleteCallback, RayModeEnum rayMode = RayModeEnum.RANDOM, int rayCount = 32, float rayOffset = 0.01f, float rayDistance = 1.5f, float intensity = 1f)
	{
		if (!threadsManager.IsBusy)
		{
			BakeManager.rayMode = rayMode;
			BakeManager.rayCount = rayCount;
			BakeManager.rayOffset = rayOffset;
			BakeManager.rayDistance = rayDistance;
			BakeManager.intensity = intensity;
			threadsManager.Start(BakeAmbientOcclusion, threadCompleteCallback);
		}
	}

	private static float[,] BakeThickness(int x, int y, int width, int height)
	{
		BakeManager bakeManager = Register.BakeManager;
		PreviewspaceMeshManager previewMeshManager = Register.PreviewspaceMeshManager;
		int rayCount = BakeManager.rayCount;
		float rayOffset = BakeManager.rayOffset;
		float rayDistance = BakeManager.rayDistance;
		float[,] outputArray = new float[width + 1, height + 1];
		if (previewMeshManager.IsMeshLoaded)
		{
			CollisionManager.CollisionData collision = CollisionManager.CollisionData.Default;
			Vector3[] vertices = new Vector3[3];
			Vector2[] uvs = new Vector2[3];
			int startX = Mathf.Max(0, x);
			int startY = Mathf.Max(0, y);
			int endX = Mathf.Min(x + width, bakeManager.width - 1);
			int endY = Mathf.Min(y + height, bakeManager.height - 1);
			for (int yy = startY; yy <= endY; yy++)
			{
				for (int xx = startX; xx <= endX; xx++)
				{
					outputArray[xx - startX, yy - startY] = 1f;
					if (bakeManager.pixelAffiliations[xx, yy].TrianglesIndicesList == null)
					{
						continue;
					}
					foreach (int t in bakeManager.pixelAffiliations[xx, yy].TrianglesIndicesList)
					{
						int ti0 = previewMeshManager.CollisionTrianglesList[t].Indices[0];
						int ti1 = previewMeshManager.CollisionTrianglesList[t].Indices[1];
						int ti2 = previewMeshManager.CollisionTrianglesList[t].Indices[2];
						uvs[0] = previewMeshManager.UVsList[ti0];
						uvs[1] = previewMeshManager.UVsList[ti1];
						uvs[2] = previewMeshManager.UVsList[ti2];
						if (!bakeManager.ConservativeTriangleProbe(xx, yy, uvs[0], uvs[1], uvs[2], out var l0, out var l1, out var l2))
						{
							continue;
						}
						vertices[0] = previewMeshManager.CollisionVerticesList[ti0];
						vertices[1] = previewMeshManager.CollisionVerticesList[ti1];
						vertices[2] = previewMeshManager.CollisionVerticesList[ti2];
						Vector3 position = vertices[0] * l0 + vertices[1] * l1 + vertices[2] * l2;
						Vector3 worldNormal = -previewMeshManager.CollisionTrianglesList[t].Normal.Normalized();
						float length = 0f;
						for (int ray = 0; ray < rayCount; ray++)
						{
							Vector3 rayNormal = ((rayMode != RayModeEnum.UNIFORM) ? (-Vector3Extension.RandomDirection(worldNormal)) : (-Vector3Extension.UniformDirection(worldNormal, ray, rayCount)));
							Register.CollisionManager.MeshIntersectRay(ref collision, position - worldNormal * rayOffset, rayNormal, backfaceCulling: false);
							if (collision.CollisionDetected)
							{
								float currentLength = (collision.Position - position).Length();
								length = ((!(currentLength < rayDistance)) ? (length + rayDistance) : (length + currentLength));
							}
							else
							{
								length += rayDistance;
							}
						}
						outputArray[xx - startX, yy - startY] = Mathf.Clamp(length / (float)rayCount / rayDistance, 0f, 1f);
					}
				}
			}
		}
		return outputArray;
	}

	public void BakeThickness(Action<float[,], bool> threadCompleteCallback, RayModeEnum rayMode = RayModeEnum.RANDOM, int rayCount = 32, float rayOffset = 0.01f, float rayDistance = 1.5f, float intensity = 1f)
	{
		if (!threadsManager.IsBusy)
		{
			BakeManager.rayMode = rayMode;
			BakeManager.rayCount = rayCount;
			BakeManager.rayOffset = rayOffset;
			BakeManager.rayDistance = rayDistance;
			BakeManager.intensity = intensity;
			threadsManager.Start(BakeThickness, threadCompleteCallback);
		}
	}

	public void DrawLine(Color[,] outputArray, int startX, int startY, int endX, int endY, float value = 1f)
	{
		Color color = new Color(value, value, value);
		if (Mathf.Abs(endX - startX) >= Mathf.Abs(endY - startY) && Mathf.Abs(endX - startX) > 0)
		{
			float increase = 1f * (float)(endY - startY) / (float)(endX - startX);
			if (startX < endX)
			{
				for (int x = startX; x <= endX; x++)
				{
					int y = (int)((float)startY + (float)(x - startX) * increase);
					outputArray[x, y] = color;
				}
			}
			else
			{
				for (int i = endX; i <= startX; i++)
				{
					int y2 = (int)((float)startY + (float)(i - startX) * increase);
					outputArray[i, y2] = color;
				}
			}
		}
		else
		{
			if (Mathf.Abs(endY - startY) <= 0)
			{
				return;
			}
			float increase = 1f * (float)(endX - startX) / (float)(endY - startY);
			if (startY < endY)
			{
				for (int j = startY; j <= endY; j++)
				{
					int x2 = (int)((float)startX + (float)(j - startY) * increase);
					outputArray[x2, j] = color;
				}
			}
			else
			{
				for (int k = endY; k <= startY; k++)
				{
					int x3 = (int)((float)startX + (float)(k - startY) * increase);
					outputArray[x3, k] = color;
				}
			}
		}
	}

	private Color[,] BakeCurvature(int x, int y, int width, int height)
	{
		Color[,] outputArray = new Color[this.width, this.height];
		if (previewMeshManager.IsMeshLoaded)
		{
			Dictionary<Vector3, BakeVector> bakeVectorDictionary = new Dictionary<Vector3, BakeVector>();
			for (int i = 0; i < previewMeshManager.IndicesList.Count; i++)
			{
				int index = previewMeshManager.IndicesList[i];
				Vector3 vertex = previewMeshManager.VerticesList[index];
				Vector3 normal = previewMeshManager.NormalsList[index];
				if (bakeVectorDictionary.ContainsKey(vertex))
				{
					bakeVectorDictionary[vertex].IndicesList.Add(i);
					bakeVectorDictionary[vertex].Normal += normal;
					continue;
				}
				BakeVector bakeVector = new BakeVector
				{
					Position = vertex,
					Normal = Vector3.Zero,
					IndicesList = new List<int> { i }
				};
				bakeVectorDictionary.Add(vertex, bakeVector);
			}
			foreach (KeyValuePair<Vector3, BakeVector> kvp in bakeVectorDictionary)
			{
				kvp.Value.Normal = kvp.Value.Normal.Normalized();
				for (int j = 0; j < kvp.Value.IndicesList.Count; j++)
				{
					switch (kvp.Value.IndicesList[j] % 3)
					{
					case 0:
					{
						int index2 = previewMeshManager.IndicesList[kvp.Value.IndicesList[j] + 1];
						Vector3 vertex2 = previewMeshManager.VerticesList[index2];
						if (!kvp.Value.Neighbor.ContainsKey(vertex2))
						{
							kvp.Value.Neighbor.Add(vertex2, bakeVectorDictionary[vertex2]);
						}
						index2 = previewMeshManager.IndicesList[kvp.Value.IndicesList[j] + 2];
						vertex2 = previewMeshManager.VerticesList[index2];
						if (!kvp.Value.Neighbor.ContainsKey(vertex2))
						{
							kvp.Value.Neighbor.Add(vertex2, bakeVectorDictionary[vertex2]);
						}
						break;
					}
					case 1:
					{
						int index2 = previewMeshManager.IndicesList[kvp.Value.IndicesList[j] - 1];
						Vector3 vertex2 = previewMeshManager.VerticesList[index2];
						if (!kvp.Value.Neighbor.ContainsKey(vertex2))
						{
							kvp.Value.Neighbor.Add(vertex2, bakeVectorDictionary[vertex2]);
						}
						index2 = previewMeshManager.IndicesList[kvp.Value.IndicesList[j] + 1];
						vertex2 = previewMeshManager.VerticesList[index2];
						if (!kvp.Value.Neighbor.ContainsKey(vertex2))
						{
							kvp.Value.Neighbor.Add(vertex2, bakeVectorDictionary[vertex2]);
						}
						break;
					}
					case 2:
					{
						int index2 = previewMeshManager.IndicesList[kvp.Value.IndicesList[j] - 2];
						Vector3 vertex2 = previewMeshManager.VerticesList[index2];
						if (!kvp.Value.Neighbor.ContainsKey(vertex2))
						{
							kvp.Value.Neighbor.Add(vertex2, bakeVectorDictionary[vertex2]);
						}
						index2 = previewMeshManager.IndicesList[kvp.Value.IndicesList[j] - 1];
						vertex2 = previewMeshManager.VerticesList[index2 - 1];
						if (!kvp.Value.Neighbor.ContainsKey(vertex2))
						{
							kvp.Value.Neighbor.Add(vertex2, bakeVectorDictionary[vertex2]);
						}
						break;
					}
					}
				}
			}
			BakeEdge bakeEdge1 = default(BakeEdge);
			BakeEdge bakeEdge2 = default(BakeEdge);
			Dictionary<BakeEdge, BakeVector[]> bakeEdgeDictionary = new Dictionary<BakeEdge, BakeVector[]>();
			foreach (KeyValuePair<Vector3, BakeVector> kvp2 in bakeVectorDictionary)
			{
				foreach (KeyValuePair<Vector3, BakeVector> nkvp in kvp2.Value.Neighbor)
				{
					bakeEdge1 = new BakeEdge
					{
						Start = nkvp.Key,
						End = kvp2.Key
					};
					bakeEdge2 = new BakeEdge
					{
						Start = kvp2.Key,
						End = nkvp.Key
					};
					if (!bakeEdgeDictionary.ContainsKey(bakeEdge1) && !bakeEdgeDictionary.ContainsKey(bakeEdge2))
					{
						bakeEdgeDictionary.Add(bakeEdge1, new BakeVector[2] { nkvp.Value, kvp2.Value });
					}
				}
				kvp2.Value.Curvature /= kvp2.Value.Neighbor.Count;
			}
			GD.Print(bakeEdgeDictionary.Count);
			Vector3[] normals = new Vector3[3];
			Vector3[] vertices = new Vector3[3];
			Vector2i[] uvs = new Vector2i[3];
			for (int k = 0; k < previewMeshManager.IndicesList.Count; k += 3)
			{
				vertices[0] = previewMeshManager.VerticesList[previewMeshManager.IndicesList[k]];
				vertices[1] = previewMeshManager.VerticesList[previewMeshManager.IndicesList[k + 1]];
				vertices[2] = previewMeshManager.VerticesList[previewMeshManager.IndicesList[k + 2]];
				normals[0] = previewMeshManager.NormalsList[previewMeshManager.IndicesList[k]];
				normals[1] = previewMeshManager.NormalsList[previewMeshManager.IndicesList[k + 1]];
				normals[2] = previewMeshManager.NormalsList[previewMeshManager.IndicesList[k + 2]];
				bakeEdge1.Start = vertices[0];
				bakeEdge1.End = vertices[1];
				bakeEdge2.Start = vertices[1];
				bakeEdge2.End = vertices[0];
				if (bakeEdgeDictionary.ContainsKey(bakeEdge1) || bakeEdgeDictionary.ContainsKey(bakeEdge2))
				{
					uvs[0] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.IndicesList[k]] / pixelSize);
					uvs[1] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.IndicesList[k + 1]] / pixelSize);
					DrawLine(outputArray, uvs[0].x, uvs[0].y, uvs[1].x, uvs[1].y);
				}
				bakeEdge1.Start = vertices[1];
				bakeEdge1.End = vertices[2];
				bakeEdge2.Start = vertices[2];
				bakeEdge2.End = vertices[1];
				if (bakeEdgeDictionary.ContainsKey(bakeEdge1) || bakeEdgeDictionary.ContainsKey(bakeEdge2))
				{
					uvs[1] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.IndicesList[k + 1]] / pixelSize);
					uvs[2] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.IndicesList[k + 2]] / pixelSize);
					DrawLine(outputArray, uvs[1].x, uvs[1].y, uvs[2].x, uvs[2].y);
				}
				bakeEdge1.Start = vertices[2];
				bakeEdge1.End = vertices[0];
				bakeEdge2.Start = vertices[0];
				bakeEdge2.End = vertices[2];
				if (bakeEdgeDictionary.ContainsKey(bakeEdge1) || bakeEdgeDictionary.ContainsKey(bakeEdge2))
				{
					uvs[0] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.IndicesList[k]] / pixelSize);
					uvs[2] = new Vector2i(previewMeshManager.UVsList[previewMeshManager.IndicesList[k + 2]] / pixelSize);
					DrawLine(outputArray, uvs[0].x, uvs[0].y, uvs[2].x, uvs[2].y);
				}
			}
			for (int yy = 0; yy < this.height; yy++)
			{
				for (int xx = 0; xx < this.width; xx++)
				{
					if (outputArray[xx, yy].a == 0f)
					{
						outputArray[xx, yy] = Settings.BlankColor;
					}
				}
			}
		}
		return outputArray;
	}

	public Color[,] BakeCurvature()
	{
		return BakeCurvature(0, 0, width, height);
	}

	private static float[,] BakeLight(int x, int y, int width, int height)
	{
		BakeManager bakeManager = Register.BakeManager;
		PreviewspaceMeshManager previewMeshManager = Register.PreviewspaceMeshManager;
		Vector3 lightPosition = BakeManager.lightPosition;
		float lightRange = BakeManager.lightRange;
		float rayOffset = BakeManager.rayOffset;
		float intensity = BakeManager.intensity;
		float[,] outputArray = new float[width + 1, height + 1];
		if (previewMeshManager.IsMeshLoaded)
		{
			CollisionManager.CollisionData collision = CollisionManager.CollisionData.Default;
			Basis basis = Basis.Identity;
			Vector3[] normals = new Vector3[3];
			Vector3[] vertices = new Vector3[3];
			Vector2[] uvs = new Vector2[3];
			if (intensity < 0.1f)
			{
				intensity = 0.1f;
			}
			int startX = Mathf.Max(0, x);
			int startY = Mathf.Max(0, y);
			int endX = Mathf.Min(x + width, bakeManager.width - 1);
			int endY = Mathf.Min(y + height, bakeManager.height - 1);
			rayCount = 1;
			for (int yy = startY; yy <= endY; yy++)
			{
				for (int xx = startX; xx <= endX; xx++)
				{
					outputArray[xx - startX, yy - startY] = 1f;
					if (bakeManager.pixelAffiliations[xx, yy].TrianglesIndicesList == null)
					{
						continue;
					}
					foreach (int t in bakeManager.pixelAffiliations[xx, yy].TrianglesIndicesList)
					{
						int ti0 = previewMeshManager.CollisionTrianglesList[t].Indices[0];
						int ti1 = previewMeshManager.CollisionTrianglesList[t].Indices[1];
						int ti2 = previewMeshManager.CollisionTrianglesList[t].Indices[2];
						uvs[0] = previewMeshManager.UVsList[ti0];
						uvs[1] = previewMeshManager.UVsList[ti1];
						uvs[2] = previewMeshManager.UVsList[ti2];
						if (!bakeManager.ConservativeTriangleProbe(xx, yy, uvs[0], uvs[1], uvs[2], out var l0, out var l1, out var l2))
						{
							continue;
						}
						vertices[0] = previewMeshManager.CollisionVerticesList[ti0];
						vertices[1] = previewMeshManager.CollisionVerticesList[ti1];
						vertices[2] = previewMeshManager.CollisionVerticesList[ti2];
						normals[0] = previewMeshManager.NormalsList[ti0];
						normals[1] = previewMeshManager.NormalsList[ti1];
						normals[2] = previewMeshManager.NormalsList[ti2];
						Vector3 tangent = previewMeshManager.CollisionTrianglesList[t].Tangent;
						Vector3 position = vertices[0] * l0 + vertices[1] * l1 + vertices[2] * l2;
						Vector3 worldNormal = -previewMeshManager.CollisionTrianglesList[t].Normal.Normalized();
						basis.x = tangent;
						basis.y = worldNormal.Cross(tangent);
						basis.z = worldNormal;
						Vector3 localNormal = new Vector3(worksheet.Data.NormalChannel.Array[xx, yy].r, worksheet.Data.NormalChannel.Array[xx, yy].g, worksheet.Data.NormalChannel.Array[xx, yy].b) * 2f - Vector3.One;
						localNormal.x *= normalFactor;
						localNormal.y *= normalFactor;
						localNormal = localNormal.Normalized();
						worldNormal = basis.Xform(localNormal).Normalized();
						float length = 0f;
						for (int ray = 0; ray < rayCount; ray++)
						{
							Vector3 rayNormal = lightPosition - position;
							float attenuation = Mathf.Clamp(rayNormal.Length() / lightRange, 0f, 1f);
							rayNormal = rayNormal.Normalized();
							Register.CollisionManager.MeshIntersectRay(ref collision, position + worldNormal * rayOffset, rayNormal, backfaceCulling: false);
							if (collision.CollisionDetected)
							{
								(collision.Position - (position + worldNormal * rayOffset)).Length();
								if ((collision.Position - lightPosition).Dot(rayNormal) > 0f)
								{
									float nDotL = worldNormal.Dot(rayNormal);
									length = ((!(nDotL > 0f)) ? 1f : (1f - (nDotL * 1f - attenuation)));
								}
								else
								{
									length = 1f;
								}
								break;
							}
							float nDotL2 = worldNormal.Dot(rayNormal);
							length = ((!(nDotL2 > 0f)) ? 1f : (1f - (nDotL2 * 1f - attenuation)));
						}
						outputArray[xx - startX, yy - startY] = Mathf.Clamp(length * intensity, 0f, 1f);
					}
				}
			}
		}
		return outputArray;
	}

	public void BakeLight(Action<float[,], bool> threadCompleteCallback, Vector3 lightPosition, float lightRange = 5f, float normalFactor = 1f, float rayOffset = 0.01f, float intensity = 1f)
	{
		if (!threadsManager.IsBusy)
		{
			BakeManager.normalFactor = normalFactor;
			BakeManager.rayOffset = rayOffset;
			BakeManager.intensity = intensity;
			BakeManager.lightPosition = lightPosition;
			BakeManager.lightRange = lightRange;
			threadsManager.Start(BakeLight, threadCompleteCallback);
		}
	}
}
