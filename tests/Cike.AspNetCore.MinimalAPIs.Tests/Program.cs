
using Cike.AspNetCore.MinimalAPIs.Extensions;
using Cike.AspNetCore.MinimalAPIs.Tests;
using Cike.Core.Extensions;
using Cike.Core.ObjectAccessor;

var builder = WebApplication.CreateBuilder(args);

await builder.Services.AddApplicationAsync<TestModule>();

var assemblyList = AppDomain.CurrentDomain.GetAssemblies();

// Add services to the container.
var app = builder.Build();

await app.InitializeApplicationAsync();
//app.AddCikeMinimalAPIs();

app.Run();
