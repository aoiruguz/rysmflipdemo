using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// 批量替换所有 TextMeshPro 组件的字体
/// </summary>
public class ReplaceAllTMPFonts : EditorWindow
{
    private TMP_FontAsset newFont;
    private bool includeInactiveObjects = true;
    private bool searchInAllScenes = false;

    [MenuItem("Tools/批量替换 TMP 字体")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceAllTMPFonts>("批量替换 TMP 字体");
    }

    private void OnGUI()
    {
        GUILayout.Label("批量替换 TextMeshPro 字体", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "此工具会替换所有 TextMeshProUGUI 和 TextMeshPro 组件的字体。\n" +
            "建议先备份项目或使用版本控制。",
            MessageType.Warning
        );

        EditorGUILayout.Space();

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "新字体资源",
            newFont,
            typeof(TMP_FontAsset),
            false
        );

        includeInactiveObjects = EditorGUILayout.Toggle("包含未激活的对象", includeInactiveObjects);
        searchInAllScenes = EditorGUILayout.Toggle("搜索所有场景", searchInAllScenes);

        EditorGUILayout.Space();

        GUI.enabled = newFont != null;

        if (GUILayout.Button("替换当前场景中的字体", GUILayout.Height(40)))
        {
            ReplaceInCurrentScene();
        }

        if (GUILayout.Button("替换所有 Prefab 中的字体", GUILayout.Height(40)))
        {
            ReplaceInAllPrefabs();
        }

        if (searchInAllScenes && GUILayout.Button("替换所有场景中的字体", GUILayout.Height(40)))
        {
            ReplaceInAllScenes();
        }

        GUI.enabled = true;

        EditorGUILayout.Space();

        if (GUILayout.Button("查找当前场景中的所有 TMP 组件"))
        {
            FindAllTMPComponents();
        }
    }

    private void ReplaceInCurrentScene()
    {
        if (newFont == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择新字体资源！", "确定");
            return;
        }

        int count = 0;

        // 替换 TextMeshProUGUI (UI)
        TextMeshProUGUI[] uiTexts = FindObjectsByType<TextMeshProUGUI>(includeInactiveObjects ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var tmp in uiTexts)
        {
            Undo.RecordObject(tmp, "Replace TMP Font");
            tmp.font = newFont;
            EditorUtility.SetDirty(tmp);
            count++;
        }

        // 替换 TextMeshPro (3D)
        TextMeshPro[] worldTexts = FindObjectsByType<TextMeshPro>(includeInactiveObjects ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var tmp in worldTexts)
        {
            Undo.RecordObject(tmp, "Replace TMP Font");
            tmp.font = newFont;
            EditorUtility.SetDirty(tmp);
            count++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        EditorUtility.DisplayDialog(
            "完成",
            $"已替换当前场景中 {count} 个 TMP 组件的字体！",
            "确定"
        );

        Debug.Log($"[TMP Font Replace] 已替换 {count} 个组件");
    }

    private void ReplaceInAllPrefabs()
    {
        if (newFont == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择新字体资源！", "确定");
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;
        int prefabCount = 0;

        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            EditorUtility.DisplayProgressBar(
                "替换 Prefab 字体",
                $"处理: {path}",
                (float)i / prefabGuids.Length
            );

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool modified = false;

            // 替换 UI 文本
            TextMeshProUGUI[] uiTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in uiTexts)
            {
                tmp.font = newFont;
                modified = true;
                count++;
            }

            // 替换 3D 文本
            TextMeshPro[] worldTexts = prefab.GetComponentsInChildren<TextMeshPro>(true);
            foreach (var tmp in worldTexts)
            {
                tmp.font = newFont;
                modified = true;
                count++;
            }

            if (modified)
            {
                EditorUtility.SetDirty(prefab);
                prefabCount++;
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog(
            "完成",
            $"已处理 {prefabCount} 个 Prefab，替换了 {count} 个 TMP 组件的字体！",
            "确定"
        );

        Debug.Log($"[TMP Font Replace] 已处理 {prefabCount} 个 Prefab，替换 {count} 个组件");
    }

    private void ReplaceInAllScenes()
    {
        if (newFont == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择新字体资源！", "确定");
            return;
        }

        if (!EditorUtility.DisplayDialog(
            "确认",
            "这将打开并修改所有场景。确定继续吗？",
            "确定",
            "取消"))
        {
            return;
        }

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
        int totalCount = 0;
        int sceneCount = 0;

        string currentScenePath = SceneManager.GetActiveScene().path;

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            EditorUtility.DisplayProgressBar(
                "替换场景字体",
                $"处理: {path}",
                (float)i / sceneGuids.Length
            );

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            int count = 0;

            // 替换 UI 文本
            TextMeshProUGUI[] uiTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var tmp in uiTexts)
            {
                tmp.font = newFont;
                EditorUtility.SetDirty(tmp);
                count++;
            }

            // 替换 3D 文本
            TextMeshPro[] worldTexts = FindObjectsByType<TextMeshPro>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var tmp in worldTexts)
            {
                tmp.font = newFont;
                EditorUtility.SetDirty(tmp);
                count++;
            }

            if (count > 0)
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                totalCount += count;
                sceneCount++;
            }
        }

        // 恢复原场景
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
        }

        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog(
            "完成",
            $"已处理 {sceneCount} 个场景，替换了 {totalCount} 个 TMP 组件的字体！",
            "确定"
        );

        Debug.Log($"[TMP Font Replace] 已处理 {sceneCount} 个场景，替换 {totalCount} 个组件");
    }

    private void FindAllTMPComponents()
    {
        TextMeshProUGUI[] uiTexts = FindObjectsByType<TextMeshProUGUI>(includeInactiveObjects ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        TextMeshPro[] worldTexts = FindObjectsByType<TextMeshPro>(includeInactiveObjects ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        Debug.Log($"=== 当前场景中的 TMP 组件 ===");
        Debug.Log($"TextMeshProUGUI (UI): {uiTexts.Length} 个");
        Debug.Log($"TextMeshPro (3D): {worldTexts.Length} 个");
        Debug.Log($"总计: {uiTexts.Length + worldTexts.Length} 个");

        foreach (var tmp in uiTexts)
        {
            Debug.Log($"  [UI] {GetGameObjectPath(tmp.gameObject)} - 当前字体: {(tmp.font != null ? tmp.font.name : "无")}");
        }

        foreach (var tmp in worldTexts)
        {
            Debug.Log($"  [3D] {GetGameObjectPath(tmp.gameObject)} - 当前字体: {(tmp.font != null ? tmp.font.name : "无")}");
        }

        EditorUtility.DisplayDialog(
            "查找结果",
            $"找到 {uiTexts.Length + worldTexts.Length} 个 TMP 组件\n" +
            $"UI: {uiTexts.Length}\n" +
            $"3D: {worldTexts.Length}\n\n" +
            "详细信息已输出到 Console",
            "确定"
        );
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}
