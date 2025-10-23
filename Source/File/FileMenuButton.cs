using System.IO;
using Godot;

public class FileMenuButton : DefaultMenuButton
{
	private enum FileIdEnum
	{
		NEW,
		NEW_FROM_TEXTURE,
		OPEN,
		OPEN_RECENT,
		SAVE,
		SAVE_AS,
		IMPORT_MESH,
		IMPORT_TEXTURE,
		IMPORT_COLORPALETTE,
		EXPORT_MESH,
		EXPORT_TEXTURES,
		REPEAT_EXPORT_TEXTURES,
		SETTINGS,
		PROJECT_SETTINGS,
		EXIT
	}

	[Export(PropertyHint.None, "")]
	private NodePath newWindowDialogNodePath;

	private NewWindowDialog newWindowDialog;

	[Export(PropertyHint.None, "")]
	private NodePath importFileDialogNodePath;

	private PreviewFileDialog importFileDialog;

	[Export(PropertyHint.None, "")]
	private NodePath importWindowDialogNodePath;

	private ImportWindowDialog importWindowDialog;

	[Export(PropertyHint.None, "")]
	private NodePath openProjectDialogNodePath;

	private PreviewFileDialog openProjectDialog;

	[Export(PropertyHint.None, "")]
	private NodePath saveProjectDialogNodePath;

	private PreviewFileDialog saveProjectDialog;

	[Export(PropertyHint.None, "")]
	private NodePath importTextureFileDialogNodePath;

	private PreviewFileDialog importTextureFileDialog;

	[Export(PropertyHint.None, "")]
	private NodePath importTextureWindowDialogNodePath;

	private ImportTextureWindowDialog importTextureWindowDialog;

	[Export(PropertyHint.None, "")]
	private NodePath importColorPaletteFileDialogNodePath;

	private PreviewFileDialog importColorPaletteFileDialog;

	[Export(PropertyHint.None, "")]
	private NodePath exportFileDialogNodePath;

	private PreviewFileDialog exportFileDialog;

	[Export(PropertyHint.None, "")]
	private NodePath exportWindowDialogNodePath;

	private ExportWindowDialog exportWindowDialog;

	[Export(PropertyHint.None, "")]
	private NodePath projectsSettingsWindowDialogNodePath;

	private ProjectsSettingsWindowDialog projectsSettingsWindowDialog;

	[Export(PropertyHint.None, "")]
	private NodePath settingsWindowDialogNodePath;

	private SettingsWindowDialog settingsWindowDialog;

	private PopupMenu openRecentPopupMenu;

	public SettingsWindowDialog SettingsWindowDialog => settingsWindowDialog;

	public PopupMenu OpenRecentPopupMenu => openRecentPopupMenu;

