using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// 在PlayScene中自动设置敌人血量UI的编辑器工具
/// 使用方法：在Unity编辑器菜单中选择 Tools > Setup Enemy Health UI
/// </summary>
public class SetupEnemyHealthUI : EditorWindow
{
    private static int heartCount = 6; // 默认6颗心

    [MenuItem("Tools/Setup Enemy Health UI")]
    public static void ShowWindow()
    {
        GetWindow<SetupEnemyHealthUI>("Setup Enemy Health");
    }

    void OnGUI()
    {
        GUILayout.Label("Enemy Health UI Setup", EditorStyles.boldLabel);

        heartCount = EditorGUILayout.IntField("Heart Count", heartCount);
        heartCount = Mathf.Clamp(heartCount, 1, 10);

        if (GUILayout.Button("Create Enemy Health UI"))
        {
            SetupEnemyHealth(heartCount);
        }
    }

    public static void SetupEnemyHealth(int numHearts)
    {
        // 查找Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene! Please add a Canvas first.");
            return;
        }

        // 创建敌人血量容器
        GameObject enemyHealthContainer = new GameObject("EnemyHealthUI");
        enemyHealthContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = enemyHealthContainer.AddComponent<RectTransform>();

        // 设置位置：屏幕右上角
        containerRect.anchorMin = new Vector2(1f, 1f);
        containerRect.anchorMax = new Vector2(1f, 1f);
        containerRect.pivot = new Vector2(1f, 1f);
        containerRect.anchoredPosition = new Vector2(-30, -30);

        // 根据心的数量计算容器大小
        float heartSize = 60f;
        float spacing = 10f;
        float totalWidth = (heartSize + spacing) * numHearts - spacing;
        containerRect.sizeDelta = new Vector2(totalWidth, heartSize);

        // 添加EnemyHealthSystem组件
        EnemyHealthSystem healthSystem = enemyHealthContainer.AddComponent<EnemyHealthSystem>();
        healthSystem.hearts = new System.Collections.Generic.List<Image>();

        // 创建心
        for (int i = 0; i < numHearts; i++)
        {
            float xPos = -(heartSize + spacing) * i;
            Image heart = CreateHeart(enemyHealthContainer.transform, $"Heart{i + 1}", new Vector2(xPos, 0), heartSize);
            healthSystem.hearts.Add(heart);
        }

        // 标记场景为已修改
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"[SetupEnemyHealthUI] Enemy health UI created with {numHearts} hearts!");
        Debug.Log("请在Inspector中为EnemyHealthSystem组件设置心的Sprite素材：");
        Debug.Log("- Heart 5 Sprite: 满血的心（5/5）");
        Debug.Log("- Heart 4 Sprite: 4/5血量的心");
        Debug.Log("- Heart 3 Sprite: 3/5血量的心");
        Debug.Log("- Heart 2 Sprite: 2/5血量的心");
        Debug.Log("- Heart 1 Sprite: 1/5血量的心");
        Debug.Log("- Heart 0 Sprite: 空血的心（0/5）");

        // 选中创建的对象
        Selection.activeGameObject = enemyHealthContainer;
    }

    private static Image CreateHeart(Transform parent, string name, Vector2 position, float size)
    {
        GameObject heartObj = new GameObject(name);
        heartObj.transform.SetParent(parent, false);

        RectTransform heartRect = heartObj.AddComponent<RectTransform>();
        heartRect.anchorMin = new Vector2(1f, 0.5f);
        heartRect.anchorMax = new Vector2(1f, 0.5f);
        heartRect.pivot = new Vector2(0.5f, 0.5f);
        heartRect.anchoredPosition = position;
        heartRect.sizeDelta = new Vector2(size, size);

        Image heartImage = heartObj.AddComponent<Image>();
        heartImage.color = Color.white;
        // 使用Unity内置的圆形Sprite作为占位符
        heartImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        heartImage.preserveAspect = true;

        return heartImage;
    }
}
