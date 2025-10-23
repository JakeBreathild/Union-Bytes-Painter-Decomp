using System.IO;
using Godot;

public class Channel
{
	public enum TypeEnum
	{
		UNDEFINED,
		COLOR,
		HEIGHT,
		NORMAL,
		ROUGHNESS,
		METALLICITY,
		EMISSION
	}

	public enum DataTypeEnum
	{
		UNDEFINED,
		INTEGER,
		FLOAT,
		COLOR,
		BOOL,
		BYTE,
		VALUE
	}

	public enum NormalModeEnum
	{
		SOBEL,
		PREWITT,
		CROSS,
		GODOT
	}

	protected static NormalModeEnum normalMode = NormalModeEnum.SOBEL;

	protected static float normalStrength = 2.5f;

	protected static bool doNormalInverseY = false;

	protected static Vector3 halfVector = Vector3.One * 0.5f;

	protected DataTypeEnum dataType;

	protected TypeEnum type;

	protected string name = "";

	protected string fileSuffix = "";

	protected int width;

	protected int height;

	protected bool isNew = true;

	protected bool isVisible;

	protected float blendingStrength = 1f;

	protected string linkedChannelName = "";

	protected Channel linkedChannel;

	protected Image.Format imageFormat = Image.Format.Rgba8;

	protected Image image;

	protected ImageTexture imageTexture;

	protected bool doUpdate;

	protected Vector2i updateStart = Vector2i.Maximum;

	protected Vector2i updateEnd = Vector2i.Minimum;

	protected Vector2i contentAreaStart = Vector2i.Maximum;

	protected Vector2i contentAreaEnd = Vector2i.Minimum;

	protected bool hasContent;

	public static NormalModeEnum NormalMode
	{
		get
		{
			return normalMode;
		}
		set
		{
			normalMode = value;
		}
	}

	public static float NormalStrength
	{
		get
		{
			return normalStrength;
		}
		set
		{
			normalStrength = value;
		}
	}

	public static bool DoNormalInverseY
	{
		get
		{
			return doNormalInverseY;
		}
		set
		{
			doNormalInverseY = value;
		}
	}

	public DataTypeEnum DataType
	{
		get
		{
			return dataType;
		}
		protected set
		{
			dataType = value;
		}
	}

	public TypeEnum Type
	{
		get
		{
			return type;
		}
		protected set
		{
			type = value;
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

	public string FileSuffix
	{
		get
		{
			return fileSuffix;
		}
		set
		{
			fileSuffix = value;
		}
	}

	public int Width
	{
		get
		{
			return width;
		}
		protected set
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
		protected set
		{
			height = value;
		}
	}

	public bool IsNew
	{
		get
		{
			return isNew;
		}
		set
		{
			isNew = value;
		}
	}

	public bool IsVisible
	{
		get
		{
			return isVisible;
		}
		set
		{
			isVisible = value;
			isNew = false;
		}
	}

	public float BlendingStrength
	{
		get
		{
			return blendingStrength;
		}
		set
		{
			blendingStrength = value;
		}
	}

	public string LinkedChannelName
	{
		get
		{
			return linkedChannelName;
		}
		set
		{
			linkedChannelName = value;
		}
	}

	public Channel LinkedChannel
	{
		get
		{
			return linkedChannel;
		}
		set
		{
			linkedChannel = value;
			linkedChannelName = ((linkedChannel != null) ? linkedChannel.Name : "");
		}
	}

	public Image.Format ImageFormat => imageFormat;

	public Image Image
	{
		get
		{
			return image;
		}
		set
		{
			image = value;
		}
	}

	public ImageTexture ImageTexture
	{
		get
		{
			return imageTexture;
		}
		set
		{
			imageTexture = value;
		}
	}

	public bool DoUpdate => doUpdate;

	public Vector2i UpdateStart => updateStart;

	public Vector2i UpdateEnd => updateEnd;

	public Vector2i ContentAreaStart
	{
		get
		{
			return contentAreaStart;
		}
		set
		{
			contentAreaStart = value;
		}
	}

	public Vector2i ContentAreaEnd
	{
		get
		{
			return contentAreaEnd;
		}
		set
		{
			contentAreaEnd = value;
		}
	}

	public bool HasContent
	{
		get
		{
			return hasContent;
		}
		set
		{
			hasContent = value;
		}
	}

	protected static float SampleHeight(float[,] heightArray, int width, int height, int x, int y)
	{
		if (x >= width)
		{
			x %= width;
		}
		while (x < 0)
		{
			x += width;
		}
		if (y >= height)
		{
			y %= height;
		}
		while (y < 0)
		{
			y += height;
		}
		return heightArray[x, y];
	}

	protected static Vector3 CalculateNormal(float[,] heightArray, int width, int height, int x, int y, bool invertY = false)
	{
		float p8 = SampleHeight(heightArray, width, height, x, y - 1);
		float p9 = SampleHeight(heightArray, width, height, x + 1, y);
		float p10 = SampleHeight(heightArray, width, height, x, y + 1);
		float p11 = SampleHeight(heightArray, width, height, x - 1, y);
		Vector3 normal = default(Vector3);
		switch (normalMode)
		{
		case NormalModeEnum.CROSS:
			normal.x = normalStrength * 2f * (p11 - p9);
			normal.y = normalStrength * 2f * (p10 - p8);
			normal.z = 1f;
			break;
		case NormalModeEnum.PREWITT:
		{
			float p12 = SampleHeight(heightArray, width, height, x + 1, y - 1);
			float num2 = SampleHeight(heightArray, width, height, x - 1, y - 1);
			float p13 = SampleHeight(heightArray, width, height, x - 1, y + 1);
			float p14 = SampleHeight(heightArray, width, height, x + 1, y + 1);
			float topSide = num2 + p8 + p12;
			float bottomSide = p13 + p10 + p14;
			float rightSide = p12 + p9 + p14;
			float leftSide = num2 + p11 + p13;
			normal.x = normalStrength * (leftSide - rightSide);
			normal.y = normalStrength * (bottomSide - topSide);
			normal.z = 1f;
			break;
		}
		case NormalModeEnum.GODOT:
		{
			float p15 = SampleHeight(heightArray, width, height, x, y);
			Vector3 up = new Vector3(0f, 1f, 2f * (p15 - p10) * normalStrength);
			normal = new Vector3(1f, 0f, 2f * (p15 - p11) * normalStrength).Cross(up);
			break;
		}
		default:
		{
			float p12 = SampleHeight(heightArray, width, height, x + 1, y - 1);
			float num = SampleHeight(heightArray, width, height, x - 1, y - 1);
			float p13 = SampleHeight(heightArray, width, height, x - 1, y + 1);
			float p14 = SampleHeight(heightArray, width, height, x + 1, y + 1);
			float topSide = num + 2f * p8 + p12;
			float bottomSide = p13 + 2f * p10 + p14;
			float rightSide = p12 + 2f * p9 + p14;
			float leftSide = num + 2f * p11 + p13;
			normal.x = normalStrength * (leftSide - rightSide);
			normal.y = normalStrength * (bottomSide - topSide);
			normal.z = 1f;
			break;
		}
		}
		if (invertY)
		{
			normal.y *= -1f;
		}
		return normal.Normalized() * 0.5f + halfVector;
	}

	public static Color[,] ConvertHeightToNormal(float[,] heightArray, int width, int height, bool invertY = false)
	{
		Color[,] normalArray = new Color[width, height];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Vector3 normal = CalculateNormal(heightArray, width, height, x, y, invertY);
				normalArray[x, y].r = normal.x;
				normalArray[x, y].g = normal.y;
				normalArray[x, y].b = normal.z;
				normalArray[x, y].a = 1f;
			}
		}
		return normalArray;
	}

