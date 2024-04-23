namespace Cike.EventBus.Local.LocalEventMiddlewares;

public delegate Task EventHandlerDelegate();
public interface ILocalEventMiddleware<TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event, EventHandlerDelegate next);

    /// <summary>
    /// Whether IEventMiddleware prevent each execution when EventBus is nested
    /// </summary>
    bool PreventRecursive { get; }
}
