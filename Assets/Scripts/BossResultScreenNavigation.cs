using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Boss战结算界面的导航按钮控制
/// 根据胜利/失败显示不同的按钮
/// </summary>
public class BossResultScreenNavigation : MonoBehaviour
{
    [Header("失败时的按钮")]
    public Button retryButton;
    public Button backToMenuButton;

    [Header("胜利时的按钮")]
    public Button playAnimationButton;
    public GameObject endingChoicePanel;
    public Button endingAButton;
    public Button endingBButton;

    [Header("按钮容器")]
    [Tooltip("失败按钮容器（包含重试和返回按钮）")]
    public GameObject failureButtonsContainer;

    [Tooltip("胜利按钮容器（包含播放动画按钮）")]
    public GameObject victoryButtonsContainer;

    private bool isGameOver = false;

    void Start()
    {
        // Setup button listeners
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }

        if (playAnimationButton != null)
        {
            playAnimationButton.onClick.AddListener(OnPlayAnimationClicked);
        }

        if (endingAButton != null)
        {
            endingAButton.onClick.AddListener(OnEndingAClicked);
        }

        if (endingBButton != null)
        {
            endingBButton.onClick.AddListener(OnEndingBClicked);
        }

        // 初始隐藏结局选择面板
        if (endingChoicePanel != null)
        {
            endingChoicePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 根据游戏结果设置按钮显示
    /// 由 BossSongCompletionDetector 调用
    /// </summary>
    public void SetupButtons(bool isGameOverResult)
    {
        isGameOver = isGameOverResult;

        if (isGameOver)
        {
            // 失败：显示重试和返回按钮
            ShowFailureButtons();
        }
        else
        {
            // 胜利：显示播放动画按钮
            ShowVictoryButtons();
        }
    }

    /// <summary>
    /// 显示失败按钮
    /// </summary>
    private void ShowFailureButtons()
    {
        if (failureButtonsContainer != null)
        {
            failureButtonsContainer.SetActive(true);
        }

        if (victoryButtonsContainer != null)
        {
            victoryButtonsContainer.SetActive(false);
        }

        Debug.Log("[BossResultScreenNavigation] Showing failure buttons");
    }

    /// <summary>
    /// 显示胜利按钮
    /// </summary>
    private void ShowVictoryButtons()
    {
        if (failureButtonsContainer != null)
        {
            failureButtonsContainer.SetActive(false);
        }

        if (victoryButtonsContainer != null)
        {
            victoryButtonsContainer.SetActive(true);
        }

        Debug.Log("[BossResultScreenNavigation] Showing victory buttons");
    }

    /// <summary>
    /// 重试当前Boss战
    /// </summary>
    private void OnRetryClicked()
    {
        Debug.Log("[BossResultScreenNavigation] Retry clicked");

        // 清理BossPlayDataCollector
        BossPlayDataCollector.Cleanup();

        // 重新加载BossPlayScene
        SceneManager.LoadScene("BossPlayScene");
    }

    /// <summary>
    /// 返回大地图
    /// </summary>
    private void OnBackToMenuClicked()
    {
        Debug.Log("[BossResultScreenNavigation] Back to menu clicked");

        // 清理BossPlayDataCollector
        BossPlayDataCollector.Cleanup();

        // 返回大地图
        SceneManager.LoadScene("Big Map");
    }

    /// <summary>
    /// 播放动画按钮点击（暂时只显示结局选择）
    /// </summary>
    private void OnPlayAnimationClicked()
    {
        Debug.Log("[BossResultScreenNavigation] Play animation clicked");

        // TODO: 播放动画（暂时跳过）

        // 显示结局选择面板
        if (endingChoicePanel != null)
        {
            endingChoicePanel.SetActive(true);
        }

        // 隐藏播放动画按钮
        if (playAnimationButton != null)
        {
            playAnimationButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 选择结局A
    /// </summary>
    private void OnEndingAClicked()
    {
        Debug.Log("[BossResultScreenNavigation] Ending A selected");

        // 清理BossPlayDataCollector
        BossPlayDataCollector.Cleanup();

        // 跳转到结局A场景
        SceneManager.LoadScene("EndingA");
    }

    /// <summary>
    /// 选择结局B
    /// </summary>
    private void OnEndingBClicked()
    {
        Debug.Log("[BossResultScreenNavigation] Ending B selected");

        // 清理BossPlayDataCollector
        BossPlayDataCollector.Cleanup();

        // 跳转到结局B场景
        SceneManager.LoadScene("EndingB");
    }

    void Update()
    {
        // ESC键返回菜单（仅失败时）
        if (Input.GetKeyDown(KeyCode.Escape) && isGameOver)
        {
            OnBackToMenuClicked();
        }

        // R键重试（仅失败时）
        if (Input.GetKeyDown(KeyCode.R) && isGameOver)
        {
            OnRetryClicked();
        }
    }
}
