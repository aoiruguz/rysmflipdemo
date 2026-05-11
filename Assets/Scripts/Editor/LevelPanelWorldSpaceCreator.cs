using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// World Space 关卡面板创建工具
/// 用于快速生成世界坐标系中的关卡选择面板
/// </summary>
public class LevelPanelWorldSpaceCreator : EditorWindow
{
    private Vector2 canvasSize = new Vector2(400, 150);
    private float canvasScale = 0.01f;
    private Vector3 spawnPosition = Vector3.zero;
    private float verticalSpacing = 2f;
    private int panelCount = 5;

    [MenuItem("Tools/Level Selection/Create World Space Level Panel")]
    public static void ShowWindow()
    {
        GetWindow<LevelPanelWorldSpaceCreator>("World Space 关卡面板创建器");
    }

    private void OnGUI()
    {
        GUILayout.Label("World Space 关卡面板创建工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "这个工具会创建 World Space Canvas 面板，适合镜头移动的关卡选择系统。",
            MessageType.Info);

        EditorGUILayout.Space();
        GUILayout.Label("Canvas 设置", EditorStyles.boldLabel);
        canvasSize = EditorGUILayout.Vector2Field("Canvas 尺寸", canvasSize);
        canvasScale = EditorGUILayout.FloatField("Canvas 缩放", canvasScale);

        EditorGUILayout.Space();
        GUILayout.Label("批量生成设置", EditorStyles.boldLabel);
        spawnPosition = EditorGUILayout.Vector3Field("起始位置", spawnPosition);
        verticalSpacing = EditorGUILayout.FloatField("垂直间距", verticalSpacing);
        panelCount = EditorGUILayout.IntField("面板数量", panelCount);

        EditorGUILayout.Space();

        if (GUILayout.Button("创建单个面板 Prefab", GUILayout.Height(40)))
        {
            CreateLevelPanelPrefab();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("批量生成面板到场景", GUILayout.Height(40)))
        {
            BatchCreatePanelsInScene();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("设置 EventSystem（用于点击交互）", GUILayout.Height(40)))
        {
            SetupEventSystem();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "使用步骤：\n" +
            "1. 点击 '创建单个面板 Prefab' 创建预制体\n" +
            "2. 点击 '批量生成面板到场景' 在场景中生成多个面板\n" +
            "3. 点击 '设置 EventSystem' 确保点击交互正常工作\n" +
            "4. 手动为每个面板分配 LevelData",
            MessageType.Info);
    }

    /// <summary>
    /// 创建 World Space 关卡面板 Prefab
    /// </summary>
    private void CreateLevelPanelPrefab()
    {
        // 创建 Canvas 根对象
        GameObject canvasObj = new GameObject("LevelPanel_WorldSpace");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // 设置 RectTransform
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = canvasSize;
        canvasRect.localScale = new Vector3(canvasScale, canvasScale, canvasScale);

        // 添加 CanvasScaler（可选，但推荐）
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        // 添加 GraphicRaycaster（用于点击检测）
        canvasObj.AddComponent<GraphicRaycaster>();

        // 创建背景面板
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // 创建敌人头像槽
        GameObject iconObj = new GameObject("EnemyIcon");
        iconObj.transform.SetParent(panelObj.transform, false);

        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(20, 0);
        iconRect.sizeDelta = new Vector2(100, 100);

        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // 添加圆角效果（可选）
        Outline iconOutline = iconObj.AddComponent<Outline>();
        iconOutline.effectColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        iconOutline.effectDistance = new Vector2(2, -2);

        // 创建敌人名字
        GameObject nameObj = new GameObject("EnemyName");
        nameObj.transform.SetParent(panelObj.transform, false);

        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(0, 0.5f);
        nameRect.pivot = new Vector2(0, 0.5f);
        nameRect.anchoredPosition = new Vector2(140, 25);
        nameRect.sizeDelta = new Vector2(200, 40);

        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "对手名字六字";
        nameText.fontSize = 28;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.fontStyle = FontStyles.Bold;

        // 创建粉丝量文本
        GameObject fansObj = new GameObject("FansCount");
        fansObj.transform.SetParent(panelObj.transform, false);

        RectTransform fansRect = fansObj.AddComponent<RectTransform>();
        fansRect.anchorMin = new Vector2(0, 0.5f);
        fansRect.anchorMax = new Vector2(0, 0.5f);
        fansRect.pivot = new Vector2(0, 0.5f);
        fansRect.anchoredPosition = new Vector2(140, -15);
        fansRect.sizeDelta = new Vector2(200, 30);

        TextMeshProUGUI fansText = fansObj.AddComponent<TextMeshProUGUI>();
        fansText.text = "粉丝: 10000";
        fansText.fontSize = 20;
        fansText.color = new Color(1f, 0.8f, 0.2f);
        fansText.alignment = TextAlignmentOptions.Left;

        // 创建挑战按钮
        GameObject buttonObj = new GameObject("ChallengeButton");
        buttonObj.transform.SetParent(panelObj.transform, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 0.5f);
        buttonRect.anchorMax = new Vector2(1, 0.5f);
        buttonRect.pivot = new Vector2(1, 0.5f);
        buttonRect.anchoredPosition = new Vector2(-20, 0);
        buttonRect.sizeDelta = new Vector2(120, 50);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.5f, 0.8f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        // 设置按钮颜色变化
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.5f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.6f, 0.9f, 1f);
        colors.pressedColor = new Color(0.15f, 0.4f, 0.7f, 1f);
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        button.colors = colors;

