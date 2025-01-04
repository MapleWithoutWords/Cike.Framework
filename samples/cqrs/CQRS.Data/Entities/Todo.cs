using Cike.Domain.Entities;

namespace CQRS.Data.Entities;

public class Todo : FullAuditedEntity<Guid, long>
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsDone { get; set; }

    public List<long> Tests { get; set; } = [];
}
