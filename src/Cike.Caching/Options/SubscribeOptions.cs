namespace Cike.Caching.Options;

public class SubscribeOptions<T> : PubSubOptionsBase
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Is it a publisher client
    /// </summary>
    public bool IsPublisherClient { get; set; }

    public SubscribeOptions(Guid uniquelyIdentifies) : base(uniquelyIdentifies)
    {
    }
}
