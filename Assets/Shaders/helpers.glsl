#ifndef ASSETS_SHADERS_HELPERS_GLSL
#define ASSETS_SHADERS_HELPERS_GLSL

//float4 idToColor(int id) {
//    id = 1 << id;
//    float4 ret = float4(0,0,0,0);
//
//    ret[id/4] = (1 << (id%8))/(1 << 8);
//    return ret;
//}

//fixed4 lightIdToBitmap(uint id) {
//    for (int i = 0; i < maxOverlapPow
//}

fixed4 bitFieldToColor(uint bitField) {
    uint b1 = bitField % 256;
    uint b2 = (bitField / 256) % 256;
    uint b3 = (bitField / (256*256)) % 256;
    uint b4 = (bitField / (256*256*256)) % 256;

    return fixed4(b1, b2, b3, b4)/(fixed)256;
}

uint colorToBitField(fixed4 color) {
    uint b1 = round(color.x*256);
    uint b2 = round(color.y*256);
    uint b3 = round(color.z*256);
    uint b4 = round(color.w*256);

    return b1 + (b2 + (b3 + b4*256)*256)*256;
}

uint mult(uint lightId) {
    return lightId * 3;
}

#endif
