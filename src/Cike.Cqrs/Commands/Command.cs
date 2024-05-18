using Cike.EventBus;

namespace Cike.Cqrs.Commands;

public record Command : Event, ICommand
{
}
