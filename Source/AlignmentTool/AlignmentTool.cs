using System;
using Godot;

public class AlignmentTool : ImmediateGeometry
{
	private Workspace workspace;

	private Vector2i startPosition = Vector2i.Zero;

	private Vector2 startViewportPosition = Vector2.Zero;

	private Label startLabel;

	private Vector2 startLabelPosition = Vector2.Zero;

	private Vector2i endPosition = Vector2i.Zero;

	private Vector2 endViewportPosition = Vector2.Zero;

	private Label endLabel;

	private Vector2 endLabelPosition = Vector2.Zero;

	private Label widthLabel;

	private Label heightLabel;

	private bool doArea;

	public Workspace Workspace
	{
		get
		{
			return workspace;
		}
		set
		{
			workspace = value;
		}
	}

	public bool DoArea
	{
		get
		{
			return doArea;
		}
		set
		{
			doArea = value;
		}
	}

	public new bool Visible
	{
		get
		{
			return base.Visible;
		}
		set
		{
			base.Visible = value;
			startLabel.Visible = base.Visible;
			endLabel.Visible = base.Visible;
			widthLabel.Visible = base.Visible;
			heightLabel.Visible = base.Visible;
		}
	}

	public AlignmentTool()
	{
		Register.AlignmentTool = this;
	}

	public AlignmentTool(Workspace workspace)
	{
		Register.AlignmentTool = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "AlignmentTool";
		this.workspace = workspace;
	}

