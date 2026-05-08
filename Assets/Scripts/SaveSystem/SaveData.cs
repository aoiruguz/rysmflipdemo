using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 游戏存档数据结构
/// 包含所有需要持久化的数据
/// </summary>
[Serializable]
public class SaveData
{
    // 游戏设置
    public GameSettingsData settings = new GameSettingsData();

    // 关卡进度
    public List<LevelProgressData> levelProgress = new List<LevelProgressData>();

    // 全局数据
    public long totalFans = 0;
    public int unlockedChapter = 0;

    // 存档版本（用于未来兼容性）
    public int saveVersion = 1;
}

/// <summary>
/// 游戏设置数据
/// </summary>
[Serializable]
public class GameSettingsData
{
    // 音频设置
    public float musicVolume = 1f;
    public float noteVolume = 1f;

    // 游戏设置
    public float globalOffset = 0f;
    public float noteTravelTime = 2.0f;
    public int inputMode = 1; // 0=Preset, 1=Accurate

    // 分辨率设置
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int fullScreenMode = 1; // 0=窗口化, 1=全屏, 2=无边框窗口
}

/// <summary>
/// 单个关卡的进度数据
/// </summary>
[Serializable]
public class LevelProgressData
{
    // 关卡标识
    public string levelName;
    public string songName;
    public ChartDifficulty difficulty;

    // 通关状态
    public bool isCleared = false;

    // 分数记录
    public int highScore = 0;
    public string rank = ""; // S, A, B, C, D

    // 判定统计（最佳记录）
    public int bestPerfectCount = 0;
    public int bestGreatCount = 0;
    public int bestGoodCount = 0;
    public int bestMissCount = 0;
    public int bestMaxCombo = 0;

    // 粉丝数记录
    public long fansEarned = 0;

    // 游玩次数
    public int playCount = 0;

    // 最后游玩时间
    public string lastPlayedTime = "";

    /// <summary>
    /// 生成关卡的唯一标识符
    /// </summary>
    public string GetLevelKey()
    {
        return $"{levelName}_{songName}_{difficulty}";
    }
}
