using System;
using Godot;

public class DrawingToolLine : DrawingTool
{
	public enum ShapeEnum
	{
		CIRCULAR,
		SQUARE
	}

	private ShapeEnum shape;

	public ShapeEnum Shape
	{
		get
		{
			return shape;
		}
		set
		{
			shape = value;
		}
	}

	public DrawingToolLine()
	{
		drawingManager = Register.DrawingManager;
		drawingPreviewManager = Register.DrawingPreviewManager;
		workspace = Register.Workspace;
		name = "Line";
		areaTool = true;
		circleMirroring = true;
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_LINE, "LINE_SIZE_INCREASE", IncreaseSize, ButtonList.WheelUp, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_LINE, "LINE_SIZE_DECREASE", DecreaseSize, ButtonList.WheelDown, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_LINE, "LINE_BLENDING_INCREASE", IncreaseBlendingStrength, ButtonList.WheelUp, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: false, alt: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_LINE, "LINE_BLENDING_DECREASE", DecreaseBlendingStrength, ButtonList.WheelDown, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: false, alt: true);
	}

	private float CalculateBlendingStrength(float x, float y)
	{
		x += 0.5f;
		y += 0.5f;
		float deltaX = Mathf.Abs(x - halfSize);
		float deltaY = Mathf.Abs(y - halfSize);
		float blending = 1f;
		switch (shape)
		{
		case ShapeEnum.CIRCULAR:
		{
			float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
			if (distance > halfSize)
			{
				return 0f;
			}
			blending = ((!(distance <= hardness * halfSize)) ? (0.5f - 0.5f * Mathf.Cos((1f - distance / halfSize) / (1f - hardness) * Mathf.Pi)) : 1f);
			break;
		}
		case ShapeEnum.SQUARE:
			if (deltaX > halfSize || deltaY > halfSize)
			{
				return 0f;
			}
			if (deltaX <= hardness * halfSize && deltaY <= hardness * halfSize)
			{
				blending = 1f;
				break;
			}
			blending = 1f;
			if (deltaX > hardness * halfSize)
			{
				blending *= 0.5f - 0.5f * Mathf.Cos((1f - deltaX / halfSize) / (1f - hardness) * Mathf.Pi);
			}
			if (deltaY > hardness * halfSize)
			{
				blending *= 0.5f - 0.5f * Mathf.Cos((1f - deltaY / halfSize) / (1f - hardness) * Mathf.Pi);
			}
			break;
		}
		return blending;
	}

	public override void DrawPreview(Vector2i position)
	{
		if (drawingManager.IsDrawing)
		{
			return;
		}
		drawingPreviewManager.ClearArea();
		if (drawingManager.Mirroring && drawingManager.CircleMirroring)
		{
			drawingManager.DrawCircleMirroredSpot(CalculateBlendingStrength, position.x, position.y, size, blendingStrength);
			return;
		}
		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				float falloffBlendingStrength = CalculateBlendingStrength(x, y);
				if (falloffBlendingStrength != 0f)
				{
					drawingPreviewManager.DrawPixel(position.x + x - Mathf.CeilToInt((float)size * 0.5f) + 1, position.y + y - Mathf.CeilToInt((float)size * 0.5f) + 1, clear: false, falloffBlendingStrength * blendingStrength);
				}
			}
		}
	}

	public override void DrawSpot(Vector2i position)
	{
		if (drawingManager.Mirroring && drawingManager.CircleMirroring)
		{
			drawingManager.DrawCircleMirroredSpot(CalculateBlendingStrength, position.x, position.y, size, blendingStrength);
			return;
		}
		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				float falloffBlendingStrength = CalculateBlendingStrength(x, y);
				if (falloffBlendingStrength != 0f)
				{
					drawingPreviewManager.DrawPixel(position.x + x - Mathf.CeilToInt((float)size * 0.5f) + 1, position.y + y - Mathf.CeilToInt((float)size * 0.5f) + 1, clear: false, falloffBlendingStrength * blendingStrength);
				}
			}
		}
	}

	public override void DrawStroke(Vector2i start, Vector2i end)
	{
		float shift = 0f;
		if ((size & 1) == 0)
		{
			shift = -0.5f;
		}
		switch (InputManager.CursorSpace)
		{
		case CameraManager.SpaceEnum.WORKSPACE:
		{
			Vector2 delta = end.ToFloat() - start.ToFloat();
			Vector2 normal = new Vector2(delta.y, 0f - delta.x).Normalized();
			Vector2i lefttop = new Vector2i(Math.Min(start.x, end.x) - size / 2, Math.Min(start.y, end.y) - size / 2);
			Vector2i rightbottom = new Vector2i(Math.Max(start.x, end.x) + size / 2, Math.Max(start.y, end.y) + size / 2);
			if (drawingManager.Mirroring && drawingManager.CircleMirroring)
			{
				drawingManager.DrawCircleMirroredStroke(CalculateBlendingStrength, start.x, start.y, end.x, end.y, size, blendingStrength);
				break;
			}
			DrawSpot(start);
			for (int y = lefttop.y; y <= rightbottom.y; y++)
			{
				for (int x = lefttop.x; x <= rightbottom.x; x++)
				{
					float t = (((float)(x - start.x) + shift) * delta.x + ((float)(y - start.y) + shift) * delta.y) / delta.Dot(delta);
					if (0f <= t && t <= 1f)
					{
						float distance = Mathf.Abs(((float)(x - start.x) + shift) * normal.x + ((float)(y - start.y) + shift) * normal.y);
						if (distance <= hardness * halfSize)
						{
							float falloffBlendingStrength = 1f;
							drawingPreviewManager.DrawPixel(x, y, clear: false, falloffBlendingStrength * blendingStrength);
						}
						else if (distance <= halfSize)
						{
							float falloffBlendingStrength = 0.5f - 0.5f * Mathf.Cos((1f - distance / halfSize) / (1f - hardness) * Mathf.Pi);
							drawingPreviewManager.DrawPixel(x, y, clear: false, falloffBlendingStrength * blendingStrength);
						}
					}
				}
			}
			DrawSpot(end);
			break;
		}
		case CameraManager.SpaceEnum.PREVIEWSPACE:
			drawingManager.DrawingPreviewManager.ClearArea();
			{
				foreach (CollisionManager.CollisionData cd in Register.CollisionManager.CollisionsList)
				{
					Vector2i drawingPosition = new Vector2i(Mathf.FloorToInt(shift + cd.UV.x * (float)workspace.Worksheet.Data.Width), Mathf.FloorToInt(shift + cd.UV.y * (float)workspace.Worksheet.Data.Height));
					DrawSpot(drawingPosition);
				}
				break;
			}
		}
	}

	public override void WhileDrawing(Vector2i position)
	{
		drawingEnd = position;
		float shift = 0f;
		if ((size & 1) == 0)
		{
			shift = -0.5f;
		}
		if (Input.IsKeyPressed(16777238))
		{
			float angle = (float)Mathf.RoundToInt(Mathf.Rad2Deg(Mathf.Atan2(drawingEnd.y - drawingStart.y, drawingEnd.x - drawingStart.x)) / 45f) * 45f;
			float length = (drawingEnd - drawingStart).Length();
			drawingEnd.x = drawingStart.x + Mathf.FloorToInt(Mathf.Cos(Mathf.Deg2Rad(angle)) * length);
			drawingEnd.y = drawingStart.y + Mathf.FloorToInt(Mathf.Sin(Mathf.Deg2Rad(angle)) * length);
			if (drawingEnd.x < drawingStart.x)
			{
				drawingEnd.x++;
			}
			if (drawingEnd.y < drawingStart.y)
			{
				drawingEnd.y++;
			}
		}
		if (base.HasDrawingPositionChanged)
		{
			switch (InputManager.CursorSpace)
			{
			case CameraManager.SpaceEnum.WORKSPACE:
				drawingManager.DrawingPreviewManager.ClearArea();
				DrawStroke(drawingStart, drawingEnd);
				break;
			case CameraManager.SpaceEnum.PREVIEWSPACE:
				drawingManager.DrawingPreviewManager.ClearArea();
				foreach (CollisionManager.CollisionData cd in Register.CollisionManager.CollisionsList)
				{
					Vector2i drawingPosition = new Vector2i(Mathf.FloorToInt(shift + cd.UV.x * (float)workspace.Worksheet.Data.Width), Mathf.FloorToInt(shift + cd.UV.y * (float)workspace.Worksheet.Data.Height));
					DrawSpot(drawingPosition);
				}
				break;
			}
		}
		else
		{
			DrawSpot(drawingEnd);
		}
		drawingPositionLastFrame = drawingEnd;
	}

	public override void StopDrawing(Vector2i position)
	{
		drawingEnd = position;
		for (int y = drawingPreviewManager.BlendingMaskUpdateStart.y; y <= drawingPreviewManager.BlendingMaskUpdateEnd.y; y++)
		{
			for (int x = drawingPreviewManager.BlendingMaskUpdateStart.x; x <= drawingPreviewManager.BlendingMaskUpdateEnd.x; x++)
			{
				if (drawingPreviewManager.BlendingMaskChannelArray[x, y] > 0f)
				{
					drawingManager.DrawPixel(workspace.Worksheet, x, y, drawingPreviewManager.BlendingMaskChannelArray[x, y]);
				}
			}
		}
		drawingPreviewManager.ClearArea();
		drawingPreviewManager.Update();
	}

	public override void Reset()
	{
		base.Reset();
	}

	public void IncreaseSize()
	{
		base.Size++;
		Register.Gui.LineSettingsContainer.ChangeSize(base.Size);
	}

	public void DecreaseSize()
	{
		base.Size--;
		Register.Gui.LineSettingsContainer.ChangeSize(base.Size);
	}

	public void IncreaseBlendingStrength()
	{
		base.BlendingStrength += 0.05f;
		Register.Gui.LineSettingsContainer.ChangeBlendingStrength(base.BlendingStrength);
	}

	public void DecreaseBlendingStrength()
	{
		base.BlendingStrength -= 0.05f;
		Register.Gui.LineSettingsContainer.ChangeBlendingStrength(base.BlendingStrength);
	}

	public void IncreaseHardness()
	{
		base.Hardness += 0.05f;
		Register.Gui.LineSettingsContainer.ChangeHardness(base.Hardness);
	}

	public void DecreaseHardness()
	{
		base.Hardness -= 0.05f;
		Register.Gui.LineSettingsContainer.ChangeHardness(base.Hardness);
	}
}
