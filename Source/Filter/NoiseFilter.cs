using System;
using Godot;

public static class NoiseFilter
{
	private static FastNoise fastNoise = new FastNoise();

	private static FastNoiseLite fastNoiseLite = new FastNoiseLite();

	private static float frequency = 0.01f;

	private static int octaves = 3;

	public static float Frequency
	{
		get
		{
			return frequency;
		}
		set
		{
			frequency = value;
			fastNoise.SetFrequency(frequency);
			fastNoiseLite.SetFrequency(frequency);
		}
	}

	public static int Octaves
	{
		get
		{
			return octaves;
		}
		set
		{
			octaves = value;
			fastNoise.SetFractalOctaves(octaves);
			fastNoiseLite.SetFractalOctaves(octaves);
		}
	}

	private static void SetNoise(int seed)
	{
		fastNoise.SetSeed(seed);
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(frequency);
		fastNoise.SetFractalOctaves(octaves);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Euclidean);
		fastNoise.SetCellularReturnType(FastNoise.CellularReturnType.Distance2Mul);
		fastNoiseLite.SetSeed(seed);
		fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		fastNoiseLite.SetFrequency(frequency);
		fastNoiseLite.SetFractalOctaves(octaves);
		fastNoiseLite.SetFractalType(FastNoiseLite.FractalType.FBm);
		fastNoiseLite.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
		fastNoiseLite.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Mul);
	}

	public static float[,] MaskArray(int width, int height, int type, int seed, float scale)
	{
		float[,] maskArray = new float[width, height];
		Vector2 offset = new Vector2(Mathf.Sin(0.423354f * (float)seed) * scale, Mathf.Cos(0.198125f * (float)seed) * scale);
		float value = 0f;
		switch (type)
		{
		case 0:
			GD.Seed((ulong)seed);
			break;
		default:
			SetNoise(seed);
			fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
			break;
		case 2:
			fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
			break;
		case 3:
			fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
			break;
		case 4:
			fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
			break;
		case 5:
			fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.Value);
			break;
		case 6:
			fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
			break;
		}
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				float noiseX = 1f * (float)x / (float)width * scale + offset.x;
				float noiseY = 1f * (float)y / (float)width * scale + offset.y;
				maskArray[x, y] = type switch
				{
					0 => GD.Randf(), 
					7 => fastNoise.GetSimplex(scale * Mathf.Cos(1f + noiseX / scale * 2f * Mathf.Pi), scale * Mathf.Sin(1f + noiseX / scale * 2f * Mathf.Pi), scale * Mathf.Cos(noiseY / scale * 2f * Mathf.Pi), scale * Mathf.Sin(noiseY / scale * 2f * Mathf.Pi)) * 0.5f + 0.5f, 
					_ => fastNoiseLite.GetNoise(noiseX, noiseY) * 0.5f + 0.5f, 
				};
			}
		}
		return maskArray;
	}
}
