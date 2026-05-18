using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 屏幕与Note震动干扰技能
/// 震动偏移通过 LateUpdate 时机叠加，不影响 Note 的正常下落行为。
/// 计时使用 unscaledDeltaTime，兼容流速（timeScale）变化。
/// </summary>
public class ShakeEffectSkill : MonoBehaviour
{
    [Header("配置")]
    [Tooltip("需要震动的游戏窗口 (Canvas下的RectTransform)")]
    public RectTransform gameWindow;

    [Tooltip("技能持续时间（秒，基于 unscaledTime）")]
    public float duration = 0.5f;

    [Header("震动强度")]
    [Tooltip("窗口震动强度（像素）")]
    public float windowShakeStrength = 30f;
    [Tooltip("Note震动强度（世界单位）")]
    public float noteShakeStrength = 0.5f;

    [Tooltip("震动频率（每秒切换方向次数）")]
    public float vibrato = 30f;

    /// <summary>
    /// 激活技能
    /// </summary>
    public void Activate()
    {
        // 1. 获取所有场景中的 Note
        Note[] notes = FindObjectsByType<Note>(FindObjectsSortMode.None);

        // 2. 对所有 Note 启动叠加式震动协程
        foreach (Note note in notes)
        {
            if (note != null && note.gameObject.activeInHierarchy)
            {
                StartCoroutine(ShakeNoteCoroutine(note.transform));
            }
        }

        // 3. 对游戏窗口启动 AnchorPos 震动协程
        if (gameWindow != null)
        {
            StartCoroutine(ShakeWindowCoroutine(gameWindow));
        }
        else
        {
            Debug.LogWarning("[ShakeEffectSkill] Game Window is not assigned!");
        }
    }

    /// <summary>
    /// Note震动协程：在 WaitForEndOfFrame 后（Note.Update 已执行）叠加偏移，结束后归零。
    /// 使用 unscaledDeltaTime 计时，不受游戏流速影响。
    /// </summary>
    private IEnumerator ShakeNoteCoroutine(Transform noteTransform)
    {
        float elapsed = 0f;
        float intervalTimer = 0f;
        float interval = 1f / vibrato;
        Vector3 currentOffset = Vector3.zero;

        while (elapsed < duration)
        {
            // 等到本帧所有 Update 执行完毕再叠加偏移
            yield return new WaitForEndOfFrame();

            if (noteTransform == null) yield break;

            float unscaledDt = Time.unscaledDeltaTime;
            elapsed += unscaledDt;
            intervalTimer += unscaledDt;

            // 按频率切换随机偏移方向（仅 X 轴，不影响 Y 下落）
            if (intervalTimer >= interval)
            {
                intervalTimer -= interval;
                float decay = 1f - Mathf.Clamp01(elapsed / duration);
                float x = Random.Range(-1f, 1f) * noteShakeStrength * decay;
                currentOffset = new Vector3(x, 0f, 0f);
            }

            // 叠加到当前 position（下落已在 Update 里完成）
            noteTransform.position += currentOffset;
        }

        // 震动结束，无需额外归零（偏移仅在该帧有效，下一帧 Update 会重置 position）
    }

    /// <summary>
    /// 窗口震动协程：抖动 AnchorPosition，结束后复位。
    /// 使用 unscaledDeltaTime 计时，不受游戏流速影响。
    /// </summary>
    private IEnumerator ShakeWindowCoroutine(RectTransform rt)
    {
        float elapsed = 0f;
        float intervalTimer = 0f;
        float interval = 1f / vibrato;
        Vector2 originalAnchorPos = rt.anchoredPosition;
        Vector2 currentOffset = Vector2.zero;

        while (elapsed < duration)
        {
            yield return new WaitForEndOfFrame();

            if (rt == null) yield break;

            float unscaledDt = Time.unscaledDeltaTime;
            elapsed += unscaledDt;
            intervalTimer += unscaledDt;

            if (intervalTimer >= interval)
            {
                intervalTimer -= interval;
                float decay = 1f - Mathf.Clamp01(elapsed / duration);
                float x = Random.Range(-1f, 1f) * windowShakeStrength * decay;
                float y = Random.Range(-1f, 1f) * windowShakeStrength * decay;
                currentOffset = new Vector2(x, y);
            }

            rt.anchoredPosition = originalAnchorPos + currentOffset;
        }

        // 确保窗口归位
        if (rt != null)
        {
            rt.anchoredPosition = originalAnchorPos;
        }
    }

    /// <summary>
    /// 获取技能总持续时间
    /// </summary>
    public float GetDuration()
    {
        return duration;
    }
}
