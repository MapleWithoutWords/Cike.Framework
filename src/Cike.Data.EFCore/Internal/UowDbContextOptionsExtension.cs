using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cike.Data.EFCore.Internal;

public class UowDbContextOptionsExtension : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    public void ApplyServices(IServiceCollection services)
    {
#pragma warning disable EF1001
        services.Replace(ServiceDescriptor.Scoped<IDbSetSource, UowDbSetSource>());
#pragma warning restore EF1001
    }

    public void Validate(IDbContextOptions options)
    {
    }

    public DbContextOptionsExtensionInfo Info
        => _info ??= new ExtensionInfo(this);


    abstract class RelationalExtensionInfo : DbContextOptionsExtensionInfo
    {
        /// <summary>
        ///     Creates a new <see cref="RelationalExtensionInfo" /> instance containing
        ///     info/metadata for the given extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        protected RelationalExtensionInfo(IDbContextOptionsExtension extension)
            : base(extension)
        {
        }

        /// <summary>
        ///     The extension for which this instance contains metadata.
        /// </summary>
        public new virtual RelationalOptionsExtension Extension
            => (RelationalOptionsExtension)base.Extension;

        /// <summary>
        ///     True, since this is a database provider base class.
        /// </summary>
        public override bool IsDatabaseProvider
            => false;

        /// <summary>
        ///     Returns a hash code created from any options that would cause a new <see cref="IServiceProvider" />
        ///     to be needed. For example, if the options affect a singleton service. However most extensions do not
        ///     have any such options and should return zero.
        /// </summary>
        /// <returns>A hash over options that require a new service provider when changed.</returns>
        public override int GetServiceProviderHashCode()
            => 0;

        /// <summary>
        ///     Returns a value indicating whether all of the options used in <see cref="GetServiceProviderHashCode" />
        ///     are the same as in the given extension.
        /// </summary>
        /// <param name="other">The other extension.</param>
        /// <returns>A value indicating whether all of the options that require a new service provider are the same.</returns>
        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is RelationalExtensionInfo;
    }

    private sealed class ExtensionInfo : RelationalExtensionInfo
    {
        public override string LogFragment { get; } = string.Empty;

        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
        {
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }
    }
}
