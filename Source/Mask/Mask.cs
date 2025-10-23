using Godot;

public class Mask
{
	private Image image;

	private Image thumbnail;

	private string name = "";

	private int width;

	private int height;

	private int arraySize = -1;

	private float[,] maskArray;

	private bool isLoaded;

	public Image Image => image;

	public Image Thumbnail => thumbnail;

	public string Name => name;

	public int Width => width;

	public int Height => height;

	public int ArraySize => arraySize;

	public float[,] MaskArray => maskArray;

	public bool IsLoaded => isLoaded;

	public void Set(Image image, string name, bool squared = true)
	{
		this.name = name;
		this.image = image;
		width = image.GetWidth();
		height = image.GetHeight();
		if (squared)
		{
			arraySize = ((width >= height) ? width : height);
		}
		else
		{
			arraySize = -1;
		}
		if (squared)
		{
			maskArray = new float[arraySize, arraySize];
			image.Lock();
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					maskArray[x + (arraySize - width) / 2, y + (arraySize - height) / 2] = image.GetPixel(x, y).v;
				}
			}
			image.Unlock();
		}
		else
		{
			maskArray = new float[width, height];
			image.Lock();
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					maskArray[j, i] = image.GetPixel(j, i).v;
				}
			}
			image.Unlock();
		}
		float ratio = 1f * (float)width / (float)height;
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
		thumbnail = new Image();
		thumbnail.CopyFrom(image);
		thumbnail.Resize(newWidth, newHeight, Image.Interpolation.Nearest);
		isLoaded = true;
	}

	public void Load(string file, bool squared = true)
	{
		isLoaded = false;
		string fileType = file.Substring(file.Length - 3, 3);
		if (fileType == "png" || fileType == "jpg")
		{
			Image image = new Image();
			image.Load(file);
			if (!image.IsEmpty())
			{
				Set(image, file.GetFile(), squared);
			}
		}
	}
}
