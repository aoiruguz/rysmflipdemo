using UnityEngine;

public static class GameSettings
{
    private const string OffsetKey = "GameOffset";
    private const string MusicVolumeKey = "MusicVolume";
    private const string NoteVolumeKey = "NoteVolume";
    private const string InputModeKey = "InputMode";
    private const string TravelTimeKey = "NoteTravelTime";

    // 新增：分辨率和窗口模式
    private const string ResolutionWidthKey = "ResolutionWidth";
    private const string ResolutionHeightKey = "ResolutionHeight";
    private const string FullScreenModeKey = "FullScreenMode";

    public static float GlobalOffset
    {
        get => PlayerPrefs.GetFloat(OffsetKey, 0f);
        set
        {
            PlayerPrefs.SetFloat(OffsetKey, value);
            PlayerPrefs.Save();
        }
    }

    public static float NoteTravelTime
    {
        get => PlayerPrefs.GetFloat(TravelTimeKey, 2.0f);
        set
        {
            float safeValue = Mathf.Max(0.1f, value);
            PlayerPrefs.SetFloat(TravelTimeKey, safeValue);
            PlayerPrefs.Save();
        }
    }

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
            PlayerPrefs.Save();
        }
    }

    public static float NoteVolume
    {
        get => PlayerPrefs.GetFloat(NoteVolumeKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(NoteVolumeKey, value);
            PlayerPrefs.Save();
        }
    }

    public static int InputMode
    {
        get => PlayerPrefs.GetInt(InputModeKey, 1); // Default to Accurate (1)
        set
        {
            PlayerPrefs.SetInt(InputModeKey, value);
            PlayerPrefs.Save();
        }
    }

    // 分辨率宽度
    public static int ResolutionWidth
    {
        get => PlayerPrefs.GetInt(ResolutionWidthKey, Screen.currentResolution.width);
        set
        {
            PlayerPrefs.SetInt(ResolutionWidthKey, value);
            PlayerPrefs.Save();
        }
    }

    // 分辨率高度
    public static int ResolutionHeight
    {
        get => PlayerPrefs.GetInt(ResolutionHeightKey, Screen.currentResolution.height);
        set
        {
            PlayerPrefs.SetInt(ResolutionHeightKey, value);
            PlayerPrefs.Save();
        }
    }

    // 全屏模式 (0=窗口化, 1=全屏, 2=无边框窗口)
    public static int FullScreenMode
    {
        get => PlayerPrefs.GetInt(FullScreenModeKey, 1); // 默认全屏
        set
        {
            PlayerPrefs.SetInt(FullScreenModeKey, value);
            PlayerPrefs.Save();
        }
    }

    public static void AdjustOffset(float delta)
    {
        GlobalOffset += delta;
        Debug.Log($"Global Offset adjusted to: {GlobalOffset:F3}s");
    }

    public static void AdjustTravelTime(float delta)
    {
        NoteTravelTime += delta;
        Debug.Log($"Global Note Travel Time adjusted to: {NoteTravelTime:F2}s");
    }

    // 应用分辨率设置
    public static void ApplyResolution(int width, int height, int fullScreenMode)
    {
        ResolutionWidth = width;
        ResolutionHeight = height;
        FullScreenMode = fullScreenMode;

        FullScreenMode mode = fullScreenMode switch
        {
            0 => UnityEngine.FullScreenMode.Windowed,
            1 => UnityEngine.FullScreenMode.ExclusiveFullScreen,
            2 => UnityEngine.FullScreenMode.FullScreenWindow,
            _ => UnityEngine.FullScreenMode.ExclusiveFullScreen
        };

        Screen.SetResolution(width, height, mode);
        Debug.Log($"Resolution applied: {width}x{height}, Mode: {mode}");
    }

    // 初始化时应用保存的设置
    public static void LoadAndApplySettings()
    {
        ApplyResolution(ResolutionWidth, ResolutionHeight, FullScreenMode);
    }
}
