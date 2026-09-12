using Cike.Contracts.EntityDtos;

namespace CQRS.Application.Contracts.Orders.Dtos;

public class GetOrderListDto : PagedAndSortedResultRequest
{
    public string? Keyword { get; set; }

    public long? BuyerId { get; set; }
}
