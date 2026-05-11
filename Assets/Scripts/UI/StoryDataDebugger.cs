using UnityEngine;

/// <summary>
/// 调试工具：检查 stories.json 是否正确加载
/// </summary>
public class StoryDataDebugger : MonoBehaviour
{
    [ContextMenu("Debug Story Data")]
    public void DebugStoryData()
    {
        string path = "StoryData/stories";
        TextAsset textAsset = Resources.Load<TextAsset>(path);

        if (textAsset == null)
        {
            Debug.LogError($"[StoryDataDebugger] 未找到文件：Resources/{path}.json");
            return;
        }

        Debug.Log($"[StoryDataDebugger] 找到文件，大小：{textAsset.text.Length} 字符");
        Debug.Log($"[StoryDataDebugger] 文件内容前 500 字符：\n{textAsset.text.Substring(0, Mathf.Min(500, textAsset.text.Length))}");

        try
        {
            StoryDataContainer container = JsonUtility.FromJson<StoryDataContainer>(textAsset.text);
            Debug.Log($"[StoryDataDebugger] 成功解析 JSON，包含 {container.stories.Count} 条剧情");

            foreach (var story in container.stories)
            {
                Debug.Log($"[StoryDataDebugger] - storyId: {story.storyId}, storyName: {story.storyName}, 对话数: {story.dialogues.Count}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[StoryDataDebugger] JSON 解析失败：{e.Message}");
        }
    }
}
