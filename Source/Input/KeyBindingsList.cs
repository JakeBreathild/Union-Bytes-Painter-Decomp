using System;
using System.Collections.Generic;
using Godot;

public class KeyBindingsList
{
	private string name = "";

	private bool enable;

	private List<KeyBinding> keyBindings;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public bool Enable
	{
		get
		{
			return enable;
		}
		set
		{
			enable = value;
		}
	}

	public List<KeyBinding> KeyBindings => keyBindings;

	public KeyBindingsList(string name, bool enable = false)
	{
		this.name = name;
		this.enable = enable;
		keyBindings = new List<KeyBinding>();
	}

	public KeyBinding AddKeyBinding(string name, Action action, KeyList key, KeyBinding.EventTypeEnum eventType, bool shift = false, bool ctrl = false, bool alt = false, int group = 0)
	{
		KeyBinding keyBinding = new KeyBinding(name, action, key, eventType, shift, ctrl, alt, group);
		keyBindings.Add(keyBinding);
		return keyBinding;
	}

	public KeyBinding AddKeyBinding(string name, Action action, ButtonList button, KeyBinding.EventTypeEnum eventType, bool shift = false, bool ctrl = false, bool alt = false, int group = 0)
	{
		KeyBinding keyBinding = new KeyBinding(name, action, button, eventType, shift, ctrl, alt, group);
		keyBindings.Add(keyBinding);
		return keyBinding;
	}
}
