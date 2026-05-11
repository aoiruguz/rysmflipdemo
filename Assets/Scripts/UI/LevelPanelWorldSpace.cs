using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// World Space 关卡面板组件
/// 显示敌人信息和挑战按钮
/// </summary>
public class LevelPanelWorldSpace : MonoBehaviour
{
    [Header("UI 引用")]
    public Image enemyIcon;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI fansCountText;
    public Button challengeButton;
    public TextMeshProUGUI buttonText;
    public Image lockIcon;

    [Header("数据")]
    public LevelData levelData;

    [Header("视觉效果")]
    public Color unlockedColor = new Color(1f, 1f, 1f, 1f);
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    private bool isUnlocked = false;

    private void Start()
    {
        // 自动初始化
        if (levelData != null)
        {
            long currentFans = SaveManager.Instance != null ? SaveManager.Instance.GetTotalFans() : 0;
            Initialize(levelData, currentFans);
        }
    }

    /// <summary>
    /// 初始化面板数据
    /// </summary>
    public void Initialize(LevelData data, long currentFans)
    {
        levelData = data;

        if (levelData == null)
        {
            Debug.LogError("[LevelPanelWorldSpace] LevelData is null!");
            return;
        }

        // 设置敌人头像
        if (enemyIcon != null && levelData.enemyAvatar != null)
        {
            enemyIcon.sprite = levelData.enemyAvatar;
        }

        // 设置敌人名字
        if (enemyNameText != null)
        {
            enemyNameText.text = levelData.levelName;
        }

        // 设置粉丝量（敌人的战斗力）
        if (fansCountText != null)
        {
            fansCountText.text = $"粉丝: {levelData.enemyFans}";
        }

        // 检查是否解锁（使用关卡组解锁逻辑）
        isUnlocked = levelData.IsChapterUnlocked(currentFans);

        // 设置按钮状态
        UpdateVisualState();

        // 绑定按钮点击事件
        if (challengeButton != null)
        {
            challengeButton.onClick.RemoveAllListeners();
            challengeButton.onClick.AddListener(OnChallengeButtonClicked);
        }
    }

    /// <summary>
    /// 更新视觉状态
    /// </summary>
    private void UpdateVisualState()
    {
        if (challengeButton == null || buttonText == null)
            return;

        if (isUnlocked)
        {
            // 解锁状态
            challengeButton.interactable = true;
            buttonText.text = "挑战";

            if (lockIcon != null)
                lockIcon.gameObject.SetActive(false);

            // 恢复颜色
            if (enemyIcon != null)
                enemyIcon.color = unlockedColor;
            if (enemyNameText != null)
                enemyNameText.color = unlockedColor;
        }
        else
        {
            // 锁定状态
            challengeButton.interactable = false;
            buttonText.text = "未解锁";

            if (lockIcon != null)
                lockIcon.gameObject.SetActive(true);

            // 变灰
            if (enemyIcon != null)
                enemyIcon.color = lockedColor;
            if (enemyNameText != null)
                enemyNameText.color = lockedColor;
        }
    }

    /// <summary>
    /// 挑战按钮点击事件
    /// </summary>
    private void OnChallengeButtonClicked()
    {
        if (!isUnlocked || levelData == null)
            return;

        Debug.Log($"[LevelPanelWorldSpace] Challenge level: {levelData.levelName}");

        // 触发关卡选择逻辑 - 打开难度选择界面
        if (LevelUIManager.Instance != null)
        {
            LevelUIManager.Instance.ShowLevelDetails(levelData);
        }
    }

    /// <summary>
    /// 刷新面板状态（当玩家粉丝数变化时调用）
    /// </summary>
    public void Refresh(long currentFans)
    {
        if (levelData == null)
            return;

        isUnlocked = levelData.IsChapterUnlocked(currentFans);
        UpdateVisualState();
    }

    /// <summary>
    /// 设置世界坐标位置
    /// </summary>
    public void SetWorldPosition(Vector3 position)
    {
        transform.position = position;
    }
}
