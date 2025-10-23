using System;
using System.Collections.Generic;
using Godot;

public class DrawingToolSmearing : DrawingTool
{
	public enum ShapeEnum
	{
		CIRCULAR,
		SQUARE
	}

	public enum ModeEnum
	{
		DEFAULT,
		SHARP
	}

	private struct PathNode
	{
		public Vector2i Position;

		public float Distance;

		public int x
		{
			get
			{
				return Position.x;
			}
			set
			{
				Position.x = value;
			}
		}

		public int y
		{
			get
			{
				return Position.y;
			}
			set
			{
				Position.y = value;
			}
		}

		public PathNode(int x, int y, float distance)
		{
			Position.x = x;
			Position.y = y;
			Distance = distance;
		}

		public PathNode(Vector2i position, float distance)
		{
			Position = position;
			Distance = distance;
		}
	}

	private ShapeEnum shape;

	private ModeEnum mode;

	private int triangleLastFrame = -1;

	private Vector2i previewspacecDrawingPositionLastFrame = Vector2i.Zero;

	private bool doFading;

	private bool doFadingSize = true;

	private float fadingPathLength = 60f;

	private float fadingFalloff = 0.35f;

	private float[] fadingFalloffStart = new float[3];

	private float[] fadingFalloffLength = new float[3];

	private List<PathNode> pathNodesList = new List<PathNode>();

	private int blurDistance = 1;

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

	public bool DoFading
	{
		get
		{
			return doFading;
		}
		set
		{
			doFading = value;
		}
	}

	public bool DoFadingSize
	{
		get
		{
			return doFadingSize;
		}
		set
		{
			doFadingSize = value;
		}
	}

	public float FadingPathLength
	{
		get
		{
			return fadingPathLength;
		}
		set
		{
			fadingPathLength = value;
		}
	}

	public float FadingFalloff
	{
		get
		{
			return fadingFalloff;
		}
		set
		{
			fadingFalloff = Mathf.Clamp(value, 0f, 0.5f);
		}
	}

	public int BlurDistance
	{
		get
		{
			return blurDistance;
		}
		set
		{
			blurDistance = Mathf.Clamp(value, 1, 8);
		}
	}

