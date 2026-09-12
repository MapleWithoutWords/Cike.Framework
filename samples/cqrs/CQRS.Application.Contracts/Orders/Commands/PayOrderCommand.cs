using Cike.Cqrs.Commands;

namespace CQRS.Application.Contracts.Orders.Commands;

public record PayOrderCommand(long OrderId) : Command;
