# Compound Box - 项目完整实现说明

## 1. 项目定位

Compound Box 是一款基于 Unity 2022.3 LTS 的 2D 网格解谜作品集 Demo。

核心机制：

- 多格复合实体
- 逐格材质
- 整块推挤
- 锚点式网格传送
- 全拆分与精确切割
- 相邻同材质重组
- 复合体旋转
- 可移动传送节点
- 复合目标形状

当前范围：

- 12 个关卡
- 4 个章节
- 程序化白盒美术
- Windows 64 位构建
- 20 个 EditMode 测试

## 2. 工程目录

```text
Assets/
  Scenes/
    SampleScene.unity

  Sokoban/
    Scripts/
      Core/
      Runtime/
    Editor/
    Tests/
      EditMode/
    Data/
      Levels/
      CompoundBoxLevelCatalog.asset
    Art/
      Generated/

Packages/
ProjectSettings/
Tools/
Docs/
```

### Core

纯规则、数据、状态和 Command 层。

主要负责：

- 网格状态
- 实体数据
- 关卡解析
- 移动模拟
- 传送
- 切割
- 重组
- 旋转
- 目标判定
- 撤销/重做
- 存档数据
- 关卡审查

### Runtime

Unity 场景运行层。

主要负责：

- 应用启动
- 关卡加载
- 输入处理
- 棋盘表现
- Sprite 渲染
- HUD
- 章节选择
- 音效
- 对象池

### Editor

Unity 编辑器工具。

主要负责：

- 关卡工作台
- 关卡验证
- 解法回放
- 一键进入指定关卡
- 关卡 Catalog 重建
- 本地存档重置
- Windows 构建入口

### Tests

EditMode 自动化测试。

主要覆盖：

- 标准解可完成
- 移动与阻挡
- 多材质
- 传送
- 拆分
- 重组
- 精确切割
- 旋转
- 撤销/重做
- 存档 JSON
- 对象池
- 关卡审查

## 3. 启动流程

场景入口是 `Assets/Scenes/SampleScene.unity`。

场景中有：

- `Main Camera`
- `Compound Box Application`

`Compound Box Application` 挂载 `CompoundBoxApp`。

### Runtime 启动

`CompoundBoxApp` 负责：

1. 设置应用帧率
2. 加载关卡 Catalog
3. 加载存档
4. 初始化摄像机
5. 创建棋盘视图
6. 创建 HUD
7. 创建音效服务
8. 根据存档或编辑器请求加载初始关卡

如果场景中没有 `CompoundBoxApp`，`RuntimeInitializeOnLoadMethod` 会自动创建。

这提供了两层保障：

- 场景中有显式入口
- 场景丢失入口时可以自动恢复

## 4. 主数据模型

### 4.1 GridBoardState

`GridBoardState` 是运行时棋盘状态。

包含：

- 宽高
- 瓦片数组
- 实体集合
- 目标集合
- 静态传送门对
- 动态传送节点
- 玩家 ID
- 下一次实体 ID
- 玩家朝向
- 出口位置
- 移动次数
- 推动次数
- 动作次数

瓦片类型：

```text
Void
Floor
Wall
Portal
Goal
Exit
```

### 4.2 GridEntity

`GridEntity` 是一个玩家、复合体或传送节点。

核心数据：

```text
Id
Kind
EntityCellState[]
EntityConnection[]
```

实体类型：

```text
Player
Matter
PortalNode
```

### 4.3 EntityCellState

每个格子包含：

```text
Vector2Int Position
MatterType Matter
```

这就是逐格材质的基础。

例如一个复合体可以同时包含：

```text
(3, 4) Cyan
(4, 4) Amber
(3, 5) Cyan
```

### 4.4 EntityConnection

连接边记录两个相邻格子的结构关系。

例如：

```text
(3, 4) <-> (4, 4)
```

连接图用于：

- 精确切割
- 重组验证
- 旋转保持内部结构
- 判断一个实体是否仍连通

### 4.5 GoalDefinition

目标包含：

- 目标格子
- 所需材质
- 是否要求复合实体

目标分为：

- 单格材质目标
- 复合材质目标

当前复合目标的实现会要求覆盖目标的实体至少有两个格子，并检查每格材质。

### 4.6 PortalPair

传送门支持：

