using Cike.Cqrs.Queries;
using CQRS.Application.Dtos;

namespace CQRS.Application.Applications.Todos.Queries;

public record TodoGetQuery(Guid Id) : Query<TodoItemDto>
{
}