	protected static float SampleHeight(Value[,] heightArray, int width, int height, int x, int y)
	{
		if (x >= width)
		{
			x %= width;
		}
		while (x < 0)
		{
			x += width;
		}
		if (y >= height)
		{
			y %= height;
		}
		while (y < 0)
		{
			y += height;
		}
		return heightArray[x, y].v * heightArray[x, y].a;
	}

	protected static Vector3 CalculateNormal(Value[,] heightArray, int width, int height, int x, int y, bool invertY = false)
	{
		float p8 = SampleHeight(heightArray, width, height, x, y - 1);
		float p9 = SampleHeight(heightArray, width, height, x + 1, y);
		float p10 = SampleHeight(heightArray, width, height, x, y + 1);
		float p11 = SampleHeight(heightArray, width, height, x - 1, y);
		Vector3 normal = default(Vector3);
		switch (normalMode)
		{
		case NormalModeEnum.CROSS:
			normal.x = normalStrength * 2f * (p11 - p9);
			normal.y = normalStrength * 2f * (p10 - p8);
			normal.z = 1f;
			break;
		case NormalModeEnum.PREWITT:
		{
			float p12 = SampleHeight(heightArray, width, height, x + 1, y - 1);
			float num2 = SampleHeight(heightArray, width, height, x - 1, y - 1);
			float p13 = SampleHeight(heightArray, width, height, x - 1, y + 1);
			float p14 = SampleHeight(heightArray, width, height, x + 1, y + 1);
			float topSide = num2 + p8 + p12;
			float bottomSide = p13 + p10 + p14;
			float rightSide = p12 + p9 + p14;
			float leftSide = num2 + p11 + p13;
			normal.x = normalStrength * (leftSide - rightSide);
			normal.y = normalStrength * (bottomSide - topSide);
			normal.z = 1f;
			break;
		}
		case NormalModeEnum.GODOT:
		{
			float p15 = SampleHeight(heightArray, width, height, x, y);
			Vector3 up = new Vector3(0f, 1f, 2f * (p15 - p10) * normalStrength);
			normal = new Vector3(1f, 0f, 2f * (p15 - p11) * normalStrength).Cross(up);
			break;
		}
		default:
		{
			float p12 = SampleHeight(heightArray, width, height, x + 1, y - 1);
			float num = SampleHeight(heightArray, width, height, x - 1, y - 1);
			float p13 = SampleHeight(heightArray, width, height, x - 1, y + 1);
			float p14 = SampleHeight(heightArray, width, height, x + 1, y + 1);
			float topSide = num + 2f * p8 + p12;
			float bottomSide = p13 + 2f * p10 + p14;
			float rightSide = p12 + 2f * p9 + p14;
			float leftSide = num + 2f * p11 + p13;
			normal.x = normalStrength * (leftSide - rightSide);
			normal.y = normalStrength * (bottomSide - topSide);
			normal.z = 1f;
			break;
		}
		}
		if (invertY)
		{
			normal.y *= -1f;
		}
		return normal.Normalized() * 0.5f + halfVector;
	}

