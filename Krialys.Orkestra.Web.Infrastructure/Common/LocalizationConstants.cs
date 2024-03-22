using Krialys.Common.Literals;

namespace Krialys.Orkestra.Web.Infrastructure.Common;

public record LanguageCode(string Code, string DisplayName, bool IsRTL = false);

public static class LocalizationConstants
{
    public static readonly LanguageCode[] SupportedLanguages;

    static LocalizationConstants()
    {
        SupportedLanguages = new LanguageCode[] {
            new(CultureLiterals.FrenchFR, nameof(CultureLiterals.French)),
            new(CultureLiterals.EnglishUS, nameof(CultureLiterals.English))
        };
    }
}