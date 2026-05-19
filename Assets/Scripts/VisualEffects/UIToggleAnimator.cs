using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 专用于 Toggle 的辅助脚本，监听状态切换并控制滑钮（Handle/Checkmark）平滑移动。
/// 需要项目已安装 DOTween。
/// </summary>
[RequireComponent(typeof(Toggle))]
public class UIToggleAnimator : MonoBehaviour
{
    [Header("UI Reference")]
    [Tooltip("需要移动的滑钮或手柄的 RectTransform")]
    public RectTransform handleRect;

    [Header("Position Settings")]
    [Tooltip("Toggle 开启 (On) 时，滑钮的目标 X 坐标")]
    public float onPosX = 25f;
    [Tooltip("Toggle 关闭 (Off) 时，滑钮的目标 X 坐标")]
    public float offPosX = -25f;

    [Header("Animation Settings")]
    [Tooltip("动画持续时间")]
    public float duration = 0.2f;
    [Tooltip("动画曲线类型")]
    public Ease animationEase = Ease.InOutQuad;

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        
        // 初始化位置（无动画，直接到位）
        if (handleRect != null)
        {
            float targetX = toggle.isOn ? onPosX : offPosX;
            Vector2 pos = handleRect.anchoredPosition;
            pos.x = targetX;
            handleRect.anchoredPosition = pos;
        }
    }

    private void OnEnable()
    {
        // 动态绑定监听事件
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    private void OnDisable()
    {
        // 移除监听
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }

    /// <summary>
    /// 当 Toggle 状态改变时调用
    /// </summary>
    public void OnToggleChanged(bool isOn)
    {
        if (handleRect == null) return;

        // 停止该物体上正在进行的 DOTween 动画，防止冲突
        handleRect.DOKill();

        float targetX = isOn ? onPosX : offPosX;

        // 使用 DOTween 平滑移动到目标位置
        handleRect.DOAnchorPosX(targetX, duration)
            .SetEase(animationEase)
            .SetUpdate(true); // 即使 Time.timeScale 为 0 (暂停) 也能运行
    }

    // 右键菜单：方便在编辑器里手动同步位置
    [ContextMenu("Sync Position Now")]
    private void SyncPosition()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
        if (handleRect != null)
        {
            float targetX = toggle.isOn ? onPosX : offPosX;
            Vector2 pos = handleRect.anchoredPosition;
            pos.x = targetX;
            handleRect.anchoredPosition = pos;
        }
    }
}
