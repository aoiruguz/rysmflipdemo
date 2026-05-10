using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Judgment Thresholds (ms)")]
    public float perfectThreshold = 50f;
    public float greatThreshold = 100f;
    public float goodThreshold = 150f;

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

        UIManager.Instance?.ShowJudgment(judgment, offsetMs, Combo);
    }

    public void OnMiss()
    {
        missCount++;
        Combo = 0;
        HealthSystem.Instance?.TakeDamage();
        HealthSystem.Instance?.ResetComboTracking();
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
