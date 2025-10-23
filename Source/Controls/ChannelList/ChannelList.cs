using Godot;

public class ChannelList : ItemList
{
	public override void _Ready()
	{
		base._Ready();
		AddItem("Full");
		AddItem("Color");
		AddItem("Roughness");
		AddItem("Metallicity");
		AddItem("Height");
		AddItem("Normal");
		AddItem("Emission");
		Select(0);
		Connect(Signals.ItemSelected, Register.Workspace, "ChangeCurrentChannel");
	}

	public void Reset()
	{
		Select(0);
	}
}
