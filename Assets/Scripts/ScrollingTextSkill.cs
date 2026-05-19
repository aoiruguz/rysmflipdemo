using UnityEngine;
using TMPro;
using System.Collections;

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

    [HideInInspector]
    public TMP_FontAsset globalFont;

    [Header("弹幕进阶设置")]
    [Tooltip("存放弹幕文本的空物体容器，并作为生成范围限制")]
    public RectTransform textContainer;
    
    [Tooltip("字体大小随机范围")]
    public Vector2 fontSizeRange = new Vector2(30f, 60f);

    [Tooltip("弹幕出现的Y坐标范围比例（例如0.6到0.9代表屏幕上半部分）")]
    public Vector2 yPositionRange = new Vector2(0.6f, 0.9f);
    
    [Tooltip("生成位置的额外随机偏移范围(X和Y)")]
    public Vector2 positionOffsetRange = new Vector2(-50f, 50f);
    
    [Tooltip("移动速度(持续时间)的随机范围")]
    public Vector2 durationRange = new Vector2(4f, 8f);

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

        if (interferenceCanvas == null && textContainer == null)
        {
            Debug.LogError("[ScrollingTextSkill] Interference Canvas and textContainer are both null!");
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
        // 确定父容器
        RectTransform container = textContainer != null ? textContainer : canvasRect;

        // 创建弹幕GameObject
        GameObject bulletObj = new GameObject("ScrollingBullet");
        bulletObj.transform.SetParent(container, false);

        // 添加RectTransform
        RectTransform rectTransform = bulletObj.AddComponent<RectTransform>();

        // 设置锚点和轴心点（重要！）
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // 锚点在容器中心
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);     // 轴心点在文本中心
        rectTransform.sizeDelta = new Vector2(800f, 100f); // 设置文本框大小

        // 添加TextMeshProUGUI组件
        TextMeshProUGUI textComponent = bulletObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = config.GetRandomText();
        textComponent.fontSize = Random.Range(fontSizeRange.x, fontSizeRange.y);
        textComponent.color = config.textColor; // 直接使用配置颜色的Alpha
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.textWrappingMode = TextWrappingModes.NoWrap;

        // 使用全局配置的中文字体
        if (globalFont != null)
        {
            textComponent.font = globalFont;
        }
        else
        {
            Debug.LogWarning("[ScrollingTextSkill] No global Chinese font assigned in InterferenceManager!");
        }

        // 设置初始位置（容器右侧外）
        float containerWidth = container.rect.width;
        float containerHeight = container.rect.height;

        // 基础随机Y坐标
        float yPositionRatio = Random.Range(yPositionRange.x, yPositionRange.y);
        float baseYPosition = (yPositionRatio - 0.5f) * containerHeight;

        // 添加随机偏移
        float randomOffsetX = Random.Range(positionOffsetRange.x, positionOffsetRange.y);
        float randomOffsetY = Random.Range(positionOffsetRange.x, positionOffsetRange.y);

        // 计算带偏移的最终位置
        float finalX = containerWidth / 2 + 200f + randomOffsetX;
        float finalY = baseYPosition + randomOffsetY;

        // 将Y坐标限制在容器范围内 (减去一半的文本高度避免出界)
        float maxY = containerHeight / 2 - 50f;
        float minY = -containerHeight / 2 + 50f;
        finalY = Mathf.Clamp(finalY, minY, maxY);

        rectTransform.anchoredPosition = new Vector2(finalX, finalY);

        // 获取随机持续时间(速度)
        float randomDuration = Random.Range(durationRange.x, durationRange.y);

        // 启动滚动协程
        StartCoroutine(ScrollBullet(rectTransform, containerWidth, randomDuration));
    }

    /// <summary>
    /// 弹幕滚动协程
    /// </summary>
    private IEnumerator ScrollBullet(RectTransform rectTransform, float containerWidth, float duration)
    {
        if (rectTransform == null) yield break;

        float elapsedTime = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = new Vector2(-containerWidth / 2 - 200f, startPos.y); // 终点在容器左侧外

        Debug.Log($"[ScrollingTextSkill] Bullet start: {startPos}, end: {endPos}, duration: {duration}");

        while (elapsedTime < duration)
        {
            // 检查对象是否还存在
            if (rectTransform == null || rectTransform.gameObject == null)
            {
                Debug.LogWarning("[ScrollingTextSkill] Bullet destroyed prematurely");
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

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
        return durationRange.y;
    }
}
