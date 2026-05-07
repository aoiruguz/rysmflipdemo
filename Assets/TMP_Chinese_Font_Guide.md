# TextMeshPro 中文字体配置指南

## 问题
TextMeshPro 默认不支持中文，显示为口口口（方框）。

## 解决方案

### 方法一：使用 TMP Font Asset Creator（推荐）

#### 步骤 1：准备中文字体文件
1. 下载支持中文的字体文件（.ttf 或 .otf）
   - 推荐字体：
     - **思源黑体**（Source Han Sans）- 免费开源
     - **思源宋体**（Source Han Serif）- 免费开源
     - **微软雅黑**（Microsoft YaHei）- Windows 自带
     - **苹方**（PingFang）- macOS 自带

2. 将字体文件拖入 Unity 项目的 `Assets/Fonts/` 文件夹

#### 步骤 2：创建 TMP 字体资源
1. 打开 **Window > TextMeshPro > Font Asset Creator**
2. 配置参数：
   - **Source Font File**: 选择你的中文字体
   - **Sampling Point Size**: 设置为 `Auto Sizing`
   - **Padding**: 5
   - **Packing Method**: `Optimum`
   - **Atlas Resolution**: 
     - 常用汉字：`2048 x 2048`
     - 完整汉字：`4096 x 4096`
   - **Character Set**: 选择 `Custom Characters`
   - **Custom Character List**: 粘贴下面的常用汉字

#### 常用汉字字符集（复制粘贴）：
```
的一是在不了有和人这中大为上个国我以要他时来用们生到作地于出就分对成会可主发年动同工也能下过子说产种面而方后多定行学法所民得经十三之进着等部度家电力里如水化高自二理起小物现实加量都两体制机当使点从业本去把性好应开它合还因由其些然前外天政四日那社义事平形相全表间样与关各重新线内数正心反你明看原又么利比或但质气第向道命此变条只没结解问意建月公无系军很情者最立代想已通并提直题党程展五果料象员革位入常文总次品式活设及管特件长求老头基资边流路级少图山统接知较将组见计别她手角期根论运农指几九区强放决西被干做必战先回则任取据处队南给色光门即保治北造百规热领七海口东导器压志世金增争济阶油思术极交受联什认六共权收证改清己美再采转更单风切打白教速花带安场身车例真务具万每目至达走积示议声报斗完类八离华名确才科张信马节话米整空元况今集温传土许步群广石记需段研界拉林律叫且究观越织装影算低持音众书布复容儿须际商非验连断深难近矿千周委素技备半办青省列习响约支般史感劳便团往酸历市克何除消构府称太准精值号率族维划选标写存候毛亲快效斯院查江型眼王按格养易置派层片始却专状育厂京识适属圆包火住调满县局照参红细引听该铁价严
，。！？；：""''（）【】《》、·—…￥%0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+-*/=<>@#$&_[]{}|\~`^
```

3. 点击 **Generate Font Atlas** 按钮
4. 等待生成完成（可能需要几分钟）
5. 点击 **Save** 或 **Save as...** 保存字体资源到 `Assets/Fonts/` 文件夹

#### 步骤 3：应用字体到所有 TMP 组件
1. 在项目中搜索所有 TextMeshProUGUI 组件
2. 将生成的字体资源拖到 `Font Asset` 字段

---

### 方法二：使用动态字体（更简单但性能稍差）

#### 步骤 1：创建动态字体资源
1. 右键点击 `Assets/Fonts/` 文件夹
2. 选择 **Create > TextMeshPro > Font Asset**
3. 选择你的中文字体文件
4. 在 Inspector 中设置：
   - **Atlas Population Mode**: `Dynamic`
   - **Atlas Width/Height**: `2048 x 2048`

#### 步骤 2：设置为默认字体
1. 打开 **Edit > Project Settings > TextMesh Pro > Settings**
2. 将你的字体资源拖到 **Default Font Asset** 字段
3. 所有新创建的 TMP 组件都会使用这个字体

---

### 方法三：使用编辑器工具（已创建）

1. 在 Unity 菜单栏选择 **Tools > TMP 中文字体配置**
2. 选择源字体文件
3. 点击 **生成 TMP 字体资源（常用汉字）**
4. 等待生成完成

---

## 推荐字体下载

### 思源黑体（Source Han Sans）
- 下载地址：https://github.com/adobe-fonts/source-han-sans/releases
- 推荐文件：`SourceHanSansCN-Regular.otf`（简体中文，常规）

### 思源宋体（Source Han Serif）
- 下载地址：https://github.com/adobe-fonts/source-han-serif/releases
- 推荐文件：`SourceHanSerifCN-Regular.otf`（简体中文，常规）

---

## 常见问题

### Q: 生成的字体文件很大怎么办？
A: 
- 减少 Atlas Resolution（如 1024x1024）
- 只包含常用汉字，不要包含所有汉字
- 使用动态字体模式

### Q: 某些字符还是显示为方框？
A: 
- 检查字体文件是否支持该字符
- 如果使用静态字体，需要重新生成并包含该字符
- 如果使用动态字体，会自动添加（首次显示可能有延迟）

### Q: 如何批量替换所有 TMP 组件的字体？
A: 使用下面的脚本（放在 Editor 文件夹）：

```csharp
using UnityEngine;
using UnityEditor;
using TMPro;

public class ReplaceAllTMPFonts : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/批量替换 TMP 字体")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceAllTMPFonts>("批量替换字体");
    }

    private void OnGUI()
    {
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField("新字体", newFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("替换所有场景中的 TMP 字体"))
        {
            ReplaceAllFonts();
        }
    }

    private void ReplaceAllFonts()
    {
        if (newFont == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择新字体！", "确定");
            return;
        }

        TextMeshProUGUI[] allTMPs = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        int count = 0;

        foreach (var tmp in allTMPs)
        {
            tmp.font = newFont;
            EditorUtility.SetDirty(tmp);
            count++;
        }

        EditorUtility.DisplayDialog("完成", $"已替换 {count} 个 TMP 组件的字体！", "确定");
    }
}
```

---

## 性能建议

1. **静态字体 vs 动态字体**：
   - 静态字体：预先生成所有字符，性能好，但文件大
   - 动态字体：按需生成字符，文件小，但首次显示有延迟

2. **图集大小**：
   - 移动平台：建议 1024x1024 或 2048x2048
   - PC 平台：可以使用 4096x4096

3. **字符集选择**：
   - 只包含游戏中实际使用的字符
   - 常用汉字（3500字）通常足够
