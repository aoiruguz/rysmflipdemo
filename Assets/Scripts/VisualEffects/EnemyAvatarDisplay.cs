using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 敌人头像显示器
/// 根据加载的 LevelData 自动设置敌人头像和名字
/// </summary>
public class EnemyAvatarDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("用于显示敌人头像的 Image 组件")]
    public Image enemyAvatarImage;

    [Tooltip("用于显示敌人名字的 TextMeshProUGUI 组件")]
    public TextMeshProUGUI enemyNameText;

    [Tooltip("敌人头像的 Animator 组件")]
    public Animator enemyAvatarAnimator;

    [Header("Settings")]
    [Tooltip("如果未找到 LevelData，是否隐藏头像")]
    public bool hideIfNoData = false;

    void Start()
    {
        LoadAndDisplayEnemyAvatar();
    }

    /// <summary>
    /// 加载并显示敌人头像和名字
    /// </summary>
    public void LoadAndDisplayEnemyAvatar()
    {
        // 从 LevelUIManager 获取当前关卡数据
        LevelData currentLevelData = LevelUIManager.GetCurrentLevelData();

        if (currentLevelData == null)
        {
            Debug.LogWarning("[EnemyAvatarDisplay] 未找到当前关卡数据！");

            if (hideIfNoData)
            {
                if (enemyAvatarImage != null)
                    enemyAvatarImage.gameObject.SetActive(false);
                if (enemyNameText != null)
                    enemyNameText.gameObject.SetActive(false);
            }
            return;
        }

        // 设置敌人头像
        if (enemyAvatarImage != null)
        {
            Sprite enemyAvatar = currentLevelData.GetCurrentEnemyAvatar();

            if (enemyAvatar != null)
            {
                enemyAvatarImage.sprite = enemyAvatar;
                enemyAvatarImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[EnemyAvatarDisplay] 关卡 {currentLevelData.levelName} 没有设置敌人头像！");

                if (hideIfNoData)
                {
                    enemyAvatarImage.gameObject.SetActive(false);
                }
            }
        }

        // 设置敌人名字
        if (enemyNameText != null)
        {
            if (!string.IsNullOrEmpty(currentLevelData.enemyName))
            {
                enemyNameText.text = currentLevelData.enemyName;
                enemyNameText.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"[EnemyAvatarDisplay] 关卡 {currentLevelData.levelName} 没有设置敌人名字！");

                if (hideIfNoData)
                {
                    enemyNameText.gameObject.SetActive(false);
                }
            }
        }

        // 设置敌人头像动画控制器
        if (enemyAvatarAnimator != null)
        {
            if (currentLevelData.enemyAvatarAnimator != null)
            {
                enemyAvatarAnimator.runtimeAnimatorController = currentLevelData.enemyAvatarAnimator;
                Debug.Log($"[EnemyAvatarDisplay] 已设置敌人头像动画控制器: {currentLevelData.enemyAvatarAnimator.name}");
            }
            else
            {
                Debug.LogWarning($"[EnemyAvatarDisplay] 关卡 {currentLevelData.levelName} 没有设置敌人头像动画控制器！");
            }
        }

        Debug.Log($"[EnemyAvatarDisplay] 已设置敌人信息 - 名字: {currentLevelData.enemyName}");
    }

    /// <summary>
    /// 手动刷新头像显示（用于运行时更新）
    /// </summary>
    public void RefreshAvatar()
    {
        LoadAndDisplayEnemyAvatar();
    }
}
