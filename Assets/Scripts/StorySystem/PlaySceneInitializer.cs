using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// PlayScene 初始化管理器
/// 负责在游戏开始前播放开场剧情
/// </summary>
public class PlaySceneInitializer : MonoBehaviour
{
    [Header("引用")]
    public StoryManager storyManager;
    public NoteManager noteManager;
    public GameObject gameplayUI;

    [Header("设置")]
    [Tooltip("是否暂停游戏时间播放剧情")]
    public bool pauseGameDuringStory = true;

    private ChartData currentChart;
    private LevelData currentLevelData;
    private bool isInitialized = false;

    private void Start()
    {
        // 获取选中的 chart
        currentChart = LevelUIManager.GetSelectedChart();

        if (currentChart == null)
        {
            Debug.LogError("[PlaySceneInitializer] No chart selected!");
            return;
        }

        // 初始化游戏
        InitializeGame();
    }

    private void InitializeGame()
    {
        // 暂时隐藏游戏 UI
        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        // 暂停 NoteManager
        if (noteManager != null)
            noteManager.enabled = false;

        // 尝试获取 LevelData（从 LevelUIManager 的静态变量或其他方式）
        currentLevelData = GetCurrentLevelData();

        // 检查是否已通关
        bool isCleared = CheckIfLevelCleared();

        // 如果有剧情数据，播放剧情
        if (currentLevelData != null && currentLevelData.openingDialogues.Length > 0)
        {
            if (storyManager != null)
            {
                // 暂停游戏时间
                if (pauseGameDuringStory)
                    Time.timeScale = 0f;

                // 播放剧情，已通关则显示跳过按钮
                storyManager.ShowDialogues(
                    currentLevelData.openingDialogues,
                    isCleared, // 已通关显示跳过按钮
                    currentLevelData.dialogDisplayDuration,
                    currentLevelData.dialogTransitionDelay,
                    OnStoryComplete
                );
            }
            else
            {
                Debug.LogWarning("[PlaySceneInitializer] StoryManager not found, skipping story");
                OnStoryComplete();
            }
        }
        else
        {
            // 没有剧情，直接开始游戏
            OnStoryComplete();
        }
    }

    /// <summary>
    /// 获取当前 LevelData（需要从某处传递过来）
    /// </summary>
    private LevelData GetCurrentLevelData()
    {
        // 方法1: 从 LevelUIManager 获取（需要添加静态变量）
        // 方法2: 通过场景参数传递
        // 方法3: 通过 ChartData 反向查找 LevelData

        // 这里暂时返回 null，需要根据你的项目结构实现
        // 建议在 LevelUIManager 中添加一个静态变量来存储当前选中的 LevelData
        return LevelUIManager.GetCurrentLevelData();
    }

    /// <summary>
    /// 检查关卡是否已通关
    /// </summary>
    private bool CheckIfLevelCleared()
    {
        if (currentChart == null || SaveManager.Instance == null || currentLevelData == null)
            return false;

        string levelName = currentLevelData.levelName;
        string songName = currentChart.songName;
        ChartDifficulty difficulty = currentChart.difficulty;

        return SaveManager.Instance.IsLevelCleared(levelName, songName, difficulty);
    }

    /// <summary>
    /// 剧情播放完成回调
    /// </summary>
    private void OnStoryComplete()
    {
        Debug.Log("[PlaySceneInitializer] Story complete, starting game");

        // 恢复游戏时间
        if (pauseGameDuringStory)
            Time.timeScale = 1f;

        // 显示游戏 UI
        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        // 启动 NoteManager
        if (noteManager != null)
            noteManager.enabled = true;

        isInitialized = true;
    }

    /// <summary>
    /// 获取当前关卡信息（供其他脚本使用）
    /// </summary>
    public string GetCurrentLevelName()
    {
        return currentLevelData != null ? currentLevelData.levelName : "Unknown";
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
