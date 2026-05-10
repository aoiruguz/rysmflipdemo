using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 敌人血量系统
/// 支持多颗心，每颗心有多个节点
/// 根据歌曲进度自动扣除敌人的血量
/// </summary>
public class EnemyHealthSystem : MonoBehaviour
{
    [Header("Heart UI References")]
    [Tooltip("所有心的Image组件（从左到右）")]
    public List<Image> hearts = new List<Image>();

    [Header("Health Settings")]
    [Tooltip("每颗心的节点数")]
    public int segmentsPerHeart = 5;

    [Tooltip("总血量节点数（会根据心的数量自动计算）")]
    [SerializeField]
    private int totalSegments = 15;

    [Header("Damage Settings")]
    [Tooltip("根据歌曲进度自动扣血")]
    public bool damageByProgress = true;

    [Tooltip("每个血量节点对应的进度阈值（0-1）\n例如：[0.1, 0.2, 0.35, 0.5, ...]表示在10%, 20%, 35%, 50%进度时扣血")]
    public float[] damageThresholds = new float[15]
    {
        0.067f, 0.133f, 0.2f, 0.267f, 0.333f,      // 前5个节点
        0.4f, 0.467f, 0.533f, 0.6f, 0.667f,        // 中间5个节点
        0.733f, 0.8f, 0.867f, 0.933f, 1.0f         // 后5个节点
    };

    [Header("Animation Settings")]
    [Tooltip("心变化时的动画时长")]
    public float heartChangeDuration = 0.3f;

    [Tooltip("心变化时是否播放缩放动画")]
    public bool playScaleAnimation = true;

    private SongCompletionDetector detector;
    private int currentHealth; // 当前血量节点数
    private int currentDamageIndex = 0; // 当前已触发的伤害阈值索引

    void Start()
    {
        // 查找 SongCompletionDetector
        detector = FindFirstObjectByType<SongCompletionDetector>();

        if (detector == null && damageByProgress)
        {
            Debug.LogWarning("[EnemyHealthSystem] No SongCompletionDetector found in scene!");
            damageByProgress = false;
        }

        // 计算总血量
        totalSegments = hearts.Count * segmentsPerHeart;
        currentHealth = totalSegments;

        // 验证配置
        if (damageThresholds.Length != totalSegments)
        {
            Debug.LogWarning($"[EnemyHealthSystem] damageThresholds数组长度({damageThresholds.Length})与总血量节点数({totalSegments})不匹配！");
        }

        // 验证阈值是否递增
        for (int i = 1; i < damageThresholds.Length; i++)
        {
            if (damageThresholds[i] <= damageThresholds[i - 1])
            {
                Debug.LogWarning($"[EnemyHealthSystem] damageThresholds[{i}]({damageThresholds[i]})应该大于damageThresholds[{i - 1}]({damageThresholds[i - 1]})");
            }
        }

        // 初始化所有心为满血状态
        UpdateAllHearts();

        Debug.Log($"[EnemyHealthSystem] Initialized with {hearts.Count} hearts, {totalSegments} total segments");
    }

    void Update()
    {
        if (!damageByProgress || detector == null) return;

        // 获取当前进度
        float progress = detector.GetProgress();

        // 检查是否达到下一个伤害阈值
        if (currentDamageIndex < damageThresholds.Length && progress >= damageThresholds[currentDamageIndex])
        {
            // 扣除1节点血量
            TakeDamage(1);
            currentDamageIndex++;

            Debug.Log($"[EnemyHealthSystem] Damage threshold {currentDamageIndex} reached at progress {progress:F2}");
        }
    }

    /// <summary>
    /// 扣除指定数量的血量节点
    /// </summary>
    public void TakeDamage(int segments)
    {
        if (segments <= 0) return;

        currentHealth -= segments;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"[EnemyHealthSystem] Took {segments} damage. Current health: {currentHealth}/{totalSegments}");

        UpdateAllHearts();
    }

    /// <summary>
    /// 更新所有心的显示
    /// </summary>
    private void UpdateAllHearts()
    {
        int remainingHealth = currentHealth;

        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i] == null) continue;

            // 计算这颗心应该显示的节点数
            int heartSegments = Mathf.Clamp(remainingHealth, 0, segmentsPerHeart);
            remainingHealth -= segmentsPerHeart;

            // 更新心的填充比例
            UpdateHeartFill(hearts[i], heartSegments, i);
        }
    }

    /// <summary>
    /// 更新单个心的填充比例
    /// </summary>
    private void UpdateHeartFill(Image heart, int segments, int heartIndex)
    {
        float targetFill = (float)segments / segmentsPerHeart;

        if (heart.fillAmount != targetFill)
        {
            heart.fillAmount = targetFill;

            // 播放动画
            if (playScaleAnimation)
            {
                StartCoroutine(HeartChangeAnimation(heart));
            }
        }
    }

    /// <summary>
    /// 心变化时的动画效果
    /// </summary>
    private System.Collections.IEnumerator HeartChangeAnimation(Image heart)
    {
        float elapsed = 0f;
        Vector3 originalScale = heart.transform.localScale;

        // 缩放动画：放大 -> 恢复
        while (elapsed < heartChangeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / heartChangeDuration;

            // 先放大到1.2倍，再恢复到1倍
            float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
            heart.transform.localScale = originalScale * scale;

            yield return null;
        }

        // 恢复原始大小
        heart.transform.localScale = originalScale;
    }

    /// <summary>
    /// 获取当前血量节点数
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 获取总血量节点数
    /// </summary>
    public int GetTotalHealth()
    {
        return totalSegments;
    }

    /// <summary>
    /// 获取血量百分比
    /// </summary>
    public float GetHealthPercentage()
    {
        if (totalSegments == 0) return 0f;
        return (float)currentHealth / totalSegments;
    }

    /// <summary>
    /// 重置血量为满血状态
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = totalSegments;
        currentDamageIndex = 0;
        UpdateAllHearts();
        Debug.Log("[EnemyHealthSystem] Health reset to full");
    }

    /// <summary>
    /// 设置血量到指定节点数
    /// </summary>
    public void SetHealth(int segments)
    {
        currentHealth = Mathf.Clamp(segments, 0, totalSegments);
        UpdateAllHearts();
    }

    /// <summary>
    /// 自动生成均匀分布的伤害阈值（用于快速配置）
    /// </summary>
    [ContextMenu("Generate Even Damage Thresholds")]
    public void GenerateEvenDamageThresholds()
    {
        int total = hearts.Count * segmentsPerHeart;
        damageThresholds = new float[total];

        for (int i = 0; i < total; i++)
        {
            damageThresholds[i] = (i + 1) / (float)total;
        }

        Debug.Log($"[EnemyHealthSystem] Generated {total} even damage thresholds");
    }
}
