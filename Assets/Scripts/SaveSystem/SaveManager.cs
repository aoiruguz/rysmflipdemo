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

        currentSaveData.totalFans = FansDataManager.GetTotalFans();
        currentSaveData.unlockedChapter = FansDataManager.GetUnlockedChapter();
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

        // 更新总粉丝数
        FansDataManager.AddFans(fansEarned);

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
}
