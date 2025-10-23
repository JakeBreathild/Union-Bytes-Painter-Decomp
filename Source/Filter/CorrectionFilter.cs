using Godot;

public static class CorrectionFilter
{
	public static float[,] FloatArray(float[,] array, int width, int height, float black, float white, float power, Blender.BlendingModeEnum blendingMode = Blender.BlendingModeEnum.NORMAL, float blendingStrength = 1f)
	{
		float[,] outputArray = new float[width, height];
		float range = white - black;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				float value = Mathf.Clamp(Mathf.Pow(black + array[x, y] * range, power), 0f, 1f);
				outputArray[x, y] = Blender.Blend(array[x, y], value, blendingMode, blendingStrength);
			}
		}
		return outputArray;
	}

	public static float[,] FloatChannel(Channel<float> channel, float black, float white, float power, Blender.BlendingModeEnum blendingMode = Blender.BlendingModeEnum.NORMAL, float blendingStrength = 1f)
	{
		return FloatArray(channel.Array, channel.Width, channel.Height, black, white, power, blendingMode, blendingStrength);
	}

	public static Value[,] ValueArray(Value[,] array, int width, int height, float black, float white, float power, Blender.BlendingModeEnum blendingMode = Blender.BlendingModeEnum.NORMAL, float blendingStrength = 1f)
	{
		Value[,] outputArray = new Value[width, height];
		float range = white - black;
		Value value = Value.Black;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				value.v = Mathf.Pow(black + array[x, y].v * range, power);
				Blender.Clamp(ref value);
				outputArray[x, y] = Blender.Blend(array[x, y], value, blendingMode, blendingStrength);
			}
		}
		return outputArray;
	}

	public static Value[,] ValueChannel(Channel<Value> channel, float black, float white, float power, Blender.BlendingModeEnum blendingMode = Blender.BlendingModeEnum.NORMAL, float blendingStrength = 1f)
	{
		return ValueArray(channel.Array, channel.Width, channel.Height, black, white, power, blendingMode, blendingStrength);
	}

	public static Color[,] ColorArray(Color[,] array, int width, int height, float black, float white, float power, float saturation, float hue, Blender.BlendingModeEnum blendingMode = Blender.BlendingModeEnum.NORMAL, float blendingStrength = 1f)
	{
		Color[,] outputArray = new Color[width, height];
		float range = white - black;
		Color color = default(Color);
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				color.r = Mathf.Pow(black + array[x, y].r * range, power);
				color.g = Mathf.Pow(black + array[x, y].g * range, power);
				color.b = Mathf.Pow(black + array[x, y].b * range, power);
				color.a = array[x, y].a;
				float currentHue = color.h + hue;
				if (currentHue > 1f)
				{
					currentHue -= 1f;
				}
				if (currentHue < 0f)
				{
					currentHue += 1f;
				}
				color.h = currentHue;
				color.s *= saturation;
				Blender.Clamp(ref color);
				outputArray[x, y] = Blender.Blend(array[x, y], color, blendingMode, blendingStrength);
			}
		}
		return outputArray;
	}

	public static Color[,] ColorChannel(Channel<Color> channel, float black, float white, float power, float saturation, float hue, Blender.BlendingModeEnum blendingMode = Blender.BlendingModeEnum.NORMAL, float blendingStrength = 1f)
	{
		return ColorArray(channel.Array, channel.Width, channel.Height, black, white, power, saturation, hue, blendingMode, blendingStrength);
	}
}
