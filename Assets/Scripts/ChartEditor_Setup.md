# Chart Editor - 制谱器设置指南

## 概述

制谱器是一个可视化的谱面编辑工具，支持：
- 实时预览所有note
- 播放/暂停/滚轮控制
- 拖动note改变位置
- 右键切换note类型
- 添加/删除note
- 吸附功能
- 保存到.asset文件

## 场景设置步骤

### 第1步：创建Chart Editor场景

1. 创建新场景：`File > New Scene`
2. 命名为 `ChartEditor`
3. 保存到 `Assets/Scenes/ChartEditor.unity`

### 第2步：设置Main Camera

1. 选中Main Camera
2. 设置Position: `(0, 0, -10)`
3. 设置Projection: `Orthographic`
4. 设置Size: `5` (根据需要调整)
5. 添加 `EditorCameraController` 组件

### 第3步：创建判定线

1. 创建空GameObject，命名为 `JudgmentLine`
2. Position: `(0, -4, 0)` (与游戏模式一致)
3. 添加SpriteRenderer或LineRenderer显示判定线
4. 判定线应该固定在屏幕下方

### 第4步：创建EditorNote预制体

1. 创建空GameObject，命名为 `EditorNote`
2. 添加组件：
   - `SpriteRenderer` (使用方形sprite)
   - `BoxCollider2D` (用于鼠标点击检测)
   - `EditorNote` 脚本
3. 设置SpriteRenderer:
   - Sprite: 使用方形或圆形sprite
   - Color: 白色 (会被脚本动态改变)
   - Sorting Layer: 确保在判定线之上
4. 设置BoxCollider2D:
   - Size: 与sprite大小匹配
5. 保存为预制体：拖到 `Assets/Prefabs/EditorNote.prefab`

### 第5步：创建ChartEditorManager

1. 创建空GameObject，命名为 `ChartEditorManager`
2. 添加 `ChartEditorManager` 组件
3. 添加 `AudioSource` 组件
4. 在Inspector中设置：

**Chart Data**:
- Current Chart: 拖入要编辑的ChartData资源

**Prefabs**:
- Editor Note Prefab: 拖入EditorNote预制体

**Audio**:
- Audio Source: 自动引用

**Camera**:
- Editor Camera: 拖入Main Camera
- Camera Controller: 自动引用

**Layout Settings**:
- Lane X Positions: `[-3.75, -1.25, 1.25, 3.75]` (与游戏一致)
- Time To Y Scale: `10` (1秒 = 10单位高度)
- Judgment Line Y: `-4` (与判定线位置一致)

**Snap Settings**:
- Snap Enabled: ✓ 勾选
- Snap Interval: `0.25` (1/4拍)

### 第6步：创建UI Canvas

1. 创建UI Canvas: `右键 > UI > Canvas`
2. 设置Canvas Scaler:
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920x1080`

### 第7步：创建UI元素

#### Playback Controls (播放控制)
```
Canvas
└── PlaybackPanel
    ├── PlayPauseButton (Button)
    │   └── Text: "Play"
    └── StopButton (Button)
        └── Text: "Stop"
```

#### Time Display (时间显示)
```
Canvas
└── TimePanel
    ├── CurrentTimeText (TextMeshProUGUI): "00:00.000"
    ├── TimelineSlider (Slider)
    └── TotalTimeText (TextMeshProUGUI): "00:00"
```

#### Selected Note Info (选中note信息)
```
Canvas
└── SelectedNotePanel
    ├── NoteTimeText (TextMeshProUGUI): "Time: 0.000s"
    ├── NoteLaneText (TextMeshProUGUI): "Lane: 0"
    └── NoteTypeText (TextMeshProUGUI): "Type: Color - Red"
```

#### Edit Controls (编辑控制)
```
Canvas
└── EditPanel
    ├── DeleteNoteButton (Button): "Delete Note"
    ├── AddNoteButton (Button): "Add Note"
    ├── SnapToggle (Toggle): "Snap"
    └── SnapIntervalInput (TMP_InputField): "0.25"
```

#### Save (保存)
```
Canvas
└── SavePanel
    ├── SaveButton (Button): "Save Chart"
    └── SaveConfirmationPanel (Panel)
        └── Text: "Chart Saved!"
```

#### Chart Info (谱面信息)
```
Canvas
└── InfoPanel
    ├── ChartNameText (TextMeshProUGUI): "Song Name - Difficulty"
    └── TotalNotesText (TextMeshProUGUI): "Total Notes: 0"
