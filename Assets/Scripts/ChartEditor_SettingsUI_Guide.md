# Chart Editor 设置面板使用指南

## 一键生成设置面板

### 使用工具生成

1. 在Unity菜单栏点击：`Tools > Chart Editor > Generate Settings UI`
2. 点击 **"Generate Settings Panel"** 按钮
3. 设置面板自动创建完成！

### 生成的内容

```
Canvas
└── ChartEditorSettingsPanel (默认隐藏)
    ├── Title: "Chart Editor Settings"
    ├── MusicVolumeContainer
    │   ├── Label: "Music Volume"
    │   ├── Value: "100%"
    │   └── Slider (0.0 - 1.0)
    ├── HitSoundVolumeContainer
    │   ├── Label: "Hit Sound Volume"
    │   ├── Value: "100%"
    │   └── Slider (0.0 - 1.0)
    ├── OffsetContainer
    │   ├── Label: "Offset (seconds)"
    │   ├── Value: "0.000s"
    │   ├── InputField
    │   ├── +0.01 Button
    │   └── -0.01 Button
    ├── ResetButton: "Reset to Defaults"
    └── CloseButton: "Close"
```

## 打开设置面板

### 方法1：通过代码

```csharp
// 在ChartEditorUI或其他脚本中
ChartEditorSettingsUI settingsUI = FindObjectOfType<ChartEditorSettingsUI>();
if (settingsUI != null)
{
    settingsUI.Show();
}
```

### 方法2：添加设置按钮

在ChartEditorUI中添加一个设置按钮：

```csharp
public Button settingsButton;
public ChartEditorSettingsUI settingsUI;

void Start()
{
    if (settingsButton != null)
    {
        settingsButton.onClick.AddListener(() => settingsUI.Show());
    }
}
```

### 方法3：快捷键

在ChartEditorManager的Update中添加：

```csharp
// 按ESC或S键打开设置
if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.S))
{
    ChartEditorSettingsUI settingsUI = FindObjectOfType<ChartEditorSettingsUI>();
    if (settingsUI != null)
    {
        settingsUI.Show();
    }
}
```

## 设置面板功能

### 1. 音乐音量 (Music Volume)
- **滑块调整**: 拖动滑块 0% - 100%
- **实时生效**: 调整后立即应用到AudioSource
- **显示**: 右侧显示当前百分比

### 2. 打击音效音量 (Hit Sound Volume)
- **滑块调整**: 拖动滑块 0% - 100%
- **作用**: 控制判定音效的音量
- **显示**: 右侧显示当前百分比

### 3. 偏移 (Offset)
- **输入框**: 直接输入偏移值（秒）
- **+0.01按钮**: 增加0.01秒偏移
- **-0.01按钮**: 减少0.01秒偏移
- **实时生效**: 调整后自动重新生成所有note
- **显示**: 右上角显示当前偏移值

### 4. 重置按钮 (Reset to Defaults)
- 点击后恢复所有设置为默认值：
  - 音乐音量: 80%
  - 打击音效音量: 50%
  - 偏移: 0.000秒
  - 判定线偏移: -4

### 5. 关闭按钮 (Close)
- 关闭设置面板（不会丢失设置）

## 设置的持久化

所有设置都自动保存到PlayerPrefs：
- 关闭Unity后再打开，设置会保留
- 不同项目的设置是独立的

## 快捷键总结

| 按键 | 功能 | 位置 |
|------|------|------|
| `+` / `Keypad +` | 偏移 +0.01秒 | 制谱器主界面（暂停时） |
| `-` / `Keypad -` | 偏移 -0.01秒 | 制谱器主界面（暂停时） |
| 设置面板 +0.01 | 偏移 +0.01秒 | 设置面板内 |
| 设置面板 -0.01 | 偏移 -0.01秒 | 设置面板内 |

## 自定义设置面板

### 修改样式

生成后可以在Inspector中修改：
- 面板大小和位置
- 颜色和透明度
- 字体大小
- 按钮样式

### 添加更多设置

在ChartEditorSettings.cs中添加新设置：

```csharp
public static float NewSetting
{
    get => PlayerPrefs.GetFloat("NewSettingKey", 1.0f);
    set
    {
        PlayerPrefs.SetFloat("NewSettingKey", value);
        PlayerPrefs.Save();
    }
}
```

然后在ChartEditorSettingsUI.cs中添加UI控制。

## 集成到现有UI

### 添加设置按钮到ChartEditorUI

1. 在ChartEditorUI的Canvas中添加一个按钮
2. 命名为 `SettingsButton`
3. 在ChartEditorUI.cs中添加：

```csharp
[Header("Settings")]
public Button settingsButton;
public ChartEditorSettingsUI settingsUI;

void Start()
{
    // ... 其他初始化代码
    
    if (settingsButton != null)
    {
        settingsButton.onClick.AddListener(OnSettingsClicked);
    }
}

void OnSettingsClicked()
{
    if (settingsUI != null)
    {
        settingsUI.Show();
    }
}
```

## 故障排除

### Q: 生成后找不到设置面板？
A: 面板默认是隐藏的，在Hierarchy中找到 `ChartEditorSettingsPanel`，勾选激活查看。

### Q: 点击按钮没反应？
A: 检查ChartEditorSettingsUI组件是否正确绑定了所有UI元素。

### Q: 偏移调整后note没变化？
A: 确保ChartEditorManager.RegenerateAllNotes()方法是public的。

### Q: 音量调整没效果？
A: 检查AudioSource是否正确引用，音量是否被其他地方覆盖。

### Q: 想删除设置面板？
A: 使用工具：`Tools > Chart Editor > Generate Settings UI` → 点击 "Delete Settings Panel"

## 相关文件

- `ChartEditorSettings.cs` - 设置管理类（静态）
- `ChartEditorSettingsUI.cs` - 设置面板UI控制器
- `ChartEditorSettingsUIGenerator.cs` - 自动生成工具
- `ChartEditor_OffsetGuide.md` - 偏移调整详细指南
