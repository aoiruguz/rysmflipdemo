using UnityEngine;
using UnityEngine.SceneManagement; // 必须引用这个命名空间

public class SceneSwitcher : MonoBehaviour
{
    // 在 Inspector 面板中手动输入要跳转的场景名称
    public string targetSceneName;

    public void LoadAssignedScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("未指定目标场景名称！");
        }
    }
}