	public override void _Ready()
	{
		base._Ready();
		base.RectMinSize = new Vector2(Resources.DefaultFont.GetStringSize(Tr(base.Text)).x, 0f);
		openRecentPopupMenu = new PopupMenu();
		openRecentPopupMenu.Name = "Open Recent";
		openRecentPopupMenu.AddFontOverride("font", Resources.DefaultFont);
		openRecentPopupMenu.Connect(Signals.IdPressed, this, "OpenRecentItemSelected");
		popupMenu.AddFontOverride("font", Resources.DefaultFont);
		popupMenu.AddItem(Tr("FILE_NEW") + "...", 0);
		popupMenu.AddItem("New From Texture...", 1);
		popupMenu.AddItem("Open...", 2);
		popupMenu.AddChild(openRecentPopupMenu);
		popupMenu.AddSubmenuItem("Open Recent", "Open Recent", 3);
		popupMenu.AddItem("Save", 4);
		popupMenu.AddItem("Save As...", 5);
		popupMenu.SetItemDisabled(4, disabled: true);
		popupMenu.SetItemTooltip(4, "Not available in the demo!");
		popupMenu.SetItemDisabled(5, disabled: true);
		popupMenu.SetItemTooltip(5, "Not available in the demo!");
		popupMenu.AddSeparator();
		popupMenu.AddItem("Import Mesh...", 6);
		popupMenu.AddItem("Import Texture...", 7);
		popupMenu.AddItem("Import Color Palette...", 8);
		popupMenu.AddItem("Export Mesh...", 9);
		popupMenu.AddItem("Export Textures...", 10);
		popupMenu.AddItem("Repeat Export Textures", 11);
		popupMenu.SetItemDisabled(10, disabled: true);
		popupMenu.SetItemTooltip(10, "Not available in the demo!");
		popupMenu.SetItemDisabled(12, disabled: true);
		popupMenu.AddSeparator();
		popupMenu.AddItem(Tr("FILE_SETTINGS") + "...", 12);
		popupMenu.AddItem("Project Settings...", 13);
		popupMenu.AddSeparator();
		popupMenu.AddItem("Exit", 14);
		popupMenu.Connect(Signals.IdPressed, this, "ItemSelected");
		newWindowDialog = GetNodeOrNull<NewWindowDialog>(newWindowDialogNodePath);
		newWindowDialog.PopupExclusive = true;
		importFileDialog = GetNodeOrNull<PreviewFileDialog>(importFileDialogNodePath);
		importFileDialog.AddFontOverride("font", Resources.DefaultFont);
		importFileDialog.PopupExclusive = true;
		importFileDialog.WindowTitle = "New From Texture";
		importWindowDialog = GetNodeOrNull<ImportWindowDialog>(importWindowDialogNodePath);
		importWindowDialog.PopupExclusive = true;
		importFileDialog.Connect(Signals.FileSelected, this, "ImportFileDialogFileSelected");
		importFileDialog.Connect(Signals.FileSelected, importWindowDialog, "PopupCentered");
		openProjectDialog = GetNodeOrNull<PreviewFileDialog>(openProjectDialogNodePath);
		openProjectDialog.AddFontOverride("font", Resources.DefaultFont);
		openProjectDialog.PopupExclusive = true;
		openProjectDialog.WindowTitle = "Open Project File";
		openProjectDialog.Connect(Signals.FileSelected, this, "OpenProject");
		saveProjectDialog = GetNodeOrNull<PreviewFileDialog>(saveProjectDialogNodePath);
		saveProjectDialog.AddFontOverride("font", Resources.DefaultFont);
		saveProjectDialog.PopupExclusive = true;
		saveProjectDialog.WindowTitle = "Save Project File";
		saveProjectDialog.Connect(Signals.FileSelected, this, "SaveProject");
		importTextureFileDialog = GetNodeOrNull<PreviewFileDialog>(importTextureFileDialogNodePath);
		importTextureFileDialog.AddFontOverride("font", Resources.DefaultFont);
		importTextureFileDialog.PopupExclusive = true;
		importTextureFileDialog.WindowTitle = "Import Texture";
		importTextureWindowDialog = GetNodeOrNull<ImportTextureWindowDialog>(importTextureWindowDialogNodePath);
		importTextureWindowDialog.PopupExclusive = true;
		importTextureFileDialog.Connect(Signals.FileSelected, this, "ImportTextureFileDialogFileSelected");
		importTextureFileDialog.Connect(Signals.FileSelected, importTextureWindowDialog, "PopupCentered");
		importColorPaletteFileDialog = GetNodeOrNull<PreviewFileDialog>(importColorPaletteFileDialogNodePath);
		importColorPaletteFileDialog.AddFontOverride("font", Resources.DefaultFont);
		importColorPaletteFileDialog.PopupExclusive = true;
		importColorPaletteFileDialog.WindowTitle = "Import Color Palette";
		importColorPaletteFileDialog.Connect(Signals.FileSelected, Register.ColorPalette, "ImportColorPalette");
		exportFileDialog = GetNodeOrNull<PreviewFileDialog>(exportFileDialogNodePath);
		exportFileDialog.AddFontOverride("font", Resources.DefaultFont);
		exportFileDialog.PopupExclusive = true;
		exportFileDialog.WindowTitle = "Export Textures";
		exportFileDialog.Connect(Signals.DirSelected, this, "Export");
		exportWindowDialog = GetNodeOrNull<ExportWindowDialog>(exportWindowDialogNodePath);
		exportWindowDialog.PopupExclusive = true;
		settingsWindowDialog = GetNodeOrNull<SettingsWindowDialog>(settingsWindowDialogNodePath);
		settingsWindowDialog.PopupExclusive = true;
		projectsSettingsWindowDialog = GetNodeOrNull<ProjectsSettingsWindowDialog>(projectsSettingsWindowDialogNodePath);
		projectsSettingsWindowDialog.PopupExclusive = true;
	}

