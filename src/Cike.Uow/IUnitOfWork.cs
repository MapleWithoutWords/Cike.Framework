using System.Data;
using System.Data.Common;

namespace Cike.Uow;

public interface IUnitOfWork
{
    Guid TransactionId { get; }

    DbTransaction DbTransaction { get; }

    bool IsTransactionBegun { get; }

    Task BeginTranscationAsync(IsolationLevel? isolationLevel, CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
