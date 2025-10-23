using Godot;

public class BakeCommand : ICommand
{
	private Data data;

	private Layer layer;

	private History.CommandTypeEnum type = History.CommandTypeEnum.BAKE;

	private string name = "Baking";

	public Channel Channel;

	public int[,] IntegerArray;

	public float[,] FloatArray;

	public Color[,] ColorArray;

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

	public BakeCommand(Worksheet worksheet)
	{
		data = worksheet.Data;
		layer = data.Layer;
	}

	public void Execute()
	{
		if (Channel != null)
		{
			switch (Channel.DataType)
			{
			case Channel.DataTypeEnum.INTEGER:
			{
				int[,] integerTempArray = (Channel as Channel<int>).Array;
				(Channel as Channel<int>).SetArray(IntegerArray, Channel.Width, Channel.Height);
				IntegerArray = integerTempArray;
				break;
			}
			case Channel.DataTypeEnum.FLOAT:
			{
				float[,] floatTempArray = (Channel as Channel<float>).Array;
				(Channel as Channel<float>).SetArray(FloatArray, Channel.Width, Channel.Height);
				FloatArray = floatTempArray;
				break;
			}
			case Channel.DataTypeEnum.COLOR:
			{
				Color[,] colorTempArray = (Channel as Channel<Color>).Array;
				(Channel as Channel<Color>).SetArray(ColorArray, Channel.Width, Channel.Height);
				ColorArray = colorTempArray;
				break;
			}
			case Channel.DataTypeEnum.BOOL:
			{
				bool[,] boolTempArray = (Channel as Channel<bool>).Array;
				(Channel as Channel<bool>).SetArray(BoolArray, Channel.Width, Channel.Height);
				BoolArray = boolTempArray;
				break;
			}
			}
			Channel.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
			Channel.LinkedChannel?.UpdateAt(Vector2i.Zero, new Vector2i(Channel.Width - 1, Channel.Height - 1));
			data.DoUpdate = true;
			Register.LayerControl.Reset();
		}
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
