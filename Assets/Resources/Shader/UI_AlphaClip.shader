// ============================================================
//  Custom/UI_AlphaClip
//  用途：用于 UGUI Image / Sprite 的像素画 Alpha 裁切 Shader。
//  解决纯像素画在 UGUI 渲染时产生的半透明白边问题。
//  兼容 Unity Mask / RectMask2D 的模板缓冲裁剪逻辑。
//  目标平台：Unity 6 URP
// ============================================================
Shader "Custom/UI_AlphaClip"
{
    Properties
    {
        // ---- 主纹理（与 UI/Default 保持一致，标记为 PerRendererData 以支持 Sprite Atlas） ----
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        // ---- 顶点颜色倍增（Inspector 面板的 Color/Tint 字段） ----
        _Color ("Tint", Color) = (1, 1, 1, 1)

        // ---- Alpha 裁切阈值 ----
        // 采样 Alpha 低于此值的像素将被完全丢弃，消除半透明白边
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        // ---- UGUI Mask 所需的模板缓冲属性（完整保留，勿删） ----
        _StencilComp    ("Stencil Comparison", Float) = 8
        _Stencil        ("Stencil ID",         Float) = 0
        _StencilOp      ("Stencil Operation",  Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255

        // ---- 颜色通道写入掩码（Mask 组件会在运行时修改此值） ----
        _ColorMask ("Color Mask", Float) = 15

        // ---- UI_AlphaClip 不需要 [Toggle] 关键字，裁切始终生效 ----
        //      如需运行时开关，可将 clip() 改为 #pragma multi_compile 控制。
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

        // ----------------------------------------------------------------
        //  模板缓冲测试块 —— Mask / RectMask2D 依赖此块进行父级裁剪
        // ----------------------------------------------------------------
        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        ZTest    [unity_GUIZTestMode]

        // Alpha Clip 后像素非0即1，使用标准预乘透明混合
        // SrcAlpha OneMinusSrcAlpha 与 UI/Default 完全一致
        Blend SrcAlpha OneMinusSrcAlpha

        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_AlphaClip"

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            // 支持 RectMask2D 的软裁剪关键字
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            // 支持 Mask 组件的 Alpha 测试关键字（UI/Default 标准关键字）
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            // ----------------------------------------------------------
            //  顶点输入
            // ----------------------------------------------------------
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // ----------------------------------------------------------
            //  顶点到片元的插值结构
            // ----------------------------------------------------------
            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;  // 用于 RectMask2D 软裁剪
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ----------------------------------------------------------
            //  Shader 变量声明
            // ----------------------------------------------------------
            sampler2D _MainTex;
            fixed4    _Color;
            float4    _ClipRect;         // 由 RectMask2D 组件自动填充
            float     _Cutoff;

            // ----------------------------------------------------------
            //  顶点着色器
            // ----------------------------------------------------------
            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.worldPosition = v.vertex;
                o.vertex        = UnityObjectToClipPos(o.worldPosition);
                o.texcoord      = v.texcoord;

                // 将顶点色与 Inspector Tint 相乘，确保 Image.color 正常生效
                o.color = v.color * _Color;

                return o;
            }

            // ----------------------------------------------------------
            //  片元着色器
            // ----------------------------------------------------------
            fixed4 frag(v2f i) : SV_Target
            {
                // 1. 采样主纹理，并与顶点（含 Tint）颜色相乘
                //    这保证了 Image/Sprite 的 Inspector Color 及外部透明渐变正常工作
                half4 color = tex2D(_MainTex, i.texcoord) * i.color;

                // 2. RectMask2D 软边缘裁剪（必须在 clip 之前执行，
                //    使 Alpha 在裁剪边缘正确衰减，然后再由 clip 硬切）
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                // 3. ---- 核心：Alpha 硬裁切 ----
                //    丢弃 Alpha 低于阈值的像素，彻底消除半透明白边。
                //    clip() 在所有目标平台上均等价于 discard，但编译器优化更好。
                clip(color.a - _Cutoff);

                // 4. 兼容 Mask 组件的 UNITY_UI_ALPHACLIP 路径
                //    （Mask 组件会将此关键字打开，依赖此处的 clip 完成软遮罩到硬遮罩的转换）
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }

    // 回退到 Unity 内置 UI 默认 Shader，确保无法加载时不显示错误粉红色
    FallBack "UI/Default"
}
