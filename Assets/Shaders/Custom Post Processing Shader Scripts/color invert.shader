Shader "examples/week 10/invert color"
{
    Properties
    {
        //must use _MainTex
        _MainTex ("render texture", 2D) = "white" {}
    }

    SubShader
    {
        //no culling
        Cull Off
        //dont write to depth buffer
        ZWrite Off
        //specify how render pipeline handles depth testing 
        //want to always render no mater what happens in depth testing
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

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
                //sample tex w UVs 
                //subtract from 1 to invert colors
                color = 1 - tex2D(_MainTex, i.uv);

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
