using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class MissEffectManager : MonoBehaviour
{
    public static MissEffectManager Instance { get; private set; }

    [Header("GameObject Effects")]
    [Tooltip("需要产生颤抖效果的物体（例如玩家角色、主要舞台）")]
    public Transform[] shakeTargets;
    [Tooltip("需要变红的物体（使用SpriteRenderer）")]
    public SpriteRenderer[] flashSpriteTargets;

    [Header("UI Effects")]
    [Tooltip("需要变红的UI元素（Image或Text等，继承自Graphic的都可以）")]
    public Graphic[] flashUITargets;

    [Header("Settings")]
    [Tooltip("颤抖和变色的持续时间")]
    public float effectDuration = 0.2f;
    [Tooltip("颤抖强度（像素或世界坐标偏移量）")]
    public float shakeIntensity = 0.1f;
    [Tooltip("变色的目标颜色")]
    public Color flashColor = Color.red;

    [Header("Miss Feedback UI")]
    [Tooltip("Miss 时闪烁的子物体 (Image)")]
    public Image missFeedback;
    public float missFadeInTime = 0.05f;
    public float missStayTime = 0.05f;
    public float missFadeOutTime = 0.2f;
    private Tween missFeedbackTween;

    // 记录原始位置和颜色，以便恢复
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<SpriteRenderer, Color> originalSpriteColors = new Dictionary<SpriteRenderer, Color>();
    private Dictionary<Graphic, Color> originalUIColors = new Dictionary<Graphic, Color>();

    private Coroutine activeEffectCoroutine;

    void Awake()
    {
        Instance = this;
        
        // 预先记录所有目标的初始状态
        foreach (var t in shakeTargets)
        {
            if (t != null) originalPositions[t] = t.localPosition;
        }
        foreach (var sr in flashSpriteTargets)
        {
            if (sr != null) originalSpriteColors[sr] = sr.color;
        }
        foreach (var ui in flashUITargets)
        {
            if (ui != null) originalUIColors[ui] = ui.color;
        }

        if (missFeedback != null)
        {
            Color c = missFeedback.color;
            c.a = 0;
            missFeedback.color = c;
        }
    }

    public void PlayMissEffects()
    {
        if (activeEffectCoroutine != null)
        {
            StopCoroutine(activeEffectCoroutine);
            RestoreAll(); // 如果正在播放，先强制恢复原状，避免偏移累积
        }
        activeEffectCoroutine = StartCoroutine(EffectRoutine());
        TriggerMissFeedback();
    }

    private void TriggerMissFeedback()
    {
        if (missFeedback == null) return;

        missFeedbackTween?.Kill();
        missFeedbackTween = DOTween.Sequence()
            .Append(missFeedback.DOFade(1f, missFadeInTime).SetEase(Ease.OutQuad))
            .AppendInterval(missStayTime)
            .Append(missFeedback.DOFade(0f, missFadeOutTime).SetEase(Ease.InQuad));
    }

    private IEnumerator EffectRoutine()
    {
        float elapsed = 0f;

        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;

            // 1. 颤抖逻辑
            foreach (var t in shakeTargets)
            {
                if (t != null && originalPositions.ContainsKey(t))
                {
                    Vector3 randomOffset = new Vector3(
                        Random.Range(-shakeIntensity, shakeIntensity),
                        Random.Range(-shakeIntensity, shakeIntensity),
                        0
                    );
                    t.localPosition = originalPositions[t] + randomOffset;
                }
            }

            // 2. 变色逻辑 (插值: 从红恢复到原色)
            // 越接近结束，颜色越接近原色
            float colorLerp = elapsed / effectDuration; 

            foreach (var sr in flashSpriteTargets)
            {
                if (sr != null && originalSpriteColors.ContainsKey(sr))
                {
                    sr.color = Color.Lerp(flashColor, originalSpriteColors[sr], colorLerp);
                }
            }

            foreach (var ui in flashUITargets)
            {
                if (ui != null && originalUIColors.ContainsKey(ui))
                {
                    ui.color = Color.Lerp(flashColor, originalUIColors[ui], colorLerp);
                }
            }

            yield return null;
        }

        // 效果结束，恢复原状
        RestoreAll();
        activeEffectCoroutine = null;
    }

    private void RestoreAll()
    {
        foreach (var t in shakeTargets)
        {
            if (t != null && originalPositions.ContainsKey(t))
                t.localPosition = originalPositions[t];
        }

        foreach (var sr in flashSpriteTargets)
        {
            if (sr != null && originalSpriteColors.ContainsKey(sr))
                sr.color = originalSpriteColors[sr];
        }

        foreach (var ui in flashUITargets)
        {
            if (ui != null && originalUIColors.ContainsKey(ui))
                ui.color = originalUIColors[ui];
        }
    }
}
