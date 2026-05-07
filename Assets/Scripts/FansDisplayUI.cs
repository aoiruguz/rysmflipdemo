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

    [Tooltip("显示章节解锁进度的文本")]
    public TextMeshProUGUI chapterProgressText;

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
        long totalFans = FansDataManager.GetTotalFans();
        int unlockedChapter = FansDataManager.GetUnlockedChapter();

        // 显示总粉丝数
        if (totalFansText != null)
        {
            totalFansText.text = $"粉丝: {ScoreCalculator.FormatLargeNumber(totalFans)}";
        }
        else
        {
            Debug.LogWarning("[FansDisplayUI] totalFansText is not assigned!");
        }

        // 显示章节进度
        if (chapterProgressText != null)
        {
            int nextChapter = unlockedChapter + 1;
            if (nextChapter < 3) // 还有下一章
            {
                long requiredFans = FansDataManager.GetRequiredFansForNextChapter(unlockedChapter);
                long remainingFans = requiredFans - totalFans;
                if (remainingFans > 0)
                {
                    chapterProgressText.text = $"第{nextChapter + 1}章解锁: 还需 {ScoreCalculator.FormatLargeNumber(remainingFans)} 粉丝";
                }
                else
                {
                    chapterProgressText.text = $"第{nextChapter + 1}章已解锁！";
                }
            }
            else
            {
                chapterProgressText.text = "已解锁全部章节！";
            }
        }
        else
        {
            Debug.LogWarning("[FansDisplayUI] chapterProgressText is not assigned!");
        }

        Debug.Log($"[FansDisplayUI] Total Fans: {ScoreCalculator.FormatLargeNumber(totalFans)}, Unlocked Chapter: {unlockedChapter + 1}");
    }

    /// <summary>
    /// 获取当前粉丝数（用于其他脚本查询）
    /// </summary>
    public long GetCurrentFans()
    {
        return FansDataManager.GetTotalFans();
    }

    /// <summary>
    /// 获取当前解锁的章节（用于其他脚本查询）
    /// </summary>
    public int GetUnlockedChapter()
    {
        return FansDataManager.GetUnlockedChapter();
    }

    /// <summary>
    /// 检查指定章节是否已解锁（用于其他脚本查询）
    /// </summary>
    public bool IsChapterUnlocked(int chapterIndex)
    {
        return FansDataManager.IsChapterUnlocked(chapterIndex);
    }
}
