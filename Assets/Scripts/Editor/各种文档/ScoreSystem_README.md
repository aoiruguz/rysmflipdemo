# 播放量和粉丝系统实现文档

## 系统概述

这个系统将传统音游的"分数"概念替换为"播放量"和"粉丝数"，模拟视频平台的数据增长机制。

### 核心概念

1. **播放量 (PlayCount)**: 单局结算分数，可达"亿"级别
2. **粉丝数 (Fans)**: 跨局累计的永久数值，用于解锁新章节
3. **章节倍数 (ChapterMultiplier)**: 指数增长系数，决定分数量级

## 核心脚本

### 1. ScoreCalculator.cs
**位置**: `Assets/Scripts/ScoreCalculator.cs`

**功能**: 核心计算逻辑

**主要方法**:
- `CalculatePlayCount()` - 计算单局播放量
- `CalculateFansGain()` - 计算粉丝增量（播放量的10%）
- `FormatLargeNumber()` - 格式化大数字显示（万、亿）

**判定得分规则**:

#### 普通关卡
- Perfect: +600 × 章节倍数
- Great: +300 × 章节倍数
- Good: +100 × 章节倍数
- Miss: 第1-2章不扣分，第3章起 -300 × 章节倍数

#### Boss关卡（高风险高回报）
- Perfect: +100 × 章节倍数
- Great: -300 × 章节倍数
- Good: -600 × 章节倍数
- Miss: -600 × 章节倍数

**连击系统**:
- 基础连击分：每个Combo额外 +100 × 章节倍数
- 连击倍率：起始×1.1，每5 Combo增长+0.1，封顶×2.0
- 公式：`Min(1.1 + (Combo / 5) * 0.1, 2.0)`

**章节倍数**:
- 第1章: 10^1 = 10
- 第2章: 10^2 = 100
- 第3章: 10^3 = 1000

### 2. FansDataManager.cs
**位置**: `Assets/Scripts/FansDataManager.cs`

**功能**: 粉丝数据持久化管理

**主要方法**:
- `GetTotalFans()` - 获取当前粉丝总数
- `AddFans(long)` - 增加粉丝数
- `IsChapterUnlocked(int)` - 检查章节是否解锁
- `ResetAllData()` - 重置所有数据（调试用）

**章节解锁条件**:
- 第1章: 默认解锁
- 第2章: 需要 100万 粉丝
- 第3章: 需要 1000万 粉丝

### 3. PlayDataCollector.cs
**位置**: `Assets/Scripts/PlayDataCollector.cs`

**修改内容**:
- 将 `achievementScoreText` 改为 `playCountText`
- `UpdateAchievementScoreUI()` 改为 `UpdatePlayCountUI()`
- 实时显示播放量而不是达成率

**新增字段**:
```csharp
public int chapterIndex = 0; // 章节索引
public bool isBossStage = false; // 是否为Boss关卡
public bool isReplay = false; // 是否为重复挑战
```

### 4. ResultScreenUI.cs
**位置**: `Assets/Scripts/ResultScreenUI.cs`

**新增UI字段**:
```csharp
public TextMeshProUGUI playCountText; // 播放量
public TextMeshProUGUI newFansText; // 本局获得粉丝
public TextMeshProUGUI totalFansText; // 总粉丝数
```

**新增方法**:
- `DisplayPlayCountAndFans()` - 显示播放量和粉丝数

### 5. SongSelectionManager.cs
**位置**: `Assets/Scripts/SongSelectionManager.cs`

**新增UI字段**:
```csharp
public TextMeshProUGUI totalFansText; // 显示总粉丝数
public TextMeshProUGUI chapterProgressText; // 显示章节进度
```

**新增方法**:
- `UpdateFansDisplay()` - 更新粉丝数和章节进度显示

## 数据流程

### 游戏进行中
1. `PlayDataCollector` 实时收集判定数据
2. `UpdatePlayCountUI()` 实时计算并显示播放量

### 关卡结束时
1. `PlayDataCollector.CollectFinalData()` 收集最终数据
2. `ScoreCalculator.CalculatePlayCount()` 计算播放量
3. `ScoreCalculator.CalculateFansGain()` 计算粉丝增量
4. `FansDataManager.AddFans()` 保存粉丝数
5. 检查是否解锁新章节

### 结算界面
1. `ResultScreenUI` 从 `PlayDataCollector` 获取数据
2. `DisplayPlayCountAndFans()` 显示播放量和粉丝数

### 选关界面
1. `SongSelectionManager.UpdateFansDisplay()` 显示总粉丝数
2. 显示下一章节解锁进度

## 使用方法

### 1. 设置章节信息（在Unity编辑器中）

**不需要写代码！** 章节信息直接在 ChartData 资源文件中设置：

1. 在Project窗口中选择你的 ChartData 资源（.asset文件）
2. 在Inspector中找到 "Chapter System" 部分
3. 设置以下字段：
   - **Chapter Index**: 0=第1章, 1=第2章, 2=第3章
   - **Is Boss Stage**: 勾选表示这是Boss关卡

