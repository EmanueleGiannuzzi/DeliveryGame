shader_type spatial;

uniform sampler2D _texture_left : filter_linear, repeat_enable;
uniform sampler2D _texture_right : filter_linear, repeat_enable;

uniform sampler2D height_map : filter_nearest;
uniform sampler2D normal_map_left : filter_linear, repeat_enable;
uniform sampler2D normal_map_right : filter_linear, repeat_enable;

uniform float height_factor : hint_range(0.0, 10.0) = 2;

varying vec3 world_position;

vec2 pointOnSphereToUV(vec3 p) {
    p = normalize(p);

    float longitude = atan(p.x, -p.z);
    float latitude = asin(p.y);

    float u = (longitude / PI + 1.0) / 2.0;
    float v = latitude / PI + 0.5;

    return vec2(u, v);
}

vec4 get_split_texture(sampler2D tex_left, sampler2D tex_right, vec2 uv) {
    if (uv.x < 0.5) {
        return texture(tex_left, uv * vec2(2.0, 1.0));
    } else {
        return texture(tex_right, (uv - vec2(0.5, 0.0)) * vec2(2.0, 1.0));
    }
}

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_position.y = -world_position.y;
    world_position.z = -world_position.z;

    vec2 uv = pointOnSphereToUV(world_position);

    if (uv.y > 0.0001 && uv.y < 0.9999) {
        float height = texture(height_map, uv).r;
        VERTEX.xyz += NORMAL * height * height_factor;
    }
}

void fragment() {
    vec2 uv = pointOnSphereToUV(world_position);

    vec4 tex_color = get_split_texture(_texture_left, _texture_right, uv);
    vec3 normal_map = get_split_texture(normal_map_left, normal_map_right, uv).xyz;

    ALBEDO = tex_color.rgb;
    NORMAL_MAP = normal_map;
}

/*void light() {
    DIFFUSE_LIGHT += clamp(dot(NORMAL, LIGHT), 0.0, 1.0) * ATTENUATION * LIGHT_COLOR;
}*/