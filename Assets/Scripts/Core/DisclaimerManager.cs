using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using TMPro;

public class DisclaimerManager : MonoBehaviour
{
    [Header("UI 组件")]
    public CanvasGroup canvasGroup;

    [Header("淡入淡出设置")]
    public float fadeDuration = 2.0f;
    public float waitTime = 3.0f;
    public string nextSceneName;

    [Header("颜色随机设置")]
    public bool randomizeColor = true;
    public Color[] colorOptions = new Color[]
    {
        new Color(1f, 1f, 1f, 1f),      // 白色
        new Color(0f, 1f, 1f, 1f),      // 青色
        new Color(1f, 0f, 1f, 1f),      // 紫色
        new Color(1f, 1f, 0f, 1f)       // 黄色
    };

    [Header("需要修改颜色的物体")]
    public TMP_Text tmpText1;
    public TMP_Text tmpText2;
    public Image image;

    void Start()
    {
        // 自动获取组件
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        // 初始化状态
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }

        // 随机颜色并应用到指定的UI物体
        if (randomizeColor)
        {
            Color randomColor = colorOptions[Random.Range(0, colorOptions.Length)];

            // 修改指定的UI物体颜色
            if (tmpText1 != null)
            {
                tmpText1.color = randomColor;
            }
            if (tmpText2 != null)
            {
                tmpText2.color = randomColor;
            }
            if (image != null)
            {
                image.color = randomColor;
            }
        }

        StartCoroutine(DisclaimerRoutine());
    }

    IEnumerator DisclaimerRoutine()
    {
        if (canvasGroup == null)
        {
            Debug.LogError("[DisclaimerManager] 当前物体上未找到 CanvasGroup，且未手动分配！");
            yield break;
        }

        // 1. 淡入
        yield return canvasGroup.DOFade(1, fadeDuration).WaitForCompletion();

        // 2. 等待
        yield return new WaitForSeconds(waitTime);

        // 3. 淡出
        yield return canvasGroup.DOFade(0, fadeDuration).WaitForCompletion();

        // 4. 跳转场景
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("未设置跳转场景名称！");
        }
    }
}



