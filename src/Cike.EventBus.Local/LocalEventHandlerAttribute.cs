namespace Cike.EventBus.LocalEvent;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LocalEventHandlerAttribute : Attribute
{
    public int Order { get; set; }

    public bool IsCancel { get; set; }

    public int RetryCount { get; set; }

    public LocalEventHandlerAttribute(int order = 99)
    {
        Order = order;
    }
}
