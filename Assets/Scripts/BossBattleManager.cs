using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using TMPro;

/// <summary>
/// Boss战后演出配置
/// </summary>
[System.Serializable]
public class BossVictoryPerformance
{
    [Tooltip("说话者名字")]
    public string speakerName;

    [Tooltip("说话内容")]
    [TextArea(2, 4)]
    public string dialogueText;

    [Tooltip("对话显示时长（秒）")]
    public float displayDuration = 3f;
}

/// <summary>
/// Boss战管理器（整合所有Boss战功能）
/// 只有当 LevelData.chapterGroup == 4 时才启用
/// </summary>
public class BossBattleManager : MonoBehaviour
{
    public static BossBattleManager Instance { get; private set; }

    #region Configuration Fields

    [Header("=== Fans Health Settings ===")]
    [Tooltip("指数倍数（基础倍数）")]
    public long multiplier = 1000;

    [Tooltip("Perfect判定：增加粉丝")]
    public int perfectFansChange = 100;

    [Tooltip("Great判定：减少粉丝")]
    public int greatFansChange = -300;

    [Tooltip("Good判定：减少粉丝")]
    public int goodFansChange = -600;

    [Tooltip("Miss判定：减少粉丝")]
    public int missFansChange = -600;

    [Tooltip("测试模式：手动设置粉丝数")]
    public bool enableTestMode = false;

    [Tooltip("测试用粉丝数（启用测试模式时使用）")]
    public long testFansAmount = 1000000;

    [Header("=== UI Panel Management ===")]
    [Tooltip("原有的播放量显示面板（Boss战时关闭）")]
    public GameObject originalPlayCountPanel;

    [Tooltip("Boss战专用粉丝数显示面板（Boss战时开启）")]
    public GameObject bossFansPanel;

    [Tooltip("Boss战粉丝数显示文本（完整数字，千位分隔符）")]
    public TextMeshProUGUI bossFansText;

    [Header("=== System References ===")]
    [Tooltip("玩家生命系统引用（自动查找）")]
    public HealthSystem playerHealthSystem;

    [Header("=== Panel Management ===")]
    [Tooltip("Boss战期间需要关闭的面板（全程关闭）")]
    public GameObject[] panelsToHideDuringBoss;

    [Tooltip("结算时需要关闭的面板")]
    public GameObject[] panelsToHideOnResult;

    [Header("=== Result Screen ===")]
    [Tooltip("Boss战结算面板")]
    public GameObject resultPanel;

    [Header("Failure Buttons (F Grade)")]
    [Tooltip("失败按钮容器（引用即可，按钮功能保持原有逻辑）")]
    public GameObject failureButtonsContainer;

    [Header("Victory Flow (Non-F Grade)")]
    [Tooltip("胜利按钮容器（非F评级时显示）")]
    public GameObject victoryButtonsContainer;

    [Tooltip("胜利后需要移出摄像机的面板列表")]
    public GameObject[] panelsToMoveOffScreen;

    [Tooltip("面板移出的目标位置（屏幕外）")]
    public Vector3 offScreenPosition = new Vector3(0, 3000, 0);

    [Tooltip("面板移动动画时间")]
    public float panelMoveTime = 0.5f;

    [Header("=== Victory Story Sequence ===")]
    [Tooltip("胜利后播放的剧情ID列表（按顺序播放）")]
    public string[] victoryStoryIds;

    [Header("=== Victory Performance (Win按钮触发) ===")]
    [Tooltip("胜利后演出配置列表（面板飞出后，Boss说的台词）")]
    public BossVictoryPerformance[] victoryPerformances;

    [Header("Ending Choice")]
    [Tooltip("结局选择面板（剧情播放完后显示）")]
    public GameObject endingChoicePanel;

    [Tooltip("结局A按钮（跳转逻辑你自己配置）")]
    public Button endingAButton;

    [Tooltip("结局B按钮（跳转逻辑你自己配置）")]
    public Button endingBButton;

    [Header("=== Boss Dialogue System ===")]
    [Tooltip("Boss战专用对话面板（索引3，由StoryManager的panel3控制）")]
    public GameObject bossDialoguePanel;

    [Header("=== Song Completion Detection ===")]
    [Tooltip("完成后等待多少秒再显示结算界面")]
    public float delayBeforeShowingResult = 2f;

    [Tooltip("游戏失败后等待多少秒再显示结算界面")]
    public float delayBeforeGameOverResult = 2f;

