using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class SetupExporterInEditor : EditorWindow
{
    [MenuItem("RhythmGame/Setup Chart Exporter (Play Mode)")]
    public static void Setup()
    {
        // 在当前活动场景中添加 ChartDataExporter
        Scene activeScene = SceneManager.GetActiveScene();

        // 检查是否已存在
        ChartDataExporter existing = Object.FindFirstObjectByType<ChartDataExporter>();
        if (existing != null)
        {
            Debug.Log("ChartDataExporter 已经存在!");
            EditorUtility.DisplayDialog("提示", "ChartDataExporter 已经存在于场景中\n\n现在可以进入 Play 模式，然后按 F9 导出谱面", "确定");
            return;
        }

        // 创建临时对象
        GameObject exporterObj = new GameObject("_ChartDataExporter_Temp");
        exporterObj.AddComponent<ChartDataExporter>();

        Debug.Log("✓ 已添加 ChartDataExporter (临时对象，不会保存到场景)");
        EditorUtility.DisplayDialog(
            "完成",
            "已添加 ChartDataExporter 到当前场景\n\n" +
            "下一步:\n" +
            "1. 点击 Play 按钮进入播放模式\n" +
            "2. 按 F9 键导出谱面数据\n" +
            "3. 会自动打开包含 JSON 文件的文件夹",
            "确定"
        );
    }
}
