using Krialys.Orkestra.Web.Module.Common.PolicyBuilders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Settings.References;

public partial class Index
{
    [CascadingParameter] private Task<AuthenticationState> AuthenticationStateTask { get; set; }

    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    private bool AllowModify;

    #region Policy

    protected override Task OnAfterRenderAsync(bool firstRender)
        => IsInPolicyAsync(nameof(PoliciesLiterals.ReferencesViewer));

    /// <summary>
    /// Checks if user verify an authorization policy.
    /// </summary>
    /// <param name="policy">The name of the policy to evaluate.</param>
    /// <returns>True if user verify the policy, false otherwise.</returns>
    public async Task IsInPolicyAsync(string policy)
    {
        try
        {
            // Get user identity.
            var user = (await AuthenticationStateTask).User;

            // Checks if a user meets the specific authorization policy.
            AllowModify = (await AuthorizationService.AuthorizeAsync(user, policy)).Succeeded;
        }
        catch (Exception ex)
        {
            // Log error
            await ProxyCore.SetLogException(new LogException(GetType(), ex));
        }
    }
    #endregion
}