using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 剧情播放器UI组件
/// 挂载到场景中的Canvas下的StoryPanel对象上
/// 需要在Inspector中配置各UI引用
/// </summary>
public class StoryPlayer : MonoBehaviour
{
    [Header("主面板")]
    public GameObject storyPanel;

    [Header("背景")]
    public Image backgroundImage;

    [Header("角色立绘")]
    public Image characterImage;

    [Header("对话框")]
    public GameObject dialogueBox;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueText;

    [Header("按钮")]
    public Button nextButton;      // 下一句 / 点击任意区域前进
    public Button skipButton;      // 跳过整段剧情

    [Header("打字机效果")]
    [Tooltip("每个字之间的间隔时间（秒）")]
    public float typewriterSpeed = 0.05f;

    // 运行时状态
    private StoryData _currentStory;
    private List<StoryDialogueData> _dialogues;
    private int _currentIndex = 0;
    private Action _onComplete;
    private Coroutine _typewriterCoroutine;
    private bool _isTyping = false;

    private void Awake()
    {
        // 绑定按钮事件
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);
    }

    private void Start()
    {
        // 在Start中隐藏所有元素，确保初始状态是隐藏的
        HideAllElements();
    }

    private void HideAllElements()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);
        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(false);
        if (characterImage != null)
            characterImage.gameObject.SetActive(false);
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
    }

    private void ShowAllElements()
    {
        if (storyPanel != null)
            storyPanel.SetActive(true);
        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(true);
        if (characterImage != null)
            characterImage.gameObject.SetActive(true);
        if (dialogueBox != null)
            dialogueBox.SetActive(true);
        if (nextButton != null)
            nextButton.gameObject.SetActive(true);
        if (skipButton != null)
            skipButton.gameObject.SetActive(true);
    }

    // ===================== 公共接口 =====================

    /// <summary>
    /// 开始播放剧情
    /// </summary>
    public void Play(StoryData storyData, Action onComplete)
    {
        _currentStory = storyData;
        _dialogues = storyData.dialogues;
        _currentIndex = 0;
        _onComplete = onComplete;

        if (_dialogues == null || _dialogues.Count == 0)
        {
            Debug.LogWarning($"[StoryPlayer] 剧情 {storyData.storyId} 没有对话数据");
            EndStory();
            return;
        }

        // 显示所有UI元素
        ShowAllElements();

        // 直接开始播放
        ShowDialogue(_currentIndex);
    }

    // ===================== 内部逻辑 =====================

    private void ShowDialogue(int index)
    {
        if (index >= _dialogues.Count)
        {
            EndStory();
            return;
        }

        StoryDialogueData dialogue = _dialogues[index];

        // 更新背景
        UpdateBackground(dialogue.backgroundSpritePath);

        // 更新角色立绘
        UpdateCharacterSprite(dialogue.characterSpritePath);

        // 更新角色名
        if (characterNameText != null)
            characterNameText.text = dialogue.characterName ?? string.Empty;

        // 播放对话文字（打字机效果）
        if (_typewriterCoroutine != null)
            StopCoroutine(_typewriterCoroutine);
        _typewriterCoroutine = StartCoroutine(TypewriterEffect(dialogue.dialogueText));
    }

    private IEnumerator TypewriterEffect(string text)
    {
        _isTyping = true;
        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSecondsRealtime(typewriterSpeed);
            }
        }
        _isTyping = false;
    }

    private void UpdateBackground(string spritePath)
    {
        if (backgroundImage == null) return;

        if (string.IsNullOrEmpty(spritePath))
        {
            // 无背景路径时保持上一张背景（或隐藏）
            return;
        }

        Sprite sprite = Resources.Load<Sprite>(spritePath);
        if (sprite != null)
        {
            backgroundImage.sprite = sprite;
            backgroundImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[StoryPlayer] 未找到背景图：{spritePath}");
        }
    }

    private void UpdateCharacterSprite(string spritePath)
    {
        if (characterImage == null) return;

        if (string.IsNullOrEmpty(spritePath))
        {
            characterImage.gameObject.SetActive(false);
            return;
        }

        Sprite sprite = Resources.Load<Sprite>(spritePath);
        if (sprite != null)
        {
            characterImage.sprite = sprite;
            characterImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[StoryPlayer] 未找到角色立绘：{spritePath}");
            characterImage.gameObject.SetActive(false);
        }
    }

    // ===================== 按钮回调 =====================

    private void OnNextClicked()
    {
        if (_isTyping)
        {
            // 打字机尚未完成时：立即显示全部文字
            if (_typewriterCoroutine != null)
                StopCoroutine(_typewriterCoroutine);
            _isTyping = false;
            if (dialogueText != null && _currentIndex < _dialogues.Count)
                dialogueText.text = _dialogues[_currentIndex].dialogueText;
        }
        else
        {
            // 文字已完全显示时：前进到下一句
            _currentIndex++;
            ShowDialogue(_currentIndex);
        }
    }

    private void OnSkipClicked()
    {
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
            _typewriterCoroutine = null;
        }
        _isTyping = false;
        EndStory();
    }

    private void EndStory()
    {
        // 隐藏所有UI元素而不是整个面板
        HideAllElements();

        _currentStory = null;
        _dialogues = null;
        _currentIndex = 0;

        _onComplete?.Invoke();
        _onComplete = null;
    }
}
