shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx;

uniform vec4 color = vec4(1.0);
uniform float roughness = 1.0;
uniform float metallicity = 0.0;
uniform vec4 emission = vec4(0.0);

uniform float specular = 0.8;
uniform float emissionStrength = 3.0;


void fragment() {
	ALBEDO = color.rgb;
	ALPHA = color.a * COLOR.a;
	ROUGHNESS = roughness;
	SPECULAR = specular - roughness * specular;
	METALLIC = metallicity;
	EMISSION = emission.rgb * emissionStrength;
}