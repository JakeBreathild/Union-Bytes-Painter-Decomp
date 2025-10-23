using System;
using System.Runtime.CompilerServices;

public class FastNoise
{
	public enum NoiseType
	{
		Value,
		ValueFractal,
		Perlin,
		PerlinFractal,
		Simplex,
		SimplexFractal,
		Cellular,
		WhiteNoise,
		Cubic,
		CubicFractal
	}

	public enum Interp
	{
		Linear,
		Hermite,
		Quintic
	}

	public enum FractalType
	{
		FBM,
		Billow,
		RigidMulti
	}

	public enum CellularDistanceFunction
	{
		Euclidean,
		Manhattan,
		Natural
	}

	public enum CellularReturnType
	{
		CellValue,
		NoiseLookup,
		Distance,
		Distance2,
		Distance2Add,
		Distance2Sub,
		Distance2Mul,
		Distance2Div
	}

	private struct Float2
	{
		public readonly float x;

		public readonly float y;

		public Float2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	}

	private struct Float3
	{
		public readonly float x;

		public readonly float y;

		public readonly float z;

		public Float3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}

	private const short FN_INLINE = 256;

	private const int FN_CELLULAR_INDEX_MAX = 3;

	private int m_seed = 1337;

	private float m_frequency = 0.01f;

	private Interp m_interp = Interp.Quintic;

	private NoiseType m_noiseType = NoiseType.Simplex;

	private int m_octaves = 3;

	private float m_lacunarity = 2f;

	private float m_gain = 0.5f;

	private FractalType m_fractalType;

	private float m_fractalBounding;

	private CellularDistanceFunction m_cellularDistanceFunction;

	private CellularReturnType m_cellularReturnType;

	private FastNoise m_cellularNoiseLookup;

	private int m_cellularDistanceIndex0;

	private int m_cellularDistanceIndex1 = 1;

	private float m_cellularJitter = 0.45f;

	private float m_gradientPerturbAmp = 1f;

	private static readonly Float2[] GRAD_2D = new Float2[8]
	{
		new Float2(-1f, -1f),
		new Float2(1f, -1f),
		new Float2(-1f, 1f),
		new Float2(1f, 1f),
		new Float2(0f, -1f),
		new Float2(-1f, 0f),
		new Float2(0f, 1f),
		new Float2(1f, 0f)
	};

	private static readonly Float3[] GRAD_3D = new Float3[16]
	{
		new Float3(1f, 1f, 0f),
		new Float3(-1f, 1f, 0f),
		new Float3(1f, -1f, 0f),
		new Float3(-1f, -1f, 0f),
		new Float3(1f, 0f, 1f),
		new Float3(-1f, 0f, 1f),
		new Float3(1f, 0f, -1f),
		new Float3(-1f, 0f, -1f),
		new Float3(0f, 1f, 1f),
		new Float3(0f, -1f, 1f),
		new Float3(0f, 1f, -1f),
		new Float3(0f, -1f, -1f),
		new Float3(1f, 1f, 0f),
		new Float3(0f, -1f, 1f),
		new Float3(-1f, 1f, 0f),
		new Float3(0f, -1f, -1f)
	};

	private static readonly Float2[] CELL_2D = new Float2[256]
	{
		new Float2(-0.2700222f, -0.9628541f),
		new Float2(0.38630927f, -0.9223693f),
		new Float2(0.04444859f, -0.9990117f),
		new Float2(-0.59925234f, -0.80056024f),
		new Float2(-0.781928f, 0.62336874f),
		new Float2(0.9464672f, 0.32279992f),
		new Float2(-0.6514147f, -0.7587219f),
		new Float2(0.93784726f, 0.34704837f),
		new Float2(-0.8497876f, -0.52712524f),
		new Float2(-0.87904257f, 0.47674325f),
		new Float2(-0.8923003f, -0.45144236f),
		new Float2(-0.37984443f, -0.9250504f),
		new Float2(-0.9951651f, 0.09821638f),
		new Float2(0.7724398f, -0.635088f),
		new Float2(0.75732833f, -0.6530343f),
		new Float2(-0.9928005f, -0.119780056f),
		new Float2(-0.05326657f, 0.99858034f),
		new Float2(0.97542536f, -0.22033007f),
		new Float2(-0.76650184f, 0.64224213f),
		new Float2(0.9916367f, 0.12906061f),
		new Float2(-0.99469686f, 0.10285038f),
		new Float2(-0.53792053f, -0.8429955f),
		new Float2(0.50228155f, -0.86470413f),
		new Float2(0.45598215f, -0.8899889f),
		new Float2(-0.8659131f, -0.50019443f),
		new Float2(0.08794584f, -0.9961253f),
		new Float2(-0.5051685f, 0.8630207f),
		new Float2(0.7753185f, -0.6315704f),
		new Float2(-0.69219446f, 0.72171104f),
		new Float2(-0.51916593f, -0.85467345f),
		new Float2(0.8978623f, -0.4402764f),
		new Float2(-0.17067741f, 0.98532695f),
		new Float2(-0.935343f, -0.35374206f),
		new Float2(-0.99924046f, 0.038967468f),
		new Float2(-0.2882064f, -0.9575683f),
		new Float2(-0.96638113f, 0.2571138f),
		new Float2(-0.87597144f, -0.48236302f),
		new Float2(-0.8303123f, -0.55729836f),
		new Float2(0.051101338f, -0.99869347f),
		new Float2(-0.85583735f, -0.51724505f),
		new Float2(0.098870255f, 0.9951003f),
		new Float2(0.9189016f, 0.39448678f),
		new Float2(-0.24393758f, -0.96979094f),
		new Float2(-0.81214094f, -0.5834613f),
		new Float2(-0.99104315f, 0.13354214f),
		new Float2(0.8492424f, -0.52800316f),
		new Float2(-0.9717839f, -0.23587295f),
		new Float2(0.9949457f, 0.10041421f),
		new Float2(0.6241065f, -0.7813392f),
		new Float2(0.6629103f, 0.74869883f),
		new Float2(-0.7197418f, 0.6942418f),
		new Float2(-0.8143371f, -0.58039224f),
		new Float2(0.10452105f, -0.9945227f),
		new Float2(-0.10659261f, -0.99430275f),
		new Float2(0.44579968f, -0.8951328f),
		new Float2(0.105547406f, 0.99441427f),
		new Float2(-0.9927903f, 0.11986445f),
		new Float2(-0.83343667f, 0.55261505f),
		new Float2(0.9115562f, -0.4111756f),
		new Float2(0.8285545f, -0.55990845f),
		new Float2(0.7217098f, -0.6921958f),
		new Float2(0.49404928f, -0.8694339f),
		new Float2(-0.36523214f, -0.9309165f),
		new Float2(-0.9696607f, 0.24445485f),
		new Float2(0.089255095f, -0.9960088f),
		new Float2(0.5354071f, -0.8445941f),
		new Float2(-0.10535762f, 0.9944344f),
		new Float2(-0.98902845f, 0.1477251f),
		new Float2(0.004856105f, 0.9999882f),
		new Float2(0.98855984f, 0.15082914f),
		new Float2(0.92861295f, -0.37104982f),
		new Float2(-0.5832394f, -0.8123003f),
		new Float2(0.30152076f, 0.9534596f),
		new Float2(-0.95751107f, 0.28839657f),
		new Float2(0.9715802f, -0.23671055f),
		new Float2(0.2299818f, 0.97319496f),
		new Float2(0.9557638f, -0.2941352f),
		new Float2(0.7409561f, 0.67155343f),
		new Float2(-0.9971514f, -0.07542631f),
		new Float2(0.69057107f, -0.7232645f),
		new Float2(-0.2907137f, -0.9568101f),
		new Float2(0.5912778f, -0.80646795f),
		new Float2(-0.94545925f, -0.3257405f),
		new Float2(0.66644555f, 0.7455537f),
		new Float2(0.6236135f, 0.78173286f),
		new Float2(0.9126994f, -0.40863165f),
		new Float2(-0.8191762f, 0.57354194f),
		new Float2(-0.8812746f, -0.4726046f),
		new Float2(0.99533135f, 0.09651673f),
		new Float2(0.98556507f, -0.16929697f),
		new Float2(-0.8495981f, 0.52743065f),
		new Float2(0.6174854f, -0.78658235f),
		new Float2(0.85081565f, 0.5254643f),
		new Float2(0.99850327f, -0.0546925f),
		new Float2(0.19713716f, -0.98037595f),
		new Float2(0.66078556f, -0.7505747f),
		new Float2(-0.030974941f, 0.9995202f),
		new Float2(-0.6731661f, 0.73949134f),
		new Float2(-0.71950185f, -0.69449055f),
		new Float2(0.97275114f, 0.2318516f),
		new Float2(0.9997059f, -0.02425069f),
		new Float2(0.44217876f, -0.89692694f),
		new Float2(0.9981351f, -0.061043672f),
		new Float2(-0.9173661f, -0.39804456f),
		new Float2(-0.81500566f, -0.579453f),
		new Float2(-0.87893313f, 0.476945f),
		new Float2(0.015860584f, 0.99987423f),
		new Float2(-0.8095465f, 0.5870558f),
		new Float2(-0.9165899f, -0.39982867f),
		new Float2(-0.8023543f, 0.5968481f),
		new Float2(-0.5176738f, 0.85557806f),
		new Float2(-0.8154407f, -0.57884055f),
		new Float2(0.40220103f, -0.91555136f),
		new Float2(-0.9052557f, -0.4248672f),
		new Float2(0.7317446f, 0.681579f),
		new Float2(-0.56476325f, -0.825253f),
		new Float2(-0.8403276f, -0.54207885f),
		new Float2(-0.93142813f, 0.36392525f),
		new Float2(0.52381986f, 0.85182905f),
		new Float2(0.7432804f, -0.66898f),
		new Float2(-0.9853716f, -0.17041974f),
		new Float2(0.46014687f, 0.88784283f),
		new Float2(0.8258554f, 0.56388193f),
		new Float2(0.6182366f, 0.785992f),
		new Float2(0.83315027f, -0.55304664f),
		new Float2(0.15003075f, 0.9886813f),
		new Float2(-0.6623304f, -0.7492119f),
		new Float2(-0.66859865f, 0.74362344f),
		new Float2(0.7025606f, 0.7116239f),
		new Float2(-0.54193896f, -0.84041786f),
		new Float2(-0.33886164f, 0.9408362f),
		new Float2(0.833153f, 0.55304253f),
		new Float2(-0.29897207f, -0.95426184f),
		new Float2(0.2638523f, 0.9645631f),
		new Float2(0.12410874f, -0.9922686f),
		new Float2(-0.7282649f, -0.6852957f),
		new Float2(0.69625f, 0.71779937f),
		new Float2(-0.91835356f, 0.395761f),
		new Float2(-0.6326102f, -0.7744703f),
		new Float2(-0.9331892f, -0.35938552f),
		new Float2(-0.11537793f, -0.99332166f),
		new Float2(0.9514975f, -0.30765656f),
		new Float2(-0.08987977f, -0.9959526f),
		new Float2(0.6678497f, 0.7442962f),
		new Float2(0.79524004f, -0.6062947f),
		new Float2(-0.6462007f, -0.7631675f),
		new Float2(-0.27335986f, 0.96191186f),
		new Float2(0.966959f, -0.25493184f),
		new Float2(-0.9792895f, 0.20246519f),
		new Float2(-0.5369503f, -0.84361386f),
		new Float2(-0.27003646f, -0.9628501f),
		new Float2(-0.6400277f, 0.76835185f),
		new Float2(-0.78545374f, -0.6189204f),
		new Float2(0.060059056f, -0.9981948f),
		new Float2(-0.024557704f, 0.9996984f),
		new Float2(-0.65983623f, 0.7514095f),
		new Float2(-0.62538946f, -0.7803128f),
		new Float2(-0.6210409f, -0.7837782f),
		new Float2(0.8348889f, 0.55041856f),
		new Float2(-0.15922752f, 0.9872419f),
		new Float2(0.83676225f, 0.54756635f),
		new Float2(-0.8675754f, -0.4973057f),
		new Float2(-0.20226626f, -0.97933054f),
		new Float2(0.939919f, 0.34139755f),
		new Float2(0.98774046f, -0.1561049f),
		new Float2(-0.90344554f, 0.42870283f),
		new Float2(0.12698042f, -0.9919052f),
		new Float2(-0.3819601f, 0.92417884f),
		new Float2(0.9754626f, 0.22016525f),
		new Float2(-0.32040158f, -0.94728184f),
		new Float2(-0.9874761f, 0.15776874f),
		new Float2(0.025353484f, -0.99967855f),
		new Float2(0.4835131f, -0.8753371f),
		new Float2(-0.28508f, -0.9585037f),
		new Float2(-0.06805516f, -0.99768156f),
		new Float2(-0.7885244f, -0.61500347f),
		new Float2(0.3185392f, -0.9479097f),
		new Float2(0.8880043f, 0.45983514f),
		new Float2(0.64769214f, -0.76190215f),
		new Float2(0.98202413f, 0.18875542f),
		new Float2(0.93572754f, -0.35272372f),
		new Float2(-0.88948953f, 0.45695552f),
		new Float2(0.7922791f, 0.6101588f),
		new Float2(0.74838185f, 0.66326815f),
		new Float2(-0.728893f, -0.68462765f),
		new Float2(0.8729033f, -0.48789328f),
		new Float2(0.8288346f, 0.5594937f),
		new Float2(0.08074567f, 0.99673474f),
		new Float2(0.97991484f, -0.1994165f),
		new Float2(-0.5807307f, -0.81409574f),
		new Float2(-0.47000498f, -0.8826638f),
		new Float2(0.2409493f, 0.9705377f),
		new Float2(0.9437817f, -0.33056942f),
		new Float2(-0.89279985f, -0.45045355f),
		new Float2(-0.80696225f, 0.59060305f),
		new Float2(0.062589735f, 0.99803936f),
		new Float2(-0.93125975f, 0.36435598f),
		new Float2(0.57774496f, 0.81621736f),
		new Float2(-0.3360096f, -0.9418586f),
		new Float2(0.69793206f, -0.71616393f),
		new Float2(-0.0020081573f, -0.999998f),
		new Float2(-0.18272944f, -0.98316324f),
		new Float2(-0.6523912f, 0.7578824f),
		new Float2(-0.43026268f, -0.9027037f),
		new Float2(-0.9985126f, -0.054520912f),
		new Float2(-0.010281022f, -0.99994713f),
		new Float2(-0.49460712f, 0.86911666f),
		new Float2(-0.299935f, 0.95395964f),
		new Float2(0.8165472f, 0.5772787f),
		new Float2(0.26974604f, 0.9629315f),
		new Float2(-0.7306287f, -0.68277496f),
		new Float2(-0.7590952f, -0.65097964f),
		new Float2(-0.9070538f, 0.4210146f),
		new Float2(-0.5104861f, -0.859886f),
		new Float2(0.86133504f, 0.5080373f),
		new Float2(0.50078815f, -0.8655699f),
		new Float2(-0.6541582f, 0.7563578f),
		new Float2(-0.83827555f, -0.54524684f),
		new Float2(0.6940071f, 0.7199682f),
		new Float2(0.06950936f, 0.9975813f),
		new Float2(0.17029423f, -0.9853933f),
		new Float2(0.26959732f, 0.9629731f),
		new Float2(0.55196124f, -0.83386976f),
		new Float2(0.2256575f, -0.9742067f),
		new Float2(0.42152628f, -0.9068162f),
		new Float2(0.48818734f, -0.87273884f),
		new Float2(-0.3683855f, -0.92967314f),
		new Float2(-0.98253906f, 0.18605645f),
		new Float2(0.81256473f, 0.582871f),
		new Float2(0.3196461f, -0.947537f),
		new Float2(0.9570914f, 0.28978625f),
		new Float2(-0.6876655f, -0.7260276f),
		new Float2(-0.9988771f, -0.04737673f),
		new Float2(-0.1250179f, 0.9921545f),
		new Float2(-0.82801336f, 0.56070834f),
		new Float2(0.93248636f, -0.36120513f),
		new Float2(0.63946533f, 0.7688199f),
		new Float2(-0.016238471f, -0.99986815f),
		new Float2(-0.99550146f, -0.094746135f),
		new Float2(-0.8145332f, 0.580117f),
		new Float2(0.4037328f, -0.91487694f),
		new Float2(0.9944263f, 0.10543368f),
		new Float2(-0.16247116f, 0.9867133f),
		new Float2(-0.9949488f, -0.10038388f),
		new Float2(-0.69953024f, 0.714603f),
		new Float2(0.5263415f, -0.85027325f),
		new Float2(-0.5395222f, 0.8419714f),
		new Float2(0.65793705f, 0.7530729f),
		new Float2(0.014267588f, -0.9998982f),
		new Float2(-0.6734384f, 0.7392433f),
		new Float2(0.6394121f, -0.7688642f),
		new Float2(0.9211571f, 0.38919085f),
		new Float2(-0.14663722f, -0.98919034f),
		new Float2(-0.7823181f, 0.6228791f),
		new Float2(-0.5039611f, -0.8637264f),
		new Float2(-0.774312f, -0.632804f)
	};

