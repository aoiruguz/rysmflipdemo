using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 按钮音效组件
/// 附加到Button上，自动播放点击音效
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSoundEffect : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("音效设置")]
    [Tooltip("点击时播放的音效名称（在AudioClipData中配置）")]
    public string clickSoundName = "ButtonClick";

    [Tooltip("或者直接指定音效文件")]
    public AudioClip clickSoundClip;

    [Tooltip("鼠标悬停时播放的音效名称（可选）")]
    public string hoverSoundName = "";

    [Tooltip("或者直接指定悬停音效文件")]
    public AudioClip hoverSoundClip;

    [Header("音量设置")]
    [Tooltip("点击音效音量缩放（0-1）")]
    [Range(0f, 1f)]
    public float clickVolumeScale = 1f;

    [Tooltip("悬停音效音量缩放（0-1）")]
    [Range(0f, 1f)]
    public float hoverVolumeScale = 0.5f;

    [Header("其他设置")]
    [Tooltip("是否在Start时自动添加点击监听")]
    public bool autoAddListener = true;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        if (autoAddListener && button != null)
        {
            // 添加点击监听
            button.onClick.AddListener(PlayClickSound);
        }
    }

    /// <summary>
    /// 播放点击音效
    /// </summary>
    public void PlayClickSound()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[ButtonSoundEffect] AudioManager未初始化！");
            return;
        }

        // 优先使用直接指定的AudioClip
        if (clickSoundClip != null)
        {
            AudioManager.Instance.PlaySFX(clickSoundClip, clickVolumeScale);
        }
        // 否则使用名称查找
        else if (!string.IsNullOrEmpty(clickSoundName))
        {
            AudioManager.Instance.PlaySFX(clickSoundName, clickVolumeScale);
        }
    }

    /// <summary>
    /// 播放悬停音效
    /// </summary>
    public void PlayHoverSound()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        // 优先使用直接指定的AudioClip
        if (hoverSoundClip != null)
        {
            AudioManager.Instance.PlaySFX(hoverSoundClip, hoverVolumeScale);
        }
        // 否则使用名称查找
        else if (!string.IsNullOrEmpty(hoverSoundName))
        {
            AudioManager.Instance.PlaySFX(hoverSoundName, hoverVolumeScale);
        }
    }

    // 实现IPointerEnterHandler接口
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 只有在按钮可交互时才播放悬停音效
        if (button != null && button.interactable)
        {
            PlayHoverSound();
        }
    }

    // 实现IPointerClickHandler接口（作为备用）
    public void OnPointerClick(PointerEventData eventData)
    {
        // 这个方法作为备用，主要还是通过Button.onClick触发
        // 如果autoAddListener为false，可以通过这个接口触发
        if (!autoAddListener && button != null && button.interactable)
        {
            PlayClickSound();
        }
    }

    private void OnDestroy()
    {
        // 清理监听
        if (button != null && autoAddListener)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }
}
