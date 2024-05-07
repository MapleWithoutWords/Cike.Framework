using Cike.Auth;
using Cike.Core.Modularity;
using Cike.Uow;

namespace Cike.Data.EFCore;

[DependsOn([typeof(CikeAuthModule),typeof(CikeDataModule),typeof(CikeUowModule),])]
public class CikeDataEFCoreModule : CikeModule
{

}
