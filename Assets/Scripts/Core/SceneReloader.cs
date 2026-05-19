using UnityEngine;
using UnityEngine.SceneManagement; // 必须引用此命名空间

public class SceneReloader : MonoBehaviour
{
    void Update()
    {
        // 检测键盘 R 键按下
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadCurrentScene();
        }
    }

    public void ReloadCurrentScene()
    {
        // 获取当前激活场景的名称或索引并重新加载
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);

        Debug.Log($"场景 {activeScene.name} 已重新加载");
    }
}