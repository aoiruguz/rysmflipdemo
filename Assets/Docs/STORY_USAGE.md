# 剧情系统使用指南

## 概述

剧情系统已重构为**事件驱动**模式，可以在任何地方调用显示对话。

---

## 核心组件

### 1. DialogueData（对话数据）
位置：`Assets/Scripts/DialogueData.cs`

```csharp
public class DialogueData
{
    public int panelIndex = 1;      // 1 或 2，在哪个 Panel 显示
    public string speakerName = ""; // 说话者名字
    public string text = "";        // 对话内容
}
```

### 2. StoryManager（剧情管理器）
位置：`Assets/Scripts/StorySystem/StoryManager.cs`

**功能**：
- 管理两个对话 Panel 的显示
- 支持单句或多句对话序列
- 支持跳过功能
- 事件驱动，可在任何地方调用

---

## 使用方法

### 方法 1：在 LevelData 中配置开场剧情

1. **在 LevelData 中配置**：
   - 打开你的 LevelData ScriptableObject
   - 找到"开场剧情"部分
   - 添加对话到 `openingDialogues` 数组
   - 设置 `dialogDisplayDuration`（每句显示时长）
   - 设置 `dialogTransitionDelay`（对话切换延迟）

2. **自动播放**：
   - PlaySceneInitializer 会自动读取并播放
   - 已通关关卡自动显示跳过按钮

### 方法 2：在代码中动态调用

#### 显示单句对话
```csharp
// 在 Panel 1 显示对话
StoryManager.Instance.ShowDialogue(1, "角色名", "这是对话内容", 3f);

// 在 Panel 2 显示对话
StoryManager.Instance.ShowDialogue(2, "另一个角色", "这是另一句对话", 3f);
```

#### 显示多句对话序列
```csharp
DialogueData[] dialogues = new DialogueData[]
{
    new DialogueData { panelIndex = 1, speakerName = "主角", text = "你好！" },
    new DialogueData { panelIndex = 2, speakerName = "敌人", text = "来战吧！" },
    new DialogueData { panelIndex = 1, speakerName = "主角", text = "好的！" }
};

// 播放对话序列
StoryManager.Instance.ShowDialogues(
    dialogues,
    showSkipButton: true,      // 是否显示跳过按钮
    displayDuration: 3f,       // 每句显示时长
    transitionDelay: 0.5f,     // 对话切换延迟
    onComplete: () => {
        Debug.Log("对话播放完成！");
        // 这里可以执行后续逻辑
    }
);
```

#### 监听对话事件
```csharp
void Start()
{
    // 订阅对话开始事件
    StoryManager.Instance.OnDialogueStart += OnDialogueStarted;
    
    // 订阅对话结束事件
    StoryManager.Instance.OnDialogueComplete += OnDialogueCompleted;
}

void OnDialogueStarted()
{
    Debug.Log("对话开始了！");
    // 例如：暂停游戏、隐藏UI等
}

void OnDialogueCompleted()
{
    Debug.Log("对话结束了！");
    // 例如：恢复游戏、显示UI等
}

void OnDestroy()
{
    // 取消订阅
    if (StoryManager.Instance != null)
    {
        StoryManager.Instance.OnDialogueStart -= OnDialogueStarted;
        StoryManager.Instance.OnDialogueComplete -= OnDialogueCompleted;
    }
}
```

#### 其他功能
```csharp
// 检查是否正在播放对话
bool isPlaying = StoryManager.Instance.IsPlayingDialogue();

// 停止当前对话
StoryManager.Instance.StopDialogue();

// 隐藏所有对话面板
StoryManager.Instance.HideAllPanels();
```

---

## 场景配置

### 在 PlayScene 中设置

1. **创建 StoryManager GameObject**：
   - 在场景中创建空物体，命名为 "StoryManager"
   - 添加 `StoryManager` 组件

2. **配置 UI 引用**：
   - `dialogPanel1`: 第一个对话面板（GameObject）
   - `dialogText1`: 对话文本（TextMeshProUGUI）
   - `speakerName1`: 说话者名字（TextMeshProUGUI）
   - `dialogPanel2`: 第二个对话面板（GameObject）
   - `dialogText2`: 对话文本（TextMeshProUGUI）
   - `speakerName2`: 说话者名字（TextMeshProUGUI）
   - `skipButton`: 跳过按钮（Button）
   - `skipButtonObject`: 跳过按钮的 GameObject

