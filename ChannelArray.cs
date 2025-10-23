using Godot;

public abstract class ChannelArray
{
	protected Channel.DataTypeEnum dataType;

	protected int width;

	protected int height;

	public Channel.DataTypeEnum DataType => dataType;

	public int Width => width;

	public int Height => height;

	public ChannelArray<Color> CastToColor()
	{
		if (dataType == Channel.DataTypeEnum.COLOR)
		{
			return (ChannelArray<Color>)this;
		}
		return null;
	}

	public ChannelArray<Value> CastToValue()
	{
		if (dataType == Channel.DataTypeEnum.VALUE)
		{
			return (ChannelArray<Value>)this;
		}
		return null;
	}

	public ChannelArray<int> CastToInteger()
	{
		if (dataType == Channel.DataTypeEnum.INTEGER)
		{
			return (ChannelArray<int>)this;
		}
		return null;
	}

	public ChannelArray<byte> CastToByte()
	{
		if (dataType == Channel.DataTypeEnum.BYTE)
		{
			return (ChannelArray<byte>)this;
		}
		return null;
	}

	public ChannelArray<float> CastToFloat()
	{
		if (dataType == Channel.DataTypeEnum.FLOAT)
		{
			return (ChannelArray<float>)this;
		}
		return null;
	}

	public ChannelArray<bool> CastToBoolean()
	{
		if (dataType == Channel.DataTypeEnum.BOOL)
		{
			return (ChannelArray<bool>)this;
		}
		return null;
	}

	public Image CreateImage(Image image = null, Image.Format imageFormat = Image.Format.Rgba8)
	{
		Color color = new Color(0f, 0f, 0f);
		if (image == null)
		{
			image = new Image();
			image.Create(width, height, useMipmaps: false, imageFormat);
		}
		image.Lock();
		switch (dataType)
		{
		case Channel.DataTypeEnum.COLOR:
		{
			ChannelArray<Color> colorArray = (ChannelArray<Color>)this;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					image.SetPixel(j, i, colorArray[j, i]);
				}
			}
			break;
		}
		case Channel.DataTypeEnum.VALUE:
		{
			ChannelArray<Value> valueArray = (ChannelArray<Value>)this;
			for (int m = 0; m < height; m++)
			{
				for (int n = 0; n < width; n++)
				{
					color.r = (color.g = (color.b = valueArray[n, m].v));
					image.SetPixel(n, m, color);
				}
			}
			break;
		}
		case Channel.DataTypeEnum.FLOAT:
		{
			ChannelArray<float> floatArray = (ChannelArray<float>)this;
			for (int num4 = 0; num4 < height; num4++)
			{
				for (int num5 = 0; num5 < width; num5++)
				{
					color.r = (color.g = (color.b = floatArray[num5, num4]));
					image.SetPixel(num5, num4, color);
				}
			}
			break;
		}
		case Channel.DataTypeEnum.BYTE:
		{
			ChannelArray<byte> byteArray = (ChannelArray<byte>)this;
			for (int k = 0; k < height; k++)
			{
				for (int l = 0; l < width; l++)
				{
					int num = (color.b8 = byteArray[l, k]);
					int r = (color.g8 = num);
					color.r8 = r;
					image.SetPixel(l, k, color);
				}
			}
			break;
		}
		case Channel.DataTypeEnum.BOOL:
		{
			ChannelArray<bool> boolArray = (ChannelArray<bool>)this;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					color.r = (color.g = (color.b = (boolArray[x, y] ? 1f : 0f)));
					image.SetPixel(x, y, color);
				}
			}
			break;
		}
		}
		image.Unlock();
		return image;
	}
}
public class ChannelArray<T> : ChannelArray where T : struct
{
	private T defaultValue;

	private T[,] array;

	public T DefaultValue
	{
		get
		{
			return defaultValue;
		}
		set
		{
			defaultValue = value;
		}
	}

	public T[,] Array
	{
		get
		{
			return array;
		}
		set
		{
			array = value;
		}
	}

	public ref T[,] ArrayRef => ref array;

	public T this[int x, int y]
	{
		get
		{
			return array[x, y];
		}
		set
		{
			array[x, y] = value;
		}
	}

	public ChannelArray()
	{
		if (typeof(T) == typeof(float))
		{
			dataType = Channel.DataTypeEnum.FLOAT;
		}
		else if (typeof(T) == typeof(Color))
		{
			dataType = Channel.DataTypeEnum.COLOR;
		}
		else if (typeof(T) == typeof(byte))
		{
			dataType = Channel.DataTypeEnum.BYTE;
		}
		else if (typeof(T) == typeof(bool))
		{
			dataType = Channel.DataTypeEnum.BOOL;
		}
		else if (typeof(T) == typeof(int))
		{
			dataType = Channel.DataTypeEnum.INTEGER;
		}
	}

	public ChannelArray(int width, int height, T defaultValue = default(T))
	{
		if (typeof(T) == typeof(Color))
		{
			dataType = Channel.DataTypeEnum.COLOR;
		}
		else if (typeof(T) == typeof(Value))
		{
			dataType = Channel.DataTypeEnum.VALUE;
		}
		else if (typeof(T) == typeof(float))
		{
			dataType = Channel.DataTypeEnum.FLOAT;
		}
		else if (typeof(T) == typeof(byte))
		{
			dataType = Channel.DataTypeEnum.BYTE;
		}
		else if (typeof(T) == typeof(bool))
		{
			dataType = Channel.DataTypeEnum.BOOL;
		}
		else if (typeof(T) == typeof(int))
		{
			dataType = Channel.DataTypeEnum.INTEGER;
		}
		base.width = width;
		base.height = height;
		this.defaultValue = defaultValue;
		array = new T[width, height];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				array[x, y] = this.defaultValue;
			}
		}
	}

	public ChannelArray(T[,] array, int width, int height, T defaultValue = default(T))
	{
		if (typeof(T) == typeof(Color))
		{
			dataType = Channel.DataTypeEnum.COLOR;
		}
		else if (typeof(T) == typeof(Value))
		{
			dataType = Channel.DataTypeEnum.VALUE;
		}
		else if (typeof(T) == typeof(float))
		{
			dataType = Channel.DataTypeEnum.FLOAT;
		}
		else if (typeof(T) == typeof(byte))
		{
			dataType = Channel.DataTypeEnum.BYTE;
		}
		else if (typeof(T) == typeof(bool))
		{
			dataType = Channel.DataTypeEnum.BOOL;
		}
		else if (typeof(T) == typeof(int))
		{
			dataType = Channel.DataTypeEnum.INTEGER;
		}
		base.width = width;
		base.height = height;
		this.defaultValue = defaultValue;
		this.array = array;
	}

	public void Clear()
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				array[x, y] = defaultValue;
			}
		}
	}

	public void Clear(T value)
	{
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				array[x, y] = value;
			}
		}
	}

	public void Clear(int startX, int startY, int endX, int endY)
	{
		for (int y = startY; y <= endY; y++)
		{
			for (int x = startX; x <= endX; x++)
			{
				array[x, y] = defaultValue;
			}
		}
	}

	public void Clear(int startX, int startY, int endX, int endY, T value)
	{
		for (int y = startY; y <= endY; y++)
		{
			for (int x = startX; x <= endX; x++)
			{
				array[x, y] = value;
			}
		}
	}

	public void Clear(Vector2i start, Vector2i end)
	{
		Clear(start.x, start.y, end.x, end.y);
	}

	public void Clear(Vector2i start, Vector2i end, T value)
	{
		Clear(start.x, start.y, end.x, end.y, value);
	}
}
