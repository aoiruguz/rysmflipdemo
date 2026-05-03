using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class SongData
{
    public string songName;
    public AudioClip previewClip;
    public Sprite backgroundImage;
    public ChartData easyChart;
    public ChartData normalChart;
    public ChartData hardChart;

#if UNITY_EDITOR
    // Get background sprite from chart's directory (Editor only)
    public Sprite GetBackgroundSprite()
    {
        if (backgroundImage != null)
            return backgroundImage;

        // Try to load from chart data
        ChartData chart = hardChart ?? normalChart ?? easyChart;
        if (chart != null && chart.audioClip != null)
        {
            string audioPath = UnityEditor.AssetDatabase.GetAssetPath(chart.audioClip);
            if (!string.IsNullOrEmpty(audioPath))
            {
                string directory = Path.GetDirectoryName(audioPath);

                // Try different bg file names
                string[] bgNames = { "bg.jpg", "BG.jpg", "bg.png", "BG.png", "background.jpg", "background.png" };
                foreach (string bgName in bgNames)
                {
                    string bgPath = Path.Combine(directory, bgName).Replace("\\", "/");
                    Sprite bgSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
                    if (bgSprite != null)
                    {
                        return bgSprite;
                    }
                }
            }
        }

        return null;
    }
#else
    public Sprite GetBackgroundSprite()
    {
        return backgroundImage;
    }
#endif
}

public class SongSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI songTitleText;
    public UnityEngine.UI.Button leftButton;
    public UnityEngine.UI.Button rightButton;
    public UnityEngine.UI.Image backgroundImage;

    [Header("Songs List")]
    public List<SongData> songs = new List<SongData>();

    [Header("Audio")]
    private AudioSource audioSource;

    [Header("Scene")]
    public string gameSceneName = "SampleScene";

    private static ChartData selectedChart;
    private int currentSongIndex = 0;

    void Start()
    {
        // Setup audio source for preview
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = GameSettings.MusicVolume;

        // Setup button listeners
        if (leftButton != null)
        {
            leftButton.onClick.AddListener(OnPreviousSong);
        }
        if (rightButton != null)
        {
            rightButton.onClick.AddListener(OnNextSong);
        }

        // Load first song
        if (songs.Count > 0)
        {
            LoadSongPreview(0);
        }
        else
        {
            Debug.LogWarning("No songs configured in SongSelectionManager!");
        }
    }

    void OnDestroy()
    {
        // Stop preview when leaving scene
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void LoadSongPreview(int index)
    {
        if (index < 0 || index >= songs.Count) return;

        currentSongIndex = index;
        SongData song = songs[currentSongIndex];

        // Update title
        if (songTitleText != null)
        {
            songTitleText.text = song.songName;
        }

        // Update background image - try to load from song directory
        if (backgroundImage != null)
        {
            Sprite bgSprite = song.GetBackgroundSprite();
            if (bgSprite != null)
            {
                backgroundImage.sprite = bgSprite;
            }
            else
            {
                Debug.LogWarning($"No background image found for song: {song.songName}");
            }
        }

        // Update audio preview
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = song.previewClip;
            if (song.previewClip != null)
            {
                audioSource.Play();
            }
        }

        // Update button interactability
        if (leftButton != null)
        {
            leftButton.interactable = (currentSongIndex > 0);
        }
        if (rightButton != null)
        {
            rightButton.interactable = (currentSongIndex < songs.Count - 1);
        }
    }

    public void OnPreviousSong()
    {
        if (currentSongIndex > 0)
        {
            LoadSongPreview(currentSongIndex - 1);
        }
    }

    public void OnNextSong()
    {
        if (currentSongIndex < songs.Count - 1)
        {
            LoadSongPreview(currentSongIndex + 1);
        }
    }

    public void OnEasySelected()
    {
        if (songs.Count > 0 && currentSongIndex < songs.Count)
        {
            Debug.Log($"[SongSelection] Easy button clicked for song: {songs[currentSongIndex].songName}");
            LoadChart(songs[currentSongIndex].easyChart);
        }
    }

    public void OnNormalSelected()
    {
        if (songs.Count > 0 && currentSongIndex < songs.Count)
        {
            Debug.Log($"[SongSelection] Normal button clicked for song: {songs[currentSongIndex].songName}");
            LoadChart(songs[currentSongIndex].normalChart);
        }
    }

    public void OnHardSelected()
    {
        if (songs.Count > 0 && currentSongIndex < songs.Count)
        {
            Debug.Log($"[SongSelection] Hard button clicked for song: {songs[currentSongIndex].songName}");
            LoadChart(songs[currentSongIndex].hardChart);
        }
    }

    private void LoadChart(ChartData chart)
    {
        if (chart == null)
        {
            Debug.LogError("Chart data is null!");
            return;
        }

        selectedChart = chart;
        Debug.Log($"[SongSelection] Loading chart: {chart.songName}, Difficulty: {chart.difficulty}");

        // Stop preview audio
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Load game scene
        SceneManager.LoadScene(gameSceneName);
    }

    public static ChartData GetSelectedChart()
    {
        return selectedChart;
    }
}
