# Compound Box - 完整游戏开发流程与制作计划

## 1. 产品定义

项目名称：Compound Box

类型：2D 网格解谜

核心体验：

> 操作可拆分、可传送、可重组的物质结构，让它满足机器的形状和材质要求。

目标平台：

- PC
- Windows 首发
- 后续可考虑 macOS 和 Linux

首发范围：

- 12 关 P0 垂直切片
- 可继续扩展到 24 到 30 关商业版本

最终目标：

- 先形成可展示、可验证、可扩展的作品集版本
- 再决定是否扩展为商业发行版本

## 2. 需求分析

### 2.1 玩家需求

目标玩家喜欢：

- Sokoban
- Patrick's Parabox
- Sokobond
- Snakebird
- Baba Is You
- A Monster's Expedition

玩家需要：

- 清晰规则
- 快速撤销
- 稳定操作
- 可理解的传送结果
- 明确的目标形状
- 不过度惩罚失败

### 2.2 产品需求

P0 必须满足：

- 12 个可完成关卡
- 主界面
- 章节选择
- 存档解锁
- 最佳成绩
- 标准解验证
- 编辑器验证
- Windows 构建
- 可替换的白盒表现层

### 2.3 非目标

P0 不做：

- 战斗
- 敌人
- 生命值
- 随机地图
- 在线功能
- 重力模拟
- 完整递归空间
- 复杂剧情分支

## 3. 游戏设计

### 3.1 核心循环

```text
观察目标结构
  -> 规划移动
  -> 推挤和传送
  -> 拆分或切割
  -> 传送部件
  -> 重组结构
  -> 满足目标
  -> 解锁下一关
```

### 3.2 核心动词

| 动词 | 输入 | 作用 |
| --- | --- | --- |
| 移动 | WASD / 方向键 | 改变玩家位置 |
| 推挤 | 移动 | 推动实体 |
| 全拆分 | X | 把实体拆成单格 |
| 精确切割 | V | 切断一条连接 |
| 旋转 | Q / E | 改变复合体方向 |
| 重组 | C | 合并相邻同材质实体 |
| 撤销/重做 | Z / Ctrl+Y | 恢复或重放操作 |

### 3.3 关卡结构

```text
Assembly
  -> Transit
  -> Alloy
  -> Structure
  -> Final Chamber
```

每个机制都要有：

- 引入关
- 应用关
- 组合关
- 综合关

### 3.4 难度曲线

| 阶段 | 目标 |
| --- | --- |
| 1-3 | 教移动、拆分、传送 |
| 4-6 | 教组合、传送和重组 |
| 7-9 | 教材质和多格目标 |
| 10-12 | 教切割、旋转和动态节点 |
| P1 | 扩展 13-24 关和高级组合 |

### 3.5 主界面

计划状态：

```text
MainMenu
LevelSelect
Playing
Completed
```

主界面按钮：

- Continue
- New Game
- Chapter Select
- Settings
- Quit

## 4. 技术选型

### 引擎

- Unity 2022.3 LTS
- C#
- 2D Renderer

### 数据

- ScriptableObject 关卡资产
- LevelCatalogAsset 关卡顺序
- JsonUtility 进度存档

### 规则

- 逻辑二维网格
- 确定性状态模拟
- Command + Memento

### 输入

- New Input System
- 键盘和手柄动作映射
- `CompoundBoxInput` 隔离应用层与输入设备

### 测试

- Unity Test Framework
- EditMode 测试
- 标准解回放
- 构建命令行脚本

### UI

P0：

- 当前 OnGUI HUD

正式版本：

- UI Toolkit
- 或 Canvas + UGUI

### 美术

P0：

- Foundry AI 主题素材
- Kenney CC0 UI 与音效
- 程序化白盒备用表现

正式版本：

- Prefab
- Sprite Atlas
- Tilemap
- Animator
- VFX
- URP 2D

## 5. 架构计划

目标结构：

