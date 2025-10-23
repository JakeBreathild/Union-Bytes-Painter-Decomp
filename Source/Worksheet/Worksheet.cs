using System.Collections.Generic;
using System.IO;
using Godot;

public class Worksheet : Node
{
	private string sheetName = "Unnamed";

	private string fileName;

	private Data data;

	private History history;

	public string SheetName
	{
		get
		{
			return sheetName;
		}
		set
		{
			sheetName = value;
		}
	}

	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
		}
	}

	public Data Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public History History => history;

	public List<DrawingPreset> DrawingPresetsList { get; set; }

	private Worksheet()
	{
		history = new History(this, HistoryAddCommand, HistoryRemoveCommandRange, HistorySelectCommand);
		HistoryClear();
		DrawingPresetsList = new List<DrawingPreset>();
		sheetName = "Unnamed";
		base.Name = "Worksheet";
	}

	private Worksheet(string name = "")
	{
		history = new History(this, HistoryAddCommand, HistoryRemoveCommandRange, HistorySelectCommand);
		HistoryClear();
		DrawingPresetsList = new List<DrawingPreset>();
		sheetName = name;
		base.Name = "Worksheet";
	}

	public Worksheet(int width, int height, bool tileable, string name = "")
	{
		history = new History(this, HistoryAddCommand, HistoryRemoveCommandRange, HistorySelectCommand);
		HistoryClear();
		data = new Data(this, width, height, tileable);
		SetShaderTextures();
		DrawingPresetsList = new List<DrawingPreset>();
		DrawingPreset.AddDefaultDrawingPresets(DrawingPresetsList);
		sheetName = name;
		base.Name = "Worksheet";
	}

	public void SetShaderTextures()
	{
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "colorMapTexture", data.ColorChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "normalMapTexture", data.NormalChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "roughnessMapTexture", data.RoughnessChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "metallicityMapTexture", data.MetallicityChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "emissionMapTexture", data.EmissionChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, "colorMapTexture", data.ColorChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "colorMapTexture", data.ColorChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "normalMapTexture", data.NormalChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "roughnessMapTexture", data.RoughnessChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "metallicityMapTexture", data.MetallicityChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "emissionMapTexture", data.EmissionChannel.ImageTexture);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, "colorMapTexture", data.ColorChannel.ImageTexture);
	}

	public void Export(string path)
	{
		data.Export(path, SheetName);
	}

	public void WriteToBinaryStream(BinaryWriter binaryWriter)
	{
		binaryWriter.Write(SheetName);
		data.WriteToBinaryStream(binaryWriter);
		binaryWriter.Write(new byte[64]);
		binaryWriter.Write((int)Register.PreviewspaceMeshManager.Shader);
		binaryWriter.Write(DrawingPresetsList.Count);
		for (int i = 0; i < DrawingPresetsList.Count; i++)
		{
			DrawingPresetsList[i].WriteToBinaryStream(binaryWriter);
		}
	}

	public static Worksheet ReadFromBinaryStream_009(BinaryReader binaryReader)
	{
		Worksheet worksheet = new Worksheet(binaryReader.ReadString());
		worksheet.Data = Data.ReadFromBinaryStream_009(binaryReader, worksheet);
		worksheet.SetShaderTextures();
		Register.PreviewspaceMeshManager.ChangeShader(worksheet.Data, (PreviewspaceMeshManager.ShaderEnum)binaryReader.ReadInt32());
		int drawingPresetsCount = binaryReader.ReadInt32();
		for (int i = 0; i < drawingPresetsCount; i++)
		{
			worksheet.DrawingPresetsList.Add(DrawingPreset.ReadFromBinaryStream_009(binaryReader));
		}
		return worksheet;
	}

	public static Worksheet ReadFromBinaryStream(BinaryReader binaryReader)
	{
		Worksheet worksheet = new Worksheet(binaryReader.ReadString());
		worksheet.Data = Data.ReadFromBinaryStream(binaryReader, worksheet);
		worksheet.SetShaderTextures();
		binaryReader.ReadBytes(64);
		Register.PreviewspaceMeshManager.ChangeShader(worksheet.Data, (PreviewspaceMeshManager.ShaderEnum)binaryReader.ReadInt32());
		int drawingPresetsCount = binaryReader.ReadInt32();
		for (int i = 0; i < drawingPresetsCount; i++)
		{
			worksheet.DrawingPresetsList.Add(DrawingPreset.ReadFromBinaryStream(binaryReader));
		}
		return worksheet;
	}

	public void WriteDrawingPresetsToBinaryStream(BinaryWriter binaryWriter)
	{
		binaryWriter.Write(DrawingPresetsList.Count);
		for (int i = 0; i < DrawingPresetsList.Count; i++)
		{
			DrawingPresetsList[i].WriteToBinaryStream(binaryWriter);
		}
	}

	public static void ReadDrawingPresetsFromBinaryStream(Worksheet worksheet, BinaryReader binaryReader)
	{
		worksheet.DrawingPresetsList.Clear();
		int drawingPresetsCount = binaryReader.ReadInt32();
		for (int i = 0; i < drawingPresetsCount; i++)
		{
			worksheet.DrawingPresetsList.Add(DrawingPreset.ReadFromBinaryStream(binaryReader));
		}
	}

	public void HistoryAddCommand(string name)
	{
		Register.Gui.HistoryItemList.UnselectAll();
		Register.Gui.HistoryItemList.AddItem(name);
		Register.Gui.HistoryItemList.EnsureCurrentIsVisible();
	}

	public void HistoryRemoveCommandRange(int index, int count)
	{
		Register.Gui.HistoryItemList.UnselectAll();
		for (int i = count - 1; i >= 0; i--)
		{
			Register.Gui.HistoryItemList.RemoveItem(index + i);
		}
	}

	public void HistoryClear()
	{
		Register.Gui.HistoryItemList?.UnselectAll();
		Register.Gui.HistoryItemList?.Clear();
	}

	public void HistorySelectCommand(int index)
	{
		if (index < 0)
		{
			Register.Gui.HistoryItemList.UnselectAll();
		}
		else
		{
			Register.Gui.HistoryItemList.Select(index);
		}
		Register.Gui.HistoryItemList.EnsureCurrentIsVisible();
	}
}
