using Godot;

public class MaterialContainer : DefaultContainer
{
	private Panel blendingModePanel;

	private OptionButton blendingModeOptionButton;

	private int blendingModeInitialValue;

	private PopupMenu blendingModePopupMenu;

	private Panel decalPanel;

	private Label decalLabel;

	private TextureRect decalPreviewTextureRect;

	private Vector2 decalPreviewTextureRectPosition = Vector2.Zero;

	private Vector2 decalPreviewTextureRectSize = Vector2.Zero;

	private Panel decalSettingsPanel;

	private DefaultSlider decalHeightOffsetSlider;

	private CheckButton decalHeightBlendCheckButton;

	private DefaultSlider decalHeightFactorSlider;

	private Panel decalListPanel;

	private ItemList decalItemList;

	private Button addDecalToListButton;

	private Button removeDecalFromListButton;

	private Button clearDecalListButton;

	private CheckButton decalRandomSelectionCheckButton;

	private Panel materialsPanel;

	private Button material1Button;

	private Button material2Button;

	private HSeparator hSeparator;

	private Panel colorPanel;

	private DefaultColorPickerButton colorDefaultColorPickerButton;

	private Color colorInitialValue = new Color(0f, 0f, 0f);

	private Button colorAlphaButton;

	private bool colorAlphaInitialValue = true;

	private DefaultSlider colorRedDefaultSlider;

	private DefaultSlider colorGreenDefaultSlider;

	private DefaultSlider colorBlueDefaultSlider;

	private DefaultSlider colorAlphaDefaultSlider;

	private TextureButton colorPaletteButton;

	private Panel colorPalettePanel;

	private ColorPalette colorPalette;

	private bool colorPanelEnabled = true;

	private Panel roughnessPanel;

	private DefaultSlider roughnessDefaultSlider;

	private float roughnessInitialValue;

	private bool roughnessPanelEnabled = true;

	private Panel metallicityPanel;

	private DefaultSlider metallicityDefaultSlider;

	private float metallicityInitialValue;

	private bool metallicityPanelEnabled = true;

	private Panel heightPanel;

	private DefaultSlider heightDefaultSlider;

	private float heightInitialValue;

	private bool heightPanelEnabled = true;

	private Panel emissionPanel;

	private DefaultColorPickerButton emissionDefaultColorPickerButton;

	private Color emissionInitialValue = new Color(0f, 0f, 0f);

	private bool emissionPanelEnabled;

	public MaterialContainer()
	{
		Register.MaterialContainer = this;
	}

