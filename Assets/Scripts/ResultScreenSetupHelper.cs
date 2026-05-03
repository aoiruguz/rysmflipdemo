using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 简单的结算界面设置助手
/// 在Game Over Scene中手动运行此脚本来创建UI
/// </summary>
public class ResultScreenSetupHelper : MonoBehaviour
{
    [ContextMenu("Setup Result Screen UI")]
    public void SetupUI()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("Created Canvas");
        }

        // Create main result panel
        GameObject resultPanel = new GameObject("ResultPanel");
        resultPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = resultPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(1200, 800);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = resultPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        // Add ResultScreenUI component
        ResultScreenUI resultUI = resultPanel.AddComponent<ResultScreenUI>();

        // Create UI structure
        CreateSongInfoSection(resultPanel.transform, resultUI);
        CreateRankSection(resultPanel.transform, resultUI);
        CreateScoreSection(resultPanel.transform, resultUI);
        CreateJudgmentSection(resultPanel.transform, resultUI);
        CreateTimingSection(resultPanel.transform, resultUI);
        CreateNavigationButtons(resultPanel.transform);

        Debug.Log("Result Screen UI setup complete!");
    }

    private void CreateSongInfoSection(Transform parent, ResultScreenUI resultUI)
    {
        // Song Name
        GameObject songNameObj = CreateText(parent, "SongName", new Vector2(0, 320), new Vector2(800, 60));
        TextMeshProUGUI songText = songNameObj.GetComponent<TextMeshProUGUI>();
        songText.fontSize = 48;
        songText.fontStyle = FontStyles.Bold;
        songText.text = "Song Name";
        resultUI.songNameText = songText;

        // Difficulty
        GameObject diffObj = CreateText(parent, "Difficulty", new Vector2(0, 270), new Vector2(400, 40));
        TextMeshProUGUI diffText = diffObj.GetComponent<TextMeshProUGUI>();
        diffText.fontSize = 28;
        diffText.text = "Normal";
        diffText.color = new Color(0.8f, 0.8f, 1f);
        resultUI.difficultyText = diffText;
    }

    private void CreateRankSection(Transform parent, ResultScreenUI resultUI)
    {
        // Rank Icon (left side)
        GameObject rankObj = new GameObject("RankIcon");
        rankObj.transform.SetParent(parent, false);

        RectTransform rankRect = rankObj.AddComponent<RectTransform>();
        rankRect.anchorMin = new Vector2(0.5f, 0.5f);
        rankRect.anchorMax = new Vector2(0.5f, 0.5f);
        rankRect.anchoredPosition = new Vector2(-400, 50);
        rankRect.sizeDelta = new Vector2(200, 200);

        Image rankImage = rankObj.AddComponent<Image>();
        rankImage.color = Color.white;
        resultUI.rankIcon = rankImage;

        // Full Combo Icon
        GameObject fcObj = CreateText(parent, "FullComboIcon", new Vector2(-400, -100), new Vector2(250, 50));
        TextMeshProUGUI fcText = fcObj.GetComponent<TextMeshProUGUI>();
        fcText.fontSize = 32;
        fcText.fontStyle = FontStyles.Bold;
        fcText.text = "FULL COMBO";
        fcText.color = Color.yellow;
        fcObj.SetActive(false);
        resultUI.fullComboIcon = fcObj;

        // All Perfect Icon
        GameObject apObj = CreateText(parent, "AllPerfectIcon", new Vector2(-400, -160), new Vector2(250, 50));
        TextMeshProUGUI apText = apObj.GetComponent<TextMeshProUGUI>();
        apText.fontSize = 32;
        apText.fontStyle = FontStyles.Bold;
        apText.text = "ALL PERFECT";
        apText.color = new Color(1f, 0.5f, 1f);
        apObj.SetActive(false);
        resultUI.allPerfectIcon = apObj;
    }

    private void CreateScoreSection(Transform parent, ResultScreenUI resultUI)
    {
        float centerX = 100;

        // Achievement Rate Label
        CreateLabel(parent, "AchievementRateLabel", new Vector2(centerX, 180), "达成率");

        // Achievement Rate Value
        GameObject rateObj = CreateText(parent, "AchievementRate", new Vector2(centerX, 140), new Vector2(300, 60));
        TextMeshProUGUI rateText = rateObj.GetComponent<TextMeshProUGUI>();
        rateText.fontSize = 48;
        rateText.fontStyle = FontStyles.Bold;
        rateText.text = "0.00%";
        rateText.color = Color.cyan;
        resultUI.achievementRateText = rateText;

        // Achievement Score Label
        CreateLabel(parent, "ScoreLabel", new Vector2(centerX, 80), "分数");

        // Achievement Score Value
        GameObject scoreObj = CreateText(parent, "AchievementScore", new Vector2(centerX, 40), new Vector2(400, 50));
        TextMeshProUGUI scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        scoreText.fontSize = 40;
        scoreText.text = "0000000";
        resultUI.achievementScoreText = scoreText;

        // Max Combo Label
        CreateLabel(parent, "ComboLabel", new Vector2(centerX, -20), "最大连击");

        // Combo Display (MaxCombo / TotalNotes)
        GameObject comboObj = CreateText(parent, "MaxCombo", new Vector2(centerX - 60, -60), new Vector2(150, 50));
        TextMeshProUGUI comboText = comboObj.GetComponent<TextMeshProUGUI>();
        comboText.fontSize = 36;
        comboText.text = "0";
        comboText.alignment = TextAlignmentOptions.Right;
        resultUI.maxComboText = comboText;

        GameObject slashObj = CreateText(parent, "Slash", new Vector2(centerX, -60), new Vector2(40, 50));
        TextMeshProUGUI slashText = slashObj.GetComponent<TextMeshProUGUI>();
        slashText.fontSize = 36;
        slashText.text = "/";

        GameObject totalObj = CreateText(parent, "TotalNotes", new Vector2(centerX + 60, -60), new Vector2(150, 50));
        TextMeshProUGUI totalText = totalObj.GetComponent<TextMeshProUGUI>();
        totalText.fontSize = 36;
        totalText.text = "0";
        totalText.alignment = TextAlignmentOptions.Left;
        resultUI.totalNotesText = totalText;
    }

    private void CreateJudgmentSection(Transform parent, ResultScreenUI resultUI)
    {
        float startX = 100;
        float startY = -140;
        float spacing = 45;

        // Perfect
        CreateJudgmentRow(parent, "Perfect", new Vector2(startX, startY), Color.yellow, out TextMeshProUGUI perfectText);
        resultUI.perfectCountText = perfectText;

        // Great
        CreateJudgmentRow(parent, "Great", new Vector2(startX, startY - spacing), new Color(0.5f, 1f, 0.5f), out TextMeshProUGUI greatText);
        resultUI.greatCountText = greatText;

        // Good
        CreateJudgmentRow(parent, "Good", new Vector2(startX, startY - spacing * 2), new Color(0.5f, 0.8f, 1f), out TextMeshProUGUI goodText);
        resultUI.goodCountText = goodText;

        // Miss
        CreateJudgmentRow(parent, "Miss", new Vector2(startX, startY - spacing * 3), new Color(1f, 0.3f, 0.3f), out TextMeshProUGUI missText);
        resultUI.missCountText = missText;
    }

    private void CreateTimingSection(Transform parent, ResultScreenUI resultUI)
    {
        float startX = 350;
        float startY = -140;

        // Late
        CreateLabel(parent, "LateLabel", new Vector2(startX, startY), "Late");
        GameObject lateObj = CreateText(parent, "LateCount", new Vector2(startX + 100, startY), new Vector2(100, 40));
        TextMeshProUGUI lateText = lateObj.GetComponent<TextMeshProUGUI>();
        lateText.fontSize = 28;
        lateText.text = "0";
        lateText.alignment = TextAlignmentOptions.Right;
        resultUI.lateCountText = lateText;

        // Fast
        CreateLabel(parent, "FastLabel", new Vector2(startX, startY - 45), "Fast");
        GameObject fastObj = CreateText(parent, "FastCount", new Vector2(startX + 100, startY - 45), new Vector2(100, 40));
        TextMeshProUGUI fastText = fastObj.GetComponent<TextMeshProUGUI>();
        fastText.fontSize = 28;
        fastText.text = "0";
        fastText.alignment = TextAlignmentOptions.Right;
        resultUI.fastCountText = fastText;
    }

    private void CreateJudgmentRow(Transform parent, string judgmentName, Vector2 position, Color color, out TextMeshProUGUI countText)
    {
        // Label
        GameObject labelObj = CreateText(parent, judgmentName + "Label", position, new Vector2(150, 40));
        TextMeshProUGUI labelTmp = labelObj.GetComponent<TextMeshProUGUI>();
        labelTmp.fontSize = 24;
        labelTmp.text = judgmentName;
        labelTmp.color = color;
        labelTmp.alignment = TextAlignmentOptions.Left;

        // Count
        GameObject countObj = CreateText(parent, judgmentName + "Count", new Vector2(position.x + 150, position.y), new Vector2(100, 40));
        countText = countObj.GetComponent<TextMeshProUGUI>();
        countText.fontSize = 28;
        countText.text = "0";
        countText.alignment = TextAlignmentOptions.Right;
    }

    private GameObject CreateText(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return textObj;
    }

    private void CreateLabel(Transform parent, string name, Vector2 position, string text)
    {
        GameObject labelObj = CreateText(parent, name, position, new Vector2(200, 30));
        TextMeshProUGUI tmp = labelObj.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 18;
        tmp.text = text;
        tmp.color = new Color(0.7f, 0.7f, 0.7f);
    }

    private void CreateNavigationButtons(Transform parent)
    {
        // Create navigation container
        GameObject navObj = new GameObject("Navigation");
        navObj.transform.SetParent(parent, false);

        RectTransform navRect = navObj.AddComponent<RectTransform>();
        navRect.anchorMin = new Vector2(0.5f, 0.5f);
        navRect.anchorMax = new Vector2(0.5f, 0.5f);
        navRect.anchoredPosition = new Vector2(0, -350);
        navRect.sizeDelta = new Vector2(600, 80);

        ResultScreenNavigation navigation = navObj.AddComponent<ResultScreenNavigation>();

        // Retry Button
        GameObject retryBtn = CreateButton(navObj.transform, "RetryButton", new Vector2(-160, 0), "重试 (R)");
        navigation.retryButton = retryBtn.GetComponent<Button>();

        // Back to Menu Button
        GameObject backBtn = CreateButton(navObj.transform, "BackButton", new Vector2(160, 0), "返回菜单 (ESC)");
        navigation.backToMenuButton = backBtn.GetComponent<Button>();
    }

    private GameObject CreateButton(Transform parent, string name, Vector2 position, string text)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(280, 70);

        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.4f, 0.6f);

        Button button = btnObj.AddComponent<Button>();

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return btnObj;
    }
}
