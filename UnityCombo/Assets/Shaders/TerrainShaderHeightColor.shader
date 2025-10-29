Shader "Unlit/TerrainShaderHeightColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SnowColor ("SnowColor", Color) = (1, 1, 1, 1)
        _RockColor ("RockColor", Color) = (.6, .6, .6, 1)
        _TreeColor ("TreeColor", Color) = (0, .6, 0, 1)
        _SandColor ("SandColor", Color) = (.7, .6, 0, 1)
        _WaterColor ("WaterColor", Color) = (0, .3, 1, 1)
        
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

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _SnowColor;
            float4 _RockColor;
            float4 _TreeColor;
            float4 _SandColor;
            float4 _WaterColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = lerp(float4(.8,0,0,1), float4(0,.8,0,1), tex2D(_MainTex, i.uv));
                col = lerp(_SandColor, _WaterColor, step(0.3, tex2D(_MainTex, i.uv).x));
                col = lerp(_TreeColor, col, step(0.2, tex2D(_MainTex, i.uv).x));
                col = lerp(_RockColor, col, step(0.1, tex2D(_MainTex, i.uv).x));
                col = lerp(_SnowColor, col, step(0.05, tex2D(_MainTex, i.uv).x));
                return col;
            }
            ENDCG
        }
    }
}
