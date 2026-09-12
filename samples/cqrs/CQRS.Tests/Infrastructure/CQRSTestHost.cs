using Cike.Core.Extensions;
using Cike.Data.EFCore;
using Cike.EventBus.Local;
using Cike.Uow;
using CQRS.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CQRS.Tests.Infrastructure;

public sealed class CQRSTestHost : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private ServiceProvider _serviceProvider = default!;

    public IServiceProvider ServiceProvider => _serviceProvider;

    public async Task InitializeAsync()
    {
        _connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationManager();
        configuration["ConnectionStrings:Cqrs"] = "DataSource=:memory:";
        services.ReplaceConfiguration(configuration);

        await services.AddApplicationAsync<CQRSTestsModule>();

        services.Configure<CikeDbContextOptions>(options =>
        {
            options.Configure(context => context.DbContextOptionsBuilder.UseSqlite(_connection));
        });

        _serviceProvider = services.BuildServiceProvider();

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<CqrsDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }
    }

    public async Task DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        _connection.Dispose();
    }

    public IServiceScope CreateScope() => _serviceProvider.CreateScope();

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : Cike.EventBus.IEvent
    {
        using var scope = CreateScope();
        await scope.ServiceProvider.GetRequiredService<ILocalEventBus>().PublishAsync(@event);
    }
}
