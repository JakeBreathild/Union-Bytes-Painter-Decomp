using Godot;

public class CameraManager : Node
{
	public enum SpaceEnum
	{
		WORKSPACE,
		PREVIEWSPACE
	}

	public enum IntersectRayModeEnum
	{
		NONE,
		FREE,
		LINE
	}

	private Workspace workspace;

	private PreviewspaceViewport previewspaceViewport;

	private CollisionManager collisionManager;

	private IntersectRayModeEnum intersectRayMode;

	private Vector2 mousePositionLastFrame = Vector2.Zero;

	private Vector3 startRayOrigin = Vector3.Zero;

	private Vector3 startRayNormal = Vector3.Zero;

	private ToolCamera workspaceToolCamera;

	private float workspaceCameraDefaultDistance = 40f;

	private Camera workspaceCamera;

	private Viewport workspaceViewport;

	private ToolCamera previewspaceToolCamera;

	private float previewspaceCameraDefaultDistance = 38f;

	private Camera previewspaceCamera;

	public IntersectRayModeEnum IntersectRayMode
	{
		get
		{
			return intersectRayMode;
		}
		set
		{
			intersectRayMode = value;
		}
	}

	public ToolCamera WorkspaceToolCamera => workspaceToolCamera;

	public Camera WorkspaceCamera => workspaceCamera;

	public Viewport WorkspaceViewport => workspaceViewport;

	public ToolCamera PreviewspaceToolCamera => previewspaceToolCamera;

	public Camera PreviewspaceCamera => previewspaceCamera;

	public CameraManager()
	{
		Register.CameraManager = this;
	}

