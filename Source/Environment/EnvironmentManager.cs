using Godot;

public class EnvironmentManager : Node
{
	private Workspace workspace;

	private PreviewspaceViewport previewViewport;

	private float environmentRotation;

	private Basis directionalLightInitialBasis = Basis.Identity;

	private float defaultSkyContribution;

	private Color defaultAmbientLightColor = new Color(0f, 0f, 0f);

	private Color defaultDirectionalLightColor = new Color(1f, 0.92f, 0.8f);

	private float defaultDirectionalLightEnergy = 0.75f;

	private Environment workspaceEnvironment;

	private WorldEnvironment workspaceWorldEnvironment;

	private DirectionalLight workspaceDirectionalLight;

	private Environment previewEnvironment;

	private DirectionalLight previewDirectionalLight;

	public EnvironmentManager()
	{
		Register.EnvironmentManager = this;
	}

	public EnvironmentManager(Workspace workspace, Environment workspaceEnvironment, Environment previewEnvironment)
	{
		Register.EnvironmentManager = this;
		this.previewEnvironment = previewEnvironment;
		this.workspaceEnvironment = workspaceEnvironment;
		workspace.AddChild(this);
		base.Owner = workspace;
		base.Name = "EnvironmentManager";
	}

	public override void _Ready()
	{
		base._Ready();
		workspace = Register.Workspace;
		previewViewport = Register.PreviewspaceViewport;
		defaultSkyContribution = previewEnvironment.AmbientLightSkyContribution;
		defaultAmbientLightColor = previewEnvironment.AmbientLightColor;
		workspaceWorldEnvironment = new WorldEnvironment();
		AddChild(workspaceWorldEnvironment);
		workspaceWorldEnvironment.Owner = this;
		workspaceWorldEnvironment.Name = "WorkspaceWorldEnvironment";
		workspaceWorldEnvironment.Environment = workspaceEnvironment;
		workspaceDirectionalLight = new DirectionalLight();
		AddChild(workspaceDirectionalLight);
		workspaceDirectionalLight.Owner = this;
		workspaceDirectionalLight.Name = "WorkspaceDirectionalLight";
		workspaceDirectionalLight.LightColor = defaultDirectionalLightColor;
		workspaceDirectionalLight.LightEnergy = defaultDirectionalLightEnergy;
		workspaceDirectionalLight.LightSpecular = 0.75f;
		previewViewport.World.Environment = previewEnvironment;
		previewDirectionalLight = new DirectionalLight();
		previewViewport.AddChild(previewDirectionalLight);
		previewDirectionalLight.Owner = previewViewport;
		previewDirectionalLight.Name = "PreviewDirectionalLight";
		previewDirectionalLight.ShadowEnabled = true;
		previewDirectionalLight.DirectionalShadowMaxDistance = 256f;
		previewDirectionalLight.LightColor = workspaceDirectionalLight.LightColor;
		previewDirectionalLight.LightEnergy = workspaceDirectionalLight.LightEnergy;
		previewDirectionalLight.LightSpecular = 0.85f;
		directionalLightInitialBasis = new Basis(new Vector3(-120f, 0f, 0f));
		Reset();
	}

	public void Reset()
	{
		environmentRotation = 0f;
		workspaceEnvironment.AmbientLightEnergy = 1f;
		workspaceEnvironment.BackgroundEnergy = 1f;
		previewEnvironment.AmbientLightEnergy = 1f;
		previewEnvironment.BackgroundEnergy = 1f;
		workspaceEnvironment.AmbientLightSkyContribution = defaultSkyContribution;
		previewEnvironment.AmbientLightSkyContribution = defaultSkyContribution;
		workspaceEnvironment.AmbientLightColor = defaultAmbientLightColor;
		previewEnvironment.AmbientLightColor = defaultAmbientLightColor;
		workspaceDirectionalLight.Visible = true;
		previewDirectionalLight.Visible = true;
		workspaceDirectionalLight.LightColor = defaultDirectionalLightColor;
		previewDirectionalLight.LightColor = defaultDirectionalLightColor;
		workspaceDirectionalLight.LightEnergy = defaultDirectionalLightEnergy;
		previewDirectionalLight.LightEnergy = defaultDirectionalLightEnergy;
		previewDirectionalLight.ShadowEnabled = true;
		workspaceEnvironment.GlowEnabled = true;
		previewEnvironment.GlowEnabled = true;
		workspaceEnvironment.GlowIntensity = 1.25f;
		previewEnvironment.GlowIntensity = 1.25f;
		Update();
	}

