using System;
using Godot;

public class KeyBinding : ICloneable
{
	public enum TypeEnum
	{
		KEY,
		MOUSE
	}

	public enum EventTypeEnum
	{
		JUST_PRESSED,
		PRESSED,
		JUST_RELEASED
	}

	public TypeEnum Type;

	public string Name;

	public int Group;

	public Action Action;

	public int Key;

	public int Button;

	public EventTypeEnum EventType;

	public bool[] Modifiers;

	public KeyBinding(string name, Action action, KeyList key, EventTypeEnum eventType, bool shift = false, bool ctrl = false, bool alt = false, int group = 0)
	{
		Type = TypeEnum.KEY;
		Name = name;
		Group = group;
		Action = action;
		Key = (int)key;
		EventType = eventType;
		Modifiers = new bool[3];
		Modifiers[0] = shift;
		Modifiers[1] = ctrl;
		Modifiers[2] = alt;
	}

	public KeyBinding(string name, Action action, ButtonList button, EventTypeEnum eventType, bool shift = false, bool ctrl = false, bool alt = false, int group = 0)
	{
		Type = TypeEnum.MOUSE;
		Name = name;
		Group = group;
		Action = action;
		Button = (int)button;
		EventType = eventType;
		Modifiers = new bool[3];
		Modifiers[0] = shift;
		Modifiers[1] = ctrl;
		Modifiers[2] = alt;
	}

	public object Clone()
	{
		KeyBinding obj = (KeyBinding)MemberwiseClone();
		obj.Modifiers = (bool[])Modifiers.Clone();
		return obj;
	}
}