        // 创建按钮文本
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);

        RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.sizeDelta = Vector2.zero;
        buttonTextRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "挑战";
        buttonText.fontSize = 24;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;

        // 创建锁定图标（默认隐藏）
        GameObject lockObj = new GameObject("LockIcon");
        lockObj.transform.SetParent(panelObj.transform, false);

        RectTransform lockRect = lockObj.AddComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0.5f, 0.5f);
        lockRect.anchorMax = new Vector2(0.5f, 0.5f);
        lockRect.pivot = new Vector2(0.5f, 0.5f);
        lockRect.anchoredPosition = Vector2.zero;
        lockRect.sizeDelta = new Vector2(80, 80);

        Image lockImage = lockObj.AddComponent<Image>();
        lockImage.color = new Color(1f, 1f, 1f, 0.8f);
        lockObj.SetActive(false);

        // 添加 LevelPanelWorldSpace 组件
        LevelPanelWorldSpace panelScript = canvasObj.AddComponent<LevelPanelWorldSpace>();
        panelScript.enemyIcon = iconImage;
        panelScript.enemyNameText = nameText;
        panelScript.fansCountText = fansText;
        panelScript.challengeButton = button;
        panelScript.buttonText = buttonText;
        panelScript.lockIcon = lockImage;

        // 保存为 Prefab
        string path = "Assets/Prefabs/UI/LevelPanel_WorldSpace.prefab";
        string directory = System.IO.Path.GetDirectoryName(path);

        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        PrefabUtility.SaveAsPrefabAsset(canvasObj, path);
        DestroyImmediate(canvasObj);

        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        EditorUtility.DisplayDialog("成功", $"World Space 关卡面板 Prefab 已创建！\n路径: {path}", "确定");
    }

    /// <summary>
    /// 批量在场景中生成面板
    /// </summary>
    private void BatchCreatePanelsInScene()
    {
        string prefabPath = "Assets/Prefabs/UI/LevelPanel_WorldSpace.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("错误", "请先创建面板 Prefab！", "确定");
            return;
        }

        // 创建容器
        GameObject container = new GameObject("LevelPanels_Container");
        container.transform.position = Vector3.zero;

        // 批量生成
        for (int i = 0; i < panelCount; i++)
        {
            GameObject panel = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            panel.transform.SetParent(container.transform);
            panel.name = $"LevelPanel_{i + 1}";

            // 设置位置（纵向排列）
            Vector3 position = spawnPosition + new Vector3(0, -i * verticalSpacing, 0);
            panel.transform.position = position;
        }

        Selection.activeGameObject = container;
        EditorUtility.DisplayDialog("成功", $"已在场景中生成 {panelCount} 个面板！", "确定");
    }

    /// <summary>
    /// 设置 EventSystem
    /// </summary>
    private void SetupEventSystem()
    {
        // 检查是否已有 EventSystem
        EventSystem existingEventSystem = FindObjectOfType<EventSystem>();
        if (existingEventSystem != null)
        {
            EditorUtility.DisplayDialog("提示", "场景中已经有 EventSystem 了！", "确定");
            Selection.activeGameObject = existingEventSystem.gameObject;
            return;
        }

        // 创建 EventSystem
        GameObject eventSystemObj = new GameObject("EventSystem");
        EventSystem eventSystem = eventSystemObj.AddComponent<EventSystem>();
        StandaloneInputModule inputModule = eventSystemObj.AddComponent<StandaloneInputModule>();

        // 添加 Physics 2D Raycaster 到 Main Camera
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Physics2DRaycaster raycaster = mainCamera.gameObject.GetComponent<Physics2DRaycaster>();
            if (raycaster == null)
            {
                mainCamera.gameObject.AddComponent<Physics2DRaycaster>();
            }
        }

        Selection.activeGameObject = eventSystemObj;
        EditorUtility.DisplayDialog("成功", "EventSystem 已创建，并为 Main Camera 添加了 Physics2DRaycaster！", "确定");
    }
}
