# 本地化

## 域定位

为框架提供自定义 `IStringLocalizer` 实现与"资源贡献者"模型：把本地化词条抽象为 `LocalizationResource`（资源类）+ 多个 `ILocalizationResourceContributor`（词条来源，可为内存/DB/远程），按 `cultureName` 取词。

**现状（必须先读）**：基础可用但严重不完整——资源贡献模型（`LocalizationResource`/`LocalizerResourceDictionary`/`ILocalizationResourceContributor`/`[LocalizationResourceName]`）已实现，但执行层（`CikeStringLocalizer`/`CikeStringLocalizerFactory`）全部成员 `throw NotImplementedException`，是存根；模块也未把任何本地化服务接入 `IStringLocalizer` 解析链。**现有业务项目多为中文硬编码消息，基本未启用本模块**；框架内（src 其他包、samples、tests）没有任何代码引用本包。新项目除非明确需要多语言，否则建议沿用团队惯例（中文硬编码消息/异常消息常量），不引入本模块。

## 覆盖的包

| 包 | 目录 | 定位 |
|---|---|---|
| `Cike.Localization` | `src/Cike.Localization`（net8.0） | 自定义 `IStringLocalizer` 实现与资源贡献者模型；直接依赖 `Cike.Core`（`CikeModule` 基类、`GetOrDefault` 字典扩展）与包 `Microsoft.Extensions.Localization` |

## Cike.Localization

### 定位

- 扩展 `Microsoft.Extensions.Localization` 的 `IStringLocalizer` 抽象：目标是让词条来源不限于 .resx，而是经 `ILocalizationResourceContributor` 从任意来源（内存/DB/远程）贡献。
- 命名与结构（`LocalizationResource`/`Contributors`/`BaseResourceNames`/`DefaultCultureName`）与 ABP 的本地化资源模型同构，但仅实现了数据结构层，取词执行层未完成。
- 后缀模型：`ILanguageProvider` + `LanguageInfo` 描述系统支持的语言清单，供业务方实现。

### 能力清单

按源码如实罗列（含未完成项）。公共类型全部位于命名空间 `Cike.Localization`，特性在 `Cike.Localization.Attributes`。

| 类型 | 成员 | 状态 |
|---|---|---|
| `CikeStringLocalizer` | 实现 `IStringLocalizer`：`this[string name]`、`this[string name, params object[] arguments]`、`GetAllStrings(bool includeParentCultures)` | **存根**：三个成员全部 `throw NotImplementedException` |
| `CikeStringLocalizerFactory` | 实现 `IStringLocalizerFactory`：`Create(Type resourceSource)`、`Create(string baseName, string location)`；主构造器类 | **存根**：两个成员全部 `throw NotImplementedException`；框架内从未注册 |
| `ILanguageProvider` | `IReadOnlyList<LanguageInfo> GetLanguageInfos()` | 可用；返回的是语言**清单**而非"当前语言"；框架内无实现、无消费方，需业务方实现并自行注册 |
| `LanguageInfo` | `CultureName`、`UiCultureName`、`DisplayName`（构造器 `(string cultureName, string uiCultureName, string displayName)`，属性可写） | 可用 |
| `ILocalizationResourceContributor` | `void Initialize(LocalizationResource resource, IServiceProvider serviceProvider)`；`LocalizedString? GetOrNull(string cultureName, string name)` | 可用；纯同步 API，无内建缓存/批量/刷新语义；框架内无任何调用点 |
| `LocalizationResource` | `Type ResourceType`、`string ResourceName { get; }`、`List<string> BaseResourceNames { get; }`、`string? DefaultCultureName { get; set; }`、`List<ILocalizationResourceContributor> Contributors { get; }`；构造器 `(Type resourceType, string? defaultCultureName = null, ILocalizationResourceContributor? initialContributor = null)` | 可用；`BaseResourceNames`/`DefaultCultureName` 在包内无任何消费逻辑 |
| `LocalizerResourceDictionary` | 继承 `Dictionary<string, LocalizationResource>`（键为 ResourceName）；`LocalizationResource Add(Type resourceType, string? defaultCultureName = null)`、`LocalizationResource? GetOrNull(Type resourceType)`、`LocalizationResource GetOrAdd(Type resourceType, string? defaultCultureName = null)` | 可用 |
| `LocalizationResourceNameAttribute` | `[AttributeUsage(AttributeTargets.Class)]`，构造器 `(string name)`，`string Name { get; set; }`；静态 `GetName(Type type)` → 特性名，未标注时回退 `type.FullName!` | 可用 |
| `CikeLocalizationOptions` | `LocalizerResourceDictionary Resources { get; set; } = new()` | 可用 POCO；模块**未**将其接入 options 系统，框架内无人消费 |
| `CikeLocalizationModule` | 模块类，`[DependsOn([])]`；`ConfigureServicesAsync` 中仅一行注册 | 见隐式行为 |

