using Cike.Core.DependencyInjection;
using Cike.Uow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.Common;

namespace Cike.Data.EFCore.Uow;

public class EFCoreUnitOfWork<TDbContext>(IServiceProvider _serviceProvider) : IUnitOfWork, ITransientDependency where TDbContext : CikeDbContext<TDbContext>
{
    public Guid TransactionId { get; set; }

    public DbTransaction DbTransaction { get; set; }

    public bool IsTransactionBegun { get; set; }

    public async Task BeginTranscationAsync(IsolationLevel? isolationLevel, CancellationToken cancellationToken = default)
    {
        if (IsTransactionBegun)
        {
            return;
        }
        var dbcontext = _serviceProvider.GetRequiredService<TDbContext>();
        _transaction = isolationLevel.HasValue ? await dbcontext.Database.BeginTransactionAsync(isolationLevel.Value, cancellationToken) : await dbcontext.Database.BeginTransactionAsync(cancellationToken);
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
