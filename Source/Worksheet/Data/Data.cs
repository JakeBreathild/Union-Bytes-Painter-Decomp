using System.Collections.Generic;
using System.IO;
using System.Threading;
using Godot;

public class Data
{
	public class Selection
	{
		public string Name = "";

		public Color Color = ColorExtension.Black;

		public bool[,] Array;
	}

	public enum ChannelEnum
	{
		FULL,
		COLOR,
		ROUGHNESS,
		METALLICITY,
		HEIGHT,
		NORMAL,
		EMISSION
	}

	public static string[] ChannelName = new string[7] { "Full", "Color", "Roughness", "Metallicity", "Height", "Normal", "Emission" };

	private Worksheet worksheet;

	private bool tileable;

	private int width;

	private int height;

	private Layer layer;

	private List<Layer> layersList;

	public bool[,] SelectionArray;

	public float[,] IntensityArray;

	public List<Selection> SelectionArraysList;

	private Channel<Color> colorChannel;

	private Channel<Value> roughnessChannel;

	private Channel<Value> metallicityChannel;

	private Channel<Value> heightChannel;

	private Channel<Color> normalChannel;

	private Channel<Color> emissionChannel;

	private bool doUpdate;

	private System.Threading.Mutex mutex = new System.Threading.Mutex();

	private static System.Threading.Thread thread = null;

	private static Queue<Data> threadDataQueue = new Queue<Data>();

	private static System.Threading.Mutex threadDataQueueMutex = new System.Threading.Mutex();

	public Worksheet Worksheet
	{
		get
		{
			return worksheet;
		}
		set
		{
			worksheet = value;
		}
	}

	public bool Tileable
	{
		get
		{
			return tileable;
		}
		set
		{
			tileable = value;
		}
	}

	public int Width
	{
		get
		{
			return width;
		}
		private set
		{
			width = value;
		}
	}

	public int Height
	{
		get
		{
			return height;
		}
		private set
		{
			height = value;
		}
	}

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

	public List<Layer> LayersList
	{
		get
		{
			return layersList;
		}
		set
		{
			layersList = value;
		}
	}

	public Channel<Color> ColorChannel
	{
		get
		{
			return colorChannel;
		}
		set
		{
			colorChannel = value;
		}
	}

	public Channel<Value> RoughnessChannel
	{
		get
		{
			return roughnessChannel;
		}
		set
		{
			roughnessChannel = value;
		}
	}

	public Channel<Value> MetallicityChannel
	{
		get
		{
			return metallicityChannel;
		}
		set
		{
			metallicityChannel = value;
		}
	}

	public Channel<Value> HeightChannel
	{
		get
		{
			return heightChannel;
		}
		set
		{
			heightChannel = value;
		}
	}

	public Channel<Color> NormalChannel
	{
		get
		{
			return normalChannel;
		}
		set
		{
			normalChannel = value;
		}
	}

	public Channel<Color> EmissionChannel
	{
		get
		{
			return emissionChannel;
		}
		set
		{
			emissionChannel = value;
		}
	}

	public bool DoUpdate
	{
		get
		{
			return doUpdate;
		}
		set
		{
			doUpdate = value;
		}
	}

	public System.Threading.Mutex Mutex => mutex;

	public static System.Threading.Thread Thread
	{
		get
		{
			return thread;
		}
		set
		{
			thread = value;
		}
	}

	public Data(Worksheet worksheet)
	{
		this.worksheet = worksheet;
	}

	public Data(Worksheet worksheet, int width, int height, bool tileable, bool initChannels = true)
	{
		this.worksheet = worksheet;
		this.width = width;
		this.height = height;
		this.tileable = tileable;
		SelectionArray = new bool[this.width, this.height];
		IntensityArray = new float[this.width, this.height];
		SelectionArraysList = new List<Selection>();
		layersList = new List<Layer>();
		layer = new Layer(this, "Base", this.width, this.height, initChannels: true, background: true);
		layersList.Add(layer);
		if (initChannels)
		{
			colorChannel = new Channel<Color>(this.width, this.height, ColorExtension.Zero, Channel.TypeEnum.COLOR, "Color", "C");
			colorChannel.FullContentArea();
			roughnessChannel = new Channel<Value>(this.width, this.height, Value.Zero, Channel.TypeEnum.ROUGHNESS, "Roughness", "R");
			roughnessChannel.FullContentArea();
			metallicityChannel = new Channel<Value>(this.width, this.height, Value.Zero, Channel.TypeEnum.METALLICITY, "Metallicity", "M");
			metallicityChannel.FullContentArea();
			heightChannel = new Channel<Value>(this.width, this.height, Value.Zero, Channel.TypeEnum.HEIGHT, "Height", "H");
			heightChannel.FullContentArea();
			normalChannel = new Channel<Color>(this.width, this.height, new Color(0.5f, 0.5f, 1f), Channel.TypeEnum.NORMAL, "Normal", "N", Image.Format.Rgb8);
			normalChannel.FullContentArea();
			normalChannel.LinkedChannel = heightChannel;
			normalChannel.LinkedChannelName = normalChannel.LinkedChannel.Name;
			normalChannel.CreateTexture();
			heightChannel.LinkedChannel = normalChannel;
			heightChannel.LinkedChannelName = heightChannel.LinkedChannel.Name;
			emissionChannel = new Channel<Color>(this.width, this.height, ColorExtension.Zero, Channel.TypeEnum.EMISSION, "Emission", "E");
			emissionChannel.FullContentArea();
		}
		CombineLayersFromAllChannels();
	}

	public Selection AddSelection(string Name = "", Color color = default(Color))
	{
		Selection selection = null;
		if (!Register.SelectionManager.IsSelecting && Register.SelectionManager.Enabled)
		{
			selection = new Selection();
			selection.Name = Name;
			selection.Color = color;
			selection.Array = (bool[,])SelectionArray.Clone();
			SelectionArraysList.Add(selection);
		}
		return selection;
	}

	public void SetSelectionArray(int index)
	{
		if (index > -1 && index < SelectionArraysList.Count)
		{
			SelectionArraysList[index].Array = (bool[,])SelectionArray.Clone();
		}
	}

	public void SetSelectionArray(Selection selection)
	{
		if (selection != null && !Register.SelectionManager.IsSelecting && Register.SelectionManager.Enabled)
		{
			selection.Array = (bool[,])SelectionArray.Clone();
		}
	}

