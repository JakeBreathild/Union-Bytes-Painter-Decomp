using Godot;

public class LayerEntry : Panel
{
	private Workspace workspace;

	private LayerPanel layerPanel;

	private Layer layer;

	private Layer lastSelectedLayer;

	private TextureButton visibleButton;

	private ColorRect colorRect;

	private Label nameLabel;

	private TextureButton lockButton;

	private TextureButton deleteButton;

	public Layer Layer => layer;

	public new bool IsVisible
	{
		get
		{
			return visibleButton.Pressed;
		}
		set
		{
			visibleButton.Pressed = value;
		}
	}

	public Color Color
	{
		get
		{
			return colorRect.Color;
		}
		set
		{
			colorRect.Color = value;
		}
	}

	public new string Name
	{
		get
		{
			return nameLabel.Text;
		}
		set
		{
			nameLabel.Text = value;
		}
	}

	public bool IsLocked
	{
		get
		{
			return lockButton.Pressed;
		}
		set
		{
			lockButton.Pressed = value;
		}
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		visibleButton = GetChildOrNull<TextureButton>(0);
		visibleButton.Connect(Signals.Toggled, this, "VisibilityToggled");
		colorRect = GetChildOrNull<ColorRect>(1);
		nameLabel = GetChildOrNull<Label>(2);
		lockButton = GetChildOrNull<TextureButton>(3);
		lockButton.Connect(Signals.Toggled, this, "LockedToggled");
		deleteButton = GetChildOrNull<TextureButton>(4);
		deleteButton.Connect(Signals.Pressed, this, "DeletePressed");
		Connect(Signals.GuiInput, this, "GuiInput");
	}

	public void SetLayerPanel(LayerPanel layerPanel)
	{
		this.layerPanel = layerPanel;
	}

	public void SetLayer(Layer layer)
	{
		visibleButton.SetBlockSignals(enable: true);
		lockButton.SetBlockSignals(enable: true);
		this.layer = layer;
		visibleButton.Pressed = this.layer.IsVisible;
		colorRect.Color = this.layer.Color;
		nameLabel.Text = this.layer.Name;
		lockButton.Pressed = this.layer.IsLocked;
		visibleButton.SetBlockSignals(enable: false);
		lockButton.SetBlockSignals(enable: false);
	}

	public void UpdateLayer()
	{
		visibleButton.SetBlockSignals(enable: true);
		lockButton.SetBlockSignals(enable: true);
		visibleButton.Pressed = layer.IsVisible;
		colorRect.Color = layer.Color;
		nameLabel.Text = layer.Name;
		lockButton.Pressed = layer.IsLocked;
		visibleButton.SetBlockSignals(enable: false);
		lockButton.SetBlockSignals(enable: false);
	}

	public void UpdateLayer(Layer layer)
	{
		visibleButton.SetBlockSignals(enable: true);
		lockButton.SetBlockSignals(enable: true);
		this.layer = layer;
		visibleButton.Pressed = layer.IsVisible;
		colorRect.Color = layer.Color;
		nameLabel.Text = layer.Name;
		lockButton.Pressed = layer.IsLocked;
		visibleButton.SetBlockSignals(enable: false);
		lockButton.SetBlockSignals(enable: false);
	}

	public void DisableVisibleCheckBox()
	{
		visibleButton.Disabled = true;
		visibleButton.Visible = false;
	}

	public void DisableDeleteButton()
	{
		deleteButton.Disabled = true;
	}

	public void GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton { ButtonIndex: 1, Pressed: not false })
		{
			lastSelectedLayer = layerPanel.LayerEntry.Layer;
			layerPanel.Select(layer);
		}
	}

	public void VisibilityToggled(bool pressed)
	{
		LayerCommand obj = (LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
		obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.VISIBILITY;
		obj.Layer = layer;
		obj.NewState = pressed;
		workspace.Worksheet.History.StopRecording("Layer Visibility [" + layer.Name + "]");
		if (lastSelectedLayer != null && layer != lastSelectedLayer)
		{
			layerPanel.Select(lastSelectedLayer);
		}
	}

	public void LockedToggled(bool pressed)
	{
		LayerCommand obj = (LayerCommand)workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
		obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.LOCKED;
		obj.Layer = layer;
		obj.NewState = pressed;
		workspace.Worksheet.History.StopRecording("Layer Locked [" + layer.Name + "]");
		if (lastSelectedLayer != null && layer != lastSelectedLayer)
		{
			layerPanel.Select(lastSelectedLayer);
		}
	}

	public void DeletePressed()
	{
		if (lastSelectedLayer != null && layer != lastSelectedLayer)
		{
			layerPanel.Select(lastSelectedLayer);
		}
		LayerCommand obj = (LayerCommand)Register.Workspace.Worksheet.History.StartRecording(History.CommandTypeEnum.LAYER);
		obj.LayerCommandType = LayerCommand.LayerCommandTypeEnum.DELETE;
		obj.Layer = layer;
		Register.Workspace.Worksheet.History.StopRecording("Layer Deleted [" + layer.Name + "]");
		layerPanel.Select(layer.Data.Layer);
		QueueFree();
	}
}
