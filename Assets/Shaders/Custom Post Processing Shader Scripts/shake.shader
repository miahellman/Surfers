Shader "Custom/shake"
{
    Properties
    {
        _MainTex ("render texture", 2D) = "white"{}
        _shakeScale ("shake scale", Range(0,1)) = 0

     
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
            #define FactorA float2(100.0,100.0)
            #define FactorB float2(1.0,1.0)
            #define FactorScale float2(0.008,0.008)


            sampler2D _MainTex;

            float _shakeScale;
            

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

                //shake
                // Normalized pixel coordinates (from 0 to 1)
                uv = i.vertex.xy/_ScreenParams.xy;
	            float2 uniA = FactorA;
                float2 uniB = FactorB;
                float uniScale = _shakeScale;
    
                float2 dt = float2(0.0, 0.0);

                dt.x = sin(_Time.z/10 * uniA.x + uniB.x) * uniScale;
                dt.y = cos(_Time.z/10 * uniA.y + uniB.y) * uniScale;

                // Output to screen by modifying uv
                // wobble in circle shape
                color = tex2D(_MainTex,uv + dt);
 
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
