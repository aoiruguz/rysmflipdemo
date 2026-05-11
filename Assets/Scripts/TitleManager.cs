using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 标题页面管理器
/// 管理标题页面的所有按钮功能
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("场景配置")]
    [Tooltip("新游戏要跳转的场景名称")]
    public string newGameSceneName = "Big Map";

    [Tooltip("继续游戏要跳转的场景名称")]
    public string continueGameSceneName = "PlayScene";

    [Header("UI面板引用")]
    [Tooltip("设置面板的引用")]
    public GameObject settingsPanel;

    [Tooltip("制作者名单面板的引用")]
    public GameObject creditsPanel;

    private void Start()
    {
        // 确保面板初始状态为隐藏
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 新游戏按钮点击事件
    /// 清除所有存档内容，但保留设置（offset和流速）
    /// </summary>
    public void OnNewGameClicked()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ClearGameProgressKeepSettings();
            Debug.Log("[TitleManager] Starting new game, progress cleared");
        }

        // 跳转到新游戏场景
        if (!string.IsNullOrEmpty(newGameSceneName))
        {
            SceneManager.LoadScene(newGameSceneName);
        }
        else
        {
            Debug.LogWarning("[TitleManager] New game scene name is not set!");
        }
    }

    /// <summary>
    /// 继续游戏按钮点击事件
    /// 检测存档，有存档则加载并跳转，无存档则不响应
    /// </summary>
    public void OnContinueGameClicked()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[TitleManager] SaveManager instance not found!");
            return;
        }

        // 检测是否有游戏进度存档
        if (!SaveManager.Instance.HasGameProgress())
        {
            Debug.Log("[TitleManager] No save data found, continue game ignored");
            return; // 没有存档，不执行任何操作
        }

        // 有存档，跳转到继续游戏场景
        if (!string.IsNullOrEmpty(continueGameSceneName))
        {
            SceneManager.LoadScene(continueGameSceneName);
            Debug.Log("[TitleManager] Continuing game with existing save data");
        }
        else
        {
            Debug.LogWarning("[TitleManager] Continue game scene name is not set!");
        }
    }

    /// <summary>
    /// 设置按钮点击事件
    /// 打开设置面板
    /// </summary>
    public void OnSettingsClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Debug.Log("[TitleManager] Settings panel opened");
        }
        else
        {
            Debug.LogWarning("[TitleManager] Settings panel reference is not set!");
        }
    }

    /// <summary>
    /// 制作者名单按钮点击事件
    /// 打开制作者名单面板
    /// </summary>
    public void OnCreditsClicked()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
            Debug.Log("[TitleManager] Credits panel opened");
        }
        else
        {
            Debug.LogWarning("[TitleManager] Credits panel reference is not set!");
        }
    }

    /// <summary>
    /// 退出按钮点击事件
    /// 退出游戏
    /// </summary>
    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        Debug.Log("[TitleManager] Quit game (Editor mode)");
#else
        Application.Quit();
        Debug.Log("[TitleManager] Quit game");
#endif
    }

    /// <summary>
    /// 关闭设置面板（供设置面板的关闭按钮调用）
    /// </summary>
    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 关闭制作者名单面板（供制作者名单面板的关闭按钮调用）
    /// </summary>
    public void CloseCreditsPanel()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }
}
