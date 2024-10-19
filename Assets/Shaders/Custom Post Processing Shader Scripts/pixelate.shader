Shader "examples/week 10/pixelate"
{
    Properties
    {
        _MainTex ("render texture", 2D) = "white"{}
        _resolution ("resolution", int) = 256
    }

    SubShader
    {
        // Cull is a way to specify triangle faces we wish to render or not render
        // Off means we wont cull triangles (most shaders cull "Back" which means all triangles that don't face the camera)
        Cull Off

        // ZWrite is a way to specify in which cases you want to write to the depth buffer when rendering using this shader
        // Off means we will never write to the depth buffer
        ZWrite Off

        // ZTest is a way to specify how you want this shader to handle depth testing
        // Always means we don't depth test and always draw
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            int _resolution;

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

                float aspect = _ScreenParams.x / _ScreenParams.y;
                uv.x *= aspect;

                int res = floor(_resolution);

                uv = floor(uv * res) / res;

                uv.x /= aspect;

                color = tex2D(_MainTex, uv);

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
