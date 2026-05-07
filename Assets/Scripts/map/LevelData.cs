using UnityEngine;

[System.Serializable]
public struct DifficultyDetail
{
    public ChartDifficulty difficulty; // 对应枚举：Easy, Normal, Hard
    public int ratingLevel;           // 难度评级，例如 1, 5, 9
    public string mapper;             // 谱师
    public ChartData chartAsset;      // 对应已经创建的 ChartData SO
}

[CreateAssetMenu(fileName = "NewLevel", menuName = "RhythmGame/LevelData")]
public class LevelData : ScriptableObject
{
    [Header("--- 基础信息 ---")]
    public string levelName;          // 关卡显示名称
    public string composer;           // 曲师
    public float displayBPM;          // UI显示的BPM
    public AudioClip previewMusic;    // 预览音乐（展开面板时播放）

    [Header("--- 敌人与剧情 ---")]
    public Sprite enemyAvatar;        // 敌人头像
    [TextArea] public string enemyInfo; // 敌人信息

    [Header("--- 状态与解锁 ---")]
    public bool isLockedByDefault = true;
    [Tooltip("解锁所需粉丝量，负数表示默认解锁")]
    public int requiredFans = -1;

    [Header("--- 难度配置 (数组按 E/N/H 顺序填写) ---")]
    public DifficultyDetail[] difficulties = new DifficultyDetail[3];

    // 获取最佳成绩的方法，通常通过 SaveManager 获取，因为这是纯数据 SO。
    // 因为 SO 在磁盘上是只读的。
    public int GetBestScore(ChartDifficulty diff)
    {
        // 示例：PlayerPrefs.GetInt(levelName + diff.ToString() + "_BestScore", 0);
        return 0;
    }

    // 检查是否解锁
    public bool IsUnlocked(int currentFans)
    {
        if (requiredFans < 0)
            return true; // 负数表示默认解锁
        return currentFans >= requiredFans;
    }

    // 获取解锁条件文本
    public string GetUnlockConditionText()
    {
        if (requiredFans < 0)
            return "已解锁";
        return $"粉丝量达到 {requiredFans} 时解锁";
    }
}