	public override void _Ready()
	{
		base._Ready();
		base.MaterialOverride = MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.GRID);
		startLabel = new Label();
		Register.CameraManager.WorkspaceViewport.CallDeferred("add_child", startLabel);
		startLabel.CallDeferred("set_owner", Register.CameraManager.WorkspaceViewport);
		startLabel.AddColorOverride("font_color", Settings.AlignmentColor);
		startLabel.AddFontOverride("font", Resources.BoldFont);
		startLabel.Align = Label.AlignEnum.Right;
		endLabel = new Label();
		Register.CameraManager.WorkspaceViewport.CallDeferred("add_child", endLabel);
		endLabel.CallDeferred("set_owner", Register.CameraManager.WorkspaceViewport);
		endLabel.AddColorOverride("font_color", Settings.AlignmentColor);
		endLabel.AddFontOverride("font", Resources.BoldFont);
		endLabel.Align = Label.AlignEnum.Left;
		widthLabel = new Label();
		Register.CameraManager.WorkspaceViewport.CallDeferred("add_child", widthLabel);
		widthLabel.CallDeferred("set_owner", Register.CameraManager.WorkspaceViewport);
		widthLabel.AddColorOverride("font_color", Settings.AlignmentColor);
		widthLabel.AddFontOverride("font", Resources.BoldFont);
		widthLabel.Align = Label.AlignEnum.Center;
		heightLabel = new Label();
		Register.CameraManager.WorkspaceViewport.CallDeferred("add_child", heightLabel);
		heightLabel.CallDeferred("set_owner", Register.CameraManager.WorkspaceViewport);
		heightLabel.AddColorOverride("font_color", Settings.AlignmentColor);
		heightLabel.AddFontOverride("font", Resources.BoldFont);
		heightLabel.Align = Label.AlignEnum.Left;
		Visible = false;
	}

	private void SetStartPosition(Vector2i position)
	{
		startPosition = position;
		Vector2i offset = Vector2i.Zero;
		if (startPosition.x > endPosition.x)
		{
			offset.x = 1;
		}
		if (startPosition.y > endPosition.y)
		{
			offset.y = 1;
		}
		startLabelPosition = Register.CameraManager.WorkspaceCamera.UnprojectPosition(new Vector3((float)(startPosition.x + offset.x) * 0.25f, 0f, (float)(startPosition.y + offset.y) * 0.25f));
		startViewportPosition = startLabelPosition;
		if (startLabelPosition.x > endLabelPosition.x)
		{
			startLabelPosition.x += 8f;
		}
		else
		{
			startLabelPosition.x -= startLabel.RectSize.x + 4f;
		}
		if (startLabelPosition.y > endLabelPosition.y)
		{
			startLabelPosition.y += 0f;
		}
		else
		{
			startLabelPosition.y -= startLabel.RectSize.y - 12f;
		}
		startLabel.RectPosition = startLabelPosition;
	}

	private void SetEndPosition(Vector2i position)
	{
		endPosition = position;
		Vector2i offset = Vector2i.Zero;
		if (startPosition.x < endPosition.x)
		{
			offset.x = 1;
		}
		if (startPosition.y < endPosition.y)
		{
			offset.y = 1;
		}
		endLabelPosition = Register.CameraManager.WorkspaceCamera.UnprojectPosition(new Vector3((float)(endPosition.x + offset.x) * 0.25f, 0f, (float)(endPosition.y + offset.y) * 0.25f));
		endViewportPosition = endLabelPosition;
		if (!workspace.Worksheet.Data.Tileable)
		{
			if (endPosition.x < 0 || endPosition.y < 0 || endPosition.x > workspace.Worksheet.Data.Width - 1 || endPosition.y > workspace.Worksheet.Data.Height - 1)
			{
				endLabel.Visible = false;
			}
			else
			{
				endLabel.Visible = true;
			}
		}
		if (doArea)
		{
			if (startLabelPosition.x < endLabelPosition.x)
			{
				endLabelPosition.x += 8f;
			}
			else
			{
				endLabelPosition.x -= endLabel.RectSize.x + 4f;
			}
			if (startLabelPosition.y < endLabelPosition.y)
			{
				endLabelPosition.y += 0f;
			}
			else
			{
				endLabelPosition.y -= endLabel.RectSize.y - 12f;
			}
		}
		else
		{
			endLabelPosition.x += 8f;
			endLabelPosition.y += 0f;
		}
		endLabel.RectPosition = endLabelPosition;
		UpdateSizeLabels();
	}

	private void UpdateSizeLabels()
	{
		Vector2i difference = endPosition - startPosition;
		Vector2 labelDifference = endViewportPosition - startViewportPosition;
		labelDifference.x = Mathf.Abs(labelDifference.x);
		labelDifference.y = Mathf.Abs(labelDifference.y);
		widthLabel.Text = (Math.Abs(difference.x) + 1).ToString();
		if (labelDifference.x >= widthLabel.RectSize.x - 8f)
		{
			widthLabel.Visible = true;
			Vector2 widthLabelPosition = default(Vector2);
			widthLabelPosition.x = ((startLabelPosition.x < endLabelPosition.x) ? startLabelPosition.x : endLabelPosition.x);
			widthLabelPosition.x += ((startLabelPosition.x > endLabelPosition.x) ? (startLabelPosition.x - endLabelPosition.x) : (endLabelPosition.x - startLabelPosition.x)) * 0.5f;
			widthLabelPosition.x += widthLabel.RectSize.x * 0.5f + 2f;
			widthLabelPosition.y = ((startLabelPosition.y > endLabelPosition.y) ? startLabelPosition.y : endLabelPosition.y);
			widthLabel.RectPosition = widthLabelPosition;
		}
		else
		{
			widthLabel.Visible = false;
		}
		heightLabel.Text = (Math.Abs(difference.y) + 1).ToString();
		if (labelDifference.y >= heightLabel.RectSize.y - 16f)
		{
			heightLabel.Visible = true;
			Vector2 heightLabelPosition = default(Vector2);
			heightLabelPosition.x = ((startLabelPosition.x > endLabelPosition.x) ? startLabelPosition.x : endLabelPosition.x);
			heightLabelPosition.y = ((startLabelPosition.y < endLabelPosition.y) ? startLabelPosition.y : endLabelPosition.y);
			heightLabelPosition.y += ((startLabelPosition.y > endLabelPosition.y) ? (startLabelPosition.y - endLabelPosition.y) : (endLabelPosition.y - startLabelPosition.y)) * 0.5f;
			heightLabel.RectPosition = heightLabelPosition;
		}
		else
		{
			heightLabel.Visible = false;
		}
	}

	public void Update(Vector2i startPosition, Vector2i currentPosition)
	{
		if (Visible)
		{
			if (doArea)
			{
				startLabel.Visible = true;
				widthLabel.Visible = true;
				heightLabel.Visible = true;
			}
			startLabel.Text = Mathf.PosMod(startPosition.x, workspace.Worksheet.Data.Width) + ", " + Mathf.PosMod(startPosition.y, workspace.Worksheet.Data.Height);
			SetStartPosition(startPosition);
			endLabel.Text = Mathf.PosMod(currentPosition.x, workspace.Worksheet.Data.Width) + ", " + Mathf.PosMod(currentPosition.y, workspace.Worksheet.Data.Height);
			SetEndPosition(currentPosition);
			if (!doArea)
			{
				startLabel.Visible = false;
				widthLabel.Visible = false;
				heightLabel.Visible = false;
			}
			Clear();
			Begin(Mesh.PrimitiveType.Lines);
			SetColor(Settings.AlignmentColor);
			Vector3 offset = new Vector3(0f, Settings.AlignmentToolOffset, 0f);
			if (workspace.Worksheet.Data.Tileable)
			{
				Vector2i minPosition = new Vector2i(-workspace.Worksheet.Data.Width, -workspace.Worksheet.Data.Height);
				Vector2i maxPosition = new Vector2i((float)workspace.Worksheet.Data.Width * 2f, (float)workspace.Worksheet.Data.Height * 2f);
				if (currentPosition.x > minPosition.x - 1 && currentPosition.y > minPosition.y - 1 && currentPosition.x < maxPosition.x && currentPosition.y < maxPosition.y)
				{
					Vector2 minWorldPosition = minPosition.ToFloat() * 0.25f;
					Vector2 maxWorldPosition = maxPosition.ToFloat() * 0.25f;
					if (doArea)
					{
						Vector2i start = startPosition;
						Vector2i end = currentPosition;
						if (start.x > end.x)
						{
							ref int x = ref start.x;
							ref int x2 = ref end.x;
							int x3 = end.x;
							int x4 = start.x;
							x = x3;
							x2 = x4;
						}
						if (start.y > end.y)
						{
							ref int x = ref start.y;
							ref int y = ref end.y;
							int x4 = end.y;
							int x3 = start.y;
							x = x4;
							y = x3;
						}
						AddVertex(offset + new Vector3(minWorldPosition.x, 0f, (float)start.y * 0.25f));
						AddVertex(offset + new Vector3(maxWorldPosition.x, 0f, (float)start.y * 0.25f));
						AddVertex(offset + new Vector3((float)start.x * 0.25f, 0f, minWorldPosition.y));
						AddVertex(offset + new Vector3((float)start.x * 0.25f, 0f, maxWorldPosition.y));
						AddVertex(offset + new Vector3(minWorldPosition.x, 0f, (float)(end.y + 1) * 0.25f));
						AddVertex(offset + new Vector3(maxWorldPosition.x, 0f, (float)(end.y + 1) * 0.25f));
						AddVertex(offset + new Vector3((float)(end.x + 1) * 0.25f, 0f, minWorldPosition.y));
						AddVertex(offset + new Vector3((float)(end.x + 1) * 0.25f, 0f, maxWorldPosition.y));
					}
					else
					{
						AddVertex(offset + new Vector3(minWorldPosition.x, 0f, ((float)currentPosition.y + 0.5f) * 0.25f));
						AddVertex(offset + new Vector3(maxWorldPosition.x, 0f, ((float)currentPosition.y + 0.5f) * 0.25f));
						AddVertex(offset + new Vector3(((float)currentPosition.x + 0.5f) * 0.25f, 0f, minWorldPosition.y));
						AddVertex(offset + new Vector3(((float)currentPosition.x + 0.5f) * 0.25f, 0f, maxWorldPosition.y));
					}
				}
			}
			else
			{
				Vector2i maxPosition2 = new Vector2i(workspace.Worksheet.Data.Width, workspace.Worksheet.Data.Height);
				if (currentPosition.x > -1 && currentPosition.y > -1 && currentPosition.x < maxPosition2.x && currentPosition.y < maxPosition2.y)
				{
					Vector2 maxWorldPosition2 = maxPosition2.ToFloat() * 0.25f;
					if (doArea)
					{
						Vector2i start2 = startPosition;
						Vector2i end2 = currentPosition;
						if (start2.x > end2.x)
						{
							ref int x = ref start2.x;
							ref int x5 = ref end2.x;
							int x3 = end2.x;
							int x4 = start2.x;
							x = x3;
							x5 = x4;
						}
						if (start2.y > end2.y)
						{
							ref int x = ref start2.y;
							ref int y2 = ref end2.y;
							int x4 = end2.y;
							int x3 = start2.y;
							x = x4;
							y2 = x3;
						}
						AddVertex(offset + new Vector3(0f, 0f, (float)start2.y * 0.25f));
						AddVertex(offset + new Vector3(maxWorldPosition2.x, 0f, (float)start2.y * 0.25f));
						AddVertex(offset + new Vector3((float)start2.x * 0.25f, 0f, 0f));
						AddVertex(offset + new Vector3((float)start2.x * 0.25f, 0f, maxWorldPosition2.y));
						AddVertex(offset + new Vector3(0f, 0f, (float)(end2.y + 1) * 0.25f));
						AddVertex(offset + new Vector3(maxWorldPosition2.x, 0f, (float)(end2.y + 1) * 0.25f));
						AddVertex(offset + new Vector3((float)(end2.x + 1) * 0.25f, 0f, 0f));
						AddVertex(offset + new Vector3((float)(end2.x + 1) * 0.25f, 0f, maxWorldPosition2.y));
					}
					else
					{
						AddVertex(offset + new Vector3(0f, 0f, ((float)currentPosition.y + 0.5f) * 0.25f));
						AddVertex(offset + new Vector3(maxWorldPosition2.x, 0f, ((float)currentPosition.y + 0.5f) * 0.25f));
						AddVertex(offset + new Vector3(((float)currentPosition.x + 0.5f) * 0.25f, 0f, 0f));
						AddVertex(offset + new Vector3(((float)currentPosition.x + 0.5f) * 0.25f, 0f, maxWorldPosition2.y));
					}
				}
			}
			End();
		}
		else
		{
			startLabel.Visible = false;
			endLabel.Visible = false;
			widthLabel.Visible = false;
			heightLabel.Visible = false;
		}
	}
}
