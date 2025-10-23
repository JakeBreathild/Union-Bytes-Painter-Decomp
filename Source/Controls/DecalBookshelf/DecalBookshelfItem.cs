using System.IO;
using Godot;

public class DecalBookshelfItem : ImageTexture
{
	private string path = "";

	private string name = "";

	private Image image;

	private bool isLoaded;

	public string Path => path;

	public string Name => name;

	public bool IsLoaded => isLoaded;

	public new void Load(string path)
	{
		this.path = System.IO.Path.GetFullPath(path).TrimEnd(new char[1] { '\\' });
		name = System.IO.Path.GetFileName(this.path);
		image = FileSystem.LoadProjectThumbnail(this.path);
		if (image != null)
		{
			CreateFromImage(image, 0u);
			isLoaded = true;
		}
	}

	public void Rename(string path)
	{
		this.path = System.IO.Path.GetFullPath(path).TrimEnd(new char[1] { '\\' });
		name = System.IO.Path.GetFileName(this.path);
	}

	public void ReloadImage()
	{
		image = FileSystem.LoadProjectThumbnail(path);
		if (image != null)
		{
			CreateFromImage(image, 0u);
			isLoaded = true;
		}
	}
}
