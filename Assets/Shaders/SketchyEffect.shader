Shader "Unlit/SketchyEffect"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _LightTex ("Bloom Texture", 2D) = "white" {}
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
                float4 world_position : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _lightTex;
            float4 _MainTex_ST;
            float4x4 cameraToWorld;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                v.vertex = float4(v.vertex.x*unity_OrthoParams.x*2, v.vertex.y*unity_OrthoParams.y*2, 0, 1);
                o.world_position = mul(unity_CameraToWorld, v.vertex);
                /*o.world_position = v.vertex;*/
                return o;
            }

            float trueMod(float n, float modulus) {
                return (n % modulus + modulus) % modulus;
            }

            // If intensity is 1, the this returns 1
            float strirationFcn(float pos, float width, float intensity) {
                /*intensity = pow(intensity, .50);*/
                /*pos = trueMod(pos, width)/width;*/
                /*float distFromCenter = abs(pos - .5);*/
                /*if (distFromCenter < intensity)*/
                /*    return 0;*/
                /*else {*/
                /*    [>return distFromCenter;<]*/
                /*    [>if (distFromCenter < intensity*.5) {<]*/
                /*    [>    return (intensity - distFromCenter)/.5;<]*/
                /*    [>}<]*/
                /*    return 1;*/
                /*}*/
                float interpol = (sin(pos/width*6.28))/2 + .5;
                interpol = clamp(interpol + (intensity-.5)*2, 0, 1);
                return interpol;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 brightColor = tex2D(_MainTex, i.uv);
                fixed4 lightValue = tex2D(_lightTex, i.uv);

                float scale = 50;
                float intensity = lightValue.r + lightValue.g + lightValue.b;
                intensity = clamp(1.1 - 1.2/(1+intensity*intensity), 0, 1);
                float striration = (strirationFcn(i.world_position.x, .0025*scale, intensity) +
                                 strirationFcn((i.world_position.x + i.world_position.y)/2, .025*scale, intensity))/2;

                /*if (trueMod(i.world_position.x ,intensity) < .5) {*/
                /*    */
                /*} else {*/
                /*    mask = float4(0,0,0,1);*/
                /*}*/
                /*if (any(col2.rgb > float3(0,0,0))) {*/
                /*    [>return col1 + col2;<]*/
                /*    [>return float4(0,0,0,1);<]*/
                /*    return col1 * mask;*/
                /*} else {*/
                /*    return col1;*/
                /*}*/
                fixed4 darkColor = brightColor/2;
                if (brightColor.a != 0) {
                    return lerp(darkColor, brightColor, striration);
                } else {
                    return brightColor + lightValue/2;
                }
            }
            ENDCG
        }
    }
}
