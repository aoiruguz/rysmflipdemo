using UnityEngine;

/// <summary>
/// 游戏初始化器
/// 在游戏启动时创建 SaveManager 单例
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // 创建 SaveManager
        GameObject saveManagerObj = new GameObject("SaveManager");
        saveManagerObj.AddComponent<SaveManager>();

        Debug.Log("[GameBootstrap] SaveManager initialized");
    }
}
