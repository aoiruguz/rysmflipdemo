# Chart Editor 偏移调整和快捷键指南

## 修复内容

### 1. Note拖拽后变大的问题 ✅
**问题**：拖拽note后，scale被重置为(1,1,1)，覆盖了预制体的原始scale

**修复**：
- EditorNote现在会在Awake时保存原始scale
- 选中/取消选中时使用原始scale进行缩放
- 拖拽后note保持原始大小

### 2. Editor专用偏移设置 ✅
**新增**：`ChartEditorSettings` 类，独立于游戏设置

**包含设置**：
- `EditorOffset` - 谱面偏移（秒）
- `JudgmentLineOffsetY` - 判定线Y偏移
- `MusicVolume` - 音乐音量
- `HitSoundVolume` - 打击音效音量

## 快捷键

### 偏移调整（暂停时可用）

| 按键 | 功能 | 效果 |
|------|------|------|
| `+` 或 `Keypad +` | 增加偏移 +0.01秒 | Note向上移动 |
| `-` 或 `Keypad -` | 减少偏移 -0.01秒 | Note向下移动 |

**使用方法**：
1. 暂停播放
2. 按 `+` 或 `-` 键调整偏移
3. Note会立即重新生成，应用新的偏移
4. 继续播放查看效果

## 偏移工作原理

### 什么是偏移？
偏移用于调整音乐和谱面的同步：
- **正偏移** (+0.05s)：Note向上移动，判定提前
- **负偏移** (-0.05s)：Note向下移动，判定延后

### 何时需要调整偏移？
- 音效和音乐不同步
- Note经过判定线的时机不对
- 预览时感觉节奏不准

### 调整步骤
1. 播放谱面
2. 观察note经过判定线的时机
3. 如果note太早到达 → 按 `-` 减少偏移
4. 如果note太晚到达 → 按 `+` 增加偏移
5. 重复调整直到同步

## 设置说明

### EditorOffset（谱面偏移）
- **默认值**: 0秒
- **范围**: 无限制（通常在 -0.5 到 +0.5 之间）
- **作用**: 调整所有note的Y位置
- **公式**: `noteY = (noteTime + EditorOffset) × timeToYScale`

### JudgmentLineOffsetY（判定线偏移）
- **默认值**: -4
- **范围**: 任意值
- **作用**: 判定线相对于镜头的Y偏移
- **说明**: 负值表示在镜头下方

### MusicVolume（音乐音量）
- **默认值**: 0.8
- **范围**: 0.0 - 1.0
- **作用**: 控制背景音乐音量

### HitSoundVolume（打击音效音量）
- **默认值**: 0.5
- **范围**: 0.0 - 1.0
- **作用**: 控制判定音效音量

## 代码使用示例

### 读取设置
```csharp
float offset = ChartEditorSettings.EditorOffset;
float volume = ChartEditorSettings.MusicVolume;
```

### 修改设置
```csharp
ChartEditorSettings.EditorOffset = 0.05f;
ChartEditorSettings.MusicVolume = 0.8f;
```

### 调整设置
```csharp
ChartEditorSettings.AdjustOffset(0.01f); // 增加0.01秒
ChartEditorSettings.AdjustJudgmentLineY(-0.5f); // 向下移动0.5单位
```

### 重置为默认值
```csharp
ChartEditorSettings.ResetToDefaults();
```

## 与游戏设置的区别

| 设置 | 游戏模式 | Chart Editor |
|------|---------|--------------|
| 偏移 | GameSettings.GlobalOffset | ChartEditorSettings.EditorOffset |
| 音乐音量 | GameSettings.MusicVolume | ChartEditorSettings.MusicVolume |
| 打击音效 | GameSettings.NoteVolume | ChartEditorSettings.HitSoundVolume |
| 存储位置 | PlayerPrefs | PlayerPrefs（独立key） |
| 作用范围 | 游戏模式 | 制谱器模式 |

**为什么分开？**
- 制谱器需要频繁调整偏移来对齐谱面
- 游戏模式的偏移是玩家的个人设置
- 两者互不影响，避免混淆

## 实际使用场景

### 场景1：制作新谱面
1. 导入音乐文件
2. 在制谱器中播放
3. 发现音乐和谱面不同步
4. 按 `+` 或 `-` 调整偏移
5. 保存谱面（偏移已应用到note位置）

### 场景2：调试现有谱面
1. 加载谱面
2. 播放预览
3. 发现某些note时机不对
4. 调整偏移查看效果
5. 决定是否需要修改谱面

### 场景3：测试不同设备延迟
1. 在不同设备上打开制谱器
2. 调整偏移补偿设备延迟
3. 找到最佳偏移值
4. 记录下来供玩家参考

## 注意事项

1. **偏移会立即应用**：按下快捷键后，所有note会重新生成
2. **偏移不会保存到谱面**：偏移只影响预览，不会修改ChartData
3. **暂停时才能调整**：播放时快捷键无效，避免误操作
4. **每次调整0.01秒**：精度足够，可以多次按键微调

## 故障排除

### Q: 按快捷键没反应？
A: 确保游戏处于暂停状态，播放时快捷键被禁用

### Q: 偏移调整后note位置没变？
A: 检查Console是否有错误，确认note重新生成成功

### Q: 偏移值太大/太小？
A: 调用 `ChartEditorSettings.ResetToDefaults()` 重置

### Q: 想要更大的调整步长？
A: 修改ChartEditorManager.cs中的0.01f为其他值（如0.05f）

## 相关文件

- `ChartEditorSettings.cs` - 设置管理类
- `ChartEditorManager.cs` - 应用偏移和快捷键
- `EditorNote.cs` - 修复scale问题
- `EditorJudgmentLine.cs` - 使用判定线偏移设置
- `EditorJudgmentDetector.cs` - 使用音效音量设置
