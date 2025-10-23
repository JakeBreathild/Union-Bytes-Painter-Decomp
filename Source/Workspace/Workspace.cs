using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Godot;

public class Workspace : Node
{
	private Viewport workspaceViewport;

	private AlignmentTool alignmentTool;

	private bool resetWorkspace = true;

	private WorkspaceMeshManager workspaceMeshManager;

	private Data.ChannelEnum channel;

	[Export(PropertyHint.None, "")]
	private NodePath previewspaceViewportContainerNodePath;

	private PreviewspaceViewport previewspaceViewport;

	private PreviewspaceMeshManager previewspaceMeshManager;

	private WorkspaceChecker checker;

	private EnvironmentManager environmentManager;

	private CameraManager cameraManager;

	private Grid grid;

	private ThreadsManager threadsManager;

	private BakeManager bakeManager;

	private List<Worksheet> worksheetsList = new List<Worksheet>();

	private int worksheetIndex = -1;

	private Worksheet worksheet;

	private SelectionManager selectionManager;

	private DrawingManager drawingManager;

	private Vector2i drawingPosition = Vector2i.NegOne;

	private Vector2i lastDrawingPosition = Vector2i.NegOne;

	private bool mask = true;

	private float delta = 1f / 60f;

	private Vector2 mouseDelta = Vector2.Zero;

	private ThumbnailRenderer thumbnailRenderer;

	private ProjectThumbnailRenderer projectThumbnailRenderer;

	[Export(PropertyHint.None, "")]
	private NodePath libraryContainerNodePath;

	private Container libraryContainer;

	private LibraryManager libraryManager;

	[Export(PropertyHint.None, "")]
	private NodePath saveLibraryPresetsFileDialogNodePath;

	private PreviewFileDialog saveLibraryPresetsFileDialog;

	[Export(PropertyHint.None, "")]
	private NodePath loadLibraryPresetsFileDialogNodePath;

	private PreviewFileDialog loadLibraryPresetsFileDialog;

	private Vector2i drawingStartPosition = Vector2i.NegOne;

	private bool doErase;

	private string[] cmdLineArgsArray;

	public Data.ChannelEnum Channel
	{
		get
		{
			return channel;
		}
		set
		{
			channel = value;
		}
	}

	public EnvironmentManager EnvironmentManager => environmentManager;

	public CameraManager CameraManager => cameraManager;

	public Grid GridManager => grid;

	public ThreadsManager ThreadsManager => threadsManager;

	public BakeManager BakeManager => bakeManager;

	public int WorksheetIndex => worksheetIndex;

	public Worksheet Worksheet => worksheet;

	public DrawingManager DrawingManager => drawingManager;

	public Vector2i DrawingPosition => drawingPosition;

	public Vector2i LastDrawingPosition => lastDrawingPosition;

	public bool Mask
	{
		get
		{
			return mask;
		}
		set
		{
			mask = value;
		}
	}

	public LibraryManager LibraryManager => libraryManager;

	public Workspace()
	{
		Register.Workspace = this;
	}

