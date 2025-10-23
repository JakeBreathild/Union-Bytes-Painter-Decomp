using Godot;

public class ControlsWindowDialog : WindowDialog
{
	[Export(PropertyHint.None, "")]
	private NodePath richTextLabelNodePath;

	public RichTextLabel RichTextLabel;

	public override void _Ready()
	{
		base._Ready();
		Connect(Signals.Hide, this, "Hide");
		RichTextLabel = GetNodeOrNull<RichTextLabel>(richTextLabelNodePath);
		RichTextLabel.BbcodeText = "[b]Workspace[/b] \n\n";
		RichTextLabel.BbcodeText += "[Ctr]+[Z] \t - \t Undo \n";
		RichTextLabel.BbcodeText += "[Ctr]+[Y] \t - \t Redo \n";
		RichTextLabel.BbcodeText += "[MMB] \t - \t Move Camera \n";
		RichTextLabel.BbcodeText += "[Q] \t - \t Move Camera (In case your [MMB] does not work, like on my linux pc ;) )\n";
		RichTextLabel.BbcodeText += "[Wheel] \t - \t Zoom In & Out \n";
		RichTextLabel.BbcodeText += "[Shift]+[RMB] \t - \t Rotate Environment \n";
		RichTextLabel.BbcodeText += "[G] \t - \t Toggle Grid \n";
		RichTextLabel.BbcodeText += "[M] \t - \t Toggle Mirror \n";
		RichTextLabel.BbcodeText += "[Ctr]+[M] \t - \t Set active Mirror Position \n";
		RichTextLabel.BbcodeText += "[A] \t - \t Toggle Alpha Mask Display \n";
		RichTextLabel.BbcodeText += "[C] \t - \t Change Channel\n";
		RichTextLabel.BbcodeText += "[Ctr]+[C] \t - \t Select Single Channel \n";
		RichTextLabel.BbcodeText += "[Shift]+[C] \t - \t Select All Channel \n";
		RichTextLabel.BbcodeText += "\n\n[b]Preview Viewport[/b] \n\n";
		RichTextLabel.BbcodeText += "[MMB] \t - \t Move Camera \n";
		RichTextLabel.BbcodeText += "[Q] \t - \t Move Camera (In case your [MMB] does not work, like on my linux pc ;) )\n";
		RichTextLabel.BbcodeText += "[Shift]+[MMB] \t - \t Rotate Camera \n";
		RichTextLabel.BbcodeText += "[Shift]+[Q] \t - \t Rotate Camera (In case your [MMB] does not work, like on my linux pc ;) )\n";
		RichTextLabel.BbcodeText += "[Wheel] \t - \t Zoom In & Out \n";
		RichTextLabel.BbcodeText += "[Shift]+[RMB] \t - \t Rotate Environment \n";
		RichTextLabel.BbcodeText += "\n\n[b]Tools[/b] \n\n";
		RichTextLabel.BbcodeText += "[1]-[8] \t - \t Change Tool \n";
		RichTextLabel.BbcodeText += "[LMB] \t - \t Drawing Material \n";
		RichTextLabel.BbcodeText += "[RMB] \t - \t Pick Material \n";
		RichTextLabel.BbcodeText += "\n\n[b]Tools - Brush[/b] \n\n";
		RichTextLabel.BbcodeText += "[+] \t - \t Increase Brush Size \n";
		RichTextLabel.BbcodeText += "[-] \t - \t Decrease Brush Size \n";
		RichTextLabel.BbcodeText += "[S] \t - \t Change Brush Shape \n";
		RichTextLabel.BbcodeText += "\n\n[b]Tools - Line[/b] \n\n";
		RichTextLabel.BbcodeText += "[Shift] \t - \t Lock angle in 45 degree steps \n";
		RichTextLabel.BbcodeText += "\n\n[b]Tools - Bucket[/b] \n\n";
		RichTextLabel.BbcodeText += "[Alt]+[LMB] \t - \t Fill Islands (in previewspace) \n";
		RichTextLabel.BbcodeText += "\n\n[b]Tools - Stamp[/b] \n\n";
		RichTextLabel.BbcodeText += "[S] \t - \t Rotate Stamp left \n";
		RichTextLabel.BbcodeText += "[D] \t - \t Rotate Stamp right \n";
		RichTextLabel.BbcodeText += "[F] \t - \t Mirroring Stamp\n";
		RichTextLabel.BbcodeText += "\n\n[b]Library[/b] \n\n";
		RichTextLabel.BbcodeText += "[LMB] \t - \t Select Material\n";
		RichTextLabel.BbcodeText += "[RMB] \t - \t Opens Material Menu\n";
	}

	public new void PopupCentered(Vector2? size = null)
	{
		InputManager.WindowShown();
		base.PopupCentered(size);
	}

	public new void Show()
	{
		InputManager.WindowShown();
		base.Show();
	}

	public new void Hide()
	{
		base.Hide();
		InputManager.WindowHidden();
		InputManager.SkipInput = true;
	}
}
