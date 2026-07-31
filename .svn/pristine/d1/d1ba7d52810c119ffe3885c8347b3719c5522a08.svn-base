Shader "Custom/BlendingCircleShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _SecondTex ("Second Texture", 2D) = "black" {}
        _FocusUV ("Focus UV", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Circle Radius", Float) = 0.1
        _Feather ("Feather Width", Float) = 0.05
        _Invert ("Invert Circle (0 = normal, 1 = inverted)", Float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _SecondTex;
            float4 _MainTex_ST;
            float4 _SecondTex_ST;
            float2 _FocusUV;
            float _Radius;
            float _Feather;
            float _Invert;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                fixed4 col1 = tex2D(_MainTex, uv);
                fixed4 col2 = tex2D(_SecondTex, uv);

                float dist = distance(uv, _FocusUV);
                float alpha = smoothstep(_Radius + _Feather, _Radius, dist);

                // Inversión del blending si _Invert = 1
                float blend = lerp(alpha, 1 - alpha, _Invert);

                return lerp(col2, col1, blend);
            }
            ENDCG
        }
    }
}
