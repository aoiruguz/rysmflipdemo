using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EyeBlinkEffect : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("完全睁眼/闭眼动画的持续时间（秒）")]
    public float animationDuration = 1.5f;
    
    [Tooltip("是否在最终睁开前，先眨两次眼")]
    public bool blinkBeforeOpening = true;

    [Tooltip("是否在游戏开始时自动执行睁眼动画")]
    public bool openOnStart = true;
    
    [Tooltip("眼皮的颜色（通常是黑色）")]
    public Color eyelidColor = Color.black;

    private RectTransform topEyelid;
    private RectTransform bottomEyelid;
    private Image topImage;
    private Image bottomImage;
    
    private float currentProgress = 0f;

    private void Awake()
    {
        // 自动初始化UI结构
        InitializeUI();
    }

    private void Start()
    {
        if (openOnStart)
        {
            SetEyesStateImmediately(0f);
            OpenEyes();
        }
    }

    private void InitializeUI()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32767;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            gameObject.AddComponent<GraphicRaycaster>();
        }

        topEyelid = CreateEyelid("TopEyelid", true, out topImage);
        bottomEyelid = CreateEyelid("BottomEyelid", false, out bottomImage);
        
        SetEyesStateImmediately(0f);
    }

    private RectTransform CreateEyelid(string name, bool isTop, out Image img)
    {
        GameObject eyelidObj = new GameObject(name);
        eyelidObj.transform.SetParent(transform, false);

        img = eyelidObj.AddComponent<Image>();
        img.color = eyelidColor;
        
        // 核心：用代码动态生成带有弯曲弧度的眼皮贴图
        img.sprite = GenerateEyelidSprite(isTop);
        img.raycastTarget = true;

        RectTransform rect = eyelidObj.GetComponent<RectTransform>();
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return rect;
    }

    /// <summary>
    /// 用代码程序化生成带有弧度的眼皮贴图
    /// </summary>
    private Sprite GenerateEyelidSprite(bool isTop)
    {
        int width = 512;
        int height = 512;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        
        Color[] pixels = new Color[width * height];
        
        for (int x = 0; x < width; x++)
        {
            float u = (float)x / (width - 1);
            // 用Sin函数生成弯曲的弧线
            float arc = Mathf.Sin(u * Mathf.PI);
            
            // 顶部眼皮中间往上凹，底部眼皮中间往下凹
            float edgeV = isTop ? (0.5f + arc * 0.3f) : (0.5f - arc * 0.3f);
            
            for (int y = 0; y < height; y++)
            {
                float v = (float)y / (height - 1);
                float distPixels = (v - edgeV) * height;
                
                // 边缘抗锯齿处理，让曲线更平滑
                float alpha = isTop ? Mathf.Clamp01(distPixels + 0.5f) : Mathf.Clamp01(-distPixels + 0.5f);
                pixels[y * width + x] = new Color(1, 1, 1, alpha);
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    public void OpenEyes()
    {
        StopAllCoroutines();
        if (blinkBeforeOpening)
        {
            StartCoroutine(AnimateBlinkSequence());
        }
        else
        {
            StartCoroutine(MoveToProgress(1f, animationDuration));
        }
    }

    public void CloseEyes()
    {
        StopAllCoroutines();
        StartCoroutine(MoveToProgress(0f, animationDuration));
    }

    public void SetEyesStateImmediately(float progress)
    {
        StopAllCoroutines();
        currentProgress = Mathf.Clamp01(progress);
        SetEyelidPosition(currentProgress);
        
        bool isFullyOpen = currentProgress >= 1f;
        topImage.raycastTarget = !isFullyOpen;
        bottomImage.raycastTarget = !isFullyOpen;
    }

    private IEnumerator AnimateBlinkSequence()
    {
        // 第一次眨眼：只睁开一点点 (20%)
        yield return StartCoroutine(MoveToProgress(0.2f, 0.3f));
        yield return StartCoroutine(MoveToProgress(0.0f, 0.2f));
        yield return new WaitForSeconds(0.15f);

        // 第二次眨眼：睁开多一点 (40%)
        yield return StartCoroutine(MoveToProgress(0.4f, 0.3f));
        yield return StartCoroutine(MoveToProgress(0.0f, 0.2f));
        yield return new WaitForSeconds(0.2f);

        // 完全睁开：慢慢睁开到 100%
        yield return StartCoroutine(MoveToProgress(1.0f, animationDuration));
    }

    private IEnumerator MoveToProgress(float targetProgress, float duration)
    {
        float startProgress = currentProgress;
        float elapsed = 0f;

        topImage.raycastTarget = true;
        bottomImage.raycastTarget = true;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = t * t * (3f - 2f * t); // 平滑缓动
            
            currentProgress = Mathf.Lerp(startProgress, targetProgress, easeT);
            SetEyelidPosition(currentProgress);
            
            yield return null;
        }

        currentProgress = targetProgress;
        SetEyelidPosition(currentProgress);

        if (currentProgress >= 1f)
        {
            topImage.raycastTarget = false;
            bottomImage.raycastTarget = false;
        }
    }

    private void SetEyelidPosition(float progress)
    {
        // progress: 0 = 闭眼, 1 = 睁眼
        
        // 闭眼时两者交叉重叠覆盖全屏，睁眼时移出屏幕
        float topOffset = Mathf.Lerp(-0.3f, 0.5f, progress);
        topEyelid.anchorMin = new Vector2(0, topOffset);
        topEyelid.anchorMax = new Vector2(1, 1f + topOffset);
        
        float bottomOffset = Mathf.Lerp(0.3f, -0.5f, progress);
        bottomEyelid.anchorMin = new Vector2(0, bottomOffset);
        bottomEyelid.anchorMax = new Vector2(1, 1f + bottomOffset);
    }

    private void OnDestroy()
    {
        // 释放动态生成的贴图内存
        if (topImage != null && topImage.sprite != null)
            Destroy(topImage.sprite.texture);
        if (bottomImage != null && bottomImage.sprite != null)
            Destroy(bottomImage.sprite.texture);
    }
}
