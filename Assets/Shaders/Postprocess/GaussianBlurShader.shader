Shader "Unlit/GaussianBlurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _height ("Height", int) = 1
        _width ("Width", int) = 1
        _texelWidth ("Texel Width", float) = .0001
        _texelHeight ("Texel Height", float) = .0001
        _texelsPerUnit ("Texels per unit", float) = 100
        _stdev ("Standard Deviation", float) = .1
        _scale ("Blur Scale", float) = 1
        _horizontal ("Horizontal pass (0 or 1)", int) = 1
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
            float _texelsPerUnit;
            float _stdev;
            float _scale;
            int _horizontal;

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
                int width = _horizontal == 0 ? 0 : _width;
                int height = _horizontal != 0 ? 0 : _height;
                fixed4 col = 0;
                float weightSum = 0;
                for (int x = -width; x <= width; x++) {
                    for (int y = -height; y <= height; y++) {
                        float actualX = x*_scale;
                        float actualY = y*_scale;
                        /*float weight = 1.0/(x*x+y*y + 1);*/
                        float variance = _stdev*_stdev;
                        float weight = 1.0/(2*3.141*variance) * exp(-(actualX*actualX + actualY*actualY)/(2*variance));
                        weightSum += weight;
                        col += weight*fixed4(tex2D(_MainTex, i.uv + float4(actualX*_texelsPerUnit*_texelWidth, actualY*_texelsPerUnit*_texelHeight, 0, 0)));
                    }
                }

                float4 currentColor = tex2D(_MainTex, i.uv);
                float4 blurColor = col/weightSum;
                /*if (dot(currentColor, currentColor) > dot(blurColor, blurColor)) {*/
                /*    return currentColor;*/
                /*} else {*/
                    return blurColor;
                /*}*/
            }
            ENDCG
        }
    }
}
