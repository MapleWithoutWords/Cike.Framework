using Cike.AspNetCore.MinimalAPIs.Options;
using Cike.Core.Extensions.DependencyInjection;
using Cike.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace Cike.AspNetCore.MinimalAPIs;

public class CikeAspNetCoreMinimalApiModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();
        context.Services.Configure<GlobalMinimalApiRouteOptions>(options =>
        {
            options.Prefix = "api";
            options.Version = "v1";
        });

        context.Services.Configure<MinimalApiOptions>(options => { });

        context.Services.AddObjectAccessor<IApplicationBuilder>();
        context.Services.AddObjectAccessor<IEndpointRouteBuilder>();
    }

    public override async Task InitializeAsync(ApplicationInitializationContext context)
    {
        var endpointRouteBuilder = context.GetEndpointRouteBuilder();

        AddCikeMinimalAPIs(endpointRouteBuilder);
        await base.InitializeAsync(context);
    }


    private IEndpointRouteBuilder AddCikeMinimalAPIs(IEndpointRouteBuilder builder)
    {
        var globalRouteOptions = builder.ServiceProvider.GetRequiredService<IOptions<GlobalMinimalApiRouteOptions>>().Value;
        var minimalApiOptions = builder.ServiceProvider.GetRequiredService<IOptions<MinimalApiOptions>>().Value;

        var cikeModuleContainer = builder.ServiceProvider.GetRequiredService<CikeModuleContainer>();

        var minimalApiServiceTypeList = minimalApiOptions.MinimalApiAsseblies.SelectMany(e => e.GetTypes().Where(x => !x.IsAbstract && x.IsClass && typeof(MinimalApiServiceBase).IsAssignableFrom(x)).Select(x => x)).ToList();
        foreach (var item in minimalApiServiceTypeList)
        {
            var serviceName = item.Name;
            var instance = (MinimalApiServiceBase)builder.ServiceProvider.GetRequiredService(item);
            if (!instance.ServiceName.IsNullOrEmpty())
            {
                serviceName = instance.ServiceName;
            }
            serviceName = serviceName.RemovePostFix(StringComparison.CurrentCultureIgnoreCase, globalRouteOptions.IgnoredUrlSuffixesInServiceNames.ToArray());
            serviceName = globalRouteOptions.GetPluralizationName(serviceName);

            foreach (var methodInfo in GetMethodInfos(item))
            {
                var methodRouteAttr = methodInfo.GetCustomAttribute<MinimalApiRouteAttribute>();
                IEnumerable<string> httpMethods = [];
                foreach (var httpMethodPrefixItem in globalRouteOptions.HttpMethodPrefixMapDic)
                {
                    if (methodInfo.Name.StartsWith(httpMethodPrefixItem.Key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        httpMethods = [httpMethodPrefixItem.Value];
                    }
                }
                if (methodRouteAttr != null && methodRouteAttr.HttpMethods.Any())
                {
                    httpMethods = methodRouteAttr.HttpMethods;
                }
                if (!httpMethods.Any())
                {
                    httpMethods = ["Post"];
                }

                var methodName = methodInfo.Name.RemovePreFix(globalRouteOptions.HttpMethodPrefixMapDic.Keys.ToArray()).RemovePostFix("Async");
                var route = $"{globalRouteOptions.RootUrl}/{serviceName}{(methodName.IsNullOrEmpty() ? "" : $"/{methodName}")}";
                if (methodRouteAttr?.Pattern.IsNullOrEmpty() == false)
                {
                    route = methodRouteAttr.Pattern;
                }

                var routeBuilder = builder.MapMethods(route, httpMethods, CreateDelegate(methodInfo, instance));
            }
        }
        return builder;
    }

    private IEnumerable<MethodInfo> GetMethodInfos(Type type)
    {
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        var routeIgnoreAttribute = typeof(NonActionAttribute);

        return type.GetMethods(bindingFlags)
            .Where(methodInfo =>
                (!methodInfo.IsSpecialName || (methodInfo.IsSpecialName && methodInfo.Name.StartsWith("get_"))) &&
                methodInfo.CustomAttributes.All(attr => attr.AttributeType != routeIgnoreAttribute));
    }

    private Delegate CreateDelegate(MethodInfo methodInfo, object targetInstance)
    {
        var type = Expression.GetDelegateType(
            methodInfo.GetParameters().Select(parameterInfo => parameterInfo.ParameterType)
            .Concat(new List<Type>
            {
                methodInfo.ReturnType
            }).ToArray()
        );
        return Delegate.CreateDelegate(type, targetInstance, methodInfo);
    }
}
