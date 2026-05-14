using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

/// <summary>
/// Boss战数据收集器
/// 收集Boss战中的游玩数据，但不转化粉丝
/// </summary>
public class BossPlayDataCollector : MonoBehaviour
{
    public static BossPlayDataCollector Instance { get; private set; }

    [Header("Play Data")]
    public PlayData CurrentPlayData { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI playCountText; // Boss战中可能不显示，但保留接口
    public float scoreTweenDuration = 0.3f;

    private float gameStartTime;
    private long lastTargetPlayCount = -1;
    private long displayedPlayCount = 0;
    private Tween scoreTween;

    void Awake()
    {
        // 如果已经有实例存在
        if (Instance != null && Instance != this)
        {
            if (Instance.CurrentPlayData == null || string.IsNullOrEmpty(Instance.CurrentPlayData.songName))
            {
                Debug.LogWarning("[BossPlayDataCollector] Destroying old instance with invalid data.");
                Destroy(Instance.gameObject);
                Instance = this;
            }
            else
            {
                Debug.LogWarning("[BossPlayDataCollector] Keeping existing instance with valid data. Destroying this one.");
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            Instance = this;
        }

        // 初始化PlayData
        if (CurrentPlayData == null)
        {
            CurrentPlayData = new PlayData();
        }
    }

    void Start()
    {
        // 检查是否在BossPlayScene中
        NoteManager noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager == null)
        {
            Debug.Log("[BossPlayDataCollector] Not in BossPlayScene, skipping initialization");
            return;
        }

        // 在BossPlayScene中，重新初始化play data
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
            CurrentPlayData.isBossStage = true; // Boss战标记

            // Boss战不检查是否为重复挑战
            CurrentPlayData.isReplay = false;
        }

        Debug.Log("[BossPlayDataCollector] Initialized for BossPlayScene");

        // Reset ScoreManager stats for new play
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetStats();
        }

        // Initialize UI (如果有的话)
        UpdatePlayCountUI();
    }

    void Update()
    {
        // Update play count UI in real-time (如果需要显示)
        UpdatePlayCountUI();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            if (scoreTween != null) scoreTween.Kill();
            Instance = null;
            Debug.Log("[BossPlayDataCollector] Instance destroyed");
        }
    }

    /// <summary>
    /// 收集最终数据（在Boss战结束时调用）
    /// 注意：Boss战不转化粉丝，不保存到存档系统
    /// </summary>
    public void CollectFinalData()
    {
        // 防止null引用
        if (CurrentPlayData == null)
        {
            Debug.LogWarning("[BossPlayDataCollector] CurrentPlayData is null! Initializing new PlayData.");
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

        // 计算播放量（仅用于显示，不转化粉丝）
        long playCount = ScoreCalculator.CalculatePlayCount(
            CurrentPlayData.chapterIndex,
            CurrentPlayData.isBossStage,
            CurrentPlayData.perfectCount,
            CurrentPlayData.greatCount,
            CurrentPlayData.goodCount,
            CurrentPlayData.missCount,
            CurrentPlayData.maxCombo,
            CurrentPlayData.isReplay);

        // Boss战不转化粉丝
        // FansDataManager.AddFans(newFans); // 不调用

        // Boss战不标记关卡为已通关
        // ClearDataManager.MarkChartCleared(chart); // 不调用

        // Boss战不保存到存档系统
        // SaveManager.Instance.SaveLevelResult(...); // 不调用

        // Log the collected data
        Debug.Log($"[BossPlayDataCollector] === Boss Battle Data Summary ===");
        Debug.Log($"Song: {CurrentPlayData.songName}");
        Debug.Log($"Difficulty: {CurrentPlayData.difficulty}");
        Debug.Log($"Boss Stage: {CurrentPlayData.isBossStage}");
        Debug.Log($"Perfect: {CurrentPlayData.perfectCount}");
        Debug.Log($"Great: {CurrentPlayData.greatCount}");
        Debug.Log($"Good: {CurrentPlayData.goodCount}");
        Debug.Log($"Miss: {CurrentPlayData.missCount}");
        Debug.Log($"Max Combo: {CurrentPlayData.maxCombo}");
        Debug.Log($"Total Notes: {CurrentPlayData.totalNotes}");
        Debug.Log($"Late: {CurrentPlayData.lateCount}, Fast: {CurrentPlayData.fastCount}");
        Debug.Log($"Accuracy: {CurrentPlayData.GetAccuracy():F2}%");
        Debug.Log($"Play Count: {ScoreCalculator.FormatLargeNumber(playCount)}");
        Debug.Log($"[Boss Battle] No fans gained (Boss battle mode)");
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
    /// 更新播放量UI显示（如果需要）
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
            scoreTween = DOTween.To(() => displayedPlayCount, x => displayedPlayCount = (long)x, targetPlayCount, scoreTweenDuration)
                .SetEase(Ease.OutQuad)
                .OnUpdate(() =>
                {
                    playCountText.text = ScoreCalculator.FormatLargeNumber(displayedPlayCount);
                });
        }
    }

    /// <summary>
    /// 清理BossPlayDataCollector实例（在结算页面使用完数据后调用）
    /// </summary>
    public static void Cleanup()
    {
        if (Instance != null)
        {
            Debug.Log("[BossPlayDataCollector] Cleaning up instance");
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }
}
