using System.IO;
using Godot;

public class PreviewspaceViewport : Viewport
{
	private struct PreviewLight
	{
		public Spatial Spatial;

		public OmniLight Light;

		public Sprite3D Sprite3D;

		public float Angle;

		public float Distance;

		public bool Rotate;

		public float Rotation;

		public float RotationSpeed;
	}

	private bool showLightIcons = true;

	private PreviewLight[] previewLightsArray = new PreviewLight[3];

	public PreviewspaceViewport()
	{
		Register.PreviewspaceViewport = this;
	}

	public override void _Ready()
	{
		base._Ready();
		for (int i = 0; i < previewLightsArray.Length; i++)
		{
			previewLightsArray[i].Spatial = new Spatial();
			AddChild(previewLightsArray[i].Spatial);
			previewLightsArray[i].Spatial.Owner = this;
			previewLightsArray[i].Spatial.Name = "PreviewLight" + (i + 1);
			previewLightsArray[i].Light = new OmniLight();
			previewLightsArray[i].Spatial.AddChild(previewLightsArray[i].Light);
			previewLightsArray[i].Light.Owner = previewLightsArray[i].Spatial;
			previewLightsArray[i].Light.SetLayerMaskBit(0, enabled: false);
			previewLightsArray[i].Light.SetLayerMaskBit(19, enabled: true);
			previewLightsArray[i].Light.ShadowBias = 1f;
			previewLightsArray[i].Light.OmniRange = 64f;
			previewLightsArray[i].Light.LightEnergy = 1f;
			previewLightsArray[i].Light.LightSpecular = 0.8f;
			previewLightsArray[i].Light.ShadowEnabled = true;
			previewLightsArray[i].Light.ShadowColor = new Color(0f, 0f, 0f, 0f);
			previewLightsArray[i].Rotate = false;
			previewLightsArray[i].RotationSpeed = 10f;
			previewLightsArray[i].Angle = 0f;
			previewLightsArray[i].Distance = 16f;
			Transform transform = Transform.Identity;
			switch (i)
			{
			case 0:
				previewLightsArray[i].Light.LightColor = new Color(0.45f, 0.5f, 1f);
				transform.origin.x = Mathf.Cos(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				transform.origin.y = 8f;
				transform.origin.z = Mathf.Sin(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				break;
			case 1:
				previewLightsArray[i].Light.LightColor = new Color(1f, 0.8f, 0.6f);
				previewLightsArray[i].Angle = 120f;
				transform.origin.x = Mathf.Cos(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				transform.origin.y = 8f;
				transform.origin.z = Mathf.Sin(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				break;
			case 2:
				previewLightsArray[i].Light.LightColor = new Color(0.5f, 1f, 0.6f);
				previewLightsArray[i].Angle = 240f;
				transform.origin.x = Mathf.Cos(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				transform.origin.y = 8f;
				transform.origin.z = Mathf.Sin(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				break;
			}
			previewLightsArray[i].Light.Transform = transform;
			previewLightsArray[i].Light.SetLayerMaskBit(19, enabled: true);
			previewLightsArray[i].Sprite3D = new Sprite3D();
			previewLightsArray[i].Light.AddChild(previewLightsArray[i].Sprite3D);
			previewLightsArray[i].Sprite3D.Owner = previewLightsArray[i].Light;
			previewLightsArray[i].Sprite3D.Texture = Resources.LightIconTexture;
			previewLightsArray[i].Sprite3D.SetLayerMaskBit(0, enabled: false);
			previewLightsArray[i].Sprite3D.SetLayerMaskBit(19, enabled: true);
			previewLightsArray[i].Sprite3D.PixelSize = 0.015f;
			previewLightsArray[i].Sprite3D.Billboard = Material3D.BillboardMode.Enabled;
			previewLightsArray[i].Sprite3D.CastShadow = GeometryInstance.ShadowCastingSetting.Off;
			previewLightsArray[i].Sprite3D.Modulate = previewLightsArray[i].Light.LightColor;
		}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		for (int i = 0; i < previewLightsArray.Length; i++)
		{
			if (previewLightsArray[i].Rotate)
			{
				previewLightsArray[i].Rotation += previewLightsArray[i].RotationSpeed * delta;
				if (previewLightsArray[i].Rotation > 360f)
				{
					previewLightsArray[i].Rotation -= 360f;
				}
				previewLightsArray[i].Spatial.RotationDegrees = new Vector3(0f, previewLightsArray[i].Rotation, 0f);
			}
		}
	}

	public void Reset()
	{
		showLightIcons = true;
		for (int i = 0; i < previewLightsArray.Length; i++)
		{
			previewLightsArray[i].Light.Visible = false;
			previewLightsArray[i].Light.OmniRange = 64f;
			previewLightsArray[i].Light.LightEnergy = 1f;
			previewLightsArray[i].Light.LightSpecular = 0.8f;
			previewLightsArray[i].Light.ShadowEnabled = true;
			previewLightsArray[i].Light.ShadowColor = new Color(0f, 0f, 0f, 0f);
			previewLightsArray[i].Rotate = false;
			previewLightsArray[i].Rotation = 0f;
			previewLightsArray[i].RotationSpeed = 10f;
			previewLightsArray[i].Angle = 0f;
			previewLightsArray[i].Distance = 16f;
			Transform transform = Transform.Identity;
			switch (i)
			{
			case 0:
				previewLightsArray[i].Light.LightColor = new Color(0.45f, 0.5f, 1f);
				transform.origin.x = Mathf.Cos(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				transform.origin.y = 8f;
				transform.origin.z = Mathf.Sin(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				break;
			case 1:
				previewLightsArray[i].Light.LightColor = new Color(1f, 0.8f, 0.6f);
				previewLightsArray[i].Angle = 120f;
				transform.origin.x = Mathf.Cos(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				transform.origin.y = 8f;
				transform.origin.z = Mathf.Sin(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				break;
			case 2:
				previewLightsArray[i].Light.LightColor = new Color(0.5f, 1f, 0.6f);
				previewLightsArray[i].Angle = 240f;
				transform.origin.x = Mathf.Cos(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				transform.origin.y = 8f;
				transform.origin.z = Mathf.Sin(Mathf.Deg2Rad(previewLightsArray[i].Angle)) * previewLightsArray[i].Distance;
				break;
			}
			previewLightsArray[i].Sprite3D.Visible = showLightIcons;
			previewLightsArray[i].Sprite3D.Modulate = previewLightsArray[i].Light.LightColor;
			previewLightsArray[i].Light.Transform = transform;
			previewLightsArray[i].Spatial.RotationDegrees = new Vector3(0f, previewLightsArray[i].Rotation, 0f);
		}
	}

	public bool IsLightEnabled(int index)
	{
		return previewLightsArray[index].Light.Visible;
	}

	public void EnableLight(int index, bool enabled)
	{
		previewLightsArray[index].Light.Visible = enabled;
	}

	public Color GetLightColor(int index)
	{
		return previewLightsArray[index].Light.LightColor;
	}

	public void SetLightColor(int index, Color color)
	{
		previewLightsArray[index].Light.LightColor = color;
		previewLightsArray[index].Sprite3D.Modulate = previewLightsArray[index].Light.LightColor;
	}

	public float GetLightEnergy(int index)
	{
		return previewLightsArray[index].Light.LightEnergy;
	}

	public void SetLightEnergy(int index, float energy)
	{
		previewLightsArray[index].Light.LightEnergy = energy;
	}

	public float GetLightRange(int index)
	{
		return previewLightsArray[index].Light.OmniRange;
	}

	public void SetLightRange(int index, float range)
	{
		previewLightsArray[index].Light.OmniRange = range;
	}

	public float GetLightAngle(int index)
	{
		return previewLightsArray[index].Angle;
	}

	public void SetLightAngle(int index, float angle)
	{
		previewLightsArray[index].Angle = angle;
		Transform transform = Transform.Identity;
		transform.origin.x = Mathf.Cos(Mathf.Deg2Rad(previewLightsArray[index].Angle)) * previewLightsArray[index].Distance;
		transform.origin.y = previewLightsArray[index].Light.Transform.origin.y;
		transform.origin.z = Mathf.Sin(Mathf.Deg2Rad(previewLightsArray[index].Angle)) * previewLightsArray[index].Distance;
		previewLightsArray[index].Light.Transform = transform;
	}

	public float GetLightHeight(int index)
	{
		return previewLightsArray[index].Light.Transform.origin.y;
	}

	public void SetLightHeight(int index, float height)
	{
		Transform transform = previewLightsArray[index].Light.Transform;
		transform.origin.y = height;
		previewLightsArray[index].Light.Transform = transform;
	}

	public float GetLightDistance(int index)
	{
		return previewLightsArray[index].Distance;
	}

	public void SetLightDistance(int index, float distance)
	{
		previewLightsArray[index].Distance = distance;
		Transform transform = Transform.Identity;
		transform.origin.x = Mathf.Cos(Mathf.Deg2Rad(previewLightsArray[index].Angle)) * previewLightsArray[index].Distance;
		transform.origin.y = previewLightsArray[index].Light.Transform.origin.y;
		transform.origin.z = Mathf.Sin(Mathf.Deg2Rad(previewLightsArray[index].Angle)) * previewLightsArray[index].Distance;
		previewLightsArray[index].Light.Transform = transform;
	}

	public Vector3 GetLightPosition(int index)
	{
		return previewLightsArray[index].Light.GlobalTransform.origin;
	}

	public bool IsLightRotationEnabled(int index)
	{
		return previewLightsArray[index].Rotate;
	}

	public void EnableLightRotation(int index, bool enabled)
	{
		previewLightsArray[index].Rotate = enabled;
	}

	public float GetLightRotation(int index)
	{
		return previewLightsArray[index].Rotation;
	}

	public void SetLightRotation(int index, float rotation)
	{
		previewLightsArray[index].Rotation = rotation;
		previewLightsArray[index].Spatial.RotationDegrees = new Vector3(0f, previewLightsArray[index].Rotation, 0f);
	}

	public float GetLightRotationSpeed(int index)
	{
		return previewLightsArray[index].RotationSpeed;
	}

	public void SetLightRotationSpeed(int index, float rotationSpeed)
	{
		previewLightsArray[index].RotationSpeed = rotationSpeed;
	}

	public bool IsLightIconsEnable()
	{
		return showLightIcons;
	}

	public void EnableLightIcons(bool enabled)
	{
		showLightIcons = enabled;
		for (int i = 0; i < previewLightsArray.Length; i++)
		{
			previewLightsArray[i].Sprite3D.Visible = showLightIcons;
		}
	}

	public void WritePointLightToBinaryStream(BinaryWriter binaryWriter, int index)
	{
		binaryWriter.Write(IsLightEnabled(index));
		Color color = GetLightColor(index);
		binaryWriter.Write(color.r);
		binaryWriter.Write(color.g);
		binaryWriter.Write(color.b);
		binaryWriter.Write(GetLightEnergy(index));
		binaryWriter.Write(GetLightRange(index));
		binaryWriter.Write(GetLightAngle(index));
		binaryWriter.Write(GetLightHeight(index));
		binaryWriter.Write(GetLightDistance(index));
		binaryWriter.Write(IsLightRotationEnabled(index));
		binaryWriter.Write(GetLightRotationSpeed(index));
	}

	public void ReadPointLightFromBinaryStream(BinaryReader binaryReader, int index)
	{
		EnableLight(index, binaryReader.ReadBoolean());
		Color color = new Color(0f, 0f, 0f);
		color.r = binaryReader.ReadSingle();
		color.g = binaryReader.ReadSingle();
		color.b = binaryReader.ReadSingle();
		SetLightColor(index, color);
		SetLightEnergy(index, binaryReader.ReadSingle());
		SetLightRange(index, binaryReader.ReadSingle());
		SetLightAngle(index, binaryReader.ReadSingle());
		SetLightHeight(index, binaryReader.ReadSingle());
		SetLightDistance(index, binaryReader.ReadSingle());
		EnableLightRotation(index, binaryReader.ReadBoolean());
		SetLightRotationSpeed(index, binaryReader.ReadSingle());
	}
}
