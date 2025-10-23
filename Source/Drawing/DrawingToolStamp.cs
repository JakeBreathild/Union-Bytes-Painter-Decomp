using System;
using System.Collections.Generic;
using Godot;

public class DrawingToolStamp : DrawingTool
{
	public enum ModeEnum
	{
		MASK,
		DECAL
	}

	private ModeEnum mode;

	private bool doRandomRotation;

	private int randomDirection;

	private int rotation;

	private float randomRotationTimer;

	private float randomRotationDelay = 1f;

	private int[] randomRotationsArray = new int[64];

	private bool doMirroring;

	private Image defaultMask;

	private bool useDecalMaskArray;

	private Mask mask;

	private List<Mask> masksList = new List<Mask>();

	private Decal decal;

	private List<Decal> decalsList = new List<Decal>();

	private bool doRandomSelectionFromList;

	private bool doHeightBlending;

	private float heightOffset;

	private float heightBlendingFactor;

	public ModeEnum Mode
	{
		get
		{
			return mode;
		}
		set
		{
			mode = value;
			switch (mode)
			{
			case ModeEnum.MASK:
				Register.MaterialContainer.EnableDecal(pressed: true, settingsEnabled: false);
				Register.MaterialContainer.EnableMaterial(pressed: true);
				Register.MaterialContainer.ChangeDecalLabel("Mask");
				Register.MaterialContainer.UpdateDecalList();
				break;
			case ModeEnum.DECAL:
				Register.MaterialContainer.EnableDecal(pressed: true, settingsEnabled: true);
				Register.MaterialContainer.EnableMaterial(pressed: false);
				Register.MaterialContainer.ChangeDecalLabel("Decal");
				Register.MaterialContainer.ChangeDecalHeightOffset(heightOffset);
				Register.MaterialContainer.EnableDecalHeightBlend(doHeightBlending);
				Register.MaterialContainer.ChangeDecalHeightFactor(heightBlendingFactor);
				Register.MaterialContainer.ToggleDecalHeightBlendSettings(enable: true);
				Register.MaterialContainer.UpdateDecalList();
				break;
			}
			Register.Gui.StampSettingsContainer.ChangeModeButtons(mode);
		}
	}

	public bool DoRandomRotation
	{
		get
		{
			return doRandomRotation;
		}
		set
		{
			doRandomRotation = value;
		}
	}

	public int Rotation
	{
		get
		{
			return rotation;
		}
		set
		{
			rotation = value;
			if (rotation > 3)
			{
				rotation = 3;
			}
			else if (rotation < 0)
			{
				rotation = 0;
			}
		}
	}

	public bool DoMirroring
	{
		get
		{
			return doMirroring;
		}
		set
		{
			doMirroring = value;
		}
	}

	public Image DefaultMask
	{
		get
		{
			return defaultMask;
		}
		set
		{
			defaultMask = value;
			SetMask(defaultMask, "Default");
		}
	}

	public bool UseDecalMaskArray
	{
		get
		{
			return useDecalMaskArray;
		}
		set
		{
			useDecalMaskArray = value;
		}
	}

	public Mask Mask => mask;

	public List<Mask> MasksList => masksList;

	public Decal Decal
	{
		get
		{
			return decal;
		}
		set
		{
			decal = value;
		}
	}

	public List<Decal> DecalsList => decalsList;

	public bool DoRandomSelectionFromList
	{
		get
		{
			return doRandomSelectionFromList;
		}
		set
		{
			doRandomSelectionFromList = value;
		}
	}

	public bool DoHeightBlending
	{
		get
		{
			return doHeightBlending;
		}
		set
		{
			doHeightBlending = value;
		}
	}

	public float HeightOffset
	{
		get
		{
			return heightOffset;
		}
		set
		{
			heightOffset = value;
		}
	}

	public float HeightBlendingFactor
	{
		get
		{
			return heightBlendingFactor;
		}
		set
		{
			heightBlendingFactor = value;
		}
	}

