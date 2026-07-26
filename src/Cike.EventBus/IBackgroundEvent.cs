namespace Cike.EventBus;

public interface IBackgroundEvent
{
    public bool IsBackgroundThread();

    public void EnableBackgroundThread();
}
