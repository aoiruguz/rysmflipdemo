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
        // Get play data from PlayDataCollector
        if (PlayDataCollector.Instance != null)
        {
            playData = PlayDataCollector.Instance.CurrentPlayData;
            DisplayResults();
        }
        else
        {
            Debug.LogWarning("[ResultScreenUI] PlayDataCollector not found! Using dummy data.");
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

        Debug.Log($"[ResultScreenUI] Displayed results for {playData.songName}");
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
