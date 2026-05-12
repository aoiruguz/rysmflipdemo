using UnityEngine;
using UnityEditor;

/// <summary>
/// 鼠标射线检测调试工具
/// 显示鼠标点击时检测到的所有物体
/// </summary>
[InitializeOnLoad]
public class MouseRaycastDebugger
{
    private static bool isEnabled = false;

    static MouseRaycastDebugger()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    [MenuItem("Tools/Debug/Toggle Mouse Raycast Debugger")]
    private static void ToggleDebugger()
    {
        isEnabled = !isEnabled;
        Debug.Log($"[MouseRaycastDebugger] {(isEnabled ? "已启用" : "已禁用")}");
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        if (!isEnabled)
            return;

        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            // 获取鼠标在场景中的世界坐标
            Vector2 mousePos = e.mousePosition;
            mousePos.y = sceneView.camera.pixelHeight - mousePos.y; // 翻转 Y 轴
            Vector3 worldPos = sceneView.camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, sceneView.camera.nearClipPlane));

            Debug.Log($"=== 鼠标点击检测 ===");
            Debug.Log($"屏幕坐标: {e.mousePosition}");
            Debug.Log($"世界坐标: {worldPos}");

            // 2D Raycast
            RaycastHit2D[] hits2D = Physics2D.RaycastAll(worldPos, Vector2.zero);
            if (hits2D.Length > 0)
            {
                Debug.Log($"检测到 {hits2D.Length} 个 2D Collider:");
                foreach (var hit in hits2D)
                {
                    Debug.Log($"  - {hit.collider.gameObject.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, Tag: {hit.collider.gameObject.tag})");
                }
            }
            else
            {
                Debug.LogWarning("未检测到任何 2D Collider！");
            }

            // 3D Raycast
            Ray ray = sceneView.camera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));
            RaycastHit[] hits3D = Physics.RaycastAll(ray, 1000f);
            if (hits3D.Length > 0)
            {
                Debug.Log($"检测到 {hits3D.Length} 个 3D Collider:");
                foreach (var hit in hits3D)
                {
                    Debug.Log($"  - {hit.collider.gameObject.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, Distance: {hit.distance})");
                }
            }

            Debug.Log($"==================");
        }
    }
}

/// <summary>
/// 运行时鼠标检测调试组件
/// 挂载到场景中的任意物体上，在 Game 视图中测试
/// </summary>
public class RuntimeMouseDebugger : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            Debug.Log($"=== 运行时鼠标点击 ===");
            Debug.Log($"屏幕坐标: {mousePos}");
            Debug.Log($"世界坐标: {worldPos}");

            // 2D Raycast
            RaycastHit2D hit2D = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit2D.collider != null)
            {
                Debug.Log($"2D 点击到: {hit2D.collider.gameObject.name}");
                Debug.Log($"  - Position: {hit2D.collider.transform.position}");
                Debug.Log($"  - Layer: {LayerMask.LayerToName(hit2D.collider.gameObject.layer)}");
                Debug.Log($"  - Sorting Layer: {hit2D.collider.GetComponent<SpriteRenderer>()?.sortingLayerName}");
                Debug.Log($"  - Sorting Order: {hit2D.collider.GetComponent<SpriteRenderer>()?.sortingOrder}");
            }
            else
            {
                Debug.LogWarning("2D Raycast 未检测到任何物体！");
            }

            // 3D Raycast
            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit3D;
            if (Physics.Raycast(ray, out hit3D, 1000f))
            {
                Debug.Log($"3D 点击到: {hit3D.collider.gameObject.name}");
            }

            Debug.Log($"==================");
        }
    }
}
