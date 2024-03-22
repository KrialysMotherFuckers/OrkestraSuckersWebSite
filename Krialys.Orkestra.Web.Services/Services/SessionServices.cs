using Krialys.Orkestra.Web.Module.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Krialys.Orkestra.Web.Module.Common.DI;

public interface ISessionServices
{
    ValueTask GetAuthorizationAsync();

    ValueTask<bool> VerifyPolicy(string policy);

    ValueTask<IDictionary<string, bool>> VerifyPolicies(string[] policies);

    string GetUserId();
}

public class SessionServices : ComponentBase, ISessionServices
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly NavigationManager _navigationManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IUserSessionStatus _state;
    private ClaimsPrincipal _claimsPrincipal;
    private AuthenticationState _authenticationState;

    public SessionServices(AuthenticationStateProvider authenticationStateProvider, NavigationManager navigationManager, IAuthorizationService authorizationService, IUserSessionStatus state)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _navigationManager = navigationManager;
        _authorizationService = authorizationService;
        _state = state;
    }

    public string GetUserId()
        => _state.Claims.Any() ? _state.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))?.Value : null;

    public async ValueTask GetAuthorizationAsync()
    {
        // Check user auth state
        if (_authenticationState is null)
            _authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();

        _state.IsConnected = false;

        if (_authenticationState is { User: not null })
        {
            _claimsPrincipal = _authenticationState.User;
            _state.Claims = _claimsPrincipal.Claims;
            _state.IsConnected = _state.Claims.Any();
        }

        if (!_state.IsConnected)
            _navigationManager.NavigateTo("logout", false, true);
    }

    /// <summary>
    /// Checks if user verifies an authorization policy.
    /// </summary>
    /// <param name="policy">Name of the policy to evaluate.</param>
    /// <returns>True if user verifies the policy, false otherwise.</returns>
    public async ValueTask<bool> VerifyPolicy(string policy)
    {
        await GetAuthorizationAsync();

        // Checks if the user meets the authorization policy.
        return (await _authorizationService.AuthorizeAsync(_claimsPrincipal, policy)).Succeeded;
    }

    /// <summary>
    /// Checks if user verifies an authorization policy.
    /// </summary>
    /// <param name="policies">Name of the policy to evaluate.</param>
    /// <returns>True if user verifies the policy, false otherwise.</returns>
    public async ValueTask<IDictionary<string, bool>> VerifyPolicies(string[] policies)
    {
        IDictionary<string, bool> authDico = new Dictionary<string, bool>();

        await GetAuthorizationAsync();

        // Checks if the user meets the authorization policies.
        foreach (var policy in policies)
        {
            authDico.Add(policy, (await _authorizationService.AuthorizeAsync(_claimsPrincipal, policy)).Succeeded);
        }

        return authDico;
    }
}