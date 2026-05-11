# AVG剧情系统 - 完整配置指南

## 📦 系统组成

### 核心文件
```
Assets/Scripts/StorySystem/
├── StoryData.cs              # 数据结构定义
├── AVGStoryManager.cs        # 剧情管理器（单例）
├── StoryPlayer.cs            # UI播放器组件
├── StoryTrigger.cs           # 触发器组件
└── Editor/
    └── StoryPlayerUICreator.cs  # UI快速创建工具

Assets/Scripts/SaveSystem/
├── SaveData.cs               # 已扩展：添加剧情进度字段
└── SaveManager.cs            # 已扩展：添加剧情相关方法

Assets/Resources/StoryData/
└── stories.json              # 剧情数据文件（JSON格式）
```

---

## 🚀 快速开始（3步配置）

### 第1步：创建AVGStoryManager

**方法1：使用菜单工具（推荐）**
```
Hierarchy 右键 → Story System → Create AVG Story Manager
```
- 自动创建 AVGStoryManager GameObject
- 自动查找并关联 StoryPlayer（如果已存在）

**方法2：手动创建**
```
1. Hierarchy → Create Empty
2. 命名为 "AVGStoryManager"
3. 添加组件：AVGStoryManager.cs
```

**Inspector配置：**
- `Story Player` - 留空（稍后自动关联）
- `Story Data File Name` - 默认 "stories"（对应 Resources/StoryData/stories.json）

---

### 第2步：创建StoryPlayer UI

**方法1：使用菜单工具（推荐）**
```
Hierarchy 右键 → Story System → Create Story Player UI
```

**自动创建的UI结构：**
```
Canvas (如果不存在会自动创建)
└── StoryPanel (挂载 StoryPlayer.cs)
    ├── Background (Image - 半透明黑色背景)
    ├── CharacterImage (Image - 角色立绘)
    ├── DialogueBox (Image - 对话框背景)
    │   ├── NameText (TextMeshProUGUI - 角色名)
    │   └── DialogText (TextMeshProUGUI - 对话内容)
    ├── NextButton (Button - "继续"按钮)
    └── SkipButton (Button - "跳过"按钮)
```

**所有引用已自动绑定！** ✅

**方法2：手动创建**
如果需要自定义UI样式，可以手动创建并在StoryPlayer组件中绑定引用。

---

### 第3步：关联组件

1. 选中 `AVGStoryManager` GameObject
2. 在Inspector中，将 `StoryPanel` 拖到 `Story Player` 字段
3. 完成！

---

## 📝 编辑剧情数据

### JSON文件位置
```
Assets/Resources/StoryData/stories.json
```

### JSON格式说明
```json
{
  "stories": [
    {
      "storyId": "唯一ID",
      "storyName": "剧情名称（用于调试）",
      "triggerType": 0,
      "triggerCondition": "触发条件",
      "dialogues": [
        {
          "characterName": "角色名",
          "dialogueText": "对话内容",
          "characterSpritePath": "角色立绘路径",
          "backgroundSpritePath": "背景图路径",
          "audioClipPath": "音效路径（可选）"
        }
      ]
    }
  ]
}
```

### 字段详解

#### triggerType（触发类型）
- `0` = SceneEnter（场景进入时触发）
- `1` = LevelComplete（关卡完成后触发）
- `2` = Condition（满足特定条件时触发）

#### triggerCondition（触发条件）
- 场景进入：填写场景名称，如 `"Big Map"`
- 关卡完成：填写关卡ID，如 `"Level_01"`
- 条件触发：填写自定义条件ID

#### 资源路径规则
- **相对于 `Resources/` 文件夹**
- **不包含文件扩展名**
- 示例：
  - 文件位置：`Assets/Resources/Characters/Player.png`
  - 填写路径：`"Characters/Player"`
- **留空字符串 `""` 表示不显示该元素**

---

## 🎮 使用方法

### 方法1：使用触发器组件（推荐）

