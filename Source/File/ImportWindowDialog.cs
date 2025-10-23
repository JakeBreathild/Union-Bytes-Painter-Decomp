using System.Collections.Generic;
using System.IO;
using Godot;

public class ImportWindowDialog : WindowDialog
{
	private Workspace workspace;

	private TextureRect filePreviewTextureRect;

	private Vector2 filePreviewTextureRectSize = Vector2.Zero;

	private Vector2 filePreviewTextureRectPosition = Vector2.Zero;

	private Image fileImage;

	private ImageTexture fileImageTexture;

	private string filePath = "";

	private LineEdit filePathLineEdit;

	private int width;

	private LineEdit widthLineEdit;

	private int height;

	private LineEdit heightLineEdit;

	private Color[,] fileColorArray;

	private float[,] fileBrightnessArray;

	private LineEdit worksheetNameLineEdit;

	private string worksheetName = "Unnamed";

	private CheckButton tileableCheckButton;

	private bool tileable;

	private CheckButton importColorCheckButton;

	private bool importColor = true;

	private CheckButton generateColorPaletteCheckButton;

	private bool generateColorPalette = true;

	private HSlider colorHueHSlider;

	private float colorHue;

	private HSlider colorMinHSlider;

	private float colorMin;

	private HSlider colorMinHueHSlider;

	private float colorMinHue;

	private HSlider colorMinSaturationHSlider;

	private float colorMinSaturation;

	private HSlider colorMaxHSlider;

	private float colorMax = 1f;

	private HSlider colorMaxHueHSlider;

	private float colorMaxHue;

	private HSlider colorMaxSaturationHSlider;

	private float colorMaxSaturation;

	private TextureRect heightPreviewTextureRect;

	private Vector2 heightPreviewTextureRectSize = Vector2.Zero;

	private Vector2 heightPreviewTextureRectPosition = Vector2.Zero;

	private CheckButton importHeightCheckButton;

	private bool importHeight = true;

	private Image heightImage;

	private ImageTexture heightImageTexture;

	private Value[,] heightArray;

	private HSlider heightPowerHSlider;

	private float heightPower = 1f;

	private HSlider heightAdditionHSlider;

	private float heightAddition;

	private HSlider heightInverseHSlider;

	private bool heightInverse;

	private TextureRect roughnessPreviewTextureRect;

	private Vector2 roughnessPreviewTextureRectSize = Vector2.Zero;

	private Vector2 roughnessPreviewTextureRectPosition = Vector2.Zero;

	private CheckButton importRoughnessCheckButton;

	private bool importRoughness = true;

	private Image roughnessImage;

	private ImageTexture roughnessImageTexture;

	private Value[,] roughnessArray;

	private HSlider roughnessPowerHSlider;

	private float roughnessPower = 1f;

	private HSlider roughnessAdditionHSlider;

	private float roughnessAddition;

	private HSlider roughnessInverseHSlider;

