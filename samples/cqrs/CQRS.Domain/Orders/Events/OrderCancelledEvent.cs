using Cike.Data.Domain.AggregateRoots;

namespace CQRS.Domain.Orders.Events;

public record OrderCancelledEvent(long OrderId, string OrderNo, long BuyerId, decimal RefundAmount) : DomainEvent;
