shader_type spatial;

uniform sampler2D _texture : filter_linear;

uniform sampler2D height_map;
uniform sampler2D normal_map;
uniform float height_factor : hint_range(0.0, 10.0) = 2; // Adjust height effect

varying vec3 world_position;

vec2 pointOnSphereToUV(vec3 p) {
	p = normalize(p);

	float longitude = atan(p.x, -p.z);
	float latitude = asin(p.y);

	float u = (longitude / PI + 1.0) / 2.0;
	float v = latitude / PI + 0.5;

	return vec2(u, v);
}

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	world_position.y = -world_position.y;
	world_position.z = -world_position.z;

	vec2 uv = pointOnSphereToUV(world_position);

	if(uv.y > 0.0001 && uv.y < 0.9999) {
	    float height = texture(height_map, uv).r;
	    VERTEX.xyz += NORMAL * height * height_factor;
	}

}

void fragment() {
    vec2 uv = pointOnSphereToUV(world_position);
	vec4 tex_color = texture(_texture, uv);

   	ALBEDO = tex_color.rgb;

  	NORMAL_MAP = texture(normal_map, uv).xyz;
}

/*void light() {
    DIFFUSE_LIGHT += clamp(dot(NORMAL, LIGHT), 0.0, 1.0) * ATTENUATION * LIGHT_COLOR;
}*/
