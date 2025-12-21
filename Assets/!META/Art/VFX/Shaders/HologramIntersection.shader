Shader "VFX-GAME/General/HologramIntersection"
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
        _MaxLineHeight ("Max Line Height", Range(-10, 10)) = 5
        _LineThickness ("Line Thickness", Range(0.01, 0.5)) = 0.05
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
        Cull Off
        
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
                float heightFactor : TEXCOORD4;
                float localHeight : TEXCOORD5; 
                UNITY_FOG_COORDS(6)
            };
            
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
            float _MaxLineHeight;
            float _LineThickness;
            
            sampler2D _CameraDepthTexture;
            
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
                
                float localHeight = v.vertex.y;
                o.heightFactor = smoothstep(_TransparentHeight - _HeightTransition, 
                                           _TransparentHeight + _HeightTransition, 
                                           localHeight);
                
                o.localHeight = localHeight;
                
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
                float objectDepth = i.screenPos.w;
                
                float depthDiff = sceneDepth - objectDepth;
                float intersection = smoothstep(0, _IntersectionWidth, depthDiff);
                
                float lineHeightMask = step(i.localHeight, _MaxLineHeight);
                
                float scanPos = (i.worldPos.y + _Time.y * _ScanSpeed) * _ScanFrequency;
                
                float sinWave = sin(scanPos);
                float scanLine = step(1.0 - _LineThickness * 10.0, sinWave * 0.5 + 0.5);
                
                scanLine *= lineHeightMask;
                
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, normalize(i.worldNormal))), _FresnelPower);
                
                float hologramNoise = noise(float2(i.worldPos.x * 0.5 + _Time.y, i.worldPos.z * 0.5)) * _NoiseStrength;
                
                float hologramPattern = scanLine * 0.8 + hologramNoise * 0.5;
                
                float4 hologramColor = _MainColor * (hologramPattern + _GlowIntensity * fresnel);
                
                float intersectionGlow = saturate(1.0 - intersection * 2.0);
                float4 intersectionEffect = _IntersectionColor * intersectionGlow * 2.0;
                
                float4 finalColor = lerp(hologramColor, intersectionEffect, intersectionGlow * 0.7);
                finalColor.rgb = saturate(finalColor.rgb);
                
                float minAlpha = 0.1;
                float maxAlpha = 0.3;
                
                float luminance = dot(finalColor.rgb, float3(0.299, 0.587, 0.114));
                float alphaFromColor = smoothstep(0.0, 0.3, luminance);
                
                float heightAlpha = lerp(maxAlpha, minAlpha, pow(i.heightFactor, _HeightFalloff));
                
                float alphaFromFresnel = fresnel * _EdgeGlow;
                float alphaFromIntersection = intersectionGlow * 0.8;
                float alphaFromScanLines = scanLine * 0.5;
                
                finalColor.a = saturate(
                    alphaFromColor * heightAlpha +
                    alphaFromFresnel * 0.5 +
                    alphaFromIntersection * 0.7 +
                    alphaFromScanLines * 0.8
                );
                
                finalColor.a = clamp(finalColor.a, minAlpha, maxAlpha);
                
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/VertexLit"
    CustomEditor "HologramIntersectionShaderEditor"
}