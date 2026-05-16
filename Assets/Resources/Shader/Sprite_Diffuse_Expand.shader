Shader "Custom/Sprite_Diffuse_Expand"
{
    Properties
    {
        // --- 主贴图（带 Alpha 通道）---
        _MainTex        ("Main Texture",        2D)     = "white" {}

        // --- 着色颜色 ---
        _TintColor      ("Tint Color",          Color)  = (1, 1, 1, 1)

        // --- 扩散控制 ---
        // 同时可见的贴图副本层数（有限层，非无限循环）
        _ExpandCount    ("Expand Count",        Float)  = 4.0
        // 每一层从生成到消失的时长倒数（越大扩散越快）
        _ExpandSpeed    ("Expand Speed",        Float)  = 1.0

        // --- 噪声扰动 ---
        _NoiseStrength  ("Noise Strength",      Float)  = 0.04

        // --- 触发控制 ---
        _StartTime      ("Start Time",          Float)  = -999.0

        // --- 像素化 ---
        _Resolution     ("Resolution (XY)",     Vector) = (480, 270, 0, 0)

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

            float     _ExpandCount;
            float     _ExpandSpeed;
            float     _NoiseStrength;
            float4    _Resolution;
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

            // ── 程序化噪声（Wang Hash，与 UI_BG_Normal 算法相同）─────────────
            float Hash(int n)
            {
                uint u = (uint)n;
                u = (u ^ 61u) ^ (u >> 16u);
                u *= 9u;
                u ^= u >> 4u;
                u *= 0x27d4eb2du;
                u ^= u >> 15u;
                return float(u) * 2.3283064365e-10; // → [0, 1)
            }

            // 基于 UV 的 2D 值噪声，返回 [-1, 1] 的二维扰动向量
            float2 ValueNoise2D(float2 uv)
            {
                float2 p  = uv * 6.0;
                int2   ip = (int2)floor(p);
                float2 fp = frac(p);
                float2 w  = fp * fp * (3.0 - 2.0 * fp); // Hermite 平滑

                // 通道 A
                float hA  = Hash(ip.x       + ip.y       * 1619);
                float hB  = Hash(ip.x + 1   + ip.y       * 1619);
                float hC  = Hash(ip.x       + (ip.y + 1) * 1619);
                float hD  = Hash(ip.x + 1   + (ip.y + 1) * 1619);
                float nx  = lerp(lerp(hA, hB, w.x), lerp(hC, hD, w.x), w.y);

                // 通道 B（用不同偏置）
                float hA2 = Hash(ip.x       + ip.y       * 1619 + 7919);
                float hB2 = Hash(ip.x + 1   + ip.y       * 1619 + 7919);
                float hC2 = Hash(ip.x       + (ip.y + 1) * 1619 + 7919);
                float hD2 = Hash(ip.x + 1   + (ip.y + 1) * 1619 + 7919);
                float ny  = lerp(lerp(hA2, hB2, w.x), lerp(hC2, hD2, w.x), w.y);

                return float2(nx, ny) * 2.0 - 1.0;
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

                // ① 像素化：将 UV 量化到 _Resolution 网格
                float2 res = max(_Resolution.xy, float2(1.0, 1.0));
                uv = floor(uv * res) / res;

                // 全局时间，驱动每层副本的生命周期
                // 计算触发后的相对时间进度
                float localT = (_Time.y - _StartTime) * _ExpandSpeed;
                
                // 如果还没触发或者已经播放完毕（所有层都超过 1.0），直接剪裁
                // 最大进度大约是 1.0 (最后一层的时间) + 1.0 (生命周期)
                if (localT < 0.0 || localT > 2.0) discard;

                // 最多支持 16 层
                int count = clamp((int)_ExpandCount, 1, 16);

                // 最终合成颜色
                float4 result = float4(0.0, 0.0, 0.0, 0.0);

                // 从后往前叠合
                for (int i = 15; i >= 0; i--)
                {
                    if (i >= count) continue;

                    // 每一层的时间偏移：0 是第一层，随着 i 增加，延迟出现
                    float delay = (float)i / (float)count;
                    float layerT = localT - delay; 

                    // 只有在生命周期 [0, 1] 内的层才显示
                    if (layerT < 0.0 || layerT > 1.0) continue;

                    // 缩放因子
                    float scale = layerT;
                    if (scale < 0.005) continue;

                    // ── 将当前像素 UV 逆变换回该层的纹理 UV ──────────────────
                    // Sprite 中心 = (0.5, 0.5)，以中心为原点缩放
                    float2 centered = uv - 0.5;
                    float2 texUV    = centered / scale + 0.5;

                    // 超出贴图范围 [0,1] 的区域不属于该层，跳过
                    if (texUV.x < 0.0 || texUV.x > 1.0 ||
                        texUV.y < 0.0 || texUV.y > 1.0)
                        continue;

                    // ── 程序化噪声 UV 扰动（随时间缓慢漂移）─────────────────
                    // 用层索引做种子，各层噪声独立互不干扰
                    float noiseTime = _Time.y * 0.15 + (float)i * 3.7;
                    float2 noise    = ValueNoise2D(texUV + float2(noiseTime, noiseTime * 0.7));
                    float2 distUV   = texUV + noise * _NoiseStrength;
                    distUV          = clamp(distUV, 0.0, 1.0);

                    // ── 采样主贴图 ─────────────────────────────────────────────
                    fixed4 texColor = tex2D(_MainTex, distUV);

                    // ── Alpha 曲线：淡入→全亮→淡出（接近边界时消失）─────────
                    // 淡入段：layerT 0~0.1 快速淡入
                    float fadeIn  = smoothstep(0.0, 0.1, layerT);
                    // 淡出段：layerT 0.5~1.0 线性淡出（越大越透明）
                    float fadeOut = 1.0 - smoothstep(0.5, 1.0, layerT);
                    float layerAlpha = fadeIn * fadeOut;

                    // 叠合贴图自带 Alpha + 生命周期 Alpha
                    float srcA = texColor.a * layerAlpha;

                    // ── Alpha Over 合成（从后向前）────────────────────────────
                    // result 是已合成的背景，当前层覆盖在其上
                    float outA   = srcA + result.a * (1.0 - srcA);
                    float3 outRGB;
                    if (outA > 0.0001)
                        outRGB = (texColor.rgb * srcA + result.rgb * result.a * (1.0 - srcA)) / outA;
                    else
                        outRGB = float3(0.0, 0.0, 0.0);

                    result = float4(outRGB, outA);
                }

                // ── 最终染色：结合顶点色（SpriteRenderer Color × Inspector TintColor）──
                fixed4 col;
                col.rgb = result.rgb * IN.color.rgb;
                col.a   = result.a   * IN.color.a;

                return col;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
