using UnityEngine;

/// <summary>
/// Big Map 场景初始化器
/// 负责检测首次进入和主线关卡通关，播放对应的 AVG 剧情
/// </summary>
public class BigMapInitializer : MonoBehaviour
{
    [Header("引用")]
    public AVGStoryManager avgStoryManager;

    [Header("关卡数据列表")]
    [Tooltip("所有关卡数据，用于查找刚通关的关卡")]
    public LevelData[] allLevelData;

    [Header("剧情配置")]
    [Tooltip("开场剧情ID")]
    public string openingStoryId = "NewGame_Opening";

    private void Start()
    {
        // 如果引用为空，尝试通过单例获取
        if (avgStoryManager == null)
        {
            avgStoryManager = AVGStoryManager.Instance;

            if (avgStoryManager == null)
            {
                Debug.LogError("[BigMapInitializer] AVGStoryManager.Instance is NULL!");
                return;
            }
        }

        Debug.Log($"[BigMapInitializer] AVGStoryManager found: {avgStoryManager.name}");

        // 强制重新加载剧情数据（确保 stories.json 的更改被加载）
        avgStoryManager.LoadStoryData();
        Debug.Log("[BigMapInitializer] Force reloaded story data");

        // 等待 SaveManager 和 AVGStoryManager 初始化完成
        StartCoroutine(CheckAfterLoad());
    }

    private System.Collections.IEnumerator CheckAfterLoad()
    {
        // 等待 SaveManager 初始化完成
        float timeout = 5f;
        float elapsed = 0f;

        while (SaveManager.Instance == null && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("[BigMapInitializer] SaveManager still not found after timeout!");
            yield break;
        }

        Debug.Log("[BigMapInitializer] SaveManager found, checking story triggers");

        // 优先检查首次进入，然后检查通关剧情
        CheckAndPlayStories();
    }

    /// <summary>
    /// 检查并播放剧情（首次进入 > 通关剧情）
    /// </summary>
    private void CheckAndPlayStories()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[BigMapInitializer] SaveManager not found!");
            return;
        }

        // 1. 检查是否首次进入 Big Map
        if (SaveManager.Instance.IsFirstTimeEnterBigMap())
        {
            Debug.Log("[BigMapInitializer] First time entering Big Map, playing opening story");
            PlayOpeningStory();
            return; // 播放开场剧情后，通关剧情在回调中检查
        }

        // 2. 如果不是首次进入，检查是否有通关剧情
        CheckAndPlayClearStory();
    }

    /// <summary>
    /// 播放开场剧情
    /// </summary>
    private void PlayOpeningStory()
    {
        if (avgStoryManager == null)
        {
            Debug.LogError("[BigMapInitializer] AVGStoryManager not assigned!");
            SaveManager.Instance.MarkBigMapEntered();
            return;
        }

        // 使用 CheckAndPlayStory，尊重已观看状态
        bool started = avgStoryManager.CheckAndPlayStory(openingStoryId, OnOpeningStoryComplete);

        if (!started)
        {
            Debug.Log($"[BigMapInitializer] Opening story '{openingStoryId}' not played (already watched or not found)");
            // 标记已进入并检查通关剧情
            SaveManager.Instance.MarkBigMapEntered();
            CheckAndPlayClearStory();
        }
        else
        {
            Debug.Log($"[BigMapInitializer] Playing opening story: {openingStoryId}");
        }
    }

    /// <summary>
    /// 开场剧情播放完成回调
    /// </summary>
    private void OnOpeningStoryComplete()
    {
        Debug.Log("[BigMapInitializer] Opening story completed");

        // 标记 Big Map 已进入
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.MarkBigMapEntered();
        }

        // 检查是否有通关剧情需要播放
        CheckAndPlayClearStory();
    }

    /// <summary>
    /// 检查是否有刚通关的主线关卡，如果有则播放通关剧情
    /// </summary>
    private void CheckAndPlayClearStory()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[BigMapInitializer] SaveManager not found!");
            return;
        }

        // 获取刚通关的主线关卡信息
        var (levelName, chapterGroup) = SaveManager.Instance.GetJustClearedMainLevel();

        Debug.Log($"[BigMapInitializer] Checking for cleared level. levelName: '{levelName}', chapterGroup: {chapterGroup}");

        // 如果没有刚通关的关卡，直接返回
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.Log("[BigMapInitializer] No recently cleared main level found");
            return;
        }

        Debug.Log($"[BigMapInitializer] Detected cleared main level: {levelName} (Chapter {chapterGroup})");

        // 查找对应的 LevelData
        LevelData clearedLevel = FindLevelData(levelName);

        if (clearedLevel == null)
        {
            Debug.LogWarning($"[BigMapInitializer] LevelData not found for: {levelName}");
            SaveManager.Instance.ClearJustClearedFlag();
            return;
        }

        Debug.Log($"[BigMapInitializer] Found LevelData. clearStoryId: '{clearedLevel.clearStoryId}'");

        // 检查是否有通关剧情 ID
        if (string.IsNullOrEmpty(clearedLevel.clearStoryId))
        {
            Debug.Log($"[BigMapInitializer] No clear story for level: {levelName}");
            SaveManager.Instance.ClearJustClearedFlag();
            return;
        }

        // 播放通关剧情
        PlayClearStory(clearedLevel.clearStoryId);
    }

    /// <summary>
    /// 播放通关剧情
    /// </summary>
    private void PlayClearStory(string storyId)
    {
        if (avgStoryManager == null)
        {
            Debug.LogError("[BigMapInitializer] AVGStoryManager not assigned!");
            SaveManager.Instance.ClearJustClearedFlag();
            return;
        }

        Debug.Log($"[BigMapInitializer] AVGStoryManager found: {avgStoryManager.name}");
        Debug.Log($"[BigMapInitializer] Playing clear story: {storyId}");

        // 使用 CheckAndPlayStory 而不是 ForcePlayStory，尊重已观看状态
        bool started = avgStoryManager.CheckAndPlayStory(storyId, OnClearStoryComplete);

        if (!started)
        {
            Debug.Log($"[BigMapInitializer] Clear story '{storyId}' not played (already watched or not found)");
            SaveManager.Instance.ClearJustClearedFlag();
        }
    }

    /// <summary>
    /// 通关剧情播放完成回调
    /// </summary>
    private void OnClearStoryComplete()
    {
        Debug.Log("[BigMapInitializer] Clear story completed");

        // 清除"刚通关"标记
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ClearJustClearedFlag();
            Debug.Log("[BigMapInitializer] Cleared 'just cleared' flag");
        }
    }

    /// <summary>
    /// 根据关卡名称查找 LevelData
    /// </summary>
    private LevelData FindLevelData(string levelName)
    {
        if (allLevelData == null || allLevelData.Length == 0)
        {
            Debug.LogWarning("[BigMapInitializer] allLevelData is empty!");
            return null;
        }

        foreach (var levelData in allLevelData)
        {
            if (levelData != null && levelData.levelName == levelName)
            {
                return levelData;
            }
        }

        return null;
    }
}
