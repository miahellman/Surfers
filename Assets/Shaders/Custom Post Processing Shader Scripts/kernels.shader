Shader "Custom/kernels"
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

            float3 convolution (float2 uv, float3x3 kernel) {
                // ts makes sure that we scale our offset by the size of the texel so we make sure to sample the next texel
                float2 ts = _MainTex_TexelSize.xy;
                
                float2 o  = 0;
                float2 n  = float2( 0,  1) * ts;
                float2 e  = float2( 1,  0) * ts;
                float2 s  = float2( 0, -1) * ts;
                float2 w  = float2(-1,  0) * ts;
                float2 nw = float2(-1,  1) * ts;
                float2 ne = float2( 1,  1) * ts;
                float2 se = float2( 1, -1) * ts;
                float2 sw = float2(-1, -1) * ts;
                
                float3 result =
                    sample(uv + nw) * kernel[0][0] + sample(uv + n ) * kernel[1][0] + sample(uv + ne) * kernel[2][0] +
                    sample(uv + w ) * kernel[0][1] + sample(uv + o ) * kernel[1][1] + sample(uv + e ) * kernel[2][1] +
                    sample(uv + sw) * kernel[0][2] + sample(uv + s ) * kernel[1][2] + sample(uv + se) * kernel[2][2];
                
                return result;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float3x3 boxBlurKernel = float3x3
                (
                    // box
                    0.11, 0.11, 0.11,
                    0.11, 0.11, 0.11,
                    0.11, 0.11, 0.11
                );

                float3x3 gaussianBlurKernel = float3x3
                (
                    // gaussian
                    0.0625, 0.125, 0.0625,
                    0.125, 0.25, 0.125,
                    0.0625, 0.125, 0.0625
                );

                float3x3 sharpenKernel = float3x3
                (
                    // sharpen
                     0, -1,  0,
                    -1,  5, -1,
                     0, -1,  0
                );

                float3x3 embossKernel = float3x3
                (
                    // emboss
                    -2, -1,  0,
                    -1,  1,  1,
                     0,  1,  2
                );

                float3x3 edgeDetectionKernel = float3x3
                (
                    // edge detection
                     1,  0, -1,
                     0,  0,  0,
                    -1,  0,  1
                );
                
                float3 color = convolution(i.uv, edgeDetectionKernel);
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}