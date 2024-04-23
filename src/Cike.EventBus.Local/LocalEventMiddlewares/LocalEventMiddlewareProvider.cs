using Cike.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.EventBus.Local.LocalEventMiddlewares;

public class LocalEventMiddlewareProvider : ILocalEventMiddlewareProvider, IScopedDependency
{
    private readonly IServiceProvider _serviceProvider;
    private int _publishTimes = 0;

    public LocalEventMiddlewareProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<ILocalEventMiddleware<TEvent>> GetEventMiddlewares<TEvent>() where TEvent : IEvent
    {
        var eventMiddlewares = _serviceProvider.GetRequiredService<IEnumerable<ILocalEventMiddleware<TEvent>>>();

        if (_publishTimes++ > 0)
        {
            eventMiddlewares = eventMiddlewares.Where(middleware => !middleware.PreventRecursive);
        }

        return eventMiddlewares;
    }

    public void Stop()
    {
        _publishTimes = 0;
    }
}
