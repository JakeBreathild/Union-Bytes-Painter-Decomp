using System.IO;
using Godot;

public class Main : Node
{
	public const string VERSION_TEXT = "Demo";

	public const int VERSION = 1000;

	public const int VERSION_YEAR = 2024;

	public const int VERSION_MONTH = 10;

	public const int VERSION_DAY = 5;

	public static string WindowTitle = "";

	[Export(PropertyHint.None, "")]
	public ShaderMaterial DebugMaterial;

	[Export(PropertyHint.None, "")]
	public NodePath PreviewViewportContainerNodePath;

	[Export(PropertyHint.None, "")]
	public NodePath DebugRichTextLabelNodePath;

	public Main()
	{
		if (!System.IO.File.Exists("Settings.xml"))
		{
			OS.CenterWindow();
		}
		Register.Main = this;
		Settings.Path = FileSystem.GetPath();
		if (OS.GetName() == "Windows")
		{
			Settings.DecalsPath = Settings.Path + "\\Decals";
			Settings.DecalsBookshelfPath = Settings.DecalsPath;
			Settings.MasksPath = Settings.Path + "\\Masks";
			Settings.MasksBookshelfPath = Settings.MasksPath;
			Settings.ImportPath = Settings.Path + "\\Import";
			Settings.ExportPath = Settings.Path + "\\Export";
			Settings.PresetsPath = Settings.Path + "\\Presets";
			Settings.ProjectsPath = Settings.Path + "\\Projects";
		}
		else
		{
			Settings.DecalsPath = Settings.Path + "/Decals";
			Settings.DecalsBookshelfPath = Settings.DecalsPath;
			Settings.MasksPath = Settings.Path + "/Masks";
			Settings.MasksBookshelfPath = Settings.MasksPath;
			Settings.ImportPath = Settings.Path + "/Import";
			Settings.ExportPath = Settings.Path + "/Export";
			Settings.PresetsPath = Settings.Path + "/Presets";
			Settings.ProjectsPath = Settings.Path + "/Projects";
		}
	}

	public override void _Ready()
	{
		base._Ready();
		OS.MinWindowSize = Settings.MinimumWindowSize;
		WindowTitle = "Union Bytes Painter Demo";
		OS.SetWindowTitle(WindowTitle);
		DebugManager.SetRichTextLabel(GetNodeOrNull<RichTextLabel>(DebugRichTextLabelNodePath));
		InputManager._Ready();
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		InputManager._Input(@event);
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		Register.delta = delta;
		if (DebugManager.GetImmediateGeometry() == null)
		{
			ImmediateGeometry immediateGeometry = new ImmediateGeometry();
			Viewport previewViewport = GetNodeOrNull<PreviewspaceViewportContainer>(PreviewViewportContainerNodePath).GetViewport();
			previewViewport.AddChild(immediateGeometry);
			immediateGeometry.Owner = previewViewport;
			immediateGeometry.SetLayerMaskBit(0, enabled: false);
			immediateGeometry.SetLayerMaskBit(19, enabled: true);
			immediateGeometry.GlobalTransform = Transform.Identity;
			immediateGeometry.MaterialOverride = DebugMaterial;
			immediateGeometry.Name = "DebugDraw";
			DebugManager.SetImmediateGeometry(immediateGeometry);
		}
		DebugManager._Process(delta);
		DebugManager.AddLine("FPS: " + Engine.GetFramesPerSecond());
		DebugManager.AddLine("");
		InputManager._Process(delta);
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Data.Thread.Abort();
	}
}
