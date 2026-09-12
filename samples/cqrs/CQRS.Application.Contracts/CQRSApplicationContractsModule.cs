using Cike.Contracts;
using Cike.Core.Modularity;
using CQRS.Domain.Shared;

namespace CQRS.Application.Contracts;

[DependsOn(typeof(CQRSDomainSharedModule), typeof(CikeContractsModule))]
public class CQRSApplicationContractsModule : CikeModule
{
}
