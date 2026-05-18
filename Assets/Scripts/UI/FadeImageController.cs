using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 图片淡入淡出控制器
/// 用于显示"开始PK"、"You Lose"等提示图片
/// </summary>
public class FadeImageController : MonoBehaviour
{
    public static FadeImageController Instance { get; private set; }

    [Header("图片引用")]
    [Tooltip("开始PK的图片")]
    public Image startBattleImage;

    [Tooltip("You Win的图片")]
    public Image youWinImage;

    [Tooltip("You Lose的图片")]
    public Image youLoseImage;

    [Header("淡入淡出设置")]
    [Tooltip("淡入时长（秒）")]
    public float fadeInDuration = 0.5f;

    [Tooltip("显示时长（秒）")]
    public float displayDuration = 1.5f;

    [Tooltip("淡出时长（秒）")]
    public float fadeOutDuration = 0.5f;

    [Header("音效设置")]
    [Tooltip("开始PK时播放的音效")]
    public AudioClip startBattleSFX;

    [Tooltip("You Win时播放的音效")]
    public AudioClip youWinSFX;

    [Tooltip("You Lose时播放的音效")]
    public AudioClip youLoseSFX;

    [Tooltip("音效播放的AudioSource（如果为空则自动创建）")]
    public AudioSource sfxAudioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 初始化AudioSource
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.loop = false;
        }

        // 初始化：隐藏所有图片
        if (startBattleImage != null)
        {
            SetImageAlpha(startBattleImage, 0f);
            startBattleImage.gameObject.SetActive(false);
        }

        if (youWinImage != null)
        {
            SetImageAlpha(youWinImage, 0f);
            youWinImage.gameObject.SetActive(false);
        }

        if (youLoseImage != null)
        {
            SetImageAlpha(youLoseImage, 0f);
            youLoseImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 显示"开始PK"图片（淡入-显示-淡出）
    /// </summary>
    public void ShowStartBattle()
    {
        if (startBattleImage != null)
        {
            PlaySFX(startBattleSFX);
            StartCoroutine(FadeInOutCoroutine(startBattleImage));
        }
        else
        {
            Debug.LogWarning("[FadeImageController] Start Battle Image is not assigned!");
        }
    }

    /// <summary>
    /// 显示"You Win"图片（淡入-显示-淡出）
    /// </summary>
    public void ShowYouWin()
    {
        if (youWinImage != null)
        {
            PlaySFX(youWinSFX);
            StartCoroutine(FadeInOutCoroutine(youWinImage));
        }
        else
        {
            Debug.LogWarning("[FadeImageController] You Win Image is not assigned!");
        }
    }

    /// <summary>
    /// 显示"You Lose"图片（淡入-显示-淡出）
    /// </summary>
    public void ShowYouLose()
    {
        if (youLoseImage != null)
        {
            PlaySFX(youLoseSFX);
            StartCoroutine(FadeInOutCoroutine(youLoseImage));
        }
        else
        {
            Debug.LogWarning("[FadeImageController] You Lose Image is not assigned!");
        }
    }

    /// <summary>
    /// 淡入淡出协程
    /// </summary>
    private IEnumerator FadeInOutCoroutine(Image image)
    {
        // 激活图片
        image.gameObject.SetActive(true);

        // 淡入
        yield return StartCoroutine(FadeIn(image, fadeInDuration));

        // 显示
        yield return new WaitForSeconds(displayDuration);

        // 淡出
        yield return StartCoroutine(FadeOut(image, fadeOutDuration));

        // 隐藏图片
        image.gameObject.SetActive(false);
    }

    /// <summary>
    /// 淡入效果
    /// </summary>
    private IEnumerator FadeIn(Image image, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / duration);
            SetImageAlpha(image, alpha);
            yield return null;
        }
        SetImageAlpha(image, 1f);
    }

    /// <summary>
    /// 淡出效果
    /// </summary>
    private IEnumerator FadeOut(Image image, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / duration);
            SetImageAlpha(image, alpha);
            yield return null;
        }
        SetImageAlpha(image, 0f);
    }

    /// <summary>
    /// 设置图片透明度
    /// </summary>
    private void SetImageAlpha(Image image, float alpha)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    /// <summary>
    /// 播放音效（应用全局音效音量）
    /// </summary>
    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxAudioSource != null)
        {
            // 使用全局音效音量设置
            sfxAudioSource.PlayOneShot(clip, GameSettings.NoteVolume);
        }
    }
}
