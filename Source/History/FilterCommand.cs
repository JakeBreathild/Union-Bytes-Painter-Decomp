using Godot;

public class FilterCommand : ICommand
{
	private Data data;

	private History.CommandTypeEnum type = History.CommandTypeEnum.FILTER;

	private string name = "Filter";

	public Layer.ChannelEnum ChannelType;

	public Layer Layer;

	public Channel Channel;

	public Color[,] ColorArray;

	public Value[,] ValueArray;

	public Value[,] ValueArray2;

	public Value[,] ValueArray3;

	public Color[,] ColorArray2;

	public int[,] IntegerArray;

	public float[,] FloatArray;

	public bool[,] BoolArray;

	public Data Data => data;

	public int Type => (int)type;

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

	public FilterCommand(Worksheet worksheet)
	{
		data = worksheet.Data;
	}

	public void Execute()
	{
		if (Channel != null && ChannelType != Layer.ChannelEnum.ALL)
		{
			switch (Channel.DataType)
			{
			case Channel.DataTypeEnum.COLOR:
			{
				Color[,] colorTempArray = (Channel as Channel<Color>).Array;
				(Channel as Channel<Color>).SetArray(ColorArray, Channel.Width, Channel.Height);
				ColorArray = colorTempArray;
				Channel.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
				Channel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
				(Channel as Channel<Color>).DetectContentArea();
				break;
			}
			case Channel.DataTypeEnum.VALUE:
			{
				Value[,] valueTempArray = (Channel as Channel<Value>).Array;
				(Channel as Channel<Value>).SetArray(ValueArray, Channel.Width, Channel.Height);
				ValueArray = valueTempArray;
				Channel.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
				Channel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
				(Channel as Channel<Value>).DetectContentArea();
				break;
			}
			case Channel.DataTypeEnum.FLOAT:
			{
				float[,] floatTempArray = (Channel as Channel<float>).Array;
				(Channel as Channel<float>).SetArray(FloatArray, Channel.Width, Channel.Height);
				FloatArray = floatTempArray;
				Channel.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
				Channel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
				(Channel as Channel<float>).DetectContentArea();
				break;
			}
			case Channel.DataTypeEnum.INTEGER:
			{
				int[,] integerTempArray = (Channel as Channel<int>).Array;
				(Channel as Channel<int>).SetArray(IntegerArray, Channel.Width, Channel.Height);
				IntegerArray = integerTempArray;
				Channel.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
				Channel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
				(Channel as Channel<int>).DetectContentArea();
				break;
			}
			case Channel.DataTypeEnum.BOOL:
			{
				bool[,] boolTempArray = (Channel as Channel<bool>).Array;
				(Channel as Channel<bool>).SetArray(BoolArray, Channel.Width, Channel.Height);
				BoolArray = boolTempArray;
				Channel.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
				Channel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
				(Channel as Channel<bool>).DetectContentArea();
				break;
			}
			}
			data.CombineLayers(ChannelType);
			data.DoUpdate = true;
		}
		else if (Layer != null && ChannelType == Layer.ChannelEnum.ALL)
		{
			if (ColorArray != null)
			{
				Color[,] colorTempArray2 = Layer.ColorChannel.Array;
				Layer.ColorChannel.SetArray(ColorArray, Layer.ColorChannel.Width, Layer.ColorChannel.Height);
				ColorArray = colorTempArray2;
				Layer.ColorChannel.UpdateAt(Vector2i.Zero, new Vector2i(Layer.ColorChannel.Width - 1, Layer.ColorChannel.Height - 1));
				Layer.ColorChannel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Layer.ColorChannel.Width - 1, Layer.ColorChannel.Height - 1));
				Layer.ColorChannel.DetectContentArea();
				data.CombineLayers(Layer.ChannelEnum.COLOR);
			}
			if (ValueArray != null)
			{
				Value[,] valueTempArray2 = Layer.RoughnessChannel.Array;
				Layer.RoughnessChannel.SetArray(ValueArray, Layer.RoughnessChannel.Width, Layer.RoughnessChannel.Height);
				ValueArray = valueTempArray2;
				Layer.RoughnessChannel.UpdateAt(Vector2i.Zero, new Vector2i(Layer.RoughnessChannel.Width - 1, Layer.RoughnessChannel.Height - 1));
				Layer.RoughnessChannel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Layer.RoughnessChannel.Width - 1, Layer.RoughnessChannel.Height - 1));
				Layer.RoughnessChannel.DetectContentArea();
				data.CombineLayers(Layer.ChannelEnum.ROUGHNESS);
			}
			if (ValueArray2 != null)
			{
				Value[,] valueTempArray3 = Layer.MetallicityChannel.Array;
				Layer.MetallicityChannel.SetArray(ValueArray2, Layer.MetallicityChannel.Width, Layer.MetallicityChannel.Height);
				ValueArray2 = valueTempArray3;
				Layer.MetallicityChannel.UpdateAt(Vector2i.Zero, new Vector2i(Layer.MetallicityChannel.Width - 1, Layer.MetallicityChannel.Height - 1));
				Layer.MetallicityChannel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Layer.MetallicityChannel.Width - 1, Layer.MetallicityChannel.Height - 1));
				Layer.MetallicityChannel.DetectContentArea();
				data.CombineLayers(Layer.ChannelEnum.METALLICITY);
			}
			if (ValueArray3 != null)
			{
				Value[,] valueTempArray4 = Layer.HeightChannel.Array;
				Layer.HeightChannel.SetArray(ValueArray3, Layer.HeightChannel.Width, Layer.HeightChannel.Height);
				ValueArray3 = valueTempArray4;
				Layer.HeightChannel.UpdateAt(Vector2i.Zero, new Vector2i(Layer.HeightChannel.Width - 1, Layer.HeightChannel.Height - 1));
				Layer.HeightChannel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Layer.HeightChannel.Width - 1, Layer.HeightChannel.Height - 1));
				Layer.HeightChannel.DetectContentArea();
				data.CombineLayers(Layer.ChannelEnum.HEIGHT);
			}
			if (ColorArray2 != null)
			{
				Color[,] colorTempArray3 = Layer.EmissionChannel.Array;
				Layer.EmissionChannel.SetArray(ColorArray2, Layer.EmissionChannel.Width, Layer.EmissionChannel.Height);
				ColorArray2 = colorTempArray3;
				Layer.EmissionChannel.UpdateAt(Vector2i.Zero, new Vector2i(Layer.EmissionChannel.Width - 1, Layer.EmissionChannel.Height - 1));
				Layer.EmissionChannel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Layer.EmissionChannel.Width - 1, Layer.EmissionChannel.Height - 1));
				Layer.EmissionChannel.DetectContentArea();
				data.CombineLayers(Layer.ChannelEnum.EMISSION);
			}
			data.DoUpdate = true;
		}
		Register.LayerControl.Reset();
	}

	public void Undo()
	{
		Execute();
	}

	public void Redo()
	{
		Execute();
	}
}
