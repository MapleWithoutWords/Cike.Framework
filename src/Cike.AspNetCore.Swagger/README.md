# Cike.AspNetCore.Swagger

Swagger/SwaggerUI 的约定式封装。两个扩展方法即完成装配。

## 提供的能力

| 扩展 | 做了什么 |
|---|---|
| `services.AddCikeSwagger("项目名", options => ...)` | Swagger 文档（Title=项目名）+ **Bearer JWT 安全定义**（调试时粘贴 token 即可）+ 自动包含输出目录全部 XML 注释 + 多态支持（`UseAllOfForInheritance` / `UseOneOfForPolymorphism`）+ 枚举字段中文描述（`EnumDescriptionSupplementSchemaFilter`）+ `SupportNonNullableReferenceTypes` 等可选配置 |
| `app.UseCikeSwaggerUI("项目名")` | `UseSwagger().UseSwaggerUI`（endpoint `/swagger/v1/swagger.json`） |

典型用法（宿主模块）：

```csharp
// ConfigureServicesAsync
context.Services.AddCikeSwagger("Cike");
// InitializeAsync
context.GetApplicationBuilder().UseCikeSwaggerUI("Cike");
```

想让枚举注释出现在文档里：枚举成员加 `/// <summary>` XML 注释（需项目开启 XML 文档输出）。

## 模块信息

- 模块类：`CikeAspNetCoreSwaggerModule`（无逻辑——两个扩展方法即全部）
- 直接依赖：无

## 更多

宿主模块标准装配：[AI 开发指南](../../docs/AI-GUIDE.md) 第 6.4 节。
