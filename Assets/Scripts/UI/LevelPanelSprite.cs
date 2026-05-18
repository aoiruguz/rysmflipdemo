using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// 基于 Sprite 的关卡面板组件
/// 使用 SpriteRenderer 和 Collider 实现交互
/// </summary>
public class LevelPanelSprite : MonoBehaviour
{
    [Header("Sprite 引用")]
    public SpriteRenderer backgroundSprite;
    public SpriteRenderer enemyIconSprite;
    public SpriteRenderer buttonSprite;
    public SpriteRenderer lockIconSprite;
    public SpriteRenderer hoverHighlightSprite;

    [Header("文本引用 (TextMeshPro 3D)")]
    public TextMeshPro enemyNameText;
    public TextMeshPro fansCountText;
    public TextMeshPro buttonText;

    [Header("交互组件")]
    public BoxCollider2D buttonCollider;

    [Header("数据")]
    public LevelData levelData;

    [Header("视觉效果")]
    public Color unlockedColor = new Color(1f, 1f, 1f, 1f);
    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    public Color buttonNormalColor = new Color(0.2f, 0.5f, 0.8f, 1f);
    public Color buttonHoverColor = new Color(0.3f, 0.6f, 0.9f, 1f);
    public Color buttonDisabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public float hoverFadeDuration = 0.15f;

    private bool isUnlocked = false;
    private bool isHovering = false;

    private void Start()
    {
        // 自动初始化
        if (levelData != null)
        {
            long currentFans = SaveManager.Instance != null ? SaveManager.Instance.GetTotalFans() : 0;
            Initialize(levelData, currentFans);
        }

        // 默认状态下强制不透明度为 0
        if (hoverHighlightSprite != null)
        {
            Color c = hoverHighlightSprite.color;
            c.a = 0f;
            hoverHighlightSprite.color = c;
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
            Debug.LogError("[LevelPanelSprite] LevelData is null!");
            return;
        }

        // 设置大地图小图标（根据击败状态）
        if (enemyIconSprite != null)
        {
            enemyIconSprite.sprite = levelData.GetCurrentMapIcon();
        }

        // 设置敌人名字
        if (enemyNameText != null)
        {
            enemyNameText.text = levelData.enemyName;
        }

        // 设置粉丝量
        if (fansCountText != null)
        {
            fansCountText.text = levelData.enemyFans.ToString();
        }

        // 使用统一的解锁判断
        isUnlocked = levelData.IsUnlocked();

        // 设置按钮状态
        UpdateVisualState();
    }

    /// <summary>
    /// 更新视觉状态
    /// </summary>
    private void UpdateVisualState()
    {
        if (isUnlocked)
        {
            // 解锁状态
            if (buttonText != null)
                buttonText.text = " ";

            if (lockIconSprite != null)
                lockIconSprite.gameObject.SetActive(false);

            // 恢复颜色
            if (enemyIconSprite != null)
                enemyIconSprite.color = unlockedColor;
            if (enemyNameText != null)
                enemyNameText.color = unlockedColor;

            // 按钮颜色
            if (buttonSprite != null)
                buttonSprite.color = isHovering ? buttonHoverColor : buttonNormalColor;
        }
        else
        {
            // 锁定状态
            if (buttonText != null)
                buttonText.text = "未解锁";

            if (lockIconSprite != null)
                lockIconSprite.gameObject.SetActive(true);

            // 变灰
            if (enemyIconSprite != null)
                enemyIconSprite.color = lockedColor;
            if (enemyNameText != null)
                enemyNameText.color = lockedColor;

            // 按钮禁用颜色
            if (buttonSprite != null)
                buttonSprite.color = buttonDisabledColor;
        }
    }

    /// <summary>
    /// 鼠标按下事件（需要在按钮对象上添加此脚本或使用子对象）
    /// </summary>
    private void OnMouseDown()
    {
        // 允许未解锁的关卡也能打开面板查看信息
        if (levelData == null)
            return;

        Debug.Log($"[LevelPanelSprite] Open level details: {levelData.levelName} (Unlocked: {isUnlocked})");

        // 触发关卡选择逻辑（无论是否解锁都可以打开）
        if (LevelUIManager.Instance != null)
        {
            LevelUIManager.Instance.ShowLevelDetails(levelData);
        }
    }

    /// <summary>
    /// 鼠标进入（悬停效果）
    /// </summary>
    private void OnMouseEnter()
    {
        // 允许未解锁的关卡也显示悬停效果
        isHovering = true;

        if (buttonSprite != null)
        {
            buttonSprite.color = isUnlocked ? buttonHoverColor : buttonDisabledColor;
        }

        // 悬浮高亮 Sprite 快速过渡到 100% 不透明度
        if (hoverHighlightSprite != null)
        {
            hoverHighlightSprite.DOKill();
            hoverHighlightSprite.DOFade(1f, hoverFadeDuration);
        }
    }

    /// <summary>
    /// 鼠标离开
    /// </summary>
    private void OnMouseExit()
    {
        isHovering = false;
        if (buttonSprite != null && isUnlocked)
            buttonSprite.color = buttonNormalColor;

        // 悬浮高亮 Sprite 快速过渡到 0% 不透明度
        if (hoverHighlightSprite != null)
        {
            hoverHighlightSprite.DOKill();
            hoverHighlightSprite.DOFade(0f, hoverFadeDuration);
        }
    }

    /// <summary>
    /// 刷新面板状态（当玩家粉丝数变化时调用）
    /// </summary>
    public void Refresh(long currentFans)
    {
        if (levelData == null)
            return;

        // 使用统一的解锁判断
        isUnlocked = levelData.IsUnlocked();
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
