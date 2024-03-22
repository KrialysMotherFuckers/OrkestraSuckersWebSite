using Krialys.Common.Literals;
using Krialys.Orkestra.Web.Infrastructure.Theme;
using System.Text.RegularExpressions;

namespace Krialys.Orkestra.Web.Infrastructure.Preferences;

public class ClientPreferenceManager : IClientPreferenceManager
{
    private readonly ILocalStorageService _localStorageService;

    public ClientPreferenceManager(
        ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async Task<bool> ToggleDarkModeAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            preference.IsDarkMode = !preference.IsDarkMode;
            await SetPreference(preference);
            return !preference.IsDarkMode;
        }

        return false;
    }

    public async Task<bool> ToggleDrawerAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            preference.IsDrawerOpen = !preference.IsDrawerOpen;
            await SetPreference(preference);
            return preference.IsDrawerOpen;
        }

        return false;
    }

    public async Task<bool> ToggleLayoutDirectionAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            preference.IsRtl = !preference.IsRtl;
            await SetPreference(preference);
            return preference.IsRtl;
        }

        return false;
    }

    public async Task<bool> ChangeLanguageAsync(string languageCode)
    {
        if (await GetPreference() is ClientPreference preference)
        {
            var language = Array.Find(LocalizationConstants.SupportedLanguages, a => a.Code == languageCode);
            if (language?.Code is not null)
            {
                preference.LanguageCode = language.Code;
                preference.IsRtl = language.IsRTL;
            }
            else
            {
                preference.LanguageCode = CultureLiterals.EnglishUS;
                preference.IsRtl = false;
            }

            await SetPreference(preference);
            return true;
        }

        return false;
    }

    public async Task<MudTheme> GetCurrentThemeAsync()
    {
        if (await GetPreference() is ClientPreference { IsDarkMode: true }) return new DarkTheme();

        return new LightTheme();
    }

    public async Task<string> GetPrimaryColorAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            string colorCode = preference.PrimaryColor;
            if (Regex.Match(colorCode, "^#(?:[0-9a-fA-F]{3,4}){1,2}$").Success)
            {
                return colorCode;
            }

            preference.PrimaryColor = CustomColors.Light.Primary;
            await SetPreference(preference);
            return preference.PrimaryColor;
        }

        return CustomColors.Light.Primary;
    }

    public async Task<bool> IsRTL()
    {
        return await GetPreference() is ClientPreference { IsRtl: true };
    }

    public async Task<bool> IsDrawerOpen()
    {
        return await GetPreference() is ClientPreference { IsDrawerOpen: true };
    }

    private static readonly string Preference = "clientPreference";

    public async Task<IPreference> GetPreference()
    {
        return await _localStorageService.GetItemAsync<ClientPreference>(Preference) ?? new ClientPreference();
    }

    public async Task SetPreference(IPreference preference)
    {
        await _localStorageService.SetItemAsync(Preference, preference as ClientPreference);
    }
}