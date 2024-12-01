using Cike.AspNetCore.MinimalAPIs;
using Cike.AspNetCore.MinimalAPIs.Options;
using Cike.AspNetCore.Swagger;
using Cike.Core.Modularity;
using Cike.Data;
using CQRS.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CQRS.WebApi;

[DependsOn([typeof(CQRSApplicationModule), typeof(CikeAspNetCoreMinimalApiModule)])]
public class CQRSWebApiModule : CikeModule
{
    public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MinimalApiOptions>(options =>
        {
            options.LoadMinimalApi(typeof(CQRSWebApiModule).Assembly);
        });

        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])),
                    // 默认允许 300s  的时间偏移量，设置为0
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        //兼容SignalR授权
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                            context?.HttpContext?.Request?.Headers?.TryAdd("Authorization", $"Bearer {accessToken}");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        var services = context.Services;
        context.Services.AddCikeSwagger("CQRS");
    }

    public override Task InitializeAsync(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseCikeSwaggerUI("CQRS");

        var connection = context.ServiceProvider.GetRequiredService<IConnectionStringResolver>();
        return base.InitializeAsync(context);
    }
}
