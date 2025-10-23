shader_type spatial;
render_mode blend_mix, depth_draw_never, unshaded;

uniform sampler2D nodeTexture : hint_albedo;

varying vec4 color;

void vertex() {
	color = COLOR;
}

void fragment() {
	vec4 fragColor = texture(nodeTexture, UV);
	
	ALBEDO = fragColor.rgb * color.rgb;
	ALPHA = fragColor.a * color.a;
}