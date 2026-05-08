using UnityEngine;
using System.IO;

/// <summary>
/// 外部谱面加载调试工具
/// 用于测试和调试外部谱面加载功能
/// </summary>
public class ExternalChartLoaderDebug : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== ExternalChartLoader Debug Info ===");
        Debug.Log($"Application.streamingAssetsPath: {Application.streamingAssetsPath}");
        Debug.Log($"Application.dataPath: {Application.dataPath}");
        Debug.Log($"Application.persistentDataPath: {Application.persistentDataPath}");

        // 检查Charts目录
        string chartsDirectory = Path.Combine(Application.streamingAssetsPath, "Charts");
        Debug.Log($"Charts directory path: {chartsDirectory}");
        Debug.Log($"Charts directory exists: {Directory.Exists(chartsDirectory)}");

        if (Directory.Exists(chartsDirectory))
        {
            string[] jsonFiles = Directory.GetFiles(chartsDirectory, "*.json");
            Debug.Log($"Found {jsonFiles.Length} JSON files:");
            foreach (string file in jsonFiles)
            {
                Debug.Log($"  - {file}");
            }

            string[] allFiles = Directory.GetFiles(chartsDirectory);
            Debug.Log($"All files in Charts directory ({allFiles.Length}):");
            foreach (string file in allFiles)
            {
                Debug.Log($"  - {file}");
            }
        }
        else
        {
            Debug.LogWarning("Charts directory does not exist!");

            // 尝试创建目录
            try
            {
                Directory.CreateDirectory(chartsDirectory);
                Debug.Log($"Created Charts directory: {chartsDirectory}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create Charts directory: {e.Message}");
            }
        }

        Debug.Log("=== End Debug Info ===");
    }

    void Update()
    {
        // 按F8键重新检查
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Start();
        }
    }
}