	private static readonly Float3[] CELL_3D = new Float3[256]
	{
		new Float3(-0.7292737f, -0.66184396f, 0.17355819f),
		new Float3(0.7902921f, -0.5480887f, -0.2739291f),
		new Float3(0.7217579f, 0.62262124f, -0.3023381f),
		new Float3(0.5656831f, -0.8208298f, -0.079000026f),
		new Float3(0.76004905f, -0.55559796f, -0.33709997f),
		new Float3(0.37139457f, 0.50112647f, 0.78162545f),
		new Float3(-0.12770624f, -0.4254439f, -0.8959289f),
		new Float3(-0.2881561f, -0.5815839f, 0.7607406f),
		new Float3(0.5849561f, -0.6628202f, -0.4674352f),
		new Float3(0.33071712f, 0.039165374f, 0.94291687f),
		new Float3(0.8712122f, -0.41133744f, -0.26793817f),
		new Float3(0.580981f, 0.7021916f, 0.41156778f),
		new Float3(0.5037569f, 0.6330057f, -0.5878204f),
		new Float3(0.44937122f, 0.6013902f, 0.6606023f),
		new Float3(-0.6878404f, 0.090188906f, -0.7202372f),
		new Float3(-0.59589565f, -0.64693505f, 0.47579765f),
		new Float3(-0.5127052f, 0.1946922f, -0.83619875f),
		new Float3(-0.99115074f, -0.054102764f, -0.12121531f),
		new Float3(-0.21497211f, 0.9720882f, -0.09397608f),
		new Float3(-0.7518651f, -0.54280573f, 0.37424695f),
		new Float3(0.5237069f, 0.8516377f, -0.021078179f),
		new Float3(0.6333505f, 0.19261672f, -0.74951047f),
		new Float3(-0.06788242f, 0.39983058f, 0.9140719f),
		new Float3(-0.55386287f, -0.47298968f, -0.6852129f),
		new Float3(-0.72614557f, -0.5911991f, 0.35099334f),
		new Float3(-0.9229275f, -0.17828088f, 0.34120494f),
		new Float3(-0.6968815f, 0.65112746f, 0.30064803f),
		new Float3(0.96080446f, -0.20983632f, -0.18117249f),
		new Float3(0.068171464f, -0.9743405f, 0.21450691f),
		new Float3(-0.3577285f, -0.6697087f, -0.65078455f),
		new Float3(-0.18686211f, 0.7648617f, -0.61649746f),
		new Float3(-0.65416974f, 0.3967915f, 0.64390874f),
		new Float3(0.699334f, -0.6164538f, 0.36182392f),
		new Float3(-0.15466657f, 0.6291284f, 0.7617583f),
		new Float3(-0.6841613f, -0.2580482f, -0.68215424f),
		new Float3(0.5383981f, 0.4258655f, 0.727163f),
		new Float3(-0.5026988f, -0.7939833f, -0.3418837f),
		new Float3(0.32029718f, 0.28344154f, 0.9039196f),
		new Float3(0.86832273f, -0.00037626564f, -0.49599952f),
		new Float3(0.79112005f, -0.085110456f, 0.60571057f),
		new Float3(-0.04011016f, -0.43972486f, 0.8972364f),
		new Float3(0.914512f, 0.35793462f, -0.18854876f),
		new Float3(-0.96120393f, -0.27564842f, 0.010246669f),
		new Float3(0.65103614f, -0.28777993f, -0.70237786f),
		new Float3(-0.20417863f, 0.73652375f, 0.6448596f),
		new Float3(-0.7718264f, 0.37906268f, 0.5104856f),
		new Float3(-0.30600828f, -0.7692988f, 0.56083715f),
		new Float3(0.45400733f, -0.5024843f, 0.73578995f),
		new Float3(0.48167956f, 0.6021208f, -0.636738f),
		new Float3(0.69619805f, -0.32221973f, 0.6414692f),
		new Float3(-0.65321606f, -0.6781149f, 0.33685157f),
		new Float3(0.50893015f, -0.61546624f, -0.60182345f),
		new Float3(-0.16359198f, -0.9133605f, -0.37284088f),
		new Float3(0.5240802f, -0.8437664f, 0.11575059f),
		new Float3(0.5902587f, 0.4983818f, -0.63498837f),
		new Float3(0.5863228f, 0.49476475f, 0.6414308f),
		new Float3(0.6779335f, 0.23413453f, 0.6968409f),
		new Float3(0.7177054f, -0.68589795f, 0.12017863f),
		new Float3(-0.532882f, -0.5205125f, 0.6671608f),
		new Float3(-0.8654874f, -0.07007271f, -0.4960054f),
		new Float3(-0.286181f, 0.79520893f, 0.53454953f),
		new Float3(-0.048495296f, 0.98108363f, -0.18741156f),
		new Float3(-0.63585216f, 0.60583484f, 0.47818002f),
		new Float3(0.62547946f, -0.28616196f, 0.72586966f),
		new Float3(-0.258526f, 0.50619495f, -0.8227582f),
		new Float3(0.021363068f, 0.50640166f, -0.862033f),
		new Float3(0.20011178f, 0.85992634f, 0.46955505f),
		new Float3(0.47435614f, 0.6014985f, -0.6427953f),
		new Float3(0.6622994f, -0.52024746f, -0.539168f),
		new Float3(0.08084973f, -0.65327203f, 0.7527941f),
		new Float3(-0.6893687f, 0.059286036f, 0.7219805f),
		new Float3(-0.11218871f, -0.96731853f, 0.22739525f),
		new Float3(0.7344116f, 0.59796685f, -0.3210533f),
		new Float3(0.5789393f, -0.24888498f, 0.776457f),
		new Float3(0.69881827f, 0.35571697f, -0.6205791f),
		new Float3(-0.86368454f, -0.27487713f, -0.4224826f),
		new Float3(-0.4247028f, -0.46408808f, 0.77733505f),
		new Float3(0.5257723f, -0.84270173f, 0.11583299f),
		new Float3(0.93438303f, 0.31630248f, -0.16395439f),
		new Float3(-0.10168364f, -0.8057303f, -0.58348876f),
		new Float3(-0.6529239f, 0.50602126f, -0.5635893f),
		new Float3(-0.24652861f, -0.9668206f, -0.06694497f),
		new Float3(-0.9776897f, -0.20992506f, -0.0073688254f),
		new Float3(0.7736893f, 0.57342446f, 0.2694238f),
		new Float3(-0.6095088f, 0.4995679f, 0.6155737f),
		new Float3(0.5794535f, 0.7434547f, 0.33392924f),
		new Float3(-0.8226211f, 0.081425816f, 0.56272936f),
		new Float3(-0.51038545f, 0.47036678f, 0.719904f),
		new Float3(-0.5764972f, -0.072316565f, -0.81389266f),
		new Float3(0.7250629f, 0.39499715f, -0.56414634f),
		new Float3(-0.1525424f, 0.48608407f, -0.8604958f),
		new Float3(-0.55509764f, -0.49578208f, 0.6678823f),
		new Float3(-0.18836144f, 0.91458696f, 0.35784173f),
		new Float3(0.76255566f, -0.54144084f, -0.35404897f),
		new Float3(-0.5870232f, -0.3226498f, -0.7424964f),
		new Float3(0.30511242f, 0.2262544f, -0.9250488f),
		new Float3(0.63795763f, 0.57724243f, -0.50970703f),
		new Float3(-0.5966776f, 0.14548524f, -0.7891831f),
		new Float3(-0.65833056f, 0.65554875f, -0.36994147f),
		new Float3(0.74348927f, 0.23510846f, 0.6260573f),
		new Float3(0.5562114f, 0.82643604f, -0.08736329f),
		new Float3(-0.302894f, -0.8251527f, 0.47684193f),
		new Float3(0.11293438f, -0.9858884f, -0.123571075f),
		new Float3(0.5937653f, -0.5896814f, 0.5474657f),
		new Float3(0.6757964f, -0.58357584f, -0.45026484f),
		new Float3(0.7242303f, -0.11527198f, 0.67985505f),
		new Float3(-0.9511914f, 0.0753624f, -0.29925808f),
		new Float3(0.2539471f, -0.18863393f, 0.9486454f),
		new Float3(0.5714336f, -0.16794509f, -0.8032796f),
		new Float3(-0.06778235f, 0.39782694f, 0.9149532f),
		new Float3(0.6074973f, 0.73306f, -0.30589226f),
		new Float3(-0.54354787f, 0.16758224f, 0.8224791f),
		new Float3(-0.5876678f, -0.3380045f, -0.7351187f),
		new Float3(-0.79675627f, 0.040978227f, -0.60290986f),
		new Float3(-0.19963509f, 0.8706295f, 0.4496111f),
		new Float3(-0.027876602f, -0.91062325f, -0.4122962f),
		new Float3(-0.7797626f, -0.6257635f, 0.019757755f),
		new Float3(-0.5211233f, 0.74016446f, -0.42495546f),
		new Float3(0.8575425f, 0.4053273f, -0.31675017f),
		new Float3(0.10452233f, 0.8390196f, -0.53396744f),
		new Float3(0.3501823f, 0.9242524f, -0.15208502f),
		new Float3(0.19878499f, 0.076476134f, 0.9770547f),
		new Float3(0.78459966f, 0.6066257f, -0.12809642f),
		new Float3(0.09006737f, -0.97509897f, -0.20265691f),
		new Float3(-0.82743436f, -0.54229957f, 0.14582036f),
		new Float3(-0.34857976f, -0.41580227f, 0.8400004f),
		new Float3(-0.2471779f, -0.730482f, -0.6366311f),
		new Float3(-0.3700155f, 0.8577948f, 0.35675845f),
		new Float3(0.59133947f, -0.54831195f, -0.59133035f),
		new Float3(0.120487355f, -0.7626472f, -0.6354935f),
		new Float3(0.6169593f, 0.03079648f, 0.7863923f),
		new Float3(0.12581569f, -0.664083f, -0.73699677f),
		new Float3(-0.6477565f, -0.17401473f, -0.74170774f),
		new Float3(0.6217889f, -0.7804431f, -0.06547655f),
		new Float3(0.6589943f, -0.6096988f, 0.44044736f),
		new Float3(-0.26898375f, -0.6732403f, -0.68876356f),
		new Float3(-0.38497752f, 0.56765425f, 0.7277094f),
		new Float3(0.57544446f, 0.81104714f, -0.10519635f),
		new Float3(0.91415936f, 0.3832948f, 0.13190056f),
		new Float3(-0.10792532f, 0.9245494f, 0.36545935f),
		new Float3(0.3779771f, 0.30431488f, 0.87437165f),
		new Float3(-0.21428852f, -0.8259286f, 0.5214617f),
		new Float3(0.58025444f, 0.41480985f, -0.7008834f),
		new Float3(-0.19826609f, 0.85671616f, -0.47615966f),
		new Float3(-0.033815537f, 0.37731808f, -0.9254661f),
		new Float3(-0.68679225f, -0.6656598f, 0.29191336f),
		new Float3(0.7731743f, -0.28757936f, -0.565243f),
		new Float3(-0.09655942f, 0.91937083f, -0.3813575f),
		new Float3(0.27157024f, -0.957791f, -0.09426606f),
		new Float3(0.24510157f, -0.6917999f, -0.6792188f),
		new Float3(0.97770077f, -0.17538553f, 0.115503654f),
		new Float3(-0.522474f, 0.8521607f, 0.029036159f),
		new Float3(-0.77348804f, -0.52612925f, 0.35341796f),
		new Float3(-0.71344924f, -0.26954725f, 0.6467878f),
		new Float3(0.16440372f, 0.5105846f, -0.84396374f),
		new Float3(0.6494636f, 0.055856112f, 0.7583384f),
		new Float3(-0.4711971f, 0.50172806f, -0.7254256f),
		new Float3(-0.63357645f, -0.23816863f, -0.7361091f),
		new Float3(-0.9021533f, -0.2709478f, -0.33571818f),
		new Float3(-0.3793711f, 0.8722581f, 0.3086152f),
		new Float3(-0.68555987f, -0.32501432f, 0.6514394f),
		new Float3(0.29009423f, -0.7799058f, -0.5546101f),
		new Float3(-0.20983194f, 0.8503707f, 0.48253515f),
		new Float3(-0.45926037f, 0.6598504f, -0.5947077f),
		new Float3(0.87159455f, 0.09616365f, -0.48070312f),
		new Float3(-0.6776666f, 0.71185046f, -0.1844907f),
		new Float3(0.7044378f, 0.3124276f, 0.637304f),
		new Float3(-0.7052319f, -0.24010932f, -0.6670798f),
		new Float3(0.081921004f, -0.72073364f, -0.68835455f),
		new Float3(-0.6993681f, -0.5875763f, -0.4069869f),
		new Float3(-0.12814544f, 0.6419896f, 0.75592864f),
		new Float3(-0.6337388f, -0.67854714f, -0.3714147f),
		new Float3(0.5565052f, -0.21688876f, -0.8020357f),
		new Float3(-0.57915545f, 0.7244372f, -0.3738579f),
		new Float3(0.11757791f, -0.7096451f, 0.69467926f),
		new Float3(-0.613462f, 0.13236311f, 0.7785528f),
		new Float3(0.69846356f, -0.029805163f, -0.7150247f),
		new Float3(0.83180827f, -0.3930172f, 0.39195976f),
		new Float3(0.14695764f, 0.055416517f, -0.98758924f),
		new Float3(0.70886856f, -0.2690504f, 0.65201014f),
		new Float3(0.27260533f, 0.67369765f, -0.68688995f),
		new Float3(-0.65912956f, 0.30354586f, -0.68804663f),
		new Float3(0.48151314f, -0.752827f, 0.4487723f),
		new Float3(0.943001f, 0.16756473f, -0.28752613f),
		new Float3(0.43480295f, 0.7695305f, -0.46772778f),
		new Float3(0.39319962f, 0.5944736f, 0.70142365f),
		new Float3(0.72543365f, -0.60392565f, 0.33018148f),
		new Float3(0.75902355f, -0.6506083f, 0.024333132f),
		new Float3(-0.8552769f, -0.3430043f, 0.38839358f),
		new Float3(-0.6139747f, 0.6981725f, 0.36822575f),
		new Float3(-0.74659055f, -0.575201f, 0.33428493f),
		new Float3(0.5730066f, 0.8105555f, -0.12109168f),
		new Float3(-0.92258775f, -0.3475211f, -0.16751404f),
		new Float3(-0.71058166f, -0.47196922f, -0.5218417f),
		new Float3(-0.0856461f, 0.35830015f, 0.9296697f),
		new Float3(-0.8279698f, -0.2043157f, 0.5222271f),
		new Float3(0.42794403f, 0.278166f, 0.8599346f),
		new Float3(0.539908f, -0.78571206f, -0.3019204f),
		new Float3(0.5678404f, -0.5495414f, -0.61283076f),
		new Float3(-0.9896071f, 0.13656391f, -0.045034185f),
		new Float3(-0.6154343f, -0.64408755f, 0.45430374f),
		new Float3(0.10742044f, -0.79463404f, 0.59750944f),
		new Float3(-0.359545f, -0.888553f, 0.28495783f),
		new Float3(-0.21804053f, 0.1529889f, 0.9638738f),
		new Float3(-0.7277432f, -0.61640507f, -0.30072346f),
		new Float3(0.7249729f, -0.0066971947f, 0.68874484f),
		new Float3(-0.5553659f, -0.5336586f, 0.6377908f),
		new Float3(0.5137558f, 0.79762083f, -0.316f),
		new Float3(-0.3794025f, 0.92456084f, -0.035227515f),
		new Float3(0.82292485f, 0.27453658f, -0.49741766f),
		new Float3(-0.5404114f, 0.60911417f, 0.5804614f),
		new Float3(0.8036582f, -0.27030295f, 0.5301602f),
		new Float3(0.60443187f, 0.68329686f, 0.40959433f),
		new Float3(0.06389989f, 0.96582085f, -0.2512108f),
		new Float3(0.10871133f, 0.74024713f, -0.6634878f),
		new Float3(-0.7134277f, -0.6926784f, 0.10591285f),
		new Float3(0.64588976f, -0.57245487f, -0.50509584f),
		new Float3(-0.6553931f, 0.73814714f, 0.15999562f),
		new Float3(0.39109614f, 0.91888714f, -0.05186756f),
		new Float3(-0.48790225f, -0.5904377f, 0.64291114f),
		new Float3(0.601479f, 0.77074414f, -0.21018201f),
		new Float3(-0.5677173f, 0.7511361f, 0.33688518f),
		new Float3(0.7858574f, 0.22667466f, 0.5753667f),
		new Float3(-0.45203456f, -0.6042227f, -0.65618575f),
		new Float3(0.0022721163f, 0.4132844f, -0.9105992f),
		new Float3(-0.58157516f, -0.5162926f, 0.6286591f),
		new Float3(-0.03703705f, 0.8273786f, 0.5604221f),
		new Float3(-0.51196927f, 0.79535437f, -0.324498f),
		new Float3(-0.26824173f, -0.957229f, -0.10843876f),
		new Float3(-0.23224828f, -0.9679131f, -0.09594243f),
		new Float3(0.3554329f, -0.8881506f, 0.29130062f),
		new Float3(0.73465204f, -0.4371373f, 0.5188423f),
		new Float3(0.998512f, 0.046590112f, -0.028339446f),
		new Float3(-0.37276876f, -0.9082481f, 0.19007573f),
		new Float3(0.9173738f, -0.3483642f, 0.19252984f),
		new Float3(0.2714911f, 0.41475296f, -0.86848867f),
		new Float3(0.5131763f, -0.71163344f, 0.4798207f),
		new Float3(-0.87373537f, 0.18886992f, -0.44823506f),
		new Float3(0.84600437f, -0.3725218f, 0.38145f),
		new Float3(0.89787275f, -0.17802091f, -0.40265754f),
		new Float3(0.21780656f, -0.9698323f, -0.10947895f),
		new Float3(-0.15180314f, -0.7788918f, -0.6085091f),
		new Float3(-0.2600385f, -0.4755398f, -0.840382f),
		new Float3(0.5723135f, -0.7474341f, -0.33734185f),
		new Float3(-0.7174141f, 0.16990171f, -0.67561114f),
		new Float3(-0.6841808f, 0.021457076f, -0.72899675f),
		new Float3(-0.2007448f, 0.06555606f, -0.9774477f),
		new Float3(-0.11488037f, -0.8044887f, 0.5827524f),
		new Float3(-0.787035f, 0.03447489f, 0.6159443f),
		new Float3(-0.20155965f, 0.68598723f, 0.69913894f),
		new Float3(-0.085810825f, -0.10920836f, -0.99030805f),
		new Float3(0.5532693f, 0.73252505f, -0.39661077f),
		new Float3(-0.18424894f, -0.9777375f, -0.100407675f),
		new Float3(0.07754738f, -0.9111506f, 0.40471104f),
		new Float3(0.13998385f, 0.7601631f, -0.63447344f),
		new Float3(0.44844192f, -0.84528923f, 0.29049253f)
	};

