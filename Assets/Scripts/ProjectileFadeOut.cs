using UnityEngine;

/// <summary>
/// 发射物体的淡出控制脚本
/// 碰撞到指定 Tag 的物体后快速透明并销毁
/// </summary>
public class ProjectileFadeOut : MonoBehaviour
{
    [Header("碰撞设置")]
    [Tooltip("碰撞触发透明化的 Tag（例如 'Wall'）")]
    public string fadeOutTriggerTag = "Wall";

    [Header("淡出设置")]
    [Tooltip("碰撞后快速淡出的持续时间（秒）")]
    public float quickFadeDuration = 0.3f;

    [Tooltip("是否在完全透明后自动销毁")]
    public bool destroyOnFadeOut = true;

    private SpriteRenderer spriteRenderer;
    private bool isFadingOut = false;
    private float fadeOutTimer = 0f;
    private float initialAlpha = 1f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            initialAlpha = spriteRenderer.color.a;
        }
    }

    void Update()
    {
        if (isFadingOut && spriteRenderer != null)
        {
            fadeOutTimer += Time.deltaTime;
            float fadeProgress = fadeOutTimer / quickFadeDuration;
            fadeProgress = Mathf.Clamp01(fadeProgress);
            float currentAlpha = Mathf.Lerp(initialAlpha, 0f, fadeProgress);

            Color color = spriteRenderer.color;
            color.a = currentAlpha;
            spriteRenderer.color = color;

            if (fadeProgress >= 1f && destroyOnFadeOut)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[ProjectileFadeOut] Trigger detected: {other.gameObject.name}, Tag: {other.tag}");

        if (!string.IsNullOrEmpty(fadeOutTriggerTag) && other.CompareTag(fadeOutTriggerTag))
        {
            Debug.Log($"[ProjectileFadeOut] Tag matched! Triggering fade out.");
            TriggerFadeOut();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[ProjectileFadeOut] Collision detected: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        if (!string.IsNullOrEmpty(fadeOutTriggerTag) && collision.gameObject.CompareTag(fadeOutTriggerTag))
        {
            Debug.Log($"[ProjectileFadeOut] Tag matched! Triggering fade out.");
            TriggerFadeOut();
        }
    }

    public void TriggerFadeOut()
    {
        if (!isFadingOut)
        {
            isFadingOut = true;
            fadeOutTimer = 0f;
        }
    }
}
