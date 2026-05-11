using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 统一的存档管理器
/// 负责保存和加载所有游戏数据
/// 使用两个独立的JSON文件：settings.json（设置）和 progress.json（进度）
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private GameSettingsData currentSettings;
    private GameProgressData currentProgress;

    private string settingsFilePath;
    private string progressFilePath;

    private const string SETTINGS_FILE_NAME = "settings.json";
    private const string PROGRESS_FILE_NAME = "progress.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        settingsFilePath = Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);
        progressFilePath = Path.Combine(Application.persistentDataPath, PROGRESS_FILE_NAME);

        LoadGame();
    }

    /// <summary>
    /// 加载存档
    /// </summary>
    public void LoadGame()
    {
        LoadSettings();
        LoadProgress();
        ApplySettings();
    }

    /// <summary>
    /// 加载设置文件
    /// </summary>
    private void LoadSettings()
    {
        if (File.Exists(settingsFilePath))
        {
            try
            {
                string json = File.ReadAllText(settingsFilePath);
                currentSettings = JsonUtility.FromJson<GameSettingsData>(json);
                Debug.Log($"[SaveManager] Settings loaded from: {settingsFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load settings file: {e.Message}");
                CreateNewSettings();
            }
        }
        else
        {
            Debug.Log("[SaveManager] No settings file found, creating new settings");
            CreateNewSettings();
        }
    }

    /// <summary>
    /// 加载进度文件
    /// </summary>
    private void LoadProgress()
    {
        if (File.Exists(progressFilePath))
        {
            try
            {
                string json = File.ReadAllText(progressFilePath);
                currentProgress = JsonUtility.FromJson<GameProgressData>(json);
                Debug.Log($"[SaveManager] Progress loaded from: {progressFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load progress file: {e.Message}");
                CreateNewProgress();
            }
        }
        else
        {
            Debug.Log("[SaveManager] No progress file found, creating new progress");
            CreateNewProgress();
        }
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            SyncCurrentSettings();
            string json = JsonUtility.ToJson(currentSettings, true);
            File.WriteAllText(settingsFilePath, json);
            Debug.Log($"[SaveManager] Settings saved to: {settingsFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save settings: {e.Message}");
        }
    }

    /// <summary>
    /// 保存进度
    /// </summary>
    public void SaveProgress()
    {
        try
        {
            string json = JsonUtility.ToJson(currentProgress, true);
            File.WriteAllText(progressFilePath, json);
            Debug.Log($"[SaveManager] Progress saved to: {progressFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save progress: {e.Message}");
        }
    }

    /// <summary>
    /// 保存游戏（同时保存设置和进度）
    /// </summary>
    public void SaveGame()
    {
        SaveSettings();
        SaveProgress();
    }

    /// <summary>
    /// 创建新设置
    /// </summary>
    private void CreateNewSettings()
    {
        currentSettings = new GameSettingsData();
        currentSettings.resolutionWidth = Screen.currentResolution.width;
        currentSettings.resolutionHeight = Screen.currentResolution.height;
        SaveSettings();
    }

    /// <summary>
    /// 创建新进度
    /// </summary>
    private void CreateNewProgress()
    {
        currentProgress = new GameProgressData();
        SaveProgress();
    }

    /// <summary>
    /// 应用设置到游戏
    /// </summary>
    private void ApplySettings()
    {
        GameSettings.MusicVolume = currentSettings.musicVolume;
        GameSettings.NoteVolume = currentSettings.noteVolume;
        GameSettings.GlobalOffset = currentSettings.globalOffset;
        GameSettings.NoteTravelTime = currentSettings.noteTravelTime;
        GameSettings.InputMode = currentSettings.inputMode;
        GameSettings.ApplyResolution(currentSettings.resolutionWidth, currentSettings.resolutionHeight, currentSettings.fullScreenMode);
    }

    /// <summary>
    /// 同步当前设置到存档数据
    /// </summary>
    private void SyncCurrentSettings()
    {
        currentSettings.musicVolume = GameSettings.MusicVolume;
        currentSettings.noteVolume = GameSettings.NoteVolume;
        currentSettings.globalOffset = GameSettings.GlobalOffset;
        currentSettings.noteTravelTime = GameSettings.NoteTravelTime;
        currentSettings.inputMode = GameSettings.InputMode;
        currentSettings.resolutionWidth = GameSettings.ResolutionWidth;
        currentSettings.resolutionHeight = GameSettings.ResolutionHeight;
        currentSettings.fullScreenMode = GameSettings.FullScreenMode;
    }

    /// <summary>
    /// 获取关卡进度数据
    /// </summary>
    public LevelProgressData GetLevelProgress(string levelName, string songName, ChartDifficulty difficulty)
    {
        string key = $"{levelName}_{songName}_{difficulty}";

        foreach (var progress in currentProgress.levelProgress)
        {
            if (progress.GetLevelKey() == key)
            {
                return progress;
            }
        }

        return null;
    }

    /// <summary>
    /// 更新关卡进度
    /// </summary>
    public void UpdateLevelProgress(LevelProgressData newProgress)
    {
        string key = newProgress.GetLevelKey();

        // 查找是否已存在
        for (int i = 0; i < currentProgress.levelProgress.Count; i++)
        {
            if (currentProgress.levelProgress[i].GetLevelKey() == key)
            {
                currentProgress.levelProgress[i] = newProgress;
                SaveProgress();
                return;
            }
        }

        // 不存在则添加
        currentProgress.levelProgress.Add(newProgress);
        SaveProgress();
    }

    /// <summary>
    /// 保存关卡结果
    /// </summary>
    public void SaveLevelResult(string levelName, string songName, ChartDifficulty difficulty,
        int score, string rank, int perfect, int great, int good, int miss, int maxCombo, long fansEarned)
    {
        var progress = GetLevelProgress(levelName, songName, difficulty);

        if (progress == null)
        {
            progress = new LevelProgressData
            {
                levelName = levelName,
                songName = songName,
                difficulty = difficulty
            };
        }

        // 检查是否是首次通关
        bool wasNotCleared = !progress.isCleared;

        // 更新数据
        progress.isCleared = true;
        progress.playCount++;
        progress.lastPlayedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 更新最高分
        if (score > progress.highScore)
        {
            progress.highScore = score;
            progress.rank = rank;
            progress.bestPerfectCount = perfect;
            progress.bestGreatCount = great;
            progress.bestGoodCount = good;
            progress.bestMissCount = miss;
            progress.bestMaxCombo = maxCombo;
        }

        // 累计粉丝数
        progress.fansEarned += fansEarned;

        UpdateLevelProgress(progress);

        // 直接更新粉丝数
        AddFans(fansEarned);

        Debug.Log($"[SaveManager] Level result saved: {levelName} - {difficulty}, Score: {score}, Rank: {rank}");

        // 检查是否是主线关卡首次通关
        if (wasNotCleared && levelName.StartsWith("MainStory_"))
        {
            Debug.Log($"[SaveManager] Main story level cleared for the first time: {levelName}");
            SetJustClearedMainLevel(levelName);
        }
    }

    /// <summary>
    /// 保存关卡结果（带 LevelData 参数，用于检测主线关卡）
    /// </summary>
    public void SaveLevelResult(LevelData levelData, string songName, ChartDifficulty difficulty,
        int score, string rank, int perfect, int great, int good, int miss, int maxCombo, long fansEarned)
    {
        // 调用原有的保存方法
        SaveLevelResult(levelData.levelName, songName, difficulty, score, rank, perfect, great, good, miss, maxCombo, fansEarned);

        // 检查是否是主线关卡
        if (levelData.isMainStoryLevel)
        {
            Debug.Log($"[SaveManager] Main story level cleared: {levelData.levelName} (Chapter {levelData.chapterGroup})");
            MarkChapterMainStoryCleared(levelData.chapterGroup, levelData.levelName);
        }
    }

    /// <summary>
    /// 检查关卡是否已通关
    /// </summary>
    public bool IsLevelCleared(string levelName, string songName, ChartDifficulty difficulty)
    {
        var progress = GetLevelProgress(levelName, songName, difficulty);
        return progress != null && progress.isCleared;
    }

    /// <summary>
    /// 获取关卡最高分
    /// </summary>
    public int GetLevelHighScore(string levelName, string songName, ChartDifficulty difficulty)
    {
        var progress = GetLevelProgress(levelName, songName, difficulty);
        return progress?.highScore ?? 0;
    }

    /// <summary>
    /// 获取关卡评级
    /// </summary>
    public string GetLevelRank(string levelName, string songName, ChartDifficulty difficulty)
    {
        var progress = GetLevelProgress(levelName, songName, difficulty);
        return progress?.rank ?? "";
    }

    /// <summary>
    /// 获取总粉丝数
    /// </summary>
    public long GetTotalFans()
    {
        return currentProgress.totalFans;
    }

    /// <summary>
    /// 设置粉丝总数
    /// </summary>
    public void SetTotalFans(long fans)
    {
        currentProgress.totalFans = fans;
        SaveProgress();
    }

    /// <summary>
    /// 增加粉丝数
    /// </summary>
    public void AddFans(long fansToAdd)
    {
        currentProgress.totalFans += fansToAdd;
        SaveProgress();
    }

    /// <summary>
    /// 获取解锁章节
    /// </summary>
    public int GetUnlockedChapter()
    {
        return currentProgress.unlockedChapter;
    }

    /// <summary>
    /// 设置解锁章节
    /// </summary>
    public void SetUnlockedChapter(int chapterIndex)
    {
        if (chapterIndex > currentProgress.unlockedChapter)
        {
            currentProgress.unlockedChapter = chapterIndex;
            SaveProgress();
            Debug.Log($"[SaveManager] Unlocked chapter {chapterIndex + 1}");
        }
    }

    /// <summary>
    /// 标记关卡组的主线关卡已通关
    /// </summary>
    public void MarkChapterMainStoryCleared(int chapterGroup, string levelName)
    {
        if (!currentProgress.clearedChapterGroups.Contains(chapterGroup))
        {
            currentProgress.clearedChapterGroups.Add(chapterGroup);
            Debug.Log($"[SaveManager] Chapter group {chapterGroup} main story cleared!");
        }

        // 设置临时标记（用于返回 Big Map 时播放剧情）
        currentProgress.justClearedMainLevel = levelName;
        currentProgress.justClearedChapterGroup = chapterGroup;

        SaveProgress();
    }

    /// <summary>
    /// 检查关卡组的主线关卡是否已通关
    /// </summary>
    public bool IsChapterMainStoryCleared(int chapterGroup)
    {
        return currentProgress.clearedChapterGroups.Contains(chapterGroup);
    }

    /// <summary>
    /// 获取刚通关的主线关卡信息
    /// </summary>
    public (string levelName, int chapterGroup) GetJustClearedMainLevel()
    {
        return (currentProgress.justClearedMainLevel, currentProgress.justClearedChapterGroup);
    }

    /// <summary>
    /// 清除"刚通关"标记
    /// </summary>
    public void ClearJustClearedFlag()
    {
        currentProgress.justClearedMainLevel = "";
        currentProgress.justClearedChapterGroup = 0;
        SaveProgress();
    }

    /// <summary>
    /// 设置刚通关的主线关卡（内部使用）
    /// </summary>
    private void SetJustClearedMainLevel(string levelName)
    {
        currentProgress.justClearedMainLevel = levelName;
        SaveProgress();
    }

    /// <summary>
    /// 重置所有数据（调试用）
    /// </summary>
    public void ResetAllData()
    {
        if (File.Exists(settingsFilePath))
        {
            File.Delete(settingsFilePath);
        }
        if (File.Exists(progressFilePath))
        {
            File.Delete(progressFilePath);
        }

        CreateNewSettings();
        CreateNewProgress();
        Debug.Log("[SaveManager] All data reset");
    }

    /// <summary>
    /// 清除游戏进度数据，但保留设置（offset和流速）
    /// 用于"新游戏"功能
    /// </summary>
    public void ClearGameProgressKeepSettings()
    {
        // 创建新的进度数据
        currentProgress = new GameProgressData();
        // 确保首次进入标记为 true
        currentProgress.isFirstTimeEnterBigMap = true;
        SaveProgress();

        Debug.Log("[SaveManager] Game progress cleared (including story progress), settings preserved");
    }

    /// <summary>
    /// 检测是否存在游戏进度存档
    /// </summary>
    public bool HasGameProgress()
    {
        if (currentProgress == null)
            return false;

        // 检查是否有任何游戏进度：关卡进度、粉丝数、解锁章节
        bool hasLevelProgress = currentProgress.levelProgress != null && currentProgress.levelProgress.Count > 0;
        bool hasFans = currentProgress.totalFans > 0;
        bool hasUnlockedChapter = currentProgress.unlockedChapter > 0;

        return hasLevelProgress || hasFans || hasUnlockedChapter;
    }

    // ===================== 剧情进度接口 =====================

    /// <summary>
    /// 检查指定剧情是否已观看
    /// </summary>
    public bool IsStoryWatched(string storyId)
    {
        var progress = currentProgress.storyProgress.Find(p => p.storyId == storyId);
        return progress != null && progress.isWatched;
    }

    /// <summary>
    /// 标记指定剧情为已观看
    /// </summary>
    public void MarkStoryWatched(string storyId)
    {
        var progress = currentProgress.storyProgress.Find(p => p.storyId == storyId);
        if (progress == null)
        {
            progress = new StoryProgressData { storyId = storyId };
            currentProgress.storyProgress.Add(progress);
        }
        progress.isWatched = true;
        progress.watchedTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        SaveProgress();
    }

    /// <summary>
    /// 重置所有剧情进度（用于调试）
    /// </summary>
    public void ResetAllStoryProgress()
    {
        currentProgress.storyProgress.Clear();
        SaveProgress();
    }

    // ===================== Big Map 首次进入接口 =====================

    /// <summary>
    /// 检查是否首次进入 Big Map
    /// </summary>
    public bool IsFirstTimeEnterBigMap()
    {
        return currentProgress.isFirstTimeEnterBigMap;
    }

    /// <summary>
    /// 标记 Big Map 已进入
    /// </summary>
    public void MarkBigMapEntered()
    {
        currentProgress.isFirstTimeEnterBigMap = false;
        SaveProgress();
        Debug.Log("[SaveManager] Big Map entered, first time flag cleared");
    }
}
