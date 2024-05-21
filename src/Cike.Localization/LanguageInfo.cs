namespace Cike.Localization;

public class LanguageInfo
{
    public LanguageInfo(string cultureName, string uiCultureName, string displayName)
    {
        CultureName = cultureName;
        UiCultureName = uiCultureName;
        DisplayName = displayName;
    }
    public string CultureName { get; set; }

    public string UiCultureName { get; set; }

    public string DisplayName { get; set; }
}
