using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Godot;

public static class InputManager
{
	public enum KeyBindingsListEnum
	{
		GLOBAL,
		WORKSPACE,
		PREVIEWSPACE,
		FULLSPACE,
		TOOL_BRUSH,
		TOOL_LINE,
		TOOL_RECTANGLE,
		TOOL_CIRCLE,
		TOOL_BUCKET,
		TOOL_STAMP,
		TOOL_SELECTION,
		TOOL_SMEARING,
		COUNT
	}

	private static int mouseOverUserInterfaceCount;

	private static bool isMouseOverUserInterface;

	private static bool isWindowShown;

	private static bool lockInput;

	private static float skipInputTimer;

	private static float skipInputDelay;

	private static bool skipInput;

	private static bool[] mouseButtonsWasPressed;

	private static bool[] mouseButtonsJustPressed;

	private static bool[] mouseButtonsPressed;

	private static bool[] mouseButtonsJustReleased;

	private static bool updateControls;

	private static CameraManager.SpaceEnum cursorSpace;

	private static CollisionManager.CollisionData cursorCollision;

	private static Dictionary<string, KeyBinding> keyBindingsDictionary;

	private static KeyBindingsList[] keyBindingsLists;

	private static KeyList lastKey;

	private static KeyList lastPhysicalKey;

	public static int MouseOverUserInterfaceCount => mouseOverUserInterfaceCount;

	public static bool IsMouseOverUserInterface => isMouseOverUserInterface;

	public static bool IsWindowShown => isWindowShown;

	public static bool LockInput
	{
		get
		{
			return lockInput;
		}
		set
		{
			lockInput = value;
		}
	}

	public static bool SkipInput
	{
		get
		{
			return skipInput;
		}
		set
		{
			skipInput = value;
			skipInputTimer = skipInputDelay;
		}
	}

	public static bool UpdateControls => updateControls;

	public static CameraManager.SpaceEnum CursorSpace
	{
		get
		{
			return cursorSpace;
		}
		set
		{
			cursorSpace = value;
		}
	}

	public static CollisionManager.CollisionData CursorCollision
	{
		get
		{
			return cursorCollision;
		}
		set
		{
			cursorCollision = value;
		}
	}

	public static Dictionary<string, KeyBinding> KeyBindingsDictionary => keyBindingsDictionary;

	public static KeyBindingsList[] KeyBindingsLists => keyBindingsLists;

	static InputManager()
	{
		mouseOverUserInterfaceCount = 0;
		isMouseOverUserInterface = false;
		isWindowShown = false;
		lockInput = false;
		skipInputTimer = 0f;
		skipInputDelay = 0.5f;
		skipInput = false;
		mouseButtonsWasPressed = new bool[16];
		mouseButtonsJustPressed = new bool[16];
		mouseButtonsPressed = new bool[16];
		mouseButtonsJustReleased = new bool[16];
		updateControls = true;
		cursorSpace = CameraManager.SpaceEnum.WORKSPACE;
		cursorCollision = CollisionManager.CollisionData.Default;
		keyBindingsDictionary = new Dictionary<string, KeyBinding>();
		keyBindingsLists = new KeyBindingsList[12];
		lastKey = KeyList.Unknown;
		lastPhysicalKey = KeyList.Unknown;
		for (int i = 0; i < 12; i++)
		{
			KeyBindingsList[] array = keyBindingsLists;
			int num = i;
			KeyBindingsListEnum keyBindingsListEnum = (KeyBindingsListEnum)i;
			array[num] = new KeyBindingsList(keyBindingsListEnum.ToString());
		}
	}

	public static void _Ready()
	{
	}

	public static void _Input(InputEvent @event)
	{
		if (!LockInput)
		{
			for (int i = 0; i < 12; i++)
			{
				if (keyBindingsLists[i].Enable)
				{
					CheckKeyBindingList(@event, keyBindingsLists[i].KeyBindings);
				}
			}
		}
		if (@event is InputEventKey { Pressed: not false } inputEventKey)
		{
			lastKey = (KeyList)inputEventKey.Scancode;
			lastPhysicalKey = (KeyList)inputEventKey.PhysicalScancode;
		}
	}

