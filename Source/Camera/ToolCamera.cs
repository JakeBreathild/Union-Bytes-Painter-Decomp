using Godot;

public class ToolCamera : Spatial
{
	private float targetDistance;

	public bool UpdateControls;

	private Vector2 mouseDelta = Vector2.Zero;

	private bool rotate;

	private bool pan;

	private bool zoomIn;

	private bool zoomOut;

	public Camera Camera { get; private set; }

	public Viewport Viewport { get; private set; }

	[Export(PropertyHint.None, "")]
	public bool Current { get; set; }

	[Export(PropertyHint.None, "")]
	public float Speed { get; set; } = 10f;

	[Export(PropertyHint.None, "")]
	public float Angle { get; set; } = 90f;

	[Export(PropertyHint.None, "")]
	public float MinAngle { get; set; } = 15f;

	[Export(PropertyHint.None, "")]
	public float MaxAngle { get; set; } = 85f;

	[Export(PropertyHint.None, "")]
	public bool LockAngle { get; set; }

	[Export(PropertyHint.None, "")]
	public bool LockYAxis { get; set; } = true;

	[Export(PropertyHint.None, "")]
	public float Distance { get; set; } = 10f;

	[Export(PropertyHint.None, "")]
	public float MinDistancen { get; set; } = 2f;

	[Export(PropertyHint.None, "")]
	public float MaxDistance { get; set; } = 512f;

	[Export(PropertyHint.None, "")]
	public float ZoomStep { get; set; } = 5f;

	[Export(PropertyHint.None, "")]
	public float Sensitivity { get; set; } = 0.02f;

	[Export(PropertyHint.None, "")]
	public bool OnlyLevel20 { get; set; }

	[Export(PropertyHint.None, "")]
	public bool Orthogonal { get; set; }

	public float TargetDistance
	{
		get
		{
			return targetDistance;
		}
		set
		{
			targetDistance = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		Camera = GetChild<Camera>(0);
		Camera.Current = Current;
		Viewport = Camera.GetViewport();
		if (Orthogonal)
		{
			Camera.Projection = Camera.ProjectionEnum.Orthogonal;
		}
		mouseDelta = Vector2.Zero;
		targetDistance = Distance;
		if (OnlyLevel20)
		{
			for (int i = 0; i < 19; i++)
			{
				Camera.SetCullMaskBit(i, enable: false);
			}
			Camera.SetCullMaskBit(19, enable: true);
		}
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (UpdateControls && @event is InputEventMouseMotion e)
		{
			mouseDelta = e.Relative;
		}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (Camera != null)
		{
			Basis cameraDirection = Camera.GlobalTransform.basis;
			Vector3 move = Vector3.Zero;
			if (UpdateControls && zoomIn)
			{
				targetDistance -= ZoomStep;
				zoomIn = false;
			}
			else if (UpdateControls && zoomOut)
			{
				targetDistance += ZoomStep;
				zoomOut = false;
			}
			if (UpdateControls && pan)
			{
				move -= cameraDirection.x * (mouseDelta.x * Sensitivity * Distance * 0.5f);
				move += cameraDirection.y * (mouseDelta.y * Sensitivity * Distance * 0.5f);
				pan = false;
			}
			if (LockYAxis)
			{
				move.y = 0f;
			}
			Transform transform = base.GlobalTransform;
			transform.origin += move * Speed * delta;
			base.GlobalTransform = transform;
			if (LockAngle)
			{
				Angle = MaxAngle;
			}
			else if (UpdateControls && rotate)
			{
				Angle += mouseDelta.y * 0.25f;
				Angle = Mathf.Clamp(Angle, MinAngle, MaxAngle);
				RotateY((0f - mouseDelta.x) * 0.01f);
				rotate = false;
			}
			mouseDelta = Vector2.Zero;
			targetDistance = Mathf.Clamp(targetDistance, MinDistancen, MaxDistance);
			Distance += (targetDistance - Distance) * delta * 4f;
			transform = Camera.Transform;
			transform.origin.y = Mathf.Sin(Mathf.Deg2Rad(Angle)) * Distance;
			transform.origin.z = Mathf.Cos(Mathf.Deg2Rad(Angle)) * Distance;
			Camera.Transform = transform;
			if (!LockAngle)
			{
				transform = Camera.GlobalTransform;
				Camera.GlobalTransform = transform.LookingAt(base.GlobalTransform.origin, Vector3.Up);
			}
			if (Orthogonal)
			{
				Camera.Size = Distance;
			}
		}
	}

	public void Rotate()
	{
		rotate = true;
	}

	public void Pan()
	{
		pan = true;
	}

	public void ZoomIn()
	{
		zoomIn = true;
	}

	public void ZoomOut()
	{
		zoomOut = true;
	}
}
