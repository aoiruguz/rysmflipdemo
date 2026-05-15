using UnityEngine;

/// <summary>
/// 粉丝数据管理器（重构版）
/// 纯业务逻辑类，不负责数据持久化
/// 所有数据从 SaveManager 读取和写入
///
/// ⚠️ 已废弃：请直接使用 SaveManager 代替
/// 此类只是 SaveManager 的简单包装，已无存在必要
/// - GetTotalFans() → SaveManager.Instance.GetTotalFans()
/// - AddFans() → SaveManager.Instance.AddFans()
/// - GetUnlockedChapter() → SaveManager.Instance.GetUnlockedChapter()
/// - IsChapterUnlocked() → SaveManager.Instance.IsChapterUnlocked()
/// - GetRequiredFansForNextChapter() → SaveManager.Instance.GetRequiredFansForNextChapter()
/// </summary>
[System.Obsolete("FansDataManager 已废弃，请直接使用 SaveManager.Instance 代替", false)]
public static class FansDataManager
{
    /// <summary>
    /// 获取当前粉丝总数
    /// </summary>
    public static long GetTotalFans()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[FansDataManager] SaveManager not found!");
            return 0;
        }
        return SaveManager.Instance.GetTotalFans();
    }

    /// <summary>
    /// 增加粉丝数
    /// </summary>
    public static void AddFans(long fansToAdd)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[FansDataManager] SaveManager not found!");
            return;
        }

        long currentFans = GetTotalFans();
        long newFans = currentFans + fansToAdd;
        SaveManager.Instance.SetTotalFans(newFans);

        Debug.Log($"[FansData] Added {fansToAdd} fans. Total: {currentFans} -> {newFans}");

        // 检查是否解锁新章节
        CheckAndUnlockChapters();
    }

    /// <summary>
    /// 获取当前解锁的章节（0=第1章, 1=第2章, 2=第3章）
    /// </summary>
    public static int GetUnlockedChapter()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[FansDataManager] SaveManager not found!");
            return 0;
        }
        return SaveManager.Instance.GetUnlockedChapter();
    }

    /// <summary>
    /// 检查是否解锁指定章节
    /// 第1章：默认解锁
    /// 第2章：需要100万粉丝
    /// 第3章：需要1000万粉丝
    /// </summary>
    public static bool IsChapterUnlocked(int chapterIndex)
    {
        long totalFans = GetTotalFans();

        switch (chapterIndex)
        {
            case 0: // 第1章
                return true;
            case 1: // 第2章
                return totalFans >= 1000000; // 100万
            case 2: // 第3章
                return totalFans >= 10000000; // 1000万
            default:
                return false;
        }
    }

    /// <summary>
    /// 获取下一章节解锁所需粉丝数
    /// </summary>
    public static long GetRequiredFansForNextChapter(int currentChapter)
    {
        switch (currentChapter)
        {
            case 0: // 第1章 -> 第2章
                return 1000000; // 100万
            case 1: // 第2章 -> 第3章
                return 10000000; // 1000万
            case 2: // 第3章（已是最后一章）
                return long.MaxValue;
            default:
                return 0;
        }
    }

    /// <summary>
    /// 检查并解锁新章节（内部方法）
    /// </summary>
    private static void CheckAndUnlockChapters()
    {
        if (SaveManager.Instance == null) return;

        long totalFans = GetTotalFans();
        int currentUnlocked = GetUnlockedChapter();

        for (int i = currentUnlocked + 1; i <= 2; i++)
        {
            if (IsChapterUnlocked(i))
            {
                SaveManager.Instance.SetUnlockedChapter(i);
                Debug.Log($"[FansData] Unlocked chapter {i + 1}");
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 格式化粉丝数显示
    /// </summary>
    public static string FormatFansCount(long fans)
    {
        return ScoreCalculator.FormatLargeNumber(fans);
    }
}
