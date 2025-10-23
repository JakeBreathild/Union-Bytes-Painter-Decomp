using System.IO;
using Godot;

public class ImportTextureWindowDialog : WindowDialog
{
	private enum SourceChannel
	{
		RGBA,
		R,
		G,
		B,
		A,
		GRAY
	}

	private Workspace workspace;

	private string textureFilePath = "";

	private LineEdit textureFilePathLineEdit;

	private Image textureImage;

	private ImageTexture textureImageTexture;

	private TextureRect previewTextureRect;

	private Vector2 previewTextureRectSize = Vector2.Zero;

	private Vector2 previewTextureRectPosition = Vector2.Zero;

	private int textureWidth;

	private LineEdit textureWidthLineEdit;

	private int textureHeight;

	private LineEdit textureHeightLineEdit;

	private ChannelArray<Color> sourceColorArray;

	private SourceChannel sourceChannel;

	private OptionButton sourceChannelOptionButton;

	private Layer.ChannelEnum outputChannel;

	private OptionButton outputChannelOptionButton;

	private int xOffset;

	private SpinBox xOffsetSpinBox;

	private int yOffset;

	private SpinBox yOffsetSpinBox;

	private CheckBox[] positionCheckBoxesArray = new CheckBox[9];

	private bool repeat;

