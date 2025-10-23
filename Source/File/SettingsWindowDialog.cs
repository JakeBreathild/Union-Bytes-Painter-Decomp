using Godot;

public class SettingsWindowDialog : WindowDialog
{
	private Gui gui;

	private Workspace workspace;

	[Export(PropertyHint.None, "")]
	private NodePath keyBindingsContainerNodePath;

	private CheckButton saveWindowPositionCheckButton;

	private CheckButton showControlsHelpCheckButton;

	public KeyBindingsContainer KeyBindingsContainer { get; private set; }

	public override void _Ready()
	{
		base._Ready();
		gui = Register.Gui;
		workspace = Register.Workspace;
		saveWindowPositionCheckButton = GetNodeOrNull<CheckButton>("TabContainer/SETTINGS/SC/VC/GroupPanel/VC/SaveWindowPosition");
		showControlsHelpCheckButton = GetNodeOrNull<CheckButton>("TabContainer/SETTINGS/SC/VC/GroupPanel/VC/ShowControlsHelp");
		KeyBindingsContainer = GetNodeOrNull<KeyBindingsContainer>(keyBindingsContainerNodePath);
		Connect(Signals.Hide, this, "Hide");
	}

	public new void PopupCentered(Vector2? size = null)
	{
		InputManager.WindowShown();
		Reset();
		base.PopupCentered(size);
	}

	public new void Show()
	{
		InputManager.WindowShown();
		Reset();
		base.Show();
	}

	public new void Hide()
	{
		base.Hide();
		InputManager.WindowHidden();
		InputManager.SkipInput = true;
	}

	public void Reset()
	{
		saveWindowPositionCheckButton.Pressed = Settings.SaveWindowPosition;
		showControlsHelpCheckButton.Pressed = Settings.ShowControlsHelp;
		KeyBindingsContainer.UpdateKeyBindings();
	}

	public void Cancel()
	{
		KeyBindingsContainer.Cancel();
		Hide();
	}

	public void Apply()
	{
		Settings.SaveWindowPosition = saveWindowPositionCheckButton.Pressed;
		Settings.ShowControlsHelp = showControlsHelpCheckButton.Pressed;
		KeyBindingsContainer.Apply();
		Hide();
	}
}
