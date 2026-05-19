using System;
using UnityEngine;

/// <summary>
/// 单句对话数据
/// </summary>
[Serializable]
public class DialogueData
{
    [Tooltip("0 或 1 表示 Panel1，2 表示 Panel2")]
    public int panelIndex = 0;

    [Tooltip("说话者名字")]
    public string speakerName = "";

    [Tooltip("对话内容")]
    [TextArea(3, 5)]
    public string text = "";
}
