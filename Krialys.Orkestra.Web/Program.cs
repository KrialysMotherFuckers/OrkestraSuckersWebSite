using Krialys.Common.Literals;
using Krialys.Orkestra.Web;
using Krialys.Orkestra.Web.Infrastructure;
using Krialys.Orkestra.Web.Infrastructure.Auth;
using Krialys.Orkestra.Web.Infrastructure.Common;
using Krialys.Orkestra.Web.Infrastructure.Preferences;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Hosting;
using System.Globalization;

// Expose the Program class for use with WebApplicationFactory<T>
public class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        var baseAddress = builder.HostEnvironment.BaseAddress;

        #region  Fill Config from appsettings in WwwRoot
        using (var http = new HttpClient())
        {
            http.BaseAddress = new Uri(baseAddress);
            using var response = await http.GetAsync("appsettings.json");
            await using var stream = await response.Content.ReadAsStreamAsync();
            builder.Configuration.AddJsonStream(stream);
#if DEBUG
            using var response2 = await http.GetAsync($"appsettings.{Environments.Development}.json");
            await using var stream2 = await response2.Content.ReadAsStreamAsync();
            builder.Configuration.AddJsonStream(stream2);
#else
            using var response2 = await http.GetAsync($"appsettings.{Environments.Production}.json");
            await using var stream2 = await response2.Content.ReadAsStreamAsync();
            builder.Configuration.AddJsonStream(stream2);
#endif
        }

        #endregion

        builder.Services.AddOrkestraServices(typeof(App), builder.Configuration, baseAddress,
            Policies.AuthOptions(
            new[] {
            ClaimsLiterals.MSORole,
            ClaimsLiterals.ADMRole,
            ClaimsLiterals.DTFRole,
            ClaimsLiterals.DTMRole,
            ClaimsLiterals.ETQRole,
            }));

        var host = builder.Build();

        var storageService = host.Services.GetRequiredService<IClientPreferenceManager>();
        if (storageService != null)
        {
            CultureInfo culture;
            if (await storageService.GetPreference() is ClientPreference preference)
                culture = new CultureInfo(preference.LanguageCode);
            else
                culture = new CultureInfo(LocalizationConstants.SupportedLanguages.FirstOrDefault()?.Code ?? CultureLiterals.FrenchFR);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        await host.RunAsync();
    }
}