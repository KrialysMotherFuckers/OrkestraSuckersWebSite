using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Krialys.Orkestra.Common.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AutoRegisterInterfaces<T>(this IServiceCollection services)
    {
        var interfaceType = typeof(T);

        var types = interfaceType
            .Assembly
            .GetExportedTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => new
            {
                Service = t.GetInterface($"I{t.Name}"),
                Implementation = t
            })
            .Where(t => t.Service != null);

        foreach (var type in types)
        {
            if (interfaceType.IsAssignableFrom(type.Service))
                services.TryAddTransient(type.Service, type.Implementation);
        }

        return services;
    }
}
