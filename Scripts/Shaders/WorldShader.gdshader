shader_type spatial;

uniform sampler2D world_texture;
uniform sampler2D height_map;
uniform float height_factor : hint_range(0.0, 100.0) = 50.0; // Adjust height effect

void vertex() {
    // Sample the height map using the UV coordinates of the vertex
    float height = texture(height_map, UV).r; // height is typically stored in the red channel
    // Apply the height factor to the displacement
    VERTEX.xyz += NORMAL * height * height_factor;
}

void fragment() {
    vec4 tex_color = texture(world_texture, UV);
    ALBEDO = tex_color.rgb;
    ALPHA = tex_color.a;
}
