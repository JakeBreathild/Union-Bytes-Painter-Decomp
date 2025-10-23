using System.Collections.Generic;
using System.IO;
using Godot;

public class DrawingPreset
{
	public Pixel Data;

	public string Name { get; set; } = "";

	public Image Image { get; set; }

	public ImageTexture ImageTexture { get; }

	public bool IsThumbnailAvailable { get; set; }

	public bool DoUpdateThumbnail { get; set; }

	public bool IsThumbnailRequested { get; set; }

	public DrawingPreset()
	{
		ImageTexture = new ImageTexture();
	}

	public static DrawingPreset[] Interpolation(DrawingPreset drawingPresetA, DrawingPreset drawingPresetB, int steps)
	{
		DrawingPreset[] drawingPresetsArray = new DrawingPreset[steps];
		for (int step = 0; step < steps; step++)
		{
			float weight = (1f + (float)step) / ((float)steps + 1f);
			drawingPresetsArray[step] = new DrawingPreset();
			drawingPresetsArray[step].Name = drawingPresetA.Name + " <" + step + "/" + steps + "> " + drawingPresetB.Name;
			drawingPresetsArray[step].Data.Color = drawingPresetA.Data.Color.LinearInterpolate(drawingPresetB.Data.Color, weight);
			drawingPresetsArray[step].Data.Roughness.v = Mathf.Lerp(drawingPresetA.Data.Roughness.v, drawingPresetB.Data.Roughness.v, weight);
			drawingPresetsArray[step].Data.Roughness.a = 1f;
			drawingPresetsArray[step].Data.Metallicity.v = Mathf.Lerp(drawingPresetA.Data.Metallicity.v, drawingPresetB.Data.Metallicity.v, weight);
			drawingPresetsArray[step].Data.Metallicity.a = 1f;
			drawingPresetsArray[step].Data.Height.v = Mathf.Lerp(drawingPresetA.Data.Height.v, drawingPresetB.Data.Height.v, weight);
			drawingPresetsArray[step].Data.Height.a = 1f;
			drawingPresetsArray[step].Data.Emission = drawingPresetA.Data.Emission.LinearInterpolate(drawingPresetB.Data.Emission, weight);
		}
		return drawingPresetsArray;
	}

	public static List<DrawingPreset> ConvertFromColorList(List<Color> colorList)
	{
		List<DrawingPreset> drawingPresetsList = new List<DrawingPreset>();
		foreach (Color color in colorList)
		{
			DrawingPreset drawingPreset = new DrawingPreset();
			drawingPreset.Name = "Import H " + color.h.ToString("0.00") + "  S " + color.s.ToString("0.00") + "  V " + color.v.ToString("0.00");
			drawingPreset.Data.Color = color;
			drawingPreset.Data.Roughness.v = 1f - color.v / 7f * 0.6f;
			drawingPreset.Data.Roughness.a = 1f;
			drawingPreset.Data.Metallicity.v = 0f;
			drawingPreset.Data.Metallicity.a = 1f;
			drawingPreset.Data.Height.v = 0.5f;
			drawingPreset.Data.Height.a = 1f;
			drawingPreset.Data.Emission = new Color(0f, 0f, 0f);
			drawingPresetsList.Add(drawingPreset);
		}
		return drawingPresetsList;
	}

