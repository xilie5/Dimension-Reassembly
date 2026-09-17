# Compound Box - Sprint 执行看板

本文档将生产计划拆成当前可追踪任务。状态只允许使用：

- `DONE`
- `DOING`
- `NEXT`
- `BLOCKED`
- `DEFERRED`

## S0 - Pre-production

| ID | 任务 | 状态 | 验收 |
| --- | --- | --- | --- |
| S0-1 | 冻结 15 关范围 | BLOCKED | 用户确认机制和关数 |
| S0-2 | 冻结机制课程 | BLOCKED | 用户确认切割、旋转、传送节点是否全部进入 1.0 |
| S0-3 | 初始化 Git | DONE | 仓库创建在 `main` 分支 |
| S0-4 | Unity 忽略规则 | DONE | 忽略 Library、Temp、Builds 等目录 |
| S0-5 | 本地测试脚本 | DONE | `Tools/Run-EditModeTests.ps1` |
| S0-6 | 本地构建脚本 | DONE | `Tools/Build-Windows.ps1` 和 Editor 构建入口 |
| S0-7 | 资产与场景规范 | BLOCKED | 用户确认场景方向、玩家身份和视觉基调 |
| S0-8 | 难度目标 | BLOCKED | 用户确认目标玩家和难度峰值 |

## S1 - Grid Entity V2

| ID | 任务 | 状态 | 依赖 |
| --- | --- | --- | --- |
| S1-1 | `EntityCellState` | DONE | S0-1 |
| S1-2 | `EntityConnection` | DONE | S0-1 |
| S1-3 | `GridEntity V2` | DONE | S1-1, S1-2 |
| S1-4 | Clone 与 Restore | DONE | S1-3 |
| S1-5 | V1 布局兼容转换 | DONE | S1-3 |
| S1-6 | 多材质移动测试 | DONE | S1-5 |
| S1-7 | 存档 Schema V2 | DONE | S1-3 |

## S2 - Preview And Editor V2

| ID | 任务 | 状态 | 依赖 |
| --- | --- | --- | --- |
| S2-1 | `TransformPreview` | BLOCKED | S1-3 |
| S2-2 | 移动 Ghost | BLOCKED | S2-1 |
| S2-3 | 传送 Ghost | BLOCKED | S2-1 |
| S2-4 | 编辑器地形与材质画笔 | BLOCKED | S1-3 |
| S2-5 | 连接边编辑 | BLOCKED | S1-2 |
| S2-6 | 标准解法验证 | BLOCKED | S2-4 |

## S3 - Multi-material

| ID | 任务 | 状态 | 依赖 |
| --- | --- | --- | --- |
| S3-1 | 逐格材质渲染 | BLOCKED | S1-3 |
| S3-2 | 复合目标 V2 | BLOCKED | S3-1 |
| S3-3 | 材质过滤器 | BLOCKED | S3-1 |
| S3-4 | 7 到 9 关 | BLOCKED | S3-2, S2-6 |

## S4 To S7 - Advanced Mechanics And Content

以下任务只有在 S0-2 设计冻结后才能启动：

- S4：连接图、精确切割、10 到 11 关
- S5：旋转、12 关
- S6：可移动传送节点、13 到 14 关
- S7：第 15 关和内容冻结

## 当前阻塞项

进入 S1 前必须确认：

1. 1.0 版本是否同时包含切割、旋转和移动传送节点。
2. 场景风格和玩家身份。
3. 难度目标。
4. 15 关是否必须全部完成，还是先交付 12 关 P0 版本。

## 当前下一步

S0 的工程基础已经完成。确认阻塞项后，下一批提交应从 `S1-1` 和 `S1-2` 开始。
