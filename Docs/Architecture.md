# Compound Box - 项目架构说明

## 1. 架构结论

项目采用的是定制化的分层架构，可以概括为：

> Domain Simulation + Command History + Presentation Adapter + Editor Tooling

它不是 ECS、MVC、MVVM、UniRx 事件驱动或依赖注入框架。

项目没有引入通用游戏框架，而是在 Unity 内建立了一套针对网格解谜的定制结构：

- 规则和数据集中在 Core
- Unity 生命周期集中在 Runtime
- 输入通过 Command 进入规则层
- 表现层只读取状态
- 编辑器工具复用生产解析器和模拟器
- 测试直接运行 Core，不启动场景

## 2. 架构分层

```text
┌──────────────────────────────────────────┐
│ Editor / Tests                           │
│ Level Workbench, Build Tools, EditMode   │
└───────────────────┬──────────────────────┘
                    │
┌───────────────────▼──────────────────────┐
│ Application / Runtime                    │
│ CompoundBoxApp, Input, Game Flow, HUD    │
└───────────────┬───────────────┬──────────┘
                │               │
┌───────────────▼───────┐ ┌─────▼──────────┐
│ Domain / Core         │ │ Presentation   │
│ GridSession           │ │ BoardView      │
│ GridBoardState        │ │ EntityCellView │
│ Commands              │ │ WhiteboxSprites│
│ LevelParser           │ │ ProceduralAudio│
│ Simulation            │ │ GameHud        │
│ Save/Audit/Validation │ │ ObjectPool     │
└───────────────────────┘ └────────────────┘
```

## 3. 依赖方向

逻辑依赖方向：

```text
Presentation -> Core State
Application -> Core + Presentation
Editor -> Core
Tests -> Core
```

规则层不能依赖：

- MonoBehaviour
- Scene
- SpriteRenderer
- Canvas
- Camera
- Input

规则层可以依赖：

- `Vector2Int`
- `Mathf`
- `JsonUtility`
- Unity 基础值类型

因此 Core 是“轻量 Unity 耦合的领域层”，不是完全脱离 Unity 的纯 C# 程序集。

## 4. 核心调用链

### 玩家移动

```text
Input
  -> CompoundBoxApp
  -> GridSession.Move()
  -> MoveGridCommand.Execute()
  -> GridBoardSimulation.Move()
  -> GridBoardState
  -> BoardView.SetState()
  -> GameHud
```

### 撤销

```text
Input Z
  -> CompoundBoxApp
  -> GridSession.Undo()
  -> IGridCommand.Undo()
  -> Restore before state
  -> BoardView.Rebuild()
```

### 完成关卡

```text
Command 提交
  -> GridBoardState.IsSolved()
  -> CompoundBoxApp
  -> SaveService.RecordCompletion()
  -> JSON
  -> HUD 更新
```

## 5. Core 层职责

### GridBoardState

负责：

- 保存棋盘状态
- 保存实体
- 保存目标
- 保存传送门
- 保存计数器
- 判断是否通关
- 提供状态克隆和恢复
- 验证状态不变量

不负责：

- 输入
- 动画
- 音效
- UI

### GridEntity

负责描述一个对象：

```text
Id
Kind
EntityCellState[]
EntityConnection[]
```

它是复合体、玩家和传送节点的统一数据表示。

### GridBoardSimulation

负责所有纯规则：

- 移动
- 推挤
- 传送
- 全拆分
- 精确切割
- 重组
- 旋转
- 预览

可以直接在 EditMode 测试中调用，不需要进入 Play Mode。

### GridSession

应用层和规则层之间的会话边界。

负责：

- 创建 Command
- 执行 Command
- undo stack
- redo stack
- 重开
- 标准解回放
- 状态不变量校验

### LevelParser

负责把静态关卡定义变成运行时棋盘。

它是唯一负责解释：

- 文本布局
- 材质层
- 目标
- 传送门
- 初始实体
- 连接关系

的模块。

## 6. Command 架构

这是项目最重要的行为架构。

```text
IGridCommand
  ├── MoveGridCommand
  ├── SplitGridCommand
  ├── PrecisionCutGridCommand
  ├── RecombineGridCommand
  └── RotateGridCommand
```

每个 Command 保存：

- before state
- after state
- action type
- history label

### 为什么使用 Command

因为需要：

- 撤销
- 重做
- 标准解回放
- 历史标签
- 未来录像和同步

### 为什么不是纯反向操作

复杂推挤、传送、切割和重组很难可靠推导逆操作。

因此使用：

```text
Command 语义 + Memento 状态恢复
```

即：

- Command 描述发生了什么
- before/after state 保证状态恢复准确

## 7. Presentation 架构

表现层是 Adapter，而不是规则层的一部分。

### BoardView

负责：

- 创建静态棋盘
- 显示实体
- 播放位置变化
- 显示移动和传送预览
- 管理实体和预览对象池
- 根据 HUD 安全区调整摄像机

### EntityCellView

负责将一个逻辑格子渲染成：

- 阴影
- 外壳
- 内芯
- 连接标记
- 玩家方向
- 传送节点样式

### WhiteboxSprites

负责运行时生成 Sprite：

- Square
- RoundedSquare
- Circle
- Ring
- Diamond
- Chevron

