using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoteManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject notePrefab;
    public ChartData currentChart;
    public AudioSource audioSource;

    [Header("Flow Control")]
    public float noteTravelTime = 2.0f; // Seconds for note to reach judgment line
    public float prePlayDelay = 2.0f; // 2 seconds delay
    public float spawnY = 6f;
    public float catchY = -4f;
    public float missY = -6f;
    public float[] laneXPositions = new float[] { -3.75f, -1.25f, 1.25f, 3.75f };

    [Header("Difficulty Hints")]
    public GameObject hintParent; // Parent object containing Easy, Normal, Hard children

    public float CurrentSpeed { get; private set; }
    private bool isGameStarted = false;
    private float musicStartTime;
    private int totalNotes = 0;
    private int processedNotes = 0; // 已处理的note数量（击打或miss）
    private bool songCompleted = false;

    void Start()
    {
        // Check if chart was selected from song selection
        if (currentChart == null)
        {
            currentChart = SongSelectionManager.GetSelectedChart();
            Debug.Log("[NoteManager] Loaded chart from SongSelectionManager");
        }
        else
        {
            Debug.Log("[NoteManager] Using chart assigned in Inspector (ignoring SongSelectionManager)");
        }

        if (currentChart == null)
        {
            Debug.LogError("No ChartData assigned to NoteManager!");
            return;
        }

        Debug.Log($"[NoteManager] Current chart: {currentChart.songName}, Difficulty: {currentChart.difficulty}");

        // Initialize total notes count
        totalNotes = currentChart.notes.Count;
        processedNotes = 0;
        songCompleted = false;

        // Initialize HealthSystem with current difficulty
        HealthSystem.Instance?.Initialize(currentChart.difficulty);

        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = GameSettings.MusicVolume;

        // Calculate Speed = Distance / Time
float distance = Mathf.Abs(spawnY - catchY);
        CurrentSpeed = distance / noteTravelTime;

        audioSource.clip = currentChart.audioClip;

        // Activate difficulty-specific hint objects
        ActivateDifficultyHints();

        StartCoroutine(PlayRoutine());
    }

    private void ActivateDifficultyHints()
    {
        if (hintParent == null)
        {
            Debug.LogWarning("Hint parent not assigned in NoteManager!");
            return;
        }

        // Get all child objects
        Transform easyHint = hintParent.transform.Find("Easy");
        Transform normalHint = hintParent.transform.Find("Normal");
        Transform hardHint = hintParent.transform.Find("Hard");

        // Deactivate all hints first
        if (easyHint != null) easyHint.gameObject.SetActive(false);
        if (normalHint != null) normalHint.gameObject.SetActive(false);
        if (hardHint != null) hardHint.gameObject.SetActive(false);

        // Activate only the hint matching current difficulty
        switch (currentChart.difficulty)
        {
            case ChartDifficulty.Easy:
                if (easyHint != null)
                {
                    easyHint.gameObject.SetActive(true);
                    Debug.Log("Activated Easy hint");
                }
                else
                {
                    Debug.LogWarning("Easy hint object not found under Hint parent!");
                }
                break;

            case ChartDifficulty.Normal:
                if (normalHint != null)
                {
                    normalHint.gameObject.SetActive(true);
                    Debug.Log("Activated Normal hint");
                }
                else
                {
                    Debug.LogWarning("Normal hint object not found under Hint parent!");
                }
                break;

            case ChartDifficulty.Hard:
                if (hardHint != null)
                {
                    hardHint.gameObject.SetActive(true);
                    Debug.Log("Activated Hard hint");
                }
                else
                {
                    Debug.LogWarning("Hard hint object not found under Hint parent!");
                }
                break;
        }
    }

    private IEnumerator PlayRoutine()
    {
        // 1. Show Song Title
        UIManager.Instance?.ShowSongTitle(currentChart.songName, prePlayDelay + 1f);

        // 2. Setup Timing
        musicStartTime = Time.time + prePlayDelay;
        isGameStarted = true;

        int noteIndex = 0;
        List<NoteData> notes = currentChart.notes;
        bool musicPlayed = false;

        while (noteIndex < notes.Count || !musicPlayed)
        {
            float currentTimeMs = GetCurrentSongTimeMs();
            UIManager.Instance?.UpdateTimeDisplay(currentTimeMs);

            // Start music exactly at 0ms
if (!musicPlayed && currentTimeMs >= 0)
            {
                audioSource.Play();
                musicPlayed = true;
            }

            // Spawn notes (handles negative spawn times)
            if (noteIndex < notes.Count)
            {
                float runtimeGlobalOffsetMs = GameSettings.GlobalOffset * 1000f;
                float targetSpawnTimeMs = notes[noteIndex].hitTime - (noteTravelTime * 1000f) + runtimeGlobalOffsetMs;

                if (currentTimeMs >= targetSpawnTimeMs)
                {
                    Debug.Log($"[NoteManager] Spawning Note {noteIndex}. HitTime: {notes[noteIndex].hitTime}ms, TargetSpawn: {targetSpawnTimeMs:F0}ms, SongTime: {currentTimeMs:F0}ms");
                    SpawnNoteFromData(notes[noteIndex]);
                    noteIndex++;
                }
            }

            yield return null;
        }

        // PlayRoutine结束，但不自动跳转
        // 跳转由OnSongCompleted()处理（当所有note被处理后）
        Debug.Log("[NoteManager] PlayRoutine finished. Waiting for all notes to be processed...");
    }

    public float GetCurrentSongTimeMs()
    {
        if (!isGameStarted) return -999999;
        return (Time.time - musicStartTime) * 1000f;
    }

    private void SpawnNoteFromData(NoteData data)
    {
        if (notePrefab == null)
        {
            Debug.LogError($"[NoteManager] Note prefab not assigned!");
            return;
        }

        Vector3 spawnPos = new Vector3(laneXPositions[data.lane], spawnY, 0);
        GameObject noteObj = Instantiate(notePrefab, spawnPos, Quaternion.identity);

        Note note = noteObj.GetComponent<Note>();
        if (note == null) note = noteObj.AddComponent<Note>();

        Rigidbody2D rb = noteObj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = noteObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        // Initialize based on note type
        if (data.noteType == NoteType.Color)
        {
            note.Initialize(data.lane, data.color, data.hitTime, CurrentSpeed, missY, (n) => {
                OnNoteProcessed();
                ScoreManager.Instance?.OnMiss();
            });
        }
        else
        {
            note.InitializeDirectional(data.lane, data.noteType, data.hitTime, CurrentSpeed, missY, (n) => {
                OnNoteProcessed();
                ScoreManager.Instance?.OnMiss();
            });
        }
    }

    /// <summary>
    /// 当note被处理时调用（击打或miss）
    /// </summary>
    public void OnNoteProcessed()
    {
        processedNotes++;
        Debug.Log($"[NoteManager] Note processed: {processedNotes}/{totalNotes}");

        // 检查是否所有note都已处理
        if (processedNotes >= totalNotes && !songCompleted)
        {
            songCompleted = true;
            StartCoroutine(OnSongCompleted());
        }
    }

    /// <summary>
    /// 歌曲完成后的处理
    /// </summary>
    private IEnumerator OnSongCompleted()
    {
        Debug.Log("[NoteManager] All notes processed! Song completed!");

        // 收集最终数据
        if (PlayDataCollector.Instance != null)
        {
            PlayDataCollector.Instance.CollectFinalData();
        }

        // 显示结算文本
        string resultText = "Song Clear!";
        PlayData data = PlayDataCollector.Instance?.CurrentPlayData;

        if (data != null)
        {
            if (data.IsAllPerfect())
            {
                resultText = "All Perfect!!";
            }
            else if (data.IsFullCombo())
            {
                resultText = "Full Combo!";
            }
        }

        UIManager.Instance?.ShowSongClearText(resultText);

        // 等待3秒
        yield return new WaitForSeconds(3f);

        // 跳转到结算界面
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game Over");
    }
}
