using System.Collections.Generic;
using Godot;

public class LayerCommand : ICommand
{
	public enum LayerCommandTypeEnum
	{
		DEFAULT,
		ADDNEW,
		ADD,
		INSERT,
		DELETE,
		DUPLICATE,
		UP,
		DOWN,
		VISIBILITY,
		LOCKED,
		NAME,
		COLOR,
		COMBINEALL,
		COMBINESINGLE,
		INSERTLIST,
		DELETELIST,
		CHANNELVISIBILITY,
		CHANNELBLENDINGMODE
	}

	private Data data;

	private Layer previousLayer;

	private Layer layer;

	private int layerIndex = -1;

	private History.CommandTypeEnum type = History.CommandTypeEnum.LAYER;

	private string name = "Layer";

	public LayerCommandTypeEnum LayerCommandType;

	public Layer.ChannelEnum Channel = Layer.ChannelEnum.ALL;

	public bool NewState;

	public float NewValue;

	public string NewText;

	public Color NewColor;

	public Blender.BlendingModeEnum NewBlendingMode;

	public List<Layer> LayersList = new List<Layer>();

	public List<int> LayersIndexList = new List<int>();

	public Data Data => data;

	public Layer Layer
	{
		get
		{
			return layer;
		}
		set
		{
			layer = value;
		}
	}

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

	public LayerCommand(Worksheet worksheet, Layer layer = null)
	{
		data = worksheet.Data;
		if (layer != null)
		{
			this.layer = layer;
		}
		else
		{
			this.layer = data.Layer;
		}
	}

