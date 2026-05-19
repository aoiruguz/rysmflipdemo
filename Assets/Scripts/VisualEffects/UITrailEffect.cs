using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI元素残影效果
/// 为移动的UI元素（Image、TextMeshPro等）添加拖尾残影
/// </summary>
public class UITrailEffect : MonoBehaviour
{
    [Header("渲染设置")]
    [Tooltip("残影父对象模式")]
    public TrailParentMode parentMode = TrailParentMode.SameAsSource;

    [Tooltip("残影渲染层级模式")]
    public TrailRenderMode renderMode = TrailRenderMode.BehindSource;

    [Tooltip("自定义层级偏移（相对于源对象）")]
    public int customSiblingOffset = -1;

    public enum TrailParentMode
    {
        SameAsSource,      // 与源对象同级（推荐用于拖尾）
        CanvasRoot         // Canvas根节点
    }

    public enum TrailRenderMode
    {
        BehindSource,      // 在源对象后面
        InFrontOfSource,   // 在源对象前面
        Custom             // 自定义偏移
    }

    [Header("残影设置")]
    [Tooltip("残影数量")]
    [Range(3, 20)]
    public int trailCount = 8;

    [Tooltip("残影间隔时间（秒）")]
    [Range(0.01f, 0.2f)]
    public float trailInterval = 0.05f;

    [Tooltip("残影持续时间（秒）")]
    [Range(0.1f, 2f)]
    public float trailDuration = 0.5f;

    [Tooltip("残影初始透明度")]
    [Range(0.1f, 1f)]
    public float trailStartAlpha = 0.6f;

    [Tooltip("残影缩放比例")]
    [Range(0.5f, 1.5f)]
    public float trailScale = 0.95f;

    [Header("调试")]
    [Tooltip("显示调试信息")]
    public bool showDebug = false;

    [Header("性能优化")]
    [Tooltip("启用对象池（推荐）")]
    public bool useObjectPool = true;

    private RectTransform rectTransform;
    private Image sourceImage;
    private TextMeshProUGUI sourceText;
    private Canvas canvas;

    private List<TrailInstance> activeTrails = new List<TrailInstance>();
    private Queue<GameObject> trailPool = new Queue<GameObject>();
    private float lastTrailTime;