- 固定入口
- 固定出口
- 可移动入口实体
- 可移动出口实体

固定端点使用坐标。

动态端点使用实体 ID，并在运行时解析实体当前位置。

## 5. 关卡数据

### 5.1 LevelDefinition

运行时关卡定义包含：

```text
Id
DisplayName
Subtitle
Rows
MaterialRows
KnownSolution
```

`Rows` 是地形和对象布局。

`MaterialRows` 是可选的材质层。

### 5.2 LevelDefinitionAsset

Unity 编辑器资产：

```text
levelId
displayName
subtitle
layout
materialLayout
knownSolution
```

LevelDefinitionAsset 转换为 LevelDefinition 后进入运行时。

### 5.3 LevelCatalogAsset

Catalog 负责：

- 保存关卡资产顺序
- 转换为运行时 LevelDefinition 列表

当前文件：

```text
Assets/Sokoban/Data/CompoundBoxLevelCatalog.asset
```

### 5.4 关卡字符格式

地形字符：

| 字符 | 含义 |
| --- | --- |
| `#` | 墙 |
| `.` | 地板 |
| 空格 | 虚空 |
| `@` | 玩家 |
| `1` - `4` | 材质块 |
| `x/y/z/w` | 单格材质目标 |
| `X/Y/Z/W` | 复合目标 |
| `g/G` | 使用材质层定义的通用目标 |
| `a` - `d` | 固定传送门入口 |
| `A` - `D` | 固定传送门出口 |
| `p/P` | 可移动传送节点 |
| `E` | 关卡出口 |

### 5.5 材质层

材质层与地图同高。

如果布局某格是 `1`，但材质层同位置是 `2`，该格实际材质为 Amber。

这允许一个初始复合体包含不同材质。

### 5.6 标准解代码

| 代码 | 含义 |
| --- | --- |
| `U` | 上 |
| `R` | 右 |
| `D` | 下 |
| `L` | 左 |
| `S` | 全拆分 |
| `V` | 精确切割 |
| `C` | 重组 |
| `Q` | 逆时针旋转 |
| `E` | 顺时针旋转 |

标准解用于：

- 自动化验证
- 关卡审查
- 内容回归
- 未来回放系统

## 6. 关卡解析流程

`LevelParser.Parse` 的步骤：

1. 计算地图宽高。
2. 创建 GridBoardState。
3. 遍历布局字符。
4. 识别墙、地板、目标、出口、传送门和对象。
5. 使用材质层覆盖格子材质。
6. 构建玩家实体。
7. 构建 Matter 实体。
8. 构建连接边。
9. 构建传送门对。
10. 刷新动态传送映射。

### 单材质关卡

普通关卡中，相邻且同材质的块会组成一个复合体。

### 多材质关卡

高级关卡使用 `MaterialRows`。

相邻的有效 payload 格子无论材质是否相同，都会进入同一个连通分量，从而形成混合材质复合体。

## 7. 移动系统

移动逻辑位于 `GridBoardSimulation.TryMove`。

### 7.1 移动链

移动不是只移动玩家，而是一次处理整条推挤链：

1. 从玩家开始。
2. 计算玩家候选位置。
3. 检查候选位置是否与实体重叠。
4. 被撞实体加入移动集合。
5. 继续检查该实体会不会推动下一个实体。
6. 重复直到移动集合稳定。

### 7.2 提案阶段

每个移动实体先生成候选格子，不立即写入状态。

候选数据保存在 proposal map 中。

### 7.3 验证阶段

所有候选位置统一检查：

- 是否越界
- 是否撞墙
- 是否与其他实体重复占格

只有全部合法才提交。

### 7.4 提交阶段

实体的 `SetCells` 会：

- 保持格子索引与原材质对应
- 同步更新连接边端点
- 刷新排序

## 8. 传送系统

### 8.1 锚点规则

传送不是逐格传送，而是整体锚点对齐。

流程：

1. 计算实体移动后的候选格子。
2. 检查是否有候选格命中入口。
3. 找到配对出口。
4. 计算候选形状锚点到出口的平移。
5. 对全部候选格应用相同平移。

这保证：

- 复合体不会变形
- 传送后连接关系不丢失
- 材质与格子对应关系不变化

### 8.2 传送循环保护

传送最多执行 4 次跳跃。

超过限制视为可疑循环并阻止移动。

