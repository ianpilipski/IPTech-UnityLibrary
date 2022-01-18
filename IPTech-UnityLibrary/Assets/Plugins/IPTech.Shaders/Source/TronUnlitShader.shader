Shader "IPTech/Tron Unlit"
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
        Tags { "RenderType"="Opaque" }
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
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = abs(i.uv - 0.5) * _edgeThickness;
                uv = pow(uv, fixed2(_edgeSharpness, _edgeSharpness)) - _edgeSubtract;
                
                float c = clamp(uv.x + uv.y, 0.0, 1.0) * _glowStrength;
                
                fixed4 col = fixed4(_Color.rgb * c, 1.0);
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
