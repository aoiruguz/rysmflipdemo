using UnityEngine;

public static class GameSettings
{
    private const string OffsetKey = "GameOffset";
    private const string MusicVolumeKey = "MusicVolume";
    private const string NoteVolumeKey = "NoteVolume";
    private const string InputModeKey = "InputMode";

    public static float GlobalOffset
    {
        get => PlayerPrefs.GetFloat(OffsetKey, 0f);
        set
        {
            PlayerPrefs.SetFloat(OffsetKey, value);
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

    public static void AdjustOffset(float delta)
    {
        GlobalOffset += delta;
        Debug.Log($"Global Offset adjusted to: {GlobalOffset:F3}s");
    }
}
