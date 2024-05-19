using Cike.Uow;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Nito.AsyncEx;
using System.Reflection;

namespace Cike.Data.EFCore.Internal;

#pragma warning disable EF1001
public class UowDbSet<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes)] TEntity> : InternalDbSet<TEntity> where TEntity : class
{

    internal const DynamicallyAccessedMemberTypes DynamicallyAccessedMemberTypes =
        System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicConstructors
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicConstructors
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicProperties
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicFields
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicProperties
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicFields
        | System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.Interfaces;

    private static readonly MethodInfo GetServiceProvider
        = typeof(CikeDbContext<>)
            .GetMethod(
                nameof(GetServiceProvider),
                BindingFlags.Instance | BindingFlags.NonPublic
            )!;


    private IUnitOfWork? _unitOfWork;

    public UowDbSet(DbContext context, string? entityTypeName) : base(context, entityTypeName)
    {
        var serviceProvider = (IServiceProvider)GetServiceProvider.Invoke(context, Array.Empty<object>())!;
        _unitOfWork = serviceProvider.GetService<IUnitOfWork>();
    }

    public override EntityEntry<TEntity> Add(TEntity entity)
    {
        SetUnitOfWorkTracking(entity);
        return base.Add(entity);
    }

    public override ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity,
        CancellationToken cancellationToken = default)
    {
        SetUnitOfWorkTracking(entity);
        return base.AddAsync(entity, cancellationToken);
    }

    public override void AddRange(params TEntity[] entities)
    {
        SetUnitOfWorkTracking(entities);
        base.AddRange(entities);
    }

    public override void AddRange(IEnumerable<TEntity> entities)
    {
        SetUnitOfWorkTracking(entities);
        base.AddRange(entities);
    }

    public override Task AddRangeAsync(params TEntity[] entities)
    {
        SetUnitOfWorkTracking(entities);
        return base.AddRangeAsync(entities);
    }

    public override Task AddRangeAsync(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = new CancellationToken())
    {
        SetUnitOfWorkTracking(entities);
        return base.AddRangeAsync(entities, cancellationToken);
    }

    public override EntityEntry<TEntity> Update(TEntity entity)
    {
        SetUnitOfWorkTracking(entity);
        return base.Update(entity);
    }

    public override void UpdateRange(IEnumerable<TEntity> entities)
    {
        SetUnitOfWorkTracking(entities);
        base.UpdateRange(entities);
    }

    public override void UpdateRange(params TEntity[] entities)
    {
        SetUnitOfWorkTracking(entities);
        base.UpdateRange(entities);
    }

    public override EntityEntry<TEntity> Remove(TEntity entity)
    {
        SetUnitOfWorkTracking(entity);
        return base.Remove(entity);
    }

    public override void RemoveRange(params TEntity[] entities)
    {
        SetUnitOfWorkTracking(entities);
        base.RemoveRange(entities);
    }

    public override void RemoveRange(IEnumerable<TEntity> entities)
    {
        SetUnitOfWorkTracking(entities);
        base.RemoveRange(entities);
    }

    private void SetUnitOfWorkTracking(params object[] entities)
    {
        var unitOfWork = _unitOfWork;
        if (unitOfWork == null)
            return;

        AsyncContext.Run(async () =>await unitOfWork.BeginTranscationAsync());
    }
}
#pragma warning restore EF1001
