using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Judgment Thresholds (ms)")]
    public float perfectThreshold = 50f;
    public float greatThreshold = 100f;
    public float goodThreshold = 150f;

    [Header("特殊弹幕触发设置")]
    [Tooltip("特殊弹幕触发按钮（把触发SC的按钮拖到这里）")]
    public Button specialBarrageButton;

    [Tooltip("是否启用Combo特殊弹幕触发")]
    public bool enableComboBarrageTrigger = true;

    [Tooltip("每多少Combo触发一次特殊弹幕")]
    public int comboTriggerInterval = 10;

    private int lastTriggeredCombo = 0;

    public int Combo { get; private set; } = 0;

    // Judgment counts
    private int perfectCount = 0;
    private int greatCount = 0;
    private int goodCount = 0;
    private int missCount = 0;
    private int maxCombo = 0;

    // Late/Fast counts (only for Great and Good)
    private int lateCount = 0;
    private int fastCount = 0;

    void Awake()
    {
        Instance = this;
    }

    public void OnCatch(float offsetMs)
    {
        string judgment;
        float absOffset = Mathf.Abs(offsetMs);

        if (absOffset <= perfectThreshold)
        {
            judgment = "PERFECT";
            perfectCount++;
            Combo++;
        }
        else if (absOffset <= greatThreshold)
        {
            judgment = "GREAT";
            greatCount++;
            Combo++;

            // Track Late/Fast for Great
            if (offsetMs > 0)
                lateCount++;
            else
                fastCount++;
        }
        else if (absOffset <= goodThreshold)
        {
            judgment = "GOOD";
            goodCount++;
            Combo++;

            // Track Late/Fast for Good
            if (offsetMs > 0)
                lateCount++;
            else
                fastCount++;
        }
        else
        {
            // This might happen if hit way too early/late but still technically caught
            judgment = "MISS";
            missCount++;
            Combo = 0;
            HealthSystem.Instance?.ResetComboTracking();
        }

        // Track max combo
        if (Combo > maxCombo)
        {
            maxCombo = Combo;
        }

        // Check for combo healing
        if (Combo > 0)
        {
            HealthSystem.Instance?.CheckComboHeal(Combo);
        }

        // Boss战模式：根据判定改变粉丝数
        if (BossBattleManager.Instance != null && BossBattleManager.Instance.IsBossMode())
        {
            BossBattleManager.Instance.OnJudgment(judgment);
        }

        // 检测Combo触发特殊弹幕
        CheckComboBarrageTrigger();

        UIManager.Instance?.ShowJudgment(judgment, offsetMs, Combo);
    }

    /// <summary>
    /// 检测Combo是否达到触发条件
    /// </summary>
    private void CheckComboBarrageTrigger()
    {
        if (!enableComboBarrageTrigger || specialBarrageButton == null) return;

        // 检查是否达到新的触发点
        if (Combo > 0 && Combo % comboTriggerInterval == 0)
        {
            // 避免重复触发同一个Combo值
            if (Combo != lastTriggeredCombo)
            {
                lastTriggeredCombo = Combo;
                // 自动点击按钮
                specialBarrageButton.onClick.Invoke();
            }
        }
    }

    public void OnMiss()
    {
        missCount++;
        Combo = 0;
        lastTriggeredCombo = 0; // 重置触发记录
        HealthSystem.Instance?.TakeDamage();
        HealthSystem.Instance?.ResetComboTracking();

        // Boss战模式：根据判定改变粉丝数
        if (BossBattleManager.Instance != null && BossBattleManager.Instance.IsBossMode())
        {
            BossBattleManager.Instance.OnJudgment("MISS");
        }

        MissEffectManager.Instance?.PlayMissEffects();
        UIManager.Instance?.ShowJudgment("MISS", 0, Combo, false);
    }

    // Data collection methods
    public int GetPerfectCount() => perfectCount;
    public int GetGreatCount() => greatCount;
    public int GetGoodCount() => goodCount;
    public int GetMissCount() => missCount;
    public int GetMaxCombo() => maxCombo;
    public int GetLateCount() => lateCount;
    public int GetFastCount() => fastCount;
    public int GetTotalNotes() => perfectCount + greatCount + goodCount + missCount;

    public void ResetStats()
    {
        perfectCount = 0;
        greatCount = 0;
        goodCount = 0;
        missCount = 0;
        maxCombo = 0;
        Combo = 0;
        lateCount = 0;
        fastCount = 0;
    }
}
