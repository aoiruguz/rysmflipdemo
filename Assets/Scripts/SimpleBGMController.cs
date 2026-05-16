using UnityEngine;

/// <summary>
/// 简单的BGM控制器
/// 附加到场景中的AudioSource上，自动关联设置界面的音乐音量
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SimpleBGMController : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // 应用初始音量
        UpdateVolume();
    }

    private void Update()
    {
        // 实时更新音量（响应设置界面的变化）
        UpdateVolume();
    }

    private void UpdateVolume()
    {
        if (audioSource != null)
        {
            audioSource.volume = GameSettings.MusicVolume;
        }
    }
}
