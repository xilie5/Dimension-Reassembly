# Compound Box - Technical Design

## 1. 技术栈

- Unity 2022.3.62f3c1
- C#，无第三方运行时依赖
- Unity Test Framework
- 旧版 Input Manager 作为 Demo 输入后端
- 所有白盒 Sprite 和音效在运行时程序化生成

## 2. 分层

```text
CompoundBoxApp
  -> LevelCatalogAsset
       -> LevelDefinitionAsset
  -> GridSession
       -> IGridCommand
            -> MoveGridCommand
            -> SplitGridCommand
            -> RecombineGridCommand
       -> GridBoardState
            -> GridEntity
            -> GoalDefinition
            -> PortalPair
  -> BoardView
       -> ObjectPool<EntityCellView>
  -> GameHud
  -> ProceduralAudio
  -> SaveService
  -> StateMachine<T>
```

`GridSession`、Command 和 `GridBoardState` 不依赖场景对象。它们可以在 EditMode 测试、编辑器工具和运行时中复用。

`BoardView` 只负责观察状态并同步视觉对象，不参与规则判定。可以在未来替换为 Tilemap、Sprite Atlas、Shaker 或 DOTS 表现方案。

## 3. 状态模型

### GridBoardState

- 棋盘尺寸
- 2D 瓦片数组
- 实体集合
- 目标定义
- 传送门映射
- 玩家 ID
- 当前朝向
- 出口位置
- 步数、推动数、动作数

### GridEntity

- 稳定 ID
- 实体类型
- 材质类型
- 绝对格子坐标集合
- 派生锚点

锚点使用最小 Y、再最小 X 的规则，保证同一形状在不同实例上具有确定性。

## 4. 动作流水线

每个玩家操作会被封装成一个 `IGridCommand`。`GridSession` 只负责将成功命令放入 undo stack，将撤销的命令放入 redo stack。

Command 的 `Execute`、`Undo` 和 `Redo` 共享同一套规则实现。Command 内部保存执行前和执行后的棋盘 Memento，因此既能恢复状态，也能保留动作语义和历史标签。

每次移动命令遵循以下顺序：

1. 保存执行前状态
2. 从玩家开始构建移动集合
3. 为每个移动实体计算目标位置
4. 如果任一目标格触发传送门，则整体平移到出口锚点
5. 检查目标位置是否与静止实体重叠，并将被撞实体加入移动集合
6. 重复直到移动集合稳定
7. 统一检查墙、越界和重叠
8. 仅在全部合法时写回状态
9. 保存执行后状态并进入 Command 历史

这种“先收集、后验证、最后提交”的方式避免了部分实体已经移动、部分实体失败造成的中间状态。

## 5. 传送语义

传送门使用锚点对齐，而不是逐格吸附：

1. 计算实体移动后的候选格子
2. 如果任一候选格是传送门入口，找到对应出口
3. 计算候选形状锚点到出口的位移
4. 对实体全部候选格应用相同位移
5. 如果传送后再次进入入口，最多处理 4 次并检测循环

这一设计保证复合实体不会在传送中变形、旋转或拆散。

## 6. Command、撤销与重做

- `MoveGridCommand`、`SplitGridCommand`、`RecombineGridCommand` 分别描述操作
- Command 的 `Execute` 内部完成规则模拟
- Command 保存 before/after Memento，实现稳定 Undo/Redo
- 失败动作不会进入历史
- 新动作会清空 redo stack
- 历史上限为 120 个命令
- `UndoHistory` 对外提供 `Move R`、`Split` 等可读标签

该结构可以直接扩展为动作回放、录像、远程同步或 AI 搜索节点。

## 7. 状态机

项目提供通用 `StateMachine<TState>`，当前用于：

- 关卡流程：`Loading -> Playing -> Completed`
- 玩家动作：`Idle -> Moving/Interacting -> Idle`
- 机关保留 `Closed / Open / Locked` 状态定义

状态机负责迁移和生命周期事件，游戏逻辑不再依赖分散的布尔标记和多重 `if-else`。

## 8. 数据驱动

关卡使用 `LevelCatalogAsset` 和 `LevelDefinitionAsset` 配置：

- 策划可以在 Inspector 修改标题、布局和解法代码
- 运行时不直接依赖具体关卡硬编码
- 如果 Catalog 缺失，仍会回退到 `BuiltInLevels`，保证工程可启动
- `Tools > Compound Box > Create or Refresh Default Catalog` 可从代码内容重建资产

关卡格式：

| 字符 | 含义 |
| --- | --- |
| `#` | 墙 |
| `.` / 空格 | 地板 / 虚空 |
| `@` | 玩家 |
| `1` - `4` | 四种材质块 |
| `x` / `y` / `z` / `w` | 对应材质的普通目标 |
| `X` / `Y` / `Z` / `W` | 对应材质的复合目标 |
| `a` - `d` | 传送门入口 |
| `A` - `D` | 对应入口的出口 |
| `E` | 出口 |

同一个连通的正交同材质区域会在解析时组成一个初始复合实体。

解法代码：

- `U` / `R` / `D` / `L`：移动
- `S`：拆分
- `C`：重组

## 9. 存档与推进

`SaveService` 使用 `JsonUtility` 将存档写入：

`Application.persistentDataPath/compound-box-save.json`

保存内容：

- 已解锁关卡
- 每关完成状态
- 最佳步数
- 最佳推动数
- 静音设置

关卡完成后自动更新存档。`N` 或 `]` 只能进入已经解锁的下一关。

## 10. 对象池

`ObjectPool<T>` 是一个轻量泛型组件池。`BoardView` 使用它复用：

- 实体根节点
- 实体格子表现

拆分和重组会频繁创建/释放表现对象，如果直接 `Destroy` 会产生明显 GC 抖动。释放到对象池后会停用并保留层级组件，下次配置颜色、形状和位置即可重新使用。

## 11. 编辑器工具

`Tools > Compound Box > Level Workbench`

- 使用生产解析器加载全部关卡
- 回放每关记录解法
- 显示关卡验证结果
- 从任意关直接进入 Play Mode

`Tools > Compound Box > Validate All Levels`

该命令将结果写入 Console，适合以后接入 CI 的 EditMode 测试入口。

## 12. 测试策略

当前 EditMode 测试覆盖：

- 全部内置关卡的标准解法
- 基本阻挡
- 拆分与重组
- 复合体传送
- 撤销与重做
- Command 双栈和历史标签
- FSM 状态迁移
- JSON 存档往返
- ScriptableObject 关卡转换
- 对象池复用
- 全部关卡验证报告

推荐后续补充：

- 每次移动后实体不重叠的不变量测试
- 随机动作模糊测试
- 关卡最短解 BFS
- 传送门循环与多门组合测试
- 分辨率适配测试

## 13. 扩展点

- 使用 Addressables 异步加载 Catalog
- 增加多步动画队列和输入缓冲
- 引入 Tilemap 和 Rule Tile 替换白盒静态层
- 增加压力板、可破坏墙、颜色过滤器
- 增加关卡编辑器并在 Scene View 直接绘制
- 将输入抽象为 `IInputSource`，同时支持旧输入和 Input System
