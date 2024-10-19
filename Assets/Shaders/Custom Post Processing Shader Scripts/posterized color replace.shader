Shader "Custom/posterized color replace"
{
    Properties 
    {
        _MainTex ("render texture", 2D) = "white"{}
        _steps ("steps", Range(1,16)) = 16

        _recolor ("recolor reference", 2D) = "gray" {}
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
            
            sampler2D _MainTex;
            int _steps;

            sampler2D _recolor;

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

                float3 tex = tex2D(_MainTex, uv);
                //weights per rgb channel
                float3 lc = float3(.299, 0.587, 0.114); //luminance coefficient
                //just math - no need to normalize
                float greyscale = dot(tex, lc); 

                greyscale = floor(greyscale * _steps) / _steps;

               // color = tex2D(_recolor, float2(greyscale + (_Time.z * 0.2), 0.5));
                color = tex2D(_recolor, float2(greyscale, 0.5));

                

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
