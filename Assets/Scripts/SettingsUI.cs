using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicVolumeSlider;
    public Slider noteVolumeSlider;
    public Button modeButton;
    public Text modeText;
    public Button closeButton;

    private void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(Close);
        if (modeButton) modeButton.onClick.AddListener(ToggleMode);
        
        if (musicVolumeSlider)
        {
            musicVolumeSlider.value = GameSettings.MusicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        if (noteVolumeSlider)
        {
            noteVolumeSlider.value = GameSettings.NoteVolume;
            noteVolumeSlider.onValueChanged.AddListener(OnNoteVolumeChanged);
        }
        
        UpdateModeUI();

        // Ensure it's hidden at start if it's in the scene
        gameObject.SetActive(false);
    }

    public void Open()
    {
        Debug.Log("[Settings] Opening Settings Menu");
        gameObject.SetActive(true);
        Time.timeScale = 0;

        // Refresh values from persistent settings
        if (musicVolumeSlider) musicVolumeSlider.value = GameSettings.MusicVolume;
        if (noteVolumeSlider) noteVolumeSlider.value = GameSettings.NoteVolume;
        UpdateModeUI();

        // Apply immediately
        ApplyMusicVolume();
        PauseAudio();
    }

    private void ApplyMusicVolume()
    {
        NoteManager nm = Object.FindFirstObjectByType<NoteManager>();
        if (nm != null && nm.audioSource != null)
        {
            nm.audioSource.volume = GameSettings.MusicVolume;
            Debug.Log($"[Settings] Applied Music Volume: {nm.audioSource.volume}");
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;

        // Resume audio when closing settings
        ResumeAudio();

        // Notify player controller of mode change
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.currentMode = (PlayerController.ControlMode)GameSettings.InputMode;
        }
    }

    private void ToggleMode()
    {
        int nextMode = (GameSettings.InputMode + 1) % 2;
        GameSettings.InputMode = nextMode;
        Debug.Log($"[Settings] Input Mode toggled to: {GameSettings.InputMode}");
        UpdateModeUI();
    }

    private void UpdateModeUI()
    {
        if (modeText)
        {
            modeText.text = GameSettings.InputMode == 0 ? "Mode: Pre-input (Preset)" : "Mode: Strict (Accurate)";
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        GameSettings.MusicVolume = value;
        Debug.Log($"[Settings] Music Volume changed: {value:F2}");
        ApplyMusicVolume();
    }

    private void OnNoteVolumeChanged(float value)
    {
        GameSettings.NoteVolume = value;
        Debug.Log($"[Settings] Note Volume changed: {value:F2}");
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