### GameHud

负责：

- 关卡信息
- 目标进度
- 移动统计
- 存档进度
- 章节选择
- 完成面板
- 操作提示

## 8. Application 架构

`CompoundBoxApp` 是 Composition Root。

它负责组装：

```text
Level Catalog
Save Service
Camera
BoardView
GameHud
ProceduralAudio
GridSession
```

同时负责：

- 输入映射
- 关卡流程
- 完成判定后的存档
- 章节选择回调
- 玩家和关卡状态机

当前 `CompoundBoxApp` 是应用层最大的类。项目规模继续扩大时，应拆分为：

```text
GameBootstrap
InputController
LevelFlowController
SaveCoordinator
```

## 9. 状态机架构

通用状态机：

```text
StateMachine<TState>
IStateBehaviour<TState>
```

当前状态：

```text
LevelFlowState
  Loading
  Playing
  Completed

PlayerActionState
  Idle
  Moving
  Interacting

MechanismState
  Closed
  Open
  Locked
```

状态机用于控制流程和生命周期，而不是替代规则模拟。

## 10. 数据驱动架构

关卡来源：

```text
LevelDefinitionAsset
  -> LevelCatalogAsset
  -> LevelDefinition
  -> LevelParser
  -> GridBoardState
```

为什么关卡使用 ScriptableObject：

- Inspector 可视化
- 资源引用稳定
- 方便批量 Catalog
- 运行时可转换为纯数据对象

为什么运行时不直接使用 ScriptableObject：

- 便于 Clone
- 便于测试
- 避免场景和资产引用进入规则层

## 11. 存档架构

```text
SaveService
  -> SaveData
  -> JsonUtility
  -> persistentDataPath/compound-box-save.json
```

SaveService 负责：

- 加载
- 默认数据
- 反序列化
- 保存
- 记录通关
- 更新最佳成绩
- 静音设置

当前不保存中途棋盘状态。

## 12. 编辑器架构

编辑器工具直接复用 Core：

```text
LevelWorkbenchWindow
  -> LevelValidation
  -> LevelAudit
  -> LevelParser
  -> GridSession.ReplaySolution()
```

这保证编辑器验证和运行时使用完全相同的规则。

当前有：

- 关卡阅读
- 指标统计
- 标准解回放
- 一键进入关卡
- Catalog 重建
- 存档重置

当前没有：

- 网格画笔
- 连接边编辑
- 可视化求解器
- Scene View 编辑

## 13. 测试架构

测试分为：

```text
ArchitectureTests
GridSessionTests
TransformMechanicsTests
```

测试直接调用 Core，不依赖场景。

覆盖：

- 命令历史
- 存档
- FSM
- Catalog
- 对象池
- 多材质
- 移动
- 传送
- 拆分
- 精确切割
- 重组
- 旋转
- 不变量
- 12 关标准解

## 14. 当前架构优点

- 规则和 Unity 生命周期分离
- Command 行为边界明确
- Memento 保证 Undo/Redo 准确
- 关卡数据和运行状态分离
- 表现层不参与判定
- 编辑器与运行时共用规则
- 测试不依赖场景
- 可以扩展求解器和回放系统

## 15. 当前架构问题

### 问题一：Core 和 Runtime 没有独立 asmdef

目前逻辑上是 Core / Runtime 分层，但物理程序集仍是同一个 `CompoundBox.Runtime`。

后果：

- 编译器不能阻止 Runtime 被 Core 引用
- 新开发者容易跨层调用
- 依赖边界主要依赖规范，不是编译约束

建议：

```text
CompoundBox.Core.asmdef
CompoundBox.Runtime.asmdef
CompoundBox.Editor.asmdef
CompoundBox.Tests.asmdef
```

### 问题二：CompoundBoxApp 职责偏多

它同时承担：

- Composition Root
- 输入控制
- 关卡流程
- 存档协调
- UI 回调

建议拆为多个控制器。

### 问题三：HUD 使用 OnGUI

优点是零依赖、快速实现。

缺点是：

- 不适合复杂动画
- 不适合复杂布局系统
- 不利于正式 UI 美术

正式产品建议迁移到 UI Toolkit 或 Canvas/UGUI。

### 问题四：SaveService 直接使用文件 API

缺少：

- 原子写入
- 备份
- 校验
- 完整迁移链

## 16. 推荐目标架构

```text
CompoundBox.Core
  State
  Entities
  Simulation
  Commands
  Parsing
  Validation
  Persistence Models

CompoundBox.Runtime
  Bootstrap
  Input
  Board Presentation
  HUD
  Audio
  Save Adapter

CompoundBox.Editor
  Level Editor
  Audit
  Solver
  Build Tools

CompoundBox.Tests
  Core Tests
  Content Tests
  Save Tests
```

这一步不需要重写游戏，只需要把现有类按依赖方向移动并补 asmdef。

## 17. 结论

项目有定制架构，而且架构核心已经明确。

它不是“没有架构”，而是：

> 一个针对网格解谜的轻量分层架构，以确定性状态模拟、Command/Memento 历史和表现层适配为核心。

当前最需要补强的不是玩法架构，而是物理程序集边界、应用层拆分、可视化关卡编辑器和正式存档可靠性。
