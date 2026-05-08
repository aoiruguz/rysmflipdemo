# Chart Editor - 快速入门指南

## 🎯 制谱器功能

一个可视化的谱面编辑工具，支持：
- ✅ 实时预览所有note
- ✅ 播放/暂停控制
- ✅ 滚轮快进/后退
- ✅ 拖动note改变位置
- ✅ 右键切换note类型
- ✅ 添加/删除note
- ✅ 吸附功能
- ✅ 保存到.asset文件
- ✅ 打击音效

## 🚀 5分钟快速设置

### 1. 创建EditorNote预制体

1. 创建GameObject → 添加 `SpriteRenderer` + `BoxCollider2D` + `EditorNote`
2. 设置Sprite为方形或圆形
3. 保存为 `Assets/Prefabs/EditorNote.prefab`

### 2. 创建ChartEditor场景

1. 新建场景 `ChartEditor`
2. Main Camera设置为Orthographic
3. 创建判定线（Y = -4）

### 3. 添加ChartEditorManager

1. 创建空GameObject → 添加 `ChartEditorManager` + `AudioSource`
2. 绑定：
   - Current Chart → 要编辑的ChartData
   - Editor Note Prefab → EditorNote预制体
   - Editor Camera → Main Camera

### 4. 创建UI

创建Canvas，添加以下UI元素：
- Play/Pause按钮
- 时间显示（当前时间 + 总时长）
- 时间轴滑块
- 选中note信息面板
- Delete/Add按钮
- Save按钮

### 5. 绑定UI

1. 创建空GameObject → 添加 `ChartEditorUI`
2. 绑定所有UI元素
3. 在ChartEditorManager中绑定EditorUI

## 🎮 使用方法

### 播放控制
- **Play** → 播放音乐，镜头向上移动
- **Pause** → 暂停，可用滚轮控制
- **Stop** → 停止并回到开头

### 编辑Note
- **左键点击** → 选中note
- **左键拖动** → 左右移动（改变lane）
- **右键点击** → 切换类型
  - 颜色: Red → Green → Blue
  - 方向: Left ⇄ Right

### 添加/删除
- **Add Note** → 在当前时间添加note
- **Delete Note** → 删除选中的note

### 吸附功能
- 勾选 **Snap** → 启用吸附
- 设置间隔（秒）→ 例如0.25 = 1/4拍

### 保存
- **Save Chart** → 保存到.asset文件

## 📊 UI布局建议

```
┌─────────────────────────────────────┐
│ [Play] [Stop]  00:00.000 / 03:45   │ ← 顶部控制栏
├─────────────────────────────────────┤
│                                     │
│         [Note在这里显示]            │ ← 主编辑区域
│                                     │
│         ─────────────────           │ ← 判定线
├─────────────────────────────────────┤
│ ████████████████░░░░░░░░░░░░░░░░   │ ← 时间轴滑块
├─────────────────────────────────────┤
│ Selected Note:                      │
│ Time: 10.250s                       │ ← 选中note信息
│ Lane: 2                             │
│ Type: Color - Red                   │
├─────────────────────────────────────┤
│ [Delete] [Add] [✓Snap] [0.25]      │ ← 编辑控制
│ [Save Chart]                        │
│ Total Notes: 150                    │
└─────────────────────────────────────┘
```

## 🔧 核心参数

### ChartEditorManager设置

| 参数 | 默认值 | 说明 |
|------|--------|------|
| Lane X Positions | [-3.75, -1.25, 1.25, 3.75] | 4条lane的X坐标 |
| Time To Y Scale | 10 | 1秒 = 10单位高度 |
| Judgment Line Y | -4 | 判定线Y坐标 |
| Snap Enabled | true | 是否启用吸附 |
| Snap Interval | 0.25 | 吸附间隔（秒） |

### EditorCameraController设置

| 参数 | 默认值 | 说明 |
|------|--------|------|
| Scroll Speed | 2 | 滚轮滚动速度 |
| Scroll Time Step | 0.1 | 每次滚动改变的时间 |

## 💡 工作流程

### 编辑现有谱面
1. 指定ChartData → 运行场景
2. 点击Play预览
3. 暂停后用滚轮定位
4. 拖动/右键编辑note
5. Save保存

### 创建新谱面
1. 创建新ChartData → 设置音频
2. 运行场景 → 播放音乐
3. 在合适时间点击Add Note
4. 调整位置和类型
5. Save保存

## ⚠️ 注意事项

1. **保存只在Editor模式有效** - 运行时保存需要Unity编辑器
2. **Note不会消失** - 与游戏模式不同
3. **拖动只能左右** - 不能改变时间
4. **需要音效系统** - 确保场景中有ScoreManager或音效

## 📁 相关文件

- `ChartEditorManager.cs` - 主控制器
- `EditorNote.cs` - 可编辑note
- `EditorCameraController.cs` - 镜头控制
- `ChartEditorUI.cs` - UI控制
- `ChartEditor_Setup.md` - 完整设置指南

## 🐛 常见问题

### Q: Note没有生成？
A: 检查ChartData和editorNotePrefab是否绑定

### Q: 无法拖动note？
A: 检查EditorNote是否有BoxCollider2D

### Q: 保存后没有变化？
A: 确保在Unity编辑器中运行

### Q: 滚轮不工作？
A: 确保游戏处于暂停状态

## 🎨 扩展功能（可选）

- BPM网格线显示
- 波形背景显示
- 多选和批量操作
- 撤销/重做
- 复制/粘贴
- 快捷键支持

---

详细文档请查看: [ChartEditor_Setup.md](ChartEditor_Setup.md)
