namespace Cike.Data.EFCore.Uow;

public class EFCoreUnitOfWork<TDbContext>(IServiceProvider _serviceProvider) : IUnitOfWork where TDbContext : CikeDbContext<TDbContext>
{
    public Guid TransactionId { get; set; }

    private DbContext? _dbContext;

    public DbContext DbContext
    {
        get
        {
            return _dbContext ??= _serviceProvider.GetRequiredService<TDbContext>();
        }
        private set
        {
            _dbContext = value;
        }
    }

    public bool IsTransactionBegun { get; set; }

    public IDbTransaction DbTransaction { get => DbContext.Database.CurrentTransaction!.GetDbTransaction(); }

    public UnitOfWorkCommitState CommitState { get; set; }

    public async Task BeginTranscationAsync(IsolationLevel? isolationLevel, CancellationToken cancellationToken = default)
    {
        if (IsTransactionBegun)
        {
            return;
        }
        IDbContextTransaction transaction = isolationLevel.HasValue ? await DbContext.Database.BeginTransactionAsync(isolationLevel.Value, cancellationToken) : await DbContext.Database.BeginTransactionAsync(cancellationToken);
        IsTransactionBegun = true;
        TransactionId = transaction.TransactionId;
        CommitState = UnitOfWorkCommitState.Uncommitted;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (CommitState == UnitOfWorkCommitState.Uncommitted)
        {
            await DbContext.SaveChangesAsync(cancellationToken);
            await DbContext.Database.CommitTransactionAsync(cancellationToken);
            CommitState = UnitOfWorkCommitState.Committed;
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (CommitState == UnitOfWorkCommitState.Uncommitted)
        {
            await DbContext.Database.RollbackTransactionAsync(cancellationToken);
            CommitState = UnitOfWorkCommitState.Rollbacked;
        }
    }
}
