using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ChartDataExtractor : EditorWindow
{
    private string builtGamePath = @"C:\Users\友子\Desktop\弹球0.0001\rysmflipdemo.exe";

    [MenuItem("RhythmGame/Extract Charts from Build")]
    public static void ShowWindow()
    {
        GetWindow<ChartDataExtractor>("Chart Extractor");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("从打包项目中提取谱面数据", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        builtGamePath = EditorGUILayout.TextField("Built Game Path", builtGamePath);

        EditorGUILayout.HelpBox(
            "这个工具会尝试从打包的游戏中提取 ChartData 资源。\n" +
            "注意：这需要资源文件没有被加密或压缩。",
            MessageType.Info
        );

        if (GUILayout.Button("Extract All Charts", GUILayout.Height(40)))
        {
            ExtractCharts();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("List All ChartData in Project", GUILayout.Height(30)))
        {
            ListAllCharts();
        }
    }

    private void ExtractCharts()
    {
        if (!File.Exists(builtGamePath))
        {
            EditorUtility.DisplayDialog("错误", "找不到游戏文件: " + builtGamePath, "确定");
            return;
        }

        string dataPath = Path.GetDirectoryName(builtGamePath) + "/" +
                         Path.GetFileNameWithoutExtension(builtGamePath) + "_Data";

        if (!Directory.Exists(dataPath))
        {
            EditorUtility.DisplayDialog("错误", "找不到 Data 文件夹: " + dataPath, "确定");
            return;
        }

        Debug.Log("Data 文件夹: " + dataPath);

        // 方法1: 尝试从 Resources 文件夹加载（如果谱面在 Resources 中）
        // 方法2: 使用 AssetBundle 提取（如果使用了 AssetBundle）
        // 方法3: 手动解析 .assets 文件（复杂）

        EditorUtility.DisplayDialog(
            "提示",
            "Unity 打包后的资源文件是二进制格式，需要使用专门的工具提取。\n\n" +
            "推荐方案：\n" +
            "1. 使用 AssetRipper (免费开源)\n" +
            "2. 使用 UABE (Unity Assets Bundle Extractor)\n\n" +
            "或者，如果你记得谱面的数据，我可以帮你重新创建。",
            "确定"
        );
    }

    private void ListAllCharts()
    {
        string[] guids = AssetDatabase.FindAssets("t:ChartData");

        Debug.Log($"=== 找到 {guids.Length} 个 ChartData 资源 ===");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChartData chart = AssetDatabase.LoadAssetAtPath<ChartData>(path);

            if (chart != null)
            {
                Debug.Log($"[{path}] {chart.songName} - {chart.notes.Count} notes");
            }
        }
    }
}
