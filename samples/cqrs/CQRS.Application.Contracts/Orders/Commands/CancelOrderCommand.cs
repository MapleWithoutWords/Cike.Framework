using Cike.Cqrs.Commands;

namespace CQRS.Application.Contracts.Orders.Commands;

public record CancelOrderCommand(long OrderId) : Command;
