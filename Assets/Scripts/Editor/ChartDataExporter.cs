using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// ChartData导出工具
/// 将ScriptableObject格式的ChartData转换为JSON文件
/// </summary>
public class ChartDataExporterEditor : Editor
{
    /// <summary>
    /// 右键菜单：导出单个ChartData为JSON
    /// </summary>
    [MenuItem("Assets/Export Chart to JSON", false, 2000)]
    private static void ExportChartToJSON()
    {
        // 获取选中的ChartData
        ChartData chartData = Selection.activeObject as ChartData;
        if (chartData == null)
        {
            EditorUtility.DisplayDialog("错误", "请选择一个ChartData文件", "确定");
            return;
        }

        // 打开保存对话框
        string assetPath = AssetDatabase.GetAssetPath(chartData);
        string directory = Path.GetDirectoryName(assetPath);
        string defaultName = chartData.songName + "_" + chartData.difficulty.ToString() + ".json";

        string savePath = EditorUtility.SaveFilePanel(
            "导出谱面为JSON",
            directory,
            defaultName,
            "json"
        );

        if (string.IsNullOrEmpty(savePath))
        {
            return; // 用户取消
        }

        // 转换为JSON
        ChartDataJson chartJson = ChartDataJson.FromChartData(chartData);

        // 设置音频文件名（相对路径）
        if (chartData.audioClip != null)
        {
            string audioPath = AssetDatabase.GetAssetPath(chartData.audioClip);
            string audioFileName = Path.GetFileNameWithoutExtension(audioPath);
            string audioExtension = Path.GetExtension(audioPath);
            chartJson.audioFileName = audioFileName + audioExtension;
        }

        // 保存JSON文件
        string jsonContent = JsonUtility.ToJson(chartJson, true);
        File.WriteAllText(savePath, jsonContent);

        Debug.Log($"[ChartDataExporterEditor] 谱面已导出: {savePath}");
        EditorUtility.DisplayDialog("成功", $"谱面已导出到:\n{savePath}", "确定");

        // 提示复制音频文件
        if (chartData.audioClip != null)
        {
            string audioSourcePath = AssetDatabase.GetAssetPath(chartData.audioClip);
            string audioSourceFullPath = Path.GetFullPath(audioSourcePath);
            string jsonDirectory = Path.GetDirectoryName(savePath);
            string audioDestPath = Path.Combine(jsonDirectory, chartJson.audioFileName);

            bool copyAudio = EditorUtility.DisplayDialog(
                "复制音频文件？",
                $"是否将音频文件复制到JSON文件所在目录？\n\n源文件: {audioSourceFullPath}\n目标: {audioDestPath}",
                "是",
                "否"
            );

            if (copyAudio)
            {
                try
                {
                    File.Copy(audioSourceFullPath, audioDestPath, true);
                    Debug.Log($"[ChartDataExporterEditor] 音频文件已复制: {audioDestPath}");
                    EditorUtility.DisplayDialog("成功", "音频文件已复制", "确定");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ChartDataExporterEditor] 复制音频文件失败: {e.Message}");
                    EditorUtility.DisplayDialog("错误", $"复制音频文件失败:\n{e.Message}", "确定");
                }
            }
        }
    }

    /// <summary>
    /// 验证菜单项是否可用（只有选中ChartData时才显示）
    /// </summary>
    [MenuItem("Assets/Export Chart to JSON", true)]
    private static bool ValidateExportChartToJSON()
    {
        return Selection.activeObject is ChartData;
    }

    /// <summary>
    /// 批量导出所有ChartData
    /// </summary>
    [MenuItem("Tools/Chart Editor/Batch Export All Charts to JSON")]
    private static void BatchExportAllCharts()
    {
        // 查找所有ChartData
        string[] guids = AssetDatabase.FindAssets("t:ChartData");

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有找到任何ChartData文件", "确定");
            return;
        }

        // 选择导出目录
        string exportDirectory = EditorUtility.OpenFolderPanel("选择导出目录", "", "");
        if (string.IsNullOrEmpty(exportDirectory))
        {
            return; // 用户取消
        }

        int exportedCount = 0;
        int failedCount = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ChartData chartData = AssetDatabase.LoadAssetAtPath<ChartData>(assetPath);

            if (chartData == null)
            {
                failedCount++;
                continue;
            }

            try
            {
                // 转换为JSON
                ChartDataJson chartJson = ChartDataJson.FromChartData(chartData);

                // 设置音频文件名
                if (chartData.audioClip != null)
                {
                    string audioPath = AssetDatabase.GetAssetPath(chartData.audioClip);
                    string audioFileName = Path.GetFileNameWithoutExtension(audioPath);
                    string audioExtension = Path.GetExtension(audioPath);
                    chartJson.audioFileName = audioFileName + audioExtension;
                }

                // 保存JSON
                string fileName = chartData.songName + "_" + chartData.difficulty.ToString() + ".json";
                string savePath = Path.Combine(exportDirectory, fileName);
                string jsonContent = JsonUtility.ToJson(chartJson, true);
                File.WriteAllText(savePath, jsonContent);

                exportedCount++;
                Debug.Log($"[ChartDataExporterEditor] 已导出: {fileName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ChartDataExporterEditor] 导出失败 {chartData.name}: {e.Message}");
                failedCount++;
            }
        }

        EditorUtility.DisplayDialog(
            "批量导出完成",
            $"成功导出: {exportedCount} 个谱面\n失败: {failedCount} 个",
            "确定"
        );
    }

    /// <summary>
    /// 导出谱面和音频到指定文件夹
    /// </summary>
    [MenuItem("Assets/Export Chart with Audio", false, 2001)]
    private static void ExportChartWithAudio()
    {
        ChartData chartData = Selection.activeObject as ChartData;
        if (chartData == null)
        {
            EditorUtility.DisplayDialog("错误", "请选择一个ChartData文件", "确定");
            return;
        }

        // 选择导出文件夹
        string exportDirectory = EditorUtility.OpenFolderPanel("选择导出文件夹", "", "");
        if (string.IsNullOrEmpty(exportDirectory))
        {
            return;
        }

        // 创建子文件夹
        string chartFolderName = chartData.songName.Replace(" ", "_");
        string chartFolder = Path.Combine(exportDirectory, chartFolderName);
        Directory.CreateDirectory(chartFolder);

        // 导出JSON
        ChartDataJson chartJson = ChartDataJson.FromChartData(chartData);

        if (chartData.audioClip != null)
        {
            string audioPath = AssetDatabase.GetAssetPath(chartData.audioClip);
            string audioFileName = Path.GetFileName(audioPath);
            chartJson.audioFileName = audioFileName;

            // 复制音频文件
            string audioSourcePath = Path.GetFullPath(audioPath);
            string audioDestPath = Path.Combine(chartFolder, audioFileName);
            File.Copy(audioSourcePath, audioDestPath, true);
            Debug.Log($"[ChartDataExporterEditor] 音频已复制: {audioFileName}");
        }

        // 保存JSON
        string jsonFileName = "chart.json";
        string jsonPath = Path.Combine(chartFolder, jsonFileName);
        string jsonContent = JsonUtility.ToJson(chartJson, true);
        File.WriteAllText(jsonPath, jsonContent);

        Debug.Log($"[ChartDataExporterEditor] 谱面包已导出到: {chartFolder}");
        EditorUtility.DisplayDialog("成功", $"谱面包已导出到:\n{chartFolder}", "确定");

        // 打开文件夹
        EditorUtility.RevealInFinder(jsonPath);
    }

    [MenuItem("Assets/Export Chart with Audio", true)]
    private static bool ValidateExportChartWithAudio()
    {
        return Selection.activeObject is ChartData;
    }
}
