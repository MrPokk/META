Shader "Custom/CelShadingEffect"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseMap ("Base Map", 2D) = "white" {}
        _ShadowColor ("Shadow Color", Color) = (0.4, 0.4, 0.4, 1)
        _ShadowStep ("Shadow Step", Range(-1, 1)) = 0.1
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 3
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.5
        
        // Новые свойства для исправления проблемы
        _Brightness ("Brightness", Range(0, 2)) = 1
        _Contrast ("Contrast", Range(0, 2)) = 1
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.8
        
        // Для отладки
        [Toggle]_DebugNormals ("Debug Normals", Float) = 0
        [Toggle]_InvertNormals ("Invert Normals", Float) = 0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _ShadowColor;
                half _ShadowStep;
                half4 _RimColor;
                half _RimPower;
                half _RimThreshold;
                half _Brightness;
                half _Contrast;
                half _ShadowIntensity;
                half _DebugNormals;
                half _InvertNormals;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInputs.normalWS;
                
                // Инвертируем нормали если нужно
                if (_InvertNormals > 0.5)
                {
                    output.normalWS = -output.normalWS;
                }
                
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(output.positionWS);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample texture
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                
                // Если включена отладка нормалей, показываем их цветом
                if (_DebugNormals > 0.5)
                {
                    float3 normal = normalize(input.normalWS);
                    return half4(normal * 0.5 + 0.5, 1.0);
                }
                
                // Получаем направление света
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                
                // Нормализуем нормаль
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);
                
                // ========== ИСПРАВЛЕНИЕ ДЛЯ ЧЕРНОЙ ПЕРЕДНЕЙ ЧАСТИ ==========
                // ВАЖНО: Проблема в том, что dot(normal, lightDir) может быть отрицательным
                // когда нормаль смотрит от источника света
                
                // Способ 1: Используем только положительные значения (стандартный подход)
                // float NdotL = max(0, dot(normal, lightDir));
                
                // Способ 2: Преобразуем [-1, 1] в [0, 1] и добавляем ambient
                float NdotL = dot(normal, lightDir);
                
                // Преобразуем NdotL из [-1, 1] в [0, 1]
                float adjustedNdotL = (NdotL + 1.0) * 0.5;
                
                // Добавляем небольшую ambient составляющую, чтобы избежать полной темноты
                adjustedNdotL = max(adjustedNdotL, 0.2);
                
                // Применяем контраст и яркость
                adjustedNdotL = saturate((adjustedNdotL - 0.5) * _Contrast + 0.5) * _Brightness;
                
                // Cel shading с учетом ShadowStep
                float shadowThreshold = (_ShadowStep + 1.0) * 0.5;
                float shadowSmoothness = 0.05;
                float lightIntensity = smoothstep(
                    shadowThreshold - shadowSmoothness, 
                    shadowThreshold + shadowSmoothness, 
                    adjustedNdotL
                );
                
                // Базовый цвет с тенью
                half3 shadedColor = lerp(
                    baseColor.rgb * _ShadowColor.rgb * _ShadowIntensity, 
                    baseColor.rgb, 
                    lightIntensity
                );
                
                // Rim lighting (ободочное освещение)
                float rimDot = 1.0 - saturate(dot(normal, viewDir));
                float rim = pow(rimDot, _RimPower);
                rim = smoothstep(_RimThreshold - 0.1, _RimThreshold + 0.1, rim);
                shadedColor += rim * _RimColor.rgb;
                
                // Применяем цвет основного света
                shadedColor *= mainLight.color;
                
                // Добавляем ambient освещение (важно для задней части!)
                half3 ambient = SampleSH(normal) * 0.5;
                shadedColor += ambient * baseColor.rgb;
                
                // Тени (если есть)
                #if _MAIN_LIGHT_SHADOWS
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                float shadowAtten = MainLightRealtimeShadow(shadowCoord);
                shadedColor *= lerp(0.6, 1.0, shadowAtten);
                #endif
                
                return half4(shadedColor, baseColor.a);
            }
            ENDHLSL
        }
        
        // Упрощенный Shadow caster pass без ApplyShadowBias
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            // Простая версия без сложного shadow bias
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Просто преобразуем позицию без сложных вычислений
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                
                // Простой bias чтобы избежать z-fighting
                #if UNITY_REVERSED_Z
                    output.positionCS.z -= 0.0001;
                #else
                    output.positionCS.z += 0.0001;
                #endif
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
        
        // Дополнительный pass для улучшенного освещения
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    // Fallback на стандартный шейдер если что-то не работает
    FallBack "Universal Render Pipeline/Simple Lit"
}