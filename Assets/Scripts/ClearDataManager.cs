using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 通关记录管理器
/// 记录玩家已通关的关卡，用于判断是否为重复挑战
///
/// ⚠️ 已废弃：请使用 SaveManager.IsLevelCleared() 代替
/// 此类使用 PlayerPrefs 存储，已被统一的 SaveManager (JSON) 系统取代
/// </summary>
[System.Obsolete("ClearDataManager 已废弃，请使用 SaveManager.IsLevelCleared() 和 SaveManager.SaveLevelResult() 代替", false)]
public static class ClearDataManager
{
    private const string CLEAR_DATA_PREFIX = "ClearedChart_";

    /// <summary>
    /// 标记关卡为已通关
    /// </summary>
    public static void MarkChartCleared(ChartData chart)
    {
        if (chart == null) return;

        string key = GetChartKey(chart);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        Debug.Log($"[ClearData] Marked as cleared: {chart.songName} ({chart.difficulty})");
    }

    /// <summary>
    /// 检查关卡是否已通关
    /// </summary>
    public static bool IsChartCleared(ChartData chart)
    {
        if (chart == null) return false;

        string key = GetChartKey(chart);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    /// <summary>
    /// 清除指定关卡的通关记录
    /// </summary>
    public static void ClearChartRecord(ChartData chart)
    {
        if (chart == null) return;

        string key = GetChartKey(chart);
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();

        Debug.Log($"[ClearData] Cleared record: {chart.songName} ({chart.difficulty})");
    }

    /// <summary>
    /// 重置所有通关记录
    /// </summary>
    public static void ResetAllClearData()
    {
        // 注意：PlayerPrefs没有直接的"删除所有带前缀的key"的方法
        // 这里只能删除所有PlayerPrefs（包括其他设置）
        // 更好的做法是维护一个已通关关卡的列表

        Debug.LogWarning("[ClearData] ResetAllClearData called - this will delete ALL PlayerPrefs!");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 生成关卡的唯一标识符
    /// 格式: ClearedChart_{songName}_{difficulty}_{chapterIndex}_{isBoss}
    /// </summary>
    private static string GetChartKey(ChartData chart)
    {
        // 使用歌曲名、难度、章节、是否Boss来生成唯一key
        string songNameSafe = chart.songName.Replace(" ", "_");
        return $"{CLEAR_DATA_PREFIX}{songNameSafe}_{chart.difficulty}_{chart.chapterIndex}_{chart.isBossStage}";
    }

    /// <summary>
    /// 获取已通关的关卡数量（估算）
    /// 注意：这个方法只能估算，因为PlayerPrefs没有列出所有key的功能
    /// </summary>
    public static int GetClearedChartCount()
    {
        // 这个方法无法准确实现，因为PlayerPrefs不支持枚举所有key
        // 如果需要这个功能，建议使用JSON文件存储通关列表
        Debug.LogWarning("[ClearData] GetClearedChartCount is not accurately implemented");
        return 0;
    }
}
