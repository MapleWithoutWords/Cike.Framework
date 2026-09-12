
namespace CQRS.Application.Contracts.Orders.Dtos;

public class CreateOrderDto
{
    public long BuyerId { get; set; }

    public AddressDto Address { get; set; } = default!;

    public List<OrderLineDto> Lines { get; set; } = new();
}
