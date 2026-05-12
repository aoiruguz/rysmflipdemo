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
    [Header("Difficulty Display")]
    public GameObject difficultyIconObject;
    [Tooltip("按顺序放入图片: 0:Easy, 1:Normal, 2:Hard")]
    public Sprite[] difficultySprites;

    [Header("Rank Display")]
    public GameObject rankIconObject; // 评级显示的物体
    public Sprite[] rankSprites;      // 所有的评级图片数组
    public GameObject rankBackgroundObject; // 评级背景物体
    [Tooltip("按顺序放入图片: 0:S, 1:A, 2:B, 3:C, 4:F")]
    public Sprite[] rankBackgroundSprites; // 评级背景图片数组

    [Header("Level (Rating) Display")]
    public GameObject levelIconObject;
    [Tooltip("按顺序放入图片: 0:LV1, 1:LV2, ... 9:LV10")]
    public Sprite[] levelSprites;

    [Header("Special Icons")]
    public GameObject fullComboIcon;
    public GameObject allPerfectIcon;

    [Header("Score Display")]
    public TextMeshProUGUI achievementRateText;

    [Header("Play Count & Fans Display")]
    public TextMeshProUGUI playCountText; // 播放量
    public TextMeshProUGUI totalFansText; // 总粉丝数

    [Header("Combo Display")]
    public TextMeshProUGUI maxComboText;

    [Header("Judgment Counts")]
    public TextMeshProUGUI perfectCountText;
    public TextMeshProUGUI greatCountText;
    public TextMeshProUGUI goodCountText;
    public TextMeshProUGUI missCountText;

    [Header("Song Info")]
    public TextMeshProUGUI songNameText;

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

        // Difficulty Icon
        DisplayDifficultyIcon(playData.difficulty);

        // Rank Icon
        DisplayRankIcon(playData.GetRank());

        // Level Icon (Rating LV.1 - LV.10)
        DisplayLevelIcon();

        // Special Icons
        if (fullComboIcon != null)
            fullComboIcon.SetActive(playData.IsFullCombo());

        if (allPerfectIcon != null)
            allPerfectIcon.SetActive(playData.IsAllPerfect());

        // Achievement Rate
        float achievementRate = playData.GetAchievementRate();
        if (achievementRateText != null)
            achievementRateText.text = $"{achievementRate:F2}%";

        // Combo
        if (maxComboText != null)
            maxComboText.text = playData.maxCombo.ToString();

        // Judgment Counts
        if (perfectCountText != null)
            perfectCountText.text = playData.perfectCount.ToString();

        if (greatCountText != null)
            greatCountText.text = playData.greatCount.ToString();

        if (goodCountText != null)
            goodCountText.text = playData.goodCount.ToString();

        if (missCountText != null)
            missCountText.text = playData.missCount.ToString();

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
        if (rankIconObject == null || rankSprites == null || rankSprites.Length == 0) return;

        // 直接获取物体上的 Image 组件并修改图片
        Image img = rankIconObject.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogWarning($"[ResultScreenUI] {rankIconObject.name} has no Image component!");
            return;
        }

        int index = rank switch
        {
            "S" => 0,
            "A" => 1,
            "B" => 2,
            "C" => 3,
            "F" => 4,
            _ => 4
        };

        if (index < rankSprites.Length && rankSprites[index] != null)
        {
            img.sprite = rankSprites[index];
            img.SetNativeSize(); // 同步修改 RectTransform 的宽高
            img.enabled = true;
        }
        else
        {
            Debug.LogWarning($"[ResultScreenUI] Rank sprite for '{rank}' at index {index} not assigned!");
        }

        // --- 设置评级背景 ---
        if (rankBackgroundObject != null && rankBackgroundSprites != null && rankBackgroundSprites.Length > 0)
        {
            Image bgImg = rankBackgroundObject.GetComponent<Image>();
            if (bgImg == null)
            {
                Debug.LogWarning($"[ResultScreenUI] {rankBackgroundObject.name} has no Image component!");
            }
            else
            {
                // 确保索引不越界（如果背景图只有4张而Rank有5个，会自动取最后一张）
                int bgIndex = Mathf.Clamp(index, 0, rankBackgroundSprites.Length - 1);
                if (rankBackgroundSprites[bgIndex] != null)
                {
                    bgImg.sprite = rankBackgroundSprites[bgIndex];
                    bgImg.SetNativeSize();
                    bgImg.enabled = true;
                }
            }
        }
    }

    /// <summary>
    /// 显示难度图标
    /// </summary>
    private void DisplayDifficultyIcon(ChartDifficulty difficulty)
    {
        if (difficultyIconObject == null || difficultySprites == null || difficultySprites.Length == 0) return;

        Image img = difficultyIconObject.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogWarning($"[ResultScreenUI] {difficultyIconObject.name} has no Image component!");
            return;
        }

        int index = (int)difficulty;

        if (index < difficultySprites.Length && difficultySprites[index] != null)
        {
            img.sprite = difficultySprites[index];
            img.SetNativeSize(); // 同步修改 RectTransform 的宽高
            img.enabled = true;
        }
        else
        {
            Debug.LogWarning($"[ResultScreenUI] Difficulty sprite for '{difficulty}' at index {index} not assigned!");
        }
    }

    /// <summary>
    /// 显示等级(RatingLevel)图标
    /// </summary>
    private void DisplayLevelIcon()
    {
        if (levelIconObject == null || levelSprites == null || levelSprites.Length == 0) return;

        // 1. 获取 LevelData 和 ChartData (仿照剧情读取逻辑)
        LevelData currentLevelData = LevelUIManager.GetCurrentLevelData();
        ChartData currentChart = LevelUIManager.GetSelectedChart();

        if (currentLevelData == null || currentChart == null)
        {
            Debug.LogWarning("[ResultScreenUI] LevelData or ChartData is null, cannot display level icon.");
            return;
        }

        // 2. 查找当前难度的 ratingLevel
        int ratingLevel = 0;
        foreach (var detail in currentLevelData.difficulties)
        {
            if (detail.difficulty == currentChart.difficulty)
            {
                ratingLevel = detail.ratingLevel;
                break;
            }
        }

        // 3. 显示对应的图片 (ratingLevel 通常是 1-10)
        Image img = levelIconObject.GetComponent<Image>();
        if (img == null)
        {
            Debug.LogWarning($"[ResultScreenUI] {levelIconObject.name} has no Image component!");
            return;
        }

        int index = Mathf.Clamp(ratingLevel - 1, 0, levelSprites.Length - 1);

        if (index < levelSprites.Length && levelSprites[index] != null)
        {
            img.sprite = levelSprites[index];
            img.SetNativeSize(); // 同步修改 RectTransform 的宽高，实现自适应缩放
            img.enabled = true;
        }
        else
        {
            Debug.LogWarning($"[ResultScreenUI] Level sprite for rating {ratingLevel} at index {index} not assigned!");
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
