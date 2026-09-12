using Cike.Core.Modularity;
using Cike.Domain;
using CQRS.Domain.Shared;

namespace CQRS.Domain;

[DependsOn(typeof(CQRSDomainSharedModule), typeof(CikeDomainModule))]
public class CQRSDomainModule : CikeModule
{
}
