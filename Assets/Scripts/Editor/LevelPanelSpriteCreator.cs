using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// 基于 Sprite 的关卡面板创建工具
/// 使用 SpriteRenderer 和 Collider 实现交互，不依赖 UI 系统
/// </summary>
public class LevelPanelSpriteCreator : EditorWindow
{
    private Vector3 spawnPosition = Vector3.zero;
    private float verticalSpacing = 2f;
    private int panelCount = 5;
    private float panelWidth = 4f;
    private float panelHeight = 1.5f;

    [MenuItem("Tools/Level Selection/Create Sprite Level Panel")]
    public static void ShowWindow()
    {
        GetWindow<LevelPanelSpriteCreator>("Sprite 关卡面板创建器");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite 关卡面板创建工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "这个工具会创建基于 SpriteRenderer 的面板，使用 Collider 和 OnMouseDown 实现交互。",
            MessageType.Info);

        EditorGUILayout.Space();
        GUILayout.Label("面板设置", EditorStyles.boldLabel);
        panelWidth = EditorGUILayout.FloatField("面板宽度", panelWidth);
        panelHeight = EditorGUILayout.FloatField("面板高度", panelHeight);

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
        EditorGUILayout.HelpBox(
            "使用步骤：\n" +
            "1. 点击 '创建单个面板 Prefab' 创建预制体\n" +
            "2. 点击 '批量生成面板到场景' 在场景中生成多个面板\n" +
            "3. 手动为每个面板分配 LevelData\n" +
            "4. 确保相机有 Physics2DRaycaster（用于鼠标检测）",
            MessageType.Info);
    }

    /// <summary>
    /// 创建基于 Sprite 的关卡面板 Prefab
    /// </summary>
    private void CreateLevelPanelPrefab()
    {
        // 创建根对象
        GameObject rootObj = new GameObject("LevelPanel_Sprite");
        rootObj.transform.position = Vector3.zero;

        // 创建背景 Sprite
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(rootObj.transform, false);
        backgroundObj.transform.localPosition = Vector3.zero;

        SpriteRenderer backgroundSprite = backgroundObj.AddComponent<SpriteRenderer>();
        backgroundSprite.sprite = CreateRectSprite(400, 150);
        backgroundSprite.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);
        backgroundSprite.sortingOrder = 0;

        // 设置背景大小
        backgroundObj.transform.localScale = new Vector3(panelWidth / 4f, panelHeight / 1.5f, 1f);

        // 创建敌人头像
        GameObject iconObj = new GameObject("EnemyIcon");
        iconObj.transform.SetParent(rootObj.transform, false);
        iconObj.transform.localPosition = new Vector3(-panelWidth * 0.3f, 0, -0.01f);

        SpriteRenderer iconSprite = iconObj.AddComponent<SpriteRenderer>();
        iconSprite.sprite = CreateRectSprite(100, 100);
        iconSprite.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        iconSprite.sortingOrder = 5;
        iconObj.transform.localScale = new Vector3(1f, 1f, 1f);

        // 创建敌人名字 (TextMeshPro 3D)
        GameObject nameObj = new GameObject("EnemyName");
        nameObj.transform.SetParent(rootObj.transform, false);
        nameObj.transform.localPosition = new Vector3(-panelWidth * 0.05f, panelHeight * 0.15f, -0.01f);

        TextMeshPro nameText = nameObj.AddComponent<TextMeshPro>();
        nameText.text = "对手名字六字";
        nameText.fontSize = 5;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.fontStyle = FontStyles.Bold;
        nameText.sortingOrder = 6;

        // 设置文本大小
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(2, 0.5f);

        // 创建粉丝量文本
        GameObject fansObj = new GameObject("FansCount");
        fansObj.transform.SetParent(rootObj.transform, false);
        fansObj.transform.localPosition = new Vector3(-panelWidth * 0.05f, -panelHeight * 0.1f, -0.01f);

        TextMeshPro fansText = fansObj.AddComponent<TextMeshPro>();
        fansText.text = "粉丝: 10000";
        fansText.fontSize = 3.5f;
        fansText.color = new Color(1f, 0.8f, 0.2f);
        fansText.alignment = TextAlignmentOptions.Left;
        fansText.sortingOrder = 6;

        RectTransform fansRect = fansObj.GetComponent<RectTransform>();
        fansRect.sizeDelta = new Vector2(2, 0.4f);

        // 创建挑战按钮背景
        GameObject buttonObj = new GameObject("ChallengeButton");
        buttonObj.transform.SetParent(rootObj.transform, false);
        buttonObj.transform.localPosition = new Vector3(panelWidth * 0.28f, 0, 0);

        SpriteRenderer buttonSprite = buttonObj.AddComponent<SpriteRenderer>();
        buttonSprite.sprite = CreateRectSprite(120, 50);
        buttonSprite.color = new Color(0.2f, 0.5f, 0.8f, 1f);
        buttonSprite.sortingOrder = 10;
        buttonObj.transform.localScale = new Vector3(1.2f, 0.5f, 1f);

        // 添加 Collider 用于点击检测（调整大小以匹配缩放后的按钮）
        BoxCollider2D buttonCollider = buttonObj.AddComponent<BoxCollider2D>();
        buttonCollider.size = new Vector2(1.2f, 1f);

        // 创建按钮文本
        GameObject buttonTextObj = new GameObject("ButtonText");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        buttonTextObj.transform.localPosition = new Vector3(0, 0, -0.01f);

        TextMeshPro buttonText = buttonTextObj.AddComponent<TextMeshPro>();
        buttonText.text = "挑战";
        buttonText.fontSize = 4;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.sortingOrder = 11;

        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.sizeDelta = new Vector2(1, 0.5f);

        // 创建锁定图标（默认隐藏）
        GameObject lockObj = new GameObject("LockIcon");
        lockObj.transform.SetParent(rootObj.transform, false);
        lockObj.transform.localPosition = new Vector3(0, 0, -0.01f);

        SpriteRenderer lockSprite = lockObj.AddComponent<SpriteRenderer>();
        lockSprite.sprite = CreateRectSprite(80, 80);
        lockSprite.color = new Color(1f, 1f, 1f, 0.8f);
        lockSprite.sortingOrder = 20;
        lockObj.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        lockObj.SetActive(false);

        // 添加 LevelPanelSprite 组件到按钮（用于接收 OnMouseDown）
        LevelPanelSprite panelScript = buttonObj.AddComponent<LevelPanelSprite>();
        panelScript.backgroundSprite = backgroundSprite;
        panelScript.enemyIconSprite = iconSprite;
        panelScript.buttonSprite = buttonSprite;
        panelScript.lockIconSprite = lockSprite;
        panelScript.enemyNameText = nameText;
        panelScript.fansCountText = fansText;
        panelScript.buttonText = buttonText;
        panelScript.buttonCollider = buttonCollider;

        // 保存为 Prefab
        string path = "Assets/Prefabs/UI/LevelPanel_Sprite.prefab";
        string directory = System.IO.Path.GetDirectoryName(path);

        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        PrefabUtility.SaveAsPrefabAsset(rootObj, path);
        DestroyImmediate(rootObj);

        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        EditorUtility.DisplayDialog("成功", $"Sprite 关卡面板 Prefab 已创建！\n路径: {path}", "确定");
    }

    /// <summary>
    /// 批量在场景中生成面板
    /// </summary>
    private void BatchCreatePanelsInScene()
    {
        string prefabPath = "Assets/Prefabs/UI/LevelPanel_Sprite.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("错误", "请先创建面板 Prefab！", "确定");
            return;
        }

        // 创建容器
        GameObject container = new GameObject("LevelPanels_Sprite_Container");
        container.transform.position = Vector3.zero;

        // 批量生成
        for (int i = 0; i < panelCount; i++)
        {
            GameObject panel = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            panel.transform.SetParent(container.transform);
            panel.name = $"LevelPanel_Sprite_{i + 1}";

            // 设置位置（纵向排列）
            Vector3 position = spawnPosition + new Vector3(0, -i * verticalSpacing, 0);
            panel.transform.position = position;
        }

        Selection.activeGameObject = container;
        EditorUtility.DisplayDialog("成功", $"已在场景中生成 {panelCount} 个面板！", "确定");
    }

    /// <summary>
    /// 创建一个简单的矩形 Sprite
    /// </summary>
    private Sprite CreateRectSprite(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100);
    }
}
