namespace Cike.Core.Modularity;

public class ApplicationWithExternalServiceProvider : IApplicationWithExternalServiceProvider
{
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var cikeModuleContainer = serviceProvider.GetRequiredService<CikeModuleContainer>();
        List<Task> tasks = new List<Task>();
        cikeModuleContainer.CikeModules.Reverse<CikeModule>().ToList().ForEach(module =>
        {
            tasks.Add(module.InitializeAsync(new ApplicationInitializationContext(serviceProvider)));
        });

        await Task.WhenAll(tasks);
    }

    public async Task ShutdownAsync(IServiceProvider serviceProvider)
    {
        var cikeModuleContainer = serviceProvider.GetRequiredService<CikeModuleContainer>();
        List<Task> tasks = new List<Task>();
        cikeModuleContainer.CikeModules.Reverse<CikeModule>().ToList().ForEach(module =>
        {
            tasks.Add(module.ShutdownAsync(new ApplicationShutdownContext(serviceProvider)));
        });

        await Task.WhenAll(tasks);
    }
}
