using Cike.Contracts.EntityDtos;

namespace CQRS.Application.Dtos;

public class TodoItemDto : FullAuditedEntityDto<Guid, long>
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsDone { get; set; }

    public List<long> Tests { get; set; } = [];
}
