using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

internal class CommandScript : IRunCommand
{
    public void Execute(ExecutionResult result)
    {
        // 获取当前场景中的所有GameObject
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        // 用于存储脚本信息的字典（避免重复）
        Dictionary<string, ScriptInfo> scriptInfos = new Dictionary<string, ScriptInfo>();

        // 遍历所有GameObject
        foreach (GameObject go in allObjects)
        {
            // 获取GameObject上的所有MonoBehaviour组件
            MonoBehaviour[] scripts = go.GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour script in scripts)
            {
                if (script == null)
                {
                    result.LogWarning("GameObject '{0}' 有缺失的脚本 (Missing Script)", go.name);
                    continue;
                }

                // 获取脚本类型
                var scriptType = script.GetType();
                string scriptName = scriptType.FullName ?? scriptType.Name;

                // 获取脚本文件路径
                MonoScript monoScript = MonoScript.FromMonoBehaviour(script);
                if (monoScript != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(monoScript);

                    // 添加到字典中（避免重复）
                    if (!scriptInfos.ContainsKey(scriptName))
                    {
                        scriptInfos.Add(scriptName, new ScriptInfo
                        {
                            ScriptName = scriptName,
                            AssetPath = assetPath,
                            GameObjects = new List<string> { go.name }
                        });
                    }
                    else
                    {
                        scriptInfos[scriptName].GameObjects.Add(go.name);
                    }
                }
            }
        }

        // 输出结果
        result.Log("========================================");
        result.Log("=== 场景中挂载的脚本列表 ===");
        result.Log("共找到 {0} 个不同的脚本", scriptInfos.Count);
        result.Log("========================================");
        result.Log("");

        // 按脚本名称排序后输出
        foreach (var kvp in scriptInfos.OrderBy(x => x.Key))
        {
            var info = kvp.Value;
            result.Log("脚本: {0}", info.ScriptName);
            result.Log("路径: {0}", info.AssetPath);
            result.Log("使用次数: {0}", info.GameObjects.Count);
            result.Log("挂载对象: {0}", string.Join(", ", info.GameObjects));
            result.Log("----------------------------------------");
        }

        result.Log("========================================");
        result.Log("检测完成！");
        result.Log("========================================");
    }

    private class ScriptInfo
    {
        public string ScriptName;
        public string AssetPath;
        public List<string> GameObjects;
    }
}
