using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public static HealthSystem Instance { get; private set; }

    [Header("Health Settings")]
    [Tooltip("Maximum health (3 hearts = 15 segments, each heart has 5 segments)")]
    public int maxHealth = 15;

    [Tooltip("Damage taken per miss (1/5 of a heart = 1 segment)")]
    public int damagePerMiss = 1;

    [Header("Hurt Animation")]
    [Tooltip("Animator component to trigger hurt animation")]
    public Animator hurtAnimator;

    [Header("Combo Healing Settings")]
    [Tooltip("Combo required to heal on Easy difficulty")]
    public int easyComboThreshold = 10;

    [Tooltip("Combo required to heal on Normal difficulty")]
    public int normalComboThreshold = 20;

    [Tooltip("Combo required to heal on Hard difficulty")]
    public int hardComboThreshold = 30;

    [Tooltip("Amount of health restored when reaching combo threshold")]
    public int healAmount = 1;

    public int CurrentHealth { get; private set; }
    public bool IsGameOver { get; private set; }

    private int lastHealCombo = 0;
    private ChartDifficulty currentDifficulty;

    void Awake()
    {
        Instance = this;
        CurrentHealth = maxHealth;
        IsGameOver = false;
    }

    public void Initialize(ChartDifficulty difficulty)
    {
        currentDifficulty = difficulty;
        CurrentHealth = maxHealth;
        IsGameOver = false;
        lastHealCombo = 0;
        Debug.Log($"[HealthSystem] Initialized with difficulty: {difficulty}");
    }

    public void TakeDamage()
    {
        if (IsGameOver) return;

        CurrentHealth -= damagePerMiss;
        CurrentHealth = Mathf.Max(0, CurrentHealth);

        Debug.Log($"[HealthSystem] Took damage. Current health: {CurrentHealth}/{maxHealth}");

        // Trigger hurt animation
        if (hurtAnimator != null)
        {
            hurtAnimator.SetTrigger("hurt");
        }

        if (CurrentHealth <= 0)
        {
            IsGameOver = true;
            OnGameOver();
        }

        // Notify UI to update
        HealthUI.Instance?.UpdateHealth(CurrentHealth, maxHealth);
    }

    public void CheckComboHeal(int currentCombo)
    {
        if (IsGameOver || CurrentHealth >= maxHealth) return;

        int comboThreshold = GetComboThreshold();

        // Check if we've reached a new healing milestone
        if (currentCombo >= comboThreshold && currentCombo / comboThreshold > lastHealCombo / comboThreshold)
        {
            Heal();
            lastHealCombo = currentCombo;
        }
    }

    private void Heal()
    {
        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);

        Debug.Log($"[HealthSystem] Healed! Current health: {CurrentHealth}/{maxHealth}");

        // Notify UI to update
        HealthUI.Instance?.UpdateHealth(CurrentHealth, maxHealth);
    }

    public void ResetComboTracking()
    {
        lastHealCombo = 0;
    }

    private int GetComboThreshold()
    {
        switch (currentDifficulty)
        {
            case ChartDifficulty.Easy:
                return easyComboThreshold;
            case ChartDifficulty.Normal:
                return normalComboThreshold;
            case ChartDifficulty.Hard:
                return hardComboThreshold;
            default:
                return normalComboThreshold;
        }
    }

    private void OnGameOver()
    {
        Debug.Log("[HealthSystem] Game Over!");

        // Set die animation
        if (hurtAnimator != null)
        {
            hurtAnimator.SetBool("die", true);
        }

        // 通知 SongCompletionDetector 处理场景跳转
        if (SongCompletionDetector.Instance != null)
        {
            SongCompletionDetector.Instance.OnGameOver();
        }
        else
        {
            Debug.LogWarning("[HealthSystem] No SongCompletionDetector found! Cannot transition to Game Over scene.");
        }
    }

    public float GetHealthPercentage()
    {
        return (float)CurrentHealth / maxHealth;
    }
}
