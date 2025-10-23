using System;
using Godot;

public static class CurvatureFilter
{
	public enum RayModeEnum
	{
		RANDOM,
		UNIFORM
	}

	private static Worksheet worksheet = null;

	private static int steps = 1;

	private static float intensity = 1f;

	private static float[,] BakeMaskArray(int x, int y, int width, int height, bool tileable, float distance, float intensity)
	{
		if (intensity < 0.1f)
		{
			intensity = 0.1f;
		}
		float radius = distance * 0.5f;
		Color[,] normalArray = worksheet.Data.NormalChannel.Array;
		float[,] outputArray = new float[width + 1, height + 1];
		int startX = Mathf.Max(0, x);
		int startY = Mathf.Max(0, y);
		int endX = Mathf.Min(x + width, worksheet.Data.Width - 1);
		int endY = Mathf.Min(y + height, worksheet.Data.Height - 1);
		for (int yy = startY; yy <= endY; yy++)
		{
			for (int xx = startX; xx <= endX; xx++)
			{
				Vector2i samplePosition = new Vector2i(Mathf.FloorToInt(1f * (float)xx - radius), yy);
				if (tileable)
				{
					samplePosition.x = Mathf.PosMod(samplePosition.x, worksheet.Data.Width);
					samplePosition.y = Mathf.PosMod(samplePosition.y, worksheet.Data.Height);
				}
				else if (samplePosition.x < 0 || samplePosition.x >= worksheet.Data.Width || samplePosition.y < 0 || samplePosition.y >= worksheet.Data.Height)
				{
					samplePosition = new Vector2i(xx, yy);
				}
				float negXValue = normalArray[samplePosition.x, samplePosition.y].r;
				samplePosition = new Vector2i(Mathf.FloorToInt(1f * (float)xx + radius), yy);
				if (tileable)
				{
					samplePosition.x = Mathf.PosMod(samplePosition.x, worksheet.Data.Width);
					samplePosition.y = Mathf.PosMod(samplePosition.y, worksheet.Data.Height);
				}
				else if (samplePosition.x < 0 || samplePosition.x >= worksheet.Data.Width || samplePosition.y < 0 || samplePosition.y >= worksheet.Data.Height)
				{
					samplePosition = new Vector2i(xx, yy);
				}
				float posXValue = normalArray[samplePosition.x, samplePosition.y].r;
				samplePosition = new Vector2i(xx, Mathf.FloorToInt(1f * (float)yy - radius));
				if (tileable)
				{
					samplePosition.x = Mathf.PosMod(samplePosition.x, worksheet.Data.Width);
					samplePosition.y = Mathf.PosMod(samplePosition.y, worksheet.Data.Height);
				}
				else if (samplePosition.x < 0 || samplePosition.x >= worksheet.Data.Width || samplePosition.y < 0 || samplePosition.y >= worksheet.Data.Height)
				{
					samplePosition = new Vector2i(xx, yy);
				}
				float negYValue = normalArray[samplePosition.x, samplePosition.y].g;
				samplePosition = new Vector2i(xx, Mathf.FloorToInt(1f * (float)yy + radius));
				if (tileable)
				{
					samplePosition.x = Mathf.PosMod(samplePosition.x, worksheet.Data.Width);
					samplePosition.y = Mathf.PosMod(samplePosition.y, worksheet.Data.Height);
				}
				else if (samplePosition.x < 0 || samplePosition.x >= worksheet.Data.Width || samplePosition.y < 0 || samplePosition.y >= worksheet.Data.Height)
				{
					samplePosition = new Vector2i(xx, yy);
				}
				float posYValue = normalArray[samplePosition.x, samplePosition.y].g;
				float target = posXValue - negXValue + 0.5f;
				float blend = negYValue - posYValue + 0.5f;
				float value = ((!(target > 0.5f)) ? (2f * target * blend) : (1f - (1f - 2f * (target - 0.5f)) * (1f - blend)));
				outputArray[xx - startX, yy - startY] = Mathf.Clamp(value * intensity, 0f, 1f);
			}
		}
		return outputArray;
	}

