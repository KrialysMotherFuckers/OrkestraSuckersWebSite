using Krialys.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Krialys.Orkestra.WebApi.Infrastructure.Common.Services;

internal static class Startup
{
    internal static IServiceCollection AddServices(this IServiceCollection services) =>
        services
            .AddServices(typeof(ITransientService), ServiceLifetime.Transient)
            .AddServices(typeof(IScopedService), ServiceLifetime.Scoped)
            .AddServices(typeof(ISingletonService), ServiceLifetime.Singleton);

    internal static IServiceCollection AddServices(this IServiceCollection services, Type interfaceType, ServiceLifetime lifetime)
    {
        AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(t => interfaceType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .Select(t => new
            {
                Service = t.GetInterfaces().FirstOrDefault(),
                Implementation = t
            })
            .Where(t => t.Service is not null && interfaceType.IsAssignableFrom(t.Service))
        .ToList()
        .ForEach(type => services.AddService(type.Service!, type.Implementation, lifetime));

        return services;
    }

    private static void AddService(this IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Transient:
                services.TryAddTransient(serviceType, implementationType);
                break;

            case ServiceLifetime.Scoped:
                services.TryAddScoped(serviceType, implementationType);
                break;

            case ServiceLifetime.Singleton:
                services.TryAddSingleton(serviceType, implementationType);
                break;

            default:
                throw new ArgumentException("Invalid lifeTime", nameof(lifetime));
        }
    }
}