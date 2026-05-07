using UnityEngine;

public class LevelNode : MonoBehaviour
{
    [Header("关联该关卡的数据文件")]
    public LevelData levelData;

    private void OnMouseDown()
    {
        // 确保你已经创建了 LevelUIManager 并且它是单例
        if (levelData != null && LevelUIManager.Instance != null)
        {
            LevelUIManager.Instance.ShowLevelDetails(levelData);
        }
        else
        {
            Debug.LogWarning("未挂载 LevelData 或场景中缺少 LevelUIManager");
        }
    }
}