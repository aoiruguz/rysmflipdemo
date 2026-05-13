# Big Map 场景 - 粉丝UI设置指南

## 需要显示的信息

在 Big Map（选关界面）中需要显示：
1. **总粉丝数** - 玩家累计的粉丝总量
2. **章节进度** - 下一章节解锁所需粉丝数

## Unity编辑器设置步骤

### 第1步：创建UI元素

在 Big Map 场景的Canvas中创建两个TextMeshProUGUI元素：

1. **总粉丝数显示**
   - 右键点击Canvas → UI → Text - TextMeshPro
   - 命名为 `TotalFansText`
   - 示例文本：`粉丝: 0`

2. **章节进度显示**
   - 右键点击Canvas → UI → Text - TextMeshPro
   - 命名为 `ChapterProgressText`
   - 示例文本：`第2章解锁: 还需 100.00万 粉丝`

### 第2步：调整UI位置和样式

**推荐布局**：
```
屏幕左上角或右上角：
┌─────────────────────────┐
│ 粉丝: 50.00万           │
│ 第2章解锁: 还需 50.00万 │
└─────────────────────────┘
```

**样式建议**：
- 字体大小：24-32
- 颜色：白色或金色（醒目）
- 对齐方式：左对齐或居中
- 添加阴影或描边以提高可读性

### 第3步：绑定到SongSelectionManager

1. 在Hierarchy中找到包含 `SongSelectionManager` 组件的GameObject
2. 在Inspector中找到 `SongSelectionManager` 脚本
3. 找到 **"Fans Display"** 部分
4. 拖拽UI元素到对应字段：
   - **Total Fans Text** ← 拖入 `TotalFansText`
   - **Chapter Progress Text** ← 拖入 `ChapterProgressText`

## 显示效果示例

### 初始状态（0粉丝）
```
粉丝: 0
第2章解锁: 还需 100.00万 粉丝
```

### 有进度（50万粉丝）
```
粉丝: 50.00万
第2章解锁: 还需 50.00万 粉丝
```

### 解锁第2章（100万粉丝）
```
粉丝: 100.00万
第2章已解锁！
```

### 进行第2章（500万粉丝）
```
粉丝: 500.00万
第3章解锁: 还需 500.00万 粉丝
```

### 全部解锁（1000万+粉丝）
```
粉丝: 1500.00万
已解锁全部章节！
```

## 自动更新机制

系统会在以下时机自动更新显示：
- ✅ 进入Big Map场景时（Start方法）
- ✅ 从结算界面返回时

如果需要手动刷新，可以调用：
```csharp
SongSelectionManager manager = FindObjectOfType<SongSelectionManager>();
if (manager != null)
{
    // 调用私有方法需要通过反射，或者将UpdateFansDisplay改为public
}
```

## 可选：添加图标和装饰

### 粉丝图标
可以在文本前添加一个Image组件显示粉丝图标：
```
[图标] 粉丝: 50.00万
```

### 进度条
可以添加一个Slider显示章节解锁进度：
```
粉丝: 50.00万
第2章解锁: [████████░░] 50%
```

### 动画效果
可以添加数字滚动动画，让粉丝数增长更有视觉冲击力。

## 测试

### 测试步骤
1. 打开 `Tools > Score System Tester`
2. 点击 "重置所有粉丝数据" 确保从0开始
3. 进入Big Map场景，应该显示：
   ```
   粉丝: 0
   第2章解锁: 还需 100.00万 粉丝
   ```
4. 使用测试工具添加粉丝（或玩一局游戏）
5. 返回Big Map场景，检查数字是否更新

### 调试
如果UI没有显示或显示错误：
1. 检查Console是否有错误日志
2. 确认UI元素已正确绑定到SongSelectionManager
3. 确认TextMeshProUGUI组件存在且启用
4. 检查Canvas是否启用
5. 查看Console日志：`[SongSelection] Total Fans: ...`

## 完整的SongSelectionManager字段

确保以下字段都已绑定：

**UI References**:
- ✅ Song Title Text
- ✅ Left Button
- ✅ Right Button
- ✅ Background Image
- ✅ Easy Button
- ✅ Normal Button
- ✅ Hard Button

**Fans Display** (新增):
- ✅ Total Fans Text
- ✅ Chapter Progress Text

**Songs List**:
- ✅ Songs (列表)

**Scene**:
- ✅ Game Scene Name

## 常见问题

### Q: 粉丝数显示为0？
A: 这是正常的初始状态。玩一局游戏后会增加。

### Q: 文本显示为空？
A: 检查UI元素是否绑定，检查TextMeshProUGUI组件是否存在。

### Q: 返回Big Map后数字没更新？
A: 系统会在Start时自动更新。如果没更新，检查Console日志。

### Q: 想要更改显示格式？
A: 修改 `SongSelectionManager.UpdateFansDisplay()` 方法中的文本格式。

## 相关文件

- `SongSelectionManager.cs` - 选关管理器（包含粉丝显示逻辑）
- `FansDataManager.cs` - 粉丝数据管理
- `ScoreCalculator.cs` - 数字格式化（FormatLargeNumber方法）
