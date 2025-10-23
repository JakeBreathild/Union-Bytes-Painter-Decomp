using System.Collections.Generic;
using Godot;

public class LibraryManager : Node
{
	private enum PopupMenuItemEnum
	{
		SHADES,
		OVERWRITE,
		DELETE
	}

	private Workspace workspace;

	private Gui gui;

	private ThumbnailRenderer thumbnailRenderer;

	private Container container;

	private HScrollBar hScrollBar;

	private TextureRect[] textureRectsArray;

	private Panel selectionPanel;

	private PopupMenu popupMenu;

	private bool hoverPanelState;

	private GridContainer gridContainer;

	private Texture syncIconTexture;

	private readonly Color fullColor = new Color(1f, 1f, 1f);

	private readonly Color syncIconColor = new Color(1f, 1f, 1f, 0.5f);

	private readonly int seperationSpace = 4;

	private int entriesPerRow = 5;

	private int entriesPerColumn;

	private int entriesPerPage;

	private int entryWidth;

	private Vector2 entrySize = Vector2.Zero;

	private float syncDelay = 0.4f;

	private float syncTimer;

	private int selectedEntry = -1;

	private int selectedEntryOffset;

	public LibraryManager()
	{
		Register.LibraryManager = this;
	}

	public LibraryManager(Workspace workspace, Container container, ThumbnailRenderer thumbnailRenderer, Texture syncIconTexture)
	{
		Register.LibraryManager = this;
		this.container = container;
		this.thumbnailRenderer = thumbnailRenderer;
		this.syncIconTexture = syncIconTexture;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "LibraryManager";
	}

	public override void _Ready()
	{
		base._Ready();
		gui = Register.Gui;
		workspace = Register.Workspace;
		gridContainer = new GridContainer();
		container.AddChild(gridContainer);
		gridContainer.Owner = container;
		gridContainer.Name = "GridContainer";
		gridContainer.AnchorRight = 1f;
		gridContainer.AnchorBottom = 1f;
		gridContainer.RectPosition = Vector2.Zero;
		gridContainer.RectSize = container.RectSize;
		gridContainer.AddConstantOverride("hseparation", seperationSpace);
		gridContainer.AddConstantOverride("vseparation", seperationSpace);
		gridContainer.FocusMode = Control.FocusModeEnum.None;
		gridContainer.MouseFilter = Control.MouseFilterEnum.Pass;
		gridContainer.Connect(Signals.GuiInput, this, "GuiInput");
		hScrollBar = new HScrollBar();
		container.AddChild(hScrollBar);
		hScrollBar.Owner = container;
		hScrollBar.Name = "HScrollBar";
		hScrollBar.Theme = gui.Theme;
		hScrollBar.FocusMode = Control.FocusModeEnum.None;
		hScrollBar.MouseFilter = Control.MouseFilterEnum.Pass;
		hScrollBar.Rounded = true;
		hScrollBar.Value = 0.0;
		hScrollBar.Connect(Signals.Scrolling, this, "ScrollBarChanged");
		selectionPanel = new Panel();
		container.AddChild(selectionPanel);
		selectionPanel.Owner = container;
		selectionPanel.Name = "SelectionPanel";
		selectionPanel.FocusMode = Control.FocusModeEnum.None;
		selectionPanel.MouseFilter = Control.MouseFilterEnum.Ignore;
		selectionPanel.Visible = false;
		selectionPanel.AddStyleboxOverride("panel", Resources.LibrarySelectionStyleBox);
		popupMenu = new PopupMenu();
		container.AddChild(popupMenu);
		popupMenu.Owner = container;
		popupMenu.Name = "PopupMenu";
		popupMenu.Theme = Resources.DefaultTheme;
		popupMenu.AddFontOverride("font", Resources.DefaultFont);
		popupMenu.AddItem("Create Shades...", 0);
		popupMenu.AddItem("Overwrite", 1);
		popupMenu.AddSeparator();
		popupMenu.AddItem("Delete", 2);
		popupMenu.MouseFilter = Control.MouseFilterEnum.Stop;
		popupMenu.Connect(Signals.IdPressed, this, "PopupMenuItemSelected");
		popupMenu.Connect(Signals.Hide, this, "PopupMenuHide");
		popupMenu.Connect(Signals.MouseExited, this, "PopupMenuMouseExited");
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		syncTimer -= delta;
		if (syncTimer < 0f)
		{
			UpdateThumbnails();
			syncTimer = syncDelay;
		}
	}

	public void Reset()
	{
		UpdateGridContainer();
		UpdateScrollBar();
		ClearSelection();
		UpdateSelectedEntryPanel();
	}

	public void Resize()
	{
		UpdateGridContainer();
		UpdateSelectedEntryPanel();
	}

