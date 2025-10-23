using System;
using Godot;

public class DrawingToolCircle : DrawingTool
{
	public enum ModeEnum
	{
		OUTLINED,
		FILLED
	}

	public enum GradientModeEnum
	{
		NONE,
		LINEAR,
		LINEAR_SQUARE,
		COSINUS,
		COSINUS_SQUARE
	}

	private ModeEnum mode;

	private GradientModeEnum gradientMode;

	public ModeEnum Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
		}
	}

	public GradientModeEnum GradientMode
	{
		get
		{
			return gradientMode;
		}
		set
		{
			gradientMode = value;
		}
	}

	public DrawingToolCircle()
	{
		drawingManager = Register.DrawingManager;
		drawingPreviewManager = Register.DrawingPreviewManager;
		workspace = Register.Workspace;
		areaTool = true;
		name = "Circle";
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_CIRCLE, "CIRCLE_SIZE_INCREASE", IncreaseSize, ButtonList.WheelUp, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_CIRCLE, "CIRCLE_SIZE_DECREASE", DecreaseSize, ButtonList.WheelDown, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_CIRCLE, "CIRCLE_BLENDING_INCREASE", IncreaseBlendingStrength, ButtonList.WheelUp, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: false, alt: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_CIRCLE, "CIRCLE_BLENDING_DECREASE", DecreaseBlendingStrength, ButtonList.WheelDown, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: false, alt: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_CIRCLE, "CIRCLE_GRADIENT", ChangeGradient, KeyList.E, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: false, alt: true);
	}

	private float CalculateBlendingStrength(float x, float y)
	{
		x += 0.5f;
		y += 0.5f;
		float num = Mathf.Abs(x - halfSize);
		float deltaY = Mathf.Abs(y - halfSize);
		float distance = Mathf.Sqrt(num * num + deltaY * deltaY);
		if (distance > halfSize)
		{
			return 0f;
		}
		if (distance <= hardness * halfSize)
		{
			return 1f;
		}
		return 0.5f - 0.5f * Mathf.Cos((1f - distance / halfSize) / (1f - hardness) * Mathf.Pi);
	}

	private void DrawLine(int startX, int startY, int endX, int endY)
	{
		Vector2i start = new Vector2i(startX, startY);
		Vector2i end = new Vector2i(endX, endY);
		Vector2 delta = end.ToFloat() - start.ToFloat();
		Vector2 normal = new Vector2(delta.y, 0f - delta.x).Normalized();
		Vector2i lefttop = new Vector2i(Math.Min(start.x, end.x) - size / 2, Math.Min(start.y, end.y) - size / 2);
		Vector2i rightbottom = new Vector2i(Math.Max(start.x, end.x) + size / 2, Math.Max(start.y, end.y) + size / 2);
		float shift = 0f;
		if ((size & 1) == 0)
		{
			shift = -0.5f;
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
	}

	public override void DrawPreview(Vector2i position)
	{
		if (drawingManager.IsDrawing)
		{
			return;
		}
		drawingPreviewManager.ClearArea();
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
		if (Input.IsKeyPressed(16777238))
		{
			int originalWidth = end.x - start.x;
			int originalHeight = end.y - start.y;
			int size = Mathf.Abs((Mathf.Abs(originalWidth) >= Mathf.Abs(originalHeight)) ? originalWidth : originalHeight);
			if (originalWidth >= 0)
			{
				end.x = start.x + size;
			}
			else
			{
				end.x = start.x - size;
			}
			if (originalHeight >= 0)
			{
				end.y = start.y + size;
			}
			else
			{
				end.y = start.y - size;
			}
		}
		if (end.x < start.x)
		{
			ref int x = ref start.x;
			ref int x2 = ref end.x;
			int x3 = end.x;
			int x4 = start.x;
			x = x3;
			x2 = x4;
		}
		if (end.y < start.y)
		{
			ref int x = ref start.y;
			ref int y = ref end.y;
			int x4 = end.y;
			int x3 = start.y;
			x = x4;
			y = x3;
		}
		Vector2i position = default(Vector2i);
		if (mode == ModeEnum.OUTLINED || gradientMode == GradientModeEnum.NONE)
		{
			if (start.x != end.x && start.y != end.y)
			{
				int width = end.x - start.x + 1;
				int height = end.y - start.y + 1;
				int cx = start.x + width / 2;
				int cy = start.y + height / 2;
				float rx = (float)width / 2f - 0.5f;
				float ry = (float)height / 2f - 0.5f;
				if (Input.IsKeyPressed(16777238) || (start.x != end.x && start.y != end.y))
				{
					if (width <= 2 || height <= 2)
					{
						for (int iy = start.y; iy < start.y + height; iy++)
						{
							for (int ix = start.x; ix < start.x + width; ix++)
							{
								DrawSpot(position.Set(ix, iy));
							}
						}
					}
					else
					{
						float evenXf = 0f;
						float evenYf = 0f;
						int evenXi = 0;
						int evenYi = 0;
						if ((width & 1) == 0)
						{
							evenXf = 0.5f;
							evenXi = 1;
						}
						if ((height & 1) == 0)
						{
							evenYf = 0.5f;
							evenYi = 1;
						}
						float shift = 0f;
						if ((base.size & 1) == 0)
						{
							shift = -0.5f;
						}
						for (int i = 0; i <= (width - 1) / 2; i++)
						{
							int iy2 = Mathf.RoundToInt(shift + Mathf.Sqrt(Mathf.Abs(1f - ((float)i + evenXf) * ((float)i + evenXf) / (rx * rx))) * ry - evenYf);
							if (evenYi > 0 || iy2 > 0)
							{
								DrawSpot(position.Set(cx + i, cy - iy2 - evenYi));
							}
							DrawSpot(position.Set(cx + i, cy + iy2));
							if (evenXi > 0 || i > 0)
							{
								if (evenYi > 0 || iy2 > 0)
								{
									DrawSpot(position.Set(cx - i - evenXi, cy - iy2 - evenYi));
								}
								DrawSpot(position.Set(cx - i - evenXi, cy + iy2));
							}
						}
						for (int j = 0; j <= (height - 1) / 2; j++)
						{
							int ix2 = Mathf.RoundToInt(shift + Mathf.Sqrt(Mathf.Abs(1f - ((float)j + evenYf) * ((float)j + evenYf) / (ry * ry))) * rx - evenXf);
							if (Mathf.RoundToInt(shift + Mathf.Sqrt(Mathf.Abs(1f - ((float)ix2 + evenXf) * ((float)ix2 + evenXf) / (rx * rx))) * ry - evenYf) != j)
							{
								DrawSpot(position.Set(cx + ix2, cy - j - evenYi));
								DrawSpot(position.Set(cx + ix2, cy + j));
								if (evenXi > 0 || ix2 > 0)
								{
									DrawSpot(position.Set(cx - ix2 - evenXi, cy - j - evenYi));
									DrawSpot(position.Set(cx - ix2 - evenXi, cy + j));
								}
							}
						}
					}
				}
				else if (!Input.IsKeyPressed(16777238))
				{
					DrawLine(start.x, start.y, end.x, end.y);
				}
			}
			else
			{
				DrawLine(start.x, start.y, end.x, end.y);
			}
		}
		if (mode == ModeEnum.FILLED)
		{
			drawingPreviewManager.DrawCircle(start, end, clear: false, blendingStrength, (int)gradientMode);
		}
	}

	public override void WhileDrawing(Vector2i position)
	{
		drawingEnd = position;
		if (base.HasDrawingPositionChanged)
		{
			if (base.HasDrawingPositionChangedSinceLastFrame)
			{
				drawingManager.DrawingPreviewManager.ClearArea();
				DrawStroke(drawingStart, drawingEnd);
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
		mode = ModeEnum.OUTLINED;
		gradientMode = GradientModeEnum.NONE;
	}

	public void ChangeGradient()
	{
		int gradientMode = (int)GradientMode;
		gradientMode++;
		if (gradientMode > 4)
		{
			gradientMode = 0;
		}
		GradientMode = (GradientModeEnum)gradientMode;
		drawingManager.DrawingPreviewManager.ClearArea();
		DrawStroke(drawingStart, drawingEnd);
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
