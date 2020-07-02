Shader "Unlit/ColorCorrect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float applyCurve(float channel) {
                return pow((1-1/(6*channel+1)), 2);
            }

            float luminance(float4 color) {
                return color.r*.299 + color.g*.587 + color.b*.114;
            }

            float maxChannel(float4 color) {
                float3 scaled = float3(color.r*.299, color.g*.587, color.b*.114);
                if (scaled.r > scaled.g && scaled.r > scaled.b) {
                    return color.r;
                } else if (scaled.g > scaled.r && scaled.g > scaled.b) {
                    return color.g;
                } else {
                    return color.b;
                }
            }

            float4 correct(float4 color) {
                float mc = maxChannel(color);
                return float4(applyCurve(color.r), applyCurve(color.g), applyCurve(color.b), color.a);
                /*return applyCurve(luminance(color))*(color/mc);*/
            }

            fixed4 frag (v2f i) : SV_Target {
                return correct(tex2D(_MainTex, i.uv));
            }
            ENDCG
        }
    }
}
