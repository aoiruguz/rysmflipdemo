using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Chart Editor UI自动生成工具
/// 一键创建制谱器所需的所有UI元素
/// </summary>
public class ChartEditorUIGenerator : EditorWindow
{
    [MenuItem("Tools/Chart Editor/Generate UI")]
    public static void ShowWindow()
    {
        GetWindow<ChartEditorUIGenerator>("Chart Editor UI Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Chart Editor UI Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool will automatically create all UI elements needed for the Chart Editor.\n\n" +
            "Make sure you have a Canvas in the scene before clicking Generate.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate All UI Elements", GUILayout.Height(40)))
        {
            GenerateUI();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Delete All Generated UI", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Delete",
                "Are you sure you want to delete all Chart Editor UI elements?",
                "Yes", "No"))
            {
                DeleteGeneratedUI();
            }
        }
    }

    private void GenerateUI()
    {
        // 查找或创建Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // 设置CanvasScaler
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        Transform canvasTransform = canvas.transform;

        // 删除已存在的UI（如果有）
        Transform existing = canvasTransform.Find("ChartEditorUI");
        if (existing != null)
        {
            DestroyImmediate(existing.gameObject);
        }

        // 创建根容器
        GameObject rootUI = new GameObject("ChartEditorUI");
        rootUI.transform.SetParent(canvasTransform, false);
        RectTransform rootRect = rootUI.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.sizeDelta = Vector2.zero;

        // 创建各个面板
        CreatePlaybackPanel(rootUI.transform);
        CreateTimePanel(rootUI.transform);
        CreateSelectedNotePanel(rootUI.transform);
        CreateEditPanel(rootUI.transform);
        CreateSavePanel(rootUI.transform);
        CreateInfoPanel(rootUI.transform);

        Debug.Log("[ChartEditorUIGenerator] UI generated successfully!");
        EditorUtility.DisplayDialog("Success", "Chart Editor UI has been generated!", "OK");
    }

    private void DeleteGeneratedUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Transform existing = canvas.transform.Find("ChartEditorUI");
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
                Debug.Log("[ChartEditorUIGenerator] UI deleted successfully!");
            }
        }
    }

    // ========== 创建各个面板 ==========

    private void CreatePlaybackPanel(Transform parent)
    {
        GameObject panel = CreatePanel("PlaybackPanel", parent, new Vector2(300, 60), new Vector2(10, -10), new Vector2(0, 1), new Vector2(0, 1));

        // Play/Pause Button
        GameObject playPauseBtn = CreateButton("PlayPauseButton", panel.transform, new Vector2(120, 50), new Vector2(10, -5));
        playPauseBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Play";

        // Stop Button
        GameObject stopBtn = CreateButton("StopButton", panel.transform, new Vector2(120, 50), new Vector2(140, -5));
        stopBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Stop";
    }

    private void CreateTimePanel(Transform parent)
    {
        GameObject panel = CreatePanel("TimePanel", parent, new Vector2(600, 80), new Vector2(0, -80), new Vector2(0.5f, 1), new Vector2(0.5f, 1));

        // Current Time Text
        GameObject currentTimeText = CreateText("CurrentTimeText", panel.transform, new Vector2(150, 30), new Vector2(-220, -10));
        currentTimeText.GetComponent<TextMeshProUGUI>().text = "00:00.000";
        currentTimeText.GetComponent<TextMeshProUGUI>().fontSize = 24;

        // Timeline Slider
        GameObject slider = CreateSlider("TimelineSlider", panel.transform, new Vector2(400, 30), new Vector2(0, -10));

        // Total Time Text
        GameObject totalTimeText = CreateText("TotalTimeText", panel.transform, new Vector2(100, 30), new Vector2(220, -10));
        totalTimeText.GetComponent<TextMeshProUGUI>().text = "00:00";
        totalTimeText.GetComponent<TextMeshProUGUI>().fontSize = 20;
    }

    private void CreateSelectedNotePanel(Transform parent)
    {
        GameObject panel = CreatePanel("SelectedNotePanel", parent, new Vector2(300, 120), new Vector2(-10, -10), new Vector2(1, 1), new Vector2(1, 1));
        panel.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Title
        GameObject title = CreateText("Title", panel.transform, new Vector2(280, 30), new Vector2(0, -15));
        title.GetComponent<TextMeshProUGUI>().text = "Selected Note";
        title.GetComponent<TextMeshProUGUI>().fontSize = 18;
        title.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Note Time Text
        GameObject noteTimeText = CreateText("NoteTimeText", panel.transform, new Vector2(280, 25), new Vector2(0, -45));
        noteTimeText.GetComponent<TextMeshProUGUI>().text = "Time: 0.000s";
        noteTimeText.GetComponent<TextMeshProUGUI>().fontSize = 16;

        // Note Lane Text
        GameObject noteLaneText = CreateText("NoteLaneText", panel.transform, new Vector2(280, 25), new Vector2(0, -70));
        noteLaneText.GetComponent<TextMeshProUGUI>().text = "Lane: 0";
        noteLaneText.GetComponent<TextMeshProUGUI>().fontSize = 16;

        // Note Type Text
        GameObject noteTypeText = CreateText("NoteTypeText", panel.transform, new Vector2(280, 25), new Vector2(0, -95));
        noteTypeText.GetComponent<TextMeshProUGUI>().text = "Type: Color - ColorA";
        noteTypeText.GetComponent<TextMeshProUGUI>().fontSize = 16;
    }

    private void CreateEditPanel(Transform parent)
    {
        GameObject panel = CreatePanel("EditPanel", parent, new Vector2(300, 200), new Vector2(-10, -140), new Vector2(1, 1), new Vector2(1, 1));

        // Delete Note Button
        GameObject deleteBtn = CreateButton("DeleteNoteButton", panel.transform, new Vector2(280, 40), new Vector2(0, -10));
        deleteBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Delete Note";

        // Add Note Button
        GameObject addBtn = CreateButton("AddNoteButton", panel.transform, new Vector2(280, 40), new Vector2(0, -60));
        addBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Add Note";

        // Snap Toggle
        GameObject snapToggle = CreateToggle("SnapToggle", panel.transform, new Vector2(100, 30), new Vector2(-90, -110));
        snapToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Snap";

        // Snap Interval Input
        GameObject snapInput = CreateInputField("SnapIntervalInput", panel.transform, new Vector2(150, 30), new Vector2(75, -110));
        snapInput.GetComponent<TMP_InputField>().text = "0.25";
    }

    private void CreateSavePanel(Transform parent)
    {
        GameObject panel = CreatePanel("SavePanel", parent, new Vector2(300, 100), new Vector2(-10, -350), new Vector2(1, 1), new Vector2(1, 1));

        // Save Button
        GameObject saveBtn = CreateButton("SaveButton", panel.transform, new Vector2(280, 50), new Vector2(0, -10));
        saveBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Save Chart";
        saveBtn.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);

        // Save Confirmation Panel
        GameObject confirmPanel = CreatePanel("SaveConfirmationPanel", panel.transform, new Vector2(280, 40), new Vector2(0, -70), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        confirmPanel.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 0.9f);
        confirmPanel.SetActive(false);

        GameObject confirmText = CreateText("Text", confirmPanel.transform, new Vector2(260, 30), new Vector2(0, 0));
        confirmText.GetComponent<TextMeshProUGUI>().text = "Chart Saved!";
        confirmText.GetComponent<TextMeshProUGUI>().fontSize = 18;
        confirmText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
    }

    private void CreateInfoPanel(Transform parent)
    {
        GameObject panel = CreatePanel("InfoPanel", parent, new Vector2(400, 80), new Vector2(10, -80), new Vector2(0, 1), new Vector2(0, 1));

        // Chart Name Text
        GameObject chartNameText = CreateText("ChartNameText", panel.transform, new Vector2(380, 30), new Vector2(0, -10));
        chartNameText.GetComponent<TextMeshProUGUI>().text = "Song Name - Difficulty";
        chartNameText.GetComponent<TextMeshProUGUI>().fontSize = 20;
        chartNameText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        // Total Notes Text
        GameObject totalNotesText = CreateText("TotalNotesText", panel.transform, new Vector2(380, 25), new Vector2(0, -45));
        totalNotesText.GetComponent<TextMeshProUGUI>().text = "Total Notes: 0";
        totalNotesText.GetComponent<TextMeshProUGUI>().fontSize = 18;
    }

    // ========== UI创建辅助方法 ==========

    private GameObject CreatePanel(string name, Transform parent, Vector2 size, Vector2 position, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);

        return panel;
    }

    private GameObject CreateButton(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent, false);

        RectTransform rect = button.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = button.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f);

        Button btn = button.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
        btn.colors = colors;

        // Add Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Button";
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        return button;
    }

    private GameObject CreateText(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Text";
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        return textObj;
    }

    private GameObject CreateSlider(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject sliderObj = new GameObject(name);
        sliderObj.transform.SetParent(parent, false);

        RectTransform rect = sliderObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Slider slider = sliderObj.AddComponent<Slider>();

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-10, 0);

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.6f, 1f);

        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-10, 0);

        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;

        return sliderObj;
    }

    private GameObject CreateToggle(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent, false);

        RectTransform rect = toggleObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Toggle toggle = toggleObj.AddComponent<Toggle>();

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(toggleObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(0, 0.5f);
        bgRect.sizeDelta = new Vector2(20, 20);
        bgRect.anchoredPosition = new Vector2(10, 0);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f);

        // Checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(bg.transform, false);
        RectTransform checkRect = checkmark.AddComponent<RectTransform>();
        checkRect.anchorMin = Vector2.zero;
        checkRect.anchorMax = Vector2.one;
        checkRect.sizeDelta = Vector2.zero;
        Image checkImage = checkmark.AddComponent<Image>();
        checkImage.color = new Color(0.3f, 0.8f, 0.3f);

        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(toggleObj.transform, false);
        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(30, 0);
        labelRect.offsetMax = new Vector2(0, 0);
        TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
        labelText.text = "Toggle";
        labelText.fontSize = 14;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.color = Color.white;

        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;

        return toggleObj;
    }

    private GameObject CreateInputField(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject inputObj = new GameObject(name);
        inputObj.transform.SetParent(parent, false);

        RectTransform rect = inputObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = inputObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f);

        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();

        // Text Area
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.sizeDelta = Vector2.zero;
        textAreaRect.offsetMin = new Vector2(5, 2);
        textAreaRect.offsetMax = new Vector2(-5, -2);

        // Text
        GameObject text = new GameObject("Text");
        text.transform.SetParent(textArea.transform, false);
        RectTransform textRect = text.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        TextMeshProUGUI textComponent = text.AddComponent<TextMeshProUGUI>();
        textComponent.text = "";
        textComponent.fontSize = 14;
        textComponent.color = Color.white;

        inputField.textViewport = textAreaRect;
        inputField.textComponent = textComponent;

        return inputObj;
    }
}
