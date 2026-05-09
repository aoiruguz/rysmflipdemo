using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 制谱器判定检测器
/// 播放时检测note经过判定线，播放判定音效
/// </summary>
public class EditorJudgmentDetector : MonoBehaviour
{
    [Header("References")]
    public ChartEditorManager manager;
    public AudioSource hitSoundSource;

    [Header("Hit Sounds")]
    public AudioClip perfectHitSound;
    public AudioClip greatHitSound;
    public AudioClip goodHitSound;

    [Header("Judgment Settings")]
    public float judgmentLineY = -4f; // 判定线Y位置
    public float judgmentWindow = 0.05f; // 判定窗口（秒），用于避免重复判定

    // 已判定的note（避免重复播放音效）
    private HashSet<EditorNote> judgedNotes = new HashSet<EditorNote>();
    private float lastJudgmentTime = 0f;

    void Start()
    {
        if (manager == null)
        {
            manager = ChartEditorManager.Instance;
        }

        // 创建独立的音效AudioSource
        if (hitSoundSource == null)
        {
            hitSoundSource = gameObject.AddComponent<AudioSource>();
            hitSoundSource.playOnAwake = false;
            hitSoundSource.volume = ChartEditorSettings.HitSoundVolume;
        }

        // 加载默认音效（如果未设置）
        if (perfectHitSound == null)
        {
            perfectHitSound = Resources.Load<AudioClip>("Audio/HitSounds/Perfect");
        }
        if (greatHitSound == null)
        {
            greatHitSound = Resources.Load<AudioClip>("Audio/HitSounds/Great");
        }
        if (goodHitSound == null)
        {
            goodHitSound = Resources.Load<AudioClip>("Audio/HitSounds/Good");
        }
    }

    void Update()
    {
        // 只在播放模式下检测判定
        if (!manager.IsPlaying) return;

        float currentTime = manager.GetCurrentTime();

        // 检测所有note
        foreach (var editorNote in FindObjectsByType<EditorNote>(FindObjectsSortMode.None))
        {
            // 跳过已判定的note
            if (judgedNotes.Contains(editorNote)) continue;

            // 获取note的时间和Y位置
            float noteTime = editorNote.GetNoteTime();
            float noteY = editorNote.transform.position.y;

            // 检测是否经过判定线
            if (HasPassedJudgmentLine(noteY, noteTime, currentTime))
            {
                // 播放判定音效
                PlayJudgmentSound(noteTime, currentTime);

                // 标记为已判定
                judgedNotes.Add(editorNote);
            }
        }
    }

    /// <summary>
    /// 检测note是否经过判定线
    /// 判定线相对于镜头固定，所以需要计算镜头位置
    /// </summary>
    private bool HasPassedJudgmentLine(float noteY, float noteTime, float currentTime)
    {
        // 获取镜头当前位置
        float cameraY = Camera.main.transform.position.y;

        // 计算判定线在世界空间的实际位置（镜头Y + 判定线相对偏移）
        float actualJudgmentLineY = cameraY + judgmentLineY;

        // 检测note的Y位置是否已经到达或低于判定线
        bool passedByPosition = noteY <= actualJudgmentLineY + 0.5f;

        // 同时检查时间（避免误判）
        bool passedByTime = currentTime >= noteTime - 0.1f;

        return passedByPosition && passedByTime;
    }

    /// <summary>
    /// 播放判定音效
    /// </summary>
    private void PlayJudgmentSound(float noteTime, float currentTime)
    {
        // 避免在同一帧播放多个音效
        if (Time.time - lastJudgmentTime < judgmentWindow)
        {
            return;
        }

        // 计算判定精度（可选，用于选择不同音效）
        float timeDiff = Mathf.Abs(currentTime - noteTime);

        AudioClip soundToPlay = null;

        if (timeDiff < 0.05f) // Perfect
        {
            soundToPlay = perfectHitSound;
        }
        else if (timeDiff < 0.1f) // Great
        {
            soundToPlay = greatHitSound;
        }
        else // Good
        {
            soundToPlay = goodHitSound;
        }

        // 播放音效
        if (soundToPlay != null && hitSoundSource != null)
        {
            hitSoundSource.PlayOneShot(soundToPlay);
            lastJudgmentTime = Time.time;
        }
    }

    /// <summary>
    /// 重置判定状态（停止播放时调用）
    /// </summary>
    public void ResetJudgment()
    {
        judgedNotes.Clear();
        lastJudgmentTime = 0f;
    }

    /// <summary>
    /// 绘制判定线（用于调试）
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(-10, judgmentLineY, 0), new Vector3(10, judgmentLineY, 0));
    }
}
