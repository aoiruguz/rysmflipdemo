using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public enum ControlMode { Preset, Accurate }

    [Header("Mode Settings")]
    public ControlMode currentMode = ControlMode.Preset;

    [Header("UI Alignment (Canvas Sync)")]
    [Tooltip("Note 判定区域的 UI 参照对象（玩家接住区域）")]
    public RectTransform rectCatchPoint;
    [Tooltip("四条Lane的 UI 参照对象，对应4条竖直的游戏路线")]
    public RectTransform[] rectLaneAnchors;

    // 内部运行时坐标（由 UI Alignment 同步得出）
    [HideInInspector] public float[] laneXPositions = new float[4];
    [HideInInspector] public float catchHeight;

    [Header("Accurate Mode Settings")]
    public float accurateTimeWindow = 150f; // 150ms 时间窗口
    public float accuratePositionTolerance = 1.5f; // 位置容错范围

    [Header("Effects")]
    public RectTransform effectContainer; // 托管反弹特效的UI容器
    public GameObject effectPrefab;
    public float effectSpeed = 10f;
    public float minEffectAngle = 30f;
    public float maxEffectAngle = 60f;
    public AudioSource hitAudioSource;

    [Header("Effect Sprites (对应JKL三种Note颜色)")]
    [Tooltip("【J（color 0）】击中红色 Note 时特效显示的图片")]
    public Sprite effectSprite0_J_Red;
    [Tooltip("【L（color 1）】击中黄色 Note 时特效显示的图片")]
    public Sprite effectSprite1_L_Yellow;
    [Tooltip("【K（color 2）】击中蓝色 Note 时特效显示的图片")]
    public Sprite effectSprite2_K_Blue;

    [Header("Effect Text Preset")]
    [Tooltip("特效TMP随机文字预设文件（每行一条）")]
    public TextAsset effectTextData;
    private string[] effectMessages;

    [Header("State Sprites")]
    [Tooltip("默认状态/准确模式下的精灵图")]
    public Sprite defaultSprite;
    [Tooltip("【J（color 0）】红色状态的精灵图")]
    public Sprite sprite0_J_Red;
    [Tooltip("【L（color 1）】黄色状态的精灵图")]
    public Sprite sprite1_L_Yellow;
    [Tooltip("【K（color 2）】蓝色状态的精灵图")]
    public Sprite sprite2_K_Blue;
    [Tooltip("左划状态的精灵图 (对应A)")]
    public Sprite leftSwipeSprite;
    [Tooltip("右划状态的精灵图 (对应D)")]
    public Sprite rightSwipeSprite;

    [Header("Feedback Settings")]
    public float feedbackScaleMultiplier = 1.2f;
    public float feedbackDuration = 0.1f;
    
    [Header("Key UI Settings")]
    [Tooltip("【J（color 0）】对应的 UI 图片")]
    public Image uiImage0_J;
    [Tooltip("【L（color 1）】对应的 UI 图片")]
    public Image uiImage1_L;
    [Tooltip("【K（color 2）】对应的 UI 图片")]
    public Image uiImage2_K;
    [Range(0f, 1f)]
    [Tooltip("未激活按键的默认透明度")]
    public float inactiveAlpha = 0.5f;

    public int CurrentLane { get; private set; } = 3;
    public GameColor CurrentPresetColor { get; private set; } = GameColor.J_Color0_Red;

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

        // --- 同步 UI 坐标到 catchHeight，确保判定逻辑使用真实位置 ---
        SyncCatchHeight();

        UpdatePosition();
        UpdateVisual();
    }

    /// <summary>
    /// 将 catchHeight 同步为 UI 参照点的真实世界坐标。
    /// 这保证了 TryAccurateCatch / TryDirectionalCatch / CheckPresetCatch
    /// 中所有基于 catchHeight 的位置判定都使用正确的坐标。
    /// </summary>
    private void SyncCatchHeight()
    {
        if (rectCatchPoint != null)
        {
            catchHeight = rectCatchPoint.position.y;
            Debug.Log($"[PlayerController] catchHeight synced from UI: {catchHeight:F3}");
        }
        else
        {
            Debug.LogError("[PlayerController] rectCatchPoint is not assigned!");
        }

        if (rectLaneAnchors != null && rectLaneAnchors.Length >= 4)
        {
            for (int i = 0; i < 4; i++)
            {
                if (rectLaneAnchors[i] != null)
                {
                    laneXPositions[i] = rectLaneAnchors[i].position.x;
                }
                else
                {
                    Debug.LogError($"[PlayerController] rectLaneAnchors[{i}] is not assigned!");
                }
            }
            Debug.Log($"[PlayerController] laneXPositions synced from UI");
        }
        else
        {
            Debug.LogError("[PlayerController] rectLaneAnchors is not assigned or has insufficient elements (need 4)!");
        }
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

        // Difficulty is set, refresh visuals and Key UI
        UpdateVisual();
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

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            ToggleSettings();
        }

        if (Time.timeScale == 0) return;

        if (keyboard.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }


    }

    public void ToggleSettings()
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
            TriggerDirectionalFeedback(leftSwipeSprite);
            if (!TryDirectionalCatch(NoteType.DirectionalLeft))
            {
                // If no directional note was caught, move lane left
                CurrentLane = Mathf.Max(0, CurrentLane - 1);
            }
        }
        if (keyboard.dKey.wasPressedThisFrame)
        {
            TriggerDirectionalFeedback(rightSwipeSprite);
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

            // Easy 难度：只使用 J 键，固定为 J_Color0_Red（红色）
            if (currentDifficulty == ChartDifficulty.Easy)
            {
                if (keyboard.jKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.J_Color0_Red; colorChanged = true; }
            }
            // Normal 难度：只使用两种颜色
            else if (currentDifficulty == ChartDifficulty.Normal)
            {
                if (keyboard.jKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.J_Color0_Red; colorChanged = true; }
                if (keyboard.kKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.K_Color2_Blue; colorChanged = true; }
            }
            else // Hard 难度：使用三种颜色
            {
                if (keyboard.jKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.J_Color0_Red; colorChanged = true; }
                if (keyboard.kKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.K_Color2_Blue; colorChanged = true; }
                if (keyboard.lKey.wasPressedThisFrame) { CurrentPresetColor = GameColor.L_Color1_Yellow; colorChanged = true; }
            }

            if (colorChanged) UpdateVisual();
        }
        else // Accurate Mode
        {
            // 精确模式下 JKL 也触发临时视觉反馈
            if (currentDifficulty == ChartDifficulty.Easy)
            {
                if (keyboard.jKey.wasPressedThisFrame) { TriggerAccurateFeedback(GameColor.J_Color0_Red); TryAccurateCatch(GameColor.J_Color0_Red); }
            }
            else if (currentDifficulty == ChartDifficulty.Normal)
            {
                if (keyboard.jKey.wasPressedThisFrame) { TriggerAccurateFeedback(GameColor.J_Color0_Red); TryAccurateCatch(GameColor.J_Color0_Red); }
                if (keyboard.kKey.wasPressedThisFrame) { TriggerAccurateFeedback(GameColor.K_Color2_Blue); TryAccurateCatch(GameColor.K_Color2_Blue); }
            }
            else // Hard
            {
                if (keyboard.jKey.wasPressedThisFrame) { TriggerAccurateFeedback(GameColor.J_Color0_Red); TryAccurateCatch(GameColor.J_Color0_Red); }
                if (keyboard.kKey.wasPressedThisFrame) { TriggerAccurateFeedback(GameColor.K_Color2_Blue); TryAccurateCatch(GameColor.K_Color2_Blue); }
                if (keyboard.lKey.wasPressedThisFrame) { TriggerAccurateFeedback(GameColor.L_Color1_Yellow); TryAccurateCatch(GameColor.L_Color1_Yellow); }
            }
        }
    }

    private void TryAccurateCatch(GameColor color)
    {
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

        SpawnCatchEffect(note.transform.position, note);

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

    private void SpawnCatchEffect(Vector3 pos, Note note = null)
    {
        if (effectPrefab == null) return;

        // 懒加载文字预设
        if (effectMessages == null && effectTextData != null)
        {
            effectMessages = effectTextData.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        
        GameObject effectObj;
        if (effectContainer != null)
        {
            // 作为子物体实例化到指定的 UI Canvas 容器下
            effectObj = Instantiate(effectPrefab, effectContainer);
            effectObj.transform.position = pos; // 保持世界坐标一致
            effectObj.transform.localScale = Vector3.one; // 重置缩放
        }
        else
        {
            effectObj = Instantiate(effectPrefab, pos, Quaternion.identity);
        }

        CatchEffect effect = effectObj.GetComponent<CatchEffect>();
        if (effect != null)
        {
            // Generate angle: 50% left (60-120°), 50% right (60-120° mirrored)
            float angle = Random.Range(minEffectAngle, maxEffectAngle);
            if (Random.value > 0.5f)
            {
                angle = 180f - angle;
            }

            // 根据击中Note颜色选择对应贴图
            Sprite hitSprite = null;
            if (note != null && note.NoteType == NoteType.Color)
            {
                switch (note.Color)
                {
                    case GameColor.J_Color0_Red: hitSprite = effectSprite0_J_Red; break;
                    case GameColor.L_Color1_Yellow: hitSprite = effectSprite1_L_Yellow; break;
                    case GameColor.K_Color2_Blue: hitSprite = effectSprite2_K_Blue; break;
                }
            }

            // 随机选取文字
            string labelText = null;
            if (effectMessages != null && effectMessages.Length > 0)
            {
                labelText = effectMessages[Random.Range(0, effectMessages.Length)];
            }

            Debug.Log($"[Effect] Spawning at angle: {angle}°");
            effect.Initialize(pos, angle, effectSpeed, hitSprite, labelText);
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
        
        // 恢复默认颜色，防止被之前的代码逻辑污染
        spriteRenderer.color = Color.white;

        if (currentMode == ControlMode.Preset)
        {
            // Preset 模式：持续显示当前 JKL 状态的贴图
            spriteRenderer.sprite = GetSpriteForColor(CurrentPresetColor);
        }
        else
        {
            // Accurate 模式：默认永远是 normal 状态
            spriteRenderer.sprite = defaultSprite;
        }

        // 刷新 JKL 按键 UI 状态
        UpdateKeyUI();
    }

    /// <summary>
    /// AD键的方向反馈：临时切换贴图后恢复到当前持久状态
    /// </summary>
    private void TriggerDirectionalFeedback(Sprite sprite)
    {
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(FeedbackSequence(sprite));
    }

    /// <summary>
    /// 精确模式下 JKL/AD 的临时视觉反馈，结束后回到 defaultSprite
    /// </summary>
    private void TriggerAccurateFeedback(GameColor color)
    {
        if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
        feedbackRoutine = StartCoroutine(FeedbackSequence(GetSpriteForColor(color), color));
    }

    private System.Collections.IEnumerator FeedbackSequence(Sprite sprite, GameColor? color = null)
    {
        spriteRenderer.color = Color.white;
        spriteRenderer.sprite = sprite;
        transform.localScale = originalScale * feedbackScaleMultiplier;

        // 如果是 JKL 按键（传入了 color），则临时高亮 UI
        UpdateKeyUI(color);

        yield return new WaitForSeconds(feedbackDuration);

        transform.localScale = originalScale;
        UpdateVisual(); // 回到持久状态（Preset=JKL贴图/UI, Accurate=normal贴图/UI）
    }
    public void RefreshControlMode()
    {
        // 重新从全局设置读取模式
        currentMode = (ControlMode)GameSettings.InputMode;
        // 立即刷新显示
        UpdateVisual();
        Debug.Log($"[PlayerController] Mode refreshed to: {currentMode}");
    }
    
    private Sprite GetSpriteForColor(GameColor color)
    {
        switch (color)
        {
            case GameColor.J_Color0_Red: return sprite0_J_Red;
            case GameColor.L_Color1_Yellow: return sprite1_L_Yellow;
            case GameColor.K_Color2_Blue: return sprite2_K_Blue;
            default: return defaultSprite;
        }
    }

    /// <summary>
    /// 更新 J/K/L 按键 UI 的高亮状态
    /// </summary>
    /// <param name="overrideColor">如果有临时覆盖颜色（如按键瞬间），优先使用</param>
    private void UpdateKeyUI(GameColor? overrideColor = null)
    {
        float jAlpha = inactiveAlpha;
        float kAlpha = inactiveAlpha;
        float lAlpha = inactiveAlpha;

        GameColor? activeColor = overrideColor;

        // 如果没有临时覆盖（不是按键瞬间），且在 Preset 模式下，则使用持久的颜色状态
        if (activeColor == null && currentMode == ControlMode.Preset)
        {
            activeColor = CurrentPresetColor;
        }

        if (activeColor != null)
        {
            // 根据难度映射高亮
            if (currentDifficulty == ChartDifficulty.Easy)
            {
                if (activeColor == GameColor.J_Color0_Red) jAlpha = 1.0f;
            }
            else if (currentDifficulty == ChartDifficulty.Normal)
            {
                // Normal 模式下 J=0, K=2
                if (activeColor == GameColor.J_Color0_Red) jAlpha = 1.0f;
                else if (activeColor == GameColor.K_Color2_Blue) kAlpha = 1.0f;
            }
            else // Hard
            {
                // Hard 模式下 J=0, K=2, L=1
                if (activeColor == GameColor.J_Color0_Red) jAlpha = 1.0f;
                else if (activeColor == GameColor.K_Color2_Blue) kAlpha = 1.0f;
                else if (activeColor == GameColor.L_Color1_Yellow) lAlpha = 1.0f;
            }
        }

        if (uiImage0_J != null) SetImageAlpha(uiImage0_J, jAlpha);
        if (uiImage1_L != null) SetImageAlpha(uiImage1_L, lAlpha);
        if (uiImage2_K != null) SetImageAlpha(uiImage2_K, kAlpha);
    }

    private void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
