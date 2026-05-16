using UnityEngine;

/// <summary>
/// 调试用关卡解锁覆盖器
/// 挂载到 Big Map 场景，强制覆盖 SaveManager 的解锁设置
/// 优先级高于 SaveManager，确保调试时关卡解锁生效
/// </summary>
public class DebugUnlockOverride : MonoBehaviour
{
    [Header("强制解锁设置")]
    [Tooltip("开启后，将强制解锁所有关卡，无视 SaveManager 的设置")]
    public bool forceUnlockAllLevels = false;

    [Header("粉丝数覆盖")]
    [Tooltip("开启后，将设置指定的粉丝数")]
    public bool overrideFans = false;
    [Tooltip("要设置的粉丝数量")]
    public long fansAmount = 999999999;

    private void Awake()
    {
        // 等待 SaveManager 初始化
        StartCoroutine(ApplyOverride());
    }

    private System.Collections.IEnumerator ApplyOverride()
    {
        // 等待 SaveManager 实例创建
        float timeout = 5f;
        float elapsed = 0f;

        while (SaveManager.Instance == null && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("[DebugUnlockOverride] SaveManager not found after timeout!");
            yield break;
        }

        // 强制设置 SaveManager 的解锁标志
        if (forceUnlockAllLevels)
        {
            SaveManager.Instance.unlockAllLevelsForDebug = true;
            Debug.Log("[DebugUnlockOverride] Force enabled unlockAllLevelsForDebug in SaveManager");
        }

        // 覆盖粉丝数
        if (overrideFans)
        {
            SaveManager.Instance.SetTotalFans(fansAmount);
            Debug.Log($"[DebugUnlockOverride] Set total fans to: {fansAmount}");
        }
    }
}
