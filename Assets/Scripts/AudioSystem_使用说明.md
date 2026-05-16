# 音频系统使用说明

## 系统概述

这个音频系统提供了完整的BGM和音效管理功能，包括：
- 全局音量控制（BGM和SFX分离）
- 音频淡入淡出效果
- 按钮音效自动绑定
- 集中化的音频资源管理

## 系统组成

### 1. AudioManager（音频管理器）
- **位置**: `Assets/Scripts/AudioManager.cs`
- **功能**: 单例模式的全局音频管理器，负责播放所有BGM和音效
- **主要方法**:
  - `PlayBGM(string bgmName)` - 播放背景音乐
  - `StopBGM()` - 停止背景音乐
  - `PlaySFX(string sfxName)` - 播放音效
  - `SetBGMVolume(float volume)` - 设置BGM音量
  - `SetSFXVolume(float volume)` - 设置SFX音量

### 2. AudioClipData（音频资源配置）
- **位置**: `Assets/Scripts/AudioClipData.cs`
- **功能**: ScriptableObject，集中管理所有音频资源
- **包含**: BGM列表和SFX列表

### 3. ButtonSoundEffect（按钮音效组件）
- **位置**: `Assets/Scripts/ButtonSoundEffect.cs`
- **功能**: 附加到Button上，自动播放点击和悬停音效
- **特性**: 支持音效名称或直接指定AudioClip

### 4. ButtonSoundEffectEditor（编辑器工具）
- **位置**: `Assets/Scripts/Editor/ButtonSoundEffectEditor.cs`
- **功能**: 批量为场景中所有按钮添加音效组件
- **菜单**: `Tools/Audio/批量添加按钮音效`

## 初始设置步骤

### 步骤1: 创建AudioClipData配置文件

1. 在Unity编辑器中，右键点击 `Assets` 文件夹
2. 选择 `Create > Audio > Audio Clip Data`
3. 命名为 `AudioClipData`
4. 在Inspector中添加你的音频资源：
   - **BGM列表**: 添加所有背景音乐
     - 例如: `TitleBGM`, `MapBGM`, `GameBGM` 等
   - **SFX列表**: 添加所有音效
     - 例如: `ButtonClick`, `ButtonHover`, `NoteHit` 等

### 步骤2: 创建AudioManager游戏对象

1. 在Hierarchy中创建空游戏对象，命名为 `AudioManager`
2. 添加 `AudioManager` 组件
3. 在Inspector中设置：
   - **Audio Clip Data**: 拖入刚创建的AudioClipData资源
   - **Fade Duration**: 设置BGM淡入淡出时间（默认1秒）
4. AudioManager会自动创建两个子对象：
   - `BGM_AudioSource` - 用于播放背景音乐
   - `SFX_AudioSource` - 用于播放音效

**重要**: AudioManager会自动设置为 `DontDestroyOnLoad`，在场景切换时不会被销毁。

### 步骤3: 为按钮添加音效

#### 方法A: 使用编辑器工具（推荐）

1. 打开 `Tools > Audio > 批量添加按钮音效`
2. 设置音效参数：
   - **点击音效名称**: `ButtonClick`（或你在AudioClipData中设置的名称）
   - **悬停音效名称**: `ButtonHover`（可选）
   - **点击音量**: 1.0
   - **悬停音量**: 0.5
3. 点击 `为所有按钮添加音效组件`

#### 方法B: 手动添加

1. 选择任意Button游戏对象
2. 添加 `ButtonSoundEffect` 组件
3. 设置音效名称或直接拖入AudioClip

### 步骤4: 集成到设置界面

`SettingsUIManager` 已经自动集成了AudioManager：
- 当用户调整音乐音量滑块时，会自动更新AudioManager的BGM音量
- 当用户调整音效音量滑块时，会自动更新AudioManager的SFX音量

## 使用示例

### 播放BGM

```csharp
// 在任意脚本中播放BGM
AudioManager.Instance.PlayBGM("TitleBGM");

// 停止BGM（带淡出效果）
AudioManager.Instance.StopBGM();

// 暂停/恢复BGM
AudioManager.Instance.PauseBGM();
AudioManager.Instance.ResumeBGM();
```

### 播放音效

