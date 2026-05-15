using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

/// <summary>
/// 收集PlayScene中的游玩数据
/// 包括判定统计、最大连击等信息
/// </summary>
public class PlayDataCollector : MonoBehaviour
{
    public static PlayDataCollector Instance { get; private set; }

    [Header("Play Data")]
    public PlayData CurrentPlayData { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI playCountText; // 改为显示播放量而不是达成率
    public float scoreTweenDuration = 0.3f; // 跑分动画持续时间

    private float gameStartTime; // 记录游戏开始时间
    private long lastTargetPlayCount = -1; // 上一次的目标分数
    private long displayedPlayCount = 0; // 当前正在显示的动态分数
    private Tween scoreTween; // 缓存Tween实例

    void Awake()
    {
        // 如果已经有实例存在
        if (Instance != null && Instance != this)
        {
            // 检查是否是从其他场景（如延迟校准）带来的旧实例
            // 如果旧实例的CurrentPlayData为null或未初始化，说明是旧实例，销毁它
            if (Instance.CurrentPlayData == null || string.IsNullOrEmpty(Instance.CurrentPlayData.songName))
            {
                Debug.LogWarning("[PlayDataCollector] Destroying old instance with invalid data.");
                Destroy(Instance.gameObject);
                Instance = this;
            }
            else
            {
                // 旧实例有有效数据，保留它，销毁新实例
                Debug.LogWarning("[PlayDataCollector] Keeping existing instance with valid data. Destroying this one.");
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            Instance = this;
        }

        // 初始化PlayData，防止null引用
        if (CurrentPlayData == null)
        {
            CurrentPlayData = new PlayData();
        }

        // 不再使用 DontDestroyOnLoad，因为结算界面已合并到 PlayScene
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 只在PlayScene中初始化数据
        // 检查是否在PlayScene中（通过查找NoteManager来判断）
        NoteManager noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager == null)
        {
            // 不在PlayScene中（可能在延迟校准或其他场景），不初始化
            Debug.Log("[PlayDataCollector] Not in PlayScene, skipping initialization");
            return;
        }

        // 在PlayScene中，重新初始化play data
        CurrentPlayData = new PlayData();

        // Get chart info
        ChartData chart = SongSelectionManager.GetSelectedChart();
        if (chart == null)
        {
            chart = noteManager.currentChart;
        }

        if (chart != null)
        {
            CurrentPlayData.songName = chart.songName;
            CurrentPlayData.difficulty = chart.difficulty;
            CurrentPlayData.totalNotes = chart.notes.Count;

            // 设置章节信息
            CurrentPlayData.chapterIndex = chart.chapterIndex;
            CurrentPlayData.isBossStage = chart.isBossStage;

            // 判断是否为重复挑战（使用 SaveManager 而不是 ClearDataManager）
            LevelData levelData = LevelUIManager.GetCurrentLevelData();
            if (levelData != null && SaveManager.Instance != null)
            {
                CurrentPlayData.isReplay = SaveManager.Instance.IsLevelCleared(
                    levelData.levelName,
                    chart.songName,
                    chart.difficulty
                );
            }
            else
            {
                CurrentPlayData.isReplay = false;
            }
        }

        Debug.Log("[PlayDataCollector] Initialized for PlayScene");

        // Reset ScoreManager stats for new play
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetStats();
        }

        // Initialize UI
        UpdatePlayCountUI();
    }

    void Update()
    {
        // Update play count UI in real-time
        UpdatePlayCountUI();
    }

    void OnDestroy()
    {
        // 只在离开PlayScene且是当前实例时收集数据
        // 注意：不要在这里收集，因为SongCompletionDetector已经在跳转前收集了
        if (Instance == this)
        {
            if (scoreTween != null) scoreTween.Kill();
            Instance = null; // 清理静态引用
            Debug.Log("[PlayDataCollector] Instance destroyed");
        }
    }

