using Godot;

public class ResourcesLoader : Node
{
	[Export(PropertyHint.None, "")]
	private Theme defaultTheme;

	[Export(PropertyHint.None, "")]
	private StyleBox librarySelectionStyleBox;

	[Export(PropertyHint.None, "")]
	private DynamicFont defaultFont;

	[Export(PropertyHint.None, "")]
	private DynamicFont boldFont;

	[Export(PropertyHint.None, "")]
	private Environment workspaceEnvironment;

	[Export(PropertyHint.None, "")]
	private Environment previewEnvironment;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial workspaceFullMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial workspaceChannelMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial checkerMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial gridMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial outlineMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial nodeMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial previewspaceDebugMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial previewFullMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial previewFullTwoSidesMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial previewFullTransmissionMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial previewChannelMaterial;

	[Export(PropertyHint.None, "")]
	private ShaderMaterial previewChannelTwoSidesMaterial;

	[Export(PropertyHint.None, "")]
	private Image defaultMaskImage;

	[Export(PropertyHint.None, "")]
	private Texture lightIconTexture;

	[Export(PropertyHint.None, "")]
	private Texture librarySyncIcon;

	[Export(PropertyHint.None, "")]
	private Texture oldFileVersionIconTexture;

	[Export(PropertyHint.None, "")]
	private PackedScene toolCameraScene;

	[Export(PropertyHint.None, "")]
	private PackedScene thumbnailRendererScene;

	[Export(PropertyHint.None, "")]
	private PackedScene projectThumbnailRendererScene;

	public override void _Ready()
	{
		base._Ready();
		Resources.DefaultTheme = defaultTheme;
		Resources.LibrarySelectionStyleBox = librarySelectionStyleBox;
		Resources.DefaultFont = defaultFont;
		Resources.BoldFont = boldFont;
		Resources.WorkspaceEnvironment = workspaceEnvironment;
		Resources.PreviewEnvironment = previewEnvironment;
		Resources.WorkspaceFullMaterial = workspaceFullMaterial;
		Resources.WorkspaceChannelMaterial = workspaceChannelMaterial;
		Resources.CheckerMaterial = checkerMaterial;
		Resources.GridMaterial = gridMaterial;
		Resources.OutlineMaterial = outlineMaterial;
		Resources.NodeMaterial = nodeMaterial;
		Resources.PreviewspaceDebugMaterial = previewspaceDebugMaterial;
		Resources.PreviewFullMaterial = previewFullMaterial;
		Resources.PreviewFullTwoSidesMaterial = previewFullTwoSidesMaterial;
		Resources.PreviewFullTransmissionMaterial = previewFullTransmissionMaterial;
		Resources.PreviewChannelMaterial = previewChannelMaterial;
		Resources.PreviewChannelTwoSidesMaterial = previewChannelTwoSidesMaterial;
		Resources.DefaultMaskImage = defaultMaskImage;
		Resources.LightIconTexture = lightIconTexture;
		Resources.LibrarySyncIconTexture = librarySyncIcon;
		Resources.OldFileVersionIconTexture = oldFileVersionIconTexture;
		Resources.ToolCameraScene = toolCameraScene;
		Resources.ThumbnailRendererScene = thumbnailRendererScene;
		Resources.ProjectThumbnailRendererScene = projectThumbnailRendererScene;
	}
}
