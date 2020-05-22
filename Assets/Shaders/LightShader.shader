Shader "Unlit/LightShader"
{
    Properties
    {
        lightId ("Light ID", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Geometry" }
        LOD 100
        Cull off

        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/helpers.glsl"

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
                /*uint mask = 1 << mult(lightId);*/
                /*mask += 2*mask;*/
                /*mask += 2*mask;*/
                /*mask = ~mask;*/
                /*[>return col;<]*/
                /*[>if (mask == (0x3<<0)) {<]*/
                /*if ((colorToBitField(col) & mask) != 0) {// || colorToBitField(col) == 2) {*/
                /*    return fixed4(1,0,0,1);*/
                /*} else {*/
                /*    return fixed4(.5,.5,.5,1);*/
                /*}*/
                /*uint mask = 0;*/
                /*for (int i = 0; i < lightIdMultiplier; i++) {*/
                /*    mask += 1 << (lightId*lightIdMultiplier+i);*/
                /*}*/
                return bitFieldToColor(1 << lightId);
            }
            ENDCG
        }
    }
}
