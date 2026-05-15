using UnityEngine;
using UnityEditor;

/// <summary>
/// 播放量和粉丝系统测试工具
/// 用于验证计算逻辑是否正确
/// </summary>
public class ScoreSystemTester : EditorWindow
{
    private int chapterIndex = 0;
    private bool isBossStage = false;
    private int perfectCount = 100;
    private int greatCount = 0;
    private int goodCount = 0;
    private int missCount = 0;
    private int maxCombo = 100;
    private bool isReplay = false;

    [MenuItem("Tools/Score System Tester")]
    public static void ShowWindow()
    {
        GetWindow<ScoreSystemTester>("Score System Tester");
    }

    void OnGUI()
    {
        GUILayout.Label("播放量和粉丝系统测试", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 章节选择
        chapterIndex = EditorGUILayout.IntSlider("章节 (0=第1章, 1=第2章, 2=第3章)", chapterIndex, 0, 2);
        isBossStage = EditorGUILayout.Toggle("是否为Boss关卡", isBossStage);
        isReplay = EditorGUILayout.Toggle("是否为重复挑战", isReplay);

        EditorGUILayout.Space();

        // 判定统计
        GUILayout.Label("判定统计", EditorStyles.boldLabel);
        perfectCount = EditorGUILayout.IntField("Perfect", perfectCount);
        greatCount = EditorGUILayout.IntField("Great", greatCount);
        goodCount = EditorGUILayout.IntField("Good", goodCount);
        missCount = EditorGUILayout.IntField("Miss", missCount);
        maxCombo = EditorGUILayout.IntField("Max Combo", maxCombo);

        EditorGUILayout.Space();

        // 计算按钮
        if (GUILayout.Button("计算播放量和粉丝", GUILayout.Height(30)))
        {
            CalculateAndDisplay();
        }

        EditorGUILayout.Space();

        // 快速测试按钮
        GUILayout.Label("快速测试场景", EditorStyles.boldLabel);

        if (GUILayout.Button("第1章普通关卡 - 全Perfect"))
        {
            SetupTest(0, false, 100, 0, 0, 0, 100, false);
        }

        if (GUILayout.Button("第2章普通关卡 - 全Perfect"))
        {
            SetupTest(1, false, 100, 0, 0, 0, 100, false);
        }

        if (GUILayout.Button("第3章普通关卡 - 全Perfect"))
        {
            SetupTest(2, false, 100, 0, 0, 0, 100, false);
        }

        if (GUILayout.Button("第3章Boss关卡 - 全Perfect"))
        {
            SetupTest(2, true, 100, 0, 0, 0, 100, false);
        }

        if (GUILayout.Button("第3章Boss关卡 - 50P/50G"))
        {
            SetupTest(2, true, 50, 50, 0, 0, 80, false);
        }

        EditorGUILayout.Space();

        // 粉丝数据管理
        GUILayout.Label("粉丝数据管理", EditorStyles.boldLabel);

        long totalFans = FansDataManager.GetTotalFans();
        EditorGUILayout.LabelField("当前总粉丝数", ScoreCalculator.FormatLargeNumber(totalFans));

        int unlockedChapter = FansDataManager.GetUnlockedChapter();
        EditorGUILayout.LabelField("已解锁章节", $"第{unlockedChapter + 1}章");

        EditorGUILayout.Space();

        if (GUILayout.Button("重置所有粉丝数据"))
        {
            if (EditorUtility.DisplayDialog("确认重置", "确定要重置所有粉丝数据吗？", "确定", "取消"))
            {
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SetTotalFans(0);
                    SaveManager.Instance.SetUnlockedChapter(0);
                    Debug.Log("[ScoreSystemTester] 粉丝数据已重置");
                }
                else
                {
                    Debug.LogError("[ScoreSystemTester] SaveManager not found!");
                }
            }
        }

        if (GUILayout.Button("重置所有通关记录"))
        {
            if (EditorUtility.DisplayDialog("确认重置", "确定要重置所有通关记录吗？这将删除所有PlayerPrefs数据！", "确定", "取消"))
            {
                ClearDataManager.ResetAllClearData();
                Debug.Log("[ScoreSystemTester] 通关记录已重置");
            }
        }
    }

    private void SetupTest(int chapter, bool boss, int perfect, int great, int good, int miss, int combo, bool replay)
    {
        chapterIndex = chapter;
        isBossStage = boss;
        perfectCount = perfect;
        greatCount = great;
        goodCount = good;
        missCount = miss;
        maxCombo = combo;
        isReplay = replay;

        CalculateAndDisplay();
    }

    private void CalculateAndDisplay()
    {
        long playCount;
        if (isBossStage)
        {
            long mult = ScoreCalculator.GetChapterMultiplier(chapterIndex);
            long bossScore = perfectCount * 100 * mult + greatCount * -300 * mult + goodCount * -600 * mult + missCount * -600 * mult;
            long comboBonus = ScoreCalculator.CalculateComboBonus(maxCombo, mult);
            playCount = bossScore + comboBonus;
            if (playCount < 0) playCount = 0;
        }
        else
        {
            playCount = ScoreCalculator.CalculatePlayCount(
                chapterIndex,
                perfectCount,
                greatCount,
                goodCount,
                missCount,
                maxCombo);
        }

        // 计算粉丝增量
        long fansGain = ScoreCalculator.CalculateFansGain(playCount, isReplay);

        // 获取章节倍数
        long multiplier = ScoreCalculator.GetChapterMultiplier(chapterIndex);

        // 输出结果
        Debug.Log("========== 计算结果 ==========");
        Debug.Log($"章节: 第{chapterIndex + 1}章 (倍数: {multiplier})");
        Debug.Log($"关卡类型: {(isBossStage ? "Boss关卡" : "普通关卡")}");
        Debug.Log($"是否重复挑战: {(isReplay ? "是" : "否")}");
        Debug.Log($"判定统计: P={perfectCount}, G={greatCount}, Go={goodCount}, M={missCount}");
        Debug.Log($"最大连击: {maxCombo}");
        Debug.Log($"播放量: {ScoreCalculator.FormatLargeNumber(playCount)} ({playCount})");
        Debug.Log($"粉丝增量: {ScoreCalculator.FormatLargeNumber(fansGain)} ({fansGain})");
        Debug.Log("==============================");
    }
}
