using UnityEngine;

/// <summary>
/// 基于物理的抛物线发射脚本
/// 物体受重力影响，并随时间逐渐透明
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsProjectile : MonoBehaviour
{
    [Header("发射参数")]
    [Tooltip("发射角度范围 - 最小值（度数）")]
    [Range(0f, 90f)]
    public float minLaunchAngle = 30f;

    [Tooltip("发射角度范围 - 最大值（度数）")]
    [Range(0f, 90f)]
    public float maxLaunchAngle = 60f;

    [Tooltip("发射速度（初速度）")]
    public float launchSpeed = 10f;

    [Tooltip("重力缩放（1=正常重力，2=两倍重力）")]
    public float gravityScale = 1f;

    [Header("透明度设置")]
    [Tooltip("碰撞触发透明化的 Tag（留空则不检测碰撞）")]
    public string fadeOutTriggerTag = "";

    [Tooltip("碰撞后快速淡出的持续时间（秒）")]
    public float quickFadeDuration = 0.3f;

    [Tooltip("是否在完全透明后自动销毁")]
    public bool destroyOnFadeOut = true;

    [Header("循环发射设置")]
    [Tooltip("是否启用循环发射（淡出后复原并重新发射）")]
    public bool enableLoop = false;

    [Tooltip("淡出后延迟多久复原并重新发射（秒）")]
    public float loopDelay = 1f;

    [Header("行为设置")]
    [Tooltip("是否在Start时自动发射")]
    public bool autoLaunch = true;

    [Tooltip("发射方向（1=右，-1=左）")]
    public float direction = 1f;

    [Header("显示层级设置")]
    [Tooltip("Sorting Layer 名称")]
    public string sortingLayerName = "Default";

    [Tooltip("Order in Layer（数值越大越靠前）")]
    public int sortingOrder = 5;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private CanvasGroup canvasGroup;
    private float elapsedTime = 0f;
    private float initialAlpha = 1f;
    private bool isLaunched = false;
    private Vector3 initialPosition;
    private bool isWaitingForRelaunch = false;
    private float relaunchTimer = 0f;
    private bool isFadingOut = false;
    private float fadeOutTimer = 0f;
    private float currentLaunchAngle = 45f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 设置显示层级
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = sortingLayerName;
            spriteRenderer.sortingOrder = sortingOrder;
            initialAlpha = spriteRenderer.color.a;
        }
        else if (canvasGroup != null)
        {
            initialAlpha = canvasGroup.alpha;
        }
    }

    void Start()
    {
        // 记录初始位置
        initialPosition = transform.position;

        if (autoLaunch)
        {
            Launch();
        }
    }

    void Update()
    {
        // 等待重新发射
        if (isWaitingForRelaunch)
        {
            relaunchTimer += Time.deltaTime;
            if (relaunchTimer >= loopDelay)
            {
                ResetAndRelaunch();
            }
            return;
        }

        if (!isLaunched) return;

        elapsedTime += Time.deltaTime;

        // 碰撞触发的快速淡出
        if (isFadingOut)
        {
            fadeOutTimer += Time.deltaTime;
            float fadeProgress = fadeOutTimer / quickFadeDuration;
            fadeProgress = Mathf.Clamp01(fadeProgress);
            float currentAlpha = Mathf.Lerp(initialAlpha, 0f, fadeProgress);

            // 应用透明度
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = currentAlpha;
                spriteRenderer.color = color;
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = currentAlpha;
            }

            // 完全透明后的处理
            if (fadeProgress >= 1f)
            {
                if (enableLoop)
                {
                    // 进入等待重新发射状态
                    isLaunched = false;
                    isFadingOut = false;
                    isWaitingForRelaunch = true;
                    relaunchTimer = 0f;

                    // 停止物理运动
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                    }
                }
                else if (destroyOnFadeOut)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 发射物体
    /// </summary>
    public void Launch()
    {
        if (rb == null) return;

        // 随机生成发射角度
        currentLaunchAngle = Random.Range(minLaunchAngle, maxLaunchAngle);

        // 设置重力
        rb.gravityScale = gravityScale;

        // 计算发射速度向量
        float angleRad = currentLaunchAngle * Mathf.Deg2Rad;
        Vector2 velocity = new Vector2(
            Mathf.Cos(angleRad) * launchSpeed * direction,
            Mathf.Sin(angleRad) * launchSpeed
        );

        // 应用速度
        rb.linearVelocity = velocity;

        isLaunched = true;
        isFadingOut = false;
        fadeOutTimer = 0f;
        elapsedTime = 0f;
    }

    /// <summary>
    /// 从指定位置以指定角度和速度发射
    /// </summary>
    public void LaunchFrom(Vector3 position, float speed, float dir = 1f)
    {
        transform.position = position;
        launchSpeed = speed;
        direction = dir;
        Launch();
    }

    /// <summary>
    /// 发射到目标点（自动计算角度和速度）
    /// </summary>
    public void LaunchToTarget(Vector3 targetPosition, float arcHeight = 2f)
    {
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, targetPosition);

        // 简化的抛物线计算
        direction = targetPosition.x > startPos.x ? 1f : -1f;

        // 根据距离和弧高估算发射参数
        launchSpeed = Mathf.Sqrt(distance * Mathf.Abs(Physics2D.gravity.y) * gravityScale);

        Launch();
    }

    /// <summary>
    /// 触发快速淡出（通常由碰撞触发）
    /// </summary>
    public void TriggerFadeOut()
    {
        if (!isFadingOut)
        {
            isFadingOut = true;
            fadeOutTimer = 0f;
        }
    }

    /// <summary>
    /// 碰撞检测 - Trigger
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[PhysicsProjectile] Trigger detected: {other.gameObject.name}, Tag: {other.tag}");

        if (!string.IsNullOrEmpty(fadeOutTriggerTag) && other.CompareTag(fadeOutTriggerTag))
        {
            Debug.Log($"[PhysicsProjectile] Tag matched! Triggering fade out.");
            TriggerFadeOut();
        }
    }

    /// <summary>
    /// 碰撞检测 - Collision
    /// </summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[PhysicsProjectile] Collision detected: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        if (!string.IsNullOrEmpty(fadeOutTriggerTag) && collision.gameObject.CompareTag(fadeOutTriggerTag))
        {
            Debug.Log($"[PhysicsProjectile] Tag matched! Triggering fade out.");
            TriggerFadeOut();
        }
    }

    /// <summary>
    /// 复原位置并重新发射
    /// </summary>
    private void ResetAndRelaunch()
    {
        // 复原位置
        transform.position = initialPosition;

        // 复原透明度
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = initialAlpha;
            spriteRenderer.color = color;
        }
        else if (canvasGroup != null)
        {
            canvasGroup.alpha = initialAlpha;
        }

        // 重置状态
        isWaitingForRelaunch = false;
        relaunchTimer = 0f;

        // 重新发射
        Launch();
    }

    void OnDrawGizmosSelected()
    {
        // 绘制发射方向预览
        if (!Application.isPlaying)
        {
            // 绘制角度范围
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
        }
    }
}
