using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DisclaimerManager : MonoBehaviour
{
    [Header("UI 组件")]
    public Image disclaimerImage;

    [Header("淡入淡出设置")]
    public float fadeDuration = 2.0f;
    public float waitTime = 3.0f;
    public string nextSceneName;

    [Header("屏幕震动设置")]
    public bool enableScreenShake = true;
    public float shakeIntensity = 2.0f;
    public float shakeSpeed = 15f;

    [Header("颜色随机设置")]
    public bool randomizeColor = true;

    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private Color[] colorOptions = new Color[]
    {
        new Color(1f, 1f, 1f, 1f),      // 255, 255, 255
        new Color(0f, 1f, 1f, 1f),      // 0, 255, 255
        new Color(1f, 0f, 1f, 1f),      // 255, 0, 255
        new Color(1f, 1f, 0f, 1f)       // 255, 255, 0
    };

    void Start()
    {
        // 初始化相机
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
            Debug.Log($"[DisclaimerManager] 找到主相机，初始位置: {originalCameraPosition}");
        }
        else
        {
            Debug.LogWarning("[DisclaimerManager] 未找到主相机，震动效果将不可用！");
        }

        // 随机颜色
        if (disclaimerImage != null)
        {
            if (randomizeColor)
            {
                Color randomColor = colorOptions[Random.Range(0, colorOptions.Length)];
                randomColor.a = 0; // 初始透明
                disclaimerImage.color = randomColor;
                Debug.Log($"[DisclaimerManager] 随机颜色: RGB({randomColor.r * 255}, {randomColor.g * 255}, {randomColor.b * 255})");
            }
            else
            {
                SetAlpha(disclaimerImage, 0);
            }
        }

        // 开始流程
        StartCoroutine(DisclaimerRoutine());
    }

    void Update()
    {
        // 屏幕震动效果
        if (enableScreenShake && mainCamera != null)
        {
            float offsetX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0) * 2 - 1);
            float offsetY = (Mathf.PerlinNoise(0, Time.time * shakeSpeed) * 2 - 1);

            Vector3 shakeOffset = new Vector3(offsetX, offsetY, 0) * shakeIntensity * 0.1f;
            mainCamera.transform.localPosition = originalCameraPosition + shakeOffset;
        }
        else if (mainCamera != null && !enableScreenShake)
        {
            mainCamera.transform.localPosition = originalCameraPosition;
        }
    }

    IEnumerator DisclaimerRoutine()
    {
        // 1. 淡入图片
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(disclaimerImage, alpha);
            yield return null;
        }

        // 确保完全不透明
        SetAlpha(disclaimerImage, 1);

        // 2. 等待
        yield return new WaitForSeconds(waitTime);

        // 3. 跳转场景
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("未设置跳转场景名称！");
        }
    }

    void SetAlpha(Image image, float alpha)
    {
        if (image != null)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }

    void OnDestroy()
    {
        // 恢复相机位置
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = originalCameraPosition;
        }
    }

    // 运行时切换震动效果
    public void ToggleScreenShake(bool enabled)
    {
        enableScreenShake = enabled;
        if (!enabled && mainCamera != null)
        {
            mainCamera.transform.localPosition = originalCameraPosition;
        }
    }
}