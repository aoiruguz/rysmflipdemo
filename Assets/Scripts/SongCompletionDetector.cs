using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 独立的歌曲完成检测器和场景跳转管理器
/// 负责：
/// 1. 检测当前场景的歌曲是否完成（所有 Note 处理完毕）
/// 2. 检测游戏失败（生命值归零）
/// 3. 统一处理所有场景跳转逻辑
/// 每个场景独立运行，不会跨场景继承状态
/// </summary>
public class SongCompletionDetector : MonoBehaviour
{
    public static SongCompletionDetector Instance { get; private set; }

    [Header("Result Panel Settings")]
    [Tooltip("结算界面 Panel")]
    public GameObject resultPanel;

    [Tooltip("显示结算界面时需要关闭的 Panel 列表")]
    public GameObject[] panelsToHideOnResult;

    [Header("Fade Image Controller")]
    [Tooltip("图片淡入淡出控制器")]
    public FadeImageController fadeImageController;

    [Header("Victory Animation")]
    [Tooltip("Animator component to trigger victory animation")]
    public Animator victoryAnimator;

    [Header("角色名字UI（结算阶段显示）")]
    [Tooltip("玩家名字Panel")]
    public GameObject playerNamePanel;
    [Tooltip("敌人名字Panel")]
    public GameObject enemyNamePanel;

    [Header("战斗界面血条UI（结算时隐藏）")]
    [Tooltip("玩家血条Panel")]
    public GameObject playerHealthBar;
    [Tooltip("敌人血条Panel")]
    public GameObject enemyHealthBar;

    [Tooltip("是否启用完成检测（调延迟界面可以禁用此项）")]
    public bool enableCompletionDetection = true;

    [Tooltip("完成后等待多少秒再显示结算界面")]
    public float delayBeforeShowingResult = 2f;

    [Tooltip("游戏失败后等待多少秒再显示结算界面")]
    public float delayBeforeGameOverResult = 2f;

    private NoteManager noteManager;
    private int totalNotes = 0;
    private int processedNotes = 0;
    private bool songCompleted = false;
    private bool isInitialized = false;
    private bool isTransitioning = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 查找当前场景的 NoteManager
        noteManager = FindFirstObjectByType<NoteManager>();

        if (noteManager == null)
        {
            Debug.LogError("[SongCompletionDetector] No NoteManager found in scene!");
            enabled = false;
            return;
        }

        if (!enableCompletionDetection)
        {
            Debug.Log("[SongCompletionDetector] Completion detection disabled for this scene.");
            enabled = false;
            return;
        }

