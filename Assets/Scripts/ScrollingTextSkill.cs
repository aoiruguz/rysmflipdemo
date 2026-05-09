using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 横向滚动弹幕技能
/// 在屏幕上半部分生成横向滚动的文字来干扰玩家
/// </summary>
public class ScrollingTextSkill : MonoBehaviour
{
    [Header("配置")]
    public InterferenceTextConfig config;

    [Header("UI引用")]
    public Canvas interferenceCanvas;
    public RectTransform canvasRect;

    private Coroutine spawnCoroutine; // 保存生成协程的引用

    /// <summary>
    /// 激活技能，生成弹幕
    /// </summary>
    public void Activate()
    {
        if (config == null)
        {
            Debug.LogError("[ScrollingTextSkill] Config is null!");
            return;
        }

        if (interferenceCanvas == null)
        {
            Debug.LogError("[ScrollingTextSkill] Interference Canvas is null!");
            return;
        }

        spawnCoroutine = StartCoroutine(SpawnBullets());
    }

    /// <summary>
    /// 停用技能
    /// </summary>
    public void Deactivate()
    {
        // 只停止生成协程，不停止已经在飞行的弹幕协程
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    /// <summary>
    /// 生成多条弹幕
    /// </summary>
    private IEnumerator SpawnBullets()
    {
        for (int i = 0; i < config.simultaneousCount; i++)
        {
            SpawnSingleBullet();
            // 每条弹幕之间间隔一小段时间
            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// 生成单条弹幕
    /// </summary>
    private void SpawnSingleBullet()
    {
        // 创建弹幕GameObject
        GameObject bulletObj = new GameObject("ScrollingBullet");
        bulletObj.transform.SetParent(interferenceCanvas.transform, false);

        // 添加RectTransform
        RectTransform rectTransform = bulletObj.AddComponent<RectTransform>();

        // 设置锚点和轴心点（重要！）
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // 锚点在Canvas中心
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);     // 轴心点在文本中心
        rectTransform.sizeDelta = new Vector2(800f, 100f); // 设置文本框大小

        // 添加TextMeshProUGUI组件
        TextMeshProUGUI textComponent = bulletObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = config.GetRandomText();
        textComponent.fontSize = config.fontSize;
        textComponent.color = new Color(config.textColor.r, config.textColor.g, config.textColor.b, config.alpha);
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.textWrappingMode = TextWrappingModes.NoWrap;

        // 使用配置中的中文字体
        if (config.chineseFont != null)
        {
            textComponent.font = config.chineseFont;
            Debug.Log($"[ScrollingTextSkill] Using font: {config.chineseFont.name}");
            Debug.Log($"[ScrollingTextSkill] Font atlas population mode: {config.chineseFont.atlasPopulationMode}");
            Debug.Log($"[ScrollingTextSkill] Source font file: {config.chineseFont.sourceFontFile}");
            Debug.Log($"[ScrollingTextSkill] Text to display: {config.GetRandomText()}");
        }
        else
        {
            Debug.LogWarning("[ScrollingTextSkill] No Chinese font assigned in config! Chinese characters will not display correctly.");
        }

        // 设置初始位置（屏幕右侧外）
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // 随机Y坐标（屏幕上半部分）
        float yPositionRatio = config.GetRandomYPosition();
        float yPosition = (yPositionRatio - 0.5f) * canvasHeight;

        // 起始位置在屏幕右侧外
        rectTransform.anchoredPosition = new Vector2(canvasWidth / 2 + 200f, yPosition);

        // 启动滚动协程
        StartCoroutine(ScrollBullet(rectTransform, canvasWidth));
    }

    /// <summary>
    /// 弹幕滚动协程
    /// </summary>
    private IEnumerator ScrollBullet(RectTransform rectTransform, float canvasWidth)
    {
        if (rectTransform == null) yield break;

        float elapsedTime = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = new Vector2(-canvasWidth / 2 - 200f, startPos.y); // 终点在屏幕左侧外

        Debug.Log($"[ScrollingTextSkill] Bullet start: {startPos}, end: {endPos}, duration: {config.duration}");

        while (elapsedTime < config.duration)
        {
            // 检查对象是否还存在
            if (rectTransform == null || rectTransform.gameObject == null)
            {
                Debug.LogWarning("[ScrollingTextSkill] Bullet destroyed prematurely");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / config.duration;

            // 线性插值移动
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);

            yield return null;
        }

        // 销毁弹幕对象
        if (rectTransform != null && rectTransform.gameObject != null)
        {
            Debug.Log($"[ScrollingTextSkill] Destroying bullet at position: {rectTransform.anchoredPosition}");
            Destroy(rectTransform.gameObject);
        }
    }

    /// <summary>
    /// 获取技能持续时间
    /// </summary>
    public float GetDuration()
    {
        return config != null ? config.duration : 3f;
    }
}
