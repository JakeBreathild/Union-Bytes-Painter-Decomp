using System.IO;
using Godot;

public class NewWindowDialog : WindowDialog
{
	private Workspace workspace;

	private ItemList sizesItemList;

	private string worksheetName = "Unnamed";

	private LineEdit nameLineEdit;

	private int worksheetWidth = 32;

	private LineEdit widthLineEdit;

	private int worksheetHeight = 32;

	private LineEdit heightLineEdit;

	private bool worksheetTileable;

	private CheckButton tileableCheckButton;

	[Export(PropertyHint.None, "")]
	private NodePath previewMeshDefaultFileDialogNodePath;

	private PreviewFileDialog previewMeshDefaultFileDialog;

	private string previewMesh = "";

	private LineEdit previewMeshLineEdit;

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		string nodeGroupPath = "SC/VC/Worksheet/";
		sizesItemList = GetNodeOrNull<ItemList>(nodeGroupPath + "Sizes");
		sizesItemList.AddItem("8 x 8");
		sizesItemList.AddItem("16 x 16");
		sizesItemList.AddItem("16 x 32");
		sizesItemList.AddItem("32 x 32");
		sizesItemList.AddItem("32 x 64");
		sizesItemList.AddItem("48 x 48");
		sizesItemList.AddItem("64 x 64");
		sizesItemList.AddItem("96 x 96");
		sizesItemList.AddItem("128 x 128");
		sizesItemList.AddItem("256 x 256");
		sizesItemList.AddItem("380 x 380");
		sizesItemList.AddItem("512 x 512");
		sizesItemList.Connect(Signals.ItemSelected, this, "SelectWorksheetSize");
		nameLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Name");
		widthLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Width");
		heightLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Height");
		tileableCheckButton = GetNodeOrNull<CheckButton>(nodeGroupPath + "Tileable");
		nodeGroupPath = "SC/VC/Mesh/";
		previewMeshDefaultFileDialog = GetNodeOrNull<PreviewFileDialog>(previewMeshDefaultFileDialogNodePath);
		previewMeshDefaultFileDialog.Connect(Signals.FileSelected, this, "SetPreviewMesh");
		previewMeshLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "PreviewMesh");
		Connect(Signals.Hide, this, "Hide");
	}

	public new void PopupCentered(Vector2? size = null)
	{
		InputManager.WindowShown();
		Reset();
		base.PopupCentered(size);
	}

	public new void Show()
	{
		InputManager.WindowShown();
		Reset();
		base.Show();
	}

	public new void Hide()
	{
		base.Hide();
		InputManager.WindowHidden();
		InputManager.SkipInput = true;
	}

	public void Reset()
	{
		GetNodeOrNull<ScrollContainer>("SC").ScrollVertical = 0;
		sizesItemList.Select(3);
		worksheetName = "Unnamed";
		nameLineEdit.Text = worksheetName;
		worksheetWidth = 32;
		widthLineEdit.Text = worksheetWidth.ToString();
		worksheetHeight = 32;
		heightLineEdit.Text = worksheetHeight.ToString();
		worksheetTileable = false;
		tileableCheckButton.Pressed = worksheetTileable;
		previewMesh = "";
		previewMeshLineEdit.Text = previewMesh;
	}

	public void ChangeWorksheetNameReleaseFocus(string name)
	{
		nameLineEdit.ReleaseFocus();
	}

	public void ChangeWorksheetName(string name)
	{
		worksheetName = name;
	}

	public void SelectWorksheetSize(int index)
	{
		string itemString = sizesItemList.GetItemText(index);
		widthLineEdit.Text = itemString.Split(new char[1] { 'x' })[0].Trim();
		ChangeWorksheetWidth(widthLineEdit.Text);
		heightLineEdit.Text = itemString.Split(new char[1] { 'x' })[1].Trim();
		ChangeWorksheetHeight(heightLineEdit.Text);
		sizesItemList.Select(index);
	}

	public void ChangeWorksheetWidthReleaseFocus(string name)
	{
		widthLineEdit.ReleaseFocus();
	}

	public void ChangeWorksheetWidth(string widthString)
	{
		sizesItemList.UnselectAll();
		if (widthString.IsValidInteger())
		{
			worksheetWidth = widthString.ToInt();
			if (worksheetWidth < 1)
			{
				worksheetWidth = 1;
			}
			else if (worksheetWidth > Settings.MaximumTextureSize)
			{
				worksheetWidth = Settings.MaximumTextureSize;
			}
		}
	}

	public void ChangeWorksheetHeightReleaseFocus(string name)
	{
		heightLineEdit.ReleaseFocus();
	}

	public void ChangeWorksheetHeight(string heightString)
	{
		sizesItemList.UnselectAll();
		if (heightString.IsValidInteger())
		{
			worksheetHeight = heightString.ToInt();
			if (worksheetHeight < 1)
			{
				worksheetHeight = 1;
			}
			else if (worksheetHeight > Settings.MaximumTextureSize)
			{
				worksheetHeight = Settings.MaximumTextureSize;
			}
		}
	}

	public void ChangeWorksheetTileable(bool pressed)
	{
		worksheetTileable = pressed;
	}

	public void OpenPreviewMeshFileDialog()
	{
		string path = Settings.ImportPath;
		if (!path.Empty())
		{
			previewMeshDefaultFileDialog.CurrentDir = path;
		}
		previewMeshDefaultFileDialog.SetWindowHidden = false;
		previewMeshDefaultFileDialog.PopupCentered();
	}

	public void ResetPreviewMesh()
	{
		previewMesh = "";
		previewMeshLineEdit.Text = previewMesh;
	}

	public void SetPreviewMesh(string file)
	{
		if (file != "")
		{
			if (System.IO.File.Exists(file) && file.Substring(file.Length - 3, 3) == "obj")
			{
				previewMesh = file;
				previewMeshLineEdit.Text = previewMesh;
			}
			else
			{
				Gui.MsgAcceptDialogPopupCentered("Only *.obj files are supported!");
			}
		}
	}

	public void CreateNewWorksheet()
	{
		workspace.CreateWorksheet(worksheetWidth, worksheetHeight, worksheetTileable, worksheetName);
		if (previewMesh != "")
		{
			Register.PreviewspaceMeshManager.SetMeshScale(1f);
			Register.PreviewspaceMeshManager.SetUvScale(1f);
			Register.PreviewspaceMeshManager.LoadMesh(previewMesh);
			Register.CollisionManager.Update();
			workspace.CameraManager.ResetPreviewspaceCamera();
			workspace.BakeManager.Update(workspace.Worksheet);
			Register.Gui.DisableBakeing(disabled: false);
			Register.Gui.DisplaySettingsContainer.DisableWireframe(disabled: false);
		}
		Hide();
	}
}
