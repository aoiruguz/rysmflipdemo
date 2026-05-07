using UnityEngine;

/// <summary>
/// 计算播放量的核心逻辑
/// 播放量 = 基础分 + 连击加成
/// </summary>
public class ScoreCalculator
{
    // 章节倍数：10^ChapterIndex
    private static readonly long[] ChapterMultipliers = { 10, 100, 1000 };

    // 普通关卡判定得分
    private const int NORMAL_PERFECT = 600;
    private const int NORMAL_GREAT = 300;
    private const int NORMAL_GOOD = 100;
    private const int NORMAL_MISS_CH3 = -300; // 第3章Miss扣分

    // Boss关卡判定得分
    private const int BOSS_PERFECT = 100;
    private const int BOSS_GREAT = -300;
    private const int BOSS_GOOD = -600;
    private const int BOSS_MISS = -600;

    // 连击系统
    private const int COMBO_BASE_SCORE = 100; // 每个Combo基础分
    private const float COMBO_MULTIPLIER_START = 1.1f;
    private const float COMBO_MULTIPLIER_INCREMENT = 0.1f;
    private const int COMBO_INCREMENT_INTERVAL = 5;
    private const float COMBO_MULTIPLIER_MAX = 2.0f;

    /// <summary>
    /// 计算单局播放量
    /// </summary>
    /// <param name="chapterIndex">章节索引 (0=第1章, 1=第2章, 2=第3章)</param>
    /// <param name="isBossStage">是否为Boss关卡</param>
    /// <param name="perfectCount">Perfect数量</param>
    /// <param name="greatCount">Great数量</param>
    /// <param name="goodCount">Good数量</param>
    /// <param name="missCount">Miss数量</param>
    /// <param name="maxCombo">最大连击数</param>
    /// <param name="isReplay">是否为重复挑战已通关关卡</param>
    /// <returns>播放量（long类型，可达亿级）</returns>
    public static long CalculatePlayCount(
        int chapterIndex,
        bool isBossStage,
        int perfectCount,
        int greatCount,
        int goodCount,
        int missCount,
        int maxCombo,
        bool isReplay = false)
    {
        // 验证章节索引
        if (chapterIndex < 0 || chapterIndex >= ChapterMultipliers.Length)
        {
            Debug.LogError($"Invalid chapter index: {chapterIndex}");
            chapterIndex = 0;
        }

        long multiplier = ChapterMultipliers[chapterIndex];

        // 计算基础分
        long baseScore = CalculateBaseScore(
            chapterIndex,
            isBossStage,
            perfectCount,
            greatCount,
            goodCount,
            missCount,
            multiplier);

        // 计算连击加成
        long comboBonus = CalculateComboBonus(maxCombo, multiplier);

        // 总播放量
        long totalPlayCount = baseScore + comboBonus;

        // 确保不为负数
        if (totalPlayCount < 0)
        {
            totalPlayCount = 0;
        }

        // 重复挑战惩罚：只获得10%
        if (isReplay)
        {
            totalPlayCount = totalPlayCount / 10;
        }

        return totalPlayCount;
    }

    /// <summary>
    /// 计算基础分（判定得分）
    /// </summary>
    private static long CalculateBaseScore(
        int chapterIndex,
        bool isBossStage,
        int perfectCount,
        int greatCount,
        int goodCount,
        int missCount,
        long multiplier)
    {
        long score = 0;

        if (isBossStage)
        {
            // Boss关卡
            score += perfectCount * BOSS_PERFECT * multiplier;
            score += greatCount * BOSS_GREAT * multiplier;
            score += goodCount * BOSS_GOOD * multiplier;
            score += missCount * BOSS_MISS * multiplier;
        }
        else
        {
            // 普通关卡
            score += perfectCount * NORMAL_PERFECT * multiplier;
            score += greatCount * NORMAL_GREAT * multiplier;
            score += goodCount * NORMAL_GOOD * multiplier;

            // 第3章Miss扣分
            if (chapterIndex >= 2)
            {
                score += missCount * NORMAL_MISS_CH3 * multiplier;
            }
            // 第1-2章Miss不扣分
        }

        return score;
    }

    /// <summary>
    /// 计算连击加成
    /// 公式：每个Combo额外+100×章节倍数
    /// 连击倍数：Min(1.1 + (Combo / 5) * 0.1, 2.0)
    /// </summary>
    private static long CalculateComboBonus(int maxCombo, long multiplier)
    {
        if (maxCombo <= 0)
        {
            return 0;
        }

        long totalComboBonus = 0;

        // 遍历每个Combo，累加得分
        for (int combo = 1; combo <= maxCombo; combo++)
        {
            // 计算当前Combo的倍数
            float comboMultiplier = CalculateComboMultiplier(combo);

            // 当前Combo得分 = 100 × 章节倍数 × 连击倍数
            long comboScore = (long)(COMBO_BASE_SCORE * multiplier * comboMultiplier);
            totalComboBonus += comboScore;
        }

        return totalComboBonus;
    }

    /// <summary>
    /// 计算连击倍数
    /// 公式：Min(1.1 + (Combo / 5) * 0.1, 2.0)
    /// </summary>
    private static float CalculateComboMultiplier(int combo)
    {
        float multiplier = COMBO_MULTIPLIER_START + (combo / COMBO_INCREMENT_INTERVAL) * COMBO_MULTIPLIER_INCREMENT;
        return Mathf.Min(multiplier, COMBO_MULTIPLIER_MAX);
    }

    /// <summary>
    /// 计算粉丝增量（播放量的10%）
    /// </summary>
    public static long CalculateFansGain(long playCount, bool isReplay = false)
    {
        long fansGain = playCount / 10;

        // 重复挑战惩罚已在播放量计算中应用，这里不需要再次应用
        // 因为传入的playCount已经是惩罚后的值

        return fansGain;
    }

    /// <summary>
    /// 格式化大数字显示（万、亿）
    /// </summary>
    public static string FormatLargeNumber(long number)
    {
        if (number >= 100000000) // 1亿
        {
            float yi = number / 100000000f;
            return $"{yi:F2}亿";
        }
        else if (number >= 10000) // 1万
        {
            float wan = number / 10000f;
            return $"{wan:F2}万";
        }
        else
        {
            return number.ToString();
        }
    }

    /// <summary>
    /// 获取章节倍数
    /// </summary>
    public static long GetChapterMultiplier(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= ChapterMultipliers.Length)
        {
            return ChapterMultipliers[0];
        }
        return ChapterMultipliers[chapterIndex];
    }
}