    private class TrailInstance
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public Image image;
        public TextMeshProUGUI text;
        public float spawnTime;
        public float startAlpha;
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        sourceImage = GetComponent<Image>();
        sourceText = GetComponent<TextMeshProUGUI>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("[UITrailEffect] 找不到父级Canvas！");
            enabled = false;
            return;
        }

        // 预创建对象池
        if (useObjectPool)
        {
            for (int i = 0; i < trailCount; i++)
            {
                GameObject trailObj = CreateTrailObject();
                trailObj.SetActive(false);
                trailPool.Enqueue(trailObj);
            }
        }

        lastTrailTime = Time.time;
    }

    void Update()
    {
        // 生成新的残影
        if (Time.time - lastTrailTime >= trailInterval)
        {
            SpawnTrail();
            lastTrailTime = Time.time;
        }

        // 更新现有残影
        UpdateTrails();
    }

    void SpawnTrail()
    {
        GameObject trailObj;

        // 从对象池获取或创建新对象
        if (useObjectPool && trailPool.Count > 0)
        {
            trailObj = trailPool.Dequeue();
            trailObj.SetActive(true);
        }
        else
        {
            trailObj = CreateTrailObject();
        }

        // 设置残影层级
        UpdateTrailSiblingIndex(trailObj);

        // 设置残影属性
        RectTransform trailRect = trailObj.GetComponent<RectTransform>();

        // 复制源对象的位置和旋转
        trailRect.anchoredPosition = rectTransform.anchoredPosition;
        trailRect.localRotation = rectTransform.localRotation;
        trailRect.localScale = rectTransform.localScale * trailScale;

        // 复制颜色和内容
        Image trailImage = trailObj.GetComponent<Image>();
        TextMeshProUGUI trailText = trailObj.GetComponent<TextMeshProUGUI>();

        float startAlpha = trailStartAlpha;

        if (trailImage != null && sourceImage != null)
        {
            trailImage.sprite = sourceImage.sprite;
            Color color = sourceImage.color;
            color.a = startAlpha;
            trailImage.color = color;
        }

        if (trailText != null && sourceText != null)
        {
            trailText.text = sourceText.text;
            trailText.fontSize = sourceText.fontSize;
            trailText.font = sourceText.font;
            Color color = sourceText.color;
            color.a = startAlpha;
            trailText.color = color;
        }

        // 添加到活跃列表
        TrailInstance trail = new TrailInstance
        {
            gameObject = trailObj,
            rectTransform = trailRect,
            image = trailImage,
            text = trailText,
            spawnTime = Time.time,
            startAlpha = startAlpha
        };

        activeTrails.Add(trail);

        if (showDebug)
        {
            Debug.Log($"[UITrail] 生成残影 - 源位置: {rectTransform.anchoredPosition}, 残影位置: {trailRect.anchoredPosition}, 父对象模式: {parentMode}");
        }

        // 限制残影数量
        while (activeTrails.Count > trailCount)
        {
            RemoveTrail(activeTrails[0]);
        }
    }

    void UpdateTrails()
    {
        for (int i = activeTrails.Count - 1; i >= 0; i--)
        {
            TrailInstance trail = activeTrails[i];
            float age = Time.time - trail.spawnTime;

            // 检查是否过期
            if (age >= trailDuration)
            {
                RemoveTrail(trail);
                continue;
            }

            // 更新透明度（线性淡出）
            float t = age / trailDuration;
            float alpha = Mathf.Lerp(trail.startAlpha, 0f, t);

            if (trail.image != null)
            {
                Color color = trail.image.color;
                color.a = alpha;
                trail.image.color = color;
            }

            if (trail.text != null)
            {
                Color color = trail.text.color;
                color.a = alpha;
                trail.text.color = color;
            }
        }
    }

    void RemoveTrail(TrailInstance trail)
    {
        activeTrails.Remove(trail);

        if (useObjectPool)
        {
            trail.gameObject.SetActive(false);
            trailPool.Enqueue(trail.gameObject);
        }
        else
        {
            Destroy(trail.gameObject);
        }
    }

    GameObject CreateTrailObject()
    {
        GameObject trailObj = new GameObject("Trail");

        // 根据父对象模式设置父级
        Transform parentTransform;
        switch (parentMode)
        {
            case TrailParentMode.SameAsSource:
                // 与源对象同级（使用源对象的父对象）
                parentTransform = transform.parent;
                break;

            case TrailParentMode.CanvasRoot:
            default:
                // Canvas根节点
                parentTransform = canvas.transform;
                break;
        }

        trailObj.transform.SetParent(parentTransform, false);

        RectTransform trailRect = trailObj.AddComponent<RectTransform>();
        trailRect.sizeDelta = rectTransform.sizeDelta;
        trailRect.anchorMin = rectTransform.anchorMin;
        trailRect.anchorMax = rectTransform.anchorMax;
        trailRect.pivot = rectTransform.pivot;

        // 复制组件
        if (sourceImage != null)
        {
            Image trailImage = trailObj.AddComponent<Image>();
            trailImage.raycastTarget = false; // 残影不接收点击
        }

        if (sourceText != null)
        {
            TextMeshProUGUI trailText = trailObj.AddComponent<TextMeshProUGUI>();
            trailText.raycastTarget = false;
            trailText.alignment = sourceText.alignment;
            trailText.fontStyle = sourceText.fontStyle;
        }

        return trailObj;
    }

    void UpdateTrailSiblingIndex(GameObject trailObj)
    {
        int sourceIndex = transform.GetSiblingIndex();

        switch (renderMode)
        {
            case TrailRenderMode.BehindSource:
                // 在源对象正后面（索引相同或-1）
                trailObj.transform.SetSiblingIndex(sourceIndex);
                break;

            case TrailRenderMode.InFrontOfSource:
                // 在源对象正前面（索引+1）
                trailObj.transform.SetSiblingIndex(sourceIndex + 1);
                break;

            case TrailRenderMode.Custom:
                // 自定义偏移
                int targetIndex = sourceIndex + customSiblingOffset;
                trailObj.transform.SetSiblingIndex(Mathf.Max(0, targetIndex));
                break;
        }
    }

    void OnDestroy()
    {
        // 清理所有残影
        foreach (var trail in activeTrails)
        {
            if (trail.gameObject != null)
            {
                Destroy(trail.gameObject);
            }
        }

        // 清理对象池
        while (trailPool.Count > 0)
        {
            GameObject obj = trailPool.Dequeue();
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }

    // 运行时调整参数
    public void SetTrailCount(int count)
    {
        trailCount = Mathf.Clamp(count, 3, 20);
    }

    public void SetTrailInterval(float interval)
    {
        trailInterval = Mathf.Clamp(interval, 0.01f, 0.2f);
    }

    public void SetTrailDuration(float duration)
    {
        trailDuration = Mathf.Clamp(duration, 0.1f, 2f);
    }
}
