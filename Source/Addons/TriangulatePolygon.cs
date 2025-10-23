using System.Collections.Generic;
using Godot;

public static class TriangulatePolygon
{
	public class Vertex
	{
		public Vector3 position;

		public int index;

		public HalfEdge halfEdge;

		public Triangle triangle;

		public Vertex prevVertex;

		public Vertex nextVertex;

		public bool isReflex;

		public bool isConvex;

		public bool isEar;

		public Vertex(Vector3 position, int index)
		{
			this.position = position;
			this.index = index;
		}

		public Vector2 GetPos2D_XZ()
		{
			return new Vector2(position.x, position.z);
		}
	}

	public class HalfEdge
	{
		public Vertex v;

		public Triangle t;

		public HalfEdge nextEdge;

		public HalfEdge prevEdge;

		public HalfEdge oppositeEdge;

		public HalfEdge(Vertex v)
		{
			this.v = v;
		}
	}

	public class Triangle
	{
		public Vertex v1;

		public Vertex v2;

		public Vertex v3;

		public HalfEdge halfEdge;

		public Triangle(Vertex v1, Vertex v2, Vertex v3)
		{
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
		}

		public Triangle(Vector3 v1, int i1, Vector3 v2, int i2, Vector3 v3, int i3)
		{
			this.v1 = new Vertex(v1, i1);
			this.v2 = new Vertex(v2, i2);
			this.v3 = new Vertex(v3, i3);
		}

		public Triangle(HalfEdge halfEdge)
		{
			this.halfEdge = halfEdge;
		}

		public void ChangeOrientation()
		{
			Vertex temp = v1;
			v1 = v2;
			v2 = temp;
		}
	}

	public static int ClampListIndex(int index, int listSize)
	{
		index = (index % listSize + listSize) % listSize;
		return index;
	}

	public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
	{
		bool isClockWise = true;
		if (p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y > 0f)
		{
			isClockWise = false;
		}
		return isClockWise;
	}

	public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
	{
		bool isWithinTriangle = false;
		float denominator = (p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y);
		float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
		float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
		float c = 1f - a - b;
		if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
		{
			isWithinTriangle = true;
		}
		return isWithinTriangle;
	}

	public static List<Triangle> TriangulateConvexPolygon(List<Vector3> convexHullpoints)
	{
		List<Triangle> triangles = new List<Triangle>();
		for (int i = 2; i < convexHullpoints.Count; i++)
		{
			Vertex a = new Vertex(convexHullpoints[0], 0);
			Vertex b = new Vertex(convexHullpoints[i - 1], i - 1);
			Vertex c = new Vertex(convexHullpoints[i], i);
			triangles.Add(new Triangle(a, b, c));
		}
		return triangles;
	}

	public static List<Triangle> TriangulateConcavePolygon(List<Vector3> points)
	{
		List<Triangle> triangles = new List<Triangle>();
		if (points.Count == 3)
		{
			triangles.Add(new Triangle(points[0], 0, points[1], 1, points[2], 2));
			return triangles;
		}
		List<Vertex> vertices = new List<Vertex>();
		for (int i = 0; i < points.Count; i++)
		{
			vertices.Add(new Vertex(points[i], i));
		}
		for (int j = 0; j < vertices.Count; j++)
		{
			int nextPos = ClampListIndex(j + 1, vertices.Count);
			int prevPos = ClampListIndex(j - 1, vertices.Count);
			vertices[j].prevVertex = vertices[prevPos];
			vertices[j].nextVertex = vertices[nextPos];
		}
		for (int k = 0; k < vertices.Count; k++)
		{
			CheckIfReflexOrConvex(vertices[k]);
		}
		List<Vertex> earVertices = new List<Vertex>();
		for (int l = 0; l < vertices.Count; l++)
		{
			IsVertexEar(vertices[l], vertices, earVertices);
		}
		while (vertices.Count != 3)
		{
			Vertex earVertex = earVertices[0];
			Vertex earVertexPrev = earVertex.prevVertex;
			Vertex earVertexNext = earVertex.nextVertex;
			Triangle newTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);
			triangles.Add(newTriangle);
			earVertices.Remove(earVertex);
			vertices.Remove(earVertex);
			earVertexPrev.nextVertex = earVertexNext;
			earVertexNext.prevVertex = earVertexPrev;
			CheckIfReflexOrConvex(earVertexPrev);
			CheckIfReflexOrConvex(earVertexNext);
			earVertices.Remove(earVertexPrev);
			earVertices.Remove(earVertexNext);
			IsVertexEar(earVertexPrev, vertices, earVertices);
			IsVertexEar(earVertexNext, vertices, earVertices);
		}
		triangles.Add(new Triangle(vertices[0], vertices[0].prevVertex, vertices[0].nextVertex));
		return triangles;
	}

	private static void CheckIfReflexOrConvex(Vertex v)
	{
		v.isReflex = false;
		v.isConvex = false;
		Vector2 pos2D_XZ = v.prevVertex.GetPos2D_XZ();
		Vector2 b = v.GetPos2D_XZ();
		Vector2 c = v.nextVertex.GetPos2D_XZ();
		if (IsTriangleOrientedClockwise(pos2D_XZ, b, c))
		{
			v.isReflex = true;
		}
		else
		{
			v.isConvex = true;
		}
	}

	private static void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
	{
		if (v.isReflex)
		{
			return;
		}
		Vector2 a = v.prevVertex.GetPos2D_XZ();
		Vector2 b = v.GetPos2D_XZ();
		Vector2 c = v.nextVertex.GetPos2D_XZ();
		bool hasPointInside = false;
		for (int i = 0; i < vertices.Count; i++)
		{
			if (vertices[i].isReflex)
			{
				Vector2 p = vertices[i].GetPos2D_XZ();
				if (IsPointInTriangle(a, b, c, p))
				{
					hasPointInside = true;
					break;
				}
			}
		}
		if (!hasPointInside)
		{
			earVertices.Add(v);
		}
	}
}
