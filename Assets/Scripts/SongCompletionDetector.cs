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

    [Header("Scene Transition Settings")]
    [Tooltip("歌曲完成后要跳转的场景名称")]
    public string targetSceneName = "Game Over";

    [Tooltip("是否启用完成检测（调延迟界面可以禁用此项）")]
    public bool enableCompletionDetection = true;

    [Tooltip("完成后等待多少秒再跳转")]
    public float delayBeforeTransition = 3f;

    [Tooltip("游戏失败后等待多少秒再跳转")]
    public float delayBeforeGameOverTransition = 2f;

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

        // 显示 Game Over 文本
        UIManager.Instance?.ShowSongClearText("Game Over");

        // 等待后跳转
        StartCoroutine(LoadSceneAfterDelay(delayBeforeGameOverTransition));
    }

    /// <summary>
    /// 歌曲完成处理
    /// </summary>
    private IEnumerator OnSongCompleted()
    {
        if (isTransitioning) yield break;
        isTransitioning = true;

        Debug.Log("[SongCompletionDetector] Song completed!");

        // 收集最终游戏数据
        if (PlayDataCollector.Instance != null)
        {
            PlayDataCollector.Instance.CollectFinalData();
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

        // 等待指定时间
        yield return new WaitForSeconds(delayBeforeTransition);

        // 跳转场景
        LoadScene();
    }

    /// <summary>
    /// 延迟后跳转场景
    /// </summary>
    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadScene();
    }

    /// <summary>
    /// 统一的场景跳转方法
    /// </summary>
    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"[SongCompletionDetector] Loading scene: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("[SongCompletionDetector] No target scene specified!");
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
