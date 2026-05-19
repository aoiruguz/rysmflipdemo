using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 制谱器UI控制器
/// 管理所有UI元素和交互
/// </summary>
public class ChartEditorUI : MonoBehaviour
{
    private ChartEditorManager manager;

    [Header("Playback Controls")]
    public Button playPauseButton;
    public TextMeshProUGUI playPauseButtonText;
    public Button stopButton;

    [Header("Time Display")]
    public TextMeshProUGUI currentTimeText;
    public TextMeshProUGUI totalTimeText;
    public Slider timelineSlider;

    [Header("Selected Note Info")]
    public GameObject selectedNotePanel;
    public TextMeshProUGUI noteTimeText;
    public TextMeshProUGUI noteLaneText;
    public TextMeshProUGUI noteTypeText;

    [Header("Edit Controls")]
    public Button deleteNoteButton;
    public Button addNoteButton;
    public Toggle snapToggle;
    public TMP_InputField snapIntervalInput;

    [Header("Save")]
    public Button saveButton;
    public Button saveExternalButton;
    public GameObject saveConfirmationPanel;

    [Header("Load")]
    public Button loadExternalButton;

    [Header("Chart Info")]
    public TextMeshProUGUI chartNameText;
    public TextMeshProUGUI totalNotesText;

    private bool isUpdatingSlider = false;

    /// <summary>
    /// 初始化UI
    /// </summary>
    public void Initialize(ChartEditorManager manager)
    {
        this.manager = manager;

        Debug.Log("[ChartEditorUI] Initializing UI");

        // 绑定按钮事件（先移除旧的监听器，避免重复绑定）
        if (playPauseButton != null)
        {
            playPauseButton.onClick.RemoveAllListeners();
            playPauseButton.onClick.AddListener(OnPlayPauseClicked);
            Debug.Log("[ChartEditorUI] PlayPause button bound");
        }
        else
        {
            Debug.LogWarning("[ChartEditorUI] playPauseButton is null!");
        }

        if (stopButton != null)
        {
            stopButton.onClick.RemoveAllListeners();
            stopButton.onClick.AddListener(OnStopClicked);
        }

        if (deleteNoteButton != null)
        {
            deleteNoteButton.onClick.RemoveAllListeners();
            deleteNoteButton.onClick.AddListener(OnDeleteNoteClicked);
        }

        if (addNoteButton != null)
        {
            addNoteButton.onClick.RemoveAllListeners();
            addNoteButton.onClick.AddListener(OnAddNoteClicked);
        }

        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(OnSaveClicked);
        }

        if (saveExternalButton != null)
        {
            saveExternalButton.onClick.RemoveAllListeners();
            saveExternalButton.onClick.AddListener(OnSaveExternalClicked);
        }

        if (loadExternalButton != null)
        {
            loadExternalButton.onClick.RemoveAllListeners();
            loadExternalButton.onClick.AddListener(OnLoadExternalClicked);
        }

        if (snapToggle != null)
        {
            snapToggle.onValueChanged.RemoveAllListeners();
            snapToggle.isOn = manager.snapEnabled;
            snapToggle.onValueChanged.AddListener(OnSnapToggleChanged);
        }

        if (snapIntervalInput != null)
        {
            snapIntervalInput.onEndEdit.RemoveAllListeners();
            snapIntervalInput.text = manager.snapInterval.ToString("F2");
            snapIntervalInput.onEndEdit.AddListener(OnSnapIntervalChanged);
        }

        if (timelineSlider != null)
        {
            float duration = manager.GetTotalDuration();

            // 先移除监听器，避免触发OnTimelineSliderChanged
            timelineSlider.onValueChanged.RemoveAllListeners();

            timelineSlider.minValue = 0f;
            timelineSlider.maxValue = duration;
            timelineSlider.value = 0f; // 设置初始值

            // 再添加监听器
            timelineSlider.onValueChanged.AddListener(OnTimelineSliderChanged);

            Debug.Log($"[ChartEditorUI] Timeline slider set: 0 to {duration}");
        }
        else
        {
            Debug.LogWarning("[ChartEditorUI] timelineSlider is null!");
        }

        // 初始化显示
        UpdateChartInfo();
        UpdateTotalTimeDisplay();
        UpdateTimeDisplay(0f);
        UpdateSelectedNoteInfo(null);

        if (saveConfirmationPanel != null)
        {
            saveConfirmationPanel.SetActive(false);
        }