关键成员行为细节：

- `LocalizationResource` 构造时计算 `ResourceName = LocalizationResourceNameAttribute.GetName(resourceType)`；传入 `initialContributor` 则直接加入 `Contributors`。构造器内会重复 new `Contributors`/`BaseResourceNames`（覆盖属性初始化器，无行为差异）。
- `LocalizerResourceDictionary.Add(Type, ...)`：以 ResourceName 为键；重复添加抛 `ArgumentException("This resource is already added before: " + resourceType.AssemblyQualifiedName)`。`GetOrNull`/`GetOrAdd` 走内部 `Dictionary<Type, LocalizationResource>` 反查（`GetOrDefault` 扩展来自 `Cike.Core` 的 `CikeDictionaryExtensions`）。
- `GetName` 回退 `type.FullName!`：未标注特性的资源类以含命名空间的 FullName 作为资源名；不同命名空间的同名资源不会冲突，但同 `Name` 特性值会撞字典键。
- `LocalizedString`（`GetOrNull` 的返回类型）来自 `Microsoft.Extensions.Localization`，非本包定义。

### 隐式行为

1. **模块不注册 `IStringLocalizerFactory`**。`CikeLocalizationModule` 的唯一注册是 `context.Services.AddSingleton<ResourceManagerStringLocalizerFactory>()`——这是对 Microsoft 的 `ResourceManagerStringLocalizerFactory` 做**自类型单例注册**，没有映射 `IStringLocalizerFactory` 服务接口。宿主若未自行调用 `AddLocalization()`（或注册工厂接口映射），从 DI 解析 `IStringLocalizer`/`IStringLocalizerFactory` 会失败。
2. `CikeStringLocalizerFactory`、`CikeStringLocalizer` 在框架内任何位置都没有注册；即便手动解析后者，所有成员也是 `NotImplementedException`。
3. `CikeLocalizationOptions` 没有 `Configure<CikeLocalizationOptions>()` 接线；贡献者的 `Initialize` 不会被任何框架代码自动调用，调用时机完全由使用方决定（未验证任何隐式初始化路径）。
4. 模块加载走 `[DependsOn]` 后序遍历（模块系统细节见 [框架内核](./framework-core.md)）；`CikeLocalizationModule` 声明空依赖。本包程序集内没有任何类型实现 `IDependencyInjection`/`ITransientDependency` 等标记接口，因此模块加载不产生自动注册——`ILanguageProvider`、贡献者等必须业务方显式注册。
5. 模块注册的 `ResourceManagerStringLocalizerFactory`（自类型单例）行为即 Microsoft 默认实现（基于 .resx/ResourceManager），与 `CikeLocalizationOptions.Resources` 字典完全无关。

### 示例

以下为基于源码签名的最小可推理用法：自定义 `ILanguageProvider` + 资源贡献者注册。步骤 1–3 使用包内已实现 API；步骤 4–5 为业务方自行接线（`IStringLocalizer` 注入路径未实现，只能直接从 options 字典取词）。**整条链路未经框架端到端验证。**

