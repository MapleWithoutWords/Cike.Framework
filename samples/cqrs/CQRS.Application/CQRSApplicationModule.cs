using Cike.Core.Modularity;
using Cike.Cqrs;
using Cike.EventBus.Local;
using Cike.FluentValidation;
using CQRS.Application.Contracts;
using CQRS.Domain;
using Mapster;

namespace CQRS.Application;

[DependsOn(typeof(CQRSDomainModule),
    typeof(CQRSApplicationContractsModule),
    typeof(CikeCqrsModule),
    typeof(CikeEventBusLocalModule),
    typeof(CikeFluentValidationModule))]
public class CQRSApplicationModule : CikeModule
{
    public override Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        TypeAdapterConfig<CQRS.Domain.Orders.OrderLine, Contracts.Orders.Dtos.OrderLineDto>.NewConfig()
            .Map(dest => dest.UnitPrice, src => src.UnitPrice.Amount);

        return base.ConfigureServicesAsync(context);
    }
}
