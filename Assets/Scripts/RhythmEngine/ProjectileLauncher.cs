using UnityEngine;

/// <summary>
/// 物体发射器 - 从当前位置发射物体，循环使用 Sprite 列表
/// 物体受重力影响，碰撞后快速透明
/// </summary>
public class ProjectileLauncher : MonoBehaviour
{
    [Header("发射物体设置")]
    [Tooltip("发射物体的预制体（需要包含 SpriteRenderer 和 Rigidbody2D）")]
    public GameObject projectilePrefab;

    [Tooltip("循环使用的 Sprite 列表")]
    public Sprite[] spriteList;

    private int currentSpriteIndex = 0;

    [Header("发射参数")]
    [Tooltip("发射角度范围 - 最小值（度数）")]
    [Range(0f, 90f)]
    public float minLaunchAngle = 30f;

    [Tooltip("发射角度范围 - 最大值（度数）")]
    [Range(0f, 90f)]
    public float maxLaunchAngle = 60f;

    [Tooltip("发射速度（初速度）")]
    public float launchSpeed = 10f;

    [Tooltip("发射方向（1=右，-1=左）")]
    public float direction = 1f;

    [Tooltip("重力缩放（1=正常重力，2=两倍重力）")]
    public float gravityScale = 1f;

    [Header("透明度设置")]
    [Tooltip("碰撞触发透明化的 Tag（例如 'Wall'）")]
    public string fadeOutTriggerTag = "Wall";

    [Tooltip("碰撞后快速淡出的持续时间（秒）")]
    public float quickFadeDuration = 0.3f;

    [Tooltip("是否在完全透明后自动销毁")]
    public bool destroyOnFadeOut = true;

    [Header("显示层级设置")]
    [Tooltip("Sorting Layer 名称")]
    public string sortingLayerName = "Default";

    [Tooltip("Order in Layer（数值越大越靠前）")]
    public int sortingOrder = 5;

    [Header("自动发射设置")]
    [Tooltip("是否启用自动循环发射")]
    public bool autoLaunch = false;

    [Tooltip("自动发射间隔（秒）")]
    public float launchInterval = 1f;

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
    /// 发射一个物体
    /// </summary>
    public void LaunchProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("[ProjectileLauncher] projectilePrefab is null!");
            return;
        }

        // 实例化物体
        GameObject obj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);

        // 设置 Sprite（循环列表）
        if (spriteList != null && spriteList.Length > 0)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = spriteList[currentSpriteIndex];

                // 设置显示层级
                sr.sortingLayerName = sortingLayerName;
                sr.sortingOrder = sortingOrder;
            }

            // 循环到下一个 Sprite
            currentSpriteIndex = (currentSpriteIndex + 1) % spriteList.Length;
        }

        // 设置物理参数
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = gravityScale;

            // 随机生成发射角度
            float randomAngle = Random.Range(minLaunchAngle, maxLaunchAngle);
            float angleRad = randomAngle * Mathf.Deg2Rad;

            // 计算发射速度向量
            Vector2 velocity = new Vector2(
                Mathf.Cos(angleRad) * launchSpeed * direction,
                Mathf.Sin(angleRad) * launchSpeed
            );

            rb.linearVelocity = velocity;
        }

        // 添加淡出脚本
        ProjectileFadeOut fadeOut = obj.AddComponent<ProjectileFadeOut>();
        fadeOut.fadeOutTriggerTag = fadeOutTriggerTag;
        fadeOut.quickFadeDuration = quickFadeDuration;
        fadeOut.destroyOnFadeOut = destroyOnFadeOut;
    }

    void OnDrawGizmosSelected()
    {
        // 绘制发射方向预览
        float minAngleRad = minLaunchAngle * Mathf.Deg2Rad;
        float maxAngleRad = maxLaunchAngle * Mathf.Deg2Rad;

        Vector3 minDir = new Vector3(
            Mathf.Cos(minAngleRad) * direction,
            Mathf.Sin(minAngleRad),
            0
        );

        Vector3 maxDir = new Vector3(
            Mathf.Cos(maxAngleRad) * direction,
            Mathf.Sin(maxAngleRad),
            0
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, minDir * 2f);
        Gizmos.DrawRay(transform.position, maxDir * 2f);

        // 绘制角度扇形区域
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        for (float angle = minLaunchAngle; angle <= maxLaunchAngle; angle += 5f)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(
                Mathf.Cos(rad) * direction,
                Mathf.Sin(rad),
                0
            );
            Gizmos.DrawRay(transform.position, dir * 1.5f);
        }

        // 绘制发射点
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}
