Shader "Custom/URP_TreeSoftOcclusionLeaves"
{
    Properties
    {
        _MainTex ("Main Texture (RGB) Alpha (A)", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        _BaseColor ("Main Color", Color) = (1, 1, 1, 1)
        [Toggle(_ALPHATEST_ON)] _UseAlphaTest ("Enable Alpha Cutout", Float) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="AlphaTest" 
            "RenderPipeline"="UniversalPipeline" 
        }
        LOD 200

        // Двусторонняя отрисовка листвы (Double-Sided)
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma shader_feature_local _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 color        : COLOR; // Vertex Color: RGB = Occlusion/Lighting Data, A = Wind
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                half4 color         : COLOR;
                half3 vLight        : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _BaseColor;
                half _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;

                // Основное солнце URP
                Light mainLight = GetMainLight();
                
                // Расчет двусторонней освещенности (Soft Lambert)
                // abs(dot) гарантирует, что изнанка листьев не будет полностью черной
                half NdotL = saturate(abs(dot(normalInput.normalWS, mainLight.direction)));

                // Эмуляция Ambient/SH с учетом вертексного затенения (input.color.rgb)
                half3 ambient = SampleSH(normalInput.normalWS) * input.color.rgb;
                half3 diffLight = mainLight.color * NdotL;

                // Запекаем суммарный свет в вершины
                output.vLight = ambient + diffLight;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 finalColor = texColor * _BaseColor;

                // Проверка прозрачности для кроны (Alpha Cutout)
                #if defined(_ALPHATEST_ON)
                    clip(finalColor.a - _Cutoff);
                #endif

                // Применяем повершинный свет и запеченный Soft Occlusion из кроны
                finalColor.rgb *= input.vLight;

                return finalColor;
            }
            ENDHLSL
        }

        // Пасс отбрасывания тени с учетом Альфа-маски (Cutout)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            Cull Off
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma shader_feature_local _ALPHATEST_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _BaseColor;
                half _Cutoff;
            CBUFFER_END

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                float3 positionWS = ApplyShadowBias(vertexInput.positionWS, normalInput.normalWS, GetMainLight().direction);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                #if defined(_ALPHATEST_ON)
                    half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a * _BaseColor.a;
                    clip(alpha - _Cutoff);
                #endif

                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}