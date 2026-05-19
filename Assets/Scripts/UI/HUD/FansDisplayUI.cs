using UnityEngine;
using TMPro;

/// <summary>
/// 粉丝数据显示UI管理器
/// 用于在选关界面（Big Map）显示粉丝数和章节解锁进度
/// </summary>
public class FansDisplayUI : MonoBehaviour
{
    [Header("Fans Display")]
    [Tooltip("显示总粉丝数的文本")]
    public TextMeshProUGUI totalFansText;

    [Tooltip("显示我方名字的文本")]
    public TextMeshProUGUI playerNameText;

    [Tooltip("显示我方称号的文本")]
    public TextMeshProUGUI playerTitleText;

    [Header("Display Settings")]
    [Tooltip("是否在Start时自动更新显示")]
    public bool updateOnStart = true;

    [Tooltip("是否在OnEnable时自动更新显示（每次场景激活时）")]
    public bool updateOnEnable = true;

    void Start()
    {
        if (updateOnStart)
        {
            UpdateFansDisplay();
        }
    }

    void OnEnable()
    {
        if (updateOnEnable)
        {
            UpdateFansDisplay();
        }
    }

    /// <summary>
    /// 更新粉丝数和章节进度显示
    /// 可以从外部调用以手动刷新
    /// </summary>
    public void UpdateFansDisplay()
    {
        long totalFans = SaveManager.Instance.GetTotalFans();

        // 显示总粉丝数
        if (totalFansText != null)
        {
            totalFansText.text = ScoreCalculator.FormatLargeNumber(totalFans);
        }
        else
        {
            Debug.LogWarning("[FansDisplayUI] totalFansText is not assigned!");
        }

        // 显示名字和称号
        if (SaveManager.Instance != null)
        {
            if (playerNameText != null)
            {
                playerNameText.text = SaveManager.Instance.GetPlayerName();
            }
            if (playerTitleText != null)
            {
                playerTitleText.text = SaveManager.Instance.GetPlayerTitle();
            }
        }

        Debug.Log($"[FansDisplayUI] Total Fans: {ScoreCalculator.FormatLargeNumber(totalFans)}");
    }

    /// <summary>
    /// 获取当前粉丝数（用于其他脚本查询）
    /// </summary>
    public long GetCurrentFans()
    {
        return SaveManager.Instance.GetTotalFans();
    }

    /// <summary>
    /// 获取当前解锁的章节（用于其他脚本查询）
    /// </summary>
    public int GetUnlockedChapter()
    {
        return SaveManager.Instance.GetUnlockedChapter();
    }

    /// <summary>
    /// 检查指定章节是否已解锁（用于其他脚本查询）
    /// </summary>
    public bool IsChapterUnlocked(int chapterIndex)
    {
        return SaveManager.Instance.IsChapterUnlocked(chapterIndex);
    }
}
