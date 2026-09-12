using CQRS.Application.Contracts.Orders.Dtos;
using Cike.Cqrs.Queries;

namespace CQRS.Application.Contracts.Orders.Queries;

public record GetOrderQuery(long Id) : Query<OrderDto>;
