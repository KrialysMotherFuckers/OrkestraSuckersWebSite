using Blazored.LocalStorage;
using Krialys.Common.Auth;
using Krialys.Orkestra.Web.Module.Common.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Krialys.Orkestra.Web.Module.Common.Authentication; // OK https://youtu.be/2c4p6RGtkps

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _client;
    private readonly ILocalStorageService _localStorage;
    private readonly IConfiguration _configuration;
    private readonly IUserSessionStatus _state;
    private readonly AuthenticationState _anonymous;
    private readonly string _appPrefix;

    public AuthStateProvider(HttpClient client, ILocalStorageService localStorage, IConfiguration configuration, IUserSessionStatus state)
    {
        _client = client;
        _localStorage = localStorage;
        _configuration = configuration;
        _state = state;
        _appPrefix = $"{Litterals.AuthToken}{GetKey("AppPrefix")}";
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    private string GetKey(string key)
    {
        return _configuration.GetChildren().Any(item => item.Key == key) ? _configuration[key] : string.Empty;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>(_appPrefix);

        if (!string.IsNullOrWhiteSpace(token))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(Litterals.TokenBearer, token);

            List<Claim> claims = new();

            foreach (var claim in JwtParser.ParseClaimsFromJwt(token))
            {
                if (claim.Type.EndsWith("Role", StringComparison.OrdinalIgnoreCase))
                {
                    if (!claims.Any(x => x.Type.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase)
                        && x.Value.Equals(claim.Value, StringComparison.OrdinalIgnoreCase)))
                    {
                        claims.Add(new Claim(claim.Type, claim.Value));
                    }
                }
                else
                {
                    claims.Add(new Claim(claim.Type, claim.Value));
                }
            }

            //var payloadClaims = JwtParser.ParseClaimsFromJwt(token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwtAuthType"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            NotifyAuthenticationStateChanged(authState);

            // Get all claims from authenticated user
            _state.Claims = authenticatedUser.Identities.FirstOrDefault()?.Claims;
            _state.IsTimeOut();

            //Console.WriteLine($"[DBG] GetAuthenticationState -> {_state.IsConnected} -> user: '{_state.GetUserName()}'");

            return new AuthenticationState(authenticatedUser);
        }

        _state.Claims = Enumerable.Empty<Claim>();
        _state.IsConnected = false;

        return _anonymous;
    }

    public void NotifyUserAuthentication(string email)
    {
        var authenticatedUser = new ClaimsPrincipal(
            new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, email) }, "jwtAuthType"));

        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(_anonymous);

        NotifyAuthenticationStateChanged(authState);
    }

    /// <summary>
    /// Get full Bearer injected token
    /// </summary>
    public string GetAuthorizationHeader
        => _client.DefaultRequestHeaders.Authorization?.ToString();
}