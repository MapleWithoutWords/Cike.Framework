using Cike.Data;

namespace Cike.Domain.Entities;

public class FullAuditedJsonEntity<TKey, TUserId, TJson> : FullAuditedEntity<TKey, TUserId>, IJsonEntity<TJson>, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    public TJson JsonProperty { get; set; } = default!;
}
public class FullAuditedJsonEntity<TKey, TJson> : FullAuditedEntity<TKey>, IJsonEntity<TJson>, IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    public TJson JsonProperty { get; set; } = default!;
}
