using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简单的按钮音效
/// 附加到Button上，点击时播放音效，音量自动关联设置界面的音效音量
/// </summary>
[RequireComponent(typeof(Button))]
public class SimpleButtonSound : MonoBehaviour
{
    [Header("音效设置")]
    [Tooltip("点击时播放的音效")]
    public AudioClip clickSound;

    [Tooltip("音量缩放（0-1），在设置音量基础上再调整")]
    [Range(0f, 1f)]
    public float volumeScale = 1f;

    private Button button;
    private AudioSource audioSource;

    private void Awake()
    {
        button = GetComponent<Button>();

        // 创建一个AudioSource用于播放音效
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void Start()
    {
        // 绑定按钮点击事件
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
        {
            // 使用设置界面的音效音量 * 音量缩放
            float volume = GameSettings.NoteVolume * volumeScale;
            audioSource.PlayOneShot(clickSound, volume);
        }
    }

    private void OnDestroy()
    {
        // 清理监听
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }
}
