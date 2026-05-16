    using UnityEngine;
using DG.Tweening;

/// <summary>
/// 音频淡入淡出控制器
/// 使用 DOTween 实现平滑的音量过渡效果
/// </summary>
public class AudioFadeController : MonoBehaviour
{
    [Header("音频源")]
    [Tooltip("要控制的 AudioSource 组件")]
    public AudioSource audioSource;

    [Header("默认设置")]
    [Tooltip("默认淡入时长（秒）")]
    public float defaultFadeDuration = 2f;

    [Tooltip("目标音量（0-1）")]
    [Range(0f, 1f)]
    public float targetVolume = 1f;

    [Header("播放控制")]
    [Tooltip("是否在 Start 时自动淡入播放")]
    public bool playOnStart = false;

    [Tooltip("启用调试日志")]
    public bool enableDebugLog = true;

    private Tweener _currentTween;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogError("[AudioFadeController] 未找到 AudioSource 组件！", this);
        }
    }

    private void Start()
    {
        if (playOnStart && audioSource != null)
        {
            FadeIn();
        }
    }

    /// <summary>
    /// 淡入播放音乐
    /// </summary>
    /// <param name="duration">淡入时长（秒）</param>
    /// <param name="targetVol">目标音量（0-1）</param>
    public void FadeIn(float duration = -1f, float targetVol = -1f)
    {
        if (audioSource == null)
        {
            Debug.LogError("[AudioFadeController] AudioSource 为空，无法淡入！", this);
            return;
        }

        if (duration < 0) duration = defaultFadeDuration;
        if (targetVol < 0) targetVol = targetVolume;

        // 停止之前的淡入淡出动画
        StopFade();

        // 设置初始音量为0
        audioSource.volume = 0f;

        // 如果没有在播放，开始播放
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            if (enableDebugLog)
                Debug.Log($"[AudioFadeController] 开始播放并淡入: {audioSource.clip?.name}, 时长={duration}秒, 目标音量={targetVol}", this);
        }
        else
        {
            if (enableDebugLog)
                Debug.Log($"[AudioFadeController] 淡入音量: 时长={duration}秒, 目标音量={targetVol}", this);
        }

        // 执行淡入动画
        _currentTween = audioSource.DOFade(targetVol, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (enableDebugLog && Time.frameCount % 30 == 0) // 每30帧打印一次
                    Debug.Log($"[AudioFadeController] 当前音量: {audioSource.volume:F2}", this);
            })
            .OnComplete(() =>
            {
                if (enableDebugLog)
                    Debug.Log($"[AudioFadeController] 淡入完成，最终音量: {audioSource.volume:F2}", this);
            });
    }

    /// <summary>
    /// 淡出停止音乐
    /// </summary>
    /// <param name="duration">淡出时长（秒）</param>
    /// <param name="stopAfterFade">淡出后是否停止播放</param>
    public void FadeOut(float duration = -1f, bool stopAfterFade = true)
    {
        if (audioSource == null)
        {
            Debug.LogError("[AudioFadeController] AudioSource 为空，无法淡出！", this);
            return;
        }

        if (duration < 0) duration = defaultFadeDuration;

        // 停止之前的淡入淡出动画
        StopFade();

        if (enableDebugLog)
            Debug.Log($"[AudioFadeController] 开始淡出: 时长={duration}秒, 停止播放={stopAfterFade}", this);

        _currentTween = audioSource.DOFade(0f, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (stopAfterFade)
                {
                    audioSource.Stop();
                    if (enableDebugLog)
                        Debug.Log("[AudioFadeController] 淡出完成并停止播放", this);
                }
                else
                {
                    if (enableDebugLog)
                        Debug.Log("[AudioFadeController] 淡出完成（继续播放）", this);
                }
            });
    }

    /// <summary>
    /// 交叉淡入淡出（切换音乐）
    /// </summary>
    /// <param name="newClip">新的音频片段</param>
    /// <param name="fadeDuration">淡入淡出时长（秒）</param>
    public void CrossFade(AudioClip newClip, float fadeDuration = -1f)
    {
        if (fadeDuration < 0) fadeDuration = defaultFadeDuration;

        FadeOut(fadeDuration, false);

        DOVirtual.DelayedCall(fadeDuration, () =>
        {
            audioSource.clip = newClip;
            FadeIn(fadeDuration);
        });
    }

    /// <summary>
    /// 停止当前的淡入淡出动画
    /// </summary>
    public void StopFade()
    {
        if (_currentTween != null && _currentTween.IsActive())
        {
            _currentTween.Kill();
            _currentTween = null;
        }
    }

    /// <summary>
    /// 立即设置音量（不淡入淡出）
    /// </summary>
    public void SetVolumeImmediate(float volume)
    {
        StopFade();
        audioSource.volume = Mathf.Clamp01(volume);
    }

    private void OnDestroy()
    {
        StopFade();
    }
}
