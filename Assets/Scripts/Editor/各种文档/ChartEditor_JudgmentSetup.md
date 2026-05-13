# Chart Editor 判定线和判定系统完整设置指南

## 一、判定线设置

### 判定线位置说明（重要！）
- **判定线跟随镜头移动**，始终显示在屏幕下方固定位置
- 相对于镜头的偏移为 Y = -4
- Note固定在世界坐标，镜头向上移动时，判定线也向上移动
- 当判定线（跟随镜头）经过note时，触发判定

### 创建判定线（推荐方法）

#### 方法1：使用EditorJudgmentLine脚本（最简单，推荐）

1. 在Hierarchy中右键 → `2D Object` → `Sprite`
2. 命名为 `JudgmentLine`
3. 设置Transform：
   - Position: `(0, -4, 0)` （初始位置，运行后会自动跟随镜头）
   - Rotation: `(0, 0, 0)`
   - Scale: `(10, 0.1, 1)`
4. 设置SpriteRenderer：
   - Sprite: 选择一个白色方块（或任意sprite）
   - Color: 黄色 `(255, 255, 0, 255)` 或白色
   - Sorting Layer: 确保可见
5. **添加组件 `EditorJudgmentLine`**
6. 设置EditorJudgmentLine：
   - Judgment Line Offset Y: `-4`
   - Line Color: 黄色
   - Line Width: `0.1`
   - Line Length: `10`

#### 方法2：手动设置为Camera子物体

1. 创建Sprite，命名为 `JudgmentLine`
2. **拖到Camera下，成为Camera的子物体**
3. 设置Transform（相对于Camera）：
   - Position: `(0, -4, 0)`
   - Rotation: `(0, 0, 0)`
   - Scale: `(10, 0.1, 1)`
4. 设置SpriteRenderer颜色为黄色或白色

**注意**：方法1使用脚本自动跟随，更灵活；方法2使用父子关系，更简单但不够灵活。

## 二、判定系统设置

### 1. 添加判定检测器

在ChartEditor场景中：

1. 选择 `ChartEditorManager` 所在的GameObject（或创建新的空GameObject）
2. 添加组件 `EditorJudgmentDetector`
3. 设置Inspector字段：
   - **Manager**: 拖入ChartEditorManager
   - **Hit Sound Source**: 留空（会自动创建）
   - **Perfect Hit Sound**: 拖入打击音效（可选）
   - **Great Hit Sound**: 拖入打击音效（可选）
   - **Good Hit Sound**: 拖入打击音效（可选）
   - **Judgment Line Y**: `-4`（与判定线位置一致）
   - **Judgment Window**: `0.05`

### 2. 绑定到ChartEditorManager

1. 选择 `ChartEditorManager` 所在的GameObject
2. 在Inspector中找到 `Judgment Detector` 字段
3. 拖入刚才添加的 `EditorJudgmentDetector` 组件

### 3. 准备打击音效（可选）

如果你想要判定音效：

#### 选项A：使用Resources文件夹（自动加载）

1. 在 `Assets/Resources/Audio/HitSounds/` 创建文件夹
2. 放入音效文件：
   - `Perfect.wav` 或 `Perfect.mp3`
   - `Great.wav` 或 `Great.mp3`
   - `Good.wav` 或 `Good.mp3`
3. 脚本会自动加载这些音效

#### 选项B：手动拖入Inspector

1. 在 `EditorJudgmentDetector` 的Inspector中
2. 直接拖入音效文件到对应字段

#### 选项C：使用游戏模式的打击音效

如果你的游戏模式已经有打击音效，可以复用：
- 找到游戏模式中的打击音效资源
- 拖入到 `EditorJudgmentDetector` 的对应字段

## 三、判定系统工作原理

### Chart Editor vs 游戏模式

| 特性 | 游戏模式 | Chart Editor模式 |
|------|---------|-----------------|
| 判定触发 | 玩家按键时 | note经过判定线时 |
| Note消失 | 判定后消失 | 永不消失（可编辑） |
| 判定音效 | 按键时播放 | 自动播放 |
| 目的 | 游玩 | 预览和编辑谱面 |

### 判定检测逻辑

```
每帧检测：
1. 检查是否在播放模式
2. 遍历所有EditorNote
3. 检测note的Y位置是否 <= 判定线Y位置
4. 检测当前时间是否 >= note的时间
5. 如果两个条件都满足 → 播放判定音效
6. 标记为已判定，避免重复播放
```

### 停止时重置

当点击Stop按钮时：
- 自动重置所有判定状态
- 下次播放时重新判定

## 四、调试工具

### Scene视图中的判定线

运行场景后，在Scene视图中可以看到：
- **黄色线条** = 判定线位置（Y = -4）
- 这是Gizmos绘制的，只在编辑器中可见

### 检查判定是否工作

1. 运行场景
2. 点击Play按钮
3. 观察Console是否有错误
4. 听是否有打击音效
5. 如果没有音效：
   - 检查AudioSource的Volume
   - 检查是否正确绑定了音效文件
   - 检查GameSettings.HitSoundVolume的值

## 五、常见问题

### Q: 判定线看不见？
A: 检查：
- SpriteRenderer的Color是否设置为可见颜色
- Sorting Layer是否正确
- 镜头是否能看到Y=-4的位置

### Q: 没有判定音效？
A: 检查：
1. EditorJudgmentDetector是否正确添加
2. 是否绑定到ChartEditorManager
3. 音效文件是否正确加载
4. AudioSource的Volume是否为0
5. GameSettings.HitSoundVolume是否为0

### Q: 判定音效重复播放？
A: 这是正常的，因为：
- 脚本有防重复机制（judgmentWindow）
- 如果还是重复，增大judgmentWindow的值

### Q: 判定线跟着镜头移动？
A: 检查：
- 判定线GameObject是否是Camera的子物体
- 如果是，移出来作为独立GameObject

## 六、完整检查清单

设置完成后，检查以下项目：

- [ ] 判定线GameObject已创建，Position = (0, -4, 0)
- [ ] 判定线不是Camera的子物体
- [ ] EditorJudgmentDetector组件已添加
- [ ] EditorJudgmentDetector.manager已绑定
- [ ] ChartEditorManager.judgmentDetector已绑定
- [ ] 打击音效已准备（可选）
- [ ] 运行场景，点击Play，能听到音效
- [ ] Scene视图中能看到黄色判定线Gizmos

## 七、下一步

设置完成后，你可以：
1. 播放谱面，预览判定效果
2. 调整judgmentLineY的值来改变判定线位置
3. 更换不同的打击音效
4. 根据需要调整判定窗口（judgmentWindow）
