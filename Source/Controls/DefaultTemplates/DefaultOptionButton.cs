using Godot;

public class DefaultOptionButton : OptionButton
{
	private PopupMenu popupMenu;

	public override void _Ready()
	{
		base._Ready();
		popupMenu = GetPopup();
		popupMenu.MouseFilter = MouseFilterEnum.Pass;
		popupMenu.Connect(Signals.IdPressed, this, "IdPressed");
		popupMenu.Connect(Signals.MouseEntered, this, "MouseEntered");
		popupMenu.Connect(Signals.MouseExited, this, "MouseExited");
		popupMenu.Connect(Signals.AboutToShow, this, "MouseEntered");
		popupMenu.Connect(Signals.Hide, this, "MouseExited");
	}

	public void MouseEntered()
	{
		InputManager.MouseEnteredUserInterface();
	}

	public void MouseExited()
	{
		InputManager.MouseExitedUserInterface();
	}

	public void IdPressed(int index)
	{
		InputManager.MouseExitedUserInterface();
	}
}
