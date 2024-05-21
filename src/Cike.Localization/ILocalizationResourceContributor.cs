namespace Cike.Localization;

public interface ILocalizationResourceContributor
{
    void Initialize(LocalizationResource resource, IServiceProvider serviceProvider);

    LocalizedString? GetOrNull(string cultureName, string name);
}
