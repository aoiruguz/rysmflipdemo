using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

    private float gameStartTime; // 记录游戏开始时间

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

        // 使用DontDestroyOnLoad，让数据能传递到结算页面
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 只在PlayScene中初始化数据
        // 检查是否在PlayScene中（通过查找NoteManager来判断）
        NoteManager noteManager = FindObjectOfType<NoteManager>();
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

            // 检查是否为重复挑战（已通关的关卡）
            CurrentPlayData.isReplay = ClearDataManager.IsChartCleared(chart);
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

        // 保存粉丝数
        FansDataManager.AddFans(newFans);

        // 标记关卡为已通关（如果不是重复挑战）
        if (!CurrentPlayData.isReplay)
        {
            ChartData chart = SongSelectionManager.GetSelectedChart();
            if (chart != null)
            {
                ClearDataManager.MarkChartCleared(chart);
            }
        }

        // 检查是否解锁新章节
        long totalFans = FansDataManager.GetTotalFans();
        int currentUnlocked = FansDataManager.GetUnlockedChapter();
        for (int i = currentUnlocked + 1; i < 3; i++)
        {
            if (FansDataManager.IsChapterUnlocked(i))
            {
                FansDataManager.SetUnlockedChapter(i);
            }
        }

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

        // Calculate and display play count
        long playCount = ScoreCalculator.CalculatePlayCount(
            CurrentPlayData.chapterIndex,
            CurrentPlayData.isBossStage,
            CurrentPlayData.perfectCount,
            CurrentPlayData.greatCount,
            CurrentPlayData.goodCount,
            CurrentPlayData.missCount,
            CurrentPlayData.maxCombo,
            CurrentPlayData.isReplay);

        playCountText.text = ScoreCalculator.FormatLargeNumber(playCount);
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
