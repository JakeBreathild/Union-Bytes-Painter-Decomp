using Godot;

public static class Blender
{
	public enum BlendingTypeEnum
	{
		COLOR = 1,
		VALUE = 2,
		FLOAT = 4
	}

	public enum BlendingModeEnum
	{
		NORMAL,
		ADDITION,
		SUBTRACTION,
		MULTIPLICATION,
		MINIMUM,
		MAXIMUM,
		SCREEN,
		ALPHABLEND,
		REPLACE,
		HUE,
		SATURATION,
		VALUE,
		COUNT
	}

	public delegate void ColorBlendingDelegate(ref Color bottom, ref Color top, float blendStrength);

	public delegate void ValueBlendingDelegate(ref Value bottom, ref Value top, float blendStrength);

	public delegate void FloatBlendingDelegate(ref float bottom, ref float top, float blendStrength);

	public static string[] BlendingModeName = new string[12]
	{
		"Normal", "Add", "Sub", "Mul", "Min", "Max", "Screen", "AlphaBlend", "Replace", "Hue",
		"Saturation", "Value"
	};

	public static BlendingTypeEnum[] BlendingType = new BlendingTypeEnum[12]
	{
		(BlendingTypeEnum)7,
		(BlendingTypeEnum)7,
		(BlendingTypeEnum)7,
		(BlendingTypeEnum)7,
		(BlendingTypeEnum)7,
		(BlendingTypeEnum)7,
		(BlendingTypeEnum)7,
		(BlendingTypeEnum)7,
		(BlendingTypeEnum)7,
		BlendingTypeEnum.COLOR,
		BlendingTypeEnum.COLOR,
		BlendingTypeEnum.COLOR
	};

	public static ColorBlendingDelegate[] ColorBlending = new ColorBlendingDelegate[12]
	{
		Normal, Addition, Subtraction, Multiplication, Minimum, Maximum, Screen, Alpha, Replace, Hue,
		Saturation, Value
	};

	public static ValueBlendingDelegate[] ValueBlending = new ValueBlendingDelegate[12]
	{
		Normal, Addition, Subtraction, Multiplication, Minimum, Maximum, Screen, Alpha, Replace, null,
		null, null
	};

	public static FloatBlendingDelegate[] FloatBlending = new FloatBlendingDelegate[12]
	{
		Normal, Addition, Subtraction, Multiplication, Minimum, Maximum, Screen, Alpha, Replace, null,
		null, null
	};

	private static readonly Color colorZero = new Color(0f, 0f, 0f, 0f);

	private static readonly Color colorBlack = new Color(0f, 0f, 0f);

	private static readonly Color colorNormal = new Color(0.5f, 0.5f, 1f);

	private static readonly Color colorWhite = new Color(1f, 1f, 1f);

	private static readonly Value valueZero = new Value(0f, 0f);

	private static readonly Value valueBlack = new Value(0f);

	private static readonly Value valueGray = new Value(0.5f);

	private static readonly Value valueWhite = new Value(1f);

	public static ColorBlendingDelegate ColorBlendingFunction(BlendingModeEnum blendingMode)
	{
		if (blendingMode < BlendingModeEnum.COUNT && ColorBlending[(int)blendingMode] != null)
		{
			return ColorBlending[(int)blendingMode];
		}
		return Normal;
	}

	public static ValueBlendingDelegate ValueBlendingFunction(BlendingModeEnum blendingMode)
	{
		if (blendingMode < BlendingModeEnum.COUNT && ValueBlending[(int)blendingMode] != null)
		{
			return ValueBlending[(int)blendingMode];
		}
		return Normal;
	}

	public static FloatBlendingDelegate FloatBlendingFunction(BlendingModeEnum blendingMode)
	{
		if (blendingMode < BlendingModeEnum.COUNT && FloatBlending[(int)blendingMode] != null)
		{
			return FloatBlending[(int)blendingMode];
		}
		return Normal;
	}

