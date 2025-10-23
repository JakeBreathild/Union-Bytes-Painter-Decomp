shader_type spatial;
render_mode blend_mix, depth_draw_never, unshaded;

uniform sampler2D maskTexture : hint_black;
uniform vec2 pixelSize = vec2(0.01, 0.01);
uniform vec4 first_color : hint_color = vec4(1.0);
uniform vec4 second_color : hint_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform bool animated = true;
uniform float stripe_direction : hint_range(0, 1) = 0.5;
uniform float alpha = 0.6;

uniform float scale = 1.0f;
uniform float width : hint_range(0, 2) = 0.05;
uniform float frequency = 50.0;



bool hasContraryNeighbour(vec2 uv, sampler2D texture) {
	
	float current_width = width * scale; 
	
	float i = -ceil(current_width);
	float j = ceil(current_width);
	float x1 = abs(i) > current_width ? current_width * sign(i) : i;
	float x2 = abs(j) > current_width ? current_width * sign(j) : j;
	float y1 = abs(i) > current_width ? current_width * sign(i) : i;
	float y2 = abs(j) > current_width ? current_width * sign(j) : j;
	
	vec2 xy1 = uv + pixelSize * vec2(x1, y1);
	vec2 xy2 = uv + pixelSize * vec2(x2, y2);
	
	if (xy1 != clamp(xy1, vec2(0.0), vec2(1.0)) || texture(texture, xy1).r == 0.0 || xy2 != clamp(xy2, vec2(0.0), vec2(1.0)) || texture(texture, xy2).r == 0.0) {
		return true;
	}
	
	return false;
}

void fragment() {
	vec2 uv = mod(UV, 1.0);
	float mask = texture(maskTexture, uv).r;
	
	ALBEDO = vec3(0.0);
	
	if ((mask > 0.0) == true && hasContraryNeighbour(uv, maskTexture)) {
		vec4 final_color = first_color;
		// Generate diagonal stripes
		if(animated)
			uv -= TIME / frequency;
		float pos = mix(uv.x, uv.y, stripe_direction) * frequency;
		float value = floor(fract(pos) + 0.5);
		if (mod(value, 2.0) == 0.0)
			final_color = second_color;

		ALBEDO.rgb = mix(ALBEDO.rgb, final_color.rgb, final_color.a);
		ALPHA += (1.0 - ALPHA) * final_color.a;
	}
	else {
		ALPHA = (1.0 - texture(maskTexture, uv).r) * alpha;
	}
	
}