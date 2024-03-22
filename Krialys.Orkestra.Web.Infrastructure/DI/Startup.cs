using BlazorComponentBus;
using Krialys.Orkestra.Web.Infrastructure.User;
using Krialys.Orkestra.Web.Module.Common.Authentication;
using Krialys.Orkestra.Web.Module.Common.DI;
using Krialys.Orkestra.Web.Module.Common.Models;
using Krialys.Orkestra.Web.Module.DTS.DI;
using Krialys.Orkestra.Web.Module.MSO.DI;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Krialys.Orkestra.Web.Infrastructure.DI;

public static class Startup
{
    /// <summary>
    /// Prepare services for service injection and common dependencies.
    /// </summary>
    /// <param name="services"></param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // MSO Appointments
        services.TryAddSingleton<IAppointmentServices, AppointmentServices>();

        // MSO data adaptor used by "rapprochements" scheduler component (dependencies: IAppointmentServices)
        services.TryAddScoped<IRapprochementsSchedulerServices, RapprochementsSchedulerServices>();

        //services.TryAddScoped<Interface, Class>();
        // Add specific WASM Javascript Interop custom services
        services.TryAddSingleton<ISfJsInteropServices, SfJsInteropServices>();

        // Toast
        services.TryAddSingleton<ISfToastServices, SfToastServices>();

        // Datagrid column reflection
        services.TryAddSingleton<ISfGridColumnParameterServices, SfGridColumnParameterServices>();

        services.TryAddScoped<IAuthenticationService, AuthenticationService>();
        services.TryAddScoped<AuthenticationStateProvider, AuthStateProvider>();
        services.AddOptions();

        // User session status + some other attributes
        services.TryAddScoped<IUserSessionStatus, UserSessionStatus>();

        // Add DataAdaptor (based on generic open type) - use default Hosted http CLient
        services.TryAddScoped(typeof(IWasmDataAdaptor<>), typeof(WasmDataAdaptor<>));
        //services.TryAddScoped(typeof(IOrkaDataAdaptor<>), typeof(OrkaDataAdaptor<>));

        // Check session validity
        services.TryAddScoped<ISessionServices, SessionServices>();

        // Cron services.
        services.TryAddScoped<ICronServices, CronServices>();

        // Dowload services.
        services.TryAddSingleton<IDownloadServices, DownloadServices>();

        // Order management.
        services.TryAddScoped<IOrderManagementServices, OrderManagementServices>();

        // Order Kanban Data Adaptor.
        services.TryAddScoped<IOrderKanbanDataAdaptorServices, OrderKanbanDataAdaptorServices>();

        // Enable loosely coupled messaging between Blazor UI Components
        services.TryAddScoped<IComponentBus, ComponentBus>();

        services.TryAddScoped<IUserManager, UserManager>();

        return services;
    }
}