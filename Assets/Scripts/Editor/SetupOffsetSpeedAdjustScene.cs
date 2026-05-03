using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupOffsetSpeedAdjustScene : EditorWindow
{
    [MenuItem("Tools/Setup Offset Speed Adjust Scene")]
    public static void Setup()
    {
        // Open the scene
        string scenePath = "Assets/Scenes/Offest Speed Adjust.unity";
        EditorSceneManager.OpenScene(scenePath);

        // Find or create Canvas
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

        // Find or create EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");
        }

        // Setup Camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Undo.RecordObject(mainCamera.transform, "Setup Camera");
            mainCamera.transform.position = new Vector3(0, 0, -10);
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 8;
            mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        }

        // Find or create Manager
        OffsetSpeedAdjustManager manager = FindObjectOfType<OffsetSpeedAdjustManager>();
        GameObject managerObj;
        if (manager == null)
        {
            managerObj = new GameObject("OffsetSpeedAdjustManager");
            manager = managerObj.AddComponent<OffsetSpeedAdjustManager>();
            Undo.RegisterCreatedObjectUndo(managerObj, "Create Manager");
        }
        else
        {
            managerObj = manager.gameObject;
        }

        // Create Left Panel
        GameObject leftPanel = CreateLeftPanel(canvas.transform);

        // Create UI Controls
        var offsetControls = CreateOffsetControls(leftPanel.transform);
        var travelTimeControls = CreateTravelTimeControls(leftPanel.transform);

        // Create Visualization Elements
        CreateVisualizationArea();
        CreateJudgmentLine();

        // Wire up manager references
        Undo.RecordObject(manager, "Setup Manager References");
        manager.offsetValueText = offsetControls.valueText;
        manager.offsetIncreaseButton = offsetControls.increaseButton;
        manager.offsetDecreaseButton = offsetControls.decreaseButton;
        manager.travelTimeValueText = travelTimeControls.valueText;
        manager.travelTimeIncreaseButton = travelTimeControls.increaseButton;
        manager.travelTimeDecreaseButton = travelTimeControls.decreaseButton;

        // Set default values
        manager.spawnY = 6f;
        manager.judgmentLineY = -4f;
        manager.laneX = 0f;
        manager.noteTravelTime = 2.0f;
        manager.spawnInterval = 2f;
        manager.offsetStep = 0.01f;
        manager.travelTimeStep = 0.1f;

        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Offset Speed Adjust scene setup complete!");
    }

    private static GameObject CreateLeftPanel(Transform canvasTransform)
    {
        GameObject leftPanel = new GameObject("LeftPanel");
        leftPanel.transform.SetParent(canvasTransform, false);
        RectTransform leftRect = leftPanel.AddComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0, 0);
        leftRect.anchorMax = new Vector2(0.35f, 1);
        leftRect.offsetMin = Vector2.zero;
        leftRect.offsetMax = Vector2.zero;
        Image leftBg = leftPanel.AddComponent<Image>();
        leftBg.color = new Color(0.2f, 0.2f, 0.25f, 0.95f);
        Undo.RegisterCreatedObjectUndo(leftPanel, "Create Left Panel");
        return leftPanel;
    }

    private static (Text valueText, Button increaseButton, Button decreaseButton) CreateOffsetControls(Transform parent)
    {
        GameObject offsetGroup = new GameObject("OffsetGroup");
        offsetGroup.transform.SetParent(parent, false);
        RectTransform groupRect = offsetGroup.AddComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0.1f, 0.65f);
        groupRect.anchorMax = new Vector2(0.9f, 0.85f);
        groupRect.offsetMin = Vector2.zero;
        groupRect.offsetMax = Vector2.zero;
        Undo.RegisterCreatedObjectUndo(offsetGroup, "Create Offset Group");

        CreateLabel(offsetGroup.transform, "OffsetTitle", "全局偏移 (Global Offset)",
            new Vector2(0, 0.7f), new Vector2(1, 1), 24, Color.white);

        Text valueText = CreateLabel(offsetGroup.transform, "OffsetValue", "0ms",
            new Vector2(0, 0.35f), new Vector2(1, 0.65f), 32, Color.yellow);

        Button decreaseBtn = CreateButton(offsetGroup.transform, "OffsetDecreaseButton", "-",
            new Vector2(0.1f, 0), new Vector2(0.45f, 0.3f));
        Button increaseBtn = CreateButton(offsetGroup.transform, "OffsetIncreaseButton", "+",
            new Vector2(0.55f, 0), new Vector2(0.9f, 0.3f));

        return (valueText, increaseBtn, decreaseBtn);
    }

    private static (Text valueText, Button increaseButton, Button decreaseButton) CreateTravelTimeControls(Transform parent)
    {
        GameObject travelGroup = new GameObject("TravelTimeGroup");
        travelGroup.transform.SetParent(parent, false);
        RectTransform groupRect = travelGroup.AddComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(0.1f, 0.35f);
        groupRect.anchorMax = new Vector2(0.9f, 0.55f);
        groupRect.offsetMin = Vector2.zero;
        groupRect.offsetMax = Vector2.zero;
        Undo.RegisterCreatedObjectUndo(travelGroup, "Create TravelTime Group");

        CreateLabel(travelGroup.transform, "TravelTimeTitle", "流速 (Note Travel Time)",
            new Vector2(0, 0.7f), new Vector2(1, 1), 24, Color.white);

        Text valueText = CreateLabel(travelGroup.transform, "TravelTimeValue", "2.00s",
            new Vector2(0, 0.35f), new Vector2(1, 0.65f), 32, Color.cyan);

        Button decreaseBtn = CreateButton(travelGroup.transform, "TravelTimeDecreaseButton", "-",
            new Vector2(0.1f, 0), new Vector2(0.45f, 0.3f));
        Button increaseBtn = CreateButton(travelGroup.transform, "TravelTimeIncreaseButton", "+",
            new Vector2(0.55f, 0), new Vector2(0.9f, 0.3f));

        return (valueText, increaseBtn, decreaseBtn);
    }

    private static Text CreateLabel(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color)
    {
        GameObject labelObj = new GameObject(name);
        labelObj.transform.SetParent(parent, false);
        RectTransform rect = labelObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text textComp = labelObj.AddComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = fontSize;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = color;

        Undo.RegisterCreatedObjectUndo(labelObj, $"Create {name}");
        return textComp;
    }

    private static Button CreateButton(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = buttonObj.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.4f);

        Button btn = buttonObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.4f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.5f);
        colors.pressedColor = new Color(0.5f, 0.5f, 0.6f);
        btn.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        Text textComp = textObj.AddComponent<Text>();
        textComp.text = text;
        textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComp.fontSize = 28;
        textComp.alignment = TextAnchor.MiddleCenter;
        textComp.color = Color.white;

        Undo.RegisterCreatedObjectUndo(buttonObj, $"Create {name}");
        return btn;
    }

    private static void CreateVisualizationArea()
    {
        GameObject separator = GameObject.CreatePrimitive(PrimitiveType.Quad);
        separator.name = "Separator";
        separator.transform.position = new Vector3(-3, 0, 0);
        separator.transform.localScale = new Vector3(0.1f, 16f, 1f);

        Renderer rend = separator.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        rend.material = mat;

        Undo.RegisterCreatedObjectUndo(separator, "Create Separator");
    }

    private static void CreateJudgmentLine()
    {
        GameObject judgmentLine = GameObject.CreatePrimitive(PrimitiveType.Quad);
        judgmentLine.name = "JudgmentLine";
        judgmentLine.transform.position = new Vector3(0, -4f, 0);
        judgmentLine.transform.localScale = new Vector3(8f, 0.2f, 1f);

        Renderer rend = judgmentLine.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(1f, 0.5f, 0f, 0.8f);
        rend.material = mat;

        Undo.RegisterCreatedObjectUndo(judgmentLine, "Create Judgment Line");
    }
}
