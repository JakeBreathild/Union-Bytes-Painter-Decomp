using Godot;

public class DefaultColorPickerButton : ColorPickerButton
{
	public override void _Ready()
	{
		base._Ready();
		Connect(Signals.Pressed, this, "PopupCreated");
		Connect(Signals.PopupClosed, this, "PopupClosed");
	}

	public void PopupCreated()
	{
		InputManager.MouseEnteredUserInterface();
	}

	public void PopupClosed()
	{
		InputManager.MouseExitedUserInterface();
		InputManager.SkipInput = true;
	}
}