	public override void _Ready()
	{
		base._Ready();
		blendingModePanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/BlendingMode");
		blendingModeOptionButton = GetNodeOrNull<OptionButton>("ScrollContainer/VBoxContainer/BlendingMode/BlendingMode");
		for (int i = 0; i <= 7; i++)
		{
			blendingModeOptionButton.AddItem(Blender.BlendingModeName[i], i);
		}
		blendingModeOptionButton.Selected = (int)DrawingManager.DefaultBlendingMode;
		blendingModeInitialValue = blendingModeOptionButton.Selected;
		blendingModePopupMenu = blendingModeOptionButton.GetPopup();
		blendingModePopupMenu.MouseFilter = MouseFilterEnum.Pass;
		blendingModePopupMenu.Connect(Signals.IdPressed, this, "BlendingModeSelected");
		blendingModePopupMenu.Connect(Signals.MouseEntered, this, "MouseEntered");
		blendingModePopupMenu.Connect(Signals.MouseExited, this, "MouseExited");
		blendingModePopupMenu.Connect(Signals.AboutToShow, this, "MouseEntered");
		blendingModePopupMenu.Connect(Signals.Hide, this, "MouseExited");
		decalPanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/Decal");
		decalLabel = GetNodeOrNull<Label>("ScrollContainer/VBoxContainer/Decal/Label");
		decalPreviewTextureRect = GetNodeOrNull<TextureRect>("ScrollContainer/VBoxContainer/Decal/Checker/Preview");
		decalPreviewTextureRectPosition = decalPreviewTextureRect.RectPosition;
		decalPreviewTextureRectPosition.x = Mathf.Round(decalPreviewTextureRectPosition.x);
		decalPreviewTextureRectPosition.y = Mathf.Round(decalPreviewTextureRectPosition.y);
		decalPreviewTextureRectSize = decalPreviewTextureRect.RectSize;
		decalPreviewTextureRectSize.x = Mathf.Round(decalPreviewTextureRectSize.x);
		decalPreviewTextureRectSize.y = Mathf.Round(decalPreviewTextureRectSize.y);
		decalSettingsPanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/DecalSettings");
		decalHeightOffsetSlider = GetNodeOrNull<DefaultSlider>("ScrollContainer/VBoxContainer/DecalSettings/HeightOffset");
		decalHeightOffsetSlider.Connect(Signals.ValueChanged, this, "DecalHeightOffsetChanged");
		decalHeightBlendCheckButton = GetNodeOrNull<CheckButton>("ScrollContainer/VBoxContainer/DecalSettings/HeightBlend");
		decalHeightBlendCheckButton.Connect(Signals.Toggled, this, "DecalHeightBlendToggled");
		decalHeightFactorSlider = GetNodeOrNull<DefaultSlider>("ScrollContainer/VBoxContainer/DecalSettings/HeightFactor");
		decalHeightFactorSlider.Connect(Signals.ValueChanged, this, "DecalHeightFactorChanged");
		decalListPanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/DecalList");
		decalItemList = GetNodeOrNull<ItemList>("ScrollContainer/VBoxContainer/DecalList/List");
		decalItemList.Connect(Signals.ItemSelected, this, "DecalListItemSelected");
		addDecalToListButton = GetNodeOrNull<Button>("ScrollContainer/VBoxContainer/DecalList/Add");
		addDecalToListButton.Connect(Signals.Pressed, this, "AddDecalToListPressed");
		removeDecalFromListButton = GetNodeOrNull<Button>("ScrollContainer/VBoxContainer/DecalList/Remove");
		removeDecalFromListButton.Connect(Signals.Pressed, this, "RemoveDecalFromListPressed");
		clearDecalListButton = GetNodeOrNull<Button>("ScrollContainer/VBoxContainer/DecalList/Clear");
		clearDecalListButton.Connect(Signals.Pressed, this, "ClearDecalListPressed");
		decalRandomSelectionCheckButton = GetNodeOrNull<CheckButton>("ScrollContainer/VBoxContainer/DecalList/Random");
		decalRandomSelectionCheckButton.Connect(Signals.Toggled, this, "RandomDecalSelectionToggled");
		materialsPanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/Materials");
		material1Button = GetNodeOrNull<Button>("ScrollContainer/VBoxContainer/Materials/Material1");
		material1Button.Connect(Signals.Pressed, this, "Material1Pressed");
		material2Button = GetNodeOrNull<Button>("ScrollContainer/VBoxContainer/Materials/Material2");
		material2Button.Connect(Signals.Pressed, this, "Material2Pressed");
		hSeparator = GetNodeOrNull<HSeparator>("ScrollContainer/VBoxContainer/HSeparator");
		colorPanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/Color");
		colorDefaultColorPickerButton = GetNodeOrNull<DefaultColorPickerButton>("ScrollContainer/VBoxContainer/Color/Color");
		colorInitialValue = colorDefaultColorPickerButton.Color;
		colorDefaultColorPickerButton.Connect(Signals.ColorChanged, this, "ColorChanged");
		colorAlphaButton = GetNodeOrNull<Button>("ScrollContainer/VBoxContainer/Color/EnablingAlpha");
		colorAlphaInitialValue = colorAlphaButton.Pressed;
		colorAlphaButton.Connect(Signals.Toggled, this, "ColorAlphaChannelToggled");
		colorRedDefaultSlider = GetNodeOrNull<DefaultSlider>("ScrollContainer/VBoxContainer/Color/Red");
		colorRedDefaultSlider.Connect(Signals.ValueChanged, this, "ColorChannelChanged");
		colorGreenDefaultSlider = GetNodeOrNull<DefaultSlider>("ScrollContainer/VBoxContainer/Color/Green");
		colorGreenDefaultSlider.Connect(Signals.ValueChanged, this, "ColorChannelChanged");
		colorBlueDefaultSlider = GetNodeOrNull<DefaultSlider>("ScrollContainer/VBoxContainer/Color/Blue");
		colorBlueDefaultSlider.Connect(Signals.ValueChanged, this, "ColorChannelChanged");
		colorAlphaDefaultSlider = GetNodeOrNull<DefaultSlider>("ScrollContainer/VBoxContainer/Color/Alpha");
		colorAlphaDefaultSlider.Connect(Signals.ValueChanged, this, "ColorChannelChanged");
		colorPaletteButton = GetNodeOrNull<TextureButton>("ScrollContainer/VBoxContainer/ColorPalette/OpenPalette");
		colorPaletteButton.Connect(Signals.Toggled, this, "ToggleColorPalette");
		colorPaletteButton.Connect(Signals.MouseEntered, this, "MouseEnteredColorPaletteButton");
		colorPalettePanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/ColorPalette");
		colorPalette = GetNodeOrNull<ColorPalette>("ScrollContainer/VBoxContainer/ColorPalette/ColorPalette");
		colorPalette.ColorSelectedCallback = ColorPaletteColorSelected;
		Register.ColorPalette = colorPalette;
		roughnessPanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/Roughness");
		roughnessDefaultSlider = GetNodeOrNull<DefaultSlider>("ScrollContainer/VBoxContainer/Roughness/Value");
		roughnessInitialValue = roughnessDefaultSlider.GetValue();
		roughnessDefaultSlider.Connect(Signals.ValueChanged, this, "RoughnessChanged");
		metallicityPanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/Metallicity");
		metallicityDefaultSlider = GetNodeOrNull<DefaultSlider>("ScrollContainer/VBoxContainer/Metallicity/Value");
		metallicityInitialValue = metallicityDefaultSlider.GetValue();
		metallicityDefaultSlider.Connect(Signals.ValueChanged, this, "MetallicityChanged");
		heightPanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/Height");
		heightDefaultSlider = GetNodeOrNull<DefaultSlider>("ScrollContainer/VBoxContainer/Height/Value");
		heightInitialValue = heightDefaultSlider.GetValue();
		heightDefaultSlider.Connect(Signals.ValueChanged, this, "HeightChanged");
		emissionPanel = GetNodeOrNull<Panel>("ScrollContainer/VBoxContainer/Emission");
		emissionDefaultColorPickerButton = GetNodeOrNull<DefaultColorPickerButton>("ScrollContainer/VBoxContainer/Emission/Color");
		emissionInitialValue = emissionDefaultColorPickerButton.Color;
		emissionDefaultColorPickerButton.Connect(Signals.ColorChanged, this, "EmissionChanged");
	}

