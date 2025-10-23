using System.Collections.Generic;
using Godot;

public class LayerPanel : Panel
{
	private struct SeparatedLayer
	{
		public Layer Layer;

		public bool WasVisible;

		public SeparatedLayer(Layer layer, bool isVisible)
		{
			Layer = layer;
			WasVisible = isVisible;
		}
	}

	[Export(PropertyHint.None, "")]
	private PackedScene layerEntryPackedScene;

	[Export(PropertyHint.None, "")]
	private StyleBox entryStyleBox;

	[Export(PropertyHint.None, "")]
	private StyleBox selectedStyleBox;

	private Workspace workspace;

	private VBoxContainer vBoxContainer;

	[Export(PropertyHint.None, "")]
	private NodePath layerControlNodePath;

	private LayerControl layerControl;

	private TextureButton addButton;

	private TextureButton duplicateButton;

	private TextureButton upButton;

	private TextureButton downButton;

	private TextureButton separateButton;

	private TextureButton combineSingleLayerButton;

	private TextureButton combineLayersButton;

	private int layerIndex;

	private LayerEntry layerEntry;

	private bool isSeparatedLayerEnabled;

	private List<SeparatedLayer> separatedLayersList = new List<SeparatedLayer>();

	public int LayerIndex => layerIndex;

