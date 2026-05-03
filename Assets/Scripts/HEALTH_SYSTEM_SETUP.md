# 生命值系统设置指南

## 已完成的工作

已创建以下脚本：
1. **HealthSystem.cs** - 生命值系统核心逻辑
2. **HealthUI.cs** - 生命值UI显示
3. 已修改 **ScoreManager.cs** - 集成miss扣血和combo回血
4. 已修改 **NoteManager.cs** - 初始化生命值系统

## Unity编辑器设置步骤

### 1. 创建HealthSystem GameObject

1. 在PlayScene场景中，创建一个空GameObject
2. 命名为 `HealthSystem`
3. 添加 `HealthSystem` 组件
4. 在Inspector中配置：
   - **Max Health**: 15 (3颗心 × 5格 = 15)
   - **Damage Per Miss**: 1 (每次miss扣1/5颗心)
   - **Easy Combo Threshold**: 10 (简单难度10连击回血)
   - **Normal Combo Threshold**: 20 (普通难度20连击回血)
   - **Hard Combo Threshold**: 30 (困难难度30连击回血)
   - **Heal Amount**: 1 (每次回血1格)

### 2. 创建HealthUI

1. 在Canvas下创建一个空GameObject
2. 命名为 `HealthUI`
3. 设置RectTransform：
   - **Anchor**: 左下角 (0, 0)
   - **Pivot**: (0, 0)
   - **Position**: (20, 20)
   - **Size**: (300, 80)

4. 在HealthUI下创建3个子对象，命名为 `Heart1`, `Heart2`, `Heart3`
5. 每个Heart设置：
   - 添加 `Image` 组件
   - **RectTransform**:
     - Anchor: 左中 (0, 0.5)
     - Pivot: (0, 0.5)
     - Position X: 0, 90, 180 (分别对应Heart1, Heart2, Heart3)
     - Position Y: 0
     - Size: (80, 80)

6. 在HealthUI GameObject上添加 `HealthUI` 组件
7. 将3个Heart的Image组件拖到 `Heart Images` 数组中

### 3. 准备心形精灵图

你需要准备6个心形精灵图（Sprite）：
- **heartFull** - 满心 (5/5)
- **heart4** - 4/5心
- **heart3** - 3/5心
- **heart2** - 2/5心
- **heart1** - 1/5心
- **heartEmpty** - 空心 (0/5)

将这些精灵图拖到HealthUI组件的对应字段中。

### 4. 系统工作原理

#### 扣血机制
- 每次 **MISS** 扣除 1/5 颗心（1格血量）
- 当血量归零时，游戏结束

#### 回血机制
- 通过连击（Combo）回血
- 不同难度需要的连击数不同：
  - **Easy**: 每10连击回血1格
  - **Normal**: 每20连击回血1格
  - **Hard**: 每30连击回血1格
- 回血上限为满血（15格）
- Miss会重置连击计数和回血进度

#### UI显示
- 3颗心显示在左下角
- 每颗心有5个状态（满、4/5、3/5、2/5、1/5、空）
- 实时反映当前血量

## 测试建议

1. 进入PlayScene
2. 开始游戏
3. 故意miss几个音符，观察心形UI的变化
4. 保持连击，观察是否在达到阈值时回血
5. 验证不同难度的回血阈值是否正确

## 可选：创建临时心形精灵图

如果暂时没有心形图片，可以：
1. 使用Unity内置的UI Sprite（如Circle）
2. 通过改变Image的fillAmount来模拟不同血量
3. 或者先用纯色方块代替，后续再替换为美术资源
