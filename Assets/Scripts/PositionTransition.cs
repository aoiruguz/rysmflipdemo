using UnityEngine;
using UnityEngine.SceneManagement;

public class PositionTransition : MonoBehaviour
{
    [Header("移动点位")]
    [Tooltip("起始点（拖拽空物体到这里）")]
    public Transform pointA;

    [Tooltip("目标点（拖拽空物体到这里）")]
    public Transform pointB;

    [Header("移动设置")]
    [Tooltip("移动时间（秒）")]
    public float duration = 1f;

    [Tooltip("缓动曲线")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("可选设置")]
    [Tooltip("移动的目标物体（如果为空则移动自身）")]
    public Transform targetObject;

    [Header("场景跳转设置")]
    [Tooltip("移动完成后是否跳转场景")]
    public bool loadSceneAfterMove = false;

    [Tooltip("要跳转的场景名称")]
    public string targetSceneName = "";

    [Tooltip("移动完成后延迟多少秒跳转场景")]
    public float delayBeforeSceneLoad = 0f;

    private bool isMoving = false;

    /// <summary>
    /// 开始从A点移动到B点
    /// </summary>
    public void StartMove()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("PositionTransition: 起始点或目标点未设置！");
            return;
        }

        if (isMoving)
        {
            Debug.LogWarning("PositionTransition: 已经在移动中！");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(MoveCoroutine());
    }

    private System.Collections.IEnumerator MoveCoroutine()
    {
        isMoving = true;

        Transform moveTarget = targetObject != null ? targetObject : transform;
        Vector3 startPosition = pointA.position;
        Vector3 endPosition = pointB.position;

        Debug.Log($"[PositionTransition] 开始移动 - 起点: {startPosition}, 终点: {endPosition}");
        Debug.Log($"[PositionTransition] 移动物体: {moveTarget.name}");

        moveTarget.position = startPosition;
        Debug.Log($"[PositionTransition] 设置初始位置后，物体实际位置: {moveTarget.position}");

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = easeCurve.Evaluate(t);

            moveTarget.position = Vector3.Lerp(startPosition, endPosition, curveValue);

            yield return null;
        }

        moveTarget.position = endPosition;
        Debug.Log($"[PositionTransition] 移动完成 - 最终位置: {moveTarget.position}, 目标位置: {endPosition}");
        Debug.Log($"[PositionTransition] 位置差异: {Vector3.Distance(moveTarget.position, endPosition)}");

        isMoving = false;

        // 如果需要跳转场景
        if (loadSceneAfterMove && !string.IsNullOrEmpty(targetSceneName))
        {
            if (delayBeforeSceneLoad > 0)
            {
                Debug.Log($"[PositionTransition] 将在 {delayBeforeSceneLoad} 秒后跳转到场景: {targetSceneName}");
                yield return new WaitForSeconds(delayBeforeSceneLoad);
            }

            Debug.Log($"[PositionTransition] 正在跳转到场景: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
    }

    /// <summary>
    /// 检查是否正在移动
    /// </summary>
    public bool IsMoving()
    {
        return isMoving;
    }

    /// <summary>
    /// 停止移动
    /// </summary>
    public void StopMove()
    {
        StopAllCoroutines();
        isMoving = false;
    }
}
