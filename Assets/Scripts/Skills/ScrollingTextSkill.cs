using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 横向滚动弹幕技能
/// 在屏幕上半部分生成横向滚动的文字来干扰玩家
/// </summary>
public class ScrollingTextSkill : MonoBehaviour
{
    [HideInInspector]
    public InterferenceTextConfig config;

    [HideInInspector]
    public Canvas interferenceCanvas;

    [HideInInspector]
    public RectTransform canvasRect;

    [Header("轨道设置")]
    [Tooltip("弹幕轨道的Y坐标位置（屏幕比例，0.5为中心）")]
    public float[] trackYPositions = new float[] { 0.7f, 0.75f, 0.8f, 0.85f };

    [Header("视觉设置")]
    [Tooltip("文字颜色（包含透明度Alpha）")]
    public Color textColor = Color.white;

    [Tooltip("文字字体")]
    public TMP_FontAsset font;

    [Tooltip("文字大小")]
    [Range(10, 200)]
    public float fontSize = 36;

    [Header("滚动边界设置")]
    [Tooltip("弹幕起点X坐标（屏幕像素，正值，屏幕最右侧外）")]
    public float spawnXOffset = 400f;

    [Tooltip("弹幕终点X坐标（屏幕像素，负值，屏幕最左侧外）")]
    public float exitXOffset = -400f;

    [Header("移动设置")]
    [Tooltip("弹幕横穿屏幕的持续时间（秒）")]
    [Range(3f, 20f)]
    public float scrollDuration = 10f;

    [Tooltip("技能持续时间（秒）- 控制弹幕生成多久")]
    [Range(5f, 30f)]
    public float skillDuration = 15f;

    [Header("弹幕进阶设置")]
    [Tooltip("存放弹幕文本的空物体容器，并作为生成范围限制")]
    public RectTransform textContainer;

    private Coroutine spawnCoroutine;

    public void Activate()
    {
        if (config == null)
        {
            Debug.LogError("[ScrollingTextSkill] Config is null!");
            return;
        }

        if (interferenceCanvas == null && textContainer == null)
        {
            Debug.LogError("[ScrollingTextSkill] Interference Canvas and textContainer are both null!");
            return;
        }

        spawnCoroutine = StartCoroutine(SpawnBullets());
    }

    public void Deactivate()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnBullets()
    {
        // 获取所有文本内容
        List<string> texts = new List<string>(config.textContents);

        // 如果文本数量少于轨道数量，循环使用
        for (int trackIndex = 0; trackIndex < trackYPositions.Length; trackIndex++)
        {
            string text = texts[trackIndex % texts.Count];
            SpawnSingleBullet(trackIndex, text);
        }

        yield return null;
    }

    private void SpawnSingleBullet(int trackIndex, string text)
    {
        RectTransform container = textContainer != null ? textContainer : canvasRect;

        GameObject bulletObj = new GameObject("ScrollingBullet");
        bulletObj.transform.SetParent(container, false);

        RectTransform rectTransform = bulletObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(0f, 0.5f);
        rectTransform.pivot = new Vector2(0f, 0.5f);
        rectTransform.sizeDelta = new Vector2(800f, 100f);

        TextMeshProUGUI textComponent = bulletObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = textColor;
        textComponent.alignment = TextAlignmentOptions.Left;
        textComponent.textWrappingMode = TextWrappingModes.NoWrap;

        if (font != null)
        {
            textComponent.font = font;
        }

        float containerWidth = container.rect.width;
        float containerHeight = container.rect.height;

        float trackYRatio = trackYPositions[trackIndex];
        float yPosition = (trackYRatio - 0.5f) * containerHeight;

        float startX = containerWidth / 2 + spawnXOffset;
        rectTransform.anchoredPosition = new Vector2(startX, yPosition);

        Debug.Log($"[ScrollingTextSkill] Track {trackIndex}: ratio={trackYRatio}, containerHeight={containerHeight}, yPosition={yPosition}, worldPos={rectTransform.position}");

        float endX = -containerWidth / 2 + exitXOffset;

        StartCoroutine(ScrollBullet(rectTransform, startX, endX, yPosition, scrollDuration));
    }

    private IEnumerator ScrollBullet(RectTransform rectTransform, float startX, float endX, float yPosition, float duration)
    {
        if (rectTransform == null) yield break;

        float elapsedTime = 0f;
        Vector2 startPos = new Vector2(startX, yPosition);
        Vector2 endPos = new Vector2(endX, yPosition);

        while (elapsedTime < duration)
        {
            if (rectTransform == null || rectTransform.gameObject == null)
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            yield return null;
        }

        if (rectTransform != null && rectTransform.gameObject != null)
        {
            Destroy(rectTransform.gameObject);
        }
    }

    public float GetDuration()
    {
        return skillDuration;
    }

    private void OnDrawGizmos()
    {
        if (trackYPositions == null || trackYPositions.Length == 0) return;

        RectTransform container = textContainer != null ? textContainer : canvasRect;
        if (container == null) return;

        float containerWidth = container.rect.width;
        float containerHeight = container.rect.height;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < trackYPositions.Length; i++)
        {
            float trackYRatio = trackYPositions[i];
            float yPosition = (trackYRatio - 0.5f) * containerHeight;

            Vector3 worldPos = container.TransformPoint(new Vector2(0, yPosition));

            float startX = containerWidth / 2 + spawnXOffset;
            float endX = -containerWidth / 2 + exitXOffset;

            Vector3 startWorld = container.TransformPoint(new Vector2(startX, yPosition));
            Vector3 endWorld = container.TransformPoint(new Vector2(endX, yPosition));

            Gizmos.DrawLine(startWorld, endWorld);

            #if UNITY_EDITOR
            UnityEditor.Handles.Label(worldPos, $"Track {i}");
            #endif
        }

        Gizmos.color = Color.green;
        float midY = 0f;
        Vector3 spawnPoint = container.TransformPoint(new Vector2(containerWidth / 2 + spawnXOffset, midY));
        Gizmos.DrawWireSphere(spawnPoint, 20f);

        Gizmos.color = Color.red;
        Vector3 exitPoint = container.TransformPoint(new Vector2(-containerWidth / 2 + exitXOffset, midY));
        Gizmos.DrawWireSphere(exitPoint, 20f);
    }
}
