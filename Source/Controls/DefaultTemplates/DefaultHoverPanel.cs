using Godot;

public class DefaultHoverPanel : Panel
{
	[Export(PropertyHint.None, "")]
	public bool Hoverable;

	[Export(PropertyHint.None, "")]
	public StyleBox DefaultStyleBox;

	[Export(PropertyHint.None, "")]
	public StyleBox HoverStyleBox;

	[Export(PropertyHint.None, "")]
	public bool HideAfterMouseExited;

	public override void _Ready()
	{
		base._Ready();
		Set("custom_styles/panel", DefaultStyleBox);
		Connect(Signals.MouseEntered, this, "MouseEntered");
		Connect(Signals.MouseExited, this, "MouseExited");
	}

	public void MouseEntered()
	{
		InputManager.MouseEnteredUserInterface();
		if (HideAfterMouseExited)
		{
			Show();
		}
		if (Hoverable)
		{
			Set("custom_styles/panel", HoverStyleBox);
		}
	}

	public void MouseExited()
	{
		if (Hoverable)
		{
			Set("custom_styles/panel", DefaultStyleBox);
		}
		if (HideAfterMouseExited)
		{
			Hide();
		}
		InputManager.MouseExitedUserInterface();
	}

	public new void Show()
	{
		base.Show();
	}

	public new void Hide()
	{
		base.Hide();
	}

	public void KeepShownAfterMouseExited(bool enable)
	{
		HideAfterMouseExited = !enable;
	}
}
