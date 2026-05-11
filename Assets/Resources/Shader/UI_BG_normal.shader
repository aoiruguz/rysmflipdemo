Shader "Custom/UI_BG_Normal"
{
    Properties
    {
        // --- Standard UGUI Required Properties ---
        _MainTex            ("Main Texture",        2D)         = "white" {}
        _Color              ("Tint",                Color)      = (1,1,1,1)

        // --- Stencil / Masking (required for UI Mask compatibility) ---
        _StencilComp        ("Stencil Comparison",  Float)      = 8
        _Stencil            ("Stencil ID",          Float)      = 0
        _StencilOp          ("Stencil Operation",   Float)      = 0
        _StencilWriteMask   ("Stencil Write Mask",  Float)      = 255
        _StencilReadMask    ("Stencil Read Mask",   Float)      = 255
        _ColorMask          ("Color Mask",          Float)      = 15

        // --- Pixelation ---
        _Resolution         ("Resolution (XY)",     Vector)     = (480, 270, 0, 0)

        // --- Wobble ---
        _WobbleSpeed        ("Wobble Speed",        Float)      = 1.2
        _WobbleAmount       ("Wobble Amount",       Float)      = 0.04

        // --- Ring Expansion ---
        _Speed              ("Expansion Speed",     Float)      = 0.4
        _RingDensity        ("Ring Density",        Float)      = 8.0

        // --- 5-Color Palette ---
        _Color1             ("Color 1",             Color)      = (0.05, 0.02, 0.15, 1)
        _Color2             ("Color 2",             Color)      = (0.10, 0.05, 0.35, 1)
        _Color3             ("Color 3",             Color)      = (0.20, 0.08, 0.55, 1)
        _Color4             ("Color 4",             Color)      = (0.50, 0.15, 0.80, 1)
        _Color5             ("Color 5",             Color)      = (0.85, 0.40, 1.00, 1)
        _Color6             ("Color 6",             Color)      = (1.00, 1.00, 1.00, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref     [_Stencil]
            Comp    [_StencilComp]
            Pass    [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            // ── Uniforms ──────────────────────────────────────────────────────
            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _Color;

            float4    _ClipRect;            // set by Unity for UI Rect Mask 2D
            float4    _Resolution;

            float     _WobbleSpeed;
            float     _WobbleAmount;
            float     _Speed;
            float     _RingDensity;

            float4    _Color1;
            float4    _Color2;
            float4    _Color3;
            float4    _Color4;
            float4    _Color5;
            float4    _Color6;

            // ── Vertex ────────────────────────────────────────────────────────
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex      : SV_POSITION;
                fixed4 color       : COLOR;
                float2 texcoord    : TEXCOORD0;
                float4 worldPos    : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPos    = v.vertex;
                OUT.vertex      = UnityObjectToClipPos(v.vertex);
                OUT.texcoord    = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color       = v.color * _Color;
                return OUT;
            }

            // ── Helper: GPU integer hash → [0, 1) ────────────────────────────
            //    Classic 32-bit finaliser (Thomas Wang)
            float Hash(int n)
            {
                uint u = (uint)n;
                u = (u ^ 61u) ^ (u >> 16u);
                u *= 9u;
                u ^= u >> 4u;
                u *= 0x27d4eb2du;
                u ^= u >> 15u;
                return float(u) * 2.3283064365e-10; // / 2^32  →  [0, 1)
            }

            // ── Helper: two independent hashes for a 2-D offset ──────────────
            float2 Hash2(int n)
            {
                return float2(Hash(n * 1619 + 3571),
                              Hash(n * 6271 + 9973));
            }

            // ── Helper: pick a palette color by RANDOM index ──────────────────
            fixed4 PaletteColor(int ringIdx)
            {
                // Use hash of ring index so the colour is pseudo-random but stable
                int i = (int)floor(Hash(ringIdx * 2749 + 1237) * 6.0);
                i = clamp(i, 0, 5);
                if (i == 0) return _Color1;
                if (i == 1) return _Color2;
                if (i == 2) return _Color3;
                if (i == 3) return _Color4;
                if (i == 4) return _Color5;
                               return _Color6;
            }

            // ── Fragment ──────────────────────────────────────────────────────
            fixed4 frag(v2f IN) : SV_Target
            {
                // ① UGUI rect-mask clipping
                half alpha = UnityGet2DClipping(IN.worldPos.xy, _ClipRect);
                clip(alpha - 0.001);

                float2 uv = IN.texcoord;

                // ② Pixelation  — quantise to _Resolution grid
                float2 res = max(_Resolution.xy, float2(1.0, 1.0));
                uv = floor(uv * res) / res;

                // ③ Aspect-correct UV so circles stay round
                //    Map uv to centred range [-0.5, 0.5], then scale X by aspect
                float  aspect = res.x / res.y;          // e.g. 480/270 ≈ 1.777…
                float2 centered = uv - 0.5;
                centered.x *= aspect;

                float t = _Time.y;

                // ④ Pass 1 — estimate ring index using the raw global center
                float dist0  = length(centered);
                float phase0 = dist0 * _RingDensity - t * _Speed;
                int   ringEst = abs((int)floor(phase0));

                // ④ Per-ring independent wobble
                //    Hash2 gives a stable 2-D random direction in [-1, 1]²
                //    then animate it with sine so each ring drifts at its own rate
                float2 randDir   = Hash2(ringEst) * 2.0 - 1.0;        // [-1, 1]
                float  ringPhase = Hash(ringEst * 3301 + 7919) * 6.2832; // unique phase offset
                float2 wobble    = randDir * _WobbleAmount
                                 * float2(sin(t * _WobbleSpeed + ringPhase),
                                          cos(t * _WobbleSpeed * 0.7 + ringPhase));
                wobble.x *= aspect;

                // ⑤ Pass 2 — recompute distance from this ring's own center
                float2 delta    = centered - wobble;
                float  dist     = length(delta);
                float  phase    = dist * _RingDensity - t * _Speed;
                int    ringIndex = abs((int)floor(phase));

                // ⑦ Palette lookup — randomised per ring
                fixed4 col = PaletteColor(ringIndex);

                // ⑧ Multiply by vertex colour (UGUI tint / alpha) and clip mask
                col   *= IN.color;
                col.a *= alpha;

                return col;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
}