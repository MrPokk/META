Shader "VFX-GAME/CelShading/CelShadingEffectMaterial_Unlit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _BaseMap ("Base Map", 2D) = "white" {}
        
        _Brightness ("Brightness", Range(0, 2)) = 1
        _Contrast ("Contrast", Range(0, 2)) = 1
        _Saturation ("Saturation", Range(0, 2)) = 1
        
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.1, 10)) = 3
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.5
        
        [Header(Outline)]
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.03
        _OutlineSmoothness ("Outline Smoothness", Range(0, 1)) = 0.5
        [Toggle]_OutlineUseVertexColor ("Use Vertex Color Alpha", Float) = 0
        [Toggle]_OutlineEnabled ("Outline Enabled", Float) = 1

        [Header(Debug)]
        [Toggle]_DebugNormals ("Debug Normals", Float) = 0
        [Toggle]_InvertNormals ("Invert Normals", Float) = 0
        [Toggle]_ShowOutlineOnly ("Show Outline Only", Float) = 0
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
        
        // ========== ПРОХОД КОНТУРА ==========
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            Cull Front
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _OutlineColor;
                half _OutlineWidth;
                half _OutlineSmoothness;
                half _OutlineUseVertexColor;
                half _OutlineEnabled;
            CBUFFER_END
            
            // Основной метод контура
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Если контур отключен, просто передаем позицию
                if (_OutlineEnabled < 0.5)
                {
                    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                    output.positionCS = positionInputs.positionCS;
                    output.color = input.color;
                    return output;
                }
                
                // Получаем позицию в мировом пространстве
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                
                // Получаем нормаль в мировом пространстве
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                normalWS = normalize(normalWS);
                
                // Смещаем позицию вдоль нормали
                positionWS += normalWS * _OutlineWidth;
                
                // Преобразуем в пространство камеры
                output.positionCS = TransformWorldToHClip(positionWS);
                
                output.color = input.color;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Если контур отключен, возвращаем прозрачный цвет
                if (_OutlineEnabled < 0.5)
                {
                    return half4(0, 0, 0, 0);
                }
                
                half4 outlineColor = _OutlineColor;
                
                // Если используем альфа-канал цвета вершины для контроля контура
                if (_OutlineUseVertexColor > 0.5)
                {
                    outlineColor.a *= input.color.a;
                }
                
                // Добавляем небольшую плавность краям
                outlineColor.a *= _OutlineSmoothness;
                
                return outlineColor;
            }
            ENDHLSL
        }
        
        // ========== ОСНОВНОЙ ПРОХОД ==========
        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull Back
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float4 color : COLOR;
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _RimColor;
                half _RimPower;
                half _RimThreshold;
                half _Brightness;
                half _Contrast;
                half _Saturation;
                half _DebugNormals;
                half _InvertNormals;
                half _ShowOutlineOnly;
                half _OutlineEnabled;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInputs.normalWS;
                
                // Инвертируем нормали если нужно
                if (_InvertNormals > 0.5)
                {
                    output.normalWS = -output.normalWS;
                }
                
                // Вектор взгляда (для rim эффекта)
                float3 positionWS = positionInputs.positionWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positionWS);
                
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.color = input.color;
                
                return output;
            }
            
            // Функция для насыщенности цвета
            half3 ApplySaturation(half3 color, half saturation)
            {
                half luminance = dot(color, half3(0.299, 0.587, 0.114));
                return lerp(half3(luminance, luminance, luminance), color, saturation);
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Если показываем только контур, возвращаем прозрачный цвет
                if (_ShowOutlineOnly > 0.5)
                {
                    return half4(0, 0, 0, 0);
                }
                
                // Sample texture
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                
                // Если включена отладка нормалей, показываем их цветом
                if (_DebugNormals > 0.5)
                {
                    float3 normal = normalize(input.normalWS);
                    return half4(normal * 0.5 + 0.5, 1.0);
                }
                
                // Применяем контраст и яркость
                half3 color = baseColor.rgb;
                
                // Контраст (среднее значение 0.5)
                color = saturate((color - 0.5) * _Contrast + 0.5);
                
                // Яркость
                color *= _Brightness;
                
                // Насыщенность
                color = ApplySaturation(color, _Saturation);
                
                // Rim эффект (ободочное свечение)
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);
                float rimDot = 1.0 - saturate(dot(normal, viewDir));
                float rim = pow(rimDot, _RimPower);
                rim = smoothstep(_RimThreshold - 0.1, _RimThreshold + 0.1, rim);
                
                // Добавляем rim цвет
                color += rim * _RimColor.rgb;
                
                // Гарантируем, что цвет не превышает 1.0 (кроме особых случаев)
                color = saturate(color);
                
                return half4(color, baseColor.a);
            }
            ENDHLSL
        }
        
        // ========== Shadow caster pass ==========
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
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                
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
        
        // ========== Depth only pass ==========
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
    
    // Fallback
    FallBack "Universal Render Pipeline/Unlit"
}