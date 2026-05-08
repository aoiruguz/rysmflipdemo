using UnityEngine;
using UnityEditor;

/// <summary>
/// 自动创建ChartFileSelector UI的Editor工具
/// </summary>
public class CreateChartFileSelectorUI : EditorWindow
{
    [MenuItem("Tools/Chart Editor/Create File Selector UI")]
    public static void CreateUI()
    {
        // 创建主面板
        GameObject selectorPanel = new GameObject("ChartFileSelector_Panel");
        Canvas canvas = selectorPanel.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // 确保在最上层

        UnityEngine.UI.CanvasScaler scaler = selectorPanel.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        selectorPanel.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // 创建背景遮罩
        GameObject background = new GameObject("Background");
        background.transform.SetParent(selectorPanel.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Image bgImage = background.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);

        // 创建内容面板
        GameObject contentPanel = new GameObject("ContentPanel");
        contentPanel.transform.SetParent(selectorPanel.transform, false);
        RectTransform contentRect = contentPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(600, 800);
        contentRect.anchoredPosition = Vector2.zero;

        UnityEngine.UI.Image contentImage = contentPanel.AddComponent<UnityEngine.UI.Image>();
        contentImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // 创建标题
        GameObject title = new GameObject("Title");
        title.transform.SetParent(contentPanel.transform, false);
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(-40, 60);
        titleRect.anchoredPosition = new Vector2(0, -30);

        TMPro.TextMeshProUGUI titleText = title.AddComponent<TMPro.TextMeshProUGUI>();
        titleText.text = "选择谱面文件";
        titleText.fontSize = 32;
        titleText.alignment = TMPro.TextAlignmentOptions.Center;
        titleText.color = Color.white;

        // 创建关闭按钮
        GameObject closeButton = new GameObject("CloseButton");
        closeButton.transform.SetParent(contentPanel.transform, false);
        RectTransform closeRect = closeButton.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1, 1);
        closeRect.anchorMax = new Vector2(1, 1);
        closeRect.sizeDelta = new Vector2(50, 50);
        closeRect.anchoredPosition = new Vector2(-25, -25);

        UnityEngine.UI.Image closeImage = closeButton.AddComponent<UnityEngine.UI.Image>();
        closeImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

        UnityEngine.UI.Button closeBtn = closeButton.AddComponent<UnityEngine.UI.Button>();

        GameObject closeText = new GameObject("Text");
        closeText.transform.SetParent(closeButton.transform, false);
        RectTransform closeTextRect = closeText.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.sizeDelta = Vector2.zero;

        TMPro.TextMeshProUGUI closeTMP = closeText.AddComponent<TMPro.TextMeshProUGUI>();
        closeTMP.text = "X";
        closeTMP.fontSize = 28;
        closeTMP.alignment = TMPro.TextAlignmentOptions.Center;
        closeTMP.color = Color.white;

        // 创建滚动视图
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(contentPanel.transform, false);
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.sizeDelta = new Vector2(-40, -120);
        scrollRect.anchoredPosition = new Vector2(0, -10);

        UnityEngine.UI.Image scrollImage = scrollView.AddComponent<UnityEngine.UI.Image>();
        scrollImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        UnityEngine.UI.ScrollRect scroll = scrollView.AddComponent<UnityEngine.UI.ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;

        // 创建Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;

        UnityEngine.UI.Mask mask = viewport.AddComponent<UnityEngine.UI.Mask>();
        mask.showMaskGraphic = false;

        UnityEngine.UI.Image viewportImage = viewport.AddComponent<UnityEngine.UI.Image>();

        // 创建Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentListRect = content.AddComponent<RectTransform>();
        contentListRect.anchorMin = new Vector2(0, 1);
        contentListRect.anchorMax = new Vector2(1, 1);
        contentListRect.pivot = new Vector2(0.5f, 1);
        contentListRect.sizeDelta = new Vector2(0, 0);

        UnityEngine.UI.VerticalLayoutGroup layout = content.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        UnityEngine.UI.ContentSizeFitter fitter = content.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = contentListRect;

        // 创建文件按钮预制体
        GameObject buttonPrefab = new GameObject("FileButton_Prefab");
        buttonPrefab.transform.SetParent(content.transform, false);
        RectTransform btnRect = buttonPrefab.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(0, 80);

        UnityEngine.UI.Image btnImage = buttonPrefab.AddComponent<UnityEngine.UI.Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        UnityEngine.UI.Button btn = buttonPrefab.AddComponent<UnityEngine.UI.Button>();

        GameObject btnText = new GameObject("Text");
        btnText.transform.SetParent(buttonPrefab.transform, false);
        RectTransform btnTextRect = btnText.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = new Vector2(-20, -20);

        TMPro.TextMeshProUGUI btnTMP = btnText.AddComponent<TMPro.TextMeshProUGUI>();
        btnTMP.text = "File Name";
        btnTMP.fontSize = 24;
        btnTMP.alignment = TMPro.TextAlignmentOptions.Center;
        btnTMP.color = Color.white;

        // 添加ChartFileSelector组件
        ChartFileSelector selector = selectorPanel.AddComponent<ChartFileSelector>();
        selector.selectorPanel = selectorPanel;
        selector.fileListContainer = contentListRect;
        selector.fileButtonPrefab = buttonPrefab;
        selector.closeButton = closeBtn;
        selector.titleText = titleText;

        // 默认隐藏
        selectorPanel.SetActive(false);

        Debug.Log("ChartFileSelector UI created successfully!");
        EditorUtility.DisplayDialog("成功", "ChartFileSelector UI已创建！\n\n请将这个GameObject拖到ChartEditorManager的fileSelector字段中。", "确定");

        Selection.activeGameObject = selectorPanel;
    }
}
