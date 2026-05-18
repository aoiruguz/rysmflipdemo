using UnityEngine;
using DG.Tweening;
using System.Collections;

/// <summary>
/// 屏幕与Note震动干扰技能
/// </summary>
public class ShakeEffectSkill : MonoBehaviour
{
    [Header("配置")]
    [Tooltip("需要震动的游戏窗口 (Canvas下的RectTransform)")]
    public RectTransform gameWindow;

    [Tooltip("技能持续时间（秒）")]
    public float duration = 0.5f;

    [Header("震动强度")]
    [Tooltip("窗口震动强度（像素）")]
    public float windowShakeStrength = 30f;
    [Tooltip("Note震动强度（世界单位）")]
    public float noteShakeStrength = 0.5f;
    
    [Tooltip("震动频率")]
    public int vibrato = 30;

    /// <summary>
    /// 激活技能
    /// </summary>
    public void Activate()
    {
        // 1. 获取所有场景中的 Note
        Note[] notes = FindObjectsByType<Note>(FindObjectsSortMode.None);

        // 2. 对所有 Note 应用震动效果
        foreach (Note note in notes)
        {
            if (note != null && note.gameObject != null)
            {
                // 只在 X 轴震动，避免影响 Note 下落 (Y轴)
                note.transform.DOShakePosition(duration, new Vector3(noteShakeStrength, 0, 0), vibrato, 90f, false, true);
            }
        }

        // 3. 对游戏窗口应用震动效果
        if (gameWindow != null)
        {
            // 对于UI元素，使用DOShakeAnchorPos
            gameWindow.DOShakeAnchorPos(duration, new Vector2(windowShakeStrength, windowShakeStrength), vibrato, 90f, false, true);
        }
        else
        {
            Debug.LogWarning("[ShakeEffectSkill] Game Window is not assigned!");
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
