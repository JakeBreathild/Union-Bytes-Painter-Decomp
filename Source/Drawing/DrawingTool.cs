using Godot;

public abstract class DrawingTool
{
	protected const int MAX_SIZE = 64;

	protected DrawingManager drawingManager;

	protected DrawingPreviewManager drawingPreviewManager;

	protected Workspace workspace;

	protected bool areaTool;

	protected string name = "";

	protected string controls = "";

	protected bool circleMirroring;

	protected int size = 1;

	protected float halfSize = 0.5f;

	protected float hardness = 1f;

	protected float blendingStrength = 1f;

	protected Vector2i drawingStart = Vector2i.Zero;

	protected Vector2i drawingEnd = Vector2i.Zero;

	protected Vector2i drawingPositionLastFrame = Vector2i.NegOne;

	public bool AreaTool => areaTool;

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

	public string Controls
	{
		get
		{
			return controls;
		}
		set
		{
			controls = value;
		}
	}

	public bool CircleMirroring => circleMirroring;

	public int Size
	{
		get
		{
			return size;
		}
		set
		{
			size = Mathf.Clamp(value, 1, 64);
			halfSize = 0.5f * (float)size;
			UpdatePreview();
		}
	}

	public float Hardness
	{
		get
		{
			return hardness;
		}
		set
		{
			hardness = Mathf.Clamp(value, 0f, 1f);
			UpdatePreview();
		}
	}

	public float BlendingStrength
	{
		get
		{
			return blendingStrength;
		}
		set
		{
			blendingStrength = Mathf.Clamp(value, 0f, 1f);
			drawingPreviewManager.MaxBlendingStrength = blendingStrength;
			UpdatePreview();
		}
	}

	public Vector2i DrawingStart => drawingStart;

	public Vector2i DrawingEnd => drawingEnd;

	public Vector2i DrawingPositionLastFrame => drawingPositionLastFrame;

	public bool HasDrawingPositionChanged
	{
		get
		{
			if (drawingStart.x == drawingEnd.x)
			{
				return drawingStart.y != drawingEnd.y;
			}
			return true;
		}
	}

	public bool HasDrawingPositionChangedSinceLastFrame
	{
		get
		{
			if (drawingPositionLastFrame.x == drawingEnd.x)
			{
				return drawingPositionLastFrame.y != drawingEnd.y;
			}
			return true;
		}
	}

	public abstract void DrawPreview(Vector2i position);

	public virtual void UpdatePreview()
	{
		drawingPositionLastFrame = Vector2i.NegOne;
	}

	public abstract void DrawSpot(Vector2i position);

	public abstract void DrawStroke(Vector2i start, Vector2i end);

	public virtual void StartDrawing(Vector2i position)
	{
		drawingStart = (drawingEnd = position);
		drawingPositionLastFrame = Vector2i.NegOne;
		drawingPreviewManager.MaxBlendingStrength = blendingStrength;
	}

	public abstract void WhileDrawing(Vector2i position);

	public abstract void StopDrawing(Vector2i position);

	public virtual void AbortDrawing()
	{
	}

	public virtual void Reset()
	{
		blendingStrength = 1f;
		size = 1;
		halfSize = 0.5f;
		hardness = 1f;
		drawingStart = Vector2i.Zero;
		drawingEnd = Vector2i.Zero;
		drawingPositionLastFrame = Vector2i.NegOne;
	}
}
