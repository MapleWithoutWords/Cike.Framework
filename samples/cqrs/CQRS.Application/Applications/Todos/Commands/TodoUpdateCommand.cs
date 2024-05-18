using Cike.Cqrs.Commands;
using CQRS.Application.Dtos;

namespace CQRS.Application.Applications.Todos.Commands;

public record TodoUpdateCommand(Guid Id, TodoCreateUpdateDto Dto) : Command
{
}
