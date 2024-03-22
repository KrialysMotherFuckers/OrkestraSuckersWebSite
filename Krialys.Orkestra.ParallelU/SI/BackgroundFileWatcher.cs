using Krialys.Orkestra.Common.Constants;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Krialys.Orkestra.ParallelU.SI;

public sealed class BackgroundFileWatcher : BackgroundService, IAsyncDisposable
{
    private const string AuthenticationToken = "GetAuthenticationToken";

    private readonly string _directoryPath = default!;
    private readonly string _destinationDirectory = default!;
    private readonly Serilog.ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string? _baseUrl = default!;

    // Supported extensions
    private readonly string[] _filters = { "*.csv", "*.json", "*.yml" };
    private FileSystemWatcher _watcher = default!;

    public BackgroundFileWatcher(Serilog.ILogger logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient(Litterals.ApplicationCpuClient);
        _baseUrl = _httpClient.BaseAddress?.ToString();
        if (!string.IsNullOrEmpty(_baseUrl))
            _baseUrl = _baseUrl.EndsWith("/") ? _baseUrl : _baseUrl + "/";

        try
        {
            _directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_FileWatcher").Replace("\\", "/");
            if (!System.IO.Directory.Exists(_directoryPath)) System.IO.Directory.CreateDirectory(_directoryPath);

            _destinationDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_FileWatcher/Output").Replace("\\", "/");
            if (!System.IO.Directory.Exists(_destinationDirectory)) System.IO.Directory.CreateDirectory(_destinationDirectory);
        }
        catch (Exception exception)
        {
            _logger.Error($"< [CFW] Returned: BackgroundFileWatcher, Error: {exception.Message}");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Create a new FileSystemWatcher instance
            using (_watcher = new FileSystemWatcher(_directoryPath))
            {
                // Watch for changes in the directory, including new file creations
                _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                foreach (var filter in _filters)
                    _watcher.Filters.Add(filter);

                // Subscribe to the event handlers
                _watcher.Created += OnFileCreatedAsync;
                _watcher.Error += OnWatcherError;

                // Start watching
                _watcher.EnableRaisingEvents = true;

                // Wait until the service is stopped
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(15000, stoppingToken); // Adjust the delay as per your needs
                }
            }
        }
        catch (TaskCanceledException)
        {
            // When the stopping token is canceled, for example, a call made from services.msc,
            // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            _logger.Error($"> [BFW] Function: ExecuteAsync, Error: Task canceled");
        }
        catch (Exception ex)
        {
            _logger.Error($"> [BFW] Function: ExecuteAsync, Error: {ex.Message}");

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

    /// <summary>
    /// Dispose all subcribed callbacks to avoid memory leaks
    /// </summary>
    public ValueTask DisposeAsync()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        _logger.Information($"< [CFW] Stopping: FileWatcher, Message: Successfully disconnected");

        if (_watcher != null)
        {
            // Unsubscribe to the event handlers
            _watcher.Created -= OnFileCreatedAsync;
            _watcher.Error -= OnWatcherError;

            _watcher.Dispose();
        }

        return ValueTask.CompletedTask;
    }

