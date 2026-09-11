namespace Cike.Data.EFCore.Repositories;

/// <summary>
/// EF Core 仓储的底层 DbContext 访问器：供扩展方法（GetQueryableAsync / GetDbContextAsync）
/// 获取仓储背后的 DbContext，规避开放泛型的模式匹配问题。
/// </summary>
public interface IEfCoreRepositoryAccessor
{
    DbContext DbContext { get; }
}
