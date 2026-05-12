using UnityEngine;
using System.Collections;

/// <summary>
/// 故障快遮挡技能
/// 通过调整Glitch Material的intensity参数控制效果开关
/// </summary>
public class GlitchEffectSkill : MonoBehaviour
{
    [Header("配置")]
    [Tooltip("Glitch Material引用")]
    public Material glitchMaterial;

    [Tooltip("技能持续时间（秒）")]
    public float duration = 3f;

    [Tooltip("最大故障强度")]
    public float maxIntensity = 1f;

    [Tooltip("淡入淡出时间（秒）")]
    public float fadeTime = 0.5f;

    private static readonly string INTENSITY_PROPERTY = "_GlitchIntensity";
    private Coroutine activeCoroutine;

    /// <summary>
    /// 激活技能
    /// </summary>
    public void Activate()
    {
        if (glitchMaterial == null)
        {
            Debug.LogError("[GlitchEffectSkill] Glitch Material is not assigned!");
            return;
        }

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(GlitchEffectRoutine());
    }

    /// <summary>
    /// 停用技能
    /// </summary>
    public void Deactivate()
    {
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        if (glitchMaterial != null)
        {
            SetGlitchIntensity(0f);
        }
    }

    /// <summary>
    /// 获取技能持续时间
    /// </summary>
    public float GetDuration()
    {
        return duration + fadeTime * 2;
    }

    /// <summary>
    /// 故障效果协程
    /// </summary>
    private IEnumerator GlitchEffectRoutine()
    {
        // 淡入
        yield return FadeIntensity(0f, maxIntensity, fadeTime);

        // 持续
        yield return new WaitForSeconds(duration);

        // 淡出
        yield return FadeIntensity(maxIntensity, 0f, fadeTime);

        activeCoroutine = null;
    }

    /// <summary>
    /// 渐变强度
    /// </summary>
    private IEnumerator FadeIntensity(float from, float to, float time)
    {
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;
            float intensity = Mathf.Lerp(from, to, t);
            SetGlitchIntensity(intensity);
            yield return null;
        }

        SetGlitchIntensity(to);
    }

    /// <summary>
    /// 设置故障强度
    /// </summary>
    private void SetGlitchIntensity(float intensity)
    {
        if (glitchMaterial != null && glitchMaterial.HasProperty(INTENSITY_PROPERTY))
        {
            glitchMaterial.SetFloat(INTENSITY_PROPERTY, intensity);
        }
    }

    void OnDestroy()
    {
        // 清理：确保Material恢复到初始状态
        if (glitchMaterial != null)
        {
            SetGlitchIntensity(0f);
        }
    }
}
