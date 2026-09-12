using CQRS.Application.Orders.Validators;
using Xunit;

namespace CQRS.Tests;

public class ValidationTests
{
    private readonly CreateOrderDtoValidator _validator = new();

    private static CreateOrderDto ValidDto() => new()
    {
        BuyerId = 1,
        Address = new AddressDto { Province = "Guangdong", City = "Shenzhen", Street = "Tech Park", ZipCode = "518000" },
        Lines = [new OrderLineDto { ProductName = "Product", Quantity = 1, UnitPrice = 10 }],
    };

    [Fact]
    public void ValidDto_Passes()
    {
        Assert.True(_validator.Validate(ValidDto()).IsValid);
    }

    [Fact]
    public void EmptyLines_Fails()
    {
        var dto = ValidDto();
        dto.Lines = [];

        var result = _validator.Validate(dto);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Lines");
    }

    [Fact]
    public void ZeroBuyer_Fails()
    {
        var dto = ValidDto();
        dto.BuyerId = 0;

        Assert.Contains(_validator.Validate(dto).Errors, e => e.PropertyName == "BuyerId");
    }

    [Fact]
    public void BadAddress_Fails()
    {
        var dto = ValidDto();
        dto.Address = new AddressDto { Province = "", City = "Shenzhen", Street = "Tech Park", ZipCode = "abc" };

        var result = _validator.Validate(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Address.Province");
        Assert.Contains(result.Errors, e => e.PropertyName == "Address.ZipCode");
    }

    [Fact]
    public void OutOfRangeQuantity_And_NonPositivePrice_Fail()
    {
        var dto = ValidDto();
        dto.Lines =
        [
            new OrderLineDto { ProductName = "P1", Quantity = 0, UnitPrice = 10 },
            new OrderLineDto { ProductName = "P2", Quantity = 1, UnitPrice = -1 },
        ];

        var result = _validator.Validate(dto);
        Assert.Contains(result.Errors, e => e.PropertyName == "Lines[0].Quantity");
        Assert.Contains(result.Errors, e => e.PropertyName == "Lines[1].UnitPrice");
    }
}
