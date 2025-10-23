using Godot;

public class FilterMenuButton : DefaultMenuButton
{
	private enum FilterIdEnum
	{
		CORRECTION,
		BLUR,
		FLOW
	}

	[Export(PropertyHint.None, "")]
	private NodePath filterWindowDialogNodePath;

	private FilterWindowDialog filterWindowDialog;

	public override void _Ready()
	{
		base._Ready();
		base.RectMinSize = new Vector2(Resources.DefaultFont.GetStringSize(Tr(base.Text)).x, 0f);
		popupMenu.AddFontOverride("font", Resources.DefaultFont);
		popupMenu.AddItem("Correction...", 0);
		popupMenu.AddItem("Blur...", 1);
		popupMenu.AddItem("Flow...", 2);
		popupMenu.Connect(Signals.IdPressed, this, "ItemSelected");
		filterWindowDialog = GetNodeOrNull<FilterWindowDialog>(filterWindowDialogNodePath);
		filterWindowDialog.PopupExclusive = true;
	}

	public void ItemSelected(int id)
	{
		switch ((FilterIdEnum)id)
		{
		case FilterIdEnum.CORRECTION:
			filterWindowDialog.Filter = FilterWindowDialog.FilterEnum.CORRECTION;
			filterWindowDialog.PopupCentered();
			break;
		case FilterIdEnum.BLUR:
			filterWindowDialog.Filter = FilterWindowDialog.FilterEnum.BLUR;
			filterWindowDialog.PopupCentered();
			break;
		case FilterIdEnum.FLOW:
			filterWindowDialog.Filter = FilterWindowDialog.FilterEnum.FLOW;
			filterWindowDialog.PopupCentered();
			break;
		}
	}
}
