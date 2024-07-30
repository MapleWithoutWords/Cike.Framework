using Cike.Contracts.EntityDtos;

namespace CQRS.Application.Dtos;

public class TodoItemDto : FullAuditedEntityDto<Guid>
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsDone { get; set; }
}