	public void Update()
	{
		UpdateScrollBar();
		UpdateThumbnails(forceUpdate: true);
		UpdateSelectedEntryPanel();
	}

	public void RequestThumbnail(DrawingPreset drawingPreset)
	{
		thumbnailRenderer.RequestThumbnail(drawingPreset);
	}

	public void RequestThumbnails(List<DrawingPreset> drawingPresetsList)
	{
		thumbnailRenderer.RequestThumbnails(drawingPresetsList);
	}

	private void UpdateThumbnails(bool forceUpdate = false)
	{
		selectedEntryOffset = GetScrollBarValue();
		for (int y = 0; y < entriesPerColumn; y++)
		{
			for (int x = 0; x < entriesPerRow; x++)
			{
				int index = x + y * entriesPerRow;
				if (selectedEntryOffset + index < workspace.Worksheet.DrawingPresetsList.Count)
				{
					if (forceUpdate && workspace.Worksheet.DrawingPresetsList[selectedEntryOffset + index].IsThumbnailAvailable)
					{
						textureRectsArray[index].Texture = workspace.Worksheet.DrawingPresetsList[selectedEntryOffset + index].ImageTexture;
						textureRectsArray[index].Modulate = fullColor;
					}
					else if (workspace.Worksheet.DrawingPresetsList[selectedEntryOffset + index].DoUpdateThumbnail)
					{
						textureRectsArray[index].Texture = workspace.Worksheet.DrawingPresetsList[selectedEntryOffset + index].ImageTexture;
						textureRectsArray[index].Modulate = fullColor;
						workspace.Worksheet.DrawingPresetsList[selectedEntryOffset + index].DoUpdateThumbnail = false;
					}
					else if (workspace.Worksheet.DrawingPresetsList[selectedEntryOffset + index].IsThumbnailRequested)
					{
						textureRectsArray[index].Texture = syncIconTexture;
						textureRectsArray[index].Modulate = syncIconColor;
					}
				}
				else
				{
					textureRectsArray[index].Texture = null;
				}
			}
		}
		if (selectedEntry < selectedEntryOffset || selectedEntry >= selectedEntryOffset + entriesPerPage)
		{
			selectionPanel.Visible = false;
		}
		else
		{
			selectionPanel.Visible = true;
		}
	}

	public void ResetThumbnailRenderer()
	{
		thumbnailRenderer.Reset();
	}

	private void UpdateGridContainer()
	{
		int entryWidth = Mathf.FloorToInt(1f * (container.RectSize.x - (float)((entriesPerRow - 1) * seperationSpace)) / (float)entriesPerRow);
		int entriesPerColumn = Mathf.FloorToInt((container.RectSize.y - hScrollBar.RectSize.y) / (float)(entryWidth + seperationSpace));
		if (this.entriesPerColumn == entriesPerColumn && this.entryWidth == entryWidth)
		{
			return;
		}
		this.entryWidth = entryWidth;
		this.entriesPerColumn = entriesPerColumn;
		entriesPerPage = entriesPerRow * this.entriesPerColumn;
		if (entriesPerPage <= 0)
		{
			return;
		}
		entrySize.x = (entrySize.y = this.entryWidth);
		gridContainer.Columns = entriesPerRow;
		gridContainer.RectSize = new Vector2(container.RectSize.x, this.entriesPerColumn * (this.entryWidth + seperationSpace));
		selectionPanel.RectSize = entrySize;
		hScrollBar.RectPosition = new Vector2(0f, gridContainer.RectPosition.y + (float)(this.entriesPerColumn * (this.entryWidth + seperationSpace)) + 4f);
		hScrollBar.RectSize = new Vector2(container.RectSize.x, 18f);
		UpdateScrollBar();
		for (int i = 0; i < gridContainer.GetChildCount(); i++)
		{
			gridContainer.GetChild(i).QueueFree();
		}
		textureRectsArray = new TextureRect[entriesPerPage];
		for (int y = 0; y < this.entriesPerColumn; y++)
		{
			for (int x = 0; x < entriesPerRow; x++)
			{
				int index = x + y * entriesPerRow;
				textureRectsArray[index] = new TextureRect();
				gridContainer.AddChild(textureRectsArray[index]);
				textureRectsArray[index].Owner = gridContainer;
				textureRectsArray[index].Expand = true;
				textureRectsArray[index].RectSize = entrySize;
				textureRectsArray[index].RectMinSize = entrySize;
				textureRectsArray[index].FocusMode = Control.FocusModeEnum.None;
				textureRectsArray[index].MouseFilter = Control.MouseFilterEnum.Ignore;
				textureRectsArray[index].Texture = null;
			}
		}
		UpdateThumbnails(forceUpdate: true);
	}

