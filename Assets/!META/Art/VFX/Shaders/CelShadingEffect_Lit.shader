Shader "VFX-GAME/CelShading/CelShadingEffect_Lit"
{
    Properties
    {
        [Header(Base)]
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseMap ("Base Map", 2D) = "white" {}
        
        [Header(Lighting)]
        _ShadowColor ("Shadow Color", Color) = (0.4, 0.4, 0.4, 1)
        _ShadowStep ("Shadow Step", Range(-1, 1)) = 0.1
        _ShadowSmoothness ("Shadow Smoothness", Range(0, 0.5)) = 0.02
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.8
        
        [Header(Rim Light)]
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 3
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.5

        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(1, 5)) = 1
        [Toggle(_OUTLINE_ON)] _OutlineEnabled ("Enable Outline", Float) = 1
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _ShadowColor;
            half _ShadowStep;
            half _ShadowSmoothness;
            half _ShadowIntensity;
            half4 _RimColor;
            half _RimPower;
            half _RimThreshold;
            half4 _OutlineColor;
            half _OutlineWidth;
            half _OutlineEnabled; 
        CBUFFER_END
        ENDHLSL

        // 1. OUTLINE PASS
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            Offset 1, 1

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _OUTLINE_ON
            
            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input) {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                #if _OUTLINE_ON
                    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                    
                    float dist = distance(positionWS, _WorldSpaceCameraPos);
                    float width = _OutlineWidth * 0.01 * sqrt(dist);
                    positionWS += normalWS * width;
                    
                    output.positionCS = TransformWorldToHClip(positionWS);
                #else
                    output.positionCS = float4(0,0,0,0);
                #endif
                return output;
            }

            half4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(input);
                return _OutlineColor;
            }
            ENDHLSL
        }

        // 2. MAIN PASS
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float2 uv : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input) {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = posInputs.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(posInputs.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.shadowCoord = GetShadowCoord(posInputs);

                return output;
            }

            half4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);

                Light light = GetMainLight(input.shadowCoord);
                
                float NdotL = dot(normal, light.direction);
                float v = (NdotL + 1.0) * 0.5;
                float lightIntensity = smoothstep(_ShadowStep - _ShadowSmoothness, _ShadowStep + _ShadowSmoothness, v);
                lightIntensity *= light.shadowAttenuation;

                half3 shadowRGB = texColor.rgb * _ShadowColor.rgb * _ShadowIntensity;
                half3 finalRGB = lerp(shadowRGB, texColor.rgb, lightIntensity);
                
                float rim = pow(1.0 - saturate(dot(normal, viewDir)), _RimPower);
                rim = smoothstep(_RimThreshold - 0.05, _RimThreshold + 0.05, rim);
                finalRGB += rim * _RimColor.rgb * lightIntensity;
                
                finalRGB *= light.color;
                finalRGB += SampleSH(normal) * texColor.rgb * 0.2;

                return half4(finalRGB, texColor.a);
            }
            ENDHLSL
        }

        // 3. SHADOW CASTER
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowPassVertex(Attributes input) {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}