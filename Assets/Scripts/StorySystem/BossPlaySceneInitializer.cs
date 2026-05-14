using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Boss战场景初始化管理器
/// 负责：
/// 1. 播放Boss开场剧情
/// 2. 初始化Boss战系统（双生命值）
/// 3. 隐藏/显示特定UI元素
/// </summary>
public class BossPlaySceneInitializer : MonoBehaviour
{
    [Header("引用")]
    public StoryManager storyManager;
    public NoteManager noteManager;
    public GameObject gameplayUI;
    public BossHealthSystem bossHealthSystem;
    public BossFansUI bossFansUI;

    [Header("需要隐藏的UI")]
    [Tooltip("刷礼物窗口")]
    public GameObject giftWindow;

    [Tooltip("播放量窗口")]
    public GameObject playCountWindow;

    [Tooltip("右上角头像")]
    public GameObject avatarIcon;

    [Header("设置")]
    [Tooltip("是否暂停游戏时间播放剧情")]
    public bool pauseGameDuringStory = true;

    private ChartData currentChart;
    private LevelData currentLevelData;

    private void Start()
    {
        // 获取选中的 chart
        currentChart = LevelUIManager.GetSelectedChart();

        if (currentChart == null)
        {
            Debug.LogError("[BossPlaySceneInitializer] No chart selected!");
            OnStoryComplete();
            return;
        }

        // 初始化Boss战
        InitializeBossGame();
    }

    private void InitializeBossGame()
    {
        // 暂时隐藏游戏 UI
        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        // 暂停 NoteManager
        if (noteManager != null)
            noteManager.enabled = false;

        // 隐藏不需要的UI元素
        HideUnnecessaryUI();

        // 显示Boss战特有UI
        ShowBossUI();

        // 初始化Boss生命系统
        if (bossHealthSystem != null)
        {
            // BossHealthSystem会在Start中自动初始化
            Debug.Log("[BossPlaySceneInitializer] BossHealthSystem initialized");
        }
        else
        {
            Debug.LogWarning("[BossPlaySceneInitializer] BossHealthSystem not assigned!");
        }

        // 尝试获取 LevelData
        currentLevelData = GetCurrentLevelData();

        // 如果有剧情数据，播放剧情
        if (currentLevelData != null && currentLevelData.openingDialogues.Length > 0)
        {
            if (storyManager != null)
            {
                // 暂停游戏时间
                if (pauseGameDuringStory)
                    Time.timeScale = 0f;

                // 播放剧情（Boss战始终显示跳过按钮）
                storyManager.ShowDialogues(
                    currentLevelData.openingDialogues,
                    true, // Boss战始终显示跳过按钮
                    currentLevelData.dialogDisplayDuration,
                    currentLevelData.dialogTransitionDelay,
                    OnStoryComplete
                );
            }
            else
            {
                Debug.LogWarning("[BossPlaySceneInitializer] StoryManager not found, skipping story");
                OnStoryComplete();
            }
        }
        else
        {
            // 没有剧情，直接开始游戏
            Debug.Log("[BossPlaySceneInitializer] No opening dialogue, starting game directly");
            OnStoryComplete();
        }
    }

    /// <summary>
    /// 隐藏不需要的UI元素
    /// </summary>
    private void HideUnnecessaryUI()
    {
        if (giftWindow != null)
        {
            giftWindow.SetActive(false);
            Debug.Log("[BossPlaySceneInitializer] Hidden gift window");
        }

        if (playCountWindow != null)
        {
            playCountWindow.SetActive(false);
            Debug.Log("[BossPlaySceneInitializer] Hidden play count window");
        }

        if (avatarIcon != null)
        {
            avatarIcon.SetActive(false);
            Debug.Log("[BossPlaySceneInitializer] Hidden avatar icon");
        }
    }

    /// <summary>
    /// 显示Boss战特有UI
    /// </summary>
    private void ShowBossUI()
    {
        if (bossFansUI != null)
        {
            bossFansUI.gameObject.SetActive(true);
            Debug.Log("[BossPlaySceneInitializer] Shown Boss Fans UI");
        }
        else
        {
            Debug.LogWarning("[BossPlaySceneInitializer] BossFansUI not assigned!");
        }
    }

    /// <summary>
    /// 获取当前 LevelData
    /// </summary>
    private LevelData GetCurrentLevelData()
    {
        return LevelUIManager.GetCurrentLevelData();
    }

    /// <summary>
    /// 剧情播放完成回调
    /// </summary>
    private void OnStoryComplete()
    {
        Debug.Log("[BossPlaySceneInitializer] Story complete, starting Boss battle");

        // 恢复游戏时间
        if (pauseGameDuringStory)
            Time.timeScale = 1f;

        // 显示游戏 UI
        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        // 启动 NoteManager
        if (noteManager != null)
            noteManager.enabled = true;
    }

    /// <summary>
    /// 获取当前关卡信息（供其他脚本使用）
    /// </summary>
    public string GetCurrentLevelName()
    {
        return currentLevelData != null ? currentLevelData.levelName : "Boss Battle";
    }

    public string GetCurrentSongName()
    {
        return currentChart != null ? currentChart.songName : "Unknown";
    }

    public ChartDifficulty GetCurrentDifficulty()
    {
        return currentChart != null ? currentChart.difficulty : ChartDifficulty.Normal;
    }
}
