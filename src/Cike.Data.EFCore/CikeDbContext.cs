using Cike.Core.Modularity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cike.Data.EFCore;

public abstract class CikeDbContext<TDbContext> : DbContext where TDbContext : DbContext
{
    private static IServiceProvider? _rootServiceProvider;

    private bool _isInitialized;
    private ILogger<TDbContext>? _logger;
    private IServiceProvider? _currentServiceProvider;

    protected IServiceProvider? CurrentServiceProvider
    {
        get
        {
            if (_isInitialized)
                return _currentServiceProvider;

            if (_currentServiceProvider == null)
            {
                _rootServiceProvider ??= ModuleLoader.Services.BuildServiceProvider();
                _logger = _rootServiceProvider.GetService<ILogger<TDbContext>>();
                _currentServiceProvider = _rootServiceProvider.GetService<IHttpContextAccessor>()?.HttpContext?.RequestServices;
            }

            _isInitialized = true;
            return _currentServiceProvider;
        }
    }
    public CikeDbContext(DbContextOptions<TDbContext> options)
        : base(options)
    {
    }
}
