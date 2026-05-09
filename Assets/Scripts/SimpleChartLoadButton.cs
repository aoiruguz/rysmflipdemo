using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 简单的外部谱面加载测试按钮
/// 直接加载StreamingAssets/Charts中的第一个谱面
/// </summary>
public class SimpleChartLoadButton : MonoBehaviour
{
    public Button loadButton;

    void Start()
    {
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClicked);
        }
    }

    void OnLoadButtonClicked()
    {
        Debug.Log("[SimpleChartLoadButton] Load button clicked");

        // 确保ExternalChartLoader存在
        if (ExternalChartLoader.Instance == null)
        {
            GameObject loaderObj = new GameObject("ExternalChartLoader");
            loaderObj.AddComponent<ExternalChartLoader>();
            Debug.Log("[SimpleChartLoadButton] Created ExternalChartLoader");
        }

        // 获取可用的谱面
        string[] charts = ExternalChartLoader.Instance.GetAvailableCharts();
        Debug.Log($"[SimpleChartLoadButton] Found {charts.Length} charts");

        if (charts.Length == 0)
        {
            Debug.LogError("[SimpleChartLoadButton] No charts found in StreamingAssets/Charts");
            return;
        }

        // 加载第一个谱面
        string firstChart = charts[0];
        Debug.Log($"[SimpleChartLoadButton] Loading chart: {firstChart}");

        ExternalChartLoader.Instance.LoadChartFromPath(
            firstChart,
            onChartLoaded: (ChartData chart) =>
            {
                Debug.Log($"[SimpleChartLoadButton] Chart loaded successfully: {chart.songName}");

                // 通知ChartEditorManager
                ChartEditorManager manager = FindFirstObjectByType<ChartEditorManager>();
                if (manager != null)
                {
                    Debug.Log("[SimpleChartLoadButton] Found ChartEditorManager, initializing...");
                    manager.currentChart = chart;
                    // 使用反射调用私有方法
                    var method = manager.GetType().GetMethod("InitializeWithChart",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(manager, null);
                        Debug.Log("[SimpleChartLoadButton] Chart initialized in editor");
                    }
                }
                else
                {
                    Debug.LogError("[SimpleChartLoadButton] ChartEditorManager not found!");
                }
            },
            onError: (string error) =>
            {
                Debug.LogError($"[SimpleChartLoadButton] Failed to load chart: {error}");
            }
        );
    }
}
