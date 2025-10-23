using System;
using Godot;

public static class GradientFilter
{
	public enum OrientationEnum
	{
		X,
		Y,
		Radial
	}

	public static float[,] MaskArray(int width, int height, OrientationEnum orientation, int centerX, int centerY, float distance = 1f, float maxValue = 1f, bool inverse = false, Blender.BlendingModeEnum blendingMode = Blender.BlendingModeEnum.NORMAL, float blendingStrength = 1f)
	{
		float[,] maskArray = new float[width, height];
		maxValue = Mathf.Clamp(maxValue, 0f, 2f);
		switch (orientation)
		{
		case OrientationEnum.Y:
		{
			int edge = Mathf.FloorToInt((float)height * Mathf.Clamp(1f - distance, 0f, 1f));
			float step = maxValue / (float)(height - edge);
			int i = 0;
			for (int y = height - 1; y >= edge; y--)
			{
				for (int x = 0; x < width; x++)
				{
					maskArray[x, y] = (inverse ? (1f - (float)i * step) : ((float)i * step));
				}
				i++;
			}
			step = maxValue / (float)edge;
			i = edge;
			for (int y = edge - 1; y > -1; y--)
			{
				for (int x = 0; x < width; x++)
				{
					maskArray[x, y] = (inverse ? (1f - (float)i * step) : ((float)i * step));
				}
				i--;
			}
			break;
		}
		case OrientationEnum.X:
		{
			int edge = Mathf.FloorToInt((float)width * Mathf.Clamp(1f - distance, 0f, 1f));
			float step = maxValue / (float)(width - edge);
			int i = 0;
			for (int x = width - 1; x >= edge; x--)
			{
				for (int y = 0; y < height; y++)
				{
					maskArray[x, y] = (inverse ? (1f - (float)i * step) : ((float)i * step));
				}
				i++;
			}
			step = maxValue / (float)edge;
			i = edge;
			for (int x = edge - 1; x > -1; x--)
			{
				for (int y = 0; y < height; y++)
				{
					maskArray[x, y] = (inverse ? (1f - (float)i * step) : ((float)i * step));
				}
				i--;
			}
			break;
		}
		case OrientationEnum.Radial:
		{
			int radius = Mathf.RoundToInt(distance * (float)Mathf.Max(width, height) * 0.5f);
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int num = x - centerX;
					int deltaY = y - centerY;
					float radialDistance = Mathf.Sqrt(num * num + deltaY * deltaY);
					float blending = 0f;
					if (radialDistance <= (float)radius)
					{
						blending = maxValue * (0.5f - 0.5f * Mathf.Cos((1f - radialDistance / (float)radius) * Mathf.Pi));
					}
					maskArray[x, y] = (inverse ? (1f - blending) : blending);
				}
			}
			break;
		}
		}
		return maskArray;
	}
}
