using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [Header("场景设置")]
    [Tooltip("要加载的场景名称")]
    public string targetSceneName;

    [Header("可选：自动绑定按钮")]
    [Tooltip("如果设置，会自动绑定按钮的点击事件")]
    public Button button;

    [Header("可选：过渡动画管理器")]
    [Tooltip("如果设置，会使用过渡动画；否则直接加载场景")]
    public TransitionManager transitionManager;

    private void Start()
    {
        // 如果指定了按钮，自动绑定点击事件
        if (button != null)
        {
            button.onClick.AddListener(LoadScene);
        }
    }

    // 供 Button 的 OnClick 事件调用
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[SceneLoader] 未设置目标场景名称！");
            return;
        }

        Debug.Log($"[SceneLoader] 开始加载场景: {targetSceneName}");

        // 如果有 TransitionManager，使用过渡动画
        if (transitionManager != null)
        {
            transitionManager.StartTransition(targetSceneName);
        }
        else
        {
            // 否则直接加载场景
            // 修复：确保在切换场景前恢复时间缩放，防止从暂停状态切换时导致新场景时间静止
            Time.timeScale = 1f;
            StartCoroutine(LoadSceneAsync());
        }
    }

    // 异步加载场景
    private System.Collections.IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            // 加载进度: asyncLoad.progress (0-0.9, 完成时为1.0)
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // 这里可以调用转场特效或更新进度条
            OnLoadingProgress(progress);

            yield return null;
        }

        Debug.Log($"[SceneLoader] 场景加载完成: {targetSceneName}");
    }

    // 加载进度回调（可被子类重写或用于触发事件）
    protected virtual void OnLoadingProgress(float progress)
    {
        // 子类可以重写此方法来实现自定义的加载进度显示
        // 或者在这里添加 UnityEvent 来触发转场特效
    }
}