	public void DeleteSelection(int index)
	{
		if (index > -1 && index < SelectionArraysList.Count)
		{
			SelectionArraysList.RemoveAt(index);
		}
	}

	public void DeleteSelection(string Name)
	{
		int index = -1;
		for (int i = 0; i < SelectionArraysList.Count; i++)
		{
			if (SelectionArraysList[i].Name == Name)
			{
				index = i;
				break;
			}
		}
		DeleteSelection(index);
	}

	public void DeleteSelection(Color color)
	{
		int index = -1;
		for (int i = 0; i < SelectionArraysList.Count; i++)
		{
			if (SelectionArraysList[i].Color == color)
			{
				index = i;
				break;
			}
		}
		DeleteSelection(index);
	}

	public void DeleteSelection(Selection selection)
	{
		SelectionArraysList.Remove(selection);
	}

	public void MoveSelectionDown(Selection selection)
	{
		int index = SelectionArraysList.IndexOf(selection);
		if (index > 0)
		{
			SelectionArraysList.Remove(selection);
			SelectionArraysList.Insert(index - 1, selection);
		}
	}

	public void MoveSelectionUp(Selection selection)
	{
		int index = SelectionArraysList.IndexOf(selection);
		if (index < SelectionArraysList.Count - 1)
		{
			SelectionArraysList.Remove(selection);
			SelectionArraysList.Insert(index + 1, selection);
		}
	}

	public void ActivateSelection(int index)
	{
		if (index > -1 && index < SelectionArraysList.Count)
		{
			SelectionArray = (bool[,])SelectionArraysList[index].Array.Clone();
			Register.SelectionManager.UpdateArea(Vector2i.Zero, new Vector2i(width - 1, height - 1));
			Register.SelectionManager.Enabled = true;
		}
	}

	public void ActivateSelection(string Name)
	{
		int index = -1;
		for (int i = 0; i < SelectionArraysList.Count; i++)
		{
			if (SelectionArraysList[i].Name == Name)
			{
				index = i;
				break;
			}
		}
		ActivateSelection(index);
	}

	public void ActivateSelection(Color color)
	{
		int index = -1;
		for (int i = 0; i < SelectionArraysList.Count; i++)
		{
			if (SelectionArraysList[i].Color == color)
			{
				index = i;
				break;
			}
		}
		ActivateSelection(index);
	}

	public void ActivateSelection(Selection selection)
	{
		SelectionArray = (bool[,])selection.Array.Clone();
		Register.SelectionManager.UpdateArea(Vector2i.Zero, new Vector2i(width - 1, height - 1));
		Register.SelectionManager.Enabled = true;
	}

	public int AddLayer()
	{
		Mutex.WaitOne();
		Layer oldLayer = layer;
		layer = new Layer(this, "Layer " + layersList.Count, width, height);
		if (layersList.Count > 0)
		{
			layersList.Insert(layersList.IndexOf(oldLayer) + 1, layer);
		}
		else
		{
			layersList.Add(layer);
		}
		Mutex.ReleaseMutex();
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
		return layersList.IndexOf(layer);
	}

	public int AddLayer(Layer layer)
	{
		Mutex.WaitOne();
		Layer oldLayer = this.layer;
		this.layer = layer;
		if (layersList.Count > 0)
		{
			layersList.Insert(layersList.IndexOf(oldLayer) + 1, this.layer);
		}
		else
		{
			layersList.Add(this.layer);
		}
		Mutex.ReleaseMutex();
		CombineLayersFromAllChannels();
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
		return layersList.IndexOf(this.layer);
	}

	public int InsertLayer(Layer layer, int index)
	{
		mutex.WaitOne();
		layersList.Insert(index, layer);
		mutex.ReleaseMutex();
		CombineLayersFromAllChannels();
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
		return layersList.IndexOf(layer);
	}

	public int DuplicateLayer(Layer layer)
	{
		mutex.WaitOne();
		Layer oldLayer = this.layer;
		this.layer = layer.Clone();
		this.layer.Name = oldLayer.Name + " [Copy]";
		if (layersList.Count > 0)
		{
			layersList.Insert(layersList.IndexOf(oldLayer) + 1, this.layer);
		}
		else
		{
			layersList.Add(this.layer);
		}
		mutex.ReleaseMutex();
		CombineLayersFromAllChannels();
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
		return layersList.IndexOf(this.layer);
	}

	public void SelectLayer(int index)
	{
		if (index >= 0 && index < layersList.Count)
		{
			layer = layersList[index];
			if (Register.GridManager.DoShowLayerContentAreas)
			{
				Register.GridManager.Update(worksheet);
			}
		}
	}

	public void SelectLayer(Layer layer)
	{
		this.layer = layer;
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
	}

	public void DeleteLayer(int index)
	{
		if (index <= 0 || index >= layersList.Count)
		{
			return;
		}
		mutex.WaitOne();
		Layer layer = layersList[index];
		if (this.layer == layer)
		{
			if (layersList.Count > index + 1)
			{
				this.layer = layersList[index + 1];
			}
			else
			{
				this.layer = layersList[index - 1];
			}
		}
		layersList.Remove(layer);
		mutex.ReleaseMutex();
		CombineLayersFromAllChannels();
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
	}

	public void DeleteLayer(Layer layer)
	{
		int index = layersList.IndexOf(layer);
		if (index <= 0)
		{
			return;
		}
		mutex.WaitOne();
		if (this.layer == layer)
		{
			if (layersList.Count > index + 1)
			{
				this.layer = layersList[index + 1];
			}
			else
			{
				this.layer = layersList[index - 1];
			}
		}
		layersList.Remove(layer);
		mutex.ReleaseMutex();
		CombineLayersFromAllChannels();
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
	}

	public void DeleteLayers(List<Layer> layersList)
	{
		mutex.WaitOne();
		foreach (Layer layer in layersList)
		{
			int index = this.layersList.IndexOf(layer);
			if (index <= 0)
			{
				continue;
			}
			if (this.layer == layer)
			{
				if (this.layersList.Count > index + 1)
				{
					this.layer = this.layersList[index + 1];
				}
				else
				{
					this.layer = this.layersList[index - 1];
				}
			}
			this.layersList.Remove(layer);
		}
		mutex.ReleaseMutex();
		CombineLayersFromAllChannels();
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
	}

	public void MoveLayerDown(Layer layer)
	{
		int index = layersList.IndexOf(layer);
		if (index > 1)
		{
			mutex.WaitOne();
			layersList.Remove(layer);
			layersList.Insert(index - 1, layer);
			mutex.ReleaseMutex();
			CombineLayersFromAllChannels();
		}
	}