	private void UpdateScrollBar()
	{
		hScrollBar.Page = entriesPerPage;
		hScrollBar.Step = hScrollBar.Page;
		hScrollBar.MaxValue = Mathf.CeilToInt(1f * (float)workspace.Worksheet.DrawingPresetsList.Count / (float)entriesPerPage) * entriesPerPage;
		if (hScrollBar.Value > hScrollBar.MaxValue)
		{
			hScrollBar.Value = hScrollBar.MaxValue;
		}
		if (hScrollBar.Value < 0.0)
		{
			hScrollBar.Value = 0.0;
		}
	}

	public void UpdateSelectedEntryPanel()
	{
		if (selectedEntry > -1 && selectedEntry < workspace.Worksheet.DrawingPresetsList.Count)
		{
			selectedEntryOffset = GetScrollBarValue();
			if (selectedEntry < selectedEntryOffset || selectedEntry >= selectedEntryOffset + entriesPerPage)
			{
				selectionPanel.Visible = false;
				return;
			}
			selectionPanel.Visible = true;
			int divider = entryWidth + seperationSpace;
			int x = (selectedEntry - selectedEntryOffset) % entriesPerRow;
			int y = (selectedEntry - selectedEntryOffset) / entriesPerRow;
			selectionPanel.RectPosition = new Vector2(x * divider, y * divider);
		}
	}

	public void ClearSelection()
	{
		selectedEntryOffset = 0;
		selectedEntry = -1;
		UpdateSelectedEntryPanel();
	}

	private int GetScrollBarValue()
	{
		return (int)hScrollBar.Value;
	}

	private void SetScrollBarValue(int value)
	{
		hScrollBar.Value = value;
	}

	public void ScrollBarChanged()
	{
		UpdateThumbnails(forceUpdate: true);
		UpdateSelectedEntryPanel();
	}

	public void ConvertColorListToDrawingPresets(List<Color> colorList)
	{
		workspace.Worksheet.DrawingPresetsList.Clear();
		workspace.Worksheet.DrawingPresetsList = DrawingPreset.ConvertFromColorList(colorList);
		thumbnailRenderer.RequestThumbnails(workspace.Worksheet.DrawingPresetsList);
		SetScrollBarValue(0);
		ClearSelection();
		Update();
	}

	public void InsertDrawingPresetInterpolation(int index, int Steps)
	{
		if (index > -1 && index < workspace.Worksheet.DrawingPresetsList.Count - 1)
		{
			DrawingPreset[] array = DrawingPreset.Interpolation(workspace.Worksheet.DrawingPresetsList[index], workspace.Worksheet.DrawingPresetsList[index + 1], Steps);
			foreach (DrawingPreset drawingPreset in array)
			{
				workspace.Worksheet.DrawingPresetsList.Insert(++index, drawingPreset);
				thumbnailRenderer.RequestThumbnail(drawingPreset);
			}
			Update();
		}
	}

	public void AddDrawingPreset()
	{
		DrawingPreset drawingPreset = new DrawingPreset();
		drawingPreset.Name = workspace.Worksheet.DrawingPresetsList.Count.ToString();
		drawingPreset.Data.Color = workspace.DrawingManager.Color;
		drawingPreset.Data.Height = workspace.DrawingManager.Height;
		drawingPreset.Data.Roughness = workspace.DrawingManager.Roughness;
		drawingPreset.Data.Metallicity = workspace.DrawingManager.Metallicity;
		drawingPreset.Data.Emission = workspace.DrawingManager.Emission;
		workspace.Worksheet.DrawingPresetsList.Add(drawingPreset);
		thumbnailRenderer.RequestThumbnail(drawingPreset);
		selectedEntry = workspace.Worksheet.DrawingPresetsList.IndexOf(drawingPreset);
		UpdateScrollBar();
		SetScrollBarValue((int)hScrollBar.MaxValue);
		Update();
	}

	public void UpdateDrawingPreset()
	{
		if (selectedEntry > -1 && selectedEntry < workspace.Worksheet.DrawingPresetsList.Count)
		{
			workspace.Worksheet.DrawingPresetsList[selectedEntry].Data.Color = workspace.DrawingManager.Color;
			workspace.Worksheet.DrawingPresetsList[selectedEntry].Data.Height = workspace.DrawingManager.Height;
			workspace.Worksheet.DrawingPresetsList[selectedEntry].Data.Roughness = workspace.DrawingManager.Roughness;
			workspace.Worksheet.DrawingPresetsList[selectedEntry].Data.Metallicity = workspace.DrawingManager.Metallicity;
			workspace.Worksheet.DrawingPresetsList[selectedEntry].Data.Emission = workspace.DrawingManager.Emission;
			thumbnailRenderer.RequestThumbnail(workspace.Worksheet.DrawingPresetsList[selectedEntry]);
			Update();
		}
	}

