Shader "URP/LineRendererOverlay"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent+300"
            "RenderPipeline"="UniversalRenderPipeline"
        }

        Pass
        {
            Name "Unlit" // <- no forwardlit (URP XR no hace fallback)
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // importante en Quest
            #pragma multi_compile_instancing
            #pragma multi_compile _ _SINGLE_PASS_INSTANCING // XR

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;

            Varyings vert (Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                Varyings OUT;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = IN.uv;
                OUT.color       = IN.color;
                return OUT;
            }

            float4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return tex * IN.color * _Color;
            }

            ENDHLSL
        }
    }
}