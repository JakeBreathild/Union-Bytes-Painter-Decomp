shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_disabled, diffuse_burley, specular_schlick_ggx;


// Constants
const float alphaScissorThreshold = 0.9;
const float gamma = 3.2;
const vec4 colorWhite = vec4(1.0, 1.0, 1.0, 1.0);

// Textures
uniform sampler2D colorMapTexture : hint_albedo;
uniform sampler2D normalMapTexture : hint_normal;
uniform sampler2D roughnessMapTexture : hint_white;
uniform sampler2D metallicityMapTexture : hint_black;
uniform sampler2D emissionMapTexture : hint_black_albedo;

// Modifiers 
uniform float specular = 0.8;
uniform float emissionStrength = 3.0;
uniform float uvScale = 1.0;

// Preview
uniform int previewBlendingMode = 0;
uniform sampler2D previewMaskTexture : hint_albedo;
uniform bool previewColorEnabled = true;
uniform vec4 previewColor : hint_color;
uniform bool previewRoughnessEnabled = true;
uniform float previewRoughness = 1.0;
uniform bool previewMetallicityEnabled = true;
uniform float previewMetallicity = 0.0;
uniform bool previewEmissionEnabled = true;
uniform vec4 previewEmission : hint_color;


// Color Space Converter
float convertFromLinear(float theLinearValue) {
	return theLinearValue <= 0.0031308
	? theLinearValue * 12.92
	: pow(theLinearValue, 1.0 / gamma) * 1.055 - 0.055;
}


// Blending Functions
float getV(vec4 color) {
	return max(color.r, max(color.g, color.b));
}

void normalBlendColor(inout vec4 bottomColor, in vec4 topColor, float blendStrength) {
	float bottomAlpha = bottomColor.a;
	float topAlpha = topColor.a * blendStrength;
	
	float num = 1.0f - topAlpha;
	bottomColor.a = bottomColor.a * num + topAlpha;
	if (bottomColor.a > 0.0f)
	{
		bottomColor.r = (bottomColor.r * bottomAlpha * num + topColor.r * topAlpha) / bottomColor.a;
		bottomColor.g = (bottomColor.g * bottomAlpha * num + topColor.g * topAlpha) / bottomColor.a;
		bottomColor.b = (bottomColor.b * bottomAlpha * num + topColor.b * topAlpha) / bottomColor.a;
	}
	bottomColor.a = topAlpha + (bottomAlpha * (1.0f - blendStrength));
}

void normalBlendFloat(inout float bottomFloat, in float topFloat, float blendStrength) {
	bottomFloat = (topFloat * blendStrength) + (bottomFloat * (1.0f - blendStrength));
}

void alphaBlendColor(inout vec4 bottomColor, in vec4 topColor, float blendStrength) {
	float bottomAlpha = bottomColor.a;
	float topAlpha = topColor.a * blendStrength;
	
	float num = 1.0f - topAlpha;
	bottomColor.a = bottomColor.a * num + topAlpha;
	if (bottomColor.a > 0.0f)
	{
		bottomColor.r = (bottomColor.r * bottomAlpha * num + topColor.r * topAlpha) / bottomColor.a;
		bottomColor.g = (bottomColor.g * bottomAlpha * num + topColor.g * topAlpha) / bottomColor.a;
		bottomColor.b = (bottomColor.b * bottomAlpha * num + topColor.b * topAlpha) / bottomColor.a;
	}
}

void alphaBlendFloat(inout float bottomFloat, in float topFloat, float blendStrength) {
	bottomFloat = (topFloat * blendStrength) + (bottomFloat * (1.0f - blendStrength));
}

void multiplicationBlendColor(inout vec4 bottomColor, in vec4 topColor, float blendStrength) {
	float bottomAlpha = bottomColor.a;
	
	blendStrength *= topColor.a;
	bottomColor = (bottomColor * topColor * blendStrength) + (bottomColor * (1.0f - blendStrength));
	bottomColor.a = bottomAlpha;
}

