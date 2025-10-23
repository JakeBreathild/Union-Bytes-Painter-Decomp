using System;
using Godot;

public class GroupPanel : Panel
{
	private Label nameLabel;

	private TextureButton hideButton;

	private TextureButton resetButton;

	[Export(PropertyHint.None, "")]
	private bool enableHideButton = true;

	[Export(PropertyHint.None, "")]
	private bool enableResetButton = true;

	[Export(PropertyHint.None, "")]
	private string name = "";

	[Export(PropertyHint.None, "")]
	private bool hide;

	private float ySize;

	private float minYSize;

	private Action resetButtonPressedCallback;

	public Label NameLabel => nameLabel;

	public TextureButton HideButton => hideButton;

	public TextureButton ResetButton => resetButton;

	public Action ResetButtonPressedCallback
	{
		set
		{
			resetButtonPressedCallback = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		minYSize = base.RectMinSize.y;
		ySize = base.RectSize.y;
		nameLabel = GetNodeOrNull<Label>("Name");
		nameLabel.Text = Tr(name);
		hideButton = GetNodeOrNull<TextureButton>("Hide");
		if (!enableHideButton)
		{
			hideButton.Visible = false;
		}
		hideButton.Pressed = hide;
		HideButtonToggled(hideButton.Pressed);
		hideButton.Connect(Signals.Toggled, this, "HideButtonToggled");
		resetButton = GetNodeOrNull<TextureButton>("Reset");
		if (!enableResetButton)
		{
			resetButton.Visible = false;
		}
		resetButton.Connect(Signals.Pressed, this, "ResetButtonPressed");
	}

	public void Reset()
	{
		hideButton.Pressed = hide;
		HideButtonToggled(hideButton.Pressed);
	}

	public void HideButtonToggled(bool pressed)
	{
		if (pressed)
		{
			base.RectMinSize = new Vector2(base.RectMinSize.x, 36f);
			base.RectSize = new Vector2(base.RectSize.x, 36f);
			for (int i = 4; i < GetChildCount(); i++)
			{
				GetChild<Control>(i).Visible = false;
			}
		}
		else
		{
			base.RectMinSize = new Vector2(base.RectMinSize.x, minYSize);
			base.RectSize = new Vector2(base.RectSize.x, ySize);
			for (int j = 4; j < GetChildCount(); j++)
			{
				GetChild<Control>(j).Visible = true;
			}
		}
	}

	public void ResetButtonPressed()
	{
		if (resetButtonPressedCallback != null)
		{
			resetButtonPressedCallback();
		}
	}
}
