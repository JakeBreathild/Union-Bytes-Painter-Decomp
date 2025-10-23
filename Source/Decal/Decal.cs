using System.IO;
using System.IO.Compression;
using Godot;

public class Decal
{
	private Image thumbnail;

	private string name = "";

	private int width;

	private int height;

	private int arraySize = -1;

	private float[,] maskArray;

	private Color[,] colorArray;

	private Value[,] roughnessArray;

	private Value[,] metallicityArray;

	private Value[,] heightArray;

	private Color[,] emissionArray;

	private bool isLoaded;

	public Image Thumbnail
	{
		get
		{
			return thumbnail;
		}
		set
		{
			thumbnail = value;
		}
	}

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public int Width
	{
		get
		{
			return width;
		}
		set
		{
			width = value;
		}
	}

	public int Height
	{
		get
		{
			return height;
		}
		set
		{
			height = value;
		}
	}

	public int ArraySize
	{
		get
		{
			return arraySize;
		}
		set
		{
			arraySize = value;
		}
	}

	public float[,] MaskArray
	{
		get
		{
			return maskArray;
		}
		set
		{
			maskArray = value;
		}
	}

	public Color[,] ColorArray
	{
		get
		{
			return colorArray;
		}
		set
		{
			colorArray = value;
		}
	}

	public Value[,] RoughnessArray
	{
		get
		{
			return roughnessArray;
		}
		set
		{
			roughnessArray = value;
		}
	}

	public Value[,] MetallicityArray
	{
		get
		{
			return metallicityArray;
		}
		set
		{
			metallicityArray = value;
		}
	}

	public Value[,] HeightArray
	{
		get
		{
			return heightArray;
		}
		set
		{
			heightArray = value;
		}
	}

	public Color[,] EmissionArray
	{
		get
		{
			return emissionArray;
		}
		set
		{
			emissionArray = value;
		}
	}

	public bool IsLoaded
	{
		get
		{
			return isLoaded;
		}
		set
		{
			isLoaded = value;
		}
	}