	public override void Reset()
	{
		base.Reset();
		blendingModeOptionButton.Select(blendingModeInitialValue);
		decalLabel.Text = "Preview";
		decalPreviewTextureRect.Texture = null;
		decalHeightOffsetSlider.SetValue(0f);
		decalHeightBlendCheckButton.Pressed = false;
		decalHeightFactorSlider.SetValue(0f);
		decalItemList.Clear();
		decalRandomSelectionCheckButton.Pressed = false;
		material1Button.Pressed = true;
		material2Button.Pressed = false;
		colorPanelEnabled = true;
		colorDefaultColorPickerButton.Color = colorInitialValue;
		colorAlphaButton.Pressed = colorAlphaInitialValue;
		colorRedDefaultSlider.SetValue(colorInitialValue.r8);
		colorGreenDefaultSlider.SetValue(colorInitialValue.g8);
		colorBlueDefaultSlider.SetValue(colorInitialValue.b8);
		colorAlphaDefaultSlider.SetValue(colorInitialValue.a8);
		colorPalette.Reset();
		roughnessPanelEnabled = true;
		roughnessDefaultSlider.SetValue(roughnessInitialValue);
		metallicityPanelEnabled = true;
		metallicityDefaultSlider.SetValue(metallicityInitialValue);
		heightPanelEnabled = true;
		heightDefaultSlider.SetValue(heightInitialValue);
		emissionPanelEnabled = false;
		emissionDefaultColorPickerButton.Color = emissionInitialValue;
		EnableBlendingMode(pressed: true);
		EnableDecal(pressed: false, settingsEnabled: false);
		EnableMaterial(pressed: true);
		emissionPanel.Visible = false;
	}

