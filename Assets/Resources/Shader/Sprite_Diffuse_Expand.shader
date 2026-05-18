Shader "Custom/Sprite_Diffuse_Expand"
{
    Properties
    {
        // --- 主贴图（带 Alpha 通道）---
        _MainTex        ("Main Texture",        2D)     = "white" {}

        // --- 着色颜色 ---
        _TintColor      ("Tint Color",          Color)  = (1, 1, 1, 1)

        // --- 动画控制 ---
        _FrameCount     ("Frame Count",         Float)  = 5.0
        _FrameSize      ("Frame Size",          Float)  = 100.0
        _PlaySpeed      ("Play Speed (FPS)",    Float)  = 12.0

        // --- 触发控制 ---
        _StartTime      ("Start Time",          Float)  = -999.0

        // --- Sprite 标准 Stencil / Masking（Mask 兼容）---
        _StencilComp        ("Stencil Comparison",  Float)  = 8
        _Stencil            ("Stencil ID",          Float)  = 0
        _StencilOp          ("Stencil Operation",   Float)  = 0
        _StencilWriteMask   ("Stencil Write Mask",  Float)  = 255
        _StencilReadMask    ("Stencil Read Mask",   Float)  = 255
        _ColorMask          ("Color Mask",          Float)  = 15
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
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
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
            fixed4    _TintColor;

            float     _FrameCount;
            float     _FrameSize;
            float     _PlaySpeed;
            float4    _ClipRect;
            float     _StartTime;

            // ── Vertex 输入 / 输出 ────────────────────────────────────────────
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPos = v.vertex;
                OUT.vertex   = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                // 顶点色 × Inspector 着色颜色，与 UI_BG_Normal 保持一致
                OUT.color    = v.color * _TintColor;
                return OUT;
            }

            // ── Fragment ──────────────────────────────────────────────────────
            fixed4 frag(v2f IN) : SV_Target
            {
                // UI 裁剪 (用于适配 RectMask2D)
                #ifdef UNITY_UI_CLIP_RECT
                float maskAlpha = UnityGet2DClipping(IN.worldPos.xy, _ClipRect);
                clip(maskAlpha - 0.001);
                #endif

                float2 uv = IN.texcoord; // [0, 1] Sprite UV

                // 像素化：将 UV 量化到 _FrameSize 网格，并偏移到像素中心进行采样
                float frameSize = max(_FrameSize, 1.0);
                uv = (floor(uv * frameSize) + 0.5) / frameSize;

                // 计算触发后的相对时间进度
                float elapsedTime = _Time.y - _StartTime;
                
                // 如果还没触发，直接剪裁
                if (elapsedTime < 0.0) discard;

                int frameCount = max((int)_FrameCount, 1);
                int currentFrame = (int)floor(elapsedTime * _PlaySpeed);

                // 一次性播放：如果播放完毕，直接剪裁
                if (currentFrame >= frameCount) discard;

                // 计算图集中的实际 UV
                // 图集是水平排列的，总宽度为 frameCount * frameSize
                // 原有的 UV (0~1) 需要被压缩到当前帧的区域
                float2 atlasUV = uv;
                atlasUV.x = (uv.x + (float)currentFrame) / (float)frameCount;

                // 采样主贴图
                fixed4 texColor = tex2D(_MainTex, atlasUV);

                // ── 最终染色：结合顶点色（SpriteRenderer Color × Inspector TintColor）──
                fixed4 col;
                col.rgb = texColor.rgb * IN.color.rgb;
                col.a   = texColor.a   * IN.color.a;

                return col;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
