namespace Cike.Data.EFCore;

public abstract class CikeDbContext<TDbContext> : DbContext, IScopedDependency where TDbContext : DbContext
{
    private static readonly MethodInfo ConfigureBasePropertiesMethodInfo
        = typeof(CikeDbContext<TDbContext>)
            .GetMethod(
                nameof(ConfigureBaseProperties),
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;

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

    public IDataFilter DataFilter => CurrentServiceProvider.GetRequiredService<IDataFilter>();

    internal EntityHelper EntityHelper => CurrentServiceProvider.GetRequiredService<EntityHelper>();

    public UnitOfWorkOptions UnitOfWorkOptions => CurrentServiceProvider.GetRequiredService<IOptions<UnitOfWorkOptions>>().Value;

    protected virtual bool IsMultiTenantFilterEnabled => DataFilter?.IsEnabled<IMultiTenant>() ?? false;

    protected virtual bool IsSoftDeleteFilterEnabled => DataFilter?.IsEnabled<ISoftDelete>() ?? false;

    public CikeDbContext(DbContextOptions<TDbContext> options, IServiceProvider serviceProvider) : base(options)
    {
        _currentServiceProvider = serviceProvider;

        ChangeTracker.Tracked += ChangeTracker_Tracked;
        ChangeTracker.StateChanged += ChangeTracker_StateChanged;
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        HandlePropertiesBeforeSave();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        HandlePropertiesBeforeSave();
        return base.SaveChanges(acceptAllChangesOnSuccess);
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
            Expression<Func<TEntity, bool>> multiTenantFilter = e => !IsMultiTenantFilterEnabled || EF.Property<long>(e, "TenantId") == CurrentUser.TenantId;
            expression = expression == null ? multiTenantFilter : QueryFilterExpressionHelper.CombineExpressions(expression, multiTenantFilter);
        }

        return expression;
    }

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

    private void HandlePropertiesBeforeSave()
    {
        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            //EntityHelper.SetCreateAuditedProperty(entry);
            if (entry.State.IsIn(EntityState.Modified, EntityState.Deleted))
            {
                UpdateConcurrencyStamp(entry);
            }
        }
    }

    protected virtual void ChangeTracker_Tracked(object? sender, EntityTrackedEventArgs e)
    {
        PublishEventsForTrackedEntity(e.Entry);
    }

    protected virtual void ChangeTracker_StateChanged(object? sender, EntityStateChangedEventArgs e)
    {
        PublishEventsForTrackedEntity(e.Entry);
    }

    protected virtual void PublishEventsForTrackedEntity(EntityEntry entry)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                AsyncContext.Run(() => BeginUnitOfWorkAsync(entry.Entity));
                EntityHelper.TrySetId(entry);
                EntityHelper.SetCreateAuditedProperty(entry);
                break;

            case EntityState.Modified:
                AsyncContext.Run(() => BeginUnitOfWorkAsync(entry.Entity));
                EntityHelper.SetUpdateAuditedProperty(entry);
                break;

            case EntityState.Deleted:
                AsyncContext.Run(() => BeginUnitOfWorkAsync(entry.Entity));
                EntityHelper.SetDeleteAuditedProperty(entry);
                break;
        }
    }

    protected async Task BeginUnitOfWorkAsync(params object[] entities)
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

    protected void UpdateConcurrencyStamp(EntityEntry entry)
    {
        var entity = entry.Entity as IHasConcurrencyStamp;
        if (entity == null)
        {
            return;
        }

        Entry(entity).Property(x => x.ConcurrencyStamp).OriginalValue = entity.ConcurrencyStamp;
        entity.ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }
}
