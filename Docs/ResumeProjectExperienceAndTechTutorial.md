# Compound Box 简历项目经历与零基础技术教学

## 1. 本文档怎么用

本文档分三部分：

1. 可以直接放进简历的项目经历文案。
2. 每个技术点用简单语言解释，帮助你学习和复述。
3. 面试常见问题和回答模板。

目标是让你做到：

- 简历上写到的内容能够解释。
- 不夸大没有实现的功能。
- 面试时能从“项目做了什么”讲到“为什么这样做”。
- 遇到不会的问题时，可以诚实说明边界，而不是硬编。

## 2. 简历项目经历

### 2.1 中文版

从 0 到 1 独立完成的 Unity 2D 网格解谜作品集 Demo，玩家通过拆分、传送、重组多格物体改变关卡结构并完成目标。项目包含 12 个递进关卡、4 个章节、主界面、关卡选择、存档推进和 Windows 独立构建。

- **核心玩法与数据建模**：使用逻辑二维网格管理地图和实体位置，将物体建模为“实体-格子-连接关系”结构，支持多格复合体、逐格材质、拆分、精确切割、旋转和相邻同材质重组。
- **移动与传送系统**：实现确定性的多实体推挤链，先计算所有候选位置，再统一检查墙、边界和占用，最后一次性提交；传送门支持双向传送、不规则复合体形状保持，以及可移动传送节点。
- **架构与状态管理**：采用逻辑层与表现层分离架构，使用 Command 封装移动、拆分、重组等操作，并结合状态快照实现撤销、重做和标准解回放；使用 ScriptableObject 管理关卡与美术主题，使用 JSON 保存解锁进度和最佳成绩。
- **工程化与验证**：接入 New Input System 统一键盘和手柄输入，使用对象池减少频繁创建销毁，编写 35 个 EditMode 自动化测试验证 12 个内置关卡的标准解、传送、切割、旋转、撤销重做和存档流程。

技术栈：Unity 2022.3 / C# / 2D Grid / ScriptableObject / Command Pattern / FSM / New Input System / JSON / ObjectPool / Unity Test Framework

### 2.2 英文版

Independently developed a Unity 2D grid-puzzle portfolio project where players split, teleport, and recombine multi-cell objects to change the level structure and solve puzzles. The demo includes 12 progression levels, 4 chapters, a main menu, level selection, save progression, and a standalone Windows build.

- **Gameplay and data modeling**: Built a logical 2D grid and modeled objects as entities with per-cell materials and connection graphs, supporting compound bodies, splitting, precision cutting, rotation, and same-material recombination.
- **Movement and portal systems**: Implemented deterministic multi-entity push chains and transactional state validation. Portals support bidirectional travel, shape-preserving teleportation for irregular compounds, and movable portal nodes.
- **Architecture and state management**: Separated simulation from presentation and used commands with state snapshots for undo/redo and solution replay. Used ScriptableObjects for level and art configuration, plus JSON for unlock progression and best scores.
- **Engineering and validation**: Integrated the New Input System for keyboard and gamepad input, used object pooling to reduce allocation spikes, and added 35 EditMode tests covering all level solutions, portals, cutting, rotation, undo/redo, and save flow.

Tech stack: Unity 2022.3 / C# / 2D Grid / ScriptableObject / Command Pattern / FSM / New Input System / JSON / ObjectPool / Unity Test Framework

## 3. 适合口语介绍的一分钟版本

我独立完成了一个 Unity 2D 网格解谜作品集 Demo，叫 Compound Box。它不是普通推箱子，而是把物体设计成可以拆分和重组的多个格子，玩家还要利用双向传送门和可移动传送节点完成关卡。

技术上，我主要做了三件事：第一，使用逻辑二维网格和连接图描述复合实体，保证不规则物体移动和传送后不会变形；第二，使用 Command 和状态快照实现撤销重做，并用同一套规则回放标准解；第三，把输入、规则、表现和关卡数据分层，加入了 New Input System、对象池和 35 个自动化测试。

这个项目目前有 12 关、4 个章节和 Windows 构建。下一步我会继续拆分程序集、开发可视化关卡编辑器和求解器。

## 4. 技术名词教学

这一部分只解释简历里出现的技术。每个技术点都包含：

```text
它是什么
项目里怎么用
为什么需要
面试怎么回答
```

## 5. Unity 和 C#

### 是什么

Unity 是游戏引擎，C# 是编写游戏逻辑的编程语言。

### 项目里怎么用