	private const int X_PRIME = 1619;

	private const int Y_PRIME = 31337;

	private const int Z_PRIME = 6971;

	private const int W_PRIME = 1013;

	private const float F3 = 1f / 3f;

	private const float G3 = 1f / 6f;

	private const float G33 = -0.5f;

	private const float SQRT3 = 1.7320508f;

	private const float F2 = 0.3660254f;

	private const float G2 = 0.21132487f;

	private static readonly byte[] SIMPLEX_4D = new byte[256]
	{
		0, 1, 2, 3, 0, 1, 3, 2, 0, 0,
		0, 0, 0, 2, 3, 1, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 1, 2,
		3, 0, 0, 2, 1, 3, 0, 0, 0, 0,
		0, 3, 1, 2, 0, 3, 2, 1, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		1, 3, 2, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 1, 2, 0, 3,
		0, 0, 0, 0, 1, 3, 0, 2, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		2, 3, 0, 1, 2, 3, 1, 0, 1, 0,
		2, 3, 1, 0, 3, 2, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 2, 0,
		3, 1, 0, 0, 0, 0, 2, 1, 3, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 2, 0, 1, 3, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 3, 0,
		1, 2, 3, 0, 2, 1, 0, 0, 0, 0,
		3, 1, 2, 0, 2, 1, 0, 3, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		3, 1, 0, 2, 0, 0, 0, 0, 3, 2,
		0, 1, 3, 2, 1, 0
	};

	private const float F4 = 0.309017f;

	private const float G4 = 0.1381966f;

	private const float CUBIC_3D_BOUNDING = 8f / 27f;

	private const float CUBIC_2D_BOUNDING = 4f / 9f;

	public FastNoise(int seed = 1337)
	{
		m_seed = seed;
		CalculateFractalBounding();
	}

