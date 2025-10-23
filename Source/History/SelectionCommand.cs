public class SelectionCommand : ICommand
{
	private Layer layer;

	private Data data;

	private History.CommandTypeEnum type = History.CommandTypeEnum.DRAWING;

	private string name = "Selection";

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

	public SelectionCommand(Worksheet worksheet)
	{
		data = worksheet.Data;
		layer = data.Layer;
	}

	public void Execute()
	{
	}

	public void Undo()
	{
	}

	public void Redo()
	{
	}
}
