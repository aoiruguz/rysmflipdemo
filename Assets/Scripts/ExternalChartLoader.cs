using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 外部谱面加载器
/// 支持从外部文件夹加载JSON格式的谱面和音频文件
/// </summary>
public class ExternalChartLoader : MonoBehaviour
{
    public static ExternalChartLoader Instance { get; private set; }

    private ChartData loadedChart;
    private string lastLoadedDirectory;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 打开文件浏览器选择谱面JSON文件
    /// </summary>
    public void OpenFileBrowser(Action<ChartData> onChartLoaded, Action<string> onError)
    {
#if UNITY_EDITOR
        // Editor模式下使用Unity的文件对话框
        string filePath = UnityEditor.EditorUtility.OpenFilePanel("选择谱面文件", "", "json");
        if (!string.IsNullOrEmpty(filePath))
        {
            StartCoroutine(LoadChartFromFile(filePath, onChartLoaded, onError));
        }
        else
        {
            onError?.Invoke("未选择文件");
        }
#else
        // Build后使用固定目录
        string chartsDirectory = Path.Combine(Application.streamingAssetsPath, "Charts");
        if (!Directory.Exists(chartsDirectory))
        {
            onError?.Invoke($"找不到Charts目录: {chartsDirectory}\n请将谱面文件放在StreamingAssets/Charts文件夹中");
            return;
        }

        // 列出所有JSON文件
        string[] jsonFiles = Directory.GetFiles(chartsDirectory, "*.json");
        if (jsonFiles.Length == 0)
        {
            onError?.Invoke("Charts目录中没有找到JSON文件");
            return;
        }

        // 加载第一个找到的文件（简单实现）
        // TODO: 可以改为显示UI列表让用户选择
        StartCoroutine(LoadChartFromFile(jsonFiles[0], onChartLoaded, onError));
#endif
    }

    /// <summary>
    /// 从指定路径加载谱面（可以在build后使用）
    /// </summary>
    public void LoadChartFromPath(string jsonFilePath, Action<ChartData> onChartLoaded, Action<string> onError)
    {
        StartCoroutine(LoadChartFromFile(jsonFilePath, onChartLoaded, onError));
    }

    /// <summary>
    /// 获取StreamingAssets/Charts目录中的所有谱面文件
    /// </summary>
    public string[] GetAvailableCharts()
    {
        string chartsDirectory = Path.Combine(Application.streamingAssetsPath, "Charts");
        if (!Directory.Exists(chartsDirectory))
        {
            Debug.LogWarning($"[ExternalChartLoader] Charts目录不存在: {chartsDirectory}");
            return new string[0];
        }

        string[] jsonFiles = Directory.GetFiles(chartsDirectory, "*.json");
        return jsonFiles;
    }

    /// <summary>
    /// 从文件加载谱面
    /// </summary>
    private IEnumerator LoadChartFromFile(string jsonFilePath, Action<ChartData> onChartLoaded, Action<string> onError)
    {
        Debug.Log($"[ExternalChartLoader] 开始加载谱面: {jsonFilePath}");

        // 读取JSON文件
        string jsonContent;
        try
        {
            // 检查文件是否存在
            if (!File.Exists(jsonFilePath))
            {
                string error = $"文件不存在: {jsonFilePath}";
                Debug.LogError($"[ExternalChartLoader] {error}");
                onError?.Invoke(error);
                yield break;
            }

            jsonContent = File.ReadAllText(jsonFilePath);
            Debug.Log($"[ExternalChartLoader] JSON内容长度: {jsonContent.Length}");
        }
        catch (Exception e)
        {
            string error = $"读取文件失败: {e.Message}";
            Debug.LogError($"[ExternalChartLoader] {error}");
            onError?.Invoke(error);
            yield break;
        }

        // 解析JSON
        ChartDataJson chartJson;
        try
        {
            chartJson = JsonUtility.FromJson<ChartDataJson>(jsonContent);
            Debug.Log($"[ExternalChartLoader] 解析成功: {chartJson.songName}, Notes: {chartJson.notes.Count}");
        }
        catch (Exception e)
        {
            string error = $"解析JSON失败: {e.Message}";
            Debug.LogError($"[ExternalChartLoader] {error}");
            onError?.Invoke(error);
            yield break;
        }

        // 转换为ChartData
        ChartData chartData = chartJson.ToChartData();
        Debug.Log($"[ExternalChartLoader] 转换为ChartData成功");

        // 保存目录路径
        lastLoadedDirectory = Path.GetDirectoryName(jsonFilePath);

        // 加载音频文件
        if (!string.IsNullOrEmpty(chartJson.audioFileName))
        {
            string audioPath = Path.Combine(lastLoadedDirectory, chartJson.audioFileName);
            Debug.Log($"[ExternalChartLoader] 尝试加载音频: {audioPath}");

            // 尝试不同的音频格式
            string[] extensions = { "", ".mp3", ".wav", ".ogg" };
            string foundAudioPath = null;

            foreach (string ext in extensions)
            {
                string testPath = ext == "" ? audioPath : Path.ChangeExtension(audioPath, ext);
                Debug.Log($"[ExternalChartLoader] 检查音频文件: {testPath}");
                if (File.Exists(testPath))
                {
                    foundAudioPath = testPath;
                    Debug.Log($"[ExternalChartLoader] 找到音频文件: {foundAudioPath}");
                    break;
                }
            }

            if (foundAudioPath != null)
            {
                yield return StartCoroutine(LoadAudioFile(foundAudioPath, chartData, onChartLoaded, onError));
            }
            else
            {
                string error = $"找不到音频文件: {audioPath}";
                Debug.LogWarning($"[ExternalChartLoader] {error}");
                // 即使没有音频也继续
                loadedChart = chartData;
                onChartLoaded?.Invoke(chartData);
            }
        }
        else
        {
            // 没有音频文件，直接返回
            Debug.Log($"[ExternalChartLoader] 没有指定音频文件，直接返回谱面数据");
            loadedChart = chartData;
            onChartLoaded?.Invoke(chartData);
        }
    }

