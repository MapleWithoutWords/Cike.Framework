using Cike.AspNetCore.MinimalAPIs;
using Cike.Core.Modularity;
using CQRS.Application;

namespace CQRS.WebApi;

[DependsOn([typeof(CQRSApplicationModule), typeof(CikeAspNetCoreMinimalApiModule)])]
public class CQRSWebApiModule : CikeModule
{
}
