using System.Collections.Generic;
using Godot;

public class ToolsContainer : DefaultContainer
{
	[Export(PropertyHint.None, "")]
	protected NodePath guiNodePath;

	protected Gui gui;

	private int pressedToolButtonIndex;

	private List<TextureButton> toolButtonsList = new List<TextureButton>();

	public override void _Ready()
	{
		base._Ready();
		gui = GetNodeOrNull<Gui>(guiNodePath);
		for (int i = 0; i < GetChildCount(); i++)
		{
			if (GetChildOrNull<TextureButton>(i) != null)
			{
				TextureButton toolButton = GetChildOrNull<TextureButton>(i);
				toolButton.Connect(Signals.Pressed, this, "ToolButtonPressed");
				toolButtonsList.Add(toolButton);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		pressedToolButtonIndex = 0;
		for (int i = 0; i < toolButtonsList.Count; i++)
		{
			if (i == 0)
			{
				toolButtonsList[i].Pressed = true;
			}
			else
			{
				toolButtonsList[i].Pressed = false;
			}
			if (i == 10)
			{
				toolButtonsList[i].Disabled = true;
			}
		}
		gui.BrushSettingsContainer.Show();
		gui.FillingBucketSettingsContainer.Hide();
		gui.StampSettingsContainer.Hide();
	}

	public void ToolButtonPressed()
	{
		Register.MaterialContainer.EnableBlendingMode(pressed: true);
		Register.MaterialContainer.EnableMaterial(pressed: true);
		Register.MaterialContainer.EnableDecal(pressed: false, settingsEnabled: false);
		Register.DrawingManager.DrawingPreviewManager.ChangeBlendingMode(Register.DrawingManager.BlendingMode);
		Register.DrawingManager.DrawingPreviewManager.ChangeColor(Register.DrawingManager.Color);
		Register.DrawingManager.DrawingPreviewManager.ChangeRoughness(Register.DrawingManager.Roughness);
		Register.DrawingManager.DrawingPreviewManager.ChangeMetallicity(Register.DrawingManager.Metallicity);
		Register.DrawingManager.DrawingPreviewManager.ChangeHeight(Register.DrawingManager.Height);
		Register.DrawingManager.DrawingPreviewManager.ChangeEmission(Register.DrawingManager.Emission);
		bool hasChanged = false;
		bool wasUvEdit = false;
		if (pressedToolButtonIndex == 10)
		{
			wasUvEdit = true;
		}
		for (int i = 0; i < toolButtonsList.Count; i++)
		{
			if (toolButtonsList[i].Pressed && !hasChanged && i != pressedToolButtonIndex)
			{
				pressedToolButtonIndex = i;
				hasChanged = true;
			}
			toolButtonsList[i].Pressed = false;
		}
		toolButtonsList[pressedToolButtonIndex].Pressed = true;
		workspace.ChangeTool(pressedToolButtonIndex);
		if (pressedToolButtonIndex == 0 || pressedToolButtonIndex == 8)
		{
			gui.BrushSettingsContainer.Show();
			gui.BrushSettingsContainer.DistanceSliderVisible = false;
		}
		else
		{
			gui.BrushSettingsContainer.Hide();
		}
		if (pressedToolButtonIndex >= 1 && pressedToolButtonIndex <= 5)
		{
			gui.LineSettingsContainer.Show();
		}
		else
		{
			gui.LineSettingsContainer.Hide();
		}
		if (pressedToolButtonIndex == 6)
		{
			gui.FillingBucketSettingsContainer.Show();
			if (Register.DrawingManager.BucketTool.Mode == DrawingToolBucket.ModeEnum.DECAL)
			{
				Register.MaterialContainer.EnableDecal(pressed: true, settingsEnabled: true);
				Register.MaterialContainer.EnableMaterial(pressed: false);
				Register.MaterialContainer.ChangeDecalLabel("Decal");
				Register.MaterialContainer.ChangeDecalPreview(Register.DrawingManager.BucketTool.Decal.Thumbnail);
				Register.MaterialContainer.ChangeDecalHeightOffset(Register.DrawingManager.BucketTool.HeightOffset);
				Register.MaterialContainer.ToggleDecalHeightBlendSettings(enable: false);
				Register.MaterialContainer.UpdateDecalList();
			}
		}
		else
		{
			gui.FillingBucketSettingsContainer.Hide();
		}
		if (pressedToolButtonIndex == 7)
		{
			gui.StampSettingsContainer.Show();
			if (Register.DrawingManager.StampTool.Mode == DrawingToolStamp.ModeEnum.MASK)
			{
				Register.MaterialContainer.EnableDecal(pressed: true, settingsEnabled: false);
				Register.MaterialContainer.ChangeDecalLabel("Mask");
				Register.MaterialContainer.ChangeDecalPreview(Register.DrawingManager.StampTool.Mask.Thumbnail);
				Register.MaterialContainer.UpdateDecalList();
			}
			else if (Register.DrawingManager.StampTool.Mode == DrawingToolStamp.ModeEnum.DECAL)
			{
				Register.MaterialContainer.EnableDecal(pressed: true, settingsEnabled: true);
				Register.MaterialContainer.EnableMaterial(pressed: false);
				Register.MaterialContainer.ChangeDecalLabel("Decal");
				Register.MaterialContainer.ChangeDecalPreview(Register.DrawingManager.StampTool.Decal.Thumbnail);
				Register.MaterialContainer.ChangeDecalHeightOffset(Register.DrawingManager.StampTool.HeightOffset);
				Register.MaterialContainer.EnableDecalHeightBlend(Register.DrawingManager.StampTool.DoHeightBlending);
				Register.MaterialContainer.ChangeDecalHeightFactor(Register.DrawingManager.StampTool.HeightBlendingFactor);
				Register.MaterialContainer.ToggleDecalHeightBlendSettings(enable: true);
				Register.MaterialContainer.UpdateDecalList();
			}
		}
		else
		{
			gui.StampSettingsContainer.Hide();
		}
		if (pressedToolButtonIndex == 8)
		{
			gui.BrushSettingsContainer.DistanceSliderVisible = true;
			Register.MaterialContainer.EnableBlendingMode(pressed: false);
			Register.MaterialContainer.EnableMaterial(pressed: false);
		}
		else
		{
			gui.BrushSettingsContainer.DistanceSliderVisible = false;
		}
		if (pressedToolButtonIndex == 9)
		{
			gui.SelectSettingsContainer.Show();
			Register.SelectionManager.Activate(activated: true);
			Register.MaterialContainer.EnableBlendingMode(pressed: false);
			Register.MaterialContainer.EnableMaterial(pressed: false);
		}
		else
		{
			gui.SelectSettingsContainer.Hide();
			Register.SelectionManager.Activate(activated: false);
		}
		if (pressedToolButtonIndex == 10)
		{
			gui.UvEditSettingsContainer.Show();
			Register.PreviewspaceMeshManager.ActivateUVsEditing(activated: true);
			Register.MaterialContainer.EnableBlendingMode(pressed: false);
			Register.MaterialContainer.EnableMaterial(pressed: false);
		}
		else
		{
			gui.UvEditSettingsContainer.Hide();
			Register.PreviewspaceMeshManager.ActivateUVsEditing(activated: false);
		}
		if (wasUvEdit)
		{
			Register.BakeManager.UpdatePixelAffiliations();
		}
	}

	public void UpdateTooltip(int index, string text, string hotkey)
	{
		toolButtonsList[index].HintTooltip = text + " " + hotkey;
	}

	public void PressToolButton(int index)
	{
		toolButtonsList[index].Pressed = true;
		ToolButtonPressed();
	}

	public void SelectBrushTool()
	{
		toolButtonsList[0].Pressed = true;
		ToolButtonPressed();
	}

	public void SelectLineTool()
	{
		toolButtonsList[1].Pressed = true;
		ToolButtonPressed();
	}

	public void SelectRectangleTool()
	{
		toolButtonsList[2].Pressed = true;
		ToolButtonPressed();
	}

	public void SelectFilledRectangleTool()
	{
		toolButtonsList[3].Pressed = true;
		ToolButtonPressed();
	}

	public void SelectCircleTool()
	{
		toolButtonsList[4].Pressed = true;
		ToolButtonPressed();
	}

	public void SelectFilledCircleTool()
	{
		toolButtonsList[5].Pressed = true;
		ToolButtonPressed();
	}

	public void SelectFillingBucketTool()
	{
		toolButtonsList[6].Pressed = true;
		ToolButtonPressed();
	}

	public void SelectStampTool()
	{
		toolButtonsList[7].Pressed = true;
		ToolButtonPressed();
	}

	public void SelectSmearingTool()
	{
		PressToolButton(8);
		workspace.ChangeTool(8);
		Register.DrawingPreviewManager.Clear();
	}

	public void SelectMaskTool()
	{
		PressToolButton(9);
		workspace.ChangeTool(9);
		Register.DrawingPreviewManager.Clear();
	}

	public void SelectUvTool()
	{
		PressToolButton(10);
		workspace.ChangeTool(10);
		Register.DrawingPreviewManager.Clear();
	}

	public void DisableBakeing(bool disabled)
	{
		toolButtonsList[10].Disabled = disabled;
	}
}
