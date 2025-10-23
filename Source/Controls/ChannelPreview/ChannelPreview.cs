using Godot;

public class ChannelPreview : Panel
{
	public enum StyleEnum
	{
		DEFAULT,
		HOVER,
		PRESSED
	}

	private Label label;

	private TextureRect textureRect;

	[Export(PropertyHint.None, "")]
	private string text = "";

	[Export(PropertyHint.None, "")]
	private int channelId = -1;

	[Export(PropertyHint.None, "")]
	private bool selectable = true;

	[Export(PropertyHint.None, "")]
	private bool selected;

	private bool initialSelectedValue;

	[Export(PropertyHint.None, "")]
	private NodePath linkedChannelPanelNodePath;

	private ChannelPreview linkedChannelPanel;

	private Vector2 textureRectPosition = Vector2.Zero;

	private Vector2 textureRectSize = Vector2.Zero;

	[Export(PropertyHint.None, "")]
	private StyleBox defaultTheme;

	[Export(PropertyHint.None, "")]
	private Color defaultTextColor = new Color(1f, 1f, 1f);

	[Export(PropertyHint.None, "")]
	private StyleBox hoverTheme;

	[Export(PropertyHint.None, "")]
	private Color hoverTextColor = new Color(0.21f, 0.21f, 0.21f);

	[Export(PropertyHint.None, "")]
	private StyleBox selectedTheme;

	[Export(PropertyHint.None, "")]
	private Color selectedTextColor = new Color(0.21f, 0.21f, 0.21f);

	private bool isMouseOver;

	public override void _Ready()
	{
		base._Ready();
		label = GetNodeOrNull<Label>("Label");
		label.Text = text;
		textureRect = GetNodeOrNull<TextureRect>("TextureRect");
		if (linkedChannelPanelNodePath != null)
		{
			linkedChannelPanel = GetNodeOrNull<ChannelPreview>(linkedChannelPanelNodePath);
		}
		initialSelectedValue = selected;
		textureRectPosition = textureRect.RectPosition;
		textureRectPosition.x = Mathf.Round(textureRectPosition.x);
		textureRectPosition.y = Mathf.Round(textureRectPosition.y);
		textureRectSize = textureRect.RectSize;
		textureRectSize.x = Mathf.Round(textureRectSize.x);
		textureRectSize.y = Mathf.Round(textureRectSize.y);
		Connect(Signals.MouseEntered, this, "MouseEntered");
		Connect(Signals.MouseExited, this, "MouseExited");
		Connect(Signals.GuiInput, this, "Input");
	}

	public void Reset()
	{
		if (selectable)
		{
			selected = initialSelectedValue;
			if (selected)
			{
				SetStyle(StyleEnum.PRESSED);
			}
			else
			{
				SetStyle(StyleEnum.DEFAULT);
			}
		}
		textureRect.Texture = null;
		textureRect.RectPosition = textureRectPosition;
		textureRect.RectSize = textureRectSize;
	}

	public void SetStyle(StyleEnum style)
	{
		switch (style)
		{
		case StyleEnum.DEFAULT:
			Set("custom_styles/panel", defaultTheme);
			label.Modulate = defaultTextColor;
			break;
		case StyleEnum.HOVER:
			Set("custom_styles/panel", hoverTheme);
			label.Modulate = hoverTextColor;
			break;
		case StyleEnum.PRESSED:
			Set("custom_styles/panel", selectedTheme);
			label.Modulate = selectedTextColor;
			break;
		}
	}

	public void SetTexture(Texture texture, bool updateAspectRatio = false)
	{
		if (updateAspectRatio)
		{
			int width = texture.GetWidth();
			int height = texture.GetHeight();
			float ratio = 1f * (float)width / (float)height;
			if (ratio != 1f)
			{
				Vector2 newTextureRectPosition = textureRectPosition;
				Vector2 newTextureRectSize = textureRectSize;
				if (ratio < 1f)
				{
					newTextureRectSize.x *= ratio;
					newTextureRectPosition.x += (textureRectSize.x - newTextureRectSize.x) * 0.5f;
				}
				else
				{
					newTextureRectSize.y /= ratio;
					newTextureRectPosition.y += (textureRectSize.y - newTextureRectSize.y) * 0.5f;
				}
				textureRect.RectPosition = newTextureRectPosition;
				textureRect.RectSize = newTextureRectSize;
			}
			else
			{
				textureRect.RectPosition = textureRectPosition;
				textureRect.RectSize = textureRectSize;
			}
		}
		textureRect.Texture = (ImageTexture)texture.Duplicate();
	}

	public void MouseEntered()
	{
		if (selectable && !selected)
		{
			SetStyle(StyleEnum.HOVER);
		}
		isMouseOver = true;
	}

	public void MouseExited()
	{
		if (selectable)
		{
			if (selected)
			{
				SetStyle(StyleEnum.PRESSED);
			}
			else
			{
				SetStyle(StyleEnum.DEFAULT);
			}
		}
		isMouseOver = false;
	}

	public void Input(InputEvent @event)
	{
		if (selectable && @event is InputEventMouseButton { ButtonIndex: 1, Pressed: not false })
		{
			selected = !selected;
			if (linkedChannelPanel != null)
			{
				linkedChannelPanel.selected = selected;
			}
			if (selected)
			{
				SetStyle(StyleEnum.PRESSED);
				linkedChannelPanel?.SetStyle(StyleEnum.PRESSED);
			}
			else if (isMouseOver)
			{
				SetStyle(StyleEnum.HOVER);
				linkedChannelPanel?.SetStyle(StyleEnum.DEFAULT);
			}
			else
			{
				SetStyle(StyleEnum.DEFAULT);
				linkedChannelPanel?.SetStyle(StyleEnum.DEFAULT);
			}
			Register.Workspace.ChangeDrawingChannel(channelId + 1, selected);
		}
	}

	public void ChangeSelected(bool selected)
	{
		if (selectable)
		{
			this.selected = selected;
			if (this.selected)
			{
				SetStyle(StyleEnum.PRESSED);
			}
			else
			{
				SetStyle(StyleEnum.DEFAULT);
			}
		}
	}
}