```text
CompoundBox.Core
  State
  Entities
  Simulation
  Commands
  Parsing
  Validation
  Save Models

CompoundBox.Runtime
  Bootstrap
  Input
  Board Presentation
  HUD
  Audio
  Save Adapter

CompoundBox.Editor
  Level Workbench
  Level Editor
  Audit
  Solver
  Build Tools

CompoundBox.Tests
  Core Tests
  Content Tests
  Save Tests
```

## 6. 开发阶段

### Phase 0：Pre-production

交付：

- GDD
- TDD
- 竞品分析
- 架构说明
- 关卡课程表
- 美术合规规范

验收：

- 机制冻结
- 范围冻结
- 目标平台冻结

### Phase 1：Vertical Slice

交付：

- 12 关
- 核心机制
- 存档
- 章节选择
- HUD
- 测试

当前状态：

- 基本完成
- 主界面和关卡编辑器仍待补齐

### Phase 2：Production

交付：

- 主界面
- Catalog-first 关卡工作流
- 可视化关卡编辑器
- 24 到 30 关
- 正式美术主题
- 动画和 VFX

### Phase 3：Alpha

交付：

- 完整内容
- 完整 UI
- 全部机制教学
- 全部存档流程

### Phase 4：Beta

交付：

- Bug 修复
- 性能优化
- 分辨率适配
- 存档迁移
- 完整通关测试

### Phase 5：Release

交付：

- Windows 构建
- 商店截图
- 演示视频
- 版本说明
- 许可证清单
- 发布检查报告

## 7. 关卡制作流程

### 当前流程

1. 修改 `BuiltInLevels.cs`
2. 执行 Create or Refresh Default Catalog
3. 进入 Level Workbench
4. 运行验证
5. 进入 Play Mode

### 目标流程

1. 在主界面或 Workbench 中点击 New Level
2. 创建 LevelDefinitionAsset
3. 在可视化网格中绘制地形
4. 放置玩家、实体、目标和传送门
5. 编辑材质层
6. 编辑连接边
7. 输入标准解
8. 运行求解器
9. 保存到 Catalog
10. 直接进入测试

## 8. 美术资源方案

### 开源免费资源

推荐来源：

- Kenney CC0 assets
- OpenGameArt
- itch.io free assets
- Google Fonts OFL
- Freesound 中标记为 CC0 的音频
- Game-icons.net

使用规则：

- CC0 可直接使用
- CC-BY 必须保留署名
- 不允许直接把未知许可证素材放入工程
- 每次导入都记录来源和许可证证明

### AI 生成素材

可以使用：

- Adobe Firefly
- OpenAI 图像生成工具
- Stable Diffusion / ComfyUI
- 其他允许商业使用的模型

必须遵守：

- 不要求模仿具体在世艺术家
- 不使用受版权保护的角色
- 不使用商标和品牌标识
- 不输入未经授权的参考图
- 保存模型、提示词、种子和生成日期

### 素材生产管线

```text
概念参考
  -> 生成或下载
  -> 人工清理
  -> 导出 PNG
  -> 设置 PPU/Pivot
  -> 建立 Prefab
  -> 建立 Sprite Atlas
  -> Unity 导入
  -> 许可证登记
```

## 9. 测试与质量

### 自动化

- 标准解
- 状态不变量
- 存档读写
- 关卡解析
- 传送节点
- 切割和旋转

### 手工

- 新手教学
- 难度曲线
- 目标可读性
- 分辨率
- 输入延迟
- 音效反馈

### 性能

- 60 FPS
- 无持续 GC 峰值
- 对象池复用
- 关卡切换内存稳定

## 10. 当前优先级

1. 拆分 Core、Presentation、Editor 程序集
2. 拆分 CompoundBoxApp 的输入、流程和 UI 职责
3. 制作可视化网格关卡编辑器
4. 将输入迁移为可重绑定的 InputActionAsset
5. 统一关卡正式来源并移除重复配置
6. 增加中途存档、原子写入和求解器

## 11. 完成定义

正式产品完成时需要满足：

- 关卡全部可完成
- 主界面、游戏、章节、存档完整
- 关卡编辑器可用于批量制作
- 所有素材有许可证记录
- Windows 构建稳定
- 测试通过
- 文档、截图和演示视频齐全
