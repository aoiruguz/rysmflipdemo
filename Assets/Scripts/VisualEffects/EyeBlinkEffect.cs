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
    private Image topFiller;
    private Image bottomFiller;
    
    private float currentProgress = 0f;

    private void Awake()
    {
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

        topEyelid = CreateEyelid("TopEyelid", true, out topImage, out topFiller);
        bottomEyelid = CreateEyelid("BottomEyelid", false, out bottomImage, out bottomFiller);
        
        SetEyesStateImmediately(0f);
    }

    private RectTransform CreateEyelid(string name, bool isTop, out Image edgeImg, out Image fillerImg)
    {
        // 1. 创建父容器
        GameObject containerObj = new GameObject(name);
        containerObj.transform.SetParent(transform, false);
        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        
        // 2. 创建带有弧度的边缘图片 (Edge)
        GameObject edgeObj = new GameObject("Edge");
        edgeObj.transform.SetParent(containerObj.transform, false);
        edgeImg = edgeObj.AddComponent<Image>();
        edgeImg.color = eyelidColor;
        edgeImg.sprite = GenerateEyelidSprite(isTop);
        edgeImg.raycastTarget = true;
        
        RectTransform edgeRect = edgeObj.GetComponent<RectTransform>();
        edgeRect.anchorMin = new Vector2(0, 0);
        edgeRect.anchorMax = new Vector2(1, 1);
        edgeRect.offsetMin = Vector2.zero;
        edgeRect.offsetMax = Vector2.zero;

        // 3. 创建用来填补背景漏出部分的纯色图片 (Filler)
        GameObject fillerObj = new GameObject("Filler");
        fillerObj.transform.SetParent(containerObj.transform, false);
        fillerImg = fillerObj.AddComponent<Image>();
        fillerImg.color = eyelidColor;
        fillerImg.raycastTarget = true;
        
        RectTransform fillerRect = fillerObj.GetComponent<RectTransform>();
        if (isTop)
        {
            // 上眼皮的填充物：固定在边缘图片的顶部，向上延伸3000像素防漏
            fillerRect.anchorMin = new Vector2(0, 1);
            fillerRect.anchorMax = new Vector2(1, 1);
            fillerRect.offsetMin = new Vector2(0, 0);
            fillerRect.offsetMax = new Vector2(0, 3000); 
        }
        else
        {
            // 下眼皮的填充物：固定在边缘图片的底部，向下延伸3000像素防漏
            fillerRect.anchorMin = new Vector2(0, 0);
            fillerRect.anchorMax = new Vector2(1, 0);
            fillerRect.offsetMin = new Vector2(0, -3000); 
            fillerRect.offsetMax = new Vector2(0, 0);
        }

        return containerRect;
    }

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
            float arc = Mathf.Sin(u * Mathf.PI);
            
            float edgeV = isTop ? (0.5f + arc * 0.3f) : (0.5f - arc * 0.3f);
            
            for (int y = 0; y < height; y++)
            {
                float v = (float)y / (height - 1);
                float distPixels = (v - edgeV) * height;
                
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
        SetRaycastState(currentProgress < 1f);
    }

    private IEnumerator AnimateBlinkSequence()
    {
        yield return StartCoroutine(MoveToProgress(0.2f, 0.3f));
        yield return StartCoroutine(MoveToProgress(0.0f, 0.2f));
        yield return new WaitForSeconds(0.15f);

        yield return StartCoroutine(MoveToProgress(0.4f, 0.3f));
        yield return StartCoroutine(MoveToProgress(0.0f, 0.2f));
        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(MoveToProgress(1.0f, animationDuration));
    }

    private IEnumerator MoveToProgress(float targetProgress, float duration)
    {
        float startProgress = currentProgress;
        float elapsed = 0f;

        SetRaycastState(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = t * t * (3f - 2f * t); 
            
            currentProgress = Mathf.Lerp(startProgress, targetProgress, easeT);
            SetEyelidPosition(currentProgress);
            
            yield return null;
        }

        currentProgress = targetProgress;
        SetEyelidPosition(currentProgress);

        SetRaycastState(currentProgress < 1f);
    }

    private void SetEyelidPosition(float progress)
    {
        float topOffset = Mathf.Lerp(-0.3f, 0.5f, progress);
        topEyelid.anchorMin = new Vector2(0, topOffset);
        topEyelid.anchorMax = new Vector2(1, 1f + topOffset);
        
        float bottomOffset = Mathf.Lerp(0.3f, -0.5f, progress);
        bottomEyelid.anchorMin = new Vector2(0, bottomOffset);
        bottomEyelid.anchorMax = new Vector2(1, 1f + bottomOffset);
    }

    private void SetRaycastState(bool state)
    {
        if (topImage != null) topImage.raycastTarget = state;
        if (bottomImage != null) bottomImage.raycastTarget = state;
        if (topFiller != null) topFiller.raycastTarget = state;
        if (bottomFiller != null) bottomFiller.raycastTarget = state;
    }

    private void OnDestroy()
    {
        if (topImage != null && topImage.sprite != null)
        {
            Destroy(topImage.sprite.texture);
            Destroy(topImage.sprite);
        }
        if (bottomImage != null && bottomImage.sprite != null)
        {
            Destroy(bottomImage.sprite.texture);
            Destroy(bottomImage.sprite);
        }
    }
}
