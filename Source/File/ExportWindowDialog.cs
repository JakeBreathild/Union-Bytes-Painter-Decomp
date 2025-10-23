using System.IO;
using Godot;

public class ExportWindowDialog : WindowDialog
{
	private Workspace workspace;

	private string exportFile = "Export.txt";

	private string exportPath = "";

	private LineEdit pathLineEdit;

	private LineEdit worksheetNameLineEdit;

	private BoxContainer exportEntriesBoxContainer;

	[Export(PropertyHint.None, "")]
	private PackedScene defaultExportEntryPackedScene;

	private Button addExportEntryButton;

	public string ExportPath
	{
		get
		{
			return exportPath;
		}
		set
		{
			exportPath = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		pathLineEdit = GetNodeOrNull<LineEdit>("Path");
		worksheetNameLineEdit = GetNodeOrNull<LineEdit>("WorksheetName");
		exportEntriesBoxContainer = GetNodeOrNull("SC").GetNodeOrNull<BoxContainer>("VC");
		for (int i = 0; i < exportEntriesBoxContainer.GetChildCount(); i++)
		{
			if (exportEntriesBoxContainer.GetChild(i) is Button)
			{
				addExportEntryButton = exportEntriesBoxContainer.GetChild<Button>(i);
				break;
			}
		}
		addExportEntryButton.Connect(Signals.Pressed, this, "AddExportEntry");
	}

	public new void PopupCentered(Vector2? size = null)
	{
		InputManager.WindowShown();
		base.PopupCentered(size);
		Reset();
	}

	public new void Hide()
	{
		base.Hide();
		InputManager.WindowHidden();
		InputManager.SkipInput = true;
	}

	public void Reset()
	{
		pathLineEdit.Text = exportPath;
		worksheetNameLineEdit.Text = workspace.Worksheet.SheetName + "_";
		if (!System.IO.File.Exists(exportFile))
		{
			return;
		}
		for (int i = 0; i < exportEntriesBoxContainer.GetChildCount(); i++)
		{
			if (exportEntriesBoxContainer.GetChild(i) is ExportEntry)
			{
				exportEntriesBoxContainer.GetChild(i).QueueFree();
			}
		}
		StreamReader streamReader = new StreamReader(exportFile);
		string line = streamReader.ReadLine();
		int count = 0;
		if (line.IsValidInteger())
		{
			count = line.ToInt();
		}
		for (int j = 0; j < count; j++)
		{
			ExportEntry defaultExportEntry = defaultExportEntryPackedScene.InstanceOrNull<ExportEntry>();
			if (defaultExportEntry != null)
			{
				exportEntriesBoxContainer.AddChild(defaultExportEntry);
				defaultExportEntry.Owner = exportEntriesBoxContainer;
				string[] stringArray = streamReader.ReadLine().Split(new char[1] { '/' });
				int index = 0;
				line = stringArray[0].Trim();
				if (line.IsValidInteger())
				{
					index = line.ToInt();
				}
				exportEntriesBoxContainer.MoveChild(defaultExportEntry, index);
				line = stringArray[1].Trim().ToLower();
				if (line != "true")
				{
					defaultExportEntry.CheckBoxPressed = false;
				}
				switch (stringArray[2].Trim().ToUpper())
				{
				case "GRAY":
					defaultExportEntry.EntryType = ExportEntry.EntryTypeEnum.GRAY;
					break;
				case "RGB":
					defaultExportEntry.EntryType = ExportEntry.EntryTypeEnum.RGB;
					break;
				case "RGBA":
					defaultExportEntry.EntryType = ExportEntry.EntryTypeEnum.RGBA;
					break;
				case "RGB_A":
					defaultExportEntry.EntryType = ExportEntry.EntryTypeEnum.RGB_A;
					break;
				case "R_G_B":
					defaultExportEntry.EntryType = ExportEntry.EntryTypeEnum.R_G_B;
					break;
				case "R_G_B_A":
					defaultExportEntry.EntryType = ExportEntry.EntryTypeEnum.R_G_B_A;
					break;
				}
				defaultExportEntry.Suffix = stringArray[3].Trim();
				switch (defaultExportEntry.EntryType)
				{
				case ExportEntry.EntryTypeEnum.GRAY:
					defaultExportEntry.RContent = ConvertStringToSingleChannelContent(stringArray[4].Trim());
					break;
				case ExportEntry.EntryTypeEnum.RGB:
				case ExportEntry.EntryTypeEnum.RGBA:
					defaultExportEntry.RgbContent = ConvertStringToFullChannelContent(stringArray[4].Trim());
					break;
				case ExportEntry.EntryTypeEnum.RGB_A:
					defaultExportEntry.RgbContent = ConvertStringToFullChannelContent(stringArray[4].Trim());
					defaultExportEntry.AContent = ConvertStringToSingleChannelContent(stringArray[5].Trim());
					break;
				case ExportEntry.EntryTypeEnum.R_G_B:
					defaultExportEntry.RContent = ConvertStringToSingleChannelContent(stringArray[4].Trim());
					defaultExportEntry.GContent = ConvertStringToSingleChannelContent(stringArray[5].Trim());
					defaultExportEntry.BContent = ConvertStringToSingleChannelContent(stringArray[6].Trim());
					break;
				case ExportEntry.EntryTypeEnum.R_G_B_A:
					defaultExportEntry.RContent = ConvertStringToSingleChannelContent(stringArray[4].Trim());
					defaultExportEntry.GContent = ConvertStringToSingleChannelContent(stringArray[5].Trim());
					defaultExportEntry.BContent = ConvertStringToSingleChannelContent(stringArray[6].Trim());
					defaultExportEntry.AContent = ConvertStringToSingleChannelContent(stringArray[7].Trim());
					break;
				}
			}
		}
		streamReader.Dispose();
	}

	private ExportEntry.SingleChannelContentEnum ConvertStringToSingleChannelContent(string text)
	{
		return text.ToUpper() switch
		{
			"BLACK" => ExportEntry.SingleChannelContentEnum.BLACK, 
			"WHITE" => ExportEntry.SingleChannelContentEnum.WHITE, 
			"COLOR_GRAYSCALE" => ExportEntry.SingleChannelContentEnum.COLOR_GRAYSCALE, 
			"COLOR_ALPHA" => ExportEntry.SingleChannelContentEnum.COLOR_ALPHA, 
			"ROUGHNESS" => ExportEntry.SingleChannelContentEnum.ROUGHNESS, 
			"METALLICITY" => ExportEntry.SingleChannelContentEnum.METALLICITY, 
			"HEIGHT" => ExportEntry.SingleChannelContentEnum.HEIGHT, 
			"EMISSION_GRAYSCALE" => ExportEntry.SingleChannelContentEnum.EMISSION_GRAYSCALE, 
			"ROUGHNESS_INV" => ExportEntry.SingleChannelContentEnum.ROUGHNESS_INV, 
			"METALLICITY_INV" => ExportEntry.SingleChannelContentEnum.METALLICITY_INV, 
			"HEIGHT_INV" => ExportEntry.SingleChannelContentEnum.HEIGHT_INV, 
			_ => ExportEntry.SingleChannelContentEnum.BLACK, 
		};
	}

	private ExportEntry.FullChannelContentEnum ConvertStringToFullChannelContent(string text)
	{
		return text.ToUpper() switch
		{
			"BLACK" => ExportEntry.FullChannelContentEnum.BLACK, 
			"WHITE" => ExportEntry.FullChannelContentEnum.WHITE, 
			"COLOR" => ExportEntry.FullChannelContentEnum.COLOR, 
			"NORMAL_OPENGL" => ExportEntry.FullChannelContentEnum.NORMAL_OPENGL, 
			"NORMAL_DIRECTX" => ExportEntry.FullChannelContentEnum.NORMAL_DIRECTX, 
			"EMISSION" => ExportEntry.FullChannelContentEnum.EMISSION, 
			_ => ExportEntry.FullChannelContentEnum.BLACK, 
		};
	}

	private float SingleChannelValue(ExportEntry.SingleChannelContentEnum channelContent, int x, int y)
	{
		return channelContent switch
		{
			ExportEntry.SingleChannelContentEnum.BLACK => 0f, 
			ExportEntry.SingleChannelContentEnum.WHITE => 1f, 
			ExportEntry.SingleChannelContentEnum.COLOR_GRAYSCALE => workspace.Worksheet.Data.ColorChannel.Array[x, y].v, 
			ExportEntry.SingleChannelContentEnum.COLOR_ALPHA => workspace.Worksheet.Data.ColorChannel.Array[x, y].a, 
			ExportEntry.SingleChannelContentEnum.ROUGHNESS => workspace.Worksheet.Data.RoughnessChannel.Array[x, y].v, 
			ExportEntry.SingleChannelContentEnum.METALLICITY => workspace.Worksheet.Data.MetallicityChannel.Array[x, y].v, 
			ExportEntry.SingleChannelContentEnum.HEIGHT => workspace.Worksheet.Data.HeightChannel.Array[x, y].v, 
			ExportEntry.SingleChannelContentEnum.EMISSION_GRAYSCALE => workspace.Worksheet.Data.EmissionChannel.Array[x, y].v, 
			ExportEntry.SingleChannelContentEnum.ROUGHNESS_INV => 1f - workspace.Worksheet.Data.RoughnessChannel.Array[x, y].v, 
			ExportEntry.SingleChannelContentEnum.METALLICITY_INV => 1f - workspace.Worksheet.Data.MetallicityChannel.Array[x, y].v, 
			ExportEntry.SingleChannelContentEnum.HEIGHT_INV => 1f - workspace.Worksheet.Data.HeightChannel.Array[x, y].v, 
			_ => 0f, 
		};
	}

	private Color FullChannelValue(ExportEntry.FullChannelContentEnum channelContent, int x, int y)
	{
		switch (channelContent)
		{
		case ExportEntry.FullChannelContentEnum.BLACK:
			return new Color(0f, 0f, 0f);
		case ExportEntry.FullChannelContentEnum.WHITE:
			return new Color(1f, 1f, 1f);
		case ExportEntry.FullChannelContentEnum.COLOR:
			return workspace.Worksheet.Data.ColorChannel.Array[x, y];
		case ExportEntry.FullChannelContentEnum.NORMAL_OPENGL:
			return workspace.Worksheet.Data.NormalChannel.Array[x, y];
		case ExportEntry.FullChannelContentEnum.NORMAL_DIRECTX:
		{
			Color color = workspace.Worksheet.Data.NormalChannel.Array[x, y];
			color.g = 1f - color.g;
			return color;
		}
		case ExportEntry.FullChannelContentEnum.EMISSION:
			return workspace.Worksheet.Data.EmissionChannel.Array[x, y];
		default:
			return new Color(0f, 0f, 0f);
		}
	}

	public void Export()
	{
		Image image = new Image();
		image.Create(workspace.Worksheet.Data.Width, workspace.Worksheet.Data.Height, useMipmaps: false, Image.Format.Rgba8);
		string name = worksheetNameLineEdit.Text.Trim();
		for (int i = 0; i < exportEntriesBoxContainer.GetChildCount(); i++)
		{
			if (!(exportEntriesBoxContainer.GetChild(i) is ExportEntry))
			{
				continue;
			}
			ExportEntry defaultExportEntry = exportEntriesBoxContainer.GetChild<ExportEntry>(i);
			if (!defaultExportEntry.CheckBoxPressed)
			{
				continue;
			}
			string suffix = defaultExportEntry.Suffix.Trim();
			string file = exportPath + "//" + name + suffix + ".png";
			Color color = new Color(0f, 0f, 0f);
			image.Lock();
			switch (defaultExportEntry.EntryType)
			{
			case ExportEntry.EntryTypeEnum.GRAY:
			{
				for (int n = 0; n < workspace.Worksheet.Data.Height; n++)
				{
					for (int num = 0; num < workspace.Worksheet.Data.Width; num++)
					{
						color.b = (color.g = (color.r = SingleChannelValue(defaultExportEntry.RContent, num, n)));
						color.a = 1f;
						image.SetPixel(num, n, color);
					}
				}
				break;
			}
			case ExportEntry.EntryTypeEnum.RGB:
			{
				for (int num4 = 0; num4 < workspace.Worksheet.Data.Height; num4++)
				{
					for (int num5 = 0; num5 < workspace.Worksheet.Data.Width; num5++)
					{
						color = FullChannelValue(defaultExportEntry.RgbContent, num5, num4);
						color.a = 1f;
						image.SetPixel(num5, num4, color);
					}
				}
				break;
			}
			case ExportEntry.EntryTypeEnum.RGBA:
			{
				for (int j = 0; j < workspace.Worksheet.Data.Height; j++)
				{
					for (int k = 0; k < workspace.Worksheet.Data.Width; k++)
					{
						color = FullChannelValue(defaultExportEntry.RgbContent, k, j);
						image.SetPixel(k, j, color);
					}
				}
				break;
			}
			case ExportEntry.EntryTypeEnum.RGB_A:
			{
				for (int num2 = 0; num2 < workspace.Worksheet.Data.Height; num2++)
				{
					for (int num3 = 0; num3 < workspace.Worksheet.Data.Width; num3++)
					{
						color = FullChannelValue(defaultExportEntry.RgbContent, num3, num2);
						color.a = SingleChannelValue(defaultExportEntry.AContent, num3, num2);
						image.SetPixel(num3, num2, color);
					}
				}
				break;
			}
			case ExportEntry.EntryTypeEnum.R_G_B:
			{
				for (int l = 0; l < workspace.Worksheet.Data.Height; l++)
				{
					for (int m = 0; m < workspace.Worksheet.Data.Width; m++)
					{
						color.r = SingleChannelValue(defaultExportEntry.RContent, m, l);
						color.g = SingleChannelValue(defaultExportEntry.GContent, m, l);
						color.b = SingleChannelValue(defaultExportEntry.BContent, m, l);
						color.a = 1f;
						image.SetPixel(m, l, color);
					}
				}
				break;
			}
			case ExportEntry.EntryTypeEnum.R_G_B_A:
			{
				for (int y = 0; y < workspace.Worksheet.Data.Height; y++)
				{
					for (int x = 0; x < workspace.Worksheet.Data.Width; x++)
					{
						color.r = SingleChannelValue(defaultExportEntry.RContent, x, y);
						color.g = SingleChannelValue(defaultExportEntry.GContent, x, y);
						color.b = SingleChannelValue(defaultExportEntry.BContent, x, y);
						color.a = SingleChannelValue(defaultExportEntry.AContent, x, y);
						image.SetPixel(x, y, color);
					}
				}
				break;
			}
			}
			image.Unlock();
			image.SavePng(file);
		}
		Hide();
	}

	public void AddExportEntry()
	{
		ExportEntry defaultExportEntry = defaultExportEntryPackedScene.InstanceOrNull<ExportEntry>();
		if (defaultExportEntry != null)
		{
			exportEntriesBoxContainer.AddChild(defaultExportEntry);
			defaultExportEntry.Owner = exportEntriesBoxContainer;
			exportEntriesBoxContainer.MoveChild(defaultExportEntry, addExportEntryButton.GetIndex());
			defaultExportEntry.EntryType = ExportEntry.EntryTypeEnum.GRAY;
		}
	}
}