	private CheckButton repeatCheckButton;

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		string nodeGroupPath = "SC/VC/Source/";
		textureFilePathLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Path");
		previewTextureRect = GetNodeOrNull<TextureRect>(nodeGroupPath + "Preview");
		previewTextureRectSize = previewTextureRect.RectSize;
		previewTextureRectSize.x = Mathf.Round(previewTextureRectSize.x);
		previewTextureRectSize.y = Mathf.Round(previewTextureRectSize.y);
		previewTextureRectPosition = previewTextureRect.RectPosition;
		previewTextureRectPosition.x = Mathf.Round(previewTextureRectPosition.x);
		previewTextureRectPosition.y = Mathf.Round(previewTextureRectPosition.y);
		textureWidthLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Width");
		textureHeightLineEdit = GetNodeOrNull<LineEdit>(nodeGroupPath + "Height");
		sourceChannelOptionButton = GetNodeOrNull<OptionButton>(nodeGroupPath + "SourceChannel");
		sourceChannelOptionButton.AddItem("RGBA");
		sourceChannelOptionButton.AddItem("R");
		sourceChannelOptionButton.AddItem("G");
		sourceChannelOptionButton.AddItem("B");
		sourceChannelOptionButton.AddItem("A");
		sourceChannelOptionButton.AddItem("Gray");
		sourceChannelOptionButton.Select((int)sourceChannel);
		sourceChannelOptionButton.Connect(Signals.ItemSelected, this, "ChangeSourceChannel");
		nodeGroupPath = "SC/VC/Target/";
		outputChannel = Layer.ChannelEnum.COLOR;
		outputChannelOptionButton = GetNodeOrNull<OptionButton>(nodeGroupPath + "Channel");
		outputChannelOptionButton.AddItem("Color");
		outputChannelOptionButton.AddItem("Roughness");
		outputChannelOptionButton.AddItem("Metallicity");
		outputChannelOptionButton.AddItem("Height");
		outputChannelOptionButton.AddItem("Emission");
		outputChannelOptionButton.Connect(Signals.ItemSelected, this, "ChangeOutputChannel");
		xOffsetSpinBox = GetNodeOrNull<SpinBox>(nodeGroupPath + "XOffset");
		xOffsetSpinBox.Connect(Signals.ValueChanged, this, "ChangeXOffset");
		yOffsetSpinBox = GetNodeOrNull<SpinBox>(nodeGroupPath + "YOffset");
		yOffsetSpinBox.Connect(Signals.ValueChanged, this, "ChangeYOffset");
		for (int i = 0; i < 9; i++)
		{
			positionCheckBoxesArray[i] = GetNodeOrNull<GridContainer>(nodeGroupPath + "Position").GetChild<CheckBox>(i);
		}
		positionCheckBoxesArray[0].Pressed = true;
		positionCheckBoxesArray[0].Connect(Signals.Pressed, this, "ChangePositionUL");
		positionCheckBoxesArray[1].Connect(Signals.Pressed, this, "ChangePositionUC");
		positionCheckBoxesArray[2].Connect(Signals.Pressed, this, "ChangePositionUR");
		positionCheckBoxesArray[3].Connect(Signals.Pressed, this, "ChangePositionCL");
		positionCheckBoxesArray[4].Connect(Signals.Pressed, this, "ChangePositionC");
		positionCheckBoxesArray[5].Connect(Signals.Pressed, this, "ChangePositionCR");
		positionCheckBoxesArray[6].Connect(Signals.Pressed, this, "ChangePositionBL");
		positionCheckBoxesArray[7].Connect(Signals.Pressed, this, "ChangePositionBC");
		positionCheckBoxesArray[8].Connect(Signals.Pressed, this, "ChangePositionBR");
		repeatCheckButton = GetNodeOrNull<CheckButton>(nodeGroupPath + "Repeat");
		repeatCheckButton.Connect(Signals.Toggled, this, "ChangeRepeat");
		Connect(Signals.Hide, this, "Hide");
	}

	public void PopupCentered(string file)
	{
		file = file.Trim();
		if (file != "")
		{
			textureFilePath = file;
			textureImage = new Image();
			textureImage.Load(textureFilePath);
			Reset();
			InputManager.WindowShown();
			PopupCentered();
		}
	}

	public new void Hide()
	{
		base.Hide();
		textureImage = null;
		textureImageTexture = null;
		sourceColorArray = null;
		InputManager.WindowHidden();
		InputManager.SkipInput = true;
	}

	public void Reset()
	{
		GetNodeOrNull<ScrollContainer>("SC").ScrollVertical = 0;
		textureFilePathLineEdit.Text = textureFilePath;
		textureWidth = textureImage.GetWidth();
		textureHeight = textureImage.GetHeight();
		sourceColorArray = new ChannelArray<Color>(textureWidth, textureHeight);
		textureImage.Lock();
		for (int y = 0; y < textureHeight; y++)
		{
			for (int x = 0; x < textureWidth; x++)
			{
				sourceColorArray[x, y] = textureImage.GetPixel(x, y);
			}
		}
		textureImage.Unlock();
		textureImageTexture = new ImageTexture();
		textureImageTexture.CreateFromImage(textureImage, 0u);
		previewTextureRect.Texture = textureImageTexture;
		textureWidthLineEdit.Text = textureWidth.ToString();
		textureHeightLineEdit.Text = textureHeight.ToString();
		sourceChannel = SourceChannel.RGBA;
		sourceChannelOptionButton.Select((int)sourceChannel);
		outputChannel = Layer.ChannelEnum.COLOR;
		outputChannelOptionButton.Select(0);
		xOffset = 0;
		xOffsetSpinBox.Value = xOffset;
		yOffset = 0;
		yOffsetSpinBox.Value = yOffset;
		ClearPosition();
		positionCheckBoxesArray[0].Pressed = true;
		repeat = false;
		repeatCheckButton.Pressed = repeat;
		float ratio = 1f * (float)textureWidth / (float)textureHeight;
		if (ratio != 1f)
		{
			Vector2 newTextureRectSize = previewTextureRectSize;
			Vector2 newTextureRectPosition = previewTextureRectPosition;
			if (ratio < 1f)
			{
				newTextureRectSize.x *= ratio;
				newTextureRectPosition.x += (previewTextureRectSize.x - newTextureRectSize.x) * 0.5f;
			}
			else
			{
				newTextureRectSize.y /= ratio;
				newTextureRectPosition.y += (previewTextureRectSize.y - newTextureRectSize.y) * 0.5f;
			}
			previewTextureRect.RectSize = newTextureRectSize;
			previewTextureRect.RectPosition = newTextureRectPosition;
		}
		else
		{
			previewTextureRect.RectSize = previewTextureRectSize;
			previewTextureRect.RectPosition = previewTextureRectPosition;
		}
	}

	public void Import()
	{
		Data data = workspace.Worksheet.Data;
		xOffset = Mathf.RoundToInt((float)xOffsetSpinBox.Value);
		yOffset = Mathf.RoundToInt((float)yOffsetSpinBox.Value);
		int startX;
		int startY;
		int endX;
		int endY;
		if (repeat)
		{
			startX = 0;
			startY = 0;
			endX = data.Width;
			endY = data.Height;
		}
		else if (data.Tileable)
		{
			startX = xOffset;
			startY = yOffset;
			endX = startX + textureWidth;
			endY = startY + textureHeight;
		}
		else
		{
			startX = Mathf.Clamp(xOffset, 0, data.Width);
			startY = Mathf.Clamp(yOffset, 0, data.Height);
			endX = Mathf.Clamp(xOffset + textureWidth, 0, data.Width);
			endY = Mathf.Clamp(yOffset + textureHeight, 0, data.Height);
		}
		Value value = new Value(0f);
		Color color = new Color(0f, 0f, 0f);
		ChannelArray<Color> outputColorChannelArray = null;
		ChannelArray<Value> outputFloatChannelArray = null;
		switch (outputChannel)
		{
		case Layer.ChannelEnum.COLOR:
			outputColorChannelArray = new ChannelArray<Color>((Color[,])data.ColorChannel.Array.Clone(), data.ColorChannel.Width, data.ColorChannel.Height);
			break;
		case Layer.ChannelEnum.ROUGHNESS:
			outputFloatChannelArray = new ChannelArray<Value>((Value[,])data.RoughnessChannel.Array.Clone(), data.RoughnessChannel.Width, data.RoughnessChannel.Height);
			break;
		case Layer.ChannelEnum.METALLICITY:
			outputFloatChannelArray = new ChannelArray<Value>((Value[,])data.MetallicityChannel.Array.Clone(), data.MetallicityChannel.Width, data.MetallicityChannel.Height);
			break;
		case Layer.ChannelEnum.HEIGHT:
			outputFloatChannelArray = new ChannelArray<Value>((Value[,])data.HeightChannel.Array.Clone(), data.HeightChannel.Width, data.HeightChannel.Height);
			break;
		case Layer.ChannelEnum.EMISSION:
			outputColorChannelArray = new ChannelArray<Color>((Color[,])data.EmissionChannel.Array.Clone(), data.EmissionChannel.Width, data.EmissionChannel.Height);
			break;
		}
		for (int y = startY; y < endY; y++)
		{
			for (int x = startX; x < endX; x++)
			{
				int xxx = x;
				int yyy = y;
				if (data.Tileable)
				{
					xxx = Mathf.PosMod(xxx, data.Width);
					yyy = Mathf.PosMod(yyy, data.Height);
				}
				int xx = Mathf.PosMod(x - xOffset, textureWidth);
				int yy = Mathf.PosMod(y - yOffset, textureHeight);
				if (outputColorChannelArray != null)
				{
					color = outputColorChannelArray[xxx, yyy];
					switch (sourceChannel)
					{
					case SourceChannel.RGBA:
						color = sourceColorArray[xx, yy];
						break;
					case SourceChannel.R:
						color.r = sourceColorArray[xx, yy].r;
						break;
					case SourceChannel.G:
						color.g = sourceColorArray[xx, yy].g;
						break;
					case SourceChannel.B:
						color.b = sourceColorArray[xx, yy].b;
						break;
					case SourceChannel.A:
						color.a = sourceColorArray[xx, yy].a;
						break;
					case SourceChannel.GRAY:
						color.r = (color.g = (color.b = sourceColorArray[xx, yy].v));
						break;
					}
					outputColorChannelArray[xxx, yyy] = color;
				}
				else if (outputFloatChannelArray != null)
				{
					switch (sourceChannel)
					{
					case SourceChannel.R:
						value.v = sourceColorArray[xx, yy].r;
						break;
					case SourceChannel.G:
						value.v = sourceColorArray[xx, yy].g;
						break;
					case SourceChannel.B:
						value.v = sourceColorArray[xx, yy].b;
						break;
					case SourceChannel.A:
						value.v = sourceColorArray[xx, yy].a;
						break;
					case SourceChannel.RGBA:
					case SourceChannel.GRAY:
						value.v = sourceColorArray[xx, yy].v;
						break;
					}
					outputFloatChannelArray[xxx, yyy] = value;
				}
			}
		}
		string layerName = "Import: " + System.IO.Path.GetFileName(textureFilePath);
		((LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER)).LayerCommandType = LayerCommand.LayerCommandTypeEnum.ADDNEW;
		workspace.Worksheet.History.StopRecording("Layer Added [" + layerName + "]");
		Layer layer = workspace.Worksheet.Data.Layer;
		layer.Name = layerName;
		FilterCommand filterCommand = (FilterCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.FILTER);
		switch (outputChannel)
		{
		case Layer.ChannelEnum.COLOR:
			filterCommand.ChannelType = Layer.ChannelEnum.COLOR;
			filterCommand.Channel = layer.ColorChannel;
			filterCommand.ColorArray = outputColorChannelArray.Array;
			break;
		case Layer.ChannelEnum.ROUGHNESS:
			filterCommand.ChannelType = Layer.ChannelEnum.ROUGHNESS;
			filterCommand.Channel = layer.RoughnessChannel;
			filterCommand.ValueArray = outputFloatChannelArray.Array;
			break;
		case Layer.ChannelEnum.METALLICITY:
			filterCommand.ChannelType = Layer.ChannelEnum.METALLICITY;
			filterCommand.Channel = layer.MetallicityChannel;
			filterCommand.ValueArray = outputFloatChannelArray.Array;
			break;
		case Layer.ChannelEnum.HEIGHT:
			filterCommand.ChannelType = Layer.ChannelEnum.HEIGHT;
			filterCommand.Channel = layer.HeightChannel;
			filterCommand.ValueArray = outputFloatChannelArray.Array;
			break;
		case Layer.ChannelEnum.EMISSION:
			filterCommand.ChannelType = Layer.ChannelEnum.EMISSION;
			filterCommand.Channel = layer.EmissionChannel;
			filterCommand.ColorArray = outputColorChannelArray.Array;
			break;
		}
		workspace.Worksheet.History.StopRecording("Texture Import [" + layerName + " (" + Layer.ChannelName[(int)outputChannel] + ")]");
		switch (outputChannel)
		{
		case Layer.ChannelEnum.COLOR:
			layer.ColorChannel.IsVisible = true;
			layer.ColorChannel.DetectContentArea();
			layer.RoughnessChannel.IsVisible = true;
			layer.MetallicityChannel.IsVisible = true;
			layer.HeightChannel.IsVisible = true;
			layer.EmissionChannel.IsVisible = false;
			break;
		case Layer.ChannelEnum.ROUGHNESS:
			layer.ColorChannel.IsVisible = true;
			layer.RoughnessChannel.IsVisible = true;
			layer.RoughnessChannel.DetectContentArea();
			layer.MetallicityChannel.IsVisible = true;
			layer.HeightChannel.IsVisible = true;
			layer.EmissionChannel.IsVisible = false;
			break;
		case Layer.ChannelEnum.METALLICITY:
			layer.ColorChannel.IsVisible = true;
			layer.RoughnessChannel.IsVisible = true;
			layer.MetallicityChannel.IsVisible = true;
			layer.MetallicityChannel.DetectContentArea();
			layer.HeightChannel.IsVisible = true;
			layer.EmissionChannel.IsVisible = false;
			break;
		case Layer.ChannelEnum.HEIGHT:
			layer.ColorChannel.IsVisible = true;
			layer.RoughnessChannel.IsVisible = true;
			layer.MetallicityChannel.IsVisible = true;
			layer.HeightChannel.IsVisible = true;
			layer.HeightChannel.DetectContentArea();
			layer.EmissionChannel.IsVisible = false;
			break;
		case Layer.ChannelEnum.EMISSION:
			layer.ColorChannel.IsVisible = true;
			layer.RoughnessChannel.IsVisible = true;
			layer.MetallicityChannel.IsVisible = true;
			layer.HeightChannel.IsVisible = true;
			layer.EmissionChannel.IsVisible = true;
			layer.EmissionChannel.DetectContentArea();
			break;
		}
		data.CombineLayersFromAllChannels();
		Register.Gui.LayerPanel.Reset();
		Settings.ImportPath = System.IO.Path.GetFullPath(textureFilePath);
		Hide();
	}

	public void ChangeSourceChannel(int index)
	{
		switch (index)
		{
		case 0:
			sourceChannel = SourceChannel.RGBA;
			break;
		case 1:
			sourceChannel = SourceChannel.R;
			break;
		case 2:
			sourceChannel = SourceChannel.G;
			break;
		case 3:
			sourceChannel = SourceChannel.B;
			break;
		case 4:
			sourceChannel = SourceChannel.A;
			break;
		case 5:
			sourceChannel = SourceChannel.GRAY;
			break;
		}
	}

	public void ChangeOutputChannel(int index)
	{
		switch (index)
		{
		case 0:
			outputChannel = Layer.ChannelEnum.COLOR;
			break;
		case 1:
			outputChannel = Layer.ChannelEnum.ROUGHNESS;
			break;
		case 2:
			outputChannel = Layer.ChannelEnum.METALLICITY;
			break;
		case 3:
			outputChannel = Layer.ChannelEnum.HEIGHT;
			break;
		case 4:
			outputChannel = Layer.ChannelEnum.EMISSION;
			break;
		}
	}

	public void ChangeXOffset(float value)
	{
		xOffset = Mathf.RoundToInt(value);
		ClearPosition();
	}

	public void ChangeYOffset(float value)
	{
		yOffset = Mathf.RoundToInt(value);
		ClearPosition();
	}

	public void ClearPosition()
	{
		for (int i = 0; i < 9; i++)
		{
			positionCheckBoxesArray[i].Pressed = false;
		}
	}

	public void ChangePositionUL()
	{
		ClearPosition();
		xOffset = 0;
		xOffsetSpinBox.Value = xOffset;
		yOffset = 0;
		yOffsetSpinBox.Value = yOffset;
		positionCheckBoxesArray[0].Pressed = true;
	}

	public void ChangePositionUC()
	{
		ClearPosition();
		xOffset = (workspace.Worksheet.Data.Width - textureWidth) / 2;
		xOffsetSpinBox.Value = xOffset;
		yOffset = 0;
		yOffsetSpinBox.Value = yOffset;
		positionCheckBoxesArray[1].Pressed = true;
	}

	public void ChangePositionUR()
	{
		ClearPosition();
		xOffset = workspace.Worksheet.Data.Width - textureWidth;
		xOffsetSpinBox.Value = xOffset;
		yOffset = 0;
		yOffsetSpinBox.Value = yOffset;
		positionCheckBoxesArray[2].Pressed = true;
	}

	public void ChangePositionCL()
	{
		ClearPosition();
		xOffset = 0;
		xOffsetSpinBox.Value = xOffset;
		yOffset = (workspace.Worksheet.Data.Height - textureHeight) / 2;
		yOffsetSpinBox.Value = yOffset;
		positionCheckBoxesArray[3].Pressed = true;
	}

	public void ChangePositionC()
	{
		ClearPosition();
		xOffset = (workspace.Worksheet.Data.Width - textureWidth) / 2;
		xOffsetSpinBox.Value = xOffset;
		yOffset = (workspace.Worksheet.Data.Height - textureHeight) / 2;
		yOffsetSpinBox.Value = yOffset;
		positionCheckBoxesArray[4].Pressed = true;
	}

	public void ChangePositionCR()
	{
		ClearPosition();
		xOffset = workspace.Worksheet.Data.Width - textureWidth;
		xOffsetSpinBox.Value = xOffset;
		yOffset = (workspace.Worksheet.Data.Height - textureHeight) / 2;
		yOffsetSpinBox.Value = yOffset;
		positionCheckBoxesArray[5].Pressed = true;
	}

	public void ChangePositionBL()
	{
		ClearPosition();
		xOffset = 0;
		xOffsetSpinBox.Value = xOffset;
		yOffset = workspace.Worksheet.Data.Height - textureHeight;
		yOffsetSpinBox.Value = yOffset;
		positionCheckBoxesArray[6].Pressed = true;
	}

	public void ChangePositionBC()
	{
		ClearPosition();
		xOffset = (workspace.Worksheet.Data.Width - textureWidth) / 2;
		xOffsetSpinBox.Value = xOffset;
		yOffset = workspace.Worksheet.Data.Height - textureHeight;
		yOffsetSpinBox.Value = yOffset;
		positionCheckBoxesArray[7].Pressed = true;
	}

	public void ChangePositionBR()
	{
		ClearPosition();
		xOffset = workspace.Worksheet.Data.Width - textureWidth;
		xOffsetSpinBox.Value = xOffset;
		yOffset = workspace.Worksheet.Data.Height - textureHeight;
		yOffsetSpinBox.Value = yOffset;
		positionCheckBoxesArray[8].Pressed = true;
	}

	public void ChangeRepeat(bool enabled)
	{
		repeat = enabled;
	}
}
