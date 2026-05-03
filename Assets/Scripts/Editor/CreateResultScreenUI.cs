using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor工具：在Game Over Scene中创建结算界面UI
/// </summary>
public class CreateResultScreenUI : EditorWindow
{
    [MenuItem("Tools/Create Result Screen UI")]
    public static void CreateUI()
    {
        // Open Game Over scene
        string scenePath = "Assets/Scenes/Game Over.unity";
        EditorSceneManager.OpenScene(scenePath);

        // Create Canvas if not exists
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }

        // Create main result panel
        GameObject resultPanel = CreatePanel(canvas.transform, "ResultPanel", new Vector2(800, 600));

        // Add ResultScreenUI component
        ResultScreenUI resultUI = resultPanel.AddComponent<ResultScreenUI>();

        // Create UI elements
        CreateRankDisplay(resultPanel.transform, resultUI);
        CreateScoreDisplay(resultPanel.transform, resultUI);
        CreateJudgmentDisplay(resultPanel.transform, resultUI);
        CreateSongInfo(resultPanel.transform, resultUI);

        EditorUtility.SetDirty(resultPanel);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Result Screen UI created successfully!");
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        return panel;
    }

    private static void CreateRankDisplay(Transform parent, ResultScreenUI resultUI)
    {
        // Rank Icon
        GameObject rankObj = new GameObject("RankIcon");
        rankObj.transform.SetParent(parent, false);
        RectTransform rankRect = rankObj.AddComponent<RectTransform>();
        rankRect.anchoredPosition = new Vector2(-250, 150);
        rankRect.sizeDelta = new Vector2(150, 150);

        Image rankImage = rankObj.AddComponent<Image>();
        resultUI.rankIcon = rankImage;

        // Full Combo Icon
        GameObject fcObj = CreateTextObject(parent, "FullComboIcon", new Vector2(-250, 0), "FULL COMBO");
        fcObj.SetActive(false);
        resultUI.fullComboIcon = fcObj;

        // All Perfect Icon
        GameObject apObj = CreateTextObject(parent, "AllPerfectIcon", new Vector2(-250, -50), "ALL PERFECT");
        apObj.SetActive(false);
        resultUI.allPerfectIcon = apObj;
    }

    private static void CreateScoreDisplay(Transform parent, ResultScreenUI resultUI)
    {
        // Achievement Rate
        CreateLabel(parent, "AchievementRateLabel", new Vector2(0, 200), "达成率");
        resultUI.achievementRateText = CreateTextObject(parent, "AchievementRateText", new Vector2(0, 170), "0.00%").GetComponent<TextMeshProUGUI>();

        // Achievement Score
        CreateLabel(parent, "AchievementScoreLabel", new Vector2(0, 120), "分数");
        resultUI.achievementScoreText = CreateTextObject(parent, "AchievementScoreText", new Vector2(0, 90), "0000000").GetComponent<TextMeshProUGUI>();

        // Max Combo
        CreateLabel(parent, "MaxComboLabel", new Vector2(0, 40), "最大连击");
        GameObject comboContainer = new GameObject("ComboContainer");
        comboContainer.transform.SetParent(parent, false);
        RectTransform comboRect = comboContainer.AddComponent<RectTransform>();
        comboRect.anchoredPosition = new Vector2(0, 10);

        resultUI.maxComboText = CreateTextObject(comboContainer.transform, "MaxComboText", new Vector2(-30, 0), "0").GetComponent<TextMeshProUGUI>();
        CreateLabel(comboContainer.transform, "Slash", new Vector2(0, 0), "/");
        resultUI.totalNotesText = CreateTextObject(comboContainer.transform, "TotalNotesText", new Vector2(30, 0), "0").GetComponent<TextMeshProUGUI>();
    }

    private static void CreateJudgmentDisplay(Transform parent, ResultScreenUI resultUI)
    {
        float startY = -50;
        float spacing = 40;

        // Perfect
        CreateLabel(parent, "PerfectLabel", new Vector2(150, startY), "Perfect");
        resultUI.perfectCountText = CreateTextObject(parent, "PerfectCount", new Vector2(250, startY), "0").GetComponent<TextMeshProUGUI>();

        // Great
        CreateLabel(parent, "GreatLabel", new Vector2(150, startY - spacing), "Great");
        resultUI.greatCountText = CreateTextObject(parent, "GreatCount", new Vector2(250, startY - spacing), "0").GetComponent<TextMeshProUGUI>();

        // Good
        CreateLabel(parent, "GoodLabel", new Vector2(150, startY - spacing * 2), "Good");
        resultUI.goodCountText = CreateTextObject(parent, "GoodCount", new Vector2(250, startY - spacing * 2), "0").GetComponent<TextMeshProUGUI>();

        // Miss
        CreateLabel(parent, "MissLabel", new Vector2(150, startY - spacing * 3), "Miss");
        resultUI.missCountText = CreateTextObject(parent, "MissCount", new Vector2(250, startY - spacing * 3), "0").GetComponent<TextMeshProUGUI>();

        // Late/Fast
        CreateLabel(parent, "LateLabel", new Vector2(150, startY - spacing * 4), "Late");
        resultUI.lateCountText = CreateTextObject(parent, "LateCount", new Vector2(250, startY - spacing * 4), "0").GetComponent<TextMeshProUGUI>();

        CreateLabel(parent, "FastLabel", new Vector2(150, startY - spacing * 5), "Fast");
        resultUI.fastCountText = CreateTextObject(parent, "FastCount", new Vector2(250, startY - spacing * 5), "0").GetComponent<TextMeshProUGUI>();
    }

    private static void CreateSongInfo(Transform parent, ResultScreenUI resultUI)
    {
        // Song Name
        resultUI.songNameText = CreateTextObject(parent, "SongNameText", new Vector2(0, 250), "Song Name").GetComponent<TextMeshProUGUI>();
        resultUI.songNameText.fontSize = 32;

        // Difficulty
        resultUI.difficultyText = CreateTextObject(parent, "DifficultyText", new Vector2(0, 220), "Normal").GetComponent<TextMeshProUGUI>();
        resultUI.difficultyText.fontSize = 20;
    }

    private static GameObject CreateTextObject(Transform parent, string name, Vector2 position, string text)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 40);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return textObj;
    }

    private static void CreateLabel(Transform parent, string name, Vector2 position, string text)
    {
        GameObject labelObj = CreateTextObject(parent, name, position, text);
        TextMeshProUGUI tmp = labelObj.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 18;
        tmp.color = new Color(0.8f, 0.8f, 0.8f);
    }
}
