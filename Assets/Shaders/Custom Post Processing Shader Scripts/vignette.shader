Shader "Custom/vignette"
{
    Properties
    {
        _MainTex ("render texture", 2D) = "white"{}

        _vr("Vignette Radius", Range(0.0, 1.0)) = 1.0
        _vs("Vignette Softness", Range(0.0, 1.0)) = 0.5

        _Color("Color", Color) = (1, 1, 1, 1)
        
        _saturation("saturation", Range(0.0, 1.0)) = 0.5



    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define MAX_OFFSET 0.15


            sampler2D _MainTex;

            float _vr;
            float _vs;

            float4 _Color;
            
            float _saturation;

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
               float3 color = 0;
                
                float2 uv = i.uv;

                color = tex2D(_MainTex, uv);

                //vignette effect
                float distFromCenter = distance(uv.xy, float2(0.5, 0.5));
                float vignette = smoothstep(_vr, _vr - _vs, distFromCenter);

                //add vignette
                color = saturate(color * vignette);

                //overtone color
                color *= _Color;

                //desaturate image
                color = lerp(color.rrr, color, _saturation);
 
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