	public CameraManager(Workspace workspace)
	{
		Register.CameraManager = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "CameraManager";
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		previewspaceViewport = Register.PreviewspaceViewport;
		collisionManager = new CollisionManager(workspace);
		workspaceToolCamera = Resources.ToolCameraScene.Instance<ToolCamera>();
		workspaceToolCamera.Orthogonal = true;
		workspaceToolCamera.Current = true;
		workspaceToolCamera.LockAngle = true;
		workspaceToolCamera.LockYAxis = true;
		workspaceToolCamera.Distance = workspaceCameraDefaultDistance;
		workspaceToolCamera.MaxDistance = 256f;
		AddChild(workspaceToolCamera);
		workspaceToolCamera.Owner = this;
		workspaceToolCamera.Name = "WorkspaceCamera";
		workspaceCamera = workspaceToolCamera.Camera;
		workspaceViewport = workspaceToolCamera.Viewport;
		previewspaceToolCamera = Resources.ToolCameraScene.Instance<ToolCamera>();
		previewspaceToolCamera.Current = true;
		previewspaceToolCamera.LockAngle = false;
		previewspaceToolCamera.LockYAxis = false;
		previewspaceToolCamera.Distance = previewspaceCameraDefaultDistance;
		previewspaceToolCamera.Angle = 50f;
		previewspaceToolCamera.MinAngle = -85f;
		previewspaceToolCamera.MaxDistance = 512f;
		previewspaceToolCamera.OnlyLevel20 = true;
		previewspaceViewport.AddChild(previewspaceToolCamera);
		previewspaceToolCamera.Owner = previewspaceViewport;
		previewspaceToolCamera.Name = "PreviewspaceCamera";
		previewspaceCamera = previewspaceToolCamera.Camera;
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);
		collisionManager._PhysicsProcess(delta);
		InputManager.CursorSpace = SpaceEnum.WORKSPACE;
		InputManager.CursorCollision = CollisionManager.CollisionData.Default;
		workspaceToolCamera.ZoomStep = 2.5f + 18f * (workspaceToolCamera.Distance / workspaceToolCamera.MaxDistance);
		UpdateWorkspaceControls();
		previewspaceToolCamera.ZoomStep = 0.8f + 24.2f * (previewspaceToolCamera.Distance / previewspaceToolCamera.MaxDistance);
		UpdatePreviewspaceControls();
	}

	public void Reset()
	{
		ResetWorkspaceCamera();
		ResetPreviewspaceCamera();
	}

	private void UpdateWorkspaceControls()
	{
		if (workspaceToolCamera.UpdateControls)
		{
			InputManager.CursorSpace = SpaceEnum.WORKSPACE;
			Vector2 mousePosition = workspaceViewport.GetMousePosition();
			Vector3 rayOrigin = workspaceCamera.ProjectRayOrigin(mousePosition);
			Vector3 rayNormal = workspaceCamera.ProjectRayNormal(mousePosition);
			collisionManager.RaysCount = 0;
			collisionManager.PlaneIntersectRay(rayOrigin, rayNormal);
			CollisionManager.CollisionData collision = collisionManager.Collision;
			collision.Object = workspace.Worksheet;
			collision.UV.x = collision.Position.x / 0.25f / (float)workspace.Worksheet.Data.Width;
			collision.UV.y = collision.Position.z / 0.25f / (float)workspace.Worksheet.Data.Height;
			InputManager.CursorCollision = collision;
		}
	}

	public void ResetWorkspaceCamera()
	{
		Transform transform = Transform.Identity;
		if (workspace.Worksheet != null)
		{
			transform.origin.x = (float)workspace.Worksheet.Data.Width * 0.25f * 0.5f;
			transform.origin.z = (float)workspace.Worksheet.Data.Height * 0.25f * 0.5f - 2.5f;
		}
		else
		{
			transform.origin.x = 0f;
			transform.origin.z = 0f;
		}
		workspaceToolCamera.TargetDistance = workspaceCameraDefaultDistance;
		workspaceToolCamera.GlobalTransform = transform;
	}

	public bool IsWorkspaceCameraControlsEnable()
	{
		return workspaceToolCamera.UpdateControls;
	}

	public void EnableWorkspaceCameraControls(bool enable)
	{
		workspaceToolCamera.UpdateControls = enable;
	}

	public float GetWorkspaceCameraHeight()
	{
		return workspaceCamera.GlobalTransform.origin.y;
	}

	private void UpdatePreviewspaceControls()
	{
		if (!previewspaceToolCamera.UpdateControls)
		{
			return;
		}
		InputManager.CursorSpace = SpaceEnum.PREVIEWSPACE;
		if (previewspaceCamera == null)
		{
			previewspaceCamera = previewspaceToolCamera.Camera;
		}
		Vector2 mousePosition = previewspaceViewport.GetMousePosition();
		Vector3 rayOrigin = previewspaceCamera.ProjectRayOrigin(mousePosition);
		Vector3 rayNormal = previewspaceCamera.ProjectRayNormal(mousePosition);
		if (InputManager.IsMouseButtonJustPressed(ButtonList.Left))
		{
			startRayOrigin = rayOrigin;
			startRayNormal = rayNormal;
			mousePositionLastFrame = mousePosition;
		}
		if (InputManager.IsMouseButtonPressed(ButtonList.Left))
		{
			collisionManager.RaysCount = 0;
			switch (intersectRayMode)
			{
			case IntersectRayModeEnum.FREE:
				if (mousePosition != mousePositionLastFrame)
				{
					collisionManager.MeshLineIntersectRays(startRayOrigin, startRayNormal, rayNormal, clearList: false);
					startRayOrigin = rayOrigin;
					startRayNormal = rayNormal;
				}
				mousePositionLastFrame = mousePosition;
				break;
			case IntersectRayModeEnum.LINE:
				collisionManager.MeshLineIntersectRays(startRayOrigin, startRayNormal, rayNormal);
				break;
			default:
				if (collisionManager.CollisionsList.Count > 0)
				{
					collisionManager.CollisionsList.Clear();
				}
				break;
			}
		}
		collisionManager.MeshIntersectRay(rayOrigin, rayNormal);
		InputManager.CursorCollision = collisionManager.Collision;
	}

	public void ResetPreviewspaceCamera()
	{
		previewspaceToolCamera.Camera.GlobalTransform = Transform.Identity;
		Transform transform = Transform.Identity;
		transform.origin.x = 0f;
		transform.origin.y = 0f;
		transform.origin.z = 0f;
		float distanceFactor = 1f;
		if (workspace.Worksheet != null)
		{
			distanceFactor = 1.5f * Register.PreviewspaceMeshManager.GetMeshScale() * Register.PreviewspaceMeshManager.AABB.Size.Length() / previewspaceCameraDefaultDistance * 1.1f;
		}
		previewspaceToolCamera.TargetDistance = previewspaceCameraDefaultDistance * distanceFactor;
		previewspaceToolCamera.Distance = previewspaceToolCamera.TargetDistance;
		previewspaceToolCamera.Angle = 45f;
		previewspaceToolCamera.GlobalTransform = transform;
		previewspaceToolCamera.RotateY(Mathf.Deg2Rad(-30f));
	}

	public void SetPreviewspaceCamera(float angle, float rotation)
	{
		previewspaceToolCamera.Camera.GlobalTransform = Transform.Identity;
		Transform transform = Transform.Identity;
		transform.origin.x = 0f;
		transform.origin.y = 0f;
		transform.origin.z = 0f;
		float distanceFactor = 1f;
		if (workspace.Worksheet != null)
		{
			distanceFactor = 1.5f * Register.PreviewspaceMeshManager.GetMeshScale() * Register.PreviewspaceMeshManager.AABB.Size.Length() / previewspaceCameraDefaultDistance * 1.1f;
		}
		previewspaceToolCamera.TargetDistance = previewspaceCameraDefaultDistance * distanceFactor;
		previewspaceToolCamera.Distance = previewspaceToolCamera.TargetDistance;
		previewspaceToolCamera.Angle = angle;
		previewspaceToolCamera.GlobalTransform = transform;
		previewspaceToolCamera.RotateY(Mathf.Deg2Rad(rotation));
	}

	public bool IsPreviewCameraControlsEnable()
	{
		return previewspaceToolCamera.UpdateControls;
	}

	public void EnablePreviewCameraControls(bool enable)
	{
		previewspaceToolCamera.UpdateControls = enable;
	}
}
