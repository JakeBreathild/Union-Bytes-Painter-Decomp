using Godot;

public class KeyBindingsContainer : VBoxContainer
{
	[Export(PropertyHint.None, "")]
	private PackedScene keyBindingEntryPackedScene;

	[Export(PropertyHint.None, "")]
	private PackedScene keyBindingSeperatorPackedScene;

	private Workspace workspace;

	private bool isRecording;

	public bool IsRecording
	{
		get
		{
			return isRecording;
		}
		set
		{
			isRecording = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
	}

	public void Initialisation()
	{
		for (int i = 0; i < 12; i++)
		{
			KeyBindingsList keyBindingsList = InputManager.KeyBindingsLists[i];
			Panel panel = keyBindingSeperatorPackedScene.InstanceOrNull<Panel>();
			AddChild(panel);
			panel.Owner = this;
			panel.GetChildOrNull<Label>(0).Text = Tr(keyBindingsList.Name);
			foreach (KeyBinding keyBinding in keyBindingsList.KeyBindings)
			{
				KeyBindingEntry keyBindingEntry = keyBindingEntryPackedScene.InstanceOrNull<KeyBindingEntry>();
				AddChild(keyBindingEntry);
				keyBindingEntry.Owner = this;
				keyBindingEntry.SetKeyBindingsContainer(this);
				keyBindingEntry.SetKeyBinding(keyBinding);
			}
		}
	}

	public void UpdateKeyBindings()
	{
		for (int i = 0; i < GetChildCount(); i++)
		{
			GetChildOrNull<KeyBindingEntry>(i)?.UpdateKeyBinding();
		}
	}

	public void Apply()
	{
		bool hasKeyBindingChanged = false;
		for (int i = 0; i < GetChildCount(); i++)
		{
			KeyBindingEntry keyBindingEntry = GetChildOrNull<KeyBindingEntry>(i);
			if (keyBindingEntry != null && keyBindingEntry.NewKeyBinding != null)
			{
				keyBindingEntry.ApplyNewKeyBinding();
				hasKeyBindingChanged = true;
			}
		}
		if (hasKeyBindingChanged)
		{
			Register.Gui.UpdateKeyBindings();
		}
	}

	public void Cancel()
	{
		for (int i = 0; i < GetChildCount(); i++)
		{
			KeyBindingEntry keyBindingEntry = GetChildOrNull<KeyBindingEntry>(i);
			if (keyBindingEntry != null)
			{
				keyBindingEntry.NewKeyBinding = null;
			}
		}
	}
}
