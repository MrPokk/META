Shader "Custom/HologramIntersection"
{
    Properties
    {
        _MainColor ("Hologram Color", Color) = (0, 1, 1, 1)
        _IntersectionColor ("Intersection Color", Color) = (1, 0, 0, 1)
        _ScanSpeed ("Scan Speed", Range(0, 10)) = 2
        _ScanFrequency ("Scan Frequency", Range(0, 100)) = 10
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 2
        _IntersectionWidth ("Intersection Width", Range(0.01, 1)) = 0.1
        _FresnelPower ("Fresnel Power", Range(0, 10)) = 2
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.1
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.1
        _EdgeGlow ("Edge Glow", Range(0, 5)) = 1
        _TransparentHeight ("Transparent Height Start", Range(-10, 10)) = 0
        _HeightTransition ("Height Transition", Range(0, 5)) = 1
        _HeightFalloff ("Height Falloff", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float heightFactor : TEXCOORD4; // Фактор высоты для прозрачности
                UNITY_FOG_COORDS(5)
            };
            
            // Properties
            float4 _MainColor;
            float4 _IntersectionColor;
            float _ScanSpeed;
            float _ScanFrequency;
            float _GlowIntensity;
            float _IntersectionWidth;
            float _FresnelPower;
            float _NoiseStrength;
            float _AlphaThreshold;
            float _EdgeGlow;
            float _TransparentHeight;
            float _HeightTransition;
            float _HeightFalloff;
            
            sampler2D _CameraDepthTexture;
            
            // Simple noise function for hologram distortion
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }
            
v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    o.worldNormal = UnityObjectToWorldNormal(v.normal);
    o.screenPos = ComputeScreenPos(o.pos);
    o.viewDir = normalize(UnityWorldSpaceViewDir(o.worldPos));
    
    // Вычисляем фактор высоты (0 внизу, 1 вверху)
    // Используем локальные координаты Y для независимости от мирового положения
    float localHeight = v.vertex.y - _TransparentHeight;
    o.heightFactor = smoothstep(-_HeightTransition, _HeightTransition, localHeight);
    
    UNITY_TRANSFER_FOG(o, o.pos);
    return o;
}
fixed4 frag(v2f i) : SV_Target
{
    // Calculate screen position and depth
    float2 screenUV = i.screenPos.xy / i.screenPos.w;
    float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
    float objectDepth = i.screenPos.w;
    
    // Depth difference for intersection detection
    float depthDiff = sceneDepth - objectDepth;
    float intersection = smoothstep(0, _IntersectionWidth, depthDiff);
    
    // Scan lines effect
    float scanLine = sin((i.worldPos.y + _Time.y * _ScanSpeed) * _ScanFrequency) * 0.5 + 0.5;
    scanLine = pow(scanLine, 3);
    
    // Fresnel effect for edge glow
    float fresnel = pow(1.0 - saturate(dot(i.viewDir, normalize(i.worldNormal))), _FresnelPower);
    
    // Hologram noise/distortion
    float hologramNoise = noise(float2(i.worldPos.x * 0.5 + _Time.y, i.worldPos.z * 0.5)) * _NoiseStrength;
    
    // Base hologram pattern
    float hologramPattern = scanLine * (0.8 + fresnel * 0.4) + hologramNoise;
    
    // Create hologram color - УБЕРИТЕ ЧЕРНЫЙ ФОН
    float4 hologramColor = _MainColor * (hologramPattern + _GlowIntensity * fresnel);
    
    // Intersection effect
    float intersectionGlow = saturate(1.0 - intersection * 2.0);
    float4 intersectionEffect = _IntersectionColor * intersectionGlow * 2.0;
    
    // Combine hologram with intersection
    float4 finalColor = lerp(hologramColor, intersectionEffect, intersectionGlow * 1);
    finalColor.rgb = saturate(finalColor.rgb);
    
    // ВЫЧИСЛЕНИЕ АЛЬФЫ - ВСЕГДА ПРОЗРАЧНО
    float minAlpha = 0.1; // Минимальная прозрачность
    float maxAlpha = 1; // Максимальная прозрачность
    
    // Альфа от яркости - черные части становятся прозрачными
    float luminance = dot(finalColor.rgb, float3(0.299, 0.587, 0.114));
    float alphaFromColor = smoothstep(0.0, 0.3, luminance); // Черное = прозрачное
    
    // Прозрачность от высоты
    float heightAlpha = lerp(maxAlpha, minAlpha, pow(i.heightFactor, _HeightFalloff));
    
    // Альфа от других эффектов
    float alphaFromFresnel = fresnel * _EdgeGlow;
    float alphaFromIntersection = intersectionGlow * 0.8;
    float alphaFromScanLines = scanLine * 0.3;
    
    // КОМБИНИРУЕМ ВСЕ ИСТОЧНИКИ АЛЬФЫ
    finalColor.a = saturate(
        alphaFromColor * heightAlpha + // Основная прозрачность
        alphaFromFresnel * 0.5 +      // Френель с уменьшенным вкладом
        alphaFromIntersection * 0.7 + // Пересечения с уменьшенным вкладом
        alphaFromScanLines * 0.4      // Сканирующие линии с уменьшенным вкладом
    );
    
    // Гарантируем, что альфа не будет слишком высокой
    finalColor.a = clamp(finalColor.a, minAlpha, maxAlpha);
    
    // Применяем туман
    UNITY_APPLY_FOG(i.fogCoord, finalColor);
    
    return finalColor;
}
            ENDCG
        }
    }
    
    FallBack "Transparent/VertexLit"
    CustomEditor "HologramIntersectionShaderEditor"
}

/*
Case File: HologramIntersection.shader
Project: Unity Holographic Visualization
Date: [Current Date]
Version: 1.4

Новые параметры для контроля прозрачности по высоте:
1. _TransparentHeight - высота, с которой начинается эффект прозрачности
2. _HeightTransition - плавность перехода между прозрачными и непрозрачными областями
3. _HeightFalloff - контроль резкости перехода (0 = плавный, 1 = резкий)

Как работает:
- В вершинном шейдере вычисляется heightFactor (0 внизу, 1 вверху)
- В фрагментном шейдере alphaFromColor интерполируется между значением от яркости и 1.0
- Чем меньше heightFactor (нижняя часть), тем больше влияние яркости на прозрачность
- Чем больше heightFactor (верхняя часть), тем меньше влияние яркости

Настройка:
1. _TransparentHeight: установите высоту, с которой должен начинаться эффект
2. _HeightTransition: для плавного перехода установите 0.5-2.0
3. _HeightFalloff: для постепенного изменения используйте 0.3-0.7, для резкого - 1.0
4. _AlphaThreshold: контролирует, насколько темные части становятся прозрачными

Пример:
- Объект высотой 2 единицы
- _TransparentHeight = 0 (эффект начинается с нижней точки)
- _HeightTransition = 1 (плавный переход на 1 единицу вверх)
- _HeightFalloff = 0.5 (плавное изменение прозрачности)
- Результат: нижняя 1/3 объекта имеет прозрачные черные части, верхняя 2/3 - нет

Примечание:
Эффект использует мировые координаты Y. Для объектов, которые двигаются вверх/вниз,
можно использовать локальные координаты (заменить o.worldPos.y на v.vertex.y).
*/