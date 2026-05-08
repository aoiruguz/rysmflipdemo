using UnityEngine;

/// <summary>
/// 制谱器镜头控制器
/// 播放时自动跟随音乐时间移动
/// 暂停时可用滚轮控制镜头位置和音乐时间
/// </summary>
public class EditorCameraController : MonoBehaviour
{
    private ChartEditorManager manager;
    private Camera cam;

    [Header("Scroll Settings")]
    public float scrollSpeed = 2f; // 滚轮滚动速度
    public float scrollTimeStep = 0.1f; // 每次滚动改变的时间（秒）

    [Header("Camera Bounds")]
    public float minY = -5f; // 镜头最低位置
    public float maxY = 1000f; // 镜头最高位置（根据歌曲长度动态调整）

    private float judgmentLineOffset = 0f; // 判定线相对于镜头的偏移

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize(ChartEditorManager manager)
    {
        this.manager = manager;

        // 计算判定线偏移（判定线在屏幕下方）
        judgmentLineOffset = manager.judgmentLineY - transform.position.y;

        // 根据歌曲长度设置最大Y
        if (manager.currentChart != null && manager.currentChart.audioClip != null)
        {
            float songLength = manager.currentChart.audioClip.length;
            maxY = manager.TimeToYPosition(songLength) + 10f; // 多留10单位空间
        }

        // 初始化镜头位置
        SetCameraPositionByTime(0f);
    }

    void Update()
    {
        // 检查manager是否已初始化
        if (manager == null) return;

        // 只在暂停状态下处理滚轮输入
        if (!manager.IsPlaying)
        {
            HandleScrollInput();
        }
    }

    /// <summary>
    /// 处理滚轮输入
    /// </summary>
    private void HandleScrollInput()
    {
        if (manager == null) return;

        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            // 计算当前时间
            float currentTime = manager.GetCurrentTime();

            // 根据滚轮方向改变时间
            float newTime = currentTime + scroll * scrollTimeStep;
            newTime = Mathf.Clamp(newTime, 0f, manager.GetTotalDuration());

            // 设置音乐时间
            manager.SetMusicTime(newTime);

            // 更新镜头位置
            SetCameraPositionByTime(newTime);
        }
    }

    /// <summary>
    /// 根据时间设置镜头位置
    /// </summary>
    public void SetCameraPositionByTime(float timeInSeconds)
    {
        if (manager == null) return;

        // 计算目标Y位置
        float targetY = manager.TimeToYPosition(timeInSeconds);

        // 加上判定线偏移，使判定线始终在屏幕下方
        targetY += Mathf.Abs(judgmentLineOffset);

        // 限制范围
        targetY = Mathf.Clamp(targetY, minY, maxY);

        // 更新镜头位置
        Vector3 newPos = transform.position;
        newPos.y = targetY;
        transform.position = newPos;
    }

    /// <summary>
    /// 获取镜头当前对应的时间
    /// </summary>
    public float GetTimeFromCameraPosition()
    {
        if (manager == null) return 0f;

        float adjustedY = transform.position.y - Mathf.Abs(judgmentLineOffset);
        return manager.YPositionToTime(adjustedY);
    }
}
