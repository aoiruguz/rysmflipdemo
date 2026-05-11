using UnityEngine;
using TMPro; // 必须引用 TMP 命名空间
using UnityEngine.SceneManagement;
using System.Collections;

public class DisclaimerManager : MonoBehaviour
{
    [Header("UI 引用")]
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;

    [Header("设置")]
    public float fadeDuration = 2.0f; // 渐显持续时间
    public float waitTime = 3.0f;     // 显示停留时间
    public string nextSceneName;      // 下一个场景的名字

    void Start()
    {
        // 初始状态：全部透明
        SetAlpha(text1, 0);
        SetAlpha(text2, 0);

        // 开始流程
        StartCoroutine(DisclaimerRoutine());
    }

    IEnumerator DisclaimerRoutine()
    {
        // 1. 同时渐显两个文本
        float elapsed = 0;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);

            SetAlpha(text1, alpha);
            SetAlpha(text2, alpha);
            yield return null;
        }

        // 确保最终完全不透明
        SetAlpha(text1, 1);
        SetAlpha(text2, 1);

        // 2. 等待 3 秒
        yield return new WaitForSeconds(waitTime);

        // 3. 跳转场景
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("未设置跳转场景的名字！");
        }
    }

    // 设置 TMP 颜色的快捷方法
    void SetAlpha(TextMeshProUGUI text, float alpha)
    {
        if (text != null)
        {
            Color c = text.color;
            c.a = alpha;
            text.color = c;
        }
    }
}