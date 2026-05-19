using UnityEngine;
using UnityEngine.UI;

public class SongSelectionSettingsUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider musicVolumeSlider;
    public Slider noteVolumeSlider;
    public Button modeButton;
    public Text modeText;
    public Button closeButton;

    private AudioSource previewAudioSource;

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

        // Ensure it's hidden at start
        gameObject.SetActive(false);
    }

    public void Open()
    {
        Debug.Log("[SongSelection Settings] Opening Settings Menu");
        gameObject.SetActive(true);

        // Find the preview audio source
        SongSelectionManager manager = Object.FindFirstObjectByType<SongSelectionManager>();
        if (manager != null)
        {
            previewAudioSource = manager.GetComponent<AudioSource>();
        }

        // Refresh values from persistent settings
        if (musicVolumeSlider) musicVolumeSlider.value = GameSettings.MusicVolume;
        if (noteVolumeSlider) noteVolumeSlider.value = GameSettings.NoteVolume;
        UpdateModeUI();

        // Apply music volume immediately to preview
        ApplyMusicVolume();
    }

    private void ApplyMusicVolume()
    {
        if (previewAudioSource != null)
        {
            previewAudioSource.volume = GameSettings.MusicVolume;
            Debug.Log($"[SongSelection Settings] Applied Music Volume: {previewAudioSource.volume}");
        }
    }

    public void Close()
    {
        Debug.Log("[SongSelection Settings] Closing Settings Menu");
        gameObject.SetActive(false);
    }

    private void ToggleMode()
    {
        int nextMode = (GameSettings.InputMode + 1) % 2;
        GameSettings.InputMode = nextMode;
        Debug.Log($"[SongSelection Settings] Input Mode toggled to: {GameSettings.InputMode}");
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
        Debug.Log($"[SongSelection Settings] Music Volume changed: {value:F2}");
        ApplyMusicVolume();
    }

    private void OnNoteVolumeChanged(float value)
    {
        GameSettings.NoteVolume = value;
        Debug.Log($"[SongSelection Settings] Note Volume changed: {value:F2}");
    }
}
