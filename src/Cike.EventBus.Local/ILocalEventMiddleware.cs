namespace Cike.EventBus.Local;

public interface ILocalEventMiddleware<TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent @event);

    /// <summary>
    /// Whether IEventMiddleware prevent each execution when EventBus is nested
    /// </summary>
    bool PreventRecursive { get; }
}
