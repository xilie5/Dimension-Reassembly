# Compound Box - 美术素材来源与版权合规规范

## 1. 当前状态

当前工程没有导入外部美术、音频或字体资源。

现有表现由代码生成：

- `WhiteboxSprites`
- `GridPalette`
- `EntityCellView`
- `BoardView`
- `ProceduralAudio`

因此当前版本的版权风险最低。

## 2. 允许的素材来源

### CC0

优先使用：

- Kenney
- OpenGameArt 中明确标为 CC0 的素材
- itch.io 中明确标为 CC0 的素材
- Freesound 中明确标为 CC0 的音频

CC0 资源仍需保留来源记录，便于审计。

### CC-BY

可以使用，但必须在游戏中或 `ThirdPartyNotices` 中署名：

- 作者
- 素材名称
- 来源链接
- 许可证
- 修改说明

### OFL 字体

推荐：

- Google Fonts
- Noto Sans
- Source Han Sans
- Roboto

字体文件及 OFL 文本必须随项目保存。

### 禁止直接使用

- 来源不明图片
- 网络上没有许可证说明的图片
- 商业游戏截图裁切素材
- 受版权保护的角色
- 商标、品牌和官方 UI
- 未经授权的音乐和音效

## 3. AI 素材生成

### 可用工作流

1. 使用自有账号或合规模型
2. 编写不含版权角色的提示词
3. 生成候选素材
4. 人工检查是否存在近似受保护内容
5. 进行清理、重绘和统一风格
6. 保存模型和提示词记录
7. 记录许可证和使用条款
8. 导入 Unity 并登记

### 必须保存的记录

```text
assetId
sourceType
tool
model
modelVersion
prompt
seed
generatedAt
licenseUrl
commercialUseStatus
modifications
finalAssetPath
```

### 禁止的提示方式

- 模仿具体艺术家
- 直接生成商业游戏角色
- 生成品牌 Logo
- 使用未授权图片作为图生图输入
- 生成后伪装为手绘原创

## 4. Unity 导入规范

### 2D Sprite

- Texture Type: Sprite (2D and UI)
- Pixels Per Unit: 64 或 128
- Filter Mode: Point 或 Bilinear
- Compression: 按平台调整
- Pivot: Center 或自定义
- Sprite Atlas: 按主题拆分

### Prefab

- 名称稳定
- 不包含规则逻辑
- 只负责视觉和动画
- 通过对象池创建

## 5. 目录规范

```text
Assets/Sokoban/Art/
  Themes/
    Foundry/
      Floors/
      Walls/
      Entities/
      Portals/
      Goals/
  UI/
  Fonts/
  VFX/
  ThirdParty/
```

每个第三方素材目录必须有：

```text
README.md
LICENSE.txt
SOURCE.txt
```

## 6. 发布前检查

- 所有外部素材都有许可证
- CC-BY 素材已署名
- OFL 字体已包含许可证
- AI 素材有生成记录
- 没有品牌和角色侵权
- 商店宣传图与游戏内素材来源一致
- `ThirdPartyNotices.md` 完整
