using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

public class OsuToChartConverter : EditorWindow
{
    private float noteTravelTime = 2.0f; // Seconds
    private string customDirPath = "Assets/Charts/tu";
    private string singleFilePath = ""; // 单个文件路径
    private ChartDifficulty targetDifficulty = ChartDifficulty.Hard; // 目标难度

    [MenuItem("RhythmGame/Osu Converter")]
    public static void ShowWindow()
    {
        GetWindow<OsuToChartConverter>("Osu to Chart");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("=== 单个文件转换 ===", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        singleFilePath = EditorGUILayout.TextField("Osu File Path", singleFilePath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Select .osu file", "Assets/Charts", "osu");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert absolute path to relative path
                if (path.StartsWith(Application.dataPath))
                {
                    singleFilePath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    singleFilePath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        targetDifficulty = (ChartDifficulty)EditorGUILayout.EnumPopup("Target Difficulty", targetDifficulty);
        EditorGUILayout.HelpBox(
            "Easy: 单色 (仅红色)\n" +
            "Normal: 双色 (红色 + 蓝色)\n" +
            "Hard: 三色 (红色 + 绿色 + 蓝色)",
            MessageType.Info);

        noteTravelTime = EditorGUILayout.FloatField("Note Travel Time (s)", noteTravelTime);

        if (GUILayout.Button("Convert Single File", GUILayout.Height(30)))
        {
            ConvertSingleFile(singleFilePath, targetDifficulty, noteTravelTime);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== 文件夹转换 ===", EditorStyles.boldLabel);
        customDirPath = EditorGUILayout.TextField("Chart Directory", customDirPath);

        if (GUILayout.Button("Convert Osu in Custom Directory", GUILayout.Height(30)))
        {
            ConvertSingleChart(customDirPath, targetDifficulty, noteTravelTime);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== 批量转换 ===", EditorStyles.boldLabel);

        if (GUILayout.Button("Batch Convert All Folders (Assets/Charts/*)", GUILayout.Height(30)))
        {
            BatchConvertAllFolders(targetDifficulty, noteTravelTime);
        }
    }

    [MenuItem("RhythmGame/Batch Convert All Charts")]
    public static void BatchConvertAllFoldersMenu()
    {
        BatchConvertAllFolders(ChartDifficulty.Hard, 2.0f);
    }

    /// <summary>
    /// 转换单个 .osu 文件
    /// </summary>
    private static void ConvertSingleFile(string osuPath, ChartDifficulty difficulty, float travelTime)
    {
        if (string.IsNullOrEmpty(osuPath))
        {
            EditorUtility.DisplayDialog("错误", "请选择一个 .osu 文件", "确定");
            return;
        }

        if (!File.Exists(osuPath))
        {
            EditorUtility.DisplayDialog("错误", $"文件不存在: {osuPath}", "确定");
            return;
        }

        string dirPath = Path.GetDirectoryName(osuPath);
        ConvertSingleOsuFile(osuPath, dirPath, difficulty, travelTime);
        EditorUtility.DisplayDialog("转换完成", $"已转换: {Path.GetFileName(osuPath)}\n难度: {difficulty}", "确定");
    }

    /// <summary>
    /// 单个文件夹的转换
    /// </summary>
    private static void ConvertSingleChart(string dirPath, ChartDifficulty difficulty, float travelTime)
    {
        if (!Directory.Exists(dirPath))
        {
            Debug.LogError("Directory not found: " + dirPath);
            return;
        }

        string[] files = Directory.GetFiles(dirPath, "*.osu");
        if (files.Length == 0)
        {
            Debug.LogError("No .osu files found in " + dirPath);
            return;
        }

        // Convert all .osu files in the directory
        foreach (string osuPath in files)
        {
            ConvertSingleOsuFile(osuPath, dirPath, difficulty, travelTime);
        }
    }

    /// <summary>
    /// 转换单个 .osu 文件
    /// </summary>
    private static void ConvertSingleOsuFile(string osuPath, string dirPath, ChartDifficulty difficulty, float travelTime)
    {
        string[] lines = File.ReadAllLines(osuPath);

        ChartData chart = ScriptableObject.CreateInstance<ChartData>();
        chart.songName = Path.GetFileNameWithoutExtension(osuPath);
        chart.difficulty = difficulty; // 设置难度


        float sliderMultiplier = 1.0f;
        float baseBeatDuration = 500f;

        bool inDifficulty = false;
        bool inTimingPoints = false;
        bool inHitObjects = false;

        int currentLane = 1;
        int laneDirection = 1;
        GameColor currentColor = GameColor.ColorA;

        // 根据难度确定可用颜色数量
        int maxColorIndex = GetMaxColorIndex(difficulty);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string trimmed = line.Trim();

            if (trimmed == "[Difficulty]") { inDifficulty = true; inTimingPoints = false; inHitObjects = false; continue; }
            if (trimmed == "[TimingPoints]") { inDifficulty = false; inTimingPoints = true; inHitObjects = false; continue; }
            if (trimmed == "[HitObjects]") { inDifficulty = false; inTimingPoints = false; inHitObjects = true; continue; }
            if (trimmed.StartsWith("[")) { inDifficulty = false; inTimingPoints = false; inHitObjects = false; continue; }

            if (inDifficulty)
            {
                if (trimmed.StartsWith("SliderMultiplier:"))
                    float.TryParse(trimmed.Split(':')[1], out sliderMultiplier);
            }

            // 方案3: 自动查找音频文件
            if (trimmed.StartsWith("AudioFilename:"))
            {
                string audioFile = trimmed.Split(':')[1].Trim();
                string audioPath = SearchAudioFile(dirPath, audioFile);
                if (!string.IsNullOrEmpty(audioPath))
                {
                    chart.audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath);
                    if (chart.audioClip != null)
                        Debug.Log($"Found audio at: {audioPath}");
                    else
                        Debug.LogWarning("Failed to load audio at: " + audioPath);
                }
                else
                {
                    Debug.LogWarning($"Audio file not found: {audioFile}");
                }
            }

            if (inTimingPoints)
            {
                // Simple: take the first timing point's beat duration
                string[] parts = trimmed.Split(',');
                if (parts.Length >= 2)
                {
                    if (float.TryParse(parts[1], out float val) && val > 0)
                    {
                        baseBeatDuration = val;
                        inTimingPoints = false; // Just take the first one for simplicity
                    }
                }
            }

            if (inHitObjects)
            {
                string[] parts = trimmed.Split(',');
                if (parts.Length < 4) continue;

                float hitTime = float.Parse(parts[2]);
                int type = int.Parse(parts[3]);

                // Check for New Combo (Bit 2 = 4)
                if ((type & 4) != 0)
                {
                    // Move to adjacent lane
                    currentLane += laneDirection;
                    if (currentLane < 0 || currentLane > 3)
                    {
                        laneDirection *= -1;
                        currentLane += laneDirection * 2;
                    }
                    currentLane = Mathf.Clamp(currentLane, 0, 3);

                    // Switch to a different color based on difficulty
                    GameColor nextColor;
                    do {
                        nextColor = GetRandomColor(difficulty, maxColorIndex);
                    }
                    while (nextColor == currentColor && maxColorIndex > 0); // 避免连续相同颜色（除非只有一种颜色）
                    currentColor = nextColor;
                }

                // Check for Slider (Bit 1 = 2)
                if ((type & 2) != 0 && parts.Length >= 8)
                {
                    float slides = float.Parse(parts[6]); // 滑条重复次数（包括初始滑动）
                    float length = float.Parse(parts[7]);

                    // 计算单次滑动的时长
                    float singleSlideDuration = length / (sliderMultiplier * 100f) * baseBeatDuration;

                    // 添加slider的所有踩音点
                    // slides = 1: 只有起点和终点 (起点 -> 终点)
                    // slides = 2: 起点、终点、折返回起点 (起点 -> 终点 -> 起点)
                    // slides = 3: 起点、终点、折返回起点、再到终点 (起点 -> 终点 -> 起点 -> 终点)

                    for (int i = 0; i <= slides; i++)
                    {
                        float noteTime = hitTime + (i * singleSlideDuration);
                        AddNote(chart, noteTime, 0, currentLane, currentColor, travelTime);
                    }
                }
                else
                {
                    // 普通圆圈note
                    AddNote(chart, hitTime, 0, currentLane, currentColor, travelTime);
                }
            }
        }

        // Generate unique filename with difficulty suffix
        string baseName = chart.songName;
        string difficultySuffix = $"_{difficulty}";
        string savePath = Path.Combine(dirPath, baseName + difficultySuffix + ".asset").Replace("\\", "/");

        int counter = 1;
        while (File.Exists(savePath))
        {
            string newName = $"{baseName}{difficultySuffix}_converted_{counter}";
            savePath = Path.Combine(dirPath, newName + ".asset").Replace("\\", "/");
            counter++;
        }

        AssetDatabase.CreateAsset(chart, savePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (counter > 1)
        {
            Debug.Log($"✓ Converted: {osuPath} → {savePath} (renamed to avoid overwrite) [Difficulty: {difficulty}]");
        }
        else
        {
            Debug.Log($"✓ Converted: {osuPath} → {savePath} [Difficulty: {difficulty}]");
        }
    }

    /// <summary>
    /// 根据难度获取最大颜色索引
    /// Easy: 0 (只有 ColorA)
    /// Normal: 2 (ColorA 和 ColorC)
    /// Hard: 2 (ColorA, ColorB, ColorC)
    /// </summary>
    private static int GetMaxColorIndex(ChartDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ChartDifficulty.Easy:
                return 0; // 只有一种颜色
            case ChartDifficulty.Normal:
                return 2; // 两种颜色 (0 和 2)
            case ChartDifficulty.Hard:
                return 2; // 三种颜色 (0, 1, 2)
            default:
                return 2;
        }
    }

    /// <summary>
    /// 根据难度随机生成颜色
    /// Easy: 只返回 ColorA (红色)
    /// Normal: 返回 ColorA (红色) 或 ColorC (蓝色)
    /// Hard: 返回 ColorA, ColorB, ColorC
    /// </summary>
    private static GameColor GetRandomColor(ChartDifficulty difficulty, int maxColorIndex)
    {
        switch (difficulty)
        {
            case ChartDifficulty.Easy:
                return GameColor.ColorA; // 只有红色

            case ChartDifficulty.Normal:
                // 50% 红色, 50% 蓝色
                return UnityEngine.Random.value < 0.5f ? GameColor.ColorA : GameColor.ColorC;

            case ChartDifficulty.Hard:
                // 随机三种颜色
                return (GameColor)UnityEngine.Random.Range(0, 3);

            default:
                return GameColor.ColorA;
        }
    }

    /// <summary>
    /// 方案2: 批量转换 Assets/Charts 下所有文件夹中的 .osu 文件
    /// </summary>
    private static void BatchConvertAllFolders(ChartDifficulty difficulty, float travelTime)
    {
        string baseDir = "Assets/Charts";
        if (!Directory.Exists(baseDir))
        {
            Debug.LogError("Directory not found: " + baseDir);
            return;
        }

        string[] folders = Directory.GetDirectories(baseDir);
        if (folders.Length == 0)
        {
            Debug.LogWarning("No subfolders found in Assets/Charts");
            return;
        }

        int convertedCount = 0;
        foreach (string folder in folders)
        {
            string[] osuFiles = Directory.GetFiles(folder, "*.osu");
            if (osuFiles.Length > 0)
            {
                Debug.Log($"Converting folder: {Path.GetFileName(folder)} ({osuFiles.Length} files) [Difficulty: {difficulty}]");
                ConvertSingleChart(folder, difficulty, travelTime);
                convertedCount += osuFiles.Length;
            }
        }

        Debug.Log($"\n=== 批量转换完成! 共转换 {convertedCount} 个谱面 [Difficulty: {difficulty}] ===");
        EditorUtility.DisplayDialog("批量转换", $"完成转换 {convertedCount} 个谱面\n难度: {difficulty}", "确定");
    }

    /// <summary>
    /// 方案3: 自动查找音频文件
    /// 策略: 同级目录 → 父目录 → 递归查找
    /// </summary>
    private static string SearchAudioFile(string dirPath, string fileName)
    {
        // 策略1: 同级目录
        string directPath = Path.Combine(dirPath, fileName).Replace("\\", "/");
        if (File.Exists(directPath) && IsAudioFile(directPath))
        {
            return directPath;
        }

        // 策略2: 支持不同的音频扩展名 (如果文件名没有扩展名)
        if (!Path.HasExtension(fileName))
        {
            string[] audioExtensions = { ".mp3", ".wav", ".ogg", ".flac" };
            foreach (string ext in audioExtensions)
            {
                string pathWithExt = Path.Combine(dirPath, fileName + ext).Replace("\\", "/");
                if (File.Exists(pathWithExt))
                {
                    return pathWithExt;
                }
            }
        }

        // 策略3: 父目录查找 (用于一个文件夹一个曲目的场景)
        string parentDir = Path.GetDirectoryName(dirPath);
        if (parentDir != null && parentDir != dirPath)
        {
            string parentPath = Path.Combine(parentDir, fileName).Replace("\\", "/");
            if (File.Exists(parentPath) && IsAudioFile(parentPath))
            {
                return parentPath;
            }
        }

        return null; // 未找到
    }

    private static bool IsAudioFile(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLower();
        return ext == ".mp3" || ext == ".wav" || ext == ".ogg" || ext == ".flac" || ext == ".aiff";
    }

    private static void AddNote(ChartData chart, float hitTime, float duration, int lane, GameColor color, float travelTime)
    {
        chart.notes.Add(new NoteData 
        { 
            hitTime = hitTime, 
            spawnTime = hitTime - (travelTime * 1000f),
            duration = duration, 
            lane = lane, 
            color = color 
        });
    }
}
