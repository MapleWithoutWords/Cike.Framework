using Cike.AspNetCore.MinimalAPIs.Extensions;
using Cike.Core.Extensions;
using CQRS.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
await builder.Services.AddApplicationAsync<CQRSWebApiModule>();

var app = builder.Build();

await app.InitializeApplicationAsync();

app.Run();
