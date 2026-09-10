# Cike.MultiTenant（已废弃 / 勿用）

⚠️ **此项目是历史遗留，未参与编译，未在解决方案中引用，不要在新项目中使用。**

具体问题：

1. `CikeMultiTenantModule` 是普通空类，**没有继承 `CikeModule`**——不是合法模块
2. 它的 `ICurrentTenant` 使用 **`Guid?`** 类型的租户 Id，而框架已全面转向 `long`（`IMultiTenant.TenantId`、`CikeClaimTypes.TenantId`）
3. 其类型写的命名空间是 `Cike.Auth.MultiTenant`，与 `Cike.Auth` 包里**现行的同名 `ICurrentTenant`（long）直接冲突**——同时引用两个包会编译冲突

现行多租户能力全部在 `Cike.Auth`：`ICurrentTenant`（long）、`TenantMiddleware`、`CikeClaimTypes`。详见 [Cike.Auth](../Cike.Auth/README.md)。

建议：直接删除本目录。