**示例**：
- 第1章普通关卡：`chapterIndex = 0`, `isBossStage = false`
- 第2章Boss关卡：`chapterIndex = 1`, `isBossStage = true`
- 第3章普通关卡：`chapterIndex = 2`, `isBossStage = false`

**重复挑战判断**：系统会自动检测玩家是否已通关该关卡，无需手动设置。

### 2. UI绑定
需要在Unity编辑器中绑定以下UI元素：

#### PlayScene (游戏场景)
- `PlayDataCollector.playCountText` → 实时播放量显示

#### ResultScreen (结算界面)
- `ResultScreenUI.playCountText` → 播放量
- `ResultScreenUI.newFansText` → 本局获得粉丝
- `ResultScreenUI.totalFansText` → 总粉丝数

#### SongSelection (选关界面)
- `SongSelectionManager.totalFansText` → 总粉丝数
- `SongSelectionManager.chapterProgressText` → 章节进度

### 3. 测试工具
使用编辑器工具测试计算逻辑：

**菜单**: `Tools > Score System Tester`

**功能**:
- 输入判定数据，计算播放量和粉丝
- 快速测试预设场景
- 查看当前粉丝数和解锁进度
- 重置所有粉丝数据
- 重置所有通关记录

## 新增脚本

### ClearDataManager.cs
**位置**: `Assets/Scripts/ClearDataManager.cs`

**功能**: 管理通关记录

**主要方法**:
- `MarkChartCleared(ChartData)` - 标记关卡为已通关
- `IsChartCleared(ChartData)` - 检查关卡是否已通关
- `ResetAllClearData()` - 重置所有通关记录

**工作原理**:
- 当玩家首次通关关卡时，系统自动标记为已通关
- 下次挑战同一关卡时，自动识别为"重复挑战"
- 重复挑战只获得10%的播放量和粉丝

## 数值示例

### 第1章普通关卡（倍数=10）
- 100个Perfect，100 Combo
- 基础分: 100 × 600 × 10 = 600,000
- 连击分: 约 150,000
- 总播放量: 约 75万
- 粉丝增量: 约 7.5万

### 第2章普通关卡（倍数=100）
- 100个Perfect，100 Combo
- 基础分: 100 × 600 × 100 = 6,000,000
- 连击分: 约 1,500,000
- 总播放量: 约 750万
- 粉丝增量: 约 75万

### 第3章Boss关卡（倍数=1000）
- 50个Perfect，50个Great，80 Combo
- 基础分: 50×100×1000 + 50×(-300)×1000 = -10,000,000
- 连击分: 约 1,200,000
- 总播放量: 约 -880万（负数会被归零）
- **Boss关卡风险极高！**

## 重要提示

### 1. 数据类型
- 播放量和粉丝数使用 `long` 类型（可达亿级）
- PlayerPrefs不支持long，使用字符串存储

### 2. 重复挑战惩罚
- 如果打已通关的旧关卡 → 播放量和粉丝都只有 10%
- 鼓励玩家挑战新关卡

### 3. 章节解锁
- 系统会在关卡结束时自动检查并解锁新章节
- 解锁信息保存在 PlayerPrefs 中

### 4. 调试
- 使用 `FansDataManager.ResetAllData()` 重置所有数据
- 使用 `ScoreSystemTester` 工具测试计算逻辑
- 查看Console日志了解详细计算过程

## 待完成的工作

### 必须完成
1. ✅ ~~设置章节信息~~：在Unity编辑器中为每个ChartData设置 `chapterIndex` 和 `isBossStage`
2. **UI绑定**: 在Unity编辑器中绑定所有UI元素
3. **测试**: 使用测试工具验证计算逻辑

### 可选优化
1. 添加章节解锁动画
2. 添加粉丝增长动画（数字滚动效果）
3. 添加章节选择界面（根据解锁状态显示/隐藏章节）
4. 添加历史最高播放量记录

## 技术细节

### 连击加成计算
连击加成采用累加方式，每个Combo都有独立的倍率：

```csharp
for (int combo = 1; combo <= maxCombo; combo++)
{
    float multiplier = Min(1.1 + (combo / 5) * 0.1, 2.0);
    long score = 100 × 章节倍数 × multiplier;
    totalBonus += score;
}
```

这样设计的好处：
- 连击越高，每个Combo的价值越大
- 鼓励玩家保持连击
- 数值增长平滑

### 数据持久化
使用 PlayerPrefs 存储：
- `TotalFans` (string) - 粉丝总数
- `ChapterProgress` (int) - 已解锁章节

### 架构分离
- `ScoreCalculator` - 纯计算逻辑，无依赖
- `FansDataManager` - 数据持久化，无UI依赖
- `PlayDataCollector` - 数据收集和传递
- `ResultScreenUI` - UI显示

这样的分离使得：
- 计算逻辑可独立测试
- UI可独立修改
- 数据流清晰明确
