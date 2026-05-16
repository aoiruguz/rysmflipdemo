Shader "Hidden/CRTEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            // 固定参数（可以根据需要调整）
            #define SCANLINE_INTENSITY 0.5
            #define SCANLINE_COUNT 400.0
            #define SCANLINE_SPEED 0.3
            #define CURVATURE 0.08
            #define BRIGHTNESS 1.1
            #define CONTRAST 1.15
            #define VIGNETTE_INTENSITY 0.4
            #define VIGNETTE_SMOOTH 0.6
            #define RGB_SHIFT 0.002

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // CRT 屏幕弯曲
            float2 CRTCurve(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / CURVATURE;
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            // 扫描线
            float Scanline(float2 uv)
            {
                float scanline = sin((uv.y + _Time.y * SCANLINE_SPEED) * SCANLINE_COUNT * 3.14159);
                scanline = scanline * 0.5 + 0.5;
                return lerp(1.0, scanline, SCANLINE_INTENSITY);
            }

            // 暗角
            float Vignette(float2 uv)
            {
                uv *= 1.0 - uv.yx;
                float vig = uv.x * uv.y * 15.0;
                vig = pow(vig, VIGNETTE_SMOOTH);
                return lerp(1.0, vig, VIGNETTE_INTENSITY);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // CRT 弯曲
                float2 curvedUV = CRTCurve(i.uv);

                // 边界检查
                if (curvedUV.x < 0.0 || curvedUV.x > 1.0 || curvedUV.y < 0.0 || curvedUV.y > 1.0)
                {
                    return fixed4(0, 0, 0, 1);
                }

                // RGB 色差
                float r = tex2D(_MainTex, curvedUV + float2(RGB_SHIFT, 0)).r;
                float g = tex2D(_MainTex, curvedUV).g;
                float b = tex2D(_MainTex, curvedUV - float2(RGB_SHIFT, 0)).b;
                fixed4 col = fixed4(r, g, b, 1);

                // 对比度和亮度
                col.rgb = ((col.rgb - 0.5) * CONTRAST + 0.5) * BRIGHTNESS;

                // 扫描线
                col.rgb *= Scanline(curvedUV);

                // 暗角
                col.rgb *= Vignette(curvedUV);

                return col;
            }
            ENDCG
        }
    }
}