### 8.3 动态传送

可移动传送节点是实体。

每次状态变化后调用 `RefreshPortals`：

- 固定端点解析为固定坐标
- 动态端点解析为实体 Anchor
- 重建入口坐标到 PortalPair 的映射

### 8.4 传送行为

如果物体传送到出口后被另一个实体占据，会进入推挤链。

这允许：

- 物体把自己推出出口
- 传送节点被传送物体推动
- 动态改变传送门位置

## 9. 拆分、切割与重组

### 9.1 全拆分

命令：`X`

行为：

- 面向一个 Matter 实体
- 每个格子生成独立单格实体
- 每个新实体保留自己的材质
- 原连接全部断开

### 9.2 精确切割

命令：`V`

行为：

1. 找到玩家面前的实体格子。
2. 找到该格子朝向内部的连接边。
3. 删除这一条连接。
4. 根据剩余连接计算连通分量。
5. 每个连通分量生成一个独立实体。

精确切割与全拆分的区别：

- 全拆分：每个格子独立
- 精确切割：只切一条连接，保留两个组件内部连接

### 9.3 重组

命令：`C`

当前规则：

- 只处理 Material 实体
- 只处理单材质实体
- 材质必须相同
- 实体必须正交相邻
- 使用 BFS 找出可合并组
- 合并格子与连接
- 自动补充合法的新连接

### 9.4 旋转

命令：

- `Q`：逆时针
- `E`：顺时针

行为：

1. 以实体 Anchor 为旋转中心。
2. 每个格子计算新的相对坐标。
3. 同步旋转连接边。
4. 检查越界、墙和其他实体占用。
5. 全部合法后提交。

## 10. Command 系统

所有 gameplay 操作都封装为 `IGridCommand`。

命令类型：

```text
MoveGridCommand
SplitGridCommand
PrecisionCutGridCommand
RecombineGridCommand
RotateGridCommand
```

每个 Command 保存：

- 执行前状态
- 执行后状态
- 历史标签
- 动作类型

### Execute

1. 克隆当前状态。
2. 调用规则模拟。
3. 成功后保存新状态。
4. 将命令加入 undo stack。
5. 清空 redo stack。

### Undo

1. 弹出 undo 命令。
2. 恢复命令的 before 状态。
3. 将命令加入 redo stack。

### Redo

1. 弹出 redo 命令。
2. 恢复命令的 after 状态。
3. 将命令放回 undo stack。

当前历史上限为 120。

## 11. 输入系统

当前使用 Unity 旧版 Input Manager。

主要输入：

```text
WASD / 方向键    移动
X                全拆分
V                精确切割
Q / E            逆时针/顺时针旋转
C                重组
Z / Ctrl+Z       撤销
Ctrl+Y           重做
Ctrl+Shift+Z     重做
R                重开
L                章节选择
N / Tab / ]      下一关
[                上一关
M                静音
```

输入层只负责：

- 读取按键
- 选择 Command
- 调用 GridSession
- 触发 HUD 和音效

规则层不直接读取 Unity Input。

## 12. 状态机

通用 FSM：

```text
StateMachine<TState>
IStateBehaviour<TState>
```

使用中的状态：

### 关卡

```text
Loading -> Playing -> Completed
```

### 玩家

```text
Idle -> Moving / Interacting -> Idle
```

### 机关

```text
Closed / Open / Locked
```

机关状态已经定义，目前的动态传送门和压力板扩展可以复用。

## 13. 表现层

### 13.1 静态棋盘

`BoardView.RebuildTiles` 创建：

- 地板
- 墙
- 目标
- 出口
- 固定传送门

### 13.2 实体表现

`EntityCellView` 组合：

- 阴影
- 外壳
- 内芯
- 连接标志
- 玩家方向箭头

### 13.3 对象池

当前使用对象池：

- 实体根节点
- 实体格子表现
- 预览格子

减少拆分、重组和移动预览产生的 GC。

### 13.4 移动预览

`TransformPreview` 在真实执行前计算：

- 玩家目标位置
- 被推动实体目标位置
- 传送后整体位置
- 合法性
- 失败原因

合法预览使用材质色，非法预览使用红色。

### 13.5 HUD

HUD 使用 `OnGUI`。

逻辑画布为：

```text
1920 x 1080
```

