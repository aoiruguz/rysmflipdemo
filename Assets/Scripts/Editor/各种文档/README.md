# 使用说明

## 新增功能

### 1. 统一存档系统 (SaveManager)

**位置**: `Assets/Scripts/SaveSystem/`

**功能**:
- 统一管理所有游戏数据（设置、关卡进度、粉丝数等）
- 使用 JSON 格式保存到本地文件
- 自动在游戏启动时加载存档

**保存的数据**:
- **游戏设置**: 音量、延迟、分辨率、输入模式等
- **关卡进度**: 每个关卡的最高分、评级、判定统计、通关状态
- **全局数据**: 总粉丝数、解锁章节

**使用方法**:
```csharp
// 获取关卡最高分
int highScore = SaveManager.Instance.GetLevelHighScore(levelName, songName, difficulty);

// 获取关卡评级
string rank = SaveManager.Instance.GetLevelRank(levelName, songName, difficulty);

// 检查是否已通关
bool isCleared = SaveManager.Instance.IsLevelCleared(levelName, songName, difficulty);

// 保存关卡结果（自动在游戏结束时调用）
SaveManager.Instance.SaveLevelResult(levelName, songName, difficulty, score, rank, ...);

// 手动保存游戏
SaveManager.Instance.SaveGame();
```

---

### 2. 剧情系统 (StoryManager)

**位置**: `Assets/Scripts/StorySystem/`

**功能**:
- 在 PlayScene 开始前播放开场剧情
- 支持两个 UI Panel 交替显示对话
- 已通关关卡自动显示"跳过"按钮
- 剧情播放时暂停游戏时间

**组件**:
1. **LevelStoryData** (ScriptableObject): 为每个关卡配置剧情内容
2. **StoryManager**: 负责播放剧情的管理器
3. **PlaySceneInitializer**: 在 PlayScene 中初始化剧情和游戏

**配置剧情**:
1. 在 Unity 中创建 `LevelStoryData` ScriptableObject:
   - 右键 → Create → RhythmGame → LevelStoryData
2. 配置剧情内容:
   - `levelName`: 关卡名称（用于存档识别）
   - `songName`: 歌曲名称
   - `openingStory.dialogues`: 对话列表
     - `panelIndex`: 1 或 2（在哪个 Panel 显示）
     - `speakerName`: 说话者名字
     - `text`: 对话内容

**在 PlayScene 中设置**:
1. 在场景中添加 `StoryManager` GameObject
2. 配置 UI 引用:
   - `storyPanel`: 剧情总面板
   - `dialogPanel1` 和 `dialogPanel2`: 两个对话面板
   - `dialogText1/2`: 对话文本
   - `speakerName1/2`: 说话者名字文本
   - `skipButton`: 跳过按钮
3. 添加 `PlaySceneInitializer` 组件
4. 配置引用:
   - `levelStoryData`: 关卡剧情数据
   - `storyManager`: StoryManager 引用
   - `noteManager`: NoteManager 引用
   - `gameplayUI`: 游戏 UI（剧情播放时会隐藏）

---

### 3. 整合到现有系统

**修改的文件**:
- `LevelUIManager.cs`: 从 SaveManager 读取最高分和评级
- `PlayDataCollector.cs`: 游戏结束时保存结果到 SaveManager
- `GameBootstrap.cs`: 游戏启动时自动创建 SaveManager

**自动功能**:
- 游戏启动时自动加载存档
- 游戏结束时自动保存结果
- 关卡选择界面自动显示最高分和评级
- 已通关关卡自动显示跳过按钮

---

## 使用流程

### 玩家流程:
1. **选择关卡** → LevelUIManager 显示最高分和评级
2. **点击开始** → 跳转到 PlayScene
3. **播放剧情** → 如果已通关，显示跳过按钮
4. **开始游戏** → 正常游玩
5. **游戏结束** → 自动保存结果到 SaveManager
6. **显示结算** → 显示本次成绩和最高分对比

### 开发者配置:
1. 为每个关卡创建 `LevelStoryData` ScriptableObject
2. 在 PlayScene 中添加 `StoryManager` 和 `PlaySceneInitializer`
3. 配置 UI 引用和剧情数据
4. 完成！系统会自动处理存档和剧情

---

## 存档文件位置

**路径**: `Application.persistentDataPath/gamesave.json`

**Windows**: `C:\Users\<用户名>\AppData\LocalLow\<公司名>\<游戏名>\gamesave.json`

**格式**: JSON（可读可编辑）

---

## 调试功能

```csharp
// 重置所有存档数据
SaveManager.Instance.ResetAllData();

// 查看存档文件路径
Debug.Log(Application.persistentDataPath);
```

---

## 注意事项

1. **关卡名称一致性**: 确保 `LevelData.levelName` 和 `LevelStoryData.levelName` 一致
2. **SaveManager 单例**: SaveManager 会在游戏启动时自动创建，无需手动添加到场景
3. **剧情 UI**: 需要在 PlayScene 中手动创建剧情 UI（两个 Panel + 文本 + 跳过按钮）
4. **兼容性**: 新系统与旧的 PlayerPrefs 系统并存，可以逐步迁移

---

## 未来扩展

- 支持更多剧情类型（结局剧情、中场剧情等）
- 支持剧情动画和特效
- 支持语音播放
- 支持多语言
- 云存档同步
