namespace Cike.Localization;

public interface ILanguageProvider
{
    IReadOnlyList<LanguageInfo> GetLanguageInfos();
}
