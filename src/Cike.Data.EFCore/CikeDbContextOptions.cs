namespace Cike.Data.EFCore;

public class CikeDbContextOptions
{
    internal Action<CikeDbContextConfigurationContext>? DefaultConfigureAction { get; set; }

    public void Configure(Action<CikeDbContextConfigurationContext> configureAction)
    {
        DefaultConfigureAction = configureAction;
    }
}