	public LayerEntry LayerEntry => layerEntry;

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		vBoxContainer = GetNodeOrNull<VBoxContainer>("ScrollContainer/VBoxContainer");
		layerControl = GetNodeOrNull<LayerControl>(layerControlNodePath);
		addButton = GetNodeOrNull<TextureButton>("AddTextureButton");
		addButton.Connect(Signals.Pressed, this, "AddPressed");
		duplicateButton = GetNodeOrNull<TextureButton>("DuplicateTextureButton");
		duplicateButton.Connect(Signals.Pressed, this, "DuplicatePressed");
		upButton = GetNodeOrNull<TextureButton>("UpTextureButton");
		upButton.Connect(Signals.Pressed, this, "UpPressed");
		downButton = GetNodeOrNull<TextureButton>("DownTextureButton");
		downButton.Connect(Signals.Pressed, this, "DownPressed");
		separateButton = GetNodeOrNull<TextureButton>("SeparateTextureButton");
		separateButton.Connect(Signals.Pressed, this, "SeparatePressed");
		combineSingleLayerButton = GetNodeOrNull<TextureButton>("CombineSingleLayerTextureButton");
		combineSingleLayerButton.Connect(Signals.Pressed, this, "CombineSingleLayerPressed");
		combineLayersButton = GetNodeOrNull<TextureButton>("CombineLayersTextureButton");
		combineLayersButton.Connect(Signals.Pressed, this, "CombineLayersPressed");
	}

	public void DisableSeparatedLayers()
	{
		if (isSeparatedLayerEnabled)
		{
			Data data = workspace.Worksheet.Data;
			for (int i = 0; i < separatedLayersList.Count; i++)
			{
				separatedLayersList[i].Layer.IsVisible = separatedLayersList[i].WasVisible;
			}
			separatedLayersList.Clear();
			isSeparatedLayerEnabled = false;
			data.CombineLayersFromAllChannels();
		}
	}

	public void Clear()
	{
		for (int i = 0; i < vBoxContainer.GetChildCount(); i++)
		{
			vBoxContainer.GetChildOrNull<LayerEntry>(i).QueueFree();
		}
	}

	public LayerEntry Add(Layer layer)
	{
		LayerEntry layerEntry = layerEntryPackedScene.InstanceOrNull<LayerEntry>();
		vBoxContainer.AddChild(layerEntry);
		layerEntry.Owner = vBoxContainer;
		layerEntry.SetLayerPanel(this);
		layerEntry.SetLayer(layer);
		return layerEntry;
	}

	public void UpdateCurrentEntry()
	{
		layerEntry?.UpdateLayer();
	}

	public void UpdateEntry(Layer layer)
	{
		for (int i = 0; i < vBoxContainer.GetChildCount(); i++)
		{
			LayerEntry layerEntry = vBoxContainer.GetChildOrNull<LayerEntry>(i);
			if (layerEntry.Layer == layer)
			{
				layerEntry.UpdateLayer();
				break;
			}
		}
	}

	public void Select(Layer layer)
	{
		workspace.Worksheet.Data.SelectLayer(layer);
		for (int i = 0; i < vBoxContainer.GetChildCount(); i++)
		{
			LayerEntry layerEntry = vBoxContainer.GetChildOrNull<LayerEntry>(i);
			if (layerEntry.Layer == layer)
			{
				this.layerEntry = layerEntry;
				this.layerEntry.Set("custom_styles/panel", selectedStyleBox);
				layerIndex = i;
			}
			else
			{
				layerEntry.Set("custom_styles/panel", entryStyleBox);
			}
		}
		layerControl.Reset();
	}

	public void Reset(bool disableSeparatedLayers = true)
	{
		Data data = workspace.Worksheet.Data;
		if (disableSeparatedLayers)
		{
			DisableSeparatedLayers();
		}
		Clear();
		for (int i = 0; i < data.LayersList.Count; i++)
		{
			LayerEntry layerEntry = Add(data.LayersList[i]);
			vBoxContainer.MoveChild(layerEntry, 0);
			if (i == 0)
			{
				layerEntry.DisableDeleteButton();
			}
			if (data.LayersList[i] == data.Layer)
			{
				this.layerEntry = layerEntry;
				this.layerEntry.Set("custom_styles/panel", selectedStyleBox);
				layerIndex = i;
			}
		}
		layerControl.Reset();
	}

	public void AddPressed()
	{
		((LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER)).LayerCommandType = LayerCommand.LayerCommandTypeEnum.ADDNEW;
		workspace.Worksheet.History.StopRecording("Layer Added [Layer " + workspace.Worksheet.Data.LayersList.Count + "]");
	}

	public void DuplicatePressed()
	{
		LayerCommand obj = (LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
		obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.DUPLICATE;
		obj.Layer = layerEntry.Layer;
		workspace.Worksheet.History.StopRecording("Layer Duplicated [" + layerEntry.Layer.Name + "]");
	}

	public void DownPressed()
	{
		if (layerEntry.Layer != workspace.Worksheet.Data.LayersList[0] && layerEntry.Layer != workspace.Worksheet.Data.LayersList[1])
		{
			LayerCommand obj = (LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
			obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.DOWN;
			obj.Layer = layerEntry.Layer;
			workspace.Worksheet.History.StopRecording("Layer Moved Down [" + layerEntry.Layer.Name + "]");
		}
	}

	public void UpPressed()
	{
		if (layerEntry.Layer != workspace.Worksheet.Data.LayersList[0] && layerEntry.Layer != workspace.Worksheet.Data.LayersList[workspace.Worksheet.Data.LayersList.Count - 1])
		{
			LayerCommand obj = (LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
			obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.UP;
			obj.Layer = layerEntry.Layer;
			workspace.Worksheet.History.StopRecording("Layer Moved Up [" + layerEntry.Layer.Name + "]");
		}
	}

	public void SeparatePressed()
	{
		Data data = workspace.Worksheet.Data;
		if (!isSeparatedLayerEnabled)
		{
			separatedLayersList.Clear();
			for (int i = 0; i < vBoxContainer.GetChildCount(); i++)
			{
				LayerEntry layerEntry = vBoxContainer.GetChildOrNull<LayerEntry>(i);
				separatedLayersList.Add(new SeparatedLayer(layerEntry.Layer, layerEntry.Layer.IsVisible));
				if (layerEntry.Layer != data.Layer)
				{
					layerEntry.Layer.IsVisible = false;
				}
				else
				{
					layerEntry.Layer.IsVisible = true;
				}
			}
			isSeparatedLayerEnabled = true;
		}
		else
		{
			for (int j = 0; j < separatedLayersList.Count; j++)
			{
				separatedLayersList[j].Layer.IsVisible = separatedLayersList[j].WasVisible;
			}
			separatedLayersList.Clear();
			isSeparatedLayerEnabled = false;
		}
		data.CombineLayersFromAllChannels();
		Reset(disableSeparatedLayers: false);
	}

	public void CombineSingleLayerPressed()
	{
		Layer layer = workspace.Worksheet.Data.Layer;
		int index = workspace.Worksheet.Data.LayersList.IndexOf(layer);
		if (index > 0)
		{
			List<Layer> layersList = new List<Layer> { layer };
			if (index > 1)
			{
				layersList.Add(workspace.Worksheet.Data.LayersList[index - 1]);
			}
			LayerCommand obj = (LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
			obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.COMBINESINGLE;
			obj.Layer = layer;
			workspace.Worksheet.History.StopRecording("Combined Layer downwards [" + layer.Name + "]");
			LayerCommand obj2 = (LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
			obj2.LayerCommandType = LayerCommand.LayerCommandTypeEnum.DELETELIST;
			obj2.LayersList = layersList;
			workspace.Worksheet.History.StopRecording("Deleted Combined Layers");
		}
	}

	public void CombineLayersPressed()
	{
		List<Layer> layersList = workspace.Worksheet.Data.GetVisibleLayers();
		((LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER)).LayerCommandType = LayerCommand.LayerCommandTypeEnum.COMBINEALL;
		workspace.Worksheet.History.StopRecording("Combined Visible Layers [" + layerEntry.Layer.Name + "]");
		LayerCommand obj = (LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
		obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.DELETELIST;
		obj.LayersList = layersList;
		workspace.Worksheet.History.StopRecording("Deleted Visible Layers");
	}
}