**适用场景：** 场景加载时自动触发剧情

**步骤：**
1. 在场景中创建空物体（或使用现有物体）
2. 添加组件：`StoryTrigger.cs`
3. 配置Inspector：
   - `Story Id` = `"BigMap_Enter"`（对应JSON中的storyId）
   - `Trigger Moment` = `OnStart`（场景启动时触发）
   - `Trigger Once` = ✅（只触发一次）
4. 完成！

**触发时机选项：**
- `OnStart` - 场景Start时自动触发
- `OnEnable` - 组件Enable时触发
- `Manual` - 手动调用（需要代码）

---

### 方法2：代码手动触发

**适用场景：** 关卡完成后、特定事件发生时

#### 示例1：关卡完成后播放剧情
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteHandler : MonoBehaviour
{
    void OnLevelComplete()
    {
        // 播放剧情，完成后跳转场景
        AVGStoryManager.Instance.CheckAndPlayStory("Level_01_Complete", () =>
        {
            // 剧情播放完成后的回调
            SceneManager.LoadScene("Big Map");
        });
    }
}
```

#### 示例2：强制播放剧情（无论是否看过）
```csharp
// 用于回顾剧情、调试等场景
AVGStoryManager.Instance.ForcePlayStory("BigMap_Enter", () =>
{
    Debug.Log("剧情播放完成");
});
```

#### 示例3：检测剧情是否已观看
```csharp
if (SaveManager.Instance.IsStoryWatched("BigMap_Enter"))
{
    Debug.Log("玩家已经看过这段剧情");
}
else
{
    Debug.Log("这是新剧情");
}
```

---

## 🎨 自定义UI样式

### 修改对话框样式
选中 `StoryPanel/DialogueBox`，调整：
- `Image.Color` - 对话框背景色
- `RectTransform` - 位置和大小

### 修改文字样式
选中 `NameText` 或 `DialogText`，调整：
- `Font Asset` - 字体
- `Font Size` - 字号
- `Color` - 颜色
- `Alignment` - 对齐方式

### 修改按钮样式
选中 `NextButton` 或 `SkipButton`，调整：
- `Image.Color` - 按钮背景色
- 子对象 `Text` 的文字样式

### 修改打字机速度
选中 `StoryPanel`，在 `StoryPlayer` 组件中：
- `Typewriter Speed` - 每个字的间隔时间（秒）
- 默认 `0.05` = 每个字间隔0.05秒

---

## 📂 添加角色立绘和背景图

### 1. 准备资源
将图片放入 `Assets/Resources/` 文件夹下，例如：
```
Assets/Resources/
├── Characters/
│   ├── Player.png
│   ├── Manager.png
│   └── Fan.png
└── Backgrounds/
    ├── BigMap.png
    └── Stage.png
