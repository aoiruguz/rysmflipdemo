using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 制谱器主控制器
/// 负责加载谱面、生成所有note、播放控制、保存功能
/// </summary>
public class ChartEditorManager : MonoBehaviour
{
    public static ChartEditorManager Instance { get; private set; }

    [Header("Chart Data")]
    public ChartData currentChart;

    [Header("Prefabs")]
    public GameObject editorNotePrefab; // 可编辑的note预制体

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Camera")]
    public Camera editorCamera;
    public EditorCameraController cameraController;

    [Header("Layout Settings")]
    public float[] laneXPositions = new float[] { -3.75f, -1.25f, 1.25f, 3.75f };
    public float timeToYScale = 10f; // 时间到Y轴的缩放比例（1秒 = 10单位）
    public float judgmentLineY = -4f; // 判定线Y位置（固定在屏幕下方）

    [Header("Snap Settings")]
    public bool snapEnabled = true;
    public float snapInterval = 0.25f; // 吸附间隔（秒），默认1/4拍

    [Header("UI")]
    public ChartEditorUI editorUI;
    public ChartFileSelector fileSelector; // 文件选择器（用于build后）

    [Header("Judgment")]
    public EditorJudgmentDetector judgmentDetector;

    // 运行时数据
    private List<EditorNote> allEditorNotes = new List<EditorNote>();
    private EditorNote selectedNote = null;
    private bool isPlaying = false;
    private float playStartTime = 0f;

    // 公共属性
    public bool IsPlaying => isPlaying;
    public EditorNote SelectedNote => selectedNote;
    public int TotalNotes => allEditorNotes.Count;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 加载谱面
        if (currentChart == null)
        {
            currentChart = SongSelectionManager.GetSelectedChart();
        }

        // 如果还是没有谱面，检查是否有外部加载的谱面
        if (currentChart == null && ExternalChartLoader.Instance != null)
        {
            currentChart = ExternalChartLoader.Instance.GetLoadedChart();
        }

        if (currentChart == null)
        {
            Debug.LogWarning("[ChartEditor] No chart data assigned! Waiting for external chart load...");
            // 初始化UI（即使没有谱面也要初始化，以便显示加载按钮）
            if (editorUI != null)
            {
                editorUI.Initialize(this);
            }
            return;
        }

