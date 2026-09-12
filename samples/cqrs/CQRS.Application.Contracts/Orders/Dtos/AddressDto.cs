namespace CQRS.Application.Contracts.Orders.Dtos;

public class AddressDto
{
    public string Province { get; set; } = default!;

    public string City { get; set; } = default!;

    public string Street { get; set; } = default!;

    public string ZipCode { get; set; } = default!;
}