    private async void OnFileCreatedAsync(object sender, FileSystemEventArgs e)
    {
        // Ignore directories and ensure the file is not in use (not locked)
        if (!File.Exists(e.FullPath))// || IsFileLocked(e.FullPath))
            return;

        _logger.Information($"> [CFW] Observed: {e.Name}");

        // Wait for the file to be completely written
        WaitForFileToBeCompletelyWritten(e.FullPath);

        try
        {
            // Name can be processed again if it contains a guid as prefix + separator
            // Valid input: 000055_EtqCalculate.json, 000055 EtqCalculate.json, 000055$EtqCalculate.json, 000055¤EtqCalculate.json or 35daadde9e784587b7cbe80adf42f5a6¤000055¤EtqCalculate.json
            var name = e.Name ?? "";
            if (name.Length > 32 && name[32] == '¤')
                name = name[33..];
            // Give a unique destination name. The file can contain ' ', '_', '$' or '¤' separator
            var pos = name.Length > 5 && (name[6] == ' ' || name[6] == '_' || name[6] == '¤' || name[6] == '$') ? 7 : 6;
            var newName = $"{Guid.NewGuid().ToString("N")}¤{name[..6]}¤{name[pos..]}";
            var elements = newName.Split('¤', StringSplitOptions.RemoveEmptyEntries);

            // Move the file to the destination directory
            var destinationFilePath = Path.Combine(_destinationDirectory, newName).Replace("\\", "/");

            // Move the file to the destination directory
            File.Move(e.FullPath, destinationFilePath);

            // Now, the file should be completely written and moved to the destination.
            _logger.Information($"< [CFW] MovingTo: {destinationFilePath}.");

            // Now, we can safely handle the moved file
            await OnAfterFileMovedAsync(destinationFilePath);
        }
        catch (Exception ex)
        {
            // Handle any exceptions that might occur while moving the file
            _logger.Error($"< [CFW] Error: {ex.Message}");
        }
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        // Handle any errors that might occur with the FileSystemWatcher
        _logger.Error($"< [CFW] Watcher error: {e.GetException().Message}");
    }

    private void WaitForFileToBeCompletelyWritten(string filePath)
    {
        const int maxWaitTimeMs = 30000; // Maximum time to wait for the file to be completely written (30 seconds)
        const int checkIntervalMs = 500; // Time interval to check the file's last write time (0.5 seconds)

        int elapsedTime = 0;

        while (IsFileLocked(filePath) && elapsedTime < maxWaitTimeMs)
        {
            Thread.Sleep(checkIntervalMs);
            elapsedTime += checkIntervalMs;
        }

        if (elapsedTime >= maxWaitTimeMs)
        {
            // Handle the case when the file is taking too long to be written.
            _logger.Error("< [CFW] File took too long to be completely written.");
        }
    }

    private bool IsFileLocked(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // The file is not locked if we can open it with exclusive access.
                }
            }
            return false;
        }
        catch (IOException)
        {
            // The file is locked if an exception is thrown while trying to open it.
            _logger.Warning("< [CFW] The file is locked.");
            return true;
        }
    }

    #region Business rules
    private async Task OnAfterFileMovedAsync(string destinationFilePath)
    {
        if (!File.Exists(destinationFilePath) || IsFileLocked(destinationFilePath))
        {
            _logger.Error($"< [CFW] Error: {destinationFilePath} file is locked.");
            return;
        }

        try
        {
            var token = await GetAuthenticationToken();
            //_logger.Information($"> [CFW] Token: {token}");
        }
        catch (Exception ex)
        {
            _logger.Error($"{ex.Message} => {_httpClient.BaseAddress?.ToString()}");
        }
    }

    private async Task<string?> GetAuthenticationToken()
    {
        var result = await GetApiCatalogResultAsync(AuthenticationToken);

        return result?[0]?.ToString();
    }
    #endregion

    #region Internal classes
#nullable disable
    /// <summary>
    /// Get ApiCatalog array of results
    /// </summary>
    /// <param name="functionId"></param>
    /// <returns>Result[]</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private async Task<object[]> GetApiCatalogResultAsync(string functionId)
    {
        var requestUri = $"{_baseUrl}api/catalog/{functionId}?applicationId=Krialys.Etl&apiVersion=v1";
        var json = await _httpClient.GetStringAsync(requestUri);

        return !string.IsNullOrEmpty(json)
            ? JsonSerializer.Deserialize<GenericApiCatalogResponse>(json)?.Value.Result
            : default;
    }

    internal class GenericApiCatalogResponse
    {
        public string TraceId { get; init; }
        public string Function { get; init; }
        public string Version { get; init; }
        public bool Success { get; init; }
        public int StatusCode { get; init; }
        public object ErrorMessage { get; init; }
        public Value Value { get; init; }
    }

    internal class Value
    {
        public object[] Result { get; init; }
        public int Count { get; init; }
    }
#nullable restore
    #endregion
}