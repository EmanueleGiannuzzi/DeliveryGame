shader_type spatial;

uniform sampler2D MainTex;
uniform float Longitude : hint_range(-180.0, 180.0);
uniform float Latitude : hint_range(-90.0, 90.0);
uniform float TestRadiusKM = 100.0;

const float EARTH_RADIUS_KM = 6371.0;

// Convert longitude and latitude to a 3D point on a sphere
vec3 longitude_latitude_to_point(vec2 longLat) {
    float longitude = radians(longLat.x);
    float latitude = radians(longLat.y);

    float y = sin(latitude);
    float r = cos(latitude);
    float x = sin(longitude) * r;
    float z = -cos(longitude) * r;
    return vec3(x, y, z);
}

// Convert a 3D point on a sphere to UV coordinates
vec2 point_to_uv(vec3 p) {
    float longitude = atan(p.x, -p.z);
    float latitude = asin(p.y);
    float u = (longitude / PI + 1.0) * 0.5;
    float v = (latitude / (PI * 0.5) + 1.0) * 0.5;
    return vec2(u, v);
}

// Calculate the great-circle distance between two points on a sphere
float distance_between_points_on_unit_sphere(vec3 a, vec3 b) {
    return acos(clamp(dot(a, b), -1.0, 1.0));
}

void fragment() {
	vec3 spherePos = normalize(INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	spherePos.x = -spherePos.x;
	spherePos.y = -spherePos.y;
    vec2 texCoord = point_to_uv(spherePos);
    vec4 col = texture(MainTex, texCoord);

    // Calculate the test point based on longitude and latitude
    vec2 coord = vec2(Longitude, Latitude);
    vec3 testPoint = longitude_latitude_to_point(coord);
    float dstKM = distance_between_points_on_unit_sphere(testPoint, spherePos) * EARTH_RADIUS_KM;

    // Apply color logic
    if (dstKM < 25.0) {
        ALBEDO = vec3(1.0); // White for very close range
    }
	else if (dstKM < TestRadiusKM) {
        ALBEDO = mix(col.rgb, vec3(1.0, 0.0, 0.0), 0.75); // Blend to red within the test radius
    }

    ALBEDO = col.rgb; // Default texture color
}
