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

    [Tooltip("故障快遮挡技能")]
    public GlitchEffectSkill glitchEffectSkill;

    [Tooltip("屏幕震动技能")]
    public ShakeEffectSkill shakeEffectSkill;

    [Header("技能配置")]
    [Tooltip("Glitch Material（拖入用于故障效果的Material）")]
    public Material glitchMaterial;

    [Tooltip("游戏主窗口 (用于屏幕震动)")]
    public RectTransform gameWindowRect;

    [Header("调试")]
    [Tooltip("显示调试信息")]
    public bool showDebugInfo = true;

    // 私有变量
    private LevelData currentLevelData;
    private InterferenceTextConfig textConfig;
    private List<InterferenceTrigger> triggers = new List<InterferenceTrigger>(); // 触发配置列表
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
        }

        // 检查干扰触发配置
        if (currentLevelData.interferenceTriggers == null || currentLevelData.interferenceTriggers.Length == 0)
        {
            Debug.Log("[InterferenceManager] No interference triggers configured, system disabled.");
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

        // 初始化故障快遮挡技能
        if (glitchEffectSkill == null)
        {
            GameObject skillObj = new GameObject("GlitchEffectSkill");
            skillObj.transform.SetParent(transform);
            glitchEffectSkill = skillObj.AddComponent<GlitchEffectSkill>();
            glitchEffectSkill.glitchMaterial = glitchMaterial;
        }

        // 初始化屏幕震动技能
        if (shakeEffectSkill == null)
        {
            GameObject skillObj = new GameObject("ShakeEffectSkill");
            skillObj.transform.SetParent(transform);
            shakeEffectSkill = skillObj.AddComponent<ShakeEffectSkill>();
            shakeEffectSkill.gameWindow = gameWindowRect;
        }

        // 加载触发配置
        LoadTriggerConfigs();

        isInitialized = true;
        Debug.Log($"[InterferenceManager] Initialized with {triggers.Count} trigger points.");
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
    /// 加载触发配置（从LevelData读取）
    /// </summary>
    private void LoadTriggerConfigs()
    {
        triggers.Clear();

        // 从LevelData复制触发配置
        foreach (var trigger in currentLevelData.interferenceTriggers)
        {
            triggers.Add(trigger);
        }

        // 按进度排序
        triggers.Sort((a, b) => a.triggerProgress.CompareTo(b.triggerProgress));

        if (showDebugInfo)
        {
            Debug.Log($"[InterferenceManager] Loaded {triggers.Count} triggers:");
            for (int i = 0; i < triggers.Count; i++)
            {
                Debug.Log($"  [{i}] {triggers[i].triggerProgress * 100:F1}% - {triggers[i].skillType}");
            }
        }
    }

    /// <summary>
    /// 检查是否到达触发点
    /// </summary>
    private void CheckTriggerPoints()
    {
        if (currentTriggerIndex >= triggers.Count) return;

        // 获取当前歌曲进度
        float currentProgress = GetSongProgress();

        // 检查是否到达下一个触发点
        if (currentProgress >= triggers[currentTriggerIndex].triggerProgress)
        {
            TriggerInterference(triggers[currentTriggerIndex].skillType);
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
    private void TriggerInterference(InterferenceSkillType skillType)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[InterferenceManager] Triggering {skillType} at {GetSongProgress() * 100:F1}%");
        }

        // 根据技能类型激活对应技能
        switch (skillType)
        {
            case InterferenceSkillType.ScrollingText:
                StartCoroutine(ActivateScrollingText());
                break;
            case InterferenceSkillType.GlitchEffect:
                StartCoroutine(ActivateGlitchEffect());
                break;
            case InterferenceSkillType.ScreenShake:
                StartCoroutine(ActivateScreenShake());
                break;
            case InterferenceSkillType.MouthAttack:
                StartCoroutine(ActivateMouthAttack());
                break;
            default:
                Debug.LogWarning($"[InterferenceManager] Unknown skill type: {skillType}");
                break;
        }
    }

    /// <summary>
    /// 激活滚动弹幕技能
    /// </summary>
    private IEnumerator ActivateScrollingText()
    {
        isSkillActive = true;

        // 显示技能对话
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.ShowDialogue(2, "对手", "海量弹幕！", -1);
        }

        scrollingTextSkill.Activate();

        // 等待技能持续时间
        yield return new WaitForSeconds(scrollingTextSkill.GetDuration());

        scrollingTextSkill.Deactivate();

        isSkillActive = false;
    }

    /// <summary>
    /// 激活故障快遮挡技能
    /// </summary>
    private IEnumerator ActivateGlitchEffect()
    {
        isSkillActive = true;

        // 显示技能对话
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.ShowDialogue(2, "对手", "故障风暴！", -1);
        }

        if (glitchEffectSkill != null)
        {
            glitchEffectSkill.Activate();

            // 等待技能持续时间
            yield return new WaitForSeconds(glitchEffectSkill.GetDuration());

            glitchEffectSkill.Deactivate();
        }
        else
        {
            Debug.LogWarning("[InterferenceManager] GlitchEffectSkill is not initialized!");
        }

        isSkillActive = false;
    }

    /// <summary>
    /// 激活屏幕震动技能
    /// </summary>
    private IEnumerator ActivateScreenShake()
    {
        isSkillActive = true;

        // 显示技能对话
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.ShowDialogue(2, "对手", "大地震！", -1);
        }

        if (shakeEffectSkill != null)
        {
            shakeEffectSkill.Activate();

            // 等待技能持续时间
            yield return new WaitForSeconds(shakeEffectSkill.GetDuration());
        }
        else
        {
            Debug.LogWarning("[InterferenceManager] ShakeEffectSkill is not initialized!");
            yield return new WaitForSeconds(2f);
        }

        isSkillActive = false;
    }

    /// <summary>
    /// 激活嘴巴攻击技能（占位）
    /// </summary>
    private IEnumerator ActivateMouthAttack()
    {
        isSkillActive = true;

        Debug.Log("[InterferenceManager] MouthAttack skill not implemented yet!");
        yield return new WaitForSeconds(2f);

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

        if (currentTriggerIndex < triggers.Count)
        {
            var nextTrigger = triggers[currentTriggerIndex];
            info += $"Next Trigger: {nextTrigger.triggerProgress * 100:F1}% ({nextTrigger.skillType})\n";
        }
        else
        {
            info += "Next Trigger: None\n";
        }

        info += $"Triggers Fired: {currentTriggerIndex}/{triggers.Count}";

        GUI.Label(new Rect(10, 10, 400, 100), info, style);
    }
}
