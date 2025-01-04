namespace CQRS.Application.Dtos;

public class TodoCreateUpdateDto
{
    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public List<long> Tests { get; set; } = [];
}
