using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;

/// <summary>
/// 谱面文件选择器UI
/// 用于在build后从StreamingAssets/Charts目录选择谱面
/// </summary>
public class ChartFileSelector : MonoBehaviour
{
    [Header("UI References")]
    public GameObject selectorPanel;
    public Transform fileListContainer;
    public GameObject fileButtonPrefab;
    public Button closeButton;
    public TextMeshProUGUI titleText;

    private Action<string> onFileSelected;
    private Action onCancelled;

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        // 默认隐藏
        if (selectorPanel != null)
        {
            selectorPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 显示文件选择器
    /// </summary>
    public void ShowSelector(Action<string> onFileSelected, Action onCancelled = null)
    {
        this.onFileSelected = onFileSelected;
        this.onCancelled = onCancelled;

        if (selectorPanel != null)
        {
            selectorPanel.SetActive(true);
        }

        RefreshFileList();
    }

    /// <summary>
    /// 隐藏文件选择器
    /// </summary>
    public void HideSelector()
    {
        if (selectorPanel != null)
        {
            selectorPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 刷新文件列表
    /// </summary>
    private void RefreshFileList()
    {
        // 清空现有列表
        if (fileListContainer != null)
        {
            foreach (Transform child in fileListContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // 获取所有谱面文件
        string chartsDirectory = Path.Combine(Application.streamingAssetsPath, "Charts");

        if (!Directory.Exists(chartsDirectory))
        {
            Debug.LogWarning($"[ChartFileSelector] Charts目录不存在: {chartsDirectory}");
            CreateInfoButton("找不到Charts目录\n请将谱面文件放在StreamingAssets/Charts文件夹中");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(chartsDirectory, "*.json");

        if (jsonFiles.Length == 0)
        {
            CreateInfoButton("没有找到谱面文件\n请将.json文件放在StreamingAssets/Charts文件夹中");
            return;
        }

        // 为每个文件创建按钮
        foreach (string filePath in jsonFiles)
        {
            CreateFileButton(filePath);
        }
    }

    /// <summary>
    /// 创建文件按钮
    /// </summary>
    private void CreateFileButton(string filePath)
    {
        if (fileButtonPrefab == null || fileListContainer == null)
        {
            Debug.LogError("[ChartFileSelector] fileButtonPrefab or fileListContainer is null!");
            return;
        }

        GameObject buttonObj = Instantiate(fileButtonPrefab, fileListContainer);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            buttonText.text = fileName;
        }

        if (button != null)
        {
            button.onClick.AddListener(() => OnFileButtonClicked(filePath));
        }
    }

    /// <summary>
    /// 创建信息按钮（不可点击）
    /// </summary>
    private void CreateInfoButton(string message)
    {
        if (fileButtonPrefab == null || fileListContainer == null)
        {
            return;
        }

        GameObject buttonObj = Instantiate(fileButtonPrefab, fileListContainer);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

        if (buttonText != null)
        {
            buttonText.text = message;
        }

        if (button != null)
        {
            button.interactable = false;
        }
    }

    /// <summary>
    /// 文件按钮点击
    /// </summary>
    private void OnFileButtonClicked(string filePath)
    {
        HideSelector();
        onFileSelected?.Invoke(filePath);
    }

    /// <summary>
    /// 关闭按钮点击
    /// </summary>
    private void OnCloseClicked()
    {
        HideSelector();
        onCancelled?.Invoke();
    }
}