    /// <summary>
    /// 加载外部音频文件
    /// </summary>
    private IEnumerator LoadAudioFile(string audioPath, ChartData chartData, Action<ChartData> onChartLoaded, Action<string> onError)
    {
        Debug.Log($"[ExternalChartLoader] 加载音频: {audioPath}");

        // 确定音频类型
        AudioType audioType = GetAudioType(audioPath);
        Debug.Log($"[ExternalChartLoader] 音频类型: {audioType}");

        // 使用UnityWebRequest加载音频
        string uri = "file:///" + audioPath.Replace("\\", "/");
        Debug.Log($"[ExternalChartLoader] 音频URI: {uri}");

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(uri, audioType))
        {
            // 设置下载处理器
            ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = false;

            yield return www.SendWebRequest();

            Debug.Log($"[ExternalChartLoader] 请求完成，结果: {www.result}");

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

                if (clip != null)
                {
                    clip.name = Path.GetFileNameWithoutExtension(audioPath);
                    chartData.audioClip = clip;

                    Debug.Log($"[ExternalChartLoader] 音频加载成功: {clip.name}, 长度: {clip.length}s, 频率: {clip.frequency}, 声道: {clip.channels}");

                    loadedChart = chartData;
                    onChartLoaded?.Invoke(chartData);
                }
                else
                {
                    string error = "AudioClip is null after download";
                    Debug.LogError($"[ExternalChartLoader] {error}");
                    onError?.Invoke(error);
                }
            }
            else
            {
                string error = $"加载音频失败: {www.error}";
                Debug.LogError($"[ExternalChartLoader] {error}");
                Debug.LogError($"[ExternalChartLoader] Response Code: {www.responseCode}");
                onError?.Invoke(error);
            }
        }
    }

    /// <summary>
    /// 根据文件扩展名确定音频类型
    /// </summary>
    private AudioType GetAudioType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLower();
        switch (extension)
        {
            case ".mp3":
                return AudioType.MPEG;
            case ".wav":
                return AudioType.WAV;
            case ".ogg":
                return AudioType.OGGVORBIS;
            default:
                return AudioType.UNKNOWN;
        }
    }

    /// <summary>
    /// 保存谱面为JSON文件
    /// </summary>
    public void SaveChartToFile(ChartData chartData, string filePath)
    {
        try
        {
            ChartDataJson chartJson = ChartDataJson.FromChartData(chartData);
            string jsonContent = JsonUtility.ToJson(chartJson, true);
            File.WriteAllText(filePath, jsonContent);
            Debug.Log($"[ExternalChartLoader] 谱面保存成功: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ExternalChartLoader] 保存失败: {e.Message}");
        }
    }

    /// <summary>
    /// 获取最后加载的谱面
    /// </summary>
    public ChartData GetLoadedChart()
    {
        return loadedChart;
    }
}
