Shader "Custom/CRTScanlines"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Header(Scanline Settings)]
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.5
        _ScanlineCount ("Scanline Count", Range(100, 1000)) = 300
        _ScanlineSpeed ("Scanline Speed", Range(0, 5)) = 0.5

        [Header(CRT Curvature)]
        _Curvature ("Screen Curvature", Range(0, 0.5)) = 0.1

        [Header(Color Settings)]
        _Brightness ("Brightness", Range(0.5, 2)) = 1.0
        _Contrast ("Contrast", Range(0.5, 2)) = 1.0

        [Header(Vignette)]
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.3
        _VignetteSmooth ("Vignette Smoothness", Range(0.1, 2)) = 0.5

        [Header(RGB Shift)]
        _RGBShift ("RGB Shift Amount", Range(0, 0.01)) = 0.002
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        LOD 100

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
            float4 _MainTex_ST;
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _ScanlineSpeed;
            float _Curvature;
            float _Brightness;
            float _Contrast;
            float _VignetteIntensity;
            float _VignetteSmooth;
            float _RGBShift;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // CRT 屏幕弯曲效果
            float2 CRTCurve(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / _Curvature;
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            // 扫描线效果
            float Scanline(float2 uv)
            {
                float scanline = sin((uv.y + _Time.y * _ScanlineSpeed) * _ScanlineCount * 3.14159);
                scanline = scanline * 0.5 + 0.5;
                return lerp(1.0, scanline, _ScanlineIntensity);
            }

            // 暗角效果
            float Vignette(float2 uv)
            {
                uv *= 1.0 - uv.yx;
                float vig = uv.x * uv.y * 15.0;
                vig = pow(vig, _VignetteSmooth);
                return lerp(1.0, vig, _VignetteIntensity);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 应用 CRT 弯曲
                float2 curvedUV = CRTCurve(i.uv);

                // 检查是否超出屏幕边界
                if (curvedUV.x < 0.0 || curvedUV.x > 1.0 || curvedUV.y < 0.0 || curvedUV.y > 1.0)
                {
                    return fixed4(0, 0, 0, 1);
                }

                // RGB 色差效果（模拟老显示器的色彩分离）
                float r = tex2D(_MainTex, curvedUV + float2(_RGBShift, 0)).r;
                float g = tex2D(_MainTex, curvedUV).g;
                float b = tex2D(_MainTex, curvedUV - float2(_RGBShift, 0)).b;
                fixed4 col = fixed4(r, g, b, 1);

                // 应用对比度和亮度
                col.rgb = ((col.rgb - 0.5) * _Contrast + 0.5) * _Brightness;

                // 应用扫描线
                float scanline = Scanline(curvedUV);
                col.rgb *= scanline;

                // 应用暗角
                float vignette = Vignette(curvedUV);
                col.rgb *= vignette;

                return col;
            }
            ENDCG
        }
    }
}
