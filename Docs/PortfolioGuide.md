# Compound Box - Portfolio Guide

## 1. 一分钟演示脚本

1. 打开 `First Push`，展示基础移动、推动、目标和出口。
2. 切到 `Split Decision`，按 `X` 拆分复合体，分别配送两个同材质碎片。
3. 切到 `Anchor Transit`，将物体推入入口，展示整体传送。
4. 切到 `Compound Gate`，展示二格复合体通过一格传送门。
5. 切到 `Reassembly`，把两个相邻同材质物体用 `C` 重组。
6. 切到 `Phase Loom`，说明“传送 -> 拆分 -> 双目标”的综合解法。
7. 打开 `Level Workbench`，运行全部关卡验证。
8. 在 Test Runner 中运行 `CompoundBox.EditModeTests`，展示 6/6 通过。

## 2. 简历项目描述

### 中文版

独立开发 Unity 2D 网格解谜 Demo，围绕“多格复合实体 + 网格传送门 + 拆分/重组”设计并实现 6 个递进式关卡。采用规则层与表现层分离架构，实现确定性推挤链、锚点式整体传送、Command 模式撤销/重做、通用 FSM、ScriptableObject 关卡配置、JSON 存档推进、对象池和程序化白盒音画表现。编写编辑器关卡工作台与 11 个自动化测试，使用同一套生产规则回放全部标准解法。

### English version

Built a Unity 2D grid puzzle vertical slice around compound polyomino entities, splitting/recombination, and grid portals. Implemented deterministic push chains, anchor-based whole-entity teleportation, command-pattern undo/redo, finite state machines, ScriptableObject level content, JSON save progression, object pooling, procedural whitebox visuals/audio, and automated solution replay for every bundled level.

## 3. 面试时重点讲什么

### 规则问题

为什么多格实体的传送必须使用锚点，而不是逐格传送：

- 逐格传送会剪切复合体
- 每个格子独立传送会产生顺序依赖
- 锚点整体传送保证形状稳定，方便关卡设计和玩家建立心智模型

### 架构问题

为什么先模拟再提交：

- 推挤链可能包含多个实体
- 传送可能让物体跨越棋盘
- 只有所有候选位置验证成功后写回，才能避免脏状态

### Command 与状态管理

为什么使用 Command 而不是只保存状态快照：

- Command 同时保留操作语义，可生成历史标签、回放和后续联机同步
- Memento 负责恢复精确状态，避免为每种操作单独编写反向推导
- Undo/Redo 只管理两个栈，输入层不需要知道具体玩法类型

为什么使用 FSM：

- 玩家、关卡和机关有明确的生命周期
- 状态迁移可以作为动画和输入门禁的统一来源
- 新状态只需要添加行为和迁移，不需要继续扩展布尔条件

### 工具问题

为什么把解法记录进关卡数据：

- 关卡是生产数据，不只是截图素材
- 规则修改后，自动回放可以立即发现关卡失效
- 编辑器工具、运行时和测试共用同一套解析与模拟代码

## 4. 可继续深化的方向

- 增加 12-18 关，引入传送门颜色过滤
- 制作 3 种具有方向关系的复合物
- 添加镜头震动、粒子、残影和切换动画
- 用 Tilemap 和正式美术替换白盒
- 加入关卡编辑器和本地关卡分享格式
- 添加最短步数统计、通关评级和提示系统

## 5. 展示素材

- `Assets/Sokoban/Art/Generated/runtime-hud.png`
- `Assets/Sokoban/Art/Generated/phase-loom-preview.png`

建议在简历或作品集页面同时放置“第一关教学”和“第六关全机制组合”两张图，避免只展示完成面板而看不到玩法。
