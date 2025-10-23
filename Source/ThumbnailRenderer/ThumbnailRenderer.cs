using System.Collections.Generic;
using Godot;

public class ThumbnailRenderer : Viewport
{
	private enum StateEnum
	{
		IDLE,
		RENDERING,
		RECEIVING
	}

	private ShaderMaterial previewMaterial;

	private Queue<DrawingPreset> drawingPresetsQueue = new Queue<DrawingPreset>();

	private DrawingPreset currentDrawingPreset;

	private StateEnum state;

	public override void _Ready()
	{
		base._Ready();
		previewMaterial = (ShaderMaterial)GetChild<MeshInstance>(0).MaterialOverride;
		VisualServer.Singleton.Connect("frame_post_draw", this, "FramePostDraw");
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		switch (state)
		{
		case StateEnum.IDLE:
			if (drawingPresetsQueue.Count > 0)
			{
				currentDrawingPreset = drawingPresetsQueue.Dequeue();
				previewMaterial.SetShaderParam("color", currentDrawingPreset.Data.Color);
				previewMaterial.SetShaderParam("roughness", currentDrawingPreset.Data.Roughness.v);
				previewMaterial.SetShaderParam("metallicity", currentDrawingPreset.Data.Metallicity.v);
				previewMaterial.SetShaderParam("emission", currentDrawingPreset.Data.Emission);
				base.RenderTargetUpdateMode = UpdateMode.Once;
				state = StateEnum.RENDERING;
			}
			break;
		case StateEnum.RENDERING:
			state = StateEnum.RECEIVING;
			break;
		}
	}

	public void Reset()
	{
		drawingPresetsQueue.Clear();
		currentDrawingPreset = null;
		state = StateEnum.IDLE;
	}

	public void FramePostDraw()
	{
		if (state == StateEnum.RECEIVING && currentDrawingPreset != null)
		{
			currentDrawingPreset.Image = GetTexture().GetData();
			currentDrawingPreset.ImageTexture.CreateFromImage(currentDrawingPreset.Image);
			currentDrawingPreset.IsThumbnailAvailable = true;
			currentDrawingPreset.DoUpdateThumbnail = true;
			currentDrawingPreset.IsThumbnailRequested = false;
			currentDrawingPreset = null;
			state = StateEnum.IDLE;
		}
	}

	public void RequestThumbnail(DrawingPreset drawingPreset)
	{
		drawingPreset.IsThumbnailRequested = true;
		drawingPreset.DoUpdateThumbnail = false;
		drawingPreset.IsThumbnailAvailable = false;
		drawingPresetsQueue.Enqueue(drawingPreset);
	}

	public void RequestThumbnails(List<DrawingPreset> drawingPresetsList)
	{
		foreach (DrawingPreset drawingPreset in drawingPresetsList)
		{
			RequestThumbnail(drawingPreset);
		}
	}
}