    /// <summary>
    /// 收集最终数据（在场景结束时调用）
    /// </summary>
    public void CollectFinalData()
    {
        // 防止null引用
        if (CurrentPlayData == null)
        {
            Debug.LogWarning("[PlayDataCollector] CurrentPlayData is null! Initializing new PlayData.");
            CurrentPlayData = new PlayData();
        }

        if (ScoreManager.Instance != null)
        {
            CurrentPlayData.perfectCount = ScoreManager.Instance.GetPerfectCount();
            CurrentPlayData.greatCount = ScoreManager.Instance.GetGreatCount();
            CurrentPlayData.goodCount = ScoreManager.Instance.GetGoodCount();
            CurrentPlayData.missCount = ScoreManager.Instance.GetMissCount();
            CurrentPlayData.maxCombo = ScoreManager.Instance.GetMaxCombo();
            CurrentPlayData.lateCount = ScoreManager.Instance.GetLateCount();
            CurrentPlayData.fastCount = ScoreManager.Instance.GetFastCount();
        }

        CurrentPlayData.playTime = Time.time;

        // 计算播放量和粉丝数
        long playCount = ScoreCalculator.CalculatePlayCount(
            CurrentPlayData.chapterIndex,
            CurrentPlayData.isBossStage,
            CurrentPlayData.perfectCount,
            CurrentPlayData.greatCount,
            CurrentPlayData.goodCount,
            CurrentPlayData.missCount,
            CurrentPlayData.maxCombo,
            CurrentPlayData.isReplay);

        long newFans = ScoreCalculator.CalculateFansGain(playCount, CurrentPlayData.isReplay);

        // ❌ 删除：不再通过 FansDataManager 添加粉丝（SaveLevelResult 内部会处理）
        // FansDataManager.AddFans(newFans);

        // ❌ 删除：不再通过 ClearDataManager 标记通关（SaveLevelResult 内部会处理）
        // if (!CurrentPlayData.isReplay)
        // {
        //     ChartData chart = SongSelectionManager.GetSelectedChart();
        //     if (chart != null)
        //     {
        //         ClearDataManager.MarkChartCleared(chart);
        //     }
        // }

        // 保存到新的存档系统（内部会自动添加粉丝、标记通关、标记敌人击败）
        SaveLevelResultToSaveManager(playCount, newFans);

        // 获取总粉丝数用于日志
        long totalFans = SaveManager.Instance.GetTotalFans();

        // Log the collected data
        Debug.Log($"[PlayDataCollector] === Play Data Summary ===");
        Debug.Log($"Song: {CurrentPlayData.songName}");
        Debug.Log($"Difficulty: {CurrentPlayData.difficulty}");
        Debug.Log($"Chapter: {CurrentPlayData.chapterIndex + 1}, Boss: {CurrentPlayData.isBossStage}, Replay: {CurrentPlayData.isReplay}");
        Debug.Log($"Perfect: {CurrentPlayData.perfectCount}");
        Debug.Log($"Great: {CurrentPlayData.greatCount}");
        Debug.Log($"Good: {CurrentPlayData.goodCount}");
        Debug.Log($"Miss: {CurrentPlayData.missCount}");
        Debug.Log($"Max Combo: {CurrentPlayData.maxCombo}");
        Debug.Log($"Total Notes: {CurrentPlayData.totalNotes}");
        Debug.Log($"Late: {CurrentPlayData.lateCount}, Fast: {CurrentPlayData.fastCount}");
        Debug.Log($"Accuracy: {CurrentPlayData.GetAccuracy():F2}%");
        Debug.Log($"Play Count: {ScoreCalculator.FormatLargeNumber(playCount)}");
        Debug.Log($"New Fans: {ScoreCalculator.FormatLargeNumber(newFans)}");
        Debug.Log($"Total Fans: {ScoreCalculator.FormatLargeNumber(totalFans)}");
    }

