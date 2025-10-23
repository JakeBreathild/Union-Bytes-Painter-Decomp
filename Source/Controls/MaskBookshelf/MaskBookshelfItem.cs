using System.IO;
using Godot;

public class MaskBookshelfItem : ImageTexture
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
		image = new Image();
		image.Load(this.path);
		if (image == null)
		{
			return;
		}
		float ratio = 1f * (float)image.GetWidth() / (float)image.GetHeight();
		int newWidth = Settings.ThumbnailSize;
		int newHeight = Settings.ThumbnailSize;
		if (ratio != 1f)
		{
			if (ratio < 1f)
			{
				newWidth = Mathf.RoundToInt((float)newWidth * ratio);
			}
			else
			{
				newHeight = Mathf.RoundToInt((float)newHeight / ratio);
			}
		}
		image.Resize(newWidth, newHeight, Image.Interpolation.Nearest);
		CreateFromImage(image, 0u);
		isLoaded = true;
	}

	public void Rename(string path)
	{
		this.path = System.IO.Path.GetFullPath(path).TrimEnd(new char[1] { '\\' });
		name = System.IO.Path.GetFileName(this.path);
	}

	public void ReloadImage()
	{
		if (image == null)
		{
			return;
		}
		image.Load(path);
		float ratio = 1f * (float)image.GetWidth() / (float)image.GetHeight();
		int newWidth = Settings.ThumbnailSize;
		int newHeight = Settings.ThumbnailSize;
		if (ratio != 1f)
		{
			if (ratio < 1f)
			{
				newWidth = Mathf.RoundToInt((float)newWidth * ratio);
			}
			else
			{
				newHeight = Mathf.RoundToInt((float)newHeight / ratio);
			}
		}
		image.Resize(newWidth, newHeight, Image.Interpolation.Nearest);
		CreateFromImage(image, 0u);
		isLoaded = true;
	}
}
