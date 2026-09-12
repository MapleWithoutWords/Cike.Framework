namespace CQRS.Application.Contracts.Orders.Dtos;

public class OrderLineDto
{
    public string ProductName { get; set; } = default!;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
