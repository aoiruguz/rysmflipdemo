using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public Animator animator;

    private void Awake()
    {
        // 让这个 Canvas 在场景切换时不被销毁
        DontDestroyOnLoad(gameObject);
    }

    // �ⲿ���õļ��ؽӿ�
    public void StartTransition(string sceneName)
    {
        StartCoroutine(LoadLevel(sceneName));
    }

    IEnumerator LoadLevel(string sceneName)
    {
        // 1. 同时触发动画和场景加载
        animator.SetTrigger("Start");

        // 立即开始异步加载场景（后台静默加载）
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // 阻止自动跳转

        // 等待一帧确保 Trigger 被处理
        yield return null;

        // 2. 追踪两个完成状态
        bool animationComplete = false;
        bool sceneLoadComplete = false;

        // 3. 循环检查两个条件
        while (!animationComplete || !sceneLoadComplete)
        {
            // 检查动画是否播放完成
            if (!animationComplete)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= 1.0f)
                {
                    animationComplete = true;
                }
            }

            // 检查场景是否加载完成（progress 达到 0.9 表示加载完成）
            if (!sceneLoadComplete)
            {
                if (operation.progress >= 0.9f)
                {
                    sceneLoadComplete = true;
                }
            }

            yield return null;
        }

        // 4. 两个条件都满足后，允许场景跳转
        // 修复：确保在切换场景前恢复时间缩放，防止从暂停状态（如设置面板）切换时导致新场景时间静止
        Time.timeScale = 1f;
        operation.allowSceneActivation = true;

        // 5. 等待场景真正切换完成后，播放结束动画
        yield return null;
        animator.SetTrigger("End");
    }
}