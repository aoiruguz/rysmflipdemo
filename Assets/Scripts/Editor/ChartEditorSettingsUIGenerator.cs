using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Chart Editor设置面板UI自动生成工具
/// </summary>
public class ChartEditorSettingsUIGenerator : EditorWindow
{
    [MenuItem("Tools/Chart Editor/Generate Settings UI")]
    public static void ShowWindow()
    {
        GetWindow<ChartEditorSettingsUIGenerator>("Settings UI Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Chart Editor Settings UI Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool will create a settings panel UI for Chart Editor.\n\n" +
            "Make sure you have a Canvas in the scene before clicking Generate.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Settings Panel", GUILayout.Height(40)))
        {
            GenerateSettingsUI();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Delete Settings Panel", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Confirm Delete",
                "Are you sure you want to delete the Settings Panel?",
                "Yes", "No"))
            {
                DeleteSettingsUI();
            }
        }
    }

    private void GenerateSettingsUI()
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

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        Transform canvasTransform = canvas.transform;

        // 删除已存在的设置面板
        Transform existing = canvasTransform.Find("ChartEditorSettingsPanel");
        if (existing != null)
        {
            DestroyImmediate(existing.gameObject);
        }

        // 创建设置面板
        GameObject settingsPanel = CreateSettingsPanel(canvasTransform);

        Debug.Log("[ChartEditorSettingsUIGenerator] Settings panel generated successfully!");
        EditorUtility.DisplayDialog("Success", "Chart Editor Settings Panel has been generated!", "OK");

        // 选中生成的面板
        Selection.activeGameObject = settingsPanel;
    }

    private void DeleteSettingsUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Transform existing = canvas.transform.Find("ChartEditorSettingsPanel");
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
                Debug.Log("[ChartEditorSettingsUIGenerator] Settings panel deleted!");
            }
        }
    }

    private GameObject CreateSettingsPanel(Transform parent)
    {
        // 创建主面板
        GameObject panel = CreatePanel("ChartEditorSettingsPanel", parent, new Vector2(500, 600), Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        panel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        panel.SetActive(false); // 默认隐藏

        // 添加ChartEditorSettingsUI组件
        ChartEditorSettingsUI settingsUI = panel.AddComponent<ChartEditorSettingsUI>();

        // 标题
        GameObject title = CreateText("Title", panel.transform, new Vector2(480, 40), new Vector2(0, -30));
        title.GetComponent<TextMeshProUGUI>().text = "Chart Editor Settings";
        title.GetComponent<TextMeshProUGUI>().fontSize = 28;
        title.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        title.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // 音乐音量
        CreateVolumeControl(panel.transform, "MusicVolume", "Music Volume", new Vector2(0, -100), out Slider musicSlider, out TextMeshProUGUI musicText);
        settingsUI.musicVolumeSlider = musicSlider;
        settingsUI.musicVolumeText = musicText;

        // 打击音效音量
        CreateVolumeControl(panel.transform, "HitSoundVolume", "Hit Sound Volume", new Vector2(0, -180), out Slider hitSoundSlider, out TextMeshProUGUI hitSoundText);
        settingsUI.hitSoundVolumeSlider = hitSoundSlider;
        settingsUI.hitSoundVolumeText = hitSoundText;

        // 偏移设置
        CreateOffsetControl(panel.transform, new Vector2(0, -260), out TMP_InputField offsetInput, out TextMeshProUGUI offsetText, out Button incBtn, out Button decBtn);
        settingsUI.offsetInputField = offsetInput;
        settingsUI.offsetText = offsetText;
        settingsUI.offsetIncreaseButton = incBtn;
        settingsUI.offsetDecreaseButton = decBtn;

        // 底部按钮
        GameObject resetBtn = CreateButton("ResetButton", panel.transform, new Vector2(200, 50), new Vector2(-110, -450));
        resetBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Reset to Defaults";
        settingsUI.resetButton = resetBtn.GetComponent<Button>();

        GameObject closeBtn = CreateButton("CloseButton", panel.transform, new Vector2(200, 50), new Vector2(110, -450));
        closeBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
        closeBtn.GetComponent<Image>().color = new Color(0.5f, 0.2f, 0.2f);
        settingsUI.closeButton = closeBtn.GetComponent<Button>();

        return panel;
    }

    private void CreateVolumeControl(Transform parent, string name, string label, Vector2 position, out Slider slider, out TextMeshProUGUI valueText)
    {
        GameObject container = new GameObject(name + "Container");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(460, 60);

        // 标签
        GameObject labelObj = CreateText("Label", container.transform, new Vector2(200, 30), new Vector2(-130, -10));
        labelObj.GetComponent<TextMeshProUGUI>().text = label;
        labelObj.GetComponent<TextMeshProUGUI>().fontSize = 18;
        labelObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // 数值显示
        GameObject valueObj = CreateText("Value", container.transform, new Vector2(80, 30), new Vector2(190, -10));
        valueObj.GetComponent<TextMeshProUGUI>().text = "100%";
        valueObj.GetComponent<TextMeshProUGUI>().fontSize = 18;
        valueObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        valueText = valueObj.GetComponent<TextMeshProUGUI>();

        // 滑块
        GameObject sliderObj = CreateSlider(name + "Slider", container.transform, new Vector2(460, 30), new Vector2(0, -40));
        slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;
    }

    private void CreateOffsetControl(Transform parent, Vector2 position, out TMP_InputField inputField, out TextMeshProUGUI valueText, out Button increaseBtn, out Button decreaseBtn)
    {
        GameObject container = new GameObject("OffsetContainer");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(460, 100);

        // 标签
        GameObject labelObj = CreateText("Label", container.transform, new Vector2(200, 30), new Vector2(-130, -10));
        labelObj.GetComponent<TextMeshProUGUI>().text = "Offset (seconds)";
        labelObj.GetComponent<TextMeshProUGUI>().fontSize = 18;
        labelObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

        // 数值显示
        GameObject valueObj = CreateText("Value", container.transform, new Vector2(100, 30), new Vector2(180, -10));
        valueObj.GetComponent<TextMeshProUGUI>().text = "0.000s";
        valueObj.GetComponent<TextMeshProUGUI>().fontSize = 18;
        valueObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Right;
        valueText = valueObj.GetComponent<TextMeshProUGUI>();

        // 输入框
        GameObject inputObj = CreateInputField("OffsetInput", container.transform, new Vector2(200, 40), new Vector2(-80, -60));
        inputField = inputObj.GetComponent<TMP_InputField>();
        inputField.text = "0.000";

        // 增加按钮
        GameObject incBtnObj = CreateButton("IncreaseButton", container.transform, new Vector2(80, 40), new Vector2(60, -60));
        incBtnObj.GetComponentInChildren<TextMeshProUGUI>().text = "+0.01";
        incBtnObj.GetComponentInChildren<TextMeshProUGUI>().fontSize = 16;
        increaseBtn = incBtnObj.GetComponent<Button>();

        // 减少按钮
        GameObject decBtnObj = CreateButton("DecreaseButton", container.transform, new Vector2(80, 40), new Vector2(150, -60));
        decBtnObj.GetComponentInChildren<TextMeshProUGUI>().text = "-0.01";
        decBtnObj.GetComponentInChildren<TextMeshProUGUI>().fontSize = 16;
        decreaseBtn = decBtnObj.GetComponent<Button>();
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
        textComponent.fontSize = 16;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;

        inputField.textViewport = textAreaRect;
        inputField.textComponent = textComponent;

        return inputObj;
    }
}
