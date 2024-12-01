namespace Cike.Caching.Options;

public class PublishOptions : PubSubOptionsBase
{
    public object? Value { get; set; }

    public PublishOptions(Guid uniquelyIdentifies) : base(uniquelyIdentifies)
    {
    }
}