    /// <summary>
    /// 获取当前实时数据
    /// </summary>
    public PlayData GetCurrentData()
    {
        if (ScoreManager.Instance != null)
        {
            CurrentPlayData.perfectCount = ScoreManager.Instance.GetPerfectCount();
            CurrentPlayData.greatCount = ScoreManager.Instance.GetGreatCount();
            CurrentPlayData.goodCount = ScoreManager.Instance.GetGoodCount();
            CurrentPlayData.missCount = ScoreManager.Instance.GetMissCount();
            CurrentPlayData.maxCombo = ScoreManager.Instance.GetMaxCombo();
            CurrentPlayData.lateCount = ScoreManager.Instance.GetLateCount();
            CurrentPlayData.fastCount = ScoreManager.Instance.GetFastCount();
        }

        return CurrentPlayData;
    }

    /// <summary>
    /// 更新播放量UI显示
    /// </summary>
    private void UpdatePlayCountUI()
    {
        if (playCountText == null) return;

        // Get current data
        GetCurrentData();

        // Calculate target play count
        long targetPlayCount = ScoreCalculator.CalculatePlayCount(
            CurrentPlayData.chapterIndex,
            CurrentPlayData.isBossStage,
            CurrentPlayData.perfectCount,
            CurrentPlayData.greatCount,
            CurrentPlayData.goodCount,
            CurrentPlayData.missCount,
            CurrentPlayData.maxCombo,
            CurrentPlayData.isReplay);

        // 只有当目标分数发生变化时才启动/更新动画
        if (targetPlayCount != lastTargetPlayCount)
        {
            lastTargetPlayCount = targetPlayCount;

            // 杀掉旧动画
            if (scoreTween != null) scoreTween.Kill();

            // 创建新的补间动画
            // 使用 DOTween.To 改变 displayedPlayCount
            scoreTween = DOTween.To(() => displayedPlayCount, x => displayedPlayCount = (long)x, targetPlayCount, scoreTweenDuration)
                .SetEase(Ease.OutQuad) // 使用淡出效果，让结束更平滑
                .OnUpdate(() =>
                {
                    playCountText.text = ScoreCalculator.FormatLargeNumber(displayedPlayCount);
                });
        }
    }

