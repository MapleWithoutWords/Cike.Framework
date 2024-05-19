using Cike.Auth;
using Cike.Core.DependencyInjection;
using Cike.Core.Modularity;
using Cike.Data.DataFilters;
using Cike.Data.EFCore.Extensions;
using Cike.Data.Guids;
using Cike.Domain.Entities;
using Cike.Uow;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System.Linq.Expressions;
using System.Reflection;

namespace Cike.Data.EFCore;

public abstract class CikeDbContext<TDbContext> : DbContext, IScopedDependency where TDbContext : DbContext
{
    private static IServiceProvider? _rootServiceProvider;

    private bool _isInitialized;
    private ILogger<TDbContext>? _logger;
    private IServiceProvider? _currentServiceProvider;

    protected IServiceProvider? CurrentServiceProvider
    {
        get
        {
            if (_isInitialized)
                return _currentServiceProvider;

            if (_currentServiceProvider == null)
            {
                _rootServiceProvider ??= ModuleLoader.Services.BuildServiceProvider();
                _logger = _rootServiceProvider.GetService<ILogger<TDbContext>>();
                _currentServiceProvider = _rootServiceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.RequestServices;
            }

            _isInitialized = true;
            return _currentServiceProvider;
        }
    }

    protected CurrentUserContext CurrentUser => CurrentServiceProvider!.GetRequiredService<CurrentUserContext>();

    public IDataFilter DataFilter => CurrentServiceProvider!.GetRequiredService<IDataFilter>();
    public IGuidGenerator GuidGenerator => CurrentServiceProvider!.GetRequiredService<IGuidGenerator>();
    protected virtual bool IsMultiTenantFilterEnabled => DataFilter?.IsEnabled<IMultiTenant>() ?? false;

    protected virtual bool IsSoftDeleteFilterEnabled => DataFilter?.IsEnabled<ISoftDelete>() ?? false;
    private static readonly MethodInfo ConfigureBasePropertiesMethodInfo
        = typeof(CikeDbContext<TDbContext>)
            .GetMethod(
                nameof(ConfigureBaseProperties),
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;
    public CikeDbContext(DbContextOptions<TDbContext> options) : base(options)
    {
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
            if (item.Entity is IAuditedEntity<Guid> auditedEntity)
            {
                if (item.State == EntityState.Added)
                {
                    auditedEntity.CreateUserId = CurrentUser.Id ?? Guid.Empty;
                    auditedEntity.CreateTime = auditedEntity.CreateTime != default ? auditedEntity.CreateTime : DateTime.Now;
                    auditedEntity.UpdateTime = auditedEntity.UpdateTime != default ? auditedEntity.UpdateTime : DateTime.Now;
                    auditedEntity.UpdateUserId = Guid.Empty;
                    if (item.Entity is IEntity<Guid> guidEntity && guidEntity.Id == Guid.Empty)
                    {
                        guidEntity.Id = GuidGenerator.Create();
                    }
                }
                else if (item.State == EntityState.Modified)
                {
                    auditedEntity.UpdateTime = auditedEntity.UpdateTime != default ? auditedEntity.UpdateTime : DateTime.Now;
                    auditedEntity.UpdateUserId = CurrentUser.Id ?? Guid.Empty;
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
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
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
}
