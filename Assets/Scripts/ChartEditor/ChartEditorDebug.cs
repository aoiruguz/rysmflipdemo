using UnityEngine;

/// <summary>
/// 制谱器调试工具
/// 用于诊断播放和镜头问题
/// </summary>
public class ChartEditorDebug : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DiagnoseChartEditor();
        }
    }

    [ContextMenu("Diagnose Chart Editor")]
    public void DiagnoseChartEditor()
    {
        Debug.Log("=== Chart Editor Diagnosis ===");

        var manager = ChartEditorManager.Instance;
        if (manager == null)
        {
            Debug.LogError("ChartEditorManager.Instance is NULL!");
            return;
        }

        Debug.Log($"Manager found: {manager.name}");
        Debug.Log($"IsPlaying: {manager.IsPlaying}");

        // 检查AudioSource
        if (manager.audioSource == null)
        {
            Debug.LogError("AudioSource is NULL!");
        }
        else
        {
            Debug.Log($"AudioSource.clip: {(manager.audioSource.clip != null ? manager.audioSource.clip.name : "NULL")}");
            Debug.Log($"AudioSource.isPlaying: {manager.audioSource.isPlaying}");
            Debug.Log($"AudioSource.time: {manager.audioSource.time}");
            Debug.Log($"AudioSource.volume: {manager.audioSource.volume}");
            Debug.Log($"AudioSource.mute: {manager.audioSource.mute}");
        }

        // 检查Camera
        if (manager.editorCamera == null)
        {
            Debug.LogError("editorCamera is NULL!");
        }
        else
        {
            Debug.Log($"Camera position: {manager.editorCamera.transform.position}");
        }

        // 检查CameraController
        if (manager.cameraController == null)
        {
            Debug.LogError("cameraController is NULL!");
        }
        else
        {
            Debug.Log($"CameraController found: {manager.cameraController.name}");
        }

        // 检查ChartData
        if (manager.currentChart == null)
        {
            Debug.LogError("currentChart is NULL!");
        }
        else
        {
            Debug.Log($"Chart: {manager.currentChart.songName}");
            Debug.Log($"Chart.audioClip: {(manager.currentChart.audioClip != null ? manager.currentChart.audioClip.name : "NULL")}");
            Debug.Log($"Chart.notes count: {manager.currentChart.notes.Count}");
        }

        // 检查UI
        if (manager.editorUI == null)
        {
            Debug.LogWarning("editorUI is NULL!");
        }
        else
        {
            Debug.Log($"UI found: {manager.editorUI.name}");
        }

        Debug.Log($"Total EditorNotes: {manager.TotalNotes}");
        Debug.Log("=== Diagnosis Complete ===");
    }
}
