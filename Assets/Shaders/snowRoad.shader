Shader "Custom/snowRoad"
{
    Properties 
    {
        _albedo ("albedo", 2D) = "white" {}
        [NoScaleOffset] _normalMap ("normal map", 2D) = "bump" {}
        [NoScaleOffset] _displacementMap ("displacement map", 2D) = "gray" {}
        [NoScaleOffset] _roughness ("roughness map", 2D) = "white" {}
        [NoScaleOffset] _IBL ("IBL cube map", Cube) = "black" {}

        // brightness of specular reflection - proportion of color contributed by diffuse and specular
        // reflectivity at 1, color is all specular
        _reflectivity ("reflectivity", Range(0,1)) = 0.5

        _fresnelPower ("fresnel power", Range(0, 10)) = 5
        _normalIntensity ("normal intensity", Range(0, 1)) = 1
        _displacementIntensity ("displacement intensity", Range(0, 1)) = 0

        _color("tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        // this tag is required to use _LightColor0
        Tags { "LightMode"="ForwardBase" }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // might be UnityLightingCommon.cginc for later versions of unity
            #include "Lighting.cginc"

            #define DIFFUSE_MIP_LEVEL 5
            #define SPECULAR_MIP_STEPS 4
            #define MAX_SPECULAR_POWER 256

            sampler2D _albedo; float4 _albedo_ST;
            sampler2D _normalMap;
            sampler2D _displacementMap;
            sampler2D _roughness;
            samplerCUBE _IBL;
            float _reflectivity;
            float _fresnelPower;
            float _normalIntensity;
            float _displacementIntensity;
            float4 _color;

            struct MeshData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;

                // xyz is the tangent direction, w is the tangent sign
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 tangent : TEXCOORD2;
                float3 bitangent : TEXCOORD3;
                float3 posWorld : TEXCOORD4;
      
                float disp : TEXCOORD5;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.uv = TRANSFORM_TEX(v.uv, _albedo);
                o.disp = sin(((v.uv.x + v.uv.y) * _displacementIntensity) + _Time.z) * 0.5 + 0.5;
                
                float height = tex2Dlod(_displacementMap, float4(o.uv, 0, 0)).r * 2 - 1;
                v.vertex.xyz += v.normal * height * _displacementIntensity * o.disp;

                o.normal = UnityObjectToWorldNormal(v.normal);
                o.tangent = UnityObjectToWorldNormal(v.tangent);
                o.bitangent = cross(o.normal, o.tangent) * v.tangent.w;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float3 color = 0;
                float2 uv = i.uv;

                float3 tangentSpaceNormal = UnpackNormal(tex2D(_normalMap, uv));
                tangentSpaceNormal = normalize(lerp(float3(0, 0, 1), tangentSpaceNormal, _normalIntensity));
                
                float3x3 tangentToWorld = float3x3 
                (
                    i.tangent.x, i.bitangent.x, i.normal.x,
                    i.tangent.y, i.bitangent.y, i.normal.y,
                    i.tangent.z, i.bitangent.z, i.normal.z
                );

                float3 normal = mul(tangentToWorld, tangentSpaceNormal);
                

                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld);
                float fresnel = 1 - saturate(dot(viewDirection, normal));
                fresnel = pow(fresnel, _fresnelPower);

                // since fresnel affects reflectivity, we'll use it to modify the reflectivity variable
                float reflectivity = _reflectivity * fresnel;

                // since the diffuse and reflective properties of an object are inversely related, we want to set up our surface color to lerp between black and the albedo based on the inverse of reflectivity
                // if 0% reflective -> all diffuse
                float3 surfaceColor = lerp(0, tex2D(_albedo, uv).rgb, 1 - reflectivity);

                float3 lightDirection = _WorldSpaceLightPos0;
                float3 lightColor = _LightColor0; // includes intensity

                // make view direction negative because reflect takes an incidence vector, meaning, it is point toward the surface
                // viewDirection is pointing toward the camera
                float3 viewReflection = reflect(-viewDirection, normal);
                
                float roughness = tex2D(_roughness, uv).r;

                float mip = roughness * SPECULAR_MIP_STEPS;
                float3 indirectSpecular = texCUBElod(_IBL, float4(viewReflection, mip));

                float3 halfDirection = normalize(viewDirection + lightDirection);

                float directDiffuse = max(0, dot(normal, lightDirection));
                float specularFalloff = max(0, dot(normal, halfDirection));
                
                // the specular power, which controls the sharpness of the direct specular light is dependent on the glossiness (smoothness)
                float3 directSpecular = pow(specularFalloff, (1 - roughness) * MAX_SPECULAR_POWER + 0.0001) * lightColor * (1 - roughness);

                float3 specular = directSpecular + indirectSpecular * reflectivity;
               
                float3 indirectDiffuse = texCUBElod(_IBL, float4(normal, DIFFUSE_MIP_LEVEL));
                float3 diffuse = surfaceColor * (directDiffuse * lightColor + indirectDiffuse);

                color = diffuse + specular;

                color *= _color;

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
