using UnityEngine;

/// <summary>
/// 游戏初始化器
/// 在游戏启动时创建 SaveManager 单例
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        // 检查场景中是否已有 SaveManager
        if (SaveManager.Instance == null)
        {
            // 创建 SaveManager
            GameObject saveManagerObj = new GameObject("SaveManager");
            saveManagerObj.AddComponent<SaveManager>();

            Debug.Log("[GameBootstrap] SaveManager initialized via code");
        }
        else
        {
            Debug.Log("[GameBootstrap] SaveManager already exists in scene");
        }
    }
}