	public static void _Process(float delta)
	{
		for (int i = 1; i <= 9; i++)
		{
			mouseButtonsWasPressed[i] = mouseButtonsPressed[i];
			mouseButtonsPressed[i] = Input.IsMouseButtonPressed(i);
			mouseButtonsJustPressed[i] = !mouseButtonsWasPressed[i] && mouseButtonsPressed[i];
			mouseButtonsJustReleased[i] = mouseButtonsWasPressed[i] && !mouseButtonsPressed[i];
		}
		if (!LockInput && !isWindowShown)
		{
			if (SkipInput)
			{
				skipInputTimer -= delta;
				updateControls = false;
				if (skipInputTimer < 0f)
				{
					SkipInput = false;
				}
			}
			else
			{
				updateControls = !IsMouseOverUserInterface;
			}
		}
		else
		{
			updateControls = false;
		}
		if (!LockInput)
		{
			for (int j = 0; j < 12; j++)
			{
				if (keyBindingsLists[j].Enable)
				{
					CheckKeyBindingList(keyBindingsLists[j].KeyBindings);
				}
			}
		}
		DebugManager.AddLine("MouseOverGUICount: " + mouseOverUserInterfaceCount);
		DebugManager.AddLine("MouseOverGUI: " + IsMouseOverUserInterface);
		DebugManager.AddLine("WindowShown: " + isWindowShown);
		DebugManager.AddLine("LockInput: " + lockInput);
		DebugManager.AddLine("LastKey: " + lastKey);
		DebugManager.AddLine("LastPhysicalKey: " + lastPhysicalKey);
		DebugManager.AddLine("");
	}

	public static void WindowShown()
	{
		isWindowShown = true;
		lockInput = true;
	}

	public static void WindowHidden()
	{
		isWindowShown = false;
		lockInput = false;
	}

	public static void Reset()
	{
		lockInput = false;
		skipInput = false;
		isWindowShown = false;
	}

	public static void MouseEnteredUserInterface()
	{
		mouseOverUserInterfaceCount++;
		isMouseOverUserInterface = true;
	}

	public static void MouseExitedUserInterface()
	{
		mouseOverUserInterfaceCount--;
		if (mouseOverUserInterfaceCount < 1)
		{
			mouseOverUserInterfaceCount = 0;
			isMouseOverUserInterface = false;
		}
	}

	public static void ResetMouseOverUserInterfaceCounter()
	{
		mouseOverUserInterfaceCount = 0;
		isMouseOverUserInterface = false;
	}

	public static bool IsMouseButtonPressed(ButtonList button)
	{
		return mouseButtonsPressed[(int)button];
	}

	public static bool IsMouseButtonJustPressed(ButtonList button)
	{
		return mouseButtonsJustPressed[(int)button];
	}

	public static bool IsMouseButtonJustReleased(ButtonList button)
	{
		return mouseButtonsJustReleased[(int)button];
	}

	public static void EnableKeyBingingsList(KeyBindingsListEnum keyBindingsList, bool enable)
	{
		keyBindingsLists[(int)keyBindingsList].Enable = enable;
	}

	public static bool IsKeyBingingsListEnabled(KeyBindingsListEnum keyBindingsList)
	{
		return keyBindingsLists[(int)keyBindingsList].Enable;
	}

	public static KeyBinding AddKeyBinding(KeyBindingsListEnum keyBindingsList, string name, Action action, KeyList key, KeyBinding.EventTypeEnum eventType, bool shift = false, bool ctrl = false, bool alt = false)
	{
		KeyBinding keyBinding = keyBindingsLists[(int)keyBindingsList].AddKeyBinding(name, action, key, eventType, shift, ctrl, alt, (int)keyBindingsList);
		keyBindingsDictionary.Add(name, keyBinding);
		return keyBinding;
	}

	public static KeyBinding AddKeyBinding(KeyBindingsListEnum keyBindingsList, string name, Action action, ButtonList button, KeyBinding.EventTypeEnum eventType, bool shift = false, bool ctrl = false, bool alt = false)
	{
		KeyBinding keyBinding = keyBindingsLists[(int)keyBindingsList].AddKeyBinding(name, action, button, eventType, shift, ctrl, alt, (int)keyBindingsList);
		keyBindingsDictionary.Add(name, keyBinding);
		return keyBinding;
	}