- Unity 负责场景、Sprite 显示、输入、音效和构建。
- C# 负责关卡解析、移动规则、状态管理、存档和测试。

### 为什么需要

Unity 提供现成的显示和构建工具，项目可以把主要精力放在玩法规则上。

### 面试怎么回答

“我使用 Unity 2022.3 和 C# 开发 2D Demo，但核心玩法不依赖 Unity 物理系统，而是使用逻辑网格完成确定性模拟，方便测试和撤销。”

## 6. 逻辑二维网格

### 是什么

二维网格可以理解成棋盘：

```text
每个格子有坐标 (x, y)
玩家、方块、目标、墙都占有一个或多个格子
```

### 项目里怎么用

地图使用 `TileKind[,]` 保存：

```text
Void
Floor
Wall
Portal
Goal
Exit
```

### 为什么不用 Tilemap Collider 直接做规则

因为游戏需要：

- 每一步结果确定
- 能撤销和重做
- 能快速复制状态
- 能在不进入 Play Mode 时测试
- 不依赖物理碰撞和帧率

### 面试怎么回答

“我把规则和表现分开。规则使用二维数组和格坐标，Tilemap 或 SpriteRenderer 只负责显示。这样移动、传送和撤销都更稳定，也方便自动化测试。”

## 7. 复合实体

### 是什么

普通方块只占一个格子。

复合实体由多个格子组成，例如：

```text
██
 █
```

它可能是不规则形状，每个格子还可以有不同材质。

### 项目里怎么用

一个实体保存：

```text
实体 ID
实体类型
每个格子的位置和材质
格子和格子之间的连接
```

### 为什么需要

如果只记录一个矩形范围，无法准确表示 L 形、凹形或不同材质的复合体。

### 面试怎么回答

“我把复合体拆成 Entity、CellState 和 Connection 三层。Entity 表示整体，CellState 表示每个格子的位置和材质，Connection 表示内部连接，因此不规则形状和精确切割都能处理。”

## 8. 确定性移动和推挤链

### 是什么

玩家推动一个方块时，这个方块可能继续推动另一个方块。

### 项目里怎么用

移动分为三个阶段：

```text
1. 计算玩家和所有被推动实体的候选位置
2. 统一检查墙、越界、占用和传送规则
3. 全部合法后，一次性修改状态
```

### 为什么先模拟再提交

如果边移动边修改，可能出现：

```text
玩家移动成功
第一个方块移动成功
第二个方块移动失败
```

这样棋盘就进入了错误状态。

统一提交可以保证一次操作要么全部成功，要么全部失败。

### 面试怎么回答

“我把移动做成事务式流程。先收集所有候选位置，再统一验证，最后才写回状态。这样推挤链失败时不会留下半完成状态。”

## 9. 双向传送门

### 是什么

两个传送门不是只能从入口到出口，而是任意一端都可以作为起点。

### 项目里怎么用

传送门分为：

```text
PortalPair：描述哪两个端点是一对
PortalLink：描述当前端点应该传送到哪里
```

### 为什么不直接逐个格子传送

如果一次只传送一个格，复合体会被拆散，材质和连接也会错位。

项目使用“整体平移”：

1. 找到一个触发传送的格子。
2. 找到另一端。
3. 保持整个实体形状不变。
4. 将所有格子一起平移。

### 不规则复合体怎么处理

传送完成后，整个实体必须完全越过出口格，不能压在传送门上。

如果出口周围空间不够，移动失败。

### 面试怎么回答

“传送不是逐格移动，而是把完整实体平移。这样 L 形或更复杂的复合体不会变形，也不会覆盖传送门格。”

## 10. 可移动传送节点

### 是什么

传送门本身也是一个实体，可以被玩家推动。

### 项目里怎么用

- 玩家进入节点时，推动节点。
- 方块或复合体进入节点时，执行传送。
- 可移动节点不能推进其他传送门格。
- 可移动节点显示四个方向标识。

### 为什么需要特殊处理

节点同时属于“实体系统”和“传送门系统”。

如果不分开处理，方块传送到节点时可能会把节点推走，表现为“方块替换了传送门”。

## 11. ScriptableObject

### 是什么

ScriptableObject 是 Unity 提供的数据资产。

它可以像配置文件一样在 Inspector 中编辑。

### 项目里怎么用

关卡和美术主题使用 ScriptableObject：

```text
LevelDefinitionAsset：关卡地图、材质层、标题、标准解
LevelCatalogAsset：关卡顺序
ArtThemeAsset：颜色、Sprite、音效
```

