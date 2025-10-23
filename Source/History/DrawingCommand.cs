using System.Collections.Generic;
using Godot;

public class DrawingCommand : ICommand
{
	private Layer layer;

	private Data data;

	private History.CommandTypeEnum type = History.CommandTypeEnum.DRAWING;

	private string name = "Drawing";

	private bool doErase;

	private Blender.BlendingModeEnum blendingMode;

	private List<Pixel> pixelsList;

	public Data Data => data;

	public int Type => (int)type;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public bool DoErase
	{
		get
		{
			return doErase;
		}
		set
		{
			doErase = value;
		}
	}

	public Blender.BlendingModeEnum BlendingMode
	{
		get
		{
			return blendingMode;
		}
		set
		{
			blendingMode = value;
		}
	}

	public bool DrawingOnColorChannel { get; set; } = true;

	public bool DrawingOnColorAlphaChannel { get; set; } = true;

	public bool DrawingOnHeightChannel { get; set; } = true;

	public bool DrawingOnRoughnessChannel { get; set; } = true;

	public bool DrawingOnMetallicityChannel { get; set; } = true;

	public bool DrawingOnEmissionChannel { get; set; } = true;

	public DrawingCommand(Worksheet worksheet)
	{
		data = worksheet.Data;
		layer = data.Layer;
		pixelsList = new List<Pixel>();
	}

	public void SetBlendingMode(Blender.BlendingModeEnum blendingMode)
	{
		this.blendingMode = blendingMode;
	}

	public void CopyDrawingManagerBlendingMode()
	{
		blendingMode = Register.DrawingManager.BlendingMode;
	}

	public void SetChannels(bool color, bool colorAlpha, bool height, bool roughness, bool metallicity, bool emission)
	{
		DrawingOnColorChannel = color;
		DrawingOnColorAlphaChannel = colorAlpha;
		DrawingOnHeightChannel = height;
		DrawingOnRoughnessChannel = roughness;
		DrawingOnMetallicityChannel = metallicity;
		DrawingOnEmissionChannel = emission;
	}

	public void CopyDrawingManagerChannels()
	{
		DrawingOnColorChannel = Register.DrawingManager.ColorEnabled;
		DrawingOnColorAlphaChannel = Register.DrawingManager.ColorAlphaEnabled;
		DrawingOnHeightChannel = Register.DrawingManager.HeightEnabled;
		DrawingOnRoughnessChannel = Register.DrawingManager.RoughnessEnabled;
		DrawingOnMetallicityChannel = Register.DrawingManager.MetallicityEnabled;
		DrawingOnEmissionChannel = Register.DrawingManager.EmissionEnabled;
	}

	public void CopyAllDrawingManagerSettings()
	{
		blendingMode = Register.DrawingManager.BlendingMode;
		DrawingOnColorChannel = Register.DrawingManager.ColorEnabled;
		DrawingOnColorAlphaChannel = Register.DrawingManager.ColorAlphaEnabled;
		DrawingOnHeightChannel = Register.DrawingManager.HeightEnabled;
		DrawingOnRoughnessChannel = Register.DrawingManager.RoughnessEnabled;
		DrawingOnMetallicityChannel = Register.DrawingManager.MetallicityEnabled;
		DrawingOnEmissionChannel = Register.DrawingManager.EmissionEnabled;
	}

	public void AddPixel(int x, int y, Color color, Value height, Value roughness, Value metallicity, Color emission, float blendingStrength = 1f)
	{
		Pixel pixel = default(Pixel);
		pixel.x = x;
		pixel.y = y;
		pixel.BlendingStrength = blendingStrength;
		pixel.Color = color;
		pixel.Height = height;
		pixel.Roughness = roughness;
		pixel.Metallicity = metallicity;
		pixel.Emission = emission;
		pixelsList.Add(pixel);
	}