	public static string GetKeyBindingKeyButton(string name)
	{
		string returnString = "";
		if (keyBindingsDictionary.TryGetValue(name, out var keyBinding))
		{
			if (keyBinding.Modifiers[0])
			{
				returnString += "[Shift]+";
			}
			if (keyBinding.Modifiers[1])
			{
				returnString += "[Ctrl]+";
			}
			if (keyBinding.Modifiers[2])
			{
				returnString += "[Alt]+";
			}
			if (keyBinding.Type == KeyBinding.TypeEnum.KEY)
			{
				string text = returnString;
				KeyList key = (KeyList)keyBinding.Key;
				returnString = text + "[" + key.ToString() + "]";
			}
			else if (keyBinding.Type == KeyBinding.TypeEnum.MOUSE)
			{
				switch ((ButtonList)keyBinding.Button)
				{
				case ButtonList.Left:
					returnString += "[LMB]";
					break;
				case ButtonList.Middle:
					returnString += "[MMB]";
					break;
				case ButtonList.Right:
					returnString += "[RMB]";
					break;
				case ButtonList.WheelUp:
					returnString += "[MWU]";
					break;
				case ButtonList.WheelDown:
					returnString += "[MWD]";
					break;
				default:
				{
					string text2 = returnString;
					ButtonList button = (ButtonList)keyBinding.Button;
					returnString = text2 + "[" + button.ToString() + "]";
					break;
				}
				}
			}
		}
		return returnString;
	}

	public static void CheckKeyBindingList(List<KeyBinding> keyBindingList)
	{
		bool shift = Input.IsKeyPressed(16777237);
		bool ctrl = Input.IsKeyPressed(16777238);
		bool alt = Input.IsKeyPressed(16777240);
		foreach (KeyBinding keyBinding in keyBindingList)
		{
			if (keyBinding.EventType != KeyBinding.EventTypeEnum.PRESSED)
			{
				continue;
			}
			if (keyBinding.Type == KeyBinding.TypeEnum.KEY)
			{
				if (Input.IsKeyPressed(keyBinding.Key) && shift == keyBinding.Modifiers[0] && ctrl == keyBinding.Modifiers[1] && alt == keyBinding.Modifiers[2])
				{
					keyBinding.Action();
					break;
				}
			}
			else if (keyBinding.Type == KeyBinding.TypeEnum.MOUSE && mouseButtonsPressed[keyBinding.Button] && shift == keyBinding.Modifiers[0] && ctrl == keyBinding.Modifiers[1] && alt == keyBinding.Modifiers[2])
			{
				keyBinding.Action();
				break;
			}
		}
	}

	public static void CheckKeyBindingList(InputEvent @event, List<KeyBinding> keyBindingList)
	{
		bool shift = Input.IsKeyPressed(16777237);
		bool ctrl = Input.IsKeyPressed(16777238);
		bool alt = Input.IsKeyPressed(16777240);
		bool doForEachBreak = false;
		foreach (KeyBinding keyBinding in keyBindingList)
		{
			if (keyBinding.Type == KeyBinding.TypeEnum.KEY && @event is InputEventKey eventKey)
			{
				switch (keyBinding.EventType)
				{
				case KeyBinding.EventTypeEnum.JUST_PRESSED:
					if (eventKey.Scancode == keyBinding.Key && eventKey.Pressed && !eventKey.Echo && shift == keyBinding.Modifiers[0] && ctrl == keyBinding.Modifiers[1] && alt == keyBinding.Modifiers[2])
					{
						keyBinding.Action();
						doForEachBreak = true;
					}
					break;
				case KeyBinding.EventTypeEnum.JUST_RELEASED:
					if (eventKey.Scancode == keyBinding.Key && !eventKey.Pressed && shift == keyBinding.Modifiers[0] && ctrl == keyBinding.Modifiers[1] && alt == keyBinding.Modifiers[2])
					{
						keyBinding.Action();
						doForEachBreak = true;
					}
					break;
				}
			}
			else if (keyBinding.Type == KeyBinding.TypeEnum.MOUSE && @event is InputEventMouseButton mouseButtonKey)
			{
				switch (keyBinding.EventType)
				{
				case KeyBinding.EventTypeEnum.JUST_PRESSED:
					if (mouseButtonKey.ButtonIndex == keyBinding.Button && mouseButtonKey.Pressed && shift == keyBinding.Modifiers[0] && ctrl == keyBinding.Modifiers[1] && alt == keyBinding.Modifiers[2])
					{
						keyBinding.Action();
						doForEachBreak = true;
					}
					break;
				case KeyBinding.EventTypeEnum.JUST_RELEASED:
					if (mouseButtonKey.ButtonIndex == keyBinding.Button && !mouseButtonKey.Pressed && shift == keyBinding.Modifiers[0] && ctrl == keyBinding.Modifiers[1] && alt == keyBinding.Modifiers[2])
					{
						keyBinding.Action();
						doForEachBreak = true;
					}
					break;
				}
			}
			if (doForEachBreak)
			{
				break;
			}
		}
	}

