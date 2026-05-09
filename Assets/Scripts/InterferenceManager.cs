using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 干扰系统主控制器
/// 负责监听歌曲播放进度，在特定时机触发干扰技能
/// </summary>
public class InterferenceManager : MonoBehaviour
{
    [Header("引用")]
    [Tooltip("SongCompletionDetector引用，用于获取谱面进度")]
    public SongCompletionDetector songCompletionDetector;

    [Tooltip("干扰UI Canvas")]
    public Canvas interferenceCanvas;

    [Header("技能组件")]
    [Tooltip("滚动弹幕技能")]
    public ScrollingTextSkill scrollingTextSkill;

    [Header("调试")]
    [Tooltip("显示调试信息")]
    public bool showDebugInfo = true;

    // 私有变量
    private LevelData currentLevelData;
    private InterferenceTextConfig textConfig;
    private List<float> triggerPoints = new List<float>(); // 触发点列表（进度百分比）
    private int currentTriggerIndex = 0; // 当前触发点索引
    private bool isInitialized = false;
    private bool isSkillActive = false;

    void Start()
    {
        Initialize();
    }

    void Update()
    {
        if (!isInitialized || isSkillActive) return;

        // 检查是否到达触发点
        CheckTriggerPoints();
    }

    /// <summary>
    /// 初始化干扰系统
    /// </summary>
    private void Initialize()
    {
        // 获取SongCompletionDetector
        if (songCompletionDetector == null)
        {
            songCompletionDetector = FindFirstObjectByType<SongCompletionDetector>();
            if (songCompletionDetector == null)
            {
                Debug.LogError("[InterferenceManager] SongCompletionDetector not found!");
                return;
            }
        }

        // 获取当前关卡数据
        currentLevelData = LevelUIManager.GetCurrentLevelData();
        if (currentLevelData == null)
        {
            Debug.LogWarning("[InterferenceManager] No LevelData found, interference system disabled.");
            return;
        }

        // 获取弹幕配置
        textConfig = currentLevelData.interferenceTextConfig;
        if (textConfig == null)
        {
            Debug.LogWarning("[InterferenceManager] No InterferenceTextConfig assigned to LevelData.");
            return;
        }

        // 检查干扰次数
        if (currentLevelData.interferenceCount <= 0)
        {
            Debug.Log("[InterferenceManager] Interference count is 0, system disabled.");
            return;
        }

        // 确保Canvas存在
        if (interferenceCanvas == null)
        {
            CreateInterferenceCanvas();
        }

        // 初始化滚动弹幕技能
        if (scrollingTextSkill == null)
        {
            GameObject skillObj = new GameObject("ScrollingTextSkill");
            skillObj.transform.SetParent(transform);
            scrollingTextSkill = skillObj.AddComponent<ScrollingTextSkill>();
            scrollingTextSkill.config = textConfig;
            scrollingTextSkill.interferenceCanvas = interferenceCanvas;
            scrollingTextSkill.canvasRect = interferenceCanvas.GetComponent<RectTransform>();
        }

        // 计算触发点
        CalculateTriggerPoints();

        isInitialized = true;
        Debug.Log($"[InterferenceManager] Initialized with {triggerPoints.Count} trigger points.");
    }

    /// <summary>
    /// 创建干扰UI Canvas
    /// </summary>
    private void CreateInterferenceCanvas()
    {
        GameObject canvasObj = new GameObject("InterferenceCanvas");
        interferenceCanvas = canvasObj.AddComponent<Canvas>();
        interferenceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        interferenceCanvas.sortingOrder = 1000; // 确保在所有UI之上

        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        Debug.Log("[InterferenceManager] Created Interference Canvas.");
    }

    /// <summary>
    /// 计算触发点（基于歌曲进度百分比）
    /// </summary>
    private void CalculateTriggerPoints()
    {
        triggerPoints.Clear();
        int count = currentLevelData.interferenceCount;

        // 均匀分布触发点
        for (int i = 0; i < count; i++)
        {
            // 计算基础触发点（例如：3次干扰 -> 25%, 50%, 75%）
            float baseProgress = (i + 1) / (float)(count + 1);

            // 添加随机偏移（±5%），增加不可预测性
            float randomOffset = Random.Range(-0.05f, 0.05f);
            float triggerProgress = Mathf.Clamp01(baseProgress + randomOffset);

            triggerPoints.Add(triggerProgress);
        }

        // 排序触发点
        triggerPoints.Sort();

        if (showDebugInfo)
        {
            Debug.Log($"[InterferenceManager] Trigger points: {string.Join(", ", triggerPoints.ConvertAll(p => $"{p * 100:F1}%"))}");
        }
    }

    /// <summary>
    /// 检查是否到达触发点
    /// </summary>
    private void CheckTriggerPoints()
    {
        if (currentTriggerIndex >= triggerPoints.Count) return;

        // 获取当前歌曲进度
        float currentProgress = GetSongProgress();

        // 检查是否到达下一个触发点
        if (currentProgress >= triggerPoints[currentTriggerIndex])
        {
            TriggerInterference();
            currentTriggerIndex++;
        }
    }

    /// <summary>
    /// 获取当前谱面进度（0-1）
    /// 基于已处理的Note数量 / 总Note数量
    /// </summary>
    private float GetSongProgress()
    {
        if (songCompletionDetector == null)
        {
            return 0f;
        }

        return songCompletionDetector.GetProgress();
    }

    /// <summary>
    /// 触发干扰技能
    /// </summary>
    private void TriggerInterference()
    {
        if (showDebugInfo)
        {
            Debug.Log($"[InterferenceManager] Triggering interference at {GetSongProgress() * 100:F1}%");
        }

        // 目前只有弹幕技能，直接激活
        StartCoroutine(ActivateScrollingText());
    }

    /// <summary>
    /// 激活滚动弹幕技能
    /// </summary>
    private IEnumerator ActivateScrollingText()
    {
        isSkillActive = true;

        scrollingTextSkill.Activate();

        // 等待技能持续时间
        yield return new WaitForSeconds(scrollingTextSkill.GetDuration());

        scrollingTextSkill.Deactivate();

        isSkillActive = false;
    }

    /// <summary>
    /// 在编辑器中显示调试信息
    /// </summary>
    void OnGUI()
    {
        if (!showDebugInfo || !isInitialized) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.yellow;

        float progress = GetSongProgress();
        string info = $"Song Progress: {progress * 100:F1}%\n";
        info += $"Next Trigger: {(currentTriggerIndex < triggerPoints.Count ? $"{triggerPoints[currentTriggerIndex] * 100:F1}%" : "None")}\n";
        info += $"Triggers Fired: {currentTriggerIndex}/{triggerPoints.Count}";

        GUI.Label(new Rect(10, 10, 400, 100), info, style);
    }
}
