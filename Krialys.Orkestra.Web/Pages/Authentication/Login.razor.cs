using Krialys.Orkestra.Web.Common;
using Krialys.Orkestra.Web.Module.Common.Authentication;
using Krialys.Orkestra.Web.Module.Common.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Krialys.Orkestra.Web.Pages.Authentication;

public partial class Login
{
    [Inject] private IAuthenticationService AuthService { get; set; }
    [Inject] private NavigationManager NavManager { get; set; }

#if DEBUG
    private readonly AuthenticationUserModel _model = new() { Login = "GGuillemin", Password = "azerty" };
#else
    private readonly AuthenticationUserModel _model = new();
#endif
    private CustomValidation _customValidation = default;
    public bool BusySubmitting { get; set; }

    private bool _passwordVisibility;
    private InputType _passwordInput = InputType.Password;
    private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

    private void ShowPassword()
    {
        if (_passwordVisibility)
        {
            _passwordVisibility = false;
            _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
            _passwordInput = InputType.Password;
        }
        else
        {
            _passwordVisibility = true;
            _passwordInputIcon = Icons.Material.Filled.Visibility;
            _passwordInput = InputType.Text;
        }
    }

    private async Task SubmitAsync()
    {
        BusySubmitting = true;

        if (await ApiHelper.ExecuteCallGuardedAsync(async () => await LoginExecuteAsync(_model), Snackbar, _customValidation))
        {
            NavManager.NavigateTo("home");
            Snackbar.Add($"Logged in as {_model.Login}", Severity.Info);
        }
        else
        {
            Snackbar.Add($"Incorrect credentials for: {_model.Login}", Severity.Warning);
        }

        BusySubmitting = false;
    }

    private async Task<bool> LoginExecuteAsync(AuthenticationUserModel userForAuthentication)
        => await AuthService.LogIn(_model) != null;
}