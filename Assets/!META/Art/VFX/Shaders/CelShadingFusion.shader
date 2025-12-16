Shader "VFX-GAME/CelShading/CelShadingFusion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorLevels ("Color Levels", Range(2, 8)) = 5
        _Saturation ("Saturation", Range(0, 3)) = 1.1
        _FilterRadius ("Filter Radius", Range(1, 5)) = 2
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.3
        _EdgeSmoothness ("Edge Smoothness", Range(0.01, 0.5)) = 0.1
        _DitherIntensity ("Dither Intensity", Range(0, 1)) = 0.3
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
            float _FilterRadius;
            float _EdgeThreshold;
            float _EdgeSmoothness;
            float _DitherIntensity;

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
            
            // Bilateral filter for edge-preserving smoothing
            float3 bilateralFilter(float2 uv, int radius)
            {
                float3 centerColor = tex2D(_MainTex, uv).rgb;
                float3 sum = 0;
                float weightSum = 0;
                
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        float2 offset = float2(_MainTex_TexelSize.x * x, _MainTex_TexelSize.y * y);
                        float3 sampleColor = tex2D(_MainTex, uv + offset).rgb;
                        
                        // Spatial weight (Gaussian)
                        float spatialDist = length(float2(x, y)) / radius;
                        float spatialWeight = exp(-spatialDist * spatialDist * 2.0);
                        
                        // Range weight (color similarity)
                        float colorDist = length(centerColor - sampleColor);
                        float rangeWeight = exp(-colorDist * colorDist * 10.0);
                        
                        float weight = spatialWeight * rangeWeight;
                        sum += sampleColor * weight;
                        weightSum += weight;
                    }
                }
                
                return sum / max(weightSum, 0.001);
            }
            
            // Improved edge detection using Sobel operator
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
            
            // Simple dithering to reduce banding
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
            
            // Simple Gaussian blur for fallback (faster)
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
                
                // Apply edge-preserving filtering (choose one method)
                // Option 1: Bilateral filter (better quality, slower)
                // float3 filteredColor = bilateralFilter(uv, _FilterRadius);
                
                // Option 2: Gaussian blur (faster)
                float3 filteredColor = gaussianBlur(uv, _FilterRadius);
                
                // Get original color
                float3 originalColor = tex2D(_MainTex, uv).rgb;
                
                // Smooth blend based on edge strength
                float blendFactor = smoothstep(_EdgeThreshold - _EdgeSmoothness, 
                                               _EdgeThreshold + _EdgeSmoothness, 
                                               edgeFactor);
                float3 col = lerp(filteredColor, originalColor, blendFactor);
                
                // Cel-shading quantization in HSV space
                float3 hsv = rgb2hsv(col);
                
                // Adjust saturation
                hsv.y *= _Saturation;
                hsv.y = saturate(hsv.y);
                
                // Quantize value (brightness)
                int levels = int(_ColorLevels);
                float quantizedValue = floor(hsv.z * levels) / levels;
                hsv.z = quantizedValue;
                
                // Convert back to RGB
                float3 result = hsv2rgb(hsv);
                
                // Optional: Apply dithering to reduce banding artifacts
                if (_DitherIntensity > 0.01)
                {
                    result = applyDither(result, uv, _DitherIntensity);
                }
                
                // Preserve very bright highlights
                float luminance = dot(originalColor, float3(0.299, 0.587, 0.114));
                if (luminance > 0.95)
                {
                    result = lerp(result, originalColor, 0.7);
                }
                
                return fixed4(result, 1.0);
            }
            ENDCG
        }
    }
    FallBack Off
}