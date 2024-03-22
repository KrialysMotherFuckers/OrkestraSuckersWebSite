using Krialys.Common.Interfaces;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Web.Common.ApiClient;
using Microsoft.Extensions.Hosting.WindowsServices;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Krialys.Orkestra.ParallelU;

internal static class AssemblyExtensions
{
    public static byte[] ReadResource(this Assembly assembly, string resourceName)
    {
        var result = new byte[0];

        // Determine path
        string resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceName));

        using (var stream = assembly.GetManifestResourceStream(resourcePath))
            if (stream != null)
                using (var reader = new BinaryReader(stream))
                    result = reader.ReadBytes((int)stream.Length);

        return result;
    }
}

public interface IAutoUpdater : IScopedService
{
    Task<bool> Start();
}

public class AutoUpdater : IAutoUpdater
{
    private readonly Serilog.ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private readonly ITechnicalClient _iTechnicalClient;

    public AutoUpdater(Serilog.ILogger logger, IConfiguration configuration, IHttpClientFactory httpClientFactory, ITechnicalClient iTechnicalClient)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(Litterals.ApplicationCpuClient);
        _configuration = configuration;
        _iTechnicalClient = iTechnicalClient;
    }

    /// <summary>
    /// TODO: check if no job is running before updating!
    /// </summary>
    /// <returns></returns>
    private async ValueTask<bool> CheckUpdate()
    {
        var result = false;

        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null)
        {
            var actualVersion = assembly.GetName().Version;
            var fileInfo = new FileInfo(assembly.Location);

            if (actualVersion != null && fileInfo != null)
                result = await _iTechnicalClient.OrkestraNodeWorkerUpdateCheckAsync(
                                    actualVersion.Major,
                                    actualVersion.Minor,
                                    actualVersion.Build,
                                    actualVersion.Revision,
                                    fileInfo.CreationTimeUtc);
        }

        return result;
    }

    public async Task<bool> Start()
    {
        return await Task.FromResult(false); //'till feature completed...

        var result = false;
        if (await CheckUpdate())
        {

            Console.ForegroundColor = ConsoleColor.Red;
            _logger.Information($"< [SYS] Update found.");

            string pathUpdateFile;
            string pathFiletoUpdate;

            using (var fileStream = await _iTechnicalClient.OrkestraNodeWorkerGetUpdateAsync())
                if (fileStream != null)
                {
                    _logger.Information($"< [SYS] Update downloading...");

                    pathFiletoUpdate = System.Reflection.Assembly.GetEntryAssembly()!.Location.Replace(".dll", ".exe");
                    pathUpdateFile = await _iTechnicalClient.OrkestraNodeWorkerFileNameGetAsync();
                    pathUpdateFile = Path.Combine(Globals.AssemblyDirectory, pathUpdateFile);

                    if (File.Exists(pathUpdateFile)) File.Delete(pathUpdateFile);

                    fileStream.Stream.Flush();
                    using (System.IO.Stream streamToWriteTo = File.Open(pathUpdateFile, FileMode.Create))
                        await fileStream.Stream.CopyToAsync(streamToWriteTo);

                    _logger.Information($"< [SYS] Update Downloaded...");

                    ExecuteUpdater(pathUpdateFile, pathFiletoUpdate);

                    result = true;
                }

        }

        return await Task.FromResult(result);
    }

    public void ExecuteUpdater(string pathFileUpdate, string pathFileToUpdate, string resourceName = "Krialys.Orkestra.AutoUpdater")
    {
        _logger.Information($"< [SYS] Execute Updater...");

        Thread.Sleep(2000);

        // Actually an non self executable needs at least EXE + DLL and muche more in fact...// NonSense when we see the //U.exe, there is just the .exe..No Dll with. So NoneSense
        var fullPathNameExe = Path.Combine(Globals.AssemblyDirectory, "Updater", $"{resourceName}.exe");
        var serviceName = string.Empty;
        var processId = string.Empty;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            processId = currentProcess.Id.ToString();

            if (WindowsServiceHelpers.IsWindowsService())
            {
                var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Service where ProcessId = {processId}");
                var collection = searcher.Get();
                serviceName = (string)collection.Cast<ManagementBaseObject>().First()["Name"];
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Do something
        }

        System.Diagnostics.Process.Start
         (
             new System.Diagnostics.ProcessStartInfo()
             {
                 FileName = fullPathNameExe,
                 Arguments = $"update {pathFileUpdate} {pathFileToUpdate} {processId} {serviceName}",

                 UseShellExecute = true,

                 RedirectStandardError = false,
                 RedirectStandardInput = false,
                 RedirectStandardOutput = false,

                 WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
                 WorkingDirectory = Path.GetFullPath(pathFileToUpdate),
             }
         )!.Start();
    }
}