	public static void SaveKeyBindings(XElement root)
	{
		XElement keyBindingsXElement = new XElement("KeyBindings");
		for (int i = 0; i < 12; i++)
		{
			SaveKeyBindingsList(keyBindingsXElement, keyBindingsLists[i].Name, keyBindingsLists[i].KeyBindings);
		}
		root.Add(keyBindingsXElement);
	}

	private static void SaveKeyBindingsList(XElement nodeToAdd, string name, List<KeyBinding> keyBindingList)
	{
		XElement keyBindings = new XElement(name);
		foreach (KeyBinding keyBinding in keyBindingList)
		{
			XElement keyBindingXElement = new XElement(keyBinding.Name);
			keyBindingXElement.SetAttributeValue("Type", keyBinding.Type);
			keyBindingXElement.SetAttributeValue("Key", keyBinding.Key);
			keyBindingXElement.SetAttributeValue("Button", keyBinding.Button);
			keyBindingXElement.SetAttributeValue("EventType", keyBinding.EventType);
			keyBindingXElement.SetAttributeValue("Shift", keyBinding.Modifiers[0]);
			keyBindingXElement.SetAttributeValue("Ctrl", keyBinding.Modifiers[1]);
			keyBindingXElement.SetAttributeValue("Alt", keyBinding.Modifiers[2]);
			keyBindings.Add(keyBindingXElement);
		}
		nodeToAdd.Add(keyBindings);
	}

	public static void LoadKeyBindings(XElement root)
	{
		XElement keyBindings = root.Element("KeyBindings");
		if (keyBindings == null)
		{
			return;
		}
		foreach (XElement item in keyBindings.Elements())
		{
			LoadKeyBindingsList(item);
		}
	}

	private static void LoadKeyBindingsList(XElement keyBindingListNode)
	{
		List<KeyBinding> keyBindingsList = null;
		for (int i = 0; i < 12; i++)
		{
			if (keyBindingListNode.Name.LocalName == keyBindingsLists[i].Name)
			{
				keyBindingsList = keyBindingsLists[i].KeyBindings;
			}
		}
		if (keyBindingsList == null)
		{
			return;
		}
		foreach (XElement xElement in keyBindingListNode.Elements())
		{
			string keyBindingName = xElement.Name.LocalName;
			for (int j = 0; j < keyBindingsList.Count; j++)
			{
				if (!(keyBindingsList[j].Name == keyBindingName))
				{
					continue;
				}
				string value = xElement.Attribute("Type").Value;
				if (!(value == "KEY"))
				{
					if (value == "MOUSE")
					{
						keyBindingsList[j].Type = KeyBinding.TypeEnum.MOUSE;
					}
				}
				else
				{
					keyBindingsList[j].Type = KeyBinding.TypeEnum.KEY;
				}
				keyBindingsList[j].Key = int.Parse(xElement.Attribute("Key").Value);
				keyBindingsList[j].Button = int.Parse(xElement.Attribute("Button").Value);
				switch (xElement.Attribute("EventType").Value)
				{
				case "JUST_PRESSED":
					keyBindingsList[j].EventType = KeyBinding.EventTypeEnum.JUST_PRESSED;
					break;
				case "PRESSED":
					keyBindingsList[j].EventType = KeyBinding.EventTypeEnum.PRESSED;
					break;
				case "JUST_RELEASED":
					keyBindingsList[j].EventType = KeyBinding.EventTypeEnum.JUST_RELEASED;
					break;
				}
				keyBindingsList[j].Modifiers[0] = bool.Parse(xElement.Attribute("Shift").Value);
				keyBindingsList[j].Modifiers[1] = bool.Parse(xElement.Attribute("Ctrl").Value);
				keyBindingsList[j].Modifiers[2] = bool.Parse(xElement.Attribute("Alt").Value);
				break;
			}
		}
	}
}
