using Godot;

public class Grid : ImmediateGeometry
{
	private Worksheet worksheet;

	private ImmediateGeometry mirrorImmediateGeometry;

	private bool mirroring;

	private bool verticalMirroring = true;

	private float verticalMirrorOffset;

	private bool horizontalMirroring;

	private float horizontalMirrorOffset;

	private bool circleMirroring;

	private float circleVerticalMirrorOffset;

	private float circleHorizontalMirrorOffset;

	private bool visible;

	private bool doShowLayerContentAreas;

	private float lineThickness = 0.1f;

	public new bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			visible = value;
			Update(worksheet, mirroring, verticalMirroring, verticalMirrorOffset, horizontalMirroring, horizontalMirrorOffset, circleMirroring, circleVerticalMirrorOffset, circleHorizontalMirrorOffset);
		}
	}

	public bool DoShowLayerContentAreas
	{
		get
		{
			return doShowLayerContentAreas;
		}
		set
		{
			doShowLayerContentAreas = value;
		}
	}

	public Grid()
	{
		Register.GridManager = this;
	}

	public Grid(Workspace workspace)
	{
		Register.GridManager = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "Grid";
	}

	public override void _Ready()
	{
		base._Ready();
		mirrorImmediateGeometry = new ImmediateGeometry();
		AddChild(mirrorImmediateGeometry);
		mirrorImmediateGeometry.Owner = this;
		mirrorImmediateGeometry.Name = "Mirror";
		mirrorImmediateGeometry.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.GRID);
		base.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.GRID);
	}

	public void AddLine(Vector3 start, Vector3 end, float thickness)
	{
		Vector3 direction = end - start;
		Vector3 camera = new Vector3(0f, 1f, 0f);
		Vector3 normal = direction.Cross(camera).Normalized();
		Vector3[] vertex = new Vector3[4]
		{
			start + normal * thickness * 0.5f,
			start - normal * thickness * 0.5f,
			end + normal * thickness * 0.5f,
			end - normal * thickness * 0.5f
		};
		AddVertex(vertex[0]);
		AddVertex(vertex[1]);
		AddVertex(vertex[2]);
		AddVertex(vertex[2]);
		AddVertex(vertex[1]);
		AddVertex(vertex[3]);
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
		AddVertex(start);
		AddVertex(rightTop);
		AddVertex(leftBottom);
		AddVertex(leftBottom);
		AddVertex(rightTop);
		AddVertex(end);
	}

	public void AddBox(Vector3 position, float size)
	{
		Vector3 sizeVector = new Vector3(size, size, size);
		AddBox(position - sizeVector, position + sizeVector);
	}

	public void Update(Worksheet worksheet, bool mirroring, bool verticalMirroring, float verticalMirrorOffset, bool horizontalMirroring, float horizontalMirrorOffset, bool circleMirroring, float circleVerticalMirrorOffset, float circleHorizontalMirrorOffset)
	{
		this.worksheet = worksheet;
		Clear();
		if (doShowLayerContentAreas)
		{
			worksheet.Data.Layer.UpdateContentArea();
			Begin(Mesh.PrimitiveType.Triangles);
			Vector3 gridOffset = new Vector3(0f, Settings.GridOffset, 0f);
			SetColor(Settings.AccentColor);
			Vector3 start = gridOffset + new Vector3((float)worksheet.Data.Layer.ContentAreaStart.x * 0.25f, 0f, (float)worksheet.Data.Layer.ContentAreaStart.y * 0.25f);
			Vector3 end = gridOffset + new Vector3((float)(worksheet.Data.Layer.ContentAreaEnd.x + 1) * 0.25f, 0f, (float)(worksheet.Data.Layer.ContentAreaEnd.y + 1) * 0.25f);
			float height = (start.y + end.y) * 0.5f;
			Vector3 leftBottom = new Vector3(start.x, height, end.z);
			Vector3 rightTop = new Vector3(end.x, height, start.z);
			AddLineBox(start, end, lineThickness);
			AddBox(start, lineThickness * 1.2f);
			AddBox(leftBottom, lineThickness * 1.2f);
			AddBox(rightTop, lineThickness * 1.2f);
			AddBox(end, lineThickness * 1.2f);
			start = gridOffset + new Vector3((worksheet.Data.Layer.ContentCenter.x - 1.5f) * 0.25f, height, worksheet.Data.Layer.ContentCenter.y * 0.25f);
			end = gridOffset + new Vector3((worksheet.Data.Layer.ContentCenter.x + 1.5f) * 0.25f, height, worksheet.Data.Layer.ContentCenter.y * 0.25f);
			AddLine(start, end, lineThickness);
			start = gridOffset + new Vector3(worksheet.Data.Layer.ContentCenter.x * 0.25f, height, (worksheet.Data.Layer.ContentCenter.y - 1.5f) * 0.25f);
			end = gridOffset + new Vector3(worksheet.Data.Layer.ContentCenter.x * 0.25f, height, (worksheet.Data.Layer.ContentCenter.y + 1.5f) * 0.25f);
			AddLine(start, end, lineThickness);
			End();
		}
		Begin(Mesh.PrimitiveType.Lines);
		SetColor(Settings.GridColor);
		if (worksheet.Data.Tileable)
		{
			Vector3 gridOffset = new Vector3((float)(-worksheet.Data.Width) * 0.25f, Settings.GridOffset, (float)(-worksheet.Data.Height) * 0.25f);
			for (int i = 0; i <= worksheet.Data.Height * 3; i++)
			{
				if (i == worksheet.Data.Height || i == worksheet.Data.Height * 2)
				{
					SetColor(Settings.AccentColor);
					AddVertex(gridOffset + new Vector3(0f, 0f, (float)i * 0.25f));
					AddVertex(gridOffset + new Vector3(3f * (float)worksheet.Data.Width * 0.25f, 0f, (float)i * 0.25f));
					SetColor(Settings.GridColor);
				}
				else if (visible)
				{
					AddVertex(gridOffset + new Vector3(0f, 0f, (float)i * 0.25f));
					AddVertex(gridOffset + new Vector3(3f * (float)worksheet.Data.Width * 0.25f, 0f, (float)i * 0.25f));
				}
			}
			for (int j = 0; j <= worksheet.Data.Width * 3; j++)
			{
				if (j == worksheet.Data.Width || j == worksheet.Data.Width * 2)
				{
					SetColor(Settings.AccentColor);
					AddVertex(gridOffset + new Vector3((float)j * 0.25f, 0f, 0f));
					AddVertex(gridOffset + new Vector3((float)j * 0.25f, 0f, 3f * (float)worksheet.Data.Height * 0.25f));
					SetColor(Settings.GridColor);
				}
				else if (visible)
				{
					AddVertex(gridOffset + new Vector3((float)j * 0.25f, 0f, 0f));
					AddVertex(gridOffset + new Vector3((float)j * 0.25f, 0f, 3f * (float)worksheet.Data.Height * 0.25f));
				}
			}
		}
		else
		{
			Vector3 gridOffset = new Vector3(0f, Settings.GridOffset, 0f);
			for (int k = 0; k <= worksheet.Data.Height; k++)
			{
				if (k == 0 || k == worksheet.Data.Height)
				{
					SetColor(Settings.AccentColor);
					AddVertex(gridOffset + new Vector3(-0.25f, 0f, (float)k * 0.25f));
					AddVertex(gridOffset + new Vector3((float)worksheet.Data.Width * 0.25f + 0.25f, 0f, (float)k * 0.25f));
					SetColor(Settings.GridColor);
				}
				else if (visible)
				{
					AddVertex(gridOffset + new Vector3(0f, 0f, (float)k * 0.25f));
					AddVertex(gridOffset + new Vector3((float)worksheet.Data.Width * 0.25f, 0f, (float)k * 0.25f));
				}
			}
			for (int l = 0; l <= worksheet.Data.Width; l++)
			{
				if (l == 0 || l == worksheet.Data.Width)
				{
					SetColor(Settings.AccentColor);
					AddVertex(gridOffset + new Vector3((float)l * 0.25f, 0f, -0.25f));
					AddVertex(gridOffset + new Vector3((float)l * 0.25f, 0f, (float)worksheet.Data.Height * 0.25f + 0.25f));
					SetColor(Settings.GridColor);
				}
				else if (visible)
				{
					AddVertex(gridOffset + new Vector3((float)l * 0.25f, 0f, 0f));
					AddVertex(gridOffset + new Vector3((float)l * 0.25f, 0f, (float)worksheet.Data.Height * 0.25f));
				}
			}
		}
		End();
		UpdateMirror(this.worksheet, mirroring, verticalMirroring, verticalMirrorOffset, horizontalMirroring, horizontalMirrorOffset, circleMirroring, circleVerticalMirrorOffset, circleHorizontalMirrorOffset);
	}

	public void Update(Worksheet worksheet)
	{
		this.worksheet = worksheet;
		Clear();
		if (doShowLayerContentAreas)
		{
			worksheet.Data.Layer.UpdateContentArea();
			Begin(Mesh.PrimitiveType.Triangles);
			Vector3 gridOffset = new Vector3(0f, Settings.GridOffset, 0f);
			SetColor(Settings.AccentColor);
			Vector3 start = gridOffset + new Vector3((float)worksheet.Data.Layer.ContentAreaStart.x * 0.25f, 0f, (float)worksheet.Data.Layer.ContentAreaStart.y * 0.25f);
			Vector3 end = gridOffset + new Vector3((float)(worksheet.Data.Layer.ContentAreaEnd.x + 1) * 0.25f, 0f, (float)(worksheet.Data.Layer.ContentAreaEnd.y + 1) * 0.25f);
			float height = (start.y + end.y) * 0.5f;
			Vector3 leftBottom = new Vector3(start.x, height, end.z);
			Vector3 rightTop = new Vector3(end.x, height, start.z);
			AddLineBox(start, end, lineThickness);
			AddBox(start, lineThickness * 1.2f);
			AddBox(leftBottom, lineThickness * 1.2f);
			AddBox(rightTop, lineThickness * 1.2f);
			AddBox(end, lineThickness * 1.2f);
			start = gridOffset + new Vector3((worksheet.Data.Layer.ContentCenter.x - 1.5f) * 0.25f, height, worksheet.Data.Layer.ContentCenter.y * 0.25f);
			end = gridOffset + new Vector3((worksheet.Data.Layer.ContentCenter.x + 1.5f) * 0.25f, height, worksheet.Data.Layer.ContentCenter.y * 0.25f);
			AddLine(start, end, lineThickness);
			start = gridOffset + new Vector3(worksheet.Data.Layer.ContentCenter.x * 0.25f, height, (worksheet.Data.Layer.ContentCenter.y - 1.5f) * 0.25f);
			end = gridOffset + new Vector3(worksheet.Data.Layer.ContentCenter.x * 0.25f, height, (worksheet.Data.Layer.ContentCenter.y + 1.5f) * 0.25f);
			AddLine(start, end, lineThickness);
			End();
		}
		Begin(Mesh.PrimitiveType.Lines);
		SetColor(Settings.GridColor);
		if (worksheet.Data.Tileable)
		{
			Vector3 gridOffset = new Vector3((float)(-worksheet.Data.Width) * 0.25f, Settings.GridOffset, (float)(-worksheet.Data.Height) * 0.25f);
			for (int i = 0; i <= worksheet.Data.Height * 3; i++)
			{
				if (i == worksheet.Data.Height || i == worksheet.Data.Height * 2)
				{
					SetColor(Settings.AccentColor);
					AddVertex(gridOffset + new Vector3(0f, 0f, (float)i * 0.25f));
					AddVertex(gridOffset + new Vector3(3f * (float)worksheet.Data.Width * 0.25f, 0f, (float)i * 0.25f));
					SetColor(Settings.GridColor);
				}
				else if (visible)
				{
					AddVertex(gridOffset + new Vector3(0f, 0f, (float)i * 0.25f));
					AddVertex(gridOffset + new Vector3(3f * (float)worksheet.Data.Width * 0.25f, 0f, (float)i * 0.25f));
				}
			}
			for (int j = 0; j <= worksheet.Data.Width * 3; j++)
			{
				if (j == worksheet.Data.Width || j == worksheet.Data.Width * 2)
				{
					SetColor(Settings.AccentColor);
					AddVertex(gridOffset + new Vector3((float)j * 0.25f, 0f, 0f));
					AddVertex(gridOffset + new Vector3((float)j * 0.25f, 0f, 3f * (float)worksheet.Data.Height * 0.25f));
					SetColor(Settings.GridColor);
				}
				else if (visible)
				{
					AddVertex(gridOffset + new Vector3((float)j * 0.25f, 0f, 0f));
					AddVertex(gridOffset + new Vector3((float)j * 0.25f, 0f, 3f * (float)worksheet.Data.Height * 0.25f));
				}
			}
		}
		else
		{
			Vector3 gridOffset = new Vector3(0f, Settings.GridOffset, 0f);
			for (int k = 0; k <= worksheet.Data.Height; k++)
			{
				if (k == 0 || k == worksheet.Data.Height)
				{
					SetColor(Settings.AccentColor);
					AddVertex(gridOffset + new Vector3(-0.25f, 0f, (float)k * 0.25f));
					AddVertex(gridOffset + new Vector3((float)worksheet.Data.Width * 0.25f + 0.25f, 0f, (float)k * 0.25f));
					SetColor(Settings.GridColor);
				}
				else if (visible)
				{
					AddVertex(gridOffset + new Vector3(0f, 0f, (float)k * 0.25f));
					AddVertex(gridOffset + new Vector3((float)worksheet.Data.Width * 0.25f, 0f, (float)k * 0.25f));
				}
			}
			for (int l = 0; l <= worksheet.Data.Width; l++)
			{
				if (l == 0 || l == worksheet.Data.Width)
				{
					SetColor(Settings.AccentColor);
					AddVertex(gridOffset + new Vector3((float)l * 0.25f, 0f, -0.25f));
					AddVertex(gridOffset + new Vector3((float)l * 0.25f, 0f, (float)worksheet.Data.Height * 0.25f + 0.25f));
					SetColor(Settings.GridColor);
				}
				else if (visible)
				{
					AddVertex(gridOffset + new Vector3((float)l * 0.25f, 0f, 0f));
					AddVertex(gridOffset + new Vector3((float)l * 0.25f, 0f, (float)worksheet.Data.Height * 0.25f));
				}
			}
		}
		End();
		UpdateMirror(this.worksheet, Register.DrawingManager.Mirroring, Register.DrawingManager.VerticalMirroring, Register.DrawingManager.VerticalMirrorPosition, Register.DrawingManager.HorizontalMirroring, Register.DrawingManager.HorizontalMirrorPosition, Register.DrawingManager.CircleMirroring, Register.DrawingManager.CircleVerticalMirrorPosition, Register.DrawingManager.CircleHorizontalMirrorPosition);
	}

	private void AddChannelContentArea(Channel channel, Color color)
	{
		if (channel.HasContent)
		{
			Vector3 vector = new Vector3(0f, Settings.GridOffset, 0f);
			SetColor(color);
			Vector3 start = vector + new Vector3((float)channel.ContentAreaStart.x * 0.25f, 0f, (float)channel.ContentAreaStart.y * 0.25f);
			Vector3 end = vector + new Vector3((float)(channel.ContentAreaEnd.x + 1) * 0.25f, 0f, (float)(channel.ContentAreaEnd.y + 1) * 0.25f);
			float height = (start.y + end.y) * 0.5f;
			Vector3 leftBottom = new Vector3(start.x, height, end.z);
			Vector3 rightTop = new Vector3(end.x, height, start.z);
			AddLineBox(start, end, lineThickness * 0.8f);
			AddBox(start, lineThickness);
			AddBox(leftBottom, lineThickness);
			AddBox(rightTop, lineThickness);
			AddBox(end, lineThickness);
		}
	}

	public void UpdateMirror(Worksheet worksheet, bool mirroring, bool verticalMirroring, float verticalMirrorOffset, bool horizontalMirroring, float horizontalMirrorOffset, bool circleMirroring, float circleVerticalMirrorOffset, float circleHorizontalMirrorOffset)
	{
		this.worksheet = worksheet;
		this.mirroring = mirroring;
		this.verticalMirroring = verticalMirroring;
		this.verticalMirrorOffset = verticalMirrorOffset;
		this.horizontalMirroring = horizontalMirroring;
		this.horizontalMirrorOffset = horizontalMirrorOffset;
		this.circleMirroring = circleMirroring;
		this.circleVerticalMirrorOffset = circleVerticalMirrorOffset;
		this.circleHorizontalMirrorOffset = circleHorizontalMirrorOffset;
		mirrorImmediateGeometry.Clear();
		mirrorImmediateGeometry.Begin(Mesh.PrimitiveType.Lines);
		mirrorImmediateGeometry.SetColor(Settings.MirrorColor);
		if (mirroring)
		{
			if (worksheet.Data.Tileable)
			{
				if (verticalMirroring)
				{
					Vector3 mirrorOffset = new Vector3((float)(-worksheet.Data.Width) * 0.25f, Settings.MirrorOffset, (float)(-worksheet.Data.Height) * 0.25f);
					mirrorImmediateGeometry.AddVertex(mirrorOffset + new Vector3(((float)worksheet.Data.Width + verticalMirrorOffset) * 0.25f, 0f, 0f));
					mirrorImmediateGeometry.AddVertex(mirrorOffset + new Vector3(((float)worksheet.Data.Width + verticalMirrorOffset) * 0.25f, 0f, 3f * (float)worksheet.Data.Height * 0.25f));
				}
				if (horizontalMirroring)
				{
					Vector3 mirrorOffset2 = new Vector3((float)(-worksheet.Data.Width) * 0.25f, Settings.MirrorOffset, (float)(-worksheet.Data.Height) * 0.25f);
					mirrorImmediateGeometry.AddVertex(mirrorOffset2 + new Vector3(0f, 0f, ((float)worksheet.Data.Height + horizontalMirrorOffset) * 0.25f));
					mirrorImmediateGeometry.AddVertex(mirrorOffset2 + new Vector3(3f * (float)worksheet.Data.Width * 0.25f, 0f, ((float)worksheet.Data.Height + horizontalMirrorOffset) * 0.25f));
				}
				if (circleMirroring)
				{
					Vector3 mirrorOffset3 = new Vector3(0f, Settings.MirrorOffset, 0f);
					mirrorImmediateGeometry.AddVertex(mirrorOffset3 + new Vector3(circleVerticalMirrorOffset * 0.25f, 0f, (circleHorizontalMirrorOffset - 2f) * 0.25f));
					mirrorImmediateGeometry.AddVertex(mirrorOffset3 + new Vector3(circleVerticalMirrorOffset * 0.25f, 0f, (circleHorizontalMirrorOffset + 2f) * 0.25f));
					mirrorImmediateGeometry.AddVertex(mirrorOffset3 + new Vector3((circleVerticalMirrorOffset - 2f) * 0.25f, 0f, circleHorizontalMirrorOffset * 0.25f));
					mirrorImmediateGeometry.AddVertex(mirrorOffset3 + new Vector3((circleVerticalMirrorOffset + 2f) * 0.25f, 0f, circleHorizontalMirrorOffset * 0.25f));
				}
			}
			else
			{
				if (verticalMirroring)
				{
					Vector3 mirrorOffset4 = new Vector3(0f, Settings.MirrorOffset, 0f);
					mirrorImmediateGeometry.AddVertex(mirrorOffset4 + new Vector3(verticalMirrorOffset * 0.25f, 0f, -0.25f));
					mirrorImmediateGeometry.AddVertex(mirrorOffset4 + new Vector3(verticalMirrorOffset * 0.25f, 0f, (float)worksheet.Data.Height * 0.25f + 0.25f));
				}
				if (horizontalMirroring)
				{
					Vector3 mirrorOffset5 = new Vector3(0f, Settings.MirrorOffset, 0f);
					mirrorImmediateGeometry.AddVertex(mirrorOffset5 + new Vector3(-0.25f, 0f, horizontalMirrorOffset * 0.25f));
					mirrorImmediateGeometry.AddVertex(mirrorOffset5 + new Vector3((float)worksheet.Data.Width * 0.25f + 0.25f, 0f, horizontalMirrorOffset * 0.25f));
				}
				if (circleMirroring)
				{
					Vector3 mirrorOffset6 = new Vector3(0f, Settings.MirrorOffset, 0f);
					mirrorImmediateGeometry.AddVertex(mirrorOffset6 + new Vector3(circleVerticalMirrorOffset * 0.25f, 0f, (circleHorizontalMirrorOffset - 2f) * 0.25f));
					mirrorImmediateGeometry.AddVertex(mirrorOffset6 + new Vector3(circleVerticalMirrorOffset * 0.25f, 0f, (circleHorizontalMirrorOffset + 2f) * 0.25f));
					mirrorImmediateGeometry.AddVertex(mirrorOffset6 + new Vector3((circleVerticalMirrorOffset - 2f) * 0.25f, 0f, circleHorizontalMirrorOffset * 0.25f));
					mirrorImmediateGeometry.AddVertex(mirrorOffset6 + new Vector3((circleVerticalMirrorOffset + 2f) * 0.25f, 0f, circleHorizontalMirrorOffset * 0.25f));
				}
			}
		}
		mirrorImmediateGeometry.End();
	}
}
