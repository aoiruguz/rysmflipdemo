using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 统一的存档管理器
/// 负责保存和加载所有游戏数据
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private SaveData currentSaveData;
    private string saveFilePath;

    private const string SAVE_FILE_NAME = "gamesave.json";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        LoadGame();
    }

    /// <summary>
    /// 加载存档
    /// </summary>
    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                currentSaveData = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveManager] Game loaded from: {saveFilePath}");

                // 应用设置到游戏
                ApplySettings();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load save file: {e.Message}");
                CreateNewSave();
            }
        }
        else
        {
            Debug.Log("[SaveManager] No save file found, creating new save");
            CreateNewSave();
        }
    }

    /// <summary>
    /// 保存游戏
    /// </summary>
    public void SaveGame()
    {
        try
        {
            // 保存前先同步当前设置
            SyncCurrentSettings();

            string json = JsonUtility.ToJson(currentSaveData, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"[SaveManager] Game saved to: {saveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
        }
    }

    /// <summary>
    /// 创建新存档
    /// </summary>
    private void CreateNewSave()
    {
        currentSaveData = new SaveData();

        // 初始化默认设置
        currentSaveData.settings.resolutionWidth = Screen.currentResolution.width;
        currentSaveData.settings.resolutionHeight = Screen.currentResolution.height;

        SaveGame();
    }

    /// <summary>
    /// 应用设置到游戏
    /// </summary>
    private void ApplySettings()
    {
        var settings = currentSaveData.settings;

        GameSettings.MusicVolume = settings.musicVolume;
        GameSettings.NoteVolume = settings.noteVolume;
        GameSettings.GlobalOffset = settings.globalOffset;
        GameSettings.NoteTravelTime = settings.noteTravelTime;
        GameSettings.InputMode = settings.inputMode;
        GameSettings.ApplyResolution(settings.resolutionWidth, settings.resolutionHeight, settings.fullScreenMode);
    }

    /// <summary>
    /// 同步当前设置到存档数据
    /// </summary>
    private void SyncCurrentSettings()
    {
        currentSaveData.settings.musicVolume = GameSettings.MusicVolume;
        currentSaveData.settings.noteVolume = GameSettings.NoteVolume;
        currentSaveData.settings.globalOffset = GameSettings.GlobalOffset;
        currentSaveData.settings.noteTravelTime = GameSettings.NoteTravelTime;
        currentSaveData.settings.inputMode = GameSettings.InputMode;
        currentSaveData.settings.resolutionWidth = GameSettings.ResolutionWidth;
        currentSaveData.settings.resolutionHeight = GameSettings.ResolutionHeight;
        currentSaveData.settings.fullScreenMode = GameSettings.FullScreenMode;

        // 粉丝数和章节数据已经在 currentSaveData 中，无需同步
    }

    /// <summary>
    /// 获取关卡进度数据
    /// </summary>
    public LevelProgressData GetLevelProgress(string levelName, string songName, ChartDifficulty difficulty)
    {
        string key = $"{levelName}_{songName}_{difficulty}";

        foreach (var progress in currentSaveData.levelProgress)
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
        for (int i = 0; i < currentSaveData.levelProgress.Count; i++)
        {
            if (currentSaveData.levelProgress[i].GetLevelKey() == key)
            {
                currentSaveData.levelProgress[i] = newProgress;
                SaveGame();
                return;
            }
        }

        // 不存在则添加
        currentSaveData.levelProgress.Add(newProgress);
        SaveGame();
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

        // 直接更新粉丝数到 SaveData
        AddFans(fansEarned);

        Debug.Log($"[SaveManager] Level result saved: {levelName} - {difficulty}, Score: {score}, Rank: {rank}");
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
        return currentSaveData.totalFans;
    }

    /// <summary>
    /// 设置粉丝总数
    /// </summary>
    public void SetTotalFans(long fans)
    {
        currentSaveData.totalFans = fans;
        SaveGame();
    }

    /// <summary>
    /// 增加粉丝数
    /// </summary>
    public void AddFans(long fansToAdd)
    {
        currentSaveData.totalFans += fansToAdd;
        SaveGame();
    }

    /// <summary>
    /// 获取解锁章节
    /// </summary>
    public int GetUnlockedChapter()
    {
        return currentSaveData.unlockedChapter;
    }

    /// <summary>
    /// 设置解锁章节
    /// </summary>
    public void SetUnlockedChapter(int chapterIndex)
    {
        if (chapterIndex > currentSaveData.unlockedChapter)
        {
            currentSaveData.unlockedChapter = chapterIndex;
            SaveGame();
            Debug.Log($"[SaveManager] Unlocked chapter {chapterIndex + 1}");
        }
    }

    /// <summary>
    /// 重置所有数据（调试用）
    /// </summary>
    public void ResetAllData()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
        }

        CreateNewSave();
        Debug.Log("[SaveManager] All data reset");
    }

    /// <summary>
    /// 清除游戏进度数据，但保留设置（offset和流速）
    /// 用于"新游戏"功能
    /// </summary>
    public void ClearGameProgressKeepSettings()
    {
        // 保存当前设置
        var savedSettings = currentSaveData.settings;

        // 清除游戏进度数据
        currentSaveData.levelProgress.Clear();
        currentSaveData.totalFans = 0;
        currentSaveData.unlockedChapter = 0;

        // 清除剧情进度
        currentSaveData.storyProgress.Clear();

        // 恢复设置
        currentSaveData.settings = savedSettings;

        SaveGame();
        Debug.Log("[SaveManager] Game progress cleared (including story progress), settings preserved");
    }

    /// <summary>
    /// 检测是否存在游戏进度存档
    /// </summary>
    public bool HasGameProgress()
    {
        if (currentSaveData == null)
            return false;

        // 检查是否有任何游戏进度：关卡进度、粉丝数、解锁章节
        bool hasLevelProgress = currentSaveData.levelProgress != null && currentSaveData.levelProgress.Count > 0;
        bool hasFans = currentSaveData.totalFans > 0;
        bool hasUnlockedChapter = currentSaveData.unlockedChapter > 0;

        return hasLevelProgress || hasFans || hasUnlockedChapter;
    }

    // ===================== 剧情进度接口 =====================

    /// <summary>
    /// 检查指定剧情是否已观看
    /// </summary>
    public bool IsStoryWatched(string storyId)
    {
        var progress = currentSaveData.storyProgress.Find(p => p.storyId == storyId);
        return progress != null && progress.isWatched;
    }

    /// <summary>
    /// 标记指定剧情为已观看
    /// </summary>
    public void MarkStoryWatched(string storyId)
    {
        var progress = currentSaveData.storyProgress.Find(p => p.storyId == storyId);
        if (progress == null)
        {
            progress = new StoryProgressData { storyId = storyId };
            currentSaveData.storyProgress.Add(progress);
        }
        progress.isWatched = true;
        progress.watchedTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        SaveGame();
    }

    /// <summary>
    /// 重置所有剧情进度（用于调试）
    /// </summary>
    public void ResetAllStoryProgress()
    {
        currentSaveData.storyProgress.Clear();
        SaveGame();
    }
}