    /// <summary>
    /// 保存关卡结果到 SaveManager
    /// </summary>
    private void SaveLevelResultToSaveManager(long playCount, long newFans)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[PlayDataCollector] SaveManager not found, skipping save");
            return;
        }

        // 获取关卡数据（从 PlaySceneInitializer 或 LevelUIManager 获取）
        PlaySceneInitializer initializer = FindFirstObjectByType<PlaySceneInitializer>();
        LevelData levelData = LevelUIManager.GetCurrentLevelData();

        // 计算评级
        string rank = CurrentPlayData.GetRank();

        if (levelData == null)
        {
            Debug.LogWarning("[PlayDataCollector] LevelData not found, using fallback save method");
            string levelName = initializer != null ? initializer.GetCurrentLevelName() : "Unknown";

            SaveManager.Instance.SaveLevelResult(
                levelName,
                CurrentPlayData.songName,
                CurrentPlayData.difficulty,
                CurrentPlayData.GetAchievementScore(),
                rank,
                CurrentPlayData.perfectCount,
                CurrentPlayData.greatCount,
                CurrentPlayData.goodCount,
                CurrentPlayData.missCount,
                CurrentPlayData.maxCombo,
                newFans
            );
            return;
        }

        // 使用带 LevelData 的重载方法保存（这样才能正确触发主线剧情）
        SaveManager.Instance.SaveLevelResult(
            levelData,
            CurrentPlayData.songName,
            CurrentPlayData.difficulty,
            CurrentPlayData.GetAchievementScore(),
            rank,
            CurrentPlayData.perfectCount,
            CurrentPlayData.greatCount,
            CurrentPlayData.goodCount,
            CurrentPlayData.missCount,
            CurrentPlayData.maxCombo,
            newFans
        );

        Debug.Log($"[PlayDataCollector] Saved level result to SaveManager: {levelData.levelName} - {CurrentPlayData.difficulty}, isMainStoryLevel: {levelData.isMainStoryLevel}");
    }

    /// <summary>
    /// 清理PlayDataCollector实例（在结算页面使用完数据后调用）
    /// </summary>
    public static void Cleanup()
    {
        if (Instance != null)
        {
            Debug.Log("[PlayDataCollector] Cleaning up instance");
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
}

/// <summary>
/// 游玩数据结构
/// </summary>
[System.Serializable]
public class PlayData
{
    public string songName;
    public ChartDifficulty difficulty;
    public int perfectCount;
    public int greatCount;
    public int goodCount;
    public int missCount;
    public int maxCombo;
    public int totalNotes;
    public float playTime;
    public int lateCount;
    public int fastCount;

    // 新增：章节和关卡信息
    public int chapterIndex = 0; // 章节索引 (0=第1章, 1=第2章, 2=第3章)
    public bool isBossStage = false; // 是否为Boss关卡
    public bool isReplay = false; // 是否为重复挑战已通关关卡

    /// <summary>
    /// 计算准确率（Perfect和Great算作准确）
    /// </summary>
    public float GetAccuracy()
    {
        if (totalNotes == 0) return 0f;
        return ((float)(perfectCount + greatCount) / totalNotes) * 100f;
    }

    /// <summary>
    /// 计算达成率（满分1000000）
    /// 判定分占90%：Perfect=100%, Great=80%, Good=50%, Miss=0%
    /// 连击分占10%：MaxCombo/TotalNotes
    /// </summary>
    public int GetAchievementScore()
    {
        if (totalNotes == 0) return 0;

        // 判定分（90%权重）
        float judgmentScore = 0f;
        judgmentScore += perfectCount * 1.0f;  // 100%
        judgmentScore += greatCount * 0.8f;    // 80%
        judgmentScore += goodCount * 0.5f;     // 50%
        // miss = 0%

        float judgmentRatio = judgmentScore / totalNotes;
        float judgmentPoints = judgmentRatio * 900000f; // 90% of 1000000

        // 连击分（10%权重）
        float comboRatio = (float)maxCombo / totalNotes;
        float comboPoints = comboRatio * 100000f; // 10% of 1000000

        // 总达成率
        int totalScore = Mathf.RoundToInt(judgmentPoints + comboPoints);
        return Mathf.Clamp(totalScore, 0, 1000000);
    }

    /// <summary>
    /// 计算得分（可以根据需要自定义计分规则）
    /// </summary>
    public int GetScore()
    {
        return (perfectCount * 100) + (greatCount * 80) + (goodCount * 50);
    }

    /// <summary>
    /// 获取评级（S, A, B, C, F）基于达成率
    /// S: 95%以上, A: 92%以上, B: 85%以上, C: 78%以上, F: 78%以下
    /// </summary>
    public string GetRank()
    {
        float achievementRate = GetAchievementRate();
        if (achievementRate >= 95f) return "S";
        if (achievementRate >= 92f) return "A";
        if (achievementRate >= 85f) return "B";
        if (achievementRate >= 78f) return "C";
        return "F";
    }

    /// <summary>
    /// 获取达成率百分比（0-100）
    /// </summary>
    public float GetAchievementRate()
    {
        return (GetAchievementScore() / 1000000f) * 100f;
    }

    /// <summary>
    /// 是否达成Full Combo
    /// </summary>
    public bool IsFullCombo()
    {
        return missCount == 0 && (perfectCount + greatCount + goodCount) == totalNotes;
    }

    /// <summary>
    /// 是否达成All Perfect
    /// </summary>
    public bool IsAllPerfect()
    {
        return perfectCount == totalNotes && missCount == 0 && greatCount == 0 && goodCount == 0;
    }
}