	public static Color Blend(Color bottomColor, Color topColor, BlendingModeEnum blendingMode, float blendStrength = 1f)
	{
		bottomColor = blendingMode switch
		{
			BlendingModeEnum.NORMAL => Normal(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.ALPHABLEND => Alpha(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.MULTIPLICATION => Multiplication(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.ADDITION => Addition(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.SUBTRACTION => Subtraction(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.SCREEN => Screen(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.MINIMUM => Minimum(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.MAXIMUM => Maximum(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.REPLACE => Replace(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.HUE => Hue(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.SATURATION => Saturation(bottomColor, topColor, blendStrength), 
			BlendingModeEnum.VALUE => Value(bottomColor, topColor, blendStrength), 
			_ => Normal(bottomColor, topColor, blendStrength), 
		};
		return bottomColor;
	}

	public static void Blend(ref Color bottomColor, ref Color topColor, BlendingModeEnum blendingMode, float blendStrength = 1f)
	{
		switch (blendingMode)
		{
		case BlendingModeEnum.NORMAL:
			Normal(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.ALPHABLEND:
			Alpha(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.MULTIPLICATION:
			Multiplication(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.ADDITION:
			Addition(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.SUBTRACTION:
			Subtraction(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.SCREEN:
			Screen(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.MINIMUM:
			Minimum(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.MAXIMUM:
			Maximum(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.REPLACE:
			Replace(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.HUE:
			Hue(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.SATURATION:
			Saturation(ref bottomColor, ref topColor, blendStrength);
			break;
		case BlendingModeEnum.VALUE:
			Value(ref bottomColor, ref topColor, blendStrength);
			break;
		default:
			Normal(ref bottomColor, ref topColor, blendStrength);
			break;
		}
	}

	public static Value Blend(Value bottomValue, Value topValue, BlendingModeEnum blendingMode, float blendStrength = 1f)
	{
		bottomValue = blendingMode switch
		{
			BlendingModeEnum.NORMAL => Normal(bottomValue, topValue, blendStrength), 
			BlendingModeEnum.ALPHABLEND => Alpha(bottomValue, topValue, blendStrength), 
			BlendingModeEnum.MULTIPLICATION => Multiplication(bottomValue, topValue, blendStrength), 
			BlendingModeEnum.ADDITION => Addition(bottomValue, topValue, blendStrength), 
			BlendingModeEnum.SUBTRACTION => Subtraction(bottomValue, topValue, blendStrength), 
			BlendingModeEnum.SCREEN => Screen(bottomValue, topValue, blendStrength), 
			BlendingModeEnum.MINIMUM => Minimum(bottomValue, topValue, blendStrength), 
			BlendingModeEnum.MAXIMUM => Maximum(bottomValue, topValue, blendStrength), 
			BlendingModeEnum.REPLACE => Replace(bottomValue, topValue, blendStrength), 
			_ => Normal(bottomValue, topValue, blendStrength), 
		};
		return bottomValue;
	}

	public static void Blend(ref Value bottomValue, ref Value topValue, BlendingModeEnum blendingMode, float blendStrength = 1f)
	{
		switch (blendingMode)
		{
		case BlendingModeEnum.NORMAL:
			Normal(ref bottomValue, ref topValue, blendStrength);
			break;
		case BlendingModeEnum.ALPHABLEND:
			Alpha(ref bottomValue, ref topValue, blendStrength);
			break;
		case BlendingModeEnum.MULTIPLICATION:
			Multiplication(ref bottomValue, ref topValue, blendStrength);
			break;
		case BlendingModeEnum.ADDITION:
			Addition(ref bottomValue, ref topValue, blendStrength);
			break;
		case BlendingModeEnum.SUBTRACTION:
			Subtraction(ref bottomValue, ref topValue, blendStrength);
			break;
		case BlendingModeEnum.SCREEN:
			Screen(ref bottomValue, ref topValue, blendStrength);
			break;
		case BlendingModeEnum.MINIMUM:
			Minimum(ref bottomValue, ref topValue, blendStrength);
			break;
		case BlendingModeEnum.MAXIMUM:
			Maximum(ref bottomValue, ref topValue, blendStrength);
			break;
		case BlendingModeEnum.REPLACE:
			Replace(ref bottomValue, ref topValue, blendStrength);
			break;
		default:
			Normal(ref bottomValue, ref topValue, blendStrength);
			break;
		}
	}

	public static float Blend(float bottomFloat, float topFloat, BlendingModeEnum blendingMode, float blendStrength = 1f)
	{
		bottomFloat = blendingMode switch
		{
			BlendingModeEnum.NORMAL => Normal(bottomFloat, topFloat, blendStrength), 
			BlendingModeEnum.ALPHABLEND => Alpha(bottomFloat, topFloat, blendStrength), 
			BlendingModeEnum.MULTIPLICATION => Multiplication(bottomFloat, topFloat, blendStrength), 
			BlendingModeEnum.ADDITION => Addition(bottomFloat, topFloat, blendStrength), 
			BlendingModeEnum.SUBTRACTION => Subtraction(bottomFloat, topFloat, blendStrength), 
			BlendingModeEnum.SCREEN => Screen(bottomFloat, topFloat, blendStrength), 
			BlendingModeEnum.MINIMUM => Minimum(bottomFloat, topFloat, blendStrength), 
			BlendingModeEnum.MAXIMUM => Maximum(bottomFloat, topFloat, blendStrength), 
			BlendingModeEnum.REPLACE => Replace(bottomFloat, topFloat, blendStrength), 
			_ => Normal(bottomFloat, topFloat, blendStrength), 
		};
		return bottomFloat;
	}

	public static void Blend(ref float bottomFloat, ref float topFloat, BlendingModeEnum blendingMode, float blendStrength = 1f)
	{
		switch (blendingMode)
		{
		case BlendingModeEnum.NORMAL:
			Normal(ref bottomFloat, ref topFloat, blendStrength);
			break;
		case BlendingModeEnum.ALPHABLEND:
			Alpha(ref bottomFloat, ref topFloat, blendStrength);
			break;
		case BlendingModeEnum.MULTIPLICATION:
			Multiplication(ref bottomFloat, ref topFloat, blendStrength);
			break;
		case BlendingModeEnum.ADDITION:
			Addition(ref bottomFloat, ref topFloat, blendStrength);
			break;
		case BlendingModeEnum.SUBTRACTION:
			Subtraction(ref bottomFloat, ref topFloat, blendStrength);
			break;
		case BlendingModeEnum.SCREEN:
			Screen(ref bottomFloat, ref topFloat, blendStrength);
			break;
		case BlendingModeEnum.MINIMUM:
			Minimum(ref bottomFloat, ref topFloat, blendStrength);
			break;
		case BlendingModeEnum.MAXIMUM:
			Maximum(ref bottomFloat, ref topFloat, blendStrength);
			break;
		case BlendingModeEnum.REPLACE:
			Replace(ref bottomFloat, ref topFloat, blendStrength);
			break;
		default:
			Normal(ref bottomFloat, ref topFloat, blendStrength);
			break;
		}
	}

	public static void Clamp(ref Color color)
	{
		color.r = ((color.r < 0f) ? 0f : ((color.r > 1f) ? 1f : color.r));
		color.g = ((color.g < 0f) ? 0f : ((color.g > 1f) ? 1f : color.g));
		color.b = ((color.b < 0f) ? 0f : ((color.b > 1f) ? 1f : color.b));
		color.a = ((color.a < 0f) ? 0f : ((color.a > 1f) ? 1f : color.a));
	}

	public static void Clamp(ref Value value)
	{
		value.v = ((value.v < 0f) ? 0f : ((value.v > 1f) ? 1f : value.v));
		value.a = ((value.a < 0f) ? 0f : ((value.a > 1f) ? 1f : value.a));
	}

	public static void Clamp(ref float value)
	{
		value = ((value < 0f) ? 0f : ((value > 1f) ? 1f : value));
	}

	public static void Replace(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		if (blendStrength >= 0.5f)
		{
			bottomColor.r = topColor.r;
			bottomColor.g = topColor.g;
			bottomColor.b = topColor.b;
			bottomColor.a = topColor.a;
		}
	}

	public static Color Replace(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Replace(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Replace(ref Value bottomValue, ref Value topValue, float blendStrength = 1f)
	{
		if (blendStrength >= 0.5f)
		{
			bottomValue.v = topValue.v;
			bottomValue.a = topValue.a;
		}
	}

	public static Value Replace(Value bottomValue, Value topValue, float blendStrength = 1f)
	{
		Replace(ref bottomValue, ref topValue, blendStrength);
		return bottomValue;
	}

	public static void Replace(ref float bottomFloat, ref float topFloat, float blendStrength = 1f)
	{
		if (blendStrength >= 0.5f)
		{
			bottomFloat = topFloat;
		}
	}

	public static float Replace(float bottomFloat, float topFloat, float blendStrength = 1f)
	{
		Replace(ref bottomFloat, ref topFloat, blendStrength);
		return bottomFloat;
	}

	public static void Normal(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		float bottomAlpha = bottomColor.a;
		float topAlpha = topColor.a * blendStrength;
		float num = 1f - topAlpha;
		bottomColor.a = bottomColor.a * num + topAlpha;
		if (bottomColor.a > 0f)
		{
			bottomColor.r = (bottomColor.r * bottomAlpha * num + topColor.r * topAlpha) / bottomColor.a;
			bottomColor.g = (bottomColor.g * bottomAlpha * num + topColor.g * topAlpha) / bottomColor.a;
			bottomColor.b = (bottomColor.b * bottomAlpha * num + topColor.b * topAlpha) / bottomColor.a;
		}
		bottomColor.a = topAlpha + bottomAlpha * (1f - blendStrength);
	}

	public static Color Normal(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Normal(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Normal(ref Value bottomValue, ref Value topValue, float blendStrength = 1f)
	{
		float bottomAlpha = bottomValue.a;
		float topAlpha = topValue.a * blendStrength;
		float num = 1f - topAlpha;
		bottomValue.a = bottomValue.a * num + topAlpha;
		if (bottomValue.a > 0f)
		{
			bottomValue.v = (bottomValue.v * bottomAlpha * num + topValue.v * topAlpha) / bottomValue.a;
		}
		bottomValue.a = topAlpha + bottomAlpha * (1f - blendStrength);
	}

	public static Value Normal(Value bottomValue, Value topValue, float blendStrength = 1f)
	{
		Normal(ref bottomValue, ref topValue, blendStrength);
		return bottomValue;
	}

	public static void Normal(ref float bottomFloat, ref float topFloat, float blendStrength = 1f)
	{
		bottomFloat = topFloat * blendStrength + bottomFloat * (1f - blendStrength);
	}

	public static float Normal(float bottomFloat, float topFloat, float blendStrength = 1f)
	{
		Normal(ref bottomFloat, ref topFloat, blendStrength);
		return bottomFloat;
	}

	public static void Alpha(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		float bottomAlpha = bottomColor.a;
		float topAlpha = topColor.a * blendStrength;
		float num = 1f - topAlpha;
		bottomColor.a = bottomColor.a * num + topAlpha;
		if (bottomColor.a > 0f)
		{
			bottomColor.r = (bottomColor.r * bottomAlpha * num + topColor.r * topAlpha) / bottomColor.a;
			bottomColor.g = (bottomColor.g * bottomAlpha * num + topColor.g * topAlpha) / bottomColor.a;
			bottomColor.b = (bottomColor.b * bottomAlpha * num + topColor.b * topAlpha) / bottomColor.a;
		}
	}

	public static Color Alpha(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Alpha(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Alpha(ref Value bottomValue, ref Value topValue, float blendStrength = 1f)
	{
		float bottomAlpha = bottomValue.a;
		float topAlpha = topValue.a * blendStrength;
		float num = 1f - topAlpha;
		bottomValue.a = bottomValue.a * num + topAlpha;
		if (bottomValue.a > 0f)
		{
			bottomValue.v = (bottomValue.v * bottomAlpha * num + topValue.v * topAlpha) / bottomValue.a;
		}
	}

	public static Value Alpha(Value bottomValue, Value topValue, float blendStrength = 1f)
	{
		Alpha(ref bottomValue, ref topValue, blendStrength);
		return bottomValue;
	}

	public static void Alpha(ref float bottomFloat, ref float topFloat, float blendStrength = 1f)
	{
		bottomFloat = topFloat * blendStrength + bottomFloat * (1f - blendStrength);
	}

	public static float Alpha(float bottomFloat, float topFloat, float blendStrength = 1f)
	{
		Alpha(ref bottomFloat, ref topFloat, blendStrength);
		return bottomFloat;
	}

	public static void Multiplication(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		float bottomAlpha = bottomColor.a;
		blendStrength *= topColor.a;
		bottomColor = bottomColor * topColor * blendStrength + bottomColor * (1f - blendStrength);
		bottomColor.a = bottomAlpha;
	}

	public static Color Multiplication(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Multiplication(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Multiplication(ref Value bottomValue, ref Value topValue, float blendStrength = 1f)
	{
		blendStrength *= topValue.a;
		bottomValue.v = bottomValue.v * topValue.v * blendStrength + bottomValue.v * (1f - blendStrength);
	}

	public static Value Multiplication(Value bottomValue, Value topValue, float blendStrength = 1f)
	{
		Multiplication(ref bottomValue, ref topValue, blendStrength);
		return bottomValue;
	}

	public static void Multiplication(ref float bottomFloat, ref float topFloat, float blendStrength = 1f)
	{
		bottomFloat = bottomFloat * topFloat * blendStrength + bottomFloat * (1f - blendStrength);
	}

	public static float Multiplication(float bottomFloat, float topFloat, float blendStrength = 1f)
	{
		Multiplication(ref bottomFloat, ref topFloat, blendStrength);
		return bottomFloat;
	}

	public static void Addition(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		float bottomAlpha = bottomColor.a;
		bottomColor += topColor * blendStrength * topColor.a;
		bottomColor.a = bottomAlpha;
		Clamp(ref bottomColor);
	}

	public static Color Addition(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Addition(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Addition(ref Value bottomValue, ref Value topValue, float blendStrength = 1f)
	{
		bottomValue.v += topValue.v * topValue.a * blendStrength;
		Clamp(ref bottomValue);
	}

	public static Value Addition(Value bottomValue, Value topValue, float blendStrength = 1f)
	{
		Addition(ref bottomValue, ref topValue, blendStrength);
		return bottomValue;
	}

	public static void Addition(ref float bottomFloat, ref float topFloat, float blendStrength = 1f)
	{
		bottomFloat += topFloat * blendStrength;
		Clamp(ref bottomFloat);
	}

	public static float Addition(float bottomFloat, float topFloat, float blendStrength = 1f)
	{
		Addition(ref bottomFloat, ref topFloat, blendStrength);
		return bottomFloat;
	}

	public static void Subtraction(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		float bottomAlpha = bottomColor.a;
		bottomColor -= topColor * blendStrength * topColor.a;
		bottomColor.a = bottomAlpha;
		Clamp(ref bottomColor);
	}

	public static Color Subtraction(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Subtraction(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Subtraction(ref Value bottomValue, ref Value topValue, float blendStrength = 1f)
	{
		bottomValue.v -= topValue.v * blendStrength * topValue.a;
		Clamp(ref bottomValue);
	}

	public static Value Subtraction(Value bottomValue, Value topValue, float blendStrength = 1f)
	{
		Subtraction(ref bottomValue, ref topValue, blendStrength);
		return bottomValue;
	}

	public static void Subtraction(ref float bottomFloat, ref float topFloat, float blendStrength = 1f)
	{
		bottomFloat -= topFloat * blendStrength * topFloat;
		Clamp(ref bottomFloat);
	}

	public static float Subtraction(float bottomFloat, float topFloat, float blendStrength = 1f)
	{
		Subtraction(ref bottomFloat, ref topFloat, blendStrength);
		return bottomFloat;
	}

	public static void Screen(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		blendStrength *= topColor.a;
		bottomColor = (colorWhite - (colorWhite - bottomColor) * (colorWhite - topColor)) * blendStrength + bottomColor * (1f - blendStrength);
	}

	public static Color Screen(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Screen(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Screen(ref Value bottomValue, ref Value topValue, float blendStrength = 1f)
	{
		blendStrength *= topValue.a;
		bottomValue = (valueWhite - (valueWhite - bottomValue) * (valueWhite - topValue)) * blendStrength + bottomValue * (1f - blendStrength);
	}

	public static Value Screen(Value bottomValue, Value topValue, float blendStrength = 1f)
	{
		Screen(ref bottomValue, ref topValue, blendStrength);
		return bottomValue;
	}

	public static void Screen(ref float bottomFloat, ref float topFloat, float blendStrength = 1f)
	{
		bottomFloat = (1f - (1f - bottomFloat) * (1f - topFloat)) * blendStrength + bottomFloat * (1f - blendStrength);
	}

	public static float Screen(float bottomFloat, float topFloat, float blendStrength = 1f)
	{
		Screen(ref bottomFloat, ref topFloat, blendStrength);
		return bottomFloat;
	}

	public static void Minimum(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		if (bottomColor.v >= topColor.v)
		{
			blendStrength *= topColor.a;
			bottomColor = topColor * blendStrength + bottomColor * (1f - blendStrength);
		}
	}

	public static Color Minimum(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Minimum(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Minimum(ref Value bottomValue, ref Value topValue, float blendStrength = 1f)
	{
		if (bottomValue.v >= topValue.v)
		{
			blendStrength *= topValue.a;
			bottomValue = topValue * blendStrength + bottomValue * (1f - blendStrength);
		}
	}

	public static Value Minimum(Value bottomValue, Value topValue, float blendStrength = 1f)
	{
		Minimum(ref bottomValue, ref topValue, blendStrength);
		return bottomValue;
	}

	public static void Minimum(ref float bottomFloat, ref float topFloat, float blendStrength = 1f)
	{
		if (bottomFloat >= topFloat)
		{
			bottomFloat = topFloat * blendStrength + bottomFloat * (1f - blendStrength);
		}
	}

	public static float Minimum(float bottomFloat, float topFloat, float blendStrength = 1f)
	{
		Minimum(ref bottomFloat, ref topFloat, blendStrength);
		return bottomFloat;
	}

	public static void Maximum(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		if (bottomColor.v <= topColor.v)
		{
			blendStrength *= topColor.a;
			bottomColor = topColor * blendStrength + bottomColor * (1f - blendStrength);
		}
	}

	public static Color Maximum(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Maximum(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Maximum(ref Value bottomValue, ref Value topValue, float blendStrength = 1f)
	{
		if (bottomValue.v <= topValue.v)
		{
			blendStrength *= topValue.a;
			bottomValue = topValue * blendStrength + bottomValue * (1f - blendStrength);
		}
	}

	public static Value Maximum(Value bottomValue, Value topValue, float blendStrength = 1f)
	{
		Maximum(ref bottomValue, ref topValue, blendStrength);
		return bottomValue;
	}

	public static void Maximum(ref float bottomFloat, ref float topFloat, float blendStrength = 1f)
	{
		if (bottomFloat <= topFloat)
		{
			bottomFloat = topFloat * blendStrength + bottomFloat * (1f - blendStrength);
		}
	}

	public static float Maximum(float bottomFloat, float topFloat, float blendStrength = 1f)
	{
		Maximum(ref bottomFloat, ref topFloat, blendStrength);
		return bottomFloat;
	}

	public static void Hue(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		float bottomAlpha = bottomColor.a;
		blendStrength *= topColor.a;
		bottomColor.h = topColor.h * blendStrength + bottomColor.h * (1f - blendStrength);
		bottomColor.a = bottomAlpha;
	}

	public static Color Hue(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Hue(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Saturation(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		float bottomAlpha = bottomColor.a;
		blendStrength *= topColor.a;
		bottomColor.s = topColor.s * blendStrength + bottomColor.s * (1f - blendStrength);
		bottomColor.a = bottomAlpha;
	}

	public static Color Saturation(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Saturation(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void Value(ref Color bottomColor, ref Color topColor, float blendStrength = 1f)
	{
		float bottomAlpha = bottomColor.a;
		blendStrength *= topColor.a;
		bottomColor.v = topColor.v * blendStrength + bottomColor.v * (1f - blendStrength);
		bottomColor.a = bottomAlpha;
	}

	public static Color Value(Color bottomColor, Color topColor, float blendStrength = 1f)
	{
		Value(ref bottomColor, ref topColor, blendStrength);
		return bottomColor;
	}

	public static void HeightBlend(float bottomHeight, ref Color bottomColor, float topHeight, ref Color topColor)
	{
		if (bottomHeight < topHeight)
		{
			bottomColor = topColor;
		}
	}

	public static Color HeightBlend(float bottomHeight, Color bottomColor, float topHeight, Color topColor)
	{
		if (bottomHeight < topHeight)
		{
			return topColor;
		}
		return bottomColor;
	}

	public static void HeightBlend(float bottomHeight, ref Color bottomColor, float topHeight, ref Color topColor, float heightBlendFactor)
	{
		float heightStart = Mathf.Max(bottomHeight, topHeight) - heightBlendFactor;
		float bottomLevel = Mathf.Max(bottomHeight - heightStart, 0f);
		float topLevel = Mathf.Max(topHeight - heightStart, 0f);
		bottomColor = (bottomColor * bottomLevel + topColor * topLevel) / (bottomLevel + topLevel);
	}

	public static Color HeightBlend(float bottomHeight, Color bottomColor, float topHeight, Color topColor, float heightBlendFactor)
	{
		float heightStart = Mathf.Max(bottomHeight, topHeight) - heightBlendFactor;
		float bottomLevel = Mathf.Max(bottomHeight - heightStart, 0f);
		float topLevel = Mathf.Max(topHeight - heightStart, 0f);
		return (bottomColor * bottomLevel + topColor * topLevel) / (bottomLevel + topLevel);
	}

	public static void HeightLerp(float bottomHeight, ref Color bottomColor, float topHeight, ref Color topColor, float heightBlendFactor, float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		bottomColor = HeightBlend(bottomHeight * (1f - t), bottomColor, topHeight * t, topColor, heightBlendFactor);
	}

	public static Color HeightLerpColors(float bottomHeight, Color bottomColor, float topHeight, Color topColor, float heightBlendFactor, float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		return HeightBlend(bottomHeight * (1f - t), bottomColor, topHeight * t, topColor, heightBlendFactor);
	}

	public static void HeightBlend(float bottomHeight, ref Value bottomValue, float topHeight, ref Value topValue)
	{
		if (bottomHeight < topHeight)
		{
			bottomValue = topValue;
		}
	}

	public static Value HeightBlend(float bottomHeight, Value bottomValue, float topHeight, Value topValue)
	{
		if (bottomHeight < topHeight)
		{
			return topValue;
		}
		return bottomValue;
	}

	public static void HeightBlend(float bottomHeight, ref Value bottomValue, float topHeight, ref Value topValue, float heightBlendFactor)
	{
		float heightStart = Mathf.Max(bottomHeight, topHeight) - heightBlendFactor;
		float bottomLevel = Mathf.Max(bottomHeight - heightStart, 0f);
		float topLevel = Mathf.Max(topHeight - heightStart, 0f);
		bottomValue = (bottomValue * bottomLevel + topValue * topLevel) / (bottomLevel + topLevel);
	}

	public static Value HeightBlend(float bottomHeight, Value bottomValue, float topHeight, Value topValue, float heightBlendFactor)
	{
		float heightStart = Mathf.Max(bottomHeight, topHeight) - heightBlendFactor;
		float bottomLevel = Mathf.Max(bottomHeight - heightStart, 0f);
		float topLevel = Mathf.Max(topHeight - heightStart, 0f);
		return (bottomValue * bottomLevel + topValue * topLevel) / (bottomLevel + topLevel);
	}

	public static void HeightLerp(float bottomHeight, ref Value bottomValue, float topHeight, ref Value topValue, float heightBlendFactor, float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		bottomValue = HeightBlend(bottomHeight * (1f - t), bottomValue, topHeight * t, topValue, heightBlendFactor);
	}

	public static Value HeightLerpValues(float bottomHeight, Value bottomValue, float topHeight, Value topValue, float heightBlendFactor, float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		return HeightBlend(bottomHeight * (1f - t), bottomValue, topHeight * t, topValue, heightBlendFactor);
	}

	public static void HeightBlend(float bottomHeight, ref float bottomFloat, float topHeight, ref float topFloat)
	{
		if (bottomHeight < topHeight)
		{
			bottomFloat = topFloat;
		}
	}

	public static float HeightBlend(float bottomHeight, float bottomFloat, float topHeight, float topFloat)
	{
		if (bottomHeight < topHeight)
		{
			return topFloat;
		}
		return bottomFloat;
	}

	public static void HeightBlend(float bottomHeight, ref float bottomFloat, float topHeight, ref float topFloat, float heightBlendFactor)
	{
		float heightStart = Mathf.Max(bottomHeight, topHeight) - heightBlendFactor;
		float bottomLevel = Mathf.Max(bottomHeight - heightStart, 0f);
		float topLevel = Mathf.Max(topHeight - heightStart, 0f);
		bottomFloat = (bottomFloat * bottomLevel + topFloat * topLevel) / (bottomLevel + topLevel);
	}

	public static float HeightBlend(float bottomHeight, float bottomFloat, float topHeight, float topFloat, float heightBlendFactor)
	{
		float heightStart = Mathf.Max(bottomHeight, topHeight) - heightBlendFactor;
		float bottomLevel = Mathf.Max(bottomHeight - heightStart, 0f);
		float topLevel = Mathf.Max(topHeight - heightStart, 0f);
		return (bottomFloat * bottomLevel + topFloat * topLevel) / (bottomLevel + topLevel);
	}

	public static void HeightLerp(float bottomHeight, ref float bottomFloat, float topHeight, ref float topFloat, float heightBlendFactor, float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		bottomFloat = HeightBlend(bottomHeight * (1f - t), bottomFloat, topHeight * t, topFloat, heightBlendFactor);
	}

	public static float HeightLerpFloats(float bottomHeight, float bottomFloat, float topHeight, float topFloat, float heightBlendFactor, float t)
	{
		t = Mathf.Clamp(t, 0f, 1f);
		return HeightBlend(bottomHeight * (1f - t), bottomFloat, topHeight * t, topFloat, heightBlendFactor);
	}
}
