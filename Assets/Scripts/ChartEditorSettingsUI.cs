using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Chart Editor设置面板UI控制器
/// 提供音量、偏移等设置的可视化调整界面
/// </summary>
public class ChartEditorSettingsUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider musicVolumeSlider;
    public TextMeshProUGUI musicVolumeText;

    public Slider hitSoundVolumeSlider;
    public TextMeshProUGUI hitSoundVolumeText;

    public TMP_InputField offsetInputField;
    public TextMeshProUGUI offsetText;

    public Button offsetIncreaseButton;
    public Button offsetDecreaseButton;
    public Button resetButton;
    public Button closeButton;

    [Header("References")]
    private ChartEditorManager manager;

    void Start()
    {
        // 查找管理器
        manager = FindObjectOfType<ChartEditorManager>();

        // 初始化UI显示当前值
        RefreshUI();

        // 绑定事件
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (hitSoundVolumeSlider != null)
            hitSoundVolumeSlider.onValueChanged.AddListener(OnHitSoundVolumeChanged);

        if (offsetInputField != null)
            offsetInputField.onEndEdit.AddListener(OnOffsetInputChanged);

        if (offsetIncreaseButton != null)
            offsetIncreaseButton.onClick.AddListener(() => AdjustOffset(0.01f));

        if (offsetDecreaseButton != null)
            offsetDecreaseButton.onClick.AddListener(() => AdjustOffset(-0.01f));

        if (resetButton != null)
            resetButton.onClick.AddListener(OnResetClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    /// <summary>
    /// 刷新UI显示
    /// </summary>
    public void RefreshUI()
    {
        // 音乐音量
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = ChartEditorSettings.MusicVolume;
        if (musicVolumeText != null)
            musicVolumeText.text = $"{ChartEditorSettings.MusicVolume:P0}";

        // 打击音效音量
        if (hitSoundVolumeSlider != null)
            hitSoundVolumeSlider.value = ChartEditorSettings.HitSoundVolume;
        if (hitSoundVolumeText != null)
            hitSoundVolumeText.text = $"{ChartEditorSettings.HitSoundVolume:P0}";

        // 偏移
        if (offsetInputField != null)
            offsetInputField.text = ChartEditorSettings.EditorOffset.ToString("F3");
        if (offsetText != null)
            offsetText.text = $"{ChartEditorSettings.EditorOffset:F3}s";
    }

    /// <summary>
    /// 音乐音量改变
    /// </summary>
    void OnMusicVolumeChanged(float value)
    {
        ChartEditorSettings.MusicVolume = value;

        // 更新显示
        if (musicVolumeText != null)
            musicVolumeText.text = $"{value:P0}";

        // 立即应用到AudioSource
        if (manager != null && manager.audioSource != null)
        {
            manager.audioSource.volume = value;
        }
    }

    /// <summary>
    /// 打击音效音量改变
    /// </summary>
    void OnHitSoundVolumeChanged(float value)
    {
        ChartEditorSettings.HitSoundVolume = value;

        // 更新显示
        if (hitSoundVolumeText != null)
            hitSoundVolumeText.text = $"{value:P0}";
    }

    /// <summary>
    /// 偏移输入改变
    /// </summary>
    void OnOffsetInputChanged(string value)
    {
        if (float.TryParse(value, out float offset))
        {
            ChartEditorSettings.EditorOffset = offset;

            // 更新显示
            if (offsetText != null)
                offsetText.text = $"{offset:F3}s";

            // 重新生成note
            RegenerateNotes();
        }
        else
        {
            // 输入无效，恢复原值
            if (offsetInputField != null)
                offsetInputField.text = ChartEditorSettings.EditorOffset.ToString("F3");
        }
    }

    /// <summary>
    /// 调整偏移
    /// </summary>
    void AdjustOffset(float delta)
    {
        ChartEditorSettings.AdjustOffset(delta);
        RefreshUI();
        RegenerateNotes();
    }

    /// <summary>
    /// 重置所有设置
    /// </summary>
    void OnResetClicked()
    {
        ChartEditorSettings.ResetToDefaults();
        RefreshUI();

        // 应用音乐音量
        if (manager != null && manager.audioSource != null)
        {
            manager.audioSource.volume = ChartEditorSettings.MusicVolume;
        }

        // 重新生成note
        RegenerateNotes();
    }

    /// <summary>
    /// 重新生成所有note
    /// </summary>
    void RegenerateNotes()
    {
        if (manager != null)
        {
            manager.RegenerateAllNotes();
        }
    }

    /// <summary>
    /// 显示设置面板
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        RefreshUI();
    }

    /// <summary>
    /// 隐藏设置面板
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
