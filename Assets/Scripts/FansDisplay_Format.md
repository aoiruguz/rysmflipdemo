# 粉丝系统显示格式说明

## 数字格式化

### 英文格式（K, M）

| 数值范围 | 显示格式 | 示例 |
|---------|---------|------|
| 0 - 9,999 | 原始数字 | 5000 |
| 10,000 - 999,999 | K (千) | 50.00K |
| 1,000,000 - 99,999,999 | M (百万) | 5.00M |
| 100,000,000+ | M (百万) | 150.00M |

### 示例
- 500 → "500"
- 15,000 → "15.00K"
- 1,000,000 → "1.00M"
- 50,000,000 → "50.00M"
- 150,000,000 → "150.00M"

## UI显示文本

### Big Map / 选关界面

**粉丝数显示**:
```
Fans: 50.00K
```

**章节进度显示**:

情况1 - 未解锁:
```
Chapter 2 Unlock: 50.00K more fans needed
```

情况2 - 刚解锁:
```
Chapter 2 Unlocked!
```

情况3 - 全部解锁:
```
All Chapters Unlocked!
```

### 结算界面 (Result Screen)

**播放量显示**:
```
Play Count: 750.00K
```

**新增粉丝显示**:
```
New Fans: +75.00K
```

**总粉丝数显示**:
```
Total Fans: 125.00K
```

## 章节解锁条件

| 章节 | 解锁条件 | 英文显示 |
|------|---------|---------|
| Chapter 1 | 默认解锁 | Default |
| Chapter 2 | 1,000,000 粉丝 | 1.00M fans |
| Chapter 3 | 10,000,000 粉丝 | 10.00M fans |

## 完整示例

### 游戏流程显示

**1. 初始状态**
```
Big Map:
  Fans: 0
  Chapter 2 Unlock: 1.00M more fans needed
```

**2. 玩了一局（获得75K粉丝）**
```
Result Screen:
  Play Count: 750.00K
  New Fans: +75.00K
  Total Fans: 75.00K

返回 Big Map:
  Fans: 75.00K
  Chapter 2 Unlock: 925.00K more fans needed
```

**3. 继续游玩（累计到1M粉丝）**
```
Big Map:
  Fans: 1.00M
  Chapter 2 Unlocked!
```

**4. 玩第2章（累计到5M粉丝）**
```
Big Map:
  Fans: 5.00M
  Chapter 3 Unlock: 5.00M more fans needed
```

**5. 全部解锁（累计到15M粉丝）**
```
Big Map:
  Fans: 15.00M
  All Chapters Unlocked!
```

## 修改的文件

1. ✅ `ScoreCalculator.cs` - FormatLargeNumber() 改为 K/M 格式
2. ✅ `FansDisplayUI.cs` - 所有显示文本改为英文
3. ✅ `SongSelectionManager.cs` - 所有显示文本改为英文

## 如果需要自定义格式

### 修改数字格式
编辑 `ScoreCalculator.FormatLargeNumber()`:

```csharp
// 当前格式: "50.00K"
return $"{thousands:F2}K";

// 改为无小数: "50K"
return $"{thousands:F0}K";

// 改为1位小数: "50.0K"
return $"{thousands:F1}K";
```

### 修改显示文本
编辑 `FansDisplayUI.UpdateFansDisplay()`:

```csharp
// 当前: "Fans: 50.00K"
totalFansText.text = $"Fans: {ScoreCalculator.FormatLargeNumber(totalFans)}";

// 改为: "👥 50.00K"
totalFansText.text = $"👥 {ScoreCalculator.FormatLargeNumber(totalFans)}";

// 改为: "Followers: 50.00K"
totalFansText.text = $"Followers: {ScoreCalculator.FormatLargeNumber(totalFans)}";
```

## TextMeshPro 字体设置

如果英文显示不正常，检查：
1. TextMeshPro 使用的字体是否支持英文字符
2. Font Asset 是否包含所需字符
3. 使用默认的 "LiberationSans SDF" 字体应该没问题

## 测试

使用 `Tools > Score System Tester` 测试不同数值的显示：
- 输入 10,000 → 应显示 "10.00K"
- 输入 1,000,000 → 应显示 "1.00M"
- 输入 50,000,000 → 应显示 "50.00M"
