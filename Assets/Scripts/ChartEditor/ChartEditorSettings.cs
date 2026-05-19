using UnityEngine;

/// <summary>
/// Chart Editor专用设置
/// 独立于游戏设置，用于制谱器的调试和预览
/// </summary>
public static class ChartEditorSettings
{
    private const string EditorOffsetKey = "ChartEditorOffset";
    private const string EditorJudgmentLineYKey = "ChartEditorJudgmentLineY";
    private const string EditorMusicVolumeKey = "ChartEditorMusicVolume";
    private const string EditorHitSoundVolumeKey = "ChartEditorHitSoundVolume";

    /// <summary>
    /// 制谱器专用偏移（秒）
    /// 用于调整音乐和谱面的同步
    /// </summary>
    public static float EditorOffset
    {
        get => PlayerPrefs.GetFloat(EditorOffsetKey, 0f);
        set
        {
            PlayerPrefs.SetFloat(EditorOffsetKey, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 判定线相对于镜头的Y偏移
    /// </summary>
    public static float JudgmentLineOffsetY
    {
        get => PlayerPrefs.GetFloat(EditorJudgmentLineYKey, -4f);
        set
        {
            PlayerPrefs.SetFloat(EditorJudgmentLineYKey, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 制谱器音乐音量
    /// </summary>
    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(EditorMusicVolumeKey, 0.8f);
        set
        {
            PlayerPrefs.SetFloat(EditorMusicVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 制谱器打击音效音量
    /// </summary>
    public static float HitSoundVolume
    {
        get => PlayerPrefs.GetFloat(EditorHitSoundVolumeKey, 0.5f);
        set
        {
            PlayerPrefs.SetFloat(EditorHitSoundVolumeKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 调整偏移
    /// </summary>
    public static void AdjustOffset(float delta)
    {
        EditorOffset += delta;
        Debug.Log($"[Chart Editor] Offset adjusted to: {EditorOffset:F3}s");
    }

    /// <summary>
    /// 调整判定线位置
    /// </summary>
    public static void AdjustJudgmentLineY(float delta)
    {
        JudgmentLineOffsetY += delta;
        Debug.Log($"[Chart Editor] Judgment Line Y adjusted to: {JudgmentLineOffsetY:F2}");
    }

    /// <summary>
    /// 重置所有设置为默认值
    /// </summary>
    public static void ResetToDefaults()
    {
        EditorOffset = 0f;
        JudgmentLineOffsetY = -4f;
        MusicVolume = 0.8f;
        HitSoundVolume = 0.5f;
        Debug.Log("[Chart Editor] Settings reset to defaults");
    }
}
