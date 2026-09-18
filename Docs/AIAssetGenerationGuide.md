# Compound Box - AI 美术素材生成指南

## 1. 可用路径

有两种生成路径：

1. Codex 内置图像生成工具
   - 最省事
   - 支持直接要求透明背景
   - 当前会话未注册该工具

2. `imagegen` CLI 回退
   - 使用 `scripts/image_gen.py`
   - 需要本地设置 `OPENAI_API_KEY`
   - 需要网络
   - 默认模型为 `gpt-image-2`

重要限制：

- `gpt-image-2` 不支持原生透明背景。
- 如果需要 AI 直接输出透明 PNG，需要明确确认使用 `gpt-image-1.5`。
- 不要把 API Key 粘贴到聊天中。

## 2. 推荐素材集

第一轮只生成统一风格的七类棋盘素材：

```text
floor_tile
wall_tile
matter_block
player_unit
goal_socket
portal_emitter
exit_gate
```

生成时使用统一风格和统一背景。建议先生成洋红背景版本，再由人工处理成透明 PNG。

## 3. 统一美术方向

- 工业科幻
- 俯视正交视角
- 清晰矢量感
- 深海军蓝、冷钢灰、青色灯光
- 明亮边缘和有限高光
- 64x64 网格逻辑尺寸
- 轮廓必须在小尺寸下可读

避免：

- 透视
- 3D 渲染感
- 写实材质
- 大量噪点和颗粒
- 品牌、文字、水印
- 具体艺术家风格

## 4. CLI 生成流程

设置环境变量后，可在 PowerShell 中运行：

```powershell
$env:OPENAI_API_KEY="your-key-here"
$IMAGE_GEN="$env:USERPROFILE\.codex\skills\.system\imagegen\scripts\image_gen.py"

python $IMAGE_GEN generate-batch `
  --input Tools\AIAssets\foundry-prompts.jsonl `
  --out-dir output\imagegen\foundry `
  --concurrency 4
```

生成完成后，把最终选定文件放入：

```text
Assets/Sokoban/Art/Themes/Foundry/
```

## 5. 透明背景处理

如果没有使用透明模型：

1. 使用统一洋红背景生成。
2. 在 Krita、Photoshop、Affinity Photo 或 GIMP 中选择背景。
3. 扩展选区并删除。
4. 清理边缘色溢。
5. 导出 PNG RGBA。

不要用简单色键处理带强反射或阴影边缘的素材。

## 6. Unity 导入设置

```text
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 64
Mesh Type: Full Rect
Extrude Edges: 0
Alpha Is Transparency: On
Generate Mip Maps: Off
Wrap Mode: Clamp
Filter Mode: Bilinear
Compression: Uncompressed
```

导入后写入 `ArtThemeAsset`：

```text
FloorSprite
WallSprite
GoalSprite
ExitSprite
PortalSprite
PlayerSprite
MatterSprite
```

运行：

```text
Tools > Compound Box > Create or Refresh Foundry Theme
```

## 7. 生成记录

每个最终素材需要记录：

```text
assetName
prompt
model
modelVersion
seed
generatedAt
license/terms
humanEdits
finalPath
```

禁止：

- 模仿具体在世艺术家
- 生成受版权保护的角色
- 生成品牌标识
- 使用未授权图片作为参考或图生图输入

## 8. 推荐迭代方式

1. 用低质量生成 4 个方向。
2. 只选择一个统一风格。
3. 每个对象单独生成。
4. 逐个清理透明边缘。
5. 在 64x64 网格中检查可读性。
6. 写入主题资产后在 Unity 中截图。
7. 如果风格不统一，先统一轮廓和调色板，再重做单体。

## 9. 更换素材不需要改玩法

棋盘表现已经通过 `IBoardVisualTheme` 解耦。

替换素材只需要修改 `FoundryTheme` 的 Sprite 引用，不需要修改：

- `GridBoardState`
- `GridSession`
- `IGridCommand`
- 传送
- 切割
- 旋转
- 关卡解析
- 存档
