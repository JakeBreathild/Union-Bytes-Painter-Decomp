using System;
using Godot;

public struct Vector2i : IEquatable<Vector2i>
{
	public enum Axis
	{
		X,
		Y
	}

	public int x;

	public int y;

	public Vector2i xy => new Vector2i(x, y);

	public Vector2i yx => new Vector2i(y, x);

	public int this[int index]
	{
		get
		{
			return index switch
			{
				0 => x, 
				1 => y, 
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
			}
		}
	}

	public static Vector2i Left { get; } = new Vector2i(-1, 0);

	public static Vector2i Right { get; } = new Vector2i(1, 0);

	public static Vector2i Down { get; } = new Vector2i(0, -1);

	public static Vector2i Up { get; } = new Vector2i(0, 1);

	public static Vector2i NegOne { get; } = new Vector2i(-1, -1);

	public static Vector2i One { get; } = new Vector2i(1, 1);

	public static Vector2i Zero { get; } = new Vector2i(0, 0);

	public static Vector2i Minimum { get; } = new Vector2i(int.MinValue, int.MinValue);

	public static Vector2i Maximum { get; } = new Vector2i(int.MaxValue, int.MaxValue);

	public Vector2i(Vector2i v)
	{
		x = v.x;
		y = v.y;
	}

	public Vector2i(Vector2 v)
	{
		x = (int)v.x;
		y = (int)v.y;
	}

	public Vector2i(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2i(float x, float y)
	{
		this.x = (int)x;
		this.y = (int)y;
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
			return Equals((Vector2i)obj);
		}
		return false;
	}

	public bool Equals(Vector2i other)
	{
		if (other.x == x && other.y == y)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)((float)(x.GetHashCode() + y.GetHashCode()) / 31.515135f * 14.524213f);
	}

	public float DistanceSquaredTo(Vector2i b)
	{
		return (x - b.x) * (x - b.x) + (y - b.y) * (y - b.y);
	}

	public float DistanceTo(Vector2i b)
	{
		return Mathf.Sqrt((x - b.x) * (x - b.x) + (y - b.y) * (y - b.y));
	}

	public float Dot(Vector2i b)
	{
		return x * b.x + y * b.y;
	}

	public float Cross(Vector2i b)
	{
		return x * b.y - b.x * y;
	}

	public float Length()
	{
		return Mathf.Sqrt(x * x + y * y);
	}

	public float LengthSquared()
	{
		return x * x + y * y;
	}

	public Vector2i Inverse()
	{
		return new Vector2i(x * -1, y * -1);
	}

	public Vector2i Min(int x, int y)
	{
		Vector2i result = default(Vector2i);
		result.x = ((x < this.x) ? x : this.x);
		result.y = ((y < this.y) ? y : this.y);
		return result;
	}

	public Vector2i Min(Vector2i v)
	{
		Vector2i result = default(Vector2i);
		result.x = ((v.x < x) ? v.x : x);
		result.y = ((v.y < y) ? v.y : y);
		return result;
	}

	public void SetMin(int x, int y)
	{
		if (x < this.x)
		{
			this.x = x;
		}
		if (y < this.y)
		{
			this.y = y;
		}
	}

	public void SetMin(Vector2i v)
	{
		if (v.x < x)
		{
			x = v.x;
		}
		if (v.y < y)
		{
			y = v.y;
		}
	}

	public Vector2i Max(int x, int y)
	{
		Vector2i result = default(Vector2i);
		result.x = ((x > this.x) ? x : this.x);
		result.y = ((y > this.y) ? y : this.y);
		return result;
	}

	public Vector2i Max(Vector2i v)
	{
		Vector2i result = default(Vector2i);
		result.x = ((v.x > x) ? v.x : x);
		result.y = ((v.y > y) ? v.y : y);
		return result;
	}

	public void SetMax(int x, int y)
	{
		if (x > this.x)
		{
			this.x = x;
		}
		if (y > this.y)
		{
			this.y = y;
		}
	}

	public void SetMax(Vector2i v)
	{
		if (v.x > x)
		{
			x = v.x;
		}
		if (v.y > y)
		{
			y = v.y;
		}
	}

	public Vector2i Set(int x, int y)
	{
		this.x = x;
		this.y = y;
		return this;
	}

	public Vector2 ToFloat()
	{
		return new Vector2(x, y);
	}

	public override string ToString()
	{
		return x + ", " + y;
	}

	public static Vector2i operator +(Vector2i left, Vector2i right)
	{
		return new Vector2i(left.x + right.x, left.y + right.y);
	}

	public static Vector2i operator -(Vector2i vec)
	{
		return new Vector2i(-vec.x, -vec.y);
	}

	public static Vector2i operator -(Vector2i left, Vector2i right)
	{
		return new Vector2i(left.x - right.x, left.y - right.y);
	}

	public static Vector2i operator *(Vector2i vec, float scale)
	{
		return new Vector2i((float)vec.x * scale, (float)vec.y * scale);
	}

	public static Vector2i operator *(float scale, Vector2i vec)
	{
		return new Vector2i(scale * (float)vec.x, scale * (float)vec.y);
	}

	public static Vector2i operator *(Vector2i left, Vector2i right)
	{
		return new Vector2i(left.x * right.x, left.y * right.y);
	}

	public static Vector2i operator /(Vector2i vec, float scale)
	{
		return new Vector2i((float)vec.x / scale, (float)vec.y / scale);
	}

	public static Vector2i operator /(Vector2i left, Vector2i right)
	{
		return new Vector2i(left.x / right.x, left.y / right.y);
	}

	public static bool operator ==(Vector2i left, Vector2i right)
	{
		if (left.x == right.x && left.y == right.y)
		{
			return true;
		}
		return false;
	}

	public static bool operator !=(Vector2i left, Vector2i right)
	{
		if (left.x == right.x && left.y == right.y)
		{
			return false;
		}
		return true;
	}
}
