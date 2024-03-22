using Krialys.Orkestra.Common.Extensions;
using Krialys.Orkestra.Web.Common.ApiClient;
using Krialys.Orkestra.Web.Infrastructure.Auth;
using Krialys.Orkestra.Web.Infrastructure.DI;
using Krialys.Orkestra.Web.Infrastructure.Localization;
using Krialys.Orkestra.Web.Infrastructure.Notifications;
using Krialys.Orkestra.Web.Infrastructure.Preferences;
using Krialys.Orkestra.Web.Infrastructure.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Popups;
using Syncfusion.Licensing;
using System.Globalization;
using TextCopy;

namespace Krialys.Orkestra.Web.Infrastructure;

public static class Startup
{
    private const string ClientName = "Orkestra.WebApi";

    public static IServiceCollection AddOrkestraServices(this IServiceCollection services,
        Type componentType, IConfiguration config, string baseAddress, Action<AuthorizationOptions> authOptions)
    {
        services
            .AddOrkestraPortailServices(componentType, config, baseAddress, authOptions)
            .AddMudServices()
            .AddPreferences()
            .AddNotifications(config)
            .InjectClipboard();

        //services.AddAuthentication(componentType, config, baseAddress, authOptions);
        services.TryAddScoped<IClientPreferenceManager, ClientPreferenceManager>();
        services.TryAddScoped<IDialogService, DialogService>();
        services.TryAddScoped<IUserManager, UserManager>();
        services.TryAddSingleton<ISnackbar, SnackbarService>();

        return services;
    }

    public static IServiceCollection AddOrkestraPortailServices(this IServiceCollection services,
        Type componentType, IConfiguration config, string baseAddress, Action<AuthorizationOptions> authOptions)
    {
        services.AddSyncfusionServices()
                .AddBlazoredLocalStorageAsSingleton()
                .AddBusinessServices()
                .AddHubNotifications(config)
                .AddMemoryCache()
                .AddOrkestraHttpAuthentication(componentType, config, baseAddress)
                .AddOrkestraPolicies(authOptions)
                .AddLocalizationServices(config)

                // Add Api Http Client
                .AutoRegisterInterfaces<IOrkaApiClient>()
                .AddHttpClient(ClientName, (sp, client) =>
                {
                    client.DefaultRequestHeaders.AcceptLanguage.Clear();
                    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(CultureInfo.DefaultThreadCurrentCulture?.TwoLetterISOLanguageName);
                    client.BaseAddress = new Uri(config[Litterals.ProxyUrl]);
                });

        // Register ClientName as HttpClient
        services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(ClientName));

        return services;
    }

    private static IServiceCollection AddSyncfusionServices(this IServiceCollection services)
    {
        // Add specific dedicated synchronous interfaces dedicated to WASM JS interop
        // https://www.meziantou.net/optimizing-js-interop-in-a-blazor-webassembly-application.htm
        services.TryAddSingleton(sp => (IJSInProcessRuntime)sp.GetRequiredService<IJSRuntime>());

        // Add Syncfusion set IgnoreScriptIsolation to true with NET 5.0 or 6.0 with SF versions >= 20.1
        services.TryAddSingleton<SfDialogService>();
        services.AddSyncfusionBlazor(options =>
        {
            options.Animation = GlobalAnimationMode.Disable;
        });

        // Set Syncfusion license
        SyncfusionLicenseProvider.RegisterLicense(Globals.SfLicenseKey);

        return services;
    }

    private static void RegisterPermissionClaims(AuthorizationOptions options)
    {
        //foreach (var permission in OrkaPermissions.All)
        //{
        //    options.AddPolicy(permission.Name, policy => policy.RequireClaim(OrkaClaims.Permission, permission.Name));
        //}

        //options.AddPolicy(ClaimsLiterals.MSORole, policy => policy.RequireClaim(OrkaClaims.Permission, ClaimsLiterals.MSORole));
        //options.AddPolicy(ClaimsLiterals.ADMRole, policy => policy.RequireClaim(OrkaClaims.Permission, ClaimsLiterals.ADMRole));
        //options.AddPolicy(ClaimsLiterals.DTFRole, policy => policy.RequireClaim(OrkaClaims.Permission, ClaimsLiterals.DTFRole));
        //options.AddPolicy(ClaimsLiterals.DTMRole, policy => policy.RequireClaim(OrkaClaims.Permission, ClaimsLiterals.DTMRole));
        //options.AddPolicy(ClaimsLiterals.ETQRole, policy => policy.RequireClaim(OrkaClaims.Permission, ClaimsLiterals.ETQRole));
    }
}