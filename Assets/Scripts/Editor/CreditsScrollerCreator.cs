using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// 制作者名单滚动 UI 自动创建工具
/// </summary>
public class CreditsScrollerCreator : EditorWindow
{
    private string[] creditsLines = new string[]
    {
        "制作人员",
        "",
        "策划",
        "XXX",
        "",
        "程序",
        "XXX",
        "",
        "美术",
        "XXX",
        "",
        "音乐",
        "XXX",
        "",
        "特别感谢",
        "XXX",
        "",
        "感谢游玩！"
    };

    private float scrollSpeed = 50f;
    private bool autoStart = true;
    private bool loop = false;
    private float startPositionY = -500f;
    private float endPositionY = 500f;
    private bool autoCalculateEndPosition = true;

    private float visibleMinY = -300f;
    private float visibleMaxY = 300f;
    private bool enableVisibilityClipping = true;

    private TMP_FontAsset font;
    private int fontSize = 36;
    private Color textColor = Color.white;

    private float viewportWidth = 800f;
    private float viewportHeight = 600f;

    private Vector2 scrollPosition;

    [MenuItem("GameObject/UI/Credits Scroller", false, 10)]
    static void CreateCreditsScroller(MenuCommand menuCommand)
    {
        CreditsScrollerCreator window = GetWindow<CreditsScrollerCreator>("创建制作者名单");
        window.minSize = new Vector2(450, 700);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("制作者名单滚动 UI 创建工具", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 滚动设置
        GUILayout.Label("滚动设置", EditorStyles.boldLabel);
        scrollSpeed = EditorGUILayout.FloatField("滚动速度", scrollSpeed);
        autoStart = EditorGUILayout.Toggle("自动开始", autoStart);
        loop = EditorGUILayout.Toggle("循环滚动", loop);
        startPositionY = EditorGUILayout.FloatField("起始位置 Y", startPositionY);
        autoCalculateEndPosition = EditorGUILayout.Toggle("自动计算结束位置", autoCalculateEndPosition);
        if (!autoCalculateEndPosition)
        {
            endPositionY = EditorGUILayout.FloatField("结束位置 Y", endPositionY);
        }

        GUILayout.Space(10);

        // 可视区域设置
        GUILayout.Label("可视区域设置", EditorStyles.boldLabel);
        enableVisibilityClipping = EditorGUILayout.Toggle("启用可视区域裁剪", enableVisibilityClipping);
        if (enableVisibilityClipping)
        {
            visibleMinY = EditorGUILayout.FloatField("可视区域最小 Y", visibleMinY);
            visibleMaxY = EditorGUILayout.FloatField("可视区域最大 Y", visibleMaxY);
        }
        viewportWidth = EditorGUILayout.FloatField("可视区域宽度", viewportWidth);
        viewportHeight = EditorGUILayout.FloatField("可视区域高度", viewportHeight);

        GUILayout.Space(10);

        // 文本设置
        GUILayout.Label("文本样式设置", EditorStyles.boldLabel);
        font = (TMP_FontAsset)EditorGUILayout.ObjectField("字体", font, typeof(TMP_FontAsset), false);
        fontSize = EditorGUILayout.IntField("字体大小", fontSize);
        textColor = EditorGUILayout.ColorField("文字颜色", textColor);

        GUILayout.Space(10);

        // 制作者名单内容
        GUILayout.Label("制作者名单内容（每行一个条目，空行表示间距）", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

        for (int i = 0; i < creditsLines.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            creditsLines[i] = EditorGUILayout.TextField($"第 {i + 1} 行", creditsLines[i]);

            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                ArrayUtility.RemoveAt(ref creditsLines, i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(5);

        if (GUILayout.Button("添加新行"))
        {
            ArrayUtility.Add(ref creditsLines, "");
        }

        GUILayout.Space(20);

        // 创建按钮
        if (GUILayout.Button("创建制作者名单 UI", GUILayout.Height(40)))
        {
            CreateCreditsUI();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "此工具会在当前选中的 Canvas 下创建制作者名单滚动 UI。\n" +
            "如果没有选中 Canvas，会自动创建一个新的 Canvas。\n" +
            "UI 完全透明，只显示文本滚动效果。\n" +
            "文本会在可视区域内显示，超出区域自动隐藏。",
            MessageType.Info
        );
    }

    void CreateCreditsUI()
    {
        // 查找或创建 Canvas
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

        // 创建 CreditsPanel（主容器）
        GameObject creditsPanel = new GameObject("CreditsPanel");
        creditsPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = creditsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(viewportWidth, viewportHeight);
        panelRect.anchoredPosition = Vector2.zero;

        // 添加 CreditsScroller 组件
        CreditsScroller scroller = creditsPanel.AddComponent<CreditsScroller>();

        // 创建 Viewport（可视区域，带遮罩）
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(creditsPanel.transform, false);

        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;

        // 添加 Mask 组件用于裁剪
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // 添加透明 Image 组件（Mask 需要）
        Image maskImage = viewport.AddComponent<Image>();
        maskImage.color = new Color(1, 1, 1, 0); // 完全透明

        // 创建 ScrollContent（滚动内容容器）
        GameObject scrollContent = new GameObject("ScrollContent");
        scrollContent.transform.SetParent(viewport.transform, false);

        RectTransform contentRect = scrollContent.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0f);
        contentRect.anchorMax = new Vector2(0.5f, 0f);
        contentRect.pivot = new Vector2(0.5f, 0f);
        contentRect.anchoredPosition = new Vector2(0, startPositionY);

        // 添加 VerticalLayoutGroup 用于自动排列文本
        VerticalLayoutGroup layoutGroup = scrollContent.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.spacing = 20f;
        layoutGroup.padding = new RectOffset(50, 50, 50, 50);

        // 添加 ContentSizeFitter 自动调整内容大小
        ContentSizeFitter sizeFitter = scrollContent.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 创建文本行
        foreach (string line in creditsLines)
        {
            GameObject textObj = new GameObject(string.IsNullOrEmpty(line) ? "Spacer" : "Text");
            textObj.transform.SetParent(scrollContent.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(viewportWidth - 100, string.IsNullOrEmpty(line) ? 40 : fontSize + 10);

            if (!string.IsNullOrEmpty(line))
            {
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = line;
                text.fontSize = fontSize;
                text.color = textColor;
                text.alignment = TextAlignmentOptions.Center;
                text.enableWordWrapping = false;

                // 设置字体
                if (font != null)
                {
                    text.font = font;
                }
            }
        }

        // 配置 CreditsScroller
        scroller.scrollContent = contentRect;
        scroller.viewportRect = viewportRect;
        scroller.scrollSpeed = scrollSpeed;
        scroller.autoStart = autoStart;
        scroller.loop = loop;
        scroller.startPositionY = startPositionY;
        scroller.endPositionY = endPositionY;
        scroller.autoCalculateEndPosition = autoCalculateEndPosition;
        scroller.visibleMinY = visibleMinY;
        scroller.visibleMaxY = visibleMaxY;
        scroller.enableVisibilityClipping = enableVisibilityClipping;
        scroller.font = font;
        scroller.fontSize = fontSize;
        scroller.textColor = textColor;
        scroller.applyTextStyleOnStart = true;

        // 注册 Undo
        Undo.RegisterCreatedObjectUndo(creditsPanel, "Create Credits Scroller");

        // 选中创建的对象
        Selection.activeGameObject = creditsPanel;

        Debug.Log("[CreditsScrollerCreator] 制作者名单 UI 创建成功！");

        Close();
    }
}
