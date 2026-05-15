using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

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
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyTitleText;
    public TextMeshProUGUI enemyFansText;

    [Header("难度评级显示 (三个难度)")]
    public TextMeshProUGUI easyRatingText;
    public TextMeshProUGUI normalRatingText;
    public TextMeshProUGUI hardRatingText;

    [Header("难度等级图标素材 (1-10)")]
    public Sprite[] difficultyLevelSprites = new Sprite[10];

    [Header("难度等级图标显示")]
    public Image easyLevelIcon;
    public Image normalLevelIcon;
    public Image hardLevelIcon;

    [Header("难度选择按钮")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;

    [Header("当前选中难度信息")]
    public TextMeshProUGUI mapperText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI bestComboText;
    public TextMeshProUGUI bestViewsText;

    [Header("评级图标显示")]
    public Image rankIcon;
    [Tooltip("按顺序存放5个评级素材: S, A, B, C, F")]
    public Sprite[] rankSprites = new Sprite[5];
    [Tooltip("未游玩或没有记录时的默认评级素材")]
    public Sprite unplayedRankSprite;

    [Header("解锁/开始按钮")]
    public Button actionButton;
    public TextMeshProUGUI actionButtonText;

    [Header("解锁条件显示")]
    [Tooltip("已解锁时激活的物体（包含开始按钮）")]
    public GameObject unlockedPanel;
    [Tooltip("未解锁时激活的物体（包含解锁条件文本）")]
    public GameObject lockedPanel;
    [Tooltip("未解锁时显示的解锁条件文本")]
    public TextMeshProUGUI unlockConditionText;

    [Header("关闭按钮")]
    public Button closeButton;

    [Header("场景设置")]
    public string gameSceneName = "PlayScene";

    [Header("打开面板时隐藏的对象")]
    public List<GameObject> objectsToHideOnOpen = new List<GameObject>();
    private Dictionary<GameObject, bool> originalActiveStates = new Dictionary<GameObject, bool>();

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

        // 隐藏指定的对象
        HideConfiguredObjects();

        if (levelNameText != null)
            levelNameText.text = data.levelName;

        if (songNameText != null)
            songNameText.text = data.levelName;

        if (composerText != null)
            composerText.text = "Artist: " + data.composer;
        if (bpmText != null)
            bpmText.text = data.displayBPM.ToString();

        // 显示敌人头像（根据击败状态）
        if (enemyImage != null)
            enemyImage.sprite = data.GetCurrentEnemyAvatar();

        // 显示敌人名称
        if (enemyNameText != null)
            enemyNameText.text = data.enemyName;

        // 显示敌人称号（根据击败状态）
        if (enemyTitleText != null)
        {
            enemyTitleText.text = data.GetCurrentEnemyTitle();
        }

        // 显示敌人粉丝数
        if (enemyFansText != null)
            enemyFansText.text = data.enemyFans.ToString();

        // 显示三个难度的 rating
        UpdateAllDifficultyRatings(data);

        // 选择默认难度：优先选择玩家上次打过的难度，否则选择 Normal
        selectedDifficulty = GetLastPlayedDifficulty(data);
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
                    // 设置难度等级图标
                    if (easyLevelIcon != null && detail.ratingLevel >= 1 && detail.ratingLevel <= 10)
                        easyLevelIcon.sprite = difficultyLevelSprites[detail.ratingLevel - 1];
                    break;
                case ChartDifficulty.Normal:
                    if (normalRatingText != null)
                        normalRatingText.text = "LV." + detail.ratingLevel;
                    // 设置难度等级图标
                    if (normalLevelIcon != null && detail.ratingLevel >= 1 && detail.ratingLevel <= 10)
                        normalLevelIcon.sprite = difficultyLevelSprites[detail.ratingLevel - 1];
                    break;
                case ChartDifficulty.Hard:
                    if (hardRatingText != null)
                        hardRatingText.text = "LV." + detail.ratingLevel;
                    // 设置难度等级图标
                    if (hardLevelIcon != null && detail.ratingLevel >= 1 && detail.ratingLevel <= 10)
                        hardLevelIcon.sprite = difficultyLevelSprites[detail.ratingLevel - 1];
                    break;
            }
        }
    }

    // 获取玩家上次打过的难度
    private ChartDifficulty GetLastPlayedDifficulty(LevelData data)
    {
        if (SaveManager.Instance == null || data.difficulties.Length == 0)
            return ChartDifficulty.Normal;

        ChartDifficulty lastPlayed = ChartDifficulty.Normal;
        string lastPlayedTime = "";

        // 遍历所有难度，找到最近游玩的
        foreach (var detail in data.difficulties)
        {
            if (detail.chartAsset != null)
            {
                var progress = SaveManager.Instance.GetLevelProgress(data.levelName, detail.chartAsset.songName, detail.difficulty);
                if (progress != null && !string.IsNullOrEmpty(progress.lastPlayedTime))
                {
                    // 比较时间，找到最近的
                    if (string.IsNullOrEmpty(lastPlayedTime) || string.Compare(progress.lastPlayedTime, lastPlayedTime) > 0)
                    {
                        lastPlayedTime = progress.lastPlayedTime;
                        lastPlayed = detail.difficulty;
                    }
                }
            }
        }

        // 如果没有游玩记录，默认返回 Normal
        return string.IsNullOrEmpty(lastPlayedTime) ? ChartDifficulty.Normal : lastPlayed;
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
                if (mapperText != null)
                    mapperText.text = "Chart: " + detail.mapper;

                // 从 SaveManager 获取最佳记录
                LevelProgressData progress = null;
                if (SaveManager.Instance != null && detail.chartAsset != null)
                {
                    progress = SaveManager.Instance.GetLevelProgress(data.levelName, detail.chartAsset.songName, diff);
                }

                if (progress != null && progress.highScore > 0)
                {
                    // 显示最高分
                    if (bestScoreText != null)
                        bestScoreText.text = $"Best: {progress.highScore}";

                    // 显示评级图标
                    if (rankIcon != null)
                    {
                        rankIcon.gameObject.SetActive(true);
                        rankIcon.sprite = GetRankSprite(progress.rank);
                    }

                    // 显示最佳连击
                    if (bestComboText != null)
                        bestComboText.text = progress.bestMaxCombo.ToString();

                    // 显示播放量 (Views) 采用最高分数值
                    if (bestViewsText != null)
                    {
                        bestViewsText.text = progress.highScore.ToString();
                    }
                }
                else
                {
                    // 没有记录
                    if (bestScoreText != null)
                        bestScoreText.text = "Best: ---";
                        
                    if (rankIcon != null)
                    {
                        rankIcon.gameObject.SetActive(true);
                        rankIcon.sprite = unplayedRankSprite;
                    }

                    if (bestComboText != null)
                        bestComboText.text = "---";
                    if (bestViewsText != null)
                        bestViewsText.text = "---";
                }
                break;
            }
        }
    }

    // 更新解锁/开始按钮
    private void UpdateActionButton()
    {
        if (currentLevelData == null)
            return;

        // 使用统一的解锁判断
        bool isUnlocked = currentLevelData.IsUnlocked();

        if (isUnlocked)
        {
            // 已解锁：激活 unlockedPanel，停用 lockedPanel
            if (unlockedPanel != null)
                unlockedPanel.SetActive(true);

            if (lockedPanel != null)
                lockedPanel.SetActive(false);

            // 更新按钮文本
            if (actionButton != null)
                actionButton.interactable = true;

            if (actionButtonText != null)
            {
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
        }
        else
        {
            // 未解锁：停用 unlockedPanel，激活 lockedPanel
            if (unlockedPanel != null)
                unlockedPanel.SetActive(false);

            if (lockedPanel != null)
                lockedPanel.SetActive(true);

            // 更新解锁条件文本（使用 LevelData 的统一方法）
            if (unlockConditionText != null)
            {
                unlockConditionText.text = currentLevelData.GetUnlockConditionText();
            }
        }
    }

    // 点击开始按钮
    private void OnActionButtonClicked()
    {
        if (currentLevelData == null)
            return;

        // 使用统一的解锁判断
        if (!currentLevelData.IsUnlocked())
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

    // 获取评级对应的图标
    private Sprite GetRankSprite(string rank)
    {
        if (rankSprites == null || rankSprites.Length < 5 || string.IsNullOrEmpty(rank)) 
            return unplayedRankSprite;

        switch (rank.ToUpper())
        {
            case "S": return rankSprites[0];
            case "A": return rankSprites[1];
            case "B": return rankSprites[2];
            case "C": return rankSprites[3];
            case "F": return rankSprites[4];
            default: return unplayedRankSprite;
        }
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

        // 恢复隐藏的对象
        ShowConfiguredObjects();

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

    // 隐藏配置的对象
    private void HideConfiguredObjects()
    {
        originalActiveStates.Clear();

        foreach (var obj in objectsToHideOnOpen)
        {
            if (obj != null)
            {
                // 记录原始激活状态
                originalActiveStates[obj] = obj.activeSelf;
                // 隐藏对象
                obj.SetActive(false);
            }
        }
    }

    // 恢复配置的对象
    private void ShowConfiguredObjects()
    {
        foreach (var kvp in originalActiveStates)
        {
            if (kvp.Key != null)
            {
                // 恢复到原始激活状态
                kvp.Key.SetActive(kvp.Value);
            }
        }

        originalActiveStates.Clear();
    }
}
