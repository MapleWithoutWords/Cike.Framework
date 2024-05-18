using Cike.Cqrs.Commands;
using CQRS.Application.Dtos;

namespace CQRS.Application.Applications.Todos.Commands;

public record TodoCreateCommand(TodoCreateUpdateDto Dto) : Command
{
}
