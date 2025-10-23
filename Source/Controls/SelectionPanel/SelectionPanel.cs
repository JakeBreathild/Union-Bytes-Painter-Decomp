using Godot;

public class SelectionPanel : Panel
{
	[Export(PropertyHint.None, "")]
	private PackedScene selectionEntryPackedScene;

	[Export(PropertyHint.None, "")]
	private StyleBox entryStyleBox;

	[Export(PropertyHint.None, "")]
	private StyleBox selectedStyleBox;

	[Export(PropertyHint.None, "")]
	private NodePath selectionWindowDialogNodePath;

	[Export(PropertyHint.None, "")]
	private NodePath SelectButtonNodePath;

	private Workspace workspace;

	private ScrollContainer scrollContainer;

	private VBoxContainer vBoxContainer;

	private TextureButton addButton;

	private TextureButton deleteButton;

	private TextureButton upButton;

	private TextureButton downButton;

	private int selectionCounter;

	private int selectionIndex = -1;

	private SelectionEntry selectionEntry;

	public SelectionWindowDialog SelectionWindowDialog { get; private set; }

	public SelectButton SelectButton { get; private set; }

	public int SelectionIndex => selectionIndex;

	public SelectionEntry SelectionEntry => selectionEntry;

	public override void _Ready()
	{
		base._Ready();
		Register.SelectionPanel = this;
		workspace = Register.Workspace;
		SelectButton = GetNodeOrNull<SelectButton>(SelectButtonNodePath);
		SelectionWindowDialog = GetNodeOrNull<SelectionWindowDialog>(selectionWindowDialogNodePath);
		scrollContainer = GetNodeOrNull<ScrollContainer>("ScrollContainer");
		vBoxContainer = GetNodeOrNull<VBoxContainer>("ScrollContainer/VBoxContainer");
		addButton = GetNodeOrNull<TextureButton>("AddTextureButton");
		addButton.Connect(Signals.Pressed, this, "AddPressed");
		deleteButton = GetNodeOrNull<TextureButton>("DeleteTextureButton");
		deleteButton.Connect(Signals.Pressed, this, "DeletePressed");
		upButton = GetNodeOrNull<TextureButton>("UpTextureButton");
		upButton.Connect(Signals.Pressed, this, "UpPressed");
		downButton = GetNodeOrNull<TextureButton>("DownTextureButton");
		downButton.Connect(Signals.Pressed, this, "DownPressed");
	}

	public void Reset()
	{
		Reset(resetCounter: false);
	}

	public void Reset(bool resetCounter = true)
	{
		if (resetCounter)
		{
			selectionCounter = 0;
		}
		Clear();
		Data data = workspace.Worksheet.Data;
		for (int i = data.SelectionArraysList.Count - 1; i >= 0; i--)
		{
			SelectionEntry selectionEntry = Add(data.SelectionArraysList[i]);
			if (this.selectionEntry != null && data.SelectionArraysList[i] == this.selectionEntry.Selection)
			{
				this.selectionEntry = selectionEntry;
				this.selectionEntry.Set("custom_styles/panel", selectedStyleBox);
				selectionIndex = i;
			}
		}
	}

	public void RemoveSelection()
	{
		selectionIndex = -1;
		selectionEntry = null;
		Reset(resetCounter: false);
	}

	public void Clear()
	{
		for (int i = 0; i < vBoxContainer.GetChildCount(); i++)
		{
			vBoxContainer.GetChildOrNull<SelectionEntry>(i).QueueFree();
		}
		selectionIndex = -1;
	}

	public SelectionEntry Add(Data.Selection selection)
	{
		SelectionEntry selectionEntry = selectionEntryPackedScene.InstanceOrNull<SelectionEntry>();
		vBoxContainer.AddChild(selectionEntry);
		selectionEntry.Owner = vBoxContainer;
		selectionEntry.Selection = selection;
		selectionEntry.SelectionPanel = this;
		selectionEntry.Name = selection.Name;
		selectionEntry.Color = selection.Color;
		return selectionEntry;
	}

	public void Select(Data.Selection selection)
	{
		workspace.Worksheet.Data.ActivateSelection(selection);
		for (int i = 0; i < vBoxContainer.GetChildCount(); i++)
		{
			SelectionEntry selectionEntry = vBoxContainer.GetChildOrNull<SelectionEntry>(i);
			if (selectionEntry.Selection == selection)
			{
				this.selectionEntry = selectionEntry;
				this.selectionEntry.Set("custom_styles/panel", selectedStyleBox);
				selectionIndex = i;
			}
			else
			{
				selectionEntry.Set("custom_styles/panel", entryStyleBox);
			}
		}
	}

	public void UpdateCurrentEntry()
	{
		selectionEntry?.UpdateSelection();
	}

	public void AddPressed()
	{
		Color color = new Color(0.8f, 0.4f, 0.4f);
		color.h = 1f * (float)(selectionCounter % 32) / 32f;
		Data.Selection selection = workspace.Worksheet.Data.AddSelection(selectionCounter.ToString("000"), color);
		if (selection != null)
		{
			selectionEntry = Add(selection);
			Reset(resetCounter: false);
			scrollContainer.ScrollVertical = 0;
			selectionCounter++;
		}
	}

	public void DeletePressed()
	{
		if (selectionEntry != null)
		{
			workspace.Worksheet.Data.DeleteSelection(selectionEntry.Selection);
			selectionEntry.QueueFree();
			selectionIndex = -1;
			selectionEntry = null;
			Reset(resetCounter: false);
		}
	}

	public void DownPressed()
	{
		if (selectionEntry != null && selectionEntry.Selection != workspace.Worksheet.Data.SelectionArraysList[0])
		{
			workspace.Worksheet.Data.MoveSelectionDown(selectionEntry.Selection);
			Reset(resetCounter: false);
		}
	}

	public void UpPressed()
	{
		if (selectionEntry != null && selectionEntry.Selection != workspace.Worksheet.Data.SelectionArraysList[workspace.Worksheet.Data.SelectionArraysList.Count - 1])
		{
			workspace.Worksheet.Data.MoveSelectionUp(selectionEntry.Selection);
			Reset(resetCounter: false);
		}
	}
}
