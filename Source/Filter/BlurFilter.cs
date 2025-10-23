using Godot;

public static class BlurFilter
{
	private const int MIN_DISTANCE = 1;

	private const int MAX_DISTANCE = 8;

	private static readonly float[][] gaussianKernel = new float[8][]
	{
		new float[3] { 0.157731f, 0.684538f, 0.157731f },
		new float[5] { 0.06136f, 0.24477f, 0.38774f, 0.24477f, 0.06136f },
		new float[7] { 0.038735f, 0.113085f, 0.215007f, 0.266346f, 0.215007f, 0.113085f, 0.038735f },
		new float[9] { 0.028532f, 0.067234f, 0.124009f, 0.179044f, 0.20236f, 0.179044f, 0.124009f, 0.067234f, 0.028532f },
		new float[11]
		{
			0.0093f, 0.028002f, 0.065984f, 0.121703f, 0.175713f, 0.198596f, 0.175713f, 0.121703f, 0.065984f, 0.028002f,
			0.0093f
		},
		new float[13]
		{
			0.018816f, 0.034474f, 0.056577f, 0.083173f, 0.109523f, 0.129188f, 0.136498f, 0.129188f, 0.109523f, 0.083173f,
			0.056577f, 0.034474f, 0.018816f
		},
		new float[15]
		{
			0.0161f, 0.027272f, 0.042598f, 0.061355f, 0.081488f, 0.099798f, 0.112705f, 0.117367f, 0.112705f, 0.099798f,
			0.081488f, 0.061355f, 0.042598f, 0.027272f, 0.0161f
		},
		new float[17]
		{
			0.014076f, 0.022439f, 0.033613f, 0.047318f, 0.062595f, 0.077812f, 0.090898f, 0.099783f, 0.102934f, 0.099783f,
			0.090898f, 0.077812f, 0.062595f, 0.047318f, 0.033613f, 0.022439f, 0.014076f
		}
	};

