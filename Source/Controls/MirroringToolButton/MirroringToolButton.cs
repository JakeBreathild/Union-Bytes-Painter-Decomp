using Godot;

public class MirroringToolButton : TextureButton
{
	private RichTextLabel informationRichTextLabel;

	private Panel panel;

	private float panelTimer;

	private float panelDelay = 0.5f;

	private TextureButton verticalToolButton;

	private DefaultSlider verticalSlider;

	private TextureButton horizontalToolButton;

	private DefaultSlider horizontalSlider;

	private TextureButton circleToolButton;

	private DefaultSlider circleVerticalSlider;

	private DefaultSlider circleHorizontalSlider;

	private DefaultSlider circleCountSlider;

	private DefaultSlider circleOffsetSlider;

	private Button resetButton;

	public MirroringToolButton()
	{
		Register.MirroringToolButton = this;
	}

	public override void _Ready()
	{
		base._Ready();
		Vector2 popupPosition = new Vector2(4f, base.RectSize.y + 6f);
		informationRichTextLabel = GetChildOrNull<RichTextLabel>(0);
		informationRichTextLabel.RectPosition = popupPosition;
		panel = GetChildOrNull<Panel>(1);
		panel.RectPosition = popupPosition;
		panel.Connect(Signals.MouseEntered, this, "MousePanelEntered");
		panel.Connect(Signals.MouseExited, this, "MousePanelExited");
		verticalToolButton = panel.GetChildOrNull<TextureButton>(0);
		verticalToolButton.Connect(Signals.Toggled, this, "VerticalToolButtonToggled");
		verticalSlider = panel.GetChildOrNull<DefaultSlider>(1);
		verticalSlider.Connect(Signals.ValueChanged, this, "VerticalSliderValueChanged");
		horizontalToolButton = panel.GetChildOrNull<TextureButton>(3);
		horizontalToolButton.Connect(Signals.Toggled, this, "HorizontalToolButtonToggled");
		horizontalSlider = panel.GetChildOrNull<DefaultSlider>(4);
		horizontalSlider.Connect(Signals.ValueChanged, this, "HorizontalSliderValueChanged");
		circleToolButton = panel.GetChildOrNull<TextureButton>(6);
		circleToolButton.Connect(Signals.Toggled, this, "CircleToolButtonToggled");
		circleVerticalSlider = panel.GetChildOrNull<DefaultSlider>(7);
		circleVerticalSlider.Connect(Signals.ValueChanged, this, "CircleVerticalSliderValueChanged");
		circleHorizontalSlider = panel.GetChildOrNull<DefaultSlider>(8);
		circleHorizontalSlider.Connect(Signals.ValueChanged, this, "CircleHorizontalSliderValueChanged");
		circleCountSlider = panel.GetChildOrNull<DefaultSlider>(9);
		circleCountSlider.Connect(Signals.ValueChanged, this, "CircleCountSliderValueChanged");
		circleOffsetSlider = panel.GetChildOrNull<DefaultSlider>(10);
		resetButton = panel.GetChildOrNull<Button>(12);
		resetButton.Connect(Signals.Pressed, this, "ResetButtonPressed");
		Connect(Signals.MouseEntered, this, "MouseEntered");
		Connect(Signals.MouseExited, this, "MouseExited");
		Connect(Signals.Toggled, this, "Toggled");
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
			}
		}
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

	public void Reset()
	{
		informationRichTextLabel.Visible = false;
		verticalToolButton.Pressed = true;
		verticalSlider.MaxValue = Register.Workspace.Worksheet.Data.Width;
		verticalSlider.Value = Register.DrawingManager.VerticalMirrorPosition;
		horizontalToolButton.Pressed = false;
		horizontalSlider.MaxValue = Register.Workspace.Worksheet.Data.Height;
		horizontalSlider.Value = Register.DrawingManager.HorizontalMirrorPosition;
		circleToolButton.Pressed = false;
		circleVerticalSlider.MaxValue = Register.Workspace.Worksheet.Data.Width;
		circleVerticalSlider.Value = Register.DrawingManager.VerticalMirrorPosition;
		circleHorizontalSlider.MaxValue = Register.Workspace.Worksheet.Data.Height;
		circleHorizontalSlider.Value = Register.DrawingManager.HorizontalMirrorPosition;
		circleCountSlider.Value = 8.0;
		circleOffsetSlider.Value = 0.0;
	}

	public void ShowInformation()
	{
		if (Register.DrawingManager.Mirroring && Register.DrawingManager.CircleMirroring)
		{
			informationRichTextLabel.Visible = true;
		}
	}

	public void HideInformation()
	{
		informationRichTextLabel.Visible = false;
	}

	public void Toggled(bool pressed)
	{
		if (pressed)
		{
			if (Register.DrawingManager.CircleMirroring)
			{
				if (Register.DrawingManager.CurrentTool != null && !Register.DrawingManager.CurrentTool.CircleMirroring)
				{
					informationRichTextLabel.Visible = true;
				}
				else
				{
					informationRichTextLabel.Visible = false;
				}
			}
		}
		else
		{
			informationRichTextLabel.Visible = false;
		}
	}

	public void VerticalToolButtonToggled(bool pressed)
	{
		informationRichTextLabel.Visible = false;
		Register.DrawingManager.VerticalMirroring = pressed;
		Register.GridManager.UpdateMirror(Register.Workspace.Worksheet, Register.DrawingManager.Mirroring, Register.DrawingManager.VerticalMirroring, Register.DrawingManager.VerticalMirrorPosition, Register.DrawingManager.HorizontalMirroring, Register.DrawingManager.HorizontalMirrorPosition, Register.DrawingManager.CircleMirroring, Register.DrawingManager.CircleVerticalMirrorPosition, Register.DrawingManager.CircleHorizontalMirrorPosition);
		if (pressed)
		{
			Register.DrawingManager.CircleMirroring = false;
			circleToolButton.Pressed = false;
		}
	}

	public void ChangeVerticalSliderValue(float value)
	{
		verticalSlider.SetValue(value);
	}

	public void VerticalSliderValueChanged(float value)
	{
		Register.DrawingManager.VerticalMirrorPosition = verticalSlider.GetValue();
		Register.GridManager.UpdateMirror(Register.Workspace.Worksheet, Register.DrawingManager.Mirroring, Register.DrawingManager.VerticalMirroring, Register.DrawingManager.VerticalMirrorPosition, Register.DrawingManager.HorizontalMirroring, Register.DrawingManager.HorizontalMirrorPosition, Register.DrawingManager.CircleMirroring, Register.DrawingManager.CircleVerticalMirrorPosition, Register.DrawingManager.CircleHorizontalMirrorPosition);
	}

	public void HorizontalToolButtonToggled(bool pressed)
	{
		informationRichTextLabel.Visible = false;
		Register.DrawingManager.HorizontalMirroring = pressed;
		Register.GridManager.UpdateMirror(Register.Workspace.Worksheet, Register.DrawingManager.Mirroring, Register.DrawingManager.VerticalMirroring, Register.DrawingManager.VerticalMirrorPosition, Register.DrawingManager.HorizontalMirroring, Register.DrawingManager.HorizontalMirrorPosition, Register.DrawingManager.CircleMirroring, Register.DrawingManager.CircleVerticalMirrorPosition, Register.DrawingManager.CircleHorizontalMirrorPosition);
		if (pressed)
		{
			Register.DrawingManager.CircleMirroring = false;
			circleToolButton.Pressed = false;
		}
	}

	public void ChangeHorizontalSliderValue(float value)
	{
		horizontalSlider.SetValue(value);
	}

	public void HorizontalSliderValueChanged(float value)
	{
		Register.DrawingManager.HorizontalMirrorPosition = horizontalSlider.GetValue();
		Register.GridManager.UpdateMirror(Register.Workspace.Worksheet, Register.DrawingManager.Mirroring, Register.DrawingManager.VerticalMirroring, Register.DrawingManager.VerticalMirrorPosition, Register.DrawingManager.HorizontalMirroring, Register.DrawingManager.HorizontalMirrorPosition, Register.DrawingManager.CircleMirroring, Register.DrawingManager.CircleVerticalMirrorPosition, Register.DrawingManager.CircleHorizontalMirrorPosition);
	}

	public void CircleToolButtonToggled(bool pressed)
	{
		informationRichTextLabel.Visible = false;
		Register.DrawingManager.CircleMirroring = pressed;
		Register.GridManager.UpdateMirror(Register.Workspace.Worksheet, Register.DrawingManager.Mirroring, Register.DrawingManager.VerticalMirroring, Register.DrawingManager.VerticalMirrorPosition, Register.DrawingManager.HorizontalMirroring, Register.DrawingManager.HorizontalMirrorPosition, Register.DrawingManager.CircleMirroring, Register.DrawingManager.CircleVerticalMirrorPosition, Register.DrawingManager.CircleHorizontalMirrorPosition);
		if (pressed)
		{
			if (Register.DrawingManager.CurrentTool != null && !Register.DrawingManager.CurrentTool.CircleMirroring)
			{
				informationRichTextLabel.Visible = true;
			}
			Register.DrawingManager.VerticalMirroring = false;
			verticalToolButton.Pressed = false;
			Register.DrawingManager.HorizontalMirroring = false;
			horizontalToolButton.Pressed = false;
		}
	}

	public void ChangeCircleVerticalSliderValue(float value)
	{
		circleVerticalSlider.SetValue(value);
	}

	public void CircleVerticalSliderValueChanged(float value)
	{
		Register.DrawingManager.CircleVerticalMirrorPosition = circleVerticalSlider.GetValue();
		Register.GridManager.UpdateMirror(Register.Workspace.Worksheet, Register.DrawingManager.Mirroring, Register.DrawingManager.VerticalMirroring, Register.DrawingManager.VerticalMirrorPosition, Register.DrawingManager.HorizontalMirroring, Register.DrawingManager.HorizontalMirrorPosition, Register.DrawingManager.CircleMirroring, Register.DrawingManager.CircleVerticalMirrorPosition, Register.DrawingManager.CircleHorizontalMirrorPosition);
	}

	public void ChangeCircleHorizontalSliderValue(float value)
	{
		circleHorizontalSlider.SetValue(value);
	}

	public void CircleHorizontalSliderValueChanged(float value)
	{
		Register.DrawingManager.CircleHorizontalMirrorPosition = circleHorizontalSlider.GetValue();
		Register.GridManager.UpdateMirror(Register.Workspace.Worksheet, Register.DrawingManager.Mirroring, Register.DrawingManager.VerticalMirroring, Register.DrawingManager.VerticalMirrorPosition, Register.DrawingManager.HorizontalMirroring, Register.DrawingManager.HorizontalMirrorPosition, Register.DrawingManager.CircleMirroring, Register.DrawingManager.CircleVerticalMirrorPosition, Register.DrawingManager.CircleHorizontalMirrorPosition);
	}

	public void CircleCountSliderValueChanged(float value)
	{
		Register.DrawingManager.CircleMirroringCount = (int)circleCountSlider.GetValue();
	}

	public void ResetButtonPressed()
	{
		verticalToolButton.Pressed = true;
		Register.DrawingManager.VerticalMirroring = true;
		Register.DrawingManager.VerticalMirrorPosition = (float)Register.Workspace.Worksheet.Data.Width * 0.5f;
		verticalSlider.Value = Register.DrawingManager.VerticalMirrorPosition;
		horizontalToolButton.Pressed = false;
		Register.DrawingManager.HorizontalMirroring = false;
		Register.DrawingManager.HorizontalMirrorPosition = (float)Register.Workspace.Worksheet.Data.Height * 0.5f;
		horizontalSlider.Value = Register.DrawingManager.HorizontalMirrorPosition;
		circleToolButton.Pressed = false;
		circleVerticalSlider.Value = Register.DrawingManager.VerticalMirrorPosition;
		circleHorizontalSlider.Value = Register.DrawingManager.HorizontalMirrorPosition;
		circleCountSlider.Value = 8.0;
		circleOffsetSlider.Value = 0.0;
		Register.GridManager.UpdateMirror(Register.Workspace.Worksheet, Register.DrawingManager.Mirroring, Register.DrawingManager.VerticalMirroring, Register.DrawingManager.VerticalMirrorPosition, Register.DrawingManager.HorizontalMirroring, Register.DrawingManager.HorizontalMirrorPosition, Register.DrawingManager.CircleMirroring, Register.DrawingManager.CircleVerticalMirrorPosition, Register.DrawingManager.CircleHorizontalMirrorPosition);
	}
}
