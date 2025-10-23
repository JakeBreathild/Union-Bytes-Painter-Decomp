using Godot;

public class SelectButton : TextureButton
{
	public Panel panel;

	private float panelTimer;

	private float panelDelay = 0.5f;

	private SelectionPanel selectionPanel;

	public override void _Ready()
	{
		base._Ready();
		Vector2 popupPosition = new Vector2(base.RectSize.x + 12f, -62f);
		panel = GetChildOrNull<Panel>(0);
		panel.RectPosition = popupPosition;
		panel.Connect(Signals.MouseEntered, this, "MousePanelEntered");
		panel.Connect(Signals.MouseExited, this, "MousePanelExited");
		selectionPanel = panel.GetChildOrNull<SelectionPanel>(0);
		panel.Connect(Signals.VisibilityChanged, selectionPanel, "Reset");
		Connect(Signals.MouseEntered, this, "MouseEntered");
		Connect(Signals.MouseExited, this, "MouseExited");
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (panelTimer > 0f)
		{
			panelTimer -= delta;
			if (panelTimer <= 0f)
			{
				panelTimer = 0f;
				panel.Hide();
				selectionPanel.RemoveSelection();
			}
		}
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);
		if (@event is InputEventMouseButton mb && mb.IsPressed() && mb.ButtonIndex == 2)
		{
			Register.SelectionManager.Clear();
		}
	}

	public void ResetTimer()
	{
		panelTimer = 0f;
	}

	public void MouseEntered()
	{
		InputManager.MouseEnteredUserInterface();
		InputManager.SkipInput = true;
		panelTimer = 0f;
		panel.Show();
	}

	public void MouseExited()
	{
		panelTimer = panelDelay;
		InputManager.MouseExitedUserInterface();
	}

	public void MousePanelEntered()
	{
		panelTimer = 0f;
		InputManager.MouseEnteredUserInterface();
	}

	public void MousePanelExited()
	{
		InputManager.MouseExitedUserInterface();
		InputManager.SkipInput = true;
		panelTimer = panelDelay;
	}

	public void MouseEnteredPanelChild()
	{
		panelTimer = 0f;
	}
}
