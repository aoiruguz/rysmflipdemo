using UnityEngine;

/// <summary>
/// 游戏初始化管理器
/// 在游戏启动时加载并应用保存的设置
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // 在场景加载前应用保存的设置
        GameSettings.LoadAndApplySettings();
        Debug.Log("[GameInitializer] 游戏设置已加载并应用");
    }
}
