Shader "URP/PostProcess/Outline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _Thickness("Thickness", Range(0, 10)) = 1.0
        _DepthThreshold("Depth Threshold", Range(0, 5)) = 1.0
        _NormalThreshold("Normal Threshold", Range(0, 1)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off 
        Cull Off
        ZTest Always

        Pass
        {
            Name "OutlinePass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            // Core URP Includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Uniforms (Properties)
            float4 _OutlineColor;
            float _Thickness;
            float _DepthThreshold;
            float _NormalThreshold;

            // The Full Screen Pass Feature automatically binds the source image here
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            // Vertex Shader: Draws a full-screen triangle without a mesh
            Varyings Vert(Attributes input)
            {
                Varyings output;
                // Standard method to generate a full-screen triangle from VertexID
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv  = GetFullScreenTriangleTexCoord(input.vertexID);

                output.positionCS = pos;
                output.uv = uv;
                return output;
            }

            // Helper to sample depth safely
            float GetDepth(float2 uv)
            {
                return SampleSceneDepth(uv);
            }

            // Helper to sample normals safely
            float3 GetNormal(float2 uv)
            {
                return SampleSceneNormals(uv);
            }

            // Fragment Shader
            half4 Frag(Varyings input) : SV_Target
            {
                // 1. Sample the original scene color (Background)
                float4 sceneColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, input.uv);

                // 2. Setup constants for offsets
                float2 texelSize = _ScreenParams.zw - 1.0; // _ScreenParams.zw is usually (1/width, 1/height)
                // In Unity 6 / RenderGraph, safely use a small manual offset if params are tricky, 
                // but _ScreenParams usually works. To be safe, we can calculate it:
                float2 uvOffset = float2(_Thickness / _ScreenParams.x, _Thickness / _ScreenParams.y);

                // 3. Roberts Cross Depth Edge Detection
                // Sample diagonal neighbors
                float depth0 = GetDepth(input.uv + float2(-uvOffset.x, -uvOffset.y)); // Bottom Left
                float depth1 = GetDepth(input.uv + float2( uvOffset.x,  uvOffset.y)); // Top Right
                float depth2 = GetDepth(input.uv + float2( uvOffset.x, -uvOffset.y)); // Bottom Right
                float depth3 = GetDepth(input.uv + float2(-uvOffset.x,  uvOffset.y)); // Top Left

                // Calculate difference
                float depthDiff0 = depth1 - depth0;
                float depthDiff1 = depth3 - depth2;
                float edgeDepth = sqrt(pow(depthDiff0, 2) + pow(depthDiff1, 2)) * 100;
                
                // Thresholding
                float depthEdgeVal = edgeDepth > _DepthThreshold ? 1.0 : 0.0;

                // 4. Roberts Cross Normal Edge Detection
                float3 normal0 = GetNormal(input.uv + float2(-uvOffset.x, -uvOffset.y));
                float3 normal1 = GetNormal(input.uv + float2( uvOffset.x,  uvOffset.y));
                float3 normal2 = GetNormal(input.uv + float2( uvOffset.x, -uvOffset.y));
                float3 normal3 = GetNormal(input.uv + float2(-uvOffset.x,  uvOffset.y));

                float3 normalDiff0 = normal1 - normal0;
                float3 normalDiff1 = normal3 - normal2;
                float edgeNormal = sqrt(dot(normalDiff0, normalDiff0) + dot(normalDiff1, normalDiff1));

                float normalEdgeVal = edgeNormal > _NormalThreshold ? 1.0 : 0.0;

                // 5. Combine Edges
                float edge = max(depthEdgeVal, normalEdgeVal);

                // 6. Blend Outline with Scene
                // If edge exists (1), show OutlineColor. Otherwise, show SceneColor.
                return lerp(sceneColor, _OutlineColor, edge);
            }
            ENDHLSL
        }
    }
}