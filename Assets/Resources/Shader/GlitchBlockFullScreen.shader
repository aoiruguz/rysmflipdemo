Shader "Custom/GlitchBlockFullScreen"
{
    Properties
    {
        // ----- Glitch Parameters -----
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.0
        _GlitchBlockSize ("Glitch Block Size", Range(0.01, 0.2)) = 0.05
        _BlockSizeRandomness ("Block Size Randomness", Range(0, 1)) = 0.5

        // ----- Stencil Parameters (exposed for external configuration) -----
        [Header(Stencil)]
        [IntRange] _StencilRef ("Stencil Reference", Range(0, 255)) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 3
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail Op", Float) = 0
        [IntRange] _StencilReadMask ("Stencil Read Mask", Range(0, 255)) = 255
        [IntRange] _StencilWriteMask ("Stencil Write Mask", Range(0, 255)) = 255
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        LOD 100
        ZWrite Off
        ZTest Always
        Blend Off
        Cull Off

        Stencil
        {
            Ref [_StencilRef]
            Comp [_StencilComp]
            Pass [_StencilPass]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            Name "GlitchBlockFullScreen"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // ---------- Uniforms ----------
            float _GlitchIntensity;
            float _GlitchBlockSize;
            float _BlockSizeRandomness;

            // ---------- Helpers ----------
            float Random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float2 GlitchBlockOffset(float2 uv, float time, float seed, float2 screenSize)
            {
                float2 pixelUV = uv * screenSize;
                float blockSizeInPixels = _GlitchBlockSize * max(screenSize.x, screenSize.y);

                float2 blockID = floor(pixelUV / blockSizeInPixels);
                float sizeVariation = lerp(1.0, Random(blockID + seed * 100.0) * 2.0, _BlockSizeRandomness);
                float2 randomBlockID = floor(pixelUV / (blockSizeInPixels * sizeVariation));

                float glitchRandom = Random(randomBlockID + floor(time * 10.0) + seed);
                float threshold = 1.0 - _GlitchIntensity * 0.5;

                float isGlitch = step(threshold, glitchRandom);

                float2 offset = float2(
                    (Random(randomBlockID + time + seed) - 0.5) * _GlitchIntensity * 0.2,
                    (Random(randomBlockID + time + seed + 1.0) - 0.5) * _GlitchIntensity * 0.1
                );

                return offset * isGlitch;
            }

            // ---------- Fragment ----------
            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                // Use Unity built-ins: _Time.y = unscaled time, _ScreenParams.xy = (width, height)
                float time = _Time.y;
                float2 screenSize = _ScreenParams.xy;

                // --- First-pass block offset ---
                float2 glitchOffset1 = GlitchBlockOffset(uv, time, 0.0, screenSize);
                float2 glitchedUV1 = saturate(uv + glitchOffset1);

                // --- Second-pass block offset (layered) ---
                float2 glitchOffset2 = GlitchBlockOffset(glitchedUV1, time, 13.7, screenSize);
                float2 glitchedUV2 = saturate(glitchedUV1 + glitchOffset2 * 0.7);

                // --- Base color ---
                half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, glitchedUV2);

                // --- Chromatic aberration ---
                float chromaStrength = saturate((_GlitchIntensity - 0.3) / 0.7);

                float2 combinedOffset = glitchOffset1 + glitchOffset2 * 0.7;
                float2 rOffset = combinedOffset * 0.5;
                float2 bOffset = -combinedOffset * 0.5;
                half r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, saturate(uv + rOffset)).r;
                half b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, saturate(uv + bOffset)).b;

                color.r = lerp(color.r, r, chromaStrength);
                color.b = lerp(color.b, b, chromaStrength);

                return color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
