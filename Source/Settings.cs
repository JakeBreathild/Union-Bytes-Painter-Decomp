using System.Collections.Generic;
using Godot;

public static class Settings
{
	public static Vector2 MinimumWindowSize = new Vector2(1400f, 820f);

	public static int MaximumTextureSize = 512;

	public static int ThumbnailSize = 96;

	public static float EmissionStrength = 3.5f;

	public static Color AccentColor = new Color(0.068125f, 0.696875f, 0.35078f);

	public static Color GridColor = new Color(0.06f, 0.06f, 0.06f, 0.5f);

	public static Color MirrorColor = new Color(1f, 0.1f, 0.06f);

	public static Color WhiteColor = new Color(1f, 1f, 1f);

	public static Color BlackColor = new Color(0f, 0f, 0f);

	public static Color BlankColor = new Color(0f, 0f, 0f, 0f);

	public static Color AlignmentColor = new Color(0.1f, 0.5f, 0.8f);

	public static Color NodeColor = AccentColor;

	public static Color NodeSelectedColor = new Color(1f, 1f, 0.3f);

	public static Color NodeErrorColor = new Color(1f, 0.25f, 0.25f);

	public static Color ColorChannelColor = new Color(0.2f, 0.8f, 0.4f, 0.7f);

	public static Color RoughnessChannelColor = new Color(0.8f, 0.2f, 0.2f, 0.7f);

	public static Color MetallicityChannelColor = new Color(0.2f, 0.2f, 0.8f, 0.7f);

	public static Color HeightChannelColor = new Color(0.8f, 0.8f, 0.1f, 0.7f);

	public static Color EmissionChannelColor = new Color(0.8f, 0.4f, 0.8f, 0.7f);

	public static float CheckerOffset = 0.1f;

	public static float CursorOffset = 0.0075f;

	public static float GridOffset = 0.05f;

	public static float AlignmentToolOffset = 0.07f;

	public static float MirrorOffset = 0.075f;

	public static float UvLayoutOffset = 0.05f;

	public static float UvNodesOffset = 0.2f;

	public static float OutlineOffset = 0.25f;

	public static string Path = "";

	public static string DecalsPath = "";

	public static string DecalsBookshelfPath = "";

	public static string MasksPath = "";

	public static string MasksBookshelfPath = "";

	public static string ImportPath = "";

	public static string ExportPath = "";

	public static string PresetsPath = "";

	public static string ProjectsPath = "";

	public static bool SaveWindowPosition = true;

	public static bool ShowControlsHelp = true;

	public static List<string> FavouritesList = new List<string>();

	public static HashSet<string> FavouritesHashSet = new HashSet<string>();

	public static List<string> RecentlyOpenedList = new List<string>();

	public static HashSet<string> RecentlyOpenedHashSet = new HashSet<string>();

	public static void SetFullscreen(bool enable)
	{
		OS.WindowFullscreen = enable;
	}

	public static void ToggleFullscreen()
	{
		OS.WindowFullscreen = !OS.WindowFullscreen;
	}
}