```

### 第8步：绑定UI到ChartEditorUI

1. 在ChartEditorManager下创建空GameObject，命名为 `EditorUI`
2. 添加 `ChartEditorUI` 组件
3. 在Inspector中绑定所有UI元素：

**Playback Controls**:
- Play Pause Button → PlayPauseButton
- Play Pause Button Text → PlayPauseButton的Text子对象
- Stop Button → StopButton

**Time Display**:
- Current Time Text → CurrentTimeText
- Total Time Text → TotalTimeText
- Timeline Slider → TimelineSlider

**Selected Note Info**:
- Selected Note Panel → SelectedNotePanel
- Note Time Text → NoteTimeText
- Note Lane Text → NoteLaneText
- Note Type Text → NoteTypeText

**Edit Controls**:
- Delete Note Button → DeleteNoteButton
- Add Note Button → AddNoteButton
- Snap Toggle → SnapToggle
- Snap Interval Input → SnapIntervalInput

**Save**:
- Save Button → SaveButton
- Save Confirmation Panel → SaveConfirmationPanel

**Chart Info**:
- Chart Name Text → ChartNameText
- Total Notes Text → TotalNotesText

4. 在ChartEditorManager中绑定：
   - Editor UI → EditorUI GameObject

## 使用方法

### 基本操作

**播放控制**:
- 点击 `Play` → 开始播放，镜头自动向上移动
- 点击 `Pause` → 暂停播放，镜头停止
- 点击 `Stop` → 停止并回到开头

**镜头控制（暂停时）**:
- 鼠标滚轮向上 → 镜头向上移动，音乐快进
- 鼠标滚轮向下 → 镜头向下移动，音乐后退
- 拖动时间轴滑块 → 跳转到指定时间

**编辑Note**:
- 左键点击note → 选中note
- 左键拖动note → 左右移动（改变lane）
- 右键点击note → 切换类型
  - 颜色note: Red → Green → Blue → Red
  - 方向note: Left → Right → Left
- 点击 `Delete Note` → 删除选中的note

**添加Note**:
- 点击 `Add Note` → 在当前时间和lane 0添加红色note
- 可以拖动到其他lane
- 可以右键切换类型

**吸附功能**:
- 勾选 `Snap` → 启用吸附
- 输入框设置吸附间隔（秒）
- 添加note时会自动吸附到最近的网格点

**保存**:
- 点击 `Save Chart` → 保存到.asset文件
- 显示 "Chart Saved!" 确认

### 快捷键（可扩展）

当前版本没有快捷键，可以在脚本中添加：
- Space: 播放/暂停
- Delete: 删除选中note
- Ctrl+S: 保存

## 工作流程示例

### 编辑现有谱面

1. 在ChartEditorManager中指定要编辑的ChartData
2. 运行场景
3. 所有note自动生成在场景中
4. 点击Play预览谱面
5. 暂停后用滚轮定位到需要修改的位置
6. 拖动note调整位置
7. 右键切换note类型
8. 点击Save保存修改

### 创建新谱面

1. 在Project中创建新的ChartData资源
2. 设置歌曲名、难度、BPM、音频文件
3. 在ChartEditorManager中指定这个ChartData
4. 运行场景
5. 播放音乐，在合适的时间点击Add Note
6. 调整note的lane和类型
7. 重复添加所有note
8. 点击Save保存

## 技术细节

### 坐标系统

**时间 → Y坐标**:
```
Y = time (秒) × timeToYScale
例如: 10秒 = 10 × 10 = Y坐标100
```

**Y坐标 → 时间**:
```
time = Y / timeToYScale
例如: Y坐标100 = 100 / 10 = 10秒
```

**X坐标 → Lane**:
```
Lane 0: X = -3.75
Lane 1: X = -1.25
Lane 2: X = 1.25
Lane 3: X = 3.75
```

### 镜头移动

**播放时**:
```csharp
镜头Y = 当前音乐时间 × timeToYScale + 判定线偏移
```

**暂停时滚轮控制**:
```csharp
新时间 = 当前时间 + 滚轮方向 × scrollTimeStep
镜头Y = 新时间 × timeToYScale + 判定线偏移
```

### Note类型切换顺序

**颜色Note**:
```
Red → Green → Blue → Red (循环)
```

**方向Note**:
```
DirectionalLeft ⇄ DirectionalRight (切换)
```

### 吸附计算

```csharp
snappedTime = Round(time / snapInterval) × snapInterval

例如: 
time = 1.37秒, snapInterval = 0.25
snappedTime = Round(1.37 / 0.25) × 0.25 = 1.25秒
```

## 注意事项

1. **保存只在Editor模式有效** - 运行时的保存使用EditorUtility，只在Unity编辑器中有效
2. **Note不会消失** - 与游戏模式不同，note经过判定线不会消失
3. **拖动只能左右** - note只能改变lane，不能改变时间（Y轴固定）
4. **时间精度** - 显示精度为毫秒（000），吸附精度可自定义
5. **音效系统** - 使用ScoreManager的音效，确保场景中有ScoreManager或音效系统

## 扩展功能（可选）

### 添加BPM网格线

在场景中生成网格线显示节拍：
```csharp
float bpm = currentChart.bpm;
float beatInterval = 60f / bpm; // 一拍的时间
for (float time = 0; time < songLength; time += beatInterval)
{
    float y = TimeToYPosition(time);
    // 绘制横线
}
```

### 添加波形显示

使用AudioClip的采样数据绘制波形背景。

### 多选和批量操作

支持Shift+点击多选note，批量移动或删除。

### 撤销/重做

实现命令模式，记录所有编辑操作。

### 复制/粘贴

Ctrl+C复制选中note，Ctrl+V粘贴到当前时间。

## 相关文件

- `ChartEditorManager.cs` - 主控制器
- `EditorNote.cs` - 可编辑note组件
- `EditorCameraController.cs` - 镜头控制
- `ChartEditorUI.cs` - UI控制
- `ChartData.cs` - 谱面数据结构

## 故障排除

### Q: Note没有生成？
A: 检查ChartData是否正确绑定，检查editorNotePrefab是否设置。

### Q: 无法拖动note？
A: 检查EditorNote是否有BoxCollider2D，检查Camera是否有Physics2DRaycaster。

### Q: 右键切换类型没反应？
A: 检查EditorNote脚本是否正确挂载，检查Console是否有错误。

### Q: 保存后没有变化？
A: 确保在Unity编辑器中运行，检查Console是否显示"Chart saved"。

### Q: 滚轮控制不工作？
A: 确保游戏处于暂停状态，检查EditorCameraController是否正确初始化。

### Q: 音效没有播放？
A: 检查场景中是否有ScoreManager，或者添加独立的音效系统。
