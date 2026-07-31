Shader "Universal Render Pipeline/UIEdgeHighlight"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        
        [Header(Height Fade)]
        _HeightFadeThreshold("Height Fade Threshold (%)", Range(0, 100)) = 100
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        
        Pass
        {
            Name "HeightFade"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _HeightFadeThreshold;
            CBUFFER_END
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = vertexInput.positionCS;
                
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Samplear textura
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 finalColor = texColor * _BaseColor;
                
                // FADE POR ALTURA USANDO UV.y
                float heightPercent = input.uv.y;
                float thresholdNormalized = _HeightFadeThreshold / 100.0;
                thresholdNormalized = max(thresholdNormalized, 0.01);
                
                float alphaFade = 1.0 - (heightPercent / thresholdNormalized);
                alphaFade = saturate(alphaFade);
                
                finalColor.a *= alphaFade;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Unlit"
}