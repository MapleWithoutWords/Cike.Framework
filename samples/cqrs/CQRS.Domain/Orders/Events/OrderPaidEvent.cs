using Cike.Data.Domain.AggregateRoots;

namespace CQRS.Domain.Orders.Events;

public record OrderPaidEvent(long OrderId, string OrderNo, long BuyerId, decimal PaidAmount) : DomainEvent;
