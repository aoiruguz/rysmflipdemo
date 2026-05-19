using UnityEngine;

public class ScaleTransition : MonoBehaviour
{
    [Header("目标物体")]
    [Tooltip("拖拽需要缩放的物体到这里")]
    public Transform targetObject;

    [Header("缩放设置")]
    [Tooltip("起始缩放值")]
    public Vector3 startScale = Vector3.one;

    [Tooltip("目标缩放值")]
    public Vector3 targetScale = Vector3.one * 2f;

    [Tooltip("过渡时间（秒）")]
    public float duration = 1f;

    [Header("可选设置")]
    [Tooltip("是否在场景启动时自动播放")]
    public bool playOnStart = true;

    [Tooltip("缓动曲线")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void Start()
    {
        if (playOnStart)
        {
            PlayTransition();
        }
    }

    public void PlayTransition()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("ScaleTransition: 目标物体未设置！");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ScaleCoroutine());
    }

    private System.Collections.IEnumerator ScaleCoroutine()
    {
        targetObject.localScale = startScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = easeCurve.Evaluate(t);

            targetObject.localScale = Vector3.Lerp(startScale, targetScale, curveValue);

            yield return null;
        }

        targetObject.localScale = targetScale;
    }
}
