using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 全局音频管理器（单例）
/// 管理所有BGM和音效，响应设置中的音量控制
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音频源")]
    [Tooltip("用于播放背景音乐的AudioSource")]
    public AudioSource bgmSource;

    [Tooltip("用于播放音效的AudioSource（可以同时播放多个音效）")]
    public AudioSource sfxSource;

    [Header("音频资源配置")]
    [Tooltip("音频资源配置数据")]
    public AudioClipData audioClipData;

    [Header("淡入淡出设置")]
    [Tooltip("BGM切换时的淡入淡出时间")]
    public float fadeDuration = 1f;

    private Coroutine fadeCoroutine;
    private string currentBGMName;

    private void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化AudioSource
        InitializeAudioSources();

        // 应用当前音量设置
        ApplyVolumeSettings();
    }

    private void InitializeAudioSources()
    {
        // 如果没有指定AudioSource，自动创建
        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM_AudioSource");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX_AudioSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// 应用音量设置（从GameSettings读取）
    /// </summary>
    public void ApplyVolumeSettings()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = GameSettings.MusicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.volume = GameSettings.NoteVolume;
        }
    }

    /// <summary>
    /// 播放BGM（带淡入淡出效果）
    /// </summary>
    public void PlayBGM(string bgmName, bool loop = true, bool fadeIn = true)
    {
        if (audioClipData == null)
        {
            Debug.LogWarning("[AudioManager] AudioClipData未设置！");
            return;
        }

        AudioClip clip = audioClipData.GetBGM(bgmName);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] 未找到BGM: {bgmName}");
            return;
        }

        // 如果正在播放相同的BGM，不重复播放
        if (currentBGMName == bgmName && bgmSource.isPlaying)
        {
            return;
        }

        currentBGMName = bgmName;

        // 停止之前的淡入淡出协程
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (fadeIn && fadeDuration > 0)
        {
            fadeCoroutine = StartCoroutine(FadeInBGM(clip, loop));
        }
        else
        {
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = GameSettings.MusicVolume;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// 停止BGM（带淡出效果）
    /// </summary>
    public void StopBGM(bool fadeOut = true)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        if (fadeOut && fadeDuration > 0)
        {
            fadeCoroutine = StartCoroutine(FadeOutBGM());
        }
        else
        {
            bgmSource.Stop();
            currentBGMName = null;
        }
    }

    /// <summary>
    /// 暂停BGM
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    /// <summary>
    /// 恢复BGM
    /// </summary>
    public void ResumeBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.UnPause();
        }
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySFX(string sfxName, float volumeScale = 1f)
    {
        if (audioClipData == null)
        {
            Debug.LogWarning("[AudioManager] AudioClipData未设置！");
            return;
        }

        AudioClip clip = audioClipData.GetSFX(sfxName);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] 未找到SFX: {sfxName}");
            return;
        }

        // 使用PlayOneShot可以同时播放多个音效
        sfxSource.PlayOneShot(clip, GameSettings.NoteVolume * volumeScale);
    }

    /// <summary>
    /// 播放音效（直接传入AudioClip）
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] AudioClip为空！");
            return;
        }

        sfxSource.PlayOneShot(clip, GameSettings.NoteVolume * volumeScale);
    }

    /// <summary>
    /// 设置BGM音量
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = Mathf.Clamp01(volume);
        }
    }

    /// <summary>
    /// 设置SFX音量
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    // ==================== 淡入淡出协程 ====================

    private IEnumerator FadeInBGM(AudioClip clip, bool loop)
    {
        // 先淡出当前BGM
        if (bgmSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutBGM());
        }

        // 设置新的BGM
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = 0f;
        bgmSource.Play();

        // 淡入
        float targetVolume = GameSettings.MusicVolume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
        fadeCoroutine = null;
    }

    private IEnumerator FadeOutBGM()
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();
        currentBGMName = null;
        fadeCoroutine = null;
    }

    // ==================== 调试方法 ====================

    /// <summary>
    /// 获取当前播放的BGM名称
    /// </summary>
    public string GetCurrentBGMName()
    {
        return currentBGMName;
    }

    /// <summary>
    /// 检查BGM是否正在播放
    /// </summary>
    public bool IsBGMPlaying()
    {
        return bgmSource != null && bgmSource.isPlaying;
    }
}
