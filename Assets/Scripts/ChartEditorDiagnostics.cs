using UnityEngine;

/// <summary>
/// 谱面编辑器诊断工具
/// 按F10键显示完整的状态信息
/// </summary>
public class ChartEditorDiagnostics : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            RunDiagnostics();
        }
    }

    void RunDiagnostics()
    {
        Debug.Log("========== Chart Editor Diagnostics ==========");

        // 检查ChartEditorManager
        ChartEditorManager manager = FindFirstObjectByType<ChartEditorManager>();
        if (manager == null)
        {
            Debug.LogError("[Diagnostics] ChartEditorManager NOT FOUND!");
            return;
        }

        Debug.Log($"[Diagnostics] ChartEditorManager found: {manager.gameObject.name}");

        // 检查currentChart
        if (manager.currentChart == null)
        {
            Debug.LogWarning("[Diagnostics] currentChart is NULL!");
        }
        else
        {
            Debug.Log($"[Diagnostics] currentChart: {manager.currentChart.songName}");
            Debug.Log($"[Diagnostics] - Notes count: {manager.currentChart.notes.Count}");
            Debug.Log($"[Diagnostics] - BPM: {manager.currentChart.bpm}");
            Debug.Log($"[Diagnostics] - Offset: {manager.currentChart.offset}");

            if (manager.currentChart.audioClip == null)
            {
                Debug.LogError("[Diagnostics] - AudioClip is NULL!");
            }
            else
            {
                Debug.Log($"[Diagnostics] - AudioClip: {manager.currentChart.audioClip.name}");
                Debug.Log($"[Diagnostics] - AudioClip length: {manager.currentChart.audioClip.length}s");
                Debug.Log($"[Diagnostics] - AudioClip frequency: {manager.currentChart.audioClip.frequency}");
                Debug.Log($"[Diagnostics] - AudioClip channels: {manager.currentChart.audioClip.channels}");
                Debug.Log($"[Diagnostics] - AudioClip loadState: {manager.currentChart.audioClip.loadState}");
            }
        }

        // 检查AudioSource
        if (manager.audioSource == null)
        {
            Debug.LogError("[Diagnostics] AudioSource is NULL!");
        }
        else
        {
            Debug.Log($"[Diagnostics] AudioSource found");
            Debug.Log($"[Diagnostics] - Clip: {(manager.audioSource.clip != null ? manager.audioSource.clip.name : "NULL")}");
            Debug.Log($"[Diagnostics] - Volume: {manager.audioSource.volume}");
            Debug.Log($"[Diagnostics] - IsPlaying: {manager.audioSource.isPlaying}");
            Debug.Log($"[Diagnostics] - Time: {manager.audioSource.time}");
            Debug.Log($"[Diagnostics] - Mute: {manager.audioSource.mute}");
        }

        // 检查Camera
        if (manager.editorCamera == null)
        {
            Debug.LogWarning("[Diagnostics] editorCamera is NULL!");
        }
        else
        {
            Debug.Log($"[Diagnostics] editorCamera: {manager.editorCamera.name}");
        }

        // 检查CameraController
        if (manager.cameraController == null)
        {
            Debug.LogWarning("[Diagnostics] cameraController is NULL!");
        }
        else
        {
            Debug.Log($"[Diagnostics] cameraController found");
        }

        // 检查UI
        if (manager.editorUI == null)
        {
            Debug.LogWarning("[Diagnostics] editorUI is NULL!");
        }
        else
        {
            Debug.Log($"[Diagnostics] editorUI found");
        }

        // 检查Notes
        Debug.Log($"[Diagnostics] Total notes in scene: {manager.TotalNotes}");

        // 检查播放状态
        Debug.Log($"[Diagnostics] IsPlaying: {manager.IsPlaying}");

        // 检查ExternalChartLoader
        if (ExternalChartLoader.Instance == null)
        {
            Debug.LogWarning("[Diagnostics] ExternalChartLoader.Instance is NULL!");
        }
        else
        {
            Debug.Log($"[Diagnostics] ExternalChartLoader.Instance found");
            ChartData loadedChart = ExternalChartLoader.Instance.GetLoadedChart();
            if (loadedChart != null)
            {
                Debug.Log($"[Diagnostics] - Loaded chart: {loadedChart.songName}");
            }
        }

        Debug.Log("========== End Diagnostics ==========");
    }
}