void multiplicationBlendFloat(inout float bottomFloat, in float topFloat, float blendStrength) {
	bottomFloat = (bottomFloat * topFloat * blendStrength) + (bottomFloat * (1.0f - blendStrength));
}

void additionBlendColor(inout vec4 bottomColor, in vec4 topColor, float blendStrength) {
	float bottomAlpha = bottomColor.a;
	bottomColor += topColor * blendStrength * topColor.a;
	bottomColor.a = bottomAlpha;
	clamp(bottomColor, 0.0, 1.0);
}

void additionBlendFloat(inout float bottomFloat, in float topFloat, float blendStrength) {
	bottomFloat += topFloat * blendStrength;
	clamp(bottomFloat, 0.0, 1.0);
}

void subtractionBlendColor(inout vec4 bottomColor, in vec4 topColor, float blendStrength) {
	float bottomAlpha = bottomColor.a;
	bottomColor -= topColor * blendStrength * topColor.a;
	bottomColor.a = bottomAlpha;
	clamp(bottomColor, 0.0, 1.0);
}

void subtractionBlendFloat(inout float bottomFloat, in float topFloat, float blendStrength) {
	bottomFloat -= topFloat * blendStrength;
	clamp(bottomFloat, 0.0, 1.0);
}

void screenBlendColor(inout vec4 bottomColor, in vec4 topColor, float blendStrength) {
	blendStrength *= topColor.a;
	bottomColor = ((colorWhite - (colorWhite - bottomColor) * (colorWhite - topColor)) * blendStrength) + (bottomColor * (1.0f - blendStrength));
}

void screenBlendFloat(inout float bottomFloat, in float topFloat, float blendStrength) {
	bottomFloat = ((1.0f - (1.0f - bottomFloat) * (1.0f - topFloat)) * blendStrength) + (bottomFloat * (1.0f - blendStrength));
}

void minimumBlendColor(inout vec4 bottomColor, in vec4 topColor, float blendStrength) {
	 if (getV(bottomColor) >= getV(topColor)) {
		blendStrength *= topColor.a;
		bottomColor = (topColor * blendStrength) + (bottomColor * (1.0f - blendStrength));
	}
}

void minimumBlendFloat(inout float bottomFloat, in float topFloat, float blendStrength) {
	if (bottomFloat >= topFloat)
		bottomFloat = (topFloat * blendStrength) + (bottomFloat * (1.0f - blendStrength));
}

void maximumBlendColor(inout vec4 bottomColor, in vec4 topColor, float blendStrength) {
	if (getV(bottomColor) <= getV(topColor)) {
		blendStrength *= topColor.a;
		bottomColor = (topColor * blendStrength) + (bottomColor * (1.0f - blendStrength));
	}
}

void maximumBlendFloat(inout float bottomFloat, in float topFloat, float blendStrength) {
	if (bottomFloat <= topFloat)
		bottomFloat = (topFloat * blendStrength) + (bottomFloat * (1.0f - blendStrength));
}


// Vertex
varying vec2 uv;
void vertex() {
	uv = UV * uvScale;
}


