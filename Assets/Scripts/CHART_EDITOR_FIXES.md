# 制谱器修复说明

## 修复的问题

### 1. Chart未赋值时的错误处理 ✅
- 添加了完整的空值检查
- 当chart或notePrefab未赋值时，会显示警告信息而不是崩溃
- 初始化失败时会阻止后续操作

### 2. 音乐播放问题 ✅
- 修复了空格键后音乐不播放的问题
- 改进了音频的暂停/恢复逻辑
- 添加了音频时间同步机制
- 支持从任意时间点开始播放

### 3. Note移动控制问题 ✅
- 创建了专门的 `EditorNote` 组件
- 使用Rigidbody2D的velocity控制移动，而不是Transform.Translate
- 这样可以通过设置velocity为0来完美暂停Note
- 恢复时重新设置velocity来继续移动

### 4. 与NoteManager的兼容性 ✅
- ChartEditorManager只在Chart Editor场景中使用
- NoteManager在PlayScene中正常工作
- 两者互不干扰，使用不同的Note组件

## 新增文件

### EditorNote.cs
专门为制谱器设计的Note控制脚本：
- 使用Rigidbody2D.linearVelocity控制移动
- 提供Pause()和Resume()方法
- 不会与NoteManager的Note类冲突

## 使用说明

### 在Unity中配置

1. **打开Chart Editor场景**
   - 场景路径: `Assets/Scenes/Chart Editer.unity`

2. **选择ChartEditorManager GameObject**
   - 在Hierarchy中找到 `ChartEditorManager`

3. **配置必需字段**
   - **Note Prefab**: 拖入你的Note预制体
   - **Current Chart**: 拖入你想要编辑的ChartData

4. **可选配置**
   - **Seek Step Size**: 左右键跳转的秒数（默认1秒）
   - **Note Travel Time**: Note从生成到判定线的时间（默认2秒）
   - **Lane X Positions**: 四个轨道的X坐标

### 控制说明

- **空格键 (SPACE)**: 暂停/播放
  - 暂停时：音乐停止，所有Note停止移动
  - 播放时：音乐继续，所有Note恢复移动

- **左箭头键 (LEFT)**: 回退1秒
  - 清除所有Note
  - 音乐时间回退
  - 根据新时间重新生成Note

- **右箭头键 (RIGHT)**: 快进1秒
  - 清除所有Note
  - 音乐时间前进
  - 根据新时间重新生成Note

### 工作流程

1. 在Inspector中配置好Chart和Note Prefab
2. 按Play进入游戏模式
3. 默认处于暂停状态
4. 按空格键开始播放
5. 使用左右键调整时间轴
6. 按空格键暂停来精确查看某个时间点的Note布局

## 技术细节

### ChartEditorManager vs NoteManager

| 特性 | ChartEditorManager | NoteManager |
|------|-------------------|-------------|
| 使用场景 | Chart Editor | PlayScene |
| Note组件 | EditorNote | Note |
| 移动方式 | Rigidbody2D.velocity | Transform.Translate |
| 暂停控制 | 设置velocity=0 | 不支持暂停 |
| 时间轴控制 | 支持跳转 | 线性播放 |
| 打击判定 | 无（仅预览） | 完整判定系统 |

### EditorNote vs Note

**EditorNote** (用于制谱器):
- 使用Rigidbody2D控制移动
- 支持暂停/恢复
- 没有打击判定逻辑
- 只负责显示和移动

**Note** (用于游戏):
- 使用Transform.Translate移动
- 不支持暂停
- 包含完整的游戏逻辑
- 与PlayerController交互

## 常见问题

### Q: 为什么按空格后没有声音？
A: 检查以下几点：
1. ChartData是否已赋值
2. ChartData中的audioClip是否已设置
3. AudioSource组件是否存在
4. 音量设置是否正确

### Q: 为什么Note不移动？
A: 检查以下几点：
1. Note Prefab是否已赋值
2. 是否按了空格键开始播放
3. 检查Console是否有错误信息

### Q: 左右键跳转后Note位置不对？
A: 这是正常的，系统会根据当前时间计算Note应该在的位置。如果Note已经过了判定线，就不会显示。

### Q: 会影响PlayScene的游戏吗？
A: 不会。ChartEditorManager只在Chart Editor场景中工作，PlayScene使用的是NoteManager，两者完全独立。

## 下一步开发建议

1. **添加Note放置功能**
   - 点击轨道添加Note
   - 拖拽调整Note位置
   - 删除Note

2. **时间轴UI**
   - 显示当前时间
   - 显示总时长
   - 可视化时间轴拖动

3. **网格对齐**
   - 按节拍对齐Note
   - BPM设置
   - 节拍线显示

4. **保存功能**
   - 保存修改后的ChartData
   - 导出为JSON
   - 撤销/重做功能
