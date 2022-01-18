Shader "IPTech/Tron"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _edgeThickness ("Edge Thickness", Float) = 2.0
        _edgeSharpness ("Edge Sharpness", Float) = 30
        _edgeSubtract ("Edge Subtract", Float) = 0.3
        _glowStrength ("Glow Strength", Float) = 10.0
    }
    
    SubShader
    {
        Tags { 
        	"RenderType"="Opaque" 
        	"LightMode"="ForwardBase" 
        }

        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc" // for _LightColor0

            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            //#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 diff : COLOR0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                fixed3 ambient : COLOR1;
            };
            
            float _edgeThickness;
            float _edgeSharpness;
            float _glowStrength;
            float _edgeSubtract;
            fixed4 _Color;
            
            //sampler2D _MainTex;
            //float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;

                // get vertex normal in world space
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting
                //half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                // factor in the light color
                //o.diff = nl * _LightColor0;
                
                o.ambient = ShadeSH9(half4(worldNormal,1));
                // compute shadows data
                TRANSFER_SHADOW(o)
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = abs(i.uv - 0.5) * _edgeThickness;
                uv = pow(uv, fixed2(_edgeSharpness, _edgeSharpness)) - _edgeSubtract;
                
                float c = clamp(uv.x + uv.y, 0.0, 1.0) * _glowStrength;
                
                fixed4 col = fixed4(_Color.rgb * c, 1.0);
                
                // add diffuse lighting
                //col *= i.diff;

                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                //fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                //fixed3 lighting = i.diff * shadow + i.ambient;
                //fixed3 lighting = round(shadow); //i.diff * round(shadow);// * (1.0 - c);
                //col.rgb += fixed4(_Color.rgb * lighting, 1.0);

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }

        // pull in shadow caster from VertexLit built-in shader
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
