using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 在PlayScene中自动设置歌曲进度条UI的编辑器工具
/// 使用方法：在Unity编辑器菜单中选择 Tools > Setup Song Progress Bar
/// </summary>
public class SetupSongProgressBar : EditorWindow
{
    [MenuItem("Tools/Setup Song Progress Bar")]
    public static void SetupProgressBar()
    {
        // 查找Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene! Please add a Canvas first.");
            return;
        }

        // 创建进度条容器
        GameObject progressBarContainer = new GameObject("SongProgressBar");
        progressBarContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = progressBarContainer.AddComponent<RectTransform>();

        // 设置位置：屏幕顶部中央
        containerRect.anchorMin = new Vector2(0.5f, 1f);
        containerRect.anchorMax = new Vector2(0.5f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = new Vector2(0, -20);
        containerRect.sizeDelta = new Vector2(600, 30);

        // 创建背景
        GameObject background = new GameObject("Background");
        background.transform.SetParent(progressBarContainer.transform, false);

        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        // 使用Unity内置的白色Sprite
        bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        // 创建填充条
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(progressBarContainer.transform, false);

        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 1f, 1f);
        // 使用Unity内置的白色Sprite
        fillImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 0f;

        // 创建百分比文本
        GameObject percentText = new GameObject("PercentText");
        percentText.transform.SetParent(progressBarContainer.transform, false);

        RectTransform percentRect = percentText.AddComponent<RectTransform>();
        percentRect.anchorMin = new Vector2(0.5f, 0.5f);
        percentRect.anchorMax = new Vector2(0.5f, 0.5f);
        percentRect.pivot = new Vector2(0.5f, 0.5f);
        percentRect.anchoredPosition = Vector2.zero;
        percentRect.sizeDelta = new Vector2(100, 30);

        TextMeshProUGUI percentTMP = percentText.AddComponent<TextMeshProUGUI>();
        percentTMP.text = "0%";
        percentTMP.fontSize = 18;
        percentTMP.alignment = TextAlignmentOptions.Center;
        percentTMP.color = Color.white;
        percentTMP.fontStyle = FontStyles.Bold;

        // 创建进度文本 (50/100)
        GameObject progressText = new GameObject("ProgressText");
        progressText.transform.SetParent(progressBarContainer.transform, false);

        RectTransform progressRect = progressText.AddComponent<RectTransform>();
        progressRect.anchorMin = new Vector2(1f, 0.5f);
        progressRect.anchorMax = new Vector2(1f, 0.5f);
        progressRect.pivot = new Vector2(1f, 0.5f);
        progressRect.anchoredPosition = new Vector2(-10, 0);
        progressRect.sizeDelta = new Vector2(100, 30);

        TextMeshProUGUI progressTMP = progressText.AddComponent<TextMeshProUGUI>();
        progressTMP.text = "0/0";
        progressTMP.fontSize = 16;
        progressTMP.alignment = TextAlignmentOptions.Right;
        progressTMP.color = Color.white;

        // 添加SongProgressBar组件
        SongProgressBar progressBarScript = progressBarContainer.AddComponent<SongProgressBar>();
        progressBarScript.fillRectTransform = fillRect;
        // progressBarScript.progressBarFill = fillImage; // 已移除
        // progressBarScript.percentageText = percentTMP; // 已移除
        // progressBarScript.progressText = progressTMP; // 已移除

        // 标记场景为已修改
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log("[SetupSongProgressBar] Song progress bar created successfully!");

        // 选中创建的对象
        Selection.activeGameObject = progressBarContainer;
    }
}
