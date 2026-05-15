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
    [Tooltip("敌人名称")]
    public string enemyName = "对手";

    [Tooltip("敌人头像（未击败时）- 用于详情面板")]
    public Sprite enemyAvatar;
    [Tooltip("敌人头像（击败后）- 用于详情面板")]
    public Sprite enemyAvatarDefeated;

    [Tooltip("大地图小图标（未击败时）")]
    public Sprite mapIconUndefeated;
    [Tooltip("大地图小图标（击败后）")]
    public Sprite mapIconDefeated;

    [TextArea] public string enemyInfo; // 敌人信息
    [Tooltip("敌人的粉丝数（战斗力）")]
    public long enemyFans = 10000;    // 敌人的粉丝数

    [Header("--- 敌人称号 ---")]
    [Tooltip("未击败时显示的称号（首次击败后玩家会获得此称号）")]
    public string enemyTitleUncleared = "挑战者";
    [Tooltip("击败后显示的称号（例如：前挑战者）")]
    public string enemyTitleCleared = "已击败";

    [Header("--- 关卡分组 ---")]
    [Tooltip("关卡组编号（1-4）")]
    public int chapterGroup = 1;
    [Tooltip("是否是主线关卡")]
    public bool isMainStoryLevel = false;

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

    /// <summary>
    /// 检查关卡是否解锁（统一入口）
    /// 同一章节的所有关卡（主线+支线）使用相同的解锁条件
    /// </summary>
    public bool IsUnlocked()
    {
        if (SaveManager.Instance == null)
            return false;

        return SaveManager.Instance.IsChapterUnlocked(chapterGroup);
    }

    /// <summary>
    /// 获取解锁条件文本
    /// </summary>
    public string GetUnlockConditionText()
    {
        if (SaveManager.Instance == null)
            return "未知解锁条件";

        return SaveManager.Instance.GetChapterUnlockConditionText(chapterGroup);
    }

    // 获取当前应该显示的敌人称号
    public string GetCurrentEnemyTitle()
    {
        if (SaveManager.Instance == null)
            return enemyTitleUncleared;

        // 使用存档系统的击败状态判断
        return SaveManager.Instance.IsEnemyDefeated(levelName) ? enemyTitleCleared : enemyTitleUncleared;
    }

    // 获取当前应该显示的敌人头像（详情面板用）
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

    // 获取当前应该显示的大地图小图标
    public Sprite GetCurrentMapIcon()
    {
        if (SaveManager.Instance == null)
            return mapIconUndefeated;

        // 使用存档系统的击败状态判断
        bool isDefeated = SaveManager.Instance.IsEnemyDefeated(levelName);

        // 如果击败后图标未设置，则使用未击败图标
        if (isDefeated && mapIconDefeated != null)
            return mapIconDefeated;

        return mapIconUndefeated;
    }
}
