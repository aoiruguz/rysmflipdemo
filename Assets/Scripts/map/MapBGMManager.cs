using UnityEngine;

/// <summary>
/// 地图场景的背景音乐管理器
/// 负责播放地图场景的背景音乐
/// </summary>
public class MapBGMManager : MonoBehaviour
{
    [Header("背景音乐设置")]
    public AudioClip bgmClip;

    private AudioSource audioSource;

    void Awake()
    {
        // 获取或创建 AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 配置 AudioSource
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = GameSettings.MusicVolume;
    }

    void Start()
    {
        // 播放背景音乐
        if (bgmClip != null)
        {
            audioSource.clip = bgmClip;
            audioSource.Play();
            Debug.Log($"[MapBGM] 播放地图背景音乐: {bgmClip.name}");
        }
        else
        {
            Debug.LogWarning("[MapBGM] 未设置背景音乐！");
        }
    }

    // 供外部调用的音量控制方法
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    public float GetVolume()
    {
        return audioSource != null ? audioSource.volume : 0f;
    }
}
