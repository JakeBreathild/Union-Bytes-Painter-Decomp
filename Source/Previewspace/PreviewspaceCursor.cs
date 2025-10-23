using Godot;

public class PreviewspaceCursor : Spatial
{
	public enum CursorTypeEnum
	{
		CIRCLE,
		CROSS
	}

	private ImmediateGeometry immediateGeometry;

	private float size = 0.2f;

	private int points = 64;

	public float SphereSize = 5f;

	public bool ForceVisible;

	public Vector3 ForcePosition;

	public bool ForceHide;

	public PreviewspaceCursor()
	{
		Register.PreviewspaceCursor = this;
	}

	public override void _Ready()
	{
		base._Ready();
		immediateGeometry = new ImmediateGeometry();
		AddChild(immediateGeometry);
		immediateGeometry.Owner = this;
		immediateGeometry.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.GRID);
		immediateGeometry.SetLayerMaskBit(0, enabled: false);
		immediateGeometry.SetLayerMaskBit(19, enabled: true);
		GenerateCursorGeometry(CursorTypeEnum.CIRCLE);
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);
		if (ForceHide)
		{
			base.Visible = false;
		}
		else if (!ForceVisible && InputManager.CursorSpace == CameraManager.SpaceEnum.PREVIEWSPACE && InputManager.CursorCollision.CollisionDetected)
		{
			Vector3 normal = InputManager.CursorCollision.Normal;
			Transform transform = Transform.Identity;
			transform = ((!(normal != Vector3.Up) || !(normal != Vector3.Down)) ? transform.LookingAt(normal, Vector3.Left) : transform.LookingAt(normal, Vector3.Up));
			transform.origin = InputManager.CursorCollision.Position + normal * Settings.CursorOffset;
			base.GlobalTransform = transform;
			base.Visible = true;
		}
		else if (ForceVisible)
		{
			if (InputManager.CursorSpace == CameraManager.SpaceEnum.PREVIEWSPACE && Input.IsKeyPressed(16777238) && InputManager.IsMouseButtonJustPressed(ButtonList.Left) && InputManager.CursorCollision.CollisionDetected)
			{
				ForcePosition = InputManager.CursorCollision.Position + InputManager.CursorCollision.Normal * 1f;
				Register.Gui.BakeMenuButton.BakeWindowDialog.UpdateLightPosition();
			}
			Transform transform2 = Transform.Identity;
			transform2.origin = ForcePosition;
			base.GlobalTransform = transform2;
			base.Visible = true;
		}
		else
		{
			base.Visible = false;
		}
	}

	public void AddLine(Vector3 start, Vector3 end, float thickness)
	{
		Vector3 direction = end - start;
		Vector3 camera = new Vector3(0f, 0f, -1f);
		Vector3 normal = direction.Cross(camera).Normalized();
		Vector3[] vertex = new Vector3[4]
		{
			start + normal * thickness * 0.5f,
			start - normal * thickness * 0.5f,
			end + normal * thickness * 0.5f,
			end - normal * thickness * 0.5f
		};
		immediateGeometry.AddVertex(vertex[0]);
		immediateGeometry.AddVertex(vertex[1]);
		immediateGeometry.AddVertex(vertex[2]);
		immediateGeometry.AddVertex(vertex[2]);
		immediateGeometry.AddVertex(vertex[1]);
		immediateGeometry.AddVertex(vertex[3]);
	}

	public void AddLineBox(Vector3 start, Vector3 end, float thickness)
	{
		Vector3 originalStart = start;
		Vector3 originalEnd = end;
		float height = (originalStart.y + originalEnd.y) * 0.5f;
		start = new Vector3(originalStart.x, height, originalStart.z);
		end = new Vector3(originalEnd.x, height, originalStart.z);
		AddLine(start, end, thickness);
		start = new Vector3(originalEnd.x, height, originalStart.z);
		end = new Vector3(originalEnd.x, height, originalEnd.z);
		AddLine(start, end, thickness);
		start = new Vector3(originalEnd.x, height, originalEnd.z);
		end = new Vector3(originalStart.x, height, originalEnd.z);
		AddLine(start, end, thickness);
		start = new Vector3(originalStart.x, height, originalEnd.z);
		end = new Vector3(originalStart.x, height, originalStart.z);
		AddLine(start, end, thickness);
	}

	public void AddBox(Vector3 start, Vector3 end)
	{
		float height = (start.y + end.y) * 0.5f;
		Vector3 leftBottom = new Vector3(start.x, height, end.z);
		Vector3 rightTop = new Vector3(end.x, height, start.z);
		immediateGeometry.AddVertex(start);
		immediateGeometry.AddVertex(rightTop);
		immediateGeometry.AddVertex(leftBottom);
		immediateGeometry.AddVertex(leftBottom);
		immediateGeometry.AddVertex(rightTop);
		immediateGeometry.AddVertex(end);
	}

	public void AddBox(Vector3 position, float size)
	{
		Vector3 sizeVector = new Vector3(size, size, size);
		AddBox(position - sizeVector, position + sizeVector);
	}

	public void GenerateCursorGeometry(CursorTypeEnum cursorType)
	{
		immediateGeometry.Clear();
		switch (cursorType)
		{
		case CursorTypeEnum.CIRCLE:
		{
			size = 0.12f;
			points = 64;
			immediateGeometry.Begin(Mesh.PrimitiveType.Triangles);
			immediateGeometry.SetColor(Settings.AccentColor);
			for (int l = 0; l < points; l++)
			{
				float anglePointStart = Mathf.Deg2Rad((float)l * 360f / (float)points - 90f);
				float anglePointEnd = Mathf.Deg2Rad((float)(l + 1) * 360f / (float)points - 90f);
				AddLine(new Vector3(Mathf.Cos(anglePointStart), Mathf.Sin(anglePointStart), 0f) * size, new Vector3(Mathf.Cos(anglePointEnd), Mathf.Sin(anglePointEnd), 0f) * size, 0.015f);
			}
			immediateGeometry.End();
			break;
		}
		case CursorTypeEnum.CROSS:
		{
			size = 0.5f;
			points = 32;
			immediateGeometry.Begin(Mesh.PrimitiveType.Lines);
			immediateGeometry.SetColor(new Color(1f, 0f, 0f));
			immediateGeometry.AddVertex(new Vector3(0f - size, 0f, 0f));
			immediateGeometry.AddVertex(new Vector3(size, 0f, 0f));
			immediateGeometry.AddVertex(new Vector3(0f - size, 0f, 0.002f));
			immediateGeometry.AddVertex(new Vector3(size, 0f, 0.002f));
			immediateGeometry.SetColor(new Color(0f, 1f, 0f));
			immediateGeometry.AddVertex(new Vector3(0f, 0f - size, 0f));
			immediateGeometry.AddVertex(new Vector3(0f, size, 0f));
			immediateGeometry.AddVertex(new Vector3(0f, 0f - size, 0.002f));
			immediateGeometry.AddVertex(new Vector3(0f, size, 0.002f));
			immediateGeometry.SetColor(new Color(0f, 0f, 1f));
			immediateGeometry.AddVertex(new Vector3(0f, 0f, 0f - size));
			immediateGeometry.AddVertex(new Vector3(0f, 0f, size));
			immediateGeometry.AddVertex(new Vector3(0.002f, 0f, 0f - size));
			immediateGeometry.AddVertex(new Vector3(0.002f, 0f, size));
			immediateGeometry.End();
			immediateGeometry.Begin(Mesh.PrimitiveType.LineLoop);
			immediateGeometry.SetColor(new Color(0.5f, 0.5f, 0.1f));
			for (int i = 0; i < points; i++)
			{
				float anglePoint = Mathf.Deg2Rad((float)i * 360f / (float)points - 90f);
				immediateGeometry.AddVertex(new Vector3(Mathf.Cos(anglePoint), Mathf.Sin(anglePoint), 0f) * SphereSize);
			}
			immediateGeometry.End();
			immediateGeometry.Begin(Mesh.PrimitiveType.LineLoop);
			immediateGeometry.SetColor(new Color(0.5f, 0.5f, 0.1f));
			for (int j = 0; j < points; j++)
			{
				float anglePoint2 = Mathf.Deg2Rad((float)j * 360f / (float)points - 90f);
				immediateGeometry.AddVertex(new Vector3(0f, Mathf.Cos(anglePoint2), Mathf.Sin(anglePoint2)) * SphereSize);
			}
			immediateGeometry.End();
			immediateGeometry.Begin(Mesh.PrimitiveType.LineLoop);
			immediateGeometry.SetColor(new Color(0.5f, 0.5f, 0.1f));
			for (int k = 0; k < points; k++)
			{
				float anglePoint3 = Mathf.Deg2Rad((float)k * 360f / (float)points - 90f);
				immediateGeometry.AddVertex(new Vector3(Mathf.Sin(anglePoint3), 0f, Mathf.Cos(anglePoint3)) * SphereSize);
			}
			immediateGeometry.End();
			break;
		}
		}
	}
}
