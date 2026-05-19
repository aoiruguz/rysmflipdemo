# FansDisplayUI - 快速使用指南

## 这是什么？

`FansDisplayUI` 是一个独立的UI管理脚本，用于在Big Map场景显示粉丝数和章节解锁进度。

## 3步快速设置

### 1️⃣ 创建UI文本
在Canvas中创建2个TextMeshProUGUI：
- `TotalFansText` - 显示 "粉丝: 50.00万"
- `ChapterProgressText` - 显示 "第2章解锁: 还需 50.00万 粉丝"

### 2️⃣ 添加脚本
创建一个GameObject，添加 `FansDisplayUI` 组件

### 3️⃣ 绑定UI
在Inspector中：
- **Total Fans Text** ← 拖入 `TotalFansText`
- **Chapter Progress Text** ← 拖入 `ChapterProgressText`
- ✓ 勾选 **Update On Start**
- ✓ 勾选 **Update On Enable**

## 完成！

系统会自动显示和更新粉丝数。

## 显示效果

```
粉丝: 50.00万
第2章解锁: 还需 50.00万 粉丝
```

## 手动刷新（可选）

```csharp
FansDisplayUI fansUI = FindObjectOfType<FansDisplayUI>();
fansUI.UpdateFansDisplay();
```

## 查询粉丝数据（可选）

```csharp
FansDisplayUI fansUI = FindObjectOfType<FansDisplayUI>();

long fans = fansUI.GetCurrentFans(); // 获取粉丝数
int chapter = fansUI.GetUnlockedChapter(); // 获取已解锁章节
bool unlocked = fansUI.IsChapterUnlocked(1); // 检查第2章是否解锁
```

## 详细文档

- **完整设置指南**: [FansDisplayUI_Setup.md](FansDisplayUI_Setup.md)
- **系统快速入门**: [ScoreSystem_QuickStart.md](ScoreSystem_QuickStart.md)

## 脚本位置

`Assets/Scripts/FansDisplayUI.cs`
