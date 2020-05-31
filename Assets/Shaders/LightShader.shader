Shader "Unlit/LightShader"
{
    Properties
    {
        lightId ("Light ID", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Geometry-1" }
        LOD 100
        Cull off
        BlendOp Max
        GrabPass { "shadows" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/helpers.glsl"

            sampler2D shadows;
            int lightId;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 grabPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(shadows, i.grabPos);
                uint bf = colorToBitField(col);
                /*uint mask = 1 << mult(lightId);*/
                /*mask += 2*mask;*/
                /*mask += 2*mask;*/
                /*mask = ~mask;*/
                /*return col;*/
                /*if (mask == (0x3<<0)) {*/
                /*if ((colorToBitField(col) & mask) != 0) {// || colorToBitField(col) == 2) {*/
                /*if ((getMask(lightId) & bf) != 0) {*/
                    return fixed4(1,1,1,1);
                /*} else {*/
                /*    return fixed4(0,0,0,0);*/
                /*}*/
                /*uint mask = 0;*/
                /*for (int i = 0; i < lightIdMultiplier; i++) {*/
                /*    mask += 1 << (lightId*lightIdMultiplier+i);*/
                /*}*/
                /*if ((mask & colorToBitField(col)) == 0) {*/
                /*    return fixed4(0,0,0,1);*/
                /*} else {*/
                /*    return fixed4(1,1,1,1);*/
                /*}*/
            }
            ENDCG
        }
    }
}