	public void MouseEntered()
	{
		InputManager.MouseEnteredUserInterface();
	}

	public void MouseExited()
	{
		InputManager.MouseExitedUserInterface();
	}

	public void EnableBlendingMode(bool pressed)
	{
		blendingModePanel.Visible = pressed;
	}

	public void SelectBlendingMode(int blendingModeIndex)
	{
		blendingModeOptionButton.Selected = blendingModeIndex;
	}

	public void BlendingModeSelected(int blendingModeIndex)
	{
		workspace.ChangeDrawingMode(blendingModeIndex);
		InputManager.MouseExitedUserInterface();
	}

	public void EnableDecal(bool pressed, bool settingsEnabled)
	{
		decalPanel.Visible = pressed;
		if (settingsEnabled)
		{
			decalSettingsPanel.Visible = pressed;
		}
		else
		{
			decalSettingsPanel.Visible = false;
		}
		decalListPanel.Visible = pressed;
	}

	public void ChangeDecalLabel(string text)
	{
		decalLabel.Text = text;
	}

	public void ChangeDecalPreview(Image image)
	{
		if (image != null)
		{
			ImageTexture texture = new ImageTexture();
			texture.CreateFromImage(image, 0u);
			Register.MaterialContainer.ChangeDecalPreview(texture);
		}
		else
		{
			Register.MaterialContainer.ChangeDecalPreview((Texture)null);
		}
	}

	public void ChangeDecalPreview(Texture texture)
	{
		if (decalPreviewTextureRect == null)
		{
			return;
		}
		if (texture != null)
		{
			int width = texture.GetWidth();
			int height = texture.GetHeight();
			float ratio = 1f * (float)width / (float)height;
			if (ratio != 1f)
			{
				Vector2 newTextureRectPosition = decalPreviewTextureRectPosition;
				Vector2 newTextureRectSize = decalPreviewTextureRectSize;
				if (ratio < 1f)
				{
					newTextureRectSize.x *= ratio;
					newTextureRectPosition.x += (decalPreviewTextureRectSize.x - newTextureRectSize.x) * 0.5f;
				}
				else
				{
					newTextureRectSize.y /= ratio;
					newTextureRectPosition.y += (decalPreviewTextureRectSize.y - newTextureRectSize.y) * 0.5f;
				}
				decalPreviewTextureRect.RectPosition = newTextureRectPosition;
				decalPreviewTextureRect.RectSize = newTextureRectSize;
			}
			else
			{
				decalPreviewTextureRect.RectPosition = decalPreviewTextureRectPosition;
				decalPreviewTextureRect.RectSize = decalPreviewTextureRectSize;
			}
		}
		decalPreviewTextureRect.Texture = texture;
	}

	public void ChangeDecalHeightOffset(float value)
	{
		decalHeightOffsetSlider.SetValue(value);
	}

	public void DecalHeightOffsetChanged(float value)
	{
		switch (Register.DrawingManager.Tool)
		{
		case DrawingManager.ToolEnum.BUCKET:
			Register.DrawingManager.BucketTool.HeightOffset = decalHeightOffsetSlider.GetValue();
			break;
		case DrawingManager.ToolEnum.STAMP:
			Register.DrawingManager.StampTool.HeightOffset = decalHeightOffsetSlider.GetValue();
			break;
		}
	}

	public void EnableDecalHeightBlend(bool pressed)
	{
		decalHeightBlendCheckButton.Pressed = pressed;
	}

	public void DecalHeightBlendToggled(bool pressed)
	{
		if (Register.DrawingManager.Tool == DrawingManager.ToolEnum.STAMP)
		{
			Register.DrawingManager.StampTool.DoHeightBlending = pressed;
		}
	}

	public void ChangeDecalHeightFactor(float value)
	{
		decalHeightFactorSlider.SetValue(value);
	}

	public void DecalHeightFactorChanged(float value)
	{
		if (Register.DrawingManager.Tool == DrawingManager.ToolEnum.STAMP)
		{
			Register.DrawingManager.StampTool.HeightBlendingFactor = decalHeightFactorSlider.GetValue();
		}
	}

