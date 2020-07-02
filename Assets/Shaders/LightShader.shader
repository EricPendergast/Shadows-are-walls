Shader "Unlit/LightShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _IntensityMultiplier ("Intensity Multiplier", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Blend One One
        LOD 100
        Cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _IntensityMultiplier;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color*_IntensityMultiplier;
            }
            ENDCG
        }
    }
}
