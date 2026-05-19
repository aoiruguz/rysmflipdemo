using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// 物体发射器 - 彻底重构版本
/// 实例化 VFX Prefab，并随机给每个实例的 VisualEffect 组件传入贴图
/// </summary>
public class ProjectileLauncher : MonoBehaviour
{
    [Header("VFX 预制体设置")]
    [Tooltip("包含 VisualEffect 组件的 VFX 预制体")]
    public GameObject vfxPrefab;

    [Tooltip("传入 VFX 的贴图列表（随机抽取之一）")]
    public Texture[] textureList;

    [Tooltip("VFX 属性中暴露的贴图参数名称")]
    public string textureParameterName = "MainTex";

    [Header("发射控制")]
    [Tooltip("是否启用自动循环发射")]
    public bool autoLaunch = true;

    [Tooltip("自动发射间隔（秒）")]
    public float launchInterval = 0.2f;

    [Tooltip("实例化后的 VFX 自动销毁延迟（秒），防止场景中物体无限累积")]
    public float destroyDelay = 3f;

    private float launchTimer = 0f;

    void Update()
    {
        if (autoLaunch)
        {
            launchTimer += Time.deltaTime;
            if (launchTimer >= launchInterval)
            {
                launchTimer = 0f;
                LaunchProjectile();
            }
        }
    }

    /// <summary>
    /// 触发一次礼物粒子喷发
    /// </summary>
    public void LaunchProjectile()
    {
        if (vfxPrefab == null)
        {
            Debug.LogError("[ProjectileLauncher] vfxPrefab is null!");
            return;
        }

        if (textureList == null || textureList.Length == 0)
        {
            Debug.LogWarning("[ProjectileLauncher] textureList is empty!");
            return;
        }

        // 实例化 VFX，并将其设为当前 GameObject 的子物体
        GameObject spawnedVFX = Instantiate(vfxPrefab, transform.position, transform.rotation, transform);

        // 获取 VisualEffect 组件
        VisualEffect vfxComponent = spawnedVFX.GetComponent<VisualEffect>();
        if (vfxComponent != null)
        {
            // 随机选取一张贴图
            int randomIndex = Random.Range(0, textureList.Length);
            Texture randomTex = textureList[randomIndex];

            if (randomTex != null)
            {
                // 传入贴图到 VFX
                vfxComponent.SetTexture(textureParameterName, randomTex);
            }
        }
        else
        {
            Debug.LogWarning($"[ProjectileLauncher] Instantiated prefab '{spawnedVFX.name}' does not have a VisualEffect component!");
        }

        // 延迟销毁，避免内存泄漏
        if (destroyDelay > 0f)
        {
            Destroy(spawnedVFX, destroyDelay);
        }
    }
}
