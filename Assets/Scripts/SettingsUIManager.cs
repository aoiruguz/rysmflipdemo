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
    /// <summary>
    /// 分辨率 ToggleGroup，用于实现多选一按钮
    /// </summary>
    public ToggleGroup resolutionToggleGroup;
    /// <summary>
    /// 按顺序对应 1280x720 / 1920x1080 / 2560x1440 / 3840x2160
    /// </summary>
    public Toggle[] resolutionToggles;
    private Resolution[] resolutions;

    [Header("窗口模式")]
    public Toggle fullScreenToggle;

    [Header("输入模式")]
    public Button inputModeButton;
    public TextMeshProUGUI inputModeText;

    [Header("音准与速度调节")]
    public Button offsetSpeedButton;
    public string offsetSpeedSceneName = "Middle Offest Speed Adjust";

    [Header("关闭按钮")]
    public Button closeButton;

    [Header("Collider 控制")]
    [Tooltip("展开设置面板的按钮（用于绑定屏蔽 Collider 的事件）")]
    public Button openSettingsButton;
    [Tooltip("关闭设置面板的按钮（用于绑定恢复 Collider 的事件）")]
    public Button closeSettingsButton;
    [Tooltip("打开设置面板时是否禁用所有场景中的 Collider（防止点击穿透）")]
    public bool disableCollidersOnOpen = true;

    // Collider 状态记录
    private List<Collider2D> _disabledColliders2D = new List<Collider2D>();
    private List<Collider> _disabledColliders3D = new List<Collider>();


    private void Awake()
    {
        // 设置按钮监听
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
        if (inputModeButton != null)
            inputModeButton.onClick.AddListener(ToggleInputMode);
        if (offsetSpeedButton != null)
            offsetSpeedButton.onClick.AddListener(OpenOffsetSpeedScene);

        // 绑定 Collider 控制按钮
        if (openSettingsButton != null)
            openSettingsButton.onClick.AddListener(DisableAllColliders);
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(RestoreAllColliders);

        // 设置滑块监听
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (noteVolumeSlider != null)
            noteVolumeSlider.onValueChanged.AddListener(OnNoteVolumeChanged);

        // 设置窗口模式 Toggle 监听
        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.AddListener(OnFullScreenToggleChanged);

        // 为分辨率 Toggle 绑定监听
        InitializeResolutionToggles();

        // 初始化分辨率选项
        InitializeResolutions();

        // 初始化窗口模式（不需要 Dropdown 初始化）

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

        // 分辨率设置：找到匹配的 Toggle 并激活
        if (resolutionToggles != null && resolutions != null)
        {
            int currentWidth = GameSettings.ResolutionWidth;
            int currentHeight = GameSettings.ResolutionHeight;

            for (int i = 0; i < resolutions.Length; i++)
            {
                if (i >= resolutionToggles.Length) break;
                bool isMatch = resolutions[i].width == currentWidth && resolutions[i].height == currentHeight;
                // 关闭通知以避免触发 ApplyResolution
                resolutionToggles[i].SetIsOnWithoutNotify(isMatch);
            }
        }

        // 窗口模式：选中对应窗口化，不选中对应全屏
        if (fullScreenToggle != null)
        {
            // 如果模式 == 0（Windowed），设为选中
            fullScreenToggle.SetIsOnWithoutNotify(GameSettings.FullScreenMode == 0);
        }

        // 输入模式
        UpdateInputModeText();
    }

    // 初始化分辨率数据数组（不再依赖 Dropdown）
    private void InitializeResolutions()
    {
        resolutions = new Resolution[]
        {
            CreateResolution(1280,  720),
            CreateResolution(1920, 1080),
            CreateResolution(2560, 1440),
            CreateResolution(3840, 2160)
        };
    }

    // 为分辨率 Toggle 数组绑定监听，并设置 ToggleGroup
    private void InitializeResolutionToggles()
    {
        if (resolutionToggles == null || resolutionToggles.Length == 0) return;

        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            if (resolutionToggles[i] == null) continue;

            // 确保 Toggle 加入 ToggleGroup
            if (resolutionToggleGroup != null)
                resolutionToggles[i].group = resolutionToggleGroup;

            // 用局部变量捕获索引，避免闭包陷阱
            int index = i;
            resolutionToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn) OnResolutionToggleChanged(index);
            });
        }
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

    // (已移除 InitializeFullScreenModes)

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

    // Toggle 选中时触发的分辨率切换
    private void OnResolutionToggleChanged(int index)
    {
        if (resolutions == null || index < 0 || index >= resolutions.Length) return;

        Resolution resolution = resolutions[index];
        GameSettings.ApplyResolution(resolution.width, resolution.height, GameSettings.FullScreenMode);
        Debug.Log($"[Settings] 分辨率已更改（Toggle）: {resolution.width}x{resolution.height}");
    }

    // 窗口模式切换：选中对应窗口化，未选中对应全屏
    private void OnFullScreenToggleChanged(bool isWindowed)
    {
        int mode = isWindowed ? 0 : 1; // 0 为窗口，1 为全屏
        GameSettings.ApplyResolution(GameSettings.ResolutionWidth, GameSettings.ResolutionHeight, mode);
        Debug.Log($"[Settings] 窗口模式已更改（Toggle）: {(isWindowed ? "窗口化" : "全屏")}");
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

    // ===================== Collider 控制 =====================

    /// <summary>
    /// 禁用场景中所有的 Collider（2D 和 3D）
    /// </summary>
    private void DisableAllColliders()
    {
        if (!disableCollidersOnOpen)
            return;

        _disabledColliders2D.Clear();
        _disabledColliders3D.Clear();

        // 禁用所有 2D Colliders
        Collider2D[] colliders2D = FindObjectsOfType<Collider2D>();
        foreach (var col in colliders2D)
        {
            if (col != null && col.enabled)
            {
                col.enabled = false;
                _disabledColliders2D.Add(col);
            }
        }

        // 禁用所有 3D Colliders
        Collider[] colliders3D = FindObjectsOfType<Collider>();
        foreach (var col in colliders3D)
        {
            if (col != null && col.enabled)
            {
                col.enabled = false;
                _disabledColliders3D.Add(col);
            }
        }

        Debug.Log($"[SettingsUIManager] Disabled {_disabledColliders2D.Count} Collider2D and {_disabledColliders3D.Count} Collider");
    }

    /// <summary>
    /// 恢复之前禁用的所有 Collider
    /// </summary>
    private void RestoreAllColliders()
    {
        if (!disableCollidersOnOpen)
            return;

        // 恢复所有 2D Colliders
        foreach (var col in _disabledColliders2D)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }

        // 恢复所有 3D Colliders
        foreach (var col in _disabledColliders3D)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }

        Debug.Log($"[SettingsUIManager] Restored {_disabledColliders2D.Count} Collider2D and {_disabledColliders3D.Count} Collider");

        _disabledColliders2D.Clear();
        _disabledColliders3D.Clear();
    }
}
