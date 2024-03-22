using Krialys.Common.Extensions;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Models.WorkerNode;
using Krialys.Orkestra.WebApi.Services.System;
using Krialys.Orkestra.WebApi.Services.System.HUB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using static Krialys.Orkestra.Common.Shared.ETQ;
using static Krialys.Orkestra.Common.Shared.Univers;

namespace Krialys.Orkestra.WebApi.Controllers.Common;

/// <summary>
/// Implements dedicated controller for PARALLELEU
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class CpuController : ControllerBase
{
    private readonly ICpuServices _service;
    private readonly IHubContext<SPUHub> _hubContext;
    private readonly ICpuConnectionManager _cpuManager;
    private readonly Serilog.ILogger _logger;

    public CpuController(Serilog.ILogger logger, ICpuServices service, IHubContext<SPUHub> hubContext,
        ICpuConnectionManager cpuManager)
    {
        _logger = logger;
        _service = service;
        _hubContext = hubContext;
        _cpuManager = cpuManager;
    }

    // URL test #1 : http://localhost:8000/api/univers/v1/CPU/MainEntryPoint?off=0
    [Produces(Litterals.ApplicationJson)]
    [HttpGet("MainEntryPoint")]
    public async Task<IActionResult> MainEntryPointAsync([FromQuery(Name = "off")] int off = 0,
        [FromQuery(Name = "firstTime")] bool firstTime = true)
    {
        try
        {
            await _service.MainEntryPointAsync(off);

            return Ok();
        }
        catch
        {
            return BadRequest("Failed to start MainEntryPointAsync"); // cpu is corrupted or null
        }
    }

    [ApiExplorerSettings(IgnoreApi = false)]
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [HttpPost("GetCpuStatus")]
    public Task<bool> GetCpuStatus()
        => Task.FromResult(_cpuManager.Started);

    [ApiExplorerSettings(IgnoreApi = false)]
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [HttpPost("StopCmd")]
    public async Task<bool> StopCmd([FromQuery(Name = "connectionid")] string connectionId)
        => await _service.StopCmdAsync(connectionId);

    /// <summary>
    /// Update CPU that is coming from CPUWorker (client)        
    /// </summary>
    /// <returns></returns>
    [Produces(Litterals.ApplicationJson)]
    [HttpPut("UpdateCpu")]
    public IActionResult UpdateCpu([FromBody] string cpu)
    {
        try
        {
            _ = _cpuManager.UpdateCpu(JsonSerializer.Deserialize<WorkerNode>(cpu));

            return Ok();
        }
        catch
        {
            return BadRequest(); // cpu is corrupted or null
        }
    }

    ///// <summary>
    ///// URL test #1 : http://localhost:8000/api/univers/v1/CPU/Enchainement?connectionid=zyz
    ///// </summary>
    ///// <param name="connectionid : ID l'instance paralleleUClient"></param>
    ///// <returns></returns>
    [Produces(Litterals.ApplicationJson)]
    [HttpPut("Enchainement")]
    public async Task<IActionResult> Enchainement([FromQuery(Name = "connectionid")] string connectionId)
    {
        // constate derniere exécution du client et annonce la prochaine méthode a appeller
        try
        {
            await _service.Enchainement(connectionId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed on: Enchainement {0}", ex.InnerException?.Message ?? ex.Message);
            return BadRequest();
        }
    }

    #region ETL APIs

    ///// URL test #1 : http://localhost:8000/api/univers/v1/CPU/EtqCalculate
    /// <summary>
    /// Calcul de 1 à N étiquette version API
    /// </summary>
    /// <param name="etiquetteInput"></param>
    /// <returns></returns>
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EtqOutput>))]
    [HttpPost("EtqCalculate")]
    public async Task<IActionResult> EtqCalculate([FromBody] IEnumerable<CalculateEtqInput> etiquetteInput)
    {
        try
        {
            return Ok(await _service.EtqGenerate(etiquetteInput));
        }
        catch (Exception ex)
        {
            return Ok(new List<EtqOutput>
            {
                new EtqOutput
                    { Success = false, Message = ex.InnerException?.Message ?? ex.Message, Source = "EtqCalculate" }
            });
        }
    }

    /// <summary>
    /// Injection des données Suivi ressource d'une étiquette 
    /// 1 a N enregistrements pour une etiquette et N etq sur plusieurs Guid
    /// </summary>
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EtqOutput>))]
    [HttpPost("EtqSuiviRessource")]
    public async Task<IActionResult> EtqSuiviRessource([FromBody] IEnumerable<EtqSuiviRessourceFileRaw> suiviRessources)
    {
        try
        {
            return Ok(await _service.EtqSuiviRessource(suiviRessources));
        }
        catch (Exception ex)
        {
            return Ok(new List<EtqOutput>
            {
                new EtqOutput
                {
                    Success = false, Message = ex.InnerException?.Message ?? ex.Message, Source = "EtqSuiviRessource"
                }
            });
        }
    }

    ///// URL test #1 : http://localhost:8000/api/univers/v1/CPU/EtqExtraInfoAddon
    /// <summary>
    /// Injection des données de libellés d'une étiquette 
    /// 1 a N enregistrements pour une etiquette
    /// </summary>
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EtqOutput>))]
    [HttpPost("EtqExtraInfoAddon")]
    public async Task<IActionResult> EtqExtraInfoAddon([FromBody] IEnumerable<EtqExtraInfoAddonFileRaw> extraInfosAddon)
    {
        try
        {
            return Ok(await _service.EtqExtraInfoAddon(extraInfosAddon));
        }
        catch (Exception ex)
        {
            return Ok(new List<EtqOutput>
            {
                new EtqOutput
                {
                    Success = false, Message = ex.InnerException?.Message ?? ex.Message, Source = "EtqExtraInfoAddon"
                }
            });
        }
    }

    /// <summary>
    /// Applique les regles sur une Etiquette
    /// </summary>
    /// <param name="etqRules"></param>
    /// <returns></returns>
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EtqOutput>))]
    [HttpPost("EtqApplyRules")]
    public async Task<IActionResult> EtqApplyRules([FromBody] IEnumerable<EtqRules> etqRules)
    {
        try
        {
            var etq = etqRules?.FirstOrDefault();

            await _service
                .EtqAppliqueRegles(etq?.ActeurId, etq!.TETQ_ETIQUETTEID, etq.TETQR_ETQ_REGLEID,
                    etq.TRGLRV_REGLES_VALEURID_PARENT,
                    etq.TRGLRV_REGLES_VALEURID, etq.TOBJE_OBJET_ETIQUETTEID, etq.COMMENT);

            return Ok(new List<EtqOutput>
            {
                new EtqOutput { Success = true, Message = "", Source = "EtqApplyRules" }
            });
        }
        catch (Exception ex)
        {
            return Ok(new List<EtqOutput>
            {
                new EtqOutput
                {
                    Success = false, Message = $"{ex.InnerException?.Message ?? ex.Message}", Source = "EtqApplyRules"
                }
            });
        }
    }

    #endregion

    #region AUTHORIZATION RULES (DTF)

    /// <summary>
    /// Liste des Etats eligibles avec role de producteur
    /// URL test #1 : http://localhost:8000/api/univers/v1/CPU/GetExecutableTeEtatsForDTF?isAdminMode=false,userId=1catId=1
    /// </summary>
    /// <param name="isAdminMode"></param>
    /// <param name="userId"></param>
    /// <param name="catId"></param>
    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TE_ETATS>))]
    [HttpPost("GetExecutableTeEtatsForDTF")]
    public IActionResult GetExecutableTeEtatsForDTF([FromQuery(Name = "isAdminMode")] bool isAdminMode,
        [FromQuery(Name = "userId")] string userId, [FromQuery(Name = "catId")] int catId)
        => Ok(_service.GetExecutableTeEtatsForDTF(isAdminMode, userId, catId));

    #endregion

    #region CALENDAR (DTF)

    [Authorize]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ModeleDemandeCalendar>))]
    [HttpPost("GetTheoricalDemandesCalendar")]
    public IActionResult GetTheoricalDemandesCalendar([FromQuery(Name = "dtUtcStart")] DateTimeOffset dtUtcStart,
        [FromQuery(Name = "dtUtcEnd")] DateTimeOffset dtUtcEnd)
        => Ok(_service.GetTheoricalDemandesCalendar(dtUtcStart, dtUtcEnd));

    #endregion

    #region TEST GEG

    /// <summary>
    /// Startup function
    /// URL test: http://localhost:8000/api/univers/v1/CPU/CPUCheckPoint?connectionId=xyz
    /// URL test: http://apiunivers.krialys.net/apiunivers/api/univers/v1/CPU/CPUCheckPoint?connectionId=xyz
    /// </summary>
    /// <returns></returns>
    [Produces(Litterals.ApplicationJson)]
    [HttpGet("CPUCheckPoint")]
    public async Task<WorkerNode> CPUCheckPointAsync([FromQuery(Name = "connectionid")] string connectionId) // Provided by and for CPU
    {
        //_cpuManager.GetOnlineConnections

        //await Task.Delay(3000);

        // Actual CPU instance
        var cpuNow = _cpuManager.GetCpuInstance(connectionId);

        // 1st time ptd_demandeid == null because the connectionId just arrives with available == true
        if (cpuNow is not null && cpuNow.IsFree.HasValue)
        {
            // Est appelé ici la toute première fonction qui pourrait retourner une DemandeId si dicponible
            if (cpuNow.IsFree.Value)
            {
                // TODO Seb
                // Vérifier si éligible et retourner un numéro de demande... étape 0 du workflow => passer le flag free à false
                cpuNow.IsFree = false;

                // TODO Seb
                // if (Cpu.dispo) // retourne liste de demandes.FirstOrDefault()
                //   var numDemande = call("GetDataDemandesATraiterParParallelU", "ServiceName");

                // TODO Seb en retour il faut réserver le numéro de demande
                //    var count = call "ConfirmePriseEnChargeDemande"(numDemande, ServiceName);

                //cpuNow.CPUDemandes.DemandeId = 1;

                //// Add BATCH list => OK
                //cpuNow.CPUDemandes.ListCPUBatchsAttributes = new KeyValuePair<string, IList<CPUBatchsAttributes>>(nameof(CPUBatchsAttributes), new List<CPUBatchsAttributes>
                //{
                //    new () { OrdreExecution = 1, NomFichier = "Toto.bat" },
                //    new () { OrdreExecution = 2, NomFichier = "Titi.bat" },
                //});

                //// Test => OK
                ////cpuNow.RecycleCPU();

                ////await _service.MainEntryPoint();

                //// CreateDirectory test => OK
                //await _service.CreateDirectoryAsync(cpuNow, "zz/yy/xx");

                //// DownloadFile test => OK
                //await _service.DownloadFileAsync(cpuNow, "C:/SolutionKdata/ENV/env_vierge", "000001", "E000001.zip");

                // LoadRefLogAsync test => OK
                //await _service.LoadRefLogAsync(cpuNow, "000001");

                //// Upload test => OK
                await _service.UploadFileAsync(cpuNow, "C:/SolutionKdata/ENV/env_vierge/zozo", "000001", "1GB.bin");

                //// Zip test => OK
                //await _service.DirectoryZipAsync(cpuNow, "000001");

                //// Unzip test => OK
                //await _service.DirectoryUnZipAsync(cpuNow, "000002", "E000001.zip", "000002");

                // Send a message to the CPU
                //await _hubContext.Clients.Client(cpuNow.Id).SendCoreAsync("OnMessageAsync",
                //       new object[] { $"[CTL] CPUCheckPoint: DemandeId:{(cpuNow.CpuDemandes.DemandeId == null ? " **No DemandeId yet**" : cpuNow.CpuDemandes)} assigned to CPU: {cpuNow.Name}, Machine: {cpuNow.Machine}, Id: {cpuNow.Id}" });
            }
            // Est appelé lorsque la 1ère étape a été validée et que le CPU est pas available
            else
            {
                // TODO Seb : appeler ici la suite du workflow, isFree == false tant que le workflow n'est pas terminé

                // Send a message to the CPU
                await _hubContext.Clients.Client(cpuNow.Id).SendCoreAsync("OnMessageAsync",
                    new object[]
                    {
                        $"[CTL] CPUCheckPoint: DemandeId:{cpuNow.Demande.DemandeId} workflow is complete for CPU: {cpuNow.Name}, Machine: {cpuNow.Machine}, Id: {cpuNow.Id}"
                    });

                // TODO Seb : uniquement si le workflow est 'terminé', afin de rendre le CPU disponible à nouveau
                cpuNow.IsFree = true;
                cpuNow.Demande.DemandeId = null;
                cpuNow.StartedAt = DateExtensions.GetLocaleNow();
            }

            // Always update CPU
            return _cpuManager.UpdateCpu(cpuNow);
        }

        return null;
    }

    #endregion TEST GEG
}