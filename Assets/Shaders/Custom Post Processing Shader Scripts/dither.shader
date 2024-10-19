Shader "examples/week 10/dither"
{
    Properties
    {
        _MainTex ("render texture", 2D) = "white" {}
        _ditherPattern ("dither pattern", 2D) = "gray" {}
        _threshold ("threshold", Range(-0.5, 0.5)) = 0
        _ditherScale ("dither scale", Range(1, 2)) = 1

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
            sampler2D _ditherPattern; float4 _ditherPattern_TexelSize;
            float _threshold;
            float _ditherScale;

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
                float color = 0;
                float2 uv = i.uv;
                
                float3 lc = float3(0.299, 0.587, 0.114);
                float greyscale = dot(tex2D(_MainTex, uv), lc);


                float2 ditherUV = (uv / _ditherPattern_TexelSize.zw) * _MainTex_TexelSize.zw;
                float ditherPattern = tex2D(_ditherPattern, (ditherUV / _ditherScale));

                color = greyscale;

                color = step(ditherPattern, greyscale + _threshold);

                return float4(color.rrr, 1.0);
            }
            ENDCG
        }

        //can run a contrast pass to make it look better 
    }
}