// Fragment
void fragment() {
	vec4 color = texture(colorMapTexture, uv);
	vec4 normal = texture(normalMapTexture, uv);
	float roughness = texture(roughnessMapTexture, uv).r;
	float metallicity = texture(metallicityMapTexture, uv).r;
	vec4 emission = texture(emissionMapTexture, uv);
	
	// Preview Blending
	float blending = convertFromLinear(((texture(previewMaskTexture, uv).r - 0.5) * 1.4) + 0.5);
	if (blending > 0.01){
		switch (previewBlendingMode) {
			
			// NORMAL
			case 0:
				if (previewColorEnabled)
					normalBlendColor(color, previewColor, blending);
				if (previewRoughnessEnabled)
					normalBlendFloat(roughness, previewRoughness, blending);
				if (previewMetallicityEnabled)
					normalBlendFloat(metallicity, previewMetallicity, blending);
				if (previewEmissionEnabled)
					normalBlendColor(emission, previewEmission, blending);
				break;
				
			// ALPHABLEND
			case 7:
				if (previewColorEnabled)
					alphaBlendColor(color, previewColor, blending);
				if (previewRoughnessEnabled)
					alphaBlendFloat(roughness, previewRoughness, blending);
				if (previewMetallicityEnabled)
					alphaBlendFloat(metallicity, previewMetallicity, blending);
				if (previewEmissionEnabled)
					alphaBlendColor(emission, previewEmission, blending);
				break;
				
			// MULTIPLICATION
			case 3:
				if (previewColorEnabled)
					multiplicationBlendColor(color, previewColor, blending);
				if (previewRoughnessEnabled)
					multiplicationBlendFloat(roughness, previewRoughness, blending);
				if (previewMetallicityEnabled)
					multiplicationBlendFloat(metallicity, previewMetallicity, blending);
				if (previewEmissionEnabled)
					multiplicationBlendColor(emission, previewEmission, blending);
				break;
			
			// ADDITION
			case 1:
				if (previewColorEnabled)
					additionBlendColor(color, previewColor, blending);
				if (previewRoughnessEnabled)
					additionBlendFloat(roughness, previewRoughness, blending);
				if (previewMetallicityEnabled)
					additionBlendFloat(metallicity, previewMetallicity, blending);
				if (previewEmissionEnabled)
					additionBlendColor(emission, previewEmission, blending);
				break;
			
			// SUBTRACTION
			case 2:
				if (previewColorEnabled)
					subtractionBlendColor(color, previewColor, blending);
				if (previewRoughnessEnabled)
					subtractionBlendFloat(roughness, previewRoughness, blending);
				if (previewMetallicityEnabled)
					subtractionBlendFloat(metallicity, previewMetallicity, blending);
				if (previewEmissionEnabled)
					subtractionBlendColor(emission, previewEmission, blending);
				break;
			
			// SCREEN
			case 6:
				if (previewColorEnabled)
					screenBlendColor(color, previewColor, blending);
				if (previewRoughnessEnabled)
					screenBlendFloat(roughness, previewRoughness, blending);
				if (previewMetallicityEnabled)
					screenBlendFloat(metallicity, previewMetallicity, blending);
				if (previewEmissionEnabled)				
					screenBlendColor(emission, previewEmission, blending);
				break;
			
			// MINIMUM
			case 4:
				if (previewColorEnabled)
					minimumBlendColor(color, previewColor, blending);
				if (previewRoughnessEnabled)
					minimumBlendFloat(roughness, previewRoughness, blending);
				if (previewMetallicityEnabled)
					minimumBlendFloat(metallicity, previewMetallicity, blending);
				if (previewEmissionEnabled)
					minimumBlendColor(emission, previewEmission, blending);
				break;
			
			// MAXIMUM
			case 5:
				if (previewColorEnabled)
					maximumBlendColor(color, previewColor, blending);
				if (previewRoughnessEnabled)
					maximumBlendFloat(roughness, previewRoughness, blending);
				if (previewMetallicityEnabled)
					maximumBlendFloat(metallicity, previewMetallicity, blending);
				if (previewEmissionEnabled)
					maximumBlendColor(emission, previewEmission, blending);
				break;
			
			default:
				if (previewColorEnabled)
					color = mix(color, previewColor, blending);
				if (previewRoughnessEnabled)
					roughness  = mix(roughness, previewRoughness, blending);
				if (previewMetallicityEnabled)
					metallicity  = mix(metallicity, previewMetallicity, blending);
				if (previewEmissionEnabled)
					emission  = mix(emission, previewEmission, blending);
				break;
		}
	}
	
	if (color.a < alphaScissorThreshold) 
		discard;
	
	// Set
	ALBEDO = color.rgb;
	ROUGHNESS = roughness;
	SPECULAR = specular - roughness * specular;
	METALLIC = metallicity;
	NORMALMAP = normal.rgb;
	NORMALMAP_DEPTH = 1.0;
	TRANSMISSION = 2.0 * emission.rgb;
}