	public DrawingToolStamp()
	{
		drawingManager = Register.DrawingManager;
		drawingPreviewManager = Register.DrawingPreviewManager;
		workspace = Register.Workspace;
		areaTool = true;
		circleMirroring = true;
		name = "Stamp";
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_STAMP, "STAMP_MIRRORING", ChangeMirroring, KeyList.S, KeyBinding.EventTypeEnum.JUST_PRESSED);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_STAMP, "STAMP_ROTATE_LEFT", RotateLeft, ButtonList.WheelUp, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
		InputManager.AddKeyBinding(InputManager.KeyBindingsListEnum.TOOL_STAMP, "STAMP_ROTATE_RIGHT", RotateRight, ButtonList.WheelDown, KeyBinding.EventTypeEnum.JUST_PRESSED, shift: false, ctrl: true);
	}

	public void SetMask(Image image, string name)
	{
		if (image.IsEmpty())
		{
			return;
		}
		mask = new Mask();
		mask.Set(image, name);
		if (mask.IsLoaded)
		{
			if (mask.Thumbnail != null)
			{
				ImageTexture texture = new ImageTexture();
				texture.CreateFromImage(mask.Thumbnail, 0u);
				Register.MaterialContainer.ChangeDecalPreview(texture);
			}
			if (image == Resources.DefaultMaskImage)
			{
				mode = ModeEnum.MASK;
			}
			else
			{
				Mode = ModeEnum.MASK;
			}
		}
	}

	public void LoadMask(string file)
	{
		mask = new Mask();
		mask.Load(file);
		if (mask.IsLoaded)
		{
			if (mask.Thumbnail != null)
			{
				ImageTexture texture = new ImageTexture();
				texture.CreateFromImage(mask.Thumbnail, 0u);
				Register.MaterialContainer.ChangeDecalPreview(texture);
			}
			Mode = ModeEnum.MASK;
		}
	}

	public void AddMaskToList(Mask mask)
	{
		masksList.Add(mask);
	}

	public void AddMaskToList()
	{
		AddMaskToList(mask);
	}

	public void RemoveMaskFromList(int index)
	{
		masksList.RemoveAt(index);
	}

	public void RemoveMaskFromList(Mask mask)
	{
		masksList.Remove(mask);
	}

	public void RemoveMaskFromList()
	{
		masksList.Remove(mask);
	}

	public void SelectMaskFromList(int index)
	{
		if (masksList.Count > 0 && index > -1 && index < masksList.Count)
		{
			mask = masksList[index];
		}
	}

	public void SelectRandomMaskFromList()
	{
		if (masksList.Count > 0)
		{
			GD.Randomize();
			int index = (int)GD.Randi() % masksList.Count;
			SelectMaskFromList(index);
		}
	}

	public void ClearMasksList()
	{
		masksList.Clear();
	}

	public void LoadDecal(string file)
	{
		decal = new Decal();
		decal.Load(file);
		if (decal.IsLoaded)
		{
			if (decal.Thumbnail != null)
			{
				Register.MaterialContainer.ChangeDecalPreview(decal.Thumbnail);
			}
			else
			{
				Register.MaterialContainer.ChangeDecalPreview((Texture)null);
			}
			Mode = ModeEnum.DECAL;
		}
	}

	public void AddDecalToList(Decal decal)
	{
		decalsList.Add(decal);
	}

	public void AddDecalToList()
	{
		AddDecalToList(decal);
	}

	public void RemoveDecalFromList(int index)
	{
		decalsList.RemoveAt(index);
	}

	public void RemoveDecalFromList(Decal decal)
	{
		decalsList.Remove(decal);
	}

	public void RemoveDecalFromList()
	{
		decalsList.Remove(decal);
	}

	public void SelectDecalFromList(int index)
	{
		if (decalsList.Count > 0 && index > -1 && index < decalsList.Count)
		{
			decal = decalsList[index];
		}
	}

	public void SelectRandomDecalFromList()
	{
		if (decalsList.Count > 0)
		{
			GD.Randomize();
			int index = (int)GD.Randi() % decalsList.Count;
			SelectDecalFromList(index);
		}
	}

	public void ClearDecalsList()
	{
		decalsList.Clear();
	}

	public override void DrawPreview(Vector2i position)
	{
		drawingPreviewManager.ClearArea();
		if (drawingManager.Mirroring && drawingManager.CircleMirroring)
		{
			float deltaX = (float)position.x - drawingManager.CircleVerticalMirrorPosition;
			float deltaY = (float)position.y - drawingManager.CircleHorizontalMirrorPosition;
			float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
			float step = Mathf.Pi * 2f / (float)drawingManager.CircleMirroringCount;
			float offset = Mathf.Atan2(deltaY, deltaX);
			drawingPreviewManager.ClearArea();
			if (doRandomRotation)
			{
				randomRotationTimer -= Register.delta;
				if (randomRotationTimer <= 0f)
				{
					GD.Randomize();
					for (int i = 0; i < drawingManager.CircleMirroringCount; i++)
					{
						randomRotationsArray[i] = (int)(GD.Randi() % 4);
					}
					randomRotationTimer = randomRotationDelay;
				}
			}
			for (int j = 0; j < drawingManager.CircleMirroringCount; j++)
			{
				if (doRandomRotation)
				{
					randomDirection = randomRotationsArray[j];
				}
				int newX = Mathf.RoundToInt(drawingManager.CircleVerticalMirrorPosition + Mathf.Cos(offset + step * (float)j) * distance);
				int newY = Mathf.RoundToInt(drawingManager.CircleHorizontalMirrorPosition + Mathf.Sin(offset + step * (float)j) * distance);
				DrawSinglePreview(new Vector2i(newX, newY));
			}
		}
		else
		{
			DrawSinglePreview(position);
		}
	}

	public void DrawSinglePreview(Vector2i position)
	{
		Value[,] heightArray = null;
		int arraySize;
		float[,] maskArray;
		if (mode == ModeEnum.MASK)
		{
			drawingManager.DrawingPreviewManager.ChangeBlendingMode(drawingManager.BlendingMode);
			drawingManager.DrawingPreviewManager.ChangeColor(drawingManager.Color);
			drawingManager.DrawingPreviewManager.ChangeRoughness(drawingManager.Roughness);
			drawingManager.DrawingPreviewManager.ChangeMetallicity(drawingManager.Metallicity);
			drawingManager.DrawingPreviewManager.ChangeHeight(drawingManager.Height);
			drawingManager.DrawingPreviewManager.ChangeEmission(drawingManager.Emission);
			arraySize = mask.ArraySize;
			maskArray = mask.MaskArray;
		}
		else
		{
			drawingManager.DrawingPreviewManager.ChangeBlendingMode(Blender.BlendingModeEnum.NORMAL);
			drawingManager.DrawingPreviewManager.ChangeColor(Settings.AccentColor);
			drawingManager.DrawingPreviewManager.ChangeRoughness(Value.White);
			drawingManager.DrawingPreviewManager.ChangeMetallicity(Value.Black);
			drawingManager.DrawingPreviewManager.ChangeHeight(new Value(0.5f));
			drawingManager.DrawingPreviewManager.ChangeEmission(Settings.BlackColor);
			arraySize = decal.ArraySize;
			maskArray = decal.MaskArray;
			heightArray = decal.HeightArray;
		}
		int rotation = ((!doRandomRotation) ? this.rotation : randomDirection);
		for (int y = 0; y < arraySize; y++)
		{
			for (int x = 0; x < arraySize; x++)
			{
				int xx;
				int yy;
				switch (rotation)
				{
				case 1:
					xx = y;
					yy = arraySize - 1 - x;
					break;
				case 2:
					xx = arraySize - 1 - x;
					yy = arraySize - 1 - y;
					break;
				case 3:
					xx = arraySize - 1 - y;
					yy = x;
					break;
				default:
					xx = x;
					yy = y;
					break;
				}
				if (doMirroring)
				{
					switch (rotation)
					{
					case 0:
					case 2:
						xx = arraySize - 1 - xx;
						break;
					case 1:
					case 3:
						yy = arraySize - 1 - yy;
						break;
					}
				}
				float currentBlendingStrength = blendingStrength;
				if (mode == ModeEnum.MASK || useDecalMaskArray)
				{
					currentBlendingStrength *= maskArray[xx, yy];
				}
				else
				{
					currentBlendingStrength = decal.ColorArray[xx, yy].a;
					currentBlendingStrength = Mathf.Max(decal.RoughnessArray[xx, yy].a * blendingStrength, currentBlendingStrength);
					currentBlendingStrength = Mathf.Max(decal.MetallicityArray[xx, yy].a * blendingStrength, currentBlendingStrength);
					currentBlendingStrength = Mathf.Max(decal.HeightArray[xx, yy].a * blendingStrength, currentBlendingStrength);
					currentBlendingStrength = Mathf.Min(1f, currentBlendingStrength);
				}
				if (mode == ModeEnum.DECAL && doHeightBlending)
				{
					float bottomHeight = 0f;
					if (workspace.Worksheet.Data.Tileable)
					{
						int xxx = Mathf.PosMod(position.x - arraySize / 2 + x, workspace.Worksheet.Data.Width);
						int yyy = Mathf.PosMod(position.y - arraySize / 2 + y, workspace.Worksheet.Data.Height);
						bottomHeight = workspace.Worksheet.Data.HeightChannel[xxx, yyy].v;
					}
					else
					{
						int xxx2 = position.x - arraySize / 2 + x;
						int yyy2 = position.y - arraySize / 2 + y;
						if (xxx2 >= 0 && xxx2 < workspace.Worksheet.Data.Width && yyy2 >= 0 && yyy2 < workspace.Worksheet.Data.Height)
						{
							bottomHeight = workspace.Worksheet.Data.HeightChannel[xxx2, yyy2].v;
						}
					}
					float topHeight = heightArray[xx, yy].v + heightOffset;
					currentBlendingStrength = ((!(heightBlendingFactor > 0f)) ? (currentBlendingStrength * Blender.HeightBlend(bottomHeight, 0f, topHeight, 1f)) : (currentBlendingStrength * Blender.HeightBlend(bottomHeight, 0f, topHeight, 1f, heightBlendingFactor)));
				}
				if ((double)currentBlendingStrength > 0.01568)
				{
					drawingPreviewManager.DrawPixel(position.x - arraySize / 2 + x, position.y - arraySize / 2 + y, clear: false, currentBlendingStrength);
				}
			}
		}
	}

	public override void DrawSpot(Vector2i position)
	{
		DrawPreview(position);
	}

	public override void DrawStroke(Vector2i start, Vector2i end)
	{
	}

	public void DrawDecal(Vector2i position)
	{
		int rotation = ((!doRandomRotation) ? this.rotation : randomDirection);
		for (int y = 0; y < decal.ArraySize; y++)
		{
			for (int x = 0; x < decal.ArraySize; x++)
			{
				int xx;
				int yy;
				switch (rotation)
				{
				case 1:
					xx = y;
					yy = decal.ArraySize - 1 - x;
					break;
				case 2:
					xx = decal.ArraySize - 1 - x;
					yy = decal.ArraySize - 1 - y;
					break;
				case 3:
					xx = decal.ArraySize - 1 - y;
					yy = x;
					break;
				default:
					xx = x;
					yy = y;
					break;
				}
				if (doMirroring)
				{
					switch (rotation)
					{
					case 0:
					case 2:
						xx = decal.ArraySize - 1 - xx;
						break;
					case 1:
					case 3:
						yy = decal.ArraySize - 1 - yy;
						break;
					}
				}
				float currentBlendingStrength = blendingStrength;
				if (mode == ModeEnum.MASK || useDecalMaskArray)
				{
					currentBlendingStrength *= decal.MaskArray[xx, yy];
				}
				if (doHeightBlending)
				{
					float bottomHeight = 0f;
					if (workspace.Worksheet.Data.Tileable)
					{
						int xxx = Mathf.PosMod(position.x - decal.ArraySize / 2 + x, workspace.Worksheet.Data.Width);
						int yyy = Mathf.PosMod(position.y - decal.ArraySize / 2 + y, workspace.Worksheet.Data.Height);
						bottomHeight = workspace.Worksheet.Data.HeightChannel[xxx, yyy].v;
					}
					else
					{
						int xxx2 = position.x - decal.ArraySize / 2 + x;
						int yyy2 = position.y - decal.ArraySize / 2 + y;
						if (xxx2 >= 0 && xxx2 < workspace.Worksheet.Data.Width && yyy2 >= 0 && yyy2 < workspace.Worksheet.Data.Height)
						{
							bottomHeight = workspace.Worksheet.Data.HeightChannel[xxx2, yyy2].v;
						}
					}
					float topHeight = decal.HeightArray[xx, yy].v + heightOffset;
					currentBlendingStrength = ((!(heightBlendingFactor > 0f)) ? (currentBlendingStrength * Blender.HeightBlend(bottomHeight, 0f, topHeight, 1f)) : (currentBlendingStrength * Blender.HeightBlend(bottomHeight, 0f, topHeight, 1f, heightBlendingFactor)));
				}
				if (!((double)currentBlendingStrength > 0.01568))
				{
					continue;
				}
				drawingManager.Color = decal.ColorArray[xx, yy];
				drawingManager.Roughness = decal.RoughnessArray[xx, yy];
				drawingManager.Metallicity = decal.MetallicityArray[xx, yy];
				drawingManager.Height = new Value(Mathf.Clamp(decal.HeightArray[xx, yy].v + heightOffset, 0f, 1f), decal.HeightArray[xx, yy].a);
				drawingManager.Emission = decal.EmissionArray[xx, yy];
				int currentX = position.x - decal.ArraySize / 2 + x;
				int currentY = position.y - decal.ArraySize / 2 + y;
				drawingManager.DrawPixel(workspace.Worksheet, currentX, currentY, currentBlendingStrength);
				if (!drawingManager.Mirroring)
				{
					continue;
				}
				if (drawingManager.VerticalMirroring)
				{
					drawingManager.DrawVerticalMirrorPixel(workspace.Worksheet, currentX, currentY, currentBlendingStrength);
				}
				if (drawingManager.HorizontalMirroring)
				{
					int newCurrentY = drawingManager.DrawHorizontalMirrorPixel(workspace.Worksheet, currentX, currentY, currentBlendingStrength);
					if (drawingManager.VerticalMirroring)
					{
						drawingManager.DrawVerticalMirrorPixel(workspace.Worksheet, currentX, newCurrentY, currentBlendingStrength);
					}
				}
			}
		}
	}

	public override void WhileDrawing(Vector2i position)
	{
		drawingEnd = position;
		DrawPreview(drawingEnd);
		drawingPositionLastFrame = drawingEnd;
	}

	public override void StopDrawing(Vector2i position)
	{
		drawingEnd = position;
		switch (mode)
		{
		case ModeEnum.MASK:
		{
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
			break;
		}
		case ModeEnum.DECAL:
			drawingManager.PushSettings();
			if (drawingManager.Mirroring && drawingManager.CircleMirroring)
			{
				float deltaX = (float)position.x - drawingManager.CircleVerticalMirrorPosition;
				float deltaY = (float)position.y - drawingManager.CircleHorizontalMirrorPosition;
				float distance = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
				float step = Mathf.Pi * 2f / (float)drawingManager.CircleMirroringCount;
				float offset = Mathf.Atan2(deltaY, deltaX);
				for (int i = 0; i < drawingManager.CircleMirroringCount; i++)
				{
					if (doRandomRotation)
					{
						randomDirection = randomRotationsArray[i];
					}
					int newX = Mathf.RoundToInt(drawingManager.CircleVerticalMirrorPosition + Mathf.Cos(offset + step * (float)i) * distance);
					int newY = Mathf.RoundToInt(drawingManager.CircleHorizontalMirrorPosition + Mathf.Sin(offset + step * (float)i) * distance);
					DrawDecal(new Vector2i(newX, newY));
				}
			}
			else
			{
				DrawDecal(drawingEnd);
			}
			drawingManager.PopSettings();
			break;
		}
		drawingPreviewManager.ClearArea();
		if (doRandomRotation)
		{
			GD.Randomize();
			randomDirection = (int)(GD.Randi() % 4);
		}
		if (doRandomSelectionFromList)
		{
			switch (mode)
			{
			case ModeEnum.MASK:
				SelectRandomMaskFromList();
				Register.MaterialContainer.ChangeDecalPreview(Mask.Thumbnail);
				break;
			case ModeEnum.DECAL:
				SelectRandomDecalFromList();
				Register.MaterialContainer.ChangeDecalPreview(Decal.Thumbnail);
				break;
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		SetMask(defaultMask, "Default");
		decal = null;
		decalsList.Clear();
		doRandomSelectionFromList = false;
		doRandomRotation = false;
		mode = ModeEnum.MASK;
		rotation = 0;
		doMirroring = false;
		doHeightBlending = false;
		heightOffset = 0f;
		heightBlendingFactor = 0f;
	}

	public void ChangeMirroring()
	{
		doMirroring = !doMirroring;
	}

	public void RotateLeft()
	{
		int rotation = this.rotation - 1;
		if (rotation < 0)
		{
			rotation = 3;
		}
		Rotation = rotation;
		Register.Gui.StampSettingsContainer.ChangeMaskRotation(this.rotation);
	}

	public void RotateRight()
	{
		int rotation = this.rotation + 1;
		if (rotation > 3)
		{
			rotation = 0;
		}
		Rotation = rotation;
		Register.Gui.StampSettingsContainer.ChangeMaskRotation(this.rotation);
	}
}
