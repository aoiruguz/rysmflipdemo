using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 结算界面的导航按钮控制
/// </summary>
public class ResultScreenNavigation : MonoBehaviour
{
    [Header("Buttons")]
    public Button retryButton;
    public Button backToMenuButton;

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
    }

    /// <summary>
    /// 重试当前歌曲
    /// </summary>
    private void OnRetryClicked()
    {
        Debug.Log("[ResultScreenNavigation] Retry clicked");

        // 清理PlayDataCollector，让PlayScene创建新的实例
        PlayDataCollector.Cleanup();

        // 重新加载PlayScene
        SceneManager.LoadScene("PlayScene");
    }

    /// <summary>
    /// 返回歌曲选择界面
    /// </summary>
    private void OnBackToMenuClicked()
    {
        Debug.Log("[ResultScreenNavigation] Back to menu clicked");

        // 清理PlayDataCollector
        PlayDataCollector.Cleanup();

        // 返回歌曲选择界面
        SceneManager.LoadScene("Big Map");
    }

    void Update()
    {
        // ESC键返回菜单
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackToMenuClicked();
        }

        // R键重试
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnRetryClicked();
        }
    }
}
