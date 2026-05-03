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

        // 不清理PlayDataCollector，保持选择的谱面
        // SongSelectionManager.GetSelectedChart() 仍然会返回之前选择的谱面

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
        if (PlayDataCollector.Instance != null)
        {
            Destroy(PlayDataCollector.Instance.gameObject);
        }

        // 返回歌曲选择界面
        SceneManager.LoadScene("SongSelection");
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