	private Color[,] ReadColorChannelFromBinary_009(BinaryReader binaryReader, bool squared)
	{
		binaryReader.ReadBytes(8);
		binaryReader.ReadString();
		binaryReader.ReadString();
		binaryReader.ReadBytes(8);
		if (binaryReader.ReadBoolean())
		{
			binaryReader.ReadString();
		}
		Color[,] outputArray;
		if (squared)
		{
			outputArray = new Color[arraySize, arraySize];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int arrayX = x + (arraySize - width) / 2;
					int arrayY = y + (arraySize - height) / 2;
					outputArray[arrayX, arrayY].r = binaryReader.ReadSingle();
					outputArray[arrayX, arrayY].g = binaryReader.ReadSingle();
					outputArray[arrayX, arrayY].b = binaryReader.ReadSingle();
					outputArray[arrayX, arrayY].a = binaryReader.ReadSingle();
				}
			}
		}
		else
		{
			outputArray = new Color[width, height];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					outputArray[j, i].r = binaryReader.ReadSingle();
					outputArray[j, i].g = binaryReader.ReadSingle();
					outputArray[j, i].b = binaryReader.ReadSingle();
					outputArray[j, i].a = binaryReader.ReadSingle();
				}
			}
		}
		return outputArray;
	}

	private Color[,] ReadColorChannelFromBinary(BinaryReader binaryReader, bool squared)
	{
		binaryReader.ReadBytes(8);
		binaryReader.ReadString();
		binaryReader.ReadString();
		binaryReader.ReadBytes(17);
		if (binaryReader.ReadBoolean())
		{
			binaryReader.ReadString();
		}
		binaryReader.ReadBytes(64);
		bool hasContent = binaryReader.ReadBoolean();
		binaryReader.ReadBytes(4);
		Color[,] outputArray;
		if (squared)
		{
			outputArray = new Color[arraySize, arraySize];
			if (hasContent)
			{
				Vector2i contentAreaStart = default(Vector2i);
				contentAreaStart.x = binaryReader.ReadInt32();
				contentAreaStart.y = binaryReader.ReadInt32();
				Vector2i contentAreaEnd = default(Vector2i);
				contentAreaEnd.x = binaryReader.ReadInt32();
				contentAreaEnd.y = binaryReader.ReadInt32();
				for (int y = contentAreaStart.y; y <= contentAreaEnd.y; y++)
				{
					for (int x = contentAreaStart.x; x <= contentAreaEnd.x; x++)
					{
						int arrayX = x + (arraySize - width) / 2;
						int arrayY = y + (arraySize - height) / 2;
						outputArray[arrayX, arrayY].r8 = binaryReader.ReadByte();
						outputArray[arrayX, arrayY].g8 = binaryReader.ReadByte();
						outputArray[arrayX, arrayY].b8 = binaryReader.ReadByte();
						outputArray[arrayX, arrayY].a8 = binaryReader.ReadByte();
					}
				}
			}
		}
		else
		{
			outputArray = new Color[width, height];
			if (hasContent)
			{
				Vector2i contentAreaStart2 = default(Vector2i);
				contentAreaStart2.x = binaryReader.ReadInt32();
				contentAreaStart2.y = binaryReader.ReadInt32();
				Vector2i contentAreaEnd2 = default(Vector2i);
				contentAreaEnd2.x = binaryReader.ReadInt32();
				contentAreaEnd2.y = binaryReader.ReadInt32();
				for (int i = contentAreaStart2.y; i <= contentAreaEnd2.y; i++)
				{
					for (int j = contentAreaStart2.x; j <= contentAreaEnd2.x; j++)
					{
						outputArray[j, i].r8 = binaryReader.ReadByte();
						outputArray[j, i].g8 = binaryReader.ReadByte();
						outputArray[j, i].b8 = binaryReader.ReadByte();
						outputArray[j, i].a8 = binaryReader.ReadByte();
					}
				}
			}
		}
		return outputArray;
	}

	private Value[,] ReadValueChannelFromBinary(BinaryReader binaryReader, bool squared)
	{
		binaryReader.ReadBytes(8);
		binaryReader.ReadString();
		binaryReader.ReadString();
		binaryReader.ReadBytes(17);
		if (binaryReader.ReadBoolean())
		{
			binaryReader.ReadString();
		}
		binaryReader.ReadBytes(64);
		bool hasContent = binaryReader.ReadBoolean();
		binaryReader.ReadBytes(3);
		Value[,] outputArray;
		if (squared)
		{
			outputArray = new Value[arraySize, arraySize];
			if (hasContent)
			{
				Vector2i contentAreaStart = default(Vector2i);
				contentAreaStart.x = binaryReader.ReadInt32();
				contentAreaStart.y = binaryReader.ReadInt32();
				Vector2i contentAreaEnd = default(Vector2i);
				contentAreaEnd.x = binaryReader.ReadInt32();
				contentAreaEnd.y = binaryReader.ReadInt32();
				for (int y = contentAreaStart.y; y <= contentAreaEnd.y; y++)
				{
					for (int x = contentAreaStart.x; x <= contentAreaEnd.x; x++)
					{
						int arrayX = x + (arraySize - width) / 2;
						int arrayY = y + (arraySize - height) / 2;
						outputArray[arrayX, arrayY].v = 1f * (float)(int)binaryReader.ReadUInt16() / 65535f;
						outputArray[arrayX, arrayY].a8 = binaryReader.ReadByte();
					}
				}
			}
		}
		else
		{
			outputArray = new Value[width, height];
			if (hasContent)
			{
				Vector2i contentAreaStart2 = default(Vector2i);
				contentAreaStart2.x = binaryReader.ReadInt32();
				contentAreaStart2.y = binaryReader.ReadInt32();
				Vector2i contentAreaEnd2 = default(Vector2i);
				contentAreaEnd2.x = binaryReader.ReadInt32();
				contentAreaEnd2.y = binaryReader.ReadInt32();
				for (int i = contentAreaStart2.y; i <= contentAreaEnd2.y; i++)
				{
					for (int j = contentAreaStart2.x; j <= contentAreaEnd2.x; j++)
					{
						outputArray[j, i].v = 1f * (float)(int)binaryReader.ReadUInt16() / 65535f;
						outputArray[j, i].a8 = binaryReader.ReadByte();
					}
				}
			}
		}
		return outputArray;
	}

	private float[,] ReadFloatChannelFromBinary_009(BinaryReader binaryReader, bool squared)
	{
		binaryReader.ReadBytes(8);
		binaryReader.ReadString();
		binaryReader.ReadString();
		binaryReader.ReadBytes(8);
		if (binaryReader.ReadBoolean())
		{
			binaryReader.ReadString();
		}
		float[,] outputArray;
		if (squared)
		{
			outputArray = new float[arraySize, arraySize];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int arrayX = x + (arraySize - width) / 2;
					int arrayY = y + (arraySize - height) / 2;
					outputArray[arrayX, arrayY] = binaryReader.ReadSingle();
				}
			}
		}
		else
		{
			outputArray = new float[width, height];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					outputArray[j, i] = binaryReader.ReadSingle();
				}
			}
		}
		return outputArray;
	}

	private Value[,] ReadFloatChannelAsValueChannelFromBinary_009(BinaryReader binaryReader, bool squared)
	{
		binaryReader.ReadBytes(8);
		binaryReader.ReadString();
		binaryReader.ReadString();
		binaryReader.ReadBytes(8);
		if (binaryReader.ReadBoolean())
		{
			binaryReader.ReadString();
		}
		Value[,] outputArray;
		if (squared)
		{
			outputArray = new Value[arraySize, arraySize];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int arrayX = x + (arraySize - width) / 2;
					int arrayY = y + (arraySize - height) / 2;
					outputArray[arrayX, arrayY].v = binaryReader.ReadSingle();
					outputArray[arrayX, arrayY].a = 1f;
				}
			}
		}
		else
		{
			outputArray = new Value[width, height];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					outputArray[j, i].v = binaryReader.ReadSingle();
					outputArray[j, i].a = 1f;
				}
			}
		}
		return outputArray;
	}

	private float[,] ReadFloatChannelFromBinary(BinaryReader binaryReader, bool squared)
	{
		binaryReader.ReadBytes(8);
		binaryReader.ReadString();
		binaryReader.ReadString();
		binaryReader.ReadBytes(17);
		if (binaryReader.ReadBoolean())
		{
			binaryReader.ReadString();
		}
		binaryReader.ReadBytes(64);
		bool hasContent = binaryReader.ReadBoolean();
		binaryReader.ReadBytes(2);
		float[,] outputArray;
		if (squared)
		{
			outputArray = new float[arraySize, arraySize];
			if (hasContent)
			{
				Vector2i contentAreaStart = default(Vector2i);
				contentAreaStart.x = binaryReader.ReadInt32();
				contentAreaStart.y = binaryReader.ReadInt32();
				Vector2i contentAreaEnd = default(Vector2i);
				contentAreaEnd.x = binaryReader.ReadInt32();
				contentAreaEnd.y = binaryReader.ReadInt32();
				for (int y = contentAreaStart.y; y <= contentAreaEnd.y; y++)
				{
					for (int x = contentAreaStart.x; x <= contentAreaEnd.x; x++)
					{
						int arrayX = x + (arraySize - width) / 2;
						int arrayY = y + (arraySize - height) / 2;
						outputArray[arrayX, arrayY] = 1f * (float)(int)binaryReader.ReadUInt16() / 65535f;
					}
				}
			}
		}
		else
		{
			outputArray = new float[width, height];
			if (hasContent)
			{
				Vector2i contentAreaStart2 = default(Vector2i);
				contentAreaStart2.x = binaryReader.ReadInt32();
				contentAreaStart2.y = binaryReader.ReadInt32();
				Vector2i contentAreaEnd2 = default(Vector2i);
				contentAreaEnd2.x = binaryReader.ReadInt32();
				contentAreaEnd2.y = binaryReader.ReadInt32();
				for (int i = contentAreaStart2.y; i <= contentAreaEnd2.y; i++)
				{
					for (int j = contentAreaStart2.x; j <= contentAreaEnd2.x; j++)
					{
						outputArray[j, i] = 1f * (float)(int)binaryReader.ReadUInt16() / 65535f;
					}
				}
			}
		}
		return outputArray;
	}

	public float[,] CreateMaskArrayFromColorArray(Color[,] colorArray, bool squared)
	{
		maskArray = null;
		if (squared)
		{
			maskArray = new float[arraySize, arraySize];
			for (int y = 0; y < arraySize; y++)
			{
				for (int x = 0; x < arraySize; x++)
				{
					maskArray[x, y] = colorArray[x, y].a;
				}
			}
			return maskArray;
		}
		maskArray = new float[width, height];
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				maskArray[j, i] = colorArray[j, i].a;
			}
		}
		return maskArray;
	}

	private void ReadDecalDataFromBinaryStream_009(BinaryReader binaryReader, bool squared)
	{
		binaryReader.ReadBytes(8);
		name = binaryReader.ReadString();
		binaryReader.ReadBoolean();
		width = binaryReader.ReadInt32();
		height = binaryReader.ReadInt32();
		if (squared)
		{
			arraySize = ((width >= height) ? width : height);
		}
		else
		{
			arraySize = -1;
		}
		colorArray = ReadColorChannelFromBinary_009(binaryReader, squared);
		CreateMaskArrayFromColorArray(colorArray, squared);
		heightArray = ReadFloatChannelAsValueChannelFromBinary_009(binaryReader, squared);
		ReadColorChannelFromBinary_009(binaryReader, squared);
		roughnessArray = ReadFloatChannelAsValueChannelFromBinary_009(binaryReader, squared);
		metallicityArray = ReadFloatChannelAsValueChannelFromBinary_009(binaryReader, squared);
		emissionArray = ReadColorChannelFromBinary_009(binaryReader, squared);
	}

	private void ReadDecalDataFromBinaryStream(BinaryReader binaryReader, bool squared)
	{
		binaryReader.ReadBytes(8);
		binaryReader.ReadBytes(32);
		name = binaryReader.ReadString();
		binaryReader.ReadBoolean();
		width = binaryReader.ReadInt32();
		height = binaryReader.ReadInt32();
		if (squared)
		{
			arraySize = ((width >= height) ? width : height);
		}
		else
		{
			arraySize = -1;
		}
		binaryReader.ReadBytes(64);
		colorArray = ReadColorChannelFromBinary(binaryReader, squared);
		CreateMaskArrayFromColorArray(colorArray, squared);
		roughnessArray = ReadValueChannelFromBinary(binaryReader, squared);
		metallicityArray = ReadValueChannelFromBinary(binaryReader, squared);
		heightArray = ReadValueChannelFromBinary(binaryReader, squared);
		emissionArray = ReadColorChannelFromBinary(binaryReader, squared);
	}

	public FileSystem.ErrorEnum Load(string file, bool squared = true)
	{
		isLoaded = false;
		file = System.IO.Path.GetFullPath(file);
		if (System.IO.File.Exists(file))
		{
			FileStream fileStream = System.IO.File.OpenRead(file);
			if (System.IO.Path.GetExtension(file) == ".dat")
			{
				thumbnail = null;
				BinaryReader binaryReader = new BinaryReader(fileStream);
				switch (binaryReader.ReadInt32())
				{
				case 9:
				{
					int width = binaryReader.ReadInt32();
					int height = binaryReader.ReadInt32();
					thumbnail = new Image();
					thumbnail.Create(width, height, useMipmaps: false, Image.Format.Rgba8);
					thumbnail.Lock();
					Color color = default(Color);
					for (int y = 0; y < height; y++)
					{
						for (int x = 0; x < width; x++)
						{
							color.r = binaryReader.ReadSingle();
							color.g = binaryReader.ReadSingle();
							color.b = binaryReader.ReadSingle();
							color.a = binaryReader.ReadSingle();
							thumbnail.SetPixel(x, y, color);
						}
					}
					thumbnail.Unlock();
					break;
				}
				default:
					binaryReader.Dispose();
					fileStream.Dispose();
					return FileSystem.ErrorEnum.VERSION;
				case 8:
					break;
				}
				ReadDecalDataFromBinaryStream_009(binaryReader, squared);
				binaryReader.Dispose();
			}
			else
			{
				GZipStream compressor = new GZipStream(fileStream, CompressionMode.Decompress);
				BinaryReader binaryReader2 = new BinaryReader(compressor);
				if (binaryReader2.ReadInt32() != 28137474)
				{
					binaryReader2.Dispose();
					compressor.Dispose();
					fileStream.Dispose();
					return FileSystem.ErrorEnum.FILE;
				}
				binaryReader2.ReadInt32();
				if (binaryReader2.ReadInt32() != 10)
				{
					binaryReader2.Dispose();
					compressor.Dispose();
					fileStream.Dispose();
					return FileSystem.ErrorEnum.VERSION;
				}
				int width2 = binaryReader2.ReadInt32();
				int height2 = binaryReader2.ReadInt32();
				thumbnail = new Image();
				thumbnail.Create(width2, height2, useMipmaps: false, Image.Format.Rgba8);
				thumbnail.Lock();
				Color color2 = default(Color);
				for (int i = 0; i < height2; i++)
				{
					for (int j = 0; j < width2; j++)
					{
						color2.r8 = binaryReader2.ReadByte();
						color2.g8 = binaryReader2.ReadByte();
						color2.b8 = binaryReader2.ReadByte();
						color2.a8 = binaryReader2.ReadByte();
						thumbnail.SetPixel(j, i, color2);
					}
				}
				thumbnail.Unlock();
				ReadDecalDataFromBinaryStream(binaryReader2, squared);
				binaryReader2.Dispose();
				compressor.Dispose();
			}
			isLoaded = true;
			fileStream.Dispose();
			return FileSystem.ErrorEnum.NONE;
		}
		return FileSystem.ErrorEnum.FILE;
	}
}