	public static float[,] FloatArrayH(float[,] Array, int width, int height, bool tileable, int distance = 1)
	{
		float[,] outputArray = new float[width, height];
		if (distance < 1)
		{
			distance = 1;
		}
		if (distance > 8)
		{
			distance = 8;
		}
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				for (int xx = -distance; xx <= distance; xx++)
				{
					int xxx;
					if (tileable)
					{
						xxx = Mathf.PosMod(x + xx, width);
					}
					else
					{
						xxx = x + xx;
						if (xxx < 0)
						{
							xxx = 0;
						}
						else if (xxx >= width)
						{
							xxx = width - 1;
						}
					}
					outputArray[x, y] += Array[xxx, y] * gaussianKernel[distance - 1][xx + distance];
				}
			}
		}
		return outputArray;
	}

	public static float[,] FloatArrayV(float[,] Array, int width, int height, bool tileable, int distance = 1)
	{
		float[,] outputArray = new float[width, height];
		if (distance < 1)
		{
			distance = 1;
		}
		if (distance > 8)
		{
			distance = 8;
		}
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				for (int yy = -distance; yy <= distance; yy++)
				{
					int yyy;
					if (tileable)
					{
						yyy = Mathf.PosMod(y + yy, height);
					}
					else
					{
						yyy = y + yy;
						if (yyy < 0)
						{
							yyy = 0;
						}
						else if (yyy >= height)
						{
							yyy = height - 1;
						}
					}
					outputArray[x, y] += Array[x, yyy] * gaussianKernel[distance - 1][yy + distance];
				}
			}
		}
		return outputArray;
	}

	public static float[,] FloatArray(float[,] array, int width, int height, bool tileable = false, int distance = 1)
	{
		return FloatArrayV(FloatArrayH(array, width, height, tileable, distance), width, height, tileable, distance);
	}

	public static float[,] FloatChannel(Channel<float> channel, bool tileable, int distance = 1)
	{
		return FloatArrayV(FloatArrayH(channel.Array, channel.Width, channel.Height, tileable, distance), channel.Width, channel.Height, tileable, distance);
	}

	public static Value[,] ValueArrayH(Value[,] Array, int width, int height, bool tileable, int distance = 1, bool includeAlpha = true)
	{
		Value[,] outputArray = new Value[width, height];
		if (distance < 1)
		{
			distance = 1;
		}
		if (distance > 8)
		{
			distance = 8;
		}
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				for (int xx = -distance; xx <= distance; xx++)
				{
					int xxx;
					if (tileable)
					{
						xxx = Mathf.PosMod(x + xx, width);
					}
					else
					{
						xxx = x + xx;
						if (xxx < 0)
						{
							xxx = 0;
						}
						else if (xxx >= width)
						{
							xxx = width - 1;
						}
					}
					outputArray[x, y].v += Array[xxx, y].v * Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
					outputArray[x, y].a += Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
				}
				if (outputArray[x, y].a > 0f)
				{
					outputArray[x, y].v /= outputArray[x, y].a;
				}
				if (!includeAlpha)
				{
					outputArray[x, y].a = Array[x, y].a;
				}
			}
		}
		return outputArray;
	}

	public static Value[,] ValueCroppedArrayH(Value[,] Array, int width, int height, bool tileable, int startX, int startY, int endX, int endY, int distance = 1, bool includeAlpha = true)
	{
		Value[,] outputArray = new Value[width, height];
		if (distance < 1)
		{
			distance = 1;
		}
		if (distance > 8)
		{
			distance = 8;
		}
		for (int y = startY; y <= endY; y++)
		{
			for (int x = startX; x <= endX; x++)
			{
				for (int xx = -distance; xx <= distance; xx++)
				{
					int xxx;
					if (tileable)
					{
						xxx = Mathf.PosMod(x + xx, width);
					}
					else
					{
						xxx = x + xx;
						if (xxx < 0)
						{
							xxx = 0;
						}
						else if (xxx >= width)
						{
							xxx = width - 1;
						}
					}
					outputArray[x, y].v += Array[xxx, y].v * Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
					outputArray[x, y].a += Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
				}
				if (outputArray[x, y].a > 0f)
				{
					outputArray[x, y].v /= outputArray[x, y].a;
				}
				if (!includeAlpha)
				{
					outputArray[x, y].a = Array[x, y].a;
				}
			}
		}
		return outputArray;
	}

	public static Value[,] ValueArrayV(Value[,] Array, int width, int height, bool tileable, int distance = 1, bool includeAlpha = true)
	{
		Value[,] outputArray = new Value[width, height];
		if (distance < 1)
		{
			distance = 1;
		}
		if (distance > 8)
		{
			distance = 8;
		}
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				for (int yy = -distance; yy <= distance; yy++)
				{
					int yyy;
					if (tileable)
					{
						yyy = Mathf.PosMod(y + yy, height);
					}
					else
					{
						yyy = y + yy;
						if (yyy < 0)
						{
							yyy = 0;
						}
						else if (yyy >= height)
						{
							yyy = height - 1;
						}
					}
					outputArray[x, y].v += Array[x, yyy].v * Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
					outputArray[x, y].a += Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
				}
				if (outputArray[x, y].a > 0f)
				{
					outputArray[x, y].v /= outputArray[x, y].a;
				}
				if (!includeAlpha)
				{
					outputArray[x, y].a = Array[x, y].a;
				}
			}
		}
		return outputArray;
	}

	public static Value[,] ValueCroppedArrayV(Value[,] Array, int width, int height, bool tileable, int startX, int startY, int endX, int endY, int distance = 1, bool includeAlpha = true)
	{
		Value[,] outputArray = new Value[width, height];
		if (distance < 1)
		{
			distance = 1;
		}
		if (distance > 8)
		{
			distance = 8;
		}
		for (int y = startY; y <= endY; y++)
		{
			for (int x = startX; x <= endX; x++)
			{
				for (int yy = -distance; yy <= distance; yy++)
				{
					int yyy;
					if (tileable)
					{
						yyy = Mathf.PosMod(y + yy, height);
					}
					else
					{
						yyy = y + yy;
						if (yyy < 0)
						{
							yyy = 0;
						}
						else if (yyy >= height)
						{
							yyy = height - 1;
						}
					}
					outputArray[x, y].v += Array[x, yyy].v * Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
					outputArray[x, y].a += Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
				}
				if (outputArray[x, y].a > 0f)
				{
					outputArray[x, y].v /= outputArray[x, y].a;
				}
				if (!includeAlpha)
				{
					outputArray[x, y].a = Array[x, y].a;
				}
			}
		}
		return outputArray;
	}

	public static Value[,] ValueArray(Value[,] array, int width, int height, bool tileable = false, int distance = 1, bool includeAlpha = true)
	{
		return ValueArrayV(ValueArrayH(array, width, height, tileable, distance, includeAlpha), width, height, tileable, distance, includeAlpha);
	}

	public static Value[,] ValueChannel(Channel<Value> channel, bool tileable, int distance = 1, bool includeAlpha = true)
	{
		return ValueArrayV(ValueArrayH(channel.Array, channel.Width, channel.Height, tileable, distance, includeAlpha), channel.Width, channel.Height, tileable, distance, includeAlpha);
	}

	public static Value[,] ValueChannel(Channel<Value> channel, int startX, int startY, int endX, int endY, bool tileable, int distance = 1, bool includeAlpha = true)
	{
		return ValueCroppedArrayV(ValueCroppedArrayH(channel.Array, channel.Width, channel.Height, tileable, startX, startY, endX, endY, distance, includeAlpha), channel.Width, channel.Height, tileable, startX, startY, endX, endY, distance, includeAlpha);
	}

	public static Color[,] ColorArrayH(Color[,] Array, int width, int height, bool tileable, int distance = 1, bool includeAlpha = true)
	{
		Color[,] outputArray = new Color[width, height];
		if (distance < 1)
		{
			distance = 1;
		}
		if (distance > 8)
		{
			distance = 8;
		}
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				for (int xx = -distance; xx <= distance; xx++)
				{
					int xxx;
					if (tileable)
					{
						xxx = Mathf.PosMod(x + xx, width);
					}
					else
					{
						xxx = x + xx;
						if (xxx < 0)
						{
							xxx = 0;
						}
						else if (xxx >= width)
						{
							xxx = width - 1;
						}
					}
					outputArray[x, y].r += Array[xxx, y].r * Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
					outputArray[x, y].g += Array[xxx, y].g * Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
					outputArray[x, y].b += Array[xxx, y].b * Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
					outputArray[x, y].a += Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
				}
				if (outputArray[x, y].a > 0f)
				{
					outputArray[x, y].r /= outputArray[x, y].a;
					outputArray[x, y].g /= outputArray[x, y].a;
					outputArray[x, y].b /= outputArray[x, y].a;
				}
				if (!includeAlpha)
				{
					outputArray[x, y].a = Array[x, y].a;
				}
			}
		}
		return outputArray;
	}

	public static Color[,] ColorCroppedArrayH(Color[,] Array, int width, int height, bool tileable, int startX, int startY, int endX, int endY, int distance = 1, bool includeAlpha = true)
	{
		Color[,] outputArray = new Color[width, height];
		if (distance < 1)
		{
			distance = 1;
		}
		if (distance > 8)
		{
			distance = 8;
		}
		for (int y = startY; y <= endY; y++)
		{
			for (int x = startX; x <= endX; x++)
			{
				for (int xx = -distance; xx <= distance; xx++)
				{
					int xxx;
					if (tileable)
					{
						xxx = Mathf.PosMod(x + xx, width);
					}
					else
					{
						xxx = x + xx;
						if (xxx < 0)
						{
							xxx = 0;
						}
						else if (xxx >= width)
						{
							xxx = width - 1;
						}
					}
					outputArray[x, y].r += Array[xxx, y].r * Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
					outputArray[x, y].g += Array[xxx, y].g * Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
					outputArray[x, y].b += Array[xxx, y].b * Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
					outputArray[x, y].a += Array[xxx, y].a * gaussianKernel[distance - 1][xx + distance];
				}
				if (outputArray[x, y].a > 0f)
				{
					outputArray[x, y].r /= outputArray[x, y].a;
					outputArray[x, y].g /= outputArray[x, y].a;
					outputArray[x, y].b /= outputArray[x, y].a;
				}
				if (!includeAlpha)
				{
					outputArray[x, y].a = Array[x, y].a;
				}
			}
		}
		return outputArray;
	}

	public static Color[,] ColorArrayV(Color[,] Array, int width, int height, bool tileable, int distance = 1, bool includeAlpha = true)
	{
		Color[,] outputArray = new Color[width, height];
		if (distance < 1)
		{
			distance = 1;
		}
		if (distance > 8)
		{
			distance = 8;
		}
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				for (int yy = -distance; yy <= distance; yy++)
				{
					int yyy;
					if (tileable)
					{
						yyy = Mathf.PosMod(y + yy, height);
					}
					else
					{
						yyy = y + yy;
						if (yyy < 0)
						{
							yyy = 0;
						}
						else if (yyy >= height)
						{
							yyy = height - 1;
						}
					}
					outputArray[x, y].r += Array[x, yyy].r * Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
					outputArray[x, y].g += Array[x, yyy].g * Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
					outputArray[x, y].b += Array[x, yyy].b * Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
					outputArray[x, y].a += Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
				}
				if (outputArray[x, y].a > 0f)
				{
					outputArray[x, y].r /= outputArray[x, y].a;
					outputArray[x, y].g /= outputArray[x, y].a;
					outputArray[x, y].b /= outputArray[x, y].a;
				}
				if (!includeAlpha)
				{
					outputArray[x, y].a = Array[x, y].a;
				}
			}
		}
		return outputArray;
	}

	public static Color[,] ColorCroppedArrayV(Color[,] Array, int width, int height, bool tileable, int startX, int startY, int endX, int endY, int distance = 1, bool includeAlpha = true)
	{
		Color[,] outputArray = new Color[width, height];
		if (distance < 1)
		{
			distance = 1;
		}
		if (distance > 8)
		{
			distance = 8;
		}
		for (int y = startY; y <= endY; y++)
		{
			for (int x = startX; x <= endX; x++)
			{
				for (int yy = -distance; yy <= distance; yy++)
				{
					int yyy;
					if (tileable)
					{
						yyy = Mathf.PosMod(y + yy, height);
					}
					else
					{
						yyy = y + yy;
						if (yyy < 0)
						{
							yyy = 0;
						}
						else if (yyy >= height)
						{
							yyy = height - 1;
						}
					}
					outputArray[x, y].r += Array[x, yyy].r * Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
					outputArray[x, y].g += Array[x, yyy].g * Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
					outputArray[x, y].b += Array[x, yyy].b * Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
					outputArray[x, y].a += Array[x, yyy].a * gaussianKernel[distance - 1][yy + distance];
				}
				if (outputArray[x, y].a > 0f)
				{
					outputArray[x, y].r /= outputArray[x, y].a;
					outputArray[x, y].g /= outputArray[x, y].a;
					outputArray[x, y].b /= outputArray[x, y].a;
				}
				if (!includeAlpha)
				{
					outputArray[x, y].a = Array[x, y].a;
				}
			}
		}
		return outputArray;
	}

	public static Color[,] ColorArray(Color[,] array, int width, int height, bool tileable, int distance = 1, bool includeAlpha = true)
	{
		return ColorArrayV(ColorArrayH(array, width, height, tileable, distance, includeAlpha), width, height, tileable, distance, includeAlpha);
	}

	public static Color[,] ColorChannel(Channel<Color> channel, bool tileable = false, int distance = 1, bool includeAlpha = true)
	{
		return ColorArrayV(ColorArrayH(channel.Array, channel.Width, channel.Height, tileable, distance, includeAlpha), channel.Width, channel.Height, tileable, distance, includeAlpha);
	}

	public static Color[,] ColorChannel(Channel<Color> channel, int startX, int startY, int endX, int endY, bool tileable = false, int distance = 1, bool includeAlpha = true)
	{
		return ColorCroppedArrayV(ColorCroppedArrayH(channel.Array, channel.Width, channel.Height, tileable, startX, startY, endX, endY, distance, includeAlpha), channel.Width, channel.Height, tileable, startX, startY, endX, endY, distance, includeAlpha);
	}
}
