using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 制作者名单滚动效果
/// 用于结算界面显示制作人员名单，从下往上滚动
/// 支持在指定区域内显示，超出区域自动隐藏
/// </summary>
public class CreditsScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("滚动内容的 RectTransform（包含制作者名单的容器）")]
    public RectTransform scrollContent;

    [Tooltip("可视区域的 RectTransform（用于裁剪显示区域）")]
    public RectTransform viewportRect;

    [Tooltip("滚动速度（像素/秒）")]
    public float scrollSpeed = 50f;

    [Tooltip("是否自动开始滚动")]
    public bool autoStart = true;

    [Tooltip("滚动完成后是否循环")]
    public bool loop = false;

    [Tooltip("滚动完成后的延迟时间（秒）再循环")]
    public float loopDelay = 2f;

    [Header("Scroll Boundaries")]
    [Tooltip("滚动起始位置（相对于父容器的 Y 坐标）")]
    public float startPositionY = -500f;

    [Tooltip("滚动结束位置（相对于父容器的 Y 坐标）")]
    public float endPositionY = 500f;

    [Tooltip("是否自动计算结束位置（内容完全滚出屏幕）")]
    public bool autoCalculateEndPosition = true;

    [Header("Visibility Control")]
    [Tooltip("可视区域的最小 Y 坐标（低于此值的内容会被隐藏）")]
    public float visibleMinY = -300f;

    [Tooltip("可视区域的最大 Y 坐标（高于此值的内容会被隐藏）")]
    public float visibleMaxY = 300f;

    [Tooltip("是否启用可视区域裁剪")]
    public bool enableVisibilityClipping = true;

    [Header("Text Style Control")]
    [Tooltip("统一控制所有文本的字体")]
    public TMP_FontAsset font;

    [Tooltip("统一控制所有文本的字体大小")]
    public float fontSize = 36f;

    [Tooltip("统一控制所有文本的颜色")]
    public Color textColor = Color.white;

    [Tooltip("是否在运行时应用文本样式")]
    public bool applyTextStyleOnStart = true;

    [Header("Control")]
    [Tooltip("是否正在滚动")]
    [SerializeField]
    private bool isScrolling = false;

    private float currentPositionY;
    private float targetEndPositionY;
    private float delayTimer = 0f;
    private bool isWaitingForLoop = false;
    private List<TextMeshProUGUI> allTexts = new List<TextMeshProUGUI>();

    void Start()
    {
        if (scrollContent == null)
        {
            Debug.LogError("[CreditsScroller] scrollContent 未设置！");
            return;
        }

        // 收集所有文本组件
        CollectAllTexts();

        // 应用文本样式
        if (applyTextStyleOnStart)
        {
            ApplyTextStyle();
        }

        // 初始化位置
        ResetPosition();

        // 计算结束位置
        if (autoCalculateEndPosition)
        {
            // 内容高度 + 可视区域高度（确保内容完全滚出屏幕）
            if (viewportRect != null)
            {
                targetEndPositionY = scrollContent.rect.height + viewportRect.rect.height;
            }
            else
            {
                targetEndPositionY = scrollContent.rect.height + Mathf.Abs(visibleMaxY - visibleMinY);
            }
        }
        else
        {
            targetEndPositionY = endPositionY;
        }

        if (autoStart)
        {
            StartScrolling();
        }

        Debug.Log($"[CreditsScroller] Initialized. Start: {startPositionY}, End: {targetEndPositionY}, Texts: {allTexts.Count}");
    }

    void Update()
    {
        if (scrollContent == null) return;

        // 等待循环延迟
        if (isWaitingForLoop)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= loopDelay)
            {
                ResetPosition();
                isWaitingForLoop = false;
                delayTimer = 0f;
                isScrolling = true;
            }
            return;
        }

        // 滚动逻辑
        if (isScrolling)
        {
            currentPositionY += scrollSpeed * Time.deltaTime;

            // 更新位置
            scrollContent.anchoredPosition = new Vector2(scrollContent.anchoredPosition.x, currentPositionY);

            // 更新可视性
            if (enableVisibilityClipping)
            {
                UpdateTextVisibility();
            }

            // 检查是否到达结束位置
            if (currentPositionY >= targetEndPositionY)
            {
                if (loop)
                {
                    // 循环模式：等待延迟后重新开始
                    isScrolling = false;
                    isWaitingForLoop = true;
                    Debug.Log("[CreditsScroller] Scroll completed. Waiting for loop...");
                }
                else
                {
                    // 非循环模式：停止滚动并隐藏
                    isScrolling = false;
                    if (scrollContent != null)
                    {
                        scrollContent.gameObject.SetActive(false);
                    }
                    Debug.Log("[CreditsScroller] Scroll completed and hidden.");
                }
            }
        }
    }

    /// <summary>
    /// 收集所有文本组件
    /// </summary>
    private void CollectAllTexts()
    {
        allTexts.Clear();
        if (scrollContent != null)
        {
            allTexts.AddRange(scrollContent.GetComponentsInChildren<TextMeshProUGUI>(true));
            Debug.Log($"[CreditsScroller] Collected {allTexts.Count} text components");
        }
    }

    /// <summary>
    /// 应用文本样式到所有文本
    /// </summary>
    public void ApplyTextStyle()
    {
        if (allTexts.Count == 0)
        {
            CollectAllTexts();
        }

        foreach (var text in allTexts)
        {
            if (text == null) continue;

            if (font != null)
            {
                text.font = font;
            }

            text.fontSize = fontSize;
            text.color = textColor;
        }

        Debug.Log($"[CreditsScroller] Applied text style to {allTexts.Count} texts");
    }

    /// <summary>
    /// 更新文本可视性（根据可视区域裁剪）
    /// </summary>
    private void UpdateTextVisibility()
    {
        foreach (var text in allTexts)
        {
            if (text == null) continue;

            // 获取文本在世界空间中的位置
            Vector3[] worldCorners = new Vector3[4];
            text.rectTransform.GetWorldCorners(worldCorners);

            // 转换到本地空间（相对于父容器）
            Vector2 localPos = transform.InverseTransformPoint(worldCorners[0]);
            float textBottomY = localPos.y;
            float textTopY = transform.InverseTransformPoint(worldCorners[1]).y;

            // 检查是否在可视区域内
            bool isVisible = textTopY >= visibleMinY && textBottomY <= visibleMaxY;

            // 设置可见性
            if (text.gameObject.activeSelf != isVisible)
            {
                text.gameObject.SetActive(isVisible);
            }
        }
    }

    /// <summary>
    /// 开始滚动
    /// </summary>
    public void StartScrolling()
    {
        if (scrollContent == null)
        {
            Debug.LogWarning("[CreditsScroller] scrollContent 未设置，无法开始滚动！");
            return;
        }

        isScrolling = true;
        isWaitingForLoop = false;
        Debug.Log("[CreditsScroller] Started scrolling");
    }

    /// <summary>
    /// 停止滚动
    /// </summary>
    public void StopScrolling()
    {
        isScrolling = false;
        isWaitingForLoop = false;
        Debug.Log("[CreditsScroller] Stopped scrolling");
    }

    /// <summary>
    /// 暂停滚动
    /// </summary>
    public void PauseScrolling()
    {
        isScrolling = false;
        Debug.Log("[CreditsScroller] Paused scrolling");
    }

    /// <summary>
    /// 恢复滚动
    /// </summary>
    public void ResumeScrolling()
    {
        if (!isWaitingForLoop)
        {
            isScrolling = true;
            Debug.Log("[CreditsScroller] Resumed scrolling");
        }
    }

    /// <summary>
    /// 重置到起始位置
    /// </summary>
    public void ResetPosition()
    {
        if (scrollContent == null) return;

        currentPositionY = startPositionY;
        scrollContent.anchoredPosition = new Vector2(scrollContent.anchoredPosition.x, currentPositionY);
        scrollContent.gameObject.SetActive(true);
        isScrolling = false;
        isWaitingForLoop = false;
        delayTimer = 0f;

        // 重新显示所有文本
        foreach (var text in allTexts)
        {
            if (text != null)
            {
                text.gameObject.SetActive(true);
            }
        }

        Debug.Log("[CreditsScroller] Position reset");
    }

    /// <summary>
    /// 设置滚动速度
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
        Debug.Log($"[CreditsScroller] Scroll speed set to {speed}");
    }

    /// <summary>
    /// 设置文本样式
    /// </summary>
    public void SetTextStyle(TMP_FontAsset newFont, float newFontSize, Color newColor)
    {
        font = newFont;
        fontSize = newFontSize;
        textColor = newColor;
        ApplyTextStyle();
    }

    /// <summary>
    /// 设置可视区域
    /// </summary>
    public void SetVisibleArea(float minY, float maxY)
    {
        visibleMinY = minY;
        visibleMaxY = maxY;
        Debug.Log($"[CreditsScroller] Visible area set to [{minY}, {maxY}]");
    }

    /// <summary>
    /// 获取滚动进度（0-1）
    /// </summary>
    public float GetScrollProgress()
    {
        if (targetEndPositionY <= startPositionY) return 0f;
        return Mathf.Clamp01((currentPositionY - startPositionY) / (targetEndPositionY - startPositionY));
    }

    /// <summary>
    /// 检查是否正在滚动
    /// </summary>
    public bool IsScrolling()
    {
        return isScrolling;
    }

    /// <summary>
    /// 检查是否滚动完成
    /// </summary>
    public bool IsCompleted()
    {
        return !isScrolling && !isWaitingForLoop && currentPositionY >= targetEndPositionY;
    }
}
