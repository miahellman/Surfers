Shader "Custom/posterized pixelated"
{

    //change sprite tint to: 898989 hex to see shading on player sprite

    Properties
    {
        _MainTex ("render texture", 2D) = "white"{}

        _resolution ("resolution", int) = 256

        _vr("Vignette Radius", Range(0.0, 1.0)) = 1.0
        _vs("Vignette Softness", Range(0.0, 1.0)) = 0.5

        _color1 ("background color", Color) = (0.06, 0.22, 0.06, 1.0)
        _color2 ("midground color", Color) = (0.19, 0.38, 0.19, 1.0)
        _color3 ("midlight color", Color) = (0.54, 0.67, 0.06, 1.0)
        _color4 ("highlight color", Color) = (0.6, 0.74, 0.06, 1.0)
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

            int _resolution;

            float _vr;
            float _vs;

            float4 _color1;
            float4 _color2;
            float4 _color3;
            float4 _color4;


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

                //greyscale 
                //weights per rgb channel
                float3 lc = float3(.299, 0.587, 0.114); //luminance coefficient;
                float greyscale = dot(tex2D(_MainTex, uv), lc);
 
                //set colors
                if (color.r <= 0.25)
                {
                    color = _color1;
                }
                else if (color.r > 0.75)
                {
                    color = _color4;
                }
                else if (color.r > 0.25 && color.r <= 0.5)
                {
                    color = _color2;
                }
                else
                {
                    color = _color3;
                } 

                //white noise
                float wn = 0;
                //create uv grid to pixelate
                float wnUV = floor(uv * 128);
                float d = dot(uv, float2(200.234, -73.598));
                wn = frac(sin(d) * 121);

                //vignette effect
                float distFromCenter = distance(uv.xy, float2(0.5, 0.5));
                float vignette = smoothstep(_vr, _vr - _vs, distFromCenter);


                //add white noise over posterized pixels
                color = lerp(color, cos(50 * _Time.z * wn).r, 0.005);

                //add vignette
                color = saturate(color * vignette);
 
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