	public void MoveLayerUp(Layer layer)
	{
		int index = layersList.IndexOf(layer);
		if (index != 0 && index < layersList.Count - 1)
		{
			mutex.WaitOne();
			layersList.Remove(layer);
			layersList.Insert(index + 1, layer);
			mutex.ReleaseMutex();
			CombineLayersFromAllChannels();
		}
	}

	public int MergeSingleLayerDownwards(Layer layer)
	{
		int index = layersList.IndexOf(layer);
		if (layer.IsVisible && index > 0)
		{
			mutex.WaitOne();
			Layer targetLayer = layersList[index - 1];
			Layer newLayer = new Layer(this, "Combined Layer", width, height, initChannels: false);
			newLayer.ColorChannel = targetLayer.ColorChannel.Clone(doCloneImage: false);
			newLayer.RoughnessChannel = targetLayer.RoughnessChannel.Clone(doCloneImage: false);
			newLayer.MetallicityChannel = targetLayer.MetallicityChannel.Clone(doCloneImage: false);
			newLayer.HeightChannel = targetLayer.HeightChannel.Clone(doCloneImage: false);
			newLayer.EmissionChannel = targetLayer.EmissionChannel.Clone(doCloneImage: false);
			if (layersList.Count > 0)
			{
				layersList.Insert(index + 1, newLayer);
			}
			else
			{
				layersList.Add(newLayer);
			}
			if (layer.ColorChannel.IsVisible && layer.ColorChannel.HasContent)
			{
				float blending = layer.BlendingStrength * layer.ColorChannel.BlendingStrength;
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < width; x++)
					{
						layer.ColorChannel.ColorBlending(ref newLayer.ColorChannel.Array[x, y], ref layer.ColorChannel.Array[x, y], blending);
					}
				}
				newLayer.ColorChannel.DetectContentArea();
			}
			if (layer.RoughnessChannel.IsVisible && layer.RoughnessChannel.HasContent)
			{
				float blending = layer.BlendingStrength * layer.RoughnessChannel.BlendingStrength;
				for (int i = 0; i < height; i++)
				{
					for (int j = 0; j < width; j++)
					{
						layer.RoughnessChannel.ValueBlending(ref newLayer.RoughnessChannel.Array[j, i], ref layer.RoughnessChannel.Array[j, i], blending);
					}
				}
				newLayer.RoughnessChannel.DetectContentArea();
			}
			if (layer.MetallicityChannel.IsVisible && layer.MetallicityChannel.HasContent)
			{
				float blending = layer.BlendingStrength * layer.MetallicityChannel.BlendingStrength;
				for (int k = 0; k < height; k++)
				{
					for (int l = 0; l < width; l++)
					{
						layer.MetallicityChannel.ValueBlending(ref newLayer.MetallicityChannel.Array[l, k], ref layer.MetallicityChannel.Array[l, k], blending);
					}
				}
				newLayer.MetallicityChannel.DetectContentArea();
			}
			if (layer.HeightChannel.IsVisible && layer.HeightChannel.HasContent)
			{
				float blending = layer.BlendingStrength * layer.HeightChannel.BlendingStrength;
				for (int m = 0; m < height; m++)
				{
					for (int n = 0; n < width; n++)
					{
						layer.HeightChannel.ValueBlending(ref newLayer.HeightChannel.Array[n, m], ref layer.HeightChannel.Array[n, m], blending);
					}
				}
				newLayer.HeightChannel.DetectContentArea();
			}
			if (layer.EmissionChannel.IsVisible && layer.EmissionChannel.HasContent)
			{
				float blending = layer.BlendingStrength * layer.EmissionChannel.BlendingStrength;
				for (int num = 0; num < height; num++)
				{
					for (int num2 = 0; num2 < width; num2++)
					{
						layer.EmissionChannel.ColorBlending(ref newLayer.EmissionChannel.Array[num2, num], ref layer.EmissionChannel.Array[num2, num], blending);
					}
				}
				newLayer.EmissionChannel.DetectContentArea();
			}
			this.layer = newLayer;
			mutex.ReleaseMutex();
			CombineLayersFromAllChannels();
			if (Register.GridManager.DoShowLayerContentAreas)
			{
				Register.GridManager.Update(worksheet);
			}
			return layersList.IndexOf(layer);
		}
		return -1;
	}

	public int MergeLayersToSingleLayer()
	{
		mutex.WaitOne();
		layer = new Layer(this, "Combined Layer", width, height, initChannels: false);
		layer.ColorChannel = colorChannel.Clone(doCloneImage: false);
		layer.RoughnessChannel = roughnessChannel.Clone(doCloneImage: false);
		layer.MetallicityChannel = metallicityChannel.Clone(doCloneImage: false);
		layer.HeightChannel = heightChannel.Clone(doCloneImage: false);
		layer.EmissionChannel = emissionChannel.Clone(doCloneImage: false);
		layersList.Add(layer);
		mutex.ReleaseMutex();
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
		return layersList.IndexOf(layer);
	}

	public List<Layer> GetVisibleLayers()
	{
		List<Layer> visibleLayersList = new List<Layer>();
		foreach (Layer listLayer in layersList)
		{
			if (listLayer != layersList[0] && listLayer.IsVisible)
			{
				visibleLayersList.Add(listLayer);
			}
		}
		return visibleLayersList;
	}

	public void CombineLayers(Layer.ChannelEnum channel, int x, int y)
	{
		switch (channel)
		{
		case Layer.ChannelEnum.COLOR:
			colorChannel[x, y] = colorChannel.DefaultValue;
			foreach (Layer layer2 in layersList)
			{
				if (layer2.IsVisible && layer2.ColorChannel.IsVisible)
				{
					layer2.ColorChannel.ColorBlending(ref colorChannel.Array[x, y], ref layer2.ColorChannel.Array[x, y], layer2.BlendingStrength * layer2.ColorChannel.BlendingStrength);
				}
			}
			colorChannel.UpdateAt(x, y);
			break;
		case Layer.ChannelEnum.ROUGHNESS:
			roughnessChannel[x, y] = roughnessChannel.DefaultValue;
			foreach (Layer layer4 in layersList)
			{
				if (layer4.IsVisible && layer4.RoughnessChannel.IsVisible)
				{
					layer4.RoughnessChannel.ValueBlending(ref roughnessChannel.Array[x, y], ref layer4.RoughnessChannel.Array[x, y], layer4.BlendingStrength * layer4.RoughnessChannel.BlendingStrength);
				}
			}
			roughnessChannel.UpdateAt(x, y);
			break;
		case Layer.ChannelEnum.METALLICITY:
			metallicityChannel[x, y] = metallicityChannel.DefaultValue;
			foreach (Layer layer5 in layersList)
			{
				if (layer5.IsVisible && layer5.MetallicityChannel.IsVisible)
				{
					layer5.MetallicityChannel.ValueBlending(ref metallicityChannel.Array[x, y], ref layer5.MetallicityChannel.Array[x, y], layer5.BlendingStrength * layer5.MetallicityChannel.BlendingStrength);
				}
			}
			metallicityChannel.UpdateAt(x, y);
			break;
		case Layer.ChannelEnum.HEIGHT:
			heightChannel[x, y] = heightChannel.DefaultValue;
			foreach (Layer layer3 in layersList)
			{
				if (layer3.IsVisible && layer3.HeightChannel.IsVisible)
				{
					layer3.HeightChannel.ValueBlending(ref heightChannel.Array[x, y], ref layer3.HeightChannel.Array[x, y], layer3.BlendingStrength * layer3.HeightChannel.BlendingStrength);
				}
			}
			heightChannel.UpdateAt(x, y);
			normalChannel.UpdateAt(x, y);
			break;
		case Layer.ChannelEnum.EMISSION:
			emissionChannel[x, y] = emissionChannel.DefaultValue;
			foreach (Layer layer in layersList)
			{
				if (layer.IsVisible && layer.EmissionChannel.IsVisible)
				{
					layer.EmissionChannel.ColorBlending(ref emissionChannel.Array[x, y], ref layer.EmissionChannel.Array[x, y], layer.BlendingStrength * layer.EmissionChannel.BlendingStrength);
				}
			}
			emissionChannel.UpdateAt(x, y);
			break;
		}
		doUpdate = true;
	}

	public void CombineLayers(Layer.ChannelEnum channel)
	{
		Mutex.WaitOne();
		switch (channel)
		{
		case Layer.ChannelEnum.COLOR:
		{
			for (int num5 = 0; num5 < height; num5++)
			{
				for (int num6 = 0; num6 < width; num6++)
				{
					colorChannel[num6, num5] = colorChannel.DefaultValue;
				}
			}
			foreach (Layer layer3 in layersList)
			{
				bool fullBlendingMode = layer3.ColorChannel.BlendingMode == Blender.BlendingModeEnum.NORMAL || layer3.ColorChannel.BlendingMode == Blender.BlendingModeEnum.MINIMUM;
				if (!layer3.IsVisible || !layer3.ColorChannel.IsVisible || !(layer3.ColorChannel.HasContent || fullBlendingMode))
				{
					continue;
				}
				float blending = layer3.BlendingStrength * layer3.ColorChannel.BlendingStrength;
				if (!fullBlendingMode)
				{
					for (int num7 = layer3.ColorChannel.ContentAreaStart.y; num7 <= layer3.ColorChannel.ContentAreaEnd.y; num7++)
					{
						for (int num8 = layer3.ColorChannel.ContentAreaStart.x; num8 <= layer3.ColorChannel.ContentAreaEnd.x; num8++)
						{
							layer3.ColorChannel.ColorBlending(ref colorChannel.Array[num8, num7], ref layer3.ColorChannel.Array[num8, num7], blending);
						}
					}
					continue;
				}
				for (int num9 = 0; num9 < height; num9++)
				{
					for (int num10 = 0; num10 < width; num10++)
					{
						layer3.ColorChannel.ColorBlending(ref colorChannel.Array[num10, num9], ref layer3.ColorChannel.Array[num10, num9], blending);
					}
				}
			}
			colorChannel.UpdateFull();
			break;
		}
		case Layer.ChannelEnum.ROUGHNESS:
		{
			for (int num11 = 0; num11 < height; num11++)
			{
				for (int num12 = 0; num12 < width; num12++)
				{
					roughnessChannel[num12, num11] = roughnessChannel.DefaultValue;
				}
			}
			foreach (Layer layer4 in layersList)
			{
				bool fullBlendingMode = layer4.RoughnessChannel.BlendingMode == Blender.BlendingModeEnum.NORMAL || layer4.RoughnessChannel.BlendingMode == Blender.BlendingModeEnum.MINIMUM;
				if (!layer4.IsVisible || !layer4.RoughnessChannel.IsVisible || !(layer4.RoughnessChannel.HasContent || fullBlendingMode))
				{
					continue;
				}
				float blending = layer4.BlendingStrength * layer4.RoughnessChannel.BlendingStrength;
				if (!fullBlendingMode)
				{
					for (int num13 = layer4.RoughnessChannel.ContentAreaStart.y; num13 <= layer4.RoughnessChannel.ContentAreaEnd.y; num13++)
					{
						for (int num14 = layer4.RoughnessChannel.ContentAreaStart.x; num14 <= layer4.RoughnessChannel.ContentAreaEnd.x; num14++)
						{
							layer4.RoughnessChannel.ValueBlending(ref roughnessChannel.Array[num14, num13], ref layer4.RoughnessChannel.Array[num14, num13], blending);
						}
					}
					continue;
				}
				for (int num15 = 0; num15 < height; num15++)
				{
					for (int num16 = 0; num16 < width; num16++)
					{
						layer4.RoughnessChannel.ValueBlending(ref roughnessChannel.Array[num16, num15], ref layer4.RoughnessChannel.Array[num16, num15], blending);
					}
				}
			}
			roughnessChannel.UpdateFull();
			break;
		}
		case Layer.ChannelEnum.METALLICITY:
		{
			for (int m = 0; m < height; m++)
			{
				for (int n = 0; n < width; n++)
				{
					metallicityChannel[n, m] = metallicityChannel.DefaultValue;
				}
			}
			foreach (Layer layer2 in layersList)
			{
				bool fullBlendingMode = layer2.MetallicityChannel.BlendingMode == Blender.BlendingModeEnum.NORMAL || layer2.MetallicityChannel.BlendingMode == Blender.BlendingModeEnum.MINIMUM;
				if (!layer2.IsVisible || !layer2.MetallicityChannel.IsVisible || !(layer2.MetallicityChannel.HasContent || fullBlendingMode))
				{
					continue;
				}
				float blending = layer2.BlendingStrength * layer2.MetallicityChannel.BlendingStrength;
				if (!fullBlendingMode)
				{
					for (int num = layer2.MetallicityChannel.ContentAreaStart.y; num <= layer2.MetallicityChannel.ContentAreaEnd.y; num++)
					{
						for (int num2 = layer2.MetallicityChannel.ContentAreaStart.x; num2 <= layer2.MetallicityChannel.ContentAreaEnd.x; num2++)
						{
							layer2.MetallicityChannel.ValueBlending(ref metallicityChannel.Array[num2, num], ref layer2.MetallicityChannel.Array[num2, num], blending);
						}
					}
					continue;
				}
				for (int num3 = 0; num3 < height; num3++)
				{
					for (int num4 = 0; num4 < width; num4++)
					{
						layer2.MetallicityChannel.ValueBlending(ref metallicityChannel.Array[num4, num3], ref layer2.MetallicityChannel.Array[num4, num3], blending);
					}
				}
			}
			metallicityChannel.UpdateFull();
			break;
		}
		case Layer.ChannelEnum.HEIGHT:
		{
			for (int num17 = 0; num17 < height; num17++)
			{
				for (int num18 = 0; num18 < width; num18++)
				{
					heightChannel[num18, num17] = heightChannel.DefaultValue;
				}
			}
			foreach (Layer layer5 in layersList)
			{
				bool fullBlendingMode = layer5.HeightChannel.BlendingMode == Blender.BlendingModeEnum.NORMAL || layer5.HeightChannel.BlendingMode == Blender.BlendingModeEnum.MINIMUM;
				if (!layer5.IsVisible || !layer5.HeightChannel.IsVisible || !(layer5.HeightChannel.HasContent || fullBlendingMode))
				{
					continue;
				}
				float blending = layer5.BlendingStrength * layer5.HeightChannel.BlendingStrength;
				if (!fullBlendingMode)
				{
					for (int num19 = layer5.HeightChannel.ContentAreaStart.y; num19 <= layer5.HeightChannel.ContentAreaEnd.y; num19++)
					{
						for (int num20 = layer5.HeightChannel.ContentAreaStart.x; num20 <= layer5.HeightChannel.ContentAreaEnd.x; num20++)
						{
							layer5.HeightChannel.ValueBlending(ref heightChannel.Array[num20, num19], ref layer5.HeightChannel.Array[num20, num19], blending);
						}
					}
					continue;
				}
				for (int num21 = 0; num21 < height; num21++)
				{
					for (int num22 = 0; num22 < width; num22++)
					{
						layer5.HeightChannel.ValueBlending(ref heightChannel.Array[num22, num21], ref layer5.HeightChannel.Array[num22, num21], blending);
					}
				}
			}
			heightChannel.UpdateFull();
			normalChannel.UpdateFull();
			break;
		}
		case Layer.ChannelEnum.EMISSION:
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					emissionChannel[x, y] = emissionChannel.DefaultValue;
				}
			}
			foreach (Layer layer in layersList)
			{
				bool fullBlendingMode = layer.EmissionChannel.BlendingMode == Blender.BlendingModeEnum.NORMAL || layer.EmissionChannel.BlendingMode == Blender.BlendingModeEnum.MINIMUM;
				if (!layer.IsVisible || !layer.EmissionChannel.IsVisible || !(layer.EmissionChannel.HasContent || fullBlendingMode))
				{
					continue;
				}
				float blending = layer.BlendingStrength * layer.EmissionChannel.BlendingStrength;
				if (!fullBlendingMode)
				{
					for (int i = layer.EmissionChannel.ContentAreaStart.y; i <= layer.EmissionChannel.ContentAreaEnd.y; i++)
					{
						for (int j = layer.EmissionChannel.ContentAreaStart.x; j <= layer.EmissionChannel.ContentAreaEnd.x; j++)
						{
							layer.EmissionChannel.ColorBlending(ref emissionChannel.Array[j, i], ref layer.EmissionChannel.Array[j, i], blending);
						}
					}
					continue;
				}
				for (int k = 0; k < height; k++)
				{
					for (int l = 0; l < width; l++)
					{
						layer.EmissionChannel.ColorBlending(ref emissionChannel.Array[l, k], ref layer.EmissionChannel.Array[l, k], blending);
					}
				}
			}
			emissionChannel.UpdateFull();
			break;
		}
		}
		doUpdate = true;
		Mutex.ReleaseMutex();
		if (Register.GridManager.DoShowLayerContentAreas)
		{
			Register.GridManager.Update(worksheet);
		}
	}

	private static void CombineLayersFromAllChannelsThread()
	{
		while (true)
		{
			threadDataQueueMutex.WaitOne();
			if (threadDataQueue.Count > 0)
			{
				Data data = threadDataQueue.Dequeue();
				threadDataQueueMutex.ReleaseMutex();
				ulong time = OS.GetSystemTimeMsecs();
				data.Mutex.WaitOne();
				for (int y = 0; y < data.Height; y++)
				{
					for (int x = 0; x < data.Width; x++)
					{
						data.ColorChannel[x, y] = data.ColorChannel.DefaultValue;
						data.RoughnessChannel[x, y] = data.RoughnessChannel.DefaultValue;
						data.MetallicityChannel[x, y] = data.MetallicityChannel.DefaultValue;
						data.HeightChannel[x, y] = data.HeightChannel.DefaultValue;
						data.EmissionChannel[x, y] = data.EmissionChannel.DefaultValue;
					}
				}
				foreach (Layer layer in data.LayersList)
				{
					if (!layer.IsVisible)
					{
						continue;
					}
					bool fullBlendingMode = layer.ColorChannel.BlendingMode == Blender.BlendingModeEnum.NORMAL || layer.ColorChannel.BlendingMode == Blender.BlendingModeEnum.MINIMUM;
					float blending;
					if (layer.ColorChannel.IsVisible && (layer.ColorChannel.HasContent || fullBlendingMode))
					{
						blending = layer.BlendingStrength * layer.ColorChannel.BlendingStrength;
						if (!fullBlendingMode)
						{
							for (int i = layer.ColorChannel.ContentAreaStart.y; i <= layer.ColorChannel.ContentAreaEnd.y; i++)
							{
								for (int j = layer.ColorChannel.ContentAreaStart.x; j <= layer.ColorChannel.ContentAreaEnd.x; j++)
								{
									layer.ColorChannel.ColorBlending(ref data.ColorChannel.Array[j, i], ref layer.ColorChannel.Array[j, i], blending);
								}
							}
						}
						else
						{
							for (int k = 0; k < data.Height; k++)
							{
								for (int l = 0; l < data.Width; l++)
								{
									layer.ColorChannel.ColorBlending(ref data.ColorChannel.Array[l, k], ref layer.ColorChannel.Array[l, k], blending);
								}
							}
						}
					}
					fullBlendingMode = layer.RoughnessChannel.BlendingMode == Blender.BlendingModeEnum.NORMAL || layer.RoughnessChannel.BlendingMode == Blender.BlendingModeEnum.MINIMUM;
					if (layer.RoughnessChannel.IsVisible && (layer.RoughnessChannel.HasContent || fullBlendingMode))
					{
						blending = layer.BlendingStrength * layer.RoughnessChannel.BlendingStrength;
						if (!fullBlendingMode)
						{
							for (int m = layer.RoughnessChannel.ContentAreaStart.y; m <= layer.RoughnessChannel.ContentAreaEnd.y; m++)
							{
								for (int n = layer.RoughnessChannel.ContentAreaStart.x; n <= layer.RoughnessChannel.ContentAreaEnd.x; n++)
								{
									layer.RoughnessChannel.ValueBlending(ref data.RoughnessChannel.Array[n, m], ref layer.RoughnessChannel.Array[n, m], blending);
								}
							}
						}
						else
						{
							for (int num = 0; num < data.Height; num++)
							{
								for (int num2 = 0; num2 < data.Width; num2++)
								{
									layer.RoughnessChannel.ValueBlending(ref data.RoughnessChannel.Array[num2, num], ref layer.RoughnessChannel.Array[num2, num], blending);
								}
							}
						}
					}
					fullBlendingMode = layer.MetallicityChannel.BlendingMode == Blender.BlendingModeEnum.NORMAL || layer.MetallicityChannel.BlendingMode == Blender.BlendingModeEnum.MINIMUM;
					if (layer.MetallicityChannel.IsVisible && (layer.MetallicityChannel.HasContent || fullBlendingMode))
					{
						blending = layer.BlendingStrength * layer.MetallicityChannel.BlendingStrength;
						if (!fullBlendingMode)
						{
							for (int num3 = layer.MetallicityChannel.ContentAreaStart.y; num3 <= layer.MetallicityChannel.ContentAreaEnd.y; num3++)
							{
								for (int num4 = layer.MetallicityChannel.ContentAreaStart.x; num4 <= layer.MetallicityChannel.ContentAreaEnd.x; num4++)
								{
									layer.MetallicityChannel.ValueBlending(ref data.MetallicityChannel.Array[num4, num3], ref layer.MetallicityChannel.Array[num4, num3], blending);
								}
							}
						}
						else
						{
							for (int num5 = 0; num5 < data.Height; num5++)
							{
								for (int num6 = 0; num6 < data.Width; num6++)
								{
									layer.MetallicityChannel.ValueBlending(ref data.MetallicityChannel.Array[num6, num5], ref layer.MetallicityChannel.Array[num6, num5], blending);
								}
							}
						}
					}
					fullBlendingMode = layer.HeightChannel.BlendingMode == Blender.BlendingModeEnum.NORMAL || layer.HeightChannel.BlendingMode == Blender.BlendingModeEnum.MINIMUM;
					if (layer.HeightChannel.IsVisible && (layer.HeightChannel.HasContent || fullBlendingMode))
					{
						blending = layer.BlendingStrength * layer.HeightChannel.BlendingStrength;
						if (!fullBlendingMode)
						{
							for (int num7 = layer.HeightChannel.ContentAreaStart.y; num7 <= layer.HeightChannel.ContentAreaEnd.y; num7++)
							{
								for (int num8 = layer.HeightChannel.ContentAreaStart.x; num8 <= layer.HeightChannel.ContentAreaEnd.x; num8++)
								{
									layer.HeightChannel.ValueBlending(ref data.HeightChannel.Array[num8, num7], ref layer.HeightChannel.Array[num8, num7], blending);
								}
							}
						}
						else
						{
							for (int num9 = 0; num9 < data.Height; num9++)
							{
								for (int num10 = 0; num10 < data.Width; num10++)
								{
									layer.HeightChannel.ValueBlending(ref data.HeightChannel.Array[num10, num9], ref layer.HeightChannel.Array[num10, num9], blending);
								}
							}
						}
					}
					fullBlendingMode = layer.EmissionChannel.BlendingMode == Blender.BlendingModeEnum.NORMAL || layer.EmissionChannel.BlendingMode == Blender.BlendingModeEnum.MINIMUM;
					if (!layer.EmissionChannel.IsVisible || !(layer.EmissionChannel.HasContent || fullBlendingMode))
					{
						continue;
					}
					blending = layer.BlendingStrength * layer.EmissionChannel.BlendingStrength;
					if (!fullBlendingMode)
					{
						for (int num11 = layer.EmissionChannel.ContentAreaStart.y; num11 <= layer.EmissionChannel.ContentAreaEnd.y; num11++)
						{
							for (int num12 = layer.EmissionChannel.ContentAreaStart.x; num12 <= layer.EmissionChannel.ContentAreaEnd.x; num12++)
							{
								layer.EmissionChannel.ColorBlending(ref data.EmissionChannel.Array[num12, num11], ref layer.EmissionChannel.Array[num12, num11], blending);
							}
						}
						continue;
					}
					for (int num13 = 0; num13 < data.Height; num13++)
					{
						for (int num14 = 0; num14 < data.Width; num14++)
						{
							layer.EmissionChannel.ColorBlending(ref data.EmissionChannel.Array[num14, num13], ref layer.EmissionChannel.Array[num14, num13], blending);
						}
					}
				}
				data.ColorChannel.UpdateFull();
				data.RoughnessChannel.UpdateFull();
				data.MetallicityChannel.UpdateFull();
				data.HeightChannel.UpdateFull();
				data.NormalChannel.UpdateFull();
				data.emissionChannel.UpdateFull();
				data.doUpdate = true;
				data.Mutex.ReleaseMutex();
				if (Register.GridManager.DoShowLayerContentAreas)
				{
					Register.GridManager.Update(data.worksheet);
				}
				GD.Print("Combine Layers Time: " + (OS.GetSystemTimeMsecs() - time) + " ms");
			}
			else
			{
				threadDataQueueMutex.ReleaseMutex();
				System.Threading.Thread.Sleep(5);
			}
		}
	}

	public void CombineLayersFromAllChannels()
	{
		threadDataQueueMutex.WaitOne();
		threadDataQueue.Enqueue(this);
		threadDataQueueMutex.ReleaseMutex();
		if (thread == null)
		{
			thread = new System.Threading.Thread(CombineLayersFromAllChannelsThread);
		}
		if (thread.ThreadState == ThreadState.Stopped)
		{
			thread.Abort();
			thread = new System.Threading.Thread(CombineLayersFromAllChannelsThread);
		}
		if (thread.ThreadState == ThreadState.Unstarted)
		{
			thread.Start();
		}
	}

	public bool PickSelection(int x, int y)
	{
		return SelectionArray[x, y];
	}

	public Color PickColor(int x, int y)
	{
		return ColorChannel[x, y];
	}

	public Color PickColor(Layer layer, int x, int y)
	{
		return layer.ColorChannel[x, y];
	}

	public Value PickRoughness(int x, int y)
	{
		return RoughnessChannel[x, y];
	}

	public Value PickRoughness(Layer layer, int x, int y)
	{
		return layer.RoughnessChannel[x, y];
	}

	public Value PickMetallicity(int x, int y)
	{
		return MetallicityChannel[x, y];
	}

	public Value PickMetallicity(Layer layer, int x, int y)
	{
		return layer.MetallicityChannel[x, y];
	}

	public Value PickHeight(int x, int y)
	{
		return HeightChannel[x, y];
	}

	public Value PickHeight(Layer layer, int x, int y)
	{
		return layer.HeightChannel[x, y];
	}

	public Color PickEmission(int x, int y)
	{
		return EmissionChannel[x, y];
	}

	public Color PickEmission(Layer layer, int x, int y)
	{
		return layer.EmissionChannel[x, y];
	}

	public void DrawSelection(int x, int y, bool selected)
	{
		SelectionArray[x, y] = selected;
	}

	public void DrawColor(int x, int y, Color color, Blender.BlendingModeEnum blendingModes, float blendingStrength)
	{
		ColorChannel.SetValue(x, y, Blender.Blend(ColorChannel[x, y], color, blendingModes, blendingStrength));
		doUpdate = true;
	}

	public void DrawColor(Layer layer, int x, int y, Color color, Blender.BlendingModeEnum blendingModes, float blendingStrength)
	{
		layer.ColorChannel.SetValue(x, y, Blender.Blend(layer.ColorChannel[x, y], color, blendingModes, blendingStrength));
		CombineLayers(Layer.ChannelEnum.COLOR, x, y);
	}

	public void DrawRoughness(int x, int y, Value roughness, Blender.BlendingModeEnum blendingMode, float blendingStrength)
	{
		RoughnessChannel.SetValue(x, y, Blender.Blend(RoughnessChannel[x, y], roughness, blendingMode, blendingStrength));
		doUpdate = true;
	}

	public void DrawRoughness(Layer layer, int x, int y, Value roughness, Blender.BlendingModeEnum blendingMode, float blendingStrength)
	{
		layer.RoughnessChannel.SetValue(x, y, Blender.Blend(layer.RoughnessChannel[x, y], roughness, blendingMode, blendingStrength));
		CombineLayers(Layer.ChannelEnum.ROUGHNESS, x, y);
	}

	public void DrawMetallicity(int x, int y, Value metallicity, Blender.BlendingModeEnum blendingMode, float blendingStrength)
	{
		MetallicityChannel.SetValue(x, y, Blender.Blend(MetallicityChannel[x, y], metallicity, blendingMode, blendingStrength));
		doUpdate = true;
	}

	public void DrawMetallicity(Layer layer, int x, int y, Value metallicity, Blender.BlendingModeEnum blendingMode, float blendingStrength)
	{
		layer.MetallicityChannel.SetValue(x, y, Blender.Blend(layer.MetallicityChannel[x, y], metallicity, blendingMode, blendingStrength));
		CombineLayers(Layer.ChannelEnum.METALLICITY, x, y);
	}

	public void DrawHeight(int x, int y, Value height, Blender.BlendingModeEnum blendingMode, float blendingStrength)
	{
		HeightChannel.SetValue(x, y, Blender.Blend(HeightChannel[x, y], height, blendingMode, blendingStrength));
		NormalChannel.UpdateAt(x, y);
		doUpdate = true;
	}

	public void DrawHeight(Layer layer, int x, int y, Value height, Blender.BlendingModeEnum blendingMode, float blendingStrength)
	{
		layer.HeightChannel.SetValue(x, y, Blender.Blend(layer.HeightChannel[x, y], height, blendingMode, blendingStrength));
		CombineLayers(Layer.ChannelEnum.HEIGHT, x, y);
	}

	public void DrawEmission(int x, int y, Color emission, Blender.BlendingModeEnum blendingModes, float blendingStrength)
	{
		EmissionChannel.SetValue(x, y, Blender.Blend(EmissionChannel[x, y], emission, blendingModes, blendingStrength));
		doUpdate = true;
	}

	public void DrawEmission(Layer layer, int x, int y, Color emission, Blender.BlendingModeEnum blendingModes, float blendingStrength)
	{
		layer.EmissionChannel.SetValue(x, y, Blender.Blend(layer.EmissionChannel[x, y], emission, blendingModes, blendingStrength));
		CombineLayers(Layer.ChannelEnum.EMISSION, x, y);
	}

	public bool UpdateTextures()
	{
		if (doUpdate)
		{
			mutex.WaitOne();
			ColorChannel.UpdateTexture();
			HeightChannel.UpdateTexture();
			NormalChannel.UpdateTexture();
			RoughnessChannel.UpdateTexture();
			MetallicityChannel.UpdateTexture();
			EmissionChannel.UpdateTexture();
			doUpdate = false;
			mutex.ReleaseMutex();
			return true;
		}
		return false;
	}

	public void Export(string path, string name)
	{
		ColorChannel.Image.SavePng(path + "//" + name + ColorChannel.FileSuffix + ".png");
		HeightChannel.Image.SavePng(path + "//" + name + HeightChannel.FileSuffix + ".png");
		NormalChannel.Image.SavePng(path + "//" + name + NormalChannel.FileSuffix + ".png");
		RoughnessChannel.Image.SavePng(path + "//" + name + RoughnessChannel.FileSuffix + ".png");
		MetallicityChannel.Image.SavePng(path + "//" + name + MetallicityChannel.FileSuffix + ".png");
		EmissionChannel.Image.SavePng(path + "//" + name + EmissionChannel.FileSuffix + ".png");
	}

	public void WriteToBinaryStream(BinaryWriter binaryWriter)
	{
		binaryWriter.Write(Tileable);
		binaryWriter.Write(Width);
		binaryWriter.Write(Height);
		binaryWriter.Write(new byte[64]);
		colorChannel.WriteToBinaryStream(binaryWriter);
		roughnessChannel.WriteToBinaryStream(binaryWriter);
		metallicityChannel.WriteToBinaryStream(binaryWriter);
		heightChannel.WriteToBinaryStream(binaryWriter);
		emissionChannel.WriteToBinaryStream(binaryWriter);
		normalChannel.WriteToBinaryStream(binaryWriter);
		binaryWriter.Write(layersList.Count);
		for (int i = 0; i < layersList.Count; i++)
		{
			layersList[i].WriteToBinaryStream(binaryWriter);
		}
	}

	public void WriteSelectionsToBinaryStream(BinaryWriter binaryWriter)
	{
		binaryWriter.Write(SelectionArraysList.Count);
		for (int i = 0; i < SelectionArraysList.Count; i++)
		{
			binaryWriter.Write(SelectionArraysList[i].Name);
			binaryWriter.Write((byte)SelectionArraysList[i].Color.r8);
			binaryWriter.Write((byte)SelectionArraysList[i].Color.g8);
			binaryWriter.Write((byte)SelectionArraysList[i].Color.b8);
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					binaryWriter.Write(SelectionArraysList[i].Array[x, y]);
				}
			}
		}
	}

	public static Data ReadFromBinaryStream_009(BinaryReader binaryReader, Worksheet worksheet)
	{
		Data data = new Data(worksheet);
		data.Tileable = binaryReader.ReadBoolean();
		data.Width = binaryReader.ReadInt32();
		data.Height = binaryReader.ReadInt32();
		data.LayersList = new List<Layer>();
		data.Layer = new Layer(data, "Layer " + data.LayersList.Count, data.width, data.height, initChannels: true, background: true);
		data.LayersList.Add(data.Layer);
		data.SelectionArray = new bool[data.Width, data.Height];
		data.IntensityArray = new float[data.Width, data.Height];
		data.SelectionArraysList = new List<Selection>();
		data.ColorChannel = Channel<Color>.ReadFromBinaryStream_009(binaryReader);
		data.HeightChannel = Channel.ConvertFromFloatToValue(Channel<float>.ReadFromBinaryStream_009(binaryReader), Value.Zero);
		data.NormalChannel = Channel<Color>.ReadFromBinaryStream_009(binaryReader);
		data.RoughnessChannel = Channel.ConvertFromFloatToValue(Channel<float>.ReadFromBinaryStream_009(binaryReader), Value.Zero);
		data.MetallicityChannel = Channel.ConvertFromFloatToValue(Channel<float>.ReadFromBinaryStream_009(binaryReader), Value.Zero);
		data.EmissionChannel = Channel<Color>.ReadFromBinaryStream_009(binaryReader);
		data.Layer.ColorChannel = data.ColorChannel.Clone();
		data.Layer.HeightChannel = data.HeightChannel.Clone();
		data.Layer.RoughnessChannel = data.RoughnessChannel.Clone();
		data.Layer.MetallicityChannel = data.MetallicityChannel.Clone();
		data.Layer.EmissionChannel = data.EmissionChannel.Clone();
		data.NormalChannel.LinkedChannel = data.HeightChannel;
		data.NormalChannel.LinkedChannelName = data.NormalChannel.LinkedChannel.Name;
		data.NormalChannel.CreateTexture();
		data.HeightChannel.LinkedChannel = data.NormalChannel;
		data.HeightChannel.LinkedChannelName = data.HeightChannel.LinkedChannel.Name;
		return data;
	}

	public static Data ReadFromBinaryStream(BinaryReader binaryReader, Worksheet worksheet)
	{
		Data data = new Data(worksheet);
		data.Tileable = binaryReader.ReadBoolean();
		data.Width = binaryReader.ReadInt32();
		data.Height = binaryReader.ReadInt32();
		binaryReader.ReadBytes(64);
		data.SelectionArray = new bool[data.Width, data.Height];
		data.IntensityArray = new float[data.Width, data.Height];
		data.SelectionArraysList = new List<Selection>();
		data.ColorChannel = Channel<Color>.ReadFromBinaryStream(binaryReader);
		data.RoughnessChannel = Channel<Value>.ReadFromBinaryStream(binaryReader);
		data.MetallicityChannel = Channel<Value>.ReadFromBinaryStream(binaryReader);
		data.HeightChannel = Channel<Value>.ReadFromBinaryStream(binaryReader);
		data.EmissionChannel = Channel<Color>.ReadFromBinaryStream(binaryReader);
		data.NormalChannel = Channel<Color>.ReadFromBinaryStream(binaryReader);
		data.NormalChannel.LinkedChannel = data.HeightChannel;
		data.NormalChannel.LinkedChannelName = data.NormalChannel.LinkedChannel.Name;
		data.NormalChannel.CreateTexture();
		data.HeightChannel.LinkedChannel = data.NormalChannel;
		data.HeightChannel.LinkedChannelName = data.HeightChannel.LinkedChannel.Name;
		int layerCount = binaryReader.ReadInt32();
		data.LayersList = new List<Layer>();
		for (int i = 0; i < layerCount; i++)
		{
			data.Layer = Layer.ReadFromBinaryStream(binaryReader, data);
			data.LayersList.Add(data.Layer);
		}
		data.Layer = data.LayersList[0];
		data.CombineLayersFromAllChannels();
		return data;
	}

	public static void ReadSelectionsFromBinaryStream(BinaryReader binaryReader, Data data)
	{
		data.SelectionArraysList.Clear();
		int selectionsCount = binaryReader.ReadInt32();
		for (int i = 0; i < selectionsCount; i++)
		{
			Selection selection = new Selection();
			selection.Name = binaryReader.ReadString();
			selection.Color = default(Color);
			selection.Color.r8 = binaryReader.ReadByte();
			selection.Color.g8 = binaryReader.ReadByte();
			selection.Color.b8 = binaryReader.ReadByte();
			selection.Color.a = 1f;
			selection.Array = new bool[data.width, data.height];
			for (int y = 0; y < data.height; y++)
			{
				for (int x = 0; x < data.width; x++)
				{
					selection.Array[x, y] = binaryReader.ReadBoolean();
				}
			}
			data.SelectionArraysList.Add(selection);
		}
	}
}
