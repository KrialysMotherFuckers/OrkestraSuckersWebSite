using BlazorComponentBus;
using Krialys.Orkestra.Common.Models.Notifications;
using Krialys.Orkestra.Web.Module.Common.DI;
using MediatR;
using MediatR.Courier;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Krialys.Orkestra.Web.Infrastructure.Notifications;

internal static class Startup
{
    public static IServiceCollection AddNotifications(this IServiceCollection services, IConfiguration config)
    {
        // Add mediator processing of notifications
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        services.TryAddSingleton<ISfToastServices, SfToastServices>();

        services.AddMediatR((config) => config
            .RegisterServicesFromAssemblies(assemblies))
            .AddCourier(assemblies);

        services.TryAddTransient<INotificationPublisher, NotificationPublisher>();
        services.AddHubNotifications(config);

        // Enable loosely coupled messaging between Blazor UI Components
        services.TryAddScoped<IComponentBus, ComponentBus>();

        // Register handlers for all INotificationMessages
        foreach (var eventType in assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetInterfaces().Any(i => i == typeof(INotificationMessage))))
        {
            services.TryAddSingleton(
                typeof(INotificationHandler<>).MakeGenericType(
                    typeof(NotificationWrapper<>).MakeGenericType(eventType)),
                serviceProvider => serviceProvider.GetRequiredService(typeof(MediatRCourier)));
        }

        return services;
    }

    public static IServiceCollection AddHubNotifications(this IServiceCollection services, IConfiguration config)
    {
        // Add SignalR (concentrator based on chathub)
        // en mode Singleton le Hub apres un close est disposé mais n'est pas réutilisable
        services.TryAddScoped(_ => new HubConnectionBuilder()
            .WithUrl($"{config[Litterals.ProxyUrl]}chathub") // Net 6
            .WithAutomaticReconnect(new[] { System.TimeSpan.Zero, System.TimeSpan.FromSeconds(1) }) // OB-326
            .Build());

        return services;
    }
}