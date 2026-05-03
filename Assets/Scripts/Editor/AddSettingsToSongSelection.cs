using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class AddSettingsToSongSelection : EditorWindow
{
    [MenuItem("Tools/Add Settings UI to Song Selection")]
    public static void AddSettingsUI()
    {
        // Check if SongSelection scene is open
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "SongSelection")
        {
            if (EditorUtility.DisplayDialog("Wrong Scene",
                "Please open the SongSelection scene first.", "OK"))
            {
                return;
            }
        }

        // Find the Canvas in the scene
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No Canvas found in the scene!", "OK");
            return;
        }

        // Create Settings Button in top-right corner
        GameObject settingsButtonObj = new GameObject("SettingsButton");
        Undo.RegisterCreatedObjectUndo(settingsButtonObj, "Create Settings Button");
        settingsButtonObj.transform.SetParent(canvas.transform, false);

        RectTransform settingsBtnRect = settingsButtonObj.AddComponent<RectTransform>();
        settingsBtnRect.anchorMin = new Vector2(1, 1);
        settingsBtnRect.anchorMax = new Vector2(1, 1);
        settingsBtnRect.pivot = new Vector2(1, 1);
        settingsBtnRect.anchoredPosition = new Vector2(-20, -20);
        settingsBtnRect.sizeDelta = new Vector2(100, 50);

        Button settingsButton = settingsButtonObj.AddComponent<Button>();
        Image settingsBtnImage = settingsButtonObj.AddComponent<Image>();
        settingsBtnImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Add text to settings button
        GameObject settingsBtnTextObj = new GameObject("Text");
        settingsBtnTextObj.transform.SetParent(settingsButtonObj.transform, false);

        TextMeshProUGUI settingsBtnText = settingsBtnTextObj.AddComponent<TextMeshProUGUI>();
        settingsBtnText.text = "设置";
        settingsBtnText.fontSize = 24;
        settingsBtnText.alignment = TextAlignmentOptions.Center;
        settingsBtnText.color = Color.white;

        RectTransform textRect = settingsBtnTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // Create Settings Panel (hidden by default)
        GameObject settingsPanel = new GameObject("SettingsPanel");
        Undo.RegisterCreatedObjectUndo(settingsPanel, "Create Settings Panel");
        settingsPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = settingsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = settingsPanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.8f);

        // Add the settings UI script
        SongSelectionSettingsUI settingsUI = settingsPanel.AddComponent<SongSelectionSettingsUI>();

        // Create content container
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(settingsPanel.transform, false);

        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(600, 400);

        Image contentBg = contentPanel.AddComponent<Image>();
        contentBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Create Title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(contentPanel.transform, false);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "设置";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -20);
        titleRect.sizeDelta = new Vector2(0, 50);

        // Create Music Volume Slider
        CreateSlider(contentPanel, "MusicVolumeSlider", "音乐音量", new Vector2(0, -100), out Slider musicSlider, out Text musicLabel);

        // Create Note Volume Slider
        CreateSlider(contentPanel, "NoteVolumeSlider", "音效音量", new Vector2(0, -180), out Slider noteSlider, out Text noteLabel);

        // Create Mode Button
        GameObject modeButtonObj = new GameObject("ModeButton");
        modeButtonObj.transform.SetParent(contentPanel.transform, false);

        RectTransform modeBtnRect = modeButtonObj.AddComponent<RectTransform>();
        modeBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
        modeBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
        modeBtnRect.pivot = new Vector2(0.5f, 0.5f);
        modeBtnRect.anchoredPosition = new Vector2(0, -50);
        modeBtnRect.sizeDelta = new Vector2(400, 50);

        Button modeButton = modeButtonObj.AddComponent<Button>();
        Image modeBtnImage = modeButtonObj.AddComponent<Image>();
        modeBtnImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        GameObject modeTextObj = new GameObject("Text");
        modeTextObj.transform.SetParent(modeButtonObj.transform, false);

        Text modeText = modeTextObj.AddComponent<Text>();
        modeText.text = "Mode: Strict (Accurate)";
        modeText.fontSize = 20;
        modeText.alignment = TextAnchor.MiddleCenter;
        modeText.color = Color.white;
        modeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform modeTextRect = modeTextObj.GetComponent<RectTransform>();
        modeTextRect.anchorMin = Vector2.zero;
        modeTextRect.anchorMax = Vector2.one;
        modeTextRect.offsetMin = Vector2.zero;
        modeTextRect.offsetMax = Vector2.zero;

        // Create Close Button
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(contentPanel.transform, false);

        RectTransform closeBtnRect = closeButtonObj.AddComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(0.5f, 0);
        closeBtnRect.anchorMax = new Vector2(0.5f, 0);
        closeBtnRect.pivot = new Vector2(0.5f, 0);
        closeBtnRect.anchoredPosition = new Vector2(0, 20);
        closeBtnRect.sizeDelta = new Vector2(200, 50);

        Button closeButton = closeButtonObj.AddComponent<Button>();
        Image closeBtnImage = closeButtonObj.AddComponent<Image>();
        closeBtnImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeButtonObj.transform, false);

        TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeText.text = "关闭";
        closeText.fontSize = 24;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;

        RectTransform closeTextRect = closeTextObj.GetComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;

        // Wire up the settings UI component
        settingsUI.musicVolumeSlider = musicSlider;
        settingsUI.noteVolumeSlider = noteSlider;
        settingsUI.modeButton = modeButton;
        settingsUI.modeText = modeText;
        settingsUI.closeButton = closeButton;

        // Wire up settings button to open panel
        settingsButton.onClick.AddListener(() => settingsUI.Open());

        // Hide settings panel initially
        settingsPanel.SetActive(false);

        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(scene);

        EditorUtility.DisplayDialog("Success",
            "Settings UI has been added to the SongSelection scene!\n\n" +
            "- Settings button is in the top-right corner\n" +
            "- All settings are shared with PlayScene via GameSettings\n" +
            "- Settings are automatically saved using PlayerPrefs", "OK");
    }

    private static void CreateSlider(GameObject parent, string name, string labelText, Vector2 position, out Slider slider, out Text label)
    {
        // Create container
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent.transform, false);

        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 1);
        containerRect.anchorMax = new Vector2(0.5f, 1);
        containerRect.pivot = new Vector2(0.5f, 1);
        containerRect.anchoredPosition = position;
        containerRect.sizeDelta = new Vector2(500, 60);

        // Create label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(container.transform, false);

        label = labelObj.AddComponent<Text>();
        label.text = labelText;
        label.fontSize = 20;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Color.white;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.pivot = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(0, 0);
        labelRect.sizeDelta = new Vector2(150, 30);

        // Create slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(container.transform, false);

        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0.5f);
        sliderRect.anchorMax = new Vector2(1, 0.5f);
        sliderRect.pivot = new Vector2(0, 0.5f);
        sliderRect.anchoredPosition = new Vector2(160, 0);
        sliderRect.sizeDelta = new Vector2(-160, 30);

        slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);

        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        // Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.3f, 0.6f, 1f, 1f);

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);

        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = Vector2.zero;
        handleAreaRect.offsetMax = Vector2.zero;

        // Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;

        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 30);

        // Wire up slider
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
    }
}
