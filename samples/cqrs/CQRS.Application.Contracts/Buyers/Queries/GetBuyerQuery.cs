using CQRS.Application.Contracts.Buyers.Dtos;
using Cike.Cqrs.Queries;

namespace CQRS.Application.Contracts.Buyers.Queries;

public record GetBuyerQuery(long Id) : Query<BuyerDto>;
