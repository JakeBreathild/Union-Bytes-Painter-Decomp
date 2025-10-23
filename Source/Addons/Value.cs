using System;
using Godot;

public struct Value : IEquatable<Value>
{
	private static readonly float floatFactor = 0.003921569f;

	public static readonly Value Zero = new Value(0f, 0f);

	public static readonly Value Black = new Value(0f);

	public static readonly Value Gray = new Value(0.5f);

	public static readonly Value White = new Value(1f);

	public float v;

	public float a;

	public byte v8
	{
		get
		{
			return (byte)Mathf.Round(v * 255f);
		}
		set
		{
			v = (float)(int)value * floatFactor;
		}
	}

	public byte a8
	{
		get
		{
			return (byte)Mathf.Round(a * 255f);
		}
		set
		{
			a = (float)(int)value * floatFactor;
		}
	}

	public int v32
	{
		get
		{
			return (int)Mathf.Round(v * 255f);
		}
		set
		{
			v = (float)value * floatFactor;
		}
	}

	public int a32
	{
		get
		{
			return (int)Mathf.Round(a * 255f);
		}
		set
		{
			a = (float)value * floatFactor;
		}
	}

	public float this[int index]
	{
		get
		{
			return index switch
			{
				0 => v, 
				1 => a, 
				_ => throw new IndexOutOfRangeException(), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				v = value;
				break;
			case 1:
				a = value;
				break;
			default:
				throw new IndexOutOfRangeException();
			}
		}
	}

	public Value Blend(Value over)
	{
		float num = 1f - over.a;
		Value result = new Value
		{
			a = a * num + over.a
		};
		if (result.a == 0f)
		{
			return new Value(0f, 0f);
		}
		result.v = (v * a * num + over.v * over.a) / result.a;
		return result;
	}

	public Value Contrasted()
	{
		return new Value((v + 0.5f) % 1f, a);
	}

	public Value Darkened(float amount)
	{
		Value result = this;
		result.v *= 1f - amount;
		return result;
	}

	public Value Inverted()
	{
		return new Value(1f - v, a);
	}

	public Value Lightened(float amount)
	{
		Value result = this;
		result.v += (1f - result.v) * amount;
		return result;
	}

	public Value LinearInterpolate(Value to, float weight)
	{
		return new Value(Mathf.Lerp(v, to.v, weight), Mathf.Lerp(a, to.a, weight));
	}

	public Value LinearInterpolate(Value to, Value weight)
	{
		return new Value(Mathf.Lerp(v, to.v, weight.v), Mathf.Lerp(a, to.a, weight.a));
	}

	public Value(float v, float a = 1f)
	{
		this.v = v;
		this.a = a;
	}

	public static Value operator +(Value left, Value right)
	{
		left.v += right.v;
		left.a += right.a;
		return left;
	}

	public static Value operator -(Value left, Value right)
	{
		left.v -= right.v;
		left.a -= right.a;
		return left;
	}

	public static Value operator -(Value value)
	{
		return White - value;
	}

	public static Value operator *(Value value, float scale)
	{
		value.v *= scale;
		value.a *= scale;
		return value;
	}

	public static Value operator *(float scale, Value value)
	{
		value.v *= scale;
		value.a *= scale;
		return value;
	}

	public static Value operator *(Value left, Value right)
	{
		left.v *= right.v;
		left.a *= right.a;
		return left;
	}

	public static Value operator /(Value value, float scale)
	{
		value.v /= scale;
		value.a /= scale;
		return value;
	}

	public static Value operator /(Value left, Value right)
	{
		left.v /= right.v;
		left.a /= right.a;
		return left;
	}

	public static bool operator ==(Value left, Value right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Value left, Value right)
	{
		return !left.Equals(right);
	}

	public static bool operator <(Value left, Value right)
	{
		if (Mathf.IsEqualApprox(left.v, right.v))
		{
			return left.a < right.a;
		}
		return left.v < right.v;
	}

	public static bool operator >(Value left, Value right)
	{
		if (Mathf.IsEqualApprox(left.v, right.v))
		{
			return left.a > right.a;
		}
		return left.v > right.v;
	}

	public override bool Equals(object obj)
	{
		if (obj is Value)
		{
			return Equals((Value)obj);
		}
		return false;
	}

	public bool Equals(Value other)
	{
		if (v == other.v)
		{
			return a == other.a;
		}
		return false;
	}

	public bool IsEqualApprox(Value other)
	{
		if (Mathf.IsEqualApprox(v, other.v))
		{
			return Mathf.IsEqualApprox(a, other.a);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return v.GetHashCode() ^ a.GetHashCode();
	}

	public override string ToString()
	{
		return v + "," + a;
	}

	public string ToString(string format)
	{
		return v.ToString(format) + "," + a.ToString(format);
	}
}
