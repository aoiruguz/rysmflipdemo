# 结算界面设置指南

## 已完成的工作

### 1. 数据收集系统更新
- ✅ 在 `ScoreManager.cs` 中添加了 Late/Fast 统计
- ✅ 在 `PlayDataCollector.cs` 中更新了数据收集逻辑
- ✅ 在 `PlayData` 类中添加了 `lateCount` 和 `fastCount` 字段
- ✅ 更新了评级系统，基于达成率计算：
  - S: 95%以上
  - A: 92%以上
  - B: 85%以上
  - C: 78%以上
  - F: 78%以下
- ✅ 添加了 `IsFullCombo()` 和 `IsAllPerfect()` 方法

### 2. UI脚本创建
- ✅ `ResultScreenUI.cs` - 结算界面UI管理器
- ✅ `ResultScreenSetupHelper.cs` - UI设置助手脚本

## 在Unity中设置结算界面

### 方法1：使用设置助手（推荐）

1. 在Unity中打开 `Game Over` Scene
2. 在Hierarchy中创建一个空GameObject，命名为 "SetupHelper"
3. 将 `ResultScreenSetupHelper.cs` 脚本添加到这个GameObject上
4. 在Inspector中右键点击脚本，选择 "Setup Result Screen UI"
5. UI会自动创建完成
6. 删除 "SetupHelper" GameObject（已完成任务）

### 方法2：手动设置

如果自动设置不工作，可以手动创建UI结构：

1. 创建Canvas（如果没有）
2. 在Canvas下创建一个Panel作为 "ResultPanel"
3. 添加 `ResultScreenUI` 组件到ResultPanel
4. 按照以下结构创建UI元素并连接到ResultScreenUI的字段：

#### 必需的UI元素：
- **rankIcon** (Image) - 评级图标
- **fullComboIcon** (GameObject) - Full Combo标识
- **allPerfectIcon** (GameObject) - All Perfect标识
- **achievementRateText** (TextMeshProUGUI) - 达成率百分比
- **achievementScoreText** (TextMeshProUGUI) - 达成分数
- **maxComboText** (TextMeshProUGUI) - 最大连击数
- **totalNotesText** (TextMeshProUGUI) - 总note数
- **perfectCountText** (TextMeshProUGUI) - Perfect数量
- **greatCountText** (TextMeshProUGUI) - Great数量
- **goodCountText** (TextMeshProUGUI) - Good数量
- **missCountText** (TextMeshProUGUI) - Miss数量
- **lateCountText** (TextMeshProUGUI) - Late数量
- **fastCountText** (TextMeshProUGUI) - Fast数量
- **songNameText** (TextMeshProUGUI) - 歌曲名称
- **difficultyText** (TextMeshProUGUI) - 难度

## 评级图标设置

你需要准备5个评级图标精灵（Sprite）：
- Rank S
- Rank A
- Rank B
- Rank C
- Rank F

将这些精灵分配到 `ResultScreenUI` 组件的对应字段：
- rankS
- rankA
- rankB
- rankC
- rankF

## 数据流程

1. **PlayScene** 中，`PlayDataCollector` 收集游玩数据
2. 游戏结束时，`PlayDataCollector.CollectFinalData()` 被调用
3. 切换到 **Game Over Scene**
4. `ResultScreenUI.Start()` 从 `PlayDataCollector.Instance` 获取数据
5. 显示所有结算信息

## 显示的信息

### 评级区域（左侧）
- 评级图标（S/A/B/C/F）
- Full Combo 标识（仅当达成时显示）
- All Perfect 标识（仅当达成时显示）

### 分数区域（中央）
- 达成率（百分比）
- 达成分数（0-1000000）
- 最大连击 / 总Note数

### 判定统计（右侧）
- Perfect 数量
- Great 数量
- Good 数量
- Miss 数量
- Late 数量
- Fast 数量

### 歌曲信息（顶部）
- 歌曲名称
- 难度

## 测试

### 方法1：使用测试脚本（推荐）

1. 在Game Over Scene中创建一个空GameObject，命名为 "Tester"
2. 添加 `ResultScreenTester.cs` 脚本
3. 在Inspector中右键点击脚本，选择以下测试选项：
   - "Test S Rank Data" - 测试S评级数据
   - "Test A Rank Data" - 测试A评级数据
   - "Test Full Combo" - 测试Full Combo
   - "Test All Perfect" - 测试All Perfect
4. 或者在Inspector中调整测试数据参数，然后勾选 "createTestData"

### 方法2：从PlayScene测试

1. 确保PlayScene中有 `PlayDataCollector` 组件
2. 正常游玩一首歌曲
3. 歌曲结束后会自动切换到Game Over Scene并显示结果

### 如果PlayDataCollector不存在

如果 `PlayDataCollector` 不存在，`ResultScreenUI` 会使用内置的测试数据，方便你在编辑器中预览UI布局。

## 注意事项

1. 确保 `PlayDataCollector` 在PlayScene中存在并正确收集数据
2. 确保场景切换时 `PlayDataCollector.Instance` 不会被销毁（可能需要 `DontDestroyOnLoad`）
3. 评级图标需要手动创建或导入
4. 所有TextMeshProUGUI组件需要TextMesh Pro包支持

## 下一步

1. 创建或导入评级图标（S/A/B/C/F）
2. 在Game Over Scene中设置UI
3. 测试从PlayScene到Game Over Scene的数据传递
4. 根据需要调整UI布局和样式
5. 添加动画效果（可选）
