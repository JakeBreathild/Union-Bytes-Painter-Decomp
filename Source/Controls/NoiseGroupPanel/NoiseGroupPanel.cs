using System;
using Godot;

public class NoiseGroupPanel : GroupPanel
{
	private FastNoiseLite fastNoiseLite = new FastNoiseLite();

	private bool doUpdate = true;

	private TextureRect previewTextureRect;

	private OptionButton typeOptionButton;

	private AdvancedSlider scaleAdvancedSlider;

	private AdvancedSlider seedAdvancedSlider;

	private AdvancedSlider frequencyAdvancedSlider;

	private AdvancedSlider octavesAdvancedSlider;

	private AdvancedSlider lacunarityAdvancedSlider;

	private OptionButton fractalTypeOptionButton;

	private OptionButton cellularTypeOptionButton;

	private Button adjustmentsInverseButton;

	private AdvancedSlider adjustmentsBlackAdvancedSlider;

	private AdvancedSlider adjustmentsWhiteAdvancedSlider;

	private AdvancedSlider adjustmentsScaleAdvancedSlider;

	private AdvancedSlider adjustmentsPositionAdvancedSlider;

	private float adjustmentsBlack;

	private float adjustmentsWhite = 1f;

	private float adjustmentsScale = 1f;

	private float adjustmentsPosition = 0.5f;

	public ChannelArray<float> Array { get; private set; }

	public Image Image { get; private set; }

	public ImageTexture Texture { get; private set; }

	public int Type { get; private set; } = 4;

	public float Scale { get; private set; } = 1f;

	public int Seed { get; private set; }

	public float Frequency { get; private set; } = 8f;

	public int Octaves { get; private set; } = 3;

	public float Lacunarity { get; private set; } = 2f;

	public FastNoiseLite.FractalType FractalType { get; private set; }

	public FastNoiseLite.CellularReturnType CellularType { get; private set; }

	public Action UpdateCallback { get; set; }

