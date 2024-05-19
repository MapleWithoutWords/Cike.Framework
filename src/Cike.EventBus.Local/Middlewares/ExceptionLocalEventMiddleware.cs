using Cike.Core.DependencyInjection;
using Cike.EventBus.Local.Enums;
using Cike.EventBus.Local.LocalEventMiddlewares;
using Cike.EventBus.LocalEvent;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.ExceptionServices;

namespace Cike.EventBus.Local.Middlewares;

public class ExceptionLocalEventMiddleware<TEvent> : ILocalEventMiddleware<TEvent>
    where TEvent : IEvent
{
    private readonly Lazy<ILocalEventExecutor> _executeProviderLazy;
    private readonly Lazy<ILocalEventBus> _localEventBusLazy;
    private ILocalEventBus LocalEventBus => _localEventBusLazy.Value;

    public bool PreventRecursive => true;

    public ExceptionLocalEventMiddleware(IServiceProvider serviceProvider)
    {
        _executeProviderLazy = new Lazy<ILocalEventExecutor>(serviceProvider.GetRequiredService<ILocalEventExecutor>);
        _localEventBusLazy = new Lazy<ILocalEventBus>(serviceProvider.GetRequiredService<ILocalEventBus>);
    }

    public async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
    {
        try
        {
            _executeProviderLazy.Value.Reset();

            await next();
        }
        catch (Exception exception)
        {
            _executeProviderLazy.Value.Exception = exception;

            if (_executeProviderLazy.Value.Status == ExecutorStatusEnum.InProgress)
            {
                _executeProviderLazy.Value.Status = ExecutorStatusEnum.Failed;
            }
            else if (_executeProviderLazy.Value.Status == ExecutorStatusEnum.Succeed)
            {
                await LocalEventBus.CancelAsync(@event, default);
            }

            ExceptionDispatchInfo.Capture(exception).Throw();
        }
        finally
        {
            _executeProviderLazy.Value.Counter = 0;
        }
    }
}
