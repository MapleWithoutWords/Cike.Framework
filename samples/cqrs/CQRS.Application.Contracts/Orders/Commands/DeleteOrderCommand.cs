using Cike.Cqrs.Commands;

namespace CQRS.Application.Contracts.Orders.Commands;

public record DeleteOrderCommand(long OrderId) : Command;