        // 延迟初始化，等待 NoteManager 加载谱面
        StartCoroutine(DelayedInitialize());
    }

    private IEnumerator DelayedInitialize()
    {
        // 等待一帧，确保 NoteManager 已经初始化
        yield return null;

        if (noteManager.currentChart != null)
        {
            totalNotes = noteManager.currentChart.notes.Count;
            processedNotes = 0;
            songCompleted = false;
            isInitialized = true;

            Debug.Log($"[SongCompletionDetector] Initialized. Total notes: {totalNotes}");
        }
        else
        {
            Debug.LogError("[SongCompletionDetector] NoteManager has no chart loaded!");
            enabled = false;
        }
    }

    /// <summary>
    /// 当一个 Note 被处理时调用此方法
    /// 由 NoteManager 调用
    /// </summary>
    public void OnNoteProcessed()
    {
        if (!isInitialized || songCompleted || isTransitioning) return;

        processedNotes++;
        Debug.Log($"[SongCompletionDetector] Note processed: {processedNotes}/{totalNotes}");

        // 检查是否所有 Note 都已处理
        if (processedNotes >= totalNotes)
        {
            songCompleted = true;
            StartCoroutine(OnSongCompleted());
        }
    }

    /// <summary>
    /// 游戏失败时调用（生命值归零）
    /// 由 HealthSystem 调用
    /// </summary>
    public void OnGameOver()
    {
        if (isTransitioning) return;

        Debug.Log("[SongCompletionDetector] Game Over triggered!");
        isTransitioning = true;

        // 收集最终游戏数据
        if (PlayDataCollector.Instance != null)
        {
            PlayDataCollector.Instance.CollectFinalData();
        }

        // 显示 "You Lose" 图片（淡入淡出）
        if (fadeImageController != null)
        {
            fadeImageController.ShowYouLose();
        }

        // 显示 Game Over 文本
        UIManager.Instance?.ShowSongClearText("Game Over");

        // 等待后显示结算界面
        StartCoroutine(ShowResultPanelAfterDelay(delayBeforeGameOverResult));
    }

    /// <summary>
    /// 歌曲完成处理
    /// </summary>
    private IEnumerator OnSongCompleted()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        Debug.Log("[SongCompletionDetector] Song completed!");

        // Set victory animation
        if (victoryAnimator != null)
        {
            victoryAnimator.SetBool("vic", true);
        }

        // 收集最终游戏数据
        if (PlayDataCollector.Instance != null)
        {
            PlayDataCollector.Instance.CollectFinalData();
        }

        // 显示 "You Win" 图片（淡入淡出）
        if (fadeImageController != null)
        {
            fadeImageController.ShowYouWin();
        }

        string resultText = "Song Clear!";

        // 判断完成等级
        PlayData data = PlayDataCollector.Instance?.CurrentPlayData;
        if (data != null)
        {
            if (data.IsAllPerfect())
                resultText = "All Perfect!!";
            else if (data.IsFullCombo())
                resultText = "Full Combo!";
        }

        // 显示完成信息
        UIManager.Instance?.ShowSongClearText(resultText);

        // 等待指定时间后显示结算界面
        yield return new WaitForSeconds(delayBeforeShowingResult);

        // 显示结算界面
        ShowResultPanel();
    }

    /// <summary>
    /// 延迟后显示结算界面
    /// </summary>
    private IEnumerator ShowResultPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowResultPanel();
    }

    /// <summary>
    /// 显示结算界面
    /// </summary>
    private void ShowResultPanel()
    {
        // 停止音乐和Note生成
        NoteManager noteManager = FindFirstObjectByType<NoteManager>();
        if (noteManager != null)
        {
            noteManager.StopGame();
        }

        // 隐藏血条，显示名字Panel（结算阶段）
        if (playerHealthBar != null)
            playerHealthBar.SetActive(false);
        if (enemyHealthBar != null)
            enemyHealthBar.SetActive(false);
        if (playerNamePanel != null)
            playerNamePanel.SetActive(true);
        if (enemyNamePanel != null)
            enemyNamePanel.SetActive(true);

        if (resultPanel != null)
        {
            Debug.Log("[SongCompletionDetector] Showing result panel via Animator");
            
            // 重点：先激活面板，触发 ResultPanelEntryAnimator 的 Awake 从而记录位置并隐藏内容
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
            else
            {
                // 回退逻辑：如果没有动画控制器，依然瞬间隐藏
                HidePanels();
            }
        }
        else
        {
            Debug.LogWarning("[SongCompletionDetector] Result panel not assigned!");
            HidePanels();
        }
    }

    /// <summary>
    /// 关闭指定的 Panel 列表
    /// </summary>
    private void HidePanels()
    {
        if (panelsToHideOnResult == null || panelsToHideOnResult.Length == 0)
        {
            return;
        }

        int hiddenCount = 0;
        foreach (GameObject panel in panelsToHideOnResult)
        {
            if (panel != null && panel.activeSelf)
            {
                panel.SetActive(false);
                hiddenCount++;
                Debug.Log($"[SongCompletionDetector] Hidden panel: {panel.name}");
            }
        }

        if (hiddenCount > 0)
        {
            Debug.Log($"[SongCompletionDetector] Hidden {hiddenCount} panel(s)");
        }
    }

    /// <summary>
    /// 获取当前进度（用于进度条显示）
    /// </summary>
    public float GetProgress()
    {
        if (totalNotes == 0) return 0f;
        return (float)processedNotes / totalNotes;
    }

    /// <summary>
    /// 获取已处理的 Note 数量
    /// </summary>
    public int GetProcessedNotes()
    {
        return processedNotes;
    }

    /// <summary>
    /// 获取总 Note 数量
    /// </summary>
    public int GetTotalNotes()
    {
        return totalNotes;
    }
}