### 为什么需要

可以把内容和代码分开。

修改关卡或美术时，不需要重新写规则代码。

### 面试怎么回答

“我使用 ScriptableObject 保存关卡和美术配置，运行时再转换成普通 C# 数据对象。这样既能用 Inspector 编辑，也能让规则层保持可测试。”

## 12. Command Pattern

### 是什么

Command 是“把一次操作封装成对象”的设计模式。

项目里的操作包括：

```text
移动
拆分
精确切割
重组
旋转
```

### 项目里怎么用

每次操作都包成一个 Command，交给 GridSession 执行。

Command 保存：

```text
操作类型
历史名称
执行前状态
执行后状态
```

### 为什么需要

可以支持：

- 撤销
- 重做
- 历史记录
- 标准解回放
- 未来录像和回放系统

### 面试怎么回答

“Command 负责表达‘发生了什么’，状态快照负责恢复精确结果。这样可以避免为每个复杂玩法手写容易出错的逆操作。”

## 13. Memento 或状态快照

### 是什么

Memento 是保存一份对象状态的快照，之后可以恢复。

### 项目里怎么用

Command 执行前复制一份 GridBoardState，执行后再复制一份。

撤销时恢复 before，重做时恢复 after。

### 优点

- 正确性高。
- 复杂操作也容易恢复。
- 新玩法不需要马上写反操作。

### 缺点

- 每一步都保存完整状态。
- 棋盘很大时，内存会增加。

### 面试怎么回答

“当前项目规模不大，我优先保证撤销正确性，所以使用 Command 加完整快照。如果棋盘继续变大，可以改成 Delta 或反向 Command。”

## 14. FSM 状态机

### 是什么

FSM 表示一个对象只能处于有限状态中的一种。

### 项目里怎么用

关卡状态：

```text
Loading
Playing
Completed
```

玩家动作：

```text
Idle
Moving
Interacting
```

### 为什么需要

避免到处出现：

```csharp
if (isMoving && !isInteracting && !isCompleted)
```

状态机把状态切换集中管理。

### 面试怎么回答

“我使用轻量 FSM 管理关卡流程和玩家动作。它只负责状态切换和通知，不负责具体玩法规则。”

## 15. New Input System

### 是什么

Unity 新版输入系统，可以统一处理键盘、手柄和其他设备。

### 项目里怎么用

创建 CompoundBoxInput，集中定义：

```text
移动
确认
拆分
切割
重组
撤销
重做
旋转
```

游戏逻辑只问：

```text
玩家是否按下了拆分操作
```

不需要知道玩家按的是 X 还是手柄按钮。

### 为什么需要

- 方便增加手柄。
- 方便未来做键位重绑定。
- 输入不会散落在玩法代码中。

### 面试怎么回答

“我把输入封装成独立输入层，游戏控制器只读取语义动作。这样以后增加手柄或重绑定键位时，不需要修改玩法逻辑。”

## 16. JSON 存档

### 是什么

JSON 是常见的数据格式，可以保存为文本文件。

### 项目里怎么用

存档保存：

```text
最高解锁关卡
每关是否完成
最佳移动数
最佳推动数
静音状态
```

文件位于：

```text
Application.persistentDataPath/compound-box-save.json
```

### 当前不保存

- 中途棋盘状态
- 撤销历史
- 当前实体位置

### 面试怎么回答

“当前存档主要保存进度和最佳成绩。正式产品还需要增加中途恢复、原子写入和版本迁移。”

## 17. ObjectPool 对象池

### 是什么

对象池是提前创建对象，用完后不销毁，而是回收再利用。

### 项目里怎么用

复用：

- 实体根节点
- 实体格子视图
- 移动预览 Sprite

### 为什么需要

频繁创建和销毁 GameObject 会产生 GC，导致卡顿。

### 面试怎么回答

“拆分、重组和移动预览会频繁创建显示对象，所以我用对象池复用它们，减少 GC 和实例化开销。”

## 18. EditMode 自动化测试

### 是什么

EditMode 测试是在 Unity 编辑器里快速运行，不需要进入 Play Mode 的测试。

### 项目里怎么用

测试直接运行玩法规则：

```text
解析关卡
执行移动
检查结果
回放标准解
```

### 为什么需要

规则修改后，可以立刻知道原来的关卡是否还能解。

### 项目测试覆盖

- 12 关标准解
- 移动和阻挡
- 双向传送
- 不规则复合体传送
- 可移动传送节点
- 拆分和重组
- 精确切割
- 旋转
- 撤销重做
- JSON 存档
- New Input System 初始化

