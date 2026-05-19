using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class EyeBlinkEffect : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("完全睁眼/闭眼动画的持续时间（秒）")]
    public float animationDuration = 1.5f;
    
    [Tooltip("是否在最终睁开前，先眨两次眼")]
    public bool blinkBeforeOpening = true;

    [Tooltip("是否在游戏开始时自动执行睁眼动画")]
    public bool openOnStart = true;
    
    [Tooltip("睁眼结束后，混合强度的渐隐消失时间（秒）")]
    public float fadeOutDuration = 0.2f;

    [Header("Volume Setup")]
    [Tooltip("场景中包含 Blink Effect 的 Volume 组件。如果不填，会尝试自动寻找。")]
    public Volume targetVolume;

    private BlinkEffectVolume blinkComponent;
    private float currentProgress = 0f;
    private float currentBlendFactor = 1f;
    
    // 从 Volume 中读取的基础配置，避免脚本变量与 Volume 覆盖层发生冲突
    private float baseEdgeBlur;
    private float baseBlendAmount;
    private Color baseEyelidColor;

    private void Awake()
    {
        InitializeVolume();
    }

    private void Start()
    {
        if (openOnStart)
        {
            SetEyesStateImmediately(0f);
            OpenEyes();
        }
    }

    private void InitializeVolume()
    {
        if (targetVolume == null)
        {
            targetVolume = GetComponent<Volume>();
            if (targetVolume == null)
            {
#if UNITY_2023_1_OR_NEWER
                targetVolume = FindFirstObjectByType<Volume>();
#else
                targetVolume = FindObjectOfType<Volume>();
#endif
            }
        }

        if (targetVolume != null && targetVolume.profile != null)
        {
            if (!targetVolume.profile.TryGet(out blinkComponent))
            {
                blinkComponent = targetVolume.profile.Add<BlinkEffectVolume>(false);
            }
            
            // 核心：在初始化时，读取 Volume 面板里配置的原始数值作为基础最大值！
            // 这样你只需在 Volume 里调参数即可，无需在脚本里再调一遍。
            baseEdgeBlur = blinkComponent.edgeBlur.value;
            baseBlendAmount = blinkComponent.blendAmount.value;
            baseEyelidColor = blinkComponent.eyelidColor.value;
        }
        else
        {
            Debug.LogWarning("EyeBlinkEffect: 场景中未找到 Volume 组件，眨眼动画将不会生效！");
        }
    }

    public void OpenEyes()
    {
        StopAllCoroutines();
        currentBlendFactor = 1f;
        SetVolumeEnable(true);

        if (blinkBeforeOpening)
        {
            StartCoroutine(AnimateBlinkSequence());
        }
        else
        {
            StartCoroutine(SimpleOpenSequence());
        }
    }

    public void CloseEyes()
    {
        StopAllCoroutines();
        currentBlendFactor = 1f;
        SetVolumeEnable(true);
        StartCoroutine(MoveToProgress(0f, animationDuration));
    }

    public void SetEyesStateImmediately(float progress)
    {
        StopAllCoroutines();
        currentProgress = Mathf.Clamp01(progress);
        currentBlendFactor = 1f;
        
        SetVolumeEnable(currentProgress < 1f);
        UpdateVolumeState();
    }

    private IEnumerator SimpleOpenSequence()
    {
        yield return StartCoroutine(MoveToProgress(1f, animationDuration));
        
        // 播放结束后快速渐隐 blendAmount
        yield return StartCoroutine(FadeBlendAmount(0f, fadeOutDuration));
        
        // 渐隐结束才真正禁用，优化性能
        SetVolumeEnable(false);
    }

    private IEnumerator AnimateBlinkSequence()
    {
        yield return StartCoroutine(MoveToProgress(0.2f, 0.3f));
        yield return StartCoroutine(MoveToProgress(0.0f, 0.2f));
        yield return new WaitForSeconds(0.15f);

        yield return StartCoroutine(MoveToProgress(0.4f, 0.3f));
        yield return StartCoroutine(MoveToProgress(0.0f, 0.2f));
        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(MoveToProgress(1.0f, animationDuration));
        
        // 播放结束后快速渐隐 blendAmount
        yield return StartCoroutine(FadeBlendAmount(0f, fadeOutDuration));
        
        // 渐隐结束才真正禁用
        SetVolumeEnable(false);
    }

    private IEnumerator MoveToProgress(float targetProgress, float duration)
    {
        float startProgress = currentProgress;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = t * t * (3f - 2f * t); 
            
            currentProgress = Mathf.Lerp(startProgress, targetProgress, easeT);
            UpdateVolumeState();
            
            yield return null;
        }

        currentProgress = targetProgress;
        UpdateVolumeState();
    }

    private IEnumerator FadeBlendAmount(float targetBlendFactor, float duration)
    {
        float startBlend = currentBlendFactor;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentBlendFactor = Mathf.Lerp(startBlend, targetBlendFactor, elapsed / duration);
            UpdateVolumeState();
            yield return null;
        }

        currentBlendFactor = targetBlendFactor;
        UpdateVolumeState();
    }

    private void UpdateVolumeState()
    {
        if (blinkComponent != null)
        {
            blinkComponent.blinkStrength.overrideState = true;
            blinkComponent.blinkStrength.value = 1f - currentProgress;
            
            blinkComponent.eyelidColor.overrideState = true;
            blinkComponent.eyelidColor.value = baseEyelidColor;

            // blur 映射：blinkStrength=1(全闭)时blur=0；睁开时最大值为 Volume 配置的 baseEdgeBlur
            blinkComponent.edgeBlur.overrideState = true;
            blinkComponent.edgeBlur.value = baseEdgeBlur * currentProgress;

            blinkComponent.blendAmount.overrideState = true;
            blinkComponent.blendAmount.value = baseBlendAmount * currentBlendFactor;
        }
    }

    private void SetVolumeEnable(bool state)
    {
        if (blinkComponent != null)
        {
            blinkComponent.enable.overrideState = true;
            blinkComponent.enable.value = state;
        }
    }
}
