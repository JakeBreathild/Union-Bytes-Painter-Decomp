using Godot;

public class LayerChannelControl : Panel
{
	[Export(PropertyHint.None, "")]
	private Layer.ChannelEnum channel;

	private Button visibleButton;

	private OptionButton blendingModeOptionButton;

	private DefaultSlider blendingStrengthDefaultSlider;

	private Panel emptyPanel;

	public override void _Ready()
	{
		base._Ready();
		visibleButton = GetChildOrNull<Button>(0);
		visibleButton.Connect(Signals.Toggled, this, "VisibilityChanged");
		blendingModeOptionButton = GetChildOrNull<OptionButton>(1);
		for (int i = 0; i < 12; i++)
		{
			blendingModeOptionButton.AddItem(Blender.BlendingModeName[i], i);
			if (channel == Layer.ChannelEnum.COLOR)
			{
				if ((Blender.BlendingType[i] & Blender.BlendingTypeEnum.COLOR) == 0)
				{
					blendingModeOptionButton.SetItemDisabled(i, disabled: true);
				}
			}
			else if ((Blender.BlendingType[i] & Blender.BlendingTypeEnum.VALUE) == 0)
			{
				blendingModeOptionButton.SetItemDisabled(i, disabled: true);
			}
		}
		blendingModeOptionButton.Connect(Signals.ItemSelected, this, "BlendingModeChanged");
		blendingStrengthDefaultSlider = GetChildOrNull<DefaultSlider>(2);
		blendingStrengthDefaultSlider.Connect(Signals.ValueChanged, this, "BlendingStrengthChanged");
		switch (channel)
		{
		case Layer.ChannelEnum.COLOR:
			visibleButton.Text = "Color";
			blendingModeOptionButton.Selected = 0;
			break;
		case Layer.ChannelEnum.ROUGHNESS:
			visibleButton.Text = "Roughness";
			blendingModeOptionButton.Selected = 0;
			break;
		case Layer.ChannelEnum.METALLICITY:
			visibleButton.Text = "Metallicity";
			blendingModeOptionButton.Selected = 0;
			break;
		case Layer.ChannelEnum.HEIGHT:
			visibleButton.Text = "Height";
			blendingModeOptionButton.Selected = 0;
			break;
		case Layer.ChannelEnum.EMISSION:
			visibleButton.Text = "Emission";
			blendingModeOptionButton.Selected = 0;
			break;
		}
		emptyPanel = GetChildOrNull<Panel>(3);
	}

	public void Reset()
	{
		Layer layer = Register.Workspace.Worksheet.Data.Layer;
		visibleButton.SetBlockSignals(enable: true);
		blendingModeOptionButton.SetBlockSignals(enable: true);
		blendingStrengthDefaultSlider.SetBlockSignals(enable: true);
		switch (channel)
		{
		case Layer.ChannelEnum.COLOR:
			visibleButton.Pressed = layer.ColorChannel.IsVisible;
			blendingModeOptionButton.Selected = (int)layer.ColorChannel.BlendingMode;
			blendingStrengthDefaultSlider.SetValue(layer.ColorChannel.BlendingStrength);
			emptyPanel.Visible = !layer.ColorChannel.HasContent;
			break;
		case Layer.ChannelEnum.ROUGHNESS:
			visibleButton.Pressed = layer.RoughnessChannel.IsVisible;
			blendingModeOptionButton.Selected = (int)layer.RoughnessChannel.BlendingMode;
			blendingStrengthDefaultSlider.SetValue(layer.RoughnessChannel.BlendingStrength);
			emptyPanel.Visible = !layer.RoughnessChannel.HasContent;
			break;
		case Layer.ChannelEnum.METALLICITY:
			visibleButton.Pressed = layer.MetallicityChannel.IsVisible;
			blendingModeOptionButton.Selected = (int)layer.MetallicityChannel.BlendingMode;
			blendingStrengthDefaultSlider.SetValue(layer.MetallicityChannel.BlendingStrength);
			emptyPanel.Visible = !layer.MetallicityChannel.HasContent;
			break;
		case Layer.ChannelEnum.HEIGHT:
			visibleButton.Pressed = layer.HeightChannel.IsVisible;
			blendingModeOptionButton.Selected = (int)layer.HeightChannel.BlendingMode;
			blendingStrengthDefaultSlider.SetValue(layer.HeightChannel.BlendingStrength);
			emptyPanel.Visible = !layer.HeightChannel.HasContent;
			break;
		case Layer.ChannelEnum.EMISSION:
			visibleButton.Pressed = layer.EmissionChannel.IsVisible;
			blendingModeOptionButton.Selected = (int)layer.EmissionChannel.BlendingMode;
			blendingStrengthDefaultSlider.SetValue(layer.EmissionChannel.BlendingStrength);
			emptyPanel.Visible = !layer.EmissionChannel.HasContent;
			break;
		}
		visibleButton.SetBlockSignals(enable: false);
		blendingModeOptionButton.SetBlockSignals(enable: false);
		blendingStrengthDefaultSlider.SetBlockSignals(enable: false);
	}

	public void VisibilityChanged(bool pressed)
	{
		Layer layer = Register.Workspace.Worksheet.Data.Layer;
		LayerCommand obj = (LayerCommand)Register.Workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
		obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.CHANNELVISIBILITY;
		obj.Layer = layer;
		obj.Channel = channel;
		obj.NewState = pressed;
		Register.Workspace.Worksheet.History.StopRecording("Channel Visibility [" + layer.Name + " (" + Layer.ChannelName[(int)channel] + ")]");
	}

	public void BlendingModeChanged(int index)
	{
		Layer layer = Register.Workspace.Worksheet.Data.Layer;
		LayerCommand obj = (LayerCommand)Register.Workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
		obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.CHANNELBLENDINGMODE;
		obj.Layer = layer;
		obj.Channel = channel;
		obj.NewBlendingMode = (Blender.BlendingModeEnum)index;
		Register.Workspace.Worksheet.History.StopRecording("Channel BlendingMode [" + layer.Name + " (" + Layer.ChannelName[(int)channel] + ")]");
	}

	public void BlendingStrengthChanged(float value)
	{
		Data data = Register.Workspace.Worksheet.Data;
		Layer layer = data.Layer;
		switch (channel)
		{
		case Layer.ChannelEnum.COLOR:
			layer.ColorChannel.BlendingStrength = blendingStrengthDefaultSlider.GetValue();
			data.CombineLayers(Layer.ChannelEnum.COLOR);
			break;
		case Layer.ChannelEnum.ROUGHNESS:
			layer.RoughnessChannel.BlendingStrength = blendingStrengthDefaultSlider.GetValue();
			data.CombineLayers(Layer.ChannelEnum.ROUGHNESS);
			break;
		case Layer.ChannelEnum.METALLICITY:
			layer.MetallicityChannel.BlendingStrength = blendingStrengthDefaultSlider.GetValue();
			data.CombineLayers(Layer.ChannelEnum.METALLICITY);
			break;
		case Layer.ChannelEnum.HEIGHT:
			layer.HeightChannel.BlendingStrength = blendingStrengthDefaultSlider.GetValue();
			data.CombineLayers(Layer.ChannelEnum.HEIGHT);
			break;
		case Layer.ChannelEnum.EMISSION:
			layer.EmissionChannel.BlendingStrength = blendingStrengthDefaultSlider.GetValue();
			data.CombineLayers(Layer.ChannelEnum.EMISSION);
			break;
		}
	}

	public void HasContentChanged(bool hasContent)
	{
		emptyPanel.Visible = !hasContent;
	}
}
