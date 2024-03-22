using Krialys.Common.Excel;
using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Mso;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.Common.Models.WorkerNode;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using static Krialys.Orkestra.Common.Shared.ETQ;

namespace Krialys.Orkestra.ParallelU.SI;

/// <summary>
/// ParallelU Client side - (main entry point) - SI is done from ApiUnivers -
/// </summary>
public sealed class CpuWorker : BackgroundService, IAsyncDisposable
{
    #region PARAMETERS

    private readonly Serilog.ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly string _clientFilesStorage;
    private readonly string _clientRefManagerPath;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IConfiguration _configuration;
    private readonly Encoding _encoding;

    //private FileSystemWatcher? FolderWatcher { get; set; } = null!;
    //private HashSet<(int?, FileSystemWatcher?)> FileWatcherHashSet { get; set; } = new HashSet<(int?, FileSystemWatcher?)>();
    //private HashSet<string> FolderWatcherHashSet { get; set; } = new HashSet<string>(); // HashSet method works faster in HashSet compared to a List for retrieving data

    private IList<IDisposable> Handlers { get; }
    private HubConnection? HubConnection { get; }
    private WorkerNode? CpuBackup { get; set; }
    private WorkerNode? CpuRefManagerBackup { get; set; }
    private string ClientEndPoint { get; }    // Used for authentification purpose
    private string ClientServiceName { get; } // Used for CPU identification purpose (value to be compared are stored in database)
    private bool _stopConnection;

    // nb d'appel effectué aux API en entrée  pour alimentation en bdd le Suivi Ressource de code etiquette,
    // conditionne le passage au bat suivant
    //private int ApiCallSuiviRessourceNb { get; set; }
    //private int ApiCallSuiviRessourceDoneNb { get; set; }
    //private ConcurrentDictionary<string, (int, int? /*, int, int*/)> DemandeEtqCompteurDico { get; set; } = new();

    //private int ApiCallDelay { get; }  // Used for timeout Api call, during batch execution
    //private string ApiOutputPath { get; set; } = string.Empty;

    // nb d'appel effectué aux API en entrée et en sortie pour generation de code etiquette, /
    // conditionne passage au bat suivant
    // private int ApiCallNb { get; set; }
    // private int ApiCallDoneNb { get; set; }

    #endregion parameters

    #region MAIN

    /// <summary>
    /// All starts here
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configuration"></param>
    /// <param name="appLifetime"></param>
    /// <param name="httpClientFactory"></param>
    /// <param name="autoUpdater"></param>
    public CpuWorker(Serilog.ILogger logger, IConfiguration configuration, IHostApplicationLifetime appLifetime, IHttpClientFactory httpClientFactory, IAutoUpdater autoUpdater)
    {
        // Session Id comes from startup and has been injected to Headers, ensuring
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(Litterals.ApplicationCpuClient);
        _appLifetime = appLifetime;
        _configuration = configuration;

        // How to manage accents issue within zip files: https://stackoverflow.com/questions/59734984/char-extraction-issue-with-zipfile-system-io-compression-c-sharp-wpf
        _encoding = Encoding.GetEncoding(850);

        // Application path
        var applicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)?.Replace("\\", "/") ?? "";

        // Core parameters
        ClientEndPoint = configuration[configuration["ParallelU:EndPoint"] ?? string.Empty] ?? string.Empty;

        Handlers = new List<IDisposable>();
        // ApiCallDelay = configuration.GetValue<int>("ApiCallDelay");

        // Real name of the Client (registered in database challenged by licensing mecanism => TODO)
        ClientServiceName = configuration["ParallelU:ServiceName"] ?? string.Empty;

        // Working folder for ETL jobs
        _clientFilesStorage = configuration["ParallelU:FilesStorage"] ?? string.Empty;

        // RefManager path where JAR files are stored
        _clientRefManagerPath = configuration["ParallelU:Extensions:RefManager:RelativePath"] ?? string.Empty;
        if (!string.IsNullOrEmpty(_clientRefManagerPath))
        {
            _clientRefManagerPath = $"{applicationPath}{_clientRefManagerPath}".Replace("\\", "/");
            Directory.CreateDirectory(_clientRefManagerPath);
        }
        else
        {
            _logger.Error($"< [CPU] Error: 'ParallelU:Extensions:RefManager' key/value not found from appsettings.json!");
            _appLifetime.StopApplication();
            return;
        }

        var versionInfo = $"{GetType().Assembly.GetName().Version?.ToString()}";
        _logger.Information($"> [CPU] Starting: {ClientServiceName}, Mode: {(WindowsServiceHelpers.IsWindowsService() ? "Service" : "Console")}, Version: [v{versionInfo}] - {configuration["Environment"]} - Net Core [v{Environment.Version}]");

        // Check network availability
        CheckNetworkAvailability();

        var nodeServiceInfo = new WorkerNodeExt()
        {
            ServiceName = ClientServiceName,
#if DEBUG
            ServerName = "KRIALYSCENTER",
#elif RELEASE
            ServerName = Environment.MachineName,
#endif
            ServerOs = Environment.OSVersion.VersionString,
            FileName = Path.GetFileName(Assembly.GetEntryAssembly()!.Location),
            Version = Assembly.GetEntryAssembly()!.GetName().Version!.ToString(),
            Folder = applicationPath,
            WorkingFilesStorage = _clientFilesStorage.Replace("\\", "/"),
            //HDDInfo = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed).Where(d => d.IsReady).ToArray(),
            //Process = Process.GetCurrentProcess(),
            CanUseRefManagerFeature = _configuration.GetValue<bool>("ParallelU:Extensions:RefManager:IsEnabled")
        };

