using Cike.Core.Modularity;
using Cike.Cqrs;
using Cike.EventBus.Local;
using CQRS.Data;

namespace CQRS.Application;

[DependsOn([typeof(CikeEventBusLocalModule),typeof(CQRSDataModule),typeof(CikeCqrsModule)])]
public class CQRSApplicationModule : CikeModule
{

}
