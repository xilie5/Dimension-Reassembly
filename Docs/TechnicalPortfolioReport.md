# Compound Box 完整技术分析与求职说明

## 0. 文档定位

本文档是 Compound Box 当前版本的权威技术说明，用于：

- 理解项目完整架构和实现原理
- 复盘每个系统为什么这样设计
- 从简历角度提炼项目亮点
- 准备技术面试中的追问
- 区分已完成功能、工程取舍和后续技术债

项目类型：Unity 2D 网格解谜作品集 Demo。

项目核心：玩家操作可拆分、可传送、可重组的复合实体，使其满足目标格子的形状和材质要求。

当前规模：

- 12 个内置关卡
- 4 个章节
- 35 个 EditMode 自动化测试
- Windows 64 位可执行构建
- Unity 2022.3.62f3c1
- New Input System 1.19.0

## 1. 项目一句话介绍

Compound Box 是一款围绕“多格复合实体、双向网格传送门、拆分与重组”设计的 2D 解谜 Demo。项目使用逻辑网格驱动的确定性模拟，将玩法规则、输入、表现、关卡数据和存档推进拆分为独立层次，并通过 Command、状态快照、标准解回放和自动化测试保证规则稳定。

## 2. 核心玩法

### 2.1 基础移动

玩家和目标实体都严格对齐逻辑网格。

一次移动不是简单修改玩家坐标，而是完整处理移动链：

1. 玩家产生目标格。
2. 目标格被实体占用时，该实体加入移动集合。
3. 被推实体继续产生目标格，可能推动下一个实体。
4. 所有候选位置先收集，不立即写入状态。
5. 统一检查墙、越界、实体重叠和传送节点限制。
6. 只有整条链都合法时才提交。

这种规则保证了推箱子类玩法需要的确定性和原子性。

### 2.2 复合实体

一个 Matter 实体不是单格对象，而是由多个格子组成：

```text
GridEntity
  Id
  Kind
  CellStates[]
  Connections[]
```

每个格子的数据：

```text
Position
MatterType
```

因此一个实体可以同时包含：

```text
(3, 4) Cyan
(4, 4) Amber
(3, 5) Rose
```

连接边记录实体内部结构：

```text
(3, 4) <-> (4, 4)
(3, 4) <-> (3, 5)
```

移动时，整个实体的所有格子和连接边一起平移，因此不规则形状不会变形。

### 2.3 拆分

全拆分 `X`：

- 每个格子变成独立单格 Matter。
- 每个单格保留自己的材质。
- 原有内部连接全部断开。

精确切割 `V`：

- 找到玩家面前的实体和朝向内部的连接边。
- 删除被切割的那一条连接。
- 根据剩余连接计算连通分量。
- 每个连通分量变成独立实体。
- 分量内部连接保留。

两者差异：

```text
全拆分：所有连接切断
精确切割：只切断指定连接
```

### 2.4 重组

重组 `C` 使用 BFS 找到可合并组。

当前条件：

- 双方都是 Matter。
- 每一方都是单材质实体。
- 材质相同。
- 实体至少存在一对正交相邻格子。

符合条件后合并：

- 汇总所有格子。
- 汇总合法连接。
- 根据相邻规则补充新连接。
- 用一个新的稳定实体 ID 表示合并结果。

### 2.5 旋转

旋转以实体 Anchor 为原点：

```text
顺时针：  (x, y) -> (y, -x)
逆时针：  (x, y) -> (-y, x)
```

旋转同时应用到：

- 所有格子坐标
- 所有连接边端点

旋转前检查：

- 是否越界
- 是否进入墙
- 是否与其他实体重叠

全部合法才提交。

## 3. 传送门设计

传送门是本项目最重要的技术系统之一。

### 3.1 数据模型

静态端点和动态端点分开描述。

```text
PortalPair
  Id
  FixedEntry
  FixedExit
  EntryEntityId
  ExitEntityId
```

固定传送门使用坐标。

可移动传送节点使用实体 ID，运行时解析其当前 Anchor。

### 3.2 双向传送

`PortalLink` 负责表示“从当前端点进入后，另一端是谁”：

```text
Entry -> Exit
Exit  -> Entry
```

因此传送门不是单向入口。

### 3.3 整体传送

传送不会逐格移动复合体，而是让整个实体保持形状进行一次平移。

处理步骤：

