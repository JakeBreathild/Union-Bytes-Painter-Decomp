using Godot;

public class WorkspaceViewportContainer : ViewportContainer
{
	private Workspace workspace;

	private Viewport workspaceViewport;

	private bool mouseEntered;

	private float timer;

	private float delay = 1f;

	public Viewport WorkspaceViewport => workspaceViewport;

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		workspaceViewport = GetChildOrNull<Viewport>(0);
		Connect(Signals.MouseEntered, this, "MouseEntered");
		Connect(Signals.MouseExited, this, "MouseExited");
		Connect(Signals.Resized, this, "Resized");
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (!mouseEntered || !(timer > 0f))
		{
			return;
		}
		timer -= delta;
		if (timer <= 0f)
		{
			if (!InputManager.IsWindowShown)
			{
				InputManager.ResetMouseOverUserInterfaceCounter();
			}
			timer = 0f;
			mouseEntered = false;
		}
	}

	public void Reset()
	{
	}

	public void Resized()
	{
	}

	public void MouseEntered()
	{
		mouseEntered = true;
		timer = delay;
	}

	public void MouseExited()
	{
		mouseEntered = false;
	}
}
