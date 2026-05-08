using UnityEngine;

/// <summary>
/// 可编辑的Note组件
/// 支持拖动、类型切换、选中状态
/// 使用子物体激活/停用的方式显示不同类型的note
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class EditorNote : MonoBehaviour
{
    [Header("Child Objects - Same as Note.cs")]
    public GameObject colorNoteVisual;      // 普通三色note的子物体
    public GameObject leftDirectionalVisual;  // 左方向note的子物体
    public GameObject rightDirectionalVisual; // 右方向note的子物体

    private ChartEditorManager manager;
    private NoteData noteData;
    private int dataIndex;
    private bool isSelected = false;
    private bool isDragging = false;
    private Vector3 dragOffset;

    private BoxCollider2D boxCollider;
    private Vector3 originalScale; // 保存原始scale

    // 视觉反馈
    private float selectedScale = 1.2f;

    public NoteData NoteData => noteData;
    public int DataIndex => dataIndex;
    public bool IsSelected => isSelected;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        // 保存原始scale
        originalScale = transform.localScale;
    }

    /// <summary>
    /// 初始化EditorNote
    /// </summary>
    public void Initialize(ChartEditorManager manager, NoteData noteData, int dataIndex)
    {
        this.manager = manager;
        this.noteData = noteData;
        this.dataIndex = dataIndex;

        UpdateVisual();
    }

    void Update()
    {
        // 拖动逻辑
        if (isDragging)
        {
            HandleDragging();
        }
    }

    void OnMouseDown()
    {
        // 左键：开始拖动
        if (Input.GetMouseButton(0))
        {
            StartDragging();
        }
    }

    void OnMouseOver()
    {
        // 右键：切换类型（在鼠标悬停时检测右键点击）
        if (Input.GetMouseButtonDown(1))
        {
            CycleNoteType();
        }
    }

    void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            StopDragging();
        }
    }

    /// <summary>
    /// 开始拖动
    /// </summary>
    private void StartDragging()
    {
        // 选中这个note
        manager.SelectNote(this);

        isDragging = true;

        // 计算鼠标与note的偏移
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        dragOffset = transform.position - mouseWorldPos;
    }

    /// <summary>
    /// 处理拖动
    /// </summary>
    private void HandleDragging()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3 targetPos = mouseWorldPos + dragOffset;

        // 只允许左右移动（X轴），不允许上下移动（Y轴）
        int newLane = manager.XPositionToLane(targetPos.x);
        float newXPos = manager.LaneToXPosition(newLane);

        // 更新位置
        transform.position = new Vector3(newXPos, transform.position.y, 0);

        // 更新NoteData
        noteData.lane = newLane;
        UpdateChartData();
    }

    /// <summary>
    /// 停止拖动
    /// </summary>
    private void StopDragging()
    {
        isDragging = false;

        // 播放音效
        manager.PlayNoteHitSound();

        Debug.Log($"[EditorNote] Moved to lane {noteData.lane}");
    }

    /// <summary>
    /// 循环切换Note类型
    /// 顺序：红色 -> 绿色 -> 蓝色 -> 左箭头 -> 右箭头 -> 红色
    /// </summary>
    private void CycleNoteType()
    {
        // 当前状态判断
        if (noteData.noteType == NoteType.Color)
        {
            switch (noteData.color)
            {
                case GameColor.ColorA: // 红色 -> 绿色
                    noteData.color = GameColor.ColorB;
                    break;
                case GameColor.ColorB: // 绿色 -> 蓝色
                    noteData.color = GameColor.ColorC;
                    break;
                case GameColor.ColorC: // 蓝色 -> 左箭头
                    noteData.noteType = NoteType.DirectionalLeft;
                    break;
            }
        }
        else if (noteData.noteType == NoteType.DirectionalLeft)
        {
            // 左箭头 -> 右箭头
            noteData.noteType = NoteType.DirectionalRight;
        }
        else if (noteData.noteType == NoteType.DirectionalRight)
        {
            // 右箭头 -> 红色
            noteData.noteType = NoteType.Color;
            noteData.color = GameColor.ColorA;
        }

        UpdateChartData();
        UpdateVisual();

        // 播放音效
        manager.PlayNoteHitSound();

        Debug.Log($"[EditorNote] Changed type to {noteData.noteType}, color: {noteData.color}");
    }

    /// <summary>
    /// 更新ChartData中的数据
    /// </summary>
    private void UpdateChartData()
    {
        if (manager.currentChart != null && dataIndex >= 0 && dataIndex < manager.currentChart.notes.Count)
        {
            manager.currentChart.notes[dataIndex] = noteData;
        }
    }

    /// <summary>
    /// 更新视觉外观 - 使用子物体激活/停用方式
    /// </summary>
    private void UpdateVisual()
    {
        // 激活/停用子物体，和Note.cs完全一致
        if (colorNoteVisual != null)
            colorNoteVisual.SetActive(noteData.noteType == NoteType.Color);

        if (leftDirectionalVisual != null)
            leftDirectionalVisual.SetActive(noteData.noteType == NoteType.DirectionalLeft);

        if (rightDirectionalVisual != null)
            rightDirectionalVisual.SetActive(noteData.noteType == NoteType.DirectionalRight);

        // 更新颜色note的颜色
        if (noteData.noteType == NoteType.Color && colorNoteVisual != null)
        {
            var spriteRenderer = colorNoteVisual.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color c = Color.white;
                switch (noteData.color)
                {
                    case GameColor.ColorA: c = Color.red; break;
                    case GameColor.ColorB: c = Color.green; break;
                    case GameColor.ColorC: c = Color.blue; break;
                }
                spriteRenderer.color = c;
            }
        }
    }

    /// <summary>
    /// 设置选中状态
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateSelectionVisual();
    }

    /// <summary>
    /// 更新选中状态的视觉效果
    /// </summary>
    private void UpdateSelectionVisual()
    {
        if (isSelected)
        {
            transform.localScale = originalScale * selectedScale;
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// 获取note的时间（秒）
    /// </summary>
    public float GetTimeInSeconds()
    {
        return noteData.hitTime / 1000f;
    }

    /// <summary>
    /// 获取note的判定时间（秒）- 用于判定检测
    /// </summary>
    public float GetNoteTime()
    {
        return noteData.hitTime / 1000f;
    }

    /// <summary>
    /// 获取note的类型描述
    /// </summary>
    public string GetTypeDescription()
    {
        if (noteData.noteType == NoteType.Color)
        {
            return $"Color - {noteData.color}";
        }
        else
        {
            return noteData.noteType.ToString();
        }
    }
}
