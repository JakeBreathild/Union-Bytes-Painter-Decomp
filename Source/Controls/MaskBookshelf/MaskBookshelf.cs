using System.Collections.Generic;
using System.IO;
using System.Threading;
using Godot;

public class MaskBookshelf : GridContainer
{
	private readonly int seperationSpace = 4;

	private ScrollContainer parentScrollContainer;

	private System.Threading.Thread thread;

	private static Queue<string> threadPathsQueue = new Queue<string>();

	private static System.Threading.Mutex threadPathsQueueMutex = new System.Threading.Mutex();

	private static Queue<MaskBookshelfItem> threadItemsQueue = new Queue<MaskBookshelfItem>();

	private static System.Threading.Mutex threadItemsQueueMutex = new System.Threading.Mutex();

	private int itemSize;

	private List<MaskBookshelfItem> itemsList = new List<MaskBookshelfItem>();

	private FileSystemWatcher pngFileSystemWatcher;

	private FileSystemWatcher jpgFileSystemWatcher;

	public override void _Ready()
	{
		parentScrollContainer = GetParentOrNull<ScrollContainer>();
		parentScrollContainer.GetVScrollbar().MouseFilter = MouseFilterEnum.Pass;
		string path = Settings.MasksBookshelfPath;
		itemSize = Mathf.FloorToInt((276 - base.Columns * seperationSpace) / base.Columns);
		itemsList.Clear();
		string[] filesArray = System.IO.Directory.GetFiles(path, "*.png");
		threadPathsQueueMutex.WaitOne();
		for (int i = 0; i < filesArray.Length; i++)
		{
			threadPathsQueue.Enqueue(filesArray[i]);
		}
		threadPathsQueueMutex.ReleaseMutex();
		pngFileSystemWatcher = new FileSystemWatcher(path);
		pngFileSystemWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
		pngFileSystemWatcher.Filter = "*.png";
		pngFileSystemWatcher.IncludeSubdirectories = true;
		pngFileSystemWatcher.EnableRaisingEvents = true;
		pngFileSystemWatcher.Created += FileSystemWatcherOnCreated;
		pngFileSystemWatcher.Deleted += FileSystemWatcherOnDeleted;
		pngFileSystemWatcher.Changed += FileSystemWatcherOnChanged;
		pngFileSystemWatcher.Renamed += FileSystemWatcherOnRenamed;
		pngFileSystemWatcher.Error += FileSystemWatcherOnError;
		filesArray = System.IO.Directory.GetFiles(path, "*.jpg");
		threadPathsQueueMutex.WaitOne();
		for (int j = 0; j < filesArray.Length; j++)
		{
			threadPathsQueue.Enqueue(filesArray[j]);
		}
		threadPathsQueueMutex.ReleaseMutex();
		jpgFileSystemWatcher = new FileSystemWatcher(path);
		jpgFileSystemWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
		jpgFileSystemWatcher.Filter = "*.jpg";
		jpgFileSystemWatcher.IncludeSubdirectories = true;
		jpgFileSystemWatcher.EnableRaisingEvents = true;
		jpgFileSystemWatcher.Created += FileSystemWatcherOnCreated;
		jpgFileSystemWatcher.Deleted += FileSystemWatcherOnDeleted;
		jpgFileSystemWatcher.Changed += FileSystemWatcherOnChanged;
		jpgFileSystemWatcher.Renamed += FileSystemWatcherOnRenamed;
		jpgFileSystemWatcher.Error += FileSystemWatcherOnError;
	}