通过 `GUI.matrix` 等比缩放到当前屏幕。

HUD 包含：

- 关卡名称和章节
- 目标进度
- 移动和推动统计
- 解锁进度
- 本地最佳成绩
- 章节选择按钮
- 操作提示
- 完成面板

## 14. 音频

音频由 `ProceduralAudio` 运行时生成。

音效类型：

- 移动
- 推动
- 阻挡
- 拆分
- 重组
- 传送
- 完成
- UI

使用 `AudioClip.Create` 和波形数据生成，不依赖音频资源文件。

## 15. 存档系统

存档文件：

```text
Application.persistentDataPath/compound-box-save.json
```

存档内容：

```text
version
highestUnlockedLevel
levels[]
audioMuted
```

每关进度：

```text
levelId
completed
bestMoves
bestPushes
```

保存时机：

- 完成关卡
- 修改静音设置
- 编辑器手动重置

当前没有保存：

- 当前棋盘状态
- 玩家位置
- Command 历史
- 当前预览状态

所以中途退出会从关卡初始状态重新开始。

### 当前风险

- 使用普通 `File.WriteAllText`
- 没有原子写入
- 没有备份
- 没有校验和
- 版本迁移目前只覆盖基础字段

正式产品应改为：

1. 写入临时文件。
2. 校验成功。
3. 替换正式存档。
4. 保留上一份备份。

## 16. 编辑器工具

### Level Workbench

入口：

```text
Tools > Compound Box > Level Workbench
```

支持：

- 浏览所有关卡
- 显示关卡指标
- 验证数据
- 回放标准解
- 一键进入指定关卡

### Catalog 构建

入口：

```text
Tools > Compound Box > Create or Refresh Default Catalog
```

作用：

- 根据 BuiltInLevels 生成关卡资产
- 刷新 Catalog 顺序
- 保留编辑器修改后的资产

### 存档重置

入口：

```text
Tools > Compound Box > Reset Local Save
```

### Windows 构建

入口：

```text
Tools > Compound Box > Build Windows64
```

同时支持命令行：

```text
Tools/Build-Windows.ps1
Tools/Run-EditModeTests.ps1
```

## 17. 关卡审查

`LevelAudit` 统计：

- 章节
- 地图尺寸
- Matter 实体数量
- 传送节点数量
- 目标数量
- 标准解动作数
- 移动次数
- 推动次数
- 是否使用传送
- 是否使用拆分
- 是否使用重组
- 是否使用精确切割
- 是否使用旋转
- 是否为混合材质关卡
- 主机制
- 是否可解

这些数据用于：

- 难度曲线分析
- 关卡分类
- 自动验证
- 编辑器工作台

## 18. 自动化测试

当前测试覆盖：

- 12 关标准解
- 移动阻挡
- 拆分与重组
- 传送
- 多材质移动
- 通用目标
- 精确切割
- 旋转
- 旋转占用失败
- 撤销与重做
- 存档 JSON
- 对象池
- FSM
- 关卡 Catalog 转换
- 12 关审查

构建前和提交前均应运行：

```text
Tools/Run-EditModeTests.ps1
```

## 19. 构建流程

```text
Unity 编译
  -> EditMode 测试
  -> BuildPlayer
  -> Builds/Windows/CompoundBox.exe
```

构建配置：

- Windows 64 位
- Clean Build
- 单场景 SampleScene
- 0 error
- 0 warning

## 20. 当前工程优点

- 规则层和表现层分离
- 数据驱动关卡
- Command 模式撤销/重做
- 可回放标准解
- 动态传送节点
- 对象池复用
- 自动关卡审查
- JSON 存档
- 无外部素材依赖
- 可命令行测试和构建

## 21. 当前限制

- 编辑器还不是网格关卡编辑器
- 没有正式美术资源
- 没有动画状态机
- 没有压力板和闸门玩法
- 没有中途状态存档
- 存档没有原子写入
- 没有 Input System 支持
- 没有求解器
- 没有关卡可视化依赖图

## 22. 推荐后续升级顺序

1. 自定义 LevelDefinitionAsset Inspector。
2. Scene View 网格画笔。
3. 连接边可视化编辑。
4. 原子存档和备份。
5. 中途状态序列化。
6. 关卡求解器。
7. 正式 Sprite/Prefab 美术替换。
8. Input System 适配。
