using Godot;

public class DefaultMenuButton : MenuButton, IMenuButton
{
	protected Gui gui;

	protected Workspace workspace;

	protected PopupMenu popupMenu;

	public new virtual void _Ready()
	{
		base._Ready();
		gui = Register.Gui;
		workspace = Register.Workspace;
		popupMenu = GetPopup();
		popupMenu.Connect(Signals.AboutToShow, this, "PopupShow");
		popupMenu.Connect(Signals.Hide, this, "PopupHide");
	}

	public void MouseEntered()
	{
		InputManager.MouseEnteredUserInterface();
	}

	public void MouseExited()
	{
		InputManager.MouseExitedUserInterface();
	}

	public void PopupShow()
	{
		InputManager.MouseEnteredUserInterface();
	}

	public void PopupHide()
	{
		InputManager.MouseExitedUserInterface();
		InputManager.SkipInput = true;
	}

	public virtual void Reset()
	{
	}
}
