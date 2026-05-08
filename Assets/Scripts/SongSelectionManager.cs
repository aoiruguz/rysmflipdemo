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
    public UnityEngine.UI.Button easyButton;
    public UnityEngine.UI.Button normalButton;
    public UnityEngine.UI.Button hardButton;

    [Header("Fans Display")]
    public TextMeshProUGUI totalFansText; // 显示总粉丝数
    public TextMeshProUGUI chapterProgressText; // 显示章节进度

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

        // 显示粉丝数和章节进度
        UpdateFansDisplay();
    }

    /// <summary>
    /// 更新粉丝数和章节进度显示
    /// </summary>
    private void UpdateFansDisplay()
    {
        long totalFans = FansDataManager.GetTotalFans();
        int unlockedChapter = FansDataManager.GetUnlockedChapter();

        // 显示总粉丝数
        if (totalFansText != null)
        {
            totalFansText.text = $"Fans: {ScoreCalculator.FormatLargeNumber(totalFans)}";
        }

        // 显示章节进度
        if (chapterProgressText != null)
        {
            int nextChapter = unlockedChapter + 1;
            if (nextChapter < 3) // 还有下一章
            {
                long requiredFans = FansDataManager.GetRequiredFansForNextChapter(unlockedChapter);
                long remainingFans = requiredFans - totalFans;
                if (remainingFans > 0)
                {
                    chapterProgressText.text = $"Chapter {nextChapter + 1} Unlock: {ScoreCalculator.FormatLargeNumber(remainingFans)} more fans needed";
                }
                else
                {
                    chapterProgressText.text = $"Chapter {nextChapter + 1} Unlocked!";
                }
            }
            else
            {
                chapterProgressText.text = "All Chapters Unlocked!";
            }
        }

        Debug.Log($"[SongSelection] Total Fans: {ScoreCalculator.FormatLargeNumber(totalFans)}, Unlocked Chapter: {unlockedChapter + 1}");
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

        // Update difficulty button visibility
        UpdateDifficultyButtons();
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

    private void UpdateDifficultyButtons()
    {
        if (currentSongIndex < 0 || currentSongIndex >= songs.Count)
            return;

        SongData song = songs[currentSongIndex];

        // Show/hide difficulty buttons based on chart availability
        if (easyButton != null)
        {
            easyButton.gameObject.SetActive(song.easyChart != null);
        }
        if (normalButton != null)
        {
            normalButton.gameObject.SetActive(song.normalChart != null);
        }
        if (hardButton != null)
        {
            hardButton.gameObject.SetActive(song.hardChart != null);
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
