using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// AVG剧情系统管理器（单例）
/// 负责加载剧情数据、检查触发条件、驱动剧情播放
/// 使用方式：AVGStoryManager.Instance.CheckAndPlayStory("storyId", callback);
/// </summary>
public class AVGStoryManager : MonoBehaviour
{
    public static AVGStoryManager Instance { get; private set; }

    [Header("剧情播放器")]
    [Tooltip("场景中的StoryPlayer组件引用")]
    public StoryPlayer storyPlayer;

    [Header("数据配置")]
    [Tooltip("剧情JSON文件名（不含扩展名），放在Resources/StoryData/目录下")]
    public string storyDataFileName = "stories";

    private Dictionary<string, StoryData> _storiesById = new Dictionary<string, StoryData>();
    private bool _isPlaying = false;

    private void Awake()
    {
        // 不使用单例模式，允许每个场景有自己的 AVGStoryManager
        Instance = this;
        LoadStoryData();
    }

    // ===================== 数据加载 =====================

    /// <summary>
    /// 从Resources加载剧情JSON数据
    /// </summary>
    public void LoadStoryData()
    {
        _storiesById.Clear();
        string path = $"StoryData/{storyDataFileName}";
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogWarning($"[AVGStoryManager] 未找到剧情数据文件：Resources/{path}.json");
            return;
        }

        try
        {
            StoryDataContainer container = JsonUtility.FromJson<StoryDataContainer>(textAsset.text);
            if (container?.stories == null)
            {
                Debug.LogWarning("[AVGStoryManager] 剧情数据为空或格式错误");
                return;
            }

            foreach (var story in container.stories)
            {
                if (string.IsNullOrEmpty(story.storyId))
                {
                    Debug.LogWarning("[AVGStoryManager] 发现storyId为空的剧情，已跳过");
                    continue;
                }
                if (_storiesById.ContainsKey(story.storyId))
                {
                    Debug.LogWarning($"[AVGStoryManager] 重复的storyId: {story.storyId}，后者已覆盖前者");
                }
                _storiesById[story.storyId] = story;
            }
            Debug.Log($"[AVGStoryManager] 已加载 {_storiesById.Count} 条剧情数据");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AVGStoryManager] 解析剧情JSON失败: {e.Message}");
        }
    }

    // ===================== 触发逻辑 =====================

    /// <summary>
    /// 检查并播放剧情（如果未观看）
    /// </summary>
    /// <param name="storyId">剧情ID</param>
    /// <param name="onComplete">剧情完成后的回调（无论跳过还是正常结束）</param>
    /// <returns>是否开始了剧情播放</returns>
    public bool CheckAndPlayStory(string storyId, Action onComplete = null)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[AVGStoryManager] SaveManager未找到！");
            onComplete?.Invoke();
            return false;
        }

        if (SaveManager.Instance.IsStoryWatched(storyId))
        {
            onComplete?.Invoke();
            return false;
        }

        if (!_storiesById.TryGetValue(storyId, out StoryData storyData))
        {
            Debug.LogWarning($"[AVGStoryManager] 未找到storyId为\"{storyId}\"的剧情数据");
            onComplete?.Invoke();
            return false;
        }

        PlayStory(storyData, onComplete);
        return true;
    }

    /// <summary>
    /// 强制播放剧情（无论是否已观看）
    /// </summary>
    public void ForcePlayStory(string storyId, Action onComplete = null)
    {
        if (!_storiesById.TryGetValue(storyId, out StoryData storyData))
        {
            Debug.LogWarning($"[AVGStoryManager] 未找到storyId为\"{storyId}\"的剧情数据");
            onComplete?.Invoke();
            return;
        }
        PlayStory(storyData, onComplete);
    }

    /// <summary>
    /// 按触发类型和条件自动查找并播放剧情
    /// </summary>
    public bool CheckAndPlayStoryByTrigger(StoryTriggerType triggerType, string triggerCondition, Action onComplete = null)
    {
        foreach (var kv in _storiesById)
        {
            var story = kv.Value;
            if (story.triggerType == triggerType &&
                story.triggerCondition == triggerCondition &&
                !SaveManager.Instance.IsStoryWatched(story.storyId))
            {
                PlayStory(story, onComplete);
                return true;
            }
        }
        onComplete?.Invoke();
        return false;
    }

    // ===================== 播放控制 =====================

    private void PlayStory(StoryData storyData, Action onComplete)
    {
        if (_isPlaying)
        {
            Debug.LogWarning("[AVGStoryManager] 已有剧情正在播放，忽略此次请求");
            onComplete?.Invoke();
            return;
        }

        // 每次播放前都重新查找 StoryPlayer（因为场景切换后引用会丢失）
        storyPlayer = FindObjectOfType<StoryPlayer>();

        if (storyPlayer == null)
        {
            Debug.LogError("[AVGStoryManager] 场景中未找到StoryPlayer组件！");
            onComplete?.Invoke();
            return;
        }

        Debug.Log($"[AVGStoryManager] Found StoryPlayer: {storyPlayer.name}, starting story: {storyData.storyId}");

        _isPlaying = true;
        storyPlayer.Play(storyData, () =>
        {
            _isPlaying = false;
            SaveManager.Instance.MarkStoryWatched(storyData.storyId);
            Debug.Log($"[AVGStoryManager] Story completed: {storyData.storyId}");
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// 当前是否正在播放剧情
    /// </summary>
    public bool IsPlaying => _isPlaying;
}
