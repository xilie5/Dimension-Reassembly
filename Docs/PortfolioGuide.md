# Compound Box - Portfolio Guide

## 1. 一分钟演示脚本

1. `First Push`：展示移动、推动、目标和出口。
2. `Split Decision`：展示全拆分和独立配送。
3. `Anchor Transit`：展示双向传送。
4. `Compound Gate`：展示多格复合体通过单格传送门。
5. `Reassembly`：展示相邻同材质重组。
6. `Precision Cut`：展示连接图和精确切割。
7. `Rotation Vault`：展示旋转和复合目标。
8. `Portable Gate`：展示可移动传送节点和双向传送。
9. 打开 `Level Workbench`，展示关卡验证、指标分析和标准解回放。
10. 在 Test Runner 中运行 `CompoundBox.EditModeTests`，展示 35/35 通过。

## 2. 简历项目描述

### 中文版

独立开发 Unity 2D 网格解谜作品集项目，围绕多格复合实体、双向传送门、拆分、精确切割和重组设计 12 个递进关卡。采用逻辑网格与表现层分离架构，实现原子推挤链、不规则复合体形状保持传送、可移动传送节点、Command/Memento 撤销重做、ScriptableObject 关卡配置、New Input System、JSON 存档、对象池和 35 个自动化测试，并使用生产规则验证全部标准解。

### English version

Built a Unity 2D grid-puzzle portfolio project around compound polyomino entities, bidirectional portals, splitting, precision cutting, recombination, rotation, and 12 progression levels. Implemented deterministic transactional movement, shape-preserving teleportation, command-based undo/redo, ScriptableObject level content, New Input System support, JSON progression, object pooling, editor validation tools, and 35 automated EditMode tests.

## 3. 面试重点

### 规则模拟

- 为什么逻辑状态不能直接使用 Transform。
- 为什么移动必须先收集提案、统一验证、最后提交。
- 为什么多格实体要保存 CellState 和 Connection，而不是只保存一个覆盖范围。

### 传送系统

- 为什么传送门需要 PortalPair 和 PortalLink 两层。
- 如何支持双向传送。
- 如何让不规则复合体传送后不变形、不覆盖传送门格。
- 如何处理可移动传送节点。

### 状态管理

- 为什么使用 Command 保存操作语义。
- 为什么用 Memento 保证复杂操作 Undo/Redo 正确。
- 这种方案的内存代价是什么。
- 如何演进为反向 Command 或 Delta 历史。

### 数据驱动

- 为什么 ScriptableObject 和运行时 POCO 分开。
- LevelParser、LevelAudit、GridSession 如何复用。
- 如何让编辑器、运行时和测试共用同一套规则。

### 工程化

- 如何使用 New Input System 隔离输入。
- 如何用对象池减少 GC。
- 如何通过标准解自动回放保证关卡回归。
- 当前架构有哪些不足，下一步如何拆分。

## 4. 当前可继续深化的方向

- 拆分 Core、Presentation、Editor 程序集。
- 建立可重绑定的 InputActionAsset。
- 制作可视化网格关卡编辑器。
- 引入 BFS/IDA* 求解器和最短解验证。
- 增加中途存档、原子写入和版本迁移。
- 扩展到 18-24 关并增加传送门过滤和方向关系。
- 用正式动画、粒子、镜头反馈和 UI Toolkit 替换当前表现层。

## 5. 推荐展示素材

- `Assets/Sokoban/Art/Generated/portfolio-main-menu.png`
- `Assets/Sokoban/Art/Generated/portfolio-assembly.png`
- `Assets/Sokoban/Art/Generated/portfolio-alloy.png`
- `Assets/Sokoban/Art/Generated/portfolio-structure.png`
- `Assets/Sokoban/Art/Generated/portfolio-foundry-ai.png`

完整技术细节见：

- `Docs/TechnicalPortfolioReport.md`
- `Docs/Architecture.md`