	public void Update()
	{
		Basis EnvironmentBasis = Basis.Identity.Rotated(Vector3.Up, environmentRotation);
		workspaceEnvironment.BackgroundSkyOrientation = EnvironmentBasis;
		previewEnvironment.BackgroundSkyOrientation = EnvironmentBasis;
		Basis DirectionalLightBasis = directionalLightInitialBasis.Rotated(Vector3.Up, environmentRotation);
		Transform transform = new Transform(DirectionalLightBasis, Vector3.Zero);
		workspaceDirectionalLight.GlobalTransform = transform;
		previewDirectionalLight.GlobalTransform = transform;
	}

	public float GetEnvironmentRotation()
	{
		return environmentRotation;
	}

	public void RotateEnvironment(float rotation)
	{
		environmentRotation += rotation;
		Update();
	}

	public void SetEnvironmentRotation(float rotation)
	{
		environmentRotation = rotation;
		Update();
	}

	public bool IsGlowEnabled()
	{
		return previewEnvironment.GlowEnabled;
	}

	public void EnableGlow(bool enable)
	{
		workspaceEnvironment.GlowEnabled = enable;
		previewEnvironment.GlowEnabled = enable;
	}

	public float GetGlowIntensity()
	{
		return previewEnvironment.GlowIntensity;
	}

	public void SetGlowIntensity(float intensity)
	{
		workspaceEnvironment.GlowIntensity = intensity;
		previewEnvironment.GlowIntensity = intensity;
	}

	public float GetEnvironmentAmbientEnergy()
	{
		return previewEnvironment.AmbientLightEnergy;
	}

	public void SetEnvironmentAmbientEnergy(float energy)
	{
		workspaceEnvironment.AmbientLightEnergy = energy;
		workspaceEnvironment.BackgroundEnergy = energy;
		previewEnvironment.AmbientLightEnergy = energy;
		previewEnvironment.BackgroundEnergy = energy;
	}

	public float GetEnvironmentSkyContribution()
	{
		return previewEnvironment.AmbientLightSkyContribution;
	}

	public void SetEnvironmentSkyContribution(float energy)
	{
		workspaceEnvironment.AmbientLightSkyContribution = energy;
		previewEnvironment.AmbientLightSkyContribution = energy;
	}

	public void SetEnvironmentAmbientColor(Color color)
	{
		workspaceEnvironment.AmbientLightColor = color;
		previewEnvironment.AmbientLightColor = color;
	}

	public Color GetEnvironmentAmbientColor()
	{
		return previewEnvironment.AmbientLightColor;
	}

	public bool IsDirectionalLightEnabled()
	{
		return previewDirectionalLight.Visible;
	}

	public void EnableDirectionalLight(bool enable)
	{
		workspaceDirectionalLight.Visible = enable;
		previewDirectionalLight.Visible = enable;
	}

	public void SetDirectionalLightEnergy(float energy)
	{
		workspaceDirectionalLight.LightEnergy = energy;
		previewDirectionalLight.LightEnergy = energy;
	}

	public float GetDirectionalLightEnergy()
	{
		return previewDirectionalLight.LightEnergy;
	}

	public void SetDirectionalLightColor(Color color)
	{
		workspaceDirectionalLight.LightColor = color;
		previewDirectionalLight.LightColor = color;
	}

	public Color GetDirectionalLightColor()
	{
		return previewDirectionalLight.LightColor;
	}

	public bool IsDirectionalLightShadowEnabled()
	{
		return previewDirectionalLight.ShadowEnabled;
	}

	public void EnableDirectionalLightShadow(bool enabled)
	{
		previewDirectionalLight.ShadowEnabled = enabled;
	}
}
