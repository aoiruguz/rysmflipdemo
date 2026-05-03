using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class AddExporterToScene : EditorWindow
{
    [MenuItem("RhythmGame/Add Exporter to SongSelection Scene")]
    public static void AddExporter()
    {
        // Load the SongSelection scene
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/SongSelection.unity");

        // Check if ChartDataExporter already exists
        ChartDataExporter existing = Object.FindFirstObjectByType<ChartDataExporter>();
        if (existing != null)
        {
            Debug.Log("ChartDataExporter already exists in the scene!");
            EditorUtility.DisplayDialog("提示", "ChartDataExporter 已经存在于场景中", "确定");
            return;
        }

        // Create a new GameObject with ChartDataExporter
        GameObject exporterObj = new GameObject("ChartDataExporter");
        exporterObj.AddComponent<ChartDataExporter>();

        // Save the scene
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("✓ Added ChartDataExporter to SongSelection scene");
        EditorUtility.DisplayDialog("完成", "已添加 ChartDataExporter 到 SongSelection 场景\n\n现在可以重新打包游戏，运行后按 F9 导出谱面数据", "确定");
    }
}
