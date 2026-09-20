# Generated Art

本目录中的 PNG 是用于作品集展示的运行时截图。

最终区域截图：

- `portfolio-assembly.png`
- `portfolio-alloy.png`
- `portfolio-structure.png`
- `portfolio-main-menu.png`
- `portfolio-foundry-ai.png`

游戏优先使用 `FoundryTheme.asset` 中配置的正式 Sprite。主题资源缺失时，`WhiteboxSprites.cs` 会提供程序化备用图形；音频由 Kenney CC0 音效和 `ProceduralAudio.cs` 的程序化备用音效共同组成。

这样做的目的：

- 保持 Demo 可独立运行
- 避免版权和素材授权问题
- 让评审先关注规则、状态变化和工程实现
- 为后续正式美术替换保留清晰的表现层边界
