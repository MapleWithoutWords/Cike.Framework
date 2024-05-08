using Cike.Uow.Enums;
using System.Data;
using System.Data.Common;

namespace Cike.Uow;

public interface IUnitOfWork
{
    Guid TransactionId { get; }

    IDbTransaction DbTransaction { get; }

    bool IsTransactionBegun { get; }
    UnitOfWorkCommitState CommitState { get; }

    Task BeginTranscationAsync(IsolationLevel? isolationLevel=default, CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
