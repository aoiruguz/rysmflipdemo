using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RysmFlipDemo.Editor
{
    public class SceneScriptDetector : EditorWindow
    {
        [MenuItem("Tools/检测场景脚本")]
        public static void DetectSceneScripts()
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
                        Debug.LogWarning($"GameObject '{go.name}' 有缺失的脚本 (Missing Script)");
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

            // 构建完整的输出信息
            StringBuilder output = new StringBuilder();
            output.AppendLine("========================================");
            output.AppendLine("=== 场景中挂载的脚本列表 ===");
            output.AppendLine($"共找到 {scriptInfos.Count} 个不同的脚本");
            output.AppendLine("========================================");
            output.AppendLine();

            // 按脚本名称排序后输出
            foreach (var kvp in scriptInfos.OrderBy(x => x.Key))
            {
                var info = kvp.Value;
                output.AppendLine($"脚本: {info.ScriptName}");
                output.AppendLine($"路径: {info.AssetPath}");
                output.AppendLine($"使用次数: {info.GameObjects.Count}");
                output.AppendLine($"挂载对象: {string.Join(", ", info.GameObjects)}");
                output.AppendLine("----------------------------------------");
            }

            output.AppendLine("========================================");
            output.AppendLine("检测完成！");
            output.AppendLine("========================================");

            // 输出到Console
            Debug.Log(output.ToString());

            // 显示完成对话框
            EditorUtility.DisplayDialog("场景脚本检测",
                $"检测完成！\n共找到 {scriptInfos.Count} 个不同的脚本\n\n详细信息已输出到Console窗口",
                "确定");
        }

        private class ScriptInfo
        {
            public string ScriptName;
            public string AssetPath;
            public List<string> GameObjects;
        }
    }
}
