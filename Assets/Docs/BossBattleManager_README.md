# Boss战系统配置指南

## 概述

`BossBattleManager.cs` 是一个整合了所有Boss战功能的统一管理器。只有当 `LevelData.chapterGroup == 4` 时才会启用Boss战模式。

---

## Unity场景配置步骤

### 1. 添加 BossBattleManager 到场景

1. 在 **BossPlayScene** 或 **PlayScene** 中创建一个空的 GameObject
2. 命名为 `BossBattleManager`
3. 添加 `BossBattleManager.cs` 脚本

---

### 2. 配置字段（Inspector）

#### **Fans Health Settings（粉丝生命值设置）**

| 字段 | 默认值 | 说明 |
|------|--------|------|
| Multiplier | 1000 | 指数倍数 |
| Perfect Fans Change | 100 | Perfect判定：+100,000粉丝 |
| Great Fans Change | -300 | Great判定：-300,000粉丝 |
| Good Fans Change | -600 | Good判定：-600,000粉丝 |
| Miss Fans Change | -600 | Miss判定：-600,000粉丝 |
| Enable Test Mode | false | 测试模式开关 |
| Test Fans Amount | 1000000 | 测试用粉丝数 |

---

#### **UI Panel Management（UI面板管理）**

| 字段 | 说明 | 如何配置 |
|------|------|----------|
| Original Play Count Panel | 原有的播放量显示面板 | 拖拽原有的播放量UI面板 |
| Boss Fans Panel | Boss战专用粉丝数显示面板 | 拖拽你创建的Boss粉丝数面板 |

**重要**：
- `Original Play Count Panel` 在Boss战时会被关闭
- `Boss Fans Panel` 在Boss战时会被开启
- 在非Boss战时，`Boss Fans Panel` 会自动隐藏

---

#### **System References（系统引用）**

| 字段 | 说明 |
|------|------|
| Player Health System | 玩家生命系统（可留空，会自动查找）|

---

#### **Panel Management（面板管理）**

| 字段 | 说明 | 示例 |
|------|------|------|
| Panels To Hide During Boss | Boss战期间需要关闭的面板（数组） | 拖拽所有在Boss战中不需要的UI面板 |
| Panels To Hide On Result | 结算时需要关闭的面板（数组） | 拖拽结算时需要隐藏的UI面板 |

**配置方法**：
1. 点击数组右侧的 `+` 按钮添加元素
2. 从 Hierarchy 中拖拽对应的 GameObject 到数组槽位

---

#### **Result Screen（结算界面）**

| 字段 | 说明 |
|------|------|
| Result Panel | Boss战结算面板 |
| Failure Buttons Container | 失败按钮容器（包含重试和返回按钮）|

---

#### **Victory Flow（胜利流程）**

| 字段 | 默认值 | 说明 |
|------|--------|------|
| Panels To Move Off Screen | - | 胜利后需要移出摄像机的面板列表（数组）|
| Off Screen Position | (0, 3000, 0) | 面板移出的目标位置 |
| Panel Move Time | 0.5 | 面板移动动画时间（秒）|

**说明**：
- 胜利后，这些面板会通过动画移出屏幕
- 移动完成后会播放剧情

---

#### **Victory Story Sequence（胜利剧情序列）**

| 字段 | 说明 |
|------|------|
| Victory Story Ids | 胜利后播放的剧情ID列表（数组，按顺序播放）|

**配置方法**：
1. 点击数组右侧的 `+` 按钮添加元素
2. 输入剧情ID（对应 `stories.json` 中的 `storyId`）
3. 剧情会按数组顺序依次播放

**示例**：
```
Victory Story Ids:
  Element 0: "boss_victory_1"
  Element 1: "boss_victory_2"
  Element 2: "boss_victory_3"
```

---

#### **Ending Choice（结局选择）**

| 字段 | 说明 |
|------|------|
| Ending Choice Panel | 结局选择面板（剧情播放完后显示）|
| Ending A Button | 结局A按钮 |
| Ending B Button | 结局B按钮 |

