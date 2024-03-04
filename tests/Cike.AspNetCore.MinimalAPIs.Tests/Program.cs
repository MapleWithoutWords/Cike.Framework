using Microsoft.AspNetCore.Mvc.ApplicationParts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

app.MapMethods("/api/users/{id}", ["get"], async (context) =>
{
    await context.Response.WriteAsync($"hello,{context.Request.Path.Value}");
});

app.Services.GetRequiredService<ApplicationPart>();

app.Map("/api/users/{id}",Delegate.CreateDelegate());

app.Run();
