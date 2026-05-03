using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 在运行时导出所有 ChartData 到 JSON 文件
/// 按 F9 键触发导出
/// </summary>
public class ChartDataExporter : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            ExportAllCharts();
        }
    }

    void ExportAllCharts()
    {
        List<ChartData> chartsList = new List<ChartData>();

        // 方法1: 从 SongSelectionManager 获取
        SongSelectionManager songManager = FindObjectOfType<SongSelectionManager>();
        if (songManager != null && songManager.songs != null)
        {
            foreach (SongData song in songManager.songs)
            {
                if (song.easyChart != null) chartsList.Add(song.easyChart);
                if (song.normalChart != null) chartsList.Add(song.normalChart);
                if (song.hardChart != null) chartsList.Add(song.hardChart);
            }
            Debug.Log($"从 SongSelectionManager 找到 {chartsList.Count} 个谱面");
        }

        // 方法2: 查找所有 ChartData 资源（备用）
        if (chartsList.Count == 0)
        {
            var foundCharts = Resources.FindObjectsOfTypeAll<ChartData>();
            chartsList.AddRange(foundCharts);
            Debug.Log($"从 Resources 找到 {chartsList.Count} 个谱面");
        }

        if (chartsList.Count == 0)
        {
            Debug.LogWarning("没有找到任何 ChartData!");
            return;
        }

        ChartData[] charts = chartsList.ToArray();

        string exportPath = Path.Combine(Application.persistentDataPath, "ChartBackup");
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        int exportedCount = 0;
        foreach (ChartData chart in charts)
        {
            if (chart == null || string.IsNullOrEmpty(chart.songName)) continue;

            ChartBackupData backup = new ChartBackupData
            {
                songName = chart.songName,
                audioClipName = chart.audioClip != null ? chart.audioClip.name : "",
                notes = new List<NoteBackupData>()
            };

            // 自动检测换轨道并生成方向note
            int previousLane = -1;
            foreach (NoteData note in chart.notes)
            {
                NoteType noteType = note.noteType;

                // 如果是三色note且发生了换轨道，自动改为方向note
                if (noteType == NoteType.Color && previousLane != -1 && note.lane != previousLane)
                {
                    // 根据换轨道方向决定方向note类型
                    if (note.lane < previousLane)
                    {
                        noteType = NoteType.DirectionalLeft;
                    }
                    else if (note.lane > previousLane)
                    {
                        noteType = NoteType.DirectionalRight;
                    }
                }

                backup.notes.Add(new NoteBackupData
                {
                    hitTime = note.hitTime,
                    spawnTime = note.spawnTime,
                    duration = note.duration,
                    lane = note.lane,
                    color = (int)note.color,
                    noteType = (int)noteType
                });

                previousLane = note.lane;
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

        // 在 Windows 上打开文件夹
        #if UNITY_STANDALONE_WIN
        System.Diagnostics.Process.Start("explorer.exe", exportPath.Replace("/", "\\"));
        #endif
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

[System.Serializable]
public class ChartBackupData
{
    public string songName;
    public string audioClipName;
    public List<NoteBackupData> notes;
}

[System.Serializable]
public class NoteBackupData
{
    public float hitTime;
    public float spawnTime;
    public float duration;
    public int lane;
    public int color;
    public int noteType; // 0=Color, 1=DirectionalLeft, 2=DirectionalRight
}