	private bool roughnessInverse = true;

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		string nodeGroupPath = "SC/VC/Source/";
		filePathLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Path");
		filePreviewTextureRect = GetNodeOrNull<TextureRect>(nodeGroupPath + "Preview");
		filePreviewTextureRectSize = filePreviewTextureRect.RectSize;
		filePreviewTextureRectSize.x = Mathf.Round(filePreviewTextureRectSize.x);
		filePreviewTextureRectSize.y = Mathf.Round(filePreviewTextureRectSize.y);
		filePreviewTextureRectPosition = filePreviewTextureRect.RectPosition;
		filePreviewTextureRectPosition.x = Mathf.Round(filePreviewTextureRectPosition.x);
		filePreviewTextureRectPosition.y = Mathf.Round(filePreviewTextureRectPosition.y);
		widthLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Width");
		heightLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Height");
		nodeGroupPath = "SC/VC/Worksheet/";
		worksheetNameLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Name");
		worksheetNameLineEdit.Connect(Signals.TextChanged, this, "ChangeWorksheetName");
		worksheetNameLineEdit.Connect(Signals.TextEntered, this, "EnteredWorksheetName");
		tileableCheckButton = GetNodeOrNull<CheckButton>(nodeGroupPath + "Tileable");
		tileableCheckButton.Connect(Signals.Toggled, this, "ChangeWorksheetTileable");
		nodeGroupPath = "SC/VC/Color/";
		importColorCheckButton = GetNodeOrNull<CheckButton>(nodeGroupPath + "Import");
		importColorCheckButton.Connect(Signals.Toggled, this, "ChangeImportColor");
		generateColorPaletteCheckButton = GetNodeOrNull<CheckButton>(nodeGroupPath + "GeneratePalette");
		generateColorPaletteCheckButton.Connect(Signals.Toggled, this, "ChangeGenerateColorPalette");
		colorHueHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "Hue");
		colorHueHSlider.Connect(Signals.ValueChanged, this, "ChangeColorHue");
		colorMinHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "GC/Min");
		colorMinHSlider.Connect(Signals.ValueChanged, this, "ChangeColorMin");
		colorMinHueHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "GC/MinHue");
		colorMinHueHSlider.Connect(Signals.ValueChanged, this, "ChangeColorMinHue");
		colorMinSaturationHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "GC/MinSaturation");
		colorMinSaturationHSlider.Connect(Signals.ValueChanged, this, "ChangeColorMinSaturation");
		colorMaxHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "GC/Max");
		colorMaxHSlider.Connect(Signals.ValueChanged, this, "ChangeColorMax");
		colorMaxHueHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "GC/MaxHue");
		colorMaxHueHSlider.Connect(Signals.ValueChanged, this, "ChangeColorMaxHue");
		colorMaxSaturationHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "GC/MaxSaturation");
		colorMaxSaturationHSlider.Connect(Signals.ValueChanged, this, "ChangeColorMaxSaturation");
		nodeGroupPath = "SC/VC/Height/";
		importHeightCheckButton = GetNodeOrNull<CheckButton>(nodeGroupPath + "Import");
		importHeightCheckButton.Connect(Signals.Toggled, this, "ChangeImportHeight");
		heightPreviewTextureRect = GetNodeOrNull<TextureRect>(nodeGroupPath + "Preview");
		heightPreviewTextureRectSize = heightPreviewTextureRect.RectSize;
		heightPreviewTextureRectSize.x = Mathf.Round(heightPreviewTextureRectSize.x);
		heightPreviewTextureRectSize.y = Mathf.Round(heightPreviewTextureRectSize.y);
		heightPreviewTextureRectPosition = heightPreviewTextureRect.RectPosition;
		heightPreviewTextureRectPosition.x = Mathf.Round(heightPreviewTextureRectPosition.x);
		heightPreviewTextureRectPosition.y = Mathf.Round(heightPreviewTextureRectPosition.y);
		heightPowerHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "Power");
		heightPowerHSlider.Connect(Signals.ValueChanged, this, "ChangeHeightPower");
		heightAdditionHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "Add");
		heightAdditionHSlider.Connect(Signals.ValueChanged, this, "ChangeHeightAddition");
		heightInverseHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "Inv");
		heightInverseHSlider.Connect(Signals.ValueChanged, this, "ChangeHeightInverse");
		nodeGroupPath = "SC/VC/Roughness/";
		importRoughnessCheckButton = GetNodeOrNull<CheckButton>(nodeGroupPath + "Import");
		importRoughnessCheckButton.Connect(Signals.Toggled, this, "ChangeImportRoughness");
		roughnessPreviewTextureRect = GetNodeOrNull<TextureRect>(nodeGroupPath + "Preview");
		roughnessPreviewTextureRectSize = roughnessPreviewTextureRect.RectSize;
		roughnessPreviewTextureRectSize.x = Mathf.Round(roughnessPreviewTextureRectSize.x);
		roughnessPreviewTextureRectSize.y = Mathf.Round(roughnessPreviewTextureRectSize.y);
		roughnessPreviewTextureRectPosition = roughnessPreviewTextureRect.RectPosition;
		roughnessPreviewTextureRectPosition.x = Mathf.Round(roughnessPreviewTextureRectPosition.x);
		roughnessPreviewTextureRectPosition.y = Mathf.Round(roughnessPreviewTextureRectPosition.y);
		roughnessPowerHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "Power");
		roughnessPowerHSlider.Connect(Signals.ValueChanged, this, "ChangeRoughnessPower");
		roughnessAdditionHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "Add");
		roughnessAdditionHSlider.Connect(Signals.ValueChanged, this, "ChangeRoughnessAddition");
		roughnessInverseHSlider = GetNodeOrNull<HSlider>(nodeGroupPath + "Inv");
		roughnessInverseHSlider.Connect(Signals.ValueChanged, this, "ChangeRoughnessInverse");
		Connect(Signals.Hide, this, "Hide");
	}

	public void PopupCentered(string file)
	{
		file = file.Trim();
		if (file != "")
		{
			filePath = file;
			fileImage = new Image();
			fileImage.Load(filePath);
			Reset();
			InputManager.WindowShown();
			PopupCentered();
		}
	}

	public new void Hide()
	{
		base.Hide();
		fileColorArray = null;
		fileBrightnessArray = null;
		fileImage = null;
		fileImageTexture = null;
		heightArray = null;
		heightImage = null;
		heightImageTexture = null;
		roughnessArray = null;
		roughnessImage = null;
		roughnessImageTexture = null;
		InputManager.WindowHidden();
		InputManager.SkipInput = true;
	}

	public void Reset()
	{
		GetNodeOrNull<ScrollContainer>("SC").ScrollVertical = 0;
		width = fileImage.GetWidth();
		height = fileImage.GetHeight();
		fileColorArray = new Color[width, height];
		fileBrightnessArray = new float[width, height];
		fileImage.Lock();
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				fileColorArray[x, y] = fileImage.GetPixel(x, y);
				fileBrightnessArray[x, y] = fileColorArray[x, y].v;
			}
		}
		fileImage.Unlock();
		fileImageTexture = new ImageTexture();
		fileImageTexture.CreateFromImage(fileImage, 0u);
		filePreviewTextureRect.Texture = fileImageTexture;
		filePathLineEdit.Text = filePath;
		widthLineEdit.Text = width.ToString();
		heightLineEdit.Text = height.ToString();
		heightArray = new Value[width, height];
		heightImage = new Image();
		heightImage.Create(width, height, useMipmaps: false, Image.Format.Rgb8);
		heightImageTexture = new ImageTexture();
		heightPreviewTextureRect.Texture = heightImageTexture;
		UpdateHeightTexture();
		roughnessArray = new Value[width, height];
		roughnessImage = new Image();
		roughnessImage.Create(width, height, useMipmaps: false, Image.Format.Rgb8);
		roughnessImageTexture = new ImageTexture();
		roughnessPreviewTextureRect.Texture = roughnessImageTexture;
		UpdateRoughnessTexture();
		float ratio = 1f * (float)width / (float)height;
		if (ratio != 1f)
		{
			Vector2 newTextureRectSize = filePreviewTextureRectSize;
			Vector2 newTextureRectPosition = filePreviewTextureRectPosition;
			if (ratio < 1f)
			{
				newTextureRectSize.x *= ratio;
				newTextureRectPosition.x += (filePreviewTextureRectSize.x - newTextureRectSize.x) * 0.5f;
			}
			else
			{
				newTextureRectSize.y /= ratio;
				newTextureRectPosition.y += (filePreviewTextureRectSize.y - newTextureRectSize.y) * 0.5f;
			}
			filePreviewTextureRect.RectSize = newTextureRectSize;
			filePreviewTextureRect.RectPosition = newTextureRectPosition;
			newTextureRectSize = heightPreviewTextureRectSize;
			newTextureRectPosition = heightPreviewTextureRectPosition;
			if (ratio < 1f)
			{
				newTextureRectSize.x *= ratio;
				newTextureRectPosition.x += (heightPreviewTextureRectSize.x - newTextureRectSize.x) * 0.5f;
			}
			else
			{
				newTextureRectSize.y /= ratio;
				newTextureRectPosition.y += (heightPreviewTextureRectSize.y - newTextureRectSize.y) * 0.5f;
			}
			heightPreviewTextureRect.RectSize = newTextureRectSize;
			heightPreviewTextureRect.RectPosition = newTextureRectPosition;
			newTextureRectSize = roughnessPreviewTextureRectSize;
			newTextureRectPosition = roughnessPreviewTextureRectPosition;
			if (ratio < 1f)
			{
				newTextureRectSize.x *= ratio;
				newTextureRectPosition.x += (roughnessPreviewTextureRectSize.x - newTextureRectSize.x) * 0.5f;
			}
			else
			{
				newTextureRectSize.y /= ratio;
				newTextureRectPosition.y += (roughnessPreviewTextureRectSize.y - newTextureRectSize.y) * 0.5f;
			}
			roughnessPreviewTextureRect.RectSize = newTextureRectSize;
			roughnessPreviewTextureRect.RectPosition = newTextureRectPosition;
		}
		else
		{
			filePreviewTextureRect.RectSize = filePreviewTextureRectSize;
			filePreviewTextureRect.RectPosition = filePreviewTextureRectPosition;
			heightPreviewTextureRect.RectSize = heightPreviewTextureRectSize;
			heightPreviewTextureRect.RectPosition = heightPreviewTextureRectPosition;
			roughnessPreviewTextureRect.RectSize = roughnessPreviewTextureRectSize;
			roughnessPreviewTextureRect.RectPosition = roughnessPreviewTextureRectPosition;
		}
		worksheetName = "Unnamed";
		worksheetNameLineEdit.Text = worksheetName;
		tileable = false;
		tileableCheckButton.Pressed = tileable;
		importColor = true;
		importColorCheckButton.Pressed = importColor;
		generateColorPalette = true;
		generateColorPaletteCheckButton.Pressed = generateColorPalette;
		colorHue = 0f;
		colorHueHSlider.Value = colorHue * 100f;
		colorMin = 0f;
		colorMinHSlider.Value = colorMin * 100f;
		colorMinHue = 0f;
		colorMinHueHSlider.Value = colorMinHue * 100f;
		colorMinSaturation = 0f;
		colorMinSaturationHSlider.Value = colorMinSaturation * 100f;
		colorMax = 1f;
		colorMaxHSlider.Value = colorMax * 100f;
		colorMaxHue = 0f;
		colorMaxHueHSlider.Value = colorMaxHue * 100f;
		colorMaxSaturation = 0f;
		colorMaxSaturationHSlider.Value = colorMaxSaturation * 100f;
		heightPower = 1f;
		heightPowerHSlider.Value = heightPower * 10f;
		heightAddition = 0f;
		heightAdditionHSlider.Value = heightAddition * 100f;
		heightInverse = false;
		heightInverseHSlider.Value = (heightInverse ? 1f : 0f);
		roughnessPower = 1f;
		roughnessPowerHSlider.Value = roughnessPower * 10f;
		roughnessAddition = 0f;
		roughnessAdditionHSlider.Value = roughnessAddition * 100f;
		roughnessInverse = true;
		roughnessInverseHSlider.Value = (roughnessInverse ? 1f : 0f);
	}

	private void UpdateColorTexture()
	{
		fileImage.Lock();
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				Color color = fileColorArray[x, y];
				color.h += colorHue;
				if (color.v > colorMax)
				{
					color.v = colorMax;
					color.h += colorMaxHue;
					color.s = Mathf.Clamp(color.s + colorMaxSaturation, 0f, 1f);
				}
				if (color.v < colorMin)
				{
					color.v = colorMin;
					color.h += colorMinHue;
					color.s = Mathf.Clamp(color.s + colorMinSaturation, 0f, 1f);
				}
				fileImage.SetPixel(x, y, color);
			}
		}
		fileImage.Unlock();
		fileImageTexture.CreateFromImage(fileImage, 0u);
	}

	private void UpdateHeightTexture()
	{
		heightImage.Lock();
		Color color = new Color(0f, 0f, 0f);
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				heightArray[x, y].v = Mathf.Clamp(Mathf.Pow(heightAddition + fileBrightnessArray[x, y], heightPower), 0f, 1f);
				if (heightInverse)
				{
					heightArray[x, y].v = 1f - heightArray[x, y].v;
				}
				heightArray[x, y].a = 1f;
				color.r = (color.g = (color.b = heightArray[x, y].v));
				heightImage.SetPixel(x, y, color);
			}
		}
		heightImage.Unlock();
		heightImageTexture.CreateFromImage(heightImage, 0u);
	}

	private void UpdateRoughnessTexture()
	{
		roughnessImage.Lock();
		Color color = new Color(0f, 0f, 0f);
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				roughnessArray[x, y].v = Mathf.Clamp(Mathf.Pow(roughnessAddition + fileBrightnessArray[x, y], roughnessPower), 0f, 1f);
				if (roughnessInverse)
				{
					roughnessArray[x, y].v = 1f - roughnessArray[x, y].v;
				}
				roughnessArray[x, y].a = 1f;
				color.r = (color.g = (color.b = roughnessArray[x, y].v));
				roughnessImage.SetPixel(x, y, color);
			}
		}
		roughnessImage.Unlock();
		roughnessImageTexture.CreateFromImage(roughnessImage, 0u);
	}

	public void Import()
	{
		if (worksheetName == "")
		{
			worksheetName = System.IO.Path.GetFileName(filePath);
		}
		if (!importColor)
		{
			fileColorArray = null;
		}
		else
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					fileColorArray[x, y].h += colorHue;
					if (fileColorArray[x, y].v > colorMax)
					{
						fileColorArray[x, y].v = colorMax;
						fileColorArray[x, y].h += colorMaxHue;
						fileColorArray[x, y].s = Mathf.Clamp(fileColorArray[x, y].s + colorMaxSaturation, 0f, 1f);
					}
					if (fileColorArray[x, y].v < colorMin)
					{
						fileColorArray[x, y].v = colorMin;
						fileColorArray[x, y].h += colorMinHue;
						fileColorArray[x, y].s = Mathf.Clamp(fileColorArray[x, y].s + colorMinSaturation, 0f, 1f);
					}
				}
			}
		}
		if (!importHeight)
		{
			heightArray = null;
		}
		if (!importRoughness)
		{
			roughnessArray = null;
		}
		workspace.CreateWorksheet(width, height, tileable, worksheetName, fileColorArray, heightArray, roughnessArray);
		if (generateColorPalette)
		{
			List<Color> colorsList = new List<Color>();
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					Color color = fileColorArray[j, i];
					if (!(color.a > 0.8f))
					{
						continue;
					}
					bool colorFound = false;
					foreach (Color listColor in colorsList)
					{
						if (ColorExtension.ColorDistance(color, listColor) < 0.05f)
						{
							colorFound = true;
							break;
						}
					}
					if (!colorFound)
					{
						colorsList.Add(color);
					}
				}
			}
			colorsList.Sort((Color colorA, Color colorB) => (!(Mathf.Abs(colorA.h - colorB.h) < 0.05f)) ? colorA.h.CompareTo(colorB.h) : (-colorA.v.CompareTo(colorB.v)));
			workspace.LibraryManager.ResetThumbnailRenderer();
			workspace.LibraryManager.ConvertColorListToDrawingPresets(colorsList);
		}
		Settings.ImportPath = System.IO.Path.GetFullPath(filePath);
		Hide();
	}

	public void ChangeWorksheetTileable(bool pressed)
	{
		tileable = pressed;
	}

	public void ChangeWorksheetName(string name)
	{
		worksheetName = name.Trim();
	}

	public void EnteredWorksheetName(string name)
	{
		worksheetName = name.Trim();
		worksheetNameLineEdit.ReleaseFocus();
	}

	public void ChangeImportColor(bool pressed)
	{
		importColor = pressed;
	}

	public void ChangeGenerateColorPalette(bool pressed)
	{
		generateColorPalette = pressed;
	}

	public void ChangeColorHue(float value)
	{
		colorHue = value / 100f;
		UpdateColorTexture();
	}

	public void ChangeColorMin(float value)
	{
		colorMin = value / 100f;
		UpdateColorTexture();
	}

	public void ChangeColorMinHue(float value)
	{
		colorMinHue = value / 100f;
		UpdateColorTexture();
	}

	public void ChangeColorMinSaturation(float value)
	{
		colorMinSaturation = value / 100f;
		UpdateColorTexture();
	}

	public void ChangeColorMax(float value)
	{
		colorMax = value / 100f;
		UpdateColorTexture();
	}

	public void ChangeColorMaxHue(float value)
	{
		colorMaxHue = value / 100f;
		UpdateColorTexture();
	}

	public void ChangeColorMaxSaturation(float value)
	{
		colorMaxSaturation = value / 100f;
		UpdateColorTexture();
	}

	public void ChangeImportHeight(bool pressed)
	{
		importHeight = pressed;
	}

	public void ChangeHeightPower(float value)
	{
		heightPower = value / 10f;
		UpdateHeightTexture();
	}

	public void ChangeHeightAddition(float value)
	{
		heightAddition = value / 100f;
		UpdateHeightTexture();
	}

	public void ChangeHeightInverse(float value)
	{
		heightInverse = value > 0f;
		UpdateHeightTexture();
	}

	public void ChangeImportRoughness(bool pressed)
	{
		importRoughness = pressed;
	}

	public void ChangeRoughnessPower(float value)
	{
		roughnessPower = value / 10f;
		UpdateRoughnessTexture();
	}

	public void ChangeRoughnessAddition(float value)
	{
		roughnessAddition = value / 100f;
		UpdateRoughnessTexture();
	}

	public void ChangeRoughnessInverse(float value)
	{
		roughnessInverse = value > 0f;
		UpdateRoughnessTexture();
	}
}
