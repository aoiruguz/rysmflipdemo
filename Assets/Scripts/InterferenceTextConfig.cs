using UnityEngine;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 弹幕文本配置文件
/// 用于配置横向滚动弹幕的参数
/// </summary>
[CreateAssetMenu(fileName = "NewInterferenceTextConfig", menuName = "RhythmGame/Interference/Text Config")]
public class InterferenceTextConfig : ScriptableObject
{
    [Header("文本内容")]
    [Tooltip("弹幕文本内容列表，将从中随机选择")]
    [TextArea(3, 10)]
    public List<string> textContents = new List<string>
    {
        "你能跟上我的节奏吗？",
        "这就是你的实力？",
        "太慢了！"
    };

    [Header("显示设置")]
    [Tooltip("同时出现的弹幕数量")]
    [Range(1, 10)]
    public int simultaneousCount = 3;

    [Header("视觉设置")]
    [Tooltip("文字颜色（包含透明度Alpha）")]
    public Color textColor = Color.white;

    /// <summary>
    /// 从配置的文本列表中随机获取一条文本
    /// </summary>
    public string GetRandomText()
    {
        if (textContents == null || textContents.Count == 0)
        {
            return "...";
        }
        return textContents[Random.Range(0, textContents.Count)];
    }

}
