using Godot;

public class DefaultHBoxContainer : HBoxContainer, IContainer
{
	protected Workspace workspace;

	public new virtual void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
	}

	public virtual void Reset()
	{
	}
}
