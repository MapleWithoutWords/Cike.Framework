using Cike.Cqrs.Commands;

namespace CQRS.Application.Applications.Todos.Commands;

public record TodoDeleteCommand(Guid Id) : Command
{
}
