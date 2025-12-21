Shader "VFX-GAME/CelShading/CelShadingEffectMaterial_Unlit_Optimized"
{
    Properties
    {
        [Header(Base)]
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseMap ("Base Map", 2D) = "white" {}
        
        [Header(Color Settings)]
        _Brightness ("Brightness", Range(0, 2)) = 1
        _Contrast ("Contrast", Range(0, 2)) = 1
        _Saturation ("Saturation", Range(0, 2)) = 1
        
        [Header(Rim Light)]
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 3
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.5
        
        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(1, 5)) = 1
        [Toggle(_OUTLINE_ON)] _OutlineEnabled ("Enable Outline", Float) = 1

        [Header(Debug)]
        [Toggle(_DEBUG_NORMALS_ON)] _DebugNormals ("Debug Normals", Float) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
            "IgnoreProjector" = "True"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _Brightness;
            half _Contrast;
            half _Saturation;
            half4 _RimColor;
            half _RimPower;
            half _RimThreshold;
            half4 _OutlineColor;
            half _OutlineWidth;
            half _OutlineEnabled;
            half _DebugNormals;
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

        // 2. MAIN UNLIT PASS
        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local _DEBUG_NORMALS_ON
            #pragma multi_compile_instancing

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

                return output;
            }

            half3 ApplySaturation(half3 color, half saturation) {
                half luma = dot(color, half3(0.2126, 0.7152, 0.0722));
                return lerp(luma.xxx, color, saturation);
            }

            half4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(input);

                #if _DEBUG_NORMALS_ON
                    return half4(normalize(input.normalWS) * 0.5 + 0.5, 1.0);
                #endif

                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                half3 color = texColor.rgb;

                // 1. Контраст (через lerp быстрее чем if)
                color = (color - 0.5) * _Contrast + 0.5;

                // 2. Яркость
                color *= _Brightness;

                // 3. Насыщенность
                color = ApplySaturation(color, _Saturation);

                // 4. Rim Effect
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);
                float rim = 1.0 - saturate(dot(normal, viewDir));
                rim = pow(rim, _RimPower);
                rim = smoothstep(_RimThreshold - 0.05, _RimThreshold + 0.05, rim);
                
                color += rim * _RimColor.rgb;

                return half4(saturate(color), texColor.a);
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