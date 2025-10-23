using Godot;

public class SelectionEntry : Panel
{
	private Workspace workspace;

	private ColorRect colorRect;

	private Label nameLabel;

	private TextureButton editButton;

	private TextureButton setButton;

	public SelectionPanel SelectionPanel { get; set; }

	public Data.Selection Selection { get; set; }

	public Color Color
	{
		get
		{
			return colorRect.Color;
		}
		set
		{
			colorRect.Color = value;
		}
	}

	public new string Name
	{
		get
		{
			return nameLabel.Text;
		}
		set
		{
			nameLabel.Text = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		colorRect = GetChildOrNull<ColorRect>(0);
		nameLabel = GetChildOrNull<Label>(1);
		editButton = GetChildOrNull<TextureButton>(2);
		editButton.Connect(Signals.Pressed, this, "EditPressed");
		setButton = GetChildOrNull<TextureButton>(4);
		setButton.Connect(Signals.Pressed, this, "SetPressed");
		Connect(Signals.GuiInput, this, "GuiInput");
	}

	public void UpdateSelection()
	{
		colorRect.Color = Selection.Color;
		nameLabel.Text = Selection.Name;
	}

	public void GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton { ButtonIndex: 1, Pressed: not false } && GetLocalMousePosition().x < base.RectSize.x - 74f)
		{
			SelectionPanel.Select(Selection);
		}
	}

	public void EditPressed()
	{
		SelectionPanel.SelectionWindowDialog.selectionEntry = this;
		SelectionPanel.SelectionWindowDialog.PopupCentered();
		SelectionPanel.SelectionWindowDialog.RectGlobalPosition = SelectionPanel.RectGlobalPosition + new Vector2(SelectionPanel.RectSize.x + 8f, -128f);
	}

	public void SetPressed()
	{
		if (!Register.SelectionManager.IsSelecting && Register.SelectionManager.Enabled)
		{
			Selection.Array = (bool[,])workspace.Worksheet.Data.SelectionArray.Clone();
		}
	}
}
