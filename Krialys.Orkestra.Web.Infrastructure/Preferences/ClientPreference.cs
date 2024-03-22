using Krialys.Common.Literals;
using Krialys.Orkestra.Web.Infrastructure.Theme;

namespace Krialys.Orkestra.Web.Infrastructure.Preferences;

public enum MainMenuType
{
    Top = 0,
    Left = 1
}

public class ClientPreference : IPreference
{
    public bool IsDarkMode { get; set; }
    public MainMenuType MainMenuType { get; set; } = MainMenuType.Left;
    public bool IsRtl { get; set; }
    public bool IsDrawerOpen { get; set; }
    public string PrimaryColor { get; set; } = CustomColors.Light.Primary;
    public string SecondaryColor { get; set; } = CustomColors.Light.Secondary;
    public double BorderRadius { get; set; } = 5;
    public string LanguageCode { get; set; } = LocalizationConstants.SupportedLanguages.FirstOrDefault()?.Code ?? CultureLiterals.EnglishUS;
    public string LanguageName { get; set; } = LocalizationConstants.SupportedLanguages.FirstOrDefault()?.DisplayName ?? nameof(CultureLiterals.English);
    public OrkaTablePreference TablePreference { get; set; } = new OrkaTablePreference();
}
