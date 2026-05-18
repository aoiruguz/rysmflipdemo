Shader "Custom/Trail_StretchFade"
{
    Properties
    {
        _MainTex        ("Texture",             2D)     = "white" {}
        _Color          ("Tint Color",          Color)  = (1,1,1,1)

        [Header(UV Configuration)]
        [Toggle] _SwapUV("Swap U/V (Rotate 90)", Float) = 1.0
        [Toggle] _FlipU ("Flip U (Horizontal)",  Float) = 0.0
        [Toggle] _FlipV ("Flip V (Vertical)",    Float) = 0.0

        [Header(Fade Settings)]
        // Fade direction: 0 = fade out at top (uv.y = 1), 1 = fade out at bottom (uv.y = 0)
        _FadeDirection  ("Fade Direction (0=Top, 1=Bottom)", Range(0, 1)) = 0
        _FadePower      ("Fade Power (Curve)",   Range(0.1, 5.0)) = 1.0
        _AlphaMultiplier("Alpha Multiplier",    Range(0, 2)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // ── Uniforms ──────────────────────────────────────────────
            sampler2D _MainTex;
            float4    _MainTex_ST;
            fixed4    _Color;
            
            float     _SwapUV;
            float     _FlipU;
            float     _FlipV;

            half      _FadeDirection;
            half      _FadePower;
            half      _AlphaMultiplier;

            // ── Structures ────────────────────────────────────────────
            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;      // Trail Renderer vertex color
                float2 uv       : TEXCOORD0;  // U = across width / along length (depends on Trail mode)
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
            };

            // ── Vertex Shader ─────────────────────────────────────────
            v2f vert(appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv    = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // ── Fragment Shader ───────────────────────────────────────
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Rotate/Swap UVs if the trail coordinates are rotated relative to the texture design
                if (_SwapUV > 0.5)
                {
                    uv = uv.yx;
                }

                // Support flipping to easily adjust orientation
                if (_FlipU > 0.5)
                {
                    uv.x = 1.0 - uv.x;
                }
                if (_FlipV > 0.5)
                {
                    uv.y = 1.0 - uv.y;
                }

                // Sample texture
                fixed4 tex = tex2D(_MainTex, uv);

                // Apply tint color and vertex color
                fixed4 col = tex * _Color;
                col *= i.color;

                // ── Gradient fade along the texture's Y (V) axis ──────
                half fadeVal = uv.y;

                // FadeDirection=0 → fade at top (uv.y=1 becomes transparent)
                // FadeDirection=1 → fade at bottom (uv.y=0 becomes transparent)
                half fadeFactor = lerp(fadeVal, 1.0 - fadeVal, _FadeDirection);

                // Apply power curve for non-linear fade
                half gradientAlpha = 1.0 - pow(fadeFactor, _FadePower);

                col.a *= gradientAlpha * _AlphaMultiplier;

                return col;
            }
            ENDCG
        }
    }

    FallBack "Particles/Alpha Blended"
}
