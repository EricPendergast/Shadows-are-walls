Shader "Unlit/ShadowShader"
{
    Properties
    {
        lightId ("Light ID", Int) = 0
    }
    SubShader
    {

        Tags { "RenderType"="Opaque" "Queue" = "Geometry+1" }
        LOD 100
        Cull off
        Blend One One

        GrabPass { "lights" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/helpers.glsl"

            sampler2D lights;
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
                uint mask = 1 << lightId;
                /*mask = ~mask;*/
                fixed4 col = tex2D(lights, i.grabPos);
                /*fixed4 prev = tex2D(_GrabTexture, i.grabPos);*/
                /*return bitFieldToColor(colorToBitField(prev) + (1 << lightId));*/
                /*return fixed4((1 << lightId*lightIdMultiplier)/256.0, 0,0,0);*/
                /*return bitFieldToColor(1 << lightId);*/
                /*if ((~mask | colorToBitField(col)) == 0) {*/
                if ((colorToBitField(col)&mask) != 0) {
                    return fixed4(1,1,1,1);
                } else {
                    return fixed4(0,0,0,0);
                }
            }
            ENDCG
        }
    }
}
