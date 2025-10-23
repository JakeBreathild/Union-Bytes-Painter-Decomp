using System;
using Godot;

public struct Vector3i : IEquatable<Vector3i>
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	public int x;

	public int y;

	public int z;

	public Vector2i xy => new Vector2i(x, y);

	public Vector2i xz => new Vector2i(x, z);

	public Vector2i yz => new Vector2i(y, z);

	public Vector3i xyz => new Vector3i(x, y, z);

	public Vector3i xzy => new Vector3i(x, z, y);

	public int this[int index]
	{
		get
		{
			return index switch
			{
				0 => x, 
				1 => y, 
				2 => z, 
				_ => 0, 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				x = value;
				break;
			case 1:
				y = value;
				break;
			case 2:
				z = value;
				break;
			}
		}
	}

	public static Vector3i Forward { get; } = new Vector3i(0, 0, 1);

	public static Vector3i Backward { get; } = new Vector3i(0, 0, -1);

	public static Vector3i Left { get; } = new Vector3i(-1, 0, 0);

	public static Vector3i Right { get; } = new Vector3i(1, 0, 0);

	public static Vector3i Down { get; } = new Vector3i(0, -1, 0);

	public static Vector3i Up { get; } = new Vector3i(0, 1, 0);

	public static Vector3i NegOne { get; } = new Vector3i(-1, -1, -1);

	public static Vector3i One { get; } = new Vector3i(1, 1, 1);

	public static Vector3i Zero { get; } = new Vector3i(0, 0, 0);

	public static Vector3i Minimum { get; } = new Vector3i(int.MinValue, int.MinValue, int.MinValue);

	public static Vector3i Maximum { get; } = new Vector3i(int.MaxValue, int.MaxValue, int.MaxValue);

	public Vector3i(Vector3i v)
	{
		x = v.x;
		y = v.y;
		z = v.z;
	}

	public Vector3i(Vector3 v)
	{
		x = (int)v.x;
		y = (int)v.y;
		z = (int)v.z;
	}

	public Vector3i(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public Vector3i(float x, float y, float z)
	{
		this.x = (int)x;
		this.y = (int)y;
		this.z = (int)z;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if ((object)this == obj)
		{
			return true;
		}
		if (obj.GetType() == GetType())
		{
			return Equals((Vector3i)obj);
		}
		return false;
	}

	public bool Equals(Vector3i other)
	{
		if (other.x == x && other.y == y && other.z == z)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)((float)(x.GetHashCode() + y.GetHashCode() + z.GetHashCode()) / 31.515135f * 14.524213f);
	}

	public float DistanceSquaredTo(Vector3i b)
	{
		return (x - b.x) * (x - b.x) + (y - b.y) * (y - b.y) + (z - b.z) * (z - b.z);
	}

	public float DistanceTo(Vector3i b)
	{
		return Mathf.Sqrt((x - b.x) * (x - b.x) + (y - b.y) * (y - b.y) + (z - b.z) * (z - b.z));
	}

	public float Dot(Vector3i b)
	{
		return x * b.x + y * b.y + z * b.z;
	}

	public Vector3 Cross(Vector3i b)
	{
		float num = 1f * (float)(y * b.z - z * b.y);
		float yy = 1f * (float)(z * b.x - x * b.z);
		float zz = 1f * (float)(x * b.y - y * b.x);
		return new Vector3(num, yy, zz);
	}

	public float Length()
	{
		return Mathf.Sqrt(x * x + y * y + z * z);
	}

	public float LengthSquared()
	{
		return x * x + y * y + z * z;
	}

	public Vector3i Inverse()
	{
		return new Vector3i(x * -1, y * -1, z * -1);
	}

	public Vector3i Min(int x, int y, int z)
	{
		Vector3i result = default(Vector3i);
		result.x = ((x < this.x) ? x : this.x);
		result.y = ((y < this.y) ? y : this.y);
		result.z = ((z < this.z) ? z : this.z);
		return result;
	}

	public Vector3i Min(Vector3i v)
	{
		Vector3i result = default(Vector3i);
		result.x = ((v.x < x) ? v.x : x);
		result.y = ((v.y < y) ? v.y : y);
		result.z = ((v.z < z) ? v.z : z);
		return result;
	}

	public void SetMin(int x, int y, int z)
	{
		if (x < this.x)
		{
			this.x = x;
		}
		if (y < this.y)
		{
			this.y = y;
		}
		if (z < this.z)
		{
			this.z = z;
		}
	}

	public void SetMin(Vector3i v)
	{
		if (v.x < x)
		{
			x = v.x;
		}
		if (v.y < y)
		{
			y = v.y;
		}
		if (v.z < z)
		{
			z = v.z;
		}
	}

	public Vector3i Max(int x, int y, int z)
	{
		Vector3i result = default(Vector3i);
		result.x = ((x > this.x) ? x : this.x);
		result.y = ((y > this.y) ? y : this.y);
		result.z = ((z > this.z) ? z : this.z);
		return result;
	}

	public Vector3i Max(Vector3i v)
	{
		Vector3i result = default(Vector3i);
		result.x = ((v.x > x) ? v.x : x);
		result.y = ((v.y > y) ? v.y : y);
		result.z = ((v.z > z) ? v.z : z);
		return result;
	}

	public void SetMax(int x, int y, int z)
	{
		if (x > this.x)
		{
			this.x = x;
		}
		if (y > this.y)
		{
			this.y = y;
		}
		if (z > this.z)
		{
			this.z = z;
		}
	}

	public void SetMax(Vector3i v)
	{
		if (v.x > x)
		{
			x = v.x;
		}
		if (v.y > y)
		{
			y = v.y;
		}
		if (v.z > z)
		{
			z = v.z;
		}
	}

	public Vector3i Set(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
		return this;
	}

	public Vector3 ToFloat()
	{
		return new Vector3(x, y, z);
	}

	public override string ToString()
	{
		return x + ", " + y + ", " + z;
	}

	public static Vector3i operator +(Vector3i left, Vector3i right)
	{
		return new Vector3i(left.x + right.x, left.y + right.y, left.z + right.z);
	}

	public static Vector3i operator -(Vector3i vec)
	{
		return new Vector3i(-vec.x, -vec.y, -vec.z);
	}

	public static Vector3i operator -(Vector3i left, Vector3i right)
	{
		return new Vector3i(left.x - right.x, left.y - right.y, left.z - right.z);
	}

	public static Vector3i operator *(Vector3i vec, float scale)
	{
		return new Vector3i((float)vec.x * scale, (float)vec.y * scale, (float)vec.z * scale);
	}

	public static Vector3i operator *(float scale, Vector3i vec)
	{
		return new Vector3i(scale * (float)vec.x, scale * (float)vec.y, scale * (float)vec.z);
	}

	public static Vector3i operator *(Vector3i left, Vector3i right)
	{
		return new Vector3i(left.x * right.x, left.y * right.y, left.z * right.z);
	}

	public static Vector3i operator /(Vector3i vec, float scale)
	{
		return new Vector3i((float)vec.x / scale, (float)vec.y / scale, (float)vec.z / scale);
	}

	public static Vector3i operator /(Vector3i left, Vector3i right)
	{
		return new Vector3i(left.x / right.x, left.y / right.y, left.z / right.z);
	}

	public static bool operator ==(Vector3i left, Vector3i right)
	{
		if (left.x == right.x && left.y == right.y && left.z == right.z)
		{
			return true;
		}
		return false;
	}

	public static bool operator !=(Vector3i left, Vector3i right)
	{
		if (left.x == right.x && left.y == right.y && left.z == right.z)
		{
			return false;
		}
		return true;
	}
}
