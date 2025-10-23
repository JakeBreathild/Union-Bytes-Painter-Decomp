using System.IO;
using Godot;

public class PreviewFileDialog : FileDialog
{
	[Export(PropertyHint.None, "")]
	private bool setWindowHidden = true;

	[Export(PropertyHint.None, "")]
	private bool showPreviewWindowDialog = true;

	private string previousPath = "";

	private Image image;

	private ImageTexture imageTexture;

	private Panel previewWindowDialog;

	private TextureRect previewTextureRect;

	private Label previewLabel;

	private Panel favouritesWindowDialog;

	private ItemList favouritesItemList;

	public bool SetWindowHidden
	{
		get
		{
			return setWindowHidden;
		}
		set
		{
			setWindowHidden = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		base.RectMinSize = new Vector2(600f, 600f);
		previewWindowDialog = GetNodeOrNull<Panel>("PreviewWindowDialog");
		if (previewWindowDialog != null)
		{
			previewTextureRect = previewWindowDialog.GetNodeOrNull<TextureRect>("TextureRect");
			previewLabel = previewWindowDialog.GetNodeOrNull<Label>("Label");
		}
		favouritesWindowDialog = GetNodeOrNull<Panel>("FavouritesWindowDialog");
		if (favouritesWindowDialog != null)
		{
			favouritesItemList = favouritesWindowDialog.GetNodeOrNull<ItemList>("ItemList");
		}
		Connect(Signals.Hide, this, "Hide");
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (!base.Visible)
		{
			return;
		}
		if (previewWindowDialog != null && showPreviewWindowDialog)
		{
			previewWindowDialog.RectSize = new Vector2(256f, 270f);
			previewWindowDialog.RectPosition = new Vector2(base.RectSize.x - 284f, 94f);
			string currentPath = System.IO.Path.GetFullPath(base.CurrentPath);
			if (currentPath != previousPath)
			{
				previousPath = currentPath;
				string fileExtension = System.IO.Path.GetExtension(currentPath);
				if (!System.IO.File.Exists(currentPath))
				{
					fileExtension = "";
				}
				switch (fileExtension)
				{
				case ".png":
				case ".jpg":
					image.Load(currentPath);
					imageTexture.CreateFromImage(image, 0u);
					previewTextureRect.Texture = imageTexture;
					previewLabel.Text = image.GetWidth() + " x " + image.GetHeight();
					if (image.GetWidth() > 512 || image.GetHeight() > 512)
					{
						previewLabel.Set("custom_colors/font_color", Settings.NodeErrorColor);
					}
					else
					{
						previewLabel.Set("custom_colors/font_color", new Color(0.08f, 0.8f, 0.55f));
					}
					break;
				case ".ubpd":
					image = FileSystem.LoadProjectThumbnail(currentPath);
					imageTexture.CreateFromImage(image, 0u);
					previewTextureRect.Texture = imageTexture;
					previewLabel.Text = "";
					break;
				default:
					previewTextureRect.Texture = null;
					previewLabel.Text = "";
					break;
				}
			}
		}
		else
		{
			showPreviewWindowDialog = false;
		}
		if (favouritesWindowDialog != null)
		{
			if (showPreviewWindowDialog)
			{
				favouritesWindowDialog.RectSize = new Vector2(256f, base.RectSize.y - (94f + previewWindowDialog.RectSize.y + 84f + 8f));
				favouritesWindowDialog.RectPosition = new Vector2(base.RectSize.x - 284f, 94f + previewWindowDialog.RectSize.y + 8f);
			}
			else
			{
				favouritesWindowDialog.RectSize = new Vector2(256f, base.RectSize.y - 94f - 84f);
				favouritesWindowDialog.RectPosition = new Vector2(base.RectSize.x - 284f, 94f);
			}
		}
	}

	public new void PopupCentered(Vector2? size = null)
	{
		InputManager.WindowShown();
		base.PopupCentered(size);
		if (previewWindowDialog != null)
		{
			if (showPreviewWindowDialog)
			{
				previewWindowDialog.Visible = true;
				image = new Image();
				imageTexture = new ImageTexture();
			}
			else
			{
				previewWindowDialog.Visible = false;
			}
		}
		if (favouritesWindowDialog != null)
		{
			favouritesItemList.Clear();
			for (int i = 0; i < Settings.FavouritesList.Count; i++)
			{
				string path = Settings.FavouritesList[i];
				favouritesItemList.AddItem("[" + System.IO.Path.GetPathRoot(path) + "]  " + StringExtensions.Split(path, "\\")[path.Count("\\") - 1]);
			}
		}
	}

	public new void Show()
	{
		InputManager.WindowShown();
		base.Show();
		if (previewWindowDialog != null)
		{
			if (showPreviewWindowDialog)
			{
				previewWindowDialog.Visible = true;
				image = new Image();
				imageTexture = new ImageTexture();
			}
			else
			{
				previewWindowDialog.Visible = false;
			}
		}
		if (favouritesWindowDialog != null)
		{
			favouritesItemList.Clear();
			for (int i = 0; i < Settings.FavouritesList.Count; i++)
			{
				string path = Settings.FavouritesList[i];
				favouritesItemList.AddItem("[" + System.IO.Path.GetPathRoot(path) + "]  " + StringExtensions.Split(path, "\\")[path.Count("\\") - 1]);
			}
		}
	}

	public new void Hide()
	{
		base.Hide();
		if (setWindowHidden)
		{
			InputManager.WindowHidden();
		}
		InputManager.SkipInput = true;
		if (previewWindowDialog != null)
		{
			previewTextureRect.Texture = null;
			imageTexture = null;
			image = null;
			previousPath = "";
		}
	}

	public void AddFavourite(string path)
	{
		path = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path));
		if (path != null && Settings.FavouritesHashSet.Add(path + "\\"))
		{
			Settings.FavouritesList.Add(path + "\\");
			favouritesItemList.AddItem("[" + System.IO.Path.GetPathRoot(path) + "]  " + StringExtensions.Split(path, "\\")[path.Count("\\")]);
		}
	}

	public void RemoveFavourite(int index)
	{
		string path = Settings.FavouritesList[index];
		if (Settings.FavouritesHashSet.Contains(path))
		{
			Settings.FavouritesHashSet.Remove(path);
		}
		Settings.FavouritesList.RemoveAt(index);
		favouritesItemList.RemoveItem(index);
	}

	public void MoveFavouriteUp(int index)
	{
		if (index > 0)
		{
			string path = Settings.FavouritesList[index];
			Settings.FavouritesList.RemoveAt(index);
			Settings.FavouritesList.Insert(index - 1, path);
			favouritesItemList.Clear();
			for (int i = 0; i < Settings.FavouritesList.Count; i++)
			{
				path = Settings.FavouritesList[i];
				favouritesItemList.AddItem("[" + System.IO.Path.GetPathRoot(path) + "]  " + StringExtensions.Split(path, "\\")[path.Count("\\") - 1]);
			}
			favouritesItemList.Select(index - 1);
		}
	}

	public void MoveFavouriteDown(int index)
	{
		if (index < Settings.FavouritesList.Count - 1)
		{
			string path = Settings.FavouritesList[index];
			Settings.FavouritesList.RemoveAt(index);
			Settings.FavouritesList.Insert(index + 1, path);
			favouritesItemList.Clear();
			for (int i = 0; i < Settings.FavouritesList.Count; i++)
			{
				path = Settings.FavouritesList[i];
				favouritesItemList.AddItem("[" + System.IO.Path.GetPathRoot(path) + "]  " + StringExtensions.Split(path, "\\")[path.Count("\\") - 1]);
			}
			favouritesItemList.Select(index + 1);
		}
	}

	public void AddFavouriteButtonPressed()
	{
		AddFavourite(base.CurrentPath);
	}

	public void RemoveFavouriteButtonPressed()
	{
		if (favouritesItemList.IsAnythingSelected())
		{
			int[] indices = favouritesItemList.GetSelectedItems();
			RemoveFavourite(indices[0]);
			favouritesItemList.UnselectAll();
		}
	}

	public void MoveFavouriteUpButtonPressed()
	{
		if (favouritesItemList.IsAnythingSelected())
		{
			int[] indices = favouritesItemList.GetSelectedItems();
			MoveFavouriteUp(indices[0]);
		}
	}

	public void MoveFavouriteDownButtonPressed()
	{
		if (favouritesItemList.IsAnythingSelected())
		{
			int[] indices = favouritesItemList.GetSelectedItems();
			MoveFavouriteDown(indices[0]);
		}
	}

	public void FavouriteItemSelected(int index)
	{
		base.CurrentPath = Settings.FavouritesList[index];
		base.CurrentFile = "";
	}
}