1. 实体先计算普通移动候选格。
2. 检查候选格是否命中任一 PortalLink。
3. 根据运动方向选择最先进入的触发格。
4. 找到目标端点。
5. 保持实体形状和连接关系。
6. 将所有格子平移。
7. 让整个实体完全越过出口格，不在出口格上停留。
8. 再进行占用、墙和越界检查。

这样处理解决：

- 复合体传送后变形
- 传送后覆盖传送门格
- 出口位于入口后方时位移判断错误
- 不规则复合体角落覆盖传送门

### 3.4 不规则复合体

不规则复合体使用完整边界计算出口位移。

以向右传送为例：

```text
新最小 X = 出口 X + 1
```

以向左传送为例：

```text
新最大 X = 出口 X - 1
```

向上和向下同理。

实体穿过出口后，所有格子都位于出口外侧。

如果空间不足，传送失败。

### 3.5 可移动传送节点

可移动节点本身是实体：

```text
EntityKind.PortalNode
```

规则：

- 玩家进入可移动节点时推动它。
- Matter 或复合体进入节点时执行传送。
- 可移动节点不能推进另一个传送门格。
- 两个可移动节点不能重叠。

视觉上，可移动节点包含四个金色方向箭头，用来和固定传送门区分。

## 4. 架构总览

项目采用定制分层架构：

```text
Editor / Tests
      |
      v
Application / Runtime
      |
      +-------------------+
      |                   |
      v                   v
Domain / Core       Presentation
```

完整调用链：

```text
LevelDefinitionAsset / LevelDefinition
      |
      v
LevelParser
      |
      v
GridBoardState
      |
      v
GridSession
      |
      v
IGridCommand
      |
      v
GridBoardSimulation
      |
      v
ActionResolution + Updated State
      |
      +------------------+
      |                  |
      v                  v
BoardView             GameHud
```

## 5. Core 层

### GridBoardState

职责：

- 保存棋盘宽高
- 保存 TileKind 二维数组
- 保存实体、目标、传送门
- 保存移动、推动、动作计数
- 判断是否通关
- 克隆和恢复状态
- 验证状态不变量

不负责：

- MonoBehaviour
- 输入
- SpriteRenderer
- UI
- 音效

### GridEntity

统一描述玩家、Matter 和传送节点。

关键能力：

- 查询某格是否属于实体
- 获取某格材质
- 计算稳定 Anchor
- 平移全部格子
- 替换形状
- 增删和验证连接

### GridBoardSimulation

规则中心。

负责：

- 移动
- 推挤链
- 传送解析
- 形状平移
- 拆分
- 精确切割
- 重组
- 旋转
- 移动预览

规则层完全不依赖场景和表现层。

### GridSession

应用层和规则层之间的会话边界。

负责：

- 创建具体 Command
- 执行 Command
- Undo Stack
- Redo Stack
- Restart
- 标准解回放
- 开发环境状态不变量检查

### LevelParser

负责把字符地图转换成 GridBoardState。

支持：

- 地形和墙体
- 玩家出生点
- 材质块
- 普通目标
- 复合目标
- 固定传送门
- 可移动传送节点
- 出口
- 材质覆盖层

## 6. Command 与撤销重做

所有玩法操作通过 Command 进入：

```text
MoveGridCommand
SplitGridCommand
PrecisionCutGridCommand
RecombineGridCommand
RotateGridCommand
```

每个 Command 保存：

- 执行前 Memento
- 执行后 Memento
- ActionType
- HistoryLabel

设计理由：

复杂推挤、传送、切割和重组很难为每种操作手写可靠逆过程。

当前方案：

```text
Command 负责语义和历史
Memento 负责精确恢复状态
```

优点：

- Undo/Redo 正确性高
- 新机制不需要立即实现逆运算
- 标准解和回放共享同一条命令链
- 历史标签可用于 UI 和调试

代价：

- 每一步保存完整状态快照
- 大棋盘和长历史会增加内存
- 它不是严格意义上的纯反向 Command

## 7. 状态机

当前 `StateMachine<TState>` 是一个轻量状态迁移器。

它负责：

- 保存 CurrentState
- 拒绝重复切换
- 触发 StateChanged 事件

当前使用：

```text
LevelFlowState
  Loading
  Playing
  Completed

PlayerActionState
  Idle
  Moving
  Interacting
```

清理后，未使用的机关状态和空行为钩子已经删除。