	public void Export(string path)
	{
		if (path != "")
		{
			exportFileDialog.SetWindowHidden = false;
			exportWindowDialog.ExportPath = path;
			exportWindowDialog.PopupCentered();
			Settings.ExportPath = System.IO.Path.GetFullPath(path);
		}
	}

	public void OpenProject(string file)
	{
		if (file != "")
		{
			switch (FileSystem.OpenProject(workspace, file))
			{
			case FileSystem.ErrorEnum.FILE:
				Gui.MsgAcceptDialogPopupCentered("File not found:\n" + file);
				break;
			case FileSystem.ErrorEnum.VERSION:
				Gui.MsgAcceptDialogPopupCentered("Incorrect file version!");
				break;
			case FileSystem.ErrorEnum.NONE:
				AddOpenRecentPopupMenuItem(file);
				break;
			}
			Settings.ProjectsPath = System.IO.Path.GetDirectoryName(file);
		}
	}

	public void SaveProject(string file)
	{
		if (file != "")
		{
			switch (FileSystem.SaveProject(workspace, file))
			{
			case FileSystem.ErrorEnum.FILE:
				Gui.MsgAcceptDialogPopupCentered("File name or path incorrect:\n" + file);
				break;
			case FileSystem.ErrorEnum.NONE:
				AddOpenRecentPopupMenuItem(file);
				break;
			}
			Settings.ProjectsPath = System.IO.Path.GetDirectoryName(file);
		}
	}

	public void UpdateOpenRecentPopupMenu()
	{
		OpenRecentPopupMenu.Clear();
		for (int i = 0; i < Settings.RecentlyOpenedList.Count; i++)
		{
			OpenRecentPopupMenu.AddItem(System.IO.Path.GetFileName(Settings.RecentlyOpenedList[i]));
		}
	}

	public void AddOpenRecentPopupMenuItem(string file)
	{
		if (Settings.RecentlyOpenedHashSet.Add(file))
		{
			Settings.RecentlyOpenedList.Insert(0, file);
		}
		else
		{
			Settings.RecentlyOpenedList.Remove(file);
			Settings.RecentlyOpenedList.Insert(0, file);
		}
		if (Settings.RecentlyOpenedList.Count > 16)
		{
			file = Settings.RecentlyOpenedList[Settings.RecentlyOpenedList.Count - 1];
			Settings.RecentlyOpenedList.Remove(file);
			Settings.RecentlyOpenedHashSet.Remove(file);
		}
		UpdateOpenRecentPopupMenu();
	}

	public void OpenRecentItemSelected(int index)
	{
		string file = Settings.RecentlyOpenedList[index];
		if (System.IO.File.Exists(file))
		{
			OpenProject(file);
			return;
		}
		Settings.RecentlyOpenedList.Remove(file);
		Settings.RecentlyOpenedHashSet.Remove(file);
		UpdateOpenRecentPopupMenu();
	}

	public void DisableBakeing(bool disabled)
	{
	}