	public override void _GuiInput(InputEvent @event)
	{
		base._GuiInput(@event);
		if (!(@event is InputEventMouseButton { Pressed: not false } mbe))
		{
			return;
		}
		int divider = itemSize + seperationSpace;
		int num = Mathf.FloorToInt(mbe.Position.x / (float)divider);
		int y = Mathf.FloorToInt(mbe.Position.y / (float)divider);
		int index = num + y * base.Columns;
		if (mbe.ButtonIndex != 1)
		{
			return;
		}
		if (index > -1 && index < itemsList.Count)
		{
			switch (Register.DrawingManager.Tool)
			{
			case DrawingManager.ToolEnum.BUCKET:
				Register.DrawingManager.BucketTool.LoadMask(itemsList[index].Path);
				break;
			case DrawingManager.ToolEnum.STAMP:
				Register.DrawingManager.StampTool.LoadMask(itemsList[index].Path);
				break;
			default:
				Register.Gui.ToolsContainer.PressToolButton(7);
				Register.DrawingManager.StampTool.LoadMask(itemsList[index].Path);
				break;
			}
		}
		else if (mbe.ButtonIndex == 2 && index > -1 && index < itemsList.Count)
		{
			switch (Register.DrawingManager.Tool)
			{
			case DrawingManager.ToolEnum.BUCKET:
				Register.DrawingManager.BucketTool.LoadMask(itemsList[index].Path);
				Register.MaterialContainer.AddDecalToListPressed();
				break;
			case DrawingManager.ToolEnum.STAMP:
				Register.DrawingManager.StampTool.LoadMask(itemsList[index].Path);
				Register.MaterialContainer.AddDecalToListPressed();
				break;
			default:
				Register.Gui.ToolsContainer.PressToolButton(7);
				Register.DrawingManager.StampTool.LoadMask(itemsList[index].Path);
				Register.MaterialContainer.AddDecalToListPressed();
				break;
			}
		}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		threadPathsQueueMutex.WaitOne();
		if (threadPathsQueue.Count > 0)
		{
			if (thread == null)
			{
				thread = new System.Threading.Thread(Thread);
			}
			if (thread.ThreadState == ThreadState.Stopped)
			{
				thread.Abort();
				thread = new System.Threading.Thread(Thread);
			}
			if (thread.ThreadState == ThreadState.Unstarted)
			{
				thread.Start();
			}
		}
		threadPathsQueueMutex.ReleaseMutex();
		threadItemsQueueMutex.WaitOne();
		if (threadItemsQueue.Count > 0)
		{
			MaskBookshelfItem item = threadItemsQueue.Dequeue();
			threadItemsQueueMutex.ReleaseMutex();
			itemsList.Add(item);
			TextureRect textureRect = new TextureRect();
			AddChild(textureRect);
			textureRect.Owner = this;
			textureRect.Expand = true;
			if (item.IsLoaded)
			{
				textureRect.Texture = item;
			}
			else
			{
				textureRect.Texture = Resources.OldFileVersionIconTexture;
			}
			Vector2 rectSize = (textureRect.RectSize = new Vector2(itemSize, itemSize));
			textureRect.RectMinSize = rectSize;
			textureRect.MouseFilter = MouseFilterEnum.Pass;
			textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			textureRect.HintTooltip = item.Name;
		}
		else
		{
			threadItemsQueueMutex.ReleaseMutex();
		}
	}

	private static void Thread()
	{
		threadPathsQueueMutex.WaitOne();
		if (threadPathsQueue.Count > 0)
		{
			string path = threadPathsQueue.Dequeue();
			threadPathsQueueMutex.ReleaseMutex();
			MaskBookshelfItem item = new MaskBookshelfItem();
			item.Load(path);
			threadItemsQueueMutex.WaitOne();
			threadItemsQueue.Enqueue(item);
			threadItemsQueueMutex.ReleaseMutex();
		}
		else
		{
			threadPathsQueueMutex.ReleaseMutex();
		}
	}

	private void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs e)
	{
		GD.Print("Created: " + e.Name);
		threadPathsQueueMutex.WaitOne();
		threadPathsQueue.Enqueue(e.Name);
		threadPathsQueueMutex.ReleaseMutex();
	}

	private void FileSystemWatcherOnDeleted(object sender, FileSystemEventArgs e)
	{
		GD.Print("Deleted: " + e.Name);
		MaskBookshelfItem item = null;
		foreach (MaskBookshelfItem i in itemsList)
		{
			if (i.Path.Equals(System.IO.Path.GetFullPath(e.Name).TrimEnd(new char[1] { '\\' })))
			{
				item = i;
				break;
			}
		}
		if (item != null)
		{
			int index = itemsList.IndexOf(item);
			GetChildOrNull<TextureRect>(index).QueueFree();
			itemsList.Remove(item);
		}
	}

	private void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs e)
	{
		if (e.ChangeType != WatcherChangeTypes.Changed)
		{
			return;
		}
		GD.Print("Changed: " + e.Name);
		foreach (MaskBookshelfItem i in itemsList)
		{
			if (i.Path.Equals(System.IO.Path.GetFullPath(e.Name).TrimEnd(new char[1] { '\\' })))
			{
				i.ReloadImage();
				int index = itemsList.IndexOf(i);
				GetChildOrNull<TextureRect>(index).Texture = i;
				break;
			}
		}
	}

	private void FileSystemWatcherOnRenamed(object sender, RenamedEventArgs e)
	{
		GD.Print("Renamed:");
		GD.Print("    Old: " + e.OldName);
		GD.Print("    New: " + e.Name);
		foreach (MaskBookshelfItem i in itemsList)
		{
			if (i.Path.Equals(System.IO.Path.GetFullPath(e.OldName).TrimEnd(new char[1] { '\\' })))
			{
				i.Rename(e.Name);
				break;
			}
		}
	}

	private void FileSystemWatcherOnError(object sender, ErrorEventArgs e)
	{
		GD.Print(e.GetException());
	}
}