	public void ToggleDecalHeightBlendSettings(bool enable)
	{
		decalHeightBlendCheckButton.Disabled = !enable;
		decalHeightFactorSlider.Disabled = !enable;
		if (!enable)
		{
			decalHeightBlendCheckButton.Pressed = false;
			decalHeightFactorSlider.SetValue(0f);
		}
	}

	public void DecalListItemSelected(int index)
	{
		if (index <= -1)
		{
			return;
		}
		switch (Register.DrawingManager.Tool)
		{
		case DrawingManager.ToolEnum.BUCKET:
			switch (Register.DrawingManager.BucketTool.Mode)
			{
			case DrawingToolBucket.ModeEnum.MASK:
				Register.DrawingManager.BucketTool.SelectMaskFromList(index);
				ChangeDecalPreview(Register.DrawingManager.BucketTool.Mask.Thumbnail);
				break;
			case DrawingToolBucket.ModeEnum.DECAL:
				Register.DrawingManager.BucketTool.SelectDecalFromList(index);
				ChangeDecalPreview(Register.DrawingManager.BucketTool.Decal.Thumbnail);
				break;
			}
			break;
		case DrawingManager.ToolEnum.STAMP:
			switch (Register.DrawingManager.StampTool.Mode)
			{
			case DrawingToolStamp.ModeEnum.MASK:
				Register.DrawingManager.StampTool.SelectMaskFromList(index);
				ChangeDecalPreview(Register.DrawingManager.StampTool.Mask.Thumbnail);
				break;
			case DrawingToolStamp.ModeEnum.DECAL:
				Register.DrawingManager.StampTool.SelectDecalFromList(index);
				ChangeDecalPreview(Register.DrawingManager.StampTool.Decal.Thumbnail);
				break;
			}
			break;
		}
	}

	public void AddDecalToListPressed()
	{
		switch (Register.DrawingManager.Tool)
		{
		case DrawingManager.ToolEnum.BUCKET:
			switch (Register.DrawingManager.BucketTool.Mode)
			{
			case DrawingToolBucket.ModeEnum.MASK:
				if (Register.DrawingManager.BucketTool.Mask != null)
				{
					Register.DrawingManager.BucketTool.AddMaskToList();
					decalItemList.AddItem(Register.DrawingManager.BucketTool.Mask.Name);
				}
				break;
			case DrawingToolBucket.ModeEnum.DECAL:
				if (Register.DrawingManager.BucketTool.Decal != null)
				{
					Register.DrawingManager.BucketTool.AddDecalToList();
					decalItemList.AddItem(Register.DrawingManager.BucketTool.Decal.Name);
				}
				break;
			}
			break;
		case DrawingManager.ToolEnum.STAMP:
			switch (Register.DrawingManager.StampTool.Mode)
			{
			case DrawingToolStamp.ModeEnum.MASK:
				if (Register.DrawingManager.StampTool.Mask != null)
				{
					Register.DrawingManager.StampTool.AddMaskToList();
					decalItemList.AddItem(Register.DrawingManager.StampTool.Mask.Name);
				}
				break;
			case DrawingToolStamp.ModeEnum.DECAL:
				if (Register.DrawingManager.StampTool.Decal != null)
				{
					Register.DrawingManager.StampTool.AddDecalToList();
					decalItemList.AddItem(Register.DrawingManager.StampTool.Decal.Name);
				}
				break;
			}
			break;
		}
	}

	public void RemoveDecalFromListPressed()
	{
		int[] selectedItems = decalItemList.GetSelectedItems();
		if (selectedItems.Length == 0)
		{
			return;
		}
		switch (Register.DrawingManager.Tool)
		{
		case DrawingManager.ToolEnum.BUCKET:
			switch (Register.DrawingManager.BucketTool.Mode)
			{
			case DrawingToolBucket.ModeEnum.MASK:
				Register.DrawingManager.BucketTool.RemoveMaskFromList(selectedItems[0]);
				break;
			case DrawingToolBucket.ModeEnum.DECAL:
				Register.DrawingManager.BucketTool.RemoveDecalFromList(selectedItems[0]);
				break;
			}
			break;
		case DrawingManager.ToolEnum.STAMP:
			switch (Register.DrawingManager.StampTool.Mode)
			{
			case DrawingToolStamp.ModeEnum.MASK:
				Register.DrawingManager.StampTool.RemoveMaskFromList(selectedItems[0]);
				break;
			case DrawingToolStamp.ModeEnum.DECAL:
				Register.DrawingManager.StampTool.RemoveDecalFromList(selectedItems[0]);
				break;
			}
			break;
		}
		decalItemList.RemoveItem(selectedItems[0]);
	}

