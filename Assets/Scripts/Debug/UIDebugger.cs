using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 创建点击事件
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            // 存放所有被射线击中的 UI
            List<RaycastResult> results = new List<RaycastResult>();

            // 关键：检测场景中所有的 UI 射线检测器
            EventSystem.current.RaycastAll(eventData, results);

            Debug.Log($"<color=orange>========= UI 点击报告 ({results.Count}个物体) =========</color>");

            if (results.Count > 0)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    // 打印出所有被击中的 UI，按照层级顺序显示（第0个就是挡在最上面的）
                    Debug.Log($"层级 [{i}]: <b>{results[i].gameObject.name}</b>");
                }
            }
            else
            {
                Debug.Log("<color=red>警告：鼠标下没有任何开启了 Raycast Target 的 UI！</color>");
            }
        }
    }
}