	public DrawingToolSmearing()
	{
		drawingManager = Register.DrawingManager;
		drawingPreviewManager = Register.DrawingPreviewManager;
		workspace = Register.Workspace;
		name = "Smearing";
		circleMirroring = true;
		hardness = 0f;
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SMEARING, "SMEARING_SIZE_INCREASE", IncreaseSize, ButtonList.WheelUp, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SMEARING, "SMEARING_SIZE_DECREASE", DecreaseSize, ButtonList.WheelDown, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SMEARING, "SMEARING_BLENDING_INCREASE", IncreaseBlendingStrength, ButtonList.WheelUp, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: false, alt: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SMEARING, "SMEARING_BLENDING_DECREASE", DecreaseBlendingStrength, ButtonList.WheelDown, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: false, alt: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SMEARING, "SMEARING_SHAPE", ChangeShape, KeyList.B, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_SMEARING, "SMEARING_FALLOFF", ChangeFalloff, KeyList.B, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: false, alt: true);
	}

	private float CalculateBlendingStrengthSimple(float x, float y)
	{
		x += 0.5f;
		y += 0.5f;
		float halfSize = base.halfSize;
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
		if (mode == ModeEnum.DEFAULT)
		{
			return blending;
		}
		return blending * blending;
	}

	public override void DrawPreview(Vector2i position)
	{
		drawingPreviewManager.ChangeColor(Settings.AccentColor);
		drawingPreviewManager.ChangeRoughness(Value.White);
		drawingPreviewManager.ChangeMetallicity(Value.Black);
		drawingPreviewManager.ChangeHeight(new Value(0.5f));
		drawingPreviewManager.ChangeEmission(Settings.BlackColor);
		if (drawingManager.IsDrawing)
		{
			return;
		}
		drawingPreviewManager.ClearArea();
		if (drawingManager.Mirroring && drawingManager.CircleMirroring)
		{
			drawingManager.DrawCircleMirroredSpot(CalculateBlendingStrengthSimple, position.x, position.y, size, blendingStrength);
			return;
		}
		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				float falloffBlendingStrength = CalculateBlendingStrengthSimple(x, y);
				if (falloffBlendingStrength != 0f)
				{
					drawingPreviewManager.DrawPixel(position.x + x - Mathf.CeilToInt((float)size * 0.5f) + 1, position.y + y - Mathf.CeilToInt((float)size * 0.5f) + 1, clear: false, falloffBlendingStrength * blendingStrength);
				}
			}
		}
	}

	public override void StartDrawing(Vector2i position)
	{
		base.StartDrawing(position);
		if (doFading)
		{
			fadingFalloffLength[0] = fadingPathLength * fadingFalloff;
			fadingFalloffLength[2] = fadingPathLength * fadingFalloff;
			fadingFalloffLength[1] = fadingPathLength - (fadingFalloffLength[0] + fadingFalloffLength[2]);
			fadingFalloffStart[0] = 0f;
			fadingFalloffStart[1] = fadingFalloffStart[0] + fadingFalloffLength[0];
			fadingFalloffStart[2] = fadingFalloffStart[1] + fadingFalloffLength[1];
			pathNodesList.Clear();
			pathNodesList.Add(new PathNode(position, 0f));
		}
		if (InputManager.CursorSpace == CameraManager.SpaceEnum.PREVIEWSPACE)
		{
			if (Register.CollisionManager.CollisionsList.Count > 0)
			{
				previewspacecDrawingPositionLastFrame = new Vector2i(Mathf.FloorToInt(Register.CollisionManager.CollisionsList[0].UV.x * (float)workspace.Worksheet.Data.Width), Mathf.FloorToInt(Register.CollisionManager.CollisionsList[0].UV.y * (float)workspace.Worksheet.Data.Height));
			}
			triangleLastFrame = -1;
		}
	}

	public override void DrawSpot(Vector2i position)
	{
		if (drawingManager.Mirroring && drawingManager.CircleMirroring)
		{
			drawingManager.DrawCircleMirroredSpot(CalculateBlendingStrengthSimple, position.x, position.y, size, blendingStrength);
			return;
		}
		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				float falloffBlendingStrength = CalculateBlendingStrengthSimple(x, y);
				if (falloffBlendingStrength != 0f)
				{
					drawingPreviewManager.DrawPixel(position.x + x - Mathf.CeilToInt((float)size * 0.5f) + 1, position.y + y - Mathf.CeilToInt((float)size * 0.5f) + 1, clear: false, falloffBlendingStrength * blendingStrength);
				}
			}
		}
	}

	public void DrawSpot(Vector2i position, float blendingModifier)
	{
		if (drawingManager.Mirroring && drawingManager.CircleMirroring)
		{
			drawingManager.DrawCircleMirroredSpot(CalculateBlendingStrengthSimple, position.x, position.y, size, blendingStrength);
			return;
		}
		int currentSize = base.Size;
		if (doFadingSize)
		{
			base.Size = Mathf.RoundToInt((float)currentSize * blendingModifier);
		}
		for (int y = 0; y < size; y++)
		{
			for (int x = 0; x < size; x++)
			{
				float falloffBlendingStrength = CalculateBlendingStrengthSimple(x, y);
				if (falloffBlendingStrength != 0f)
				{
					drawingPreviewManager.DrawPixel(position.x + x - Mathf.CeilToInt((float)size * 0.5f) + 1, position.y + y - Mathf.CeilToInt((float)size * 0.5f) + 1, clear: false, falloffBlendingStrength * blendingStrength * blendingModifier);
				}
			}
		}
		base.Size = currentSize;
	}

	private void DrawLine(Vector2i start, Vector2i end)
	{
		float pathModifier = 1f;
		float pathSizeModifier = 1f;
		if (drawingManager.Mirroring && drawingManager.CircleMirroring)
		{
			drawingManager.DrawCircleMirroredStroke(CalculateBlendingStrengthSimple, start.x, start.y, end.x, end.y, size, blendingStrength);
			return;
		}
		Vector2 delta = end.ToFloat() - start.ToFloat();
		Vector2 deltaNormalized = delta.Normalized();
		Vector2 normal = new Vector2(delta.y, 0f - delta.x).Normalized();
		Vector2i lefttop = new Vector2i(Math.Min(start.x, end.x) - size / 2, Math.Min(start.y, end.y) - size / 2);
		Vector2i rightbottom = new Vector2i(Math.Max(start.x, end.x) + size / 2, Math.Max(start.y, end.y) + size / 2);
		if (!doFading)
		{
			DrawSpot(start);
		}
		for (int y = lefttop.y; y <= rightbottom.y; y++)
		{
			for (int x = lefttop.x; x <= rightbottom.x; x++)
			{
				float t = ((float)(x - start.x) * delta.x + (float)(y - start.y) * delta.y) / delta.Dot(delta);
				if (0f <= t && t <= 1f)
				{
					if (doFading)
					{
						float nodeDistance = pathNodesList[pathNodesList.Count - 2].Distance + Mathf.Abs((float)(x - start.x) * deltaNormalized.x + (float)(y - start.y) * deltaNormalized.y);
						pathModifier = ((nodeDistance > fadingFalloffStart[2]) ? Mathf.Clamp((fadingFalloffLength[2] - (nodeDistance - fadingFalloffStart[2])) / fadingFalloffLength[2], 0f, 1f) : ((!(nodeDistance > fadingFalloffStart[1])) ? Mathf.Clamp(nodeDistance / fadingFalloffLength[0], 0f, 1f) : 1f));
					}
					if (doFadingSize)
					{
						pathSizeModifier = pathModifier;
					}
					float distance = Mathf.Abs((float)(x - start.x) * normal.x + (float)(y - start.y) * normal.y);
					if (distance <= hardness * halfSize * pathSizeModifier)
					{
						float falloffBlendingStrength = 1f;
						drawingPreviewManager.DrawPixel(x, y, clear: false, falloffBlendingStrength * blendingStrength * pathModifier);
					}
					else if (distance <= halfSize * pathSizeModifier)
					{
						float falloffBlendingStrength = 0.5f - 0.5f * Mathf.Cos((1f - distance / (halfSize * pathSizeModifier)) / (1f - hardness) * Mathf.Pi);
						drawingPreviewManager.DrawPixel(x, y, clear: false, falloffBlendingStrength * blendingStrength * pathModifier);
					}
				}
			}
		}
		if (!doFading)
		{
			DrawSpot(end);
		}
		else
		{
			float nodeDistance2 = pathNodesList[pathNodesList.Count - 2].Distance;
			DrawSpot(blendingModifier: (nodeDistance2 > fadingFalloffStart[2]) ? Mathf.Clamp((fadingFalloffLength[2] - (nodeDistance2 - fadingFalloffStart[2])) / fadingFalloffLength[2], 0f, 1f) : ((!(nodeDistance2 > fadingFalloffStart[1])) ? Mathf.Clamp(nodeDistance2 / fadingFalloffLength[0], 0f, 1f) : 1f), position: start);
		}
	}

	public override void DrawStroke(Vector2i start, Vector2i end)
	{
		switch (InputManager.CursorSpace)
		{
		case CameraManager.SpaceEnum.WORKSPACE:
			DrawLine(start, end);
			break;
		case CameraManager.SpaceEnum.PREVIEWSPACE:
		{
			BakeManager.PixelAffiliation[,] pixelAffiliations = Register.BakeManager.PixelAffiliations;
			foreach (CollisionManager.CollisionData cd in Register.CollisionManager.CollisionsList)
			{
				Vector2i drawingPosition = new Vector2i(Mathf.FloorToInt(cd.UV.x * (float)workspace.Worksheet.Data.Width), Mathf.FloorToInt(cd.UV.y * (float)workspace.Worksheet.Data.Height));
				bool edgeJump = false;
				if (drawingPosition.x < 0 || drawingPosition.x >= workspace.Worksheet.Data.Width || drawingPosition.y < 0 || drawingPosition.y >= workspace.Worksheet.Data.Height)
				{
					edgeJump = true;
				}
				drawingPosition.x = Mathf.PosMod(drawingPosition.x, workspace.Worksheet.Data.Width);
				drawingPosition.y = Mathf.PosMod(drawingPosition.y, workspace.Worksheet.Data.Height);
				bool doLine;
				if (pixelAffiliations != null)
				{
					if (pixelAffiliations[drawingPosition.x, drawingPosition.y].TrianglesIndicesList != null && pixelAffiliations[drawingPosition.x, drawingPosition.y].TrianglesIndicesList.Count > 0)
					{
						doLine = ((pixelAffiliations[drawingPosition.x, drawingPosition.y].TrianglesIndicesList[0] == triangleLastFrame) ? true : false);
						triangleLastFrame = pixelAffiliations[drawingPosition.x, drawingPosition.y].TrianglesIndicesList[0];
					}
					else
					{
						doLine = false;
						triangleLastFrame = -1;
					}
				}
				else
				{
					doLine = false;
				}
				if (edgeJump)
				{
					doLine = false;
					triangleLastFrame = -1;
				}
				if (drawingPosition != previewspacecDrawingPositionLastFrame)
				{
					if (doLine)
					{
						DrawLine(new Vector2i(drawingPosition.x, drawingPosition.y), new Vector2i(previewspacecDrawingPositionLastFrame.x, previewspacecDrawingPositionLastFrame.y));
					}
					else if (!doFading)
					{
						DrawSpot(new Vector2i(drawingPosition.x, drawingPosition.y));
					}
				}
				previewspacecDrawingPositionLastFrame = drawingPosition;
			}
			Register.CollisionManager.CollisionsList.Clear();
			break;
		}
		}
	}

	public override void WhileDrawing(Vector2i position)
	{
		drawingEnd = position;
		if (base.HasDrawingPositionChanged)
		{
			if (doFading)
			{
				pathNodesList.Add(new PathNode(position, pathNodesList[pathNodesList.Count - 1].Distance + pathNodesList[pathNodesList.Count - 1].Position.DistanceTo(position)));
			}
			DrawStroke(drawingStart, drawingEnd);
		}
		else if (!doFading && drawingEnd != drawingPositionLastFrame)
		{
			DrawSpot(drawingEnd);
		}
		drawingPositionLastFrame = (drawingStart = drawingEnd);
	}

	public override void StopDrawing(Vector2i position)
	{
		drawingManager.PushSettings();
		drawingManager.BlendingMode = Blender.BlendingModeEnum.ALPHABLEND;
		Color[,] colorArray = null;
		if (drawingManager.ColorEnabled)
		{
			colorArray = BlurFilter.ColorChannel(workspace.Worksheet.Data.Layer.ColorChannel, drawingPreviewManager.BlendingMaskUpdateStart.x, drawingPreviewManager.BlendingMaskUpdateStart.y, drawingPreviewManager.BlendingMaskUpdateEnd.x, drawingPreviewManager.BlendingMaskUpdateEnd.y, workspace.Worksheet.Data.Tileable, blurDistance);
		}
		Value[,] roughnessArray = null;
		if (drawingManager.RoughnessEnabled)
		{
			roughnessArray = BlurFilter.ValueChannel(workspace.Worksheet.Data.Layer.RoughnessChannel, drawingPreviewManager.BlendingMaskUpdateStart.x, drawingPreviewManager.BlendingMaskUpdateStart.y, drawingPreviewManager.BlendingMaskUpdateEnd.x, drawingPreviewManager.BlendingMaskUpdateEnd.y, workspace.Worksheet.Data.Tileable, blurDistance);
		}
		Value[,] metallicityArray = null;
		if (drawingManager.MetallicityEnabled)
		{
			metallicityArray = BlurFilter.ValueChannel(workspace.Worksheet.Data.Layer.MetallicityChannel, drawingPreviewManager.BlendingMaskUpdateStart.x, drawingPreviewManager.BlendingMaskUpdateStart.y, drawingPreviewManager.BlendingMaskUpdateEnd.x, drawingPreviewManager.BlendingMaskUpdateEnd.y, workspace.Worksheet.Data.Tileable, blurDistance);
		}
		Value[,] heightArray = null;
		if (drawingManager.HeightEnabled)
		{
			heightArray = BlurFilter.ValueChannel(workspace.Worksheet.Data.Layer.HeightChannel, drawingPreviewManager.BlendingMaskUpdateStart.x, drawingPreviewManager.BlendingMaskUpdateStart.y, drawingPreviewManager.BlendingMaskUpdateEnd.x, drawingPreviewManager.BlendingMaskUpdateEnd.y, workspace.Worksheet.Data.Tileable, blurDistance);
		}
		Color[,] emissionArray = null;
		if (drawingManager.EmissionEnabled)
		{
			emissionArray = BlurFilter.ColorChannel(workspace.Worksheet.Data.Layer.EmissionChannel, drawingPreviewManager.BlendingMaskUpdateStart.x, drawingPreviewManager.BlendingMaskUpdateStart.y, drawingPreviewManager.BlendingMaskUpdateEnd.x, drawingPreviewManager.BlendingMaskUpdateEnd.y, workspace.Worksheet.Data.Tileable, blurDistance);
		}
		drawingEnd = position;
		for (int y = drawingPreviewManager.BlendingMaskUpdateStart.y; y <= drawingPreviewManager.BlendingMaskUpdateEnd.y; y++)
		{
			for (int x = drawingPreviewManager.BlendingMaskUpdateStart.x; x <= drawingPreviewManager.BlendingMaskUpdateEnd.x; x++)
			{
				if (drawingPreviewManager.BlendingMaskChannelArray[x, y] > 0f)
				{
					if (drawingManager.ColorEnabled)
					{
						drawingManager.Color = colorArray[x, y];
					}
					if (drawingManager.RoughnessEnabled)
					{
						drawingManager.Roughness = roughnessArray[x, y];
					}
					if (drawingManager.MetallicityEnabled)
					{
						drawingManager.Metallicity = metallicityArray[x, y];
					}
					if (drawingManager.HeightEnabled)
					{
						drawingManager.Height = heightArray[x, y];
					}
					if (drawingManager.EmissionEnabled)
					{
						drawingManager.Emission = emissionArray[x, y];
					}
					drawingManager.DrawPixel(workspace.Worksheet, x, y, drawingPreviewManager.BlendingMaskChannelArray[x, y]);
				}
			}
		}
		drawingPreviewManager.ClearArea();
		drawingPreviewManager.Update();
		drawingManager.PopSettings();
	}

	public override void Reset()
	{
		base.Reset();
		shape = ShapeEnum.CIRCULAR;
		mode = ModeEnum.DEFAULT;
		doFading = false;
		doFadingSize = true;
		fadingFalloff = 0.35f;
		fadingPathLength = 48f;
		blurDistance = 1;
	}

	public void ChangeShape()
	{
		if (shape == ShapeEnum.SQUARE)
		{
			shape = ShapeEnum.CIRCULAR;
			Register.Gui.BrushSettingsContainer.PressShapeCircle();
		}
		else
		{
			shape = ShapeEnum.SQUARE;
			Register.Gui.BrushSettingsContainer.PressShapeSquare();
		}
	}

	public void ChangeFalloff()
	{
		if (mode == ModeEnum.DEFAULT)
		{
			mode = ModeEnum.SHARP;
		}
		else if (mode == ModeEnum.SHARP)
		{
			mode = ModeEnum.DEFAULT;
		}
	}

	public void IncreaseSize()
	{
		base.Size++;
		Register.Gui.BrushSettingsContainer.ChangeSize(base.Size);
	}

	public void DecreaseSize()
	{
		base.Size--;
		Register.Gui.BrushSettingsContainer.ChangeSize(base.Size);
	}

	public void IncreaseBlendingStrength()
	{
		base.BlendingStrength += 0.05f;
		Register.Gui.BrushSettingsContainer.ChangeBlendingStrength(base.BlendingStrength);
	}

	public void DecreaseBlendingStrength()
	{
		base.BlendingStrength -= 0.05f;
		Register.Gui.BrushSettingsContainer.ChangeBlendingStrength(base.BlendingStrength);
	}

	public void IncreaseHardness()
	{
		base.Hardness += 0.05f;
		Register.Gui.BrushSettingsContainer.ChangeHardness(base.Hardness);
	}

	public void DecreaseHardness()
	{
		base.Hardness -= 0.05f;
		Register.Gui.BrushSettingsContainer.ChangeHardness(base.Hardness);
	}

	public void IncreaseBlurDistance()
	{
		BlurDistance++;
	}

	public void DecreaseBlurDistance()
	{
		BlurDistance--;
	}
}
