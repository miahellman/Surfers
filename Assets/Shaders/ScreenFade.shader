Shader "Unlit/ScreenFade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FadeColor ("Fade Color", Color) = (0,0,0,1)

        _AnimProgress ("Fading Progress", Range(0,1) ) = 0

        _CrackHeight ("Crack Height", float) = .5
        _CrackOpenness ("Crack Openness", float) = 0
        _CrackAmplitude ("Crack Amplitude", float) = 0


        
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        
        ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _FadeColor;
            float _AnimProgress;

            float _CrackHeight;
            float _CrackOpenness;
            float _CrackAmplitude;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float toroidalDistance( float2 pos1, float2 pos2 ){
                
                float dx = abs(pos2.x-pos1.x);
                float dy = abs(pos2.y-pos1.y);

                dx = step(.5,dx) * (1-dx) + step(dx,.5) *dx;
                dy = step(.5,dy) * (1-dy) + step(dy,.5) *dy;

                return sqrt(dx*dx + dy*dy);
            
            
            }


            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                //col = smoothstep(0,1, _AnimProgress);

                //float boltMask = ( 1-step(_BoltWidth/2,toroidalDistance( float2( lightningUVX, 0 ) , float2(i.uv.x, 0 ) )) ) * lightningVisibilityMask;
                
                float midScaling = pow( 1- distance( i.uv *2 -1, float2(0,0)), .8);

                float crackBending = midScaling * _CrackAmplitude * _AnimProgress * sin(i.uv.x * 50 * 3.1415 + _Time.x);

                float crackUVY =  saturate( frac(_CrackHeight) ) + crackBending;
                float opennessMask = 1-step( _AnimProgress/2 ,toroidalDistance( float2( 0, i.uv.y ), float2(0, crackUVY) )) ;

                col = _FadeColor;


                return float4(col.rgb, min(opennessMask, pow( _AnimProgress, .5) ));
            }
            ENDCG
        }
    }
}
