using Cike.EventBus.Local.LocalEventMiddlewares;

namespace Cike.EventBus.Local.Middlewares;

public class PreventRecursiveMiddleware<TEvent> : ILocalEventMiddleware<TEvent>
where TEvent : IEvent
{
    public bool PreventRecursive => true;

    private readonly ILocalEventMiddlewareProvider _preventRecursiveProvider;

    public PreventRecursiveMiddleware(ILocalEventMiddlewareProvider preventRecursiveProvider)
    {
        _preventRecursiveProvider = preventRecursiveProvider;
    }

    public async Task HandleAsync(TEvent @event, EventHandlerDelegate next)
    {
        try
        {
            await next();
        }
        finally
        {
            _preventRecursiveProvider.Stop();
        }
    }
}
