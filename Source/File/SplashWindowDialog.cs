using Godot;

public class SplashWindowDialog : WindowDialog
{
	[Export(PropertyHint.None, "")]
	private bool doShowOnStart;

	private Label versionLabel;

	public override void _Ready()
	{
		base._Ready();
		Connect(Signals.Hide, this, "Hide");
		versionLabel = GetNodeOrNull<Label>("Version");
		versionLabel.Text = "Demo V" + 1000.ToString("0,000") + " (" + 2024.ToString("0000") + "-" + 10.ToString("00") + "-" + 5.ToString("00") + ")";
		if (doShowOnStart)
		{
			CallDeferred("PopupCentered");
			InputManager.WindowShown();
		}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (base.Visible)
		{
			InputManager.WindowShown();
		}
	}

	public new void PopupCentered(Vector2? size = null)
	{
		InputManager.WindowShown();
		base.PopupCentered(size);
	}

	public void PopupCentered()
	{
		PopupCentered(null);
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
