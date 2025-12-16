Shader "VFX-GAME/CelShading/CelShadingHybrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorLevels ("Color Levels", Range(2, 8)) = 5
        _Saturation ("Saturation", Range(0, 3)) = 1
        _KernelSize ("Region Kernel Size", Int) = 16
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.3
        _EdgeSmoothness ("Edge Smoothness", Range(0.01, 0.5)) = 0.1
        _DitherIntensity ("Dither Intensity", Range(0, 1)) = 0
        _RegionBlendStrength ("Region Blend Strength", Range(0, 1)) = 1
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Cull Off ZWrite Off ZTest Always
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _ColorLevels;
            float _Saturation;
            int _KernelSize;
            float _EdgeThreshold;
            float _EdgeSmoothness;
            float _DitherIntensity;
            float _RegionBlendStrength;

            // RGB to HSV conversion
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = c.g < c.b ? float4(c.bg, K.wz) : float4(c.gb, K.xy);
                float4 q = c.r < p.x ? float4(p.xyw, c.r) : float4(c.r, p.yzx);

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            // HSV to RGB conversion
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }
            
            // Region-based structure from CelShading.shader
            struct region
            {
                float3 mean;
                float variance;
            };
            
            // Calculate region statistics (from CelShading.shader)
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
                        float3 tex = tex2D(_MainTex, uv + offset).rgb;
                        sum += tex;
                        squareSum += tex * tex;
                    }
                }
                
                r.mean = sum / samples;
                float3 variance = abs((squareSum / samples) - (r.mean * r.mean));
                r.variance = length(variance);
                
                return r;
            }
            
            // Adaptive region blending (from CelShading.shader)
            float3 blendRegions(float2 uv)
            {
                int upper = (_KernelSize - 1) / 2;
                int lower = -upper;
                int regionSize = upper + 1;
                int samples = regionSize * regionSize;
                
                // Divide into 4 quadrants
                region regionA = calcRegion(int2(lower, lower), int2(0, 0), samples, uv);
                region regionB = calcRegion(int2(0, lower), int2(upper, 0), samples, uv);
                region regionC = calcRegion(int2(lower, 0), int2(0, upper), samples, uv);
                region regionD = calcRegion(int2(0, 0), int2(upper, upper), samples, uv);
                
                // Find region with minimum variance
                region regions[4] = {regionA, regionB, regionC, regionD};
                float minVariance = 1e10;
                float3 bestColor = regionA.mean;
                
                for (int i = 0; i < 4; i++)
                {
                    if (regions[i].variance < minVariance)
                    {
                        minVariance = regions[i].variance;
                        bestColor = regions[i].mean;
                    }
                }
                
                return bestColor;
            }
            
            // Improved edge detection using Sobel operator (from CelShadingV2.shader)
            float detectEdgeSobel(float2 uv)
            {
                float2 texelSize = _MainTex_TexelSize;
                
                // Sample the 3x3 neighborhood
                float3 topLeft = tex2D(_MainTex, uv + float2(-texelSize.x, -texelSize.y)).rgb;
                float3 top = tex2D(_MainTex, uv + float2(0, -texelSize.y)).rgb;
                float3 topRight = tex2D(_MainTex, uv + float2(texelSize.x, -texelSize.y)).rgb;
                float3 left = tex2D(_MainTex, uv + float2(-texelSize.x, 0)).rgb;
                float3 right = tex2D(_MainTex, uv + float2(texelSize.x, 0)).rgb;
                float3 bottomLeft = tex2D(_MainTex, uv + float2(-texelSize.x, texelSize.y)).rgb;
                float3 bottom = tex2D(_MainTex, uv + float2(0, texelSize.y)).rgb;
                float3 bottomRight = tex2D(_MainTex, uv + float2(texelSize.x, texelSize.y)).rgb;
                
                // Convert to luminance for edge detection
                float gx = dot(topLeft + 2.0 * left + bottomLeft, float3(0.333, 0.333, 0.333)) * -1.0
                           + dot(topRight + 2.0 * right + bottomRight, float3(0.333, 0.333, 0.333));
                
                float gy = dot(topLeft + 2.0 * top + topRight, float3(0.333, 0.333, 0.333)) * -1.0
                           + dot(bottomLeft + 2.0 * bottom + bottomRight, float3(0.333, 0.333, 0.333));
                
                return saturate(sqrt(gx * gx + gy * gy) * 2.0);
            }
            
            // Simple dithering to reduce banding (from CelShadingV2.shader)
            float3 applyDither(float3 color, float2 uv, float intensity)
            {
                // Bayer matrix for dithering
                float BayerMatrix[16] = {
                    0.0, 8.0, 2.0, 10.0,
                    12.0, 4.0, 14.0, 6.0,
                    3.0, 11.0, 1.0, 9.0,
                    15.0, 7.0, 13.0, 5.0
                };
                
                int2 pixelPos = int2(uv * _ScreenParams.xy);
                float ditherValue = (BayerMatrix[(pixelPos.x % 4) * 4 + (pixelPos.y % 4)] / 16.0 - 0.5) * intensity;
                
                return color + ditherValue / 255.0;
            }
            
            // Simple Gaussian blur for fallback (from CelShadingV2.shader)
            float3 gaussianBlur(float2 uv, int radius)
            {
                float3 sum = 0;
                float weightSum = 0;
                
                // Simple 3x3 Gaussian kernel weights
                float kernel[9] = {
                    0.077847, 0.123317, 0.077847,
                    0.123317, 0.195346, 0.123317,
                    0.077847, 0.123317, 0.077847
                };
                
                int index = 0;
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(_MainTex_TexelSize.x * x, _MainTex_TexelSize.y * y);
                        float3 sampleColor = tex2D(_MainTex, uv + offset).rgb;
                        
                        sum += sampleColor * kernel[index];
                        weightSum += kernel[index];
                        index++;
                    }
                }
                
                return sum / weightSum;
            }
            
            fixed4 frag (v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                
                // Detect edges with improved Sobel operator
                float edgeFactor = detectEdgeSobel(uv);
                
                // Get original color
                float3 originalColor = tex2D(_MainTex, uv).rgb;
                
                // Apply region-based adaptive filtering (from CelShading.shader)
                float3 regionFilteredColor = blendRegions(uv);
                
                // Apply simple Gaussian blur as fallback (from CelShadingV2.shader)
                float3 gaussianFilteredColor = gaussianBlur(uv, 1);
                
                // Blend between region-based and Gaussian filtering based on region strength
                float3 filteredColor = lerp(gaussianFilteredColor, regionFilteredColor, _RegionBlendStrength);
                
                // Smooth blend based on edge strength (from CelShadingV2.shader)
                float blendFactor = smoothstep(_EdgeThreshold - _EdgeSmoothness, 
                                               _EdgeThreshold + _EdgeSmoothness, 
                                               edgeFactor);
                float3 col = lerp(filteredColor, originalColor, blendFactor);
                
                // Cel-shading quantization in HSV space
                float3 hsv = rgb2hsv(col);
                
                // Adjust saturation
                hsv.y *= _Saturation;
                hsv.y = saturate(hsv.y);
                
                // Quantize value (brightness) - fixed to prevent breaks
                int levels = int(_ColorLevels);
                float quantizedValue = floor(hsv.z * levels) / max(levels - 1, 1);
                hsv.z = quantizedValue;
                
                // Convert back to RGB
                float3 result = hsv2rgb(hsv);
                
                // Optional: Apply dithering to reduce banding artifacts
                if (_DitherIntensity > 0.01)
                {
                    result = applyDither(result, uv, _DitherIntensity);
                }
                
                // Preserve very bright highlights (improved from CelShadingV2.shader)
                float luminance = dot(originalColor, float3(0.299, 0.587, 0.114));
                if (luminance > 0.9)
                {
                    // Smoothly blend back original highlights
                    float highlightStrength = smoothstep(0.9, 1.0, luminance);
                    result = lerp(result, originalColor, highlightStrength * 0.8);
                }
                
                // Edge preservation - ensure edges are sharp
                if (edgeFactor > _EdgeThreshold + _EdgeSmoothness)
                {
                    // Keep original color at strong edges for clarity
                    result = lerp(result, originalColor, 0.3);
                }
                
                return fixed4(result, 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}