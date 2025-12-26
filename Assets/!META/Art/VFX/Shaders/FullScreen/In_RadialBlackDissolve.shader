Shader "VFX-GAME/General/In_RadialBlackDissolve"
{   
    Properties
    {
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _NoiseScale ("Noise Scale", Float) = 10
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.2
        _PixelSize ("Pixel Size", Float) = 2024
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        
        ZWrite Off 
        Cull Off 
        ZTest Always

        Pass
        {
            Name "PixelEdgeDissolvePass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _DissolveAmount;
                float _NoiseScale;
                float _NoiseIntensity;
                float _PixelSize;
            CBUFFER_END

            half4 Frag (Varyings input) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);

                float2 uv = input.texcoord;
                float2 pixelUV = floor(uv * _PixelSize) / _PixelSize;
                
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 aspectCorrectedUV = pixelUV;
                aspectCorrectedUV.x *= aspect;
                float2 center = float2(0.5 * aspect, 0.5);
                
                float distFromCenter = distance(aspectCorrectedUV, center);
                float maxDist = distance(float2(0, 0), center);
                
                float edgeValue = 1.0 - (distFromCenter / maxDist);
                
                float3 noisePos = float3(pixelUV * _NoiseScale, _Time.y * 0.1);
                float noiseVal = SimplexNoise(noisePos) * 0.5 + 0.5;

                float threshold = _DissolveAmount * (1.0 + _NoiseIntensity);
                float finalValue = edgeValue + (noiseVal * _NoiseIntensity);
                
                float mask = step(threshold, finalValue);
                
                return lerp(float4(0, 0, 0, 1), color, mask);
            }
            ENDHLSL
        }
    }
}