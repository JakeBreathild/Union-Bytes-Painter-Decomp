using Godot;

public class KeyBindingEntry : Panel
{
	private Workspace workspace;

	private KeyBindingsContainer keyBindingsContainer;

	private KeyBinding keyBinding;

	private KeyBinding newKeyBinding;

	private Label nameLabel;

	private Button keyButton;

	private bool isWaitingInput;

	public KeyBinding KeyBinding
	{
		get
		{
			return keyBinding;
		}
		set
		{
			keyBinding = value;
		}
	}

	public KeyBinding NewKeyBinding
	{
		get
		{
			return newKeyBinding;
		}
		set
		{
			newKeyBinding = value;
		}
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (!isWaitingInput)
		{
			return;
		}
		if (@event is InputEventKey { Pressed: false } inputEventKey)
		{
			if (inputEventKey.Scancode == 16777217)
			{
				keyButton.Pressed = false;
				keyBindingsContainer.IsRecording = false;
				isWaitingInput = false;
				SetKeyButtonText(keyBinding);
				SetBlockSignals(enable: false);
			}
			else if (inputEventKey.Scancode != 16777237 && inputEventKey.Scancode != 16777238 && inputEventKey.Scancode != 16777240)
			{
				newKeyBinding = (KeyBinding)keyBinding.Clone();
				newKeyBinding.Type = KeyBinding.TypeEnum.KEY;
				newKeyBinding.Key = (int)inputEventKey.Scancode;
				newKeyBinding.Button = 0;
				newKeyBinding.Modifiers[0] = Input.IsKeyPressed(16777237);
				newKeyBinding.Modifiers[1] = Input.IsKeyPressed(16777238);
				newKeyBinding.Modifiers[2] = Input.IsKeyPressed(16777240);
				SetKeyButtonText(newKeyBinding);
				keyButton.Pressed = false;
				keyBindingsContainer.IsRecording = false;
				isWaitingInput = false;
				SetBlockSignals(enable: false);
			}
		}
		if (@event is InputEventMouseButton inputEventMouseButton)
		{
			newKeyBinding = (KeyBinding)keyBinding.Clone();
			newKeyBinding.Type = KeyBinding.TypeEnum.MOUSE;
			newKeyBinding.Button = inputEventMouseButton.ButtonIndex;
			newKeyBinding.Key = 0;
			newKeyBinding.Modifiers[0] = Input.IsKeyPressed(16777237);
			newKeyBinding.Modifiers[1] = Input.IsKeyPressed(16777238);
			newKeyBinding.Modifiers[2] = Input.IsKeyPressed(16777240);
			SetKeyButtonText(newKeyBinding);
			keyButton.Pressed = false;
			keyBindingsContainer.IsRecording = false;
			isWaitingInput = false;
			SetBlockSignals(enable: false);
		}
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		nameLabel = GetChildOrNull<Label>(0);
		keyButton = GetChildOrNull<Button>(1);
		keyButton.Connect(Signals.Pressed, this, "KeyButtonPressed");
	}

	public void SetKeyBindingsContainer(KeyBindingsContainer keyBindingsContainer)
	{
		this.keyBindingsContainer = keyBindingsContainer;
	}

	public void SetKeyButtonText(KeyBinding keyBinding)
	{
		keyButton.Text = "";
		if (keyBinding.Modifiers[0])
		{
			keyButton.Text += "[Shift] + ";
		}
		if (keyBinding.Modifiers[1])
		{
			keyButton.Text += "[Ctrl] + ";
		}
		if (keyBinding.Modifiers[2])
		{
			keyButton.Text += "[Alt] + ";
		}
		switch (keyBinding.Type)
		{
		case KeyBinding.TypeEnum.KEY:
		{
			Button button3 = keyButton;
			string text2 = button3.Text;
			KeyList key = (KeyList)keyBinding.Key;
			button3.Text = text2 + "[" + key.ToString().ToUpper() + "]";
			break;
		}
		case KeyBinding.TypeEnum.MOUSE:
		{
			Button button = keyButton;
			string text = button.Text;
			ButtonList button2 = (ButtonList)keyBinding.Button;
			button.Text = text + "[MOUSE: " + button2.ToString().ToUpper() + "]";
			break;
		}
		}
	}

	public void SetKeyBinding(KeyBinding keyBinding)
	{
		this.keyBinding = keyBinding;
		nameLabel.Text = Tr(this.keyBinding.Name);
		SetKeyButtonText(this.keyBinding);
	}

	public void UpdateKeyBinding()
	{
		SetKeyButtonText(keyBinding);
	}

	public void ApplyNewKeyBinding()
	{
		keyBinding.Type = newKeyBinding.Type;
		keyBinding.Key = newKeyBinding.Key;
		keyBinding.Button = newKeyBinding.Button;
		keyBinding.Modifiers[0] = newKeyBinding.Modifiers[0];
		keyBinding.Modifiers[1] = newKeyBinding.Modifiers[1];
		keyBinding.Modifiers[2] = newKeyBinding.Modifiers[2];
		newKeyBinding = null;
	}

	public void KeyButtonPressed()
	{
		if (!keyBindingsContainer.IsRecording && !isWaitingInput)
		{
			keyButton.Pressed = true;
			keyBindingsContainer.IsRecording = true;
			isWaitingInput = true;
			keyButton.Text = "...";
			SetBlockSignals(enable: true);
		}
		else
		{
			keyButton.Pressed = false;
		}
	}
}
