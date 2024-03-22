using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Models.WorkerNode;
using Krialys.Orkestra.WebApi.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;
using static Krialys.Orkestra.Common.Shared.Logs;

namespace Krialys.Orkestra.WebApi.Services.System.HUB;

/// <summary>
/// ParallelU Server side - (main entry point) - DI is done thanks to Apiunivers for this file - all other controllers have access thanks to Injection mechanism
/// </summary>
public sealed class SPUHub : Hub
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICpuConnectionManager _cpuConnectionManager;
    private readonly KrialysDbContext _dbContext;
    private readonly Serilog.ILogger _logger;
    private readonly ICpuServices _cpu;
    private readonly ICommonServices _commonServices;
    private readonly (string userId, string userName) _user;

    //private readonly string _envVierge;
    //private readonly JsonSerializerOptions _jsonSerializerOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };

    public SPUHub(ICpuServices cpu, ICpuConnectionManager cpuConnectionManager,
        IHttpContextAccessor httpContextAccessor, KrialysDbContext dbContext, Serilog.ILogger logger, ICommonServices commonServices)
    {
        _cpuConnectionManager = cpuConnectionManager ?? throw new ArgumentNullException(nameof(cpuConnectionManager));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        //_envVierge = configuration.GetValue<string>("ParallelU:PathEnvVierge");
        _cpu = cpu;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commonServices = commonServices;

        // Identified userId and userName
        _user = _commonServices.GetUserIdAndName();
    }

    /// <summary>
    /// When a user has just been connected, its name is given as URL parameter
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        if (_httpContextAccessor.HttpContext != null)
        {
            var cpuName = _httpContextAccessor.HttpContext.Request.Query["cpuname"];
            var machine = _httpContextAccessor.HttpContext.Request.Query["machine"];

            var serviceName = _httpContextAccessor.HttpContext.Request.Query[nameof(WorkerNodeExt.ServiceName)];
            var serverName = _httpContextAccessor.HttpContext.Request.Query[nameof(WorkerNodeExt.ServerName)];
            var fileName = _httpContextAccessor.HttpContext.Request.Query[nameof(WorkerNodeExt.FileName)];
            var version = _httpContextAccessor.HttpContext.Request.Query[nameof(WorkerNodeExt.Version)];
            var serverOs = _httpContextAccessor.HttpContext.Request.Query[nameof(WorkerNodeExt.ServerOs)];
            var folder = _httpContextAccessor.HttpContext.Request.Query[nameof(WorkerNodeExt.Folder)];
            var workingFilesStorage = _httpContextAccessor.HttpContext.Request.Query[nameof(WorkerNodeExt.WorkingFilesStorage)];
            //var hDDInfo = _httpContextAccessor.HttpContext.Request.Query[nameof(NodeServiceInfo.HDDInfo)];
            //var process = _httpContextAccessor.HttpContext.Request.Query[nameof(NodeServiceInfo.Process)];
            var refManager = _httpContextAccessor.HttpContext.Request.Query[nameof(WorkerNodeExt.CanUseRefManagerFeature)];

            var cpu = _cpuConnectionManager.AddConnection(
                                                new WorkerNode(Context.ConnectionId, cpuName, machine)
                                                {
                                                    ServiceName = serviceName,
                                                    ServerName = serverName,
                                                    FileName = fileName,
                                                    Version = version,
                                                    ServerOs = serverOs,
                                                    Folder = folder,
                                                    WorkingFilesStorage = workingFilesStorage,
                                                    //HDDInfo = null, //JsonConvert.DeserializeObject<DriveInfo[]>(hDDInfo),
                                                    //Process = null, //JsonConvert.DeserializeObject<Process>(process),
                                                    CanUseRefManagerFeature = Convert.ToBoolean(refManager)
                                                });

            if (cpu.IsFree.HasValue && cpu.IsFree.Value)
            {
                await Task.Delay(250);

                // Check if CPU exists from the global CPUList + use chkCpuInDb below + write to DB if necessary
                ChkCpuInDb(cpu.Name, cpu.Machine);

                // Client will execute
                await Clients.Client(cpu.Id).SendCoreAsync("OnAfterConnectedAsync", new object[] { cpu });
            }
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Vérifie qu'une instance //U client est bien déclaré et affecté à au moins une ferme
    /// ce qui le rend eligible a recevoir des demandes
    /// </summary>
    /// <returns></returns>
    /// <param name="pTpuInstance">Non de l'instance ParalleleU</param>
    /// <param name="pHostname">nom d'Hote hébergeant l'instance</param>

    private void ChkCpuInDb(string pTpuInstance, string pHostname)
    {
        string pMsg = string.Empty;
        bool autorize = true;

        // TODO : ma liste de cpu 'actifs'
        var cpus = _cpuConnectionManager.GetOnlineConnections(pTpuInstance);

        // Check if a CPU is already there (only one is expected) 
        if (cpus.Count > 1)
        {
            // non autorisé si déja présent mais sous un autre ID 
            var cpuSameSignature = cpus.FirstOrDefault(x => x.Id != Context.ConnectionId);

            if (cpuSameSignature is not null)
            {
                pMsg = $"Non conforme - nom d'instance déjà utilisé sur {cpuSameSignature.Machine}";
                autorize = false;
            }

            //// Liste de tous les CPUs autorisés
            //var cpuAuthorized = cpus.Where(x => x.Authorization.IsAuthorized);

            //// Liste de ceux non autorisés
            //var cpuNotAuthorized = cpus.Where(x => !x.Authorization.IsAuthorized);
        }

        if (autorize) // == not blocked by multi CPU
        {
            // le grouping utilisé est assimilable a un left join entre TPU_PARALLELEU et TSRV_SERVEUR
            var infoInstance = from TPU_PARALLELEU in _dbContext.TPU_PARALLELEUS
                               join TSRV_SERVEUR in _dbContext.TSRV_SERVEURS on TPU_PARALLELEU.TSRV_SERVEURID equals TSRV_SERVEUR.TSRV_SERVEURID into grouping
                               from TSRV_SERVEUR in grouping.DefaultIfEmpty()
                               where TPU_PARALLELEU.TPU_INSTANCE.ToUpper() == pTpuInstance.ToUpper()
                               select new { TPU_PARALLELEU, TSRV_SERVEUR };

            // infoInstance.count() =1 => instance trouvée
            // verif statut instance
            // verif serveur de rattachement identique a celui réellement utilisé
            // verif au moins une ferme active associé
            if (!infoInstance.Any())
            {
                pMsg = "Non conforme - nom d'instance inconnu";
                autorize = false;
            }
            else
            {
                if (infoInstance.FirstOrDefault()!.TPU_PARALLELEU.TRST_STATUTID != StatusLiteral.Available)
                {
                    pMsg = "Non conforme - Instance connue mais non active";
                    autorize = false;
                }
                else
                {
                    if (!string.Equals(infoInstance.FirstOrDefault()!.TSRV_SERVEUR.TSRV_NOM, pHostname, StringComparison.CurrentCultureIgnoreCase))
                    {
                        pMsg = "Instance connue mais non affectée sur ce serveur";
                        autorize = false;
                    }
                    else
                    {
                        var tpu_parallelu = (from TPU_PARALLELEU in _dbContext.TPU_PARALLELEUS
                                             join tpufParalleleuFerme in _dbContext.TPUF_PARALLELEU_FERMES on TPU_PARALLELEU.TPU_PARALLELEUID equals tpufParalleleuFerme.TPU_PARALLELEUID
                                             join tfFerme in _dbContext.TF_FERMES on tpufParalleleuFerme.TF_FERMEID equals tfFerme.TF_FERMEID
                                             where
                                             tfFerme.TRST_STATUTID == StatusLiteral.Available
                                             && tpufParalleleuFerme.TRST_STATUTID == StatusLiteral.Available
                                             && TPU_PARALLELEU.TPU_INSTANCE.ToUpper() == pTpuInstance.ToUpper()
                                             select TPU_PARALLELEU).FirstOrDefault();

                        if (tpu_parallelu is null)
                        {
                            pMsg = "Non conforme - Instance et Serveur connus mais non affecté à une ferme Active";
                            autorize = false;
                        }
                    }
                }
            }
        }

        // Update CPU message if error
        cpus.FirstOrDefault(x => Context != null && x.Id == Context.ConnectionId)!.Authorization.IsAuthorized = autorize;

        if (!autorize)
        {
            cpus.FirstOrDefault(x => x.Id == Context.ConnectionId)!.Authorization.Message = pMsg + $" pour {pTpuInstance} sur {pHostname}";
            cpus.FirstOrDefault(x => x.Id == Context.ConnectionId)!.Authorization.Action = CPULitterals.OnCLientShutdown;
            //  _logger.Warning($"Satellite ParalleleU non eligible pour traiter des demandes: {pTPU_INSTANCE} sur {pHostname}, raison: {pMsg}");
        }
        // Update CPU within cpus
        _cpuConnectionManager.UpdateCpu(cpus.FirstOrDefault());
        // Update DB when necessary
    }

    /// <summary>
    /// When a user is disconnecting
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override Task OnDisconnectedAsync(Exception exception)
    {
        _cpuConnectionManager.RemoveConnection(Context.ConnectionId);
        Console.Write($@"> [SPU] [{DateTime.Now}] - OnDisconnectedAsync Id: {Context.ConnectionId}");

        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Send notification to all online users, see SendMessageAsync for more details
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendMessageToAllAsync(string message)
    {
        var onlineBots = _cpuConnectionManager.GetOnlineInstances?.ToList() ?? new List<string>();

        foreach (var onlineBot in onlineBots)
            await SendMessageAsync(onlineBot, message);
    }

    /// <summary>
    /// Update Client structure
    /// </summary>
    /// <param name="cpu"></param>
    public WorkerNode UpdateMappedConnectionAsync(WorkerNode cpu) => _cpuConnectionManager.UpdateCpu(cpu);

    /// <summary>
    /// Send notification for an explicit user if useConnectionId = true, else notifications are sent to all online clients
    /// </summary>
    /// <param name="botName">CPU User</param>
    /// <param name="message">Message to deliver to CPU</param>
    /// <returns></returns>
    public async Task<Task> SendMessageAsync(string botName, string message)
    {
        try
        {
            var connections = _cpuConnectionManager.GetOnlineConnections(botName);

            if (connections is not { Count: > 0 })
                return Task.CompletedTask;

            foreach (var conn in connections)
            {
                try
                {
                    await Clients.Client(conn.Id).SendCoreAsync("OnMessageAsync", new object[] { message });
                }
                catch
                {
                    throw new Exception($"ERROR: No connection found for: {botName}");
                }
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"{ex.Message} {ex.InnerException?.Message}";

            // Log error
            _commonServices.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, null);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Download file => used by Client to download a file (by chunks) stored on the Server
    /// </summary>
    /// <param name="fullPathName"></param>
    /// <param name="fileName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<byte[]> DownloadStreamAsync(string fullPathName, string fileName, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (File.Exists($"{Path.Combine(fullPathName)}/{fileName}"))
        {
            FileStream fileStream = new($"{Path.Combine(fullPathName)}/{fileName}", FileMode.Open, FileAccess.Read);

            for (var bytesRemaining = fileStream.Length; bytesRemaining > 0;)
            {
                // Check the cancellation token regularly so that the server will stop producing items if the client disconnects.
                cancellationToken.ThrowIfCancellationRequested();

                var chunk = new byte[(int)Math.Min(bytesRemaining, WorkerNode.BufferSize)];
                bytesRemaining -= await fileStream.ReadAsync(chunk.AsMemory(0, chunk.Length), cancellationToken);

                yield return chunk;
            }

            fileStream.Close();
        }
    }

    /// <summary>
    /// Upload stream => used by Client to upload stream (by chunks) onto the Server
    /// TODO : call a method back telling the updalod was successful
    /// </summary>
    /// <param name="clientStream"></param>
    /// <param name="clientFileName"></param>
    /// <param name="fullServerPath"></param>
    /// <returns></returns>
    public async Task UploadStreamAsync(IAsyncEnumerable<byte[]> clientStream, string clientFileName, string fullServerPath)
    {
        string outputPath = $"{Path.Combine(fullServerPath)}/{clientFileName}";

        try
        {
            // Create dir if not exist
            Directory.CreateDirectory(Path.Combine(fullServerPath).Replace("\\", "/"));

            // Delete file if present before creating a new file
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            using var cts = new CancellationTokenSource();
            await using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);

            await foreach (var chunk in clientStream.WithCancellation(cts.Token))
            {
                await fs.WriteAsync(chunk, cts.Token);
            }

            await fs.FlushAsync(cts.Token);
            var isEmpty = fs.Length < 1;
            fs.Close();

            if (isEmpty)// && File.Exists(outputPath))
            {
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"{ex.Message} {ex.InnerException?.Message}";

            // Log error
            _commonServices.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, null);
        }
    }

    /// <summary>
    /// [UPDATE] Update CPU via ICpuConnectionManager then invoke ICpuServices Enchainement (DEMANDES)
    /// </summary>
    /// <param name="cpu"></param>
    /// <returns>true if success, else false</returns>
    public async Task<bool> OnUpdateCpuThenEnchainement(WorkerNode cpu)
    {
        try
        {
            // Update Cpu
            var newCpu = _cpuConnectionManager.UpdateCpu(cpu);

            if (newCpu != null && !string.IsNullOrEmpty(newCpu.Id))
            {
                // Continue workflow
                await _cpu.Enchainement(newCpu.Id);

                return true;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"{ex.Message} {ex.InnerException?.Message}";

            // Log error
            _commonServices.StdLogException(new LogException(GetType(), ex), _user.userId, _user.userName, null);
        }

        return false;
    }

    /// <summary>
    /// Callback invoked after refManager has completed its JAR execution (REFMANAGER)
    /// </summary>
    /// <param name="cpu"></param>
    /// <returns>Nothing</returns>
    public Task<bool> OnAfterRunRefManagerJar(WorkerNode cpu)
    {
        if (cpu.IsRefManagerRunning)
        {
            cpu.IsRefManagerRunning = false;

            _logger.Information($"< -------- GdbRequest: the WorkerNode {cpu.Id} has successufully ran RefManager jar");

            // Update Cpu
            _ = _cpuConnectionManager.UpdateCpu(cpu);

            // TODO : update la table TM_RFS_ReferentialSettings + histo pour débloquer les flags

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}