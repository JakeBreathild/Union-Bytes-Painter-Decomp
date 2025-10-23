using System;
using Godot;

public static class AmbientOcclusionFilter
{
	public enum RayModeEnum
	{
		RANDOM,
		UNIFORM
	}

	private static Worksheet worksheet = null;

	private static RayModeEnum rayMode = RayModeEnum.UNIFORM;

	private static int raysCount = 32;

	private static float radius = 4f;

	private static float bias = 0.01f;

	private static float intensity = 1f;

	private static bool allLayers = true;

	private static float[,] BakeMaskArray(int x, int y, int width, int height)
	{
		GD.Randomize();
		Value[,] heightArray = ((!allLayers) ? worksheet.Data.Layer.HeightChannel.Array : worksheet.Data.HeightChannel.Array);
		bool tileable = worksheet.Data.Tileable;
		RayModeEnum rayMode = AmbientOcclusionFilter.rayMode;
		int raysCount = AmbientOcclusionFilter.raysCount;
		float radius = AmbientOcclusionFilter.radius;
		float bias = AmbientOcclusionFilter.bias;
		float intensity = AmbientOcclusionFilter.intensity;
		if (intensity < 0.1f)
		{
			intensity = 0.1f;
		}
		float[,] outputArray = new float[width + 1, height + 1];
		int startX = Mathf.Max(0, x);
		int startY = Mathf.Max(0, y);
		int endX = Mathf.Min(x + width, worksheet.Data.Width - 1);
		int endY = Mathf.Min(y + height, worksheet.Data.Height - 1);
		Vector3[] kernel = new Vector3[raysCount];
		if (rayMode == RayModeEnum.UNIFORM)
		{
			for (int i = 0; i < raysCount; i++)
			{
				kernel[i] = Vector3Extension.UniformDirection(Vector3.Forward, i, raysCount);
			}
		}
		Vector3 position = Vector3.Zero;
		Vector2i sampleIntegerPosition = default(Vector2i);
		for (int yy = startY; yy <= endY; yy++)
		{
			for (int xx = startX; xx <= endX; xx++)
			{
				position.x = (float)xx + 0.5f;
				position.y = (float)yy + 0.5f;
				position.z = heightArray[xx, yy].v;
				int samples = 0;
				float occlusion = 0f;
				for (int j = 0; j < raysCount; j++)
				{
					if (rayMode == RayModeEnum.RANDOM)
					{
						kernel[j] = Vector3Extension.RandomDirection(Vector3.Forward);
					}
					Vector2i samplePosition = new Vector2i(position.x, position.y);
					Vector2 rayStepSize = new Vector2(Mathf.Sqrt(1f + kernel[j].y / kernel[j].x * (kernel[j].y / kernel[j].x)), Mathf.Sqrt(1f + kernel[j].x / kernel[j].y * (kernel[j].x / kernel[j].y)));
					Vector2 rayLength = Vector2.Zero;
					Vector2i step = Vector2i.Zero;
					if (kernel[j].x < 0f)
					{
						step.x = -1;
						rayLength.x = (position.x - (float)samplePosition.x) * rayStepSize.x;
					}
					else
					{
						step.x = 1;
						rayLength.x = ((float)samplePosition.x + 1f - position.x) * rayStepSize.x;
					}
					if (kernel[j].y < 0f)
					{
						step.y = -1;
						rayLength.y = (position.y - (float)samplePosition.y) * rayStepSize.y;
					}
					else
					{
						step.y = 1;
						rayLength.y = ((float)samplePosition.y + 1f - position.y) * rayStepSize.y;
					}
					float distance = 0f;
					bool isOccluded = false;
					while (distance < radius)
					{
						if (rayLength.x < rayLength.y)
						{
							samplePosition.x += step.x;
							distance = rayLength.x;
							rayLength.x += rayStepSize.x;
						}
						else
						{
							samplePosition.y += step.y;
							distance = rayLength.y;
							rayLength.y += rayStepSize.y;
						}
						sampleIntegerPosition.x = Mathf.RoundToInt(samplePosition.x);
						sampleIntegerPosition.y = Mathf.RoundToInt(samplePosition.y);
						if (tileable)
						{
							sampleIntegerPosition.x = Mathf.PosMod(sampleIntegerPosition.x, worksheet.Data.Width);
							sampleIntegerPosition.y = Mathf.PosMod(sampleIntegerPosition.y, worksheet.Data.Height);
						}
						else if (sampleIntegerPosition.x < 0 || sampleIntegerPosition.x >= worksheet.Data.Width || sampleIntegerPosition.y < 0 || sampleIntegerPosition.y >= worksheet.Data.Height)
						{
							isOccluded = true;
							break;
						}
						if (heightArray[sampleIntegerPosition.x, sampleIntegerPosition.y].v > position.z + bias)
						{
							float scale = 1f - distance / radius;
							occlusion += (heightArray[sampleIntegerPosition.x, sampleIntegerPosition.y].v - position.z) * scale * scale;
							samples++;
							isOccluded = true;
							break;
						}
					}
					if (!isOccluded)
					{
						samples++;
					}
				}
				occlusion /= (float)samples;
				outputArray[xx - startX, yy - startY] = Mathf.Clamp(occlusion * intensity * 8f, 0f, 1f);
			}
		}
		return outputArray;
	}

	public static void MaskArray(Action<float[,], bool> threadCompleteCallback, Worksheet worksheet, float radius = 4f, float bias = 0.01f, int raysCount = 32, RayModeEnum rayMode = RayModeEnum.UNIFORM, float intensity = 1f, bool allLayers = true)
	{
		if (!Register.ThreadsManager.IsBusy)
		{
			AmbientOcclusionFilter.worksheet = worksheet;
			AmbientOcclusionFilter.radius = radius;
			AmbientOcclusionFilter.bias = bias;
			AmbientOcclusionFilter.raysCount = raysCount;
			AmbientOcclusionFilter.rayMode = rayMode;
			AmbientOcclusionFilter.intensity = intensity;
			AmbientOcclusionFilter.allLayers = allLayers;
			Register.ThreadsManager.Update(worksheet);
			Register.ThreadsManager.Start(BakeMaskArray, threadCompleteCallback);
		}
	}
}
