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
    [Tooltip("进度条填充区域的 RectTransform (Background/Fill)")]
    public RectTransform fillRectTransform;

    /* 旧版文本引用，暂时注释
    [Tooltip("进度文本显示（可选，例如：\"50/100\"）")]
    public TextMeshProUGUI progressText;

    [Tooltip("百分比文本显示（可选，例如：\"50%\"）")]
    public TextMeshProUGUI percentageText;
    */

    [Header("Width Settings")]
    [Tooltip("进度为 0% 时的最小宽度")]
    public float minWidth = 20f;
    [Tooltip("进度为 100% 时的最大宽度")]
    public float maxWidth = 243f;

    /* 旧版颜色设置，暂时注释
    [Header("Visual Settings")]
    [Tooltip("进度条颜色")]
    public Color progressColor = new Color(0.2f, 0.8f, 1f, 1f);

    [Tooltip("进度条完成时的颜色")]
    public Color completedColor = new Color(0.2f, 1f, 0.2f, 1f);
    */

    [Header("Animation Settings")]
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

        // 初始化进度条宽度
        if (fillRectTransform != null)
        {
            SetFillWidth(0f);
        }
        else
        {
            Debug.LogError("[SongProgressBar] fillRectTransform is NULL!");
        }

        // UpdateDisplay(0f, 0, 0); // 旧版文本更新，已注释
    }

    void Update()
    {
        if (detector == null) return;

        // 获取当前进度
        targetProgress = detector.GetProgress();
        int processed = detector.GetProcessedNotes();
        int total = detector.GetTotalNotes();

        // 平滑过渡或直接设置
        if (smoothTransition)
        {
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * transitionSpeed);
        }
        else
        {
            currentProgress = targetProgress;
        }

        // 更新进度条宽度
        if (fillRectTransform != null)
        {
            SetFillWidth(currentProgress);
        }

        // UpdateDisplay(currentProgress, processed, total); // 旧版文本更新，已注释
    }

    /// <summary>
    /// 根据进度计算并设置 RectTransform 的宽度
    /// </summary>
    private void SetFillWidth(float progress)
    {
        // 确保 progress 在 0-1 之间
        progress = Mathf.Clamp01(progress);
        float targetWidth = Mathf.Lerp(minWidth, maxWidth, progress);
        
        Vector2 sizeDelta = fillRectTransform.sizeDelta;
        sizeDelta.x = targetWidth;
        fillRectTransform.sizeDelta = sizeDelta;
    }

    /* 旧版文本更新逻辑，暂时注释
    private void UpdateDisplay(float progress, int processed, int total)
    {
        if (progressText != null)
        {
            progressText.text = $"{processed}/{total}";
        }

        if (percentageText != null)
        {
            percentageText.text = $"{Mathf.RoundToInt(progress * 100)}%";
        }
    }
    */
}