	private static float[,] BakeMaskArray(Color[,] normalArray, int width, int height, bool tileable, float distance, float intensity)
	{
		if (intensity < 0.1f)
		{
			intensity = 0.1f;
		}
		float radius = distance * 0.5f;
		float[,] outputArray = new float[width + 1, height + 1];
		for (int y = 0; y < width; y++)
		{
			for (int x = 0; x < height; x++)
			{
				Vector2i samplePosition = new Vector2i(Mathf.FloorToInt(1f * (float)x - radius), y);
				if (tileable)
				{
					samplePosition.x = Mathf.PosMod(samplePosition.x, width);
					samplePosition.y = Mathf.PosMod(samplePosition.y, height);
				}
				else if (samplePosition.x < 0 || samplePosition.x >= width || samplePosition.y < 0 || samplePosition.y >= height)
				{
					samplePosition = new Vector2i(x, y);
				}
				float negXValue = normalArray[samplePosition.x, samplePosition.y].r;
				samplePosition = new Vector2i(Mathf.FloorToInt(1f * (float)x + radius), y);
				if (tileable)
				{
					samplePosition.x = Mathf.PosMod(samplePosition.x, width);
					samplePosition.y = Mathf.PosMod(samplePosition.y, height);
				}
				else if (samplePosition.x < 0 || samplePosition.x >= width || samplePosition.y < 0 || samplePosition.y >= height)
				{
					samplePosition = new Vector2i(x, y);
				}
				float posXValue = normalArray[samplePosition.x, samplePosition.y].r;
				samplePosition = new Vector2i(x, Mathf.FloorToInt(1f * (float)y - radius));
				if (tileable)
				{
					samplePosition.x = Mathf.PosMod(samplePosition.x, width);
					samplePosition.y = Mathf.PosMod(samplePosition.y, height);
				}
				else if (samplePosition.x < 0 || samplePosition.x >= width || samplePosition.y < 0 || samplePosition.y >= height)
				{
					samplePosition = new Vector2i(x, y);
				}
				float g = normalArray[samplePosition.x, samplePosition.y].g;
				samplePosition = new Vector2i(x, Mathf.FloorToInt(1f * (float)y + radius));
				if (tileable)
				{
					samplePosition.x = Mathf.PosMod(samplePosition.x, width);
					samplePosition.y = Mathf.PosMod(samplePosition.y, height);
				}
				else if (samplePosition.x < 0 || samplePosition.x >= width || samplePosition.y < 0 || samplePosition.y >= height)
				{
					samplePosition = new Vector2i(x, y);
				}
				float posYValue = normalArray[samplePosition.x, samplePosition.y].g;
				float target = posXValue - negXValue + 0.5f;
				float blend = g - posYValue + 0.5f;
				float value = ((!(target > 0.5f)) ? (2f * target * blend) : (1f - (1f - 2f * (target - 0.5f)) * (1f - blend)));
				outputArray[x, y] = Mathf.Clamp(value * intensity, 0f, 1f);
			}
		}
		return outputArray;
	}

	private static float[,] BakeMaskArray(int x, int y, int width, int height)
	{
		int steps = CurvatureFilter.steps;
		float distance = 1f;
		float[,] outputArray = BakeMaskArray(x, y, width, height, worksheet.Data.Tileable, distance, intensity);
		if (steps > 1)
		{
			outputArray = BlurFilter.FloatArray(outputArray, width, height, worksheet.Data.Tileable, Mathf.RoundToInt(2f * distance));
		}
		int startX = Mathf.Max(0, x);
		int startY = Mathf.Max(0, y);
		int endX = Mathf.Min(x + width, worksheet.Data.Width - 1);
		int endY = Mathf.Min(y + height, worksheet.Data.Height - 1);
		for (int i = 2; i < steps; i++)
		{
			distance *= 2f;
			float[,] stepOutputArray = BakeMaskArray(x, y, width, height, worksheet.Data.Tileable, distance, intensity);
			stepOutputArray = BlurFilter.FloatArray(stepOutputArray, width, height, worksheet.Data.Tileable, Mathf.RoundToInt(2f * distance));
			for (int yy = startY; yy <= endY; yy++)
			{
				for (int xx = startX; xx <= endX; xx++)
				{
					float target = outputArray[xx, yy];
					float blend = stepOutputArray[xx, yy];
					float value = ((!(target > 0.5f)) ? (2f * target * blend) : (1f - (1f - 2f * (target - 0.5f)) * (1f - blend)));
					outputArray[xx - startX, yy - startY] = Mathf.Clamp(value, 0f, 1f);
				}
			}
		}
		return outputArray;
	}

	public static void MaskArray(Action<float[,], bool> threadCompleteCallback, Worksheet worksheet, int steps = 1, float intensity = 1f)
	{
		if (!Register.ThreadsManager.IsBusy)
		{
			CurvatureFilter.worksheet = worksheet;
			CurvatureFilter.steps = Mathf.Clamp(steps, 1, 7);
			CurvatureFilter.intensity = intensity;
			Register.ThreadsManager.Update(worksheet);
			Register.ThreadsManager.Start(BakeMaskArray, threadCompleteCallback);
		}
	}

	public static float[,] MaskArray(Color[,] normalArray, int width, int height, bool tileable = false, int steps = 1, float intensity = 1f)
	{
		float distance = 1f;
		float[,] outputArray = BakeMaskArray(normalArray, width, height, tileable, distance, intensity);
		if (steps > 1)
		{
			outputArray = BlurFilter.FloatArray(outputArray, width, height, tileable, Mathf.RoundToInt(distance));
		}
		for (int i = 2; i < steps; i++)
		{
			distance *= 2f;
			float[,] stepOutputArray = BakeMaskArray(normalArray, width, height, tileable, distance, intensity);
			stepOutputArray = BlurFilter.FloatArray(stepOutputArray, width, height, tileable, Mathf.RoundToInt(distance));
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					float target = outputArray[x, y];
					float blend = stepOutputArray[x, y];
					float value = ((!(target > 0.5f)) ? (2f * target * blend) : (1f - (1f - 2f * (target - 0.5f)) * (1f - blend)));
					outputArray[x, y] = Mathf.Clamp(value, 0f, 1f);
				}
			}
		}
		return outputArray;
	}
}
