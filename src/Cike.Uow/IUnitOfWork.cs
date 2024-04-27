namespace Cike.Uow;

public interface IUnitOfWork
{
    Task BeginTranscationAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
