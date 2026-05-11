using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Chart Editor Controller - 为制谱器添加编辑功能
/// 不替换NoteManager，而是在其基础上添加编辑能力
/// </summary>
public class ChartEditorController : MonoBehaviour
{
    [Header("References")]
    public NoteManager noteManager;
    public AudioSource audioSource;

    [Header("Editor Settings")]
    public float seekStepSeconds = 1f;
    public KeyCode pauseKey = KeyCode.Space;
    public KeyCode rewindKey = KeyCode.LeftArrow;
    public KeyCode forwardKey = KeyCode.RightArrow;

    [Header("Note Editing")]
    public LayerMask noteLayer;

    private bool isPaused = false;
    private Note selectedNote = null;
    private bool isDraggingNote = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (noteManager == null)
        {
            noteManager = FindFirstObjectByType<NoteManager>();
        }

        if (audioSource == null && noteManager != null)
        {
            audioSource = noteManager.audioSource;
        }

        // Start paused in editor mode
        isPaused = true;
        Time.timeScale = 0f;

        Debug.Log("[ChartEditor] Editor ready. SPACE=pause, LEFT/RIGHT=seek, Left-click=move note, Right-click=change color");
    }

    void Update()
    {
        HandleEditorInput();
        HandleNoteSelection();
    }

    void HandleEditorInput()
    {
        // Toggle pause with Space
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }

        // Seek timeline (works in both paused and playing states)
        if (Input.GetKeyDown(rewindKey))
        {
            Seek(-seekStepSeconds);
        }

        if (Input.GetKeyDown(forwardKey))
        {
            Seek(seekStepSeconds);
        }
    }

    void HandleNoteSelection()
    {
        // Left click - select and drag note
        if (Input.GetMouseButtonDown(0))
        {
            TrySelectNote();
        }

        if (Input.GetMouseButton(0) && selectedNote != null)
        {
            DragNote();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDraggingNote && selectedNote != null)
            {
                FinalizeDrag();
            }
            isDraggingNote = false;
        }

        // Right click - change note color
        if (Input.GetMouseButtonDown(1))
        {
            Note clickedNote = GetNoteUnderMouse();
            if (clickedNote != null)
            {
                ChangeNoteColor(clickedNote);
            }
        }
    }

    void TrySelectNote()
    {
        Note clickedNote = GetNoteUnderMouse();
        if (clickedNote != null)
        {
            selectedNote = clickedNote;
            isDraggingNote = true;
            Debug.Log($"[ChartEditor] Selected note at lane {clickedNote.Lane}");
        }
    }

    void DragNote()
    {
        if (selectedNote == null) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // Snap to nearest lane
        int nearestLane = GetNearestLane(mouseWorldPos.x);
        float targetX = noteManager.laneXPositions[nearestLane];

        // Only change X (lane), keep original Y (timing)
        float originalY = selectedNote.transform.position.y;
        selectedNote.transform.position = new Vector3(targetX, originalY, 0);
    }

    void FinalizeDrag()
    {
        if (selectedNote == null) return;

        // Update note's lane based on final position
        int newLane = GetNearestLane(selectedNote.transform.position.x);

        // Use reflection to update the Lane property
        var laneField = typeof(Note).GetProperty("Lane");
        if (laneField != null)
        {
            // Lane is a property with private setter, we need to use reflection
            var backingField = typeof(Note).GetField("<Lane>k__BackingField",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (backingField != null)
            {
                backingField.SetValue(selectedNote, newLane);
            }
        }

        Debug.Log($"[ChartEditor] Moved note to lane {newLane}");
        selectedNote = null;
    }

    void ChangeNoteColor(Note note)
    {
        if (note == null) return;

        // Cycle through: ColorA -> ColorB -> ColorC -> DirectionalLeft -> DirectionalRight -> ColorA
        NoteType currentType = note.NoteType;

        if (currentType == NoteType.Color)
        {
            GameColor currentColor = note.Color;

            if (currentColor == GameColor.ColorC)
            {
                // Switch from ColorC to DirectionalLeft
                ChangeToDirectionalNote(note, NoteType.DirectionalLeft);
            }
            else
            {
                // Cycle through colors: A -> B -> C
                GameColor newColor = currentColor switch
                {
                    GameColor.ColorA => GameColor.ColorB,
                    GameColor.ColorB => GameColor.ColorC,
                    _ => GameColor.ColorA
                };

                UpdateNoteColor(note, newColor);
                Debug.Log($"[ChartEditor] Changed note color from {currentColor} to {newColor}");
            }
        }
        else if (currentType == NoteType.DirectionalLeft)
        {
            // Switch from DirectionalLeft to DirectionalRight
            ChangeToDirectionalNote(note, NoteType.DirectionalRight);
        }
        else if (currentType == NoteType.DirectionalRight)
        {
            // Switch from DirectionalRight to ColorA
            ChangeToColorNote(note, GameColor.ColorA);
        }
    }

    void UpdateNoteColor(Note note, GameColor newColor)
    {
        var colorField = typeof(Note).GetField("<Color>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (colorField != null)
        {
            colorField.SetValue(note, newColor);

            // Update visual
            var updateVisualMethod = typeof(Note).GetMethod("UpdateVisual",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (updateVisualMethod != null)
            {
                updateVisualMethod.Invoke(note, null);
            }
        }
    }

    void ChangeToDirectionalNote(Note note, NoteType directionType)
    {
        // Update NoteType
        var noteTypeField = typeof(Note).GetField("<NoteType>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (noteTypeField != null)
        {
            noteTypeField.SetValue(note, directionType);
        }

        // Update visual
        var updateVisualMethod = typeof(Note).GetMethod("UpdateVisual",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (updateVisualMethod != null)
        {
            updateVisualMethod.Invoke(note, null);
        }

        Debug.Log($"[ChartEditor] Changed note to {directionType}");
    }

    void ChangeToColorNote(Note note, GameColor color)
    {
        // Update NoteType
        var noteTypeField = typeof(Note).GetField("<NoteType>k__BackingField",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (noteTypeField != null)
        {
            noteTypeField.SetValue(note, NoteType.Color);
        }

        // Update Color
        UpdateNoteColor(note, color);

        Debug.Log($"[ChartEditor] Changed note to ColorNote with color {color}");
    }

    Note GetNoteUnderMouse()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, noteLayer);

        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Note>();
        }

        // Fallback: check all notes
        Note[] allNotes = FindObjectsByType<Note>(FindObjectsSortMode.None);
        float minDist = 0.5f; // Click tolerance
        Note closest = null;

        foreach (Note note in allNotes)
        {
            float dist = Vector2.Distance(mouseWorldPos, note.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = note;
            }
        }

        return closest;
    }

    int GetNearestLane(float xPosition)
    {
        int nearestLane = 0;
        float minDistance = Mathf.Abs(xPosition - noteManager.laneXPositions[0]);

        for (int i = 1; i < noteManager.laneXPositions.Length; i++)
        {
            float distance = Mathf.Abs(xPosition - noteManager.laneXPositions[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestLane = i;
            }
        }

        return nearestLane;
    }

    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
            Debug.Log("[ChartEditor] Paused");
        }
        else
        {
            Time.timeScale = 1f;
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.UnPause();
            }
            Debug.Log("[ChartEditor] Playing");
        }
    }

    void Seek(float deltaSeconds)
    {
        if (audioSource == null || audioSource.clip == null || noteManager == null) return;

        float newTime = Mathf.Clamp(audioSource.time + deltaSeconds, 0f, audioSource.clip.length);

        // Clear all existing notes
        Note[] allNotes = FindObjectsByType<Note>(FindObjectsSortMode.None);
        foreach (Note note in allNotes)
        {
            Destroy(note.gameObject);
        }

        // Stop the current PlayRoutine coroutine
        noteManager.StopAllCoroutines();

        // Set audio time without stopping/restarting
        audioSource.time = newTime;

        // Adjust musicStartTime so that GetCurrentSongTimeMs() returns newTime * 1000
        // GetCurrentSongTimeMs() = (Time.time - musicStartTime) * 1000
        // We want: newTime * 1000 = (Time.time - musicStartTime) * 1000
        // So: musicStartTime = Time.time - newTime
        float newMusicStartTime = Time.time - newTime;

        // Use reflection to update musicStartTime
        var musicStartTimeField = typeof(NoteManager).GetField("musicStartTime",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (musicStartTimeField != null)
        {
            musicStartTimeField.SetValue(noteManager, newMusicStartTime);
        }

        // Start a custom coroutine that continues from the current time
        noteManager.StartCoroutine(EditorPlayRoutine(newTime));

        Debug.Log($"[ChartEditor] Seeked to {newTime:F2}s");
    }

    private System.Collections.IEnumerator EditorPlayRoutine(float startTimeSeconds)
    {
        float startTimeMs = startTimeSeconds * 1000f;

        // Find the first note that should spawn after this time
        var chartField = typeof(NoteManager).GetField("currentChart",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        ChartData chart = chartField?.GetValue(noteManager) as ChartData;

        if (chart == null || chart.notes == null)
        {
            Debug.LogError("[ChartEditor] Cannot access chart data");
            yield break;
        }

        // Calculate note index to start from
        int noteIndex = 0;
        float noteTravelTimeMs = noteManager.noteTravelTime * 1000f;
        float runtimeGlobalOffsetMs = GameSettings.GlobalOffset * 1000f;

        for (int i = 0; i < chart.notes.Count; i++)
        {
            float targetSpawnTimeMs = chart.notes[i].hitTime - noteTravelTimeMs + runtimeGlobalOffsetMs;
            if (targetSpawnTimeMs < startTimeMs)
            {
                noteIndex = i + 1; // Skip this note, it should have already spawned
            }
            else
            {
                break;
            }
        }

        Debug.Log($"[ChartEditor] Starting from note index {noteIndex}/{chart.notes.Count}");

        // Resume music if not paused
        if (!isPaused && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        // Continue spawning notes from this point
        while (noteIndex < chart.notes.Count)
        {
            float currentTimeMs = noteManager.GetCurrentSongTimeMs();
            UIManager.Instance?.UpdateTimeDisplay(currentTimeMs);

            float targetSpawnTimeMs = chart.notes[noteIndex].hitTime - noteTravelTimeMs + runtimeGlobalOffsetMs;

            if (currentTimeMs >= targetSpawnTimeMs)
            {
                // Use reflection to call SpawnNoteFromData
                var spawnMethod = typeof(NoteManager).GetMethod("SpawnNoteFromData",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (spawnMethod != null)
                {
                    spawnMethod.Invoke(noteManager, new object[] { chart.notes[noteIndex] });
                }
                noteIndex++;
            }

            yield return null;
        }
    }

    void OnDestroy()
    {
        // Restore time scale
        Time.timeScale = 1f;
    }
}