```

### 2. 在JSON中引用
```json
{
  "characterName": "主角",
  "dialogueText": "你好！",
  "characterSpritePath": "Characters/Player",
  "backgroundSpritePath": "Backgrounds/BigMap",
  "audioClipPath": ""
}
```

### 3. 不显示立绘或背景
留空字符串即可：
```json
{
  "characterSpritePath": "",
  "backgroundSpritePath": ""
}
```

---

## 🔧 常见问题

### Q1: 剧情不播放？
**检查清单：**
1. ✅ AVGStoryManager 存在于场景中
2. ✅ StoryPlayer 已关联到 AVGStoryManager
3. ✅ JSON文件路径正确：`Resources/StoryData/stories.json`
4. ✅ storyId 拼写正确
5. ✅ 查看Console是否有错误日志

### Q2: 剧情已看过，如何重置？
**方法1：代码重置**
```csharp
SaveManager.Instance.ResetAllStoryProgress();
```

**方法2：删除存档文件**
删除 `Application.persistentDataPath/gamesave.json`

### Q3: 如何让剧情每次都播放？
使用 `ForcePlayStory` 而不是 `CheckAndPlayStory`：
```csharp
AVGStoryManager.Instance.ForcePlayStory("storyId");
```

### Q4: 如何在剧情播放时暂停游戏？
在 `StoryPlayer.cs` 的 `Play` 方法开始时添加：
```csharp
Time.timeScale = 0f;
```
在 `EndStory` 方法中恢复：
```csharp
Time.timeScale = 1f;
```

### Q5: 如何添加音效？
1. 将音效文件放入 `Resources/Audio/` 文件夹
2. 在JSON中填写路径：`"audioClipPath": "Audio/dialogue_sound"`
3. 在 `StoryPlayer.cs` 中添加音效播放逻辑（需要自行扩展）

---

## 📋 完整示例

### 示例JSON（两段剧情）
```json
{
  "stories": [
    {
      "storyId": "BigMap_Enter",
      "storyName": "初入大地图",
      "triggerType": 0,
      "triggerCondition": "Big Map",
      "dialogues": [
        {
          "characterName": "经纪人",
          "dialogueText": "欢迎来到这里！这是你第一次踏上这片舞台。",
          "characterSpritePath": "Characters/Manager",
          "backgroundSpritePath": "Backgrounds/BigMap",
          "audioClipPath": ""
        },
        {
          "characterName": "经纪人",
          "dialogueText": "前方的路还很长，但我相信你一定能成为最耀眼的明星！",
          "characterSpritePath": "Characters/Manager",
          "backgroundSpritePath": "Backgrounds/BigMap",
          "audioClipPath": ""
        },
        {
          "characterName": "主角",
          "dialogueText": "谢谢！我会努力的！",
          "characterSpritePath": "Characters/Player",
          "backgroundSpritePath": "Backgrounds/BigMap",
          "audioClipPath": ""
        }
      ]
    },
    {
      "storyId": "Level_01_Complete",
      "storyName": "完成第一关",
      "triggerType": 1,
      "triggerCondition": "Level_01",
      "dialogues": [
        {
          "characterName": "粉丝",
          "dialogueText": "太棒了！你的表演让我热血沸腾！",
          "characterSpritePath": "Characters/Fan",
          "backgroundSpritePath": "Backgrounds/Stage",
          "audioClipPath": ""
        },
        {
          "characterName": "经纪人",
          "dialogueText": "不错的开始，但这只是第一步。接下来的挑战会更难哦。",
          "characterSpritePath": "Characters/Manager",
          "backgroundSpritePath": "Backgrounds/Stage",
          "audioClipPath": ""
        }
      ]
    }
  ]
}
```

### 示例代码（关卡完成触发）
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    void OnLevelComplete(string levelId)
    {
        // 保存关卡成绩
        SaveManager.Instance.SaveLevelResult(/* 参数 */);

        // 播放剧情
        string storyId = levelId + "_Complete";
        AVGStoryManager.Instance.CheckAndPlayStory(storyId, () =>
        {
            // 剧情结束后返回大地图
            SceneManager.LoadScene("Big Map");
        });
    }
}
```

---

## ✅ 配置完成检查清单

- [ ] AVGStoryManager 已创建并存在于场景中
- [ ] StoryPlayer UI 已创建（使用工具或手动）
- [ ] AVGStoryManager.storyPlayer 已关联
- [ ] stories.json 文件已创建并填写剧情数据
- [ ] 测试：在场景中添加 StoryTrigger 并运行测试
- [ ] 确认剧情可以正常播放
- [ ] 确认跳过按钮功能正常
- [ ] 确认存档系统记录剧情观看状态

---

## 🎯 下一步

1. **编写你的剧情**：编辑 `stories.json` 添加游戏剧情
2. **准备美术资源**：角色立绘、背景图放入 Resources 文件夹
3. **配置触发点**：在关键场景添加 StoryTrigger 或代码触发
4. **测试流程**：完整测试剧情播放、跳过、存档功能

---

**系统已完全配置完成，开始创作你的故事吧！** 🎉