	public static Color[,] ConvertHeightToNormal(Value[,] heightArray, int width, int height, bool invertY = false)
	{
		Color[,] normalArray = new Color[width, height];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Vector3 normal = CalculateNormal(heightArray, width, height, x, y, invertY);
				normalArray[x, y].r = normal.x;
				normalArray[x, y].g = normal.y;
				normalArray[x, y].b = normal.z;
				normalArray[x, y].a = 1f;
			}
		}
		return normalArray;
	}

	public void UpdateAt(int x, int y)
	{
		doUpdate = true;
		if (x < updateStart.x)
		{
			updateStart.x = x;
		}
		if (x > updateEnd.x)
		{
			updateEnd.x = x;
		}
		if (y < updateStart.y)
		{
			updateStart.y = y;
		}
		if (y > updateEnd.y)
		{
			updateEnd.y = y;
		}
		hasContent = true;
		if (x < contentAreaStart.x)
		{
			contentAreaStart.x = x;
		}
		if (x > contentAreaEnd.x)
		{
			contentAreaEnd.x = x;
		}
		if (y < contentAreaStart.y)
		{
			contentAreaStart.y = y;
		}
		if (y > contentAreaEnd.y)
		{
			contentAreaEnd.y = y;
		}
		if (isNew)
		{
			isVisible = true;
			isNew = false;
		}
	}

	public void UpdateAt(int x, int y, int width, int height)
	{
		int xx = x + width - 1;
		int yy = y + height - 1;
		doUpdate = true;
		if (x < updateStart.x)
		{
			updateStart.x = x;
		}
		if (xx > updateEnd.x)
		{
			updateEnd.x = xx;
		}
		if (y < updateStart.y)
		{
			updateStart.y = y;
		}
		if (yy > updateEnd.y)
		{
			updateEnd.y = yy;
		}
		hasContent = true;
		if (x < contentAreaStart.x)
		{
			contentAreaStart.x = x;
		}
		if (xx > contentAreaEnd.x)
		{
			contentAreaEnd.x = xx;
		}
		if (y < contentAreaStart.y)
		{
			contentAreaStart.y = y;
		}
		if (yy > contentAreaEnd.y)
		{
			contentAreaEnd.y = yy;
		}
		if (isNew)
		{
			isVisible = true;
			isNew = false;
		}
	}

	public void UpdateAt(Vector2i start, Vector2i end)
	{
		doUpdate = true;
		updateStart = start;
		updateEnd = end;
		hasContent = true;
		if (updateStart.x < contentAreaStart.x)
		{
			contentAreaStart.x = updateStart.x;
		}
		if (updateEnd.x > contentAreaEnd.x)
		{
			contentAreaEnd.x = updateEnd.x;
		}
		if (updateStart.y < contentAreaStart.y)
		{
			contentAreaStart.y = updateStart.y;
		}
		if (updateEnd.y > contentAreaEnd.y)
		{
			contentAreaEnd.y = updateEnd.y;
		}
		if (isNew)
		{
			isVisible = true;
			isNew = false;
		}
	}

	public void UpdateFull()
	{
		doUpdate = true;
		updateStart.x = 0;
		updateStart.y = 0;
		updateEnd.x = width - 1;
		updateEnd.y = height - 1;
		hasContent = true;
		contentAreaStart = updateStart;
		contentAreaEnd = updateEnd;
		if (isNew)
		{
			isVisible = true;
			isNew = false;
		}
	}

	public Channel<Color> CastToColor()
	{
		if (dataType == DataTypeEnum.COLOR)
		{
			return (Channel<Color>)this;
		}
		return null;
	}

	public Channel<Value> CastToValue()
	{
		if (dataType == DataTypeEnum.VALUE)
		{
			return (Channel<Value>)this;
		}
		return null;
	}

	public Channel<int> CastToInteger()
	{
		if (dataType == DataTypeEnum.INTEGER)
		{
			return (Channel<int>)this;
		}
		return null;
	}

	public Channel<byte> CastToByte()
	{
		if (dataType == DataTypeEnum.BYTE)
		{
			return (Channel<byte>)this;
		}
		return null;
	}

	public Channel<float> CastToFloat()
	{
		if (dataType == DataTypeEnum.FLOAT)
		{
			return (Channel<float>)this;
		}
		return null;
	}

	public Channel<bool> CastToBoolean()
	{
		if (dataType == DataTypeEnum.BOOL)
		{
			return (Channel<bool>)this;
		}
		return null;
	}

	public static Channel<Value> ConvertFromFloatToValue(Channel<float> floatChannel, Value defaultValue)
	{
		Channel<Value> valueChannel = new Channel<Value>(floatChannel.Width, floatChannel.height, defaultValue, floatChannel.Type, floatChannel.Name, floatChannel.FileSuffix, floatChannel.ImageFormat, doCreateImage: false);
		valueChannel.BlendingMode = floatChannel.BlendingMode;
		valueChannel.BlendingStrength = floatChannel.BlendingStrength;
		valueChannel.IsVisible = floatChannel.IsVisible;
		for (int y = 0; y < floatChannel.Height; y++)
		{
			for (int x = 0; x < floatChannel.Width; x++)
			{
				valueChannel.Array[x, y].v = floatChannel[x, y];
				valueChannel.Array[x, y].a = 1f;
			}
		}
		valueChannel.DetectContentArea();
		valueChannel.LinkedChannelName = floatChannel.LinkedChannelName;
		valueChannel.LinkedChannel = floatChannel.LinkedChannel;
		valueChannel.Image = floatChannel.Image;
		valueChannel.ImageTexture = floatChannel.ImageTexture;
		return valueChannel;
	}

	public static Channel<Value> ConvertFromFloatToValue(Channel<float> floatChannel, Value defaultValue, Channel<Color> colorChannel)
	{
		Channel<Value> valueChannel = new Channel<Value>(floatChannel.Width, floatChannel.height, defaultValue, floatChannel.Type, floatChannel.Name, floatChannel.FileSuffix, floatChannel.ImageFormat, doCreateImage: false);
		valueChannel.BlendingMode = floatChannel.BlendingMode;
		valueChannel.BlendingStrength = floatChannel.BlendingStrength;
		valueChannel.IsVisible = floatChannel.IsVisible;
		for (int y = 0; y < floatChannel.Height; y++)
		{
			for (int x = 0; x < floatChannel.Width; x++)
			{
				valueChannel.Array[x, y].v = floatChannel[x, y];
				valueChannel.Array[x, y].a = colorChannel[x, y].a;
			}
		}
		valueChannel.DetectContentArea();
		valueChannel.LinkedChannelName = floatChannel.LinkedChannelName;
		valueChannel.LinkedChannel = floatChannel.LinkedChannel;
		valueChannel.Image = floatChannel.Image;
		valueChannel.ImageTexture = floatChannel.ImageTexture;
		return valueChannel;
	}

	public static Channel<float> ConvertFromValueToFloat(Channel<Value> valueChannel, float defaultValue)
	{
		Channel<float> floatChannel = new Channel<float>(valueChannel.Width, valueChannel.height, defaultValue, valueChannel.Type, valueChannel.Name, valueChannel.FileSuffix, valueChannel.ImageFormat, doCreateImage: false);
		floatChannel.BlendingMode = valueChannel.BlendingMode;
		floatChannel.BlendingStrength = valueChannel.BlendingStrength;
		floatChannel.IsVisible = valueChannel.IsVisible;
		for (int y = 0; y < valueChannel.Height; y++)
		{
			for (int x = 0; x < valueChannel.Width; x++)
			{
				floatChannel[x, y] = valueChannel[x, y].v;
			}
		}
		floatChannel.DetectContentArea();
		floatChannel.LinkedChannelName = valueChannel.LinkedChannelName;
		floatChannel.LinkedChannel = valueChannel.LinkedChannel;
		floatChannel.Image = valueChannel.Image;
		floatChannel.ImageTexture = valueChannel.ImageTexture;
		return floatChannel;
	}
}
public class Channel<T> : Channel where T : struct
{
	private T defaultValue;