状态机用于流程和输入门禁，不负责规则模拟。

## 8. 表现层

### BoardView

负责：

- 根据 TileKind 创建静态棋盘
- 绘制地板、墙、目标、出口、固定传送门
- 根据实体状态创建和更新动态实体
- 显示移动预览
- 管理实体和预览对象池
- 根据 HUD 安全区调整相机

### EntityCellView

负责渲染动态实体：

- 阴影
- 外壳
- 内芯
- 复合体连接
- 玩家方向
- 可移动传送节点的四个方向标识

### ArtThemeAsset

ArtThemeAsset 是 ScriptableObject 美术主题：

- 背景和地板颜色
- 墙体和 UI 颜色
- 四种材质颜色
- 地板、墙体、玩家、方块、目标、出口和传送门 Sprite
- UI 按钮纹理
- Kenney CC0 UI 音效

表现选择优先级：

```text
ArtThemeAsset 中的正式 Sprite
      |
      v
WhiteboxSprites 程序化备用图形
```

### 对象池

BoardView 使用泛型 ObjectPool 复用：

- 实体根节点
- EntityCellView
- 预览 SpriteRenderer

对象池减少拆分、重组和连续移动预览产生的 GC。

## 9. 输入系统

项目已经迁移到 New Input System。

输入不会散落在游戏规则中。

`CompoundBoxInput` 统一创建 InputActionMap：

- Move Up
- Move Right
- Move Down
- Move Left
- Confirm
- Advance
- Level Select
- Previous Level
- Next Level
- Restart
- Undo
- Redo
- Split
- Precision Cut
- Recombine
- Rotate Left
- Rotate Right
- Mute

键盘和手柄绑定同时存在。

应用层只读取语义：

```csharp
if (input.SplitPressed)
{
    ApplyResolution(session.SplitFacingEntity());
}
```

规则层不知道玩家按的是 X、手柄按钮还是未来的触屏按钮。

## 10. 数据驱动关卡

关卡链：

```text
LevelDefinitionAsset
      |
      v
LevelCatalogAsset
      |
      v
LevelDefinition
      |
      v
LevelParser
      |
      v
GridBoardState
```

为什么保留两层：

- ScriptableObject 适合 Inspector 编辑和资源引用。
- LevelDefinition 是轻量 POCO，方便解析、克隆和测试。
- 运行时逻辑不直接依赖 Unity 资产对象。

如果 Catalog 缺失，会回退到 BuiltInLevels，保证项目仍能启动。

## 11. 存档系统

存档使用 JsonUtility：

```text
Application.persistentDataPath/compound-box-save.json
```

保存：

- 最高解锁关卡
- 关卡完成状态
- 最佳移动数
- 最佳推动数
- 静音状态

当前不保存：

- 中途棋盘状态
- 当前 Command 历史
- 当前预览

因此退出后重新进入会从关卡初始状态开始。

## 12. 编辑器工具

### Level Workbench

入口：

```text
Tools > Compound Box > Level Workbench
```

支持：

- 浏览全部关卡
- 显示尺寸、实体、目标和解法统计
- 验证关卡
- 回放标准解
- 直接进入指定关卡

### Catalog Builder

入口：

```text
Tools > Compound Box > Create or Refresh Default Catalog
```

从 BuiltInLevels 生成或刷新关卡资产。

### Windows Build

入口：

```text
Tools > Compound Box > Build Windows64
```

命令行：

```text
Tools/Build-Windows.ps1
Tools/Run-EditModeTests.ps1
```

## 13. 测试与质量

自动化测试分为：

```text
ArchitectureTests
GridSessionTests
TransformMechanicsTests
```

覆盖：

- 12 关标准解
- 移动阻挡
- 推挤链
- 双向传送
- 上下传送
- 出口位于入口后方
- 不规则复合体水平传送
- 不规则复合体上下传送
- 可移动传送节点
- 可移动节点冲突
- 拆分
- 精确切割
- 重组
- 旋转
- Undo/Redo
- JSON 存档
- 对象池
- FSM 状态迁移
- Input System 初始化
- 可移动传送节点标识
- 关卡 Catalog
- 全关卡可解性

当前结果：

```text
35/35 passed
```

## 14. 构建与性能

当前构建：

```text
Windows x64
Builds/Windows/CompoundBox.exe
```

最近构建大小：约 75.13 MB。