	public void Execute()
	{
		Pixel pixel = default(Pixel);
		if (!doErase)
		{
			for (int i = 0; i < pixelsList.Count; i++)
			{
				pixel.x = Mathf.PosMod(pixelsList[i].x, data.Width);
				pixel.y = Mathf.PosMod(pixelsList[i].y, data.Height);
				pixel.BlendingStrength = pixelsList[i].BlendingStrength;
				if (DrawingOnColorChannel)
				{
					pixel.Color = layer.ColorChannel[pixel.x, pixel.y];
					data.DrawColor(layer, pixel.x, pixel.y, pixelsList[i].Color, blendingMode, pixelsList[i].BlendingStrength);
				}
				if (DrawingOnRoughnessChannel)
				{
					pixel.Roughness = layer.RoughnessChannel[pixel.x, pixel.y];
					data.DrawRoughness(layer, pixel.x, pixel.y, pixelsList[i].Roughness, blendingMode, pixelsList[i].BlendingStrength);
				}
				if (DrawingOnMetallicityChannel)
				{
					pixel.Metallicity = layer.MetallicityChannel[pixel.x, pixel.y];
					data.DrawMetallicity(layer, pixel.x, pixel.y, pixelsList[i].Metallicity, blendingMode, pixelsList[i].BlendingStrength);
				}
				if (DrawingOnHeightChannel)
				{
					pixel.Height = layer.HeightChannel[pixel.x, pixel.y];
					data.DrawHeight(layer, pixel.x, pixel.y, pixelsList[i].Height, blendingMode, pixelsList[i].BlendingStrength);
				}
				if (DrawingOnEmissionChannel)
				{
					pixel.Emission = layer.EmissionChannel[pixel.x, pixel.y];
					data.DrawEmission(layer, pixel.x, pixel.y, pixelsList[i].Emission, blendingMode, pixelsList[i].BlendingStrength);
				}
				pixelsList[i] = pixel;
			}
		}
		else
		{
			for (int j = 0; j < pixelsList.Count; j++)
			{
				pixel.x = Mathf.PosMod(pixelsList[j].x, data.Width);
				pixel.y = Mathf.PosMod(pixelsList[j].y, data.Height);
				pixel.BlendingStrength = pixelsList[j].BlendingStrength;
				if (DrawingOnColorChannel)
				{
					pixel.Color = layer.ColorChannel[pixel.x, pixel.y];
					layer.ColorChannel.SetValue(pixel.x, pixel.y, layer.ColorChannel[pixel.x, pixel.y] * (1f - pixelsList[j].BlendingStrength) + layer.ColorChannel.DefaultValue * pixelsList[j].BlendingStrength);
					data.CombineLayers(Layer.ChannelEnum.COLOR, pixel.x, pixel.y);
				}
				if (DrawingOnRoughnessChannel)
				{
					pixel.Roughness = layer.RoughnessChannel[pixel.x, pixel.y];
					layer.RoughnessChannel.SetValue(pixel.x, pixel.y, layer.RoughnessChannel[pixel.x, pixel.y] * (1f - pixelsList[j].BlendingStrength) + layer.RoughnessChannel.DefaultValue * pixelsList[j].BlendingStrength);
					data.CombineLayers(Layer.ChannelEnum.ROUGHNESS, pixel.x, pixel.y);
				}
				if (DrawingOnMetallicityChannel)
				{
					pixel.Metallicity = layer.MetallicityChannel[pixel.x, pixel.y];
					layer.MetallicityChannel.SetValue(pixel.x, pixel.y, layer.MetallicityChannel[pixel.x, pixel.y] * (1f - pixelsList[j].BlendingStrength) + layer.MetallicityChannel.DefaultValue * pixelsList[j].BlendingStrength);
					data.CombineLayers(Layer.ChannelEnum.METALLICITY, pixel.x, pixel.y);
				}
				if (DrawingOnHeightChannel)
				{
					pixel.Height = layer.HeightChannel[pixel.x, pixel.y];
					layer.HeightChannel.SetValue(pixel.x, pixel.y, layer.HeightChannel[pixel.x, pixel.y] * (1f - pixelsList[j].BlendingStrength) + layer.HeightChannel.DefaultValue * pixelsList[j].BlendingStrength);
					data.CombineLayers(Layer.ChannelEnum.HEIGHT, pixel.x, pixel.y);
				}
				if (DrawingOnEmissionChannel)
				{
					pixel.Emission = layer.EmissionChannel[pixel.x, pixel.y];
					layer.EmissionChannel.SetValue(pixel.x, pixel.y, layer.EmissionChannel[pixel.x, pixel.y] * (1f - pixelsList[j].BlendingStrength) + layer.EmissionChannel.DefaultValue * pixelsList[j].BlendingStrength);
					data.CombineLayers(Layer.ChannelEnum.EMISSION, pixel.x, pixel.y);
				}
				pixelsList[j] = pixel;
			}
			if (DrawingOnColorChannel)
			{
				layer.ColorChannel.DetectContentArea();
			}
			if (DrawingOnRoughnessChannel)
			{
				layer.RoughnessChannel.DetectContentArea();
			}
			if (DrawingOnMetallicityChannel)
			{
				layer.MetallicityChannel.DetectContentArea();
			}
			if (DrawingOnHeightChannel)
			{
				layer.HeightChannel.DetectContentArea();
			}
			if (DrawingOnEmissionChannel)
			{
				layer.EmissionChannel.DetectContentArea();
			}
		}
		Register.LayerControl.Reset();
	}