	public void DisableRepeatExportTextures(bool disabled)
	{
		popupMenu.SetItemDisabled(12, disabled);
	}

	public void ItemSelected(int id)
	{
		string path = "";
		switch ((FileIdEnum)id)
		{
		case FileIdEnum.NEW:
			newWindowDialog.PopupCentered();
			break;
		case FileIdEnum.NEW_FROM_TEXTURE:
			path = Settings.ImportPath;
			if (!path.Empty())
			{
				importFileDialog.CurrentDir = path;
			}
			importFileDialog.Update();
			importFileDialog.PopupCentered();
			break;
		case FileIdEnum.SAVE:
			SaveProject();
			break;
		case FileIdEnum.SAVE_AS:
			SaveProjectAs();
			break;
		case FileIdEnum.OPEN:
			path = Settings.ProjectsPath;
			if (!path.Empty())
			{
				openProjectDialog.CurrentDir = path;
			}
			openProjectDialog.Update();
			openProjectDialog.PopupCentered();
			break;
		case FileIdEnum.EXIT:
			GetTree().Quit();
			break;
		case FileIdEnum.IMPORT_TEXTURE:
			path = Settings.ImportPath;
			if (!path.Empty())
			{
				importTextureFileDialog.CurrentDir = path;
			}
			importTextureFileDialog.Update();
			importTextureFileDialog.PopupCentered();
			break;
		case FileIdEnum.EXPORT_TEXTURES:
			path = Settings.ExportPath;
			if (!path.Empty())
			{
				exportFileDialog.CurrentDir = path;
			}
			exportFileDialog.Update();
			exportFileDialog.PopupCentered();
			break;
		case FileIdEnum.REPEAT_EXPORT_TEXTURES:
			exportWindowDialog.Export();
			break;
		case FileIdEnum.IMPORT_MESH:
			gui.SettingsContainer.OpenLoadPreviewMeshFileDialog();
			break;
		case FileIdEnum.IMPORT_COLORPALETTE:
			path = Settings.PresetsPath;
			if (!path.Empty())
			{
				importColorPaletteFileDialog.CurrentDir = path;
			}
			importColorPaletteFileDialog.Update();
			importColorPaletteFileDialog.PopupCentered();
			break;
		case FileIdEnum.SETTINGS:
			settingsWindowDialog.PopupCentered();
			break;
		case FileIdEnum.PROJECT_SETTINGS:
			projectsSettingsWindowDialog.PopupCentered();
			break;
		case FileIdEnum.OPEN_RECENT:
		case FileIdEnum.EXPORT_MESH:
			break;
		}
	}

	public override void Reset()
	{
		base.Reset();
		popupMenu.SetItemDisabled(10, disabled: true);
		popupMenu.SetItemDisabled(12, disabled: true);
	}

	public void ImportFileDialogFileSelected(string file)
	{
		file = file.Trim();
		if (file != "")
		{
			importFileDialog.SetWindowHidden = false;
		}
	}

	public void ImportTextureFileDialogFileSelected(string file)
	{
		file = file.Trim();
		if (file != "")
		{
			importTextureFileDialog.SetWindowHidden = false;
		}
	}

	public void SaveProjectAs()
	{
		string path = Settings.ProjectsPath;
		if (!path.Empty())
		{
			saveProjectDialog.CurrentDir = path;
		}
		saveProjectDialog.CurrentFile = workspace.Worksheet.SheetName + ".ubpd";
		saveProjectDialog.Update();
		saveProjectDialog.PopupCentered();
	}

	public void SaveProject()
	{
		if (workspace.Worksheet.FileName != null && workspace.Worksheet.FileName != "" && System.IO.Path.GetExtension(workspace.Worksheet.FileName) != ".dat")
		{
			FileSystem.SaveProject(workspace, workspace.Worksheet.FileName);
		}
		else
		{
			SaveProjectAs();
		}
	}
}
