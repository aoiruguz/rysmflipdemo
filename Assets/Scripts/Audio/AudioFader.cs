using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioFader : MonoBehaviour
{
    [Header("Fader Settings")]
    [Tooltip("是否在游戏开始时自动播放并淡入")]
    public bool playAndFadeOnStart = false;
    
    [Tooltip("默认的淡入/淡出时间（秒）")]
    public float defaultFadeDuration = 2f;
    
    [Tooltip("淡入完成后的目标音量")]
    [Range(0f, 1f)]
    public float defaultTargetVolume = 1f;

    private AudioSource audioSource;
    private Coroutine currentFadeCoroutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // 如果开启了自动淡入，我们在Awake时立刻把音量设为0，防止游戏开始第一帧出现爆音
        if (playAndFadeOnStart)
        {
            audioSource.volume = 0f;
        }
    }

    private void Start()
    {
        // 在Start中调用淡入，确保所有组件都已初始化完成
        if (playAndFadeOnStart)
        {
            FadeIn(defaultFadeDuration, defaultTargetVolume);
        }
    }

    /// <summary>
    /// 使用 Inspector 中的默认设置淡入音频
    /// </summary>
    public void FadeIn()
    {
        FadeIn(defaultFadeDuration, defaultTargetVolume);
    }

    /// <summary>
    /// 使用 Inspector 中的默认设置淡出音频
    /// </summary>
    public void FadeOut()
    {
        FadeOut(defaultFadeDuration);
    }

    /// <summary>
    /// 自定义参数淡入音频
    /// </summary>
    public void FadeIn(float duration, float targetVolume)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentFadeCoroutine = StartCoroutine(FadeInCoroutine(duration, targetVolume));
    }

    /// <summary>
    /// 自定义参数淡出音频
    /// </summary>
    public void FadeOut(float duration)
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        currentFadeCoroutine = StartCoroutine(FadeOutCoroutine(duration));
    }

    private IEnumerator FadeInCoroutine(float duration, float targetVolume)
    {
        // 确保音频源已播放
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        float currentTime = 0f;
        float startVolume = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        currentFadeCoroutine = null;
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float currentTime = 0f;
        float startVolume = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, currentTime / duration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        
        // 停止后恢复原始音量，这样如果你不使用Fade而是直接调用Play()，也会有声音
        audioSource.volume = startVolume;
        currentFadeCoroutine = null;
    }
}
