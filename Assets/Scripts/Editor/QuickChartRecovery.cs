using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class QuickChartRecovery : EditorWindow
{
    [MenuItem("RhythmGame/Quick Recovery - Export Current Charts to JSON")]
    public static void ExportCurrentCharts()
    {
        // 查找所有 ChartData
        string[] guids = AssetDatabase.FindAssets("t:ChartData");

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "当前项目中没有找到任何 ChartData", "确定");
            return;
        }

        string exportPath = Path.Combine(Application.dataPath, "../ChartBackup_Current");
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        int exportedCount = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChartData chart = AssetDatabase.LoadAssetAtPath<ChartData>(path);

            if (chart == null || string.IsNullOrEmpty(chart.songName)) continue;

            ChartBackupData backup = new ChartBackupData
            {
                songName = chart.songName,
                audioClipName = chart.audioClip != null ? chart.audioClip.name : "",
                notes = new List<NoteBackupData>()
            };

            foreach (NoteData note in chart.notes)
            {
                backup.notes.Add(new NoteBackupData
                {
                    hitTime = note.hitTime,
                    spawnTime = note.spawnTime,
                    duration = note.duration,
                    lane = note.lane,
                    color = (int)note.color
                });
            }

            string json = JsonUtility.ToJson(backup, true);
            string fileName = SanitizeFileName(chart.songName) + ".json";
            string filePath = Path.Combine(exportPath, fileName);

            File.WriteAllText(filePath, json);
            exportedCount++;

            Debug.Log($"导出: {fileName} ({backup.notes.Count} notes)");
        }

        Debug.Log($"=== 导出完成! 共 {exportedCount} 个谱面 ===");
        Debug.Log($"保存位置: {exportPath}");

        EditorUtility.RevealInFinder(exportPath);
        EditorUtility.DisplayDialog("完成", $"已导出当前项目中的 {exportedCount} 个谱面到:\n{exportPath}", "确定");
    }

    static string SanitizeFileName(string name)
    {
        char[] invalids = Path.GetInvalidFileNameChars();
        foreach (char c in invalids)
        {
            name = name.Replace(c, '_');
        }
        return name;
    }
}
