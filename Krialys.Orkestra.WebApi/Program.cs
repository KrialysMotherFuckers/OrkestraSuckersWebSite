using Krialys.Orkestra.WebApi.Infrastructure.Configuration;
using Krialys.Orkestra.WebApi.Services.System;
using Serilog;
using System.Diagnostics;

// CRUD => https://medium.com/@sumantmishra/create-a-crud-application-using-asp-net-core-3-0-web-api-a85019cb23c
namespace Krialys.Orkestra.WebApi;

#nullable enable
public static class Program
{
    static IHost _host = default!;

    public static async Task<int> Main(string[] args)
    {
        IServiceScope scopedCpuLifeTime = default!;
        IServiceScope scopedAppLifetime = default!;
        int exitCode = 0;

        try
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            // https://dnchannel.blogspot.com/2007/09/getting-line-numbers-in-exception-stack.html
            var result = await CreateHostBuilderAsync(args, scopedCpuLifeTime, scopedAppLifetime);
            if (result) await _host.RunAsync();

            await _host.StopAsync(TimeSpan.FromSeconds(20));
        }
        catch (OperationCanceledException ex)
        {
            exitCode = -1;
            ErrorGeneration(ex, exitCode);
        }
        catch (Exception ex)
        {
            exitCode = -2;
            ErrorGeneration(ex, exitCode);
        }

        return exitCode;
    }

    private static void ErrorGeneration(Exception ex, int exitCode)
    {
        try
        {
            // Invoke EventLog
            string message = $"[Krialys.Orkestra.WebApi] Error [exitCode {exitCode}]:\r\n{ex.Message}\r\n{ex.StackTrace}";

            Console.WriteLine(message);
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(message, EventLogEntryType.Error);
            }

            Log.Error(message, EventLogEntryType.Error);
        }
        catch { }
    }

    /// <summary>
    /// Host
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private static async Task<bool> CreateHostBuilderAsync(string[] args, IServiceScope scopedCpuLifeTime, IServiceScope scopedAppLifetime)
    {
        var configSettings = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
            .AddJsonFile($"appsettings.{Environments.Development}.json", optional: false, reloadOnChange: true)
#else
            .AddJsonFile($"appsettings.{Environments.Production}.json", optional: false, reloadOnChange: true)
#endif
            .AddEnvironmentVariables()
            .Build();

        _host = Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(config =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddEnvironmentVariables(prefix: "PREFIX_");
                config.AddCommandLine(args);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<HostOptions>(option =>
                {
                    option.ShutdownTimeout = System.TimeSpan.FromSeconds(20);
                });
            })
            .ConfigureAppConfiguration(config => config.AddConfiguration(configSettings))
            .ConfigureWebHostDefaults(builder =>
            {
                builder.CaptureStartupErrors(true);
                builder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                builder.UseStartup<Startup>();
            })
            .AddLog(configSettings)
            .Build();

        if (Orkestra.WebApi.Infrastructure.Startup.EfMigrationUpdate(_host, _host.Services, configSettings))
        {
            Orkestra.WebApi.Infrastructure.Startup.SmtpCheck(configSettings);

            // Invoke CPU Services MainEntryPoint
            scopedCpuLifeTime = _host.Services.CreateScope();
            await scopedCpuLifeTime
                .ServiceProvider
                .GetRequiredService<ICpuServices>()
                .MainEntryPointAsync(off: 0);

            return true;
        }

        return false;
    }
}