using UnityEngine;

/// <summary>
/// Boss战专用生命系统
/// 管理双生命值：玩家生命值 + 粉丝数量
/// </summary>
public class BossHealthSystem : MonoBehaviour
{
    public static BossHealthSystem Instance { get; private set; }

    [Header("Fans Health Settings")]
    [Tooltip("指数倍数（基础倍数）")]
    public long multiplier = 1000;

    [Header("Judgment Fans Change")]
    [Tooltip("Perfect判定：增加粉丝")]
    public int perfectFansChange = 100;

    [Tooltip("Great判定：减少粉丝")]
    public int greatFansChange = -300;

    [Tooltip("Good判定：减少粉丝")]
    public int goodFansChange = -600;

    [Tooltip("Miss判定：减少粉丝")]
    public int missFansChange = -600;

    [Header("References")]
    [Tooltip("玩家生命系统引用")]
    public HealthSystem playerHealthSystem;

    [Header("Testing / Debug")]
    [Tooltip("测试模式：手动设置粉丝数")]
    public bool enableTestMode = false;

    [Tooltip("测试用粉丝数（启用测试模式时使用）")]
    public long testFansAmount = 1000000;

    // 粉丝生命值
    public long MaxFans { get; private set; }
    public long CurrentFans { get; private set; }
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 自动查找HealthSystem
        if (playerHealthSystem == null)
        {
            playerHealthSystem = FindFirstObjectByType<HealthSystem>();
        }

        // 初始化粉丝生命值
        InitializeFansHealth();
    }

    /// <summary>
    /// 初始化粉丝生命值（从累计粉丝池复制）
    /// </summary>
    private void InitializeFansHealth()
    {
        // 测试模式：使用手动设置的粉丝数
        if (enableTestMode)
        {
            MaxFans = testFansAmount;
            CurrentFans = testFansAmount;
            Debug.Log($"[BossHealthSystem] Test Mode Enabled. Using test fans: {testFansAmount}");
        }
        else
        {
            // 从FansDataManager读取累计粉丝数作为最大值
            MaxFans = FansDataManager.GetTotalFans();
            CurrentFans = MaxFans; // 复制为当前值
        }

        IsGameOver = false;

        Debug.Log($"[BossHealthSystem] Initialized. Max Fans: {MaxFans}, Current Fans: {CurrentFans}");

        // 通知UI更新
        BossFansUI.Instance?.UpdateFans(CurrentFans, MaxFans);
    }

    /// <summary>
    /// 根据判定类型改变粉丝数量
    /// </summary>
    /// <param name="judgment">判定类型：PERFECT, GREAT, GOOD, MISS</param>
    public void OnJudgment(string judgment)
    {
        if (IsGameOver) return;

        long fansChange = 0;

        switch (judgment.ToUpper())
        {
            case "PERFECT":
                fansChange = perfectFansChange * multiplier; // +100 × 1000 = +100,000
                break;
            case "GREAT":
                fansChange = greatFansChange * multiplier; // -300 × 1000 = -300,000
                break;
            case "GOOD":
                fansChange = goodFansChange * multiplier; // -600 × 1000 = -600,000
                break;
            case "MISS":
                fansChange = missFansChange * multiplier; // -600 × 1000 = -600,000
                break;
            default:
                Debug.LogWarning($"[BossHealthSystem] Unknown judgment: {judgment}");
                return;
        }

        // 应用粉丝变化
        CurrentFans += fansChange;
        CurrentFans = System.Math.Max(0, CurrentFans);

        Debug.Log($"[BossHealthSystem] Judgment: {judgment}, Fans Change: {fansChange:+#;-#;0}, Current Fans: {CurrentFans}/{MaxFans}");

        // 通知UI更新
        BossFansUI.Instance?.UpdateFans(CurrentFans, MaxFans);

        // 检查粉丝数是否归零
        if (CurrentFans <= 0)
        {
            IsGameOver = true;
            OnGameOver("粉丝数归零");
        }
    }

    /// <summary>
    /// 扣除粉丝数量（由ScoreManager的OnMiss调用）
    /// 已弃用，使用OnJudgment代替
    /// </summary>
    [System.Obsolete("Use OnJudgment instead")]
    public void TakeFansDamage()
    {
        OnJudgment("MISS");
    }

    /// <summary>
    /// 检查玩家生命值是否归零（每帧检查）
    /// </summary>
    void Update()
    {
        if (IsGameOver) return;

        // 检查玩家生命值
        if (playerHealthSystem != null && playerHealthSystem.IsGameOver)
        {
            IsGameOver = true;
            OnGameOver("生命值归零");
        }
    }

    /// <summary>
    /// 游戏失败处理
    /// </summary>
    private void OnGameOver(string reason)
    {
        Debug.Log($"[BossHealthSystem] Game Over! Reason: {reason}");

        // 通知 BossSongCompletionDetector 处理场景跳转
        if (BossSongCompletionDetector.Instance != null)
        {
            BossSongCompletionDetector.Instance.OnGameOver();
        }
        else
        {
            Debug.LogWarning("[BossHealthSystem] No BossSongCompletionDetector found! Cannot transition to Game Over scene.");
        }
    }

    /// <summary>
    /// 获取粉丝数百分比
    /// </summary>
    public float GetFansPercentage()
    {
        if (MaxFans == 0) return 0f;
        return (float)CurrentFans / MaxFans;
    }

    /// <summary>
    /// 格式化粉丝数显示
    /// </summary>
    public string GetFormattedCurrentFans()
    {
        return ScoreCalculator.FormatLargeNumber(CurrentFans);
    }

    /// <summary>
    /// 格式化最大粉丝数显示
    /// </summary>
    public string GetFormattedMaxFans()
    {
        return ScoreCalculator.FormatLargeNumber(MaxFans);
    }

    /// <summary>
    /// 手动设置粉丝数（用于测试）
    /// </summary>
    public void SetFansForTesting(long fans)
    {
        CurrentFans = fans;
        CurrentFans = System.Math.Max(0, CurrentFans);

        Debug.Log($"[BossHealthSystem] Manually set fans to: {CurrentFans}");

        // 通知UI更新
        BossFansUI.Instance?.UpdateFans(CurrentFans, MaxFans);

        // 检查是否归零
        if (CurrentFans <= 0 && !IsGameOver)
        {
            IsGameOver = true;
            OnGameOver("粉丝数归零（测试）");
        }
    }
}
