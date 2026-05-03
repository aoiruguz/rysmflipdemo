using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 用于测试结算界面的辅助脚本
/// 可以在Game Over Scene中直接测试UI显示
/// </summary>
public class ResultScreenTester : MonoBehaviour
{
    [Header("Test Data")]
    public string testSongName = "Test Song";
    public ChartDifficulty testDifficulty = ChartDifficulty.Normal;
    public int testPerfect = 450;
    public int testGreat = 40;
    public int testGood = 8;
    public int testMiss = 2;
    public int testMaxCombo = 480;
    public int testTotalNotes = 500;
    public int testLate = 25;
    public int testFast = 23;

    [Header("Actions")]
    [Tooltip("在Inspector中勾选此项来创建测试数据")]
    public bool createTestData = false;

    void Update()
    {
        if (createTestData)
        {
            createTestData = false;
            CreateTestPlayData();
        }
    }

    [ContextMenu("Create Test Play Data")]
    public void CreateTestPlayData()
    {
        // Create a temporary PlayDataCollector if it doesn't exist
        if (PlayDataCollector.Instance == null)
        {
            GameObject collectorObj = new GameObject("PlayDataCollector (Test)");
            PlayDataCollector collector = collectorObj.AddComponent<PlayDataCollector>();

            // Manually set the instance
            typeof(PlayDataCollector)
                .GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .SetValue(null, collector);
        }

        // Set test data
        PlayData testData = new PlayData
        {
            songName = testSongName,
            difficulty = testDifficulty,
            perfectCount = testPerfect,
            greatCount = testGreat,
            goodCount = testGood,
            missCount = testMiss,
            maxCombo = testMaxCombo,
            totalNotes = testTotalNotes,
            lateCount = testLate,
            fastCount = testFast,
            playTime = 120f
        };

        // Set the current play data
        typeof(PlayDataCollector)
            .GetProperty("CurrentPlayData", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .SetValue(PlayDataCollector.Instance, testData);

        Debug.Log($"[ResultScreenTester] Created test data: {testData.songName}");
        Debug.Log($"Achievement Rate: {testData.GetAchievementRate():F2}%");
        Debug.Log($"Rank: {testData.GetRank()}");
        Debug.Log($"Full Combo: {testData.IsFullCombo()}");
        Debug.Log($"All Perfect: {testData.IsAllPerfect()}");

        // Reload the scene to refresh UI
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [ContextMenu("Test S Rank Data")]
    public void TestSRankData()
    {
        testPerfect = 495;
        testGreat = 5;
        testGood = 0;
        testMiss = 0;
        testMaxCombo = 500;
        testTotalNotes = 500;
        testLate = 3;
        testFast = 2;
        CreateTestPlayData();
    }

    [ContextMenu("Test A Rank Data")]
    public void TestARankData()
    {
        testPerfect = 450;
        testGreat = 40;
        testGood = 8;
        testMiss = 2;
        testMaxCombo = 480;
        testTotalNotes = 500;
        testLate = 25;
        testFast = 23;
        CreateTestPlayData();
    }

    [ContextMenu("Test Full Combo")]
    public void TestFullComboData()
    {
        testPerfect = 400;
        testGreat = 80;
        testGood = 20;
        testMiss = 0;
        testMaxCombo = 500;
        testTotalNotes = 500;
        testLate = 50;
        testFast = 50;
        CreateTestPlayData();
    }

    [ContextMenu("Test All Perfect")]
    public void TestAllPerfectData()
    {
        testPerfect = 500;
        testGreat = 0;
        testGood = 0;
        testMiss = 0;
        testMaxCombo = 500;
        testTotalNotes = 500;
        testLate = 0;
        testFast = 0;
        CreateTestPlayData();
    }
}
