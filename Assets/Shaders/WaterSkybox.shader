Shader "Unlit/WaterSkybox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightPos ("LightPos", Vector) = (0,1,0,0)
        _LightLayersCount( "Light Rings Count", float) = 8
        _LightLayersStepOffset( "Light Rings Border Contrast", float ) = 1
        _LightLayersPowerContrast( "Light Rings Total Contrast", float ) = 1
        _LightLayersScale ("Light Rings Jitteryness", float) = 22
        _LightLayersExtensionRange ("Light Rings JitterynessRange", float) = .1
        _TimeScale("Time Scale", float  ) = 1
        _SkyBrightColor("Sky Bright Color",Color) = (1 ,1 ,0 ,1 ) 
        _SkyMediumColor("Sky Medium Color",Color) = (0 ,0 ,1 ,1 ) 
        _SkyDarkColor("Sky Dark Color",Color) = (0 ,0 ,0 ,1 ) 
        _SkyMediumPercent("Sky Medium Color Percent", range(0,1) ) = .5

        _WaterSurfaceMinHeight( "Water Surface Minimum Height", float ) = .9
        [NoScaleOffset] _normalMap ("Waves Normal map", 2D) = "bump" {}
        _normalIntensity ("Normal Intensity" ,float) = 1
        _WaterSurfaceColor("Water Surface Color", Color) = (0,0,1,1)
        _WaterRipplesColor("Water Ripples Color", Color) = (0,1,1,1)
        _WaterSurfaceRipplesScale("Surface Ripples Scale", float) = 1
        _WaterSurfaceRipplesSpeed("Surface Ripples Speed", float) = 1

    }
    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background"  "PreviewType" = "Skybox" } //Ask question about wher to find info on tags!
        Cull Off 
        ZWrite Off 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            
            #include "Lighting.cginc" // might be UnityLightingCommon.cginc for later versions of unity

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float circle (float2 uv, float size) {
                return smoothstep(0.0, 0.005, 1 - length(uv) / size);
            }

            sampler2D _MainTex;
            sampler2D _normalMap;
            float4 _MainTex_ST;
            float3 _LightPos;
            float _LightLayersCount;
            float _LightLayersStepOffset;
            float _LightLayersPowerContrast;
            float _LightLayersScale;
            float _LightLayersExtensionRange;
            float _TimeScale;

            float4 _SkyBrightColor;
            float4 _SkyMediumColor;
            float4 _SkyDarkColor;
            float _SkyMediumPercent;

            float _WaterSurfaceMinHeight;
            float _normalIntensity;
            float4 _WaterSurfaceColor;
            float4 _WaterRipplesColor;
            float _WaterSurfaceRipplesScale;
            float _WaterSurfaceRipplesSpeed;


            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = o.uv = v.uv;
                return o;
            }

            float rand (float2 uv) {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float value_noise (float2 uv) {
                float2 ipos = floor(uv);
                float2 fpos = frac(uv); 
                
                float o  = rand(ipos);
                float x  = rand(ipos + float2(1, 0));
                float y  = rand(ipos + float2(0, 1));
                float xy = rand(ipos + float2(1, 1));

                float2 smooth = smoothstep(0, 1, fpos);
                return lerp( lerp(o,  x, smooth.x), 
                             lerp(y, xy, smooth.x), smooth.y);
            }

            float fractal_noise (float2 uv) {
                float n = 0;

                n  = (1 / 2.0)  * value_noise( uv * 1);
                n += (1 / 4.0)  * value_noise( uv * 2); 
                n += (1 / 8.0)  * value_noise( uv * 4); 
                n += (1 / 16.0) * value_noise( uv * 8);
                
                return n;
            }


            float4 weightedThreewayLerp(float4  startValue, float4 midValue, float4 endValue, float midPercent, float inputPercent){

                midPercent = saturate(midPercent);

                float4 midToStart = lerp( startValue, midValue, inputPercent/midPercent ) * step(inputPercent, midPercent);
                float4 midToEnd = lerp(midValue, endValue, (inputPercent-midPercent)/(1-midPercent) ) * step(midPercent, inputPercent);



                return midToStart + midToEnd;


            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normalizedLightPos = normalize(_LightPos);

                float3 lightSideDirection =  normalize( cross( float3(0,1,0), normalizedLightPos ) );

                float3 lightUpDirection = normalize(cross( normalizedLightPos , lightSideDirection )  ); 


                float sideAmount = dot( lightSideDirection, i.uv  )*.5 +.5;

                float upAmount = dot( lightUpDirection, i.uv  )*.5 +.5;

                float3 coord = normalize(i.uv) * 0.5 + 0.5;

                float3 randomOffsetLightDirection = normalizedLightPos;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                float lightDistPercent = 1-distance(i.uv, normalizedLightPos  )/2;



                //float lightDistPercent = dot( normalizedLightPos, normalize(i.uv) )*.5 +.5;
                lightDistPercent += _LightLayersExtensionRange * fractal_noise( _LightLayersScale*float2( sideAmount + _Time.x * _TimeScale, upAmount - _Time.x * _TimeScale) );
                
                lightDistPercent = pow(lightDistPercent, _LightLayersPowerContrast);


                float lightDistFracPercent = frac( lightDistPercent*_LightLayersCount);

                float lightDistFracUpperPercent = floor( (lightDistPercent - 0/_LightLayersCount )*_LightLayersCount )/_LightLayersCount;
                float lightDistFraclowerPercent = floor( (lightDistPercent - _LightLayersStepOffset/_LightLayersCount )*_LightLayersCount )/_LightLayersCount;

                float lightDistTotalPercent = lerp(lightDistFracUpperPercent, lightDistFraclowerPercent, lightDistFracPercent );

                
                float3 lightRefractionColors = weightedThreewayLerp( _SkyDarkColor, _SkyMediumColor, _SkyBrightColor, _SkyMediumPercent, lightDistTotalPercent );





                //And then down here I create the cieling waves

                float surfaceMask = smoothstep(_WaterSurfaceMinHeight, 1, coord.y );

                float4 uvPan = float4(float2(1,1) * _Time.x * _WaterSurfaceRipplesSpeed, float2(-1,-1) * _Time.x * _WaterSurfaceRipplesSpeed);

                float3 surfaceNormal = float3(0,1,0);
                float3 surfaceTangent = float3(1,0,0);
                float3 surfaceBitangent = float3(0,0,1);



                float3 tangentSpaceNormal = UnpackNormal(tex2D(_normalMap, _WaterSurfaceRipplesScale * i.uv.xz + uvPan.xy));
                float3 tangentSpaceDetail = UnpackNormal(tex2D(_normalMap, _WaterSurfaceRipplesScale * (i.uv.xz ) + uvPan.zw) );

                tangentSpaceNormal = BlendNormals(tangentSpaceNormal, tangentSpaceDetail);

                tangentSpaceNormal = normalize(lerp(float3(0, 0, 1), tangentSpaceNormal, _normalIntensity));
                
                //float2 refractionUV = screenUV.xy + (tangentSpaceNormal.xy * _refractionIntensity);
                //float3 background = tex2D(_BackgroundTex, refractionUV);

                float3x3 tangentToWorld = float3x3 
                (
                    surfaceTangent.x, surfaceBitangent.x, surfaceNormal.x,
                    surfaceTangent.y, surfaceBitangent.y, surfaceNormal.y,
                    surfaceTangent.z, surfaceBitangent.z, surfaceNormal.z
                );

                float3 normal = mul(tangentToWorld, tangentSpaceNormal);


                // blinn phong
                //float3 surfaceColor = tex2D(_albedo, uv + i.uvPan.xy).rgb;

                float3 lightDirection = _WorldSpaceLightPos0;
                float3 lightColor = _LightColor0; // includes intensity

                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - (coord) );
                float3 halfDirection = normalize(viewDirection + lightDirection);

                float diffuseFalloff = 1-max(0, dot(normal, lightDirection));
                float specularFalloff = max(0, dot(normal, halfDirection));

                //float3 specular = pow(specularFalloff, _gloss * MAX_SPECULAR_POWER + 0.0001) * _gloss * lightColor;
                //float3 diffuse = diffuseFalloff * _WaterSurfaceColor * lightColor;
                float3 diffuse = lerp(_WaterSurfaceColor , _WaterRipplesColor ,diffuseFalloff);

                float3 coloredSurfaceMask = surfaceMask * diffuse;

                float3 coloredLightRefractionMask = (1-surfaceMask) * lightRefractionColors;

                col.rgb = lerp( coloredLightRefractionMask , coloredSurfaceMask , surfaceMask) ;

                return float4( col.rgb , 1);
            }
            ENDCG
        }
    }
}
