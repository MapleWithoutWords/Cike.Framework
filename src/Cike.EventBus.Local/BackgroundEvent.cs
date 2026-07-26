namespace Cike.EventBus.Local;

public abstract record BackgroundEvent : LocalEvent
{
    public BackgroundEvent()
    {
        EnableBackgroundThread();
    }
}
