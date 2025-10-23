using Godot;

public class DefaultScrollContainer : ScrollContainer
{
	public override void _Ready()
	{
		base._Ready();
		GetVScrollbar().MouseFilter = MouseFilterEnum.Pass;
	}
}
