using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("SF/Blink Effect")]
public class BlinkEffectVolume : VolumeComponent, IPostProcessComponent
{
    [Tooltip("是否启用眨眼效果")]
    public BoolParameter enable = new BoolParameter(false);
    
    [Tooltip("眨眼强度：0=完全睁开，1=完全闭上")]
    public ClampedFloatParameter blinkStrength = new ClampedFloatParameter(0f, 0f, 1f);
    
    [Tooltip("边缘模糊强度：0=无模糊，1=最大模糊")]
    public ClampedFloatParameter edgeBlur = new ClampedFloatParameter(0f, 0f, 1f);
    
    [Tooltip("眼睑颜色")]
    public ColorParameter eyelidColor = new ColorParameter(new Color(0.05f, 0.03f, 0.02f, 1f));

    [Tooltip("混合强度：0=不影响原图，1=完全使用眼睑遮罩颜色")]
    public ClampedFloatParameter blendAmount = new ClampedFloatParameter(1f, 0f, 1f);
    
    public bool IsActive() => enable.value && (blinkStrength.value > 0.01f || edgeBlur.value > 0.01f || blendAmount.value > 0.01f);
    
    public bool IsTileCompatible() => false;
}
