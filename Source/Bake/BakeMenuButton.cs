using Godot;

public class BakeMenuButton : DefaultMenuButton
{
	private enum BakeIdEnum
	{
		VISIBLE_WIREFRAME,
		WIREFRAME,
		FACES,
		ISLANDS,
		MASK
	}

	[Export(PropertyHint.None, "")]
	private NodePath bakeWindowDialogNodePath;

	private BakeWindowDialog bakeWindowDialog;

	public BakeWindowDialog BakeWindowDialog => bakeWindowDialog;

	public override void _Ready()
	{
		base._Ready();
		base.RectMinSize = new Vector2(Resources.DefaultFont.GetStringSize(Tr(base.Text)).x, 0f);
		popupMenu.AddFontOverride("font", Resources.DefaultFont);
		popupMenu.AddItem("Show Wireframe", 0);
		popupMenu.AddSeparator();
		popupMenu.SetItemAsCheckable(0, enable: true);
		popupMenu.SetItemChecked(0, @checked: true);
		popupMenu.AddItem("Wireframe", 1);
		popupMenu.AddItem("Faces", 2);
		popupMenu.AddItem("Islands", 3);
		popupMenu.AddSeparator();
		popupMenu.AddItem("Mask...", 4);
		popupMenu.Connect(Signals.IdPressed, this, "ItemSelected");
		bakeWindowDialog = GetNodeOrNull<BakeWindowDialog>(bakeWindowDialogNodePath);
		bakeWindowDialog.PopupExclusive = true;
	}

	public override void Reset()
	{
		base.Reset();
		popupMenu.SetItemChecked(0, @checked: true);
		popupMenu.SetItemDisabled(0, disabled: true);
		popupMenu.SetItemDisabled(2, disabled: true);
		popupMenu.SetItemDisabled(3, disabled: true);
		popupMenu.SetItemDisabled(4, disabled: true);
		popupMenu.SetItemDisabled(6, disabled: true);
	}

	public void DisableBakeing(bool disabled)
	{
		popupMenu.SetItemDisabled(0, disabled);
		popupMenu.SetItemDisabled(2, disabled);
		popupMenu.SetItemDisabled(3, disabled);
		popupMenu.SetItemDisabled(4, disabled);
		popupMenu.SetItemDisabled(6, disabled);
	}

	public void ItemSelected(int id)
	{
		switch ((BakeIdEnum)id)
		{
		case BakeIdEnum.VISIBLE_WIREFRAME:
			popupMenu.ToggleItemChecked(0);
			Register.PreviewspaceMeshManager.UvLayoutVisible = popupMenu.IsItemChecked(0);
			gui.DisplaySettingsContainer.ToggleWireframe(Register.PreviewspaceMeshManager.UvLayoutVisible);
			break;
		case BakeIdEnum.WIREFRAME:
			workspace.BakeManager.BakeWireframe();
			Register.Gui.LayerPanel.Reset();
			break;
		case BakeIdEnum.FACES:
		{
			string layerName = "Bake: Faces";
			((LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER)).LayerCommandType = LayerCommand.LayerCommandTypeEnum.ADDNEW;
			workspace.Worksheet.History.StopRecording("Layer Added [" + layerName + "]");
			Layer layer = workspace.Worksheet.Data.Layer;
			layer.Name = layerName;
			layer.RoughnessChannel.IsVisible = false;
			layer.MetallicityChannel.IsVisible = false;
			layer.HeightChannel.IsVisible = false;
			layer.EmissionChannel.IsVisible = false;
			BakeCommand obj2 = (BakeCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.BAKE);
			obj2.Channel = layer.ColorChannel;
			obj2.ColorArray = workspace.BakeManager.BakeFaces();
			workspace.Worksheet.History.StopRecording("Faces Bake [" + workspace.Worksheet.Data.Layer.Name + "]");
			layer.ColorChannel.DetectContentArea();
			workspace.Worksheet.Data.CombineLayers(Layer.ChannelEnum.COLOR);
			Register.Gui.LayerPanel.Reset();
			break;
		}
		case BakeIdEnum.ISLANDS:
		{
			string layerName = "Bake: Islands";
			((LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER)).LayerCommandType = LayerCommand.LayerCommandTypeEnum.ADDNEW;
			workspace.Worksheet.History.StopRecording("Layer Added [" + layerName + "]");
			Layer layer = workspace.Worksheet.Data.Layer;
			layer.Name = layerName;
			layer.RoughnessChannel.IsVisible = false;
			layer.MetallicityChannel.IsVisible = false;
			layer.HeightChannel.IsVisible = false;
			layer.EmissionChannel.IsVisible = false;
			BakeCommand obj = (BakeCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.BAKE);
			obj.Channel = layer.ColorChannel;
			obj.ColorArray = workspace.BakeManager.BakeIslands();
			workspace.Worksheet.History.StopRecording("Faces Islands [" + workspace.Worksheet.Data.Layer.Name + "]");
			layer.ColorChannel.DetectContentArea();
			workspace.Worksheet.Data.CombineLayers(Layer.ChannelEnum.COLOR);
			Register.Gui.LayerPanel.Reset();
			break;
		}
		case BakeIdEnum.MASK:
			bakeWindowDialog.DrawingToolBefore = Register.DrawingManager.Tool;
			Register.DrawingManager.Tool = DrawingManager.ToolEnum.DISABLED;
			bakeWindowDialog.PopupCentered();
			break;
		}
	}

	public void ToggleWireframe(bool pressed)
	{
		popupMenu.SetItemChecked(0, pressed);
	}
}
