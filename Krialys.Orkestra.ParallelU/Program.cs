using Krialys.Common.Formatters;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Extensions;
using Krialys.Orkestra.ParallelU.SI;
using Krialys.Orkestra.Web.Common.ApiClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Serilog;
using Serilog.Events;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace Krialys.Orkestra.ParallelU;

public static class Program
{
    private const string ClientName = "Orkestra.WebApi";

    private async static Task<int> Main(string[] args)
    {
        int exitCode = 0;

        var configSettings = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
            .AddJsonFile($"appsettings.{Environments.Development}.json", optional: false, reloadOnChange: true)
#else
            .AddJsonFile($"appsettings.{Environments.Production}.json", optional: false, reloadOnChange: true)
#endif
            .AddEnvironmentVariables()
            .Build();

        // Anti con #1 (use case : le cas d'un proxy web qui redirige sur localhost en http)
        var httpProxy = Environment.GetEnvironmentVariable("http_proxy");
        if (!string.IsNullOrEmpty(httpProxy))
        {
            Environment.SetEnvironmentVariable("http_proxy", configSettings["HttpProxy"]);
        }

        // Anti con #2 (use case : le cas d'un proxy web qui redirige sur localhost en https)
        var httpsProxy = Environment.GetEnvironmentVariable("https_proxy");
        if (!string.IsNullOrEmpty(httpsProxy))
        {
            Environment.SetEnvironmentVariable("https_proxy", configSettings["HttpsProxy"]);
        }

        try
        {
            var host = CreateHostBuilder(args, configSettings).Build().Services.GetService<IHost>();
            using var cts = new CancellationTokenSource();

            await (host == null ? Task.CompletedTask : host.RunAsync(cts.Token))
                .ContinueWith(async prev =>
                {
                    if (prev.IsCompleted)
                    {
                        if (host != null)
                        {
                            await host.StopAsync(cts.Token);
                            host.Dispose();
                        }
                        cts.Cancel();
                    }
                }, cts.Token);
        }
        catch (OperationCanceledException ex)
        {
            exitCode = -1;
            // Invoke EventLog
            Console.WriteLine($"< [CPU] OperationCanceledException: {ex.Message}");
        }
        catch (Exception ex)
        {
            exitCode = ex.HResult;

            if (!string.IsNullOrEmpty(ex.Message))
                Console.WriteLine($"< [CPU] Error: {ex.Message}");

            await Task.Delay(5000);
        }

        return exitCode;
    }

    // https://docs.microsoft.com/fr-fr/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-3.1&tabs=visual-studio
    // https://github.com/luisperezphd/RunAsService
    private static IHostBuilder CreateHostBuilder(string[] args, IConfigurationRoot configSettings)
    {
        var serverEndPoint = configSettings[configSettings["ParallelU:EndPoint"] ?? string.Empty]?.Trim();
        if(!string.IsNullOrEmpty(serverEndPoint) && !serverEndPoint.EndsWith('/'))
            serverEndPoint += "/";

        var logPath = (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_Logs/ex.log")).Replace("\\", "/");

        Serilog.ILogger logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(LogEventLevel.Information)
            .WriteTo.File(new CompactJsonFormatter(),
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)//, fileSizeLimitBytes: 1000000)
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .CreateLogger();

        var versionInfo = $"{typeof(Program).Assembly.GetName().Version?.ToString()}";

        // Style and fashion text logos: http://patorjk.com/software/taag
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($@"
                _____ _ _            _     _____                _ _      _   _    _          
               / ____| (_)          | |   |  __ \              | | |    | | | |  | |         
      ______  | |    | |_  ___ _ __ | |_  | |__) |_ _ _ __ __ _| | | ___| | | |  | |  ______ 
     |______| | |    | | |/ _ \ '_ \| __| |  ___/ _` | '__/ _` | | |/ _ \ | | |  | | |______|
              | |____| | |  __/ | | | |_  | |  | (_| | | | (_| | | |  __/ | | |__| |         
               \_____|_|_|\___|_| |_|\__| |_|   \__,_|_|  \__,_|_|_|\___|_|  \____/  [v{versionInfo}]
    ");
        Console.ForegroundColor = ConsoleColor.Gray;

        // Spécial gros anti-con : avertir le user qu'il a oublié de configurer le endpoint, sinon ça crash avec un vilain message dans le worker..
        if (string.IsNullOrEmpty(serverEndPoint))
        {
            logger.Error("< [CPU] Error: ParallelU:EndPoint has no value set in appsettings.");

            throw new InvalidProgramException(message: string.Empty);
        }

        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(config => config.AddConfiguration(configSettings))
            .UseConsoleLifetime(opts =>
            {
                opts.SuppressStatusMessages = false;
            })
            .UseWindowsService()
            .ConfigureLogging((context, logging) =>
            {
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                logging.AddSerilog(logger);
            })
            .ConfigureServices((_, services) =>
            {
                // Register logger
                services.TryAddSingleton(logger);

                // Register codepages
                System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                // Add cross origin ressource support https://www.infoworld.com/article/3327562/how-to-enable-cors-in-aspnet-core.html
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(builder => builder
                        .WithOrigins(serverEndPoint)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetIsOriginAllowed(_ => true));
                });

                // 4GB limit for Uploading datas
                services.Configure<FormOptions>(options =>
                {
                    options.MultipartBodyLengthLimit = uint.MaxValue; // 4 GB limit
                    options.ValueLengthLimit = int.MaxValue; // 2 GB limit
                });

                services.AddResponseCompression(options =>
                {
                    options.Providers.Add<BrotliCompressionProvider>();
                    options.Providers.Add<GzipCompressionProvider>();
                    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream", "image/svg+xml", "application/atom+xml" });
                });

                services.Configure<GzipCompressionProviderOptions>(options =>
                {
                    options.Level = CompressionLevel.Optimal;
                });

                // Each Session must be identified
                var sessionId = $"{Guid.NewGuid():N}-{Litterals.ApplicationCpuClient}";
                var baseAddress = new Uri(serverEndPoint);

                // HTTP client => mandatory for //U
                services.AddHttpClient(Litterals.ApplicationCpuClient, factory =>
                {
                    factory.BaseAddress = baseAddress;
                    factory.DefaultRequestHeaders.Add(Litterals.ApplicationClientSessionId, sessionId);
                })
                .AddTransientHttpErrorPolicy(policyBuilder =>
                    // See: https://brooker.co.za/blog/2015/03/21/backoff.html
                    policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(System.TimeSpan.FromSeconds(1), Litterals.MaxPollyRetryCount))
                );

                services.TryAddScoped<IAutoUpdater, AutoUpdater>();
                services
                    .AutoRegisterInterfaces<IOrkaApiClient>()
                    .AddHttpClient(ClientName, (sp, client) =>
                    {
                        client.DefaultRequestHeaders.AcceptLanguage.Clear();
                        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(CultureInfo.DefaultThreadCurrentCulture?.TwoLetterISOLanguageName);
                        client.BaseAddress = baseAddress;
                    });

                // Register ClientName as HttpClient
                services.TryAddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(ClientName));

                // PU Client
                services.AddHostedService<CpuWorker>();
            });
    }
}