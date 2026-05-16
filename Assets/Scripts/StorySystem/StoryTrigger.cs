using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 剧情触发器组件
/// 挂载到场景对象上，配置触发时机后自动调用AVGStoryManager播放剧情
/// </summary>
public class StoryTrigger : MonoBehaviour
{
    [Header("触发配置")]
    [Tooltip("要触发的剧情ID，必须与JSON数据中的storyId一致")]
    public string storyId;

    [Tooltip("触发时机")]
    public TriggerMoment triggerMoment = TriggerMoment.OnStart;

    [Tooltip("是否只触发一次（推荐保持true，配合存档系统使用）")]
    public bool triggerOnce = true;

    [Tooltip("是否允许重复播放（true=每次都播放且不记录到存档，false=正常检查存档）")]
    public bool allowRepeat = false;

    [Header("剧情完成后触发移动")]
    [Tooltip("剧情播放完成后要触发的位置移动组件（可选）")]
    public PositionTransition positionTransition;

    [Header("回调事件")]
    [Tooltip("剧情播放完成后触发（无论是跳过还是正常结束）")]
    public UnityEvent onStoryComplete;

    private bool _hasTriggered = false;

    public enum TriggerMoment
    {
        OnStart,        // 场景Start时触发
        OnEnable,       // 组件Enable时触发
        Manual          // 手动调用Trigger()触发
    }

    private void Start()
    {
        if (triggerMoment == TriggerMoment.OnStart)
            Trigger();
    }

    private void OnEnable()
    {
        if (triggerMoment == TriggerMoment.OnEnable)
            Trigger();
    }

    /// <summary>
    /// 手动触发剧情（供外部代码调用）
    /// </summary>
    public void Trigger()
    {
        if (triggerOnce && _hasTriggered) return;

        if (string.IsNullOrEmpty(storyId))
        {
            Debug.LogWarning($"[StoryTrigger] {gameObject.name} 的storyId未配置！");
            return;
        }

        if (AVGStoryManager.Instance == null)
        {
            Debug.LogError("[StoryTrigger] 场景中未找到AVGStoryManager！");
            return;
        }

        _hasTriggered = true;

        // 如果允许重复播放，使用不保存到存档的播放方式
        if (allowRepeat)
        {
            AVGStoryManager.Instance.PlayStoryWithoutSaving(storyId, OnComplete);
        }
        else
        {
            bool started = AVGStoryManager.Instance.CheckAndPlayStory(storyId, OnComplete);
            if (!started)
            {
                // 剧情已看过或不存在，直接触发回调
                OnComplete();
            }
        }
    }

    /// <summary>
    /// 强制触发剧情（无论是否已观看）
    /// </summary>
    public void ForceTrigger()
    {
        if (string.IsNullOrEmpty(storyId))
        {
            Debug.LogWarning($"[StoryTrigger] {gameObject.name} 的storyId未配置！");
            return;
        }

        if (AVGStoryManager.Instance == null)
        {
            Debug.LogError("[StoryTrigger] 场景中未找到AVGStoryManager！");
            return;
        }

        _hasTriggered = true;
        AVGStoryManager.Instance.ForcePlayStory(storyId, OnComplete);
    }

    /// <summary>
    /// 重置触发状态（允许再次触发）
    /// </summary>
    public void ResetTrigger()
    {
        _hasTriggered = false;
    }

    private void OnComplete()
    {
        // 触发位置移动
        if (positionTransition != null)
        {
            positionTransition.StartMove();
            Debug.Log($"[StoryTrigger] 剧情 {storyId} 播放完成，触发位置移动");
        }

        // 触发回调事件
        onStoryComplete?.Invoke();
    }
}