	public static float GetDecimalType()
	{
		return 0f;
	}

	public int GetSeed()
	{
		return m_seed;
	}

	public void SetSeed(int seed)
	{
		m_seed = seed;
	}

	public void SetFrequency(float frequency)
	{
		m_frequency = frequency;
	}

	public void SetInterp(Interp interp)
	{
		m_interp = interp;
	}

	public void SetNoiseType(NoiseType noiseType)
	{
		m_noiseType = noiseType;
	}

	public void SetFractalOctaves(int octaves)
	{
		m_octaves = octaves;
		CalculateFractalBounding();
	}

	public void SetFractalLacunarity(float lacunarity)
	{
		m_lacunarity = lacunarity;
	}

	public void SetFractalGain(float gain)
	{
		m_gain = gain;
		CalculateFractalBounding();
	}

	public void SetFractalType(FractalType fractalType)
	{
		m_fractalType = fractalType;
	}

	public void SetCellularDistanceFunction(CellularDistanceFunction cellularDistanceFunction)
	{
		m_cellularDistanceFunction = cellularDistanceFunction;
	}

	public void SetCellularReturnType(CellularReturnType cellularReturnType)
	{
		m_cellularReturnType = cellularReturnType;
	}

	public void SetCellularDistance2Indicies(int cellularDistanceIndex0, int cellularDistanceIndex1)
	{
		m_cellularDistanceIndex0 = Math.Min(cellularDistanceIndex0, cellularDistanceIndex1);
		m_cellularDistanceIndex1 = Math.Max(cellularDistanceIndex0, cellularDistanceIndex1);
		m_cellularDistanceIndex0 = Math.Min(Math.Max(m_cellularDistanceIndex0, 0), 3);
		m_cellularDistanceIndex1 = Math.Min(Math.Max(m_cellularDistanceIndex1, 0), 3);
	}

	public void SetCellularJitter(float cellularJitter)
	{
		m_cellularJitter = cellularJitter;
	}

	public void SetCellularNoiseLookup(FastNoise noise)
	{
		m_cellularNoiseLookup = noise;
	}

