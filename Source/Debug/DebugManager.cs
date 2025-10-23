using Godot;

public static class DebugManager
{
	private static RichTextLabel richTextLabel = null;

	private static bool doDebug = false;

	private static string text = "";

	private static bool enable = true;

	private static ImmediateGeometry immediateGeometry = null;

	public static bool Enable
	{
		get
		{
			return enable;
		}
		set
		{
			enable = value;
			if (enable && richTextLabel != null)
			{
				doDebug = true;
			}
			else
			{
				doDebug = false;
			}
		}
	}

	public static ImmediateGeometry ImmediateGeometry => immediateGeometry;

	public static void _Process(float delta)
	{
		if (doDebug)
		{
			richTextLabel.Clear();
			richTextLabel.Text = text;
		}
		Clear();
	}

	public static RichTextLabel GetRichTextLabel()
	{
		return richTextLabel;
	}

	public static void SetRichTextLabel(RichTextLabel richTextLabel)
	{
		DebugManager.richTextLabel = richTextLabel;
		if (enable && DebugManager.richTextLabel != null)
		{
			doDebug = true;
		}
		else
		{
			doDebug = false;
		}
	}

	public static void Clear()
	{
		text = "";
	}

	public static void AddLine(string line)
	{
		text = text + line + "\n";
	}

	public static void AddLine(object line)
	{
		text = text + line.ToString() + "\n";
	}

	public static void SetImmediateGeometry(ImmediateGeometry immediateGeometry)
	{
		DebugManager.immediateGeometry = immediateGeometry;
	}

	public static ImmediateGeometry GetImmediateGeometry()
	{
		return immediateGeometry;
	}
}
