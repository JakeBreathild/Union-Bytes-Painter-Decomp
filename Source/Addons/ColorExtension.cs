using Godot;

public static class ColorExtension
{
	public static readonly Color Zero = new Color(0f, 0f, 0f, 0f);

	public static readonly Color Black = new Color(0f, 0f, 0f);

	public static readonly Color Normal = new Color(0.5f, 0.5f, 1f);

	public static readonly Color White = new Color(1f, 1f, 1f);

	public static float ColorDistance(Color colorA, Color colorB)
	{
		Color colorDifference = colorA - colorB;
		return Mathf.Sqrt(colorDifference.r * colorDifference.r + colorDifference.g * colorDifference.g + colorDifference.b * colorDifference.b + colorDifference.a * colorDifference.a) * 0.5f;
	}

	public static Color VectorToColor(Vector3 vector, bool clamp = true)
	{
		if (clamp)
		{
			vector.x = Mathf.Clamp(vector.x, 0f, 1f);
			vector.y = Mathf.Clamp(vector.y, 0f, 1f);
			vector.z = Mathf.Clamp(vector.z, 0f, 1f);
		}
		return new Color(vector.x, vector.y, vector.z);
	}

	public static Vector3 ColorToVector(Color color)
	{
		return new Vector3(color.r, color.g, color.b);
	}

	public static Color NormalVectorToColor(Vector3 vector)
	{
		return new Color(vector.x * 0.5f + 0.5f, vector.y * 0.5f + 0.5f, vector.z * 0.5f + 0.5f);
	}

	public static Vector3 NormalColorToVector(Color color)
	{
		return new Vector3(color.r * 2f - 1f, color.g * 2f - 1f, color.b * 2f - 1f);
	}
}