	public void SetGradientPerturbAmp(float gradientPerturbAmp)
	{
		m_gradientPerturbAmp = gradientPerturbAmp;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int FastFloor(float f)
	{
		if (!(f >= 0f))
		{
			return (int)f - 1;
		}
		return (int)f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int FastRound(float f)
	{
		if (!(f >= 0f))
		{
			return (int)(f - 0.5f);
		}
		return (int)(f + 0.5f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float Lerp(float a, float b, float t)
	{
		return a + t * (b - a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float InterpHermiteFunc(float t)
	{
		return t * t * (3f - 2f * t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float InterpQuinticFunc(float t)
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float CubicLerp(float a, float b, float c, float d, float t)
	{
		float p = d - c - (a - b);
		return t * t * t * p + t * t * (a - b - p) + t * (c - a) + b;
	}

	private void CalculateFractalBounding()
	{
		float amp = m_gain;
		float ampFractal = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			ampFractal += amp;
			amp *= m_gain;
		}
		m_fractalBounding = 1f / ampFractal;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Hash2D(int seed, int x, int y)
	{
		int hash = seed;
		hash ^= 1619 * x;
		hash ^= 31337 * y;
		hash = hash * hash * hash * 60493;
		return (hash >> 13) ^ hash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Hash3D(int seed, int x, int y, int z)
	{
		int hash = seed;
		hash ^= 1619 * x;
		hash ^= 31337 * y;
		hash ^= 6971 * z;
		hash = hash * hash * hash * 60493;
		return (hash >> 13) ^ hash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Hash4D(int seed, int x, int y, int z, int w)
	{
		int hash = seed;
		hash ^= 1619 * x;
		hash ^= 31337 * y;
		hash ^= 6971 * z;
		hash ^= 1013 * w;
		hash = hash * hash * hash * 60493;
		return (hash >> 13) ^ hash;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float ValCoord2D(int seed, int x, int y)
	{
		int n = seed;
		n ^= 1619 * x;
		n ^= 31337 * y;
		return (float)(n * n * n * 60493) / 2.1474836E+09f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float ValCoord3D(int seed, int x, int y, int z)
	{
		int n = seed;
		n ^= 1619 * x;
		n ^= 31337 * y;
		n ^= 6971 * z;
		return (float)(n * n * n * 60493) / 2.1474836E+09f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float ValCoord4D(int seed, int x, int y, int z, int w)
	{
		int n = seed;
		n ^= 1619 * x;
		n ^= 31337 * y;
		n ^= 6971 * z;
		n ^= 1013 * w;
		return (float)(n * n * n * 60493) / 2.1474836E+09f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GradCoord2D(int seed, int x, int y, float xd, float yd)
	{
		int hash = seed;
		hash ^= 1619 * x;
		hash ^= 31337 * y;
		hash = hash * hash * hash * 60493;
		hash = (hash >> 13) ^ hash;
		Float2 g = GRAD_2D[hash & 7];
		return xd * g.x + yd * g.y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GradCoord3D(int seed, int x, int y, int z, float xd, float yd, float zd)
	{
		int hash = seed;
		hash ^= 1619 * x;
		hash ^= 31337 * y;
		hash ^= 6971 * z;
		hash = hash * hash * hash * 60493;
		hash = (hash >> 13) ^ hash;
		Float3 g = GRAD_3D[hash & 0xF];
		return xd * g.x + yd * g.y + zd * g.z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GradCoord4D(int seed, int x, int y, int z, int w, float xd, float yd, float zd, float wd)
	{
		int hash = seed;
		hash ^= 1619 * x;
		hash ^= 31337 * y;
		hash ^= 6971 * z;
		hash ^= 1013 * w;
		hash = hash * hash * hash * 60493;
		hash = (hash >> 13) ^ hash;
		hash &= 0x1F;
		float a = yd;
		float b = zd;
		float c = wd;
		switch (hash >> 3)
		{
		case 1:
			a = wd;
			b = xd;
			c = yd;
			break;
		case 2:
			a = zd;
			b = wd;
			c = xd;
			break;
		case 3:
			a = yd;
			b = zd;
			c = wd;
			break;
		}
		return (((hash & 4) == 0) ? (0f - a) : a) + (((hash & 2) == 0) ? (0f - b) : b) + (((hash & 1) == 0) ? (0f - c) : c);
	}

	public float GetNoise(float x, float y, float z)
	{
		x *= m_frequency;
		y *= m_frequency;
		z *= m_frequency;
		switch (m_noiseType)
		{
		case NoiseType.Value:
			return SingleValue(m_seed, x, y, z);
		case NoiseType.ValueFractal:
			return m_fractalType switch
			{
				FractalType.FBM => SingleValueFractalFBM(x, y, z), 
				FractalType.Billow => SingleValueFractalBillow(x, y, z), 
				FractalType.RigidMulti => SingleValueFractalRigidMulti(x, y, z), 
				_ => 0f, 
			};
		case NoiseType.Perlin:
			return SinglePerlin(m_seed, x, y, z);
		case NoiseType.PerlinFractal:
			return m_fractalType switch
			{
				FractalType.FBM => SinglePerlinFractalFBM(x, y, z), 
				FractalType.Billow => SinglePerlinFractalBillow(x, y, z), 
				FractalType.RigidMulti => SinglePerlinFractalRigidMulti(x, y, z), 
				_ => 0f, 
			};
		case NoiseType.Simplex:
			return SingleSimplex(m_seed, x, y, z);
		case NoiseType.SimplexFractal:
			return m_fractalType switch
			{
				FractalType.FBM => SingleSimplexFractalFBM(x, y, z), 
				FractalType.Billow => SingleSimplexFractalBillow(x, y, z), 
				FractalType.RigidMulti => SingleSimplexFractalRigidMulti(x, y, z), 
				_ => 0f, 
			};
		case NoiseType.Cellular:
		{
			CellularReturnType cellularReturnType = m_cellularReturnType;
			if ((uint)cellularReturnType <= 2u)
			{
				return SingleCellular(x, y, z);
			}
			return SingleCellular2Edge(x, y, z);
		}
		case NoiseType.WhiteNoise:
			return GetWhiteNoise(x, y, z);
		case NoiseType.Cubic:
			return SingleCubic(m_seed, x, y, z);
		case NoiseType.CubicFractal:
			return m_fractalType switch
			{
				FractalType.FBM => SingleCubicFractalFBM(x, y, z), 
				FractalType.Billow => SingleCubicFractalBillow(x, y, z), 
				FractalType.RigidMulti => SingleCubicFractalRigidMulti(x, y, z), 
				_ => 0f, 
			};
		default:
			return 0f;
		}
	}

	public float GetNoise(float x, float y)
	{
		x *= m_frequency;
		y *= m_frequency;
		switch (m_noiseType)
		{
		case NoiseType.Value:
			return SingleValue(m_seed, x, y);
		case NoiseType.ValueFractal:
			return m_fractalType switch
			{
				FractalType.FBM => SingleValueFractalFBM(x, y), 
				FractalType.Billow => SingleValueFractalBillow(x, y), 
				FractalType.RigidMulti => SingleValueFractalRigidMulti(x, y), 
				_ => 0f, 
			};
		case NoiseType.Perlin:
			return SinglePerlin(m_seed, x, y);
		case NoiseType.PerlinFractal:
			return m_fractalType switch
			{
				FractalType.FBM => SinglePerlinFractalFBM(x, y), 
				FractalType.Billow => SinglePerlinFractalBillow(x, y), 
				FractalType.RigidMulti => SinglePerlinFractalRigidMulti(x, y), 
				_ => 0f, 
			};
		case NoiseType.Simplex:
			return SingleSimplex(m_seed, x, y);
		case NoiseType.SimplexFractal:
			return m_fractalType switch
			{
				FractalType.FBM => SingleSimplexFractalFBM(x, y), 
				FractalType.Billow => SingleSimplexFractalBillow(x, y), 
				FractalType.RigidMulti => SingleSimplexFractalRigidMulti(x, y), 
				_ => 0f, 
			};
		case NoiseType.Cellular:
		{
			CellularReturnType cellularReturnType = m_cellularReturnType;
			if ((uint)cellularReturnType <= 2u)
			{
				return SingleCellular(x, y);
			}
			return SingleCellular2Edge(x, y);
		}
		case NoiseType.WhiteNoise:
			return GetWhiteNoise(x, y);
		case NoiseType.Cubic:
			return SingleCubic(m_seed, x, y);
		case NoiseType.CubicFractal:
			return m_fractalType switch
			{
				FractalType.FBM => SingleCubicFractalFBM(x, y), 
				FractalType.Billow => SingleCubicFractalBillow(x, y), 
				FractalType.RigidMulti => SingleCubicFractalRigidMulti(x, y), 
				_ => 0f, 
			};
		default:
			return 0f;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int FloatCast2Int(float f)
	{
		long num = BitConverter.DoubleToInt64Bits(f);
		return (int)(num ^ (num >> 32));
	}

	public float GetWhiteNoise(float x, float y, float z, float w)
	{
		int xi = FloatCast2Int(x);
		int yi = FloatCast2Int(y);
		int zi = FloatCast2Int(z);
		int wi = FloatCast2Int(w);
		return ValCoord4D(m_seed, xi, yi, zi, wi);
	}

	public float GetWhiteNoise(float x, float y, float z)
	{
		int xi = FloatCast2Int(x);
		int yi = FloatCast2Int(y);
		int zi = FloatCast2Int(z);
		return ValCoord3D(m_seed, xi, yi, zi);
	}

	public float GetWhiteNoise(float x, float y)
	{
		int xi = FloatCast2Int(x);
		int yi = FloatCast2Int(y);
		return ValCoord2D(m_seed, xi, yi);
	}

	public float GetWhiteNoiseInt(int x, int y, int z, int w)
	{
		return ValCoord4D(m_seed, x, y, z, w);
	}

	public float GetWhiteNoiseInt(int x, int y, int z)
	{
		return ValCoord3D(m_seed, x, y, z);
	}

	public float GetWhiteNoiseInt(int x, int y)
	{
		return ValCoord2D(m_seed, x, y);
	}

	public float GetValueFractal(float x, float y, float z)
	{
		x *= m_frequency;
		y *= m_frequency;
		z *= m_frequency;
		return m_fractalType switch
		{
			FractalType.FBM => SingleValueFractalFBM(x, y, z), 
			FractalType.Billow => SingleValueFractalBillow(x, y, z), 
			FractalType.RigidMulti => SingleValueFractalRigidMulti(x, y, z), 
			_ => 0f, 
		};
	}

	private float SingleValueFractalFBM(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = SingleValue(seed, x, y, z);
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum += SingleValue(++seed, x, y, z) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleValueFractalBillow(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = Math.Abs(SingleValue(seed, x, y, z)) * 2f - 1f;
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum += (Math.Abs(SingleValue(++seed, x, y, z)) * 2f - 1f) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleValueFractalRigidMulti(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = 1f - Math.Abs(SingleValue(seed, x, y, z));
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum -= (1f - Math.Abs(SingleValue(++seed, x, y, z))) * amp;
		}
		return sum;
	}

	public float GetValue(float x, float y, float z)
	{
		return SingleValue(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
	}

	private float SingleValue(int seed, float x, float y, float z)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		int z2 = FastFloor(z);
		int x3 = x2 + 1;
		int y3 = y2 + 1;
		int z3 = z2 + 1;
		float xs;
		float ys;
		float zs;
		switch (m_interp)
		{
		default:
			xs = x - (float)x2;
			ys = y - (float)y2;
			zs = z - (float)z2;
			break;
		case Interp.Hermite:
			xs = InterpHermiteFunc(x - (float)x2);
			ys = InterpHermiteFunc(y - (float)y2);
			zs = InterpHermiteFunc(z - (float)z2);
			break;
		case Interp.Quintic:
			xs = InterpQuinticFunc(x - (float)x2);
			ys = InterpQuinticFunc(y - (float)y2);
			zs = InterpQuinticFunc(z - (float)z2);
			break;
		}
		float a = Lerp(ValCoord3D(seed, x2, y2, z2), ValCoord3D(seed, x3, y2, z2), xs);
		float xf10 = Lerp(ValCoord3D(seed, x2, y3, z2), ValCoord3D(seed, x3, y3, z2), xs);
		float xf11 = Lerp(ValCoord3D(seed, x2, y2, z3), ValCoord3D(seed, x3, y2, z3), xs);
		float xf12 = Lerp(ValCoord3D(seed, x2, y3, z3), ValCoord3D(seed, x3, y3, z3), xs);
		float a2 = Lerp(a, xf10, ys);
		float yf1 = Lerp(xf11, xf12, ys);
		return Lerp(a2, yf1, zs);
	}

	public float GetValueFractal(float x, float y)
	{
		x *= m_frequency;
		y *= m_frequency;
		return m_fractalType switch
		{
			FractalType.FBM => SingleValueFractalFBM(x, y), 
			FractalType.Billow => SingleValueFractalBillow(x, y), 
			FractalType.RigidMulti => SingleValueFractalRigidMulti(x, y), 
			_ => 0f, 
		};
	}

	private float SingleValueFractalFBM(float x, float y)
	{
		int seed = m_seed;
		float sum = SingleValue(seed, x, y);
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum += SingleValue(++seed, x, y) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleValueFractalBillow(float x, float y)
	{
		int seed = m_seed;
		float sum = Math.Abs(SingleValue(seed, x, y)) * 2f - 1f;
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum += (Math.Abs(SingleValue(++seed, x, y)) * 2f - 1f) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleValueFractalRigidMulti(float x, float y)
	{
		int seed = m_seed;
		float sum = 1f - Math.Abs(SingleValue(seed, x, y));
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum -= (1f - Math.Abs(SingleValue(++seed, x, y))) * amp;
		}
		return sum;
	}

	public float GetValue(float x, float y)
	{
		return SingleValue(m_seed, x * m_frequency, y * m_frequency);
	}

	private float SingleValue(int seed, float x, float y)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		int x3 = x2 + 1;
		int y3 = y2 + 1;
		float xs;
		float ys;
		switch (m_interp)
		{
		default:
			xs = x - (float)x2;
			ys = y - (float)y2;
			break;
		case Interp.Hermite:
			xs = InterpHermiteFunc(x - (float)x2);
			ys = InterpHermiteFunc(y - (float)y2);
			break;
		case Interp.Quintic:
			xs = InterpQuinticFunc(x - (float)x2);
			ys = InterpQuinticFunc(y - (float)y2);
			break;
		}
		float a = Lerp(ValCoord2D(seed, x2, y2), ValCoord2D(seed, x3, y2), xs);
		float xf1 = Lerp(ValCoord2D(seed, x2, y3), ValCoord2D(seed, x3, y3), xs);
		return Lerp(a, xf1, ys);
	}

	public float GetPerlinFractal(float x, float y, float z)
	{
		x *= m_frequency;
		y *= m_frequency;
		z *= m_frequency;
		return m_fractalType switch
		{
			FractalType.FBM => SinglePerlinFractalFBM(x, y, z), 
			FractalType.Billow => SinglePerlinFractalBillow(x, y, z), 
			FractalType.RigidMulti => SinglePerlinFractalRigidMulti(x, y, z), 
			_ => 0f, 
		};
	}

	private float SinglePerlinFractalFBM(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = SinglePerlin(seed, x, y, z);
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum += SinglePerlin(++seed, x, y, z) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SinglePerlinFractalBillow(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = Math.Abs(SinglePerlin(seed, x, y, z)) * 2f - 1f;
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum += (Math.Abs(SinglePerlin(++seed, x, y, z)) * 2f - 1f) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SinglePerlinFractalRigidMulti(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = 1f - Math.Abs(SinglePerlin(seed, x, y, z));
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum -= (1f - Math.Abs(SinglePerlin(++seed, x, y, z))) * amp;
		}
		return sum;
	}

	public float GetPerlin(float x, float y, float z)
	{
		return SinglePerlin(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
	}

	private float SinglePerlin(int seed, float x, float y, float z)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		int z2 = FastFloor(z);
		int x3 = x2 + 1;
		int y3 = y2 + 1;
		int z3 = z2 + 1;
		float xs;
		float ys;
		float zs;
		switch (m_interp)
		{
		default:
			xs = x - (float)x2;
			ys = y - (float)y2;
			zs = z - (float)z2;
			break;
		case Interp.Hermite:
			xs = InterpHermiteFunc(x - (float)x2);
			ys = InterpHermiteFunc(y - (float)y2);
			zs = InterpHermiteFunc(z - (float)z2);
			break;
		case Interp.Quintic:
			xs = InterpQuinticFunc(x - (float)x2);
			ys = InterpQuinticFunc(y - (float)y2);
			zs = InterpQuinticFunc(z - (float)z2);
			break;
		}
		float xd0 = x - (float)x2;
		float yd0 = y - (float)y2;
		float zd0 = z - (float)z2;
		float xd1 = xd0 - 1f;
		float yd1 = yd0 - 1f;
		float zd1 = zd0 - 1f;
		float a = Lerp(GradCoord3D(seed, x2, y2, z2, xd0, yd0, zd0), GradCoord3D(seed, x3, y2, z2, xd1, yd0, zd0), xs);
		float xf10 = Lerp(GradCoord3D(seed, x2, y3, z2, xd0, yd1, zd0), GradCoord3D(seed, x3, y3, z2, xd1, yd1, zd0), xs);
		float xf11 = Lerp(GradCoord3D(seed, x2, y2, z3, xd0, yd0, zd1), GradCoord3D(seed, x3, y2, z3, xd1, yd0, zd1), xs);
		float xf12 = Lerp(GradCoord3D(seed, x2, y3, z3, xd0, yd1, zd1), GradCoord3D(seed, x3, y3, z3, xd1, yd1, zd1), xs);
		float a2 = Lerp(a, xf10, ys);
		float yf1 = Lerp(xf11, xf12, ys);
		return Lerp(a2, yf1, zs);
	}

	public float GetPerlinFractal(float x, float y)
	{
		x *= m_frequency;
		y *= m_frequency;
		return m_fractalType switch
		{
			FractalType.FBM => SinglePerlinFractalFBM(x, y), 
			FractalType.Billow => SinglePerlinFractalBillow(x, y), 
			FractalType.RigidMulti => SinglePerlinFractalRigidMulti(x, y), 
			_ => 0f, 
		};
	}

	private float SinglePerlinFractalFBM(float x, float y)
	{
		int seed = m_seed;
		float sum = SinglePerlin(seed, x, y);
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum += SinglePerlin(++seed, x, y) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SinglePerlinFractalBillow(float x, float y)
	{
		int seed = m_seed;
		float sum = Math.Abs(SinglePerlin(seed, x, y)) * 2f - 1f;
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum += (Math.Abs(SinglePerlin(++seed, x, y)) * 2f - 1f) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SinglePerlinFractalRigidMulti(float x, float y)
	{
		int seed = m_seed;
		float sum = 1f - Math.Abs(SinglePerlin(seed, x, y));
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum -= (1f - Math.Abs(SinglePerlin(++seed, x, y))) * amp;
		}
		return sum;
	}

	public float GetPerlin(float x, float y)
	{
		return SinglePerlin(m_seed, x * m_frequency, y * m_frequency);
	}

	private float SinglePerlin(int seed, float x, float y)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		int x3 = x2 + 1;
		int y3 = y2 + 1;
		float xs;
		float ys;
		switch (m_interp)
		{
		default:
			xs = x - (float)x2;
			ys = y - (float)y2;
			break;
		case Interp.Hermite:
			xs = InterpHermiteFunc(x - (float)x2);
			ys = InterpHermiteFunc(y - (float)y2);
			break;
		case Interp.Quintic:
			xs = InterpQuinticFunc(x - (float)x2);
			ys = InterpQuinticFunc(y - (float)y2);
			break;
		}
		float xd0 = x - (float)x2;
		float yd0 = y - (float)y2;
		float xd1 = xd0 - 1f;
		float yd1 = yd0 - 1f;
		float a = Lerp(GradCoord2D(seed, x2, y2, xd0, yd0), GradCoord2D(seed, x3, y2, xd1, yd0), xs);
		float xf1 = Lerp(GradCoord2D(seed, x2, y3, xd0, yd1), GradCoord2D(seed, x3, y3, xd1, yd1), xs);
		return Lerp(a, xf1, ys);
	}

	public float GetSimplexFractal(float x, float y, float z)
	{
		x *= m_frequency;
		y *= m_frequency;
		z *= m_frequency;
		return m_fractalType switch
		{
			FractalType.FBM => SingleSimplexFractalFBM(x, y, z), 
			FractalType.Billow => SingleSimplexFractalBillow(x, y, z), 
			FractalType.RigidMulti => SingleSimplexFractalRigidMulti(x, y, z), 
			_ => 0f, 
		};
	}

	private float SingleSimplexFractalFBM(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = SingleSimplex(seed, x, y, z);
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum += SingleSimplex(++seed, x, y, z) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleSimplexFractalBillow(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = Math.Abs(SingleSimplex(seed, x, y, z)) * 2f - 1f;
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum += (Math.Abs(SingleSimplex(++seed, x, y, z)) * 2f - 1f) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleSimplexFractalRigidMulti(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = 1f - Math.Abs(SingleSimplex(seed, x, y, z));
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum -= (1f - Math.Abs(SingleSimplex(++seed, x, y, z))) * amp;
		}
		return sum;
	}

	public float GetSimplex(float x, float y, float z)
	{
		return SingleSimplex(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
	}

	private float SingleSimplex(int seed, float x, float y, float z)
	{
		float t = (x + y + z) * (1f / 3f);
		int i = FastFloor(x + t);
		int j = FastFloor(y + t);
		int k = FastFloor(z + t);
		t = (float)(i + j + k) * (1f / 6f);
		float x2 = x - ((float)i - t);
		float y2 = y - ((float)j - t);
		float z2 = z - ((float)k - t);
		int i2;
		int j2;
		int k2;
		int i3;
		int j3;
		int k3;
		if (x2 >= y2)
		{
			if (y2 >= z2)
			{
				i2 = 1;
				j2 = 0;
				k2 = 0;
				i3 = 1;
				j3 = 1;
				k3 = 0;
			}
			else if (x2 >= z2)
			{
				i2 = 1;
				j2 = 0;
				k2 = 0;
				i3 = 1;
				j3 = 0;
				k3 = 1;
			}
			else
			{
				i2 = 0;
				j2 = 0;
				k2 = 1;
				i3 = 1;
				j3 = 0;
				k3 = 1;
			}
		}
		else if (y2 < z2)
		{
			i2 = 0;
			j2 = 0;
			k2 = 1;
			i3 = 0;
			j3 = 1;
			k3 = 1;
		}
		else if (x2 < z2)
		{
			i2 = 0;
			j2 = 1;
			k2 = 0;
			i3 = 0;
			j3 = 1;
			k3 = 1;
		}
		else
		{
			i2 = 0;
			j2 = 1;
			k2 = 0;
			i3 = 1;
			j3 = 1;
			k3 = 0;
		}
		float x3 = x2 - (float)i2 + 1f / 6f;
		float y3 = y2 - (float)j2 + 1f / 6f;
		float z3 = z2 - (float)k2 + 1f / 6f;
		float x4 = x2 - (float)i3 + 1f / 3f;
		float y4 = y2 - (float)j3 + 1f / 3f;
		float z4 = z2 - (float)k3 + 1f / 3f;
		float x5 = x2 + -0.5f;
		float y5 = y2 + -0.5f;
		float z5 = z2 + -0.5f;
		t = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
		float n0;
		if (t < 0f)
		{
			n0 = 0f;
		}
		else
		{
			t *= t;
			n0 = t * t * GradCoord3D(seed, i, j, k, x2, y2, z2);
		}
		t = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
		float n1;
		if (t < 0f)
		{
			n1 = 0f;
		}
		else
		{
			t *= t;
			n1 = t * t * GradCoord3D(seed, i + i2, j + j2, k + k2, x3, y3, z3);
		}
		t = 0.6f - x4 * x4 - y4 * y4 - z4 * z4;
		float n2;
		if (t < 0f)
		{
			n2 = 0f;
		}
		else
		{
			t *= t;
			n2 = t * t * GradCoord3D(seed, i + i3, j + j3, k + k3, x4, y4, z4);
		}
		t = 0.6f - x5 * x5 - y5 * y5 - z5 * z5;
		float n3;
		if (t < 0f)
		{
			n3 = 0f;
		}
		else
		{
			t *= t;
			n3 = t * t * GradCoord3D(seed, i + 1, j + 1, k + 1, x5, y5, z5);
		}
		return 32f * (n0 + n1 + n2 + n3);
	}

	public float GetSimplexFractal(float x, float y)
	{
		x *= m_frequency;
		y *= m_frequency;
		return m_fractalType switch
		{
			FractalType.FBM => SingleSimplexFractalFBM(x, y), 
			FractalType.Billow => SingleSimplexFractalBillow(x, y), 
			FractalType.RigidMulti => SingleSimplexFractalRigidMulti(x, y), 
			_ => 0f, 
		};
	}

	private float SingleSimplexFractalFBM(float x, float y)
	{
		int seed = m_seed;
		float sum = SingleSimplex(seed, x, y);
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum += SingleSimplex(++seed, x, y) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleSimplexFractalBillow(float x, float y)
	{
		int seed = m_seed;
		float sum = Math.Abs(SingleSimplex(seed, x, y)) * 2f - 1f;
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum += (Math.Abs(SingleSimplex(++seed, x, y)) * 2f - 1f) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleSimplexFractalRigidMulti(float x, float y)
	{
		int seed = m_seed;
		float sum = 1f - Math.Abs(SingleSimplex(seed, x, y));
		float amp = 1f;
		for (int i = 1; i < m_octaves; i++)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum -= (1f - Math.Abs(SingleSimplex(++seed, x, y))) * amp;
		}
		return sum;
	}

	public float GetSimplex(float x, float y)
	{
		return SingleSimplex(m_seed, x * m_frequency, y * m_frequency);
	}

	private float SingleSimplex(int seed, float x, float y)
	{
		float t = (x + y) * 0.3660254f;
		int i = FastFloor(x + t);
		int j = FastFloor(y + t);
		t = (float)(i + j) * 0.21132487f;
		float X0 = (float)i - t;
		float Y0 = (float)j - t;
		float x2 = x - X0;
		float y2 = y - Y0;
		int i2;
		int j2;
		if (x2 > y2)
		{
			i2 = 1;
			j2 = 0;
		}
		else
		{
			i2 = 0;
			j2 = 1;
		}
		float x3 = x2 - (float)i2 + 0.21132487f;
		float y3 = y2 - (float)j2 + 0.21132487f;
		float x4 = x2 - 1f + 0.42264974f;
		float y4 = y2 - 1f + 0.42264974f;
		t = 0.5f - x2 * x2 - y2 * y2;
		float n0;
		if (t < 0f)
		{
			n0 = 0f;
		}
		else
		{
			t *= t;
			n0 = t * t * GradCoord2D(seed, i, j, x2, y2);
		}
		t = 0.5f - x3 * x3 - y3 * y3;
		float n1;
		if (t < 0f)
		{
			n1 = 0f;
		}
		else
		{
			t *= t;
			n1 = t * t * GradCoord2D(seed, i + i2, j + j2, x3, y3);
		}
		t = 0.5f - x4 * x4 - y4 * y4;
		float n2;
		if (t < 0f)
		{
			n2 = 0f;
		}
		else
		{
			t *= t;
			n2 = t * t * GradCoord2D(seed, i + 1, j + 1, x4, y4);
		}
		return 50f * (n0 + n1 + n2);
	}

	public float GetSimplex(float x, float y, float z, float w)
	{
		return SingleSimplex(m_seed, x * m_frequency, y * m_frequency, z * m_frequency, w * m_frequency);
	}

	private float SingleSimplex(int seed, float x, float y, float z, float w)
	{
		float t = (x + y + z + w) * 0.309017f;
		int i = FastFloor(x + t);
		int j = FastFloor(y + t);
		int k = FastFloor(z + t);
		int l = FastFloor(w + t);
		t = (float)(i + j + k + l) * 0.1381966f;
		float X0 = (float)i - t;
		float Y0 = (float)j - t;
		float Z0 = (float)k - t;
		float W0 = (float)l - t;
		float x2 = x - X0;
		float y2 = y - Y0;
		float z2 = z - Z0;
		float w2 = w - W0;
		int c = ((x2 > y2) ? 32 : 0);
		c += ((x2 > z2) ? 16 : 0);
		c += ((y2 > z2) ? 8 : 0);
		c += ((x2 > w2) ? 4 : 0);
		c += ((y2 > w2) ? 2 : 0);
		c += ((z2 > w2) ? 1 : 0);
		c <<= 2;
		int i2 = ((SIMPLEX_4D[c] >= 3) ? 1 : 0);
		int i3 = ((SIMPLEX_4D[c] >= 2) ? 1 : 0);
		int i4 = ((SIMPLEX_4D[c++] >= 1) ? 1 : 0);
		int j2 = ((SIMPLEX_4D[c] >= 3) ? 1 : 0);
		int j3 = ((SIMPLEX_4D[c] >= 2) ? 1 : 0);
		int j4 = ((SIMPLEX_4D[c++] >= 1) ? 1 : 0);
		int k2 = ((SIMPLEX_4D[c] >= 3) ? 1 : 0);
		int k3 = ((SIMPLEX_4D[c] >= 2) ? 1 : 0);
		int k4 = ((SIMPLEX_4D[c++] >= 1) ? 1 : 0);
		int l2 = ((SIMPLEX_4D[c] >= 3) ? 1 : 0);
		int l3 = ((SIMPLEX_4D[c] >= 2) ? 1 : 0);
		int l4 = ((SIMPLEX_4D[c] >= 1) ? 1 : 0);
		float x3 = x2 - (float)i2 + 0.1381966f;
		float y3 = y2 - (float)j2 + 0.1381966f;
		float z3 = z2 - (float)k2 + 0.1381966f;
		float w3 = w2 - (float)l2 + 0.1381966f;
		float x4 = x2 - (float)i3 + 0.2763932f;
		float y4 = y2 - (float)j3 + 0.2763932f;
		float z4 = z2 - (float)k3 + 0.2763932f;
		float w4 = w2 - (float)l3 + 0.2763932f;
		float x5 = x2 - (float)i4 + 0.41458982f;
		float y5 = y2 - (float)j4 + 0.41458982f;
		float z5 = z2 - (float)k4 + 0.41458982f;
		float w5 = w2 - (float)l4 + 0.41458982f;
		float x6 = x2 - 1f + 0.5527864f;
		float y6 = y2 - 1f + 0.5527864f;
		float z6 = z2 - 1f + 0.5527864f;
		float w6 = w2 - 1f + 0.5527864f;
		t = 0.6f - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;
		float n0;
		if (t < 0f)
		{
			n0 = 0f;
		}
		else
		{
			t *= t;
			n0 = t * t * GradCoord4D(seed, i, j, k, l, x2, y2, z2, w2);
		}
		t = 0.6f - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;
		float n1;
		if (t < 0f)
		{
			n1 = 0f;
		}
		else
		{
			t *= t;
			n1 = t * t * GradCoord4D(seed, i + i2, j + j2, k + k2, l + l2, x3, y3, z3, w3);
		}
		t = 0.6f - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;
		float n2;
		if (t < 0f)
		{
			n2 = 0f;
		}
		else
		{
			t *= t;
			n2 = t * t * GradCoord4D(seed, i + i3, j + j3, k + k3, l + l3, x4, y4, z4, w4);
		}
		t = 0.6f - x5 * x5 - y5 * y5 - z5 * z5 - w5 * w5;
		float n3;
		if (t < 0f)
		{
			n3 = 0f;
		}
		else
		{
			t *= t;
			n3 = t * t * GradCoord4D(seed, i + i4, j + j4, k + k4, l + l4, x5, y5, z5, w5);
		}
		t = 0.6f - x6 * x6 - y6 * y6 - z6 * z6 - w6 * w6;
		float n4;
		if (t < 0f)
		{
			n4 = 0f;
		}
		else
		{
			t *= t;
			n4 = t * t * GradCoord4D(seed, i + 1, j + 1, k + 1, l + 1, x6, y6, z6, w6);
		}
		return 27f * (n0 + n1 + n2 + n3 + n4);
	}

	public float GetCubicFractal(float x, float y, float z)
	{
		x *= m_frequency;
		y *= m_frequency;
		z *= m_frequency;
		return m_fractalType switch
		{
			FractalType.FBM => SingleCubicFractalFBM(x, y, z), 
			FractalType.Billow => SingleCubicFractalBillow(x, y, z), 
			FractalType.RigidMulti => SingleCubicFractalRigidMulti(x, y, z), 
			_ => 0f, 
		};
	}

	private float SingleCubicFractalFBM(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = SingleCubic(seed, x, y, z);
		float amp = 1f;
		int i = 0;
		while (++i < m_octaves)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum += SingleCubic(++seed, x, y, z) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleCubicFractalBillow(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = Math.Abs(SingleCubic(seed, x, y, z)) * 2f - 1f;
		float amp = 1f;
		int i = 0;
		while (++i < m_octaves)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum += (Math.Abs(SingleCubic(++seed, x, y, z)) * 2f - 1f) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleCubicFractalRigidMulti(float x, float y, float z)
	{
		int seed = m_seed;
		float sum = 1f - Math.Abs(SingleCubic(seed, x, y, z));
		float amp = 1f;
		int i = 0;
		while (++i < m_octaves)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			z *= m_lacunarity;
			amp *= m_gain;
			sum -= (1f - Math.Abs(SingleCubic(++seed, x, y, z))) * amp;
		}
		return sum;
	}

	public float GetCubic(float x, float y, float z)
	{
		return SingleCubic(m_seed, x * m_frequency, y * m_frequency, z * m_frequency);
	}

	private float SingleCubic(int seed, float x, float y, float z)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		int z2 = FastFloor(z);
		int x3 = x2 - 1;
		int y3 = y2 - 1;
		int z3 = z2 - 1;
		int x4 = x2 + 1;
		int y4 = y2 + 1;
		int z4 = z2 + 1;
		int x5 = x2 + 2;
		int y5 = y2 + 2;
		int z5 = z2 + 2;
		float xs = x - (float)x2;
		float ys = y - (float)y2;
		float zs = z - (float)z2;
		return CubicLerp(CubicLerp(CubicLerp(ValCoord3D(seed, x3, y3, z3), ValCoord3D(seed, x2, y3, z3), ValCoord3D(seed, x4, y3, z3), ValCoord3D(seed, x5, y3, z3), xs), CubicLerp(ValCoord3D(seed, x3, y2, z3), ValCoord3D(seed, x2, y2, z3), ValCoord3D(seed, x4, y2, z3), ValCoord3D(seed, x5, y2, z3), xs), CubicLerp(ValCoord3D(seed, x3, y4, z3), ValCoord3D(seed, x2, y4, z3), ValCoord3D(seed, x4, y4, z3), ValCoord3D(seed, x5, y4, z3), xs), CubicLerp(ValCoord3D(seed, x3, y5, z3), ValCoord3D(seed, x2, y5, z3), ValCoord3D(seed, x4, y5, z3), ValCoord3D(seed, x5, y5, z3), xs), ys), CubicLerp(CubicLerp(ValCoord3D(seed, x3, y3, z2), ValCoord3D(seed, x2, y3, z2), ValCoord3D(seed, x4, y3, z2), ValCoord3D(seed, x5, y3, z2), xs), CubicLerp(ValCoord3D(seed, x3, y2, z2), ValCoord3D(seed, x2, y2, z2), ValCoord3D(seed, x4, y2, z2), ValCoord3D(seed, x5, y2, z2), xs), CubicLerp(ValCoord3D(seed, x3, y4, z2), ValCoord3D(seed, x2, y4, z2), ValCoord3D(seed, x4, y4, z2), ValCoord3D(seed, x5, y4, z2), xs), CubicLerp(ValCoord3D(seed, x3, y5, z2), ValCoord3D(seed, x2, y5, z2), ValCoord3D(seed, x4, y5, z2), ValCoord3D(seed, x5, y5, z2), xs), ys), CubicLerp(CubicLerp(ValCoord3D(seed, x3, y3, z4), ValCoord3D(seed, x2, y3, z4), ValCoord3D(seed, x4, y3, z4), ValCoord3D(seed, x5, y3, z4), xs), CubicLerp(ValCoord3D(seed, x3, y2, z4), ValCoord3D(seed, x2, y2, z4), ValCoord3D(seed, x4, y2, z4), ValCoord3D(seed, x5, y2, z4), xs), CubicLerp(ValCoord3D(seed, x3, y4, z4), ValCoord3D(seed, x2, y4, z4), ValCoord3D(seed, x4, y4, z4), ValCoord3D(seed, x5, y4, z4), xs), CubicLerp(ValCoord3D(seed, x3, y5, z4), ValCoord3D(seed, x2, y5, z4), ValCoord3D(seed, x4, y5, z4), ValCoord3D(seed, x5, y5, z4), xs), ys), CubicLerp(CubicLerp(ValCoord3D(seed, x3, y3, z5), ValCoord3D(seed, x2, y3, z5), ValCoord3D(seed, x4, y3, z5), ValCoord3D(seed, x5, y3, z5), xs), CubicLerp(ValCoord3D(seed, x3, y2, z5), ValCoord3D(seed, x2, y2, z5), ValCoord3D(seed, x4, y2, z5), ValCoord3D(seed, x5, y2, z5), xs), CubicLerp(ValCoord3D(seed, x3, y4, z5), ValCoord3D(seed, x2, y4, z5), ValCoord3D(seed, x4, y4, z5), ValCoord3D(seed, x5, y4, z5), xs), CubicLerp(ValCoord3D(seed, x3, y5, z5), ValCoord3D(seed, x2, y5, z5), ValCoord3D(seed, x4, y5, z5), ValCoord3D(seed, x5, y5, z5), xs), ys), zs) * (8f / 27f);
	}

	public float GetCubicFractal(float x, float y)
	{
		x *= m_frequency;
		y *= m_frequency;
		return m_fractalType switch
		{
			FractalType.FBM => SingleCubicFractalFBM(x, y), 
			FractalType.Billow => SingleCubicFractalBillow(x, y), 
			FractalType.RigidMulti => SingleCubicFractalRigidMulti(x, y), 
			_ => 0f, 
		};
	}

	private float SingleCubicFractalFBM(float x, float y)
	{
		int seed = m_seed;
		float sum = SingleCubic(seed, x, y);
		float amp = 1f;
		int i = 0;
		while (++i < m_octaves)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum += SingleCubic(++seed, x, y) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleCubicFractalBillow(float x, float y)
	{
		int seed = m_seed;
		float sum = Math.Abs(SingleCubic(seed, x, y)) * 2f - 1f;
		float amp = 1f;
		int i = 0;
		while (++i < m_octaves)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum += (Math.Abs(SingleCubic(++seed, x, y)) * 2f - 1f) * amp;
		}
		return sum * m_fractalBounding;
	}

	private float SingleCubicFractalRigidMulti(float x, float y)
	{
		int seed = m_seed;
		float sum = 1f - Math.Abs(SingleCubic(seed, x, y));
		float amp = 1f;
		int i = 0;
		while (++i < m_octaves)
		{
			x *= m_lacunarity;
			y *= m_lacunarity;
			amp *= m_gain;
			sum -= (1f - Math.Abs(SingleCubic(++seed, x, y))) * amp;
		}
		return sum;
	}

	public float GetCubic(float x, float y)
	{
		x *= m_frequency;
		y *= m_frequency;
		return SingleCubic(m_seed, x, y);
	}

	private float SingleCubic(int seed, float x, float y)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		int x3 = x2 - 1;
		int y3 = y2 - 1;
		int x4 = x2 + 1;
		int y4 = y2 + 1;
		int x5 = x2 + 2;
		int y5 = y2 + 2;
		float xs = x - (float)x2;
		float ys = y - (float)y2;
		return CubicLerp(CubicLerp(ValCoord2D(seed, x3, y3), ValCoord2D(seed, x2, y3), ValCoord2D(seed, x4, y3), ValCoord2D(seed, x5, y3), xs), CubicLerp(ValCoord2D(seed, x3, y2), ValCoord2D(seed, x2, y2), ValCoord2D(seed, x4, y2), ValCoord2D(seed, x5, y2), xs), CubicLerp(ValCoord2D(seed, x3, y4), ValCoord2D(seed, x2, y4), ValCoord2D(seed, x4, y4), ValCoord2D(seed, x5, y4), xs), CubicLerp(ValCoord2D(seed, x3, y5), ValCoord2D(seed, x2, y5), ValCoord2D(seed, x4, y5), ValCoord2D(seed, x5, y5), xs), ys) * (4f / 9f);
	}

	public float GetCellular(float x, float y, float z)
	{
		x *= m_frequency;
		y *= m_frequency;
		z *= m_frequency;
		CellularReturnType cellularReturnType = m_cellularReturnType;
		if ((uint)cellularReturnType <= 2u)
		{
			return SingleCellular(x, y, z);
		}
		return SingleCellular2Edge(x, y, z);
	}

	private float SingleCellular(float x, float y, float z)
	{
		int xr = FastRound(x);
		int yr = FastRound(y);
		int zr = FastRound(z);
		float distance = 999999f;
		int xc = 0;
		int yc = 0;
		int zc = 0;
		switch (m_cellularDistanceFunction)
		{
		case CellularDistanceFunction.Euclidean:
		{
			for (int i = xr - 1; i <= xr + 1; i++)
			{
				for (int j = yr - 1; j <= yr + 1; j++)
				{
					for (int k = zr - 1; k <= zr + 1; k++)
					{
						Float3 vec2 = CELL_3D[Hash3D(m_seed, i, j, k) & 0xFF];
						float num = (float)i - x + vec2.x * m_cellularJitter;
						float vecY2 = (float)j - y + vec2.y * m_cellularJitter;
						float vecZ2 = (float)k - z + vec2.z * m_cellularJitter;
						float newDistance2 = num * num + vecY2 * vecY2 + vecZ2 * vecZ2;
						if (newDistance2 < distance)
						{
							distance = newDistance2;
							xc = i;
							yc = j;
							zc = k;
						}
					}
				}
			}
			break;
		}
		case CellularDistanceFunction.Manhattan:
		{
			for (int l = xr - 1; l <= xr + 1; l++)
			{
				for (int m = yr - 1; m <= yr + 1; m++)
				{
					for (int n = zr - 1; n <= zr + 1; n++)
					{
						Float3 vec3 = CELL_3D[Hash3D(m_seed, l, m, n) & 0xFF];
						float value = (float)l - x + vec3.x * m_cellularJitter;
						float vecY3 = (float)m - y + vec3.y * m_cellularJitter;
						float vecZ3 = (float)n - z + vec3.z * m_cellularJitter;
						float newDistance3 = Math.Abs(value) + Math.Abs(vecY3) + Math.Abs(vecZ3);
						if (newDistance3 < distance)
						{
							distance = newDistance3;
							xc = l;
							yc = m;
							zc = n;
						}
					}
				}
			}
			break;
		}
		case CellularDistanceFunction.Natural:
		{
			for (int xi = xr - 1; xi <= xr + 1; xi++)
			{
				for (int yi = yr - 1; yi <= yr + 1; yi++)
				{
					for (int zi = zr - 1; zi <= zr + 1; zi++)
					{
						Float3 vec = CELL_3D[Hash3D(m_seed, xi, yi, zi) & 0xFF];
						float vecX = (float)xi - x + vec.x * m_cellularJitter;
						float vecY = (float)yi - y + vec.y * m_cellularJitter;
						float vecZ = (float)zi - z + vec.z * m_cellularJitter;
						float newDistance = Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ) + (vecX * vecX + vecY * vecY + vecZ * vecZ);
						if (newDistance < distance)
						{
							distance = newDistance;
							xc = xi;
							yc = yi;
							zc = zi;
						}
					}
				}
			}
			break;
		}
		}
		switch (m_cellularReturnType)
		{
		case CellularReturnType.CellValue:
			return ValCoord3D(m_seed, xc, yc, zc);
		case CellularReturnType.NoiseLookup:
		{
			Float3 vec4 = CELL_3D[Hash3D(m_seed, xc, yc, zc) & 0xFF];
			return m_cellularNoiseLookup.GetNoise((float)xc + vec4.x * m_cellularJitter, (float)yc + vec4.y * m_cellularJitter, (float)zc + vec4.z * m_cellularJitter);
		}
		case CellularReturnType.Distance:
			return distance;
		default:
			return 0f;
		}
	}

	private float SingleCellular2Edge(float x, float y, float z)
	{
		int xr = FastRound(x);
		int yr = FastRound(y);
		int zr = FastRound(z);
		float[] distance = new float[4] { 999999f, 999999f, 999999f, 999999f };
		switch (m_cellularDistanceFunction)
		{
		case CellularDistanceFunction.Euclidean:
		{
			for (int j = xr - 1; j <= xr + 1; j++)
			{
				for (int k = yr - 1; k <= yr + 1; k++)
				{
					for (int l = zr - 1; l <= zr + 1; l++)
					{
						Float3 vec2 = CELL_3D[Hash3D(m_seed, j, k, l) & 0xFF];
						float num = (float)j - x + vec2.x * m_cellularJitter;
						float vecY2 = (float)k - y + vec2.y * m_cellularJitter;
						float vecZ2 = (float)l - z + vec2.z * m_cellularJitter;
						float newDistance2 = num * num + vecY2 * vecY2 + vecZ2 * vecZ2;
						for (int i2 = m_cellularDistanceIndex1; i2 > 0; i2--)
						{
							distance[i2] = Math.Max(Math.Min(distance[i2], newDistance2), distance[i2 - 1]);
						}
						distance[0] = Math.Min(distance[0], newDistance2);
					}
				}
			}
			break;
		}
		case CellularDistanceFunction.Manhattan:
		{
			for (int m = xr - 1; m <= xr + 1; m++)
			{
				for (int n = yr - 1; n <= yr + 1; n++)
				{
					for (int num2 = zr - 1; num2 <= zr + 1; num2++)
					{
						Float3 vec3 = CELL_3D[Hash3D(m_seed, m, n, num2) & 0xFF];
						float value = (float)m - x + vec3.x * m_cellularJitter;
						float vecY3 = (float)n - y + vec3.y * m_cellularJitter;
						float vecZ3 = (float)num2 - z + vec3.z * m_cellularJitter;
						float newDistance3 = Math.Abs(value) + Math.Abs(vecY3) + Math.Abs(vecZ3);
						for (int i3 = m_cellularDistanceIndex1; i3 > 0; i3--)
						{
							distance[i3] = Math.Max(Math.Min(distance[i3], newDistance3), distance[i3 - 1]);
						}
						distance[0] = Math.Min(distance[0], newDistance3);
					}
				}
			}
			break;
		}
		case CellularDistanceFunction.Natural:
		{
			for (int xi = xr - 1; xi <= xr + 1; xi++)
			{
				for (int yi = yr - 1; yi <= yr + 1; yi++)
				{
					for (int zi = zr - 1; zi <= zr + 1; zi++)
					{
						Float3 vec = CELL_3D[Hash3D(m_seed, xi, yi, zi) & 0xFF];
						float vecX = (float)xi - x + vec.x * m_cellularJitter;
						float vecY = (float)yi - y + vec.y * m_cellularJitter;
						float vecZ = (float)zi - z + vec.z * m_cellularJitter;
						float newDistance = Math.Abs(vecX) + Math.Abs(vecY) + Math.Abs(vecZ) + (vecX * vecX + vecY * vecY + vecZ * vecZ);
						for (int i = m_cellularDistanceIndex1; i > 0; i--)
						{
							distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
						}
						distance[0] = Math.Min(distance[0], newDistance);
					}
				}
			}
			break;
		}
		}
		return m_cellularReturnType switch
		{
			CellularReturnType.Distance2 => distance[m_cellularDistanceIndex1], 
			CellularReturnType.Distance2Add => distance[m_cellularDistanceIndex1] + distance[m_cellularDistanceIndex0], 
			CellularReturnType.Distance2Sub => distance[m_cellularDistanceIndex1] - distance[m_cellularDistanceIndex0], 
			CellularReturnType.Distance2Mul => distance[m_cellularDistanceIndex1] * distance[m_cellularDistanceIndex0], 
			CellularReturnType.Distance2Div => distance[m_cellularDistanceIndex0] / distance[m_cellularDistanceIndex1], 
			_ => 0f, 
		};
	}

	public float GetCellular(float x, float y)
	{
		x *= m_frequency;
		y *= m_frequency;
		CellularReturnType cellularReturnType = m_cellularReturnType;
		if ((uint)cellularReturnType <= 2u)
		{
			return SingleCellular(x, y);
		}
		return SingleCellular2Edge(x, y);
	}

	private float SingleCellular(float x, float y)
	{
		int xr = FastRound(x);
		int yr = FastRound(y);
		float distance = 999999f;
		int xc = 0;
		int yc = 0;
		switch (m_cellularDistanceFunction)
		{
		default:
		{
			for (int i = xr - 1; i <= xr + 1; i++)
			{
				for (int j = yr - 1; j <= yr + 1; j++)
				{
					Float2 vec2 = CELL_2D[Hash2D(m_seed, i, j) & 0xFF];
					float num = (float)i - x + vec2.x * m_cellularJitter;
					float vecY2 = (float)j - y + vec2.y * m_cellularJitter;
					float newDistance2 = num * num + vecY2 * vecY2;
					if (newDistance2 < distance)
					{
						distance = newDistance2;
						xc = i;
						yc = j;
					}
				}
			}
			break;
		}
		case CellularDistanceFunction.Manhattan:
		{
			for (int k = xr - 1; k <= xr + 1; k++)
			{
				for (int l = yr - 1; l <= yr + 1; l++)
				{
					Float2 vec3 = CELL_2D[Hash2D(m_seed, k, l) & 0xFF];
					float value = (float)k - x + vec3.x * m_cellularJitter;
					float vecY3 = (float)l - y + vec3.y * m_cellularJitter;
					float newDistance3 = Math.Abs(value) + Math.Abs(vecY3);
					if (newDistance3 < distance)
					{
						distance = newDistance3;
						xc = k;
						yc = l;
					}
				}
			}
			break;
		}
		case CellularDistanceFunction.Natural:
		{
			for (int xi = xr - 1; xi <= xr + 1; xi++)
			{
				for (int yi = yr - 1; yi <= yr + 1; yi++)
				{
					Float2 vec = CELL_2D[Hash2D(m_seed, xi, yi) & 0xFF];
					float vecX = (float)xi - x + vec.x * m_cellularJitter;
					float vecY = (float)yi - y + vec.y * m_cellularJitter;
					float newDistance = Math.Abs(vecX) + Math.Abs(vecY) + (vecX * vecX + vecY * vecY);
					if (newDistance < distance)
					{
						distance = newDistance;
						xc = xi;
						yc = yi;
					}
				}
			}
			break;
		}
		}
		switch (m_cellularReturnType)
		{
		case CellularReturnType.CellValue:
			return ValCoord2D(m_seed, xc, yc);
		case CellularReturnType.NoiseLookup:
		{
			Float2 vec4 = CELL_2D[Hash2D(m_seed, xc, yc) & 0xFF];
			return m_cellularNoiseLookup.GetNoise((float)xc + vec4.x * m_cellularJitter, (float)yc + vec4.y * m_cellularJitter);
		}
		case CellularReturnType.Distance:
			return distance;
		default:
			return 0f;
		}
	}

	private float SingleCellular2Edge(float x, float y)
	{
		int xr = FastRound(x);
		int yr = FastRound(y);
		float[] distance = new float[4] { 999999f, 999999f, 999999f, 999999f };
		switch (m_cellularDistanceFunction)
		{
		default:
		{
			for (int j = xr - 1; j <= xr + 1; j++)
			{
				for (int k = yr - 1; k <= yr + 1; k++)
				{
					Float2 vec2 = CELL_2D[Hash2D(m_seed, j, k) & 0xFF];
					float num = (float)j - x + vec2.x * m_cellularJitter;
					float vecY2 = (float)k - y + vec2.y * m_cellularJitter;
					float newDistance2 = num * num + vecY2 * vecY2;
					for (int i2 = m_cellularDistanceIndex1; i2 > 0; i2--)
					{
						distance[i2] = Math.Max(Math.Min(distance[i2], newDistance2), distance[i2 - 1]);
					}
					distance[0] = Math.Min(distance[0], newDistance2);
				}
			}
			break;
		}
		case CellularDistanceFunction.Manhattan:
		{
			for (int l = xr - 1; l <= xr + 1; l++)
			{
				for (int m = yr - 1; m <= yr + 1; m++)
				{
					Float2 vec3 = CELL_2D[Hash2D(m_seed, l, m) & 0xFF];
					float value = (float)l - x + vec3.x * m_cellularJitter;
					float vecY3 = (float)m - y + vec3.y * m_cellularJitter;
					float newDistance3 = Math.Abs(value) + Math.Abs(vecY3);
					for (int i3 = m_cellularDistanceIndex1; i3 > 0; i3--)
					{
						distance[i3] = Math.Max(Math.Min(distance[i3], newDistance3), distance[i3 - 1]);
					}
					distance[0] = Math.Min(distance[0], newDistance3);
				}
			}
			break;
		}
		case CellularDistanceFunction.Natural:
		{
			for (int xi = xr - 1; xi <= xr + 1; xi++)
			{
				for (int yi = yr - 1; yi <= yr + 1; yi++)
				{
					Float2 vec = CELL_2D[Hash2D(m_seed, xi, yi) & 0xFF];
					float vecX = (float)xi - x + vec.x * m_cellularJitter;
					float vecY = (float)yi - y + vec.y * m_cellularJitter;
					float newDistance = Math.Abs(vecX) + Math.Abs(vecY) + (vecX * vecX + vecY * vecY);
					for (int i = m_cellularDistanceIndex1; i > 0; i--)
					{
						distance[i] = Math.Max(Math.Min(distance[i], newDistance), distance[i - 1]);
					}
					distance[0] = Math.Min(distance[0], newDistance);
				}
			}
			break;
		}
		}
		return m_cellularReturnType switch
		{
			CellularReturnType.Distance2 => distance[m_cellularDistanceIndex1], 
			CellularReturnType.Distance2Add => distance[m_cellularDistanceIndex1] + distance[m_cellularDistanceIndex0], 
			CellularReturnType.Distance2Sub => distance[m_cellularDistanceIndex1] - distance[m_cellularDistanceIndex0], 
			CellularReturnType.Distance2Mul => distance[m_cellularDistanceIndex1] * distance[m_cellularDistanceIndex0], 
			CellularReturnType.Distance2Div => distance[m_cellularDistanceIndex0] / distance[m_cellularDistanceIndex1], 
			_ => 0f, 
		};
	}

	public void GradientPerturb(ref float x, ref float y, ref float z)
	{
		SingleGradientPerturb(m_seed, m_gradientPerturbAmp, m_frequency, ref x, ref y, ref z);
	}

	public void GradientPerturbFractal(ref float x, ref float y, ref float z)
	{
		int seed = m_seed;
		float amp = m_gradientPerturbAmp * m_fractalBounding;
		float freq = m_frequency;
		SingleGradientPerturb(seed, amp, m_frequency, ref x, ref y, ref z);
		for (int i = 1; i < m_octaves; i++)
		{
			freq *= m_lacunarity;
			amp *= m_gain;
			SingleGradientPerturb(++seed, amp, freq, ref x, ref y, ref z);
		}
	}

	private void SingleGradientPerturb(int seed, float perturbAmp, float frequency, ref float x, ref float y, ref float z)
	{
		float xf = x * frequency;
		float yf = y * frequency;
		float zf = z * frequency;
		int x2 = FastFloor(xf);
		int y2 = FastFloor(yf);
		int z2 = FastFloor(zf);
		int x3 = x2 + 1;
		int y3 = y2 + 1;
		int z3 = z2 + 1;
		float xs;
		float ys;
		float zs;
		switch (m_interp)
		{
		default:
			xs = xf - (float)x2;
			ys = yf - (float)y2;
			zs = zf - (float)z2;
			break;
		case Interp.Hermite:
			xs = InterpHermiteFunc(xf - (float)x2);
			ys = InterpHermiteFunc(yf - (float)y2);
			zs = InterpHermiteFunc(zf - (float)z2);
			break;
		case Interp.Quintic:
			xs = InterpQuinticFunc(xf - (float)x2);
			ys = InterpQuinticFunc(yf - (float)y2);
			zs = InterpQuinticFunc(zf - (float)z2);
			break;
		}
		Float3 @float = CELL_3D[Hash3D(seed, x2, y2, z2) & 0xFF];
		Float3 vec1 = CELL_3D[Hash3D(seed, x3, y2, z2) & 0xFF];
		float lx0x = Lerp(@float.x, vec1.x, xs);
		float ly0x = Lerp(@float.y, vec1.y, xs);
		float lz0x = Lerp(@float.z, vec1.z, xs);
		Float3 float2 = CELL_3D[Hash3D(seed, x2, y3, z2) & 0xFF];
		vec1 = CELL_3D[Hash3D(seed, x3, y3, z2) & 0xFF];
		float lx1x = Lerp(float2.x, vec1.x, xs);
		float ly1x = Lerp(float2.y, vec1.y, xs);
		float lz1x = Lerp(float2.z, vec1.z, xs);
		float lx0y = Lerp(lx0x, lx1x, ys);
		float ly0y = Lerp(ly0x, ly1x, ys);
		float lz0y = Lerp(lz0x, lz1x, ys);
		Float3 float3 = CELL_3D[Hash3D(seed, x2, y2, z3) & 0xFF];
		vec1 = CELL_3D[Hash3D(seed, x3, y2, z3) & 0xFF];
		lx0x = Lerp(float3.x, vec1.x, xs);
		ly0x = Lerp(float3.y, vec1.y, xs);
		lz0x = Lerp(float3.z, vec1.z, xs);
		Float3 float4 = CELL_3D[Hash3D(seed, x2, y3, z3) & 0xFF];
		vec1 = CELL_3D[Hash3D(seed, x3, y3, z3) & 0xFF];
		lx1x = Lerp(float4.x, vec1.x, xs);
		ly1x = Lerp(float4.y, vec1.y, xs);
		lz1x = Lerp(float4.z, vec1.z, xs);
		x += Lerp(lx0y, Lerp(lx0x, lx1x, ys), zs) * perturbAmp;
		y += Lerp(ly0y, Lerp(ly0x, ly1x, ys), zs) * perturbAmp;
		z += Lerp(lz0y, Lerp(lz0x, lz1x, ys), zs) * perturbAmp;
	}

	public void GradientPerturb(ref float x, ref float y)
	{
		SingleGradientPerturb(m_seed, m_gradientPerturbAmp, m_frequency, ref x, ref y);
	}

	public void GradientPerturbFractal(ref float x, ref float y)
	{
		int seed = m_seed;
		float amp = m_gradientPerturbAmp * m_fractalBounding;
		float freq = m_frequency;
		SingleGradientPerturb(seed, amp, m_frequency, ref x, ref y);
		for (int i = 1; i < m_octaves; i++)
		{
			freq *= m_lacunarity;
			amp *= m_gain;
			SingleGradientPerturb(++seed, amp, freq, ref x, ref y);
		}
	}

	private void SingleGradientPerturb(int seed, float perturbAmp, float frequency, ref float x, ref float y)
	{
		float xf = x * frequency;
		float yf = y * frequency;
		int x2 = FastFloor(xf);
		int y2 = FastFloor(yf);
		int x3 = x2 + 1;
		int y3 = y2 + 1;
		float xs;
		float ys;
		switch (m_interp)
		{
		default:
			xs = xf - (float)x2;
			ys = yf - (float)y2;
			break;
		case Interp.Hermite:
			xs = InterpHermiteFunc(xf - (float)x2);
			ys = InterpHermiteFunc(yf - (float)y2);
			break;
		case Interp.Quintic:
			xs = InterpQuinticFunc(xf - (float)x2);
			ys = InterpQuinticFunc(yf - (float)y2);
			break;
		}
		Float2 @float = CELL_2D[Hash2D(seed, x2, y2) & 0xFF];
		Float2 vec1 = CELL_2D[Hash2D(seed, x3, y2) & 0xFF];
		float lx0x = Lerp(@float.x, vec1.x, xs);
		float ly0x = Lerp(@float.y, vec1.y, xs);
		Float2 float2 = CELL_2D[Hash2D(seed, x2, y3) & 0xFF];
		vec1 = CELL_2D[Hash2D(seed, x3, y3) & 0xFF];
		float lx1x = Lerp(float2.x, vec1.x, xs);
		float ly1x = Lerp(float2.y, vec1.y, xs);
		x += Lerp(lx0x, lx1x, ys) * perturbAmp;
		y += Lerp(ly0x, ly1x, ys) * perturbAmp;
	}
}
