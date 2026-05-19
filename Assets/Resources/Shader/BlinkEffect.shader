Shader "Custom/BlinkEffect"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _EyelidColor ("Eyelid Color", Color) = (0.05,0.03,0.02,1)
        _BlendAmount ("Blend Amount", Range(0,1)) = 1
    }
    
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZTest Always ZWrite Off Cull Off
        
        Pass
        {
            Name "BlinkEffect"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            float4 _EyelidColor;
            float _BlendAmount;
            float _BlinkStrength;
            float _EdgeBlur;
            
            float GetEyelidCurve(float x, float curvature)
            {
                return (-curvature * x * x + curvature * 0.25);
            }
            
            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                
                half4 originalCol = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
                
                float2 center = float2(0.5, 0.5);
                float2 offset = uv - center;

                float curvature = 2.0;
                float maxOpen = 0.42;
                float openFactor = (1.0 - _BlinkStrength);

                float rawShape = GetEyelidCurve(offset.x, curvature);
                float shapeN = saturate(rawShape / (curvature * 0.25));

                float upperEyelidPos = shapeN * maxOpen * openFactor;
                float lowerEyelidPos = -upperEyelidPos;

                float upperMask = step(offset.y, upperEyelidPos);
                float lowerMask = step(lowerEyelidPos, offset.y);
                float visibilityMask = saturate(upperMask * lowerMask) * step(abs(offset.x), 0.5);

                float coverage = 1.0 - visibilityMask;
                float eyelidCoverage = coverage;

                if (_EdgeBlur > 0.0001)
                {
                    float distUpper = abs(offset.y - upperEyelidPos);
                    float distLower = abs(offset.y - lowerEyelidPos);
                    float verticalDist = min(distUpper, distLower);

                    float insideEyeHoriz = step(0.0005, shapeN);

                    float featherWidth = 0.12 * _EdgeBlur;
                    featherWidth = max(featherWidth, 1e-4);

                    float inFeatherBand = step(verticalDist, featherWidth) * insideEyeHoriz;

                    float featherAlpha = saturate(verticalDist / featherWidth);

                    float coverageFeatherFactor = lerp(featherAlpha, 1.0, 1.0 - inFeatherBand);

                    eyelidCoverage = coverage * coverageFeatherFactor;
                }

                half3 blendedColor = lerp(originalCol.rgb, _EyelidColor.rgb, eyelidCoverage * _BlendAmount);
                float shadowAmount = eyelidCoverage * 0.2 * _BlendAmount;
                blendedColor *= (1.0 - shadowAmount);
                return half4(blendedColor, 1.0);
            }
            ENDHLSL
        }
    }
}
