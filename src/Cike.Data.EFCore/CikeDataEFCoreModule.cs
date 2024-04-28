using Cike.Auth;
using Cike.Core.Modularity;

namespace Cike.Data.EFCore;

[DependsOn([typeof(CikeAuthModule),typeof(CikeDataModule),])]
public class CikeDataEFCoreModule : CikeModule
{

}
