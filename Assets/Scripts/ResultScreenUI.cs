using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 结算界面UI管理器
/// 显示游玩结果、评级、判定统计等信息
/// </summary>
public class ResultScreenUI : MonoBehaviour
{
    [Header("Rank Display")]
    public Image rankIcon;
    public Sprite rankS;
    public Sprite rankA;
    public Sprite rankB;
    public Sprite rankC;
    public Sprite rankF;

    [Header("Special Icons")]
    public GameObject fullComboIcon;
    public GameObject allPerfectIcon;

    [Header("Score Display")]
    public TextMeshProUGUI achievementRateText;
    public TextMeshProUGUI achievementScoreText;

    [Header("Play Count & Fans Display")]
    public TextMeshProUGUI playCountText; // 播放量
    public TextMeshProUGUI newFansText; // 本局获得粉丝
    public TextMeshProUGUI totalFansText; // 总粉丝数

    [Header("Combo Display")]
    public TextMeshProUGUI maxComboText;
    public TextMeshProUGUI totalNotesText;

    [Header("Judgment Counts")]
    public TextMeshProUGUI perfectCountText;
    public TextMeshProUGUI greatCountText;
    public TextMeshProUGUI goodCountText;
    public TextMeshProUGUI missCountText;

    [Header("Timing Display")]
    public TextMeshProUGUI lateCountText;
    public TextMeshProUGUI fastCountText;

    [Header("Song Info")]
    public TextMeshProUGUI songNameText;
    public TextMeshProUGUI difficultyText;

    [Header("Animation")]
    public float animationDelay = 0.5f;
    public float countUpDuration = 1.0f;

    private PlayData playData;

    void Start()
    {
        // 如果在 GameOver 场景中，从 PlayDataCollector 获取数据
        // 如果在 PlayScene 中，等待 SongCompletionDetector 调用 DisplayResultsFromPlayData
        if (PlayDataCollector.Instance != null && PlayDataCollector.Instance.CurrentPlayData != null)
        {
            playData = PlayDataCollector.Instance.CurrentPlayData;
            DisplayResults();
        }
    }

    /// <summary>
    /// 从外部传入 PlayData 并显示结果（用于同场景内调用）
    /// </summary>
    public void DisplayResultsFromPlayData(PlayData data)
    {
        if (data != null)
        {
            playData = data;
            DisplayResults();
        }
        else
        {
            Debug.LogWarning("[ResultScreenUI] PlayData is null! Using dummy data.");
            playData = CreateDummyData();
            DisplayResults();
        }
    }

    /// <summary>
    /// 显示结算结果
    /// </summary>
    private void DisplayResults()
    {
        if (playData == null)
        {
            Debug.LogError("[ResultScreenUI] PlayData is null!");
            return;
        }

        // Song Info
        if (songNameText != null)
            songNameText.text = playData.songName;

        if (difficultyText != null)
            difficultyText.text = playData.difficulty.ToString();

        // Rank Icon
        DisplayRankIcon(playData.GetRank());

        // Special Icons
        if (fullComboIcon != null)
            fullComboIcon.SetActive(playData.IsFullCombo());

        if (allPerfectIcon != null)
            allPerfectIcon.SetActive(playData.IsAllPerfect());

        // Achievement Rate
        float achievementRate = playData.GetAchievementRate();
        if (achievementRateText != null)
            achievementRateText.text = $"{achievementRate:F2}%";

        if (achievementScoreText != null)
            achievementScoreText.text = playData.GetAchievementScore().ToString("D7");

        // Combo
        if (maxComboText != null)
            maxComboText.text = playData.maxCombo.ToString();

        if (totalNotesText != null)
            totalNotesText.text = playData.totalNotes.ToString();

        // Judgment Counts
        if (perfectCountText != null)
            perfectCountText.text = playData.perfectCount.ToString();

        if (greatCountText != null)
            greatCountText.text = playData.greatCount.ToString();

        if (goodCountText != null)
            goodCountText.text = playData.goodCount.ToString();

        if (missCountText != null)
            missCountText.text = playData.missCount.ToString();

        // Late/Fast Counts
        if (lateCountText != null)
            lateCountText.text = playData.lateCount.ToString();

        if (fastCountText != null)
            fastCountText.text = playData.fastCount.ToString();

        // 播放量和粉丝数显示
        DisplayPlayCountAndFans();

        Debug.Log($"[ResultScreenUI] Displayed results for {playData.songName}");
    }

    /// <summary>
    /// 显示播放量和粉丝数
    /// </summary>
    private void DisplayPlayCountAndFans()
    {
        // 计算播放量
        long playCount = ScoreCalculator.CalculatePlayCount(
            playData.chapterIndex,
            playData.isBossStage,
            playData.perfectCount,
            playData.greatCount,
            playData.goodCount,
            playData.missCount,
            playData.maxCombo,
            playData.isReplay);

        // 计算本局获得粉丝
        long newFans = ScoreCalculator.CalculateFansGain(playCount, playData.isReplay);

        // 获取总粉丝数
        long totalFans = FansDataManager.GetTotalFans();

        // 显示播放量
        if (playCountText != null)
        {
            playCountText.text = ScoreCalculator.FormatLargeNumber(playCount);
        }

        // 显示本局获得粉丝
        if (newFansText != null)
        {
            newFansText.text = $"+{ScoreCalculator.FormatLargeNumber(newFans)}";
        }

        // 显示总粉丝数
        if (totalFansText != null)
        {
            totalFansText.text = ScoreCalculator.FormatLargeNumber(totalFans);
        }

        Debug.Log($"[ResultScreenUI] Play Count: {ScoreCalculator.FormatLargeNumber(playCount)}, New Fans: +{ScoreCalculator.FormatLargeNumber(newFans)}, Total Fans: {ScoreCalculator.FormatLargeNumber(totalFans)}");
    }

    /// <summary>
    /// 显示评级图标
    /// </summary>
    private void DisplayRankIcon(string rank)
    {
        if (rankIcon == null) return;

        Sprite sprite = rank switch
        {
            "S" => rankS,
            "A" => rankA,
            "B" => rankB,
            "C" => rankC,
            "F" => rankF,
            _ => rankF
        };

        if (sprite != null)
        {
            rankIcon.sprite = sprite;
            rankIcon.enabled = true;
        }
        else
        {
            Debug.LogWarning($"[ResultScreenUI] Rank sprite for '{rank}' not assigned!");
        }
    }

    /// <summary>
    /// 创建测试用的假数据
    /// </summary>
    private PlayData CreateDummyData()
    {
        return new PlayData
        {
            songName = "Test Song",
            difficulty = ChartDifficulty.Normal,
            perfectCount = 450,
            greatCount = 40,
            goodCount = 8,
            missCount = 2,
            maxCombo = 480,
            totalNotes = 500,
            lateCount = 25,
            fastCount = 23,
            playTime = 120f
        };
    }
}
