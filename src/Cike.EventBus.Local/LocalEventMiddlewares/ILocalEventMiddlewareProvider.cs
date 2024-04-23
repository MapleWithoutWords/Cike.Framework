namespace Cike.EventBus.Local.LocalEventMiddlewares;

public interface ILocalEventMiddlewareProvider
{
    IEnumerable<ILocalEventMiddleware<TEvent>> GetEventMiddlewares<TEvent>()
        where TEvent : IEvent;

    void Stop();
}
