using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SettingsUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Button modeButton;
    public Text modeText;
    public Button retryButton;
    public Button exitButton;
    public Button closeButton;

    [Header("Countdown Settings")]
    public Image countdownImage;
    public Sprite[] countdownSprites; // 索引 0:3, 1:2, 2:1

    [Header("Transition Settings")]
    [Tooltip("设置面板关闭时的淡出时长（秒）")]
    public float panelFadeDuration = 0.25f;

    [Header("Countdown Animation")]
    public float countdownPunchScale = 1.3f;
    public float countdownPunchDuration = 0.4f;

    private CanvasGroup _canvasGroup;
    public bool IsCountingDown { get; private set; } = false;

    private void Awake()
    {
        // 获取或添加 CanvasGroup，用于在倒计时期间隐藏面板但保持 GameObject 激活
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (closeButton) closeButton.onClick.AddListener(Close);
        if (modeButton) modeButton.onClick.AddListener(ToggleMode);
        if (retryButton) retryButton.onClick.AddListener(RestartScene);
        
        UpdateModeUI();

        // Ensure it's hidden at start if it's in the scene
        gameObject.SetActive(false);
    }

    public void Open()
    {
        Debug.Log("[Settings] Opening Settings Menu");
        
        // 如果正在倒计时，停止它
        if (IsCountingDown)
        {
            StopAllCoroutines();
            IsCountingDown = false;
            if (countdownImage) countdownImage.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
        SetPanelVisible(true);
        Time.timeScale = 0;

        // Refresh values from persistent settings
        UpdateModeUI();

        PauseAudio();
    }


    public void Close()
    {
        // 禁用按钮交互，防止淡出期间重复点击
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        // 如果配置了倒计时，则：淡出面板 → 倒计时 → 平滑恢复
        // 否则：淡出面板 → 直接恢复
        StartCoroutine(CloseSequence());
    }

    private System.Collections.IEnumerator CloseSequence()
    {
        // 1. 平滑淡出设置面板
        yield return FadePanelRoutine(1f, 0f, panelFadeDuration);

        // 2. 如果配置了倒计时，则执行倒计时
        if (countdownImage != null && countdownSprites != null && countdownSprites.Length >= 3)
        {
            yield return CountdownRoutine();
        }

        // 3. 立即恢复游戏
        ResumeGameImmediately();
    }

    /// <summary>
    /// 控制面板的视觉显示/隐藏，不影响 GameObject 的激活状态
    /// </summary>
    private void SetPanelVisible(bool visible)
    {
        if (_canvasGroup == null) return;
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;
    }

    private System.Collections.IEnumerator CountdownRoutine()
    {
        IsCountingDown = true;
        countdownImage.gameObject.SetActive(true);
        
        // 依次显示 3, 2, 1
        for (int i = 0; i < countdownSprites.Length; i++)
        {
            countdownImage.sprite = countdownSprites[i];
            countdownImage.SetNativeSize();
            
            // 动效：先设为放大状态，然后平滑缩回到原始大小 (1,1,1)
            countdownImage.transform.localScale = Vector3.one * countdownPunchScale;
            countdownImage.transform.DOScale(1f, countdownPunchDuration).SetUpdate(true).SetEase(Ease.OutQuad);
            
            yield return new WaitForSecondsRealtime(1f);
        }
        
        // 1 结束后，快速缩小到 0
        float shrinkDuration = 0.2f;
        countdownImage.transform.DOScale(0f, shrinkDuration).SetUpdate(true).SetEase(Ease.InBack);
        yield return new WaitForSecondsRealtime(shrinkDuration);
        
        countdownImage.gameObject.SetActive(false);
        countdownImage.transform.localScale = Vector3.one; // 恢复缩放以便下次使用
        IsCountingDown = false;
    }

    /// <summary>
    /// 平滑淡入/淡出面板 CanvasGroup 的 alpha
    /// </summary>
    private System.Collections.IEnumerator FadePanelRoutine(float from, float to, float duration)
    {
        if (_canvasGroup == null) yield break;

        float elapsed = 0f;
        _canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        _canvasGroup.alpha = to;
    }

    /// <summary>
    /// 立即恢复游戏：直接将 TimeScale 恢复为 1，避免渐变导致音频/谱面错位
    /// </summary>
    private void ResumeGameImmediately()
    {
        Time.timeScale = 1f;

        // 恢复音频
        ResumeAudio();

        // 彻底隐藏 SettingsUI 物体
        gameObject.SetActive(false);

        // Notify player controller of mode change
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.currentMode = (PlayerController.ControlMode)GameSettings.InputMode;
            player.RefreshControlMode();
        }
    }

    private void ToggleMode()
    {
        int nextMode = (GameSettings.InputMode + 1) % 2;
        GameSettings.InputMode = nextMode;
        Debug.Log($"[Settings] Input Mode toggled to: {GameSettings.InputMode}");
        UpdateModeUI();
    }

    private void RestartScene()
    {
        Debug.Log("[Settings] Restarting Scene");
        Time.timeScale = 1; // 必须恢复时间缩放，否则场景加载后可能处于暂停状态
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void UpdateModeUI()
    {
        if (modeText)
        {
            modeText.text = GameSettings.InputMode == 0 ? "Mode: Pre-input (Preset)" : "Mode: Strict (Accurate)";
        }
    }


    private void OnInputModeChanged(int value)
    {
        GameSettings.InputMode = value;
    }

    private void PauseAudio()
    {
        NoteManager nm = Object.FindFirstObjectByType<NoteManager>();
        if (nm != null && nm.audioSource != null && nm.audioSource.isPlaying)
        {
            nm.audioSource.Pause();
        }
    }

    private void ResumeAudio()
    {
        NoteManager nm = Object.FindFirstObjectByType<NoteManager>();
        if (nm != null && nm.audioSource != null)
        {
            nm.audioSource.UnPause();
        }
    }
}
