Shader "Unlit/BlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _height ("Height", int) = 1
        _width ("Width", int) = 1
        _texelWidth ("Texel Width", float) = .0001
        _texelHeight ("Texel Height", float) = .0001
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

            int _width;
            int _height;
            float _texelWidth;
            float _texelHeight;

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = 0;
                int count = 0;
                for (int x = -_width; x <= _width; x++) {
                    for (int y = -_height; y <= _height; y++) {
                        count++;
                        col += fixed4(tex2D(_MainTex, i.uv + float4(x*_texelWidth, y*_texelHeight, 0, 0)));
                    }
                }

                float4 currentColor = tex2D(_MainTex, i.uv);
                float4 blurColor = col/count;
                if (dot(currentColor, currentColor) > dot(blurColor, blurColor)) {
                    return currentColor;
                } else {
                    return blurColor;
                }
            }
            ENDCG
        }
    }
}
