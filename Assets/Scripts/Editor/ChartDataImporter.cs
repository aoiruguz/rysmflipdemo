using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ChartDataImporter : EditorWindow
{
    private string jsonFolderPath = "";

    [MenuItem("RhythmGame/Import Charts from JSON")]
    public static void ShowWindow()
    {
        GetWindow<ChartDataImporter>("Chart Importer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("从 JSON 导入谱面数据", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "步骤:\n" +
            "1. 运行打包的游戏 (C:\\Users\\友子\\Desktop\\弹球0.0001\\rysmflipdemo.exe)\n" +
            "2. 在游戏中按 F9 键导出谱面数据\n" +
            "3. 复制导出路径，粘贴到下方\n" +
            "4. 点击 Import 按钮",
            MessageType.Info
        );

        EditorGUILayout.Space();

        jsonFolderPath = EditorGUILayout.TextField("JSON Folder Path", jsonFolderPath);

        if (GUILayout.Button("Browse...", GUILayout.Width(100)))
        {
            string path = EditorUtility.OpenFolderPanel("选择 JSON 文件夹", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                jsonFolderPath = path;
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Import All Charts from JSON", GUILayout.Height(40)))
        {
            ImportCharts();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Open Persistent Data Path", GUILayout.Height(30)))
        {
            string path = Application.persistentDataPath;
            EditorUtility.RevealInFinder(path);
            Debug.Log("Persistent Data Path: " + path);
        }
    }

    private void ImportCharts()
    {
        if (string.IsNullOrEmpty(jsonFolderPath) || !Directory.Exists(jsonFolderPath))
        {
            EditorUtility.DisplayDialog("错误", "请选择有效的文件夹路径", "确定");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(jsonFolderPath, "*.json");

        if (jsonFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "文件夹中没有找到 JSON 文件", "确定");
            return;
        }

        int importedCount = 0;
        foreach (string jsonPath in jsonFiles)
        {
            try
            {
                string json = File.ReadAllText(jsonPath);
                ChartBackupData backup = JsonUtility.FromJson<ChartBackupData>(json);

                if (backup == null || string.IsNullOrEmpty(backup.songName))
                {
                    Debug.LogWarning($"跳过无效文件: {Path.GetFileName(jsonPath)}");
                    continue;
                }

                // 创建 ChartData
                ChartData chart = ScriptableObject.CreateInstance<ChartData>();
                chart.songName = backup.songName;

                // 尝试查找音频文件
                if (!string.IsNullOrEmpty(backup.audioClipName))
                {
                    string[] audioGuids = AssetDatabase.FindAssets(backup.audioClipName + " t:AudioClip");
                    if (audioGuids.Length > 0)
                    {
                        string audioPath = AssetDatabase.GUIDToAssetPath(audioGuids[0]);
                        chart.audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                    }
                }

                // 导入 notes
                chart.notes = new List<NoteData>();
                foreach (NoteBackupData noteBackup in backup.notes)
                {
                    chart.notes.Add(new NoteData
                    {
                        hitTime = noteBackup.hitTime,
                        spawnTime = noteBackup.spawnTime,
                        duration = noteBackup.duration,
                        lane = noteBackup.lane,
                        color = (GameColor)noteBackup.color
                    });
                }

                // 保存为 .asset 文件
                string savePath = $"Assets/Charts/Recovered/{SanitizeFileName(backup.songName)}.asset";
                string saveDir = Path.GetDirectoryName(savePath);

                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }

                AssetDatabase.CreateAsset(chart, savePath);
                importedCount++;

                Debug.Log($"✓ 导入: {backup.songName} ({chart.notes.Count} notes) → {savePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"导入失败 {Path.GetFileName(jsonPath)}: {e.Message}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完成", $"成功导入 {importedCount} 个谱面到 Assets/Charts/Recovered/", "确定");
    }

    string SanitizeFileName(string name)
    {
        char[] invalids = Path.GetInvalidFileNameChars();
        foreach (char c in invalids)
        {
            name = name.Replace(c, '_');
        }
        return name;
    }
}
