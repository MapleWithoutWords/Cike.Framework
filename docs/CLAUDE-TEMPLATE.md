<!-- ────────────────────────────────────────────────────────────────
  使用说明（人类读者，部署后可删除本注释块）：
  1. 新建项目后，把本文件复制到解决方案根目录，重命名为 CLAUDE.md
  2. 替换所有【填】标记；删除所有不适用的段落
  3. 第 2 节的框架指南路径二选一：填本机 Cike.Framework 仓库路径，
     或把 AI-GUIDE.md 拷贝进项目（如 docs/ai-framework-guide.md）后填相对路径
  4. 项目约定的"偏离点"段落很重要：AI 默认按框架指南工作，
     项目与指南不一致的地方必须在这里逐条声明，否则 AI 会按指南纠正你的项目
──────────────────────────────────────────────────────────────── -->

# CLAUDE.md

## 1. 项目身份

- 项目：【填：项目名，如 Cike.Workflow】——【填：一句话业务定位】
- 技术栈：.NET 8 + Cike.Framework（模块化 DDD 分层框架）
- 解决方案分层遵循框架指南第 2 节（Domain.Shared / Domain / Application.Contracts / Application / EntityFrameworkCore / Service.Open 【填：按实际增删，如 Caching、Common】）

## 2. 框架指南（AI 开工前必读）

本项目基于 **Cike.Framework** 开发。框架的全部机制（模块系统、自动 DI、CQRS 事件派发、MinimalAPI 路由约定、数据访问自动化、隐式行为与反模式）都写在：

**【填：AI-GUIDE.md 的路径】**

AI 开始写任何代码前，必须先读该指南，特别是：
- 第 5-6 节（CQRS Handler 写法、路由生成规则）——决定你写的每个文件长什么样
- 第 9 节 Recipe——新增业务功能时照此复制
- 第 10-11 节（隐式行为清单、反模式）——禁止违反

**冲突裁决**：本文件与框架指南冲突时，以本文件为准（本文件代表项目实际情况）；指南与代码冲突时，以代码为准并向用户报告。

## 3. 项目硬事实

| 事项 | 值 |
|---|---|
| 启动模块 | 【填：如 CikeWorkflowServiceOpenModule】 |
| 数据库 | 【填：MySQL / SqlServer】（Provider 模块：`CikeDataEFCoreMySqlModule` / `CikeDataEFCoreSqlServerModule`） |
| DbContext | 【填：类名，如 CikeWorkflowDbContext；连接串 key = 类名去掉 DbContext 后缀，注意核对 7.3 节规则】 |
| 多租户 | 【填：启用 / 不启用。启用则：实体必须实现 IMultiTenant + 宿主模块 InitializeAsync 必须调用 app.UseMultiTenant()】 |
| 缓存 | 【填：Redis 配置节 RedisConfig / 无】 |
| 认证 | 【填：JWT（Claims 结构、策略名）/ 开发期关闭（GlobalMinimalApiRouteOptions.EnabledAuthorization=false）】 |
| 全局 usings | 每个项目根有 `_Imports.cs`；新命名空间若频繁引用先加到这里，不要在文件里散写 |

## 4. 常用命令

```bash
# 构建
dotnet build 【填：解决方案名】
# 运行宿主
dotnet run --project src/【填：*.Service.Open 项目目录】
# 数据库迁移（在 EntityFrameworkCore 项目目录下执行）
dotnet ef migrations add <MigrationName>
dotnet ef database update
# 测试
【填：如有】
```

## 5. 项目约定与指南的偏离点

【以下逐条列出本项目与框架指南不一致的地方。没有偏离则保留这一行说明。示例格式：】

- 【示例】本项目的 DbContext 类名历史拼写为 `CikeWorkflowDbContenxt`（Contenxt 少了 x）——连接串 key 必须写完整的 `CikeWorkflowDbContenxt`，AI 写新 DbContext 时**不要**沿用此拼写
- 【示例】Application 层 Handler 命名统一用正确拼写 `XxxCommandHandler`；看到存量代码里的 `Hanlder` 属于拼写错误，不做批量修正，但新代码禁止模仿
- 【示例】项目的 Store 不走缓存（无 `*.Caching` 项目），新增聚合直接继承 `BaseStore<TEntity>` 而非 `BaseStore<TEntity, TCacheModel>`
- 【填：项目自己的规则……】

## 6. 工作规则

1. 新增业务功能：严格按框架指南第 9 节 Recipe 的文件结构执行，不自创结构
2. 改动跨层时，自上而下检查调用链各环节的约定（Service 方法名前缀→动词、Handler 的 `[LocalEventHandler]`、Store 接口）
3. 提交前自查：指南第 9 节"验收清单"全部通过；无反模式清单（第 11 节）所列行为
4. 拿不准框架行为时，先查指南第 10 节隐式行为清单；指南没有的，读框架源码 `src/` 对应项目后写代码，不要猜
5. 【填：项目特有的流程要求，如：提交前跑 lint / 遵循某种分支策略……】
