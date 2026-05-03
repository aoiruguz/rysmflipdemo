using UnityEngine;
using TMPro;

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
    public TextMeshProUGUI achievementScoreText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 保持数据在场景切换时不被销毁
    }

    void Start()
    {
        // Initialize play data
        CurrentPlayData = new PlayData();

        // Get chart info
        ChartData chart = SongSelectionManager.GetSelectedChart();
        if (chart == null)
        {
            NoteManager noteManager = FindObjectOfType<NoteManager>();
            if (noteManager != null)
            {
                chart = noteManager.currentChart;
            }
        }

        if (chart != null)
        {
            CurrentPlayData.songName = chart.songName;
            CurrentPlayData.difficulty = chart.difficulty;
            CurrentPlayData.totalNotes = chart.notes.Count;
        }

        Debug.Log("[PlayDataCollector] Initialized for PlayScene");

        // Reset ScoreManager stats for new play
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetStats();
        }

        // Initialize UI
        UpdateAchievementScoreUI();
    }

    void Update()
    {
        // Update achievement score UI in real-time
        UpdateAchievementScoreUI();
    }

    void OnDestroy()
    {
        // Collect final data when leaving PlayScene
        if (Instance == this)
        {
            CollectFinalData();
        }
    }

    /// <summary>
    /// 收集最终数据（在场景结束时调用）
    /// </summary>
    public void CollectFinalData()
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

        CurrentPlayData.playTime = Time.time;

        // Log the collected data
        Debug.Log($"[PlayDataCollector] === Play Data Summary ===");
        Debug.Log($"Song: {CurrentPlayData.songName}");
        Debug.Log($"Difficulty: {CurrentPlayData.difficulty}");
        Debug.Log($"Perfect: {CurrentPlayData.perfectCount}");
        Debug.Log($"Great: {CurrentPlayData.greatCount}");
        Debug.Log($"Good: {CurrentPlayData.goodCount}");
        Debug.Log($"Miss: {CurrentPlayData.missCount}");
        Debug.Log($"Max Combo: {CurrentPlayData.maxCombo}");
        Debug.Log($"Total Notes: {CurrentPlayData.totalNotes}");
        Debug.Log($"Late: {CurrentPlayData.lateCount}, Fast: {CurrentPlayData.fastCount}");
        Debug.Log($"Accuracy: {CurrentPlayData.GetAccuracy():F2}%");
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
    /// 更新达成率UI显示
    /// </summary>
    private void UpdateAchievementScoreUI()
    {
        if (achievementScoreText == null) return;

        // Get current data
        GetCurrentData();

        // Calculate and display achievement score
        int score = CurrentPlayData.GetAchievementScore();
        achievementScoreText.text = score.ToString("D7"); // 7位数字，前面补0
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
