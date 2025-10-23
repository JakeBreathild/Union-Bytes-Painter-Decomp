using System;
using Godot;

public class DrawingPreviewManager : Node
{
	private Workspace workspace;

	private DrawingManager drawingManager;

	private float maxBlendingStrength = 1f;

	private bool isEmpty = true;

	private ChannelArray<float> blendingMaskChannelArray;

	private Image blendingMaskImage;

	private ImageTexture blendingMaskImageTexture;

	private const int blendingMaskUpdatesPerSecond = 40;

	private readonly float blendingMaskUpdateDelay = 0.025f;

	private float blendingMaskUpdateTimer;

	private bool doBlendingMaskUpdate;

	private Vector2i blendingMaskUpdateStart = Vector2i.Maximum;

	private Vector2i blendingMaskUpdateEnd = Vector2i.Minimum;

	private Vector2i blendingMaskUpdateLastStart = Vector2i.Maximum;

	private Vector2i blendingMaskUpdateLastEnd = Vector2i.Minimum;

	public float MaxBlendingStrength
	{
		get
		{
			return maxBlendingStrength;
		}
		set
		{
			maxBlendingStrength = value;
		}
	}

	public bool IsEmpty => isEmpty;

	public ChannelArray<float> BlendingMaskChannelArray => blendingMaskChannelArray;

	public ImageTexture BlendingMaskImageTexture => blendingMaskImageTexture;

	public Vector2i BlendingMaskUpdateStart => blendingMaskUpdateStart;

	public Vector2i BlendingMaskUpdateEnd => blendingMaskUpdateEnd;

	public Vector2i BlendingMaskUpdateLastStart => blendingMaskUpdateLastStart;

	public Vector2i BlendingMaskUpdateLastEnd => blendingMaskUpdateLastEnd;

	public DrawingPreviewManager()
	{
		Register.DrawingPreviewManager = this;
	}

