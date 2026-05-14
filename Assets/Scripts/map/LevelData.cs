using UnityEngine;

[System.Serializable]
public struct DifficultyDetail
{
    public ChartDifficulty difficulty; // 对应枚举：Easy, Normal, Hard
    public int ratingLevel;           // 难度评级，例如 1, 5, 9
    public string mapper;             // 谱师
    public ChartData chartAsset;      // 对应已经创建的 ChartData SO
}

[System.Serializable]
public struct BackgroundMaterialSettings
{
    [Header("动画参数")]
    [Tooltip("波动速度")]
    public float wobbleSpeed;

    [Tooltip("波动幅度")]
    public float wobbleAmount;

    [Tooltip("扩张速度")]
    public float expansionSpeed;

    [Tooltip("环密度")]
    public float ringDensity;

    [Header("颜色配置")]
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;
    public Color color5;

    public static BackgroundMaterialSettings Default()
    {
        return new BackgroundMaterialSettings
        {
            wobbleSpeed = 5.15f,
            wobbleAmount = 0.04f,
            expansionSpeed = 1.75f,
            ringDensity = 15.32f,
            color1 = new Color(0.965f, 0.765f, 0.969f, 1f),
            color2 = new Color(0.918f, 0.604f, 0.941f, 1f),
            color3 = new Color(0.784f, 0.537f, 0.980f, 1f),
            color4 = new Color(0.471f, 0.494f, 0.992f, 1f),
            color5 = new Color(0.310f, 0.549f, 0.992f, 1f)
        };
    }
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
    [Tooltip("敌人头像（未击败时）")]
    public Sprite enemyAvatar;
    [Tooltip("敌人头像（击败后）")]
    public Sprite enemyAvatarDefeated;
    [TextArea] public string enemyInfo; // 敌人信息
    [Tooltip("敌人的粉丝数（战斗力）")]
    public long enemyFans = 10000;    // 敌人的粉丝数

    [Header("--- 敌人称号 ---")]
    [Tooltip("未击败时显示的称号")]
    public string enemyTitleUncleared = "挑战者";
    [Tooltip("击败后显示的称号（例如：前挑战者）")]
    public string enemyTitleCleared = "已击败";

    [Header("--- 关卡分组 ---")]
    [Tooltip("关卡组编号（1-4）")]
    public int chapterGroup = 1;
    [Tooltip("是否是主线关卡")]
    public bool isMainStoryLevel = false;
    [Tooltip("解锁所需的前置关卡组（0 表示默认解锁）")]
    public int requiredChapterGroup = 0;
    [Tooltip("解锁该组所需的粉丝数")]
    public long requiredFansForChapter = 0;

    [Header("--- 状态与解锁 ---")]
    public bool isLockedByDefault = true;
    [Tooltip("解锁所需粉丝量，负数表示默认解锁")]
    public int requiredFans = -1;

    [Header("--- 难度配置 (数组按 E/N/H 顺序填写) ---")]
    public DifficultyDetail[] difficulties = new DifficultyDetail[3];

    [Header("--- 开场剧情 ---")]
    [Tooltip("开场剧情对话列表")]
    public DialogueData[] openingDialogues = new DialogueData[0];

    [Tooltip("每句对话显示时长（秒）")]
    public float dialogDisplayDuration = 3f;

    [Tooltip("对话切换延迟（秒）")]
    public float dialogTransitionDelay = 0.5f;

    [Header("--- 通关剧情 ---")]
    [Tooltip("通关后播放的剧情ID（对应 stories.json 中的 storyId）")]
    public string clearStoryId = "";

    [Header("--- 干扰系统 ---")]
    [Tooltip("干扰技能触发配置列表（配置触发时机和技能类型）")]
    public InterferenceTrigger[] interferenceTriggers = new InterferenceTrigger[0];

    [Tooltip("弹幕文本配置")]
    public InterferenceTextConfig interferenceTextConfig;

    [Header("--- 背景材质配置 ---")]
    [Tooltip("关卡背景材质参数（控制 UI_BG_normal 材质的颜色和动画）")]
    public BackgroundMaterialSettings backgroundMaterial = BackgroundMaterialSettings.Default();

    [Header("--- 干扰系统（已弃用） ---")]
    [Tooltip("干扰技能触发次数（已弃用，请使用 interferenceTriggers）")]
    public int interferenceCount = 0;

    // 获取最佳成绩的方法，通常通过 SaveManager 获取，因为这是纯数据 SO。
    // 因为 SO 在磁盘上是只读的。
    public int GetBestScore(ChartDifficulty diff)
    {
        // 示例：PlayerPrefs.GetInt(levelName + diff.ToString() + "_BestScore", 0);
        return 0;
    }

    // 检查是否解锁
    public bool IsUnlocked(long currentFans)
    {
        if (requiredFans < 0)
            return true; // 负数表示默认解锁
        return currentFans >= requiredFans;
    }

    // 检查关卡组是否解锁
    public bool IsChapterUnlocked(long currentFans)
    {
        // 第一组默认解锁
        if (requiredChapterGroup == 0)
            return true;

        // 检查前置关卡组的主线是否通关
        bool previousChapterCleared = CheckPreviousChapterCleared();

        // 检查粉丝数是否达标
        bool fansEnough = currentFans >= requiredFansForChapter;

        return previousChapterCleared && fansEnough;
    }

    // 检查前置关卡组的主线关卡是否通关
    private bool CheckPreviousChapterCleared()
    {
        if (SaveManager.Instance == null)
            return false;

        // 需要检查前置关卡组的主线关卡是否通关
        // 这里需要通过 SaveManager 查询
        return SaveManager.Instance.IsChapterMainStoryCleared(requiredChapterGroup);
    }

    // 获取解锁条件文本
    public string GetUnlockConditionText()
    {
        if (requiredFans < 0)
            return "已解锁";
        return $"粉丝量达到 {requiredFans} 时解锁";
    }

    // 获取关卡组解锁条件文本
    public string GetChapterUnlockConditionText()
    {
        if (requiredChapterGroup == 0)
            return "默认解锁";

        string condition = $"需要：\n";
        condition += $"1. 通关第 {requiredChapterGroup} 组主线关卡\n";
        condition += $"2. 粉丝数达到 {requiredFansForChapter}";

        return condition;
    }

    // 获取当前应该显示的敌人称号
    public string GetCurrentEnemyTitle()
    {
        if (SaveManager.Instance == null)
            return enemyTitleUncleared;

        // 使用存档系统的击败状态判断
        return SaveManager.Instance.IsEnemyDefeated(levelName) ? enemyTitleCleared : enemyTitleUncleared;
    }

    // 获取当前应该显示的敌人头像
    public Sprite GetCurrentEnemyAvatar()
    {
        if (SaveManager.Instance == null)
            return enemyAvatar;

        // 使用存档系统的击败状态判断
        bool isDefeated = SaveManager.Instance.IsEnemyDefeated(levelName);

        // 如果击败后头像未设置，则使用原头像
        if (isDefeated && enemyAvatarDefeated != null)
            return enemyAvatarDefeated;

        return enemyAvatar;
    }
}