性能措施：

- 逻辑二维数组，不依赖物理系统
- 移动过程无 Unity 物理查询
- 对象池减少频繁创建销毁
- 数据与表现分离，减少不必要的状态扫描
- 应用目标帧率 120 FPS

## 15. 架构优点

### 规则测试不依赖场景

Core 测试可以直接解析关卡、执行移动和检查状态，无需进入 Play Mode。

### 状态修正原子化

所有移动提案先验证再提交，避免半成功状态。

### 关卡与表现解耦

关卡定义不包含 Sprite，表现层根据 TileKind、EntityKind 和 MatterType 选择视觉。

### 编辑器与运行时共用规则

Level Workbench、测试和游戏都使用相同的 Parser、Simulation 和 Session。

### 传送门规则可扩展

PortalPair 负责端点关系，PortalLink 负责方向解析，可移动节点使用实体 ID 动态解析。

## 16. 当前技术债

### 16.1 Core 和 Runtime 仍在同一程序集

逻辑分层已经存在，但物理程序集没有拆开。

风险：

- Core 哪天可能直接引用表现层。
- 编译器不能强制依赖方向。

建议：

```text
CompoundBox.Core
CompoundBox.Presentation
CompoundBox.Editor
CompoundBox.Tests
```

### 16.2 CompoundBoxApp 职责偏多

它同时是：

- Composition Root
- 输入消费
- 关卡流程控制
- UI 回调
- 存档协调

建议拆分：

```text
GameBootstrap
GameplayController
LevelFlowController
HudCoordinator
```

### 16.3 GridPalette 存在全局主题状态

`GridPalette.Theme` 是静态状态。

对于单棋盘 Demo 足够，但不利于：

- 多棋盘场景
- 编辑器预览
- 多主题切换
- 测试隔离

建议把主题完全注入 View。

### 16.4 Undo/Redo 使用全量快照

优点是正确性高。

缺点是内存随历史步数和实体数量增加。

后续可改成：

- 反向 Command
- 结构化 Delta
- 分块快照
- 历史压缩

### 16.5 状态集合仍暴露可变 List

外部代码可以直接修改：

```text
Entities
Goals
PortalPairs
```

建议改为只读集合，内部提供受控修改 API。

### 16.6 关卡来源有双重入口

当前同时存在 Catalog 和 BuiltInLevels。

风险：

- 两边内容不同步
- 测试和运行时可能使用不同来源
- 修改关卡后标准解可能失配

建议最终只保留一个正式来源。

### 16.7 存档不是完整恢复系统

没有中途暂停恢复。

也没有：

- 原子写入
- 备份
- 校验
- 完整版本迁移

### 16.8 HUD 使用 OnGUI

适合快速 Demo，不适合正式产品。

后续建议迁移：

- UI Toolkit
- uGUI
- 独立 UI Controller

### 16.9 Scene 是运行时生成

优点是关卡数据驱动，Scene 干净。

缺点是：

- 美术无法直接在 Scene 调整最终对象
- Prefab 和动画复用较弱

## 17. 为什么这些取舍合理

### 为什么使用逻辑网格而不是 Tilemap Collider

玩法需要：

- 确定性
- 可回放
- 精确状态克隆
- 无物理抖动
- 可以在 EditMode 直接测试

物理碰撞不适合作为核心规则来源。

### 为什么使用 ScriptableObject 和 POCO 两层

ScriptableObject 适合编辑，POCO 适合模拟和测试。

两者分开避免 Core 依赖 AssetDatabase。

### 为什么 Command 搭配 Memento

复合实体、传送和切割的逆操作复杂。

Memento 优先保证正确性，Command 保留操作语义。

### 为什么表现层不参与判定

判定只依赖 GridBoardState。

这样更换白盒、AI 素材或未来正式美术时，不需要改玩法。

## 18. 简历写法

### 中文精简版

独立开发 Unity 2D 网格解谜作品集项目，围绕多格复合实体、双向传送门、拆分、精确切割和重组设计 12 个递进关卡。采用逻辑网格与表现层分离架构，实现原子推挤链、复杂实体形状保持传送、Command/Memento 撤销重做、ScriptableObject 关卡配置、New Input System、JSON 存档、对象池和 35 个自动化测试，并使用同一套生产规则验证全部关卡标准解。

### 中文技术强化版

