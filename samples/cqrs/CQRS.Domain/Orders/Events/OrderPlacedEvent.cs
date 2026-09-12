using Cike.Data.Domain.AggregateRoots;

namespace CQRS.Domain.Orders.Events;

public record OrderPlacedEvent(string OrderNo, long BuyerId, decimal TotalAmount) : DomainEvent;
