shader_type spatial;

uniform sampler2D _ColWest : source_color;
uniform sampler2D _ColEast : source_color;
uniform sampler2D _NormalMapWest : hint_normal;
uniform sampler2D _NormalMapEast : hint_normal;
uniform sampler2D _LightMap : hint_default_white;
uniform sampler2D _LakeMask : hint_default_white;
uniform sampler2D _WaveNormalA : hint_normal;

uniform vec4 _ColWest_TexelSize;
uniform float _ShadowStrength;
uniform vec3 _ShadowEdgeCol;
uniform vec3 _ShadowInnerCol;
uniform float _WaveNormalScale;
uniform float _WaveStrength;
uniform float _Specular;
uniform float _Contrast;
uniform vec4 _AmbientNight;
uniform vec4 _CityLightAmbient;
uniform vec4 _FresnelCol;
uniform vec4 _TestParams;

// Custom functions
float calculateGeoMipLevel(vec2 texCoord, ivec2 texSize) {
    vec2 dx, dy;
    if (texCoord.x < 0.75 && texCoord.x > 0.25) {
        dx = dFdx(texCoord);
        dy = dFdy(texCoord);
    } else {
        dx = dFdx(mod(texCoord + vec2(0.5, 0.0), 1.0));
        dy = dFdy(mod(texCoord + vec2(0.5, 0.0), 1.0));
    }
    float mipMapWeight = 0.5;
    dx *= vec2(texSize) * mipMapWeight;
    dy *= vec2(texSize) * mipMapWeight;

    float maxSqrLength = max(dot(dx, dx), dot(dy, dy));
    float mipLevel = 0.5 * log2(maxSqrLength);
    const float maxMipLevel = 8.0;
    mipLevel = min(maxMipLevel, mipLevel);
    return mipLevel;
}

float calculateSpecular(vec3 normal, vec3 viewDir, vec3 dirToSun, float smoothness) {
    float specularAngle = acos(dot(normalize(dirToSun - viewDir), normal));
    float specularExponent = specularAngle / smoothness;
    return exp(-max(0.0, specularExponent) * specularExponent);
}

vec3 triplanarNormal(sampler2D normalMap, vec3 worldPos, vec3 pointOnUnitSphere, float scale, float offset, float strength) {
    vec3 worldNormalX = texture(normalMap, worldPos.yz * scale + offset).rgb * 2.0 - 1.0;
    vec3 worldNormalY = texture(normalMap, worldPos.xz * scale + offset).rgb * 2.0 - 1.0;
    vec3 worldNormalZ = texture(normalMap, worldPos.xy * scale + offset).rgb * 2.0 - 1.0;

    vec3 blendWeights = abs(worldPos);
    blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);

    vec3 blendedNormal = normalize(worldNormalX * blendWeights.x + worldNormalY * blendWeights.y + worldNormalZ * blendWeights.z);
    return normalize(blendedNormal * strength);
}

vec2 pointToLongitudeLatitude(vec3 p) {
    float longitude = atan(p.x, -p.z);
    float latitude = asin(p.y);
    return vec2(longitude, latitude);
}

vec2 longitudeLatitudeToUV(vec2 longLat) {
    float longitude = longLat.x;
    float latitude = longLat.y;

    float u = (longitude / PI + 1.0) * 0.5;
    float v = latitude / PI + 0.5;
    return vec2(u, v);
}

vec2 pointToUV(vec3 p) {
    vec2 longLat = pointToLongitudeLatitude(p);
    return longitudeLatitudeToUV(longLat);
}

void fragment() {
    vec3 pointOnUnitSphere = normalize(FRAGCOORD.xyz);
    vec2 texCoord = pointToUV(pointOnUnitSphere);
    float lightMap = texture(_LightMap, texCoord).r;
    float lakeMask = texture(_LakeMask, texCoord).r;

    vec3 detailNormal = vec3(0.0);
    vec3 unlitTerrainCol = vec3(0.0);

    if (texCoord.x < 0.5) {
        vec2 tileTexCoord = texCoord * 2.0;
		unlitTerrainCol = texture(_ColWest, tileTexCoord, calculateGeoMipLevel(tileTexCoord, ivec2(_ColWest_TexelSize.zw))).rgb;
        detailNormal = texture(_NormalMapWest, tileTexCoord).rgb;
    } else {
        vec2 tileTexCoord = (texCoord - vec2(0.5, 0.0)) * 2.0;
        unlitTerrainCol = textureLod(_ColEast, tileTexCoord, calculateGeoMipLevel(tileTexCoord, ivec2(_ColWest_TexelSize.zw))).rgb;
        detailNormal = texture(_NormalMapEast, tileTexCoord).rgb;
    }

    vec3 meshWorldNormal = normalize(NORMAL);
    detailNormal = normalize(detailNormal * 2.0 - 1.0);
    vec3 worldNormal = normalize(meshWorldNormal * 2.0 + detailNormal * 1.25);

    vec3 dirToSun = normalize(LIGHT_POSITION.xyz);
    vec3 viewDir = normalize(FRAGCOORD.xyz - _WorldSpaceCameraPos.xyz);

    vec3 waveA = triplanarNormal(_WaveNormalA, FRAGCOORD.xyz, pointOnUnitSphere, _WaveNormalScale, 0.0, _WaveStrength);
    float lakeSpecular = calculateSpecular(waveA, viewDir, dirToSun, _Specular) * lakeMask;

    float shadows = LIGHT_ATTENUATION();
    vec3 shadowCol = mix(_ShadowEdgeCol, _ShadowInnerCol, saturate((1.0 - shadows) * 1.5));
    shadows = mix(1.0, shadows, _ShadowStrength);

    float fakeLighting = pow(dot(worldNormal, pointOnUnitSphere), 3.0);


::contentReference[oaicite:2]{index=2}

