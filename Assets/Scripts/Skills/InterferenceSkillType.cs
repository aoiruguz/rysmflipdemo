using UnityEngine;

/// <summary>
/// 干扰技能类型枚举
/// </summary>
public enum InterferenceSkillType
{
    ScrollingText,  // 弹幕遮挡
    ScreenShake,    // 屏幕震动
    GlitchEffect,   // 故障块遮挡
    MouthAttack     // 嘴巴攻击
}

/// <summary>
/// 干扰技能触发配置
/// 定义在歌曲的哪个进度触发什么技能
/// </summary>
[System.Serializable]
public class InterferenceTrigger
{
    [Tooltip("触发时的歌曲进度百分比（0-1，例如0.25表示25%进度时触发）")]
    [Range(0f, 1f)]
    public float triggerProgress = 0.5f;

    [Tooltip("触发的技能类型")]
    public InterferenceSkillType skillType = InterferenceSkillType.ScrollingText;
}
