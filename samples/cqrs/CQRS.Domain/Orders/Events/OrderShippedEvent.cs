using Cike.Data.Domain.AggregateRoots;

namespace CQRS.Domain.Orders.Events;

public record OrderShippedEvent(long OrderId, string OrderNo, long BuyerId, string ShippingAddress) : DomainEvent;
