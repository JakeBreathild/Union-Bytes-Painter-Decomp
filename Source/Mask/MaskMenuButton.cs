using Godot;

public class MaskMenuButton : DefaultMenuButton
{
	private enum MaskIdEnum
	{
		GRADIENT,
		NOISE,
		AMBIENTOCCLUSION,
		CURVATURE
	}

	[Export(PropertyHint.None, "")]
	private NodePath maskWindowDialogNodePath;

	private MaskWindowDialog maskWindowDialog;

	public override void _Ready()
	{
		base._Ready();
		base.RectMinSize = new Vector2(Resources.DefaultFont.GetStringSize(Tr(base.Text)).x, 0f);
		popupMenu.AddFontOverride("font", Resources.DefaultFont);
		popupMenu.AddItem("Gradient...", 0);
		popupMenu.AddItem("Noise...", 1);
		popupMenu.AddItem("Ambient Occlusion...", 2);
		popupMenu.AddItem("Curvature...", 3);
		popupMenu.Connect(Signals.IdPressed, this, "ItemSelected");
		maskWindowDialog = GetNodeOrNull<MaskWindowDialog>(maskWindowDialogNodePath);
		maskWindowDialog.PopupExclusive = true;
	}

	public void ItemSelected(int id)
	{
		switch ((MaskIdEnum)id)
		{
		case MaskIdEnum.GRADIENT:
			maskWindowDialog.DrawingToolBefore = Register.DrawingManager.Tool;
			Register.DrawingManager.Tool = DrawingManager.ToolEnum.DISABLED;
			maskWindowDialog.Mask = MaskWindowDialog.MaskEnum.GRADIENT;
			maskWindowDialog.PopupCentered();
			break;
		case MaskIdEnum.NOISE:
			maskWindowDialog.DrawingToolBefore = Register.DrawingManager.Tool;
			Register.DrawingManager.Tool = DrawingManager.ToolEnum.DISABLED;
			maskWindowDialog.Mask = MaskWindowDialog.MaskEnum.NOISE;
			maskWindowDialog.PopupCentered();
			break;
		case MaskIdEnum.AMBIENTOCCLUSION:
			maskWindowDialog.DrawingToolBefore = Register.DrawingManager.Tool;
			Register.DrawingManager.Tool = DrawingManager.ToolEnum.DISABLED;
			maskWindowDialog.Mask = MaskWindowDialog.MaskEnum.AMBIENTOCCLUSION;
			maskWindowDialog.PopupCentered();
			break;
		case MaskIdEnum.CURVATURE:
			maskWindowDialog.DrawingToolBefore = Register.DrawingManager.Tool;
			Register.DrawingManager.Tool = DrawingManager.ToolEnum.DISABLED;
			maskWindowDialog.Mask = MaskWindowDialog.MaskEnum.CURVATURE;
			maskWindowDialog.PopupCentered();
			break;
		}
	}
}
