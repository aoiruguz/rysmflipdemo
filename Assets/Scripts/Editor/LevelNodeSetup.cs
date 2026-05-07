using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelNode))]
public class LevelNodeSetup : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelNode node = (LevelNode)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("点击检测需要 Collider2D 组件", MessageType.Info);

        // 检查是否有 Collider2D
        Collider2D collider = node.GetComponent<Collider2D>();
        if (collider == null)
        {
            EditorGUILayout.HelpBox("未找到 Collider2D！点击下方按钮自动添加", MessageType.Warning);

            if (GUILayout.Button("添加 CircleCollider2D"))
            {
                Undo.AddComponent<CircleCollider2D>(node.gameObject);
                Debug.Log("已添加 CircleCollider2D 到 " + node.gameObject.name);
            }

            if (GUILayout.Button("添加 BoxCollider2D"))
            {
                Undo.AddComponent<BoxCollider2D>(node.gameObject);
                Debug.Log("已添加 BoxCollider2D 到 " + node.gameObject.name);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("✓ 已配置 " + collider.GetType().Name, MessageType.Info);
        }

        // 测试按钮
        EditorGUILayout.Space();
        if (GUILayout.Button("测试显示详情面板"))
        {
            if (Application.isPlaying)
            {
                if (node.levelData != null && LevelUIManager.Instance != null)
                {
                    LevelUIManager.Instance.ShowLevelDetails(node.levelData);
                }
                else
                {
                    Debug.LogWarning("需要在运行时测试，且确保 LevelData 和 LevelUIManager 已配置");
                }
            }
            else
            {
                Debug.LogWarning("请在 Play 模式下测试");
            }
        }
    }
}