3. **配置默认设置**：
   - `defaultDialogDisplayDuration`: 默认每句显示时长（秒）
   - `defaultDialogTransitionDelay`: 默认对话切换延迟（秒）

4. **添加 PlaySceneInitializer**（可选）：
   - 如果需要自动播放开场剧情
   - 添加 `PlaySceneInitializer` 组件
   - 配置引用：storyManager, noteManager, gameplayUI

---

## UI 设计建议

### 对话面板布局
```
Canvas
├── DialogPanel1 (GameObject)
│   ├── Background (Image)
│   ├── SpeakerName (TextMeshProUGUI)
│   └── DialogText (TextMeshProUGUI)
├── DialogPanel2 (GameObject)
│   ├── Background (Image)
│   ├── SpeakerName (TextMeshProUGUI)
│   └── DialogText (TextMeshProUGUI)
└── SkipButton (Button)
    └── Text (TextMeshProUGUI) "跳过"
```

### 建议样式
- **Panel 1**：左侧或下方，主角对话
- **Panel 2**：右侧或上方，敌人/NPC 对话
- **跳过按钮**：右上角，半透明

---

## 使用示例

### 示例 1：Boss 战开场剧情
```csharp
void StartBossBattle()
{
    DialogueData[] bossIntro = new DialogueData[]
    {
        new DialogueData { panelIndex = 2, speakerName = "Boss", text = "你终于来了..." },
        new DialogueData { panelIndex = 1, speakerName = "主角", text = "我会打败你！" },
        new DialogueData { panelIndex = 2, speakerName = "Boss", text = "那就来试试吧！" }
    };

    StoryManager.Instance.ShowDialogues(bossIntro, false, 2.5f, 0.3f, () => {
        // 剧情结束，开始战斗
        StartBattle();
    });
}
```

### 示例 2：关卡中途触发剧情
```csharp
void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        // 暂停游戏
        Time.timeScale = 0f;
        
        // 显示对话
        StoryManager.Instance.ShowDialogue(1, "提示", "前方危险！", 2f);
        
        // 2秒后恢复游戏
        StartCoroutine(ResumeGameAfterDelay(2f));
    }
}

IEnumerator ResumeGameAfterDelay(float delay)
{
    yield return new WaitForSecondsRealtime(delay);
    Time.timeScale = 1f;
}
```

### 示例 3：结算后显示剧情
```csharp
void OnGameComplete()
{
    // 显示结算界面
    ShowResultScreen();
    
    // 如果是 S 评级，显示特殊剧情
    if (rank == "S")
    {
        DialogueData[] victoryDialogue = new DialogueData[]
        {
            new DialogueData { panelIndex = 2, speakerName = "Boss", text = "你...赢了..." },
            new DialogueData { panelIndex = 1, speakerName = "主角", text = "这只是开始！" }
        };
        
        StoryManager.Instance.ShowDialogues(victoryDialogue, true, 3f, 0.5f);
    }
}
```

---

## 注意事项

1. **时间控制**：
   - 对话播放时使用 `Time.unscaledDeltaTime`，不受 `Time.timeScale` 影响
   - 如果需要暂停游戏，手动设置 `Time.timeScale = 0f`

2. **面板管理**：
   - 不需要"剧情总面板"，直接控制两个对话 Panel
   - 对话结束后会自动隐藏所有面板

3. **跳过功能**：
   - 只在需要时显示跳过按钮（通过参数控制）
   - 跳过会立即结束对话并触发完成回调

4. **事件订阅**：
   - 记得在 `OnDestroy` 中取消订阅事件，避免内存泄漏

---

## 调试

```csharp
// 测试对话系统
void TestDialogue()
{
    StoryManager.Instance.ShowDialogue(1, "测试", "这是测试对话", 2f);
}

// 在 Inspector 中添加按钮调用此方法进行测试
```

---

## 未来扩展

- 支持对话动画（淡入淡出、打字机效果）
- 支持角色立绘显示
- 支持语音播放
- 支持选项分支
- 支持表情图标
