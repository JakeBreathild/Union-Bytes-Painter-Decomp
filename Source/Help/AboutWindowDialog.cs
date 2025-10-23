using Godot;

public class AboutWindowDialog : WindowDialog
{
	public override void _Ready()
	{
		base._Ready();
		Connect(Signals.Hide, this, "Hide");
	}

	public new void PopupCentered(Vector2? size = null)
	{
		InputManager.WindowShown();
		base.PopupCentered(size);
	}

	public new void Show()
	{
		InputManager.WindowShown();
		base.Show();
	}

	public new void Hide()
	{
		base.Hide();
		InputManager.WindowHidden();
		InputManager.SkipInput = true;
	}
}
