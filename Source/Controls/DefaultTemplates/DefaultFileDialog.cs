using Godot;

public class DefaultFileDialog : FileDialog
{
	private bool setWindowHidden = true;

	public bool SetWindowHidden
	{
		get
		{
			return setWindowHidden;
		}
		set
		{
			setWindowHidden = value;
		}
	}

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
		if (setWindowHidden)
		{
			InputManager.WindowHidden();
		}
		setWindowHidden = true;
		InputManager.SkipInput = true;
	}
}
