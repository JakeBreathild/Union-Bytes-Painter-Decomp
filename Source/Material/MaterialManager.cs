using Godot;

public static class MaterialManager
{
	public enum MaterialEnum
	{
		CHECKER,
		WORKSPACE,
		WORKSPACE_FULL,
		WORKSPACE_CHANNEL,
		PREVIEW,
		PREVIEW_FULL,
		PREVIEW_FULL_TWOSIDES,
		PREVIEW_FULL_TRANSMISSION,
		PREVIEW_CHANNEL,
		PREVIEW_CHANNEL_TWOSIDES,
		GRID,
		NODE,
		OUTLINE,
		COUNT
	}

	private static ShaderMaterial[] shaderMaterialsArray = new ShaderMaterial[13];

	public static ShaderMaterial GetShaderMaterial(MaterialEnum material)
	{
		return shaderMaterialsArray[(int)material];
	}

	public static void SetShaderMaterial(MaterialEnum material, ShaderMaterial shaderMaterial)
	{
		shaderMaterialsArray[(int)material] = shaderMaterial;
	}

	public static void SetShaderParam(MaterialEnum material, string param, object value)
	{
		shaderMaterialsArray[(int)material].SetShaderParam(param, value);
	}

	public static void UpdateChannelMaterials()
	{
		Workspace workspace = Register.Workspace;
		Data textureData = workspace.Worksheet.Data;
		switch (workspace.Channel)
		{
		case Data.ChannelEnum.COLOR:
			shaderMaterialsArray[3].SetShaderParam("channelTexture", textureData.ColorChannel.ImageTexture);
			break;
		case Data.ChannelEnum.ROUGHNESS:
			shaderMaterialsArray[3].SetShaderParam("channelTexture", textureData.RoughnessChannel.ImageTexture);
			break;
		case Data.ChannelEnum.METALLICITY:
			shaderMaterialsArray[3].SetShaderParam("channelTexture", textureData.MetallicityChannel.ImageTexture);
			break;
		case Data.ChannelEnum.HEIGHT:
			shaderMaterialsArray[3].SetShaderParam("channelTexture", textureData.HeightChannel.ImageTexture);
			break;
		case Data.ChannelEnum.NORMAL:
			shaderMaterialsArray[3].SetShaderParam("channelTexture", textureData.NormalChannel.ImageTexture);
			break;
		case Data.ChannelEnum.EMISSION:
			shaderMaterialsArray[3].SetShaderParam("channelTexture", textureData.EmissionChannel.ImageTexture);
			break;
		}
		if (Register.PreviewspaceMeshManager.Illumination == PreviewspaceMeshManager.IlluminationEnum.CHANNEL)
		{
			switch (workspace.Channel)
			{
			case Data.ChannelEnum.COLOR:
				shaderMaterialsArray[8].SetShaderParam("channelTexture", textureData.ColorChannel.ImageTexture);
				break;
			case Data.ChannelEnum.ROUGHNESS:
				shaderMaterialsArray[8].SetShaderParam("channelTexture", textureData.RoughnessChannel.ImageTexture);
				break;
			case Data.ChannelEnum.METALLICITY:
				shaderMaterialsArray[8].SetShaderParam("channelTexture", textureData.MetallicityChannel.ImageTexture);
				break;
			case Data.ChannelEnum.HEIGHT:
				shaderMaterialsArray[8].SetShaderParam("channelTexture", textureData.HeightChannel.ImageTexture);
				break;
			case Data.ChannelEnum.NORMAL:
				shaderMaterialsArray[8].SetShaderParam("channelTexture", textureData.NormalChannel.ImageTexture);
				break;
			case Data.ChannelEnum.EMISSION:
				shaderMaterialsArray[8].SetShaderParam("channelTexture", textureData.EmissionChannel.ImageTexture);
				break;
			}
		}
	}

