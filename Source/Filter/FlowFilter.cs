using Godot;

public static class FlowFilter
{
	public enum ModeEnum
	{
		SHIFT,
		BLUR,
		FADE,
		WARP
	}

	public static float[,] FloatArray(float[,] array, ChannelArray<Value> flowArray, int width, int height, ModeEnum mode, float direction, float distance, int steps)
	{
		float[,] bluredFloatArray = new float[width, height];
		Vector2 directionVector = new Vector2(Mathf.Cos(Mathf.Deg2Rad(direction)), Mathf.Sin(Mathf.Deg2Rad(direction))).Normalized();
		switch (mode)
		{
		case ModeEnum.SHIFT:
		{
			for (int num3 = 0; num3 < height; num3++)
			{
				for (int num4 = 0; num4 < width; num4++)
				{
					int modX = Mathf.PosMod(num4, flowArray.Width);
					int modY = Mathf.PosMod(num3, flowArray.Height);
					float flowValue = flowArray[modX, modY].v;
					modX = Mathf.PosMod(num4 + Mathf.FloorToInt(flowValue * directionVector.x * distance), width);
					modY = Mathf.PosMod(num3 + Mathf.FloorToInt(flowValue * directionVector.y * distance), height);
					bluredFloatArray[num4, num3] = array[modX, modY];
				}
			}
			break;
		}
		case ModeEnum.BLUR:
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < width; k++)
				{
					int modX = Mathf.PosMod(k, flowArray.Width);
					int modY = Mathf.PosMod(j, flowArray.Height);
					float flowValue = flowArray[modX, modY].v;
					float stepX = flowValue * directionVector.x * distance / (float)steps;
					float stepY = flowValue * directionVector.y * distance / (float)steps;
					for (int l = 0; l < steps; l++)
					{
						modX = Mathf.PosMod(k + Mathf.RoundToInt((float)l * stepX), width);
						modY = Mathf.PosMod(j + Mathf.RoundToInt((float)l * stepY), height);
						bluredFloatArray[k, j] += array[modX, modY];
					}
					bluredFloatArray[k, j] /= steps;
				}
			}
			break;
		}
		case ModeEnum.FADE:
		{
			float deltaFactor = 1f / (float)steps;
			for (int m = 0; m < height; m++)
			{
				for (int n = 0; n < width; n++)
				{
					int modX = Mathf.PosMod(n, flowArray.Width);
					int modY = Mathf.PosMod(m, flowArray.Height);
					float flowValue = flowArray[modX, modY].v;
					float stepX = flowValue * directionVector.x * distance / (float)steps;
					float stepY = flowValue * directionVector.y * distance / (float)steps;
					float factor = 0f;
					for (int num2 = 0; num2 < steps; num2++)
					{
						modX = Mathf.PosMod(n + Mathf.RoundToInt((float)num2 * stepX), width);
						modY = Mathf.PosMod(m + Mathf.RoundToInt((float)num2 * stepY), height);
						float intensity = Mathf.Lerp(1f, 0f, (float)num2 * deltaFactor);
						intensity *= intensity;
						bluredFloatArray[n, m] += array[modX, modY] * intensity;
						factor += intensity;
					}
					bluredFloatArray[n, m] /= factor;
				}
			}
			break;
		}
		case ModeEnum.WARP:
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int modX = Mathf.PosMod(x, flowArray.Width);
					int modY = Mathf.PosMod(y - 1, flowArray.Height);
					float p8 = flowArray[modX, modY].v;
					modX = Mathf.PosMod(x + 1, flowArray.Width);
					modY = Mathf.PosMod(y, flowArray.Height);
					float p9 = flowArray[modX, modY].v;
					modX = Mathf.PosMod(x, flowArray.Width);
					modY = Mathf.PosMod(y + 1, flowArray.Height);
					float p10 = flowArray[modX, modY].v;
					modX = Mathf.PosMod(x - 1, flowArray.Width);
					modY = Mathf.PosMod(y, flowArray.Height);
					float p11 = flowArray[modX, modY].v;
					float num = Mathf.Abs(p11 - p9 + (p10 - p8)) * 0.5f;
					float normalStrength = 5f;
					directionVector.x = normalStrength * (p11 - p9);
					directionVector.y = normalStrength * (p10 - p8);
					directionVector = directionVector.Normalized();
					float stepX = num * directionVector.x * distance / (float)steps;
					float stepY = num * directionVector.y * distance / (float)steps;
					for (int i = 0; i < steps; i++)
					{
						modX = Mathf.PosMod(x + Mathf.RoundToInt((float)i * stepX), width);
						modY = Mathf.PosMod(y + Mathf.RoundToInt((float)i * stepY), height);
						bluredFloatArray[x, y] += array[modX, modY];
					}
					bluredFloatArray[x, y] /= steps;
				}
			}
			break;
		}
		}
		return bluredFloatArray;
	}

	public static float[,] FloatChannel(Channel<float> channel, ChannelArray<Value> flowArray, int width, int height, ModeEnum mode, float direction, float distance, int steps)
	{
		return FloatArray(channel.Array, flowArray, width, height, mode, direction, distance, steps);
	}

	public static Value[,] ValueArray(Value[,] array, ChannelArray<Value> flowArray, int width, int height, ModeEnum mode, float direction, float distance, int steps, bool includeAlpha = true)
	{
		Value[,] bluredValueArray = new Value[width, height];
		Vector2 directionVector = new Vector2(Mathf.Cos(Mathf.Deg2Rad(direction)), Mathf.Sin(Mathf.Deg2Rad(direction))).Normalized();
		switch (mode)
		{
		case ModeEnum.SHIFT:
		{
			for (int num3 = 0; num3 < height; num3++)
			{
				for (int num4 = 0; num4 < width; num4++)
				{
					int modX = Mathf.PosMod(num4, flowArray.Width);
					int modY = Mathf.PosMod(num3, flowArray.Height);
					float flowValue = flowArray[modX, modY].v;
					modX = Mathf.PosMod(num4 + Mathf.FloorToInt(flowValue * directionVector.x * distance), width);
					modY = Mathf.PosMod(num3 + Mathf.FloorToInt(flowValue * directionVector.y * distance), height);
					bluredValueArray[num4, num3] = array[modX, modY];
					if (!includeAlpha)
					{
						bluredValueArray[num4, num3].a = array[num4, num3].a;
					}
				}
			}
			break;
		}
		case ModeEnum.BLUR:
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < width; k++)
				{
					int modX = Mathf.PosMod(k, flowArray.Width);
					int modY = Mathf.PosMod(j, flowArray.Height);
					float flowValue = flowArray[modX, modY].v;
					float stepX = flowValue * directionVector.x * distance / (float)steps;
					float stepY = flowValue * directionVector.y * distance / (float)steps;
					for (int l = 0; l < steps; l++)
					{
						modX = Mathf.PosMod(k + Mathf.RoundToInt((float)l * stepX), width);
						modY = Mathf.PosMod(j + Mathf.RoundToInt((float)l * stepY), height);
						bluredValueArray[k, j] += array[modX, modY];
					}
					bluredValueArray[k, j] /= (float)steps;
					if (!includeAlpha)
					{
						bluredValueArray[k, j].a = array[k, j].a;
					}
				}
			}
			break;
		}
		case ModeEnum.FADE:
		{
			float deltaFactor = 1f / (float)steps;
			for (int m = 0; m < height; m++)
			{
				for (int n = 0; n < width; n++)
				{
					int modX = Mathf.PosMod(n, flowArray.Width);
					int modY = Mathf.PosMod(m, flowArray.Height);
					float flowValue = flowArray[modX, modY].v;
					float stepX = flowValue * directionVector.x * distance / (float)steps;
					float stepY = flowValue * directionVector.y * distance / (float)steps;
					float factor = 0f;
					for (int num2 = 0; num2 < steps; num2++)
					{
						modX = Mathf.PosMod(n + Mathf.RoundToInt((float)num2 * stepX), width);
						modY = Mathf.PosMod(m + Mathf.RoundToInt((float)num2 * stepY), height);
						float intensity = Mathf.Lerp(1f, 0f, (float)num2 * deltaFactor);
						intensity *= intensity;
						bluredValueArray[n, m] += array[modX, modY] * intensity;
						factor += intensity;
					}
					bluredValueArray[n, m] /= factor;
					if (!includeAlpha)
					{
						bluredValueArray[n, m].a = array[n, m].a;
					}
				}
			}
			break;
		}
		case ModeEnum.WARP:
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int modX = Mathf.PosMod(x, flowArray.Width);
					int modY = Mathf.PosMod(y - 1, flowArray.Height);
					float p8 = flowArray[modX, modY].v;
					modX = Mathf.PosMod(x + 1, flowArray.Width);
					modY = Mathf.PosMod(y, flowArray.Height);
					float p9 = flowArray[modX, modY].v;
					modX = Mathf.PosMod(x, flowArray.Width);
					modY = Mathf.PosMod(y + 1, flowArray.Height);
					float p10 = flowArray[modX, modY].v;
					modX = Mathf.PosMod(x - 1, flowArray.Width);
					modY = Mathf.PosMod(y, flowArray.Height);
					float p11 = flowArray[modX, modY].v;
					float num = Mathf.Abs(p11 - p9 + (p10 - p8)) * 0.5f;
					float normalStrength = 5f;
					directionVector.x = normalStrength * (p11 - p9);
					directionVector.y = normalStrength * (p10 - p8);
					directionVector = directionVector.Normalized();
					float stepX = num * directionVector.x * distance / (float)steps;
					float stepY = num * directionVector.y * distance / (float)steps;
					for (int i = 0; i < steps; i++)
					{
						modX = Mathf.PosMod(x + Mathf.RoundToInt((float)i * stepX), width);
						modY = Mathf.PosMod(y + Mathf.RoundToInt((float)i * stepY), height);
						bluredValueArray[x, y] += array[modX, modY];
					}
					bluredValueArray[x, y] /= (float)steps;
					if (!includeAlpha)
					{
						bluredValueArray[x, y].a = array[x, y].a;
					}
				}
			}
			break;
		}
		}
		return bluredValueArray;
	}

	public static Value[,] ValueChannel(Channel<Value> channel, ChannelArray<Value> flowArray, int width, int height, ModeEnum mode, float direction, float distance, int steps, bool includeAlpha = true)
	{
		return ValueArray(channel.Array, flowArray, width, height, mode, direction, distance, steps, includeAlpha);
	}

	public static Color[,] ColorArray(Color[,] array, ChannelArray<Value> flowArray, int width, int height, ModeEnum mode, float direction, float distance, int steps, bool includeAlpha = true)
	{
		Color[,] bluredColorArray = new Color[width, height];
		Vector2 directionVector = new Vector2(Mathf.Cos(Mathf.Deg2Rad(direction)), Mathf.Sin(Mathf.Deg2Rad(direction))).Normalized();
		switch (mode)
		{
		case ModeEnum.SHIFT:
		{
			for (int num3 = 0; num3 < height; num3++)
			{
				for (int num4 = 0; num4 < width; num4++)
				{
					int modX = Mathf.PosMod(num4, flowArray.Width);
					int modY = Mathf.PosMod(num3, flowArray.Height);
					float flowValue = flowArray[modX, modY].v;
					modX = Mathf.PosMod(num4 + Mathf.FloorToInt(flowValue * directionVector.x * distance), width);
					modY = Mathf.PosMod(num3 + Mathf.FloorToInt(flowValue * directionVector.y * distance), height);
					bluredColorArray[num4, num3] = array[modX, modY];
					if (!includeAlpha)
					{
						bluredColorArray[num4, num3].a = array[num4, num3].a;
					}
				}
			}
			break;
		}
		case ModeEnum.BLUR:
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < width; k++)
				{
					int modX = Mathf.PosMod(k, flowArray.Width);
					int modY = Mathf.PosMod(j, flowArray.Height);
					float flowValue = flowArray[modX, modY].v;
					float stepX = flowValue * directionVector.x * distance / (float)steps;
					float stepY = flowValue * directionVector.y * distance / (float)steps;
					for (int l = 0; l < steps; l++)
					{
						modX = Mathf.PosMod(k + Mathf.RoundToInt((float)l * stepX), width);
						modY = Mathf.PosMod(j + Mathf.RoundToInt((float)l * stepY), height);
						bluredColorArray[k, j] += array[modX, modY];
					}
					bluredColorArray[k, j] /= (float)steps;
					if (!includeAlpha)
					{
						bluredColorArray[k, j].a = array[k, j].a;
					}
				}
			}
			break;
		}
		case ModeEnum.FADE:
		{
			float deltaFactor = 1f / (float)steps;
			for (int m = 0; m < height; m++)
			{
				for (int n = 0; n < width; n++)
				{
					int modX = Mathf.PosMod(n, flowArray.Width);
					int modY = Mathf.PosMod(m, flowArray.Height);
					float flowValue = flowArray[modX, modY].v;
					float stepX = flowValue * directionVector.x * distance / (float)steps;
					float stepY = flowValue * directionVector.y * distance / (float)steps;
					float factor = 0f;
					for (int num2 = 0; num2 < steps; num2++)
					{
						modX = Mathf.PosMod(n + Mathf.RoundToInt((float)num2 * stepX), width);
						modY = Mathf.PosMod(m + Mathf.RoundToInt((float)num2 * stepY), height);
						float intensity = Mathf.Lerp(1f, 0f, (float)num2 * deltaFactor);
						intensity *= intensity;
						bluredColorArray[n, m] += array[modX, modY] * intensity;
						factor += intensity;
					}
					bluredColorArray[n, m] /= factor;
					if (!includeAlpha)
					{
						bluredColorArray[n, m].a = array[n, m].a;
					}
				}
			}
			break;
		}
		case ModeEnum.WARP:
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int modX = Mathf.PosMod(x, flowArray.Width);
					int modY = Mathf.PosMod(y - 1, flowArray.Height);
					float p8 = flowArray[modX, modY].v;
					modX = Mathf.PosMod(x + 1, flowArray.Width);
					modY = Mathf.PosMod(y, flowArray.Height);
					float p9 = flowArray[modX, modY].v;
					modX = Mathf.PosMod(x, flowArray.Width);
					modY = Mathf.PosMod(y + 1, flowArray.Height);
					float p10 = flowArray[modX, modY].v;
					modX = Mathf.PosMod(x - 1, flowArray.Width);
					modY = Mathf.PosMod(y, flowArray.Height);
					float p11 = flowArray[modX, modY].v;
					float num = Mathf.Abs(p11 - p9 + (p10 - p8)) * 0.5f;
					float normalStrength = 5f;
					directionVector.x = normalStrength * (p11 - p9);
					directionVector.y = normalStrength * (p10 - p8);
					directionVector = directionVector.Normalized();
					float stepX = num * directionVector.x * distance / (float)steps;
					float stepY = num * directionVector.y * distance / (float)steps;
					for (int i = 0; i < steps; i++)
					{
						modX = Mathf.PosMod(x + Mathf.RoundToInt((float)i * stepX), width);
						modY = Mathf.PosMod(y + Mathf.RoundToInt((float)i * stepY), height);
						bluredColorArray[x, y] += array[modX, modY];
					}
					bluredColorArray[x, y] /= (float)steps;
					if (!includeAlpha)
					{
						bluredColorArray[x, y].a = array[x, y].a;
					}
				}
			}
			break;
		}
		}
		return bluredColorArray;
	}

	public static Color[,] ColorChannel(Channel<Color> channel, ChannelArray<Value> flowArray, int width, int height, ModeEnum mode, float direction, float distance, int steps, bool includeAlpha = true)
	{
		return ColorArray(channel.Array, flowArray, width, height, mode, direction, distance, steps, includeAlpha);
	}
}
