using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// StoryPlayer UI 快速创建工具
/// 使用方法：在Hierarchy中右键 → Story System → Create Story Player UI
/// </summary>
public class StoryPlayerUICreator
{
#if UNITY_EDITOR
    [MenuItem("GameObject/Story System/Create Story Player UI", false, 10)]
    static void CreateStoryPlayerUI(MenuCommand menuCommand)
    {
        // 查找或创建Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }

        // 创建StoryPanel根对象
        GameObject storyPanel = new GameObject("StoryPanel");
        storyPanel.transform.SetParent(canvas.transform, false);

        RectTransform storyPanelRect = storyPanel.AddComponent<RectTransform>();
        storyPanelRect.anchorMin = Vector2.zero;
        storyPanelRect.anchorMax = Vector2.one;
        storyPanelRect.sizeDelta = Vector2.zero;
        storyPanelRect.anchoredPosition = Vector2.zero;

        // 添加StoryPlayer组件
        StoryPlayer storyPlayer = storyPanel.AddComponent<StoryPlayer>();

        // 创建Background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(storyPanel.transform, false);
        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);

        // 创建左侧立绘槽
        GameObject leftCharacterImage = new GameObject("LeftCharacterImage");
        leftCharacterImage.transform.SetParent(storyPanel.transform, false);
        RectTransform leftCharRect = leftCharacterImage.AddComponent<RectTransform>();
        leftCharRect.anchorMin = new Vector2(0f, 0f);
        leftCharRect.anchorMax = new Vector2(0f, 1f);
        leftCharRect.sizeDelta = new Vector2(600, 0);
        leftCharRect.anchoredPosition = new Vector2(300, 0);

        Image leftCharImage = leftCharacterImage.AddComponent<Image>();
        leftCharImage.color = Color.white;
        leftCharImage.preserveAspect = true;

        // 创建右侧立绘槽
        GameObject rightCharacterImage = new GameObject("RightCharacterImage");
        rightCharacterImage.transform.SetParent(storyPanel.transform, false);
        RectTransform rightCharRect = rightCharacterImage.AddComponent<RectTransform>();
        rightCharRect.anchorMin = new Vector2(1f, 0f);
        rightCharRect.anchorMax = new Vector2(1f, 1f);
        rightCharRect.sizeDelta = new Vector2(600, 0);
        rightCharRect.anchoredPosition = new Vector2(-300, 0);

        Image rightCharImage = rightCharacterImage.AddComponent<Image>();
        rightCharImage.color = Color.white;
        rightCharImage.preserveAspect = true;

        // 创建DialogueBox
        GameObject dialogueBox = new GameObject("DialogueBox");
        dialogueBox.transform.SetParent(storyPanel.transform, false);
        RectTransform dialogueBoxRect = dialogueBox.AddComponent<RectTransform>();
        dialogueBoxRect.anchorMin = new Vector2(0.1f, 0.05f);
        dialogueBoxRect.anchorMax = new Vector2(0.9f, 0.3f);
        dialogueBoxRect.sizeDelta = Vector2.zero;
        dialogueBoxRect.anchoredPosition = Vector2.zero;

        Image dialogueBoxImage = dialogueBox.AddComponent<Image>();
        dialogueBoxImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // 创建NameText
        GameObject nameText = new GameObject("NameText");
        nameText.transform.SetParent(dialogueBox.transform, false);
        RectTransform nameRect = nameText.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.05f, 0.7f);
        nameRect.anchorMax = new Vector2(0.4f, 0.95f);
        nameRect.sizeDelta = Vector2.zero;
        nameRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI nameTMP = nameText.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "角色名";
        nameTMP.fontSize = 28;
        nameTMP.color = Color.yellow;
        nameTMP.alignment = TextAlignmentOptions.Left;

        // 创建DialogText
        GameObject dialogText = new GameObject("DialogText");
        dialogText.transform.SetParent(dialogueBox.transform, false);
        RectTransform dialogRect = dialogText.AddComponent<RectTransform>();
        dialogRect.anchorMin = new Vector2(0.05f, 0.1f);
        dialogRect.anchorMax = new Vector2(0.95f, 0.65f);
        dialogRect.sizeDelta = Vector2.zero;
        dialogRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI dialogTMP = dialogText.AddComponent<TextMeshProUGUI>();
        dialogTMP.text = "对话内容会显示在这里...";
        dialogTMP.fontSize = 24;
        dialogTMP.color = Color.white;
        dialogTMP.alignment = TextAlignmentOptions.TopLeft;

        // 创建NextButton
        GameObject nextButton = CreateButton("NextButton", "继续", storyPanel.transform);
        RectTransform nextRect = nextButton.GetComponent<RectTransform>();
        nextRect.anchorMin = new Vector2(0.75f, 0.05f);
        nextRect.anchorMax = new Vector2(0.9f, 0.15f);
        nextRect.sizeDelta = Vector2.zero;
        nextRect.anchoredPosition = Vector2.zero;

        // 创建SkipButton
        GameObject skipButton = CreateButton("SkipButton", "跳过", storyPanel.transform);
        RectTransform skipRect = skipButton.GetComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(0.85f, 0.85f);
        skipRect.anchorMax = new Vector2(0.95f, 0.95f);
        skipRect.sizeDelta = Vector2.zero;
        skipRect.anchoredPosition = Vector2.zero;

        // 绑定引用到StoryPlayer
        storyPlayer.storyPanel = storyPanel;
        storyPlayer.backgroundImage = bgImage;
        storyPlayer.leftCharacterImage = leftCharImage;
        storyPlayer.rightCharacterImage = rightCharImage;
        storyPlayer.dialogueBox = dialogueBox;
        storyPlayer.characterNameText = nameTMP;
        storyPlayer.dialogueText = dialogTMP;
        storyPlayer.nextButton = nextButton.GetComponent<Button>();
        storyPlayer.skipButton = skipButton.GetComponent<Button>();

        // 初始隐藏面板
        storyPanel.SetActive(false);

        // 注册Undo
        Undo.RegisterCreatedObjectUndo(storyPanel, "Create Story Player UI");

        // 选中创建的对象
        Selection.activeGameObject = storyPanel;

        Debug.Log("[StoryPlayerUICreator] StoryPlayer UI 创建成功！");
    }

    static GameObject CreateButton(string name, string text, Transform parent)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        // 创建按钮文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 20;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        return buttonObj;
    }

    [MenuItem("GameObject/Story System/Create AVG Story Manager", false, 11)]
    static void CreateAVGStoryManager(MenuCommand menuCommand)
    {
        // 检查是否已存在
        if (Object.FindObjectOfType<AVGStoryManager>() != null)
        {
            EditorUtility.DisplayDialog("提示", "场景中已存在 AVGStoryManager！", "确定");
            return;
        }

        GameObject managerObj = new GameObject("AVGStoryManager");
        AVGStoryManager manager = managerObj.AddComponent<AVGStoryManager>();

        // 尝试查找StoryPlayer
        StoryPlayer player = Object.FindObjectOfType<StoryPlayer>();
        if (player != null)
        {
            manager.storyPlayer = player;
            Debug.Log("[StoryPlayerUICreator] 已自动关联 StoryPlayer");
        }

        Undo.RegisterCreatedObjectUndo(managerObj, "Create AVG Story Manager");
        Selection.activeGameObject = managerObj;

        Debug.Log("[StoryPlayerUICreator] AVGStoryManager 创建成功！");
    }
#endif
}
