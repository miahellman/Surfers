Shader "Custom/speed lines"
{
    Properties
    {
        _MainTex ("render texture", 2D) = "white"{}

        _lineIntensity ("line intensity", Range(0, 2)) = 0.1


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

            float _lineIntensity;

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

            //rand function
            float rand(float2 p) 
            { 
                return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453123); 
            }

            //noise
            float noise(float2 p) 
            {
                float2 i = floor(p); 
                float2 f = frac(p);	
	            float2 u = f*f*(3.0-2.0*f);
                float a = rand(i + float2(0, 0));
	            float b = rand(i + float2(1, 0));
	            float c = rand(i + float2(0, 1));
	            float d = rand(i + float2(1, 1));
                return float(a+(b-a)*u.x+(c-a)*u.y+(a-b-c+d)*u.x*u.y)/4.;
            }

            //mirroring obj
            float mirror(float t, float r) 
            {
                t = frac(t+r);
                return 2.0*abs(t-0.5);
            }

            //radial noise
            float radialNoise(float t, float d)
            {
                float2x2 m2 = float2x2(0.90,0.44,-0.44,0.90);
                float SCALE = 45.;
                d = pow(d, 0.01);
                float doff = -_Time.y*0.07;
                float2 p = float2(mirror(t,0.1),d+doff);
                float f1 = noise(p * SCALE);
                p = 2.1*float2(mirror(t,0.4),d+doff);
                float f2 = noise(p * SCALE);
                p = 3.7*float2(mirror(t,0.8),d+doff);
                float f3 = noise(p * SCALE);
                p = 5.8*float2(mirror(t,0.0),d+doff);
                float f4 = noise(p * SCALE);
                return pow((f1+0.5*f2+0.25*f3+0.125*f4)*3., 1.);
            }

            //add color 
            float3 colorize(float x)
            {
                x = clamp(x,0.0,1.0);
                float3 c = lerp(float3(0,0,1.1), float3(0,1,1), x);
                c = lerp(c, float3(1,1,1), x*4.-3.0) * x;
                c = max(c, 0);
                c = lerp(c, float3(1, .25, 1), smoothstep(1.0, 0.2, x) * smoothstep(0.15, 0.9, x));
                return c;
            }

            float4 frag (Interpolators i) : SV_Target
            {
               float3 color = 0;
                
                float2 uv = i.uv;

                color = tex2D(_MainTex, uv);

                uv = ((i.vertex.xy) * 2. - _ScreenParams.xy) / _ScreenParams.y * 0.5;
                float d = dot(uv, uv);
                float t = atan2(uv.y, uv.x) / 6.28318530718;
                float v = radialNoise(t, d);
                v = v * 2.5 - 1.4;
                v = lerp(0., v, .8 * smoothstep(0.0, 0.8, d));
                float3 output = colorize(v);

                //add lines to scene w/ line intensity defined in other script
                //switch to black and white 
                color += output.rrr * _lineIntensity;
 
                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}

