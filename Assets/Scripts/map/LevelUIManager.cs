using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelUIManager : MonoBehaviour
{
    public static LevelUIManager Instance;

    [Header("UI 引用组件")]
    public GameObject detailsPanel;

    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI songNameText;
    public TextMeshProUGUI composerText;
    public TextMeshProUGUI bpmText;

    public Image enemyImage;
    public TextMeshProUGUI enemyDescriptionText;
    public TextMeshProUGUI enemyTitleText;

    [Header("难度评级显示 (三个难度)")]
    public TextMeshProUGUI easyRatingText;
    public TextMeshProUGUI normalRatingText;
    public TextMeshProUGUI hardRatingText;

    [Header("难度选择按钮")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;

    [Header("当前选中难度信息")]
    public TextMeshProUGUI mapperText;
    public TextMeshProUGUI bestScoreText;

    [Header("解锁/开始按钮")]
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    [Header("关闭按钮")]
    public Button closeButton;

    [Header("场景设置")]
    public string gameSceneName = "PlayScene";

    [Header("音频设置")]
    public AudioSource bgmAudioSource;      // 背景音乐播放器
    [HideInInspector]
    public AudioSource previewAudioSource;  // 预览音乐播放器（供外部访问）
    private float originalBgmVolume = 1f;   // 原始背景音乐音量

    private LevelData currentLevelData;
    private ChartDifficulty selectedDifficulty = ChartDifficulty.Normal;
    private static ChartData selectedChart;
    private static LevelData selectedLevelData; // 新增：存储选中的 LevelData

    private void Awake()
    {
        Instance = this;
        if (detailsPanel != null) detailsPanel.SetActive(false);

        // 设置按钮监听
        if (easyButton != null)
            easyButton.onClick.AddListener(() => OnDifficultySelected(ChartDifficulty.Easy));
        if (normalButton != null)
            normalButton.onClick.AddListener(() => OnDifficultySelected(ChartDifficulty.Normal));
        if (hardButton != null)
            hardButton.onClick.AddListener(() => OnDifficultySelected(ChartDifficulty.Hard));
        if (actionButton != null)
            actionButton.onClick.AddListener(OnActionButtonClicked);
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        // 初始化音频
        InitializeAudio();
    }

    private void InitializeAudio()
    {
        // 如果没有指定背景音乐播放器，尝试查找场景中的
        if (bgmAudioSource == null)
        {
            GameObject bgmObj = GameObject.Find("BGM");
            if (bgmObj != null)
            {
                bgmAudioSource = bgmObj.GetComponent<AudioSource>();
            }
        }

        // 记录原始背景音乐音量
        if (bgmAudioSource != null)
        {
            originalBgmVolume = bgmAudioSource.volume;
        }

        // 创建预览音乐播放器
        if (previewAudioSource == null)
        {
            GameObject previewObj = new GameObject("LevelPreviewAudio");
            previewObj.transform.SetParent(transform);
            previewAudioSource = previewObj.AddComponent<AudioSource>();
            previewAudioSource.loop = true;
            previewAudioSource.playOnAwake = false;
            previewAudioSource.volume = GameSettings.MusicVolume;
        }
    }

    public void ShowLevelDetails(LevelData data)
    {
        if (data == null) return;

        currentLevelData = data;
        detailsPanel.SetActive(true);

        levelNameText.text = data.levelName;
        if (data.difficulties.Length > 0 && data.difficulties[0].chartAsset != null)
        {
            songNameText.text = data.difficulties[0].chartAsset.songName;
        }

        composerText.text = "Artist: " + data.composer;
        bpmText.text = "BPM: " + data.displayBPM.ToString();

        // 显示敌人头像（根据击败状态）
        enemyImage.sprite = data.GetCurrentEnemyAvatar();
        enemyDescriptionText.text = data.enemyInfo;

        // 显示敌人称号（根据击败状态）
        if (enemyTitleText != null)
        {
            enemyTitleText.text = data.GetCurrentEnemyTitle();
        }

        // 显示三个难度的 rating
        UpdateAllDifficultyRatings(data);

        // 默认选择 Normal 难度
        selectedDifficulty = ChartDifficulty.Normal;
        RefreshDifficultyUI(data, selectedDifficulty);
        UpdateActionButton();

        // 播放预览音乐并降低背景音乐音量
        PlayPreviewMusic(data.previewMusic);
    }

    // 更新三个难度的 rating 显示
    private void UpdateAllDifficultyRatings(LevelData data)
    {
        foreach (var detail in data.difficulties)
        {
            switch (detail.difficulty)
            {
                case ChartDifficulty.Easy:
                    if (easyRatingText != null)
                        easyRatingText.text = "LV." + detail.ratingLevel;
                    break;
                case ChartDifficulty.Normal:
                    if (normalRatingText != null)
                        normalRatingText.text = "LV." + detail.ratingLevel;
                    break;
                case ChartDifficulty.Hard:
                    if (hardRatingText != null)
                        hardRatingText.text = "LV." + detail.ratingLevel;
                    break;
            }
        }
    }

    // 玩家选择难度
    private void OnDifficultySelected(ChartDifficulty difficulty)
    {
        selectedDifficulty = difficulty;
        RefreshDifficultyUI(currentLevelData, difficulty);
        UpdateActionButton();
    }

    public void RefreshDifficultyUI(LevelData data, ChartDifficulty diff)
    {
        foreach (var detail in data.difficulties)
        {
            if (detail.difficulty == diff)
            {
                mapperText.text = "Chart: " + detail.mapper;

                // 从 SaveManager 获取最高分
                int highScore = 0;
                string rank = "";
                if (SaveManager.Instance != null)
                {
                    highScore = SaveManager.Instance.GetLevelHighScore(data.levelName, detail.chartAsset.songName, diff);
                    rank = SaveManager.Instance.GetLevelRank(data.levelName, detail.chartAsset.songName, diff);
                }

                if (highScore > 0)
                {
                    bestScoreText.text = $"Best: {highScore} ({rank})";
                }
                else
                {
                    bestScoreText.text = "Best: ---";
                }
                break;
            }
        }
    }

    // 更新解锁/开始按钮
    private void UpdateActionButton()
    {
        if (currentLevelData == null || actionButton == null || actionButtonText == null)
            return;

        // 获取当前粉丝量（这里需要从你的游戏数据系统获取）
        int currentFans = GetCurrentFans();

        bool isUnlocked = currentLevelData.IsUnlocked(currentFans);

        if (isUnlocked)
        {
            // 已解锁，显示 "Easy GO!" / "Normal GO!" / "Hard GO!"
            actionButton.interactable = true;
            switch (selectedDifficulty)
            {
                case ChartDifficulty.Easy:
                    actionButtonText.text = "Easy GO!";
                    break;
                case ChartDifficulty.Normal:
                    actionButtonText.text = "Normal GO!";
                    break;
                case ChartDifficulty.Hard:
                    actionButtonText.text = "Hard GO!";
                    break;
            }
        }
        else
        {
            // 未解锁，显示解锁条件
            actionButton.interactable = false;
            actionButtonText.text = currentLevelData.GetUnlockConditionText();
        }
    }

    // 点击开始按钮
    private void OnActionButtonClicked()
    {
        if (currentLevelData == null)
            return;

        int currentFans = GetCurrentFans();
        if (!currentLevelData.IsUnlocked(currentFans))
        {
            Debug.LogWarning("关卡未解锁！");
            return;
        }

        // 找到对应难度的 ChartData
        ChartData chartToLoad = null;
        foreach (var detail in currentLevelData.difficulties)
        {
            if (detail.difficulty == selectedDifficulty)
            {
                chartToLoad = detail.chartAsset;
                break;
            }
        }

        if (chartToLoad == null)
        {
            Debug.LogError($"未找到 {selectedDifficulty} 难度的 ChartData！");
            return;
        }

        // 传递 chart 和 LevelData 并启动异步加载
        selectedChart = chartToLoad;
        selectedLevelData = currentLevelData;
        Debug.Log($"[LevelUI] 加载关卡: {currentLevelData.levelName}, 难度: {selectedDifficulty}");
        StartCoroutine(LoadSceneAsync());
    }

    // 异步加载场景
    private System.Collections.IEnumerator LoadSceneAsync()
    {
        // 开始异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            // 这里可以获取加载进度: asyncLoad.progress (0-0.9)
            // 转场特效可以在这里添加
            yield return null;
        }
    }

    // 获取当前粉丝量
    private int GetCurrentFans()
    {
        if (SaveManager.Instance != null)
        {
            return (int)SaveManager.Instance.GetTotalFans();
        }
        return (int)FansDataManager.GetTotalFans();
    }

    // 供其他场景获取选中的 chart
    public static ChartData GetSelectedChart()
    {
        return selectedChart;
    }

    // 供其他场景获取选中的 LevelData
    public static LevelData GetCurrentLevelData()
    {
        return selectedLevelData;
    }

    public void ClosePanel()
    {
        detailsPanel.SetActive(false);

        // 停止预览音乐并恢复背景音乐音量
        StopPreviewMusic();
    }

    // 播放预览音乐
    private void PlayPreviewMusic(AudioClip previewClip)
    {
        if (previewAudioSource == null) return;

        // 停止之前的预览音乐
        previewAudioSource.Stop();

        // 降低背景音乐音量
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.volume = 0f;
        }

        // 播放新的预览音乐
        if (previewClip != null)
        {
            previewAudioSource.clip = previewClip;
            previewAudioSource.Play();
            Debug.Log($"[LevelUI] 播放预览音乐: {previewClip.name}");
        }
    }

    // 停止预览音乐
    private void StopPreviewMusic()
    {
        if (previewAudioSource != null && previewAudioSource.isPlaying)
        {
            previewAudioSource.Stop();
            Debug.Log("[LevelUI] 停止预览音乐");
        }

        // 恢复背景音乐音量
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = originalBgmVolume;
        }
    }
}
