using Blazored.LocalStorage;
using Krialys.Common.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Krialys.Orkestra.Web.Module.Common.Models;

public interface IUserSessionStatus
{
    bool IsConnected { get; set; }

    bool IsTimeOut();

    string GetUserName();

    IEnumerable<Claim> Claims { get; set; }

    string GetClaimValue(string claimType);
}

public class UserSessionStatus : IUserSessionStatus
{
    private readonly ISyncLocalStorageService _localStorage;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserSessionStatus(ISyncLocalStorageService localStorage, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _localStorage = localStorage;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        IsConnected = !string.IsNullOrEmpty(localStorage.GetItem<string>($"{Litterals.AuthToken}{configuration["AppPrefix"]}"));

        Claims = Claims
            ?? ((ClaimsIdentity)_httpContextAccessor.HttpContext?.User.Identity)?.Claims
            ?? Enumerable.Empty<Claim>();
    }

    private bool HasToken
        => !string.IsNullOrEmpty(_localStorage.GetItem<string>($"{Litterals.AuthToken}{_configuration["AppPrefix"]}"));

    public bool IsConnected { get; set; }

    public IEnumerable<Claim> Claims { get; set; }

    public string GetUserName()
        => GetClaimValue(ClaimTypes.Name);

    public bool IsTimeOut()
    {
        var timeout = true;

        if (HasToken)
        {
            var expireAt = GetClaimValue("exp");
            if (expireAt != null)
            {
                var exp = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(expireAt)).ToLocalTime();
                var now = DateExtensions.GetLocaleNow();
                var datediff = (int)(exp - now).TotalSeconds;

                timeout = datediff < 1;
            }
        }

        return timeout;
    }

    public string GetClaimValue(string claimType)
        => Claims?.FirstOrDefault(x => x.Type.Equals(claimType))?.Value;
}
