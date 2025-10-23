using System.IO;
using Godot;

public class Layer
{
	public enum ChannelEnum
	{
		COLOR,
		ROUGHNESS,
		METALLICITY,
		HEIGHT,
		EMISSION,
		ALPHA,
		ALL
	}

	public static string[] ChannelName = new string[7] { "Color", "Roughness", "Metallicity", "Height", "Emission", "Alpha", "All" };

	private static int counter = 5;

	private static int steps = 12;

	private Data data;

	private string name = "";

	private Color color;

	private int width;

	private int height;

	private bool isLocked;

	private bool isVisible = true;

	private float blendingStrength = 1f;

	private Channel<Color> colorChannel;

	private Channel<Value> roughnessChannel;

	private Channel<Value> metallicityChannel;

	private Channel<Value> heightChannel;

	private Channel<Color> emissionChannel;

	private Channel<float> alphaChannel;

	protected Vector2i contentAreaStart = Vector2i.Maximum;

	protected Vector2i contentAreaEnd = Vector2i.Minimum;

	protected bool hasContent;

	protected Vector2 contentCenter = new Vector2(float.MinValue, float.MinValue);

	public Data Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
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

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
		}
	}

	public int Width
	{
		get
		{
			return width;
		}
		private set
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
		private set
		{
			height = value;
		}
	}

	public bool IsLocked
	{
		get
		{
			return isLocked;
		}
		set
		{
			isLocked = value;
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

	public Channel<Color> ColorChannel
	{
		get
		{
			return colorChannel;
		}
		set
		{
			colorChannel = value;
		}
	}

	public Channel<Value> RoughnessChannel
	{
		get
		{
			return roughnessChannel;
		}
		set
		{
			roughnessChannel = value;
		}
	}

	public Channel<Value> MetallicityChannel
	{
		get
		{
			return metallicityChannel;
		}
		set
		{
			metallicityChannel = value;
		}
	}

	public Channel<Value> HeightChannel
	{
		get
		{
			return heightChannel;
		}
		set
		{
			heightChannel = value;
		}
	}

	public Channel<Color> EmissionChannel
	{
		get
		{
			return emissionChannel;
		}
		set
		{
			emissionChannel = value;
		}
	}

	public Channel<float> AlphaChannel
	{
		get
		{
			return alphaChannel;
		}
		set
		{
			alphaChannel = value;
		}
	}

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

	public Vector2 ContentCenter
	{
		get
		{
			return contentCenter;
		}
		set
		{
			contentCenter = value;
		}
	}

	private Layer(Data data)
	{
		this.data = data;
		color = Color.FromHsv(1f * (float)counter / (float)steps, 0.6f, 1f);
		counter++;
		if (counter >= steps)
		{
			counter = 0;
		}
	}

	public Layer(Data data, string name, int width, int height, bool initChannels = true, bool background = false)
	{
		this.data = data;
		this.name = name;
		this.width = width;
		this.height = height;
		color = Color.FromHsv(1f * (float)counter / (float)steps, 0.6f, 1f);
		counter++;
		if (counter >= steps)
		{
			counter = 0;
		}
		if (initChannels)
		{
			if (!background)
			{
				colorChannel = new Channel<Color>(this.width, this.height, ColorExtension.Zero, Channel.TypeEnum.COLOR, "Color", "C", Image.Format.Rgba8, doCreateImage: false);
				colorChannel.BlendingMode = Blender.BlendingModeEnum.ALPHABLEND;
				roughnessChannel = new Channel<Value>(this.width, this.height, Value.Zero, Channel.TypeEnum.ROUGHNESS, "Roughness", "R", Image.Format.Rgb8, doCreateImage: false);
				roughnessChannel.BlendingMode = Blender.BlendingModeEnum.ALPHABLEND;
				metallicityChannel = new Channel<Value>(this.width, this.height, Value.Zero, Channel.TypeEnum.METALLICITY, "Metallicity", "M", Image.Format.Rgb8, doCreateImage: false);
				metallicityChannel.BlendingMode = Blender.BlendingModeEnum.ALPHABLEND;
				heightChannel = new Channel<Value>(this.width, this.height, Value.Zero, Channel.TypeEnum.HEIGHT, "Height", "H", Image.Format.Rgb8, doCreateImage: false);
				heightChannel.BlendingMode = Blender.BlendingModeEnum.ALPHABLEND;
				emissionChannel = new Channel<Color>(this.width, this.height, ColorExtension.Zero, Channel.TypeEnum.EMISSION, "Emission", "E", Image.Format.Rgb8, doCreateImage: false);
				emissionChannel.BlendingMode = Blender.BlendingModeEnum.ALPHABLEND;
				return;
			}
			colorChannel = new Channel<Color>(this.width, this.height, ColorExtension.Zero, Channel.TypeEnum.COLOR, "Color", "C", Image.Format.Rgba8, doCreateImage: false);
			colorChannel.Clear(ColorExtension.White);
			colorChannel.FullContentArea();
			colorChannel.IsVisible = true;
			roughnessChannel = new Channel<Value>(this.width, this.height, Value.Zero, Channel.TypeEnum.ROUGHNESS, "Roughness", "R", Image.Format.Rgb8, doCreateImage: false);
			roughnessChannel.Clear(Value.White);
			roughnessChannel.FullContentArea();
			roughnessChannel.IsVisible = true;
			metallicityChannel = new Channel<Value>(this.width, this.height, Value.Zero, Channel.TypeEnum.METALLICITY, "Metallicity", "M", Image.Format.Rgb8, doCreateImage: false);
			metallicityChannel.Clear(Value.Black);
			metallicityChannel.FullContentArea();
			metallicityChannel.IsVisible = true;
			heightChannel = new Channel<Value>(this.width, this.height, Value.Zero, Channel.TypeEnum.HEIGHT, "Height", "H", Image.Format.Rgb8, doCreateImage: false);
			heightChannel.Clear(new Value(0.5f));
			heightChannel.FullContentArea();
			heightChannel.IsVisible = true;
			emissionChannel = new Channel<Color>(this.width, this.height, ColorExtension.Zero, Channel.TypeEnum.EMISSION, "Emission", "E", Image.Format.Rgb8, doCreateImage: false);
			emissionChannel.Clear(ColorExtension.Black);
			emissionChannel.FullContentArea();
			emissionChannel.IsVisible = true;
		}
	}

	public Layer Clone()
	{
		return new Layer(data, name, width, height, initChannels: false)
		{
			color = color,
			IsLocked = isLocked,
			IsVisible = isVisible,
			BlendingStrength = blendingStrength,
			ColorChannel = colorChannel.Clone(),
			HeightChannel = heightChannel.Clone(),
			RoughnessChannel = roughnessChannel.Clone(),
			MetallicityChannel = metallicityChannel.Clone(),
			EmissionChannel = emissionChannel.Clone()
		};
	}

	public void UpdateContentArea()
	{
		hasContent = false;
		contentAreaStart = Vector2i.Maximum;
		contentAreaEnd = Vector2i.Minimum;
		contentCenter = new Vector2(float.MinValue, float.MinValue);
		Channel channel = colorChannel;
		if (channel.HasContent)
		{
			contentAreaStart.SetMin(channel.ContentAreaStart);
			contentAreaEnd.SetMax(channel.ContentAreaEnd);
			hasContent = true;
		}
		channel = roughnessChannel;
		if (channel.HasContent)
		{
			contentAreaStart.SetMin(channel.ContentAreaStart);
			contentAreaEnd.SetMax(channel.ContentAreaEnd);
			hasContent = true;
		}
		channel = metallicityChannel;
		if (channel.HasContent)
		{
			contentAreaStart.SetMin(channel.ContentAreaStart);
			contentAreaEnd.SetMax(channel.ContentAreaEnd);
			hasContent = true;
		}
		channel = heightChannel;
		if (channel.HasContent)
		{
			contentAreaStart.SetMin(channel.ContentAreaStart);
			contentAreaEnd.SetMax(channel.ContentAreaEnd);
			hasContent = true;
		}
		channel = emissionChannel;
		if (channel.HasContent)
		{
			contentAreaStart.SetMin(channel.ContentAreaStart);
			contentAreaEnd.SetMax(channel.ContentAreaEnd);
			hasContent = true;
		}
		if (hasContent)
		{
			contentCenter.x = 1f * (float)contentAreaStart.x + 0.5f * (float)(contentAreaEnd.x + 1 - contentAreaStart.x);
			contentCenter.y = 1f * (float)contentAreaStart.y + 0.5f * (float)(contentAreaEnd.y + 1 - contentAreaStart.y);
		}
	}

	public void WriteToBinaryStream(BinaryWriter binaryWriter)
	{
		binaryWriter.Write(name);
		binaryWriter.Write(color.r);
		binaryWriter.Write(color.g);
		binaryWriter.Write(color.b);
		binaryWriter.Write(color.a);
		binaryWriter.Write(width);
		binaryWriter.Write(height);
		binaryWriter.Write(isLocked);
		binaryWriter.Write(isVisible);
		binaryWriter.Write(blendingStrength);
		binaryWriter.Write(new byte[64]);
		colorChannel.WriteToBinaryStream(binaryWriter);
		roughnessChannel.WriteToBinaryStream(binaryWriter);
		metallicityChannel.WriteToBinaryStream(binaryWriter);
		heightChannel.WriteToBinaryStream(binaryWriter);
		emissionChannel.WriteToBinaryStream(binaryWriter);
		if (alphaChannel == null)
		{
			binaryWriter.Write(value: false);
			return;
		}
		binaryWriter.Write(value: true);
		alphaChannel.WriteToBinaryStream(binaryWriter);
	}

	public static Layer ReadFromBinaryStream(BinaryReader binaryReader, Data data)
	{
		Layer layer = new Layer(data);
		layer.Name = binaryReader.ReadString();
		layer.Color = new Color
		{
			r = binaryReader.ReadSingle(),
			g = binaryReader.ReadSingle(),
			b = binaryReader.ReadSingle(),
			a = binaryReader.ReadSingle()
		};
		layer.Width = binaryReader.ReadInt32();
		layer.Height = binaryReader.ReadInt32();
		layer.IsLocked = binaryReader.ReadBoolean();
		layer.IsVisible = binaryReader.ReadBoolean();
		layer.BlendingStrength = binaryReader.ReadSingle();
		binaryReader.ReadBytes(64);
		layer.ColorChannel = Channel<Color>.ReadFromBinaryStream(binaryReader, doCreateImage: false);
		layer.RoughnessChannel = Channel<Value>.ReadFromBinaryStream(binaryReader, doCreateImage: false);
		layer.MetallicityChannel = Channel<Value>.ReadFromBinaryStream(binaryReader, doCreateImage: false);
		layer.HeightChannel = Channel<Value>.ReadFromBinaryStream(binaryReader, doCreateImage: false);
		layer.EmissionChannel = Channel<Color>.ReadFromBinaryStream(binaryReader, doCreateImage: false);
		if (binaryReader.ReadBoolean())
		{
			layer.AlphaChannel = Channel<float>.ReadFromBinaryStream(binaryReader, doCreateImage: false);
		}
		return layer;
	}
}
