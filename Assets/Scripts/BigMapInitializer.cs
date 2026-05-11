using UnityEngine;

/// <summary>
/// Big Map场景初始化脚本
/// 负责在第一次进入时触发剧情
/// </summary>
public class BigMapInitializer : MonoBehaviour
{
    [Header("剧情配置")]
    [Tooltip("要触发的剧情ID")]
    public string storyId = "Level_04_Opening";

    private void Start()
    {
        // 检查并播放剧情（只在第一次播放）
        if (AVGStoryManager.Instance != null)
        {
            AVGStoryManager.Instance.CheckAndPlayStory(storyId, OnStoryComplete);
        }
        else
        {
            Debug.LogWarning("[BigMapInitializer] AVGStoryManager未找到！");
            OnStoryComplete();
        }
    }

    private void OnStoryComplete()
    {
        // 剧情播放完成后的逻辑
        Debug.Log("[BigMapInitializer] 剧情播放完成，Big Map已就绪");

        // 这里可以添加其他初始化逻辑
        // 例如：启用UI、开始背景音乐等
    }
}
