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
    public Image leftCharacterImage;
    public Image rightCharacterImage;

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

    [Header("立绘动画效果")]
    [Tooltip("说话角色的透明度")]
    public float speakingAlpha = 1.0f;
    [Tooltip("非说话角色的透明度")]
    public float nonSpeakingAlpha = 0.5f;
    [Tooltip("说话角色的缩放比例")]
    public float speakingScale = 1.05f;
    [Tooltip("非说话角色的缩放比例")]
    public float nonSpeakingScale = 0.95f;
    [Tooltip("透明度和缩放过渡时间")]
    public float transitionDuration = 0.3f;

    // 运行时状态
    private StoryData _currentStory;
    private List<StoryDialogueData> _dialogues;
    private int _currentIndex = 0;
    private Action _onComplete;
    private Coroutine _typewriterCoroutine;
    private bool _isTyping = false;
    private bool _isInitialized = false;

    private void Awake()
    {
        // 如果 storyPanel 未配置，使用自己所在的 GameObject
        if (storyPanel == null)
        {
            storyPanel = gameObject;
            Debug.Log($"[StoryPlayer] storyPanel 未配置，自动使用当前对象: {gameObject.name}");
        }

        // 绑定按钮事件
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextClicked);
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);

        // 在 Awake 中初始化并隐藏元素（只执行一次）
        if (!_isInitialized)
        {
            HideAllElements();
            _isInitialized = true;
        }
    }

    private void HideAllElements()
    {
        // 不要禁用 storyPanel 本身，只禁用子元素
        // 因为如果 storyPanel 就是 StoryPlayer 所在的对象，禁用它会导致组件无法工作

        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(false);
        if (leftCharacterImage != null)
            leftCharacterImage.gameObject.SetActive(false);
        if (rightCharacterImage != null)
            rightCharacterImage.gameObject.SetActive(false);
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        Debug.Log("[StoryPlayer] Hidden all UI elements");
    }

    private void ShowAllElements()
    {
        // 不需要激活 storyPanel 本身，只激活子元素

        if (backgroundImage != null)
            backgroundImage.gameObject.SetActive(true);
        // 立绘会在 ShowDialogue 中根据数据动态显示
        if (dialogueBox != null)
            dialogueBox.SetActive(true);
        if (nextButton != null)
            nextButton.gameObject.SetActive(true);
        if (skipButton != null)
            skipButton.gameObject.SetActive(true);

        Debug.Log("[StoryPlayer] Shown all UI elements");
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

        // 更新角色立绘（双立绘槽）
        UpdateCharacterSprites(dialogue);

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

    private void UpdateCharacterSprites(StoryDialogueData dialogue)
    {
        // 兼容旧数据格式：如果使用旧的 characterSpritePath，自动转换为左侧立绘
        if (!string.IsNullOrEmpty(dialogue.characterSpritePath) &&
            (dialogue.leftCharacter == null || string.IsNullOrEmpty(dialogue.leftCharacter.spritePath)) &&
            (dialogue.rightCharacter == null || string.IsNullOrEmpty(dialogue.rightCharacter.spritePath)))
        {
            dialogue.leftCharacter = new CharacterDisplayData
            {
                spritePath = dialogue.characterSpritePath,
                scale = 1.0f
            };
            dialogue.speakerPosition = "left";
        }

        // 更新左侧立绘
        UpdateSingleCharacterSprite(leftCharacterImage, dialogue.leftCharacter, dialogue.speakerPosition == "left");

        // 更新右侧立绘
        UpdateSingleCharacterSprite(rightCharacterImage, dialogue.rightCharacter, dialogue.speakerPosition == "right");
    }

    private void UpdateSingleCharacterSprite(Image characterImage, CharacterDisplayData characterData, bool isSpeaking)
    {
        if (characterImage == null) return;

        // 如果没有数据或路径为空，隐藏立绘
        if (characterData == null || string.IsNullOrEmpty(characterData.spritePath))
        {
            characterImage.gameObject.SetActive(false);
            return;
        }

        // 加载立绘
        Sprite sprite = Resources.Load<Sprite>(characterData.spritePath);
        if (sprite != null)
        {
            characterImage.sprite = sprite;
            characterImage.gameObject.SetActive(true);

            // 设置立绘基础缩放
            float baseScale = characterData.scale;

            // 应用说话/非说话状态的动画效果
            StartCoroutine(AnimateCharacterState(characterImage, baseScale, isSpeaking));
        }
        else
        {
            Debug.LogWarning($"[StoryPlayer] 未找到角色立绘：{characterData.spritePath}");
            characterImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator AnimateCharacterState(Image characterImage, float baseScale, bool isSpeaking)
    {
        if (characterImage == null) yield break;

        float targetAlpha = isSpeaking ? speakingAlpha : nonSpeakingAlpha;
        float targetScale = baseScale * (isSpeaking ? speakingScale : nonSpeakingScale);

        Color startColor = characterImage.color;
        Vector3 startScale = characterImage.transform.localScale;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        Vector3 targetScaleVec = new Vector3(targetScale, targetScale, 1f);

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            // 平滑插值
            characterImage.color = Color.Lerp(startColor, targetColor, t);
            characterImage.transform.localScale = Vector3.Lerp(startScale, targetScaleVec, t);

            yield return null;
        }

        // 确保最终值精确
        characterImage.color = targetColor;
        characterImage.transform.localScale = targetScaleVec;
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
