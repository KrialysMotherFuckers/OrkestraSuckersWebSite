using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Krialys.Orkestra.Web.Infrastructure.Preferences;

internal static class Startup
{
    /// <summary>
    /// Prepare services for service injection and common dependencies.
    /// </summary>
    /// <param name="services"></param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddPreferences(this IServiceCollection services)
    {
        services.TryAddScoped<IClientPreferenceManager, ClientPreferenceManager>();

        return services;
    }

}