	public void DeleteDrawingPreset()
	{
		if (selectedEntry > -1 && selectedEntry < workspace.Worksheet.DrawingPresetsList.Count)
		{
			workspace.Worksheet.DrawingPresetsList.RemoveAt(selectedEntry);
			selectedEntry--;
			if (selectedEntry < 0 && workspace.Worksheet.DrawingPresetsList.Count > 0)
			{
				selectedEntry++;
			}
			Update();
		}
	}

	public void PushDrawingPresetUp()
	{
		if (selectedEntry > 0 && selectedEntry < workspace.Worksheet.DrawingPresetsList.Count)
		{
			DrawingPreset drawingPreset = workspace.Worksheet.DrawingPresetsList[selectedEntry];
			workspace.Worksheet.DrawingPresetsList.RemoveAt(selectedEntry);
			workspace.Worksheet.DrawingPresetsList.Insert(selectedEntry - 1, drawingPreset);
			selectedEntry--;
			UpdateThumbnails(forceUpdate: true);
			UpdateSelectedEntryPanel();
		}
	}

	public void PushDrawingPresetDown()
	{
		if (selectedEntry > -1 && selectedEntry < workspace.Worksheet.DrawingPresetsList.Count - 1)
		{
			DrawingPreset drawingPreset = workspace.Worksheet.DrawingPresetsList[selectedEntry];
			workspace.Worksheet.DrawingPresetsList.RemoveAt(selectedEntry);
			workspace.Worksheet.DrawingPresetsList.Insert(selectedEntry + 1, drawingPreset);
			selectedEntry++;
			UpdateThumbnails(forceUpdate: true);
			UpdateSelectedEntryPanel();
		}
	}

	public void PopupMenuItemSelected(int id)
	{
		switch ((PopupMenuItemEnum)id)
		{
		case PopupMenuItemEnum.SHADES:
			if (selectedEntry < workspace.Worksheet.DrawingPresetsList.Count - 1)
			{
				InsertDrawingPresetInterpolation(selectedEntry, 4);
			}
			break;
		case PopupMenuItemEnum.OVERWRITE:
			UpdateDrawingPreset();
			break;
		case PopupMenuItemEnum.DELETE:
			DeleteDrawingPreset();
			break;
		}
	}

	public void PopupMenuShow(Vector2 position)
	{
		InputManager.MouseEnteredUserInterface();
		hoverPanelState = gui.LibraryHoverPanel.HideAfterMouseExited;
		gui.LibraryHoverPanel.HideAfterMouseExited = false;
		popupMenu.RectPosition = position;
		popupMenu.Show();
	}

	public void PopupMenuHide()
	{
		gui.LibraryHoverPanel.HideAfterMouseExited = hoverPanelState;
		InputManager.MouseExitedUserInterface();
		InputManager.SkipInput = true;
	}

	public void PopupMenuMouseExited()
	{
		popupMenu.Hide();
	}

	public void GuiInput(InputEvent @event)
	{
		if (!(@event is InputEventMouseButton { Pressed: not false } mbe))
		{
			return;
		}
		int divider = entryWidth + seperationSpace;
		int num = Mathf.FloorToInt(mbe.Position.x / (float)divider);
		int y = Mathf.FloorToInt(mbe.Position.y / (float)divider);
		int index = num + y * entriesPerRow;
		if (index >= entriesPerPage)
		{
			return;
		}
		int indexOffset = (int)hScrollBar.Value;
		index += indexOffset;
		if (index < workspace.Worksheet.DrawingPresetsList.Count)
		{
			if (mbe.ButtonIndex == 1)
			{
				selectedEntry = index;
				workspace.DrawingManager.Color = workspace.Worksheet.DrawingPresetsList[selectedEntry].Data.Color;
				workspace.DrawingManager.Height = workspace.Worksheet.DrawingPresetsList[selectedEntry].Data.Height;
				workspace.DrawingManager.Roughness = workspace.Worksheet.DrawingPresetsList[selectedEntry].Data.Roughness;
				workspace.DrawingManager.Metallicity = workspace.Worksheet.DrawingPresetsList[selectedEntry].Data.Metallicity;
				workspace.DrawingManager.Emission = workspace.Worksheet.DrawingPresetsList[selectedEntry].Data.Emission;
				gui.SetDrawingMaterial(workspace.DrawingManager.Color, workspace.DrawingManager.Height, workspace.DrawingManager.Roughness, workspace.DrawingManager.Metallicity, workspace.DrawingManager.Emission);
				selectedEntry = index;
				UpdateSelectedEntryPanel();
			}
			else if (mbe.ButtonIndex == 2)
			{
				selectedEntry = index;
				PopupMenuShow(new Vector2(container.RectGlobalPosition.x + mbe.Position.x - 8f, container.RectGlobalPosition.y + mbe.Position.y - 8f));
				UpdateSelectedEntryPanel();
			}
		}
	}
}
