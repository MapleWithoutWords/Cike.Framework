# Cike.Localization

本地化基础设施：自定义 `IStringLocalizer` 实现与资源贡献者模型。基础可用，**现有业务项目多为中文硬编码消息，未启用本模块**。

## 提供的能力

| 类型 | 说明 |
|---|---|
| `CikeStringLocalizer` / `CikeStringLocalizerFactory` | `IStringLocalizer` 实现，从注册的资源字典取词 |
| `[LocalizationResourceName]` | 标注资源类名 |
| `ILanguageProvider` | 当前语言提供者（业务方实现） |
| `ILocalizationResourceContributor` / `LocalizationResource` / `LocalizerResourceDictionary` | 多来源资源贡献模型（支持从 DB/远程等拉取词条） |
| `CikeLocalizationOptions` | 模块选项 |
| `CikeLocalizationModule` | 模块类，注册 `ResourceManagerStringLocalizerFactory` |

直接依赖：无。

## 更多

现状说明：[AI 开发指南](../../docs/AI-GUIDE.md) 第 8.9 节。