	public void Undo()
	{
		Pixel pixel = default(Pixel);
		for (int i = pixelsList.Count - 1; i >= 0; i--)
		{
			int x = (pixel.x = pixelsList[i].x);
			int y = (pixel.y = pixelsList[i].y);
			pixel.BlendingStrength = pixelsList[i].BlendingStrength;
			if (DrawingOnColorChannel)
			{
				pixel.Color = layer.ColorChannel[x, y];
				layer.ColorChannel.SetValue(x, y, pixelsList[i].Color);
				data.CombineLayers(Layer.ChannelEnum.COLOR, x, y);
			}
			if (DrawingOnRoughnessChannel)
			{
				pixel.Roughness = layer.RoughnessChannel[x, y];
				layer.RoughnessChannel.SetValue(x, y, pixelsList[i].Roughness);
				data.CombineLayers(Layer.ChannelEnum.ROUGHNESS, x, y);
			}
			if (DrawingOnMetallicityChannel)
			{
				pixel.Metallicity = layer.MetallicityChannel[x, y];
				layer.MetallicityChannel.SetValue(x, y, pixelsList[i].Metallicity);
				data.CombineLayers(Layer.ChannelEnum.METALLICITY, x, y);
			}
			if (DrawingOnHeightChannel)
			{
				pixel.Height = layer.HeightChannel[x, y];
				layer.HeightChannel.SetValue(x, y, pixelsList[i].Height);
				data.CombineLayers(Layer.ChannelEnum.HEIGHT, x, y);
			}
			if (DrawingOnEmissionChannel)
			{
				pixel.Emission = layer.EmissionChannel[x, y];
				layer.EmissionChannel.SetValue(x, y, pixelsList[i].Emission);
				data.CombineLayers(Layer.ChannelEnum.EMISSION, x, y);
			}
			pixelsList[i] = pixel;
		}
		if (DrawingOnColorChannel)
		{
			layer.ColorChannel.DetectContentArea();
		}
		if (DrawingOnRoughnessChannel)
		{
			layer.RoughnessChannel.DetectContentArea();
		}
		if (DrawingOnMetallicityChannel)
		{
			layer.MetallicityChannel.DetectContentArea();
		}
		if (DrawingOnHeightChannel)
		{
			layer.HeightChannel.DetectContentArea();
		}
		if (DrawingOnEmissionChannel)
		{
			layer.EmissionChannel.DetectContentArea();
		}
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(data.Worksheet);
		}
		Register.LayerControl.Reset();
	}

	public void Redo()
	{
		Pixel pixel = default(Pixel);
		for (int i = 0; i < pixelsList.Count; i++)
		{
			int x = (pixel.x = pixelsList[i].x);
			int y = (pixel.y = pixelsList[i].y);
			pixel.BlendingStrength = pixelsList[i].BlendingStrength;
			if (DrawingOnColorChannel)
			{
				pixel.Color = layer.ColorChannel[x, y];
				layer.ColorChannel.SetValue(x, y, pixelsList[i].Color);
				data.CombineLayers(Layer.ChannelEnum.COLOR, x, y);
			}
			if (DrawingOnRoughnessChannel)
			{
				pixel.Roughness = layer.RoughnessChannel[x, y];
				layer.RoughnessChannel.SetValue(x, y, pixelsList[i].Roughness);
				data.CombineLayers(Layer.ChannelEnum.ROUGHNESS, x, y);
			}
			if (DrawingOnMetallicityChannel)
			{
				pixel.Metallicity = layer.MetallicityChannel[x, y];
				layer.MetallicityChannel.SetValue(x, y, pixelsList[i].Metallicity);
				data.CombineLayers(Layer.ChannelEnum.METALLICITY, x, y);
			}
			if (DrawingOnHeightChannel)
			{
				pixel.Height = layer.HeightChannel[x, y];
				layer.HeightChannel.SetValue(x, y, pixelsList[i].Height);
				data.CombineLayers(Layer.ChannelEnum.HEIGHT, x, y);
			}
			if (DrawingOnEmissionChannel)
			{
				pixel.Emission = layer.EmissionChannel[x, y];
				layer.EmissionChannel.SetValue(x, y, pixelsList[i].Emission);
				data.CombineLayers(Layer.ChannelEnum.EMISSION, x, y);
			}
			pixelsList[i] = pixel;
		}
		if (DrawingOnColorChannel)
		{
			layer.ColorChannel.DetectContentArea();
		}
		if (DrawingOnRoughnessChannel)
		{
			layer.RoughnessChannel.DetectContentArea();
		}
		if (DrawingOnMetallicityChannel)
		{
			layer.MetallicityChannel.DetectContentArea();
		}
		if (DrawingOnHeightChannel)
		{
			layer.HeightChannel.DetectContentArea();
		}
		if (DrawingOnEmissionChannel)
		{
			layer.EmissionChannel.DetectContentArea();
		}
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(data.Worksheet);
		}
		Register.LayerControl.Reset();
	}
}
