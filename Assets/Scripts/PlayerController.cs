using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public enum ControlMode { Preset, Accurate }

    [Header("Mode Settings")]
    public ControlMode currentMode = ControlMode.Preset;

    [Header("Lane Settings")]
    public float[] laneXPositions = new float[] { -3.75f, -1.25f, 1.25f, 3.75f };
    public float catchHeight = -4f;

    [Header("Accurate Mode Settings")]
    public float accurateTimeWindow = 150f; // 150ms 时间窗口
    public float accuratePositionTolerance = 1.5f; // 位置容错范围

    [Header("Effects")]
    public GameObject effectPrefab;
    public float effectSpeed = 10f;
    public float minEffectAngle = 30f;
    public float maxEffectAngle = 60f;
    public AudioSource hitAudioSource;

    [Header("Feedback Settings")]
    public float feedbackScaleMultiplier = 1.2f;
    public float feedbackDuration = 0.1f;

    public int CurrentLane { get; private set; } = 3;
    public GameColor CurrentPresetColor { get; private set; } = GameColor.ColorA;

    private List<Note> notesInZone = new List<Note>();
    private Vector3 originalScale;
    private Coroutine feedbackRoutine;
    private SpriteRenderer spriteRenderer;
    private ChartDifficulty currentDifficulty = ChartDifficulty.Hard; // 默认 Hard 难度

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    void Start()
    {
        // Initialize mode from settings
        currentMode = (ControlMode)GameSettings.InputMode;

        // Delay difficulty check to ensure NoteManager has initialized
        StartCoroutine(InitializeDifficulty());

        UpdatePosition();
        UpdateVisual();
    }

    private System.Collections.IEnumerator InitializeDifficulty()
    {
        // Wait one frame to ensure NoteManager has loaded the chart
        yield return null;

        // Get current chart difficulty - try multiple sources
        ChartData chart = SongSelectionManager.GetSelectedChart();

        // If not found, try to get from NoteManager
        if (chart == null)
        {
            NoteManager noteManager = Object.FindFirstObjectByType<NoteManager>();
            if (noteManager != null && noteManager.currentChart != null)
            {
                chart = noteManager.currentChart;
            }
        }

        if (chart != null)
        {
            currentDifficulty = chart.difficulty;
            Debug.Log($"[PlayerController] Current difficulty: {currentDifficulty}");
        }
        else
        {
            Debug.LogWarning("[PlayerController] Could not find chart data, using default difficulty: Hard");
            currentDifficulty = ChartDifficulty.Hard;
        }
    }

    void Update()
    {
        HandleDebugInput();
        
        if (Time.timeScale == 0) return;

        HandleLaneInput();
        HandleActionInput();
        
        if (currentMode == ControlMode.Preset)
        {
            CheckPresetCatch();
        }
        
        UpdatePosition();
    }

    private void HandleDebugInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.oKey.wasPressedThisFrame)
        {
            ToggleSettings();
        }

        if (Time.timeScale == 0) return;

        if (keyboard.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Adjust Global Offset: Up = increase (+5ms), Down = decrease (-5ms)
        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            GameSettings.AdjustOffset(0.005f);
            UIManager.Instance?.UpdateGlobalOffsetDisplay();
        }
        if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            GameSettings.AdjustOffset(-0.005f);
            UIManager.Instance?.UpdateGlobalOffsetDisplay();
        }
    }

    private void ToggleSettings()
    {
        SettingsUI settings = Object.FindFirstObjectByType<SettingsUI>(FindObjectsInactive.Include);
        if (settings != null)
        {
            if (settings.gameObject.activeSelf)
            {
                settings.Close();
            }
            else
            {
                settings.Open();
            }
        }
    }

    private void HandleLaneInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Check for directional note judgment first
        if (keyboard.aKey.wasPressedThisFrame)
        {
            if (!TryDirectionalCatch(NoteType.DirectionalLeft))
            {
                // If no directional note was caught, move lane left
                CurrentLane = Mathf.Max(0, CurrentLane - 1);
            }
        }
        if (keyboard.dKey.wasPressedThisFrame)
        {
            if (!TryDirectionalCatch(NoteType.DirectionalRight))
            {
                // If no directional note was caught, move lane right
                CurrentLane = Mathf.Min(3, CurrentLane + 1);
            }
        }
    }

    private void HandleActionInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (currentMode == ControlMode.Preset)
        {
            bool colorChanged = false;

            // Easy 难度：只使用 J 键，固定为 ColorA（红色）
            if (currentDifficulty == ChartDifficulty.Easy)
            {
                if (keyboard.jKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.ColorA; colorChanged = true; }
                // K 和 L 键在 Easy 难度下不使用
            }
            // Normal 难度：只使用两种颜色
            else if (currentDifficulty == ChartDifficulty.Normal)
            {
                if (keyboard.jKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.ColorA; colorChanged = true; } // 红色
                if (keyboard.kKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.ColorC; colorChanged = true; } // 蓝色
                // L 键在 Normal 难度下不使用
            }
            else // Hard 难度：使用三种颜色
            {
                if (keyboard.jKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.ColorA; colorChanged = true; }
                if (keyboard.kKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.ColorB; colorChanged = true; }
                if (keyboard.lKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.ColorC; colorChanged = true; }
            }

            if (colorChanged) UpdateVisual();
        }
        else // Accurate Mode
        {
            // Easy 难度：只使用 J 键
            if (currentDifficulty == ChartDifficulty.Easy)
            {
                if (keyboard.jKey.wasPressedThisFrame) TryAccurateCatch(GameColor.ColorA);
                // K 和 L 键在 Easy 难度下不使用
            }
            // Normal 难度：只使用两种颜色
            else if (currentDifficulty == ChartDifficulty.Normal)
            {
                if (keyboard.jKey.wasPressedThisFrame) TryAccurateCatch(GameColor.ColorA); // 红色
                if (keyboard.kKey.wasPressedThisFrame) TryAccurateCatch(GameColor.ColorC); // 蓝色
                // L 键在 Normal 难度下不使用
            }
            else // Hard 难度：使用三种颜色
            {
                if (keyboard.jKey.wasPressedThisFrame) TryAccurateCatch(GameColor.ColorA);
                if (keyboard.kKey.wasPressedThisFrame) TryAccurateCatch(GameColor.ColorB);
                if (keyboard.lKey.wasPressedThisFrame) TryAccurateCatch(GameColor.ColorC);
            }
        }
    }

    private void TryAccurateCatch(GameColor color)
    {
        TriggerFeedback(color);

        NoteManager nm = Object.FindFirstObjectByType<NoteManager>();
        float currentTimeMs = nm != null ? nm.GetCurrentSongTimeMs() : Time.time * 1000f;

        Note bestNote = null;
        float bestOffset = float.MaxValue;

        // 方案1: 扩大判定范围 - 检查所有note，不只是触发器区域内的
        Note[] allNotes = Object.FindObjectsByType<Note>(FindObjectsSortMode.None);
        foreach (Note note in allNotes)
        {
            if (note == null || note.Color != color || note.Lane != CurrentLane) continue;

            // 计算时间偏移（包含全局偏移）
            float targetHitTimeMs = note.HitTimeMs + (GameSettings.GlobalOffset * 1000f);
            float timeOffset = Mathf.Abs(currentTimeMs - targetHitTimeMs);

            // 计算位置偏移
            float positionOffset = Mathf.Abs(note.transform.position.y - catchHeight);

            // 在时间窗口内且位置相近的note都可以判定
            if (timeOffset <= accurateTimeWindow && positionOffset <= accuratePositionTolerance)
            {
                if (timeOffset < bestOffset)
                {
                    bestOffset = timeOffset;
                    bestNote = note;
                }
            }
        }

        if (bestNote != null)
        {
            Debug.Log($"[Accurate] 判定成功! 时间偏移: {bestOffset:F1}ms, 位置偏移: {Mathf.Abs(bestNote.transform.position.y - catchHeight):F2}");
            PerformCatch(bestNote);
        }
        else
        {
            Debug.Log($"[Accurate] 判定失败! 没有找到匹配的note. 当前时间: {currentTimeMs:F0}ms, Lane: {CurrentLane}, Color: {color}");
        }
    }

    private bool TryDirectionalCatch(NoteType directionType)
    {
        NoteManager nm = Object.FindFirstObjectByType<NoteManager>();
        float currentTimeMs = nm != null ? nm.GetCurrentSongTimeMs() : Time.time * 1000f;

        Note bestNote = null;
        float bestOffset = float.MaxValue;

        // Find all directional notes at current lane
        Note[] allNotes = Object.FindObjectsByType<Note>(FindObjectsSortMode.None);
        foreach (Note note in allNotes)
        {
            if (note == null || note.NoteType != directionType || note.Lane != CurrentLane) continue;

            // Calculate time offset
            float targetHitTimeMs = note.HitTimeMs + (GameSettings.GlobalOffset * 1000f);
            float timeOffset = Mathf.Abs(currentTimeMs - targetHitTimeMs);

            // Calculate position offset
            float positionOffset = Mathf.Abs(note.transform.position.y - catchHeight);

            // Check if within judgment window
            if (timeOffset <= accurateTimeWindow && positionOffset <= accuratePositionTolerance)
            {
                if (timeOffset < bestOffset)
                {
                    bestOffset = timeOffset;
                    bestNote = note;
                }
            }
        }

        if (bestNote != null)
        {
            Debug.Log($"[Directional] Caught {directionType} note! Time offset: {bestOffset:F1}ms");
            PerformCatch(bestNote);

            // Move player in the direction
            if (directionType == NoteType.DirectionalLeft)
            {
                CurrentLane = Mathf.Max(0, CurrentLane - 1);
            }
            else if (directionType == NoteType.DirectionalRight)
            {
                CurrentLane = Mathf.Min(3, CurrentLane + 1);
            }

            return true;
        }

        return false;
    }

    private void CheckPresetCatch()
    {
        for (int i = notesInZone.Count - 1; i >= 0; i--)
        {
            Note note = notesInZone[i];
            if (note == null)
            {
                notesInZone.RemoveAt(i);
                continue;
            }

            if (note.Color == CurrentPresetColor && note.Lane == CurrentLane)
            {
                // Calculate current offset
                NoteManager nm = Object.FindFirstObjectByType<NoteManager>();
                float currentTimeMs = nm != null ? nm.GetCurrentSongTimeMs() : Time.time * 1000f;

                // JUDGMENT FIX: The "perfect" time is now HitTime + GlobalOffset
                float targetHitTimeMs = note.HitTimeMs + (GameSettings.GlobalOffset * 1000f);
                float offset = currentTimeMs - targetHitTimeMs;

                // Triggering at offset >= 0 will ensure it's a perfect hit (0ms offset) relative to GlobalOffset
                if (offset >= 0)
                {
                    PerformCatch(note);
                }
            }
        }
    }

    private void PerformCatch(Note note)
    {
        NoteManager nm = Object.FindFirstObjectByType<NoteManager>();
        float currentTimeMs = nm != null ? nm.GetCurrentSongTimeMs() : Time.time * 1000f;

        // JUDGMENT FIX: Offset calculation must include GlobalOffset
        float targetHitTimeMs = note.HitTimeMs + (GameSettings.GlobalOffset * 1000f);
        float offset = currentTimeMs - targetHitTimeMs;

        // 准确模式使用实际偏移，预设模式使用修正后的偏移
        if (currentMode == ControlMode.Accurate)
        {
            // 准确模式：使用实际的时间偏移（可能为负数，表示提前按）
            ScoreManager.Instance?.OnCatch(offset);
        }
        else
        {
            // 预设模式：确保offset >= 0（只在时间点后按）
            ScoreManager.Instance?.OnCatch(offset);
        }

        SpawnCatchEffect(note.transform.position);

        if (hitAudioSource != null)
        {
            hitAudioSource.volume = GameSettings.NoteVolume;
            hitAudioSource.PlayOneShot(hitAudioSource.clip);
        }

        notesInZone.Remove(note);

        // 通知NoteManager note已被处理
        NoteManager noteManager = Object.FindFirstObjectByType<NoteManager>();
        if (noteManager != null)
        {
            noteManager.OnNoteProcessed();
        }

        Destroy(note.gameObject);
    }

    private void SpawnCatchEffect(Vector3 pos)
    {
        if (effectPrefab == null) return;
        GameObject effectObj = Instantiate(effectPrefab, pos, Quaternion.identity);
        NoteEffect effect = effectObj.GetComponent<NoteEffect>();
        if (effect != null)
        {
            // Generate angle: 50% left (60-120°), 50% right (60-120° mirrored)
            float angle = Random.Range(minEffectAngle, maxEffectAngle);
            // Mirror to right side (180° - angle gives symmetric bounce)
            if (Random.value > 0.5f)
            {
                angle = 180f - angle;
            }
            Debug.Log($"[Effect] Spawning at angle: {angle}°");
            effect.Initialize(pos, angle, effectSpeed);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Note note = other.GetComponent<Note>();
        if (note != null)
        {
            if (!notesInZone.Contains(note))
            {
                notesInZone.Add(note);
                Debug.Log($"[Player] Note entered zone: {note.name} in lane {note.Lane}");
            }
        }
        else
        {
            Debug.Log($"[Player] Something entered zone that is not a Note: {other.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Note note = other.GetComponent<Note>();
        if (note != null)
        {
            notesInZone.Remove(note);
            Debug.Log($"[Player] Note exited zone: {note.name}");
        }
    }

    private void UpdatePosition()
    {
        transform.position = new Vector3(laneXPositions[CurrentLane], catchHeight, 0);
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        if (currentMode == ControlMode.Preset)
        {
            spriteRenderer.color = GetUnityColor(CurrentPresetColor);
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void TriggerFeedback(GameColor color)
    {
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(FeedbackSequence(color));
    }

    private System.Collections.IEnumerator FeedbackSequence(GameColor color)
    {
        spriteRenderer.color = GetUnityColor(color);
        transform.localScale = originalScale * feedbackScaleMultiplier;

        yield return new WaitForSeconds(feedbackDuration);

        transform.localScale = originalScale;
        UpdateVisual();
    }
    public void RefreshControlMode()
    {
        // 重新从全局设置读取模式
        currentMode = (ControlMode)GameSettings.InputMode;
        // 立即刷新颜色显示
        UpdateVisual();
        Debug.Log($"[PlayerController] Mode refreshed to: {currentMode}");
    }
    private Color GetUnityColor(GameColor color)
    {
        switch (color)
        {
            case GameColor.ColorA: return Color.red;
            case GameColor.ColorB: return Color.green;
            case GameColor.ColorC: return Color.blue;
            default: return Color.white;
        }
    }
}
