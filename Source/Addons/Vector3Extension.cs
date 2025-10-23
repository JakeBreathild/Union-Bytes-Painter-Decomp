using System;
using Godot;

public static class Vector3Extension
{
	public static Basis ConvertToBasis(Vector3 vector)
	{
		Basis basis = Basis.Identity;
		if ((double)vector.x != 0.0 || (double)vector.y != 0.0 || (double)vector.z != 0.0)
		{
			basis.Column2 = -vector;
		}
		basis.Column0 = vector.Cross(basis.Column2);
		if (basis.Column0.Dot(basis.Column0) == 0f)
		{
			basis.Column0 = vector.Cross(Vector3.Back);
			if (basis.Column0.Dot(basis.Column0) == 0f)
			{
				basis.Column0 = vector.Cross(Vector3.Right);
			}
		}
		basis.Column1 = basis.Column2.Cross(basis.Column0);
		return basis;
	}

	public static Vector3 RandomDirection(Vector3 normal)
	{
		float z = 2f * GD.Randf() - 1f;
		float t = 2f * GD.Randf() * Mathf.Pi;
		float r = Mathf.Sqrt(1f - z * z);
		Vector3 result = default(Vector3);
		result.x = r * Mathf.Cos(t);
		result.y = r * Mathf.Sin(t);
		result.z = z;
		return (result * Mathf.Sign(result.Dot(normal))).Normalized();
	}

	public static Vector3 UniformDirection(Vector3 normal, int ray, int rayCount)
	{
		float theta = Mathf.Acos(1f - ((float)ray + 0.5f) / ((float)rayCount * 1.2f));
		float phi = Mathf.PosMod(2.4f * ((float)ray + 0.5f), Mathf.Pi * 2f);
		Vector3 result = default(Vector3);
		result.x = Mathf.Cos(phi) * Mathf.Sin(theta);
		result.y = Mathf.Sin(phi) * Mathf.Sin(theta);
		result.z = Mathf.Cos(theta);
		return ConvertToBasis(-normal).Xform(result).Normalized();
	}
}
