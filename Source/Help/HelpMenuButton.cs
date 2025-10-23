using Godot;

public class HelpMenuButton : DefaultMenuButton
{
	private enum HelpIDEnum
	{
		CONTROLS,
		ABOUT
	}

	[Export(PropertyHint.None, "")]
	private NodePath controlsWindowDialogNodePath = "";

	private ControlsWindowDialog controlsWindowDialog;

	[Export(PropertyHint.None, "")]
	private NodePath aboutWindowDialogNodePath = "";

	private AboutWindowDialog aboutWindowDialog;

	public override void _Ready()
	{
		base._Ready();
		base.RectMinSize = new Vector2(Resources.DefaultFont.GetStringSize(Tr(base.Text)).x, 0f);
		popupMenu.AddFontOverride("font", Resources.DefaultFont);
		popupMenu.AddItem("Controls", 0);
		popupMenu.SetItemDisabled(0, disabled: true);
		popupMenu.SetItemTooltip(0, "Moved To: File -> Settings -> Key Bindings");
		popupMenu.AddItem("About", 1);
		popupMenu.Connect(Signals.IdPressed, this, "ItemSelected");
		controlsWindowDialog = GetNodeOrNull<ControlsWindowDialog>(controlsWindowDialogNodePath);
		controlsWindowDialog.PopupExclusive = true;
		aboutWindowDialog = GetNodeOrNull<AboutWindowDialog>(aboutWindowDialogNodePath);
		aboutWindowDialog.PopupExclusive = true;
	}

	public void ItemSelected(int id)
	{
		switch ((HelpIDEnum)id)
		{
		case HelpIDEnum.CONTROLS:
			controlsWindowDialog.PopupCentered();
			break;
		case HelpIDEnum.ABOUT:
			aboutWindowDialog.PopupCentered();
			break;
		}
	}
}
