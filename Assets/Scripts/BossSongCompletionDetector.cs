using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Boss战歌曲完成检测器和场景跳转管理器
/// 负责：
/// 1. 检测Boss战歌曲是否完成
/// 2. 检测游戏失败（生命值或粉丝数归零）
/// 3. 处理Boss战特有的结算逻辑（不转化粉丝）
/// </summary>
public class BossSongCompletionDetector : MonoBehaviour
{
    public static BossSongCompletionDetector Instance { get; private set; }

    [Header("Result Panel Settings")]
    [Tooltip("结算界面 Panel")]
    public GameObject resultPanel;

    [Tooltip("显示结算界面时需要关闭的 Panel 列表")]
    public GameObject[] panelsToHideOnResult;

    [Tooltip("是否启用完成检测")]
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
    private bool isGameOverTriggered = false;

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
            Debug.LogError("[BossSongCompletionDetector] No NoteManager found in scene!");
            enabled = false;
            return;
        }

        if (!enableCompletionDetection)
        {
            Debug.Log("[BossSongCompletionDetector] Completion detection disabled for this scene.");
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

            Debug.Log($"[BossSongCompletionDetector] Initialized. Total notes: {totalNotes}");
        }
        else
        {
            Debug.LogError("[BossSongCompletionDetector] NoteManager has no chart loaded!");
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
        Debug.Log($"[BossSongCompletionDetector] Note processed: {processedNotes}/{totalNotes}");

        // 检查是否所有 Note 都已处理
        if (processedNotes >= totalNotes)
        {
            songCompleted = true;
            StartCoroutine(OnSongCompleted());
        }
    }

    /// <summary>
    /// 游戏失败时调用（生命值或粉丝数归零）
    /// 由 BossHealthSystem 调用
    /// </summary>
    public void OnGameOver()
    {
        if (isTransitioning || isGameOverTriggered) return;

        Debug.Log("[BossSongCompletionDetector] Game Over triggered!");
        isTransitioning = true;
        isGameOverTriggered = true;

        // 收集最终游戏数据（但不转化粉丝）
        if (BossPlayDataCollector.Instance != null)
        {
            BossPlayDataCollector.Instance.CollectFinalData();
        }

        // 显示 Game Over 文本
        UIManager.Instance?.ShowSongClearText("Game Over");

        // 等待后显示结算界面
        StartCoroutine(ShowResultPanelAfterDelay(delayBeforeGameOverResult));
    }

    /// <summary>
    /// 歌曲完成处理（Boss战胜利）
    /// </summary>
    private IEnumerator OnSongCompleted()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        Debug.Log("[BossSongCompletionDetector] Boss battle completed!");

        // 收集最终游戏数据（但不转化粉丝）
        if (BossPlayDataCollector.Instance != null)
        {
            BossPlayDataCollector.Instance.CollectFinalData();
        }

        string resultText = "Boss Defeated!";

        // 判断完成等级
        PlayData data = BossPlayDataCollector.Instance?.CurrentPlayData;
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

        // 关闭指定的 Panel 列表
        HidePanels();

        if (resultPanel != null)
        {
            Debug.Log("[BossSongCompletionDetector] Showing result panel");
            resultPanel.SetActive(true);

            // 通知 ResultScreenUI 更新显示
            ResultScreenUI resultUI = resultPanel.GetComponent<ResultScreenUI>();
            if (resultUI != null)
            {
                resultUI.DisplayResultsFromPlayData(BossPlayDataCollector.Instance?.CurrentPlayData);
            }

            // 通知 BossResultScreenNavigation 更新按钮
            BossResultScreenNavigation bossNav = resultPanel.GetComponent<BossResultScreenNavigation>();
            if (bossNav != null)
            {
                bossNav.SetupButtons(isGameOverTriggered);
            }
        }
        else
        {
            Debug.LogWarning("[BossSongCompletionDetector] Result panel not assigned!");
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
                Debug.Log($"[BossSongCompletionDetector] Hidden panel: {panel.name}");
            }
        }

        if (hiddenCount > 0)
        {
            Debug.Log($"[BossSongCompletionDetector] Hidden {hiddenCount} panel(s)");
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

    /// <summary>
    /// 检查是否为游戏失败
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOverTriggered;
    }
}