	public void Execute()
	{
		LayerPanel layerPanel = Register.Gui.LayerPanel;
		LayerControl layerControl = Register.Gui.LayerControl;
		data.Mutex.WaitOne();
		switch (LayerCommandType)
		{
		case LayerCommandTypeEnum.ADDNEW:
			previousLayer = data.Layer;
			data.AddLayer();
			layer = data.Layer;
			layerIndex = data.LayersList.IndexOf(layer);
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.DELETE;
			break;
		case LayerCommandTypeEnum.ADD:
			previousLayer = data.Layer;
			data.AddLayer(layer);
			layerIndex = data.LayersList.IndexOf(layer);
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.DELETE;
			break;
		case LayerCommandTypeEnum.DUPLICATE:
			previousLayer = data.Layer;
			data.DuplicateLayer(layer);
			layer = data.Layer;
			layerIndex = data.LayersList.IndexOf(layer);
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.DELETE;
			break;
		case LayerCommandTypeEnum.DELETE:
			previousLayer = data.Layer;
			layerIndex = data.LayersList.IndexOf(layer);
			data.DeleteLayer(layer);
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.INSERT;
			break;
		case LayerCommandTypeEnum.DELETELIST:
		{
			previousLayer = data.Layer;
			LayersIndexList.Clear();
			for (int j = 0; j < LayersList.Count; j++)
			{
				LayersIndexList.Add(data.LayersList.IndexOf(LayersList[j]));
			}
			data.DeleteLayers(LayersList);
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.INSERTLIST;
			break;
		}
		case LayerCommandTypeEnum.UP:
			previousLayer = data.Layer;
			layerIndex = data.LayersList.IndexOf(layer);
			data.MoveLayerUp(layer);
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.DOWN;
			break;
		case LayerCommandTypeEnum.DOWN:
			previousLayer = data.Layer;
			layerIndex = data.LayersList.IndexOf(layer);
			data.MoveLayerDown(layer);
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.UP;
			break;
		case LayerCommandTypeEnum.VISIBILITY:
			layer.IsVisible = NewState;
			NewState = !NewState;
			data.CombineLayersFromAllChannels();
			layerPanel.UpdateEntry(layer);
			break;
		case LayerCommandTypeEnum.LOCKED:
			layer.IsLocked = NewState;
			NewState = !NewState;
			layerPanel.UpdateEntry(layer);
			break;
		case LayerCommandTypeEnum.NAME:
		{
			string oldName = layer.Name;
			layer.Name = NewText;
			NewText = oldName;
			layerPanel.UpdateEntry(layer);
			layerControl.Reset();
			break;
		}
		case LayerCommandTypeEnum.COLOR:
		{
			Color oldColor = layer.Color;
			layer.Color = NewColor;
			NewColor = oldColor;
			layerPanel.UpdateEntry(layer);
			layerControl.Reset();
			break;
		}
		case LayerCommandTypeEnum.CHANNELVISIBILITY:
			switch (Channel)
			{
			case Layer.ChannelEnum.COLOR:
				layer.ColorChannel.IsVisible = NewState;
				break;
			case Layer.ChannelEnum.ROUGHNESS:
				layer.RoughnessChannel.IsVisible = NewState;
				break;
			case Layer.ChannelEnum.METALLICITY:
				layer.MetallicityChannel.IsVisible = NewState;
				break;
			case Layer.ChannelEnum.HEIGHT:
				layer.HeightChannel.IsVisible = NewState;
				break;
			case Layer.ChannelEnum.EMISSION:
				layer.EmissionChannel.IsVisible = NewState;
				break;
			}
			NewState = !NewState;
			data.CombineLayers(Channel);
			layerControl.Reset();
			break;
		case LayerCommandTypeEnum.CHANNELBLENDINGMODE:
		{
			Blender.BlendingModeEnum previousBlendingMode = Blender.BlendingModeEnum.NORMAL;
			switch (Channel)
			{
			case Layer.ChannelEnum.COLOR:
				previousBlendingMode = layer.ColorChannel.BlendingMode;
				layer.ColorChannel.BlendingMode = NewBlendingMode;
				break;
			case Layer.ChannelEnum.ROUGHNESS:
				previousBlendingMode = layer.RoughnessChannel.BlendingMode;
				layer.RoughnessChannel.BlendingMode = NewBlendingMode;
				break;
			case Layer.ChannelEnum.METALLICITY:
				previousBlendingMode = layer.MetallicityChannel.BlendingMode;
				layer.MetallicityChannel.BlendingMode = NewBlendingMode;
				break;
			case Layer.ChannelEnum.HEIGHT:
				previousBlendingMode = layer.HeightChannel.BlendingMode;
				layer.HeightChannel.BlendingMode = NewBlendingMode;
				break;
			case Layer.ChannelEnum.EMISSION:
				previousBlendingMode = layer.EmissionChannel.BlendingMode;
				layer.EmissionChannel.BlendingMode = NewBlendingMode;
				break;
			}
			NewBlendingMode = previousBlendingMode;
			data.CombineLayers(Channel);
			layerControl.Reset();
			break;
		}
		case LayerCommandTypeEnum.COMBINEALL:
			previousLayer = data.Layer;
			data.MergeLayersToSingleLayer();
			layer = data.Layer;
			layerIndex = data.LayersList.IndexOf(layer);
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.DELETE;
			break;
		case LayerCommandTypeEnum.COMBINESINGLE:
			previousLayer = data.Layer;
			data.MergeSingleLayerDownwards(layer);
			layer = data.Layer;
			layerIndex = data.LayersList.IndexOf(layer);
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.DELETE;
			break;
		case LayerCommandTypeEnum.INSERT:
			data.InsertLayer(layer, layerIndex);
			data.Layer = previousLayer;
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.DELETE;
			break;
		case LayerCommandTypeEnum.INSERTLIST:
		{
			for (int i = 0; i < LayersList.Count; i++)
			{
				data.InsertLayer(LayersList[i], LayersIndexList[i]);
			}
			data.Layer = previousLayer;
			layerPanel.Reset();
			LayerCommandType = LayerCommandTypeEnum.DELETELIST;
			break;
		}
		}
		data.Mutex.ReleaseMutex();
	}

	public void Undo()
	{
		Execute();
	}

	public void Redo()
	{
		Execute();
	}
}
