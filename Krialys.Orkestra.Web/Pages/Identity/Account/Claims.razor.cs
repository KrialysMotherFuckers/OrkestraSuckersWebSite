using Krialys.Orkestra.Web.Common.ApiClient;
using MudBlazor;

namespace Krialys.Orkestra.Web.Pages.Identity.Account;

public partial class Claims
{
    //[Inject]
    //public IPersonalClient PersonalClient { get; set; } = default!;

    private readonly ChangePasswordRequest _passwordModel = new();

    //private CustomValidation _customValidation;

    private Task ChangePasswordAsync()
    {
        return Task.CompletedTask;
    }

    private bool _currentPasswordVisibility;
    private InputType _currentPasswordInput = InputType.Password;
    private string _currentPasswordInputIcon = Icons.Material.Filled.VisibilityOff;
    private bool _newPasswordVisibility;
    private InputType _newPasswordInput = InputType.Password;
    private string _newPasswordInputIcon = Icons.Material.Filled.VisibilityOff;

    private void TogglePasswordVisibility(bool newPassword)
    {
        if (newPassword)
        {
            if (_newPasswordVisibility)
            {
                _newPasswordVisibility = false;
                _newPasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                _newPasswordInput = InputType.Password;
            }
            else
            {
                _newPasswordVisibility = true;
                _newPasswordInputIcon = Icons.Material.Filled.Visibility;
                _newPasswordInput = InputType.Text;
            }
        }
        else
        {
            if (_currentPasswordVisibility)
            {
                _currentPasswordVisibility = false;
                _currentPasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                _currentPasswordInput = InputType.Password;
            }
            else
            {
                _currentPasswordVisibility = true;
                _currentPasswordInputIcon = Icons.Material.Filled.Visibility;
                _currentPasswordInput = InputType.Text;
            }
        }
    }
}