	public static void AddDefaultDrawingPresets(List<DrawingPreset> drawingPresetsList)
	{
		int count = 9;
		for (int v = 0; v <= count; v++)
		{
			DrawingPreset drawingPreset = new DrawingPreset();
			drawingPreset.Name = "Black V " + ((float)v / ((float)count + 1f)).ToString("0.00");
			drawingPreset.Data.Color = Color.FromHsv(0f, 0f, 1f - (float)v / (1f * (float)count));
			drawingPreset.Data.Roughness.v = 1f - (float)v / (1f * (float)count) * 0.6f;
			drawingPreset.Data.Roughness.a = 1f;
			drawingPreset.Data.Metallicity.v = 0f;
			drawingPreset.Data.Metallicity.a = 1f;
			drawingPreset.Data.Height.v = 0.5f;
			drawingPreset.Data.Height.a = 1f;
			drawingPreset.Data.Emission = new Color(0f, 0f, 0f);
			drawingPresetsList.Add(drawingPreset);
		}
		count = 4;
		for (int h = 0; h < 16; h++)
		{
			for (int i = 0; i <= count; i++)
			{
				DrawingPreset drawingPreset2 = new DrawingPreset();
				drawingPreset2.Name = "Default H " + ((float)h / 16f).ToString("0.00") + "  S 0.80  V " + (1f - (float)i / (1f * (float)count + 1.5f)).ToString("0.00");
				drawingPreset2.Data.Color = Color.FromHsv((float)h / 16f + (1f - (float)i / (1f * (float)count + 1.5f)) * 0.02f, 0.8f, 1f - (float)i / (1f * (float)count + 1.5f));
				drawingPreset2.Data.Roughness.v = 1f - (float)i / (1f * (float)count) * 0.6f;
				drawingPreset2.Data.Roughness.a = 1f;
				drawingPreset2.Data.Metallicity.v = 0f;
				drawingPreset2.Data.Metallicity.a = 1f;
				drawingPreset2.Data.Height.v = 0.5f;
				drawingPreset2.Data.Height.a = 1f;
				drawingPreset2.Data.Emission = new Color(0f, 0f, 0f);
				drawingPresetsList.Add(drawingPreset2);
			}
		}
		count = 12;
		for (int j = 0; j < 12; j++)
		{
			DrawingPreset drawingPreset3 = new DrawingPreset();
			drawingPreset3.Name = "Glow H " + ((float)j / 16f).ToString("0.00") + "  S 1.00  V 0.80";
			drawingPreset3.Data.Color = Color.FromHsv((float)j / 12f, 1f, 0.8f);
			drawingPreset3.Data.Roughness.v = 1f;
			drawingPreset3.Data.Roughness.a = 1f;
			drawingPreset3.Data.Metallicity.v = 0f;
			drawingPreset3.Data.Metallicity.a = 1f;
			drawingPreset3.Data.Emission = drawingPreset3.Data.Color;
			drawingPreset3.Data.Height.v = 0.5f;
			drawingPreset3.Data.Height.a = 1f;
			drawingPresetsList.Add(drawingPreset3);
		}
	}

	public void WriteToBinaryStream(BinaryWriter binaryWriter)
	{
		binaryWriter.Write(Name);
		binaryWriter.Write((byte)Data.Color.r8);
		binaryWriter.Write((byte)Data.Color.g8);
		binaryWriter.Write((byte)Data.Color.b8);
		binaryWriter.Write((byte)Data.Color.a8);
		binaryWriter.Write(Data.Height.v);
		binaryWriter.Write(Data.Roughness.v);
		binaryWriter.Write(Data.Metallicity.v);
		binaryWriter.Write((byte)Data.Emission.r8);
		binaryWriter.Write((byte)Data.Emission.g8);
		binaryWriter.Write((byte)Data.Emission.b8);
	}

	public static DrawingPreset ReadFromBinaryStream_009(BinaryReader binaryReader)
	{
		return new DrawingPreset
		{
			Name = binaryReader.ReadString(),
			Data = 
			{
				Color = 
				{
					r = binaryReader.ReadSingle(),
					g = binaryReader.ReadSingle(),
					b = binaryReader.ReadSingle(),
					a = binaryReader.ReadSingle()
				},
				Height = 
				{
					v = binaryReader.ReadSingle(),
					a = 1f
				},
				Roughness = 
				{
					v = binaryReader.ReadSingle(),
					a = 1f
				},
				Metallicity = 
				{
					v = binaryReader.ReadSingle(),
					a = 1f
				},
				Emission = 
				{
					r = binaryReader.ReadSingle(),
					g = binaryReader.ReadSingle(),
					b = binaryReader.ReadSingle(),
					a = 1f
				}
			}
		};
	}

	public static DrawingPreset ReadFromBinaryStream(BinaryReader binaryReader)
	{
		return new DrawingPreset
		{
			Name = binaryReader.ReadString(),
			Data = 
			{
				Color = 
				{
					r8 = binaryReader.ReadByte(),
					g8 = binaryReader.ReadByte(),
					b8 = binaryReader.ReadByte(),
					a8 = binaryReader.ReadByte()
				},
				Height = 
				{
					v = binaryReader.ReadSingle(),
					a = 1f
				},
				Roughness = 
				{
					v = binaryReader.ReadSingle(),
					a = 1f
				},
				Metallicity = 
				{
					v = binaryReader.ReadSingle(),
					a = 1f
				},
				Emission = 
				{
					r8 = binaryReader.ReadByte(),
					g8 = binaryReader.ReadByte(),
					b8 = binaryReader.ReadByte(),
					a = 1f
				}
			}
		};
	}
}