	public void ClearDecalListPressed()
	{
		switch (Register.DrawingManager.Tool)
		{
		case DrawingManager.ToolEnum.BUCKET:
			switch (Register.DrawingManager.BucketTool.Mode)
			{
			case DrawingToolBucket.ModeEnum.MASK:
				Register.DrawingManager.BucketTool.ClearMasksList();
				break;
			case DrawingToolBucket.ModeEnum.DECAL:
				Register.DrawingManager.BucketTool.ClearDecalsList();
				break;
			}
			break;
		case DrawingManager.ToolEnum.STAMP:
			switch (Register.DrawingManager.StampTool.Mode)
			{
			case DrawingToolStamp.ModeEnum.MASK:
				Register.DrawingManager.StampTool.ClearMasksList();
				break;
			case DrawingToolStamp.ModeEnum.DECAL:
				Register.DrawingManager.StampTool.ClearDecalsList();
				break;
			}
			break;
		}
		decalItemList.Clear();
	}

	public void UpdateDecalList()
	{
		decalItemList.Clear();
		switch (Register.DrawingManager.Tool)
		{
		case DrawingManager.ToolEnum.BUCKET:
			switch (Register.DrawingManager.BucketTool.Mode)
			{
			case DrawingToolBucket.ModeEnum.MASK:
			{
				foreach (Mask mask2 in Register.DrawingManager.BucketTool.MasksList)
				{
					decalItemList.AddItem(mask2.Name);
				}
				break;
			}
			case DrawingToolBucket.ModeEnum.DECAL:
			{
				foreach (Decal decal2 in Register.DrawingManager.BucketTool.DecalsList)
				{
					decalItemList.AddItem(decal2.Name);
				}
				break;
			}
			}
			break;
		case DrawingManager.ToolEnum.STAMP:
			switch (Register.DrawingManager.StampTool.Mode)
			{
			case DrawingToolStamp.ModeEnum.MASK:
			{
				foreach (Mask mask in Register.DrawingManager.StampTool.MasksList)
				{
					decalItemList.AddItem(mask.Name);
				}
				break;
			}
			case DrawingToolStamp.ModeEnum.DECAL:
			{
				foreach (Decal decal in Register.DrawingManager.StampTool.DecalsList)
				{
					decalItemList.AddItem(decal.Name);
				}
				break;
			}
			}
			break;
		}
	}

	public void RandomDecalSelectionToggled(bool pressed)
	{
		if (Register.DrawingManager.Tool == DrawingManager.ToolEnum.STAMP)
		{
			Register.DrawingManager.StampTool.DoRandomSelectionFromList = pressed;
		}
	}

	public void EnableMaterial(bool pressed)
	{
		materialsPanel.Visible = pressed;
		hSeparator.Visible = pressed;
		if (colorPanelEnabled)
		{
			colorPanel.Visible = pressed;
			if (pressed)
			{
				colorPalettePanel.Visible = true;
				if (colorPaletteButton.Pressed)
				{
					colorPalettePanel.RectMinSize = new Vector2(0f, 312f);
					colorPalette.Visible = true;
				}
				else
				{
					colorPalettePanel.RectMinSize = new Vector2(0f, 40f);
					colorPalette.Visible = false;
				}
			}
			else
			{
				colorPalettePanel.Visible = false;
			}
		}
		else
		{
			colorPanel.Visible = false;
			colorPalettePanel.Visible = false;
			colorPalette.Visible = false;
		}
		if (roughnessPanelEnabled)
		{
			roughnessPanel.Visible = pressed;
		}
		else
		{
			roughnessPanel.Visible = false;
		}
		if (metallicityPanelEnabled)
		{
			metallicityPanel.Visible = pressed;
		}
		else
		{
			metallicityPanel.Visible = false;
		}
		if (heightPanelEnabled)
		{
			heightPanel.Visible = pressed;
		}
		else
		{
			heightPanel.Visible = false;
		}
		if (emissionPanelEnabled)
		{
			emissionPanel.Visible = pressed;
		}
		else
		{
			emissionPanel.Visible = false;
		}
	}

