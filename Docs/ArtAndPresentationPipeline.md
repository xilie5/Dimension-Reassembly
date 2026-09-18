# Compound Box - 美术与表现层升级方案

## 1. 为什么 Scene 里看不到棋盘

当前 `SampleScene` 只包含：

- `Main Camera`
- `Compound Box Application`

`Compound Box Application` 上挂载 `CompoundBoxApp`。

棋盘、墙体、目标、玩家、材质块、传送门、HUD 和音效都不是场景序列化对象，而是在运行时创建。

因此：

- 编辑模式打开 Scene，只能看到 Camera 和应用入口。
- 进入 Play Mode 后，才会生成完整棋盘。
- 这不是场景损坏，也不是漏挂 Prefab，而是当前的代码生成表现方案。

## 2. 当前画面生成流程

```text
SampleScene
  -> CompoundBoxApp.Awake()
      -> 创建 BoardView
      -> 创建 GameHud
      -> 创建 ProceduralAudio
      -> 加载 LevelCatalogAsset
      -> LevelParser.Parse()
      -> BoardView.Rebuild()
          -> 创建地板、墙、目标、出口、传送门
          -> 创建玩家和 Matter 实体
```

### 静态棋盘

`BoardView.RebuildTiles()` 根据 `GridBoardState` 的瓦片数据创建：

- 地板 SpriteRenderer
- 墙体 SpriteRenderer
- 目标 SpriteRenderer
- 出口 SpriteRenderer
- 固定传送门 SpriteRenderer

### 玩家和物体

`BoardView.CreateEntityVisual()` 为每个逻辑实体创建视觉根节点，然后使用 `EntityCellView` 绘制：

- 阴影
- 外壳
- 内芯
- 连接标志
- 玩家方向箭头

`EntityCellView.ConfigurePlayer()` 当前绘制的是一个白色圆形维护单元，不是正式角色模型。

### Sprite 来源

`WhiteboxSprites.cs` 在运行时生成 64x64 Texture2D：

- `Square`
- `RoundedSquare`
- `Circle`
- `Ring`
- `Diamond`
- `Chevron`

`GridPalette.cs` 为这些 Sprite 提供统一颜色。

### 音效

`ProceduralAudio.cs` 使用 `AudioClip.Create` 运行时合成：

- 移动
- 推动
- 阻挡
- 拆分
- 重组
- 传送
- 完成

所以当前项目没有外部图片和音频资源依赖。

## 3. 当前方案的定位

当前表现层不是正式美术管线，而是：

```text
可替换的程序化白盒表现层
```

它的目的：

- 快速验证规则
- 保持工程独立可运行
- 不引入素材依赖
- 让评审先看到机制、状态变化和工程结构

它的缺点：

- Scene 编辑模式不可视
- 美术无法直接摆放和调整
- 没有 Sprite Atlas、Prefab、动画和材质资产
- 运行时动态创建 Texture 和 GameObject
- 正式视觉替换需要新增表现层接口

## 4. 是否要重写数据驱动框架

不需要。

应该保留：

```text
GridBoardState
GridEntity
EntityCellState
EntityConnection
GridSession
IGridCommand
GridBoardSimulation
LevelParser
LevelDefinitionAsset
LevelCatalogAsset
SaveService
```

这些负责：

- 规则
- 数据
- 回溯
- 关卡解析
- 存档
- 测试

需要替换或扩展的是：

```text
BoardView
EntityCellView
WhiteboxSprites
GameHud
ProceduralAudio
```

也就是：

```text
Core 保持
Presentation 替换
```

## 5. 推荐的正式表现架构

```text
ArtThemeAsset
  ├── floorSprites
  ├── wallPrefabs
  ├── goalPrefabs
  ├── exitPrefab
  ├── playerPrefab
  ├── matterCellPrefabs
  ├── portalPrefabs
  ├── materials
  └── VFX references

LevelDefinitionAsset
  └── optional themeId

BoardView
  -> reads ArtThemeAsset
  -> uses PrefabFactory or SpriteFactory
  -> uses ObjectPool
```

### 建议新增的接口

```text
IBoardVisualFactory
  CreateFloor()
  CreateWall()
  CreateGoal()
  CreateEntity()

IEntityVisualProvider
  GetPlayerVisual()
  GetMatterVisual()
  GetPortalVisual()
```

`BoardView` 不再直接决定 Sprite，而是向视觉提供者请求明确的视觉对象。

## 6. 推荐的两个正式方案

### 方案 A：Prefab 驱动

适合正式商业项目。

场景结构：

```text
Gameplay Scene
  Main Camera
  GridRoot
    StaticTileRoot
    EntityRoot
    PreviewRoot
  UIRoot
  AudioRoot
  GameFlow
```

每个对象使用 Prefab：

- PlayerPrefab
- MatterCellPrefab
- WallPrefab
- GoalPrefab
- PortalPrefab

运行时仍由 `GridBoardState` 驱动，但视觉对象从 Prefab 创建。

### 方案 B：Tilemap + Prefab 混合

适合关卡体量更大的项目。

- 静态地板和墙使用 Tilemap。
- 目标和出口使用 Tilemap 或 Prefab。
- 玩家、Matter 和 PortalNode 使用 Prefab。
- 逻辑层仍然是 `GridBoardState`。

Tilemap 只负责静态表现，不负责移动规则。

## 7. 迁移步骤

### 第一阶段：保持规则不变

1. 保留 Core 全部代码。
2. 为关卡增加 `themeId` 或 `visualTheme` 引用。
3. 让 `BoardView` 从主题资产读取 Sprite 和 Prefab。
4. 保留对象池。

### 第二阶段：替换玩家和材质块

1. 建立 `PlayerPrefab`。
2. 建立 `MatterCellPrefab`。
3. 建立不同材质颜色变体。
4. 用 Animator/VFX 替换当前 SpriteRenderer 组合。

### 第三阶段：替换静态棋盘

1. 把地板改成 Tilemap 或 Prefab。
2. 墙使用不同环境主题 Prefab。
3. 目标和出口使用正式美术。

### 第四阶段：替换 HUD

当前 OnGUI HUD 适合原型，不适合正式 UI。

建议迁移到：

- UI Toolkit
- 或 Canvas + UGUI

保留同样的数据接口：

```text
HUD 只读取 GridSession 和 SaveService
```

## 8. 正式产品还需要补充的内容

- 主菜单
- 设置页
- 暂停菜单
- 动画状态机
- VFX
- 精灵图集
- 材质和 Shader
- 章节主题
- 正式音频
- 本地化
- 中途关卡存档
- 关卡编辑器
- 构建自动化

## 9. 结论

当前 Scene 空白不是架构问题，而是表现层的设计选择：

> 规则层是数据驱动的，表现层是运行时程序化生成的。

做正式美术版本时，不需要推倒 Core。正确做法是：

```text
保留 Core 数据与规则
替换 BoardView / EntityCellView / HUD
新增 ArtThemeAsset 和视觉工厂
把程序化 Sprite 替换为 Prefab/Tilemap/正式资源
```

这样既能保留现有工程稳定性，又能让美术和关卡设计逐步接管表现层。
