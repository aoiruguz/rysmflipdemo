using UnityEngine;

/// <summary>
/// 碰撞诊断工具 - 挂载到发射物上，检测所有碰撞事件
/// </summary>
public class CollisionDebugger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"<color=green>[CollisionDebugger] ✓ TRIGGER ENTER: {other.gameObject.name}, Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}</color>");
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Debug.Log($"<color=yellow>[CollisionDebugger] TRIGGER STAY: {other.gameObject.name}</color>");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"<color=orange>[CollisionDebugger] TRIGGER EXIT: {other.gameObject.name}</color>");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"<color=cyan>[CollisionDebugger] ✓ COLLISION ENTER: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}</color>");
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Debug.Log($"<color=yellow>[CollisionDebugger] COLLISION STAY: {collision.gameObject.name}</color>");
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"<color=orange>[CollisionDebugger] COLLISION EXIT: {collision.gameObject.name}</color>");
    }

    void Start()
    {
        // 输出当前物体的碰撞配置
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Collider2D col = GetComponent<Collider2D>();

        Debug.Log($"<color=magenta>[CollisionDebugger] === {gameObject.name} 配置 ===</color>");
        Debug.Log($"Layer: {LayerMask.LayerToName(gameObject.layer)}");

        if (rb != null)
        {
            Debug.Log($"Rigidbody2D: Body Type = {rb.bodyType}, Simulated = {rb.simulated}, Gravity Scale = {rb.gravityScale}");
        }
        else
        {
            Debug.LogError("❌ 缺少 Rigidbody2D!");
        }

        if (col != null)
        {
            Debug.Log($"Collider2D: Type = {col.GetType().Name}, IsTrigger = {col.isTrigger}, Enabled = {col.enabled}");
        }
        else
        {
            Debug.LogError("❌ 缺少 Collider2D!");
        }
    }
}
