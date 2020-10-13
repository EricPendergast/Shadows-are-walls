#ifndef CUSTOM_POST_FX_PASSES_INCLUDED
#define CUSTOM_POST_FX_PASSES_INCLUDED

struct Varyings {
    float4 positionCS : SV_POSITION;
    float2 fxUV : VAR_FX_UV;
};

Varyings DefaultPassVertex(uint vertexID : SV_VertexID) {
    Varyings output;
    output.positionCS = float4(
        vertexID <= 1 ? -1.0 : 3.0,
        vertexID == 1 ? 3.0 : -1.0,
        0.0, 1.0
    );

    output.fxUV = float2(
        vertexID <= 1 ? 0.0 : 2.0,
        vertexID == 1 ? 2.0 : 0.0
    );

    if (_ProjectionParams.x < 0.0) {
        output.fxUV.y = 1.0 - output.fxUV.y;
    }

    return output;
}

TEXTURE2D(_PostFXSource);
SAMPLER(sample_linear_clamp);

float4 GetSource(float2 fxUV) {
    return SAMPLE_TEXTURE2D(_PostFXSource, sample_linear_clamp, fxUV);
}


float4 CopyPassFragment(Varyings input) : SV_TARGET {
    return GetSource(input.fxUV);
}

#endif
