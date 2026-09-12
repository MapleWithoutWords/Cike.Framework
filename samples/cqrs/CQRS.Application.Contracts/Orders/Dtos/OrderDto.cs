using CQRS.Application.Contracts.Orders.Dtos;
using CQRS.Domain.Shared.Enums;

namespace CQRS.Application.Contracts.Orders.Dtos;

public class OrderDto
{
    public long Id { get; set; }

    public string OrderNo { get; set; } = default!;

    public long BuyerId { get; set; }

    public OrderStatus Status { get; set; }

    public string StatusName => Status.ToString();

    public decimal TotalAmount { get; set; }

    public AddressDto Address { get; set; } = default!;

    public List<OrderLineDto> Lines { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}
