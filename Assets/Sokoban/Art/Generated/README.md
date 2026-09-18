# Generated Art

本目录中的 PNG 是用于作品集展示的运行时截图。

最终区域截图：

- `portfolio-assembly.png`
- `portfolio-alloy.png`
- `portfolio-structure.png`
- `portfolio-main-menu.png`

游戏中的棋盘、方块、玩家、目标、出口和传送门 Sprite 均由 `WhiteboxSprites.cs` 在运行时生成，不依赖外部贴图。音效同样由 `ProceduralAudio.cs` 在运行时合成。

这样做的目的：

- 保持 Demo 可独立运行
- 避免版权和素材授权问题
- 让评审先关注规则、状态变化和工程实现
- 为后续正式美术替换保留清晰的表现层边界