	private T[,] array;

	private Blender.ColorBlendingDelegate colorBlending;

	private Blender.ValueBlendingDelegate valueBlending;

	private Blender.FloatBlendingDelegate floatBlending;

	protected Blender.BlendingModeEnum blendingMode;

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

	public T this[int x, int y]
	{
		get
		{
			return array[x, y];
		}
		set
		{
			SetValue(x, y, value);
		}
	}

	public Blender.ColorBlendingDelegate ColorBlending => colorBlending;

	public Blender.ValueBlendingDelegate ValueBlending => valueBlending;

	public Blender.FloatBlendingDelegate FloatBlending => floatBlending;

	public Blender.BlendingModeEnum BlendingMode
	{
		get
		{
			return blendingMode;
		}
		set
		{
			blendingMode = value;
			if (typeof(T) == typeof(Color))
			{
				colorBlending = Blender.ColorBlendingFunction(blendingMode);
			}
			else if (typeof(T) == typeof(Value))
			{
				valueBlending = Blender.ValueBlendingFunction(blendingMode);
			}
			else if (typeof(T) == typeof(float))
			{
				floatBlending = Blender.FloatBlendingFunction(blendingMode);
			}
		}
	}

	private Channel(Image.Format imageFormat = Image.Format.Rgba8, bool doCreateImage = true)
	{
		if (typeof(T) == typeof(Color))
		{
			dataType = DataTypeEnum.COLOR;
			colorBlending = Blender.ColorBlendingFunction(blendingMode);
		}
		else if (typeof(T) == typeof(Value))
		{
			dataType = DataTypeEnum.VALUE;
			valueBlending = Blender.ValueBlendingFunction(blendingMode);
		}
		else if (typeof(T) == typeof(float))
		{
			dataType = DataTypeEnum.FLOAT;
			floatBlending = Blender.FloatBlendingFunction(blendingMode);
		}
		else if (typeof(T) == typeof(bool))
		{
			dataType = DataTypeEnum.BOOL;
		}
		else if (typeof(T) == typeof(int))
		{
			dataType = DataTypeEnum.INTEGER;
		}
		base.imageFormat = imageFormat;
		if (doCreateImage)
		{
			base.Image = new Image();
			base.ImageTexture = new ImageTexture();
		}
	}

	public Channel(int width, int height, T defaultValue, TypeEnum type, string name, string fileSuffix, Image.Format imageFormat = Image.Format.Rgba8, bool doCreateImage = true)
	{
		if (typeof(T) == typeof(Color))
		{
			dataType = DataTypeEnum.COLOR;
			colorBlending = Blender.ColorBlendingFunction(blendingMode);
		}
		else if (typeof(T) == typeof(Value))
		{
			dataType = DataTypeEnum.VALUE;
			valueBlending = Blender.ValueBlendingFunction(blendingMode);
		}
		else if (typeof(T) == typeof(float))
		{
			dataType = DataTypeEnum.FLOAT;
			floatBlending = Blender.FloatBlendingFunction(blendingMode);
		}
		else if (typeof(T) == typeof(bool))
		{
			base.DataType = DataTypeEnum.BOOL;
		}
		else if (typeof(T) == typeof(int))
		{
			base.DataType = DataTypeEnum.INTEGER;
		}
		base.Type = type;
		base.Name = name;
		base.FileSuffix = fileSuffix;
		this.defaultValue = defaultValue;
		base.Width = width;
		base.Height = height;
		Array = new T[base.Width, base.Height];
		for (int y = 0; y < base.Height; y++)
		{
			for (int x = 0; x < base.Width; x++)
			{
				Array[x, y] = this.defaultValue;
			}
		}
		base.imageFormat = imageFormat;
		if (doCreateImage)
		{
			image = new Image();
			image.Create(base.Width, base.Height, useMipmaps: false, base.imageFormat);
			imageTexture = new ImageTexture();
			imageTexture.CreateFromImage(image, 2u);
			if (base.Type != TypeEnum.NORMAL)
			{
				CreateTexture();
			}
		}
	}

	public Channel<T> Clone(bool doCloneImage = true)
	{
		Channel<T> channel = (Channel<T>)MemberwiseClone();
		if (array != null)
		{
			channel.Array = (T[,])array.Clone();
		}
		if (doCloneImage)
		{
			if (image != null)
			{
				channel.Image = (Image)image.Duplicate();
			}
			if (imageTexture != null)
			{
				channel.ImageTexture = (ImageTexture)imageTexture.Duplicate();
			}
		}
		else
		{
			channel.image = null;
			channel.ImageTexture = null;
		}
		return channel;
	}

	public void SetArray(T[,] array, int width, int height)
	{
		base.width = width;
		base.height = height;
		this.array = array;
	}

	public T[,] GetArray()
	{
		return array;
	}

	public void SetValue(int x, int y, T value)
	{
		Array[x, y] = value;
		UpdateAt(x, y);
	}

	public T GetValue(int x, int y)
	{
		return Array[x, y];
	}

