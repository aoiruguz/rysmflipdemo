using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Boss战粉丝数量UI显示
/// 只显示完整的粉丝数字，不使用简写
/// </summary>
public class BossFansUI : MonoBehaviour
{
    public static BossFansUI Instance { get; private set; }

    [Header("Fans Text Display")]
    [Tooltip("显示当前粉丝数的文本（完整数字）")]
    public TextMeshProUGUI currentFansText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 初始化显示
        if (BossBattleManager.Instance != null && BossBattleManager.Instance.IsBossMode())
        {
            UpdateFans(BossBattleManager.Instance.GetCurrentFans(), BossBattleManager.Instance.GetMaxFans());
        }
    }

    /// <summary>
    /// 更新粉丝数显示
    /// </summary>
    public void UpdateFans(long currentFans, long maxFans)
    {
        // 更新文本显示（完整数字，不简写）
        if (currentFansText != null)
        {
            currentFansText.text = currentFans.ToString("N0"); // N0格式：千位分隔符，无小数
        }

        Debug.Log($"[BossFansUI] Updated. Current: {currentFans}, Max: {maxFans}");
    }
}
