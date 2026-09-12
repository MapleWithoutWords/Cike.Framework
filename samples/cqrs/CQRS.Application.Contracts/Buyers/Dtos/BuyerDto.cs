namespace CQRS.Application.Contracts.Buyers.Dtos;

public class BuyerDto
{
    public long Id { get; set; }

    public string Name { get; set; } = default!;

    public int OrderCount { get; set; }

    public decimal TotalAmount { get; set; }
}
