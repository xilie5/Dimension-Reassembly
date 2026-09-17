# Compound Box - Sprint 执行看板

本文档将生产计划拆成当前可追踪任务。状态只允许使用：

- `DONE`
- `DOING`
- `NEXT`
- `BLOCKED`
- `DEFERRED`

## P0 Scope Override

用户已确认：

- P0 交付 12 关
- 不包含任何美术素材，继续使用程序化白盒
- 保留多材质、精确切割、旋转、可移动传送节点
- 世界观使用相位铸造厂和维护机器人
- 难度采用教学关、机制应用关、中高难组合关
- 线性章节推进，完成后开放已通关章节选择

原计划中的 13 到 15 关移动为 P1 延期内容。

## S0 - Pre-production

| ID | 任务 | 状态 | 验收 |
| --- | --- | --- | --- |
| S0-1 | 冻结 12 关 P0 范围 | DONE | 已确认 |
| S0-2 | 冻结机制课程 | DONE | 已确认四类机制进入 P0 |
| S0-3 | 初始化 Git | DONE | 仓库创建在 `main` 分支 |
| S0-4 | Unity 忽略规则 | DONE | 忽略 Library、Temp、Builds 等目录 |
| S0-5 | 本地测试脚本 | DONE | `Tools/Run-EditModeTests.ps1` |
| S0-6 | 本地构建脚本 | DONE | `Tools/Build-Windows.ps1` 和 Editor 构建入口 |
| S0-7 | 资产与场景规范 | DONE | 相位铸造厂、维护机器人、程序化白盒 |
| S0-8 | 难度目标 | DONE | 教学、应用、中高难组合 |

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
| S2-1 | `TransformPreview` | DONE | S1-3 |
| S2-2 | 移动 Ghost | DONE | S2-1 |
| S2-3 | 传送 Ghost | DONE | S2-1 |
| S2-4 | 编辑器地形与材质画笔 | BLOCKED | S1-3 |
| S2-5 | 连接边编辑 | BLOCKED | S1-2 |
| S2-6 | 标准解法验证 | BLOCKED | S2-4 |

## S3 - Multi-material

| ID | 任务 | 状态 | 依赖 |
| --- | --- | --- | --- |
| S3-1 | 逐格材质渲染 | DONE | S1-3 |
| S3-2 | 复合目标 V2 | DONE | S3-1 |
| S3-3 | 材质过滤器 | DEFERRED | S3-1；P0 用材质分配目标替代 |
| S3-4 | 7 到 9 关 | DONE | S3-2 |

## S4 To S7 - Advanced Mechanics And Content

- S4：连接图、精确切割、第 10 关，已完成
- S5：旋转、第 11 关，已完成
- S6：可移动传送节点、第 12 关，已完成
- S7：12 关内容审计和冻结，下一步

## 当前阻塞项

当前没有阻塞 S1 和 S2 的设计问题。

## 当前下一步

S0、S1 和 S2 的移动预览部分已经完成。下一步是编辑器 V2，然后是 7 到 9 关的多材质内容。
