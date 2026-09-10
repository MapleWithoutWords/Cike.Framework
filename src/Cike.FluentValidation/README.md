# Cike.FluentValidation

参数校验自动化：Validator 自动注册 + MinimalAPI 端点的 FluentValidation 自动校验。

## 提供的能力

- **`[AutoValidation]`**（在 `Cike.AspNetCore.MinimalAPIs` 定义的端点过滤器特性）：标注在 `MinimalApiServiceBase` 子类/方法上，请求参数自动校验
- `AutoFluentValidationEndpointFilter`：参数中任何**有对应 Validator** 的对象自动校验；失败返回 **400 ValidationProblem**（ProblemDetails 格式：`{"errors": {属性: [消息数组]}}`）
- `CikeFluentValidationModule`：模块加载时**自动对全部已加载模块程序集调用 `AddValidatorsFromAssembly`**

## 隐式行为与陷阱

- 依赖本模块后**不要再手写** `AddValidatorsFromAssembly`——会重复注册
- 校验对象是 **Dto**（`AddTodoDto`），不是 Command；Validator 命名 `XxxDtoValidator`
- 校验失败响应（ProblemDetails）与 `UserFriendlyException` 的纯文本 400 是**两种响应形状**，前端要分别处理

## 模块信息

- 模块类：`CikeFluentValidationModule`
- 直接依赖：无

## 更多

校验规范与示例：[AI 开发指南](../../docs/AI-GUIDE.md) 第 8.2 节。