```csharp
using Cike.Localization;
using Cike.Localization.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

// 1. 资源类：纯标记类，无成员要求；资源名默认取 FullName，可用特性显式命名
[LocalizationResourceName("Ordering")]
public class OrderingResource { }

// 2. 词条来源：实现 ILocalizationResourceContributor（示例为内存字典，可替换为 DB/远程拉取）
public class InMemoryContributor : ILocalizationResourceContributor
{
    private readonly Dictionary<(string Culture, string Name), string> _map = new()
    {
        [("zh-CN", "OrderNotFound")] = "订单不存在",
        [("en-US", "OrderNotFound")] = "Order not found",
    };

    public void Initialize(LocalizationResource resource, IServiceProvider serviceProvider)
    {
        // 初始化回调：可在此按 resource.ResourceName 预热词条。
        // 框架不会自动调用（调用时机需自行确认）。
    }

    public LocalizedString? GetOrNull(string cultureName, string name)
        => _map.TryGetValue((cultureName, name), out var value)
            ? new LocalizedString(name, value)
            : null;
}

// 3. 语言清单提供者：返回系统支持的语言列表（不是"当前语言"）
public class DefaultLanguageProvider : ILanguageProvider
{
    public IReadOnlyList<LanguageInfo> GetLanguageInfos() => new List<LanguageInfo>
    {
        new("zh-CN", "zh-CN", "简体中文"),
        new("en-US", "en-US", "English"),
    };
}

// 4. 注册（Program.cs 或宿主模块的 ConfigureServicesAsync）
services.AddSingleton<ILanguageProvider, DefaultLanguageProvider>();
services.Configure<CikeLocalizationOptions>(options =>
{
    var resource = options.Resources.Add(typeof(OrderingResource), defaultCultureName: "zh-CN");
    resource.Contributors.Add(new InMemoryContributor());
    // 若同类型可能被多处注册，用 GetOrAdd 幂等替代 Add（Add 重复注册抛 ArgumentException）
});

// 5. 取词：直接走 options 字典（IStringLocalizer 路径为 NotImplementedException 存根）
var options = serviceProvider.GetRequiredService<IOptions<CikeLocalizationOptions>>();
var resource = options.Value.Resources.GetOrNull(typeof(OrderingResource));
// cultureName 的来源（请求头/当前线程 Culture 等）由业务方自行决定，包内无此 API
var entry = resource?.Contributors
    .Select(c => c.GetOrNull("zh-CN", "OrderNotFound"))
    .FirstOrDefault(r => r != null);
string message = entry?.Value ?? "OrderNotFound"; // 无词条时的兜底策略由使用方定义
```

### 配置

| 项 | 说明 |
|---|---|
| `CikeLocalizationOptions.Resources` | 唯一配置项，类型 `LocalizerResourceDictionary`，默认空字典；经 `services.Configure<CikeLocalizationOptions>(...)` 填充（模块未预注册该 options，需自行确认宿主接线） |
| `LocalizationResource.DefaultCultureName` | 单资源级默认文化，构造器或属性设置；**包内无消费逻辑**，设置了不影响任何取词行为 |
| `LocalizationResource.BaseResourceNames` | 预留的基础资源继承列表，**包内从不填充、无消费逻辑** |
| appsettings 绑定 | 无内建支持；不通过配置文件注册资源/贡献者 |
| 模块引用 | 宿主模块以 `[DependsOn(typeof(CikeLocalizationModule))]` 引入；`CikeLocalizationModule` 自身 `[DependsOn([])]` |

### 边界与反模式

- **不要注入 `IStringLocalizer`/`IStringLocalizerFactory` 并期望走本包**：服务映射未注册，`CikeStringLocalizer`/`CikeStringLocalizerFactory` 是 `NotImplementedException` 存根。本包当前不能作为 ASP.NET Core 内建本地化的替换品。
- 不要假设 `ILanguageProvider` 提供"当前语言"：它只返回语言清单；当前 culture 需业务方自取（如 `CultureInfo.CurrentUICulture` 或请求头解析），包内无此 API。
- 不要期待贡献者被自动 `Initialize`、缓存或刷新：接口仅两个同步方法，所有调用时机由使用方决定；DB/远程来源的并发与缓存策略需自行实现（未验证）。
- `LocalizerResourceDictionary.Add` 对同一资源名/资源类型重复注册抛 `ArgumentException`；幂等场景用 `GetOrAdd`。
- 资源名冲突：两处使用相同的 `[LocalizationResourceName("X")]` 会在 `Add` 时撞键抛异常；字典键是 ResourceName 而非 Type。
- 宿主若已调用 `AddLocalization()`，DI 中的 `IStringLocalizerFactory` 是 Microsoft 默认 .resx 实现，与 `CikeLocalizationOptions.Resources` 无任何关联，二者不可混用。
- 反模式：为"以后可能要多语言"而提前引入本包——执行层未完成，引入后仍需自行实现取词链路；多语言需求真实出现前，沿用团队中文硬编码惯例是当前合理选择。
