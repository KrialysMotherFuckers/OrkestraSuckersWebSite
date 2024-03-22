using Krialys.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Krialys.Orkestra.Web.Module.Common.Components;

public partial class OrkaAuthorizedPage : IDisposable
{
    [CascadingParameter] protected Task<AuthenticationState> AuthenticationStateTask { get; set; }

    [Inject] protected IAuthorizationService AuthorizationService { get; set; }

    public RolesEnums.RolesValues PolicyApplied;

    public bool AllowModify { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Enum.IsDefined(typeof(RolesEnums.RolesValues), PolicyApplied))
            AllowModify = await IsInPolicy(nameof(PolicyApplied));

        await base.OnInitializedAsync();
    }

    #region Policy
    /// <summary>
    /// Checks if user verify an authorization policy.
    /// </summary>
    /// <param name="policy">The name of the policy to evaluate.</param>
    /// <returns>True if user verify the policy, false otherwise.</returns>
    protected async Task<bool> IsInPolicy(string policy)
    {
        var result = false;
        try
        {
            var user = (await AuthenticationStateTask).User;
            if ((await AuthorizationService.AuthorizeAsync(user, policy)).Succeeded)
            {
                result = true;
            }
        }
        catch (Exception ex)
        {
            await ProxyCore.SetLogException(new LogException(this.GetType(), ex));
        }

        return result;
    }
    #endregion

    // No need here to avoid confusion
    //public void Dispose()
    //{
    //    base.Dispose();
    //}
}