	public override void _Ready()
	{
		base._Ready();
		workspaceViewport = GetTree().Root;
		workspaceViewport.Connect(Signals.SizeChanged, this, "Resize");
		Register.WorkspaceViewport = workspaceViewport;
		previewspaceViewport = GetNodeOrNull<PreviewspaceViewportContainer>(previewspaceViewportContainerNodePath).GetChildOrNull<PreviewspaceViewport>(0);
		Register.PreviewspaceViewport = previewspaceViewport;
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.CHECKER, Resources.CheckerMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.WORKSPACE, Resources.WorkspaceFullMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.WORKSPACE_FULL, Resources.WorkspaceFullMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, Resources.WorkspaceChannelMaterial);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, "showMask", mask);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW, Resources.PreviewFullMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW_FULL, Resources.PreviewFullMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW_FULL_TWOSIDES, Resources.PreviewFullTwoSidesMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW_FULL_TRANSMISSION, Resources.PreviewFullTransmissionMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW_CHANNEL, Resources.PreviewChannelMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.PREVIEW_CHANNEL_TWOSIDES, Resources.PreviewChannelTwoSidesMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.GRID, Resources.GridMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.NODE, Resources.NodeMaterial);
		MaterialManager.SetShaderMaterial(MaterialManager.MaterialEnum.OUTLINE, Resources.OutlineMaterial);
		workspaceMeshManager = new WorkspaceMeshManager(this);
		previewspaceMeshManager = new PreviewspaceMeshManager(this);
		thumbnailRenderer = Resources.ThumbnailRendererScene.Instance<ThumbnailRenderer>();
		AddChild(thumbnailRenderer);
		thumbnailRenderer.Owner = this;
		thumbnailRenderer.Name = "ThumbnailRenderer";
		Register.ThumbnailRenderer = thumbnailRenderer;
		projectThumbnailRenderer = Resources.ProjectThumbnailRendererScene.Instance<ProjectThumbnailRenderer>();
		AddChild(projectThumbnailRenderer);
		projectThumbnailRenderer.Owner = this;
		projectThumbnailRenderer.Name = "ProjectThumbnailRenderer";
		Register.ProjectThumbnailRenderer = projectThumbnailRenderer;
		environmentManager = new EnvironmentManager(this, Resources.WorkspaceEnvironment, Resources.PreviewEnvironment);
		cameraManager = new CameraManager(this);
		threadsManager = new ThreadsManager(this);
		selectionManager = new SelectionManager(this);
		drawingManager = new DrawingManager(this);
		drawingManager.StampTool.DefaultMask = Resources.DefaultMaskImage;
		bakeManager = new BakeManager(this);
		libraryContainer = GetNodeOrNull<Container>(libraryContainerNodePath);
		libraryManager = new LibraryManager(this, libraryContainer, thumbnailRenderer, Resources.LibrarySyncIconTexture);
		saveLibraryPresetsFileDialog = GetNodeOrNull<PreviewFileDialog>(saveLibraryPresetsFileDialogNodePath);
		loadLibraryPresetsFileDialog = GetNodeOrNull<PreviewFileDialog>(loadLibraryPresetsFileDialogNodePath);
		checker = new WorkspaceChecker(this);
		grid = new Grid(this);
		alignmentTool = new AlignmentTool(this);
		CreateWorksheet(128, 128, tileable: false);
		cmdLineArgsArray = OS.GetCmdlineArgs();
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (@event is InputEventMouseMotion e)
		{
			mouseDelta = e.Relative;
		}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		Register.Gui.InformationsLabel.Text = "";
		if (resetWorkspace)
		{
			Reset();
			InputManager.ResetMouseOverUserInterfaceCounter();
			resetWorkspace = false;
			LoadSettings("Settings.xml");
			if (cmdLineArgsArray != null && cmdLineArgsArray.Length != 0 && !cmdLineArgsArray[0].Empty())
			{
				GD.Print(cmdLineArgsArray[0]);
				FileSystem.OpenProject(this, cmdLineArgsArray[0]);
				cmdLineArgsArray = null;
			}
		}
		this.delta = delta;
		CameraManager.EnableWorkspaceCameraControls(InputManager.UpdateControls);
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.GLOBAL, enable: true);
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.WORKSPACE, CameraManager.IsWorkspaceCameraControlsEnable());
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.PREVIEWSPACE, CameraManager.IsPreviewCameraControlsEnable());
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.FULLSPACE, CameraManager.IsWorkspaceCameraControlsEnable() || CameraManager.IsPreviewCameraControlsEnable());
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_BRUSH, enable: false);
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_LINE, enable: false);
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_RECTANGLE, enable: false);
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_CIRCLE, enable: false);
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_BUCKET, enable: false);
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_STAMP, enable: false);
		InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_SELECTION, enable: false);
		if (InputManager.IsKeyBingingsListEnabled(InputManager.KeyBindingsListEnum.FULLSPACE))
		{
			switch (DrawingManager.Tool)
			{
			case DrawingManager.ToolEnum.BRUSH:
				InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_BRUSH, enable: true);
				break;
			case DrawingManager.ToolEnum.LINE:
				InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_LINE, enable: true);
				break;
			case DrawingManager.ToolEnum.RECTANGLE:
			case DrawingManager.ToolEnum.RECTANGLE_FILLED:
				InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_RECTANGLE, enable: true);
				break;
			case DrawingManager.ToolEnum.CIRCLE:
			case DrawingManager.ToolEnum.CIRCLE_FILLED:
				InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_CIRCLE, enable: true);
				break;
			case DrawingManager.ToolEnum.BUCKET:
				InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_BUCKET, enable: true);
				break;
			case DrawingManager.ToolEnum.STAMP:
				InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_STAMP, enable: true);
				break;
			case DrawingManager.ToolEnum.DISABLED:
				if (selectionManager.IsEditingActivated)
				{
					InputManager.EnableKeyBingingsList(InputManager.KeyBindingsListEnum.TOOL_SELECTION, enable: true);
				}
				break;
			}
		}
		if (InputManager.CursorCollision.CollisionDetected)
		{
			lastDrawingPosition = drawingPosition;
			if (DrawingManager.CurrentTool != null && (DrawingManager.CurrentTool.Size & 1) == 0)
			{
				drawingPosition = new Vector2i(Mathf.FloorToInt(-0.5f + InputManager.CursorCollision.UV.x * (float)worksheet.Data.Width), Mathf.FloorToInt(-0.5f + InputManager.CursorCollision.UV.y * (float)worksheet.Data.Height));
			}
			else
			{
				drawingPosition = new Vector2i(Mathf.FloorToInt(InputManager.CursorCollision.UV.x * (float)worksheet.Data.Width), Mathf.FloorToInt(InputManager.CursorCollision.UV.y * (float)worksheet.Data.Height));
			}
			if (Register.CollisionManager.Collision.CollisionDetected)
			{
				DebugManager.AddLine("Collision: " + Register.CollisionManager.Collision.Position.x.ToString("0.000") + ", " + Register.CollisionManager.Collision.Position.y.ToString("0.000") + ", " + Register.CollisionManager.Collision.Position.z.ToString("0.000"));
			}
			DebugManager.AddLine("UV: " + InputManager.CursorCollision.UV.x.ToString("0.000") + ", " + InputManager.CursorCollision.UV.y.ToString("0.000"));
		}
		Register.Gui.ControlIconTextureRect.Visible = false;
		if (InputManager.IsKeyBingingsListEnabled(InputManager.KeyBindingsListEnum.FULLSPACE))
		{
			if (InputManager.CursorCollision.CollisionDetected)
			{
				if (DrawingManager.Tool != DrawingManager.ToolEnum.DISABLED)
				{
					if (Register.Gui.DisplaySettingsContainer.AlignmentToolToolButton.Pressed)
					{
						alignmentTool.Visible = true;
					}
					if (Register.DrawingManager.CurrentTool.AreaTool && Register.DrawingManager.IsDrawing)
					{
						alignmentTool.DoArea = true;
					}
					else
					{
						alignmentTool.DoArea = false;
					}
					alignmentTool.Update(drawingStartPosition, drawingPosition);
					if (doErase || (Input.IsKeyPressed(16777240) && !Input.IsKeyPressed(16777237) && !Input.IsKeyPressed(16777238)))
					{
						Register.Gui.ControlIconTextureRect.Visible = true;
					}
					if (!drawingManager.IsDrawing && InputManager.IsMouseButtonJustPressed(ButtonList.Left))
					{
						if (Input.IsKeyPressed(16777240))
						{
							doErase = true;
							drawingManager.PushSettings();
							drawingManager.BlendingMode = Blender.BlendingModeEnum.NORMAL;
							drawingManager.Color = worksheet.Data.ColorChannel.DefaultValue;
							drawingManager.Roughness = worksheet.Data.RoughnessChannel.DefaultValue;
							drawingManager.Metallicity = worksheet.Data.MetallicityChannel.DefaultValue;
							drawingManager.Height = worksheet.Data.HeightChannel.DefaultValue;
							drawingManager.Emission = worksheet.Data.EmissionChannel.DefaultValue;
							drawingManager.DrawingPreviewManager.ChangeBlendingMode(Blender.BlendingModeEnum.NORMAL);
							drawingManager.DrawingPreviewManager.ChangeColor(new Color(0.63f, 0.12f, 0.12f));
							drawingManager.DrawingPreviewManager.ChangeRoughness(Value.White);
							drawingManager.DrawingPreviewManager.ChangeMetallicity(Value.Black);
							drawingManager.DrawingPreviewManager.ChangeEmission(new Color(0.63f, 0.12f, 0.12f));
						}
						drawingStartPosition = drawingPosition;
						drawingManager.StartDrawing(worksheet, drawingStartPosition);
					}
					drawingManager.UpdateDrawing(drawingPosition);
					if (drawingManager.IsDrawing && InputManager.IsMouseButtonJustReleased(ButtonList.Left))
					{
						if (!doErase)
						{
							drawingManager.StopDrawing(worksheet, drawingPosition);
						}
						else
						{
							drawingManager.StopDrawing(worksheet, drawingPosition, doErase);
							drawingManager.PopSettings();
							doErase = false;
						}
					}
				}
				else
				{
					alignmentTool.Visible = false;
					if (drawingManager.IsDrawing)
					{
						drawingManager.AbortDrawing();
						if (doErase)
						{
							drawingManager.PopSettings();
							doErase = false;
						}
						if (!Register.DrawingPreviewManager.IsEmpty)
						{
							Register.DrawingPreviewManager.Clear();
							Register.DrawingPreviewManager.Update();
						}
					}
					if (selectionManager.IsEditingActivated && !InputManager.IsWindowShown)
					{
						selectionManager.Mode = SelectionManager.ModeEnum.REPLACE;
						if (Input.IsKeyPressed(16777240))
						{
							selectionManager.Mode = SelectionManager.ModeEnum.REMOVE;
						}
						else if (Input.IsKeyPressed(16777237))
						{
							selectionManager.Mode = SelectionManager.ModeEnum.ADD;
						}
						if (!selectionManager.IsSelecting && InputManager.IsMouseButtonJustPressed(ButtonList.Left))
						{
							selectionManager.Enabled = true;
							selectionManager.StartSelecting(worksheet, drawingPosition);
						}
						if (selectionManager.IsSelecting && InputManager.IsMouseButtonJustReleased(ButtonList.Left))
						{
							selectionManager.StopSelecting(worksheet, drawingPosition);
						}
						selectionManager.UpdateSelecting(drawingPosition);
					}
				}
			}
			else if (DrawingManager.Tool != DrawingManager.ToolEnum.DISABLED && drawingManager.IsDrawing && InputManager.IsMouseButtonJustReleased(ButtonList.Left))
			{
				if (!doErase)
				{
					drawingManager.StopDrawing(worksheet, drawingPosition);
				}
				else
				{
					drawingManager.StopDrawing(worksheet, drawingPosition, doErase);
					drawingManager.PopSettings();
					doErase = false;
				}
				drawingManager.UpdateDrawing(drawingPosition);
			}
		}
		else
		{
			if (drawingManager.IsDrawing)
			{
				drawingManager.StopDrawing(worksheet, drawingPosition);
				drawingManager.UpdateDrawing(drawingPosition);
				if (doErase)
				{
					drawingManager.PopSettings();
					doErase = false;
				}
			}
			drawingManager.AbortDrawing();
			drawingManager.ClearPreview();
			if (!Register.DrawingPreviewManager.IsEmpty)
			{
				Register.DrawingPreviewManager.Clear();
				Register.DrawingPreviewManager.Update();
			}
			selectionManager.AbortSelecting();
			alignmentTool.Visible = false;
		}
		if (worksheet.Data.UpdateTextures())
		{
			UpdateShaders();
			Register.Gui.UpdateChannelsTextures(updateAspectRatio: false);
		}
		mouseDelta = Vector2.Zero;
	}

	public override void _ExitTree()
	{
		SaveSettings("Settings.xml");
		base._ExitTree();
	}

	public void Resize()
	{
		libraryManager.Resize();
	}

	public Worksheet CreateWorksheet(int width, int height, bool tileable, string name = "Unnamed", Color[,] colorArray = null, Value[,] heightArray = null, Value[,] roughnessArray = null)
	{
		global::Channel.NormalMode = global::Channel.NormalModeEnum.SOBEL;
		global::Channel.NormalStrength = 2.5f;
		if (worksheet != null)
		{
			for (int i = 0; i < worksheetsList.Count; i++)
			{
				if (worksheetsList[i] == worksheet)
				{
					worksheetsList.RemoveAt(i);
					break;
				}
			}
			worksheet.QueueFree();
		}
		worksheetIndex = -1;
		worksheet = new Worksheet(width, height, tileable, name);
		AddChild(worksheet);
		worksheet.Owner = this;
		worksheetsList.Add(worksheet);
		worksheetIndex = worksheetsList.IndexOf(worksheet);
		thumbnailRenderer.Reset();
		thumbnailRenderer.RequestThumbnails(worksheet.DrawingPresetsList);
		Layer layer = worksheet.Data.Layer;
		if (colorArray != null)
		{
			layer.ColorChannel.SetArray(colorArray, width, height);
			layer.ColorChannel.DetectContentArea();
		}
		if (heightArray != null)
		{
			layer.HeightChannel.SetArray(heightArray, width, height);
			layer.HeightChannel.DetectContentArea();
		}
		if (roughnessArray != null)
		{
			layer.RoughnessChannel.SetArray(roughnessArray, width, height);
			layer.HeightChannel.DetectContentArea();
		}
		drawingManager.VerticalMirrorPosition = (float)width * 0.5f;
		drawingManager.HorizontalMirrorPosition = (float)height * 0.5f;
		UpdateMesh();
		UpdateGrid();
		previewspaceMeshManager.ChangeShader(worksheet.Data, PreviewspaceMeshManager.ShaderEnum.DEFAULT);
		previewspaceMeshManager.Reset();
		Register.CollisionManager.Update();
		resetWorkspace = true;
		OS.SetWindowTitle(Main.WindowTitle);
		return worksheet;
	}

	public Worksheet AddWorksheet(Worksheet worksheet, bool resetWorkspace = true)
	{
		this.worksheet = worksheet;
		AddChild(this.worksheet);
		this.worksheet.Owner = this;
		worksheetsList.Add(this.worksheet);
		worksheetIndex = worksheetsList.IndexOf(this.worksheet);
		drawingManager.VerticalMirrorPosition = (float)worksheet.Data.Width * 0.5f;
		drawingManager.HorizontalMirrorPosition = (float)worksheet.Data.Height * 0.5f;
		UpdateMesh();
		UpdateGrid();
		this.resetWorkspace = resetWorkspace;
		return worksheet;
	}

	public void DeleteWorksheets()
	{
		for (int i = 0; i < worksheetsList.Count; i++)
		{
			worksheetsList[i].QueueFree();
		}
		worksheetsList.Clear();
		worksheet = null;
		worksheetIndex = -1;
	}

	public void UpdateMesh()
	{
		workspaceMeshManager.UpdateMesh(worksheet);
		checker.SetMesh(workspaceMeshManager.Mesh);
		selectionManager.SetMesh(workspaceMeshManager.Mesh);
	}

	public void UpdateGrid()
	{
		grid.Update(worksheet, drawingManager.Mirroring, drawingManager.VerticalMirroring, drawingManager.VerticalMirrorPosition, drawingManager.HorizontalMirroring, drawingManager.HorizontalMirrorPosition, drawingManager.CircleMirroring, drawingManager.CircleVerticalMirrorPosition, drawingManager.CircleHorizontalMirrorPosition);
	}

	public void UpdateBake()
	{
		bakeManager.Update(worksheet);
	}

	public void UpdateShaders()
	{
		if (channel == Data.ChannelEnum.FULL)
		{
			workspaceMeshManager.SetMeshMaterial(MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.WORKSPACE));
		}
		else
		{
			workspaceMeshManager.SetMeshMaterial(MaterialManager.GetShaderMaterial(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL));
		}
		if (Register.PreviewspaceMeshManager.Illumination == PreviewspaceMeshManager.IlluminationEnum.ILLUMINATED)
		{
			previewspaceMeshManager.ActivateFullShaderMaterial();
		}
		else
		{
			previewspaceMeshManager.ActivateChannelShaderMaterial();
		}
		MaterialManager.UpdateChannelMaterials();
		MaterialManager.UpdateDrawingMaterials();
	}

	public void Reset()
	{
		if (worksheetsList.Count > 0)
		{
			worksheetIndex = 0;
			worksheet = worksheetsList[0];
		}
		else
		{
			worksheetIndex = -1;
			worksheet = null;
		}
		channel = Data.ChannelEnum.FULL;
		UpdateShaders();
		previewspaceViewport.Reset();
		drawingManager.Mirroring = false;
		UpdateGrid();
		grid.Visible = false;
		grid.DoShowLayerContentAreas = false;
		UpdateBake();
		Register.PreviewspaceMeshManager.UvLayoutVisible = true;
		environmentManager.Reset();
		cameraManager.Reset();
		selectionManager.Reset();
		drawingManager.Reset();
		libraryManager.Reset();
		Register.Gui.Reset();
		if (Register.PreviewspaceMeshManager.IsMeshLoaded)
		{
			Register.Gui.DisableBakeing(disabled: false);
		}
		else
		{
			Register.Gui.DisableBakeing(disabled: true);
		}
		InputManager.Reset();
	}

	public void ToggleGrid(bool visible)
	{
		grid.Visible = visible;
	}

	public void ToggleMask(bool visible)
	{
		mask = visible;
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE_CHANNEL, "showMask", mask);
	}

	public void ToggleMirroring(bool enable)
	{
		drawingManager.Mirroring = enable;
		UpdateGrid();
	}

	public void ToggleMirroring()
	{
		ToggleMirroring(!drawingManager.Mirroring);
		Register.Gui.DisplaySettingsContainer.ToggleMirroring(drawingManager.Mirroring);
	}

	public void SetMirrorAxis()
	{
		if (InputManager.CursorCollision.CollisionDetected)
		{
			if (drawingManager.VerticalMirroring)
			{
				drawingManager.VerticalMirrorPosition = drawingPosition.x;
				Register.Gui.DisplaySettingsContainer.MirroringToolButton.ChangeVerticalSliderValue(drawingManager.VerticalMirrorPosition);
				grid.UpdateMirror(worksheet, drawingManager.Mirroring, drawingManager.VerticalMirroring, drawingManager.VerticalMirrorPosition, drawingManager.HorizontalMirroring, drawingManager.HorizontalMirrorPosition, drawingManager.CircleMirroring, drawingManager.CircleVerticalMirrorPosition, drawingManager.CircleHorizontalMirrorPosition);
			}
			if (drawingManager.HorizontalMirroring)
			{
				drawingManager.HorizontalMirrorPosition = drawingPosition.y;
				Register.Gui.DisplaySettingsContainer.MirroringToolButton.ChangeHorizontalSliderValue(drawingManager.HorizontalMirrorPosition);
				grid.UpdateMirror(worksheet, drawingManager.Mirroring, drawingManager.VerticalMirroring, drawingManager.VerticalMirrorPosition, drawingManager.HorizontalMirroring, drawingManager.HorizontalMirrorPosition, drawingManager.CircleMirroring, drawingManager.CircleVerticalMirrorPosition, drawingManager.CircleHorizontalMirrorPosition);
			}
			if (drawingManager.CircleMirroring)
			{
				drawingManager.CircleVerticalMirrorPosition = drawingPosition.x;
				drawingManager.CircleHorizontalMirrorPosition = drawingPosition.y;
				Register.Gui.DisplaySettingsContainer.MirroringToolButton.ChangeCircleVerticalSliderValue(drawingManager.CircleVerticalMirrorPosition);
				Register.Gui.DisplaySettingsContainer.MirroringToolButton.ChangeCircleHorizontalSliderValue(drawingManager.CircleHorizontalMirrorPosition);
				grid.UpdateMirror(worksheet, drawingManager.Mirroring, drawingManager.VerticalMirroring, drawingManager.VerticalMirrorPosition, drawingManager.HorizontalMirroring, drawingManager.HorizontalMirrorPosition, drawingManager.CircleMirroring, drawingManager.CircleVerticalMirrorPosition, drawingManager.CircleHorizontalMirrorPosition);
			}
		}
	}

	public void ToggleWireframe(bool visible)
	{
		Register.PreviewspaceMeshManager.UvLayoutVisible = visible;
	}

	public void ChangeCurrentChannel(int index)
	{
		channel = (Data.ChannelEnum)index;
		UpdateShaders();
		drawingManager.Channel = channel;
		Register.Gui.ChannelLabel.Text = Register.Gui.CurrentChannelItemList.GetItemText((int)channel);
	}

	public void ChangeAllDrawingChannels(bool enable)
	{
		drawingManager.ColorEnabled = enable;
		Register.MaterialContainer.EnableMaterialColor(enable);
		drawingManager.RoughnessEnabled = enable;
		Register.MaterialContainer.EnableMaterialRoughness(enable);
		drawingManager.MetallicityEnabled = enable;
		Register.MaterialContainer.EnableMaterialMetallicity(enable);
		drawingManager.HeightEnabled = enable;
		Register.MaterialContainer.EnableMaterialHeight(enable);
		drawingManager.EmissionEnabled = enable;
		Register.MaterialContainer.EnableMaterialEmission(enable);
	}

	public void ChangeDrawingChannel(int index, bool enable)
	{
		switch ((Data.ChannelEnum)index)
		{
		case Data.ChannelEnum.COLOR:
			drawingManager.ColorEnabled = enable;
			Register.MaterialContainer.EnableMaterialColor(enable);
			break;
		case Data.ChannelEnum.ROUGHNESS:
			drawingManager.RoughnessEnabled = enable;
			Register.MaterialContainer.EnableMaterialRoughness(enable);
			break;
		case Data.ChannelEnum.METALLICITY:
			drawingManager.MetallicityEnabled = enable;
			Register.MaterialContainer.EnableMaterialMetallicity(enable);
			break;
		case Data.ChannelEnum.HEIGHT:
			drawingManager.HeightEnabled = enable;
			Register.MaterialContainer.EnableMaterialHeight(enable);
			break;
		case Data.ChannelEnum.EMISSION:
			drawingManager.EmissionEnabled = enable;
			Register.MaterialContainer.EnableMaterialEmission(enable);
			break;
		case Data.ChannelEnum.NORMAL:
			break;
		}
	}

	public void ChangeTool(int index)
	{
		if (index < 9)
		{
			drawingManager.Tool = (DrawingManager.ToolEnum)index;
		}
		else
		{
			drawingManager.Tool = DrawingManager.ToolEnum.DISABLED;
		}
		switch (index)
		{
		case 9:
			Register.Gui.ControlsRichTextLabel.BbcodeText = selectionManager.Controls;
			break;
		case 10:
			Register.Gui.ControlsRichTextLabel.BbcodeText = previewspaceMeshManager.Controls;
			break;
		}
	}

	public void ChangeDrawingMode(int index)
	{
		drawingManager.BlendingMode = (Blender.BlendingModeEnum)index;
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.WORKSPACE, "previewBlendingMode", index);
		MaterialManager.SetShaderParam(MaterialManager.MaterialEnum.PREVIEW, "previewBlendingMode", index);
	}

	public void ChangeDrawingColor(Color color)
	{
		drawingManager.Color = color;
		libraryManager.ClearSelection();
	}

	public void ChangeDrawingColorAlphaChannel(bool enable)
	{
		drawingManager.ColorAlphaEnabled = enable;
	}

	public void ChangeDrawingRoughness(float roughness)
	{
		drawingManager.Roughness = new Value(roughness);
		libraryManager.ClearSelection();
	}

	public void ChangeDrawingMetallicity(float metallicity)
	{
		drawingManager.Metallicity = new Value(metallicity);
		libraryManager.ClearSelection();
	}

	public void ChangeDrawingHeight(float height)
	{
		drawingManager.Height = new Value(height);
		libraryManager.ClearSelection();
	}

	public void ChangeDrawingEmission(Color emission)
	{
		drawingManager.Emission = emission;
		libraryManager.ClearSelection();
	}

	public void AddLibraryPreset()
	{
		libraryManager.AddDrawingPreset();
	}

	public void DeleteLibraryPreset()
	{
		libraryManager.DeleteDrawingPreset();
	}

	public void PushLibraryPresetUp()
	{
		libraryManager.PushDrawingPresetUp();
	}

	public void PushLibraryPresetDown()
	{
		libraryManager.PushDrawingPresetDown();
	}

	public void ShowSaveLibraryPresetsFileDialog()
	{
		saveLibraryPresetsFileDialog.PopupCentered();
	}

	public void ShowLoadLibraryPresetsFileDialog()
	{
		loadLibraryPresetsFileDialog.PopupCentered();
	}

	public void SaveLibraryPresets(string file)
	{
		if (FileSystem.IsFilePathValid(file))
		{
			BinaryWriter binaryWriter = new BinaryWriter(System.IO.File.Create(file));
			worksheet.WriteDrawingPresetsToBinaryStream(binaryWriter);
			binaryWriter.Dispose();
			GD.Print("Save Preset File: " + file);
		}
	}

	public void LoadLibraryPresets(string file)
	{
		if (System.IO.File.Exists(file))
		{
			BinaryReader binaryReader = new BinaryReader(System.IO.File.OpenRead(file));
			Worksheet.ReadDrawingPresetsFromBinaryStream(worksheet, binaryReader);
			binaryReader.Dispose();
			libraryManager.RequestThumbnails(worksheet.DrawingPresetsList);
			libraryManager.Reset();
		}
	}

	public void SaveSettings(string file)
	{
		file = System.IO.Path.GetFullPath(file);
		if (FileSystem.IsFilePathValid(file))
		{
			XDocument xDocument = new XDocument();
			XElement root = new XElement("Root");
			XElement application = new XElement("Application");
			XElement windowPositon = new XElement("WindowPositon");
			windowPositon.SetAttributeValue("Load", Settings.SaveWindowPosition);
			windowPositon.SetAttributeValue("Fullscreen", OS.WindowFullscreen);
			windowPositon.SetAttributeValue("X", OS.WindowPosition.x);
			windowPositon.SetAttributeValue("Y", OS.WindowPosition.y);
			windowPositon.SetAttributeValue("Width", OS.WindowSize.x);
			windowPositon.SetAttributeValue("Height", OS.WindowSize.y);
			application.Add(windowPositon);
			XElement controlsHelp = new XElement("ControlsHelp");
			controlsHelp.SetAttributeValue("Show", Settings.ShowControlsHelp);
			application.Add(controlsHelp);
			XElement favourites = new XElement("Favourites");
			for (int i = 0; i < Settings.FavouritesList.Count; i++)
			{
				XElement favouritesElement = new XElement("ID" + i.ToString("0000"));
				favouritesElement.SetAttributeValue("Path", Settings.FavouritesList[i]);
				favourites.Add(favouritesElement);
			}
			application.Add(favourites);
			XElement recentlyOpened = new XElement("RecentlyOpened");
			for (int j = 0; j < Settings.RecentlyOpenedList.Count; j++)
			{
				XElement recentlyOpenedElement = new XElement("ID" + j.ToString("0000"));
				recentlyOpenedElement.SetAttributeValue("Path", Settings.RecentlyOpenedList[j]);
				recentlyOpened.Add(recentlyOpenedElement);
			}
			application.Add(recentlyOpened);
			root.Add(application);
			InputManager.SaveKeyBindings(root);
			xDocument.Add(root);
			xDocument.Save(file);
			GD.Print("Settings saved!");
		}
	}

	public void LoadSettings(string file)
	{
		if (!System.IO.File.Exists(file))
		{
			return;
		}
		XElement root = XDocument.Load(file).Element("Root");
		XElement application = root.Element("Application");
		XElement windowPositon = application.Element("WindowPositon");
		if (windowPositon != null)
		{
			Settings.SaveWindowPosition = bool.Parse(windowPositon.Attribute("Load").Value);
			if (Settings.SaveWindowPosition)
			{
				if (bool.Parse(windowPositon.Attribute("Fullscreen").Value))
				{
					Settings.SetFullscreen(enable: true);
				}
				else
				{
					float x = float.Parse(windowPositon.Attribute("X").Value);
					float y = float.Parse(windowPositon.Attribute("Y").Value);
					float x2 = float.Parse(windowPositon.Attribute("Width").Value);
					float height = float.Parse(windowPositon.Attribute("Height").Value);
					OS.WindowPosition = new Vector2(x, y);
					OS.WindowSize = new Vector2(x2, height);
				}
			}
			else
			{
				OS.CenterWindow();
			}
		}
		XElement controlsHelp = application.Element("ControlsHelp");
		if (controlsHelp != null)
		{
			Settings.ShowControlsHelp = bool.Parse(controlsHelp.Attribute("Show").Value);
			if (!Settings.ShowControlsHelp)
			{
				Register.Gui.SetHelp(enable: false);
			}
		}
		XElement favourites = application.Element("Favourites");
		if (favourites != null)
		{
			Settings.FavouritesHashSet.Clear();
			Settings.FavouritesList.Clear();
			foreach (XElement item in favourites.Elements())
			{
				string path = item.Attribute("Path").Value;
				if (Settings.FavouritesHashSet.Add(path))
				{
					Settings.FavouritesList.Add(path);
				}
			}
		}
		XElement recentlyOpened = application.Element("RecentlyOpened");
		if (recentlyOpened != null)
		{
			Settings.RecentlyOpenedHashSet.Clear();
			Settings.RecentlyOpenedList.Clear();
			foreach (XElement item2 in recentlyOpened.Elements())
			{
				string path2 = item2.Attribute("Path").Value;
				if (Settings.RecentlyOpenedHashSet.Add(path2))
				{
					Settings.RecentlyOpenedList.Add(path2);
				}
			}
		}
		Register.Gui.FileMenuButton.UpdateOpenRecentPopupMenu();
		InputManager.LoadKeyBindings(root);
		Register.Gui.UpdateKeyBindings();
	}

	public void AbortDrawing()
	{
		if (drawingManager.IsDrawing)
		{
			drawingManager.AbortDrawing();
		}
	}

	public void PickMaterialFromLayer()
	{
		if (InputManager.CursorCollision.CollisionDetected && !drawingManager.IsDrawing && drawingManager.Tool != DrawingManager.ToolEnum.DISABLED)
		{
			drawingManager.PickPixel(worksheet, drawingPosition.x, drawingPosition.y);
			Register.Gui.SetDrawingMaterial(drawingManager.Color, drawingManager.Height, drawingManager.Roughness, drawingManager.Metallicity, drawingManager.Emission);
		}
	}

	public void PickMaterial()
	{
		if (InputManager.CursorCollision.CollisionDetected && !drawingManager.IsDrawing && drawingManager.Tool != DrawingManager.ToolEnum.DISABLED)
		{
			drawingManager.PickPixel(worksheet, drawingPosition.x, drawingPosition.y, layerMode: false);
			Register.Gui.SetDrawingMaterial(drawingManager.Color, drawingManager.Height, drawingManager.Roughness, drawingManager.Metallicity, drawingManager.Emission);
		}
	}

	public void RotateEnvironment()
	{
		if (!drawingManager.IsDrawing)
		{
			EnvironmentManager.RotateEnvironment(mouseDelta.x * delta * 0.5f);
		}
	}

	public void Undo()
	{
		worksheet.History.UndoLastCommand();
	}

	public void Redo()
	{
		worksheet.History.RedoLastCommand();
	}
}