独立完成 Unity 2022.3 2D 网格解谜 Demo 的架构设计、玩法实现、编辑器工具、美术接入、测试和 Windows 构建。核心为自定义 GridBoardState + GridSession + Command 模拟架构，支持逐格材质、复合实体连接图、不规则形状整体传送、动态传送节点、拆分/切割/重组/旋转和确定性多实体推挤。通过状态提案与统一验证实现原子移动，通过 PortalLink 实现双向传送，通过 Command 语义与 Memento 快照实现稳定 Undo/Redo，并建立 35 个 EditMode 测试保证 12 关标准解可持续回归。

### English

Developed a Unity 2D grid-puzzle portfolio project featuring compound polyomino entities, bidirectional portals, splitting, precision cutting, recombination, rotation, and 12 progression levels. Built a deterministic simulation architecture with logical grid state, shape-preserving teleportation, transactional push resolution, command-based undo/redo, ScriptableObject content, New Input System support, JSON progression, object pooling, editor validation tools, and 35 automated EditMode tests.

## 19. 面试重点问题

### 为什么不是直接把位置写在 Transform 上

Transform 是表现状态，不适合做确定性规则、撤销和测试。

逻辑事实保存在 GridBoardState，Transform 只负责显示。

### 为什么移动要先提案再提交

推挤链可能包含多个实体，并可能经过传送门。

如果边检查边修改，一个后续失败会导致棋盘进入半完成状态。

### 为什么传送不能逐格处理

逐格处理会：

- 拆散复合体
- 让材质和结构错位
- 产生处理顺序依赖
- 让不规则形状结果不确定

整体平移可以保证形状、材质和连接完整。

### 为什么可移动传送节点仍然需要特殊规则

它既是实体，又是传送系统端点。

如果不区分玩家推动和 Matter 传送，会出现：

- box 抵达出口时把节点推走
- 节点覆盖出口格
- 玩家无法移动节点
- 传送配对位置与实体位置不同步

### 为什么测试不进入 Play Mode

绝大多数玩法不依赖帧更新或场景对象。

EditMode 测试启动快，可以高频验证规则回归。

## 20. 五分钟演示顺序

1. `First Push`：移动、推箱子、目标和出口。
2. `Split Decision`：拆分和独立配送。
3. `Anchor Transit`：双向传送与复合体通过传送门。
4. `Reassembly`：同材质实体重组。
5. `Precision Cut`：精确切割和连接图。
6. `Rotation Vault`：旋转和复合目标。
7. `Portable Gate`：可移动传送节点。
8. `Level Workbench`：关卡验证和标准解回放。
9. Test Runner：展示 35 个测试全部通过。
10. Windows Build：展示独立运行版本。

## 21. 后续路线

优先级从高到低：

1. 拆分 Core、Presentation、Editor 程序集。
2. 将 CompoundBoxApp 拆成输入、关卡流程和 UI Controller。
3. 把 GridPalette 全局主题改为依赖注入。
4. 将 InputActionMap 迁移为可重绑定的 InputActionAsset。
5. 制作可视化网格关卡编辑器。
6. 统一关卡来源，移除正式内容对 BuiltInLevels 的依赖。
7. 增加中途存档、原子写入和版本迁移。
8. 增加 BFS/IDA* 求解器和最短解验证。
9. 将 OnGUI 替换为 UI Toolkit。
10. 添加正式动画、粒子、镜头反馈和音频混音。

## 22. 结论

Compound Box 的技术价值不只在于实现了推箱子，而在于围绕复杂网格实体建立了一套完整、可测试、可扩展的规则系统。

核心技术亮点：

- 逻辑网格与表现层分离
- 多格实体连接图
- 双向传送门
- 不规则复合体形状保持
- 动态传送节点
- 事务式推挤链
- Command + Memento 历史
- ScriptableObject 数据管线
- New Input System
- 对象池
- 编辑器验证
- 标准解自动回归

项目仍存在应用层过大、程序集未拆分、存档不够工业和 HUD 使用 OnGUI 等缺陷，但这些缺陷已经明确，并且有可执行的演进路线。

从求职角度，最适合重点讲：

1. 如何设计可撤销的复杂网格模拟。
2. 如何处理不规则复合体的传送。
3. 如何做到逻辑状态和表现层解耦。
4. 如何用标准解和自动化测试保证关卡回归。
5. 如何识别当前架构缺陷并提出拆分方案。