    [Header("=== Scene Names ===")]
    public string bossPlaySceneName = "BossPlayScene";
    public string mapSceneName = "Big Map";

    #endregion

    #region Private State

    private bool isBossMode = false;
    private long maxFans;
    private long currentFans;
    private bool isGameOver = false;
    private int totalNotes = 0;
    private int processedNotes = 0;
    private bool songCompleted = false;
    private bool isTransitioning = false;

    #endregion

    #region Lifecycle

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 检测是否为Boss战
        LevelData levelData = LevelUIManager.GetCurrentLevelData();
        if (levelData == null || levelData.chapterGroup != 4)
        {
            // 不是Boss战，确保所有Boss专用UI隐藏
            isBossMode = false;
            HideBossUI();
            Debug.Log("[BossBattleManager] Not Boss mode, all Boss features disabled.");
            return;
        }

        // 是Boss战，初始化
        isBossMode = true;
        Debug.Log("[BossBattleManager] Boss mode detected! Initializing...");
        InitializeBossMode();
    }

    void Update()
    {
        if (!isBossMode || isGameOver) return;

        // 检查玩家生命值
        if (playerHealthSystem != null && playerHealthSystem.IsGameOver)
        {
            isGameOver = true;
            OnGameOver();
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 初始化Boss战模式
    /// </summary>
    private void InitializeBossMode()
    {
        // 自动查找HealthSystem
        if (playerHealthSystem == null)
        {
            playerHealthSystem = FindFirstObjectByType<HealthSystem>();
        }

        // 设置UI
        SetupBossUI();

        // 关闭Boss战期间不需要的面板
        HidePanels(panelsToHideDuringBoss);

        // 初始化粉丝生命值
        InitializeFansHealth();

        // 初始化歌曲检测
        InitializeSongDetection();

        // 设置Boss对话面板
        SetupBossDialoguePanel();

        Debug.Log("[BossBattleManager] Boss mode initialized successfully.");
    }

    /// <summary>
    /// 设置Boss战UI
    /// </summary>
    private void SetupBossUI()
    {
        // 关闭原有播放量面板
        if (originalPlayCountPanel != null)
        {
            originalPlayCountPanel.SetActive(false);
            Debug.Log("[BossBattleManager] Original play count panel hidden.");
        }

        // 开启Boss粉丝数面板
        if (bossFansPanel != null)
        {
            bossFansPanel.SetActive(true);
            Debug.Log("[BossBattleManager] Boss fans panel activated.");
        }

        // 确保结局选择面板初始隐藏
        if (endingChoicePanel != null)
        {
            endingChoicePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 隐藏所有Boss专用UI（非Boss战时调用）
    /// </summary>
    private void HideBossUI()
    {
        if (bossFansPanel != null)
        {
            bossFansPanel.SetActive(false);
        }

        if (bossDialoguePanel != null)
        {
            bossDialoguePanel.SetActive(false);
        }

        if (endingChoicePanel != null)
        {
            endingChoicePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 初始化粉丝生命值
    /// </summary>
    private void InitializeFansHealth()
    {
        // 测试模式：使用手动设置的粉丝数
        if (enableTestMode)
        {
            maxFans = testFansAmount;
            currentFans = testFansAmount;
            Debug.Log($"[BossBattleManager] Test Mode Enabled. Using test fans: {testFansAmount}");
        }
        else
        {
            // 从SaveManager读取累计粉丝数作为最大值
            maxFans = SaveManager.Instance.GetTotalFans();
            currentFans = maxFans;
        }

        isGameOver = false;

        Debug.Log($"[BossBattleManager] Initialized. Max Fans: {maxFans}, Current Fans: {currentFans}");

        // 通知UI更新
        UpdateFansUI();
    }

    /// <summary>
    /// 初始化歌曲检测
    /// </summary>
    private void InitializeSongDetection()
    {
        NoteManager noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager == null)
        {
            Debug.LogError("[BossBattleManager] No NoteManager found in scene!");
            return;
        }

        if (noteManager.currentChart != null)
        {
            totalNotes = noteManager.currentChart.notes.Count;
            processedNotes = 0;
            songCompleted = false;

            Debug.Log($"[BossBattleManager] Song detection initialized. Total notes: {totalNotes}");
        }
        else
        {
            Debug.LogError("[BossBattleManager] NoteManager has no chart loaded!");
        }
    }

    /// <summary>
    /// 设置Boss对话面板（索引2）
    /// </summary>
    private void SetupBossDialoguePanel()
    {
        if (bossDialoguePanel != null)
        {
            bossDialoguePanel.SetActive(true);
            Debug.Log("[BossBattleManager] Boss dialogue panel (index 2) activated.");
        }
    }

    #endregion

    #region Public Interfaces

    /// <summary>
    /// 判定事件接口（由ScoreManager调用）
    /// </summary>
    public void OnJudgment(string judgment)
    {
        if (!isBossMode || isGameOver) return;

        long fansChange = 0;

        switch (judgment.ToUpper())
        {
            case "PERFECT":
                fansChange = perfectFansChange * multiplier;
                break;
            case "GREAT":
                fansChange = greatFansChange * multiplier;
                break;
            case "GOOD":
                fansChange = goodFansChange * multiplier;
                break;
            case "MISS":
                fansChange = missFansChange * multiplier;
                break;
            default:
                Debug.LogWarning($"[BossBattleManager] Unknown judgment: {judgment}");
                return;
        }

        // 应用粉丝变化
        currentFans += fansChange;
        currentFans = System.Math.Max(0, currentFans);

        Debug.Log($"[BossBattleManager] Judgment: {judgment}, Fans Change: {fansChange:+#;-#;0}, Current Fans: {currentFans}/{maxFans}");

        // 通知UI更新
        UpdateFansUI();

        // 检查粉丝数是否归零
        if (currentFans <= 0)
        {
            isGameOver = true;
            OnGameOver();
        }
    }

    /// <summary>
    /// Note处理完成接口（由NoteManager调用）
    /// </summary>
    public void OnNoteProcessed()
    {
        if (!isBossMode || songCompleted || isTransitioning) return;

        processedNotes++;
        Debug.Log($"[BossBattleManager] Note processed: {processedNotes}/{totalNotes}");

        // 检查是否所有Note都已处理
        if (processedNotes >= totalNotes)
        {
            songCompleted = true;
            StartCoroutine(OnSongCompleted());
        }
    }

    /// <summary>
    /// 获取当前粉丝数
    /// </summary>
    public long GetCurrentFans()
    {
        return currentFans;
    }

    /// <summary>
    /// 获取最大粉丝数
    /// </summary>
    public long GetMaxFans()
    {
        return maxFans;
    }

    /// <summary>
    /// 获取粉丝数百分比
    /// </summary>
    public float GetFansPercentage()
    {
        if (maxFans == 0) return 0f;
        return (float)currentFans / maxFans;
    }

    /// <summary>
    /// 格式化当前粉丝数显示
    /// </summary>
    public string GetFormattedCurrentFans()
    {
        return ScoreCalculator.FormatLargeNumber(currentFans);
    }

    /// <summary>
    /// 格式化最大粉丝数显示
    /// </summary>
    public string GetFormattedMaxFans()
    {
        return ScoreCalculator.FormatLargeNumber(maxFans);
    }

    /// <summary>
    /// 是否为Boss战模式
    /// </summary>
    public bool IsBossMode()
    {
        return isBossMode;
    }

    /// <summary>
    /// 是否游戏结束
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOver;
    }

    #endregion

    #region Game Logic

    /// <summary>
    /// 歌曲完成处理
    /// </summary>
    private IEnumerator OnSongCompleted()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        Debug.Log("[BossBattleManager] Boss battle completed!");

        // 收集最终游戏数据（重要：确保数据被正确存储）
        if (PlayDataCollector.Instance != null)
        {
            PlayDataCollector.Instance.CollectFinalData();
        }

        // 等待指定时间
        yield return new WaitForSeconds(delayBeforeShowingResult);

        // 判断是否为失败评级
        if (IsFailureGrade())
        {
            ShowFailureResult();
        }
        else
        {
            ShowVictoryResult();
        }
    }

    /// <summary>
    /// 游戏失败处理
    /// </summary>
    private void OnGameOver()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        Debug.Log("[BossBattleManager] Game Over!");

        // 收集最终游戏数据（重要：确保数据被正确存储）
        if (PlayDataCollector.Instance != null)
        {
            PlayDataCollector.Instance.CollectFinalData();
        }

        // 停止音乐和Note生成
        NoteManager noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager != null)
        {
            noteManager.StopGame();
        }

        // 等待后显示结算界面
        StartCoroutine(ShowGameOverResult());
    }

    /// <summary>
    /// 显示游戏失败结算
    /// </summary>
    private IEnumerator ShowGameOverResult()
    {
        yield return new WaitForSeconds(delayBeforeGameOverResult);
        ShowFailureResult();
    }

    #endregion

    #region Result Logic

    /// <summary>
    /// 判断是否为失败评级
    /// </summary>
    private bool IsFailureGrade()
    {
        // F评级条件：粉丝数归零 或 玩家生命值归零
        if (currentFans <= 0) return true;
        if (playerHealthSystem != null && playerHealthSystem.IsGameOver) return true;

        return false;
    }

    /// <summary>
    /// 显示失败结算
    /// </summary>
    private void ShowFailureResult()
    {
        Debug.Log("[BossBattleManager] Showing failure result.");

        // 停止音乐和Note生成
        NoteManager noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager != null)
        {
            noteManager.StopGame();
        }

        // 关闭结算时需要关闭的面板
        HidePanels(panelsToHideOnResult);

        // 显示结算面板
        if (resultPanel != null)
        {
            // 先激活面板，触发 ResultPanelEntryAnimator 的 Awake 从而记录位置并隐藏内容
            resultPanel.SetActive(true);

            // 通知 ResultScreenUI 更新显示
            ResultScreenUI resultUI = resultPanel.GetComponent<ResultScreenUI>();
            if (resultUI != null)
            {
                resultUI.DisplayResultsFromPlayData(PlayDataCollector.Instance?.CurrentPlayData);
            }

            // 获取动画控制器并执行【退场 -> 禁用 -> 进场】流水线
            ResultPanelEntryAnimator entryAnimator = resultPanel.GetComponent<ResultPanelEntryAnimator>();
            if (entryAnimator != null)
            {
                entryAnimator.PlayExitThenEntryAnimation();
            }
        }

        // 显示失败按钮容器
        if (failureButtonsContainer != null)
        {
            failureButtonsContainer.SetActive(true);
        }

        // 隐藏胜利按钮容器
        if (victoryButtonsContainer != null)
        {
            victoryButtonsContainer.SetActive(false);
        }

        // 隐藏结局选择面板
        if (endingChoicePanel != null)
        {
            endingChoicePanel.SetActive(false);
        }

        Debug.Log("[BossBattleManager] Failure result displayed.");
    }

    /// <summary>
    /// 显示胜利结算
    /// </summary>
    private void ShowVictoryResult()
    {
        Debug.Log("[BossBattleManager] Showing victory result.");

        // 停止音乐和Note生成
        NoteManager noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager != null)
        {
            noteManager.StopGame();
        }

        // 关闭结算时需要关闭的面板
        HidePanels(panelsToHideOnResult);

        // 显示结算面板
        if (resultPanel != null)
        {
            // 先激活面板，触发 ResultPanelEntryAnimator 的 Awake 从而记录位置并隐藏内容
            resultPanel.SetActive(true);

            // 通知 ResultScreenUI 更新显示
            ResultScreenUI resultUI = resultPanel.GetComponent<ResultScreenUI>();
            if (resultUI != null)
            {
                resultUI.DisplayResultsFromPlayData(PlayDataCollector.Instance?.CurrentPlayData);
            }

            // 获取动画控制器并执行【退场 -> 禁用 -> 进场】流水线
            ResultPanelEntryAnimator entryAnimator = resultPanel.GetComponent<ResultPanelEntryAnimator>();
            if (entryAnimator != null)
            {
                entryAnimator.PlayExitThenEntryAnimation();
            }
        }

        // 隐藏失败按钮容器
        if (failureButtonsContainer != null)
        {
            failureButtonsContainer.SetActive(false);
        }

        // 显示胜利按钮容器
        if (victoryButtonsContainer != null)
        {
            victoryButtonsContainer.SetActive(true);
        }

        // 注意：不自动开始胜利流程，等待玩家点击Win按钮
        Debug.Log("[BossBattleManager] Victory result displayed. Waiting for player to click Win button.");
    }

    /// <summary>
    /// Win按钮调用此方法（公开接口）
    /// 触发面板飞出和后续演出
    /// </summary>
    public void OnWinButtonClicked()
    {
        Debug.Log("[BossBattleManager] Win button clicked! Starting victory sequence.");
        StartCoroutine(VictorySequence());
    }

    /// <summary>
    /// 胜利流程：移动面板 → 执行演出配置 → 播放剧情 → 显示结局选择
    /// </summary>
    private IEnumerator VictorySequence()
    {
        Debug.Log("[BossBattleManager] Starting victory sequence.");

        // 1. 移动面板出屏幕
        MovePanelsOffScreen();

        // 等待面板移动完成
        yield return new WaitForSeconds(panelMoveTime);

        // 2. 执行胜利演出配置（Boss台词，使用 panel 3）
        if (victoryPerformances != null && victoryPerformances.Length > 0)
        {
            for (int i = 0; i < victoryPerformances.Length; i++)
            {
                var performance = victoryPerformances[i];
                Debug.Log($"[BossBattleManager] Playing dialogue {i + 1}/{victoryPerformances.Length}: {performance.speakerName}");

                // 显示对话（使用 panel 3，Boss战专用）
                if (StoryManager.Instance != null)
                {
                    StoryManager.Instance.ShowDialogue(3, performance.speakerName, performance.dialogueText, performance.displayDuration);
                }

                // 如果是最后一句台词，只等待3秒
                if (i == victoryPerformances.Length - 1)
                {
                    Debug.Log("[BossBattleManager] Last dialogue displayed, waiting 3 seconds before continuing.");
                    yield return new WaitForSeconds(3f);
                }
                else
                {
                    // 前面的台词等待完整的 displayDuration
                    yield return new WaitForSeconds(performance.displayDuration);
                }
            }
        }

        // 3. 按顺序播放剧情
        if (victoryStoryIds != null && victoryStoryIds.Length > 0)
        {
            for (int i = 0; i < victoryStoryIds.Length; i++)
            {
                string storyId = victoryStoryIds[i];
                Debug.Log($"[BossBattleManager] Playing victory story {i + 1}/{victoryStoryIds.Length}: {storyId}");

                bool storyCompleted = false;

                // 播放剧情（不保存到存档）
                if (AVGStoryManager.Instance != null)
                {
                    AVGStoryManager.Instance.PlayStoryWithoutSaving(storyId, () =>
                    {
                        storyCompleted = true;
                    });

                    // 等待剧情播放完成
                    yield return new WaitUntil(() => storyCompleted);
                }
                else
                {
                    Debug.LogWarning("[BossBattleManager] AVGStoryManager not found! Skipping story.");
                }
            }
        }

        // 4. 显示结局选择面板
        if (endingChoicePanel != null)
        {
            endingChoicePanel.SetActive(true);
            Debug.Log("[BossBattleManager] Ending choice panel displayed.");
        }

        Debug.Log("[BossBattleManager] Victory sequence completed.");
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// 隐藏面板列表
    /// </summary>
    private void HidePanels(GameObject[] panels)
    {
        if (panels == null || panels.Length == 0)
        {
            Debug.LogWarning("[BossBattleManager] No panels to hide (array is null or empty).");
            return;
        }

        int hiddenCount = 0;
        foreach (GameObject panel in panels)
        {
            if (panel != null && panel.activeSelf)
            {
                panel.SetActive(false);
                hiddenCount++;
                Debug.Log($"[BossBattleManager] Hidden panel: {panel.name}");
            }
        }

        if (hiddenCount > 0)
        {
            Debug.Log($"[BossBattleManager] Hidden {hiddenCount} panel(s).");
        }
        else
        {
            Debug.LogWarning("[BossBattleManager] No panels were hidden (all panels were already inactive or null).");
        }
    }

    /// <summary>
    /// 移动面板出屏幕
    /// </summary>
    private void MovePanelsOffScreen()
    {
        if (panelsToMoveOffScreen == null || panelsToMoveOffScreen.Length == 0)
        {
            Debug.LogWarning("[BossBattleManager] No panels to move off screen.");
            return;
        }

        foreach (GameObject panel in panelsToMoveOffScreen)
        {
            if (panel != null)
            {
                RectTransform rectTransform = panel.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.DOAnchorPos(offScreenPosition, panelMoveTime)
                        .SetEase(Ease.InOutQuad);
                    Debug.Log($"[BossBattleManager] Moving panel off screen: {panel.name}");
                }
                else
                {
                    Debug.LogWarning($"[BossBattleManager] Panel {panel.name} has no RectTransform!");
                }
            }
        }
    }

    /// <summary>
    /// 更新粉丝数UI
    /// </summary>
    private void UpdateFansUI()
    {
        // 直接更新文本显示（完整数字，千位分隔符）
        if (bossFansText != null)
        {
            bossFansText.text = currentFans.ToString("N0"); // N0格式：千位分隔符，无小数
        }
    }

    #endregion
}
