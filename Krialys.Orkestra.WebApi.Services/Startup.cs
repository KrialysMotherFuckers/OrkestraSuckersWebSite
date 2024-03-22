using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Krialys.Orkestra.WebApi.Services.DI;

public static class Startup
{
    public static void AddBusinessServices(this IServiceCollection services)
    {
        // Inject generic service for managing any Entity
        services.TryAddTransient(typeof(GenericCrud<,>), typeof(DbmsServices<,>));
        services.TryAddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
        services.TryAddScoped(typeof(GenericServiceInjectHack<,>));

        services.TryAddScoped<EtiquettesServices>();

        services
            .AddServices(typeof(ITransientService), ServiceLifetime.Transient)
            .AddServices(typeof(IScopedService), ServiceLifetime.Scoped)
            .AddServices(typeof(ISingletonService), ServiceLifetime.Singleton);
    }

    private static IServiceCollection AddServices(this IServiceCollection services, Type interfaceType, ServiceLifetime lifetime)
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
