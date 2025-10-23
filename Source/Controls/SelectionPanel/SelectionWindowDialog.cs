using Godot;

public class SelectionWindowDialog : WindowDialog
{
	public SelectionEntry selectionEntry;

	private LineEdit lineEdit;

	private ColorPicker colorPicker;

	private Button okayButton;

	private Button cancelButton;

	public override void _Ready()
	{
		base._Ready();
		lineEdit = GetNodeOrNull<LineEdit>("LineEdit");
		lineEdit.Connect(Signals.TextEntered, this, "LineEditTextEntered");
		colorPicker = GetNodeOrNull<ColorPicker>("ColorPicker");
		okayButton = GetNodeOrNull<Button>("Okay");
		okayButton.Connect(Signals.Pressed, this, "OkayButtonPressed");
		cancelButton = GetNodeOrNull<Button>("Cancel");
		cancelButton.Connect(Signals.Pressed, this, "CancelButtonPressed");
		Connect(Signals.Hide, this, "Hide");
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (base.Visible && selectionEntry != null)
		{
			selectionEntry.SelectionPanel.SelectButton.ResetTimer();
		}
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
		if (selectionEntry != null)
		{
			lineEdit.Text = selectionEntry.Name;
			colorPicker.Color = selectionEntry.Color;
		}
	}

	public void LineEditTextEntered(string text)
	{
		lineEdit.ReleaseFocus();
	}

	public void OkayButtonPressed()
	{
		if (selectionEntry != null)
		{
			if (lineEdit.Text != "")
			{
				selectionEntry.Selection.Name = lineEdit.Text;
			}
			Color color = colorPicker.Color;
			color.a = 1f;
			selectionEntry.Selection.Color = color;
			selectionEntry.UpdateSelection();
		}
		Hide();
	}

	public void CancelButtonPressed()
	{
		Hide();
	}
}