	public DrawingPreviewManager(Workspace workspace)
	{
		Register.DrawingPreviewManager = this;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "DrawingPreviewManager";
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		drawingManager = Register.DrawingManager;
	}

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);
		blendingMaskUpdateTimer += delta;
		if (doBlendingMaskUpdate && blendingMaskUpdateTimer > blendingMaskUpdateDelay)
		{
			Update();
			doBlendingMaskUpdate = false;
			blendingMaskUpdateTimer = 0f;
		}
	}

	public void Reset()
	{
		maxBlendingStrength = 1f;
		blendingMaskChannelArray = new ChannelArray<float>(workspace.Worksheet.Data.Width, workspace.Worksheet.Data.Height, 0f);
		blendingMaskImage = new Image();
		blendingMaskImage.Create(blendingMaskChannelArray.Width, blendingMaskChannelArray.Height, useMipmaps: false, Image.Format.R8);
		blendingMaskImageTexture = new ImageTexture();
		blendingMaskImageTexture.CreateFromImage(blendingMaskImage, 18u);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "previewMaskTexture", blendingMaskImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, "previewMaskTexture", blendingMaskImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "previewMaskTexture", blendingMaskImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "previewMaskTexture", blendingMaskImageTexture);
		doBlendingMaskUpdate = false;
		blendingMaskUpdateTimer = 0f;
		blendingMaskUpdateStart = Vector2i.Maximum;
		blendingMaskUpdateEnd = Vector2i.Minimum;
	}

	public void UpdatePosition(int x, int y)
	{
		doBlendingMaskUpdate = true;
		blendingMaskUpdateStart.SetMin(x, y);
		blendingMaskUpdateEnd.SetMax(x, y);
	}

	public void UpdateArea(Vector2i start, Vector2i end)
	{
		doBlendingMaskUpdate = true;
		blendingMaskUpdateStart = start;
		blendingMaskUpdateEnd = end;
	}

	public void Update()
	{
		Color color = new Color(0f, 0f, 0f, 0f);
		blendingMaskUpdateStart.SetMax(0, 0);
		blendingMaskUpdateEnd.SetMin(blendingMaskChannelArray.Width - 1, blendingMaskChannelArray.Height - 1);
		blendingMaskImage.Lock();
		for (int y = blendingMaskUpdateStart.y; y <= blendingMaskUpdateEnd.y; y++)
		{
			for (int x = blendingMaskUpdateStart.x; x <= blendingMaskUpdateEnd.x; x++)
			{
				color.r = blendingMaskChannelArray[x, y];
				blendingMaskImage.SetPixel(x, y, color);
			}
		}
		blendingMaskImage.Unlock();
		VisualServer.TextureSetData(blendingMaskImageTexture.GetRid(), blendingMaskImage);
		color.r = 0f;
		blendingMaskImage.Lock();
		for (int i = blendingMaskUpdateStart.y; i <= blendingMaskUpdateEnd.y; i++)
		{
			for (int j = blendingMaskUpdateStart.x; j <= blendingMaskUpdateEnd.x; j++)
			{
				blendingMaskImage.SetPixel(j, i, color);
			}
		}
		blendingMaskImage.Unlock();
	}

	public void ClearArea()
	{
		blendingMaskChannelArray.Clear(blendingMaskUpdateStart, blendingMaskUpdateEnd);
		blendingMaskUpdateLastStart = blendingMaskUpdateStart;
		blendingMaskUpdateLastEnd = blendingMaskUpdateEnd;
		blendingMaskUpdateStart = Vector2i.Maximum;
		blendingMaskUpdateEnd = Vector2i.Minimum;
	}

	public void ClearLastFrameArea()
	{
		blendingMaskChannelArray.Clear(blendingMaskUpdateLastStart, blendingMaskUpdateLastEnd);
	}

	public void Clear()
	{
		blendingMaskChannelArray.Clear(new Vector2i(0, 0), new Vector2i(workspace.Worksheet.Data.ColorChannel.Width - 1, workspace.Worksheet.Data.ColorChannel.Height - 1));
		blendingMaskUpdateLastStart = blendingMaskUpdateStart;
		blendingMaskUpdateLastEnd = blendingMaskUpdateEnd;
		blendingMaskUpdateStart = Vector2i.Maximum;
		blendingMaskUpdateEnd = Vector2i.Minimum;
		isEmpty = true;
	}

	public bool IsPixelPerfect(int x, int y)
	{
		return false;
	}

	private int DrawVerticalMirrorPixel(int x, int y, float blendingStrength = 1f)
	{
		int newX = Mathf.RoundToInt(drawingManager.VerticalMirrorPosition - ((float)x - drawingManager.VerticalMirrorPosition)) - 1;
		if (!workspace.Worksheet.Data.Tileable && (newX < 0 || newX >= blendingMaskChannelArray.Width))
		{
			return x;
		}
		newX = Mathf.PosMod(newX, blendingMaskChannelArray.Width);
		if (!Register.SelectionManager.Enabled || workspace.Worksheet.Data.SelectionArray[newX, y])
		{
			blendingMaskChannelArray[newX, y] = Mathf.Clamp(Mathf.Max(blendingMaskChannelArray[newX, y], blendingStrength), 0f, maxBlendingStrength);
			UpdatePosition(newX, y);
		}
		return newX;
	}

	private int DrawHorizontalMirrorPixel(int x, int y, float blendingStrength = 1f)
	{
		int newY = Mathf.RoundToInt(drawingManager.HorizontalMirrorPosition - ((float)y - drawingManager.HorizontalMirrorPosition)) - 1;
		if (!workspace.Worksheet.Data.Tileable && (newY < 0 || newY >= blendingMaskChannelArray.Height))
		{
			return y;
		}
		newY = Mathf.PosMod(newY, blendingMaskChannelArray.Height);
		if (!Register.SelectionManager.Enabled || workspace.Worksheet.Data.SelectionArray[x, newY])
		{
			blendingMaskChannelArray[x, newY] = Mathf.Clamp(Mathf.Max(blendingMaskChannelArray[x, newY], blendingStrength), 0f, maxBlendingStrength);
			UpdatePosition(x, newY);
		}
		return newY;
	}

	private void DrawCircleMirrorPixel(int x, int y, float blendingStrength = 1f)
	{
		float deltaX = (float)x - drawingManager.CircleVerticalMirrorPosition;
		float deltaY = (float)y - drawingManager.CircleHorizontalMirrorPosition;
		float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
		float step = Mathf.Pi * 2f / (float)drawingManager.CircleMirroringCount;
		float offset = Mathf.Atan2(deltaY, deltaX);
		for (int i = 1; i < drawingManager.CircleMirroringCount; i++)
		{
			int newX = Mathf.RoundToInt(drawingManager.CircleVerticalMirrorPosition + Mathf.Cos(offset + step * (float)i) * distance);
			int newY = Mathf.RoundToInt(drawingManager.CircleHorizontalMirrorPosition + Mathf.Sin(offset + step * (float)i) * distance);
			if (workspace.Worksheet.Data.Tileable || (newX >= 0 && newX < blendingMaskChannelArray.Width && newY >= 0 && newY < blendingMaskChannelArray.Height))
			{
				newX = Mathf.PosMod(newX, blendingMaskChannelArray.Width);
				newY = Mathf.PosMod(newY, blendingMaskChannelArray.Height);
				if (!Register.SelectionManager.Enabled || workspace.Worksheet.Data.SelectionArray[newX, newY])
				{
					blendingMaskChannelArray[newX, newY] = Mathf.Clamp(Mathf.Max(blendingMaskChannelArray[newX, newY], blendingStrength), 0f, maxBlendingStrength);
					UpdatePosition(newX, newY);
				}
			}
		}
	}

	public void DrawPixel(int x, int y, bool clear = false, float blendingStrength = 1f)
	{
		if (clear)
		{
			ClearArea();
		}
		if (!workspace.Worksheet.Data.Tileable && (x < 0 || x >= blendingMaskChannelArray.Width || y < 0 || y >= blendingMaskChannelArray.Height))
		{
			return;
		}
		x = Mathf.PosMod(x, blendingMaskChannelArray.Width);
		y = Mathf.PosMod(y, blendingMaskChannelArray.Height);
		if (!Register.SelectionManager.Enabled || workspace.Worksheet.Data.SelectionArray[x, y])
		{
			blendingMaskChannelArray[x, y] = Mathf.Clamp(Mathf.Max(blendingMaskChannelArray[x, y], blendingStrength), 0f, maxBlendingStrength);
			UpdatePosition(x, y);
		}
		if (drawingManager.Mirroring)
		{
			if (drawingManager.VerticalMirroring)
			{
				DrawVerticalMirrorPixel(x, y, blendingStrength);
			}
			if (drawingManager.HorizontalMirroring)
			{
				int newY = DrawHorizontalMirrorPixel(x, y, blendingStrength);
				if (drawingManager.VerticalMirroring)
				{
					DrawVerticalMirrorPixel(x, newY, blendingStrength);
				}
			}
		}
		isEmpty = false;
	}

	public void DrawPixel(Vector2i position, bool clear = false, float blendingStrength = 1f)
	{
		DrawPixel(position.x, position.y, clear, blendingStrength);
	}

	public void DrawLine(int startX, int startY, int endX, int endY, bool clear = false, float blendingStrength = 1f)
	{
		if (clear)
		{
			ClearArea();
		}
		if (Mathf.Abs(endX - startX) >= Mathf.Abs(endY - startY) && Mathf.Abs(endX - startX) > 0)
		{
			float increase = 1f * (float)(endY - startY) / (float)(endX - startX);
			if (startX < endX)
			{
				for (int x = startX; x <= endX; x++)
				{
					int y = (int)((float)startY + (float)(x - startX) * increase);
					DrawPixel(x, y, clear: false, blendingStrength);
				}
			}
			else
			{
				for (int i = endX; i <= startX; i++)
				{
					int y2 = (int)((float)startY + (float)(i - startX) * increase);
					DrawPixel(i, y2, clear: false, blendingStrength);
				}
			}
		}
		else
		{
			if (Mathf.Abs(endY - startY) <= 0)
			{
				return;
			}
			float increase = 1f * (float)(endX - startX) / (float)(endY - startY);
			if (startY < endY)
			{
				for (int j = startY; j <= endY; j++)
				{
					int x2 = (int)((float)startX + (float)(j - startY) * increase);
					DrawPixel(x2, j, clear: false, blendingStrength);
				}
			}
			else
			{
				for (int k = endY; k <= startY; k++)
				{
					int x3 = (int)((float)startX + (float)(k - startY) * increase);
					DrawPixel(x3, k, clear: false, blendingStrength);
				}
			}
		}
	}

	public void DrawLine(Vector2i start, Vector2i end, bool clear = false, float blendingStrength = 1f)
	{
		DrawLine(start.x, start.y, end.x, end.y, clear, blendingStrength);
	}

	public void DrawBox(int startX, int startY, int endX, int endY, bool clear = false, float blendingStrength = 1f)
	{
		if (clear)
		{
			ClearArea();
		}
		if (endX < startX)
		{
			int num = endX;
			endX = startX;
			startX = num;
		}
		if (endY < startY)
		{
			int num2 = endY;
			endY = startY;
			startY = num2;
		}
		for (int y = startY; y <= endY; y++)
		{
			for (int x = startX; x <= endX; x++)
			{
				DrawPixel(x, y, clear: false, blendingStrength);
			}
		}
	}

	public void DrawBox(Vector2i start, Vector2i end, bool clear = false, float blendingStrength = 1f)
	{
		DrawBox(start.x, start.y, end.x, end.y, clear, blendingStrength);
	}

	public void DrawCircle(int startX, int startY, int endX, int endY, bool clear = false, float blendingStrength = 1f, int gradientMode = 0, bool gradientInverse = false)
	{
		if (clear)
		{
			ClearArea();
		}
		if (endX < startX)
		{
			int num = endX;
			endX = startX;
			startX = num;
		}
		if (endY < startY)
		{
			int num2 = endY;
			endY = startY;
			startY = num2;
		}
		int width = endX - startX + 1;
		int height = endY - startY + 1;
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				float num3 = Mathf.Abs((float)x + 0.5f - (float)width / 2f);
				float deltaY = Mathf.Abs((float)y + 0.5f - (float)height / 2f);
				float distance = Mathf.Sqrt(num3 * num3 / ((float)(width * width) * 0.25f) + deltaY * deltaY / ((float)(height * height) * 0.25f));
				if (!(distance <= 1f))
				{
					continue;
				}
				float blending = blendingStrength;
				if (!gradientInverse)
				{
					switch (gradientMode)
					{
					case 1:
						blending *= 1f - distance;
						break;
					case 2:
						blending *= 1f - distance * distance;
						break;
					case 3:
						blending *= 0.5f - 0.5f * Mathf.Cos((1f - distance) * Mathf.Pi);
						break;
					case 4:
						blending *= 0.5f - 0.5f * Mathf.Cos((1f - distance * distance) * Mathf.Pi);
						break;
					}
				}
				else
				{
					switch (gradientMode)
					{
					case 1:
						blending *= distance;
						break;
					case 2:
						blending *= distance * distance;
						break;
					case 3:
						blending *= 0.5f - 0.5f * Mathf.Cos(distance * Mathf.Pi);
						break;
					case 4:
						blending *= 0.5f - 0.5f * Mathf.Cos(distance * distance * Mathf.Pi);
						break;
					}
				}
				DrawPixel(startX + x, startY + y, clear: false, blending);
			}
		}
	}

	public void DrawCircle(Vector2i start, Vector2i end, bool clear = false, float blendingStrength = 1f, int gradientMode = 0)
	{
		DrawCircle(start.x, start.y, end.x, end.y, clear, blendingStrength, gradientMode);
	}

	public void ChangeBlendingMode(Blender.BlendingModeEnum blendingMode)
	{
		if (drawingManager.Channel == Data.ChannelEnum.FULL)
		{
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "previewBlendingMode", (int)blendingMode);
		}
		else
		{
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, "previewBlendingMode", (int)blendingMode);
		}
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "previewBlendingMode", (int)blendingMode);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "previewBlendingMode", (int)blendingMode);
	}

	public void ChangeColor(Color color)
	{
		switch (drawingManager.Channel)
		{
		case Data.ChannelEnum.FULL:
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "previewColor", color);
			break;
		case Data.ChannelEnum.COLOR:
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, "previewColor", color);
			break;
		}
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "previewColor", color);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "previewColor", color);
	}

	public void ChangeHeight(Value height)
	{
		if (drawingManager.Channel == Data.ChannelEnum.HEIGHT || drawingManager.Channel == Data.ChannelEnum.NORMAL)
		{
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, "previewColor", new Color(height.v, height.v, height.v));
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "previewColor", new Color(height.v, height.v, height.v));
		}
	}

	public void ChangeRoughness(Value roughness)
	{
		switch (drawingManager.Channel)
		{
		case Data.ChannelEnum.FULL:
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "previewRoughness", roughness.v);
			break;
		case Data.ChannelEnum.ROUGHNESS:
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, "previewColor", new Color(roughness.v, roughness.v, roughness.v));
			break;
		}
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "previewRoughness", roughness.v);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "previewColor", new Color(roughness.v, roughness.v, roughness.v));
	}

	public void ChangeMetallicity(Value metallicity)
	{
		switch (drawingManager.Channel)
		{
		case Data.ChannelEnum.FULL:
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "previewMetallicity", metallicity.v);
			break;
		case Data.ChannelEnum.METALLICITY:
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, "previewColor", new Color(metallicity.v, metallicity.v, metallicity.v));
			break;
		}
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "previewMetallicity", metallicity.v);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "previewColor", new Color(metallicity.v, metallicity.v, metallicity.v));
	}

	public void ChangeEmission(Color emission)
	{
		switch (drawingManager.Channel)
		{
		case Data.ChannelEnum.FULL:
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "previewEmission", emission);
			break;
		case Data.ChannelEnum.EMISSION:
			MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, "previewColor", emission);
			break;
		}
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "previewEmission", emission);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "previewColor", emission);
	}
}
