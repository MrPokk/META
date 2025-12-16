Shader "VFX-GAME/CelShading/CelShadingRegion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorLevels ("Color Levels", Range(2, 8)) = 5
        _Saturation ("Saturation", Range(0, 3)) = 1.1
        _KernelSize ("Kernel Size (N)", Int) = 32
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Cull Off
        ZWrite Off
        ZTest Always
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float2 _MainTex_TexelSize;
            float _ColorLevels;
            float _Saturation;
            int _KernelSize;
            
            struct region
            {
                float3 mean;
                float variance;
            };
            
            // Функции RGB to HSV и HSV to RGB из Bloom.shader
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = c.g < c.b ? float4(c.bg, K.wz) : float4(c.gb, K.xy);
                float4 q = c.r < p.x ? float4(p.xyw, c.r) : float4(c.r, p.yzx);
                
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }
            
            // Простое усреднение с фиксированным размером 3x3 для устранения ошибки компиляции
            fixed3 sampleAverage(float2 uv)
            {
                fixed3 sum = 0.0;
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(_MainTex_TexelSize.x * x, _MainTex_TexelSize.y * y);
                        sum += tex2D(_MainTex, uv + offset).rgb;
                    }
                }
                
                return sum / 9;
            }
            
            fixed detectEdge(float2 uv)
            {
                float2 texelSize = 1.0 / _ScreenParams.xy;
                
                fixed3 center = tex2D(_MainTex, uv).rgb;
                float edge = 0;
                
                // Проверяем только 4 соседних пикселя
                edge += length(center - tex2D(_MainTex, uv + float2(texelSize.x, 0)).rgb);
                edge += length(center - tex2D(_MainTex, uv - float2(texelSize.x, 0)).rgb);
                edge += length(center - tex2D(_MainTex, uv + float2(0, texelSize.y)).rgb);
                edge += length(center - tex2D(_MainTex, uv - float2(0, texelSize.y)).rgb);
                
                return saturate(edge);
            }
            
            region calcRegion(int2 lower, int2 upper, int samples, float2 uv)
            {
                region r;
                float3 sum = 0.0;
                float3 squareSum = 0.0;
                
                for (int x = lower.x; x <= upper.x; ++x)
                {
                    for (int y = lower.y; y <= upper.y; ++y)
                    {
                        float2 offset = float2(_MainTex_TexelSize.x * x, _MainTex_TexelSize.y * y);
                        float3 tex = tex2D(_MainTex, uv + offset);
                        sum += tex;
                        squareSum += tex * tex;
                    }
                }
                
                r.mean = sum / samples;
                float3 variance = abs((squareSum / samples) - (r.mean * r.mean));
                r.variance = length(variance);
                
                return r;
            }
            
            fixed3 addRegion(v2f_img i)
            {
                int upper = (_KernelSize - 1) / 2;
                int lower = -upper;
                int samples = (upper + 1) * (upper + 1);
                
                region regionA = calcRegion(int2(lower, lower), int2(0, 0), samples, i.uv);
                region regionB = calcRegion(int2(0, lower), int2(upper, 0), samples, i.uv);
                region regionC = calcRegion(int2(lower, 0), int2(0, upper), samples, i.uv);
                region regionD = calcRegion(int2(0, 0), int2(upper, upper), samples, i.uv);
                
                fixed3 col = regionA.mean;
                fixed minVar = regionA.variance;
                float testVal;
                
                testVal = step(regionB.variance, minVar);
                col = lerp(col, regionB.mean, testVal);
                minVar = lerp(minVar, regionB.variance, testVal);
                
                testVal = step(regionC.variance, minVar);
                col = lerp(col, regionC.mean, testVal);
                minVar = lerp(minVar, regionC.variance, testVal);
                
                testVal = step(regionD.variance, minVar);
                col = lerp(col, regionD.mean, testVal);
                
                return col;
            }
            
            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed3 col;
                float2 uv = i.uv;
                
                col = sampleAverage(uv);
                
                float edgeFactor = detectEdge(uv);
                if (edgeFactor > .3)
                {
                    col = tex2D(_MainTex, i.uv);
                }
                
                float3 hsv = rgb2hsv(col);
                
                hsv.y *= _Saturation;
                hsv.y = saturate(hsv.y);
                
                int levels = int(_ColorLevels);
                
                static const float EPSILON = 1e-10;
                float quantizedValue = floor((hsv.z - EPSILON) * levels) / levels;
                hsv.z = quantizedValue;
                
                float3 result = hsv2rgb(hsv);
                
                float3 original = tex2D(_MainTex, i.uv);
                
                float lum = dot(original, float3(0.3, 0.6, 0.1));
                int gb = lum * levels / 2;
                
                result = lerp(result, original, saturate(gb - 1));
                return fixed4(result, 1.0);
            }
            ENDCG
        }
    }
}