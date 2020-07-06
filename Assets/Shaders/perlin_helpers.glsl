#ifndef ASSETS_PERLIN_HELPERS_GLSL
#define ASSETS_PERLIN_HELPERS_GLSL

float fract(float f) {
    return f - floor(f);
}

float rand(float3 co){
    return fract(sin(dot(co.xyz, float3(12.9898,78.233,144.7272))) * 43758.5453);
}

float rand(float2 co, float seed){
    return rand(float3(co, seed));
}
float rand(float2 co) {
    return rand(co, 0);
}

float2 rand2D(float2 co) {
    return float2(rand(co, 1), rand(co, 2));
}
float2 rand2D(float2 co, float seed) {
    return float2(rand(co, seed), rand(co, 2*seed));
}

float interpolate(float a, float b, float t) {
    return (b-a)*t*t*t*(6*t*t-15*t+10) + a;
}

float2 interpolate(float2 a, float2 b, float t) {
    return (b-a)*t*t*t*(6*t*t-15*t+10) + a;
}

float4 interpolate(float4 a, float4 b, float t) {
    if (t < 0 || t > 1) {
        return float4(1,1,0,1);
    }
    return (b-a)*t*t*t*(6*t*t-15*t+10) + a;
}

float2 rotate(float2 p, float rad) {
    float2x2 rot = float2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
    return mul(rot, p);
}

float lengthsq(float2 vec) {
    return dot(vec, vec);
}

float perlin(float2 pos) {
    float2 ll = floor(pos);
    float2 lr = ll + float2(1,0);
    float2 ul = ll + float2(0,1);
    float2 ur = ll + float2(1,1);

    float2 inCell = pos - ll;
    float top = interpolate(rand(ul), rand(ur), inCell.x);
    float bot = interpolate(rand(ll), rand(lr), inCell.x);
    return interpolate(bot, top, inCell.y);
}

float2 perlin2D(float2 pos) {
    float2 ll = floor(pos);
    float2 lr = ll + float2(1,0);
    float2 ul = ll + float2(0,1);
    float2 ur = ll + float2(1,1);

    float2 inCell = pos - ll;
    float top = interpolate(rand2D(ul), rand2D(ur), inCell.x);
    float bot = interpolate(rand2D(ll), rand2D(lr), inCell.x);
    return interpolate(bot, top, inCell.y);
}

float2 naturalPerlin2D(float2 pos) {
    return perlin2D(pos) + perlin2D(rotate(pos, 3.141/6));
}

float blend(float color1, float color2, float alpha) {
    return pow(alpha * sqrt(color1) + (1-alpha) * sqrt(color2), 2);
}
// Blends as if c1 is in front of c2
float4 blend(float4 c1, float4 c2) {
    return float4(  blend(c1.r, c2.r, c1.a),
                    blend(c1.g, c2.g, c1.a),
                    blend(c1.b, c2.b, c1.a),
                    max(c1.a, c2.a));
}

float lin(float a, float b, float x) {
    return a*(1-x) + b*x;
}

float2 lin(float2 a, float2 b, float x) {
    return a*(1-x) + b*x;
}

float3 lin(float3 a, float3 b, float x) {
    return a*(1-x) + b*x;
}

float4 lin(float4 a, float4 b, float x) {
    return a*(1-x) + b*x;
}

#endif
