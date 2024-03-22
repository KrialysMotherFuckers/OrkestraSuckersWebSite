using Krialys.Data.EF.Logs;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Krialys.Orkestra.WebApi.Infrastructure.Configuration;

public static class Startup
{
    public static IHostBuilder AddLog(this IHostBuilder builder, IConfiguration configuration)
    {
        if (!Directory.Exists("App_Data")) Directory.CreateDirectory("App_Data");
        if (!Directory.Exists(@"App_Data\Database")) Directory.CreateDirectory(@"App_Data\Database");

        var dbLogPath = configuration["Logging:LogDbPath"] ?? throw new InvalidProgramException("Logging:LogDbPath is missing from appsettings!");
        Serilog.ILogger logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            /*.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                .WithDefaultDestructurers()
                .WithDestructurers(new[] {
                    new ApiExceptionDestructurer(destructureCommonExceptionProperties: false)
                }))*/
            .WriteTo.SQLite(dbLogPath, tableName: nameof(TM_LOG_Logs), maxDatabaseSize: 1000, restrictedToMinimumLevel: LogEventLevel.Information, storeTimestampInUtc: true)
            .WriteTo.Console(LogEventLevel.Warning)
            .Filter.ByExcluding(c => c.Properties.Any(p => p.Value.ToString().Contains("swagger")))
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
        .CreateLogger();

        builder.ConfigureLogging((context, logging) =>
            {
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddSerilog(logger);
            });

        builder.ConfigureServices((_, svc) =>
        {
            // Register logger
            svc.TryAddSingleton(logger);
        });

        return builder;
    }
}
