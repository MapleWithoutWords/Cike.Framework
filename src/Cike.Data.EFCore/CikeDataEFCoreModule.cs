using Cike.Auth;
using Cike.Core.Modularity;
using Cike.Domain;
using Cike.Uow;

namespace Cike.Data.EFCore;

[DependsOn([typeof(CikeAuthModule),
    typeof(CikeDomainModule),
    typeof(CikeUowModule),])]
public class CikeDataEFCoreModule : CikeModule
{

}
