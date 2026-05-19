using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 设置按钮控制器
/// 用于打开设置面板
/// </summary>
public class SettingsButtonController : MonoBehaviour
{
    [Header("设置面板引用")]
    public SettingsUIManager settingsPanel;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OpenSettings);
        }
        else
        {
            Debug.LogError("[SettingsButton] 未找到 Button 组件！");
        }
    }

    private void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.Open();
        }
        else
        {
            Debug.LogError("[SettingsButton] 未设置 SettingsPanel 引用！");
        }
    }
}
