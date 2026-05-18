using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 让 TextMeshPro 文字或 Image 在屏幕中斜飞并反弹，同时颜色渐变
/// 支持 TextMeshProUGUI 和 Image 组件
/// </summary>
public class BouncingTextEffect : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("移动速度（像素/秒）")]
    public float speed = 200f;

    [Tooltip("初始移动方向（会被归一化）")]
    public Vector2 initialDirection = new Vector2(1f, 1f);

    [Header("颜色渐变设置")]
    [Tooltip("颜色渐变速度（0-1循环的速度）")]
    public float colorChangeSpeed = 0.5f;

    [Tooltip("使用渐变色（如果不设置则使用HSV彩虹色）")]
    public Gradient colorGradient;

    [Tooltip("如果为true，使用HSV彩虹色循环")]
    public bool useRainbowColor = true;

    [Header("边界设置")]
    [Tooltip("边界内缩（避免文字完全贴边）")]
    public float boundaryPadding = 50f;

    [Header("调试")]
    [Tooltip("显示调试信息")]
    public bool showDebug = false;

    [Header("组件设置")]
    [Tooltip("如果对象有Button组件，是否禁用交互（避免点击冲突）")]
    public bool disableButtonInteraction = true;

    private RectTransform rectTransform;
    private TextMeshProUGUI tmpText;
    private Image image;
    private Button button;
    private Vector2 velocity;
    private float colorTime = 0f;
    private Canvas canvas;
    private RectTransform canvasRect;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        tmpText = GetComponent<TextMeshProUGUI>();
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        if (rectTransform == null)
        {
            Debug.LogError("[BouncingTextEffect] 需要 RectTransform 组件！");
            enabled = false;
            return;
        }

        if (tmpText == null && image == null)
        {
            Debug.LogError("[BouncingTextEffect] 需要 TextMeshProUGUI 或 Image 组件！");
            enabled = false;
            return;
        }

        // 如果有Button组件，根据设置禁用交互
        if (button != null && disableButtonInteraction)
        {
            button.interactable = false;
            if (showDebug)
            {
                Debug.Log("[BouncingText] 检测到Button组件，已禁用交互");
            }
        }

        // 获取Canvas
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("[BouncingTextEffect] 找不到父级 Canvas！");
            enabled = false;
            return;
        }

        // 初始化速度方向
        velocity = initialDirection.normalized * speed;

        // 设置文字的锚点和pivot为中心
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // 强制刷新布局
        Canvas.ForceUpdateCanvases();

        if (showDebug)
        {
            Debug.Log($"[BouncingText] 初始化完成");
            Debug.Log($"组件类型: TMP={tmpText != null}, Image={image != null}, Button={button != null}");
            Debug.Log($"Canvas尺寸: {canvasRect.rect.size}");
            Debug.Log($"Canvas锚点: Min={canvasRect.anchorMin}, Max={canvasRect.anchorMax}");
            Debug.Log($"对象尺寸: {rectTransform.rect.size}");
            Debug.Log($"对象锚点: Min={rectTransform.anchorMin}, Max={rectTransform.anchorMax}");
            Debug.Log($"初始速度: {velocity}");
        }

        // 如果没有设置渐变色，创建一个默认的彩虹渐变
        if (colorGradient == null && !useRainbowColor)
        {
            colorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[7];
            colorKeys[0] = new GradientColorKey(Color.red, 0f);
            colorKeys[1] = new GradientColorKey(Color.yellow, 0.166f);
            colorKeys[2] = new GradientColorKey(Color.green, 0.333f);
            colorKeys[3] = new GradientColorKey(Color.cyan, 0.5f);
            colorKeys[4] = new GradientColorKey(Color.blue, 0.666f);
            colorKeys[5] = new GradientColorKey(Color.magenta, 0.833f);
            colorKeys[6] = new GradientColorKey(Color.red, 1f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            colorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    void Update()
    {
        if (rectTransform == null || tmpText == null || canvasRect == null)
            return;

        // 更新位置
        Vector2 currentPos = rectTransform.anchoredPosition;
        currentPos += velocity * Time.deltaTime;

        // 获取屏幕边界（基于Canvas尺寸）
        float halfWidth = rectTransform.rect.width / 2f;
        float halfHeight = rectTransform.rect.height / 2f;

        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // 使用世界坐标转换来获取准确的屏幕边界
        Vector3[] canvasCorners = new Vector3[4];
        canvasRect.GetWorldCorners(canvasCorners);

        // 转换为Canvas本地坐标
        Vector2 canvasMin = canvasRect.InverseTransformPoint(canvasCorners[0]);
        Vector2 canvasMax = canvasRect.InverseTransformPoint(canvasCorners[2]);

        float minX = canvasMin.x + halfWidth + boundaryPadding;
        float maxX = canvasMax.x - halfWidth - boundaryPadding;
        float minY = canvasMin.y + halfHeight + boundaryPadding;
        float maxY = canvasMax.y - halfHeight - boundaryPadding;

        if (showDebug && Time.frameCount % 60 == 0) // 每60帧打印一次
        {
            Debug.Log($"[BouncingText] 位置: {currentPos:F1}, 边界: X[{minX:F1}, {maxX:F1}] Y[{minY:F1}, {maxY:F1}], 速度: {velocity:F1}");
        }

        // 检测碰撞并反弹
        bool bounced = false;

        if (currentPos.x <= minX)
        {
            velocity.x = Mathf.Abs(velocity.x); // 确保向右
            currentPos.x = minX;
            bounced = true;
            if (showDebug) Debug.Log("[BouncingText] 碰到左边界");
        }
        else if (currentPos.x >= maxX)
        {
            velocity.x = -Mathf.Abs(velocity.x); // 确保向左
            currentPos.x = maxX;
            bounced = true;
            if (showDebug) Debug.Log("[BouncingText] 碰到右边界");
        }

        if (currentPos.y <= minY)
        {
            velocity.y = Mathf.Abs(velocity.y); // 确保向上
            currentPos.y = minY;
            bounced = true;
            if (showDebug) Debug.Log("[BouncingText] 碰到下边界");
        }
        else if (currentPos.y >= maxY)
        {
            velocity.y = -Mathf.Abs(velocity.y); // 确保向下
            currentPos.y = maxY;
            bounced = true;
            if (showDebug) Debug.Log("[BouncingText] 碰到上边界");
        }

        // 反弹时可以添加一些随机性，让运动更有趣
        if (bounced)
        {
            // 可选：添加轻微的角度变化
            float angleVariation = Random.Range(-5f, 5f);
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            angle += angleVariation;
            float magnitude = velocity.magnitude;
            velocity = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * magnitude,
                Mathf.Sin(angle * Mathf.Deg2Rad) * magnitude
            );
        }

        rectTransform.anchoredPosition = currentPos;

        // 更新颜色
        UpdateColor();
    }

    void UpdateColor()
    {
        colorTime += Time.deltaTime * colorChangeSpeed;
        if (colorTime > 1f)
            colorTime -= 1f;

        Color newColor;

        if (useRainbowColor)
        {
            // 使用HSV彩虹色
            newColor = Color.HSVToRGB(colorTime, 1f, 1f);
        }
        else if (colorGradient != null)
        {
            // 使用自定义渐变
            newColor = colorGradient.Evaluate(colorTime);
        }
        else
        {
            newColor = Color.white;
        }

        // 应用颜色到对应的组件
        if (tmpText != null)
        {
            tmpText.color = newColor;
        }

        if (image != null)
        {
            image.color = newColor;
        }
    }

    // 可以在运行时改变速度
    public void SetSpeed(float newSpeed)
    {
        float currentMagnitude = velocity.magnitude;
        if (currentMagnitude > 0)
        {
            velocity = velocity.normalized * newSpeed;
        }
        speed = newSpeed;
    }

    // 可以在运行时改变方向
    public void SetDirection(Vector2 newDirection)
    {
        velocity = newDirection.normalized * speed;
    }
}