	public void DetectContentArea()
	{
		hasContent = false;
		bool isFound = false;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (!array[x, y].Equals(defaultValue))
				{
					contentAreaStart.y = y;
					isFound = true;
					hasContent = true;
					break;
				}
			}
			if (isFound)
			{
				break;
			}
		}
		isFound = false;
		for (int y2 = height - 1; y2 >= 0; y2--)
		{
			for (int i = 0; i < width; i++)
			{
				if (!array[i, y2].Equals(defaultValue))
				{
					contentAreaEnd.y = y2;
					isFound = true;
					hasContent = true;
					break;
				}
			}
			if (isFound)
			{
				break;
			}
		}
		isFound = false;
		for (int j = 0; j < width; j++)
		{
			for (int k = 0; k < height; k++)
			{
				if (!array[j, k].Equals(defaultValue))
				{
					contentAreaStart.x = j;
					isFound = true;
					hasContent = true;
					break;
				}
			}
			if (isFound)
			{
				break;
			}
		}
		isFound = false;
		for (int x2 = width - 1; x2 >= 0; x2--)
		{
			for (int l = 0; l < height; l++)
			{
				if (!array[x2, l].Equals(defaultValue))
				{
					contentAreaEnd.x = x2;
					isFound = true;
					hasContent = true;
					break;
				}
			}
			if (isFound)
			{
				break;
			}
		}
		if (isNew && hasContent)
		{
			isVisible = true;
			isNew = false;
		}
		else if (!hasContent)
		{
			isVisible = false;
			isNew = true;
		}
	}

	public void FullContentArea()
	{
		hasContent = true;
		contentAreaStart = new Vector2i(0, 0);
		contentAreaEnd = new Vector2i(width - 1, height - 1);
		if (isNew)
		{
			isVisible = true;
			isNew = false;
		}
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
		hasContent = false;
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
		hasContent = false;
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

	public void CreateNormalTexture(Channel<float> heightChannel, int x, int y, int width, int height, bool invertY = false)
	{
		if (heightChannel.Type != TypeEnum.HEIGHT || !(array is Color[,]))
		{
			return;
		}
		base.width = heightChannel.Width;
		base.height = heightChannel.Height;
		Color[,] colorArray2 = Channel.ConvertHeightToNormal(heightChannel.Array, base.width, base.height, invertY);
		CastToColor().Array = colorArray2;
		int xxx = x + width;
		int yyy = y + height;
		if (image == null)
		{
			image = new Image();
			image.Create(base.width, base.height, useMipmaps: false, imageFormat);
		}
		image.Lock();
		for (int yy = y - 1; yy < yyy + 1; yy++)
		{
			int nyy = yy;
			if (nyy < 0)
			{
				nyy += base.height;
			}
			if (nyy > base.height - 1)
			{
				nyy -= base.height;
			}
			for (int xx = x - 1; xx < xxx + 1; xx++)
			{
				int nxx = xx;
				if (nxx < 0)
				{
					nxx += base.width;
				}
				if (nxx > base.width - 1)
				{
					nxx -= base.width;
				}
				image.SetPixel(nxx, nyy, colorArray2[nxx, nyy]);
			}
		}
		image.Unlock();
		if (imageTexture == null)
		{
			imageTexture = new ImageTexture();
			imageTexture.CreateFromImage(image, 2u);
		}
		VisualServer.TextureSetData(imageTexture.GetRid(), image);
	}

	public void CreateNormalTexture(Channel<Value> heightChannel, int x, int y, int width, int height, bool invertY = false)
	{
		if (heightChannel.Type != TypeEnum.HEIGHT || !(array is Color[,]))
		{
			return;
		}
		base.width = heightChannel.Width;
		base.height = heightChannel.Height;
		Color[,] colorArray2 = Channel.ConvertHeightToNormal(heightChannel.Array, base.width, base.height, invertY);
		CastToColor().Array = colorArray2;
		int xxx = x + width;
		int yyy = y + height;
		if (image == null)
		{
			image = new Image();
			image.Create(base.width, base.height, useMipmaps: false, imageFormat);
		}
		image.Lock();
		for (int yy = y - 1; yy < yyy + 1; yy++)
		{
			int nyy = yy;
			if (nyy < 0)
			{
				nyy += base.height;
			}
			if (nyy > base.height - 1)
			{
				nyy -= base.height;
			}
			for (int xx = x - 1; xx < xxx + 1; xx++)
			{
				int nxx = xx;
				if (nxx < 0)
				{
					nxx += base.width;
				}
				if (nxx > base.width - 1)
				{
					nxx -= base.width;
				}
				image.SetPixel(nxx, nyy, colorArray2[nxx, nyy]);
			}
		}
		image.Unlock();
		if (imageTexture == null)
		{
			imageTexture = new ImageTexture();
			imageTexture.CreateFromImage(image, 2u);
		}
		VisualServer.TextureSetData(imageTexture.GetRid(), image);
	}

	public void CreateTexture(int x, int y, int width, int height)
	{
		Color defaultColor = default(Color);
		Value defaultValue = default(Value);
		Color color = ColorExtension.Black;
		_ = Value.Black;
		switch (type)
		{
		case TypeEnum.COLOR:
			defaultColor = ColorExtension.Zero;
			break;
		case TypeEnum.ROUGHNESS:
			defaultValue = Value.White;
			break;
		case TypeEnum.METALLICITY:
			defaultValue = Value.Black;
			break;
		case TypeEnum.HEIGHT:
			defaultValue = Value.Gray;
			break;
		case TypeEnum.NORMAL:
			defaultColor = ColorExtension.Normal;
			break;
		case TypeEnum.EMISSION:
			defaultColor = ColorExtension.Black;
			break;
		}
		if (image == null)
		{
			image = new Image();
			image.Create(base.width, base.height, useMipmaps: false, imageFormat);
		}
		image.Lock();
		switch (dataType)
		{
		case DataTypeEnum.COLOR:
		{
			if (!(array is Color[,] colorArray))
			{
				break;
			}
			for (int yy2 = y + height - 1; yy2 >= y; yy2--)
			{
				for (int xx2 = x + width - 1; xx2 >= x; xx2--)
				{
					image.SetPixel(xx2, yy2, defaultColor.Blend(colorArray[xx2, yy2]));
				}
			}
			break;
		}
		case DataTypeEnum.VALUE:
		{
			if (!(array is Value[,] valueArray))
			{
				break;
			}
			for (int yy5 = y + height - 1; yy5 >= y; yy5--)
			{
				for (int xx5 = x + width - 1; xx5 >= x; xx5--)
				{
					color.r = (color.g = (color.b = defaultValue.Blend(valueArray[xx5, yy5]).v));
					image.SetPixel(xx5, yy5, color);
				}
			}
			break;
		}
		case DataTypeEnum.FLOAT:
		{
			if (!(array is float[,] floatArray))
			{
				break;
			}
			for (int yy3 = y + height - 1; yy3 >= y; yy3--)
			{
				for (int xx3 = x + width - 1; xx3 >= x; xx3--)
				{
					color.r = (color.g = (color.b = floatArray[xx3, yy3]));
					image.SetPixel(xx3, yy3, color);
				}
			}
			break;
		}
		case DataTypeEnum.BYTE:
		{
			if (!(array is byte[,] byteArray))
			{
				break;
			}
			for (int yy4 = y + height - 1; yy4 >= y; yy4--)
			{
				for (int xx4 = x + width - 1; xx4 >= x; xx4--)
				{
					int num = (color.b8 = byteArray[xx4, yy4]);
					int r = (color.g8 = num);
					color.r8 = r;
					image.SetPixel(xx4, yy4, color);
				}
			}
			break;
		}
		case DataTypeEnum.BOOL:
		{
			if (!(array is bool[,] boolArray))
			{
				break;
			}
			for (int yy = y + height - 1; yy >= y; yy--)
			{
				for (int xx = x + width - 1; xx >= x; xx--)
				{
					color.r = (color.g = (color.b = (boolArray[xx, yy] ? 1f : 0f)));
					image.SetPixel(xx, yy, color);
				}
			}
			break;
		}
		}
		image.Unlock();
		if (imageTexture == null)
		{
			imageTexture = new ImageTexture();
			imageTexture.CreateFromImage(image, 2u);
		}
		VisualServer.TextureSetData(imageTexture.GetRid(), image);
	}

	public ImageTexture CreateTexture()
	{
		if (type != TypeEnum.NORMAL)
		{
			CreateTexture(0, 0, width, height);
		}
		else if (linkedChannel != null && linkedChannel.DataType == DataTypeEnum.VALUE)
		{
			CreateNormalTexture(linkedChannel as Channel<Value>, 0, 0, width, height);
		}
		else if (linkedChannel != null && linkedChannel.DataType == DataTypeEnum.FLOAT)
		{
			CreateNormalTexture(linkedChannel as Channel<float>, 0, 0, width, height);
		}
		return imageTexture;
	}

	public bool UpdateTexture()
	{
		if (doUpdate)
		{
			if (type != TypeEnum.NORMAL)
			{
				CreateTexture(updateStart.x, updateStart.y, updateEnd.x - updateStart.x + 1, updateEnd.y - updateStart.y + 1);
			}
			else if (linkedChannel != null && linkedChannel.DataType == DataTypeEnum.VALUE)
			{
				CreateNormalTexture(linkedChannel as Channel<Value>, updateStart.x, updateStart.y, updateEnd.x - updateStart.x + 1, updateEnd.y - updateStart.y + 1);
			}
			else if (linkedChannel != null && linkedChannel.DataType == DataTypeEnum.FLOAT)
			{
				CreateNormalTexture(linkedChannel as Channel<float>, updateStart.x, updateStart.y, updateEnd.x - updateStart.x + 1, updateEnd.y - updateStart.y + 1);
			}
			updateStart = Vector2i.Maximum;
			updateEnd = Vector2i.Minimum;
			doUpdate = false;
			return true;
		}
		return false;
	}

	public void WriteToBinaryStream(BinaryWriter binaryWriter)
	{
		binaryWriter.Write((int)base.DataType);
		binaryWriter.Write((int)base.Type);
		binaryWriter.Write(base.Name);
		binaryWriter.Write(base.FileSuffix);
		binaryWriter.Write(width);
		binaryWriter.Write(height);
		binaryWriter.Write(isVisible);
		binaryWriter.Write((int)blendingMode);
		binaryWriter.Write(blendingStrength);
		if (base.LinkedChannel == null)
		{
			binaryWriter.Write(value: false);
		}
		else
		{
			binaryWriter.Write(value: true);
			binaryWriter.Write(base.LinkedChannel.Name);
		}
		binaryWriter.Write(new byte[64]);
		DetectContentArea();
		binaryWriter.Write(hasContent);
		switch (base.DataType)
		{
		case DataTypeEnum.COLOR:
		{
			Color defaultColor = (this as Channel<Color>).defaultValue;
			binaryWriter.Write((byte)defaultColor.r8);
			binaryWriter.Write((byte)defaultColor.g8);
			binaryWriter.Write((byte)defaultColor.b8);
			binaryWriter.Write((byte)defaultColor.a8);
			if (!hasContent)
			{
				break;
			}
			binaryWriter.Write(contentAreaStart.x);
			binaryWriter.Write(contentAreaStart.y);
			binaryWriter.Write(contentAreaEnd.x);
			binaryWriter.Write(contentAreaEnd.y);
			Color[,] colorArray = (this as Channel<Color>).Array;
			for (int i = contentAreaStart.y; i <= contentAreaEnd.y; i++)
			{
				for (int j = contentAreaStart.x; j <= contentAreaEnd.x; j++)
				{
					Color color = colorArray[j, i];
					Blender.Clamp(ref color);
					binaryWriter.Write((byte)color.r8);
					binaryWriter.Write((byte)color.g8);
					binaryWriter.Write((byte)color.b8);
					binaryWriter.Write((byte)color.a8);
				}
			}
			break;
		}
		case DataTypeEnum.VALUE:
		{
			Value defaultValue = (this as Channel<Value>).defaultValue;
			binaryWriter.Write((ushort)(defaultValue.v * 65535f));
			binaryWriter.Write(defaultValue.a8);
			if (!hasContent)
			{
				break;
			}
			binaryWriter.Write(contentAreaStart.x);
			binaryWriter.Write(contentAreaStart.y);
			binaryWriter.Write(contentAreaEnd.x);
			binaryWriter.Write(contentAreaEnd.y);
			Value[,] valueArray = (this as Channel<Value>).Array;
			for (int num = contentAreaStart.y; num <= contentAreaEnd.y; num++)
			{
				for (int num2 = contentAreaStart.x; num2 <= contentAreaEnd.x; num2++)
				{
					Value value = valueArray[num2, num];
					Blender.Clamp(ref value);
					binaryWriter.Write((ushort)(value.v * 65535f));
					binaryWriter.Write(value.a8);
				}
			}
			break;
		}
		case DataTypeEnum.FLOAT:
		{
			binaryWriter.Write((ushort)((this as Channel<float>).defaultValue * 65535f));
			if (!hasContent)
			{
				break;
			}
			binaryWriter.Write(contentAreaStart.x);
			binaryWriter.Write(contentAreaStart.y);
			binaryWriter.Write(contentAreaEnd.x);
			binaryWriter.Write(contentAreaEnd.y);
			float[,] floatArray = (this as Channel<float>).Array;
			for (int k = contentAreaStart.y; k <= contentAreaEnd.y; k++)
			{
				for (int l = contentAreaStart.x; l <= contentAreaEnd.x; l++)
				{
					float floatValue = floatArray[l, k];
					Blender.Clamp(ref floatValue);
					binaryWriter.Write((ushort)(floatValue * 65535f));
				}
			}
			break;
		}
		case DataTypeEnum.BOOL:
		{
			binaryWriter.Write((this as Channel<bool>).defaultValue);
			if (!hasContent)
			{
				break;
			}
			binaryWriter.Write(contentAreaStart.x);
			binaryWriter.Write(contentAreaStart.y);
			binaryWriter.Write(contentAreaEnd.x);
			binaryWriter.Write(contentAreaEnd.y);
			bool[,] boolArray = (this as Channel<bool>).Array;
			for (int m = contentAreaStart.y; m <= contentAreaEnd.y; m++)
			{
				for (int n = contentAreaStart.x; n <= contentAreaEnd.x; n++)
				{
					binaryWriter.Write(boolArray[n, m]);
				}
			}
			break;
		}
		case DataTypeEnum.INTEGER:
		{
			binaryWriter.Write((this as Channel<int>).defaultValue);
			if (!hasContent)
			{
				break;
			}
			binaryWriter.Write(contentAreaStart.x);
			binaryWriter.Write(contentAreaStart.y);
			binaryWriter.Write(contentAreaEnd.x);
			binaryWriter.Write(contentAreaEnd.y);
			int[,] integerArray = (this as Channel<int>).Array;
			for (int y = contentAreaStart.y; y <= contentAreaEnd.y; y++)
			{
				for (int x = contentAreaStart.x; x <= contentAreaEnd.x; x++)
				{
					binaryWriter.Write(integerArray[x, y]);
				}
			}
			break;
		}
		case DataTypeEnum.BYTE:
			break;
		}
	}

	public static Channel<T> ReadFromBinaryStream_009(BinaryReader binaryReader)
	{
		Channel<T> channel = new Channel<T>();
		channel.DataType = (DataTypeEnum)binaryReader.ReadInt32();
		channel.Type = (TypeEnum)binaryReader.ReadInt32();
		channel.Name = binaryReader.ReadString();
		channel.FileSuffix = binaryReader.ReadString();
		channel.Width = binaryReader.ReadInt32();
		channel.Height = binaryReader.ReadInt32();
		if (binaryReader.ReadBoolean())
		{
			channel.LinkedChannelName = binaryReader.ReadString();
		}
		switch (channel.DataType)
		{
		case DataTypeEnum.INTEGER:
		case DataTypeEnum.FLOAT:
		case DataTypeEnum.BOOL:
		{
			float[,] array3 = ((channel as Channel<float>).Array = new float[channel.Width, channel.Height]);
			float[,] floatArray = array3;
			for (int i = 0; i < channel.Height; i++)
			{
				for (int j = 0; j < channel.Width; j++)
				{
					floatArray[j, i] = binaryReader.ReadSingle();
				}
			}
			break;
		}
		case DataTypeEnum.COLOR:
		{
			Color[,] array = ((channel as Channel<Color>).Array = new Color[channel.Width, channel.Height]);
			Color[,] colorArray = array;
			for (int y = 0; y < channel.Height; y++)
			{
				for (int x = 0; x < channel.Width; x++)
				{
					colorArray[x, y].r = binaryReader.ReadSingle();
					colorArray[x, y].g = binaryReader.ReadSingle();
					colorArray[x, y].b = binaryReader.ReadSingle();
					colorArray[x, y].a = binaryReader.ReadSingle();
				}
			}
			break;
		}
		}
		channel.DetectContentArea();
		channel.Image.Create(channel.Width, channel.Height, useMipmaps: false, channel.imageFormat);
		channel.ImageTexture.CreateFromImage(channel.Image, 2u);
		if (channel.Type != TypeEnum.NORMAL)
		{
			channel.CreateTexture();
		}
		return channel;
	}

	public static Channel<T> ReadFromBinaryStream(BinaryReader binaryReader, bool doCreateImage = true)
	{
		Channel<T> channel = new Channel<T>(Image.Format.Rgba8, doCreateImage: false);
		channel.DataType = (DataTypeEnum)binaryReader.ReadInt32();
		channel.Type = (TypeEnum)binaryReader.ReadInt32();
		channel.Name = binaryReader.ReadString();
		channel.FileSuffix = binaryReader.ReadString();
		channel.Width = binaryReader.ReadInt32();
		channel.Height = binaryReader.ReadInt32();
		channel.IsVisible = binaryReader.ReadBoolean();
		channel.BlendingMode = (Blender.BlendingModeEnum)binaryReader.ReadInt32();
		channel.BlendingStrength = binaryReader.ReadSingle();
		if (binaryReader.ReadBoolean())
		{
			channel.LinkedChannelName = binaryReader.ReadString();
		}
		binaryReader.ReadBytes(64);
		channel.HasContent = binaryReader.ReadBoolean();
		switch (channel.DataType)
		{
		case DataTypeEnum.COLOR:
		{
			Color defaultColor = ColorExtension.Zero;
			defaultColor.r8 = binaryReader.ReadByte();
			defaultColor.g8 = binaryReader.ReadByte();
			defaultColor.b8 = binaryReader.ReadByte();
			defaultColor.a8 = binaryReader.ReadByte();
			(channel as Channel<Color>).defaultValue = defaultColor;
			Color[,] array7 = ((channel as Channel<Color>).Array = new Color[channel.Width, channel.Height]);
			Color[,] colorArray = array7;
			for (int num5 = 0; num5 < channel.Height; num5++)
			{
				for (int num6 = 0; num6 < channel.Width; num6++)
				{
					channel.Array[num6, num5] = channel.DefaultValue;
				}
			}
			if (!channel.HasContent)
			{
				break;
			}
			Vector2i vector2i4 = default(Vector2i);
			vector2i4.x = binaryReader.ReadInt32();
			vector2i4.y = binaryReader.ReadInt32();
			channel.ContentAreaStart = vector2i4;
			vector2i4.x = binaryReader.ReadInt32();
			vector2i4.y = binaryReader.ReadInt32();
			channel.ContentAreaEnd = vector2i4;
			for (int num7 = channel.ContentAreaStart.y; num7 <= channel.ContentAreaEnd.y; num7++)
			{
				for (int num8 = channel.ContentAreaStart.x; num8 <= channel.ContentAreaEnd.x; num8++)
				{
					colorArray[num8, num7].r8 = binaryReader.ReadByte();
					colorArray[num8, num7].g8 = binaryReader.ReadByte();
					colorArray[num8, num7].b8 = binaryReader.ReadByte();
					colorArray[num8, num7].a8 = binaryReader.ReadByte();
				}
			}
			channel.DetectContentArea();
			break;
		}
		case DataTypeEnum.VALUE:
		{
			(channel as Channel<Value>).defaultValue = new Value
			{
				v = 1f * (float)(int)binaryReader.ReadUInt16() / 65535f,
				a8 = binaryReader.ReadByte()
			};
			Value[,] array9 = ((channel as Channel<Value>).Array = new Value[channel.Width, channel.Height]);
			Value[,] valueArray = array9;
			for (int num9 = 0; num9 < channel.Height; num9++)
			{
				for (int num10 = 0; num10 < channel.Width; num10++)
				{
					channel.Array[num10, num9] = channel.DefaultValue;
				}
			}
			if (!channel.HasContent)
			{
				break;
			}
			Vector2i vector2i5 = default(Vector2i);
			vector2i5.x = binaryReader.ReadInt32();
			vector2i5.y = binaryReader.ReadInt32();
			channel.ContentAreaStart = vector2i5;
			vector2i5.x = binaryReader.ReadInt32();
			vector2i5.y = binaryReader.ReadInt32();
			channel.ContentAreaEnd = vector2i5;
			for (int num11 = channel.ContentAreaStart.y; num11 <= channel.ContentAreaEnd.y; num11++)
			{
				for (int num12 = channel.ContentAreaStart.x; num12 <= channel.ContentAreaEnd.x; num12++)
				{
					valueArray[num12, num11].v = 1f * (float)(int)binaryReader.ReadUInt16() / 65535f;
					valueArray[num12, num11].a8 = binaryReader.ReadByte();
				}
			}
			channel.DetectContentArea();
			break;
		}
		case DataTypeEnum.FLOAT:
		{
			(channel as Channel<float>).defaultValue = 1f * (float)(int)binaryReader.ReadUInt16() / 65535f;
			float[,] array3 = ((channel as Channel<float>).Array = new float[channel.Width, channel.Height]);
			float[,] floatArray = array3;
			for (int k = 0; k < channel.Height; k++)
			{
				for (int l = 0; l < channel.Width; l++)
				{
					channel.Array[l, k] = channel.DefaultValue;
				}
			}
			if (!channel.HasContent)
			{
				break;
			}
			Vector2i vector2i2 = default(Vector2i);
			vector2i2.x = binaryReader.ReadInt32();
			vector2i2.y = binaryReader.ReadInt32();
			channel.ContentAreaStart = vector2i2;
			vector2i2.x = binaryReader.ReadInt32();
			vector2i2.y = binaryReader.ReadInt32();
			channel.ContentAreaEnd = vector2i2;
			for (int m = channel.ContentAreaStart.y; m <= channel.ContentAreaEnd.y; m++)
			{
				for (int n = channel.ContentAreaStart.x; n <= channel.ContentAreaEnd.x; n++)
				{
					floatArray[n, m] = 1f * (float)(int)binaryReader.ReadUInt16() / 65535f;
				}
			}
			channel.DetectContentArea();
			break;
		}
		case DataTypeEnum.BOOL:
		{
			(channel as Channel<bool>).defaultValue = binaryReader.ReadBoolean();
			bool[,] array5 = ((channel as Channel<bool>).Array = new bool[channel.Width, channel.Height]);
			bool[,] boolArray = array5;
			for (int num = 0; num < channel.Height; num++)
			{
				for (int num2 = 0; num2 < channel.Width; num2++)
				{
					channel.Array[num2, num] = channel.DefaultValue;
				}
			}
			if (!channel.HasContent)
			{
				break;
			}
			Vector2i vector2i3 = default(Vector2i);
			vector2i3.x = binaryReader.ReadInt32();
			vector2i3.y = binaryReader.ReadInt32();
			channel.ContentAreaStart = vector2i3;
			vector2i3.x = binaryReader.ReadInt32();
			vector2i3.y = binaryReader.ReadInt32();
			channel.ContentAreaEnd = vector2i3;
			for (int num3 = channel.ContentAreaStart.y; num3 <= channel.ContentAreaEnd.y; num3++)
			{
				for (int num4 = channel.ContentAreaStart.x; num4 <= channel.ContentAreaEnd.x; num4++)
				{
					boolArray[num4, num3] = binaryReader.ReadBoolean();
				}
			}
			channel.DetectContentArea();
			break;
		}
		case DataTypeEnum.INTEGER:
		{
			(channel as Channel<int>).defaultValue = binaryReader.ReadInt32();
			int[,] array = ((channel as Channel<int>).Array = new int[channel.Width, channel.Height]);
			int[,] integerArray = array;
			for (int y = 0; y < channel.Height; y++)
			{
				for (int x = 0; x < channel.Width; x++)
				{
					channel.Array[x, y] = channel.DefaultValue;
				}
			}
			if (!channel.HasContent)
			{
				break;
			}
			Vector2i vector2i = default(Vector2i);
			vector2i.x = binaryReader.ReadInt32();
			vector2i.y = binaryReader.ReadInt32();
			channel.ContentAreaStart = vector2i;
			vector2i.x = binaryReader.ReadInt32();
			vector2i.y = binaryReader.ReadInt32();
			channel.ContentAreaEnd = vector2i;
			for (int i = channel.ContentAreaStart.y; i <= channel.ContentAreaEnd.y; i++)
			{
				for (int j = channel.ContentAreaStart.x; j <= channel.ContentAreaEnd.x; j++)
				{
					integerArray[j, i] = binaryReader.ReadInt32();
				}
			}
			channel.DetectContentArea();
			break;
		}
		}
		if (doCreateImage)
		{
			channel.Image = new Image();
			channel.Image.Create(channel.Width, channel.Height, useMipmaps: false, channel.ImageFormat);
			channel.ImageTexture = new ImageTexture();
			channel.ImageTexture.CreateFromImage(channel.Image, 2u);
			if (channel.Type != TypeEnum.NORMAL)
			{
				channel.CreateTexture();
			}
		}
		return channel;
	}
}
