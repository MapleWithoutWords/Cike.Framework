﻿namespace Cike.Data.EFCore;

public abstract class CikeDbContext<TDbContext> : DbContext, IScopedDependency where TDbContext : DbContext
{
    private ILogger<TDbContext> _logger => _currentServiceProvider.GetRequiredService<ILogger<TDbContext>>();

    private IServiceProvider _currentServiceProvider;

    protected IServiceProvider CurrentServiceProvider
    {
        get
        {
            if (_currentServiceProvider == null)
            {
                throw new Exception("Please Use the constructor with IServiceProvider");
            }
            return _currentServiceProvider;
        }
    }

    protected ICurrentUser CurrentUser => CurrentServiceProvider.GetRequiredService<ICurrentUser>();
    protected ICurrentTenant CurrentTenant => CurrentServiceProvider.GetRequiredService<ICurrentTenant>();

    public IDataFilter DataFilter => CurrentServiceProvider.GetRequiredService<IDataFilter>();
    public IGuidGenerator GuidGenerator => CurrentServiceProvider.GetRequiredService<IGuidGenerator>();
    public ISnowflakeIdGenerator SnowflakeIdGenerator => CurrentServiceProvider.GetRequiredService<ISnowflakeIdGenerator>();
    public UnitOfWorkOptions UnitOfWorkOptions => CurrentServiceProvider.GetRequiredService<IOptions<UnitOfWorkOptions>>().Value;
    protected virtual bool IsMultiTenantFilterEnabled => DataFilter?.IsEnabled<IMultiTenant>() ?? false;

