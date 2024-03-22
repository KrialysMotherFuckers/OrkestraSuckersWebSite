using Krialys.Orkestra.WebApi.Proxy;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Refit;
using System.IdentityModel.Tokens.Jwt;

namespace Krialys.Orkestra.Web.Infrastructure.Auth;

public static class Startup
{
    //public static IServiceCollection AddAuthentication(this IServiceCollection services,
    //    Type componentType, IConfiguration config, string baseAddress, Action<AuthorizationOptions> authOptions)
    //{
    //    switch (config[nameof(AuthProvider)])
    //    {
    //        case nameof(AuthProvider.AzureAd):
    //            services.TryAddScoped<IAuthenticationService, AzureAdAuthenticationService>();
    //            services.TryAddScoped<AzureAdAuthorizationMessageHandler>();
    //            services.AddMsalAuthentication(options =>
    //                {
    //                    config.Bind(nameof(AuthProvider.AzureAd), options.ProviderOptions.Authentication);
    //                    options.ProviderOptions.DefaultAccessTokenScopes.Add(
    //                        config[$"{nameof(AuthProvider.AzureAd)}:{ConfigNames.ApiScope}"]);
    //                    options.ProviderOptions.LoginMode = "redirect";
    //                })
    //                .AddAccountClaimsPrincipalFactory<AzureAdClaimsPrincipalFactory>();
    //            break;

    //        default:
    //            services.TryAddScoped<AuthenticationStateProvider, JwtAuthenticationService>();
    //            services.TryAddScoped(sp => (IAuthenticationService)sp.GetRequiredService<AuthenticationStateProvider>());
    //            services.TryAddScoped(sp => (IAccessTokenProvider)sp.GetRequiredService<AuthenticationStateProvider>());
    //            services.TryAddScoped<IAccessTokenProviderAccessor, AccessTokenProviderAccessor>();
    //            services.TryAddScoped<JwtAuthenticationHeaderHandler>();
    //            services.AddAuthorizationCore(authOptions);
    //            break;
    //    }

    //    #region Authentication
    //    // https://youtu.be/2c4p6RGtkps (part 1) and https://youtu.be/usl2zJLzCJk (part 2)
    //    //services.TryAddScoped<IAuthenticationService, AuthenticationService>();
    //    services.TryAddScoped<AuthenticationStateProvider, AuthStateProvider>();
    //    services.AddOptions();

    //    // User session status + some other attributes
    //    services.TryAddScoped<IUserSessionStatus, UserSessionStatus>();
    //    #endregion

    //    services.AddOrkestraHttpAuthentication(componentType, config, baseAddress);
    //    services.AddOrkestraPolicies(authOptions);

    //    return services;
    //}

    // public static IHttpClientBuilder AddAuthenticationHandler(this IHttpClientBuilder builder, IConfiguration config) =>
    //     config[nameof(AuthProvider)] switch
    //     {
    //         // AzureAd
    //         nameof(AuthProvider.AzureAd) =>
    //             builder.AddHttpMessageHandler<AzureAdAuthorizationMessageHandler>(),
    //
    //         // Jwt
    //         _ => builder.AddHttpMessageHandler<JwtAuthenticationHeaderHandler>()
    //     };

    private static IMemoryCache _cache;
    private static string _appPrefix;

    public static IServiceCollection AddOrkestraHttpAuthentication(this IServiceCollection services,
        Type componentType, IConfiguration config, string baseAddress)
    {
        _appPrefix = config["AppPrefix"];

        // Each Session must be identified
        var sessionId = $"{Guid.NewGuid():N}-{_appPrefix}";

        // RefIt - uses 'ApiUniversProxyClient' http client
        services.AddRefitClient<IHttpProxyDef>(CustomRefitSettings, Litterals.ApiUniversProxyClient).ConfigureHttpClient((sp, cl) =>
            {
                _cache = sp.GetRequiredService<IMemoryCache>();
                cl.BaseAddress = new((config[Litterals.ProxyUrl]
                    ?? throw new InvalidDataException($"Parameter value missing: {nameof(Litterals.ProxyUrl)}"))
                    .TrimEnd('/')); // Net 6
                cl.DefaultRequestHeaders.Add(Litterals.ApplicationNamespace, componentType.Namespace);
                cl.DefaultRequestHeaders.Add(Litterals.ApplicationClientName, Litterals.ApiUniversProxyClient);
                cl.DefaultRequestHeaders.Add("Cache-Control", "no-store");
                // Each Session must be identified
                cl.DefaultRequestHeaders.Add(Litterals.ApplicationClientSessionId, sessionId);
            })
            .AddTransientHttpErrorPolicy(policyBuilder =>
                // See: https://brooker.co.za/blog/2015/03/21/backoff.html
                policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(System.TimeSpan.FromSeconds(1), Litterals.MaxPollyRetryCount))
            );

        // Add Refit Proxy support
        services.TryAddScoped<IHttpProxyCore, HttpProxyCore>();

        return services;
    }

    /// <summary>
    /// REFIT custom settings
    /// </summary>
    /// <param name="arg"></param>
    /// <returns>Custom Refit settings</returns>
    private static RefitSettings CustomRefitSettings(IServiceProvider sp) => new()
    {
        Buffered = true,
        AuthorizationHeaderValueGetter = (rq, ct) => GetTokenAuthorizationHeader(sp, rq, ct)
    };

    private static Task<string> GetTokenAuthorizationHeader(IServiceProvider sp, HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var localStorage = sp.GetRequiredService<ISyncLocalStorageService>();
        var keyApp = $"{Litterals.AuthToken}{_appPrefix}";
        var token = localStorage.GetItem<string>(keyApp);

        JwtSecurityToken securityToken = default;
        try
        {
            securityToken = new JwtSecurityToken(token);
        }
        catch
        {
            ClearAuthenticationData(sp, localStorage, keyApp, ref token);

            return Task.FromResult(token);
        }

        // Use cache, get JWT from local storage
        var authToken = _cache?.GetOrCreate(Litterals.BearerTokenAuthorizationHeader, entry =>
            {
                var expirationClaim = string.Empty;
                var exp = TimeSpan.Zero;

                expirationClaim = securityToken.Claims
                    ?.FirstOrDefault(c => c.Type.Equals("exp", StringComparison.Ordinal))
                    .Value;

                if (string.IsNullOrEmpty(expirationClaim) || !long.TryParse(expirationClaim, out var expirationUnixTime))
                    ClearAuthenticationData(sp, localStorage, keyApp, ref token);
                else
                {
                    exp = TimeSpan.FromMinutes(DateTimeOffset.FromUnixTimeSeconds(expirationUnixTime)
                        .DateTime
                        .ToLocalTime()
                        .Subtract(DateTime.Now)
                        .TotalMinutes);

                    if (exp > TimeSpan.Zero)
                        entry.AbsoluteExpirationRelativeToNow = exp;
                    else
                        ClearAuthenticationData(sp, localStorage, keyApp, ref token);
                }

                return token;
            });

        return Task.FromResult(authToken);
    }

    private static void ClearAuthenticationData(IServiceProvider sp, ISyncLocalStorageService localStorage, string keyApp, ref string token)
    {
        if (!string.IsNullOrEmpty(keyApp))
            localStorage.RemoveItem(keyApp);

        token = string.Empty;
        _cache?.Remove(Litterals.BearerTokenAuthorizationHeader);
        sp.GetRequiredService<NavigationManager>()?.NavigateTo("logout", false, true);
    }
}