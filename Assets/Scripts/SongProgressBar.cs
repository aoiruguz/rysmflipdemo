using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 歌曲进度条显示器
/// 显示当前歌曲的完成进度（已处理 Note 数 / 总 Note 数）
/// </summary>
public class SongProgressBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("进度条的 Image 组件（Fill Type 设为 Filled）")]
    public Image progressBarFill;

    [Tooltip("进度文本显示（可选，例如：\"50/100\"）")]
    public TextMeshProUGUI progressText;

    [Tooltip("百分比文本显示（可选，例如：\"50%\"）")]
    public TextMeshProUGUI percentageText;

    [Header("Visual Settings")]
    [Tooltip("进度条颜色")]
    public Color progressColor = new Color(0.2f, 0.8f, 1f, 1f);

    [Tooltip("进度条完成时的颜色")]
    public Color completedColor = new Color(0.2f, 1f, 0.2f, 1f);

    [Tooltip("是否启用平滑过渡动画")]
    public bool smoothTransition = true;

    [Tooltip("平滑过渡速度")]
    public float transitionSpeed = 5f;

    private SongCompletionDetector detector;
    private float targetProgress = 0f;
    private float currentProgress = 0f;

    void Start()
    {
        // 查找 SongCompletionDetector
        detector = FindFirstObjectByType<SongCompletionDetector>();

        if (detector == null)
        {
            Debug.LogWarning("[SongProgressBar] No SongCompletionDetector found in scene!");
            enabled = false;
            return;
        }

        Debug.Log("[SongProgressBar] SongCompletionDetector found!");

        // 初始化进度条
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = 0f;
            progressBarFill.color = progressColor;
            Debug.Log($"[SongProgressBar] ProgressBarFill initialized. Type: {progressBarFill.type}, FillMethod: {progressBarFill.fillMethod}");
        }
        else
        {
            Debug.LogError("[SongProgressBar] progressBarFill is NULL!");
        }

        UpdateDisplay(0f, 0, 0);
    }

    void Update()
    {
        if (detector == null) return;

        // 获取当前进度
        targetProgress = detector.GetProgress();
        int processed = detector.GetProcessedNotes();
        int total = detector.GetTotalNotes();

        // 调试日志（每秒输出一次）
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[SongProgressBar] Progress: {targetProgress:F2} ({processed}/{total})");
        }

        // 平滑过渡或直接设置
        if (smoothTransition)
        {
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * transitionSpeed);
        }
        else
        {
            currentProgress = targetProgress;
        }

        // 更新进度条填充
        if (progressBarFill != null)
        {
            progressBarFill.fillAmount = currentProgress;

            // 完成时改变颜色
            if (currentProgress >= 0.99f)
            {
                progressBarFill.color = completedColor;
            }
        }

        // 更新文本显示
        UpdateDisplay(currentProgress, processed, total);
    }

    private void UpdateDisplay(float progress, int processed, int total)
    {
        // 更新进度文本（例如："50/100"）
        if (progressText != null)
        {
            progressText.text = $"{processed}/{total}";
        }

        // 更新百分比文本（例如："50%"）
        if (percentageText != null)
        {
            percentageText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
    }
}