	public override void _Ready()
	{
		base._Ready();
		previewTextureRect = GetNodeOrNull<TextureRect>("VC/Preview");
		previewTextureRect.Texture = null;
		typeOptionButton = GetNodeOrNull<OptionButton>("VC/Type");
		typeOptionButton.AddItem("Random");
		typeOptionButton.AddItem(FastNoiseLite.NoiseType.OpenSimplex2.ToString());
		typeOptionButton.AddItem(FastNoiseLite.NoiseType.OpenSimplex2S.ToString());
		typeOptionButton.AddItem(FastNoiseLite.NoiseType.Cellular.ToString());
		typeOptionButton.AddItem(FastNoiseLite.NoiseType.Perlin.ToString());
		typeOptionButton.AddItem(FastNoiseLite.NoiseType.ValueCubic.ToString());
		typeOptionButton.AddItem(FastNoiseLite.NoiseType.Value.ToString());
		typeOptionButton.Selected = Type;
		typeOptionButton.Connect(Signals.ItemSelected, this, "TypeSelected");
		scaleAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/Scale");
		scaleAdvancedSlider.Value = Scale;
		scaleAdvancedSlider.DefaultValue = Scale;
		scaleAdvancedSlider.ValueChangedCallback = ScaleChanged;
		seedAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/Seed");
		seedAdvancedSlider.IntValue = Seed;
		seedAdvancedSlider.DefaultIntValue = Seed;
		seedAdvancedSlider.ValueChangedCallback = SeedChanged;
		frequencyAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/Frequency");
		frequencyAdvancedSlider.Value = Frequency;
		frequencyAdvancedSlider.DefaultValue = Frequency;
		frequencyAdvancedSlider.ValueChangedCallback = FrequencyChanged;
		octavesAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/Octaves");
		octavesAdvancedSlider.IntValue = Octaves;
		octavesAdvancedSlider.DefaultIntValue = Octaves;
		octavesAdvancedSlider.ValueChangedCallback = OctavesChanged;
		lacunarityAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/Lacunarity");
		lacunarityAdvancedSlider.Value = Lacunarity;
		lacunarityAdvancedSlider.DefaultValue = Lacunarity;
		lacunarityAdvancedSlider.ValueChangedCallback = LacunarityChanged;
		fractalTypeOptionButton = GetNodeOrNull<OptionButton>("VC/FractalType");
		fractalTypeOptionButton.AddItem(FastNoiseLite.FractalType.None.ToString());
		fractalTypeOptionButton.AddItem(FastNoiseLite.FractalType.FBm.ToString());
		fractalTypeOptionButton.AddItem(FastNoiseLite.FractalType.Ridged.ToString());
		fractalTypeOptionButton.AddItem(FastNoiseLite.FractalType.PingPong.ToString());
		fractalTypeOptionButton.AddItem(FastNoiseLite.FractalType.DomainWarpProgressive.ToString());
		fractalTypeOptionButton.AddItem(FastNoiseLite.FractalType.DomainWarpIndependent.ToString());
		fractalTypeOptionButton.Selected = (int)FractalType;
		fractalTypeOptionButton.Connect(Signals.ItemSelected, this, "FractalTypeSelected");
		cellularTypeOptionButton = GetNodeOrNull<OptionButton>("VC/CellularType");
		cellularTypeOptionButton.AddItem(FastNoiseLite.CellularReturnType.CellValue.ToString());
		cellularTypeOptionButton.AddItem(FastNoiseLite.CellularReturnType.Distance.ToString());
		cellularTypeOptionButton.AddItem(FastNoiseLite.CellularReturnType.Distance2.ToString());
		cellularTypeOptionButton.AddItem(FastNoiseLite.CellularReturnType.Distance2Add.ToString());
		cellularTypeOptionButton.AddItem(FastNoiseLite.CellularReturnType.Distance2Sub.ToString());
		cellularTypeOptionButton.AddItem(FastNoiseLite.CellularReturnType.Distance2Mul.ToString());
		cellularTypeOptionButton.AddItem(FastNoiseLite.CellularReturnType.Distance2Div.ToString());
		cellularTypeOptionButton.Selected = (int)CellularType;
		cellularTypeOptionButton.Connect(Signals.ItemSelected, this, "CellularTypeSelected");
		adjustmentsInverseButton = GetNodeOrNull<Button>("VC/Inverse");
		adjustmentsInverseButton.Connect(Signals.Pressed, this, "AdjustmentsInversePressed");
		adjustmentsBlackAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/AdjustmentBlack");
		adjustmentsBlackAdvancedSlider.Value = adjustmentsBlack;
		adjustmentsBlackAdvancedSlider.DefaultValue = adjustmentsBlack;
		adjustmentsBlackAdvancedSlider.ValueChangedCallback = AdjustmentsBlackChanged;
		adjustmentsWhiteAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/AdjustmentWhite");
		adjustmentsWhiteAdvancedSlider.Value = adjustmentsWhite;
		adjustmentsWhiteAdvancedSlider.DefaultValue = adjustmentsWhite;
		adjustmentsWhiteAdvancedSlider.ValueChangedCallback = AdjustmentsWhiteChanged;
		adjustmentsScaleAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/AdjustmentScale");
		adjustmentsScaleAdvancedSlider.Value = adjustmentsScale;
		adjustmentsScaleAdvancedSlider.DefaultValue = adjustmentsScale;
		adjustmentsScaleAdvancedSlider.ValueChangedCallback = AdjustmentsScaleChanged;
		adjustmentsPositionAdvancedSlider = GetNodeOrNull<AdvancedSlider>("VC/AdjustmentPosition");
		adjustmentsPositionAdvancedSlider.Value = adjustmentsPosition;
		adjustmentsPositionAdvancedSlider.DefaultValue = adjustmentsPosition;
		adjustmentsPositionAdvancedSlider.ValueChangedCallback = AdjustmentsPositionChanged;
		base.ResetButtonPressedCallback = Reset;
		Reset();
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (doUpdate)
		{
			UpdateArray();
			doUpdate = false;
		}
	}

	public new void Reset()
	{
		base.Reset();
		Type = 4;
		Scale = 1f;
		Seed = 0;
		Frequency = 8f;
		Octaves = 3;
		Lacunarity = 2f;
		FractalType = FastNoiseLite.FractalType.None;
		CellularType = FastNoiseLite.CellularReturnType.CellValue;
		adjustmentsBlackAdvancedSlider.Reset();
		adjustmentsBlack = adjustmentsBlackAdvancedSlider.Value;
		adjustmentsWhiteAdvancedSlider.Reset();
		adjustmentsWhite = adjustmentsWhiteAdvancedSlider.Value;
		adjustmentsScaleAdvancedSlider.Reset();
		adjustmentsScale = adjustmentsScaleAdvancedSlider.Value;
		adjustmentsPositionAdvancedSlider.Reset();
		adjustmentsPosition = adjustmentsPositionAdvancedSlider.Value;
		typeOptionButton.Selected = Type;
		scaleAdvancedSlider.Value = Scale;
		seedAdvancedSlider.IntValue = Seed;
		frequencyAdvancedSlider.Value = Frequency;
		octavesAdvancedSlider.IntValue = Octaves;
		lacunarityAdvancedSlider.Value = Lacunarity;
		fractalTypeOptionButton.Selected = (int)FractalType;
		cellularTypeOptionButton.Selected = (int)CellularType;
		adjustmentsBlackAdvancedSlider.Value = adjustmentsBlack;
		adjustmentsWhiteAdvancedSlider.Value = adjustmentsWhite;
		adjustmentsScaleAdvancedSlider.Value = adjustmentsScale;
		adjustmentsPositionAdvancedSlider.Value = adjustmentsPosition;
		doUpdate = true;
	}

