using System;
using UnityEngine;

/// <summary>
/// 单句对话数据
/// </summary>
[Serializable]
public class DialogueData
{
    [Tooltip("1 或 2，表示在哪个 panel 显示")]
    public int panelIndex = 1;

    [Tooltip("说话者名字")]
    public string speakerName = "";

    [Tooltip("对话内容")]
    [TextArea(3, 5)]
    public string text = "";
}
