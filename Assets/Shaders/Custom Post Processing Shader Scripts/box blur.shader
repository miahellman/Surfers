Shader "examples/week 10/box blur"
{
    Properties
    {
        _MainTex ("render texture", 2D) = "white"{}
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

            sampler2D _MainTex; float4 _MainTex_TexelSize;

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

            float3 sample (float2 uv)
            {
                return tex2D(_MainTex, uv);
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float2 uv = i.uv;
                float3 color = 0;

                //texel size
                float2 ts = _MainTex_TexelSize.xy;

                //offset dir
                float2 o = 0;
                float2 n = float2(0,1) * ts;
                float2 e = float2(1,0) * ts;
                float2 s = float2(0,-1) * ts;
                float2 w = float2(-1,0) * ts;
                float2 nw = float2(-1,1) * ts;
                float2 ne = float2(1,1) * ts;
                float2 se = float2(1,-1) * ts;
                float2 sw = -1 * ts;

                float3 result = 
                    sample(uv + nw) + sample(uv + n) + sample(uv + ne) +
                    sample(uv + w) + sample(uv + o) + sample(uv + e) +
                    sample(uv + sw) + sample(uv + s) + sample(uv + se);


                result /= 9;

                color = result;

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