	public void EnableMaterialColor(bool pressed)
	{
		colorPanelEnabled = pressed;
		if (materialsPanel.Visible && colorPanelEnabled)
		{
			colorPanel.Visible = pressed;
			if (pressed)
			{
				colorPalettePanel.Visible = true;
				if (colorPaletteButton.Pressed)
				{
					colorPalettePanel.RectMinSize = new Vector2(0f, 312f);
					colorPalette.Visible = true;
				}
				else
				{
					colorPalettePanel.RectMinSize = new Vector2(0f, 40f);
					colorPalette.Visible = false;
				}
			}
			else
			{
				colorPalettePanel.Visible = false;
			}
		}
		else
		{
			colorPanel.Visible = false;
			colorPalettePanel.Visible = false;
			colorPalette.Visible = false;
		}
	}

	public void EnableMaterialRoughness(bool pressed)
	{
		roughnessPanelEnabled = pressed;
		if (materialsPanel.Visible && roughnessPanelEnabled)
		{
			roughnessPanel.Visible = pressed;
		}
		else
		{
			roughnessPanel.Visible = false;
		}
	}

	public void EnableMaterialMetallicity(bool pressed)
	{
		metallicityPanelEnabled = pressed;
		if (materialsPanel.Visible && metallicityPanelEnabled)
		{
			metallicityPanel.Visible = pressed;
		}
		else
		{
			metallicityPanel.Visible = false;
		}
	}

	public void EnableMaterialHeight(bool pressed)
	{
		heightPanelEnabled = pressed;
		if (materialsPanel.Visible && heightPanelEnabled)
		{
			heightPanel.Visible = pressed;
		}
		else
		{
			heightPanel.Visible = false;
		}
	}

	public void EnableMaterialEmission(bool pressed)
	{
		emissionPanelEnabled = pressed;
		if (materialsPanel.Visible && emissionPanelEnabled)
		{
			emissionPanel.Visible = pressed;
		}
		else
		{
			emissionPanel.Visible = false;
		}
	}

	public void UpdateMaterial(int index)
	{
		material1Button.SetBlockSignals(enable: true);
		material2Button.SetBlockSignals(enable: true);
		blendingModeOptionButton.SetBlockSignals(enable: true);
		colorDefaultColorPickerButton.SetBlockSignals(enable: true);
		colorRedDefaultSlider.SetBlockSignals(enable: true);
		colorGreenDefaultSlider.SetBlockSignals(enable: true);
		colorBlueDefaultSlider.SetBlockSignals(enable: true);
		colorAlphaDefaultSlider.SetBlockSignals(enable: true);
		roughnessDefaultSlider.SetBlockSignals(enable: true);
		metallicityDefaultSlider.SetBlockSignals(enable: true);
		heightDefaultSlider.SetBlockSignals(enable: true);
		emissionDefaultColorPickerButton.SetBlockSignals(enable: true);
		switch (index)
		{
		case 0:
			material1Button.Pressed = true;
			material2Button.Pressed = false;
			break;
		case 1:
			material1Button.Pressed = false;
			material2Button.Pressed = true;
			break;
		}
		blendingModeOptionButton.Selected = (int)Register.DrawingManager.BlendingMode;
		colorDefaultColorPickerButton.Color = Register.DrawingManager.Color;
		colorRedDefaultSlider.SetValue(Register.DrawingManager.Color.r8);
		colorGreenDefaultSlider.SetValue(Register.DrawingManager.Color.g8);
		colorBlueDefaultSlider.SetValue(Register.DrawingManager.Color.b8);
		colorAlphaDefaultSlider.SetValue(Register.DrawingManager.Color.a8);
		colorPalette.CurrentColor = Register.DrawingManager.Color;
		roughnessDefaultSlider.SetValue(Register.DrawingManager.Roughness.v);
		metallicityDefaultSlider.SetValue(Register.DrawingManager.Metallicity.v);
		heightDefaultSlider.SetValue(Register.DrawingManager.Height.v);
		emissionDefaultColorPickerButton.Color = Register.DrawingManager.Emission;
		material1Button.SetBlockSignals(enable: false);
		material2Button.SetBlockSignals(enable: false);
		blendingModeOptionButton.SetBlockSignals(enable: false);
		colorDefaultColorPickerButton.SetBlockSignals(enable: false);
		colorRedDefaultSlider.SetBlockSignals(enable: false);
		colorGreenDefaultSlider.SetBlockSignals(enable: false);
		colorBlueDefaultSlider.SetBlockSignals(enable: false);
		colorAlphaDefaultSlider.SetBlockSignals(enable: false);
		roughnessDefaultSlider.SetBlockSignals(enable: false);
		metallicityDefaultSlider.SetBlockSignals(enable: false);
		heightDefaultSlider.SetBlockSignals(enable: false);
		emissionDefaultColorPickerButton.SetBlockSignals(enable: false);
	}

