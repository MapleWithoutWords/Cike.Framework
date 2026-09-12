using Cike.Cqrs.Commands;

namespace CQRS.Application.Contracts.Orders.Commands;

public record ShipOrderCommand(long OrderId) : Command;
