using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 设置界面管理器
/// 管理所有游戏设置：音量、分辨率、窗口模式、输入模式等
/// </summary>
public class SettingsUIManager : MonoBehaviour
{
    [Header("音量设置")]
    public Slider musicVolumeSlider;
    public Slider noteVolumeSlider;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI noteVolumeText;

    [Header("分辨率设置")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    [Header("窗口模式")]
    public TMP_Dropdown fullScreenModeDropdown;

    [Header("输入模式")]
    public Button inputModeButton;
    public TextMeshProUGUI inputModeText;

    [Header("音准与速度调节")]
    public Button offsetSpeedButton;
    public string offsetSpeedSceneName = "Middle Offest Speed Adjust";

    [Header("关闭按钮")]
    public Button closeButton;

    [Header("返回标题页")]
    [Tooltip("返回标题页按钮（可选）")]
    public Button returnToTitleButton;
    [Tooltip("标题页场景名称")]
    public string titleSceneName = "Title";

    private void Awake()
    {
        // 设置按钮监听
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
        if (inputModeButton != null)
            inputModeButton.onClick.AddListener(ToggleInputMode);
        if (offsetSpeedButton != null)
            offsetSpeedButton.onClick.AddListener(OpenOffsetSpeedScene);
        if (returnToTitleButton != null)
            returnToTitleButton.onClick.AddListener(ReturnToTitle);

        // 设置滑块监听
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (noteVolumeSlider != null)
            noteVolumeSlider.onValueChanged.AddListener(OnNoteVolumeChanged);

        // 设置下拉菜单监听
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (fullScreenModeDropdown != null)
            fullScreenModeDropdown.onValueChanged.AddListener(OnFullScreenModeChanged);

        // 初始化分辨率选项
        InitializeResolutions();

        // 初始化窗口模式选项
        InitializeFullScreenModes();

        // 默认隐藏
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        LoadCurrentSettings();
        Debug.Log("[Settings] 设置面板已打开");
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Debug.Log("[Settings] 设置面板已关闭");
    }

    // 加载当前设置
    private void LoadCurrentSettings()
    {
        // 音量设置
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = GameSettings.MusicVolume;
            UpdateMusicVolumeText(GameSettings.MusicVolume);
        }

        if (noteVolumeSlider != null)
        {
            noteVolumeSlider.value = GameSettings.NoteVolume;
            UpdateNoteVolumeText(GameSettings.NoteVolume);
        }

        // 分辨率设置
        if (resolutionDropdown != null)
        {
            int currentWidth = GameSettings.ResolutionWidth;
            int currentHeight = GameSettings.ResolutionHeight;

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == currentWidth && resolutions[i].height == currentHeight)
                {
                    resolutionDropdown.value = i;
                    break;
                }
            }
        }

        // 窗口模式
        if (fullScreenModeDropdown != null)
        {
            fullScreenModeDropdown.value = GameSettings.FullScreenMode;
        }

        // 输入模式
        UpdateInputModeText();
    }

    // 初始化分辨率选项
    private void InitializeResolutions()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();

        // 定义支持的16:9分辨率
        List<Resolution> supportedResolutions = new List<Resolution>
        {
            CreateResolution(1280, 720),
            CreateResolution(1920, 1080),
            CreateResolution(2560, 1440),
            CreateResolution(3840, 2160)
        };

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        // 添加支持的分辨率选项
        for (int i = 0; i < supportedResolutions.Count; i++)
        {
            Resolution res = supportedResolutions[i];
            options.Add(res.width + " x " + res.height);

            // 查找最接近当前分辨率的选项
            if (res.width == Screen.width && res.height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutions = supportedResolutions.ToArray();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    // 创建分辨率对象
    private Resolution CreateResolution(int width, int height)
    {
        Resolution res = new Resolution();
        res.width = width;
        res.height = height;
        res.refreshRate = Screen.currentResolution.refreshRate;
        return res;
    }

    // 初始化窗口模式选项
    private void InitializeFullScreenModes()
    {
        if (fullScreenModeDropdown == null) return;

        fullScreenModeDropdown.ClearOptions();
        List<string> options = new List<string>
        {
            "Windowed",
            "Fullscreen",
            "Borderless Window"
        };
        fullScreenModeDropdown.AddOptions(options);
        fullScreenModeDropdown.value = GameSettings.FullScreenMode;
        fullScreenModeDropdown.RefreshShownValue();
    }

    // 音乐音量改变
    private void OnMusicVolumeChanged(float value)
    {
        GameSettings.MusicVolume = value;
        UpdateMusicVolumeText(value);

        // 实时应用到场景中的音频源
        ApplyMusicVolumeToScene();
    }

    // 音效音量改变
    private void OnNoteVolumeChanged(float value)
    {
        GameSettings.NoteVolume = value;
        UpdateNoteVolumeText(value);
    }

    // 分辨率改变
    private void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;

        Resolution resolution = resolutions[index];
        GameSettings.ApplyResolution(resolution.width, resolution.height, GameSettings.FullScreenMode);
        Debug.Log($"[Settings] 分辨率已更改: {resolution.width}x{resolution.height}");
    }

    // 窗口模式改变
    private void OnFullScreenModeChanged(int index)
    {
        GameSettings.ApplyResolution(GameSettings.ResolutionWidth, GameSettings.ResolutionHeight, index);
        Debug.Log($"[Settings] 窗口模式已更改: {index}");
    }

    // 切换输入模式
    private void ToggleInputMode()
    {
        int nextMode = (GameSettings.InputMode + 1) % 2;
        GameSettings.InputMode = nextMode;
        UpdateInputModeText();
        Debug.Log($"[Settings] 输入模式已切换: {nextMode}");
    }

    // 打开音准与速度调节场景
    private void OpenOffsetSpeedScene()
    {
        Debug.Log($"[Settings] 跳转到音准与速度调节场景: {offsetSpeedSceneName}");

        // 设置返回场景为当前场景
        string currentScene = SceneManager.GetActiveScene().name;
        OffsetSpeedAdjustManager.SetReturnScene(currentScene);

        SceneManager.LoadScene(offsetSpeedSceneName);
    }

    /// <summary>
    /// 返回标题页
    /// 可以在 Inspector 中将按钮的 OnClick 事件绑定到此方法
    /// </summary>
    public void ReturnToTitle()
    {
        if (string.IsNullOrEmpty(titleSceneName))
        {
            Debug.LogWarning("[Settings] 标题页场景名称未设置！");
            return;
        }

        Debug.Log($"[Settings] 返回标题页: {titleSceneName}");

        // 保存当前设置
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }

        // 加载标题页场景
        SceneManager.LoadScene(titleSceneName);
    }

    // 更新音乐音量文本
    private void UpdateMusicVolumeText(float value)
    {
        if (musicVolumeText != null)
        {
            musicVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    // 更新音效音量文本
    private void UpdateNoteVolumeText(float value)
    {
        if (noteVolumeText != null)
        {
            noteVolumeText.text = $"{Mathf.RoundToInt(value * 100)}%";
        }
    }

    // 更新输入模式文本
    private void UpdateInputModeText()
    {
        if (inputModeText != null)
        {
            inputModeText.text = GameSettings.InputMode == 0
                ? "Preset mode"
                : "Accurate Mode";

        }
    }

    // 应用音乐音量到场景中的所有音频源
    private void ApplyMusicVolumeToScene()
    {
        // 应用到地图背景音乐
        MapBGMManager bgmManager = FindFirstObjectByType<MapBGMManager>();
        if (bgmManager != null)
        {
            bgmManager.SetVolume(GameSettings.MusicVolume);
        }

        // 应用到 LevelUIManager 的预览音乐
        if (LevelUIManager.Instance != null && LevelUIManager.Instance.previewAudioSource != null)
        {
            LevelUIManager.Instance.previewAudioSource.volume = GameSettings.MusicVolume;
        }
    }
}
