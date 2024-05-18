using Cike.Core.Modularity;
using Cike.EventBus;

namespace Cike.Cqrs;

[DependsOn(typeof(CikeEventBusModule))]
public class CikeCqrsModule : CikeModule
{

}
