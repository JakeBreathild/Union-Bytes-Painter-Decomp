shader_type spatial;
render_mode blend_mix, depth_draw_never, unshaded;

varying vec4 color;

void vertex() {
	color = COLOR;
	POINT_SIZE = 8.0f;
	
}

void fragment() {
	ALBEDO = color.rgb;
	ALPHA = color.a;
}