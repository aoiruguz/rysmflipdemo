# 播放量和粉丝系统 - 快速入门指南

## 🎯 核心概念

- **播放量**: 单局得分（可达亿级）
- **粉丝数**: 累计进度（播放量的10%转化为粉丝）
- **章节系统**: 第1章(×10) → 第2章(×100) → 第3章(×1000)

## 📝 设置步骤（3步完成）

### 第1步：设置ChartData的章节信息

1. 在Project窗口找到你的ChartData资源（.asset文件）
2. 在Inspector中找到 **"Chapter System"** 部分
3. 设置：
   - **Chapter Index**: 0=第1章, 1=第2章, 2=第3章
   - **Is Boss Stage**: 勾选=Boss关卡（高风险高回报）

**示例配置**：
```
第1章普通关卡: chapterIndex=0, isBossStage=false
第2章Boss关卡: chapterIndex=1, isBossStage=true
第3章普通关卡: chapterIndex=2, isBossStage=false
```

### 第2步：绑定UI元素

#### PlayScene（游戏场景）
找到 `PlayDataCollector` 组件，绑定：
- **Play Count Text** → 实时播放量显示的TextMeshProUGUI

#### ResultScreen（结算界面）
找到 `ResultScreenUI` 组件，绑定：
- **Play Count Text** → 播放量显示
- **New Fans Text** → 本局获得粉丝显示
- **Total Fans Text** → 总粉丝数显示

#### SongSelection（选关界面）
找到 `SongSelectionManager` 组件，绑定：
- **Total Fans Text** → 总粉丝数显示
- **Chapter Progress Text** → 章节解锁进度显示

### 第3步：测试

1. 打开 `Tools > Score System Tester`
2. 点击快速测试按钮验证计算逻辑
3. 进入游戏测试实际流程

## 🎮 系统工作流程

### 自动化流程（无需编码）

1. **选关时**: 系统自动从ChartData读取章节信息
2. **游戏中**: 实时计算并显示播放量
3. **结算时**: 
   - 计算最终播放量
   - 转化10%为粉丝并累加
   - 自动标记为已通关
   - 检查是否解锁新章节
4. **重复挑战**: 系统自动识别，只给10%奖励

### 重复挑战判定

系统会自动记录已通关的关卡：
- **首次通关**: 获得100%播放量和粉丝
- **重复挑战**: 只获得10%（鼓励挑战新关卡）

判定依据：`歌曲名 + 难度 + 章节 + 是否Boss`

## 📊 数值参考

### 章节目标
- 第1章: 100万 - 999万播放量
- 第2章: 1000万 - 9999万播放量（需100万粉丝解锁）
- 第3章: 1亿 - 9亿播放量（需1000万粉丝解锁）

### 判定得分

**普通关卡**:
- Perfect: +600 × 章节倍数
- Great: +300 × 章节倍数
- Good: +100 × 章节倍数
- Miss: 第1-2章不扣分，第3章 -300 × 章节倍数

**Boss关卡**（高风险高回报）:
- Perfect: +100 × 章节倍数
- Great: -300 × 章节倍数（扣分！）
- Good: -600 × 章节倍数（扣分！）
- Miss: -600 × 章节倍数（扣分！）

### 连击加成
- 每个Combo: +100 × 章节倍数
- 连击倍率: 1.1x → 2.0x（每5 Combo +0.1）

## 🛠️ 测试工具

### Tools > Score System Tester

**功能**:
- 输入判定数据，实时计算播放量和粉丝
- 快速测试预设场景
- 查看当前粉丝数和解锁进度
- 重置粉丝数据
- 重置通关记录

**使用场景**:
- 验证数值设计
- 测试章节解锁逻辑
- 调试计算问题

## ⚠️ 注意事项

1. **ChartData必须设置章节信息**，否则默认为第1章普通关卡
2. **UI元素必须绑定**，否则不会显示数据
3. **重置通关记录会删除所有PlayerPrefs数据**，谨慎使用
4. **Boss关卡风险极高**，Great/Good/Miss都会扣分

## 🐛 常见问题

### Q: 播放量显示为0？
A: 检查ChartData是否设置了章节信息，检查UI是否绑定

### Q: 粉丝数没有增加？
A: 检查Console日志，确认计算逻辑是否执行

### Q: 重复挑战没有生效？
A: 确认首次通关时是否成功标记，检查Console日志

### Q: 章节没有解锁？
A: 检查粉丝数是否达到要求（第2章需100万，第3章需1000万）

## 📁 相关文件

- `ScoreCalculator.cs` - 核心计算逻辑
- `FansDataManager.cs` - 粉丝数据管理
- `ClearDataManager.cs` - 通关记录管理
- `PlayDataCollector.cs` - 数据收集
- `ResultScreenUI.cs` - 结算界面
- `SongSelectionManager.cs` - 选关界面
- `ChartData.cs` - 谱面数据（新增章节字段）

## 📖 完整文档

详细技术文档请查看: `ScoreSystem_README.md`
