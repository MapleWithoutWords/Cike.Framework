namespace Cike.Core.Modularity;

public interface IApplicationWithExternalServiceProvider
{
    /// <summary>
    /// Sets the service provider and initializes all the modules.
    /// If <see cref="SetServiceProvider"/> was called before, the same
    /// <see cref="serviceProvider"/> instance should be passed to this method.
    /// </summary>
    Task InitializeAsync(IServiceProvider serviceProvider);

    /// <summary>
    /// Used to gracefully shutdown the application and all modules.
    /// </summary>
    Task ShutdownAsync(IServiceProvider serviceProvider);
}