	public void UpdateArray()
	{
		Vector2 offset = new Vector2(Mathf.Sin(0.423354f * (float)Seed) * Scale, Mathf.Cos(0.198125f * (float)Seed) * Scale);
		float value = 0f;
		if (Type > 0)
		{
			fastNoiseLite.SetSeed(Seed);
			switch (Type)
			{
			case 1:
				fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
				break;
			case 2:
				fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
				break;
			case 3:
				fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
				break;
			case 4:
				fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
				break;
			case 5:
				fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
				break;
			case 6:
				fastNoiseLite.SetNoiseType(FastNoiseLite.NoiseType.Value);
				break;
			}
			fastNoiseLite.SetFrequency(Frequency);
			fastNoiseLite.SetFractalOctaves(Octaves);
			fastNoiseLite.SetFractalLacunarity(Lacunarity);
			fastNoiseLite.SetFractalType(FractalType);
			fastNoiseLite.SetCellularReturnType(CellularType);
			fastNoiseLite.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
		}
		else
		{
			GD.Seed((ulong)Seed);
		}
		Array = new ChannelArray<float>(Register.Workspace.Worksheet.Data.Width, Register.Workspace.Worksheet.Data.Height, 0f);
		float range = adjustmentsWhite - adjustmentsBlack;
		for (int y = 0; y < Array.Height; y++)
		{
			for (int x = 0; x < Array.Width; x++)
			{
				float noiseX = 1f * (float)x / (float)Array.Width * Scale + offset.x;
				float noiseY = 1f * (float)y / (float)Array.Height * Scale + offset.y;
				value = ((Type != 0) ? (fastNoiseLite.GetNoise(noiseX, noiseY) * 0.5f + 0.5f) : GD.Randf());
				value = Mathf.Clamp((value - adjustmentsPosition) / adjustmentsScale + 0.5f, 0f, 1f);
				value = adjustmentsBlack + value * range;
				Array[x, y] = value;
			}
		}
		Image = Array.CreateImage();
		Texture = new ImageTexture();
		Texture.CreateFromImage(Image, 0u);
		previewTextureRect.Texture = Texture;
		if (UpdateCallback != null)
		{
			UpdateCallback();
		}
	}

	public void TypeSelected(int index)
	{
		Type = index;
		doUpdate = true;
	}

	public void ScaleChanged(float value)
	{
		Scale = scaleAdvancedSlider.Value;
		doUpdate = true;
	}

	public void SeedChanged(float value)
	{
		Seed = seedAdvancedSlider.IntValue;
		doUpdate = true;
	}

	public void FrequencyChanged(float value)
	{
		Frequency = frequencyAdvancedSlider.Value;
		doUpdate = true;
	}

	public void OctavesChanged(float value)
	{
		Octaves = octavesAdvancedSlider.IntValue;
		doUpdate = true;
	}

	public void LacunarityChanged(float value)
	{
		Lacunarity = lacunarityAdvancedSlider.Value;
		doUpdate = true;
	}

	public void FractalTypeSelected(int index)
	{
		FractalType = (FastNoiseLite.FractalType)index;
		doUpdate = true;
	}

	public void CellularTypeSelected(int index)
	{
		CellularType = (FastNoiseLite.CellularReturnType)index;
		doUpdate = true;
	}

	public void AdjustmentsInversePressed()
	{
		float num = adjustmentsBlack;
		float num2 = adjustmentsWhite;
		adjustmentsWhite = num;
		adjustmentsBlack = num2;
		adjustmentsBlackAdvancedSlider.Value = adjustmentsBlack;
		adjustmentsWhiteAdvancedSlider.Value = adjustmentsWhite;
		doUpdate = true;
	}

	public void AdjustmentsBlackChanged(float value)
	{
		adjustmentsBlack = adjustmentsBlackAdvancedSlider.Value;
		doUpdate = true;
	}

	public void AdjustmentsWhiteChanged(float value)
	{
		adjustmentsWhite = adjustmentsWhiteAdvancedSlider.Value;
		doUpdate = true;
	}

	public void AdjustmentsScaleChanged(float value)
	{
		adjustmentsScale = adjustmentsScaleAdvancedSlider.Value;
		doUpdate = true;
	}

	public void AdjustmentsPositionChanged(float value)
	{
		adjustmentsPosition = adjustmentsPositionAdvancedSlider.Value;
		doUpdate = true;
	}
}
