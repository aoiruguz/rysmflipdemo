using UnityEngine;

/// <summary>
/// 检查场景中的编辑器组件
/// 按F11显示场景中所有的编辑器相关组件
/// </summary>
public class SceneComponentChecker : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            CheckScene();
        }
    }

    void CheckScene()
    {
        Debug.Log("========== Scene Component Check ==========");

        // 检查ChartEditorManager
        ChartEditorManager[] managers = FindObjectsOfType<ChartEditorManager>();
        Debug.Log($"[SceneCheck] ChartEditorManager count: {managers.Length}");
        foreach (var manager in managers)
        {
            Debug.Log($"[SceneCheck] - ChartEditorManager on: {manager.gameObject.name}");
        }

        // 检查ChartEditorController
        ChartEditorController[] controllers = FindObjectsOfType<ChartEditorController>();
        Debug.Log($"[SceneCheck] ChartEditorController count: {controllers.Length}");
        foreach (var controller in controllers)
        {
            Debug.Log($"[SceneCheck] - ChartEditorController on: {controller.gameObject.name}");
        }

        // 检查ChartEditorUI
        ChartEditorUI[] uis = FindObjectsOfType<ChartEditorUI>();
        Debug.Log($"[SceneCheck] ChartEditorUI count: {uis.Length}");
        foreach (var ui in uis)
        {
            Debug.Log($"[SceneCheck] - ChartEditorUI on: {ui.gameObject.name}");
        }

        // 检查AudioSource
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        Debug.Log($"[SceneCheck] AudioSource count: {audioSources.Length}");
        foreach (var source in audioSources)
        {
            Debug.Log($"[SceneCheck] - AudioSource on: {source.gameObject.name}, clip: {(source.clip != null ? source.clip.name : "NULL")}, isPlaying: {source.isPlaying}");
        }

        Debug.Log("========== End Scene Check ==========");
    }
}
