using UnityEngine;
using UnityEngine.UI;

public class CatchEffect : MonoBehaviour
{
    private Vector3 velocity;
    
    // UI 组件
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image image;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
    }

    public void Initialize(Vector3 startPos, float angle, float speed)
    {
        transform.position = startPos;
        
        // Convert angle to direction vector
        float rad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        
        velocity = new Vector3(direction.x, direction.y, 0) * speed;
        
        // Initial look
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    void Update()
    {
        // Movement
        transform.position += velocity * Time.deltaTime;

        float currentMinX = -3f;
        float currentMaxX = 3f;
        float currentMaxY = 10f; // 默认 fallback 的顶部高度

        // 如果在 UI 容器下，动态获取容器的世界坐标边界
        if (rectTransform != null && rectTransform.parent != null)
        {
            RectTransform parentRect = rectTransform.parent.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                Vector3[] corners = new Vector3[4];
                parentRect.GetWorldCorners(corners);
                // corners[0] = Bottom Left
                // corners[1] = Top Left
                // corners[2] = Top Right
                // corners[3] = Bottom Right
                currentMinX = corners[0].x;
                currentMaxX = corners[2].x;
                currentMaxY = corners[1].y;
            }
        }

        // 左右边缘反弹逻辑
        if (transform.position.x <= currentMinX || transform.position.x >= currentMaxX)
        {
            velocity.x *= -1;
            // Clamp position to prevent getting stuck
            float clampedX = Mathf.Clamp(transform.position.x, currentMinX, currentMaxX);
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
        }

        // 碰到上边界销毁
        if (transform.position.y >= currentMaxY)
        {
            Destroy(gameObject);
        }
    }
}