**重要**：
- 按钮的跳转场景逻辑需要你自己在 Inspector 中配置
- 在按钮的 `OnClick()` 事件中添加场景跳转代码
- 脚本不处理按钮的跳转逻辑

**配置按钮跳转示例**：
1. 选中 `Ending A Button`
2. 在 Inspector 中找到 `Button (Script)` 组件
3. 在 `On Click ()` 列表中点击 `+`
4. 拖拽一个包含场景跳转脚本的对象到 Object 槽位
5. 选择对应的跳转方法（如 `SceneManager.LoadScene("EndingA")`）

---

#### **Boss Dialogue System（Boss对话系统）**

| 字段 | 说明 |
|------|------|
| Boss Dialogue Panel | Boss战专用对话面板（索引2）|

**配置步骤**：
1. 在 Canvas 中创建一个新的对话面板
2. 设计Boss战专用的对话UI样式
3. 拖拽到这个字段
4. 这个面板会在Boss战开场对话时使用

**重要**：
- 这个面板在非Boss战时会自动隐藏
- 索引2表示这是第三个对话面板（0, 1, 2）

---

#### **Song Completion Detection（歌曲完成检测）**

| 字段 | 默认值 | 说明 |
|------|--------|------|
| Delay Before Showing Result | 2.0 | 完成后等待多少秒再显示结算界面 |
| Delay Before Game Over Result | 2.0 | 游戏失败后等待多少秒再显示结算界面 |

---

#### **Scene Names（场景名称）**

| 字段 | 默认值 | 说明 |
|------|--------|------|
| Boss Play Scene Name | "BossPlayScene" | Boss战场景名称（用于重试）|
| Map Scene Name | "Big Map" | 大地图场景名称（用于返回）|

---

## 3. 创建必需的UI面板

### A. Boss粉丝数显示面板

1. 在 Canvas 中创建一个新的 Panel
2. 命名为 `BossFansPanel`
3. 添加 TextMeshProUGUI 组件显示粉丝数
4. 添加 `BossFansUI.cs` 脚本
5. 配置 `BossFansUI` 的 `Current Fans Text` 字段
6. 拖拽 `BossFansPanel` 到 `BossBattleManager` 的 `Boss Fans Panel` 字段

### B. Boss对话面板

1. 在 Canvas 中创建一个新的对话面板
2. 命名为 `BossDialoguePanel`
3. 设计Boss战专用的对话UI（可以参考现有的对话面板）
4. 拖拽到 `BossBattleManager` 的 `Boss Dialogue Panel` 字段

### C. 结局选择面板

1. 在 Canvas 中创建一个新的 Panel
2. 命名为 `EndingChoicePanel`
3. 添加两个按钮：`Ending A Button` 和 `Ending B Button`
4. 配置按钮的 `OnClick()` 事件（跳转到对应的结局场景）
5. 拖拽面板和按钮到 `BossBattleManager` 的对应字段

---

## 4. 配置 LevelData

确保Boss关卡的 `LevelData` 配置正确：

```
Chapter Group: 4
```

只有当 `chapterGroup == 4` 时，Boss战系统才会启用。

---

## 5. 测试流程

### 测试Boss战启动

1. 设置 `LevelData.chapterGroup = 4`
2. 启动游戏
3. 检查 Console 日志：
   - `[BossBattleManager] Boss mode detected! Initializing...`
   - `[BossBattleManager] Boss mode initialized successfully.`

### 测试粉丝数系统

1. 启用 `Enable Test Mode`
2. 设置 `Test Fans Amount = 1000000`
3. 启动游戏
4. 观察粉丝数变化：
   - Perfect: +100,000
   - Great: -300,000
   - Good: -600,000
   - Miss: -600,000

### 测试失败流程

1. 让粉丝数归零或生命值归零
2. 检查是否显示失败按钮容器
3. 测试重试和返回大地图按钮

### 测试胜利流程

