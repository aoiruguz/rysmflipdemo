using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 音频资源配置数据（ScriptableObject）
/// 用于集中管理所有BGM和音效资源
/// </summary>
[CreateAssetMenu(fileName = "AudioClipData", menuName = "Audio/Audio Clip Data")]
public class AudioClipData : ScriptableObject
{
    [System.Serializable]
    public class AudioClipEntry
    {
        [Tooltip("音频名称（用于代码中引用）")]
        public string name;

        [Tooltip("音频文件")]
        public AudioClip clip;
    }

    [Header("背景音乐")]
    [Tooltip("所有BGM音频资源")]
    public List<AudioClipEntry> bgmList = new List<AudioClipEntry>();

    [Header("音效")]
    [Tooltip("所有SFX音频资源")]
    public List<AudioClipEntry> sfxList = new List<AudioClipEntry>();

    // 缓存字典，提高查找效率
    private Dictionary<string, AudioClip> bgmDict;
    private Dictionary<string, AudioClip> sfxDict;

    private void OnEnable()
    {
        BuildDictionaries();
    }

    /// <summary>
    /// 构建查找字典
    /// </summary>
    private void BuildDictionaries()
    {
        bgmDict = new Dictionary<string, AudioClip>();
        foreach (var entry in bgmList)
        {
            if (!string.IsNullOrEmpty(entry.name) && entry.clip != null)
            {
                if (!bgmDict.ContainsKey(entry.name))
                {
                    bgmDict[entry.name] = entry.clip;
                }
                else
                {
                    Debug.LogWarning($"[AudioClipData] 重复的BGM名称: {entry.name}");
                }
            }
        }

        sfxDict = new Dictionary<string, AudioClip>();
        foreach (var entry in sfxList)
        {
            if (!string.IsNullOrEmpty(entry.name) && entry.clip != null)
            {
                if (!sfxDict.ContainsKey(entry.name))
                {
                    sfxDict[entry.name] = entry.clip;
                }
                else
                {
                    Debug.LogWarning($"[AudioClipData] 重复的SFX名称: {entry.name}");
                }
            }
        }
    }

    /// <summary>
    /// 获取BGM音频
    /// </summary>
    public AudioClip GetBGM(string name)
    {
        if (bgmDict == null || bgmDict.Count == 0)
        {
            BuildDictionaries();
        }

        if (bgmDict.TryGetValue(name, out AudioClip clip))
        {
            return clip;
        }

        Debug.LogWarning($"[AudioClipData] 未找到BGM: {name}");
        return null;
    }

    /// <summary>
    /// 获取SFX音频
    /// </summary>
    public AudioClip GetSFX(string name)
    {
        if (sfxDict == null || sfxDict.Count == 0)
        {
            BuildDictionaries();
        }

        if (sfxDict.TryGetValue(name, out AudioClip clip))
        {
            return clip;
        }

        Debug.LogWarning($"[AudioClipData] 未找到SFX: {name}");
        return null;
    }

    /// <summary>
    /// 检查BGM是否存在
    /// </summary>
    public bool HasBGM(string name)
    {
        if (bgmDict == null || bgmDict.Count == 0)
        {
            BuildDictionaries();
        }

        return bgmDict.ContainsKey(name);
    }

    /// <summary>
    /// 检查SFX是否存在
    /// </summary>
    public bool HasSFX(string name)
    {
        if (sfxDict == null || sfxDict.Count == 0)
        {
            BuildDictionaries();
        }

        return sfxDict.ContainsKey(name);
    }

    /// <summary>
    /// 获取所有BGM名称
    /// </summary>
    public List<string> GetAllBGMNames()
    {
        List<string> names = new List<string>();
        foreach (var entry in bgmList)
        {
            if (!string.IsNullOrEmpty(entry.name))
            {
                names.Add(entry.name);
            }
        }
        return names;
    }

    /// <summary>
    /// 获取所有SFX名称
    /// </summary>
    public List<string> GetAllSFXNames()
    {
        List<string> names = new List<string>();
        foreach (var entry in sfxList)
        {
            if (!string.IsNullOrEmpty(entry.name))
            {
                names.Add(entry.name);
            }
        }
        return names;
    }
}
