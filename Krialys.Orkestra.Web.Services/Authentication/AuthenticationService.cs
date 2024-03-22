using Blazored.LocalStorage;
using Krialys.Common.Auth;
using Krialys.Orkestra.Web.Module.Common.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Krialys.Orkestra.Web.Module.Common.Authentication; // OK https://youtu.be/2c4p6RGtkps

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _client;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILocalStorageService _localStorage;
    private readonly IConfiguration _configuration;
    private readonly IUserSessionStatus _state;
    private readonly string _authorizeUser;
    private readonly string _appPrefix;

    public AuthenticationService(HttpClient client, AuthenticationStateProvider authStateProvider,
        ILocalStorageService localStorage, IConfiguration configuration, IUserSessionStatus state)
    {
        _client = client;
        _localStorage = localStorage;
        _configuration = configuration;
        _state = state;
        _appPrefix = $"{Litterals.AuthToken}{GetKey("AppPrefix")}";

        try
        {
            _authStateProvider = authStateProvider;
            _authorizeUser = $"{GetKey(Litterals.ProxyUrl)}api/common/v1/oauth/{Litterals.AuthorizeUser}";
        }
        catch (Exception ex)
        {
            throw new Exception($"> AuthenticationService error: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    private string GetKey(string key)
    {
        return _configuration.GetChildren().Any(item => item.Key == key) ? _configuration[key] : string.Empty;
    }

    /// <summary>
    /// LogIn
    /// </summary>
    /// <param name="userForAuthentication"></param>
    /// <returns></returns>
    public async Task<AuthenticatedUserModel> LogIn(AuthenticationUserModel userForAuthentication)
    {
        _state.IsConnected = false;
        _state.Claims = Enumerable.Empty<Claim>();

        //Console.WriteLine($"Login: {_client.BaseAddress.AbsoluteUri}");

        using var authResult = await _client.PostAsync(_authorizeUser, new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("login", userForAuthentication.Login),
            new KeyValuePair<string, string>("password", EncodeTo64(userForAuthentication.Password))
        }));

        if (!authResult.IsSuccessStatusCode)
        {
            return null;
        }

        var authContent = await authResult.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<AuthenticatedUserModel>(
            authContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (result == null) return null;
        await _localStorage.SetItemAsync(_appPrefix, result.Access_Token);

        ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Access_Token);
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(Litterals.TokenBearer, result.Access_Token);

        // Transpose jwt token as claims
        IList<Claim> claims = new List<Claim>();

        foreach (var claim in JwtParser.ParseClaimsFromJwt(result.Access_Token))
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

        // Update State
        _state.IsConnected = claims.Any();
        _state.Claims = claims;

        return result;
    }

    /// <summary>
    /// LogOut
    /// </summary>
    /// <returns></returns>
    public async Task LogOut()
    {
        try
        {
            _client.DefaultRequestHeaders.Authorization = null;

            _state.IsConnected = false;
            _state.Claims = Enumerable.Empty<Claim>();

            await _localStorage.RemoveItemAsync(_appPrefix);
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
        }
        catch
        {
            // not used
        }
    }

    private static string EncodeTo64(string toEncode)
    {
        byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
        string returnValue = Convert.ToBase64String(toEncodeAsBytes);

        return returnValue;
    }
}