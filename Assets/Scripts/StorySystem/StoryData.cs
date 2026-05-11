using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 剧情触发类型
/// </summary>
public enum StoryTriggerType
{
    SceneEnter,      // 场景进入时触发
    LevelComplete,   // 关卡完成后触发
    Condition        // 满足特定条件时触发
}

/// <summary>
/// 剧情数据容器（用于JSON反序列化）
/// </summary>
[Serializable]
public class StoryDataContainer
{
    public List<StoryData> stories = new List<StoryData>();
}

/// <summary>
/// 单个剧情的完整数据
/// </summary>
[Serializable]
public class StoryData
{
    [Tooltip("剧情唯一标识符")]
    public string storyId;

    [Tooltip("剧情名称（用于调试和显示）")]
    public string storyName;

    [Tooltip("触发类型")]
    public StoryTriggerType triggerType;

    [Tooltip("触发条件（场景名/关卡名/条件ID）")]
    public string triggerCondition;

    [Tooltip("对话列表")]
    public List<StoryDialogueData> dialogues = new List<StoryDialogueData>();
}

/// <summary>
/// 剧情系统单句对话数据
/// </summary>
[Serializable]
public class StoryDialogueData
{
    [Tooltip("角色名称")]
    public string characterName;

    [Tooltip("对话文本")]
    [TextArea(3, 10)]
    public string dialogueText;

    [Tooltip("角色立绘资源路径（Resources文件夹下的相对路径）")]
    public string characterSpritePath;

    [Tooltip("背景图资源路径（Resources文件夹下的相对路径）")]
    public string backgroundSpritePath;

    [Tooltip("音效资源路径（可选）")]
    public string audioClipPath;
}

/// <summary>
/// 剧情进度数据（用于存档）
/// </summary>
[Serializable]
public class StoryProgressData
{
    [Tooltip("剧情ID")]
    public string storyId;

    [Tooltip("是否已观看")]
    public bool isWatched;

    [Tooltip("观看时间")]
    public string watchedTime;
}