```csharp
// 通过名称播放音效
AudioManager.Instance.PlaySFX("ButtonClick");

// 通过AudioClip播放音效
AudioManager.Instance.PlaySFX(myAudioClip);

// 播放音效并调整音量
AudioManager.Instance.PlaySFX("NoteHit", 0.8f);
```

### 按钮音效

```csharp
// ButtonSoundEffect组件会自动处理按钮点击音效
// 无需额外代码，只需在Inspector中配置即可

// 如果需要手动触发
ButtonSoundEffect soundEffect = button.GetComponent<ButtonSoundEffect>();
soundEffect.PlayClickSound();
soundEffect.PlayHoverSound();
```

## 音频资源命名建议

### BGM命名
- `TitleBGM` - 标题界面背景音乐
- `MapBGM` - 地图界面背景音乐
- `GameBGM_Easy` - 游戏简单难度背景音乐
- `GameBGM_Hard` - 游戏困难难度背景音乐
- `ResultBGM` - 结算界面背景音乐

### SFX命名
- `ButtonClick` - 按钮点击音效
- `ButtonHover` - 按钮悬停音效
- `NoteHit` - 音符击中音效
- `NoteMiss` - 音符未击中音效
- `ComboBreak` - 连击中断音效
- `LevelComplete` - 关卡完成音效
- `MenuOpen` - 菜单打开音效
- `MenuClose` - 菜单关闭音效

## 场景切换时的BGM管理

由于AudioManager使用了 `DontDestroyOnLoad`，它会在场景切换时保持存在。你可以：

```csharp
// 在场景加载时切换BGM
void Start()
{
    // 检查当前播放的BGM
    if (AudioManager.Instance.GetCurrentBGMName() != "MapBGM")
    {
        AudioManager.Instance.PlayBGM("MapBGM");
    }
}
```

## 音量控制流程

1. 用户在设置界面调整音量滑块
2. `SettingsUIManager` 更新 `GameSettings.MusicVolume` 或 `GameSettings.NoteVolume`
3. `SettingsUIManager` 调用 `AudioManager.Instance.SetBGMVolume()` 或 `SetSFXVolume()`
4. AudioManager更新对应AudioSource的音量
5. 设置通过 `PlayerPrefs` 持久化保存

## 注意事项

1. **AudioManager必须在场景中存在**: 建议在第一个加载的场景（如Title场景）中创建
2. **音频资源必须在AudioClipData中配置**: 否则会找不到音频文件
3. **音效可以同时播放多个**: 使用 `PlayOneShot` 实现
4. **BGM同时只能播放一个**: 切换BGM时会自动停止当前BGM
5. **音量范围是0-1**: GameSettings中保存的音量值范围是0-1

## 调试技巧

### 检查AudioManager是否存在
```csharp
if (AudioManager.Instance == null)
{
    Debug.LogError("AudioManager未初始化！");
}
```

### 检查音频是否配置
```csharp
if (AudioManager.Instance.audioClipData == null)
{
    Debug.LogError("AudioClipData未设置！");
}
```

### 检查当前播放状态
```csharp
Debug.Log($"当前BGM: {AudioManager.Instance.GetCurrentBGMName()}");
Debug.Log($"BGM是否播放: {AudioManager.Instance.IsBGMPlaying()}");
```

## 扩展功能建议

如果需要更多功能，可以考虑添加：
- 音频混音器（Audio Mixer）支持
- 3D空间音效
- 音频池（Audio Pool）优化性能
- 音频事件系统
- 音频可视化

## 常见问题

**Q: 为什么听不到音效？**
A: 检查以下几点：
1. AudioManager是否存在于场景中
2. AudioClipData是否正确配置
3. 音效名称是否正确
4. GameSettings.NoteVolume是否为0

**Q: BGM切换时没有淡入淡出效果？**
A: 检查AudioManager的 `fadeDuration` 是否大于0

**Q: 按钮点击没有音效？**
A: 检查：
1. Button是否有ButtonSoundEffect组件
2. ButtonSoundEffect的音效名称或AudioClip是否设置
3. autoAddListener是否为true

**Q: 场景切换后AudioManager消失了？**
A: AudioManager使用了DontDestroyOnLoad，不应该消失。检查是否有多个AudioManager实例导致冲突。

## 技术支持

如有问题，请检查Unity Console中的日志输出，AudioManager会输出详细的调试信息。