	public void Material1Pressed()
	{
		Register.DrawingManager.SetSettings(0);
	}

	public void Material2Pressed()
	{
		Register.DrawingManager.SetSettings(1);
	}

	public void ChangeColor(Color color)
	{
		colorDefaultColorPickerButton.Color = color;
		colorRedDefaultSlider.SetValue(color.r8);
		colorGreenDefaultSlider.SetValue(color.g8);
		colorBlueDefaultSlider.SetValue(color.b8);
		colorAlphaDefaultSlider.SetValue(color.a8);
		colorPalette.CurrentColor = color;
	}

	public void ColorChanged(Color color)
	{
		workspace.ChangeDrawingColor(color);
		colorRedDefaultSlider.SetValue(color.r8);
		colorGreenDefaultSlider.SetValue(color.g8);
		colorBlueDefaultSlider.SetValue(color.b8);
		colorAlphaDefaultSlider.SetValue(color.a8);
		colorPalette.CurrentColor = color;
	}

	public void ToggleColorAlphaChannel(bool pressed)
	{
		colorAlphaButton.Pressed = pressed;
	}

	public void ColorAlphaChannelToggled(bool pressed)
	{
		workspace.ChangeDrawingColorAlphaChannel(pressed);
	}

	public void ColorChannelChanged(float value)
	{
		Color color = new Color
		{
			r8 = (int)colorRedDefaultSlider.GetValue(),
			g8 = (int)colorGreenDefaultSlider.GetValue(),
			b8 = (int)colorBlueDefaultSlider.GetValue(),
			a8 = (int)colorAlphaDefaultSlider.GetValue()
		};
		colorDefaultColorPickerButton.Color = color;
		workspace.ChangeDrawingColor(color);
		colorPalette.CurrentColor = color;
	}

	public void ToggleColorPalette(bool pressed)
	{
		if (pressed)
		{
			colorPalettePanel.RectMinSize = new Vector2(0f, 312f);
			colorPalette.Visible = true;
		}
		else
		{
			colorPalettePanel.RectMinSize = new Vector2(0f, 40f);
			colorPalette.Visible = false;
		}
	}

	public void MouseEnteredColorPaletteButton()
	{
		colorPalettePanel.HintTooltip = "";
	}

	public void ColorPaletteColorSelected(Color color)
	{
		ChangeColor(color);
		workspace.ChangeDrawingColor(color);
	}

	public void ChangeRoughness(float value)
	{
		roughnessDefaultSlider.SetValue(value);
	}

	public void RoughnessChanged(float value)
	{
		workspace.ChangeDrawingRoughness(roughnessDefaultSlider.GetValue());
	}

	public void ChangeMetallicity(float value)
	{
		metallicityDefaultSlider.SetValue(value);
	}

	public void MetallicityChanged(float value)
	{
		workspace.ChangeDrawingMetallicity(metallicityDefaultSlider.GetValue());
	}

	public void ChangeHeight(float value)
	{
		heightDefaultSlider.SetValue(value);
	}

	public void HeightChanged(float value)
	{
		workspace.ChangeDrawingHeight(heightDefaultSlider.GetValue());
	}

	public void ChangeEmission(Color color)
	{
		emissionDefaultColorPickerButton.Color = color;
	}

	public void EmissionChanged(Color color)
	{
		workspace.ChangeDrawingEmission(color);
	}
}