        InitializeWithChart();
    }

    /// <summary>
    /// 使用谱面数据初始化编辑器
    /// </summary>
    private void InitializeWithChart()
    {
        if (currentChart == null)
        {
            Debug.LogError("[ChartEditor] Cannot initialize without chart data!");
            return;
        }

        Debug.Log($"[ChartEditor] InitializeWithChart called for: {currentChart.songName}");
        Debug.Log($"[ChartEditor] Chart has {currentChart.notes.Count} notes");
        Debug.Log($"[ChartEditor] AudioClip: {(currentChart.audioClip != null ? currentChart.audioClip.name : "NULL")}");

        // 初始化音频
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("[ChartEditor] Created AudioSource");
        }

        audioSource.clip = currentChart.audioClip;
        audioSource.playOnAwake = false;
        audioSource.volume = ChartEditorSettings.MusicVolume;

        if (audioSource.clip != null)
        {
            Debug.Log($"[ChartEditor] AudioSource clip set: {audioSource.clip.name}, length: {audioSource.clip.length}s");
        }
        else
        {
            Debug.LogWarning("[ChartEditor] AudioSource clip is NULL!");
        }

        // 初始化镜头控制器
        if (cameraController == null)
        {
            cameraController = editorCamera.GetComponent<EditorCameraController>();
            if (cameraController == null)
            {
                cameraController = editorCamera.gameObject.AddComponent<EditorCameraController>();
                Debug.Log("[ChartEditor] Created EditorCameraController");
            }
        }
        cameraController.Initialize(this);
        Debug.Log("[ChartEditor] CameraController initialized");

        // 生成所有note
        GenerateAllNotes();
        Debug.Log($"[ChartEditor] Generated {allEditorNotes.Count} notes");

        // 初始化UI
        if (editorUI != null)
        {
            editorUI.Initialize(this);
            Debug.Log("[ChartEditor] UI initialized");
        }
        else
        {
            Debug.LogWarning("[ChartEditor] editorUI is null!");
        }

        Debug.Log($"[ChartEditor] Initialization complete: {currentChart.songName}, Total notes: {allEditorNotes.Count}");
    }

    void Update()
    {
        // 播放模式下更新镜头位置
        if (isPlaying && audioSource.isPlaying)
        {
            float currentTime = audioSource.time;
            cameraController.SetCameraPositionByTime(currentTime);

            // 更新UI
            if (editorUI != null)
            {
                editorUI.UpdateTimeDisplay(currentTime);
            }
        }

        // 快捷键：空格键切换播放/暂停
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayPause();
        }

        // 快捷键：调整偏移（只在暂停时）
        if (!isPlaying)
        {
            // + 键：增加偏移（note向上移动）
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                ChartEditorSettings.AdjustOffset(0.01f);
                RegenerateAllNotes();
            }
            // - 键：减少偏移（note向下移动）
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                ChartEditorSettings.AdjustOffset(-0.01f);
                RegenerateAllNotes();
            }
        }
    }

    /// <summary>
    /// 重新生成所有note（应用新的偏移）
    /// </summary>
    public void RegenerateAllNotes()
    {
        GenerateAllNotes();

        // 更新相机控制器的maxY以适应新的offset
        if (cameraController != null)
        {
            cameraController.UpdateMaxY();
        }
    }

    /// <summary>
    /// 生成所有note到场景中
    /// </summary>
    private void GenerateAllNotes()
    {
        if (currentChart == null || currentChart.notes == null)
        {
            Debug.LogWarning("[ChartEditor] No notes to generate!");
            return;
        }

        // 清空现有note
        foreach (var note in allEditorNotes)
        {
            if (note != null)
            {
                Destroy(note.gameObject);
            }
        }
        allEditorNotes.Clear();

        // 生成所有note
        for (int i = 0; i < currentChart.notes.Count; i++)
        {
            NoteData noteData = currentChart.notes[i];
            CreateEditorNote(noteData, i);
        }

        Debug.Log($"[ChartEditor] Generated {allEditorNotes.Count} notes");
    }

    /// <summary>
    /// 创建一个可编辑的note
    /// </summary>
    private EditorNote CreateEditorNote(NoteData noteData, int dataIndex)
    {
        if (editorNotePrefab == null)
        {
            Debug.LogError("[ChartEditor] editorNotePrefab is not assigned!");
            return null;
        }

        // 计算位置
        float xPos = laneXPositions[noteData.lane];
        float yPos = TimeToYPosition(noteData.hitTime / 1000f); // hitTime是毫秒，转换为秒
        Vector3 position = new Vector3(xPos, yPos, 0);

        // 实例化note
        GameObject noteObj = Instantiate(editorNotePrefab, position, Quaternion.identity);
        noteObj.name = $"EditorNote_{dataIndex}_{noteData.hitTime}ms";

        // 初始化EditorNote组件
        EditorNote editorNote = noteObj.GetComponent<EditorNote>();
        if (editorNote == null)
        {
            editorNote = noteObj.AddComponent<EditorNote>();
        }

        editorNote.Initialize(this, noteData, dataIndex);
        allEditorNotes.Add(editorNote);

        return editorNote;
    }

    /// <summary>
    /// 播放/暂停切换
    /// </summary>
    public void TogglePlayPause()
    {
        if (isPlaying)
        {
            Pause();
        }
        else
        {
            Play();
        }
    }

    /// <summary>
    /// 播放
    /// </summary>
    public void Play()
    {
        if (audioSource == null)
        {
            Debug.LogError("[ChartEditor] AudioSource is null!");
            return;
        }

        if (audioSource.clip == null)
        {
            Debug.LogError("[ChartEditor] AudioSource.clip is null!");
            return;
        }

        isPlaying = true;
        audioSource.Play();
        playStartTime = Time.time;

        Debug.Log($"[ChartEditor] Playing: {audioSource.clip.name}, time: {audioSource.time}");
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
        audioSource.Pause();

        Debug.Log("[ChartEditor] Paused");
        Debug.LogWarning($"[ChartEditor] Pause called from: {System.Environment.StackTrace}");
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        audioSource.Stop();
        audioSource.time = 0f;
        cameraController.SetCameraPositionByTime(0f);

        if (editorUI != null)
        {
            editorUI.UpdateTimeDisplay(0f);
        }

        // 重置判定状态
        if (judgmentDetector != null)
        {
            judgmentDetector.ResetJudgment();
        }

        Debug.Log("[ChartEditor] Stopped");
    }

    /// <summary>
    /// 设置音乐时间（用于拖动进度条或滚轮控制）
    /// </summary>
    public void SetMusicTime(float time)
    {
        time = Mathf.Clamp(time, 0f, audioSource.clip.length);
        audioSource.time = time;

        if (editorUI != null)
        {
            editorUI.UpdateTimeDisplay(time);
        }
    }

    /// <summary>
    /// 获取当前音乐时间
    /// </summary>
    public float GetCurrentTime()
    {
        return audioSource.time;
    }

    /// <summary>
    /// 获取音乐总时长
    /// </summary>
    public float GetTotalDuration()
    {
        return audioSource.clip != null ? audioSource.clip.length : 0f;
    }

    /// <summary>
    /// 时间转换为Y坐标（应用偏移）
    /// </summary>
    public float TimeToYPosition(float timeInSeconds)
    {
        // 应用Editor专用偏移
        float adjustedTime = timeInSeconds + ChartEditorSettings.EditorOffset;
        return adjustedTime * timeToYScale;
    }

    /// <summary>
    /// Y坐标转换为时间（应用偏移）
    /// </summary>
    public float YPositionToTime(float yPos)
    {
        float time = yPos / timeToYScale;
        // 减去偏移
        return time - ChartEditorSettings.EditorOffset;
    }

    /// <summary>
    /// X坐标转换为Lane索引
    /// </summary>
    public int XPositionToLane(float xPos)
    {
        int closestLane = 0;
        float minDistance = Mathf.Abs(xPos - laneXPositions[0]);

        for (int i = 1; i < laneXPositions.Length; i++)
        {
            float distance = Mathf.Abs(xPos - laneXPositions[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestLane = i;
            }
        }

        return closestLane;
    }

    /// <summary>
    /// Lane索引转换为X坐标
    /// </summary>
    public float LaneToXPosition(int lane)
    {
        if (lane < 0 || lane >= laneXPositions.Length)
        {
            return laneXPositions[0];
        }
        return laneXPositions[lane];
    }

    /// <summary>
    /// 应用吸附（如果启用）
    /// </summary>
    public float ApplySnap(float timeInSeconds)
    {
        if (!snapEnabled) return timeInSeconds;

        float snappedTime = Mathf.Round(timeInSeconds / snapInterval) * snapInterval;
        return snappedTime;
    }

    /// <summary>
    /// 选中一个note
    /// </summary>
    public void SelectNote(EditorNote note)
    {
        // 取消之前的选中
        if (selectedNote != null)
        {
            selectedNote.SetSelected(false);
        }

        selectedNote = note;

        if (selectedNote != null)
        {
            selectedNote.SetSelected(true);
        }

        // 更新UI
        if (editorUI != null)
        {
            editorUI.UpdateSelectedNoteInfo(selectedNote);
        }
    }

    /// <summary>
    /// 取消选中
    /// </summary>
    public void DeselectNote()
    {
        SelectNote(null);
    }

    /// <summary>
    /// 删除选中的note
    /// </summary>
    public void DeleteSelectedNote()
    {
        if (selectedNote == null) return;

        int dataIndex = selectedNote.DataIndex;

        // 从列表中移除
        allEditorNotes.Remove(selectedNote);

        // 从ChartData中移除
        if (dataIndex >= 0 && dataIndex < currentChart.notes.Count)
        {
            currentChart.notes.RemoveAt(dataIndex);
        }

        // 销毁GameObject
        Destroy(selectedNote.gameObject);
        selectedNote = null;

        // 更新所有note的dataIndex
        RefreshNoteIndices();

        // 更新UI
        if (editorUI != null)
        {
            editorUI.UpdateSelectedNoteInfo(null);
        }

        Debug.Log($"[ChartEditor] Deleted note, remaining: {allEditorNotes.Count}");
    }

    /// <summary>
    /// 添加新note
    /// </summary>
    public void AddNote(float timeInSeconds, int lane, NoteType noteType, GameColor color = GameColor.J_Color0_Red)
    {
        // 应用吸附
        timeInSeconds = ApplySnap(timeInSeconds);

        // 创建NoteData
        NoteData newNoteData = new NoteData
        {
            hitTime = timeInSeconds * 1000f, // 转换为毫秒
            lane = lane,
            noteType = noteType,
            color = color,
            spawnTime = 0f, // 编辑器模式不需要
            duration = 0f
        };

        // 添加到ChartData
        currentChart.notes.Add(newNoteData);

        // 按时间排序
        currentChart.notes = currentChart.notes.OrderBy(n => n.hitTime).ToList();

        // 重新生成所有note（简单方式）
        GenerateAllNotes();

        Debug.Log($"[ChartEditor] Added note at {timeInSeconds:F2}s, lane {lane}");
    }

    /// <summary>
    /// 刷新所有note的dataIndex
    /// </summary>
    private void RefreshNoteIndices()
    {
        // 按时间排序ChartData
        currentChart.notes = currentChart.notes.OrderBy(n => n.hitTime).ToList();

        // 重新生成所有note
        GenerateAllNotes();
    }

    /// <summary>
    /// 加载外部谱面文件
    /// </summary>
    public void LoadExternalChart()
    {
        Debug.Log("[ChartEditor] LoadExternalChart called");

        if (ExternalChartLoader.Instance == null)
        {
            // 创建ExternalChartLoader实例
            GameObject loaderObj = new GameObject("ExternalChartLoader");
            loaderObj.AddComponent<ExternalChartLoader>();
            Debug.Log("[ChartEditor] Created ExternalChartLoader instance");
        }

#if UNITY_EDITOR
        Debug.Log("[ChartEditor] Running in Editor mode");
        // Editor模式：使用文件浏览器
        ExternalChartLoader.Instance.OpenFileBrowser(
            onChartLoaded: (ChartData loadedChart) =>
            {
                Debug.Log($"[ChartEditor] Chart loaded callback: {loadedChart.songName}");
                currentChart = loadedChart;
                InitializeWithChart();
                Debug.Log($"[ChartEditor] External chart loaded successfully: {loadedChart.songName}");
            },
            onError: (string error) =>
            {
                Debug.LogError($"[ChartEditor] Failed to load external chart: {error}");
            }
        );
#else
        Debug.Log("[ChartEditor] Running in Build mode");
        // Build模式：使用文件选择器UI
        if (fileSelector != null)
        {
            Debug.Log("[ChartEditor] FileSelector found, showing selector");
            fileSelector.ShowSelector(
                onFileSelected: (string filePath) =>
                {
                    Debug.Log($"[ChartEditor] File selected: {filePath}");
                    ExternalChartLoader.Instance.LoadChartFromPath(
                        filePath,
                        onChartLoaded: (ChartData loadedChart) =>
                        {
                            Debug.Log($"[ChartEditor] Chart loaded callback: {loadedChart.songName}");
                            currentChart = loadedChart;
                            InitializeWithChart();
                            Debug.Log($"[ChartEditor] External chart loaded successfully: {loadedChart.songName}");
                        },
                        onError: (string error) =>
                        {
                            Debug.LogError($"[ChartEditor] Failed to load external chart: {error}");
                        }
                    );
                },
                onCancelled: () =>
                {
                    Debug.Log("[ChartEditor] File selection cancelled");
                }
            );
        }
        else
        {
            Debug.LogError("[ChartEditor] ChartFileSelector is not assigned!");

            // 尝试直接加载（不使用UI）
            Debug.Log("[ChartEditor] Attempting direct load without UI");
            ExternalChartLoader.Instance.OpenFileBrowser(
                onChartLoaded: (ChartData loadedChart) =>
                {
                    Debug.Log($"[ChartEditor] Chart loaded callback: {loadedChart.songName}");
                    currentChart = loadedChart;
                    InitializeWithChart();
                    Debug.Log($"[ChartEditor] External chart loaded successfully: {loadedChart.songName}");
                },
                onError: (string error) =>
                {
                    Debug.LogError($"[ChartEditor] Failed to load external chart: {error}");
                }
            );
        }
#endif
    }

    /// <summary>
    /// 保存谱面到外部JSON文件
    /// </summary>
    public void SaveChartToExternalFile()
    {
        if (currentChart == null)
        {
            Debug.LogError("[ChartEditor] No chart to save!");
            return;
        }

#if UNITY_EDITOR
        string filePath = UnityEditor.EditorUtility.SaveFilePanel(
            "保存谱面文件",
            "",
            currentChart.songName + ".json",
            "json"
        );

        if (!string.IsNullOrEmpty(filePath))
        {
            if (ExternalChartLoader.Instance == null)
            {
                GameObject loaderObj = new GameObject("ExternalChartLoader");
                loaderObj.AddComponent<ExternalChartLoader>();
            }

            ExternalChartLoader.Instance.SaveChartToFile(currentChart, filePath);
            Debug.Log($"[ChartEditor] Chart saved to: {filePath}");
        }
#else
        Debug.LogWarning("[ChartEditor] External save is only available in Unity Editor!");
#endif
    }

    /// <summary>
    /// 保存谱面到Asset文件
    /// </summary>
    public void SaveChart()
    {
#if UNITY_EDITOR
        if (currentChart == null)
        {
            Debug.LogError("[ChartEditor] No chart to save!");
            return;
        }

        // 标记为脏数据
        UnityEditor.EditorUtility.SetDirty(currentChart);

        // 保存Asset
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();

        Debug.Log($"[ChartEditor] Chart saved: {currentChart.name}");

        if (editorUI != null)
        {
            editorUI.ShowSaveConfirmation();
        }
#else
        Debug.LogWarning("[ChartEditor] Save is only available in Unity Editor!");
#endif
    }

    /// <summary>
    /// 播放note打击音效
    /// </summary>
    public void PlayNoteHitSound()
    {
        // 使用ScoreManager的音效系统
        if (ScoreManager.Instance != null)
        {
            //ScoreManager.Instance.PlayHitSound();
        }
    }
}
