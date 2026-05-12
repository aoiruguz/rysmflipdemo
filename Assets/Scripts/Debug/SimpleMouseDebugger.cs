using UnityEngine;

/// <summary>
/// 简单的鼠标点击调试器
/// 直接挂载到相机上，运行游戏后点击查看检测结果
/// </summary>
public class SimpleMouseDebugger : MonoBehaviour
{
    private void Update()
    {
        // 每帧都显示鼠标位置
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("========== 鼠标点击了！==========");

            Vector3 mouseScreenPos = Input.mousePosition;
            Debug.Log($"鼠标屏幕坐标: {mouseScreenPos}");

            Camera cam = GetComponent<Camera>();
            if (cam == null)
                cam = Camera.main;

            if (cam == null)
            {
                Debug.LogError("找不到相机！");
                return;
            }

            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(mouseScreenPos);
            Debug.Log($"鼠标世界坐标: {mouseWorldPos}");

            // 尝试 2D Raycast
            Debug.Log("--- 2D Raycast 检测 ---");
            Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.zero, 100f);

            Debug.Log($"检测到 {hits.Length} 个 2D Collider");

            if (hits.Length == 0)
            {
                Debug.LogWarning("没有检测到任何 2D Collider！");

                // 尝试用 OverlapPoint
                Collider2D[] overlaps = Physics2D.OverlapPointAll(rayOrigin);
                Debug.Log($"OverlapPoint 检测到 {overlaps.Length} 个 Collider");

                foreach (var col in overlaps)
                {
                    Debug.Log($"  OverlapPoint 找到: {col.gameObject.name} at {col.transform.position}");
                }
            }
            else
            {
                foreach (var hit in hits)
                {
                    Debug.Log($"  检测到: {hit.collider.gameObject.name}");
                    Debug.Log($"    位置: {hit.collider.transform.position}");
                    Debug.Log($"    Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

                    SpriteRenderer sr = hit.collider.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Debug.Log($"    Sorting Layer: {sr.sortingLayerName}");
                        Debug.Log($"    Sorting Order: {sr.sortingOrder}");
                    }
                }
            }

            // 尝试 3D Raycast
            Debug.Log("--- 3D Raycast 检测 ---");
            Ray ray = cam.ScreenPointToRay(mouseScreenPos);
            RaycastHit[] hits3D = Physics.RaycastAll(ray, 1000f);
            Debug.Log($"检测到 {hits3D.Length} 个 3D Collider");

            foreach (var hit in hits3D)
            {
                Debug.Log($"  3D 检测到: {hit.collider.gameObject.name} at distance {hit.distance}");
            }

            Debug.Log("====================================");
        }
    }

    private void OnGUI()
    {
        // 在屏幕上显示提示
        GUI.Label(new Rect(10, 10, 300, 20), "点击屏幕查看 Console 输出");
        GUI.Label(new Rect(10, 30, 300, 20), $"鼠标位置: {Input.mousePosition}");
    }
}
