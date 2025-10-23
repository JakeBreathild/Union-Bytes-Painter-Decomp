using System;
using System.Collections.Generic;
using System.IO;
using Godot;

public class ColorPalette : GridContainer
{
	private Panel parentPanel;

	[Export(PropertyHint.None, "")]
	private int columns = 12;

	[Export(PropertyHint.None, "")]
	private int rows = 12;

	private int entries;

	[Export(PropertyHint.None, "")]
	private int seperationSpace = 4;

	private int entryWidth;

	private Color currentColor;

	public ColorRect[,] colorRectsArray;

	private Action<Color> colorSelectedCallback;

	public int Rows => rows;

	public int Entries => entries;

	public int SeperationSpace => seperationSpace;

	public Color CurrentColor
	{
		get
		{
			return currentColor;
		}
		set
		{
			currentColor = value;
		}
	}

	public Action<Color> ColorSelectedCallback
	{
		get
		{
			return colorSelectedCallback;
		}
		set
		{
			colorSelectedCallback = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		parentPanel = GetParentOrNull<Panel>();
		entryWidth = (Mathf.RoundToInt(base.RectSize.x) - columns * seperationSpace) / columns;
		entryWidth = 18;
		base.Columns = columns;
		entries = columns * rows;
		colorRectsArray = new ColorRect[columns, rows];
		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < columns; x++)
			{
				ColorRect colorRect = new ColorRect();
				colorRect.MouseFilter = MouseFilterEnum.Pass;
				colorRect.RectMinSize = new Vector2(entryWidth, entryWidth);
				colorRect.Color = new Color(0f, 0f, 0f);
				colorRect.Name = "[" + x + "," + y + "]";
				AddChild(colorRect);
				colorRect.Owner = this;
				colorRectsArray[x, y] = colorRect;
			}
		}
		Reset();
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);
		if (parentPanel != null && @event is InputEventMouseMotion mme)
		{
			int divider = entryWidth + seperationSpace;
			int x = Mathf.FloorToInt(mme.Position.x / (float)divider);
			int y = Mathf.FloorToInt(mme.Position.y / (float)divider);
			if (x >= columns || x < 0 || y >= rows || y < 0)
			{
				parentPanel.HintTooltip = "";
				return;
			}
			Color color = colorRectsArray[x, y].Color;
			parentPanel.HintTooltip = "R" + Mathf.RoundToInt(color.r * 255f) + " G" + Mathf.RoundToInt(color.g * 255f) + " B" + Mathf.RoundToInt(color.b * 255f) + " A" + Mathf.RoundToInt(color.a * 255f);
			parentPanel.HintTooltip += "\n\n[LMB] Pick Color\n[RMB]  Set Color";
		}
		if (!(@event is InputEventMouseButton { Pressed: not false } mbe))
		{
			return;
		}
		int divider2 = entryWidth + seperationSpace;
		int x2 = Mathf.FloorToInt(mbe.Position.x / (float)divider2);
		int y2 = Mathf.FloorToInt(mbe.Position.y / (float)divider2);
		if (x2 < columns && x2 >= 0 && y2 < rows && y2 >= 0)
		{
			Color color2 = colorRectsArray[x2, y2].Color;
			if (colorSelectedCallback != null && mbe.ButtonIndex == 1)
			{
				colorSelectedCallback(color2);
			}
			else if (mbe.ButtonIndex == 2)
			{
				colorRectsArray[x2, y2].Color = currentColor;
			}
		}
	}

	public void Reset()
	{
		int index = 0;
		int count = 12;
		for (int v = 0; v < count; v++)
		{
			Color color = Color.FromHsv(0f, 0f, 1f - (float)v / (1f * (float)count));
			colorRectsArray[index % columns, index / columns].Color = color;
			index++;
		}
		count = 6;
		for (int h = 0; h < 22; h++)
		{
			for (int i = 0; i < count; i++)
			{
				Color color2 = Color.FromHsv((float)h / 22f + (1f - (float)i / (1f * (float)count + 1.5f)) * 0.02f, 0.8f, 1f - (float)i / (1f * (float)count + 1.5f));
				colorRectsArray[index % columns, index / columns].Color = color2;
				index++;
			}
		}
	}

	public Color GetColor(int x, int y)
	{
		return colorRectsArray[x, y].Color;
	}

	public List<Color> GetColors()
	{
		List<Color> colorsList = new List<Color>();
		for (int i = 0; i < entries; i++)
		{
			colorsList.Add(colorRectsArray[i % columns, i / columns].Color);
		}
		return colorsList;
	}

	public void SetColor(int x, int y, Color color)
	{
		colorRectsArray[x, y].Color = color;
	}

	public void SetColor(int i, Color color)
	{
		if (i < entries)
		{
			colorRectsArray[i % columns, i / columns].Color = color;
		}
	}

	public void SetColors(List<Color> colorsList, int index = 0)
	{
		for (int i = 0; i < colorsList.Count && i + index < entries; i++)
		{
			colorRectsArray[(i + index) % columns, (i + index) / columns].Color = colorsList[i];
		}
	}

	public void WriteToBinaryStream(BinaryWriter binaryWriter)
	{
		binaryWriter.Write(entries);
		for (int i = 0; i < entries; i++)
		{
			binaryWriter.Write((byte)colorRectsArray[i % columns, i / columns].Color.r8);
			binaryWriter.Write((byte)colorRectsArray[i % columns, i / columns].Color.g8);
			binaryWriter.Write((byte)colorRectsArray[i % columns, i / columns].Color.b8);
			binaryWriter.Write((byte)colorRectsArray[i % columns, i / columns].Color.a8);
		}
	}

	public void ReadFromBinaryStream_009(BinaryReader binaryReader)
	{
		int entries = binaryReader.ReadInt32();
		Color color = default(Color);
		for (int i = 0; i < entries && i < columns * rows; i++)
		{
			color.r = binaryReader.ReadSingle();
			color.g = binaryReader.ReadSingle();
			color.b = binaryReader.ReadSingle();
			color.a = binaryReader.ReadSingle();
			colorRectsArray[i % columns, i / columns].Color = color;
		}
	}

	public void ReadFromBinaryStream(BinaryReader binaryReader)
	{
		int entries = binaryReader.ReadInt32();
		for (int i = 0; i < entries && i < columns * rows; i++)
		{
			Color color = new Color
			{
				r8 = binaryReader.ReadByte(),
				g8 = binaryReader.ReadByte(),
				b8 = binaryReader.ReadByte(),
				a8 = binaryReader.ReadByte()
			};
			colorRectsArray[i % columns, i / columns].Color = color;
		}
	}

	public void ImportColorPalette(string file)
	{
		Color color = new Color(0f, 0f, 0f);
		List<Color> colorsList = new List<Color>();
		if (!System.IO.File.Exists(file))
		{
			return;
		}
		string filename = System.IO.Path.GetFileName(file);
		if (filename.Right(filename.Length - 8) == "aseprite" || filename.Right(filename.Length - 3) == "ase")
		{
			GD.Print("Import Color Palette from Aseprite File:" + filename);
			BinaryReader binaryReader = new BinaryReader(System.IO.File.OpenRead(file));
			binaryReader.ReadUInt32();
			binaryReader.ReadUInt16();
			ushort frames = binaryReader.ReadUInt16();
			binaryReader.ReadBytes(120);
			for (int f = 0; f < frames; f++)
			{
				binaryReader.ReadUInt32();
				binaryReader.ReadUInt16();
				ushort oldChunks = binaryReader.ReadUInt16();
				binaryReader.ReadUInt16();
				binaryReader.ReadBytes(2);
				uint chunks = binaryReader.ReadUInt32();
				if (chunks == 0)
				{
					chunks = oldChunks;
				}
				for (int c = 0; c < chunks; c++)
				{
					uint chunkSize = binaryReader.ReadUInt32();
					ushort chunkType = binaryReader.ReadUInt16();
					bool paletteFound = false;
					switch (chunkType)
					{
					case 8199:
					{
						ushort num2 = binaryReader.ReadUInt16();
						binaryReader.ReadUInt16();
						binaryReader.ReadSingle();
						binaryReader.ReadBytes(8);
						if (num2 == 2)
						{
							uint profileLength = binaryReader.ReadUInt32();
							binaryReader.ReadBytes((int)profileLength);
						}
						break;
					}
					case 4:
					{
						GD.Print("Palette found 0x0004");
						ushort numberOfPackets = binaryReader.ReadUInt16();
						for (int p = 0; p < numberOfPackets; p++)
						{
							binaryReader.ReadByte();
							uint paletteSize = binaryReader.ReadByte();
							if (paletteSize == 0)
							{
								paletteSize = 256u;
							}
							colorsList.Clear();
							int i;
							for (i = 0; i < paletteSize; i++)
							{
								color.r8 = binaryReader.ReadByte();
								color.g8 = binaryReader.ReadByte();
								color.b8 = binaryReader.ReadByte();
								color.a = 1f;
								colorsList.Add(color);
							}
							if (i < columns * rows - 1)
							{
								color = new Color(0f, 0f, 0f);
								for (int j = i; j < columns * rows; j++)
								{
									colorsList.Add(color);
								}
							}
							SetColors(colorsList);
						}
						break;
					}
					case 8217:
					{
						GD.Print("Palette found 0x2019");
						uint paletteSize = binaryReader.ReadUInt32();
						binaryReader.ReadUInt32();
						binaryReader.ReadUInt32();
						binaryReader.ReadBytes(8);
						colorsList.Clear();
						int i;
						for (i = 0; i < paletteSize; i++)
						{
							ushort num = binaryReader.ReadUInt16();
							color.r8 = binaryReader.ReadByte();
							color.g8 = binaryReader.ReadByte();
							color.b8 = binaryReader.ReadByte();
							color.a8 = binaryReader.ReadByte();
							if (num > 0)
							{
								ushort stringLength = binaryReader.ReadUInt16();
								binaryReader.ReadBytes(stringLength);
							}
							colorsList.Add(color);
						}
						if (i < columns * rows - 1)
						{
							color = new Color(0f, 0f, 0f);
							for (int ii = i; ii < columns * rows; ii++)
							{
								colorsList.Add(color);
							}
						}
						SetColors(colorsList);
						paletteFound = true;
						break;
					}
					default:
						binaryReader.ReadBytes((int)chunkSize);
						break;
					}
					if (paletteFound)
					{
						break;
					}
				}
			}
			binaryReader.Dispose();
		}
		else
		{
			switch (filename.Right(filename.Length - 3))
			{
			case "gpl":
			{
				StreamReader streamReader = new StreamReader(file);
				if (streamReader.ReadLine().Trim() == "GIMP Palette")
				{
					GD.Print("Import Color Palette from GIMP Palette File:" + filename);
					color = new Color(0f, 0f, 0f);
					colorsList.Clear();
					while (!streamReader.EndOfStream)
					{
						string line = streamReader.ReadLine().Trim();
						if (!(line.Left(1) != "#"))
						{
							continue;
						}
						string[] wordsArray = line.Split(' ', '\t');
						List<string> wordsList = new List<string>();
						for (int k = 0; k < wordsArray.Length; k++)
						{
							wordsArray[k] = wordsArray[k].Trim();
							if (wordsArray[k] != "")
							{
								wordsList.Add(wordsArray[k]);
							}
						}
						if (wordsList.Count < 3 || !wordsList[0].IsValidInteger())
						{
							continue;
						}
						int ci = 0;
						for (int l = 0; l < wordsList.Count; l++)
						{
							if (wordsList[l].IsValidInteger())
							{
								switch (ci)
								{
								case 0:
									color.r8 = wordsList[l].ToInt();
									break;
								case 1:
									color.g8 = wordsList[l].ToInt();
									break;
								case 2:
									color.b8 = wordsList[l].ToInt();
									break;
								}
								ci++;
							}
						}
						colorsList.Add(color);
					}
					if (colorsList.Count < columns * rows - 1)
					{
						color = new Color(0f, 0f, 0f);
						for (int m = colorsList.Count; m < columns * rows; m++)
						{
							colorsList.Add(color);
						}
					}
					SetColors(colorsList);
				}
				streamReader.Dispose();
				break;
			}
			case "jpg":
			case "png":
				GD.Print("Import Color Palette from Image File:" + filename);
				GD.Print("WIP!!!!!!!!!!!!!");
				break;
			}
		}
		Settings.ImportPath = System.IO.Path.GetFullPath(file);
	}
}
