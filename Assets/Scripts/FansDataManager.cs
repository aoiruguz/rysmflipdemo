using UnityEngine;

/// <summary>
/// 粉丝数据管理器
/// 负责保存和加载粉丝总数（跨局累计）
/// </summary>
public class FansDataManager
{
    private const string FANS_KEY = "TotalFans";
    private const string CHAPTER_PROGRESS_KEY = "ChapterProgress"; // 当前解锁到第几章

    /// <summary>
    /// 获取当前粉丝总数
    /// </summary>
    public static long GetTotalFans()
    {
        // PlayerPrefs不支持long，需要分两部分存储
        string fansString = PlayerPrefs.GetString(FANS_KEY, "0");
        if (long.TryParse(fansString, out long fans))
        {
            return fans;
        }
        return 0;
    }

    /// <summary>
    /// 增加粉丝数
    /// </summary>
    public static void AddFans(long fansToAdd)
    {
        long currentFans = GetTotalFans();
        long newFans = currentFans + fansToAdd;
        SetTotalFans(newFans);

        Debug.Log($"[FansData] Added {fansToAdd} fans. Total: {currentFans} -> {newFans}");
    }

    /// <summary>
    /// 设置粉丝总数（慎用，一般使用AddFans）
    /// </summary>
    public static void SetTotalFans(long fans)
    {
        PlayerPrefs.SetString(FANS_KEY, fans.ToString());
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 获取当前解锁的章节（0=第1章, 1=第2章, 2=第3章）
    /// </summary>
    public static int GetUnlockedChapter()
    {
        return PlayerPrefs.GetInt(CHAPTER_PROGRESS_KEY, 0);
    }

    /// <summary>
    /// 设置解锁的章节
    /// </summary>
    public static void SetUnlockedChapter(int chapterIndex)
    {
        int current = GetUnlockedChapter();
        if (chapterIndex > current)
        {
            PlayerPrefs.SetInt(CHAPTER_PROGRESS_KEY, chapterIndex);
            PlayerPrefs.Save();
            Debug.Log($"[FansData] Unlocked chapter {chapterIndex + 1}");
        }
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
    /// 重置所有数据（调试用）
    /// </summary>
    public static void ResetAllData()
    {
        PlayerPrefs.DeleteKey(FANS_KEY);
        PlayerPrefs.DeleteKey(CHAPTER_PROGRESS_KEY);
        PlayerPrefs.Save();
        Debug.Log("[FansData] All data reset");
    }

    /// <summary>
    /// 格式化粉丝数显示
    /// </summary>
    public static string FormatFansCount(long fans)
    {
        return ScoreCalculator.FormatLargeNumber(fans);
    }
}