        Debug.Log("[ChartEditorUI] UI initialization complete");
    }

    void Update()
    {
        if (manager == null) return;

        // 实时更新播放/暂停按钮文本
        UpdatePlayPauseButton();

        // 更新时间轴滑块（播放时）
        if (manager.IsPlaying && !isUpdatingSlider)
        {
            UpdateTimelineSlider();
        }
    }

    /// <summary>
    /// 播放/暂停按钮点击
    /// </summary>
    private void OnPlayPauseClicked()
    {
        manager.TogglePlayPause();
    }

    /// <summary>
    /// 停止按钮点击
    /// </summary>
    private void OnStopClicked()
    {
        manager.Stop();
    }

    /// <summary>
    /// 删除note按钮点击
    /// </summary>
    private void OnDeleteNoteClicked()
    {
        manager.DeleteSelectedNote();
    }

    /// <summary>
    /// 添加note按钮点击
    /// </summary>
    private void OnAddNoteClicked()
    {
        // 在当前时间和第0条lane添加一个 J_Color0_Red 的note
        float currentTime = manager.GetCurrentTime();
        manager.AddNote(currentTime, 0, NoteType.Color, GameColor.J_Color0_Red);
    }

    /// <summary>
    /// 保存按钮点击
    /// </summary>
    private void OnSaveClicked()
    {
        manager.SaveChart();
    }

    /// <summary>
    /// 保存到外部文件按钮点击
    /// </summary>
    private void OnSaveExternalClicked()
    {
        manager.SaveChartToExternalFile();
    }

    /// <summary>
    /// 加载外部文件按钮点击
    /// </summary>
    private void OnLoadExternalClicked()
    {
        manager.LoadExternalChart();
    }

    /// <summary>
    /// 吸附开关切换
    /// </summary>
    private void OnSnapToggleChanged(bool value)
    {
        manager.snapEnabled = value;
        Debug.Log($"[ChartEditorUI] Snap enabled: {value}");
    }

    /// <summary>
    /// 吸附间隔输入
    /// </summary>
    private void OnSnapIntervalChanged(string value)
    {
        if (float.TryParse(value, out float interval))
        {
            manager.snapInterval = Mathf.Max(0.01f, interval);
            Debug.Log($"[ChartEditorUI] Snap interval: {manager.snapInterval}");
        }
    }

    /// <summary>
    /// 时间轴滑块改变
    /// </summary>
    private void OnTimelineSliderChanged(float value)
    {
        Debug.Log($"[ChartEditorUI] OnTimelineSliderChanged called, value: {value}, isUpdatingSlider: {isUpdatingSlider}");

        if (isUpdatingSlider) return;

        Debug.Log($"[ChartEditorUI] Slider changed by user, pausing playback");

        // 暂停播放
        if (manager.IsPlaying)
        {
            manager.Pause();
        }

        // 设置音乐时间
        manager.SetMusicTime(value);

        // 更新镜头位置
        manager.cameraController.SetCameraPositionByTime(value);
    }

    /// <summary>
    /// 更新播放/暂停按钮
    /// </summary>
    private void UpdatePlayPauseButton()
    {
        if (playPauseButtonText != null)
        {
            playPauseButtonText.text = manager.IsPlaying ? "Pause" : "Play";
        }
    }

    /// <summary>
    /// 更新时间显示
    /// </summary>
    public void UpdateTimeDisplay(float currentTime)
    {
        if (currentTimeText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            int milliseconds = Mathf.FloorToInt((currentTime * 1000f) % 1000f);
            currentTimeText.text = $"{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
        }
    }

    /// <summary>
    /// 更新总时长显示
    /// </summary>
    private void UpdateTotalTimeDisplay()
    {
        if (totalTimeText != null)
        {
            float totalTime = manager.GetTotalDuration();
            int minutes = Mathf.FloorToInt(totalTime / 60f);
            int seconds = Mathf.FloorToInt(totalTime % 60f);
            totalTimeText.text = $"{minutes:D2}:{seconds:D2}";
        }
    }

    /// <summary>
    /// 更新时间轴滑块
    /// </summary>
    private void UpdateTimelineSlider()
    {
        if (timelineSlider != null)
        {
            isUpdatingSlider = true;
            timelineSlider.value = manager.GetCurrentTime();
            isUpdatingSlider = false;
        }
    }

    /// <summary>
    /// 更新选中note信息
    /// </summary>
    public void UpdateSelectedNoteInfo(EditorNote note)
    {
        if (selectedNotePanel != null)
        {
            selectedNotePanel.SetActive(note != null);
        }

        if (note != null)
        {
            if (noteTimeText != null)
            {
                noteTimeText.text = $"Time: {note.GetTimeInSeconds():F3}s";
            }

            if (noteLaneText != null)
            {
                noteLaneText.text = $"Lane: {note.NoteData.lane}";
            }

            if (noteTypeText != null)
            {
                noteTypeText.text = $"Type: {note.GetTypeDescription()}";
            }
        }

        // 更新删除按钮状态
        if (deleteNoteButton != null)
        {
            deleteNoteButton.interactable = (note != null);
        }
    }

    /// <summary>
    /// 更新谱面信息
    /// </summary>
    private void UpdateChartInfo()
    {
        if (chartNameText != null && manager.currentChart != null)
        {
            chartNameText.text = $"{manager.currentChart.songName} - {manager.currentChart.difficulty}";
        }

        UpdateTotalNotesCount();
    }

    /// <summary>
    /// 更新总note数显示
    /// </summary>
    public void UpdateTotalNotesCount()
    {
        if (totalNotesText != null)
        {
            totalNotesText.text = $"Total Notes: {manager.TotalNotes}";
        }
    }

    /// <summary>
    /// 显示保存确认
    /// </summary>
    public void ShowSaveConfirmation()
    {
        if (saveConfirmationPanel != null)
        {
            saveConfirmationPanel.SetActive(true);
            Invoke(nameof(HideSaveConfirmation), 2f);
        }
    }

    /// <summary>
    /// 隐藏保存确认
    /// </summary>
    private void HideSaveConfirmation()
    {
        if (saveConfirmationPanel != null)
        {
            saveConfirmationPanel.SetActive(false);
        }
    }
}
