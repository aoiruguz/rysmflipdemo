using UnityEngine;

/// <summary>
/// Big Map 场景初始化器
/// 负责检测主线关卡通关并播放 AVG 剧情
/// </summary>
public class BigMapInitializer : MonoBehaviour
{
    [Header("引用")]
    public AVGStoryManager avgStoryManager;

    [Header("关卡数据列表")]
    [Tooltip("所有关卡数据，用于查找刚通关的关卡")]
    public LevelData[] allLevelData;

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

        Debug.Log("[BigMapInitializer] SaveManager found, checking for cleared level");
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

        // 使用 AVGStoryManager 强制播放剧情（无论是否已观看）
        avgStoryManager.ForcePlayStory(storyId, OnClearStoryComplete);
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
