using Cike.AspNetCore.MinimalAPIs.Extensions;
using Cike.Core.Extensions;
using CQRS.Service.Open;

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddApplicationAsync<CQRSServiceOpenModule>();

var app = builder.Build();

await app.InitializeApplicationAsync();

app.Run();
