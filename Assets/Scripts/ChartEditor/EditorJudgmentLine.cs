using UnityEngine;

/// <summary>
/// 制谱器判定线控制器
/// 判定线跟随镜头移动，始终显示在屏幕下方固定位置
/// </summary>
public class EditorJudgmentLine : MonoBehaviour
{
    [Header("Settings")]
    public float judgmentLineOffsetY = -4f; // 相对于镜头的Y偏移

    [Header("Visual")]
    public Color lineColor = Color.yellow;
    public float lineWidth = 0.1f;
    public float lineLength = 10f;

    private Camera editorCamera;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        editorCamera = Camera.main;
        if (editorCamera == null)
        {
            editorCamera = FindFirstObjectByType<Camera>();
        }

        // 设置视觉效果
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = lineColor;
        }
    }

    void LateUpdate()
    {
        if (editorCamera == null) return;

        // 判定线跟随镜头，保持在镜头下方固定位置（使用ChartEditorSettings）
        Vector3 newPos = transform.position;
        newPos.y = editorCamera.transform.position.y + ChartEditorSettings.JudgmentLineOffsetY;
        transform.position = newPos;
    }

    void OnDrawGizmos()
    {
        // 在Scene视图中绘制判定线
        Gizmos.color = lineColor;
        Vector3 center = transform.position;
        Gizmos.DrawLine(center + Vector3.left * lineLength / 2f, center + Vector3.right * lineLength / 2f);
    }
}
