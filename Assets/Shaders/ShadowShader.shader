Shader "Unlit/ShadowShader"
{
    Properties
    {
        lightId ("Light ID", Int) = 0
    }
    SubShader
    {

        Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
        LOD 100
        Cull off
        Blend off
        /*BlendOp LogicalOr*/
        Blend One One

        /*GrabPass { }*/

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/helpers.glsl"

            int lightId;
            sampler2D _GrabTexture;

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
                /*fixed4 prev = tex2D(_GrabTexture, i.grabPos);*/
                /*return bitFieldToColor(colorToBitField(prev) + (1 << lightId));*/
                /*return fixed4((1 << lightId*lightIdMultiplier)/256.0, 0,0,0);*/
                return bitFieldToColor(getBit(lightId));
            }
            ENDCG
        }
    }
}