### 面试怎么回答

“我把关卡标准解作为回归测试。修改规则后，测试会自动重放解法，避免出现某个关卡突然不可解。”

## 19. 编辑器工具

### 是什么

Unity 编辑器工具是只在开发时运行的工具，不会进入玩家版本。

### 项目里怎么用

Level Workbench 可以：

- 查看全部关卡
- 显示尺寸、实体、目标和机制统计
- 验证关卡
- 回放标准解
- 直接进入指定关卡

### 为什么需要

编辑器工具复用生产规则，避免“编辑器验证通过但运行时失败”。

## 20. 对象池、GC 和性能

### GC 是什么

GC 是垃圾回收。

程序不断创建和丢弃对象时，内存管理器需要清理它们，可能造成短暂卡顿。

### 项目怎么处理

- 经常复用的显示对象使用对象池。
- 核心规则使用简单数据结构。
- 移动规则不依赖 Unity 物理。
- 关卡切换复用结构，而不是反复创建大量场景对象。

### 面试怎么回答

“我目前没有宣称大规模性能优化，主要是针对项目里最频繁的显示对象复用做了对象池，并在架构上避免把玩法规则放到物理系统里。”

## 21. 面试常见问题与回答

### 问：为什么不用 Unity 物理碰撞

答：

“这个项目是格子解谜，需要确定和可回放。物理引擎会受到帧率和碰撞精度影响，所以我使用逻辑网格计算位置，物理只适合做表现，不适合做核心规则。”

### 问：为什么使用 Command

答：

“移动、拆分和传送都可能一次改变多个对象，如果每个操作单独写撤销逻辑会很容易出错。我先用 Command 表示操作，再用状态快照恢复，保证正确性。”

### 问：传送门怎么保证复合体不变形

答：

“我会让整个实体保持形状一起平移，而不是每个格子单独传送。传送后还会检查出口附近空间是否足够。”

### 问：为什么用 ScriptableObject

答：

“ScriptableObject 适合在 Inspector 配置关卡和美术。运行时我会转成普通 C# 数据对象，方便测试和状态复制。”

### 问：测试怎么做

答：

“我现在有 35 个 EditMode 测试，规则不需要启动场景就能运行。每个关卡的已知解法也会自动回放，确认修改规则后关卡仍然可解。”

### 问：项目最大的问题是什么

答：

“当前架构还有很多可以改进的地方，例如逻辑层和应用层还在同一个程序集，CompoundBoxApp 职责偏多，HUD 使用 OnGUI，存档没有中途恢复和原子写入。下一步我会先拆程序集和控制器，再补关卡编辑器和求解器。”

## 22. 学习顺序

### 第一天

学习：

- GridBoardState
- GridEntity
- EntityCellState
- EntityConnection
- LevelParser

目标：

能解释“一个复合体在程序里是怎么保存的”。

### 第二天

学习：

- GridBoardSimulation
- 推挤链
- 双向传送
- 不规则复合体
- GridSession 和 Command

目标：

能解释“玩家按一次移动后，程序做了哪些步骤”。

### 第三天

学习：

- New Input System
- ScriptableObject
- JSON 存档
- 对象池
- EditMode 测试

目标：

能解释“为什么这些系统要和玩法逻辑分开”。

## 23. 不建议写的夸张内容

不建议写：

- 精通 Unity 和 C#
- 高并发服务器
- 大型多人联机
- 商业级美术
- 自研物理引擎
- 支持百万用户
- 完整商业项目

可以诚实写：

- 独立完成的 Unity 作品集 Demo
- 使用自定义逻辑网格架构
- 12 个可完成关卡
- 35 个自动化测试
- Windows 独立构建
- 已实现主界面、存档推进和关卡验证工具

## 24. 简历最精简版

从 0 到 1 独立完成的 Unity 2D 网格解谜作品集 Demo，围绕多格复合实体、双向传送门和拆分重组设计 12 个递进关卡。使用逻辑网格实现确定性移动和推挤链，通过 Command 和状态快照实现撤销重做，使用 ScriptableObject 管理关卡与美术主题，接入 New Input System 和 JSON 存档，并编写 35 个自动化测试验证全部标准解和核心机制。项目包含主界面、关卡选择、四章节、存档推进和 Windows 独立构建。

技术栈：Unity / C# / 2D Grid / ScriptableObject / Command Pattern / FSM / New Input System / JSON / ObjectPool / Unity Test Framework