        //Instanciate a new Hub connection
        HubConnection = new HubConnectionBuilder()
            .WithUrl($"{ClientEndPoint}{CPULitterals.CpuHub}?" +
                $"&cpuname={nodeServiceInfo.ServiceName}"
                + $"&machine={nodeServiceInfo.ServerName}"
                + $"&{nameof(WorkerNodeExt.FileName)}={nodeServiceInfo.FileName}"
                + $"&{nameof(WorkerNodeExt.Version)}={nodeServiceInfo.Version}"
                + $"&{nameof(WorkerNodeExt.ServerOs)}={nodeServiceInfo.ServerOs}"
                + $"&{nameof(WorkerNodeExt.Folder)}={nodeServiceInfo.Folder}"
                + $"&{nameof(WorkerNodeExt.WorkingFilesStorage)}={nodeServiceInfo.WorkingFilesStorage}"
                //+ $"&{nameof(NodeServiceInfo.HDDInfo)}=" // no serialization please
                //+ $"&{nameof(NodeServiceInfo.Process)}=" // no serialization please
                + $"&{nameof(WorkerNodeExt.CanUseRefManagerFeature)}={nodeServiceInfo.CanUseRefManagerFeature}" // no serialization please
            )
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(1) }) // OB-326
            .ConfigureLogging(logging =>
            {
                logging.AddSerilog();
                logging.AddJsonConsole();
            })
            .Build();

        // Resilience level 0: connection has been closed due to network problem, connectionId will expire soon
        HubConnection.Closed += async exception =>
        {
            // Show error when any
            if (exception is not null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                _logger.Error($"< [SYS] Returned: HubConnection.Closed, Error: {exception.Message}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                if (_stopConnection)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    _logger.Error("< [SYS] Returned: HubConnection.Closed, Action: stopConnection invoked");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    return;
                }
            }

            // On network shutdown
            do
            {
                _logger.Warning("< [SYS] Returned: HubConnection.Closed, Action: ShouldLoopAgain (delayed 30s)");
                await Task.Delay(30000);
            }
            while (await ShouldLoopAgain());
        };

        if (autoUpdater.Start() is { Result: true })
        {
            _appLifetime.StopApplication();
            return;
        }
    }

    private async ValueTask<bool> ShouldLoopAgain()
    {
        bool loopAgain = false;

        try
        {
            // Check Hub status
            if (HubConnection is { State: HubConnectionState.Disconnected })
            {
                // Network is back, make HubConnection activated again
                await HubConnection.StartAsync();
            }
            else
            {
                // Check network availability
                CheckNetworkAvailability();
            }
        }
        catch (Exception ex)
        {
            loopAgain = true;
            Console.ForegroundColor = ConsoleColor.Red;
            _logger.Error($"< [CPU] Returned: HubConnection.Closed, Error: {ex.Message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        return loopAgain;
    }

    /// <summary>
    /// Dispose all subcribed callbacks to avoid memory leaks
    /// </summary>
    public ValueTask DisposeAsync()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        _logger.Information($"< [CPU] Stopping: {ClientServiceName}, Message: Successfully disconnected");

        for (int index = Handlers.Count - 1; index >= 0; index--)
            Handlers[index].Dispose();

        return ValueTask.CompletedTask;
    }

    #endregion

    #region EVENTS

    /// <summary>
    /// Register all handlers to fit the needs here
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        // Starts the connection
        await HubConnection!.StartAsync(cancellationToken);

        if (Handlers.Count.Equals(0))
        {
            // CPU Management
            Handlers.Add(HubConnection.On(nameof(OnAfterConnectedAsync), async (WorkerNode cpu) =>
            {
                // Bug fix OB-211
                if (CpuBackup is not null && !cpu.Demande.DemandeId.HasValue)
                {
                    var idBackup = cpu.Id;
                    cpu = CpuBackup;
                    cpu.Id = idBackup;
                    cpu.Resilience = true;
                }

                if (cpu.Demande.DemandeId.HasValue)
                    _logger.Information($"> [CPU] Function: OnAfterConnectedAsync, DemandeId: {cpu.Demande.DemandeId}");

                await OnAfterConnectedAsync(cpu);
            }));

            // OnMessageAsync (log to console)
            Handlers.Add(HubConnection.On(nameof(OnMessageAsync), async (string message) =>
                await OnMessageAsync(message)));

            // OnDownloadFileAsync a Server file onto the Client
            Handlers.Add(HubConnection.On(nameof(OnDownloadFileAsync), async (WorkerNode cpu, string fullServerPath, string clientRelativePath, string fileName) =>
            {
                _logger.Information($"> [CPU] Function: OnDownloadFileAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnDownloadFileAsync(cpu, fullServerPath, clientRelativePath, fileName);
            }));

            // OnUploadFileAsync a Client File onto the Server
            Handlers.Add(HubConnection.On(nameof(OnUploadFileAsync), async (WorkerNode cpu, string fullServerPath, string clientRelativePath, string fileName) =>
            {
                _logger.Information($"> [CPU] Function: OnUploadFileAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnUploadFileAsync(cpu, fullServerPath, clientRelativePath, fileName);
            }));

            // OnClientWakeUp (new request)
            Handlers.Add(HubConnection.On(nameof(OnClientWakeUp), async (WorkerNode cpu) =>
            {
                _logger.Information($"> [NEW] --------: DemandeId: {(cpu.Demande.DemandeId.HasValue ? cpu.Demande.DemandeId : "(null)")}, Status: Started");
                await OnClientWakeUp(cpu);
            }));

            // OnDirectoryZipAsync
            Handlers.Add(HubConnection.On(nameof(OnDirectoryZipAsync), async (WorkerNode cpu, string clientZipFolder, string clientRelativePath, string zipFileName) =>
            {
                _logger.Information($"> [CPU] Function: OnDirectoryZipAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnDirectoryZipAsync(cpu, clientZipFolder, clientRelativePath, zipFileName);
            }));

            // OnDirectoryUnZipAsync
            Handlers.Add(HubConnection.On(nameof(OnDirectoryUnZipAsync), async (WorkerNode cpu, string clientRelativeSourcePath, string zipFileName, string clientRelativeDestPath) =>
            {
                _logger.Information($"> [CPU] Function: OnDirectoryUnZipAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnDirectoryUnZipAsync(cpu, clientRelativeSourcePath, zipFileName, clientRelativeDestPath);
            }));

            // OnCreateDirectoryAsync
            Handlers.Add(HubConnection.On(nameof(OnCreateDirectoryAsync), async (WorkerNode cpu, string clientRelativePath) =>
            {
                _logger.Information($"> [CPU] Function: OnCreateDirectoryAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnCreateDirectoryAsync(cpu, clientRelativePath);
            }));

            // OnCreateMetaFileAsync
            Handlers.Add(HubConnection.On(nameof(OnCreateMetaFileAsync), async (WorkerNode cpu, string clientRelativePath, string metadata) =>
            {
                _logger.Information($"> [CPU] Function: OnCreateMetaFileAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnCreateMetaFileAsync(cpu, clientRelativePath, metadata);
            }));

            // OnAlterCmdAsync
            Handlers.Add(HubConnection.On(nameof(OnAlterCmdAsync), async (WorkerNode cpu, string clientRelativePath, string clientSuffixPath) =>
            {
                _logger.Information($"> [CPU] Function: OnAlterCmdAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnAlterCmdAsync(cpu, clientRelativePath, clientSuffixPath);
            }));

            // OnLoadQualifAsync
            Handlers.Add(HubConnection.On(nameof(OnLoadQualifAsync), async (WorkerNode cpu, string clientRelativePath, string csvFileName) =>
            {
                _logger.Information($"> [CPU] Function: OnLoadQualifAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnLoadQualifAsync(cpu, clientRelativePath, csvFileName);
            }));

            // OnDeleteDirectoryAsync
            Handlers.Add(HubConnection.On(nameof(OnDeleteDirectoryAsync), async (WorkerNode cpu, string clientRelativePath) =>
            {
                _logger.Information($"> [CPU] Function: OnDeleteDirectoryAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnDeleteDirectoryAsync(cpu, clientRelativePath);
            }));

            // OnRunCmdAsync
            Handlers.Add(HubConnection.On(nameof(OnRunCmdAsync), async (WorkerNode cpu, string clientFileName, string parameters, bool useShellExecute) =>
            {
                _logger.Information($"> [CPU] Function: OnRunCmdAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnRunCmdAsync(cpu, clientFileName, parameters, useShellExecute);
            }));

            // OnStopCmdAsync
            Handlers.Add(HubConnection.On(nameof(OnStopCmdAsync), async (WorkerNode cpu, string userName) =>
            {
                _logger.Information($"> [CPU] Function: OnStopCmdAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnStopCmdAsync(cpu, userName);
            }));

            // OnLoadRefLogAsync
            Handlers.Add(HubConnection.On(nameof(OnLoadRefLogAsync), async (WorkerNode cpu, string clientRelativePath) =>
            {
                _logger.Information($"> [CPU] Function: OnLoadRefLogAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnLoadRefLogAsync(cpu, clientRelativePath);
            }));

            // OnCreateEtlSettingsFileAsync
            Handlers.Add(HubConnection.On(nameof(OnCreateEtlSettingsFileAsync), async (WorkerNode cpu, string clientRelativePath, string jwtToken) =>
            {
                _logger.Information($"> [CPU] Function: OnCreateEtlSettingsFileAsync, DemandeId: {cpu.Demande.DemandeId}");
                await OnCreateEtlSettingsFileAsync(cpu, clientRelativePath, jwtToken);
            }));

            // OnAfterFinalizeDemande (log to console)
            Handlers.Add(HubConnection.On(nameof(OnAfterFinalizeDemande), (int demandeId, string cpuId) =>
            {
                _logger.Information($"> [CPU] Function: OnAfterFinalizeDemande, DemandeId: {demandeId}");
                OnAfterFinalizeDemande(demandeId, cpuId);
            }));

            // OnKillProcessAsync
            Handlers.Add(HubConnection.On(nameof(OnKillProcessAsync), async (WorkerNode cpu, string processName) =>
            {
                _logger.Information($"> [CPU] Function: OnKillProcessAsync, DemandeId: {cpu.Demande.DemandeId}, Process: {processName}");
                await OnKillProcessAsync(cpu, processName);
            }));

            // OnRefManagerRunJarAsync
            Handlers.Add(HubConnection.On(nameof(OnRefManagerRunJarAsync), async (WorkerNode cpu, GdbRequestAction action) =>
            {
                if (_onRunRefManagerProcess == null)
                {
                    _logger.Information($"> [CPU] Function: OnRefManagerRunJarAsync, DemandeId: {cpu.Demande.DemandeId}, Action: {action}");
                    await OnRefManagerRunJarAsync(cpu, action);
                }
            }));
        }

        //await ExecuteAsync(cancellationToken);
    }

    /// <summary>
    /// Required by implementation but not used
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            async void Callback()
            {
                await StopAsync(stoppingToken);
            }

            stoppingToken.Register(Callback);

            // Count until the worker exits
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            _logger.Error($"> [CPU] Function: ExecuteAsync, Error: Task canceled");
        }
        catch (Exception ex)
        {
            _logger.Error($"> [CPU] Function: ExecuteAsync, Error: {ex.Message}");

            // Ref: https://learn.microsoft.com/fr-fr/dotnet/core/extensions/windows-service?pivots=dotnet-7-0
            // Terminates this process and returns an exit code to the operating system.
            // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
            // performs one of two scenarios:
            // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
            // 2. When set to "StopHost": will cleanly stop the host, and log errors.
            //
            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code.
            Environment.Exit(1);
        }
    }

    // <summary>
    // Required by implementation but not used yet
    // </summary>
    // <param name="cancellationToken"></param>
    // <returns></returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
        => await base.StopAsync(cancellationToken);

    /// <summary>
    /// Create directory
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <returns></returns>
    public ValueTask OnCreateDirectoryAsync(WorkerNode cpu, string clientRelativePath)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information("> [CPU] Function: OnCreateDirectoryAsync, Status: Running");
        Console.ForegroundColor = ConsoleColor.Gray;

        try
        {
            Directory.CreateDirectory(Path.Combine(_clientFilesStorage, clientRelativePath).Replace("\\", "/"));

            // Starts to watch JSON files
            //Directory.CreateDirectory(Path.Combine(_clientFilesStorage, clientRelativePath, "IN").Replace("\\", "/"));
            //ApiOutputPath = Path.Combine(_clientFilesStorage, clientRelativePath, "OUT").Replace("\\", "/");
            //Directory.CreateDirectory(ApiOutputPath);
            //WatcherStart(Path.Combine(_clientFilesStorage, clientRelativePath, "IN").Replace("\\", "/"), "*.json", cpu.Demande.DemandeId.HasValue ? cpu.Demande.DemandeId.Value : 0);

            cpu.LastCmdResult = true;
        }
        catch (Exception ex)
        {
            cpu.LastCmdResult = false;
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
        }

        var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        if (string.IsNullOrEmpty(cpu.LastCmdError))
            _logger.Information($"< [CPU] Returned: OnCreateDirectoryAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        else
            _logger.Error($"< [CPU] Returned: OnCreateDirectoryAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        Console.ForegroundColor = ConsoleColor.Gray;

        // Update CPU
        return UpdateCpuThenEnchainementAsync(cpu);
    }

    /// <summary>
    /// Create meta.txt file
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="meta"></param>
    /// <returns></returns>
    public async ValueTask OnCreateMetaFileAsync(WorkerNode cpu, string clientRelativePath, string meta)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information("> [CPU] Function: OnCreateMetaFileAsync, Status: Running");
        Console.ForegroundColor = ConsoleColor.Gray;

        try
        {
            var metaFilePath = $"{Path.Combine(_clientFilesStorage, clientRelativePath).Replace("\\", "/")}/meta.txt";
            await File.WriteAllBytesAsync(metaFilePath, Encoding.UTF8.GetBytes(meta));
            cpu.LastCmdResult = true;
        }
        catch (Exception ex)
        {
            cpu.LastCmdResult = false;
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
        }

        var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        if (string.IsNullOrEmpty(cpu.LastCmdError))
            _logger.Information($"< [CPU] Returned: OnCreateMetaFileAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        else
            _logger.Warning($"< [CPU] Returned: OnCreateMetaFileAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        Console.ForegroundColor = ConsoleColor.Gray;

        // Update CPU
        await UpdateCpuThenEnchainementAsync(cpu);
    }

    /// <summary>
    /// Create etlsettings.json
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="jwtToken"></param>
    /// <returns></returns>
    public async ValueTask OnCreateEtlSettingsFileAsync(WorkerNode cpu, string clientRelativePath, string jwtToken)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information("> [CPU] Function: OnCreateEtlSettingsFileAsync, Status: Running");
        Console.ForegroundColor = ConsoleColor.Gray;

        try
        {
            var etlSettingsFilePath = $"{Path.Combine(_clientFilesStorage, clientRelativePath).Replace("\\", "/")}/etlsettings.json";

            var apiCatalog = new ApiCatalog
            {
                AuthToken = jwtToken, // => (obsolete)
                WwwRoot = ClientEndPoint.EndsWith("/") ? ClientEndPoint : $"{ClientEndPoint}/", // => add '/' for better url construction
                Authentication = _configuration["ApiCatalog:Authentication"],
                EndPoint = _configuration["ApiCatalog:EndPoint"],
                Logger = _configuration["ApiCatalog:Logger"], // => (obsolete)
                                                              // => APIs catalog entries for Etiquettes (obsolete)
                EtqCalculate = _configuration["ApiCatalog:EtqCalculate"],
                EtqExtraInfoAddon = _configuration["ApiCatalog:EtqExtraInfoAddon"],
                EtqSuiviRessource = _configuration["ApiCatalog:EtqSuiviRessource"],
            };

            var missing = new List<string>();

            if (string.IsNullOrEmpty(apiCatalog.WwwRoot))
                missing.Add(nameof(apiCatalog.WwwRoot));

            if (string.IsNullOrEmpty(apiCatalog.EndPoint))
                missing.Add(nameof(apiCatalog.EndPoint));

            if (string.IsNullOrEmpty(apiCatalog.Authentication))
                missing.Add(nameof(apiCatalog.Authentication));

            if (missing.Any())
                throw new Exception($"{string.Join(", ", missing)} {(missing.Count > 1 ? "were" : "was")} not found in appsettings.");

            await File.WriteAllBytesAsync(etlSettingsFilePath, System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(apiCatalog));
            cpu.LastCmdResult = true;
        }
        catch (Exception ex)
        {
            cpu.LastCmdResult = false;
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
        }

        var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        if (string.IsNullOrEmpty(cpu.LastCmdError))
            _logger.Information($"< [CPU] Returned: OnCreateEtlSettingsFileAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        else
            _logger.Warning($"< [CPU] Returned: OnCreateEtlSettingsFileAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        Console.ForegroundColor = ConsoleColor.Gray;

        // Update CPU
        await UpdateCpuThenEnchainementAsync(cpu);
    }

    /// <summary>
    /// Invoke WebMethod:  Update CPU
    /// </summary>
    /// <param name="cpu"></param>
    /// <returns></returns>
    public ValueTask OnClientWakeUp(WorkerNode cpu) => UpdateCpuThenEnchainementAsync(cpu);

    /// <summary>
    /// [UPDATE] Update CPU via CPUController (that has full control through CPUManager) + invoke Enchainement
    /// Resilience implemented when a problem occurred like network shutdown, a backup is performed and is used when the connection is back
    /// </summary>
    /// <param name="cpu"></param>
    /// <returns>true if success, else false</returns>
    private async ValueTask UpdateCpuThenEnchainementAsync(WorkerNode cpu)
    {
        try
        {
            if (!string.IsNullOrEmpty(ClientEndPoint))
            {
                // Default rule: always perform a backup
                cpu = System.Text.Json.JsonSerializer.Deserialize<WorkerNode>(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(cpu)) ?? new();
                CpuBackup = cpu;

                // Invoke UpdateCpu service then Enchainement callback
                await HubConnection!.InvokeCoreAsync<bool>("OnUpdateCpuThenEnchainement", new object[] { cpu });
            }
            else
                throw new Exception($"{nameof(ClientEndPoint)} is null.");
        }
        catch (Exception ex)
        {
            cpu.LastCmdResult = false;
            cpu.LastCmdError = $"{ex.Message} {ex.StackTrace?.Replace('\\', '/').Replace("\r\n", " ").Trim()}";
        }

        var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        if (string.IsNullOrEmpty(cpu.LastCmdError))
            _logger.Information($"< [CPU] Returned: UpdateCpuThenEnchainementAsync, Returned: {lastError}");
        else
            _logger.Warning($"< [CPU] Returned: UpdateCpuThenEnchainementAsync, Returned: {lastError}");
        Console.ForegroundColor = ConsoleColor.Gray;
    }

    /// <summary>
    /// Zip client workspace
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientZipFolder"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="zipFileName"></param>
    /// <returns></returns>
    public ValueTask OnDirectoryZipAsync(WorkerNode cpu, string clientZipFolder, string clientRelativePath, string zipFileName)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information("> [CPU] Function: OnDirectoryZipAsync, Status: Running");
        Console.ForegroundColor = ConsoleColor.Gray;

        // Default
        cpu.LastCmdResult = true;

        try
        {
            // Qualif et Output à checker si vide => on fait comme si c'était OK
            string destpathZip = Path.Combine(_clientFilesStorage, clientZipFolder).Replace("\\", "/");
            string destzipFilePath = Path.Combine(destpathZip, zipFileName).Replace("\\", "/");
            string srcPathToZip = Path.Combine(_clientFilesStorage, clientZipFolder, clientRelativePath).Replace("\\", "/");

            if (File.Exists(destzipFilePath))
            {   // zip detected => destroy
                File.Delete(destzipFilePath);
            }

            // if no files in the directory, simply ignore and tell it is ok
            if (!Directory.EnumerateFileSystemEntries(srcPathToZip).Any())
            {
                cpu.LastCmdError = "NO FILE (normal case: directory is empty)";
            }
            else
            {
                // Do not create zip in folder to zip to avoid conflict
                ZipFile.CreateFromDirectory(srcPathToZip, destzipFilePath, CompressionLevel.Optimal, includeBaseDirectory: false, _encoding);
            }
        }
        catch (Exception ex)
        {
            cpu.LastCmdResult = false;
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
        }

        var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        if (string.IsNullOrEmpty(cpu.LastCmdError))
            _logger.Information($"< [CPU] Returned: OnDirectoryZipAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        else
            _logger.Warning($"< [CPU] Returned: OnDirectoryZipAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        Console.ForegroundColor = ConsoleColor.Gray;

        // Update CPU
        return UpdateCpuThenEnchainementAsync(cpu);
    }

    /// <summary>
    /// UnZip client workspace
    /// Path is based on Client path + demandeId + overwriteFiles? + deleteZipAfter?
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativeSourcePath"></param>
    /// <param name="zipFileName"></param>
    /// <param name="clientRelativeDestPath"></param>
    /// <returns></returns>
    public async ValueTask OnDirectoryUnZipAsync(WorkerNode cpu, string clientRelativeSourcePath, string zipFileName, string clientRelativeDestPath)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information("> [CPU] Function: OnDirectoryUnZipAsync, Status: Running");
        Console.ForegroundColor = ConsoleColor.Gray;
        // Default
        cpu.LastCmdResult = true;

        try
        {
            string zipPath = Path.Combine(_clientFilesStorage, clientRelativeSourcePath, zipFileName).Replace("\\", "/");
            string extractPath = Path.Combine(_clientFilesStorage, clientRelativeDestPath).Replace("\\", "/");

            ZipFile.ExtractToDirectory(zipPath, extractPath, _encoding, overwriteFiles: true); // annule et remplace le contenu
            if (File.Exists(zipPath)) File.Delete(zipPath);
        }
        catch (Exception ex)
        {
            cpu.LastCmdResult = false;
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
        }

        var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        if (string.IsNullOrEmpty(cpu.LastCmdError))
            _logger.Information($"< [CPU] Returned: OnDirectoryUnZipAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        else
            _logger.Warning($"< [CPU] Returned: OnDirectoryUnZipAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        Console.ForegroundColor = ConsoleColor.Gray;

        // Update CPU
        await UpdateCpuThenEnchainementAsync(cpu);
    }

    /// <summary>
    /// OnMessageAsync that was sent (== "SendMessageAsync" from server)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public ValueTask OnMessageAsync(string input)
        => LogAsync(input);

    /// <summary>
    /// Client can download a file stored from the Server (SPU) => tested from a 940 Mb stored onto server, download rate: 30 Mb/sec
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="fullServerPath"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="clientFileName"></param>
    /// <returns></returns>
    public async ValueTask OnDownloadFileAsync(WorkerNode cpu, string fullServerPath, string clientRelativePath, string clientFileName)
    {
        bool isEmpty = true;
        cpu.LastCmdError = null;

        // Default
        cpu.LastCmdResult = true;

        try
        {
            string outputPath = Path.Combine(_clientFilesStorage, clientRelativePath, clientFileName).Replace("\\", "/");
            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.Information($"> [CPU] Function: OnDownloadFileAsync, Status: Running, FileName: {outputPath}");
            Console.ForegroundColor = ConsoleColor.Gray;

            // Create dir if not exist
            Directory.CreateDirectory(Path.Combine(_clientFilesStorage, clientRelativePath).Replace("\\", "/"));

            await using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            await foreach (var buffer in HubConnection!.StreamAsync<byte[]>("DownloadStreamAsync", fullServerPath, clientFileName, CancellationToken.None))
            {
                await stream.WriteAsync(buffer);
            }
            if (stream.Length > 0)
                isEmpty = false;

            await stream.FlushAsync();
            stream.Close();

            if (isEmpty)
            {
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                cpu.LastCmdResult = false;
                cpu.LastCmdError = $"File not found: {outputPath}";
            }
        }
        catch (OperationCanceledException ex)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.Error($"> [CPU] Download canceled: {ex.Message}");
            Console.ForegroundColor = ConsoleColor.Gray;

            cpu.LastCmdResult = false;
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
        }
        catch (Exception ex)
        {
            _logger.Error($"> [CPU] Command::StartProcess: {ex.Message}");
            cpu.LastCmdResult = false;
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
        }

        var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        if (string.IsNullOrEmpty(cpu.LastCmdError))
            _logger.Information($"< [CPU] Returned: OnDownloadFileAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        else
            _logger.Warning($"< [CPU] Returned: OnDownloadFileAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {lastError}");
        Console.ForegroundColor = ConsoleColor.Gray;

        // Update CPU
        await UpdateCpuThenEnchainementAsync(cpu);
    }

    /// <summary>
    /// Client can upload a file onto the Server (SPU)
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="fullServerPath"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="clientFileName"></param>
    /// <returns></returns>
    public async ValueTask OnUploadFileAsync(WorkerNode cpu, string fullServerPath, string clientRelativePath, string clientFileName)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information("> [CPU] Function: OnUploadFileAsync, Status: Running");
        Console.ForegroundColor = ConsoleColor.Gray;
        cpu.LastCmdError = null;

        try
        {
            // Check if file to upload is present locally
            if (File.Exists(Path.Combine(_clientFilesStorage, clientRelativePath, clientFileName).Replace("\\", "/")))
            {
                // Set to null before calling Upload
                cpu.LastCmdResult = null;
                cpu.LastCmdError = "Running";
                await HubConnection!.SendAsync("UploadStreamAsync", UploadFileCallbackAsync(cpu, Path.Combine(_clientFilesStorage, clientRelativePath)
                    .Replace("\\", "/"), clientFileName), clientFileName, fullServerPath, cancellationToken: CancellationToken.None);

                // UploadFileCallbackAsync once ended return true/false
                while (cpu.LastCmdResult is null)
                {
                    await Task.Delay(200);
                }
            }
            else
            {
                var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

                cpu.LastCmdResult = false;
                cpu.LastCmdError = $"File not found: {Path.Combine(_clientFilesStorage, clientRelativePath, clientFileName).Replace("\\", "/")}";
                Console.ForegroundColor = ConsoleColor.Cyan;
                _logger.Information($"< [CPU] Returned: OnUploadFileAsync, Returned: {lastError}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.Information($"> [CPU] canceled: {ex.Message}");
            cpu.LastCmdResult = false;
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;

            var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.Information($"> [CPU] Function: OnUploadFileAsync, Returned: {lastError}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        catch (Exception ex)
        {
            _logger.Error($"> [CPU] Command::StartProcess: {ex.Message}");
            cpu.LastCmdResult = false;
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;

            var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.Information($"< [CPU] Returned: OnUploadFileAsync, Returned: {lastError}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static async IAsyncEnumerable<byte[]> UploadFileCallbackAsync(WorkerNode cpu, string clientRelativePath, string clientFileName)
        {
            await using FileStream stream = new(Path.Combine(clientRelativePath, clientFileName).Replace("\\", "/"), FileMode.Open, FileAccess.Read);

            for (var remaining = stream.Length; remaining > 0;)
            {
                var chunk = new byte[(int)Math.Min(remaining, WorkerNode.BufferSize)];
                remaining -= await stream.ReadAsync(chunk.AsMemory(0, chunk.Length));

                yield return chunk;
            }

            cpu.LastCmdResult = stream.Length > 0;
            cpu.LastCmdError = null;
            //Console.WriteLine("< [CPU] [{0}] Returned: {1}, Returned: {2}", DateTime.UtcNow, "OnUploadFileAsync", cpu.LastCmdResult);
        }

        // Update CPU at the very end, we are sure that LastCmdResult is not null at this point
        await UpdateCpuThenEnchainementAsync(cpu);
    }

    //private void OutputDataReceived(object sender, DataReceivedEventArgs e)
    //{
    //    if (!string.IsNullOrWhiteSpace(e?.Data))
    //        ProcessOutput.Add(e.Data);
    //}


    /// <summary>
    /// Specific process relative to OnRunCmdAsync
    /// </summary>
    private Process? _onRunCmdProcess;

    /// <summary>
    /// Client can run a program
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientFileName"></param>
    /// <param name="parameters"></param>
    /// <param name="useShellExecute"></param>
    /// <returns></returns>
    public async ValueTask OnRunCmdAsync(WorkerNode cpu, string clientFileName, string parameters, bool useShellExecute)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information($"> [CPU] Function: OnRunCmdAsync, DemandeId: {cpu.Demande.DemandeId}, Status: Running {clientFileName}");
        Console.ForegroundColor = ConsoleColor.Gray;

        var pathFileName = Path.Combine(_clientFilesStorage, clientFileName).Replace("\\", "/");
        var cPuBatchsAttributes = cpu.Demande.ListBatchsAttributes?.FirstOrDefault(x => x.NomFichier.Equals(clientFileName.Split("/")[2], StringComparison.OrdinalIgnoreCase));

        cpu.LastCmdError = null;
        _onRunCmdProcess = null;

        _onRunCmdProcess = new Process
        {
            EnableRaisingEvents = true
        };

        _onRunCmdProcess.StartInfo = new ProcessStartInfo
        {
            FileName = pathFileName,
            //WorkingDirectory = @"C:\Program Files (x86)\IIS Express\",
            Arguments = parameters,
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = useShellExecute,
            //CreateNoWindow = true,

            //RedirectStandardOutput = true,
            //RedirectStandardError = true,
            //RedirectStandardInput = true,
        };

        //ProcessOutput.Clear();
        //process.OutputDataReceived += OutputDataReceived;
        //process.ErrorDataReceived += OutputDataReceived;
        //process.StartInfo.CreateNoWindow = true;

        //// BatchOutput.log
        //var outputStream = new MemoryStream();
        //var started = Encoding.UTF8.GetBytes($"> *** {DateTime.UtcNow:G} ***{Environment.NewLine}");
        //await outputStream.WriteAsync(started, 0, started.Length);

        //process.OutputDataReceived += async (sender, e) =>
        //{
        //    if (!string.IsNullOrEmpty(e?.Data))
        //    {
        //        byte[] data = Encoding.UTF8.GetBytes(e.Data + Environment.NewLine);
        //        await outputStream.WriteAsync(data, 0, data.Length);
        //    }
        //};

        try
        {
            if (_onRunCmdProcess.Start())
            {
                //process.BeginOutputReadLine();
                //process.BeginErrorReadLine();

                cpu.LastCmdResult = true;
                if (cPuBatchsAttributes is not null)
                {
                    cPuBatchsAttributes.ProcessId = _onRunCmdProcess.Id;
                }

                if (CpuBackup is not null)
                {
                    CpuBackup.LastCmdResult = true;
                    CpuBackup.Demande.ListBatchsAttributes.First(x => x.NomFichier.Equals(clientFileName.Split("/")[2], StringComparison.OrdinalIgnoreCase)).ProcessId = _onRunCmdProcess.Id;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                _logger.Information($"> [CPU] Function: OnRunCmdAsync, DemandeId: {cpu.Demande.DemandeId}, ProcessId: {_onRunCmdProcess.Id}");
                Console.ForegroundColor = ConsoleColor.Gray;

                // TEST: await Task.Delay(10000); OnStopCmd();
            }
        }
        catch (Exception ex)
        {
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
            cpu.LastCmdResult = false;

            var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.Information($"< [CPU] Returned: OnRunCmdAsync, DemandeId: {cpu.Demande.DemandeId}, Returned: {lastError}");
            Console.ForegroundColor = ConsoleColor.Gray;

            //process.OutputDataReceived -= OutputDataReceived;
            //process.ErrorDataReceived -= OutputDataReceived;
            _onRunCmdProcess.Dispose();
            _onRunCmdProcess = null;
        }

        // Callback when process exit
        if (_onRunCmdProcess != null)
        {
            _onRunCmdProcess.Exited += async (sender, _) =>
            {
                if (sender is not Process)
                    return;

                if (cPuBatchsAttributes is not null)
                {
                    // Update cpu exitCode
                    cPuBatchsAttributes.ExitCode = _onRunCmdProcess.ExitCode;

                    // Reset ProcessId here, also confirm process has elegantly exited, as pId is now released by system
                    if (cPuBatchsAttributes.ProcessId.HasValue)
                    {
                        cPuBatchsAttributes.ProcessId = null;

                        //// Log batch output to $demandeId\output
                        //if (outputStream.Length > 0 && cpu.Demande.DemandeId.HasValue)
                        //{
                        //    var batchOutputFile = Path.Combine(_clientFilesStorage,
                        //        cpu.Demande.DemandeId.Value.ToString().PadLeft(6, '0'),
                        //        "Output"
                        //        ).Replace("\\", "/");

                        //    using (var fileStream = new FileStream($"{batchOutputFile}/BatchOutput.log", FileMode.Create, FileAccess.Write))
                        //    {
                        //        var ended = Encoding.UTF8.GetBytes($"< *** {DateTime.UtcNow:G} ***");
                        //        await outputStream.WriteAsync(ended, 0, ended.Length);

                        //        outputStream.Seek(0, SeekOrigin.Begin);
                        //        outputStream.WriteTo(fileStream);
                        //        await outputStream.FlushAsync();
                        //    }
                        //}
                        //await outputStream.DisposeAsync();

                        if (CpuBackup is not null)
                        {
                            var bkp = CpuBackup.Demande.ListBatchsAttributes.First(x => x.NomFichier.Equals(clientFileName.Split("/")[2], StringComparison.OrdinalIgnoreCase));

                            bkp.ProcessId = null;
                            bkp.ExitCode = cPuBatchsAttributes.ExitCode;
                            bkp.SubmitExec = cPuBatchsAttributes.SubmitExec;
                        }
                    }
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                _logger.Information($"< [CPU] Returned: OnRunCmdAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {_onRunCmdProcess.ExitCode}");
                Console.ForegroundColor = ConsoleColor.Gray;

                //process.OutputDataReceived -= OutputDataReceived;
                //process.ErrorDataReceived -= OutputDataReceived;
                _onRunCmdProcess.Dispose();
                _onRunCmdProcess = null;

                // une coupure reseau peut avoir lieu pendant l exécution du bat dont on doit reprendre
                // la version backupé pour avoir le bon socketid qui aura été mis à jour lors de le reconnexion
                // on aura mis a jour les infos liées a ce bat en particulier avant renvoi au central
                if (CpuBackup is not null)
                {
                    if (!_stopProcess) // => Only if process was not killed from UI
                    {
                        CpuBackup.LastCmdResult = cpu.LastCmdResult;
                        CpuBackup.LastCmdError = cpu.LastCmdError;
                    }

                    cpu = System.Text.Json.JsonSerializer.Deserialize<WorkerNode>(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(CpuBackup)) ?? new();
                }

                // Update CPU
                await UpdateCpuThenEnchainementAsync(cpu);
            };
        }

        await ValueTask.CompletedTask;
    }

    private bool _stopProcess;

    /// <summary>
    /// Client can stop current running process
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async ValueTask OnStopCmdAsync(WorkerNode cpu, string userName)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information($"> [CPU] Function: OnStopCmdAsync, DemandeId: {cpu.Demande?.DemandeId}, Status: Stopping current process");
        Console.ForegroundColor = ConsoleColor.Gray;

        if (_onRunCmdProcess != null && CpuBackup != null)
        {
            try
            {
                _stopProcess = true;
                CpuBackup.LastCmdError = $"Stopped by {userName}";
                CpuBackup.LastCmdResult = false;

                _onRunCmdProcess.CloseMainWindow();
                _onRunCmdProcess.WaitForExit(1000);
                if (!_onRunCmdProcess.HasExited)
                {
                    _onRunCmdProcess.Kill(true);
                }
            }
            catch (Exception ex)
            {
                CpuBackup.LastCmdError = ex.InnerException?.Message ?? ex.Message;
                CpuBackup.LastCmdResult = false;
            }
            finally
            {
                _stopProcess = false;
            }
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information($"< [CPU] Returned: OnStopCmdAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: OK");
        Console.ForegroundColor = ConsoleColor.Gray;

        await ValueTask.CompletedTask;
    }

    //private bool apiNbCallCompare(string key)
    //{
    //    // si clé PAS présente on n'attend rien
    //    // si cle présente le nb de retour doit etre iso a celui de l'appel
    //    if (DemandeEtqCompteurDico.Count > 0 && DemandeEtqCompteurDico.TryGetValue(key, out var donelastvalues))
    //    {
    //        if (donelastvalues.Item1 != donelastvalues.Item2)
    //        {
    //            return false;
    //        }
    //    }
    //
    //    return true;
    //}

    /// <summary>
    /// Patch all batch files
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="clientSuffixPath"></param>
    /// <returns></returns>
    public async ValueTask OnAlterCmdAsync(WorkerNode cpu, string clientRelativePath, string clientSuffixPath)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information("> [CPU] Function: OnAlterCmdAsync, Status: Running");
        Console.ForegroundColor = ConsoleColor.Gray;

        var pathFileName = Path.Combine(_clientFilesStorage, clientRelativePath, clientSuffixPath).Replace("\\", "/");

        try
        {
            foreach (var bat in Directory.GetFiles(pathFileName, "*.bat"))
            {
                var batList = new List<string>(await File.ReadAllLinesAsync(bat.Replace("\\", "/")));

                for (int i = 0; i < batList.Count; i++)
                {
                    // Replace WORKSPACE_PATH
                    if (batList[i].Trim().StartsWith("@Set WORKSPACE_PATH", StringComparison.OrdinalIgnoreCase))
                    {
                        // *** ATTENTION *** replace / with \\ because of a MAJOR REPORTIVE BUG => *** ONLY COMPATIBLE WITH WINDOWS ***
                        batList[i] = $"@Set WORKSPACE_PATH={Path.Combine(_clientFilesStorage, clientRelativePath).Replace("/", "\\")}";
                    }

                    // Replace key / values dans chaque bat
                    foreach (var attr in cpu.Demande.ListParamAttributes)
                    {
                        if (batList != null && attr is not null && batList[i].Trim().StartsWith(attr.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            batList[i] = $"{attr.Key}={attr.Value}";
                        }
                    }

                    //Specific Reportive, pour traduire 'statistique_reportive^' :
                    //@echo EXECUTE:statistique_reportive^|00-statistique^|ENDPT-11s37hs8bikav3fs2hl6172ra3>
                    //comme 'E123456^' (id demande)
                    //@echo EXECUTE:E123456^|00-statistique^|ENDPT-11s37hs8bikav3fs2hl6172ra3>
                    //TEST => batList[i] = "@echo EXECUTE:statistique_reportive^|00-statistique^|ENDPT-11s37hs8bikav3fs2hl6172ra3>";
                    if (batList != null && !batList[i].Trim().StartsWith("@echo EXECUTE:", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var pos = batList![i].IndexOf("^|", StringComparison.Ordinal);
                    if (pos > 0)
                    {
                        batList[i] = $"@echo EXECUTE:{clientRelativePath}{batList[i][pos..]}";
                    }
                }

                // Save bat
                await File.WriteAllLinesAsync(bat, batList);
            }

            // launch 2 specific alter file for REPORTIVE (chainage interne) to remove when Reportive decommitting
            await SpecificAlterfileReportiveConf(clientRelativePath);
            await SpecificAlterfileReportiveInputOutput(clientRelativePath);

            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.Information("< [CPU] Returned: OnAlterCmdAsync, Returned: true");
            Console.ForegroundColor = ConsoleColor.Gray;

            cpu.LastCmdResult = true;
        }
        catch (Exception ex)
        {
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
            cpu.LastCmdResult = false;
        }

        await UpdateCpuThenEnchainementAsync(cpu);
    }

    #region adaptefichierSpecificReportive

    /// <summary>
    /// Patch Reportive Specific file  config.ini
    /// </summary>
    /// <param name="clientRelativePath"></param>
    /// <returns></returns>
    private async ValueTask SpecificAlterfileReportiveConf(string clientRelativePath)
    {
        var fileNameWithPath = Path.Combine(_clientFilesStorage, clientRelativePath, "resources", "config.ini").Replace("\\", "/");
        var searchedString = "logical_name=";

        if (File.Exists(fileNameWithPath))
        {
            var fileContent = new List<string>(await File.ReadAllLinesAsync(fileNameWithPath));

            for (int i = 0; i < fileContent.Count; i++)
            {
                if (fileContent[i].Trim().StartsWith(searchedString, StringComparison.OrdinalIgnoreCase))
                {
                    fileContent[i] = $"{searchedString}{clientRelativePath}";
                    break;
                }

                var pos = fileContent[i].IndexOf("^|", StringComparison.Ordinal);
                if (pos > 0)
                {
                    fileContent[i] = $"@echo EXECUTE:{clientRelativePath}{fileContent[i][pos..]}";
                }
            }

            await File.WriteAllLinesAsync(fileNameWithPath, fileContent, Encoding.UTF8);

            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.Information("> [CPU] Function: SpecificAlterfileReportiveConf");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    /// <summary>
    /// Patch Reportive Specific file InputOutput.xml
    /// </summary>
    /// <param name="clientRelativePath"></param>
    /// <returns></returns>
    private async ValueTask SpecificAlterfileReportiveInputOutput(string clientRelativePath)
    {
        // Rechercher dans la chaine comprise entre les 2 x
        //  la chaine :             <Property Name="Full" Value="
        //  et le caractere Pipe
        //  et remplacer 123456 par le numéro de demande concerné
        // x            <Property Name="Full" Value="123456|01_extract_param|Out-177efbq54arq82pa5ppa7b5di1"/>x
        var fileNameWithPath = Path.Combine(_clientFilesStorage, clientRelativePath, "resources", "InputOutput.xml").Replace("\\", "/");
        var searchedString = @"            <Property Name=""Full"" Value=""";

        if (File.Exists(fileNameWithPath))
        {
            var fileContent = new List<string>(await File.ReadAllLinesAsync(fileNameWithPath));

            for (int i = 0; i < fileContent.Count; i++)
            {
                if (!fileContent[i].StartsWith(searchedString, StringComparison.OrdinalIgnoreCase))
                    continue;

                // ligne trouvée
                var pos = fileContent[i].IndexOf("|", StringComparison.Ordinal);
                if (pos > 0)
                {
                    fileContent[i] = $"{searchedString}{clientRelativePath}{fileContent[i][pos..]}";
                }
                break;
            }

            await File.WriteAllLinesAsync(fileNameWithPath, fileContent, Encoding.UTF8);

            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.Information("< [CPU] Returned: SpecificAlterfileReportiveInputOutput, Returned: true");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

    #endregion adaptefichierSpecificReportive

    /// <summary>
    /// Load Qualif.csv file then transmit the whole content within CPU
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="csvFileName"></param>
    /// <returns></returns>
    public async ValueTask OnLoadQualifAsync(WorkerNode cpu, string clientRelativePath, string csvFileName)
    {
        string csvPathFileName = Path.Combine(_clientFilesStorage, clientRelativePath, csvFileName).Replace("\\", "/");

        // nominal case
        cpu.LastCmdResult = true;

        // Question : valable dans tous les cas si le fichier csv n'est pas trouvé on le considère ainsi?
        if (!File.Exists(csvPathFileName))
        {
            cpu.LastCmdError = "NO FILE (normal case: Qualif.csv is optional)";
        }
        else
        {
            // Create instance
            var xlReader = ExcelReader.CreateInstance();
            var qualifCsv = await xlReader.Load<MappingQualifDemande>(csvPathFileName, null);

            // In case of error ExcelReader retains its last exception detailed error message, it is internally try/catched
            cpu.LastCmdError = xlReader.GetLastError();

            if (string.IsNullOrEmpty(cpu.LastCmdError))
            {
                cpu.Demande.JsonData = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(qualifCsv);
            }
            else
            {
                cpu.LastCmdResult = false;
            }
        }

        // Update CPU
        await UpdateCpuThenEnchainementAsync(cpu);
    }

    /// <summary>
    /// Load file from reflog path
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <returns></returns>
    public ValueTask OnLoadRefLogAsync(WorkerNode cpu, string clientRelativePath)
    {
        try
        {
            var jsonLogFiles = Path.Combine(_clientFilesStorage, clientRelativePath).Replace("\\", "/");
            IList<TTL_LOGS> listLogFiles = new List<TTL_LOGS>();

            if (Directory.Exists(Path.GetDirectoryName(jsonLogFiles)))
            {
                foreach (var log in Directory.GetFiles(jsonLogFiles, "*.json"))
                {
                    var json = File.ReadAllText(log);
                    foreach (var data in System.Text.Json.JsonSerializer.Deserialize<IEnumerable<TTL_LOGS>>(json) ?? Enumerable.Empty<TTL_LOGS>())
                    {
                        listLogFiles.Add(data);
                    }
                }
            }

            // We only pass if json files were found using, otherwise null is returned
            cpu.Demande.JsonData = listLogFiles.Any()
                ? System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(listLogFiles)
                : null;

            // Regardless of whether or not we found a json file, there have been no exceptions so far
            cpu.LastCmdResult = true;
        }
        catch (Exception ex)
        {
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
            cpu.LastCmdResult = false;
            cpu.Demande.JsonData = null;
        }

        return UpdateCpuThenEnchainementAsync(cpu);
    }

    /// <summary>
    /// Delete directory
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <returns></returns>
    public ValueTask OnDeleteDirectoryAsync(WorkerNode cpu, string clientRelativePath)
    {
        try
        {
            var path = Path.Combine(_clientFilesStorage, clientRelativePath).Replace("\\", "/");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            cpu.LastCmdResult = true;
        }
        catch (Exception ex)
        {
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
            cpu.LastCmdResult = false;
        }

        // Update CPU
        return UpdateCpuThenEnchainementAsync(cpu);
    }

    /// <summary>
    /// Kill recursively a process
    /// <br />Exemple: OnKillProcessAsync(cpu, "Excel")
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="processName">Process name</param>
    /// <returns></returns>
    public ValueTask OnKillProcessAsync(WorkerNode cpu, string processName)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information("> [CPU] Function: OnKillProcessAsync, Status: Running");
        Console.ForegroundColor = ConsoleColor.Gray;

        try
        {
            var processList = Process.GetProcessesByName(processName)
                .Where(p => !p.CloseMainWindow());

            // Try to close process gracefully, if not, then kill process
            foreach (var process in processList)
            {
                process.CloseMainWindow();
                process.WaitForExit(1000);
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            cpu.LastCmdResult = true;
        }
        catch (Exception ex)
        {
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
            cpu.LastCmdResult = false;
        }

        var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information($"< [CPU] Returned: OnKillProcessAsync, Returned: {lastError}");
        Console.ForegroundColor = ConsoleColor.Gray;

        // Update CPU
        return UpdateCpuThenEnchainementAsync(cpu);
    }

    #endregion

    #region AUTHENTICATION

    /// <summary>
    /// Event just after a CPU has been registered
    /// </summary>
    /// <param name="cpu"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private async ValueTask OnAfterConnectedAsync(WorkerNode cpu)
    {
        // Check if we are authorized or not
        if (!cpu.Authorization.IsAuthorized)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                _logger.Warning($"< [CPU] Returned: OnAfterConnectedAsync, DemandeId: {cpu.Demande?.DemandeId}, Message: {cpu.Authorization.Message}, Id: {cpu.Id} is not authorized!");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                _logger.Warning($"< [CPU] Returned: OnAfterConnectedAsync, DemandeId: {cpu.Demande?.DemandeId}, Status: Gracefully stop application...");
                Console.ForegroundColor = ConsoleColor.Gray;

                // Gracefully stop Hub connection
                _stopConnection = true;
                //await HubConnection.StopAsync(CancellationToken.None);

                // Gracefully shutdown Host https://mcguirev10.com/2020/01/05/lifecycle-of-generic-host-background-services.html
                _appLifetime.StopApplication();
            }
            catch
            {
                // Gracefully shutdown Host https://mcguirev10.com/2020/01/05/lifecycle-of-generic-host-background-services.html
                _appLifetime.StopApplication();
            }
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        _logger.Information("< [CPU] Returned: OnAfterConnectedAsync, Message: Successfully connected");
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        _logger.Information("< [CPU] Returned: OnAfterConnectedAsync, Status: Ready");
        Console.ForegroundColor = ConsoleColor.Gray;

        // Updating cpuBackup with new cpu.Id (coming from SPU) (a reconnection just happened at this point because cpuBackup is not null)
        // UpdateCPU => c'est le tout 1er qui est appelé après que le server ait donné son webSocketId (cpu.Id)
        await UpdateCpuThenEnchainementAsync(cpu);
    }

    /// <summary>
    /// Last event send by CPU services, purpose is to reset CpuBackup
    /// </summary>
    /// <param name="demandeId"></param>
    /// <param name="cpuId"></param>
    /// <returns></returns>
    private void OnAfterFinalizeDemande(int demandeId, string cpuId)
    {
        _logger.Information($"> [CPU] Function: OnAfterFinalizeDemande, DemandeId: {demandeId}, Status: Running");

        if (CpuBackup is not null && CpuBackup.Id.Equals(cpuId, StringComparison.Ordinal)
                                  && CpuBackup.Demande.DemandeId.HasValue
                                  && demandeId == CpuBackup.Demande.DemandeId.Value)
        {
            var total = (DateExtensions.GetUtcNowSecond() - CpuBackup.Demande!.StartedAt)?.ToString(@"hh\:mm\:ss");

            // Recycle Cpu
            CpuBackup = CpuBackup.RecycleCpu();

            _logger.Information($"< [CPU] Returned: OnAfterFinalizeDemande, DemandeId: {demandeId}, Returned: OK");
            _logger.Information($"> [END] --------: DemandeId: {demandeId}, Status: Completed, Time elapsed: {total} (hh:mm:ss)");
        }
    }
    #endregion

    #region RefManager

    private Process? _onRunRefManagerProcess;

    /// <summary>
    /// Run refManager (read or write).jar file.
    /// <br />Expected JAR filename: RefManager_Read.jar or RefManager_Write.jar
    /// <br />Only the client whose RefManager flag is set to true is able to execute the jar file.
    /// </summary>
    /// <param name="cpu">Client instance</param>
    /// <param name="action">Read or Write mode to run the dedicated jar file</param>
    /// <returns></returns>
    public ValueTask OnRefManagerRunJarAsync(WorkerNode cpu, GdbRequestAction action)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information($"> [CPU] Function: OnRefManagerRunJarAsync, Action: {action}, Status: Running");
        Console.ForegroundColor = ConsoleColor.Gray;

        cpu.LastCmdError = null;
        _onRunRefManagerProcess = null;

        // Only the client whose RefManager flag is set to true is able to execute the jar file
        if (!cpu.CanUseRefManagerFeature)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.Information($"< [CPU] Returned: OnRefManagerRunJarAsync, DemandeId: {cpu.Demande.DemandeId}, Returned: Unable to run RefManager on this client!");
            Console.ForegroundColor = ConsoleColor.Gray;

            return ValueTask.CompletedTask;
        }

        // Backup
        CpuRefManagerBackup = System.Text.Json.JsonSerializer.Deserialize<WorkerNode>(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(cpu)) ?? new();

        // Expected JAR filename: RefManager_Read.jar or RefManager_Write.jar
        var jarPathFileName = Directory.GetFiles(_clientRefManagerPath, "*.jar", SearchOption.AllDirectories)
            .FirstOrDefault(f => new FileInfo(f).Name.StartsWith($"RefManager_{action}", StringComparison.CurrentCultureIgnoreCase))
            ?.Replace("\\", "/") ?? "";

        var javaDir = Path.GetDirectoryName(jarPathFileName)?.Replace("\\", "/");

        // Running the jar file for reading or writing?
        var javaArguments = action == GdbRequestAction.Read
            ? (_configuration["ParallelU:Extensions:RefManager:ReadModeArguments"] ?? "").Replace("[javaDir]", javaDir)
            : (_configuration["ParallelU:Extensions:RefManager:WriteModeArguments"] ?? "").Replace("[javaDir]", javaDir);

        // Prepare process parameters
        _onRunRefManagerProcess = new Process
        {
            EnableRaisingEvents = true,
            StartInfo = new ProcessStartInfo
            {
                FileName = "java.exe",
                WorkingDirectory = javaDir,
                Arguments = javaArguments,
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = false
            }
        };

        try
        {
            if (_onRunRefManagerProcess.Start())
            {
                cpu.LastCmdResult = true;
                //  _onRunRefManagerProcess.Id;

                if (CpuRefManagerBackup is not null)
                {
                    CpuRefManagerBackup.LastCmdResult = true;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                _logger.Information($"> [CPU] Function: OnRefManagerRunJarAsync, DemandeId: {cpu.Demande.DemandeId}, ProcessId: {_onRunRefManagerProcess.Id}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        catch (Exception ex)
        {
            cpu.LastCmdError = ex.InnerException?.Message ?? ex.Message;
            cpu.LastCmdResult = false;

            var lastError = $"{(!string.IsNullOrEmpty(cpu.LastCmdError) ? cpu.LastCmdError : "OK")}"; // {lastError}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            _logger.Information($"< [CPU] Returned: OnRefManagerRunJarAsync, DemandeId: {cpu.Demande.DemandeId}, Returned: {lastError}");
            Console.ForegroundColor = ConsoleColor.Gray;

            _onRunRefManagerProcess.Dispose();
            _onRunRefManagerProcess = null;
        }

        // Callback when process exit
        if (_onRunRefManagerProcess != null)
        {
            _onRunRefManagerProcess.Exited += async (sender, _) =>
            {
                if (sender is not Process)
                    return;

                Console.ForegroundColor = ConsoleColor.Cyan;
                _logger.Information($"< [CPU] Returned: OnRefManagerRunJarAsync, DemandeId: {cpu.Demande?.DemandeId}, Returned: {_onRunRefManagerProcess.ExitCode}");
                Console.ForegroundColor = ConsoleColor.Gray;

                _onRunRefManagerProcess.Dispose();
                _onRunRefManagerProcess = null;

                // une coupure reseau peut avoir lieu pendant l exécution du bat dont on doit reprendre
                // la version backupé pour avoir le bon socketid qui aura été mis à jour lors de le reconnexion
                // on aura mis a jour les infos liées a ce bat en particulier avant renvoi au central
                if (CpuRefManagerBackup is not null)
                {
                    //if (!_stopProcess) // => Only if process was not killed from UI (TODO: revoir cette portion de code)
                    {
                        CpuRefManagerBackup.LastCmdResult = cpu.LastCmdResult;
                        CpuRefManagerBackup.LastCmdError = cpu.LastCmdError;
                    }
                }

                // Invoke OnAfterRunRefManagerJarAsync callback
                var retCode = await HubConnection!.InvokeCoreAsync<bool>("OnAfterRunRefManagerJar", new object[] { cpu });

                // When successfull, then reset the backup to default structure
                if (retCode)
                    CpuRefManagerBackup = new WorkerNode();
            };
        }

        return ValueTask.CompletedTask;
    }

    #endregion

    #region PRIVATE

    /// <summary>
    /// Log message from the console
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private ValueTask LogAsync(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        _logger.Information($"> [CPU] Messages: {message}");
        Console.ForegroundColor = ConsoleColor.Gray;

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Check network availability
    /// </summary>
    /// <returns></returns>
    private void CheckNetworkAvailability() => Thread.Sleep(new Random().Next(2, 5) * 1000);

    #endregion
}