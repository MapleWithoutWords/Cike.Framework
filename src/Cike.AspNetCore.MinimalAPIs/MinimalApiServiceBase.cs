using Cike.AspNetCore.MinimalAPIs.Options;
using Cike.Core.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using System.Reflection;

namespace Cike.AspNetCore.MinimalAPIs;

public abstract class MinimalApiServiceBase : ISingletonDependency
{
    public string? Prefix { get; set; }
    public string? Version { get; set; }
    public string? ServiceName { get; set; }

    public MinimalApiServiceBase() { }
}
