using Krialys.Common.Localization;
using Krialys.Orkestra.Web.Components.Dialogs;
using Krialys.Orkestra.Web.Module.ADM.Components.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Krialys.Orkestra.Web.Shared;

public partial class MainLayout
{
    [Inject]
    private ILanguageContainerService _trad { get; set; } = default!;

    [Parameter]
    public RenderFragment ChildContent { get; set; } = default!;
    [Parameter]
    public EventCallback<bool> OnDarkModeToggle { get; set; }

    private string _imageHeaderSource = "icons/HomePage/logo_orkestra.svg";
    private string _darkModeIcon = Icons.Material.Filled.WbSunny;
    private bool _drawerOpen = true;
    private string darkModeTooltipText { get; set; } = "Switch to Dark mode";

    private void DrawerToggle()
    {
        _drawerOpen = !_drawerOpen;
    }

    private async Task DarkModeToggle()
    {
        if (_darkModeIcon == Icons.Material.Filled.WbSunny)
        {
            _darkModeIcon = Icons.Material.Filled.DarkMode;
            darkModeTooltipText = "Switch to Light mode";
        }
        else
        {
            _darkModeIcon = Icons.Material.Filled.WbSunny;
            darkModeTooltipText = "Switch to Dark mode";
        }
        await OnDarkModeToggle.InvokeAsync(_darkModeIcon == Icons.Material.Filled.DarkMode);
    }

    private void OpenUserSettings()
    {

    }

    private void About()
        => DialogService.Show<About>(
                        _trad.Keys["COMMON:About"],
                        new DialogParameters(),
                        new DialogOptions { CloseOnEscapeKey = false, CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true });

    private void Profile() => Navigation.NavigateTo("account");

    private void RegisterLicenseKey()
        => DialogService.Show<LicenseDetailsAndRegister>(
                        _trad.Keys["COMMON:Register"],
                        new DialogParameters(),
                        new DialogOptions { CloseOnEscapeKey = false, CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true });

    private void LogOut()
    {
        var parameters = new DialogParameters
            {
                { nameof(Logout.ContentText), _trad.Keys["Auth:LogOut"]},
                { nameof(Logout.ButtonText), _trad.Keys["Auth:SignOut"]},
                { nameof(Logout.Color), Color.Error}
            };

        var options = new DialogOptions { CloseOnEscapeKey = false, CloseButton = false, MaxWidth = MaxWidth.Small, FullWidth = true };
        DialogService.Show<Logout>(_trad.Keys["Auth:LogOut"], parameters, options);
    }

    private void OnMenuChanged(string url)
    {
        _imageHeaderSource = string.IsNullOrEmpty(url) ? "icons/HomePage/logo_orkestra.svg" : url;
    }
}