    protected virtual bool IsSoftDeleteFilterEnabled => DataFilter?.IsEnabled<ISoftDelete>() ?? false;
    private static readonly MethodInfo ConfigureBasePropertiesMethodInfo
        = typeof(CikeDbContext<TDbContext>)
            .GetMethod(
                nameof(ConfigureBaseProperties),
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;

    public CikeDbContext(DbContextOptions<TDbContext> options, IServiceProvider serviceProvider) : base(options.UseUow())
    {
        _currentServiceProvider = serviceProvider;
    }

    public CikeDbContext(DbContextOptions<TDbContext> options, IServiceProvider serviceProvider, bool isUow) : base(isUow ? options.UseUow() : options)
    {
        _currentServiceProvider = serviceProvider;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetAuditedProperty();

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void SetAuditedProperty()
    {
        foreach (var item in ChangeTracker.Entries())
        {
            if (item.State == EntityState.Added)
            {
                if (item.Entity is IEntity<long> longIdEntity && longIdEntity.Id <= 0)
                {
                    longIdEntity.Id = SnowflakeIdGenerator.NextId();
                }
                else if (item.Entity is IEntity<Guid> guidIdEntity && guidIdEntity.Id == Guid.Empty)
                {
                    guidIdEntity.Id = GuidGenerator.Create();
                }
                if (item.Entity is IAuditedEntity<long> longAuditedEntity)
                {
                    long.TryParse(CurrentUser.Id, out var userId);
                    longAuditedEntity.CreateUserId = userId;
                    longAuditedEntity.CreateTime = DateTime.Now;
                    longAuditedEntity.UpdateTime = DateTime.Now;
                    longAuditedEntity.UpdateUserId = userId;
                }
                else if (item.Entity is IAuditedEntity<Guid> guidAuditedEntity)
                {
                    Guid.TryParse(CurrentUser.Id, out var userId);
                    guidAuditedEntity.CreateUserId = userId;
                    guidAuditedEntity.CreateTime = DateTime.Now;
                    guidAuditedEntity.UpdateTime = DateTime.Now;
                    guidAuditedEntity.UpdateUserId = userId;
                }
            }
            else if (item.State == EntityState.Modified)
            {
                if (item.Entity is IAuditedEntity<long> longAuditedEntity)
                {
                    long.TryParse(CurrentUser.Id, out var userId);
                    longAuditedEntity.UpdateTime = DateTime.Now;
                    longAuditedEntity.UpdateUserId = userId;
                }
                else if (item.Entity is IAuditedEntity<Guid> guidAuditedEntity)
                {
                    Guid.TryParse(CurrentUser.Id, out var userId);
                    guidAuditedEntity.UpdateTime = DateTime.Now;
                    guidAuditedEntity.UpdateUserId = userId;
                }

            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetAuditedProperty();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public virtual void SoftDelete<TEntity>(TEntity entity) where TEntity : ISoftDelete
    {
        entity.IsDeleted = true;
        Update(entity);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            ConfigureBasePropertiesMethodInfo
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, new object[] { modelBuilder, entityType });
        }
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
        base.OnConfiguring(optionsBuilder);
    }
    protected virtual void ConfigureBaseProperties<TEntity>(ModelBuilder modelBuilder, IMutableEntityType mutableEntityType)
        where TEntity : class
    {
        if (mutableEntityType.IsOwned())
        {
            return;
        }

        if (!typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            return;
        }

        modelBuilder.Entity<TEntity>().ConfigureByConvention();

        ConfigureGlobalFilters<TEntity>(modelBuilder, mutableEntityType);
    }

    protected virtual void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder, IMutableEntityType mutableEntityType)
        where TEntity : class
    {
        if (mutableEntityType.BaseType == null && ShouldFilterEntity<TEntity>(mutableEntityType))
        {
            var filterExpression = CreateFilterExpression<TEntity>();
            if (filterExpression != null)
            {
                HasCikeQueryFilter(modelBuilder.Entity<TEntity>(), filterExpression);
            }
        }
    }
    protected virtual bool ShouldFilterEntity<TEntity>(IMutableEntityType entityType) where TEntity : class
    {
        if (typeof(IMultiTenant).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }

        return false;
    }
    protected virtual Expression<Func<TEntity, bool>>? CreateFilterExpression<TEntity>()
        where TEntity : class
    {
        Expression<Func<TEntity, bool>>? expression = null;

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            expression = e => !IsSoftDeleteFilterEnabled || !EF.Property<bool>(e, "IsDeleted");
        }

        if (typeof(IMultiTenant).IsAssignableFrom(typeof(TEntity)))
        {
            Expression<Func<TEntity, bool>> multiTenantFilter = e => !IsMultiTenantFilterEnabled || EF.Property<Guid>(e, "TenantId") == CurrentUser.TenantId;
            expression = expression == null ? multiTenantFilter : QueryFilterExpressionHelper.CombineExpressions(expression, multiTenantFilter);
        }

        return expression;
    }

    /// <summary>
    /// This method is used to add a query filter to this entity which combine with ABP EF Core builtin query filters.
    /// </summary>
    /// <returns></returns>
    public static EntityTypeBuilder<TEntity> HasCikeQueryFilter<TEntity>(EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, bool>> filter)
        where TEntity : class
    {
#pragma warning disable EF1001
        var queryFilterAnnotation = builder.Metadata.FindAnnotation(CoreAnnotationNames.QueryFilter);
#pragma warning restore EF1001
        if (queryFilterAnnotation != null && queryFilterAnnotation.Value != null && queryFilterAnnotation.Value is Expression<Func<TEntity, bool>> existingFilter)
        {
            filter = QueryFilterExpressionHelper.CombineExpressions(filter, existingFilter);
        }

        return builder.HasQueryFilter(filter);
    }

    public override EntityEntry Add(object entity)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entity));
        return base.Add(entity);
    }

    public override EntityEntry<TEntity> Add<TEntity>(TEntity entity)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entity));
        return base.Add(entity);
    }

    public override async ValueTask<EntityEntry> AddAsync(object entity, CancellationToken cancellationToken = default)
    {
        await BeginUnitOfWorkAsync(entity);
        return await base.AddAsync(entity, cancellationToken);
    }

    public override async ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
    {
        await BeginUnitOfWorkAsync(entity);
        return await base.AddAsync(entity, cancellationToken);
    }

    public override void AddRange(IEnumerable<object> entities)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entities));
        base.AddRange(entities);
    }

    public override void AddRange(params object[] entities)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entities));
        base.AddRange(entities);
    }

    public override async Task AddRangeAsync(IEnumerable<object> entities, CancellationToken cancellationToken = default)
    {
        await BeginUnitOfWorkAsync(entities);
        await base.AddRangeAsync(entities, cancellationToken);
    }

    public override async Task AddRangeAsync(params object[] entities)
    {
        await BeginUnitOfWorkAsync(entities);
        await base.AddRangeAsync(entities);
    }

    public override EntityEntry Update(object entity)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entity));
        return base.Update(entity);
    }

    public override EntityEntry<TEntity> Update<TEntity>(TEntity entity)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entity));
        return base.Update(entity);
    }

    public override void UpdateRange(IEnumerable<object> entities)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entities));
        base.UpdateRange(entities);
    }

    public override void UpdateRange(params object[] entities)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entities));
        base.UpdateRange(entities);
    }

    public override EntityEntry Remove(object entity)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entity));
        return base.Remove(entity);
    }

    public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entity));
        return base.Remove(entity);
    }

    public override void RemoveRange(IEnumerable<object> entities)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entities));
        base.RemoveRange(entities);
    }

    public override void RemoveRange(params object[] entities)
    {
        AsyncContext.Run(() => BeginUnitOfWorkAsync(entities));
        base.RemoveRange(entities);
    }

    private async Task BeginUnitOfWorkAsync(params object[] entities)
    {
        if (!UnitOfWorkOptions.Enable)
        {
            return;
        }
        if (entities?.Any() != true)
        {
            return;
        }
        var unitOfWork = CurrentServiceProvider?.GetService<IUnitOfWork>()!;
        if (!unitOfWork.IsTransactionBegun)
        {
            await unitOfWork.BeginTranscationAsync();
        }
    }

    private IServiceProvider GetServiceProvider()
    {
        return CurrentServiceProvider!;
    }
}
