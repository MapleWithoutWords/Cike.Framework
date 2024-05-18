using Cike.Cqrs.Queries;
using CQRS.Application.Dtos;

namespace CQRS.Application.Applications.Todos.Queries;

public record TodoGetListQuery : Query<IEnumerable<TodoItemDto>>
{
}