	public static void UpdateDrawingMaterials()
	{
		Workspace workspace = Register.Workspace;
		DrawingManager drawingManager = Register.DrawingManager;
		switch (workspace.Channel)
		{
		case Data.ChannelEnum.FULL:
			shaderMaterialsArray[1].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
			shaderMaterialsArray[1].SetShaderParam("previewColor", drawingManager.Color);
			shaderMaterialsArray[1].SetShaderParam("previewRoughness", drawingManager.Roughness.v);
			shaderMaterialsArray[1].SetShaderParam("previewMetallicity", drawingManager.Metallicity.v);
			shaderMaterialsArray[1].SetShaderParam("previewEmission", drawingManager.Emission);
			break;
		case Data.ChannelEnum.COLOR:
			shaderMaterialsArray[3].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
			shaderMaterialsArray[3].SetShaderParam("previewColor", drawingManager.Color);
			break;
		case Data.ChannelEnum.ROUGHNESS:
		{
			shaderMaterialsArray[3].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
			Color valueColor = new Color(drawingManager.Roughness.v, drawingManager.Roughness.v, drawingManager.Roughness.v);
			shaderMaterialsArray[3].SetShaderParam("previewColor", valueColor);
			break;
		}
		case Data.ChannelEnum.METALLICITY:
		{
			shaderMaterialsArray[3].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
			Color valueColor = new Color(drawingManager.Metallicity.v, drawingManager.Metallicity.v, drawingManager.Metallicity.v);
			shaderMaterialsArray[3].SetShaderParam("previewColor", valueColor);
			break;
		}
		case Data.ChannelEnum.HEIGHT:
		{
			shaderMaterialsArray[3].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
			Color valueColor = new Color(drawingManager.Height.v, drawingManager.Height.v, drawingManager.Height.v);
			shaderMaterialsArray[3].SetShaderParam("previewColor", valueColor);
			break;
		}
		case Data.ChannelEnum.NORMAL:
		{
			shaderMaterialsArray[3].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
			Color valueColor = new Color(drawingManager.Height.v, drawingManager.Height.v, drawingManager.Height.v);
			shaderMaterialsArray[3].SetShaderParam("previewColor", valueColor);
			break;
		}
		case Data.ChannelEnum.EMISSION:
			shaderMaterialsArray[3].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
			shaderMaterialsArray[3].SetShaderParam("previewColor", drawingManager.Emission);
			break;
		}
		switch (Register.PreviewspaceMeshManager.Illumination)
		{
		case PreviewspaceMeshManager.IlluminationEnum.ILLUMINATED:
			shaderMaterialsArray[4].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
			shaderMaterialsArray[4].SetShaderParam("previewColor", drawingManager.Color);
			shaderMaterialsArray[4].SetShaderParam("previewRoughness", drawingManager.Roughness.v);
			shaderMaterialsArray[4].SetShaderParam("previewMetallicity", drawingManager.Metallicity.v);
			shaderMaterialsArray[4].SetShaderParam("previewEmission", drawingManager.Emission);
			break;
		case PreviewspaceMeshManager.IlluminationEnum.UNLIT:
			shaderMaterialsArray[8].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
			shaderMaterialsArray[8].SetShaderParam("previewColor", drawingManager.Color);
			break;
		case PreviewspaceMeshManager.IlluminationEnum.CHANNEL:
			switch (workspace.Channel)
			{
			case Data.ChannelEnum.FULL:
				Register.PreviewspaceMeshManager.ActivateFullShaderMaterial();
				shaderMaterialsArray[4].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
				shaderMaterialsArray[4].SetShaderParam("previewColor", drawingManager.Color);
				shaderMaterialsArray[4].SetShaderParam("previewRoughness", drawingManager.Roughness.v);
				shaderMaterialsArray[4].SetShaderParam("previewMetallicity", drawingManager.Metallicity.v);
				shaderMaterialsArray[4].SetShaderParam("previewEmission", drawingManager.Emission);
				break;
			case Data.ChannelEnum.COLOR:
				Register.PreviewspaceMeshManager.ActivateChannelShaderMaterial();
				shaderMaterialsArray[8].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
				shaderMaterialsArray[8].SetShaderParam("previewColor", drawingManager.Color);
				break;
			case Data.ChannelEnum.ROUGHNESS:
			{
				Register.PreviewspaceMeshManager.ActivateChannelShaderMaterial();
				shaderMaterialsArray[8].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
				Color valueColor = new Color(drawingManager.Roughness.v, drawingManager.Roughness.v, drawingManager.Roughness.v);
				shaderMaterialsArray[8].SetShaderParam("previewColor", valueColor);
				break;
			}
			case Data.ChannelEnum.METALLICITY:
			{
				Register.PreviewspaceMeshManager.ActivateChannelShaderMaterial();
				shaderMaterialsArray[8].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
				Color valueColor = new Color(drawingManager.Metallicity.v, drawingManager.Metallicity.v, drawingManager.Metallicity.v);
				shaderMaterialsArray[8].SetShaderParam("previewColor", valueColor);
				break;
			}
			case Data.ChannelEnum.HEIGHT:
			{
				Register.PreviewspaceMeshManager.ActivateChannelShaderMaterial();
				shaderMaterialsArray[8].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
				Color valueColor = new Color(drawingManager.Height.v, drawingManager.Height.v, drawingManager.Height.v);
				shaderMaterialsArray[8].SetShaderParam("previewColor", valueColor);
				break;
			}
			case Data.ChannelEnum.NORMAL:
			{
				Register.PreviewspaceMeshManager.ActivateChannelShaderMaterial();
				shaderMaterialsArray[8].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
				Color valueColor = new Color(drawingManager.Height.v, drawingManager.Height.v, drawingManager.Height.v);
				shaderMaterialsArray[8].SetShaderParam("previewColor", valueColor);
				break;
			}
			case Data.ChannelEnum.EMISSION:
				Register.PreviewspaceMeshManager.ActivateChannelShaderMaterial();
				shaderMaterialsArray[8].SetShaderParam("previewBlendingMode", (int)drawingManager.BlendingMode);
				shaderMaterialsArray[8].SetShaderParam("previewColor", drawingManager.Emission);
				break;
			}
			break;
		}
	}

	public static float GetEmissionStrength()
	{
		return Settings.EmissionStrength;
	}

	public static void SetEmissionStrength(float emissionStrength)
	{
		Settings.EmissionStrength = emissionStrength;
		SetShaderParam(MaterialEnum.WORKSPACE, "emissionStrength", Settings.EmissionStrength);
		SetShaderParam(MaterialEnum.PREVIEW, "emissionStrength", Settings.EmissionStrength);
	}
}
