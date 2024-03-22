using Krialys.Common.Literals;
using Krialys.Common.Localization;
using Krialys.Data.EF.Resources;
using Krialys.Orkestra.Web.Module.Common.Shared;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Syncfusion.Blazor;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Krialys.Orkestra.Web.Infrastructure.Localization;

public static class Startup
{
    public static IServiceCollection AddLocalizationServices(this IServiceCollection services, IConfiguration config)
    {
        // Add localization (uses resx files)
        services.AddLocalization();

        /*** Syncfusion localisation (uses resx files). ***/
        // Register the Syncfusion locale service as Singleton
        services.AddSingleton(typeof(ISyncfusionStringLocalizer), typeof(SyncfusionLocalizer));

        // Add specific DataAnnotations as Singleton
        services.TryAddSingleton<IDataAnnotations, DataAnnotations>();

        var cultureName = string.IsNullOrEmpty(config["CultureInfo"])
            ? CultureLiterals.FrenchFR
            : config["CultureInfo"];

        // Set the default culture of the application (mandatory code when you switch to an other UI culture)
        try
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(cultureName);
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentCulture;
        }
        catch { }

        /*** Application localization (uses yaml files). ***/
        // Globalization as Singleton => appsettings.json "CultureInfo" => ApiUnivers\Librairies\05-LibRazor\Resources\*.yml
        services.AddLanguageContainer(Assembly.GetAssembly(typeof(ILanguageContainerService)),
            CultureInfo.GetCultureInfo(cultureName));

        // Register codepages
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        return services;
    }
}