1. 完成歌曲且不触发失败条件
2. 检查面板是否移出屏幕
3. 检查剧情是否按顺序播放
4. 检查结局选择面板是否显示

---

## 6. 常见问题

### Q: Boss战功能影响了普通关卡？

**A**: 检查以下内容：
- 确保普通关卡的 `LevelData.chapterGroup != 4`
- 检查 Console 日志是否有 `[BossBattleManager] Not Boss mode, all Boss features disabled.`

### Q: 粉丝数UI不显示？

**A**: 检查以下内容：
- `Boss Fans Panel` 是否正确拖拽
- `BossFansUI.cs` 脚本是否添加到面板上
- `BossFansUI` 的 `Current Fans Text` 字段是否配置

### Q: 结局选择面板不显示？

**A**: 检查以下内容：
- `Victory Story Ids` 数组是否配置了剧情ID
- 剧情ID是否在 `stories.json` 中存在
- `Ending Choice Panel` 是否正确拖拽

### Q: 按钮跳转不工作？

**A**: 
- 按钮的跳转逻辑需要你自己在 Inspector 中配置
- 脚本只负责显示/隐藏按钮容器，不处理跳转逻辑

---

## 7. 调试技巧

### 启用详细日志

所有关键操作都会输出日志，格式为：
```
[BossBattleManager] <message>
```

### 常见日志信息

| 日志 | 含义 |
|------|------|
| `Boss mode detected! Initializing...` | 检测到Boss战，开始初始化 |
| `Not Boss mode, all Boss features disabled.` | 不是Boss战，所有功能禁用 |
| `Judgment: PERFECT, Fans Change: +100000` | 判定结果和粉丝数变化 |
| `Game Over!` | 游戏失败 |
| `Boss battle completed!` | Boss战完成 |
| `Starting victory sequence.` | 开始胜利流程 |
| `Playing victory story 1/3: boss_victory_1` | 播放第1个胜利剧情 |
| `Ending choice panel displayed.` | 显示结局选择面板 |

---

## 8. 与现有系统的兼容性

### 自动切换系统

- **Boss战模式**：使用 `BossBattleManager`
- **普通模式**：使用原有的 `HealthSystem`、`SongCompletionDetector`、`PlayDataCollector`

### 修改的脚本

以下脚本已经修改以支持Boss战系统：

1. **ScoreManager.cs**
   - 判定时会调用 `BossBattleManager.OnJudgment()`

2. **NoteManager.cs**
   - Note处理完成时会调用 `BossBattleManager.OnNoteProcessed()`

3. **BossFansUI.cs**
   - 从 `BossBattleManager` 获取粉丝数数据

---

## 9. 完整配置检查清单

- [ ] `BossBattleManager` 已添加到场景
- [ ] `Original Play Count Panel` 已配置
- [ ] `Boss Fans Panel` 已创建并配置
- [ ] `Boss Dialogue Panel` 已创建并配置
- [ ] `Panels To Hide During Boss` 数组已配置
- [ ] `Panels To Hide On Result` 数组已配置
- [ ] `Result Panel` 已配置
- [ ] `Failure Buttons Container` 已配置
- [ ] `Panels To Move Off Screen` 数组已配置
- [ ] `Victory Story Ids` 数组已配置
- [ ] `Ending Choice Panel` 已创建并配置
- [ ] `Ending A Button` 和 `Ending B Button` 已配置跳转逻辑
- [ ] `LevelData.chapterGroup = 4` 已设置
- [ ] 测试Boss战启动成功
- [ ] 测试粉丝数变化正常
- [ ] 测试失败流程正常
- [ ] 测试胜利流程正常

---

## 10. 技术支持

如果遇到问题，请检查：
1. Console 日志中的错误信息
2. 所有必需的引用是否已配置
3. 场景中是否有必需的组件（NoteManager、ScoreManager等）
4. LevelData 的 chapterGroup 是否正确设置

---

**祝你配置顺利！** 🎮
