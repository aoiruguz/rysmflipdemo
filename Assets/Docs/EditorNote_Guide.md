# EditorNote 使用指南

## 重要改动

### 使用子物体激活方案
EditorNote现在和Note.cs使用**完全相同**的子物体激活/停用方式来显示不同类型的note。

### 预制体设置

**EditorNote预制体必须包含以下子物体：**

```
EditorNote (根物体)
├── BoxCollider2D (必需)
├── colorNoteVisual (子物体 - 颜色note的视觉)
│   └── SpriteRenderer (用于显示颜色)
├── leftDirectionalVisual (子物体 - 左箭头的视觉)
│   └── SpriteRenderer
└── rightDirectionalVisual (子物体 - 右箭头的视觉)
    └── SpriteRenderer
```

**推荐做法：**
1. 复制你的正常Note预制体
2. 重命名为 `EditorNote`
3. 移除 `Note.cs` 组件
4. 添加 `EditorNote.cs` 组件
5. 在Inspector中将三个子物体拖到对应字段：
   - `colorNoteVisual`
   - `leftDirectionalVisual`
   - `rightDirectionalVisual`

## 右键切换功能

### 切换顺序（5种类型循环）

```
红色 (ColorA) 
  ↓ 右键
绿色 (ColorB)
  ↓ 右键
蓝色 (ColorC)
  ↓ 右键
左箭头 (DirectionalLeft)
  ↓ 右键
右箭头 (DirectionalRight)
  ↓ 右键
红色 (ColorA) ← 循环回到开始
```

### 使用方法
1. 在制谱器中，将鼠标悬停在note上
2. **右键点击** note
3. note会切换到下一个类型
4. 播放打击音效作为反馈

## 功能说明

### 左键拖动
- **按住左键**：选中note并开始拖动
- **只能左右移动**：note会自动吸附到最近的轨道
- **松开左键**：停止拖动，播放音效

### 右键切换类型
- **右键点击**：循环切换5种类型
- **实时更新**：子物体立即激活/停用
- **颜色更新**：颜色note会自动更新SpriteRenderer颜色

### 选中状态
- 选中时：scale放大1.2倍
- 取消选中：恢复原始scale
- **保持原始scale**：不会覆盖预制体设置

## 代码对比

### Note.cs (游戏运行时)
```csharp
// 激活/停用子物体
colorNoteVisual.SetActive(NoteType == NoteType.Color);
leftDirectionalVisual.SetActive(NoteType == NoteType.DirectionalLeft);
rightDirectionalVisual.SetActive(NoteType == NoteType.DirectionalRight);

// 更新颜色
if (NoteType == NoteType.Color)
{
    spriteRenderer.color = GetColorForGameColor(Color);
}
```

### EditorNote.cs (制谱器)
```csharp
// 完全相同的逻辑
colorNoteVisual.SetActive(noteData.noteType == NoteType.Color);
leftDirectionalVisual.SetActive(noteData.noteType == NoteType.DirectionalLeft);
rightDirectionalVisual.SetActive(noteData.noteType == NoteType.DirectionalRight);

// 完全相同的颜色更新
if (noteData.noteType == NoteType.Color)
{
    spriteRenderer.color = GetColorForGameColor(noteData.color);
}
```

## 设置步骤

### 1. 创建EditorNote预制体

**方法A：从现有Note复制**
1. 在Project窗口找到你的Note预制体
2. 复制并重命名为 `EditorNote`
3. 双击打开预制体编辑模式
4. 移除 `Note` 组件
5. 添加 `EditorNote` 组件
6. 保存预制体

**方法B：手动创建**
1. 创建空GameObject，命名为 `EditorNote`
2. 添加 `BoxCollider2D` 组件
3. 添加 `EditorNote` 组件
4. 创建三个子物体：
   - `colorNoteVisual` (添加SpriteRenderer)
   - `leftDirectionalVisual` (添加SpriteRenderer)
   - `rightDirectionalVisual` (添加SpriteRenderer)
5. 在EditorNote组件中绑定三个子物体
6. 保存为预制体

### 2. 在ChartEditorManager中设置

1. 选中ChartEditorManager对象
2. 在Inspector中找到 `Editor Note Prefab` 字段
3. 将EditorNote预制体拖入该字段

### 3. 测试

1. 运行制谱器场景
2. 加载一个谱面
3. 右键点击note，观察是否正确切换
4. 检查Console是否有错误

## 故障排除

### Q: 右键点击没反应？
A: 检查以下几点：
- EditorNote是否有BoxCollider2D组件
- 镜头是否有Physics2D Raycaster
- 是否在播放模式下（Edit模式下OnMouseDown不工作）

### Q: 切换类型后note消失？
A: 检查子物体是否正确绑定：
- 在Inspector中查看EditorNote组件
- 确认三个子物体字段都有引用
- 确认子物体有SpriteRenderer组件

### Q: 颜色不对？
A: 检查colorNoteVisual的SpriteRenderer：
- 确认子物体有SpriteRenderer组件
- 确认Sprite已设置
- 确认Material正确

### Q: 切换到箭头后看不见？
A: 检查箭头子物体：
- leftDirectionalVisual和rightDirectionalVisual是否有Sprite
- SpriteRenderer是否启用
- 位置是否正确（相对于父物体）

### Q: 拖拽后scale变大？
A: 这个问题已修复：
- EditorNote在Awake时保存原始scale
- 选中效果基于原始scale进行缩放
- 不会覆盖预制体设置

## 与Note.cs的兼容性

EditorNote和Note使用**完全相同**的子物体结构，这意味着：

✅ **可以直接复制Note预制体**
✅ **子物体结构完全兼容**
✅ **颜色显示逻辑一致**
✅ **类型切换逻辑一致**

唯一的区别：
- Note.cs：游戏运行时使用，有移动逻辑
- EditorNote.cs：制谱器使用，有拖动和编辑逻辑

## 相关文件

- `EditorNote.cs` - 可编辑note组件
- `Note.cs` - 游戏运行时note组件
- `ChartEditorManager.cs` - 制谱器管理器
- `NoteData.cs` - note数据结构
