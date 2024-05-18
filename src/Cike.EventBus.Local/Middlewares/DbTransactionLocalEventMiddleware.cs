using Cike.Core.DependencyInjection;
using Cike.EventBus.Local.LocalEventMiddlewares;
using Cike.Uow;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.EventBus.Local.Middlewares;

public class DbTransactionLocalEventMiddleware<TEvent> : ILocalEventMiddleware<TEvent>, ITransientDependency
    where TEvent : IEvent
{
    private IUnitOfWork _unitOfWork;

    public bool PreventRecursive => true;

    public DbTransactionLocalEventMiddleware(IServiceProvider serviceProvider)
    {
        _unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
    }

    public async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
    {
        try
        {
            await next();

            await _unitOfWork.CommitAsync();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
