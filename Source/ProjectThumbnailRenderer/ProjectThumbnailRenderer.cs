using Godot;

public class ProjectThumbnailRenderer : Viewport
{
	public override void _Ready()
	{
		base._Ready();
	}

	public Image RenderThumbnail()
	{
		switch (Register.PreviewspaceMeshManager.Shader)
		{
		case PreviewspaceMeshManager.ShaderEnum.DEFAULT:
			GetChildOrNull<MeshInstance>(0).MaterialOverride = null;
			break;
		case PreviewspaceMeshManager.ShaderEnum.TRANSMISSION:
			GetChildOrNull<MeshInstance>(0).MaterialOverride = Resources.PreviewFullTransmissionMaterial;
			break;
		}
		base.RenderTargetUpdateMode = UpdateMode.Once;
		VisualServer.ForceDraw();
		Image data = GetTexture().GetData();
		int width = Settings.ThumbnailSize;
		int height = Settings.ThumbnailSize;
		float ratio = 1f * (float)Register.Workspace.Worksheet.Data.Width / (float)Register.Workspace.Worksheet.Data.Height;
		if (ratio != 1f)
		{
			if (ratio < 1f)
			{
				width = Mathf.RoundToInt((float)width * ratio);
			}
			else
			{
				height = Mathf.RoundToInt((float)height / ratio);
			}
		}
		data.Resize(width, height, Image.Interpolation.Nearest);
		return data;
	}
}
