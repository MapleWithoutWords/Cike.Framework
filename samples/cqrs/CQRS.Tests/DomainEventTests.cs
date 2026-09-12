using CQRS.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace CQRS.Tests;

public class DomainEventTests : IClassFixture<CQRSTestHost>
{
    private readonly CQRSTestHost _host;
    private readonly ITestOutputHelper _output;

    public DomainEventTests(CQRSTestHost host, ITestOutputHelper output)
    {
        _host = host;
        _output = output;
    }

    private static CreateOrderDto NewOrderDto(long buyerId, decimal amount = 100m, int lines = 1)
    {
        return new CreateOrderDto
        {
            BuyerId = buyerId,
            Address = new AddressDto
            {
                Province = "Guangdong",
                City = "Shenzhen",
                Street = "Tech Park",
                ZipCode = "518000",
            },
            Lines = Enumerable.Range(0, lines).Select(i => new OrderLineDto
            {
                ProductName = $"Product-{i}",
                Quantity = 1,
                UnitPrice = amount,
            }).ToList(),
        };
    }

    [Fact]
    public async Task CreateOrder_PublishesEvent_AndUpdatesBuyerStatistics()
    {
        var command = new CreateOrderCommand(NewOrderDto(101, 100m, 2));
        await _host.PublishAsync(command);
        _output.WriteLine($"Created order id: {command.Id}");
        Assert.NotEqual(0, command.Id);

        var query = new GetBuyerQuery(101);
        await _host.PublishAsync(query);

        Assert.NotNull(query.Result);
        Assert.Equal(1, query.Result.OrderCount);
        Assert.Equal(200m, query.Result.TotalAmount);
    }

    [Fact]
    public async Task CancelOrder_RollsBackBuyerStatistics()
    {
        var command = new CreateOrderCommand(NewOrderDto(102, 50m, 1));
        await _host.PublishAsync(command);

        await _host.PublishAsync(new CancelOrderCommand(command.Id));

        var query = new GetBuyerQuery(102);
        await _host.PublishAsync(query);
        Assert.Equal(0, query.Result!.OrderCount);
        Assert.Equal(0m, query.Result.TotalAmount);
    }

    [Fact]
    public async Task InvalidTransition_Throws_AndPersistsNothing()
    {
        var command = new CreateOrderCommand(NewOrderDto(103, 10m, 1));
        await _host.PublishAsync(command);

        await Assert.ThrowsAsync<UserFriendlyException>(() => _host.PublishAsync(new ShipOrderCommand(command.Id)));

        var query = new GetOrderQuery(command.Id);
        await _host.PublishAsync(query);
        Assert.Equal(CQRS.Domain.Shared.Enums.OrderStatus.Pending, query.Result!.Status);
    }
}
