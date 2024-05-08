using Cike.Core.Modularity;
using Cike.Data;

namespace Cike.Domain;

[DependsOn([typeof(CikeDataModule)])]
public class CikeDomainModule:CikeModule
{

}
