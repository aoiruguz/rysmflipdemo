using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 剧情对话管理器 - 事件驱动版本
/// 可以在任何地方调用显示对话
/// </summary>
public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("对话 Panel 1")]
    public GameObject dialogPanel1;
    public TextMeshProUGUI dialogText1;
    public TextMeshProUGUI speakerName1;

    [Header("对话 Panel 2")]
    public GameObject dialogPanel2;
    public TextMeshProUGUI dialogText2;
    public TextMeshProUGUI speakerName2;

    [Header("对话 Panel 3 (Boss战专用)")]
    public GameObject dialogPanel3;
    public TextMeshProUGUI dialogText3;
    public TextMeshProUGUI speakerName3;

    [Header("跳过按钮")]
    public Button skipButton;
    public GameObject skipButtonObject;
    [Tooltip("开启后，跳过按钮将始终显示，无视 showSkipButton 参数")]
    public bool alwaysShowSkipButton = false;

    [Header("默认设置")]
    public float defaultDialogDisplayDuration = 3f;
    public float defaultDialogTransitionDelay = 0.5f;

    private bool isPlayingDialogue = false;
    private bool skipRequested = false;
    private Coroutine currentDialogueCoroutine;

    // 事件：对话开始
    public event Action OnDialogueStart;
    // 事件：对话结束
    public event Action OnDialogueComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 初始化 UI
        HideAllPanels();

        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipButtonClicked);
    }

    private void Update()
    {
        // 当跳过按钮显示时，按 Space 键可以跳过
        if (skipButtonObject != null && skipButtonObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnSkipButtonClicked();
            }
        }
    }

    /// <summary>
    /// 显示单句对话（简单版本）
    /// </summary>
    /// <param name="panelIndex">面板索引</param>
    /// <param name="speakerName">说话者名称</param>
    /// <param name="text">对话文本</param>
    /// <param name="duration">显示时长</param>
    /// <param name="showSkipButton">是否显示跳过按钮（默认false）</param>
    public void ShowDialogue(int panelIndex, string speakerName, string text, float duration = -1, bool showSkipButton = false)
    {
        if (duration < 0)
            duration = defaultDialogDisplayDuration;

        DialogueData dialogue = new DialogueData
        {
            panelIndex = panelIndex,
            speakerName = speakerName,
            text = text
        };

        ShowDialogues(new DialogueData[] { dialogue }, showSkipButton, duration, 0f);
    }

    /// <summary>
    /// 显示多句对话序列
    /// </summary>
    public void ShowDialogues(DialogueData[] dialogues, bool showSkipButton = false,
        float displayDuration = -1, float transitionDelay = -1, Action onComplete = null)
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            Debug.LogWarning("[StoryManager] No dialogues provided");
            onComplete?.Invoke();
            return;
        }

        // 如果正在播放，先停止
        if (isPlayingDialogue && currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }

        // 使用默认值
        if (displayDuration < 0)
            displayDuration = defaultDialogDisplayDuration;
        if (transitionDelay < 0)
            transitionDelay = defaultDialogTransitionDelay;

        skipRequested = false;

        // 显示跳过按钮
        if (skipButtonObject != null)
        {
            // 如果开启了 alwaysShowSkipButton，则始终显示
            bool shouldShow = alwaysShowSkipButton || showSkipButton;
            skipButtonObject.SetActive(shouldShow);
        }

        currentDialogueCoroutine = StartCoroutine(PlayDialogueSequence(dialogues, displayDuration, transitionDelay, onComplete));
    }

    /// <summary>
    /// 播放对话序列
    /// </summary>
    private IEnumerator PlayDialogueSequence(DialogueData[] dialogues, float displayDuration, float transitionDelay, Action onComplete)
    {
        isPlayingDialogue = true;

        // 触发开始事件
        OnDialogueStart?.Invoke();

        // 播放每句对话
        for (int i = 0; i < dialogues.Length; i++)
        {
            if (skipRequested)
                break;

            var dialogue = dialogues[i];

            // 显示对话
            if (dialogue.panelIndex == 0 || dialogue.panelIndex == 1)
            {
                ShowDialogOnPanel1(dialogue);
            }
            else if (dialogue.panelIndex == 2)
            {
                ShowDialogOnPanel2(dialogue);
            }
            else if (dialogue.panelIndex == 3)
            {
                ShowDialogOnPanel3(dialogue);
            }

            // 等待显示时长
            float elapsed = 0f;
            while (elapsed < displayDuration && !skipRequested)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (skipRequested)
                break;

            // 对话切换延迟
            if (i < dialogues.Length - 1) // 不是最后一句
            {
                yield return new WaitForSecondsRealtime(transitionDelay);
            }
        }

        // 隐藏所有面板（包括跳过按钮）
        HideAllPanels();

        isPlayingDialogue = false;

        // 触发完成事件
        OnDialogueComplete?.Invoke();

        // 回调完成
        onComplete?.Invoke();
    }

    /// <summary>
    /// 在 Panel 1 显示对话
    /// </summary>
    private void ShowDialogOnPanel1(DialogueData dialogue)
    {
        if (dialogPanel1 != null)
            dialogPanel1.SetActive(true);
        if (dialogPanel2 != null)
            dialogPanel2.SetActive(false);
        if (dialogPanel3 != null)
            dialogPanel3.SetActive(false);

        if (speakerName1 != null)
            speakerName1.text = dialogue.speakerName;

        if (dialogText1 != null)
            dialogText1.text = dialogue.text;
    }

    /// <summary>
    /// 在 Panel 2 显示对话
    /// </summary>
    private void ShowDialogOnPanel2(DialogueData dialogue)
    {
        if (dialogPanel1 != null)
            dialogPanel1.SetActive(false);
        if (dialogPanel2 != null)
            dialogPanel2.SetActive(true);
        if (dialogPanel3 != null)
            dialogPanel3.SetActive(false);

        if (speakerName2 != null)
            speakerName2.text = dialogue.speakerName;

        if (dialogText2 != null)
            dialogText2.text = dialogue.text;
    }

    /// <summary>
    /// 在 Panel 3 显示对话 (Boss战专用)
    /// </summary>
    private void ShowDialogOnPanel3(DialogueData dialogue)
    {
        if (dialogPanel1 != null)
            dialogPanel1.SetActive(false);
        if (dialogPanel2 != null)
            dialogPanel2.SetActive(false);
        if (dialogPanel3 != null)
            dialogPanel3.SetActive(true);

        if (speakerName3 != null)
            speakerName3.text = dialogue.speakerName;

        if (dialogText3 != null)
            dialogText3.text = dialogue.text;
    }

    /// <summary>
    /// 隐藏所有对话面板
    /// </summary>
    public void HideAllPanels()
    {
        if (dialogPanel1 != null)
            dialogPanel1.SetActive(false);
        if (dialogPanel2 != null)
            dialogPanel2.SetActive(false);
        if (dialogPanel3 != null)
            dialogPanel3.SetActive(false);
        if (skipButtonObject != null)
            skipButtonObject.SetActive(false);
    }

    /// <summary>
    /// 跳过按钮点击
    /// </summary>
    private void OnSkipButtonClicked()
    {
        if (isPlayingDialogue)
        {
            skipRequested = true;
            Debug.Log("[StoryManager] Dialogue skipped by player");
        }
    }

    /// <summary>
    /// 检查是否正在播放对话
    /// </summary>
    public bool IsPlayingDialogue()
    {
        return isPlayingDialogue;
    }

    /// <summary>
    /// 停止当前对话
    /// </summary>
    public void StopDialogue()
    {
        if (isPlayingDialogue && currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            HideAllPanels(); // 会自动隐藏跳过按钮
            isPlayingDialogue = false;
            OnDialogueComplete?.Invoke();
        }
    }
}
