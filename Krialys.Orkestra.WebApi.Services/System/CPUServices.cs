using Cronos;
using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Logs;
using Krialys.Data.EF.Mso;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Models.WorkerNode;
using Krialys.Orkestra.Common.Shared;
using Krialys.Orkestra.WebApi.Services.Authentification;
using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.Data;
using Krialys.Orkestra.WebApi.Services.System.HUB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog.Events;
using System.Globalization;
using System.Text;
using System.Text.Json;
using TimeZoneConverter;
using static Krialys.Orkestra.Common.Shared.ETQ;
using static Krialys.Orkestra.Common.Shared.Logs;
using static Krialys.Orkestra.Common.Shared.Univers;
using KrialysDbContext = Krialys.Data.EF.Univers.KrialysDbContext;

namespace Krialys.Orkestra.WebApi.Services.System;

/// <summary>
/// Implements IGenericCRUDService for Client PARALLELU
/// </summary>
public interface ICpuServices : IScopedService
{
    Task Enchainement(string connectionId);
    Task MainEntryPointAsync(int off = 0);

    Task UploadFileAsync(WorkerNode cpu, string fullServerPath, string clientRelativePath, string fileName);
    // Etiquette : Genere un code etq selon les criteres (ajout en bdd hors mode simulation)
    Task<EtqOutput> EtqCalculate(string guid, string codeEtq, int? version, string codePerimetre, string valDynPerimetre, int demandeid, string source, bool pSimul);
    Task<IEnumerable<EtqOutput>> EtqGenerate(IEnumerable<CalculateEtqInput> etiquetteInput);
    // Etiquette : enregistre en BDD dans Suivi Ressource      
    Task<IEnumerable<EtqOutput>> EtqSuiviRessource(IEnumerable<EtqSuiviRessourceFileRaw> suiviRessources);

    Task EtqAppliqueRegles(string pActeurId, int pTETQ_ETIQUETTEID, int pTETQR_ETQ_REGLEID,
        int? pTRGLRV_REGLES_VALEURID_PARENT, int pTRGLRV_REGLES_VALEURID, int pTOBJE_OBJET_ETIQUETTEID,
        string pCommentaire = "");

    Task<IEnumerable<EtqOutput>> EtqExtraInfoAddon(IEnumerable<EtqExtraInfoAddonFileRaw> extraInfosAddon);

    // DTF : new production
    IEnumerable<TE_ETATS> GetExecutableTeEtatsForDTF(bool isAdminMode, string userId, int catId);

    // DTF : calcul des demandes théoriques via décodage du CRON par demande éligibles
    IEnumerable<ModeleDemandeCalendar> GetTheoricalDemandesCalendar(DateTimeOffset dtUtcStart, DateTimeOffset dtUtcEnd);

    /// <summary>
    /// Maintenance: stop a running process
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns></returns>
    ValueTask<bool> StopCmdAsync(string connectionId);

    ValueTask CmdCheckLifePhaseEveryDay();

    ValueTask LogsCheckForCleanupInformation();
}

/// <summary>
/// Implements IGenericCRUDService for Client PARALLELU
/// </summary>
public class CpuServices : ICpuServices
{
    private readonly KrialysDbContext _dbContext;
    private readonly Krialys.Data.EF.Mso.KrialysDbContext _dbContextMso;
    private readonly Krialys.Data.EF.Etq.KrialysDbContext _dbContextEtq;
    private readonly Krialys.Data.EF.Logs.KrialysDbContext _dbContextLogs;
    private readonly Serilog.ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly ICpuConnectionManager _cpuManager;
    private readonly IHubContext<SPUHub> _hubContext;
    private readonly ITrackedEntitiesServices _trackedEntitiesServices;
    private readonly string _metaSeparator;
    private readonly string _sessionId;
    private readonly IOAuthServices _authServices;
    private readonly IEmailServices _emailServices;
    private readonly ICommonServices _commonService;
    private (string userId, string userName) _connectedUser;
    private readonly IOrdersServices _ordersServices;

    // As far as we use a callback via a threaded timer, we have to set it as static (will be visible from any threads)
    private static Timer MainEntryTimer { get; set; } = null!;
    private static SemaphoreSlim _cpuSemaphore = new SemaphoreSlim(1, 1);

    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private static string _administratorMail;

    public CpuServices(KrialysDbContext dbContext,
       Krialys.Data.EF.Mso.KrialysDbContext dbContextMso,
        Krialys.Data.EF.Etq.KrialysDbContext dbContextEtq,
        Krialys.Data.EF.Logs.KrialysDbContext dbContextLogs,
        Serilog.ILogger logger, IConfiguration configuration,
        ICpuConnectionManager cpuManager, IHubContext<SPUHub> hubContext,
        ITrackedEntitiesServices trackedEntitiesServices, ICommonServices commonService,
        IHttpContextAccessor httpContextAccessor, IOAuthServices authServices,
        IEmailServices emailServices, IOrdersServices orderService, IHostApplicationLifetime hostApplicationLifetime)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext)); // accès complet à DBUnivers
        _dbContextMso = dbContextMso ?? throw new ArgumentNullException(nameof(dbContextMso)); // accès complet à MSO
        _dbContextEtq = dbContextEtq ?? throw new ArgumentNullException(nameof(dbContextEtq)); // accès complet à ETQ
        _dbContextLogs = dbContextLogs ?? throw new ArgumentNullException(nameof(dbContextLogs)); // accès complet à LOGS
        _logger = logger; // permet de tracer les erreurs dans la base DBLogs
        _configuration = configuration; // accès à toutes les variables de appsettings json de apiunivers
        _cpuManager = cpuManager; // accès à tous les CPU
        _hubContext = hubContext; // accès au SPU
        _trackedEntitiesServices = trackedEntitiesServices; // accès au tracking
        _commonService = commonService;
        _authServices = authServices;
        _emailServices = emailServices;
        _ordersServices = orderService;
        _hostApplicationLifetime = hostApplicationLifetime;

        // Session Id comes from startup and has been injected to Headers, ensuring
        _sessionId = httpContextAccessor.HttpContext?.Request.Headers[Litterals.ApplicationClientSessionId].ToString();

        // Meta file character separator globally used, with a fallback value if not precised
        _metaSeparator = _configuration["ParallelU:MetaSeparator"] ?? "¤";
    }

    /// <summary>
    /// CPU logger for Error, Warning (with tracking) and Information levels (without tracking)
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="cpuNow"></param>
    /// <param name="functionName"></param>
    /// <param name="lastError"></param>
    /// <param name="exception"></param>
    private async Task CpuLogger(LogEventLevel logLevel, WorkerNode cpuNow, string functionName, string lastError = null,
        Exception exception = null)
    {
        static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;
        }

        if (!string.IsNullOrEmpty(lastError) || exception is not null)
        {
            WorkerNodeLog workerNodeLog;
            switch (logLevel)
            {
                case LogEventLevel.Error:
                    {
                        workerNodeLog = exception is not null
                            ? new WorkerNodeLog(cpuNow, functionName, lastError ?? exception.Message)
                            : new WorkerNodeLog(cpuNow, functionName, lastError);

                        if (exception is not null)
                        {
                            var user = _commonService.GetUserIdAndName();
                            _commonService.StdLogException(new LogException(GetType(), exception), user.userId, user.userName, lastError ?? cpuNow.LastCmdError);
                        }
                        else
                        {
                            _commonService.CpuLogException(LogEventLevel.Error, cpuNow.Demande.DemandeId, lastError ?? exception.Message, workerNodeLog);
                        }

                        // track pour aider refresh des interfaces abonnées
                        string cpuLogValues = JsonSerializer.Serialize(workerNodeLog);
                        await _trackedEntitiesServices.AddTrackedEntitiesAsync(null, new[] { typeof(TM_LOG_Logs).FullName }, Litterals.LogError, cpuNow.Id, EncodeTo64(cpuLogValues));
                        break;
                    }

                case LogEventLevel.Warning:
                    {
                        workerNodeLog = new WorkerNodeLog(cpuNow, functionName, lastError ?? cpuNow.LastCmdError);

                        _commonService.CpuLogException(LogEventLevel.Warning, cpuNow.Demande.DemandeId, nameof(LogEventLevel.Warning), workerNodeLog);

                        // track pour aider refresh des interfaces abonnées
                        string cpuLogValues = JsonSerializer.Serialize(workerNodeLog);
                        await _trackedEntitiesServices.AddTrackedEntitiesAsync(null, new[] { typeof(TM_LOG_Logs).FullName }, Litterals.LogWarning, cpuNow.Id, EncodeTo64(cpuLogValues));
                        break;
                    }

                default:
                    workerNodeLog = new WorkerNodeLog(cpuNow, functionName, lastError ?? cpuNow.LastCmdError);

                    _commonService.CpuLogException(LogEventLevel.Information, cpuNow.Demande.DemandeId, nameof(LogEventLevel.Information), workerNodeLog);
                    break;
            }
        }
    }

    private void OnAfterMainEntry(int off)
    {
        const int interval = 15000;

        async void Callback(object obj)
        {
            await _cpuSemaphore.WaitAsync();
            try { await MainEntryPointAsync(off); }
            catch (Exception ex) { _logger.Error(ex, "< -------- Failed: OnAfterMainEntry. {Message}", ex.Message); }
            finally { _cpuSemaphore.Release(); }
        }

        MainEntryTimer = new Timer(Callback, null, TimeSpan.FromMilliseconds(interval), TimeSpan.FromMilliseconds(interval));
    }

    /// <summary>
    /// Main entry point controlled by semaphore that prevents from thread reentrance
    /// </summary>
    /// <param name="off">0 means Start, all other values means Suspend all activities</param>
    public async Task MainEntryPointAsync(int off = 0)
    {
        while (true)
        {
            switch (off == 0)
            {
                // Run
                case true:
                    if (!_cpuManager.Started) // 1st time: starts //U central + stamp as started
                    {
                        _cpuManager.Started = true;

                        if (MainEntryTimer != null)
                        {
                            const int interval = 15000;

                            // Restart the timer                        
                            MainEntryTimer.Change(TimeSpan.FromMilliseconds(interval), TimeSpan.FromMilliseconds(interval));
                        }
#if RELEASE // => OB-286 Send email code here when server is dis/connected (run on IIS, but fails with Kestrel!)
                    _administratorMail = _dbContext.TRU_USERS
                        .FirstOrDefault(e => e.TRU_LOGIN.ToUpper() == Litterals.KADMIN) // MUST be shipped with an email, see ticket OB-322
                        ?.TRU_EMAIL ?? null;

                    // Should notify as well when the server has been connected
                    _hostApplicationLifetime.ApplicationStarted.Register(async () =>
                    {
                        var server = _configuration["Environment"]?.ToLower() ?? "xxx";
                        var body = $"<span>The <b>orkestra-data-{server}@{Environment.MachineName}</b> Web server has started at: <b>{DateTime.UtcNow:r}</b>.</span><br /><br /><span>Please contact your local administrator for further informations.</span>";

                        if (!string.IsNullOrEmpty(_administratorMail))
                        {
                            await _emailServices.SendGenericMail(new Orkestra.Common.Models.Email.EmailTemplate
                            {
                                From = $"Orkestra Noreply <{_configuration["MailKit:SMTP:Sender"]}>",
                                Subject = $"[Alert|orkestra-data-{server}@{Environment.MachineName}] The Web server has started",
                                To = _administratorMail,
                                Body = body,
                                TextFormatHtml = true,
                                Importance = Orkestra.Common.Models.Email.Importance.High,
                                Priority = Orkestra.Common.Models.Email.Priority.High,
                            });
                        }
                        else
                        {
                            _logger.Warning("[ApplicationStarted] User 'KADMIN' has no email configured!");
                        }
                    });

                    // Should notify as well when the server has been disconnected
                    _hostApplicationLifetime.ApplicationStopped.Register(async () =>
                    {
                        var server = _configuration["Environment"]?.ToLower() ?? "xxx";
                        var body = $"<span>The <b>orkestra-data-{server}@{Environment.MachineName}</b> Web server has stopped at: <b>{DateTime.UtcNow:r}</b>.</span><br /><br /><span>Please contact your local administrator for further informations.</span>";

                        if (!string.IsNullOrEmpty(_administratorMail))
                        {
                            await _emailServices.SendGenericMail(new Orkestra.Common.Models.Email.EmailTemplate
                            {
                                From = $"Orkestra Noreply <{_configuration["MailKit:SMTP:Sender"]}>",
                                Subject = $"[Alert|orkestra-data-{server}@{Environment.MachineName}] The Web server has stopped",
                                To = _administratorMail,
                                Body = body,
                                TextFormatHtml = true,
                                Importance = Orkestra.Common.Models.Email.Importance.High,
                                Priority = Orkestra.Common.Models.Email.Priority.High,
                            });
                        }
                        else
                        {
                            _logger.Warning("[ApplicationStopped] User 'KADMIN' has no email configured!");
                        }
                    });
#endif
                        // Identified userId and userName
                        _connectedUser = _commonService.GetUserIdAndName();
                        var message = $"> The CPU service has been {(_connectedUser.userName is null ? "automatically" : "manually")} started by {_connectedUser.userName ?? _sessionId ?? nameof(CpuServices)}";

                        if (string.IsNullOrEmpty(_connectedUser.userName))
                            _logger.Information(message);
                        else
                            _logger.Warning(message);

                        using var cts = new CancellationTokenSource();
                        await Task.Run(PUCentralConfirmeDemandesAnnulees, cts.Token)
                            .ContinueWith(async prev =>
                            {
                                if (prev.IsCompleted) await PUCentralReinitaliseDemandes();
                            }, cts.Token);
                    }
                    else
                    {
                        /* Actions indépendantes des CPU connectés */
                        using var cts = new CancellationTokenSource();
                        await Task.Run(PUCentralConfirmeDemandesAnnulees, cts.Token)
                            .ContinueWith(async prev =>
                            {
                                if (prev.IsCompleted) await PUCentralAnalysePrepareDemandesEligibles();
                            }, cts.Token);

                        // At least one CPU running, else no need to check DB
                        if (_cpuManager is not null && _cpuManager.GetOnlineInstances.Any() && _cpuManager
                                .GetOnlineConnections(_cpuManager.GetOnlineInstances.FirstOrDefault()).Any())
                        {
                            PUCentralGestionFermetureCPU(); // on gere l'info liée à l'arret normal ou prématuré d'un client
                            await PUCentral_Gestion_Distribution_demande_CPU(); // on gere la distribution de demande pour exécution immediate aux // connectés et libres
                        }
                    }

                    if (MainEntryTimer is null)
                    {
                        OnAfterMainEntry(0);
                    }

                    break;

                // Suspend
                case false:
                    if (_cpuManager.Started)
                    {
                        _cpuManager.Started = false;

                        if (MainEntryTimer is not null)
                        {
                            // Suspend the timer
                            MainEntryTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                        }

                        // Identified userId and userName
                        _connectedUser = _commonService.GetUserIdAndName();
                        var message = $"< The CPU service has been {(_connectedUser.userName is null ? "automatically" : "manually")} suspended by {_connectedUser.userName ?? _sessionId}";

                        if (string.IsNullOrEmpty(_connectedUser.userName))
                            _logger.Information(message);
                        else
                            _logger.Warning(message);

                        using var cts = new CancellationTokenSource();
                        await Task.Run(PUCentralConfirmeDemandesAnnulees, cts.Token)
                            .ContinueWith(async prev =>
                            {
                                if (prev.IsCompleted) await PUCentralReinitaliseDemandes();
                            }, cts.Token);

                        off = 1;
                        continue;
                    }

                    break;
            }
            break;
        }
    }

    #region LANCEMENT INITIAL

    /// <summary>
    /// Remettre à 0 les demandes potentiellement eligibles et non traitées mais qui ne sont pas en mémoire dans le pool apiunivers
    /// </summary>
    /// <returns></returns>
    private Task PUCentralReinitaliseDemandes()
    {
        return _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContext.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");

                foreach (var item in from tdDemande in _dbContext.TD_DEMANDES
                                     where
                                         tdDemande.TRST_STATUTID == StatusLiteral.ScheduledRequest &&
                                         (
                                             from teEtat in _dbContext.TE_ETATS
                                             join temEtatMaster in _dbContext.TEM_ETAT_MASTERS on teEtat.TEM_ETAT_MASTERID equals temEtatMaster.TEM_ETAT_MASTERID
                                             where teEtat.TRST_STATUTID == StatusLiteral.Available || teEtat.TRST_STATUTID == StatusLiteral.Prototype
                                             select teEtat.TE_ETATID
                                         ).Contains(tdDemande.TE_ETATID)
                                     select
                                         tdDemande
                        )
                {
                    item.TRST_STATUTID = StatusLiteral.CreatedRequestAndWaitForExecution;
                    _ = _dbContext.TD_DEMANDES.Update(item);
                }

                _ = await _dbContext.SaveChangesAsync();
                await dbCTrans.CommitAsync();
            }
            catch //(Exception ex)
            {
                await dbCTrans.RollbackToSavepointAsync("Step_001");
            }
        });
    }

    #endregion "lancement initial"

    /// <summary>
    /// Anti-con pour empêcher une réentrance de thread sur le précédent qui n'aurait pas terminé sa boucle de calculs 
    /// </summary>
    private static bool _stop;

    /// <summary>
    /// Identifie les modeles de demandes ayant des CRONS pour créer les instances de demandes non encore créés a l execution précédente
    /// </summary>
    /// <returns></returns>
    private async Task PUCentralAnalysePrepareDemandesEligibles()
    {
        if (_stop == false)
        {
            _stop = true;
            var utcNow = DateExtensions.GetUtcNow();

            //implémentation lecture du CRON
            await PUCentralConfirmeDemandesAnnulees();
            /* POSTULA : DATE DEBUT ET DATE FIN sont  en GMT de l'utilisateur, le champ TPF_TIMEZONE_INFOID etant utilisé pour convertir en UTC
            */
            var listeModeledemande = (from
                        TE_ETAT in _dbContext.TE_ETATS
                                      join TD_DEMANDE in _dbContext.TD_DEMANDES on TE_ETAT.TE_ETATID equals TD_DEMANDE.TE_ETATID
                                      join TPF_PLANIF in _dbContext.TPF_PLANIFS on TD_DEMANDE.TD_DEMANDEID equals TPF_PLANIF.TPF_DEMANDE_ORIGINEID
                                      where TPF_PLANIF.TRST_STATUTID == StatusLiteral.Available
                                      join TS_SCENARIO in _dbContext.TS_SCENARIOS on TD_DEMANDE.TS_SCENARIOID equals TS_SCENARIO.TS_SCENARIOID
                                      where TS_SCENARIO.TRST_STATUTID == StatusLiteral.Available
                                      join TEM_ETAT_MASTER in _dbContext.TEM_ETAT_MASTERS on TE_ETAT.TEM_ETAT_MASTERID equals TEM_ETAT_MASTER.TEM_ETAT_MASTERID
                                      where TEM_ETAT_MASTER.TRST_STATUTID == StatusLiteral.Available &&
                                            TD_DEMANDE.TRST_STATUTID == StatusLiteral.ScheduleModel && (TE_ETAT.TRST_STATUTID == StatusLiteral.Available || TE_ETAT.TRST_STATUTID == StatusLiteral.Prototype)
                                            &&
                                            (from temfEtatMasterFerme in _dbContext.TEMF_ETAT_MASTER_FERMES
                                             join tfFerme in _dbContext.TF_FERMES on temfEtatMasterFerme.TF_FERMEID equals tfFerme.TF_FERMEID
                                             where tfFerme.TRST_STATUTID == StatusLiteral.Available
                                             join tpufParalleleuFerme in _dbContext.TPUF_PARALLELEU_FERMES on tfFerme.TF_FERMEID equals tpufParalleleuFerme.TF_FERMEID
                                             where tpufParalleleuFerme.TRST_STATUTID == StatusLiteral.Available
                                             join tpuParalleleu in _dbContext.TPU_PARALLELEUS on tpufParalleleuFerme.TPU_PARALLELEUID equals tpuParalleleu.TPU_PARALLELEUID
                                             where tpuParalleleu.TRST_STATUTID == StatusLiteral.Available
                                             select temfEtatMasterFerme.TEM_ETAT_MASTERID
                                            ).Contains(TEM_ETAT_MASTER.TEM_ETAT_MASTERID)
                                      /* where TD_DEMANDE.TD_DEMANDEID == 84  ----------------TEST */
                                      select new ModeleDemande
                                      {
                                          TD_DEMANDEID = TD_DEMANDE.TD_DEMANDEID,
                                          TE_ETATID = TD_DEMANDE.TE_ETATID,
                                          TD_COMMENTAIRE_UTILISATEUR = TD_DEMANDE.TD_COMMENTAIRE_UTILISATEUR,
                                          TS_SCENARIOID = TD_DEMANDE.TS_SCENARIOID,
                                          TRU_DEMANDEURID = TD_DEMANDE.TRU_DEMANDEURID,
                                          TD_SEND_MAIL_GESTIONNAIRE = TD_DEMANDE.TD_SEND_MAIL_GESTIONNAIRE,
                                          TD_SEND_MAIL_CLIENT = TD_DEMANDE.TD_SEND_MAIL_CLIENT,
                                          TPF_PLANIFID = TPF_PLANIF.TPF_PLANIFID,
                                          TPF_CRON = TPF_PLANIF.TPF_CRON,
                                          TPF_DATE_DEBUT = TPF_PLANIF.TPF_DATE_DEBUT,
                                          TPF_DATE_FIN = TPF_PLANIF.TPF_DATE_FIN,
                                          TPF_DEMANDE_ORIGINEID = TPF_PLANIF.TPF_DEMANDE_ORIGINEID,
                                          TPF_PREREQUIS_DELAI_CHK = TPF_PLANIF.TPF_PREREQUIS_DELAI_CHK,
                                          TPF_TIMEZONE_INFOID = TPF_PLANIF.TPF_TIMEZONE_INFOID
                                      }).AsEnumerable()
                .Where(a => utcNow >= DateExtensions.ConvertToUTC(a.TPF_DATE_DEBUT, a.TPF_TIMEZONE_INFOID) && utcNow <=
                    (a.TPF_DATE_FIN.HasValue
                        // on autorise une date de fin non rensigné a.TPF_DATE_FIN. Dans ce cas la on utilise valeur virtuelle de fin ex = now + 60jours
                        ? DateExtensions.ConvertToUTC(a.TPF_DATE_FIN.Value, a.TPF_TIMEZONE_INFOID)
                        : utcNow.AddDays(60)))
                ?.ToList();

            foreach (var itemPlanifmodeleDemandeUnitaire in listeModeledemande)
            {
                try
                {
                    // Timezone offset of the planif (for time "now").
                    var startDateOffset = TZConvert.GetTimeZoneInfo(itemPlanifmodeleDemandeUnitaire.TPF_TIMEZONE_INFOID).GetUtcOffset(DateTimeOffset.Now);

                    // Start date is "now" converted from server timezone to planif timezone.
                    var debut = DateTimeOffset.Now.ToOffset(startDateOffset).Truncate(TimeSpan.FromMinutes(1)); // Fix to make last CRON eligible

                    // If end date exists in db, end date is converted from undefined to planif timezone.
                    // Otherwise end date is start date plus sixty days.
                    DateTimeOffset fin;
                    if (itemPlanifmodeleDemandeUnitaire.TPF_DATE_FIN.HasValue)
                    {
                        // Timezone offset of the planif (for time TPF_DATE_FIN).
                        var endDateOffset = TZConvert
                            .GetTimeZoneInfo(itemPlanifmodeleDemandeUnitaire.TPF_TIMEZONE_INFOID)
                            .GetUtcOffset(itemPlanifmodeleDemandeUnitaire.TPF_DATE_FIN.Value);

                        fin = new DateTimeOffset(itemPlanifmodeleDemandeUnitaire.TPF_DATE_FIN.Value, endDateOffset)
                            .Truncate(TimeSpan.FromMinutes(1)).AddMinutes(1); // Fix to make last CRON eligible
                    }
                    else
                    {
                        fin = debut.AddDays(60);
                    }

                    // CRON Sample :
                    // "*/15 * * * *" toutes les 15 minutes
                    // "10 13 ? * MON,TUE,WED,THU,FRI" a 13H10 du lundi au vendredi
                    // "10 13 ? * MON-FRI" a 13H10 du lundi au vendredi,n variante du précédent
                    // cas qui n'est PAS compatible : "0 10 13 ? * MON-FRI" : la précision des secondes (0) ou (*) fait planter le parser de CRON

                    //var cronTEST1 = " 0 10 13 ? * MON,TUE,WED,THU,FRI";
                    //var cronTEST2 = "0 15 10 ? * MON-FRI";
                    // var cronTEST3 = "*/15 * * * *";
                    var tz = TZConvert.GetTimeZoneInfo(itemPlanifmodeleDemandeUnitaire.TPF_TIMEZONE_INFOID);
                    DateTimeOffset dateUserTZProchaineOccurence = default;

                    var prochaineOccurences = debut <= fin
                        ? CronExpression.Parse(itemPlanifmodeleDemandeUnitaire.TPF_CRON).GetOccurrences(debut, fin, tz)
                        : Enumerable.Empty<DateTimeOffset>();

                    var dateTimeOffsets = prochaineOccurences as DateTimeOffset[] ?? prochaineOccurences.ToArray();
                    if (prochaineOccurences is not null && dateTimeOffsets.Any())
                    {
                        dateUserTZProchaineOccurence = dateTimeOffsets.First();
                    }

                    if (dateUserTZProchaineOccurence == default)
                    {
                        continue;
                    }

                    // la date générée par GetNextOccurrences est basée sur une heure UTC
                    var dateProchaineOccurence = DateExtensions.ConvertToUTC(
                        dateUserTZProchaineOccurence.DateTime,
                        itemPlanifmodeleDemandeUnitaire.TPF_TIMEZONE_INFOID);

                    // si date de prochaine exécution rentre bien dans la période de validité
                    //if (dateProchaineOccurence >= DateExtensions.ConvertToUTC(
                    //        itemPlanifmodeleDemandeUnitaire.TPF_DATE_DEBUT,
                    //        itemPlanifmodeleDemandeUnitaire.TPF_TIMEZONE_INFOID)
                    //    && dateProchaineOccurence <= fin)
                    //{
                    // controle si instance de demande est deja présent
                    // si oui rien a faire
                    // si non  :
                    //           on ajoute l instance de demande
                    //           on ajoute les batchs

                    // on verifie si l'instance de demande a déja été créée précédemment
                    var chkInstanceDemande = (from tdDemande in _dbContext.TD_DEMANDES
                                              where tdDemande.TD_DEMANDE_ORIGINEID == itemPlanifmodeleDemandeUnitaire.TD_DEMANDEID
                                                    && tdDemande.TPF_PLANIF_ORIGINEID == itemPlanifmodeleDemandeUnitaire.TPF_PLANIFID
                                                    && tdDemande.TRST_STATUTID != StatusLiteral.PlanningCancelled
                                              select new
                                              {
                                                  TD_DEMANDE = tdDemande
                                              }).AsEnumerable()
                        .Where(a => a.TD_DEMANDE.TD_DATE_EXECUTION_SOUHAITEE.HasValue && a.TD_DEMANDE.TD_DATE_EXECUTION_SOUHAITEE.Value.Equals(dateProchaineOccurence));

                    if (!chkInstanceDemande.Any())
                    {
                        await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
                        {
                            await _dbContext.Database.OpenConnectionAsync();
                            await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

                            try
                            {
                                await dbCTrans.CreateSavepointAsync("Step_001");

                                // DEBUT TRANSACTION
                                string myguid = Guid.NewGuid().ToString("N"); // un Guid pour chaque nouvelle demande

                                await _dbContext
                                    .AddAsync( //new List<TD_DEMANDES> { // si on met List il dit que List<TD_DEMANDES> n'existe pas dasn Entities à l'exécution
                                        new TD_DEMANDES
                                        {
                                            TE_ETATID = itemPlanifmodeleDemandeUnitaire.TE_ETATID,
                                            TD_DATE_DEMANDE = utcNow,
                                            TD_DATE_EXECUTION_SOUHAITEE = dateProchaineOccurence, //.Value,  // verifier format
                                            TD_COMMENTAIRE_UTILISATEUR = itemPlanifmodeleDemandeUnitaire.TD_COMMENTAIRE_UTILISATEUR,
                                            TD_SEND_MAIL_GESTIONNAIRE = itemPlanifmodeleDemandeUnitaire.TD_SEND_MAIL_GESTIONNAIRE,
                                            TD_SEND_MAIL_CLIENT = itemPlanifmodeleDemandeUnitaire.TD_SEND_MAIL_CLIENT,
                                            TRST_STATUTID = StatusLiteral.CreatedRequestAndWaitForExecution, // attention statut DC si insertion des batch dans la meme transaction sinon mettre DB et refaire une maj du statut en DC a la fin
                                            TRU_DEMANDEURID = itemPlanifmodeleDemandeUnitaire.TRU_DEMANDEURID,
                                            TD_GUID = myguid,
                                            TS_SCENARIOID = itemPlanifmodeleDemandeUnitaire.TS_SCENARIOID,
                                            TD_DEMANDE_ORIGINEID = itemPlanifmodeleDemandeUnitaire.TD_DEMANDEID,
                                            TPF_PLANIF_ORIGINEID = itemPlanifmodeleDemandeUnitaire.TPF_PLANIFID,
                                            TD_PREREQUIS_DELAI_CHK = itemPlanifmodeleDemandeUnitaire.TPF_PREREQUIS_DELAI_CHK
                                        });

                                await _dbContext.SaveChangesAsync();

                                //pour chaque instance de demande on créé ses batchs
                                // creera de 1 a N enregistrements
                                foreach (var item in from tebEtatBatch in _dbContext.TEB_ETAT_BATCHS
                                                     join tbsBatchScenario in _dbContext.TBS_BATCH_SCENARIOS on tebEtatBatch.TEB_ETAT_BATCHID equals tbsBatchScenario.TEB_ETAT_BATCHID
                                                     join tsScenario in _dbContext.TS_SCENARIOS on tbsBatchScenario.TS_SCENARIOID equals tsScenario.TS_SCENARIOID
                                                     where tsScenario.TRST_STATUTID == StatusLiteral.Available
                                                     join tdDemande in _dbContext.TD_DEMANDES on tsScenario.TS_SCENARIOID equals tdDemande.TS_SCENARIOID
                                                     where tdDemande.TD_GUID == myguid
                                                     select new
                                                     {
                                                         tdDemande.TD_DEMANDEID,
                                                         tebEtatBatch.TE_ETATID,
                                                         tbsBatchScenario.TBS_ORDRE_EXECUTION,
                                                         tebEtatBatch.TEB_ETAT_BATCHID
                                                     })
                                {
                                    await _dbContext.AddAsync(
                                        new TBD_BATCH_DEMANDES
                                        {
                                            TD_DEMANDEID = item.TD_DEMANDEID,
                                            TE_ETATID = item.TE_ETATID,
                                            TBD_ORDRE_EXECUTION = item.TBS_ORDRE_EXECUTION,
                                            TBD_EXECUTION = StatusLiteral.Yes,
                                            TEB_ETAT_BATCHID = item.TEB_ETAT_BATCHID
                                        });
                                }

                                await _dbContext.SaveChangesAsync();

                                // track pour aider refresh des interfaces abonnées
                                await _trackedEntitiesServices.AddTrackedEntitiesAsync(null,
                                    new[] { typeof(VDE_DEMANDES_ETENDUES).FullName }, Litterals.Insert,
                                    _sessionId, "PUCentralAnalysePrepareDemandesEligibles");

                                await dbCTrans.CommitAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "< -------- Failed: PUCentralAnalysePrepareDemandesEligibles. {Message}", ex.InnerException?.Message ?? ex.Message);
                                await dbCTrans.RollbackToSavepointAsync("Step_001");
                            }
                        });

                        // FIN TRANSACTION
                    } // fin si pas trouvé
                    //}
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"< -------- Failed: PUCentralAnalysePrepareDemandesEligibles. Message: {0} on TPF_PLANIFID {1}", ex.InnerException?.Message ?? ex.Message, itemPlanifmodeleDemandeUnitaire.TPF_PLANIFID);
                }
            }

            _stop = false;
        }
    }

    /// <summary>
    /// Gere la distribution de demande aux //U connectés et libre
    /// </summary>
    /// <returns></returns>
    private async Task PUCentral_Gestion_Distribution_demande_CPU()
    {
        var onlineInstances = _cpuManager.GetOnlineInstances?.ToList() ?? new List<string>();

        // Récupère les instances des //U (nom du //U)
        foreach (var cpu in onlineInstances
                     .Select(instance => _cpuManager.GetOnlineConnections(instance)?.ToList() ?? new List<WorkerNode>())
                     .Where(cpus => cpus.Any())
                     .SelectMany(cpus => cpus.Where(x => x.IsFree.HasValue && x.Demande.DemandeId.Equals(null))))
        {
            // verifie eligibilité du poste 
            if (cpu.Authorization.IsAuthorized) //(ChkCPUInDB(cpu.Name, cpu.Machine, ref msg))
            {
                var infoDemande = GetDataDemandesaTraiterParParalleleU(cpu.Name)?.FirstOrDefault();

                if (infoDemande is null)
                {
                    continue;
                }

                cpu.Demande.DemandeId = infoDemande.TD_DEMANDEID;
                cpu.Demande.EtatId = infoDemande.TE_ETATID;

                _logger.Information(
                    $"> -------- DemandeId: {(cpu.Demande.DemandeId.HasValue ? cpu.Demande.DemandeId.Value : "(null)")}, Machine: {cpu.Machine}, Action: {cpu.StepWorkflow ?? "ClientWakeUp"}");

                // récupération données complémentaires disponibles avant exécution effective (batch / ressources)
                // peuplement des batchs associés
                bool bbatch = SetDataBatch(infoDemande.TD_DEMANDEID, cpu);

                // peuplement des ressources associées (si existe)
                bool bressource = SetDataRessource(infoDemande.TD_DEMANDEID, cpu);

                // peuplement des parametres associés au serveur physique client
                bool bparam = SetParamServeur(cpu.Name, cpu);

                if (bbatch && bressource && bparam)
                {
                    cpu.Demande.StartedAt = DateExtensions.GetUtcNowSecond();
                    cpu.IsFree = false;

                    await ConfirmePriseEnChargeDemande(infoDemande.TD_DEMANDEID, cpu.Name, cpu.Demande.StartedAt!.Value);
                    cpu.StepWorkflow = CPULitterals.ClientWakeUp;
                    _cpuManager.UpdateCpu(cpu);

                    //ultime controle CPU encore présent TODO
                    // demande au cpu concerné de se présenter au guichet pour demander sa todo list
                    await _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnClientWakeUp", new object[] { cpu });
                } // Erreur (peu probable) dans la préparation de la demande
                else
                {
                    _logger.Warning($"DemandeId: {cpu.Demande.DemandeId}, Reason: demande défectueuse");
                    await FinalizeDemande(infoDemande.TD_DEMANDEID, cpu.Id, false, "Echec sur phase de préparation");
                }
            }
            else
            {
                _logger.Warning($"< Client: {cpu.Name}, Action: Reject, Reason: non éligible pour traiter des demandes sur {cpu.Machine}, Id: {cpu.Id}");
            }
        }
    }

    /// <summary>
    /// Gere la fin de vie d un CPU = // U client
    /// </summary>
    /// <returns></returns>
    private void PUCentralGestionFermetureCPU()
    {
        // Récupère les instances des //U (nom du //U) qui viennent de disparaitre des //U connectés
        foreach (string cpusLost in _cpuManager.GetLostInstances)
        {
            var cpus = _cpuManager.GetLostConnections(cpusLost);

            // Récupère les connexions perdues normalement ou pas
            foreach (var cpu in cpus)
            {
                // TODO ICi
                //  cpu.IsFree.HasValue && cpu.CPUDemandes.DemandeId
                // check si demande en cours
                // finalizedemande + autre
                //  _logger.Warning("warning si cpu lost normalement");
                // _logger.Warning("error si cpu lost prématurément");
                // gestion mail...
                // fin TODO
                if (cpu.Demande.DemandeId != null)
                {
                    _logger.Warning($"DemandeId: {cpu.Demande.DemandeId}, Reason: perte de connexion de : {cpu.Id} durant le traitement, Action: {cpu.StepWorkflow}");
                }

                _cpuManager.RemoveLostConnection(cpu.Id);
            }
        }
    }

    /// <summary>
    /// prise en compte des demandes dont l'annulation est demandée par l utilisateur
    /// </summary>
    /// <returns></returns>
    private Task PUCentralConfirmeDemandesAnnulees()
    {
        return _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContext.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");

                var listedemande = from tdDemande in _dbContext.TD_DEMANDES
                                   where tdDemande.TRST_STATUTID == StatusLiteral.Stopping
                                   select tdDemande;

                foreach (var item in listedemande)
                {
                    item.TRST_STATUTID = StatusLiteral.CanceledRequest;
                    _ = _dbContext.TD_DEMANDES.Update(item);
                }

                _ = await _dbContext.SaveChangesAsync();
                await dbCTrans.CommitAsync();
            }
            catch //(Exception ex)
            {
                await dbCTrans.RollbackToSavepointAsync("Step_001");
            }
        });
    }

    /// <summary>
    /// Demandes_par_ParallelU
    /// </summary>
    private record DemandesParParallelU(int TD_DEMANDEID, DateTime? TD_DATE_EXECUTION_SOUHAITEE, int TE_ETATID);

    // public record DemandesParParallelU(int TD_DEMANDEID, DateTimeOffset? TD_DATE_EXECUTION_SOUHAITEE, int TE_ETATID);

    /// <summary>
    ///  Lister les instances de demandes eligibles	pour une instance de //U associé
    /// </summary>
    /// <returns></returns>
    /// <param name="pTpuInstance">Nom de l'instance ParalleleUClient</param>
    private IEnumerable<DemandesParParallelU> GetDataDemandesaTraiterParParalleleU(string pTpuInstance)
    {
        var listedemande =
            (from teEtat in _dbContext.TE_ETATS
             where teEtat.TRST_STATUTID == StatusLiteral.Available || teEtat.TRST_STATUTID == StatusLiteral.Prototype
             join tdDemande in _dbContext.TD_DEMANDES on teEtat.TE_ETATID equals tdDemande.TE_ETATID
             where tdDemande.TRST_STATUTID == StatusLiteral.CreatedRequestAndWaitForExecution || tdDemande.TRST_STATUTID == StatusLiteral.ScheduledRequest /* criteres a ajuster */
             join temEtatMaster in _dbContext.TEM_ETAT_MASTERS on teEtat.TEM_ETAT_MASTERID equals temEtatMaster.TEM_ETAT_MASTERID
             where temEtatMaster.TRST_STATUTID == StatusLiteral.Available
             join temfEtatMasterFerme in _dbContext.TEMF_ETAT_MASTER_FERMES on temEtatMaster.TEM_ETAT_MASTERID equals temfEtatMasterFerme.TEM_ETAT_MASTERID
             join tfFerme in _dbContext.TF_FERMES on temfEtatMasterFerme.TF_FERMEID equals tfFerme.TF_FERMEID
             where tfFerme.TRST_STATUTID == StatusLiteral.Available
             join tpufParalleleuFerme in _dbContext.TPUF_PARALLELEU_FERMES on tfFerme.TF_FERMEID equals tpufParalleleuFerme.TF_FERMEID
             where tpufParalleleuFerme.TRST_STATUTID == StatusLiteral.Available
             join tpuParalleleu in _dbContext.TPU_PARALLELEUS on tpufParalleleuFerme.TPU_PARALLELEUID equals tpuParalleleu.TPU_PARALLELEUID
             where tpuParalleleu.TRST_STATUTID == StatusLiteral.Available && tpuParalleleu.TPU_INSTANCE == pTpuInstance
             select
                 new
                 {
                     tdDemande.TD_DEMANDEID,
                     tdDemande.TD_DATE_EXECUTION_SOUHAITEE,
                     tpuParalleleu.TPU_PARALLELEUID,
                     tpuParalleleu.TPU_INSTANCE,
                     tdDemande.TE_ETATID
                 }).AsEnumerable()
            .Where(a => (a.TD_DATE_EXECUTION_SOUHAITEE ?? DateExtensions.GetUtcNow()) <= DateExtensions.GetUtcNow())
            .OrderBy(a => a.TD_DATE_EXECUTION_SOUHAITEE)
            ?.ToList();

        return JsonSerializer.Deserialize<IEnumerable<DemandesParParallelU>>(JsonSerializer.SerializeToUtf8Bytes(listedemande));
    }

    private static string GetEnvViergeFilename(int? EtatId) => EtatId.HasValue
        ? string.Concat("E", EtatId.ToString().PadLeft(6, '0'), ".zip")
        : string.Empty;

    private static string DemandeID(int? demandeId) =>
        demandeId.HasValue ? demandeId.ToString().PadLeft(6, '0') : string.Empty;

    #region Méthodes pour traiter une demande

    #region Séquencement

    /// <summary>
    /// constate derniere exécution du client et annonce la prochaine méthode a appeller
    /// </summary>
    /// <returns></returns>
    /// <param name="connectionId">Id instance ParalleleU</param>
    //#nullable enable
    public async Task Enchainement(string connectionId)
    {
        string lastCmdError = null;

        var cpuNow = _cpuManager.GetCpuInstance(connectionId);

        // CPU  structure avec un chapeau '== nom du CPU' contenant de 1 à N 'instances' ( 1 instance == 1 cpu.Id ou connectionId (son jeton de socketId))
        try
        {
            if (cpuNow is not null)
            {
                byte[] jsonData;
                bool LastCmdResult;
                if (cpuNow.LastCmdResult.HasValue)
                {
                    LastCmdResult = cpuNow.LastCmdResult.Value;
                    lastCmdError = cpuNow.LastCmdError;
                    jsonData = cpuNow.Demande.JsonData;
                }
                else
                {
                    LastCmdResult = false;
                    jsonData = null;
                }

                cpuNow.ClearPartOfCpu();

                var statutdemandecourante = "";

                if (cpuNow.Demande.DemandeId.HasValue)
                {
                    statutdemandecourante = (from tdDemande in _dbContext.TD_DEMANDES
                                             where tdDemande.TD_DEMANDEID == cpuNow.Demande.DemandeId
                                             select tdDemande).FirstOrDefault()
                                             ?.TRST_STATUTID;
                }

                // une perte de connexion a eu lieu
                // Une reconnexion est constatée (resilience) et  le client est en train d'exécuter un bat
                // alors on a rien a refournir au client, il demandera la suite qd il aura besoin d'obtenir sa prochaine instruction
                if (cpuNow.Resilience && cpuNow.Demande.DemandeId.HasValue &&
                    cpuNow.Demande.ListBatchsAttributes.Any(x => x.ProcessId.HasValue))
                {
                    _logger.Warning($"DemandeId: {cpuNow.Demande.DemandeId}, Machine: {cpuNow.Machine}, Reason: Reprise de connexion pendant l'execution d'un bat");
                }
                else if (cpuNow.Resilience && cpuNow.Demande.DemandeId.HasValue && !statutdemandecourante!.Equals("") &&
                         !statutdemandecourante.Equals(StatusLiteral.InProgress, StringComparison.Ordinal))
                {
                    // on est en resiliance et on constate que la demande est déjà terminée alors on libere le cpu
                    _logger.Warning($"DemandeId: {cpuNow.Demande.DemandeId}, Machine: {cpuNow.Machine}, Reason: Reprise de connexion (demande déjà terminée)");

                    cpuNow.RecycleCpu();
                }
                else
                {
                    // fonctionnement nominal, on supprime le fait qu'on revient d'une reconnexion
                    cpuNow.Resilience = false;

                    //// DEBUG ONLY (simulating network interruption: put a breakpoint below)
                    //if (cpuNow.StepWorkflow == "OnCleanClientDirectory") //"OnExecuteCmd")
                    //{
                    //    var stop = 1;
                    //}

                    //  Step Number
                    if (cpuNow.Demande.DemandeId.HasValue)
                    {
                        _logger.Information(
                            $"DemandeId: {cpuNow.Demande.DemandeId}, Machine: {cpuNow.Machine}, Id: {cpuNow.Id}, Action: {cpuNow.StepWorkflow}");
                        Console.WriteLine(@"Action: " + cpuNow.StepWorkflow);

                        cpuNow.StepNumber += 1; // pour tracage a venir en bdd
                    }

                    cpuNow.StepStartedAt = DateExtensions.GetLocaleNow();

                    switch (cpuNow.StepWorkflow) // derniere opération effectuée par le client
                    {
                        case CPULitterals.ClientWakeUp:

                            Console.WriteLine("DemandeId: " + cpuNow.Demande.DemandeId);
                            cpuNow.StepWorkflow = CPULitterals.OnCreateDirectory;
                            Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                            await CreateDirectoryAsync(cpuNow, DemandeID(cpuNow.Demande.DemandeId));
                            break;

                        case CPULitterals.OnCreateDirectory:
                            if (LastCmdResult)
                            {
                                cpuNow.StepWorkflow = CPULitterals.OnDownloadEnvVierge;
                                Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                                //DownloadEnvVierge
                                await DownloadFileAsync(cpuNow,
                                    _configuration["ParallelU:PathEnvVierge"],
                                    //$"{Path.AltDirectorySeparatorChar}{DemandeID(cpuNow.CpuDemandes.DemandeId)}{Path.AltDirectorySeparatorChar}",
                                    DemandeID(cpuNow.Demande.DemandeId),
                                    GetEnvViergeFilename(cpuNow.Demande.EtatId));
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed xxxxxxxxxxxxxxx TODO ICI ");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false, $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");
                                cpuNow.RecycleCpu();
                            }

                            break;

                        case CPULitterals.OnDownloadEnvVierge:
                            if (LastCmdResult)
                            {
                                cpuNow.StepWorkflow = CPULitterals.OnUnzipEnvVierge;
                                Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                                await DirectoryUnZipAsync(cpuNow,
                                    DemandeID(cpuNow.Demande.DemandeId),
                                    GetEnvViergeFilename(cpuNow.Demande.EtatId),
                                    DemandeID(cpuNow.Demande.DemandeId));
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed xxxxxxxxxxxxxxx TODO ICI ");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false,
                                    $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");

                                cpuNow.RecycleCpu();
                            }

                            break;

                        // double case possible
                        case CPULitterals.OnUnzipEnvVierge:
                        case CPULitterals.OnDownloadRessourceFile:
                            if (LastCmdResult)
                            {
                                // si au moins un fichier restant a transferer 
                                if (cpuNow.Demande.ListRessourcesAttributes.Any(x => !x.IsTransfertStarted))
                                {
                                    cpuNow.StepWorkflow = CPULitterals.OnDownloadRessourceFile;
                                    Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                                    // un seul fichier est envoyé par appel
                                    var resfile =
                                        cpuNow.Demande.ListRessourcesAttributes?.FirstOrDefault(x =>
                                            !x.IsTransfertStarted);

                                    if (resfile is not null)
                                    {
                                        var RessFolderSrc = Path.Combine(_configuration["ParallelU:PathRessource"]!,
                                            DemandeID(resfile.DemandeOrigine));
                                        var RessFolderTarget = string.Concat(DemandeID(cpuNow.Demande.DemandeId),
                                            Path.AltDirectorySeparatorChar, "Documents", Path.AltDirectorySeparatorChar,
                                            resfile.Destination);
                                        resfile.IsTransfertStarted = true;
                                        await DownloadFileAsync(cpuNow, RessFolderSrc, RessFolderTarget,
                                            resfile.NomFichier);
                                    }
                                }
                                else // pas ou plus de fichier a transferer
                                {
                                    cpuNow.StepWorkflow = CPULitterals.OnCreateMetafile;
                                    Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                                    // Create meta.txt
                                    await CreateMetaFileAsync(cpuNow, DemandeID(cpuNow.Demande.DemandeId),
                                        GetDataMeta(cpuNow.Demande.DemandeId ?? 0));
                                }
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed xxxxxxxxxxxxxxx TODO ICI ");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false,
                                    $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");

                                cpuNow.RecycleCpu();
                            }

                            break;

                        case CPULitterals.OnCreateMetafile:
                            if (LastCmdResult)
                            {
                                cpuNow.StepWorkflow = CPULitterals.OnCreateEtlSettingsFile;
                                Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                                // Create etlsettings.json, jwtToken will be based on CPULitterals.KrialysEtl application name (immutable name)
                                await CreateEtlSettingsFileAsync(cpuNow, DemandeID(cpuNow.Demande.DemandeId),
                                    _authServices.GetAppAccessToken(CPULitterals.KrialysEtl));
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed xxxxxxxxxxxxxxx TODO ICI ");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false,
                                    $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");

                                cpuNow.RecycleCpu();
                            }

                            break;

                        case CPULitterals.OnCreateEtlSettingsFile:
                            if (LastCmdResult)
                            {
                                cpuNow.StepWorkflow = CPULitterals.OnAlterCmd;
                                Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);
                                //var cliBatPath = string.Concat(DemandeID(cpuNow.Demande.DemandeId), Path.AltDirectorySeparatorChar, "Batch");

                                await AlterCmdAsync(cpuNow, DemandeID(cpuNow.Demande.DemandeId),
                                    "Batch"); //   un seul appel qui traite tous les bats en croisants avec paramserveur et WORKSPACE_PATH
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed xxxxxxxxxxxxxxx TODO ICI ");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false,
                                    $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");

                                cpuNow.RecycleCpu();
                            }

                            break;

                        // double entrée possible
                        case CPULitterals.OnAlterCmd:
                        case CPULitterals.OnExecuteCmd:
                            if (LastCmdResult)
                            {
                                // confirmation par le client qu'il a starté un bat et qu'il est en cours
                                // le processID n est renseigné que le temp de vie du bat donc doit etre vide autrement
                                if (cpuNow.Demande.ListBatchsAttributes.Any(x => x.ProcessId.HasValue))
                                {
                                    //return; // on sort : on est en attente de la fin du bat
                                }
                                else
                                {
                                    // tracer fin du dernier bat exécuté (si pas d'erreur)
                                    var lastCmdDone = cpuNow.Demande.ListBatchsAttributes
                                        .Where(x => x.SubmitExec && x.ExitCode != null).MaxBy(x => x.OrdreExecution);

                                    if (lastCmdDone is not null && cpuNow.Demande.DemandeId.HasValue)
                                    {
                                        await SetAvancementBatchDemande("F", cpuNow.Demande.DemandeId.Value,
                                            lastCmdDone.OrdreExecution, lastCmdDone.ExitCode);
                                    }

                                    // si demande du 1er bat ou si le dernier bat traité n est pas en erreur et qu il reste au moins un bat a traiter, on le fournit
                                    // sinon on passe a step suivante
                                    //---------------------------------
                                    // SI au moins un batch pas encore lancé existe 
                                    // ET
                                    //  (   c est le 1er bat qui est lancé
                                    //    ou
                                    //    le dernier bat lancé renvoi un exit code a 0 )

                                    if (cpuNow.Demande.ListBatchsAttributes.Any(x => !x.SubmitExec)
                                        && (lastCmdDone is null || lastCmdDone.ExitCode == 0))
                                    {
                                        cpuNow.StepWorkflow = CPULitterals.OnExecuteCmd;
                                        Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                                        // attention verifier que le firstorDefault soit est dans l ordre des batchs a exécuter
                                        var batchfile =
                                            cpuNow.Demande.ListBatchsAttributes?.FirstOrDefault(x => !x.SubmitExec);

                                        if (batchfile is not null)
                                        {
                                            batchfile.SubmitExec = true;

                                            if (cpuNow.Demande.DemandeId.HasValue)
                                            {
                                                await SetAvancementBatchDemande("D", cpuNow.Demande.DemandeId.Value,
                                                    batchfile.OrdreExecution, null);
                                                var cmdFileWithPartialPath = string.Concat(
                                                    DemandeID(cpuNow.Demande.DemandeId),
                                                    Path.AltDirectorySeparatorChar, "Batch",
                                                    Path.AltDirectorySeparatorChar, batchfile.NomFichier);

                                                Console.WriteLine("...Run file n°: " + batchfile.OrdreExecution +
                                                                  ", Name: " + batchfile.NomFichier);

                                                await RunCmdAsync(cpuNow, cmdFileWithPartialPath, "", false);
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("CAS IMPOSSIBLE : OnExecuteCmd  batchfile is null");
                                            await CpuLogger(LogEventLevel.Error, cpuNow, CPULitterals.OnExecuteCmd,
                                                lastCmdError);
                                        }
                                    }
                                    else // pas ou plus de bat a exécuter ou fin suite a erreur de bat
                                    {
                                        cpuNow.StepWorkflow = CPULitterals.OnZipOutput;
                                        Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                                        await DirectoryZipAsync(cpuNow,
                                            DemandeID(cpuNow.Demande.DemandeId),
                                            "Output",
                                            string.Concat(DemandeID(cpuNow.Demande.DemandeId), "_RESULT.zip")
                                        );
                                    }
                                }
                            }
                            else // cas en erreur ou de StopCmd
                            {
                                if (cpuNow.StepWorkflow.Equals(CPULitterals.OnAlterCmd, StringComparison.Ordinal))
                                {
                                    // TODO erreur préparation cmd
                                    await CpuLogger(LogEventLevel.Error, cpuNow, CPULitterals.OnExecuteCmd, lastCmdError);
                                    await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false, $"Action: {cpuNow.StepWorkflow}, lastCmdError: {lastCmdError}");

                                    cpuNow.RecycleCpu();
                                }
                                else // on est sur une erreur de OnExecuteCmd (plantage autour du lancement du bat pas le bat lui meme)
                                {
                                    var lastCmdDone = cpuNow.Demande.ListBatchsAttributes
                                        .Where(x => x.SubmitExec && x.ExitCode != null)
                                        .MaxBy(x => x.OrdreExecution);

                                    if (lastCmdDone is not null && cpuNow.Demande.DemandeId.HasValue)
                                    {
                                        Console.WriteLine(" OnExecuteCmd : erreur sur batch n°" + lastCmdDone.OrdreExecution);
                                        await CpuLogger(LogEventLevel.Error, cpuNow, CPULitterals.OnExecuteCmd, lastCmdError);
                                        await SetAvancementBatchDemande("F", cpuNow.Demande.DemandeId.Value, lastCmdDone.OrdreExecution, lastCmdDone.ExitCode);
                                    }

                                    cpuNow.StepWorkflow = CPULitterals.OnZipOutput;
                                    Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                                    await DirectoryZipAsync(cpuNow,
                                        DemandeID(cpuNow.Demande.DemandeId),
                                        "Output", string.Concat(DemandeID(cpuNow.Demande.DemandeId), "_RESULT.zip"));
                                }
                            }

                            break;

                        case CPULitterals.OnZipOutput:
                            if (LastCmdResult)
                            {
                                if (string.IsNullOrEmpty(lastCmdError))
                                {
                                    cpuNow.StepWorkflow = CPULitterals.OnUploadOutput;
                                    Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                                    await UploadFileAsync(cpuNow, _configuration["ParallelU:PathResult"],
                                        DemandeID(cpuNow.Demande.DemandeId),
                                        string.Concat(DemandeID(cpuNow.Demande.DemandeId), "_RESULT.zip"));
                                }
                                else // l'Output du client est vide donc on ne cherche pas a recupérer de zip associé
                                {
                                    // recup data Qualif
                                    cpuNow.StepWorkflow = CPULitterals.OnSetQualif;
                                    Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);
                                    await LoadQualifAsync(cpuNow, string.Concat(DemandeID(cpuNow.Demande.DemandeId),
                                        Path.AltDirectorySeparatorChar, "Qualif"), "Qualif.csv");
                                }
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed xxxxxxxxxxxxxxx TODO ICI ");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false,
                                    $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");

                                cpuNow.RecycleCpu();
                            }

                            break;

                        case CPULitterals.OnUploadOutput:
                            if (LastCmdResult)
                            {
                                cpuNow.StepWorkflow = CPULitterals.OnSetQualif;
                                Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);
                                await LoadQualifAsync(cpuNow, string.Concat(DemandeID(cpuNow.Demande.DemandeId),
                                    Path.AltDirectorySeparatorChar, "Qualif"), "Qualif.csv");
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow +
                                                  " Failed OnUploadOutput xxxxxxxxxxxxxxx TODO ICI ");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false,
                                    $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");

                                cpuNow.RecycleCpu();
                            }

                            break;

                        case CPULitterals.OnSetQualif:
                            if (LastCmdResult)
                            {
                                // présence d'un fichier qualif
                                if (string.IsNullOrEmpty(lastCmdError))
                                {
                                    //chargement en bdd de cpu.CpuDemandes.JsonData
                                    if (cpuNow.Demande.DemandeId != null)
                                    {
                                        await SetQualifDemande((int)cpuNow.Demande.DemandeId, jsonData)
                                            ;

                                        cpuNow.StepWorkflow = CPULitterals.OnZipQualif;
                                        Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                                        await DirectoryZipAsync(cpuNow,
                                            DemandeID(cpuNow.Demande.DemandeId)
                                            , "Qualif",
                                            string.Concat(DemandeID(cpuNow.Demande.DemandeId), "_QUALIF.zip")
                                        );
                                    }
                                }
                                else
                                {
                                    cpuNow.StepWorkflow = CPULitterals.OnLoadRefLog;
                                    Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);
                                    await LoadRefLogAsync(cpuNow, string.Concat(DemandeID(cpuNow.Demande.DemandeId),
                                        Path.AltDirectorySeparatorChar, "REFLOG", Path.AltDirectorySeparatorChar));
                                }
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed xxxxxxxxxxxxxxx TODO ICI ");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false,
                                    $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");

                                cpuNow.RecycleCpu();
                            }

                            break;

                        case CPULitterals.OnZipQualif:
                            if (LastCmdResult)
                            {
                                if (string.IsNullOrEmpty(lastCmdError))
                                {
                                    cpuNow.StepWorkflow = CPULitterals.OnUploadQualif;
                                    Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);
                                    await UploadFileAsync(cpuNow, _configuration["ParallelU:PathQualif"],
                                        DemandeID(cpuNow.Demande.DemandeId),
                                        string.Concat(DemandeID(cpuNow.Demande.DemandeId), "_QUALIF.zip"));
                                }
                                else // le dossier Qualif du client est vide donc on ne cherche pas a recupérer de zip associé
                                {
                                    cpuNow.StepWorkflow = CPULitterals.OnLoadRefLog;
                                    Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);
                                    await LoadRefLogAsync(cpuNow, string.Concat(DemandeID(cpuNow.Demande.DemandeId),
                                        Path.AltDirectorySeparatorChar, "REFLOG", Path.AltDirectorySeparatorChar));
                                }
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed xxxxxxxxxxxxxxx TODO ICI ");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false,
                                    $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");

                                cpuNow.RecycleCpu();
                            }

                            break;

                        case CPULitterals.OnUploadQualif:
                            cpuNow.StepWorkflow = CPULitterals.OnLoadRefLog;
                            Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);
                            await LoadRefLogAsync(cpuNow, string.Concat(DemandeID(cpuNow.Demande.DemandeId),
                                Path.AltDirectorySeparatorChar, "REFLOG", Path.AltDirectorySeparatorChar));
                            break;

                        case CPULitterals.OnLoadRefLog:
                            //  tache locale : InsertRefLogAsync

                            //OB-348  
                            cpuNow.StepWorkflow = CPULitterals.OnInsertRefLog;

                            if (LastCmdResult)
                            {
                                Console.WriteLine("...Step to come (local): " + cpuNow.StepWorkflow);
                                Console.WriteLine("...Save to MSO if some data");
                                await InsertRefLogAsync(cpuNow);
                                Console.WriteLine("...Saved to MSO done.");
                            }
                            else // l'echec de l'etape ne doit pas influencer le statut de la demande
                                 // On se contente de tracer le soucis rencontré
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed : NOT blocking error");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                            }

                            cpuNow.LastCmdResult = true;
                            _cpuManager.UpdateCpu(cpuNow);
                            await Enchainement(cpuNow.Id);

                            break;

                        case CPULitterals.OnInsertRefLog:
                            if (LastCmdResult)
                            {
                                //  suppression conditionnée à  erreur sur au moins un bat (ou non exécuté) et parametre général 
                                if (cpuNow.Demande.ListBatchsAttributes.All(x => x.ExitCode == 0)) // pas d'erreur on supprime les fichiers temp
                                {
                                    cpuNow.StepWorkflow = CPULitterals.OnCleanClientDirectory;
                                    Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);
                                    await DeleteDirectoryAsync(cpuNow, DemandeID(cpuNow.Demande.DemandeId));
                                }
                                else
                                {
                                    if (!Convert.ToBoolean(_configuration["ParallelU:KeepClientDemandeFolderOnFailed"] ?? "0"))
                                    {
                                        cpuNow.StepWorkflow = CPULitterals.OnCleanClientDirectory;
                                        Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);
                                        await DeleteDirectoryAsync(cpuNow, DemandeID(cpuNow.Demande.DemandeId));
                                    }
                                    else // sur le client malgres les erreurs sur execution de batch on ne souhaite pas supprimer le dossier temporaire donc on finalize
                                    {
                                        cpuNow.StepWorkflow = CPULitterals.Finalize;
                                        Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);
                                        cpuNow.LastCmdResult = true;
                                        _cpuManager.UpdateCpu(cpuNow);
                                        await Enchainement(cpuNow.Id);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed xxxxxxxxxxxxxxx TODO ICI ");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false,
                                    $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");

                                cpuNow.RecycleCpu();
                            }

                            break;

                        case CPULitterals.OnCleanClientDirectory: //DeleteDirectory
                            //OB-348  
                            cpuNow.StepWorkflow = CPULitterals.Finalize;
                            Console.WriteLine("...Step to come: " + cpuNow.StepWorkflow);

                            // L'echec de l'etape ne doit pas influencer le statut de la demande
                            // On se contente de tracer le soucis rencontré (Echec lors de la suppression de l'env de travail sur le noeud de prod)
                            if (!LastCmdResult)
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed Not blocking error");
                                await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, lastCmdError);
                            }

                            cpuNow.LastCmdResult = true;
                            _cpuManager.UpdateCpu(cpuNow);
                            await Enchainement(cpuNow.Id);

                            break;

                        case CPULitterals.Finalize:
                            if (LastCmdResult)
                            {
                                Console.WriteLine("Finalize...");
                                _ = await SetQualifExtraInfo(cpuNow.Demande.DemandeId);

                                var msg = "";
                                bool reussite = !cpuNow.Demande.ListBatchsAttributes.Any(x => x.ExitCode != 0);

                                if (!reussite)
                                {
                                    var batchEnErreur =
                                        cpuNow.Demande.ListBatchsAttributes?.FirstOrDefault(x => x.ExitCode != 0);

                                    if (batchEnErreur is not null)
                                    {
                                        msg =
                                            $"Batch n°{batchEnErreur.OrdreExecution} ({batchEnErreur.NomFichier}) failed";
                                        await CpuLogger(LogEventLevel.Error, cpuNow, cpuNow.StepWorkflow, msg);
                                    }
                                }

                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, reussite, msg);

                                // Send mail to the Demandeur before disposing Cpu
                                if (cpuNow.Demande.DemandeId.HasValue)
                                {
                                    var total = (DateExtensions.GetUtcNowSecond() - cpuNow.Demande.StartedAt)?.ToString(@"hh\:mm\:ss");
                                    _logger.Information($"< -------- DemandeId: {(cpuNow.Demande.DemandeId.HasValue ? cpuNow.Demande.DemandeId.Value : "(null)")}, Machine: {cpuNow.Machine}, Action: {cpuNow.StepWorkflow}, Time elapsed: {total} (hh:mm:ss)");

                                    if (cpuNow.Demande.DemandeId != null)
                                    {
                                        await _emailServices.SendAutomatedMailForRequest(cpuNow.Demande.DemandeId.Value, OrderStatus.EndOfProduction);
                                    }
                                }

                                cpuNow.RecycleCpu();
                            }
                            else
                            {
                                Console.WriteLine(cpuNow.StepWorkflow + " Failed xxxxxxxxxxxxxxx TODO ICI ");

                                await FinalizeDemande(cpuNow.Demande.DemandeId, cpuNow.Id, false, $"Action: {cpuNow.StepWorkflow}, LastCmdError: {lastCmdError}");

                                if (cpuNow.Demande.DemandeId.HasValue)
                                {
                                    var total = (DateExtensions.GetUtcNowSecond() - cpuNow.Demande.StartedAt)?.ToString(@"hh\:mm\:ss");
                                    _logger.Information($"< -------- DemandeId: {(cpuNow.Demande.DemandeId.HasValue ? cpuNow.Demande.DemandeId.Value : "(null)")}, Machine: {cpuNow.Machine}, Action: {cpuNow.StepWorkflow}, Time elapsed: {total} (hh:mm:ss)");
                                }

                                cpuNow.RecycleCpu();
                            }

                            cpuNow.StepNumber = 0;
                            cpuNow.LastJobRun = DateExtensions.GetUtcNowSecond();
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Empty CPU in Enchainement!");
            }
            // s inspirer de private void CallbackGrantClient(int interval) 
            // pour gerer le cas d un cpu qui serait sur une demande mais sans avoir donné signe de vie depuis un certain temps
            // cpu heure de passation d heure de lancement
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Enchainement failed: {ex.Message}");
            await CpuLogger(LogEventLevel.Error, cpuNow, "Enchainement", lastCmdError, ex);
            throw;
        }
    }

    #endregion sequencement

    /// <summary>
    /// Enregistre le fait que la demande est bien réservée pour un //U donné et va etre immédiatement traitée
    /// </summary>
    /// <returns></returns>
    /// <param name="pTdDemandeid">N° de demande</param>
    /// <param name="pTpuInstance">Non de l'instance ParalleleU</param>
    /// <param name="pDemandStartedAt"></param>
    private async Task
        ConfirmePriseEnChargeDemande(int pTdDemandeid, string pTpuInstance, DateTime pDemandStartedAt) // DateTimeOffset pDemandStartedAt)
    {
        var listedata = (from tpuParalleleu in _dbContext.TPU_PARALLELEUS
                         where tpuParalleleu.TPU_INSTANCE == pTpuInstance
                         select new
                         {
                             tpuParalleleu.TSRV_SERVEURID
                         }).FirstOrDefault();

        var listedemande = (from tdDemande in _dbContext.TD_DEMANDES
                            where
                                (tdDemande.TRST_STATUTID == StatusLiteral.ScheduledRequest ||
                                 tdDemande.TRST_STATUTID == StatusLiteral.CreatedRequestAndWaitForExecution) && // pour etre certain que la demande n'a pas été écarté pour x raisons
                                tdDemande.TD_DEMANDEID == pTdDemandeid
                            select new
                            {
                                TD_DEMANDE = tdDemande
                            }).FirstOrDefault();

        await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContext.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");

                if (listedemande != null)
                {
                    listedemande.TD_DEMANDE.TRST_STATUTID = StatusLiteral.InProgress;
                    listedemande.TD_DEMANDE.TD_DATE_PRISE_EN_CHARGE = pDemandStartedAt;
                    if (listedata != null)
                        listedemande.TD_DEMANDE.TSRV_SERVEURID = listedata.TSRV_SERVEURID;
                }

                await _dbContext.SaveChangesAsync();

                // track pour aider refresh des interfaces abonnées
                await _trackedEntitiesServices.AddTrackedEntitiesAsync(null,
                    new[] { typeof(VDE_DEMANDES_ETENDUES).FullName }, Litterals.Update, _sessionId, "ConfirmePriseEnChargeDemande");

                await dbCTrans.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "< -------- Failed: ConfirmePriseEnChargeDemande. {Message}", ex.InnerException?.Message ?? ex.Message);
                await dbCTrans.RollbackToSavepointAsync("Step_001");
            }
        });
    }

    /// <summary>
    /// Données pour générer fichier meta.txt spécifique à une demande
    /// </summary>
    /// <returns></returns>
    /// <param name="pTdDemandeid">N° de demande</param>
    private string GetDataMeta(int pTdDemandeid)
    {
        var metaData = from tdDemande in _dbContext.TD_DEMANDES
                       where tdDemande.TD_DEMANDEID == pTdDemandeid
                       join teEtat in _dbContext.TE_ETATS on tdDemande.TE_ETATID equals teEtat.TE_ETATID
                       join temEtatMaster in _dbContext.TEM_ETAT_MASTERS on teEtat.TEM_ETAT_MASTERID equals temEtatMaster.TEM_ETAT_MASTERID
                       join tsScenario in _dbContext.TS_SCENARIOS on tdDemande.TS_SCENARIOID equals tsScenario.TS_SCENARIOID
                       join truUser in _dbContext.TRU_USERS on tdDemande.TRU_DEMANDEURID equals truUser.TRU_USERID
                       select new
                       {
                           tdDemande.TD_DEMANDEID,
                           teEtat.TE_NOM_ETAT,
                           teEtat.TE_INDICE_REVISION_L1,
                           teEtat.TE_INDICE_REVISION_L2,
                           teEtat.TE_INDICE_REVISION_L3,
                           teEtat.TE_COMMENTAIRE,
                           teEtat.TE_INFO_REVISION,
                           teEtat.TE_DATE_REVISION,
                           teEtat.TRST_STATUTID,
                           tdDemande.TD_DATE_PRISE_EN_CHARGE,
                           tdDemande.TD_DATE_EXECUTION_SOUHAITEE,
                           tdDemande.TD_COMMENTAIRE_UTILISATEUR,
                           truUser.TRU_NAME,
                           truUser.TRU_FIRST_NAME,
                           truUser.TRU_LOGIN,
                           /*    , TB_PROFIL.PROFIL  as PROFIL, */
                           tsScenario.TS_NOM_SCENARIO,
                           tsScenario.TS_DESCR,
                           tsScenario.TS_SCENARIOID,
                           temEtatMaster.TP_PERIMETREID
                       };

        var e = metaData.FirstOrDefault();

        StringBuilder meta = new();
        meta.AppendLine(string.Concat("METAINFO", _metaSeparator, "CONTENT"));

        meta.AppendLine(string.Concat("TD_DEMANDEID", _metaSeparator, e?.TD_DEMANDEID.ToString()));
        if (e != null)
        {
            meta.AppendLine(string.Concat("TE_NOM_ETAT", _metaSeparator, e.TE_NOM_ETAT));
            meta.AppendLine(string.Concat("ETAT_VERSION", _metaSeparator, e.TE_INDICE_REVISION_L1.ToString(), ".", e.TE_INDICE_REVISION_L2.ToString(), ".", e.TE_INDICE_REVISION_L3.ToString()));
            meta.AppendLine(string.Concat("TE_COMMENTAIRE", _metaSeparator, e.TE_COMMENTAIRE ?? string.Empty));
            meta.AppendLine(string.Concat("TE_INFO_REVISION", _metaSeparator, e.TE_INFO_REVISION ?? string.Empty));
            meta.AppendLine(string.Concat("TE_DATE_REVISION", _metaSeparator, e.TE_DATE_REVISION.ToString(CultureInfo.InvariantCulture)));
            meta.AppendLine(string.Concat("TRST_STATUTID", _metaSeparator, e.TRST_STATUTID));
            meta.AppendLine(string.Concat("TD_DATE_PRISE_EN_CHARGE", _metaSeparator, e.TD_DATE_PRISE_EN_CHARGE.HasValue ? e.TD_DATE_PRISE_EN_CHARGE.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
            meta.AppendLine(string.Concat("TD_DATE_EXECUTION_SOUHAITEE", _metaSeparator, e.TD_DATE_EXECUTION_SOUHAITEE.ToString()));
            meta.AppendLine(string.Concat("TD_COMMENTAIRE_UTILISATEUR", _metaSeparator, e.TD_COMMENTAIRE_UTILISATEUR ?? string.Empty));
            meta.AppendLine(string.Concat("TRU_NAME", _metaSeparator, e.TRU_NAME, " ", e.TRU_FIRST_NAME));
            meta.AppendLine(string.Concat("TRU_LOGIN", _metaSeparator, e.TRU_LOGIN));
            meta.AppendLine(string.Concat("TS_NOM_SCENARIO", _metaSeparator, e.TS_NOM_SCENARIO ?? string.Empty));
            meta.AppendLine(string.Concat("TS_DESCR", _metaSeparator, e.TS_DESCR ?? string.Empty));
            meta.AppendLine(string.Concat("TS_SCENARIOID", _metaSeparator, e.TS_SCENARIOID.ToString()));
            meta.AppendLine(string.Concat("TP_PERIMETREID", _metaSeparator, e.TP_PERIMETREID.ToString()));

            meta.AppendLine(string.Concat("--OLDNAME--", _metaSeparator, "**"));

            meta.AppendLine(string.Concat("ID_DEMANDE", _metaSeparator, e.TD_DEMANDEID.ToString()));
            meta.AppendLine(string.Concat("NOM_ETAT", _metaSeparator, e.TE_NOM_ETAT));
            meta.AppendLine(string.Concat("ETAT_VERSION", _metaSeparator, e.TE_INDICE_REVISION_L1.ToString(), ".", e.TE_INDICE_REVISION_L2.ToString(), ".", e.TE_INDICE_REVISION_L3.ToString()));
            meta.AppendLine(string.Concat("COMMENTAIRE", _metaSeparator, e.TE_COMMENTAIRE ?? string.Empty));
            meta.AppendLine(string.Concat("INFO_REVISION", _metaSeparator, e.TE_INFO_REVISION ?? string.Empty));

            meta.AppendLine(string.Concat("DATE_REVISION_ETAT", _metaSeparator, e.TE_DATE_REVISION.ToString(CultureInfo.InvariantCulture)));
            meta.AppendLine(string.Concat("STATUT_ETAT", _metaSeparator, e.TRST_STATUTID));
            meta.AppendLine(string.Concat("DATE_PRISE_EN_CHARGE", _metaSeparator, e.TD_DATE_PRISE_EN_CHARGE.HasValue ? e.TD_DATE_PRISE_EN_CHARGE.Value.ToString(CultureInfo.InvariantCulture) : string.Empty));
            meta.AppendLine(string.Concat("DATE_EXECUTION_SOUHAITEE", _metaSeparator, e.TD_DATE_EXECUTION_SOUHAITEE.ToString()));
            meta.AppendLine(string.Concat("COMMENTAIRE_UTILISATEUR", _metaSeparator, e.TD_COMMENTAIRE_UTILISATEUR ?? string.Empty));
            meta.AppendLine(string.Concat("DEMANDEUR", _metaSeparator, e.TRU_NAME, " ", e.TRU_FIRST_NAME));
            meta.AppendLine(string.Concat("LOGIN", _metaSeparator, e.TRU_LOGIN));
            meta.AppendLine(string.Concat("NOM_SCENARIO", _metaSeparator, e.TS_NOM_SCENARIO ?? string.Empty));
            meta.AppendLine(string.Concat("COMMENTAIRE_SCENARIO", _metaSeparator, e.TS_DESCR ?? string.Empty));
            meta.AppendLine(string.Concat("ID_SCENARIO", _metaSeparator, e.TS_SCENARIOID.ToString()));
            meta.AppendLine(string.Concat("ID_PERIMETRE", _metaSeparator, e.TP_PERIMETREID.ToString()));
        }

        meta.AppendLine(string.Concat("PROFIL", _metaSeparator, ""));

        return meta.ToString();
    }

    /// <summary>
    /// Produire une demande identifiée  : liste des fichiers de ressources et potentiellement le chemin relatif  dans lesquels ils doivent etre enregistrés sur le serveur de production.
    /// Les fichiers sont issus soit de la demande soit de son modèle
    /// </summary>
    /// <returns>record FichierRessources</returns>
    /// <param name="pTdDemandeid">N° de demande</param>
    /// <param name="cpu">Client</param>
    private bool SetDataRessource(int pTdDemandeid, WorkerNode cpu)
    {
        // Convert Anonymous to structured Record
        var queryable = (from trdRessourceDemande in _dbContext.TRD_RESSOURCE_DEMANDES
                         join terEtatRessource in _dbContext.TER_ETAT_RESSOURCES on trdRessourceDemande.TER_ETAT_RESSOURCEID equals terEtatRessource.TER_ETAT_RESSOURCEID
                         where trdRessourceDemande.TRD_FICHIER_PRESENT == StatusLiteral.Yes && trdRessourceDemande.TD_DEMANDEID == pTdDemandeid
                         select new
                         {
                             trdRessourceDemande.TD_DEMANDEID,
                             trdRessourceDemande.TRD_NOM_FICHIER,
                             terEtatRessource.TER_PATH_RELATIF,
                             terEtatRessource.TER_IS_PATTERN,
                             trdRessourceDemande.TRD_NOM_FICHIER_ORIGINAL
                         })
            .Union(from tdDemande in _dbContext.TD_DEMANDES
                   where tdDemande.TD_DEMANDEID == pTdDemandeid
                   join trdRessourceDemandes in _dbContext.TRD_RESSOURCE_DEMANDES on tdDemande.TD_DEMANDE_ORIGINEID equals trdRessourceDemandes.TD_DEMANDEID
                   join terEtatRessources in _dbContext.TER_ETAT_RESSOURCES on trdRessourceDemandes.TER_ETAT_RESSOURCEID equals terEtatRessources.TER_ETAT_RESSOURCEID
                   where trdRessourceDemandes.TRD_FICHIER_PRESENT == StatusLiteral.Yes
                   select new
                   {
                       trdRessourceDemandes.TD_DEMANDEID, // demande origine  pouvant etre différent de demande courante pour tâches recurrente
                       trdRessourceDemandes.TRD_NOM_FICHIER,
                       terEtatRessources.TER_PATH_RELATIF,
                       terEtatRessources.TER_IS_PATTERN,
                       trdRessourceDemandes.TRD_NOM_FICHIER_ORIGINAL
                   });

        IList<RessourcesAttributes> dataList = new List<RessourcesAttributes>();

        foreach (var e in queryable)
        {
            dataList.Add(new()
            {
                DemandeOrigine = e.TD_DEMANDEID,
                NomFichier = e.TER_IS_PATTERN.Equals(StatusLiteral.No)
                    ? e.TRD_NOM_FICHIER
                    : e.TRD_NOM_FICHIER_ORIGINAL, // exploitation du nom du fichier déclaré sinon du nom du fichier d'origine si on est sur un masque
                Destination = e.TER_PATH_RELATIF,
                IsTransfertStarted = false
            });
        }

        cpu.Demande.ListRessourcesAttributes = dataList;

        return true;
    }

    /// <summary>
    /// Produire une demande identifiée  : liste les noms des batchs a lancer et dans quel ordre
    /// </summary>
    /// <returns></returns>
    /// <param name="pTdDemandeid">N° de demande </param>
    /// <param name="cpu">Client</param>
    private bool SetDataBatch(int pTdDemandeid, WorkerNode cpu)
    {
        var queryable = from tdDemande in _dbContext.TD_DEMANDES
                        where tdDemande.TD_DEMANDEID == pTdDemandeid
                        join tbdBatchDemande in _dbContext.TBD_BATCH_DEMANDES on tdDemande.TD_DEMANDEID equals tbdBatchDemande.TD_DEMANDEID
                        join tebEtatBatch in _dbContext.TEB_ETAT_BATCHS on tbdBatchDemande.TEB_ETAT_BATCHID equals tebEtatBatch.TEB_ETAT_BATCHID
                        orderby tbdBatchDemande.TBD_ORDRE_EXECUTION
                        select new
                        {
                            tbdBatchDemande.TBD_ORDRE_EXECUTION,
                            tebEtatBatch.TEB_CMD
                        };

        IList<BatchsAttributes> dataList = new List<BatchsAttributes>();

        foreach (var e in queryable)
        {
            dataList.Add(new BatchsAttributes
            {
                OrdreExecution = e.TBD_ORDRE_EXECUTION,
                NomFichier = e.TEB_CMD,
                SubmitExec = false
            });
        }

        cpu.Demande.ListBatchsAttributes = dataList;

        return true;
    }

    /// <summary>
    /// Délivre les paramètres du serveur associé à une instance de ParalelleUClient
    /// </summary>
    /// <returns></returns>
    /// <param name="pTpuInstance">Nom de l'instance ParalleleUClient</param>
    /// <param name="cpu">Client</param> 
    private bool SetParamServeur(string pTpuInstance, WorkerNode cpu)
    {
        var queryable = from tpuParalleleu in _dbContext.TPU_PARALLELEUS
                        join tsrvServeur in _dbContext.TSRV_SERVEURS on tpuParalleleu.TSRV_SERVEURID equals tsrvServeur.TSRV_SERVEURID
                        join TSP_SERVEUR_PARAM in _dbContext.TSP_SERVEUR_PARAMS on tsrvServeur.TSRV_SERVEURID equals TSP_SERVEUR_PARAM.TSRV_SERVEURID
                        where tpuParalleleu.TRST_STATUTID == StatusLiteral.Available && tpuParalleleu.TPU_INSTANCE == pTpuInstance && TSP_SERVEUR_PARAM.TRST_STATUTID == StatusLiteral.Available
                        select new
                        {
                            TSP_SERVEUR_PARAM.TSP_KEY,
                            TSP_SERVEUR_PARAM.TSP_VALUE
                        };

        IList<ParamAttributes> dataList = new List<ParamAttributes>();

        foreach (var e in queryable)
        {
            dataList.Add(new()
            {
                Key = e.TSP_KEY,
                Value = e.TSP_VALUE
            });
        }

        cpu.Demande.ListParamAttributes = dataList;

        return true;
    }

    /// <summary>
    /// Produire une demande identifiée  : Enregistre les valeurs de Qualif (si fichier qualif a été généré par l'ETL)
    /// </summary>
    /// <returns></returns>
    /// <param name="pTD_DEMANDEID">N° de demande </param>
    /// <param name="pQualif">Enregistrements du fichier Qualif </param>
    private async Task SetQualifDemande(int pTD_DEMANDEID, byte[] pQualif)
    {
        //  structure de pQualif = structure de la table d'accueil TDQ_DEMANDE_QUALIFS hors le champ de la séquence
        var qualif = JsonSerializer.Deserialize<IEnumerable<MappingQualifDemande>>(pQualif);

        //controle conformité contenu
        // on  n'enregistre pas les données si incohérence de n° de demande,  on sort, non bloquant pour les autres process
        if (qualif != null)
        {
            var mappingQualifDemandes = qualif.ToList();

            if (mappingQualifDemandes.Any(x => x.ID_DEMANDE != pTD_DEMANDEID))
            {
                return;
            }

            await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
            {
                await _dbContext.Database.OpenConnectionAsync();
                await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    await dbCTrans.CreateSavepointAsync("Step_001");

                    int nbRecord = 0;

                    foreach (var item in mappingQualifDemandes)
                    {
                        nbRecord += 1;

                        await _dbContext.AddAsync(new TDQ_DEMANDE_QUALIFS
                        {
                            TDQ_NUM_ORDRE = nbRecord,
                            TD_DEMANDEID = item.ID_DEMANDE,
                            TDQ_CODE = item.CODE_QUALIF,
                            TDQ_NOM = item.NOM_QUALIF,
                            TDQ_VALEUR = item.VALEUR_QUALIF,
                            TDQ_NATURE = item.NATURE_QUALIF,
                            TDQ_DATASET = item.DATASET_QUALIF,
                            TDQ_OBJECTIF = "",
                            TDQ_TYPOLOGIE = item.OBJET_QUALIF,
                            TDQ_COMMENT = item.COMMENT_QUALIF
                        });
                    }

                    //on sauve le détail des lignes de Qualif
                    await _dbContext.SaveChangesAsync();

                    int maxqualif = 0;

                    if (pQualif is not null)
                    {
                        // calcul valeur d'indicateur (feux)  on prend la plus mauvaise cad + gde valeur
                        maxqualif = (from tdqDemandeQualif in _dbContext.TDQ_DEMANDE_QUALIFS
                                     where tdqDemandeQualif.TD_DEMANDEID == pTD_DEMANDEID
                                     select tdqDemandeQualif.TDQ_VALEUR).Max() ?? 0;

                        if (maxqualif.Equals(0))
                            _logger.Error($"< -------- Failed: SetQualifDemande, MaxQualif: {maxqualif}, DemandeId: {pTD_DEMANDEID} (wrong parameters)");

                        var demandeAmodifier = (from tdDemande in _dbContext.TD_DEMANDES
                                                where tdDemande.TD_DEMANDEID == pTD_DEMANDEID
                                                select new
                                                {
                                                    TD_DEMANDE = tdDemande
                                                }).First();

                        //on sauve le bilan général
                        if (demandeAmodifier is not null)
                            demandeAmodifier.TD_DEMANDE.TD_QUALIF_BILAN = maxqualif;

                        //       demandeAmodifier.TD_DEMANDE.TD_QUALIF_EXIST_FILE = StatusLiteral.Yes;
                        //      demandeAmodifier.TD_DEMANDE.TD_QUALIF_FILE_SIZE = pTD_QUALIF_FILE_SIZE;
                    }

                    // On ne sauve que si maxqualif est différent de 0, sinon on retourne 0
                    _ = maxqualif.Equals(0)
                        ? 0
                        : await _dbContext.SaveChangesAsync();

                    await dbCTrans.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "< -------- Failed: SetQualifDemande. {Message}", ex.InnerException?.Message ?? ex.Message);

                    await dbCTrans.RollbackToSavepointAsync("Step_001");
                }
            });
        }
    }

    /// <summary>
    /// Enregistre extra info sur upload du fichier de qualif
    /// </summary>
    /// <returns></returns>
    /// <param name="pTdDemandeid">N° de demande </param>
    private async Task<long> SetQualifExtraInfo(int? pTdDemandeid)
    {
        string qualifFile = string.Concat(_configuration["ParallelU:PathQualif"], DemandeID(pTdDemandeid), "_QUALIF.zip");
        long count = 0;
        int vTdQualifFileSize;

        if (!File.Exists(qualifFile))
        {
            return count;
        }

        var qualifFileInfo = new FileInfo(qualifFile);
        vTdQualifFileSize = (int)Math.Floor(Math.Round((double)qualifFileInfo.Length / 1024, 3));

        await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContext.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");

                var demandeAmodifier = (from tdDemande in _dbContext.TD_DEMANDES
                                        where tdDemande.TD_DEMANDEID == pTdDemandeid
                                        select new
                                        {
                                            TD_DEMANDE = tdDemande
                                        }).First();

                if (demandeAmodifier is not null)
                {
                    demandeAmodifier.TD_DEMANDE.TD_QUALIF_EXIST_FILE = StatusLiteral.Yes;
                    demandeAmodifier.TD_DEMANDE.TD_QUALIF_FILE_SIZE = vTdQualifFileSize;
                    count = await _dbContext.SaveChangesAsync();
                }

                // track pour aider refresh des interfaces abonnées
                await _trackedEntitiesServices.AddTrackedEntitiesAsync(null, new[] { typeof(VDE_DEMANDES_ETENDUES).FullName }, Litterals.Update, _sessionId, "SetQualifExtraInfo");
                await dbCTrans.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "< -------- Failed: SetQualifExtraInfo. {Message}", ex.InnerException?.Message ?? ex.Message);
                await dbCTrans.RollbackToSavepointAsync("Step_001");
            }
        });

        return count;
    }

    /// <summary>
    /// Produire une demande identifiée  : Enregistre avancement des batchs
    /// le début de l'exécution d'un batch ou la fin avec son code retour
    /// </summary>
    /// <returns></returns>
    /// <param name="pPhase">Phase (D: Début / F:Fin)</param>
    /// <param name="pTdDemandeid">N° de demande </param>
    /// <param name="pOrdreExecution">N° d'Ordre d'exécution du bat</param>
    /// <param name="pCodeRetour">Code retour du bat </param>
    private async Task SetAvancementBatchDemande(string pPhase, int pTdDemandeid, int pOrdreExecution, int? pCodeRetour)
    {
        await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContext.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");
                var nbTotalBatch = 0;

                foreach (var item in from tbdBatchDemande in _dbContext.TBD_BATCH_DEMANDES
                                     where tbdBatchDemande.TD_DEMANDEID == pTdDemandeid /* && TBD_BATCH_DEMANDE.TBD_ORDRE_EXECUTION == pORDRE_EXECUTION */
                                     select new
                                     {
                                         TBD_BATCH_DEMANDE = tbdBatchDemande
                                     })
                {
                    nbTotalBatch += 1;

                    if (item.TBD_BATCH_DEMANDE.TBD_ORDRE_EXECUTION != pOrdreExecution)
                    {
                        continue;
                    }

                    if (pPhase.Equals("D", StringComparison.Ordinal))
                    {
                        item.TBD_BATCH_DEMANDE.TBD_DATE_DEBUT_EXECUTION = DateExtensions.GetUtcNow();
                    }
                    else
                    {
                        item.TBD_BATCH_DEMANDE.TBD_DATE_FIN_EXECUTION = DateExtensions.GetUtcNow();
                        item.TBD_BATCH_DEMANDE.TBD_CODE_RETOUR = pCodeRetour;
                    }
                }

                await _dbContext.SaveChangesAsync();

                // enregistre pour trace dans table principale des demandes
                foreach (
                    var demandespecifique in from tdDemande in _dbContext.TD_DEMANDES
                                             where tdDemande.TD_DEMANDEID == pTdDemandeid
                                             select new
                                             {
                                                 TD_DEMANDE = tdDemande
                                             }
                )
                {
                    demandespecifique.TD_DEMANDE.TD_INFO_RETOUR_TRAITEMENT =
                        pPhase.Equals("D", StringComparison.Ordinal)
                            ? $"Run batch ordre d'exécution: {pOrdreExecution}, Nb batchs: {nbTotalBatch}"
                            : "";
                }


                await _dbContext.SaveChangesAsync();

                // track pour le refresh des interfaces abonnées
                await _trackedEntitiesServices.AddTrackedEntitiesAsync(null, new[] { typeof(VDE_DEMANDES_ETENDUES).FullName }, Litterals.Update, _sessionId, "SetAvancementBatchDemande");
                await dbCTrans.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "< -------- Failed: SetAvancementBatchDemande. {Message}", ex.InnerException?.Message ?? ex.Message);
                await dbCTrans.RollbackToSavepointAsync("Step_001");
            }
        });
    }

    /// <summary>
    /// Produire une demande identifiée  : Enregistre le bilan de l'éxécution d'une demande (echec ou réussite)
    /// </summary>
    /// <returns></returns>
    /// <param name="pTD_DEMANDEID">N° de demande </param>
    /// <param name="cpuId">Client Id</param>
    /// <param name="pReussite">Tous les batchs ont-ils pu se lancer sans CR d'erreur ?  </param>
    /// <param name="pMsgPourInterface">Msg eventuel ou ras de l avancement  </param>
    private async Task FinalizeDemande(int? pTD_DEMANDEID, string cpuId, bool pReussite, string pMsgPourInterface)
    {
        long count = 0;

        //  verifie présence d un fichier output(Résultat)-- D
        bool vResultExistFile = false;
        int vResultFileSize = 0;

        string resultFile =
            string.Concat(_configuration["ParallelU:PathResult"], DemandeID(pTD_DEMANDEID), "_RESULT.zip");

        if (File.Exists(resultFile))
        {
            vResultExistFile = true;
            var resultFileInfo = new FileInfo(resultFile);
            vResultFileSize = (int)Math.Floor(Math.Round((double)resultFileInfo.Length / 1024, 3));
        }
        //  verifie présence d un fichier output(Résultat)-- F

        await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContext.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContext.Database.BeginTransactionAsync();

            int etatId;
            int? serveurid;
            var ts = TimeSpan.Zero; // durée d'exécution

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");

                var momentFin = DateExtensions.GetUtcNowSecond();
                var momentFinBis = DateExtensions.GetUtcNow(); // momentFin

                foreach (var demandespecifique in from tdDemande in _dbContext.TD_DEMANDES
                                                  where tdDemande.TD_DEMANDEID == pTD_DEMANDEID
                                                  select tdDemande
                        )
                {
                    demandespecifique.TD_DATE_LIVRAISON = momentFinBis;
                    demandespecifique.TRST_STATUTID = pReussite ? StatusLiteral.RealizedRequest : StatusLiteral.InError;
                    demandespecifique.TD_RESULT_EXIST_FILE = vResultExistFile ? StatusLiteral.Yes : StatusLiteral.No;
                    demandespecifique.TD_RESULT_FILE_SIZE = vResultFileSize;
                    demandespecifique.TD_INFO_RETOUR_TRAITEMENT = pMsgPourInterface;

                    etatId = demandespecifique.TE_ETATID;
                    serveurid = demandespecifique.TSRV_SERVEURID;

                    if (pReussite)
                        demandespecifique.TD_DATE_AVIS_GESTIONNAIRE = momentFin;

                    // calcul durée totale du traitement : fix => never try to substract a nullable without checking if it has a value, else you get wrong results!
                    if (demandespecifique.TD_DATE_PRISE_EN_CHARGE.HasValue)
                    {
                        // As of now, we can save total seconds
                        ts = momentFin - demandespecifique.TD_DATE_PRISE_EN_CHARGE.Value;
                        demandespecifique.TD_DUREE_PRODUCTION_REEL = (int)(ts.TotalSeconds > int.MaxValue
                                ? int.MaxValue
                                : ts.TotalSeconds); // (int)Math.Round(ts.TotalMinutes);
                    }
                    else
                    {
                        demandespecifique.TD_DUREE_PRODUCTION_REEL = 0;
                    }

                    count = await _dbContext.SaveChangesAsync();

                    var etat = (from teEtat in _dbContext.TE_ETATS
                                where teEtat.TE_ETATID == etatId
                                select teEtat).FirstOrDefault();

                    if (etat is not null)
                    {
                        etat.TE_DATE_DERNIERE_PRODUCTION = momentFin;
                        etat.TE_DUREE_DERNIERE_PRODUCTION = (int)Math.Round(ts.TotalMinutes);
                    }

                    await _dbContext.SaveChangesAsync();

                    var serveur = (from tsrvServeur in _dbContext.TSRV_SERVEURS
                                   where tsrvServeur.TSRV_SERVEURID == serveurid
                                   select
                                       tsrvServeur).FirstOrDefault();

                    if (serveur is not null)
                    {
                        serveur.TSRV_DATE_DERNIERE_ACTIVITE = momentFin;
                        serveur.TSRV_DUREE_EXPLOITATION = serveur.TSRV_DUREE_EXPLOITATION.HasValue
                            ? serveur.TSRV_DUREE_EXPLOITATION + (int)Math.Round(ts.TotalMinutes)
                            : (int)Math.Round(ts.TotalMinutes);
                    }

                    await _dbContext.SaveChangesAsync();

                    /***** Association TD_DEMANDES and TCMD_COMMANDES. *****/
                    // Association created when the production is created.
                    TDC_DEMANDES_COMMANDES demandesCommandes;

                    // Case of a reccurent production.
                    if (demandespecifique.TD_DEMANDE_ORIGINEID is not null)
                    {
                        // Search if there is an association, using original production id.
                        demandesCommandes = _dbContext.TDC_DEMANDES_COMMANDES
                            .FirstOrDefault(dc => dc.TD_DEMANDEID.Equals(demandespecifique.TD_DEMANDE_ORIGINEID));
                    }
                    // Other cases.
                    else
                    {
                        // Search if there is an association, using production id.
                        demandesCommandes = _dbContext.TDC_DEMANDES_COMMANDES
                            .FirstOrDefault(dc => dc.TD_DEMANDEID.Equals(demandespecifique.TD_DEMANDEID));
                    }

                    // If there is an association.
                    if (demandesCommandes is not null)
                    {
                        // Add new association, between an order TCMD_COMMANDES
                        // and a completed production TD_DEMANDES.
                        TCMD_DA_DEMANDES_ASSOCIEES demandesAssociees = new()
                        {
                            TCMD_COMMANDEID = demandesCommandes.TCMD_COMMANDEID,
                            TD_DEMANDEID = demandespecifique.TD_DEMANDEID,
                            TCMD_DA_DATE_ASSOCIATION = DateExtensions.GetUtcNowSecond()
                        };

                        await _dbContext.AddAsync(demandesAssociees);

                        await _dbContext.SaveChangesAsync();
                    }
                }

                await dbCTrans.CommitAsync();

                if (count > 0)
                {
                    // Reset CpuBackup.Demande.DemandeId from the //U client that is connected
                    if (pTD_DEMANDEID != null)
                    {
                        await _hubContext.Clients.Client(cpuId).SendCoreAsync("OnAfterFinalizeDemande",
                            new object[] { pTD_DEMANDEID.Value, cpuId });
                    }

                    // Track pour aider refresh des interfaces abonnées
                    await _trackedEntitiesServices.AddTrackedEntitiesAsync(null,
                        new[] {
                            typeof(VDE_DEMANDES_ETENDUES).FullName,
                            typeof(TCMD_DA_DEMANDES_ASSOCIEES).FullName
                        },
                        Litterals.Update, _sessionId, "FinalizeDemande");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "< -------- Failed: FinalizeDemande. {Message}", ex.InnerException?.Message ?? ex.Message);
                if (dbCTrans is not null)
                    await dbCTrans.RollbackToSavepointAsync("Step_001");
            }
        });
    }

    #endregion "methodes pour traiter une demande"

    #region CPU API

    /// <summary>
    /// UpdateCpu + Download => ask Client to download a file (relative to FilesStorage) from the Server
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="fullServerPath"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private Task DownloadFileAsync(WorkerNode cpu, string fullServerPath, string clientRelativePath, string fileName)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnDownloadFileAsync", new object[]
        {
            cpu, fullServerPath, clientRelativePath, fileName
        });
    }

    /// <summary>
    /// UpdateCpu + Upload => ask Client to upload a file onto the Server
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="fullServerPath"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public Task UploadFileAsync(WorkerNode cpu, string fullServerPath, string clientRelativePath, string fileName)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnUploadFileAsync", new object[]
        {
            cpu, fullServerPath, clientRelativePath, fileName
        });
    }

    /// <summary>
    /// UpdateCpu + Zip client workspace
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientZipFolder"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="zipFileName"></param>
    /// <returns></returns>
    private Task DirectoryZipAsync(WorkerNode cpu, string clientZipFolder, string clientRelativePath, string zipFileName)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnDirectoryZipAsync", new object[]
        {
            cpu, clientZipFolder, clientRelativePath, zipFileName
        });
    }

    /// <summary>
    /// UpdateCpu + UnZip client workspace
    /// Path is based on Client path + demandeId + overwriteFiles? + deleteZipAfter?
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativeSourcePath"></param>
    /// <param name="zipFileName"></param>
    /// <param name="clientRelativeDestPath"></param>
    /// <returns></returns>
    private Task DirectoryUnZipAsync(WorkerNode cpu, string clientRelativeSourcePath, string zipFileName, string clientRelativeDestPath)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnDirectoryUnZipAsync", new object[]
        {
            cpu, clientRelativeSourcePath, zipFileName, clientRelativeDestPath
        });
    }

    /// <summary>
    /// UpdateCpu + create a directory onto Client (relative to FilesStorage)
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <returns></returns>
    private Task CreateDirectoryAsync(WorkerNode cpu, string clientRelativePath)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnCreateDirectoryAsync", new object[]
        {
            cpu, clientRelativePath
        });
    }

    /// <summary>
    /// Create meta.txt + UpdateCpu + provide metainfo for Demande to Client
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="metadata"></param>
    /// <returns></returns>
    private Task CreateMetaFileAsync(WorkerNode cpu, string clientRelativePath, string metadata)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnCreateMetaFileAsync", new object[]
        {
            cpu, clientRelativePath, metadata
        });
    }

    /// <summary>
    /// Create etlsettings.json + UpdateCpu
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="jwtToken"></param>
    /// <returns></returns>
    private Task CreateEtlSettingsFileAsync(WorkerNode cpu, string clientRelativePath, string jwtToken)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnCreateEtlSettingsFileAsync", new object[]
        {
            cpu, clientRelativePath, jwtToken
        });
    }

    /// <summary>
    /// UpdateCpu + alter Bat on client for specific Demande (path)
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="clientSuffixPath"></param>
    /// <returns></returns>
    private Task AlterCmdAsync(WorkerNode cpu, string clientRelativePath, string clientSuffixPath)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnAlterCmdAsync", new object[]
        {
            cpu, clientRelativePath, clientSuffixPath
        });
    }

    /// <summary>
    /// Run a batch file (or any executable) when useShellExecute = false
    /// https://docs.microsoft.com/fr-fr/dotnet/api/system.diagnostics.processstartinfo.useshellexecute?view=net-5.0
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="parameters"></param>
    /// <param name="useShellExecute"></param>
    /// <returns></returns>
    private Task RunCmdAsync(WorkerNode cpu, string clientRelativePath, string parameters, bool useShellExecute)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnRunCmdAsync", new object[]
        {
            cpu, clientRelativePath, parameters, useShellExecute
        });
    }

    /// <summary>
    /// UpdateCpu + get qualif.csv content from the client
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <param name="csvFileName"></param>
    /// <returns></returns>
    private Task LoadQualifAsync(WorkerNode cpu, string clientRelativePath, string csvFileName)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnLoadQualifAsync", new object[]
        {
            cpu, clientRelativePath, csvFileName
        });
    }

    /// <summary>
    /// UpdateCpu + Delete directory
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <returns></returns>
    private Task DeleteDirectoryAsync(WorkerNode cpu, string clientRelativePath)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnDeleteDirectoryAsync", new object[]
        {
            cpu, clientRelativePath
        });
    }

    /// <summary>
    /// Get RefLog data from demande
    /// TODO Seb: then insert bulk using this code: await _service.InsertRefLogAsync(newCpu);
    /// </summary>
    /// <param name="cpu"></param>
    /// <param name="clientRelativePath"></param>
    /// <returns></returns>
    private Task LoadRefLogAsync(WorkerNode cpu, string clientRelativePath)
    {
        _cpuManager.UpdateCpu(cpu);

        return _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnLoadRefLogAsync", new object[]
        {
            cpu, clientRelativePath
        });
    }

    /// <summary>
    /// Insert ETL logs into TTL_LOGS (replace TTL_DATE_DEBUT and TTL_DATE_FIN accordingly)
    ///   Seb: check if the rule is OK  => changement attribut de date de début
    ///   If we consider TTL_DATE_DEBUT + TTL_DATE_FIN to be set here, then set these infos to NULL within the .json file
    /// </summary>
    /// <param name="cpu"></param>
    /// <returns></returns>
    private async Task InsertRefLogAsync(WorkerNode cpu)
    {
        if (cpu.Demande.JsonData is not null)
        {
            await _dbContextMso.Database.CreateExecutionStrategy().Execute(async () =>
            {
                await _dbContextMso.Database.OpenConnectionAsync();
                await using var dbCTrans = await _dbContextMso.Database.BeginTransactionAsync();

                try
                {
                    await dbCTrans.CreateSavepointAsync("Step_001");
                    var dateDeFin = DateExtensions.GetUtcNow();
                    foreach (var item in JsonSerializer.Deserialize<IEnumerable<TTL_LOGS>>(cpu.Demande.JsonData)!)
                    {
                        // Rules : update each TTL_DATE_DEBUT/TTL_DATE_FIN before inserting, done here because we have to deal with strongly typed TTL_LOGS
                        item.TTL_DATE_DEBUT = cpu.Demande.StartedAt; // moment de la prise en charge de la demande, pas du lancement du client
                        item.TTL_DATE_FIN = dateDeFin;

                        // Rule : set UTC date here
                        item.TTL_FICHIER_DATE_MODIF = DateExtensions.GetUtcNow();

                        // Insert each new line
                        _ = await _dbContextMso.TTL_LOGS.AddAsync(item);
                    }

                    _ = await _dbContextMso.SaveChangesAsync();
                    await dbCTrans.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "< -------- Failed: InsertRefLogAsync. {Message}", ex.InnerException?.Message ?? ex.Message);
                    await dbCTrans.RollbackToSavepointAsync("Step_001");
                }
            });

            // Once terminated JsonData must be null to avoid loop by inadvertance
            cpu.Demande.JsonData = null;
        }
    }

    #endregion

    #region CATALOG (ETQ)

    /// <summary>
    ///  Genere les codes etiquette   
    /// </summary>
    public async Task<IEnumerable<EtqOutput>> EtqGenerate(IEnumerable<CalculateEtqInput> etiquetteInput)
    {
        IList<EtqOutput> etqGenerateGenerate = new List<EtqOutput>();

        foreach (var etq in etiquetteInput)
        {
            etqGenerateGenerate.Add(await EtqCalculate(etq.Guid, etq.CodeObjEtq, etq.Version, etq.CodePerimetre,
                etq.ValDynPerimetre, etq.DemandeId, "EtqCalculate", etq.Simulation));
        }

        return etqGenerateGenerate;
    }

    /// <summary>
    ///  Genere le code etiquette sur la base des criteres
    ///  Injecte en BDD le code généré si executé en mode simul = false
    /// </summary>
    /// <param name="guid">Identifiant technique pour usage ETL</param>
    /// <param name="codeObjEtq">Code de l'objet d'etiquette</param>
    /// <param name="version">Version de l'etiquette</param>
    /// <param name="codePerimetre">Code périmètre ou Null</param>
    /// <param name="valDynPerimetre">Valeur périmètre dynamique</param>
    /// <param name="demandeid">N° demande Orkestra</param>
    /// <param name="pSimul">Mode Simulation</param>
    /// <param name="source">(obsolète) Identifiant technique pour reexploitation entre appel et retour de cette méthode usage hors mode Simulation</param>
    public async Task<EtqOutput> EtqCalculate(string guid, string codeObjEtq, int? version, string codePerimetre,
        string valDynPerimetre, int demandeid, string source, bool pSimul)
    {
        EtqOutput etqgeneration = new()
        {
            Guid = guid, // reprise en en sortie et en l'état du guid fournit en entrée
            Success = false,
            CodeEtq = "",
            Source = source
        };

        var etqDatum =
            from tobjeObjetEtiquette in _dbContextEtq.TOBJE_OBJET_ETIQUETTES
            join tdomDomaine in _dbContextEtq.TDOM_DOMAINES on tobjeObjetEtiquette.TDOM_DOMAINEID equals tdomDomaine.TDOM_DOMAINEID
            join teqcEtqCodif in _dbContextEtq.TEQC_ETQ_CODIFS on tobjeObjetEtiquette.TEQC_ETQ_CODIFID equals teqcEtqCodif.TEQC_ETQ_CODIFID
            join tobfObjFormat in _dbContextEtq.TOBF_OBJ_FORMATS on tobjeObjetEtiquette.TOBF_OBJ_FORMATID equals tobfObjFormat.TOBF_OBJ_FORMATID
            join tobnObjNature in _dbContextEtq.TOBN_OBJ_NATURES on tobjeObjetEtiquette.TOBN_OBJ_NATUREID equals tobnObjNature.TOBN_OBJ_NATUREID
            /*sous requete avec critere restrictif et  en jointure externe , NE PAS DEPLACER LE WHERE AILLEURS ! */
            join sub in from tprcpPrcPerimetre in _dbContextEtq.TPRCP_PRC_PERIMETRES
                        where tprcpPrcPerimetre.TPRCP_CODE == codePerimetre /* pour question de logique ce critere ne peut etre que dans la sous requete, ne surtout pas déplacer */
                        select tprcpPrcPerimetre
             on tdomDomaine.TDOM_DOMAINEID equals sub.TDOM_DOMAINEID into zPerim
            from semifinalPerim in zPerim.DefaultIfEmpty()
            where tobjeObjetEtiquette.TOBJE_CODE == codeObjEtq
            select new
            {
                tdomDomaine.TDOM_CODE,
                tobjeObjetEtiquette.TOBJE_OBJET_ETIQUETTEID,
                tobjeObjetEtiquette.TOBJE_CODE_ETIQUETTAGE,
                teqcEtqCodif.TEQC_CODE_PRC_ORDRE,
                teqcEtqCodif.TEQC_CODE_ETIQUETTAGE_OBJ_ORDRE,
                teqcEtqCodif.TEQC_CODE_PRM_ORDRE,
                teqcEtqCodif.TEQC_INCREMENT_ORDRE,
                teqcEtqCodif.TEQC_INCREMENT_TAILLE,
                teqcEtqCodif.TEQC_INCREMENT_VAL_INIT,
                teqcEtqCodif.TEQC_SEPARATEUR,
                tobjeObjetEtiquette.TOBJE_VERSION,
                tobjeObjetEtiquette.TOBJE_VERSION_ETQ_STATUT,
                finalPerim = semifinalPerim
                /* obligé de controler la nullité sinon plantage EF  */
            };

        string pMsg;

        if (!etqDatum.Any())
        {
            // stop code etiquette introuvable qqe soit la version
            pMsg = "Code étiquette introuvable qqe soit la version";
            etqgeneration.Message = pMsg;

            return etqgeneration;
        }

        // si version est non renseigné alors utiliser critere TOBJE_VERSION_ETQ_STATUT
        etqDatum = version is null or 0
            ? etqDatum.Where(x => x.TOBJE_VERSION_ETQ_STATUT == 1)
            : etqDatum.Where(x => x.TOBJE_VERSION == version);

        // on a filtré sur la version 
        if (!etqDatum.Any()) // stop 
        {
            // ce code etiquette n existe pas sur la version par defaut
            pMsg = version is null or 0
                ? "Ce code étiquette n'existe pas sur la version par défaut"
                :
                // ce code etiquette n existe pas sur la version souhaitée
                "Ce code étiquette n'existe pas sur la version souhaitée";
            etqgeneration.Message = pMsg;

            return etqgeneration;
        }

        /* EtqDatum is not null */
        if (etqDatum.Count() != 1)
        {
            pMsg = "Incohérence en lecture de bdd - tuple trouvé avec ces critères";
            etqgeneration.Message = pMsg;

            return etqgeneration;
            // ko count <> 1
        }

        var item = etqDatum.FirstOrDefault();

        // on doit verifier que la requete a bien trouvé les infos du perimetre si celui ci a été renseigné en parametre
        if (item != null && !string.IsNullOrEmpty(codePerimetre) &&
            codePerimetre != item.finalPerim?.TPRCP_CODE)
        {
            // perimetre non trouvé 
            // stop
            pMsg = "Code et version corrects, mais périmètre non trouvé";
            etqgeneration.Message = pMsg;

            return etqgeneration;
        }

        if (item != null && !string.IsNullOrEmpty(codePerimetre) && codePerimetre == item.finalPerim?.TPRCP_CODE && !string.IsNullOrEmpty(valDynPerimetre))
        {
            // perimetre trouvé,  valeur dynamique fournie par l'utilisateur mais perimetre non configurée pour etre dynamique
            // si  la valeur du parametre valDynPerimetre est identique au code périmetre on accepte meme si la perimetre n'est pas configuré dynamique 
            if (item.finalPerim.TPRCP_PRM_DYN == StatusLiteral.No && item.finalPerim.TPRCP_CODE != valDynPerimetre)
            {
                pMsg = "Code, version et périmètre corrects, mais périmètre non dynamique";
                etqgeneration.Message = pMsg;
                return etqgeneration;
            }
        }
        else if (string.IsNullOrEmpty(codePerimetre) && !string.IsNullOrEmpty(valDynPerimetre))
        {
            // code perimetre non renseigné mais valeur dynamique fournie a tort par l'utilisateur
            pMsg = "Un code périmètre est requis si une valeur de périmètre dynamique est présentée";
            etqgeneration.Message = pMsg;
            return etqgeneration;
        }

        Etq prm = new();
        int? prmid = null; // pour alim BDD

        /* objet constituant le code etiquette final*/
        /*-----dont obligatoire ----*/
        var codeEtiquettage = new Etq
        {
            Code = "TOBJE_CODE_ETIQUETTAGE",
            OrdreField = "TEQC_CODE_ETIQUETTAGE_OBJ_ORDRE",
            OrdreAssemblage = item.TEQC_CODE_ETIQUETTAGE_OBJ_ORDRE ?? 99999,
            Val = item.TOBJE_CODE_ETIQUETTAGE
        };
        //  PRC = new Etq { Code = "TPRS_CODE", OrdreField = "TEQC_CODE_PRC_ORDRE", OrdreAssemblage = item.TEQC_CODE_PRC_ORDRE ?? 99999, Val = item.TPRS_CODE };
        var prc = new Etq
        {
            Code = "TDOM_CODE",
            OrdreField = "TEQC_CODE_PRC_ORDRE",
            OrdreAssemblage = item.TEQC_CODE_PRC_ORDRE ?? 99999,
            Val = item.TDOM_CODE
        };

        /*-----dont optionel ----*/
        if (item.finalPerim is not null && item.finalPerim.TPRCP_CODE is not null) /* 2 criteres requis */
        {
            prm = new Etq
            {
                Code = "TPRCP_CODE",
                OrdreField = "TEQC_CODE_PRM_ORDRE",
                OrdreAssemblage = item.TEQC_CODE_PRM_ORDRE ?? 99999,
                Val = valDynPerimetre ?? item.finalPerim.TPRCP_CODE
            };
            prmid = item.finalPerim.TPRCP_PRC_PERIMETREID;
        }

        /*-----dont automatique (calculé) ----*/
        var increment = new Etq
        {
            Code = "INCREMENT",
            OrdreField = "TEQC_INCREMENT_ORDRE",
            OrdreAssemblage = item.TEQC_INCREMENT_ORDRE,
            Val = null
        };

        IList<Etq> listEtq = new List<Etq>(); // { PRC, PRM, CODE_Etiquettage, INCREMENT };

        if (codeEtiquettage.Val is not null) listEtq.Add(codeEtiquettage);
        if (prc.Val is not null) listEtq.Add(prc); // verif si null ou si ==""
        if (prm.Val is not null) listEtq.Add(prm); // optionnel donc possiblement nul
        listEtq.Add(increment); // sa valeur est a déterminer selon la table cible

        // creation de la chaine au format attendu
        string patternEtq = "";
        int i = 1;
        const string incrementsubstitut = "¤"; // ne pas changer, totalement indépendant du meta separator

        foreach (var etqraw in listEtq.OrderBy(o => o.OrdreAssemblage)) // important de trier cette liste
        {
            if (etqraw.Code.Equals("INCREMENT", StringComparison.Ordinal))
            {
                patternEtq += incrementsubstitut;
            }
            else
            {
                patternEtq += etqraw.Val;
            }

            if (i < listEtq.Count) // on est pas sur le dernier element constitutif du code final
            {
                patternEtq += item.TEQC_SEPARATEUR;
            }

            i += 1;
        }

        // verif présence d'un code etiquette généré précédemment, obtention  valeur max
        // détermination prochaine valeur d'incrément
        //un CONTAINS  est transcrit en instr ce qui n est pas un like donc a proscrire !!

        // recherche par pattern pour garantir que la recherche se fasse qqe soit la version
        var ChkEtqExistant = (
            from tetqEtiquette in _dbContextEtq.TETQ_ETIQUETTES
            where //TETQ_ETIQUETTE.TOBJE_OBJET_ETIQUETTEID == item.TOBJE_OBJET_ETIQUETTEID &&  /*&&   verifier si preferabe de mettre ce critere ou pas*/
                tetqEtiquette.TETQ_PATTERN == patternEtq
            orderby tetqEtiquette.TETQ_INCREMENT descending
            select new
            {
                tetqEtiquette.TETQ_CODE,
                tetqEtiquette.TETQ_INCREMENT
            }
        ).FirstOrDefault();

        int EtqCompteur = ChkEtqExistant is not null
            ? ChkEtqExistant.TETQ_INCREMENT + 1
            : item.TEQC_INCREMENT_VAL_INIT ?? 1;

        string intermediaryEtq = patternEtq.Replace(incrementsubstitut, EtqCompteur.ToString().PadLeft(item.TEQC_INCREMENT_TAILLE, '0'));

        if (!pSimul)
        {
            // enregistrement en bdd
            if (await EtqSaveEtqBdd(intermediaryEtq, patternEtq, EtqCompteur, item.TOBJE_OBJET_ETIQUETTEID, prmid, valDynPerimetre, demandeid))
            {
                // reussite save n db
                etqgeneration.CodeEtq = intermediaryEtq;
                etqgeneration.Success = true;

                return etqgeneration;
            }

            pMsg = "Echec d'insertion en BDD";
            etqgeneration.Message = pMsg;

            return etqgeneration;
        }

        // mode simul, renvoi la valeur sans action en bdd
        etqgeneration.CodeEtq = intermediaryEtq;
        etqgeneration.Success = true;

        return etqgeneration;
        // fin si on a bien trouvé un modele correspondant aux criteres
    }

    /// <summary>
    /// Save new Etq generated previously into DB 
    /// </summary>
    /// <returns></returns>
    private async Task<bool> EtqSaveEtqBdd(string pTetqCode, string patternEtq, int pEtqCompteur,
        int pTobjeObjetEtiquetteid, int? pTprcpPrcPerimetreid = null, string valDynPerimetre = "", int? pDemandeid = null)
    {
        bool reussite = false;

        await _dbContextEtq.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContextEtq.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContextEtq.Database.BeginTransactionAsync();

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");

                var tetq_etiquette = new TETQ_ETIQUETTES
                {
                    TETQ_CODE = pTetqCode,
                    TOBJE_OBJET_ETIQUETTEID = pTobjeObjetEtiquetteid,
                    /*  TETQ_LIB, 
                      TETQ_DESC, */
                    TPRCP_PRC_PERIMETREID = pTprcpPrcPerimetreid,
                    DEMANDEID = pDemandeid,
                    TETQ_VERSION_ETQ = 1,
                    TETQ_INCREMENT = pEtqCompteur,
                    TETQ_PATTERN = patternEtq,
                    TETQ_PRM_VAL = valDynPerimetre,
                    TETQ_DATE_CREATION = DateExtensions.GetUtcNow() //  date de creation de l etiquette
                };

                await _dbContextEtq.AddAsync(tetq_etiquette);

                await _dbContextEtq.SaveChangesAsync();

                var listeregle = from tobjrObjetRegle in _dbContextEtq.TOBJR_OBJET_REGLES
                                 where tobjrObjetRegle.TOBJE_OBJET_ETIQUETTEID == pTobjeObjetEtiquetteid && tobjrObjetRegle.TOBJR_APPLICABLE == StatusLiteral.Yes
                                 select tobjrObjetRegle;

                foreach (var item in listeregle)
                {
                    var tetqrEtqRegles = new TETQR_ETQ_REGLES
                    {
                        TETQ_ETIQUETTEID = tetq_etiquette.TETQ_ETIQUETTEID,
                        TRGL_REGLEID = item.TRGL_REGLEID,
                        TRGLRV_REGLES_VALEURID = item.TRGLRV_REGLES_VALEURID
                    };

                    // ajout dans etqregles de chacune des regles retenues
                    await _dbContextEtq.AddAsync(tetqrEtqRegles);
                }

                await _dbContextEtq.SaveChangesAsync();

                //+ Etq Suivi evt 
                var tseqSuiviEvenenmentEtq = new TSEQ_SUIVI_EVENEMENT_ETQS
                {
                    TETQ_ETIQUETTEID = tetq_etiquette.TETQ_ETIQUETTEID,
                    TSEQ_DATE_EVENEMENT = DateExtensions.GetUtcNow(),
                    TTE_TYPE_EVENEMENTID = 1,
                    TSEQ_COMMENTAIRE = "",
                    TSEQ_DESC = "Création étiquette"
                };

                // ajout dans suivi evenement de l'opération creation d'etiquette
                await _dbContextEtq.AddAsync(tseqSuiviEvenenmentEtq);
                await _dbContextEtq.SaveChangesAsync();

                await _trackedEntitiesServices.AddTrackedEntitiesAsync(null, new[]
                {
                    typeof(TETQ_ETIQUETTES).FullName,
                    typeof(TETQR_ETQ_REGLES).FullName,
                    typeof(TSEQ_SUIVI_EVENEMENT_ETQS).FullName
                }, Litterals.Insert, _sessionId, "EtqSaveBdd");

                await dbCTrans.CommitAsync();

                // parcours de chaque regle etq inséré pour appliquer unitairement les calculs unitaire
                var listeEtqRegle = from tetqrEtqRegle in _dbContextEtq.TETQR_ETQ_REGLES
                                    where tetqrEtqRegle.TETQ_ETIQUETTEID == tetq_etiquette.TETQ_ETIQUETTEID
                                    select tetqrEtqRegle;

                foreach (var item in listeEtqRegle)
                {
                    await EtqAppliqueRegles("", tetq_etiquette.TETQ_ETIQUETTEID, item.TETQR_ETQ_REGLEID, null,
                        item.TRGLRV_REGLES_VALEURID, pTobjeObjetEtiquetteid, "CREATION ETIQUETTE");
                }

                reussite = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "< -------- Failed: EtqSaveEtqBDD. {Message}", ex.InnerException?.Message ?? ex.Message);
                await dbCTrans.RollbackToSavepointAsync("Step_001");
            }
        });

        return reussite;
    }

    /// <summary>
    /// Etiquette  - applique les regles
    ///   
    /// </summary>
    /// <param name="pActeurId">ID acteur</param>
    /// <param name="pTetqEtiquetteid">ID Etiquette</param>
    /// <param name="pTetqrEtqRegleid">ID Règle de l etiquette</param>
    /// <param name="pTrglrvReglesValeuridParent">Valeur précédente associée a la règle parente </param>
    /// <param name="pTrglrvReglesValeurid">Valeur associée à la règle - Sélectionnée par l'utilisateur</param>
    /// <param name="pTobjeObjetEtiquetteid">ID Objet associé a l'étiquette</param>
    /// <param name="pCommentaire">Commentaire pour tracabilité fonctionnelle</param>
    public async Task EtqAppliqueRegles(string pActeurId, int pTetqEtiquetteid, int pTetqrEtqRegleid,
        int? pTrglrvReglesValeuridParent, int pTrglrvReglesValeurid, int pTobjeObjetEtiquetteid, string pCommentaire = "")
    {
        /* DEV en cours => au 18/04/2023 est-ce toujours d'actualité ? */
        /* attention regle liée applicable uniquement si la regle associée existe pour l etiquette donnée*/

        await _dbContextEtq.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContextEtq.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContextEtq.Database.BeginTransactionAsync();

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");

                /*recuperation des infos directement associées à la nouvelle regle/valeur , 1 seul enregistrement */
                var detailreglevaleur = (from trglrvReglesValeur in _dbContextEtq.TRGLRV_REGLES_VALEURS
                                             /*  sous requete  en jointure externe , NE PAS DEPLACER LE WHERE AILLEURS si existant ! */
                                         join sub2 in from tactAction in _dbContextEtq.TACT_ACTIONS
                                                      select tactAction

                                             on trglrvReglesValeur.TACT_ACTIONID equals sub2.TACT_ACTIONID into subAction
                                         from tact_action_optionnel in subAction.DefaultIfEmpty()
                                         join tobjrObjetRegle in _dbContextEtq.TOBJR_OBJET_REGLES on
                                             new { aa = trglrvReglesValeur.TRGL_REGLEID, bb = pTobjeObjetEtiquetteid }
                                             equals new
                                             {
                                                 aa = tobjrObjetRegle.TRGL_REGLEID,
                                                 bb = tobjrObjetRegle.TOBJE_OBJET_ETIQUETTEID
                                             }
                                         where trglrvReglesValeur.TRGLRV_REGLES_VALEURID == pTrglrvReglesValeurid
                                         join trglRegle in _dbContextEtq.TRGL_REGLES on trglrvReglesValeur.TRGL_REGLEID equals
                                             trglRegle.TRGL_REGLEID
                                         select new
                                         {
                                             TRGLRV_REGLES_VALEUR = trglrvReglesValeur,
                                             ActionOptionnel = tact_action_optionnel,
                                             tobjrObjetRegle.TOBJR_ECHEANCE_DUREE,
                                             trglRegle.TRGL_LIMITE_TEMPS
                                         }
                    ).FirstOrDefault();

                // on applique la nouvelle valeur retenue ( valeur par defaut a la création ou retenu par l'utilisateur via IHM)
                var tetqr_etq_regle = (from tetqrEtqRegle in _dbContextEtq.TETQR_ETQ_REGLES
                                       where tetqrEtqRegle.TETQR_ETQ_REGLEID == pTetqrEtqRegleid
                                       select
                                           tetqrEtqRegle).FirstOrDefault();

                if (tetqr_etq_regle != null)
                {
                    tetqr_etq_regle.TRGLRV_REGLES_VALEURID = pTrglrvReglesValeurid;

                    // on change la valeur de la regle donc on reinitialise les anciennes propriétés avant de déterminer si ils doivent etre de nouveaux renseignés
                    /*  tetqr_etq_regle.TETQR_ECHEANCE = null;
                  tetqr_etq_regle.TETQR_DATE_DEBUT = null;
                  tetqr_etq_regle.TETQR_DATE_FIN = null;
                */
                    /* FINALEMENT ON GARDE les valeurs des anciennes date provenant de la regle précédente , elles seront ecrasées par nouveau calcul si la nouvelle valeur de regle est configurée comme telle */
                    // si il existe une action associée on récupère le code de l action sinon on efface l ancienne valeur potentielle
                    tetqr_etq_regle.TETQR_ETQ_REGLES_ACTION = detailreglevaleur?.ActionOptionnel?.TACT_LIB;

                    tetqr_etq_regle.TETQR_REGLE_LIEE = EtqGetLibRegleValeur(1, pTrglrvReglesValeuridParent);

                    if (detailreglevaleur != null && detailreglevaleur.TOBJR_ECHEANCE_DUREE is not null
                                                  && detailreglevaleur.TRGLRV_REGLES_VALEUR
                                                      ?.TRGLRV_DEPART_LIMITE_TEMPS == StatusLiteral.Yes)
                    {
                        // ICI CALCUL TEMPS
                        //Si LIMITE_TEMPS='ECHEANCE' alors T_ETQ_REGLES.DATE_APPLICABLE = Date du jour + ECHEANCE_DUREE.
                        //Si LIMITE_TEMPS='DUREE'    alors T_EQT_REGLES.DATE_DEBUT = Date du jour et T_ETQ_REGLES.DATE_FIN = Date du jour +ECHEANCE_DUREE."			

                        switch (detailreglevaleur.TRGL_LIMITE_TEMPS)
                        {
                            case "ECHEANCE":
                                tetqr_etq_regle.TETQR_ECHEANCE = DateExtensions.GetUtcNow().AddDays((double)detailreglevaleur.TOBJR_ECHEANCE_DUREE);
                                break;
                            case "DUREE":
                                tetqr_etq_regle.TETQR_DATE_DEBUT = DateExtensions.GetUtcNow();
                                tetqr_etq_regle.TETQR_DATE_FIN = DateExtensions.GetUtcNow().AddDays((double)detailreglevaleur.TOBJR_ECHEANCE_DUREE);
                                break;
                        }
                    }
                }

                await _dbContextEtq.SaveChangesAsync();

                //+ Etq Suivi evt 
                var tseqSuiviEvenenmentEtq = new TSEQ_SUIVI_EVENEMENT_ETQS
                {
                    TETQ_ETIQUETTEID = pTetqEtiquetteid,
                    TSEQ_DATE_EVENEMENT = DateExtensions.GetUtcNow(),
                    TTE_TYPE_EVENEMENTID = 3,
                    TSEQ_COMMENTAIRE = string.IsNullOrEmpty(pCommentaire) ? null : pCommentaire,
                    TSEQ_DESC = EtqGetLibRegleValeur(0, pTrglrvReglesValeurid),
                    TRU_ACTEURID = string.IsNullOrEmpty(pActeurId) ? null : pActeurId
                };

                // ajout dans suivi evenement de l'opération MAJ valeur de regle
                await _dbContextEtq.AddAsync(tseqSuiviEvenenmentEtq);
                await _dbContextEtq.SaveChangesAsync();

                await _trackedEntitiesServices.AddTrackedEntitiesAsync(null, new[]
                {
                    typeof(TETQR_ETQ_REGLES).FullName,
                    typeof(TSEQ_SUIVI_EVENEMENT_ETQS).FullName
                }, Litterals.InsertOrUpdate, _sessionId, "EtqAppliqueRegles");

                await dbCTrans.CommitAsync();

                /* 1 a N regles/valeurs associées a la regle/valeur initiale */
                var linkedRules = from trgliReglesLiee in _dbContextEtq.TRGLI_REGLES_LIEES
                                  join trglrvReglesValeurliee in _dbContextEtq.TRGLRV_REGLES_VALEURS on trgliReglesLiee.TRGLRV_REGLES_VALEURLIEEID equals trglrvReglesValeurliee.TRGLRV_REGLES_VALEURID
                                  join ztrglRegle in _dbContextEtq.TRGL_REGLES on trglrvReglesValeurliee.TRGL_REGLEID equals ztrglRegle.TRGL_REGLEID
                                  where trgliReglesLiee.TRGLRV_REGLES_VALEURID == pTrglrvReglesValeurid
                                  select new
                                  {
                                      yy = trglrvReglesValeurliee,
                                      zz = ztrglRegle
                                  };

                // si au moins une regle liée applicable pour cette même etiquette
                if (linkedRules.Any())
                {
                    foreach (var item in linkedRules)
                    {
                        var tetqr_etq_regle_cible = from tetqrEtqRegle in _dbContextEtq.TETQR_ETQ_REGLES
                                                    where tetqrEtqRegle.TRGL_REGLEID == item.zz.TRGL_REGLEID
                                                          && tetqrEtqRegle.TETQ_ETIQUETTEID == pTetqEtiquetteid
                                                    select
                                                        tetqrEtqRegle;

                        if (tetqr_etq_regle_cible.Any())
                        {
                            // on met a jout la valeur pour la RG liée et pour l effet cascade 
                            // on appelle en recursif ce qui permet de mettre a jour les regles liées 
                            //  await EtqAppliqueRegles(pActeurId, pTETQ_ETIQUETTEID, tetqr_etq_regle_cible.FirstOrDefault().TETQR_ETQ_REGLEID, tetqr_etq_regle_cible.FirstOrDefault().TRGLRV_REGLES_VALEURID, item.yy.TRGLRV_REGLES_VALEURID, pTOBJE_OBJET_ETIQUETTEID, pcommentaire);
                            await EtqAppliqueRegles(pActeurId, pTetqEtiquetteid,
                                tetqr_etq_regle_cible.FirstOrDefault()!.TETQR_ETQ_REGLEID, pTrglrvReglesValeurid,
                                item.yy.TRGLRV_REGLES_VALEURID, pTobjeObjetEtiquetteid, pCommentaire);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "< -------- Failed: EtqAppliqueRegles. {Message}", ex.InnerException?.Message ?? ex.Message);

                if (dbCTrans is not null)
                    await dbCTrans.RollbackToSavepointAsync("Step_001");
            }
        });
    }

    /// <summary>
    /// Etiquette - Construit chaine concaténée du Code Regle et sa valeur 
    /// </summary>
    /// <param name="pTrglrvReglesValeurid">ID de Règle Valeur</param>
    /// <param name="mode">pour déterminer le format 0 pour suivi evt, 1 pour alimenter Etq</param>
    private string EtqGetLibRegleValeur(int mode, int? pTrglrvReglesValeurid)
    {
        // cas lors de la 1ere phase de creation d'etiquette
        if (pTrglrvReglesValeurid is not null)
        {
            try
            {
                var listCodeEtq = (from trglrvReglesValeur in _dbContextEtq.TRGLRV_REGLES_VALEURS
                                   join trglRegle in _dbContextEtq.TRGL_REGLES on trglrvReglesValeur.TRGL_REGLEID equals trglRegle.TRGL_REGLEID
                                   where trglrvReglesValeur.TRGLRV_REGLES_VALEURID == pTrglrvReglesValeurid
                                   select new
                                   {
                                       trglrvReglesValeur.TRGLRV_VALEUR,
                                       trglRegle.TRGL_CODE_REGLE,
                                       trglRegle.TRGL_LIB_REGLE
                                   }).FirstOrDefault();

                if (listCodeEtq != null)
                    return mode == 0
                        ? $"La règle '{listCodeEtq.TRGL_LIB_REGLE}' prend la valeur '{listCodeEtq.TRGLRV_VALEUR}'"
                        : $"{listCodeEtq.TRGL_CODE_REGLE}  : {listCodeEtq.TRGLRV_VALEUR}";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "< -------- Failed: EtqGetLibRegleValeur. {Message}", ex.InnerException?.Message ?? ex.Message);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Etiquette - Alimente table des ressources pour 1 à N etiquettes
    /// </summary>
    /// <param name="suiviRessources">records</param>
    public async Task<IEnumerable<EtqOutput>> EtqSuiviRessource(IEnumerable<EtqSuiviRessourceFileRaw> suiviRessources)
    {
        IList<EtqOutput> EtqSuiviRessourceOutput = new List<EtqOutput>();

        // verifier cohérence des enregistrements qui présentent le memem guid 
        var etqSuiviRessourceFileRaws = suiviRessources.ToList();

        foreach (string guid in etqSuiviRessourceFileRaws.Select(x => x.Guid).Distinct())
        {
            var myout = await EtqSuiviRessourceUnitaire(etqSuiviRessourceFileRaws.Where(x => x.Guid == guid));
            EtqSuiviRessourceOutput.Add(myout);
        }

        return EtqSuiviRessourceOutput;
    }

    /// <summary>
    /// Etiquette - Alimente table des ressources pour une etiquette donnée 
    /// Plusieurs enregistrements possibles mais toutes portent sur la même Etq
    /// </summary>
    /// <param name="suiviRessources">records</param>
    private async Task<EtqOutput> EtqSuiviRessourceUnitaire(IEnumerable<EtqSuiviRessourceFileRaw> suiviRessources)
    {
        var etqSuiviRessourceFileRaws = suiviRessources.ToList();

        EtqOutput etqSuiviRessourceLoad = new()
        {
            Success = false,
            CodeEtq = string.Empty,
            Guid = etqSuiviRessourceFileRaws.FirstOrDefault()?.Guid
        };

        var etqSuiviRessourceFile = new EtqSuiviRessourceFile();

        foreach (var item in etqSuiviRessourceFileRaws)
        {
            etqSuiviRessourceFile.ListEtqSuiviRessourceFileRaw.Add(item);
        }

        if (etqSuiviRessourceFile.ListEtqSuiviRessourceFileRaw.Count == 0)
        {
            etqSuiviRessourceLoad.Message = "Etq - Pas de données exploitable";
            return etqSuiviRessourceLoad;
        }

        var listCodeEtq = from chkEtqUnique in etqSuiviRessourceFile.ListEtqSuiviRessourceFileRaw
                          group chkEtqUnique by chkEtqUnique.CodeETQ
            into gpe
                          select new { Code_ETQ = gpe.Key };

        var enumerable = listCodeEtq.ToList();
        if (enumerable.Count > 1)
        {
            // erreur multi code Etq trouvé au sein d'un même ensemble                
            etqSuiviRessourceLoad.Message =
                "Etq - Multi code Etq trouvé au sein d'un même ensemble, un unique est attendu";
            return etqSuiviRessourceLoad;
        }

        var codeEtq = enumerable.FirstOrDefault()?.Code_ETQ;
        etqSuiviRessourceLoad.CodeEtq = codeEtq;



        // un seul Code Etq est dans l'ensemble constitué par le Guid
        // controle commun a tous les enregistrements :
        // verif que le codeEtq est dans la table TETQ_ETIQUETTES, recupération de TETQ_ETIQUETTEID
        var tetq_etiquette =
            from tetqEtiquette in _dbContextEtq.TETQ_ETIQUETTES
            where tetqEtiquette.TETQ_CODE == codeEtq
            select tetqEtiquette;

        if (!tetq_etiquette.Any())
        {
            // code etq inconnu                
            etqSuiviRessourceLoad.Message = $"Code etq inconnu : {codeEtq}";

            return etqSuiviRessourceLoad;
        }

        //--------OB-415 part 1/3
        //--------- vérification de cohérence entre etq cible et d'origine  
        var listEtq = (from chkEtqCoherence in etqSuiviRessourceFile.ListEtqSuiviRessourceFileRaw
                       .Where(x => x.CodeETQ.Equals(x.ValeurEntree))
                       select chkEtqCoherence)?.FirstOrDefault();
        if (listEtq is not null)
        {
            etqSuiviRessourceLoad.CodeEtq = listEtq.CodeETQ;
            etqSuiviRessourceLoad.Message = "Etq - Valeur d'entrée ne peut être identique au code etiquette";
            return etqSuiviRessourceLoad;
        }
        //----------------------
        //--------OB-415 part 2/3
        //-------- controle de viabilité des données du champ ENTREE---------
        // une ENTREE ne doit etre présent qu'une fois maximum, on recupere le code etq qui est unique pour renvoi a ETL 
        var listEntrees = (from chkEntreesUnique in etqSuiviRessourceFile.ListEtqSuiviRessourceFileRaw
                           group chkEntreesUnique by new { chkEntreesUnique.CodeETQ, chkEntreesUnique.Entree } into gpe
                           select new { CodeETQ = gpe.Key.CodeETQ, Entree = gpe.Key.Entree, cnt = gpe.Count() });

        foreach (var line in listEntrees.ToList())
        {
            if (line.cnt > 1)
            {
                etqSuiviRessourceLoad.CodeEtq = line.CodeETQ;
                etqSuiviRessourceLoad.Message = "Etq - Présence de valeurs ENTREE non distinctes";
                return etqSuiviRessourceLoad;
            }
        }

        //--------OB-415 part 3/3
        //--------- vérification d'unicité de ValeurEntree 2023.06.27 
        var listValeurEntree = (from chkValeurEntreesUnique in etqSuiviRessourceFile.ListEtqSuiviRessourceFileRaw
                                group chkValeurEntreesUnique by new { chkValeurEntreesUnique.CodeETQ, chkValeurEntreesUnique.ValeurEntree } into gpe
                                select new { CodeETQ = gpe.Key.CodeETQ, ValeurEntree = gpe.Key.ValeurEntree, cnt = gpe.Count() });

        foreach (var line in listValeurEntree.ToList())
        {
            if (line.cnt > 1)
            {
                etqSuiviRessourceLoad.CodeEtq = line.CodeETQ;
                etqSuiviRessourceLoad.Message = "Etq - Présence de valeurs VALEURENTREE non distinctes";
                return etqSuiviRessourceLoad;
            }
        }
        /* ---------------------------------------------------------------------*/

        // verif si existance d'enregistrements sur cette meme etq dans la table d'accueil
        // on supprimera ces enregistrements avant d'ajouter les nvx exemplaires 
        var tsrSuiviRessource =
            from tsrSuiviRessources in _dbContextEtq.TSR_SUIVI_RESSOURCES
            where tsrSuiviRessources.TETQ_ETIQUETTEID == tetq_etiquette.FirstOrDefault().TETQ_ETIQUETTEID
            select tsrSuiviRessources;

        await _dbContextEtq.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContextEtq.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContextEtq.Database.BeginTransactionAsync();

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");

                // des données de suivi concernant cette meme etq sont présent dans la table, on les supprime
                if (tsrSuiviRessource.Any())
                {
                    //await _dbContextEtq.BulkDeleteAsync(tsrSuiviRessource.ToList());
                    _dbContextEtq.RemoveRange(tsrSuiviRessource.ToList());
                }

                bool continueRun = true;

                foreach (var record in etqSuiviRessourceFile.ListEtqSuiviRessourceFileRaw)
                {
                    int? etiquetteParentId = null;

                    var ttrTypeRessource =
                        from ttrTypeRessources in _dbContextEtq.TTR_TYPE_RESSOURCES
                        where ttrTypeRessources.TTR_TYPE_ENTREE == record.TypeRessource
                        select ttrTypeRessources;

                    if (!ttrTypeRessource.Any())
                    {
                        etqSuiviRessourceLoad.Message = $"Code etq connu mais type entrée de ressource inconnu : {record.TypeRessource}";
                        continueRun = false;
                        break; //return etqSuiviRessourceLoad;
                    }

                    if (ttrTypeRessource.FirstOrDefault()!.TTR_TYPE_ENTREE.Equals("ETIQUETTE", StringComparison.OrdinalIgnoreCase))
                    {
                        // verification que la valeur de code etiquette fournie en valeur est elle même présente dans la table etiquette
                        var tetq_etiquetteBis =
                            from tetqEtiquette in _dbContextEtq.TETQ_ETIQUETTES
                            where tetqEtiquette.TETQ_CODE == record.ValeurEntree
                            select tetqEtiquette;

                        if (record.ValeurEntree is null || !tetq_etiquetteBis.Any())
                        {
                            etqSuiviRessourceLoad.Message = $"Valeur d'entrée Code etq introuvable ou non renseigné : {record.ValeurEntree}";
                            continueRun = false;
                            break; //   return etqSuiviRessourceLoad;
                        }

                        etiquetteParentId = tetq_etiquetteBis.FirstOrDefault()?.TETQ_ETIQUETTEID;
                    }

                    await _dbContextEtq.AddAsync(
                        new TSR_SUIVI_RESSOURCES
                        {
                            TETQ_ETIQUETTEID = tetq_etiquette.FirstOrDefault()?.TETQ_ETIQUETTEID,
                            TSR_ENTREE = record.Entree,
                            TTR_TYPE_RESSOURCEID = ttrTypeRessource.FirstOrDefault()?.TTR_TYPE_RESSOURCEID,
                            TSR_VALEUR_ENTREE = record.ValeurEntree,
                            TETQ_ETIQUETTE_ENTREEID = etiquetteParentId
                        });

                    await _dbContextEtq.SaveChangesAsync();
                } // boucle sur chaque enregistrement 

                if (continueRun)
                {
                    await dbCTrans.CommitAsync();
                    etqSuiviRessourceLoad.Success = true;
                }
                else
                {
                    // on annule les actions qui auraient pu être faite si on considere qu'une donnée en entrée ne justifie pas la validation globale
                    await dbCTrans.RollbackToSavepointAsync("Step_001");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "< -------- Failed: EtqSuiviRessource. {Message}", ex.InnerException?.Message ?? ex.Message);
                etqSuiviRessourceLoad.Message = "Exception dans EtqSuiviRessource: " + ex.Message;

                await dbCTrans.RollbackToSavepointAsync("Step_001");
            }
        });

        return etqSuiviRessourceLoad;
    }

    /// <summary>
    /// Etiquette - Mettre a jour les champs Descriptif de 1 à N etiquette (LIB /DESC) 
    /// </summary>
    /// <param name="extraInfosAddon">records</param>
    public async Task<IEnumerable<EtqOutput>> EtqExtraInfoAddon(IEnumerable<EtqExtraInfoAddonFileRaw> extraInfosAddon)
    {
        IList<EtqOutput> etqExtraInfoAddonOutput = new List<EtqOutput>();
        var etqExtraInfoAddonFileRaws = extraInfosAddon.ToList();

        foreach (var guid in etqExtraInfoAddonFileRaws.Select(x => x.Guid).Distinct())
        {
            var myout = await EtqExtraInfoAddonUnitaire(etqExtraInfoAddonFileRaws.Where(x => x.Guid.Equals(guid, StringComparison.OrdinalIgnoreCase)));
            etqExtraInfoAddonOutput.Add(myout);
        }

        return etqExtraInfoAddonOutput;
    }

    /// <summary>
    /// Etiquette - Mettre a jour les champs Descriptif d'une etiquette (LIB /DESC) 
    /// </summary>
    /// <param name="extraInfosAddon">Records</param>
    private async Task<EtqOutput> EtqExtraInfoAddonUnitaire(IEnumerable<EtqExtraInfoAddonFileRaw> extraInfosAddon)
    {
        var etqExtraInfoAddonFileRaws = extraInfosAddon.ToList();

        EtqOutput etqOutput = new()
        {
            Success = false,
            CodeEtq = string.Empty,
            Guid = etqExtraInfoAddonFileRaws.FirstOrDefault()?.Guid
        };

        var etqExtraInfoAddonFile = new EtqExtraInfoAddonFile();

        foreach (var item in etqExtraInfoAddonFileRaws)
        {
            etqExtraInfoAddonFile.ListEtqExtraInfoAddonFileRaw.Add(item);
        }

        string pMsg;

        if (etqExtraInfoAddonFile.ListEtqExtraInfoAddonFileRaw.Count == 0)
        {
            pMsg = "Etq - Pas de données exploitable";
            etqOutput.Message = pMsg;

            return etqOutput;
        }

        await _dbContextEtq.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContextEtq.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContextEtq.Database.BeginTransactionAsync();

            try
            {
                await dbCTrans.CreateSavepointAsync("Step_001");

                //long nb = 0;
                foreach (var addonFile in etqExtraInfoAddonFile.ListEtqExtraInfoAddonFileRaw)
                {
                    etqOutput.CodeEtq = addonFile.CodeETQ;

                    var tetq_etiquette =
                        (from tetqEtiquette in _dbContextEtq.TETQ_ETIQUETTES
                         where tetqEtiquette.TETQ_CODE == addonFile.CodeETQ
                         select tetqEtiquette).FirstOrDefault();

                    if (tetq_etiquette is not null)
                    {
                        tetq_etiquette.TETQ_LIB = addonFile.EtqLib;
                        tetq_etiquette.TETQ_DESC = addonFile.EtqDesc;
                        await _dbContextEtq.SaveChangesAsync();
                        etqOutput.Success = true;
                    }
                    else
                    {
                        etqOutput.Message = "Mise à jour impossible. Code etiquette introuvable";
                    }
                } // boucle sur chaque enregistrement 

                await dbCTrans.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "< -------- Failed: EtqExtraInfoAddon. {Message}", ex.InnerException?.Message ?? ex.Message);
                pMsg = $"Exception dans EtqExtraInfoAddon: {ex.Message}";
                etqOutput.Message = pMsg;

                await dbCTrans.RollbackToSavepointAsync("Step_001");
            }
        });

        return etqOutput;
    }

    #endregion

    #region AUTHORIZATION RULES (DTF)

    /// <summary>
    ///  DTF : menu nouvelle production : extraction des etats pour lesquels il existe au moins une habilitation de producteur sur un des scenario de l etat pour un user spécifique
    /// </summary>
    /// <param name="isAdminMode">Accès en mode admin</param>
    /// <param name="userId">Utilisateur connecté</param>
    /// <param name="catId">valeur optionnelle de filtrage de catégorie </param>
    public IEnumerable<TE_ETATS> GetExecutableTeEtatsForDTF(bool isAdminMode, string userId, int catId)
    {
        // le LET pour faire un Exists sans ramener des enregistrements de la sous requete
        if (!isAdminMode)
        {
            var data = from teEtat in _dbContext.TE_ETATS
                       join temEtatMaster in _dbContext.TEM_ETAT_MASTERS.Where(tem =>
                           tem.TRST_STATUTID == StatusLiteral.Available) on teEtat.TEM_ETAT_MASTERID equals temEtatMaster
                           .TEM_ETAT_MASTERID
                       join tcCategorie in _dbContext.TC_CATEGORIES on temEtatMaster.TC_CATEGORIEID equals tcCategorie
                           .TC_CATEGORIEID
                       let scenarios =
                           (from tsScenario in _dbContext.TS_SCENARIOS.Where(
                                   ts => ts.TRST_STATUTID == StatusLiteral.Available)
                            join vdtfhHabilitation in _dbContext.VDTFH_HABILITATIONS.Where(x =>
                            x.TRU_USERID == userId && x.PRODUCTEUR == 1) on tsScenario.TS_SCENARIOID equals
                        vdtfhHabilitation.TS_SCENARIOID
                            select tsScenario
                           ).ToList()
                       where (teEtat.TRST_STATUTID == StatusLiteral.Available ||
                              teEtat.TRST_STATUTID == StatusLiteral.Prototype)
                             && scenarios.Any(x => x.TE_ETATID.Equals(teEtat.TE_ETATID))
                       orderby tcCategorie.TC_NOM, teEtat.TE_NOM_ETAT, teEtat.TE_VERSION descending
                       select new { TE_ETAT = teEtat, TEM_ETAT_MASTER = temEtatMaster, TC_CATEGORIE = tcCategorie };

            return catId > 0
                ? data.Where(a => a.TE_ETAT.TEM_ETAT_MASTER.TC_CATEGORIEID == catId).AsEnumerable()
                    .Select(x => x.TE_ETAT)
                : data.AsEnumerable().Select(x => x.TE_ETAT);
        }
        else
        {
            // admin mode donc on n'utilise pas VDTFH_HABILITATIONS
            var data = from teEtat in _dbContext.TE_ETATS
                       join temEtatMaster in _dbContext.TEM_ETAT_MASTERS.Where(tem => tem.TRST_STATUTID == StatusLiteral.Available) on teEtat.TEM_ETAT_MASTERID equals temEtatMaster.TEM_ETAT_MASTERID
                       join tcCategorie in _dbContext.TC_CATEGORIES on temEtatMaster.TC_CATEGORIEID equals tcCategorie.TC_CATEGORIEID
                       let scenarios = (from tsScenario in _dbContext.TS_SCENARIOS.Where(ts => ts.TRST_STATUTID == StatusLiteral.Available)
                                        select tsScenario
                       ).ToList()
                       where (teEtat.TRST_STATUTID == StatusLiteral.Available || teEtat.TRST_STATUTID == StatusLiteral.Prototype) && scenarios.Any(x => x.TE_ETATID.Equals(teEtat.TE_ETATID))
                       orderby tcCategorie.TC_NOM, teEtat.TE_NOM_ETAT, teEtat.TE_VERSION descending
                       select new { TE_ETAT = teEtat, TEM_ETAT_MASTER = temEtatMaster, TC_CATEGORIE = tcCategorie };

            return catId > 0
                ? data.Where(a => a.TE_ETAT.TEM_ETAT_MASTER.TC_CATEGORIEID == catId).AsEnumerable()
                    .Select(x => x.TE_ETAT)
                : data.AsEnumerable().Select(x => x.TE_ETAT);
        }
    }

    #endregion

    #region CALENDAR (DTF)

    /// <summary>
    /// Calcul des demandes théoriques via décodage du CRON par demande éligibles
    /// </summary>
    /// <param name="dtUtcStart">Date de départ (UTC)</param>
    /// <param name="dtUtcEnd">Date de fin (UTC)</param>
    /// <returns>List of ModeleDemandeCalendar</returns>
    public IEnumerable<ModeleDemandeCalendar> GetTheoricalDemandesCalendar(DateTimeOffset dtUtcStart, DateTimeOffset dtUtcEnd)
    {
        /* Implémentation lecture du CRON pour décoder les 
           DATE DEBUT ET DATE FIN sont  en GMT de l'utilisateur, le champ TPF_TIMEZONE_INFOID etant utilisé pour convertir en UTC
        */
        var listModelDemande = (from teEtat in _dbContext.TE_ETATS
                                join tdDemande in _dbContext.TD_DEMANDES on teEtat.TE_ETATID equals tdDemande.TE_ETATID
                                join tpfPlanif in _dbContext.TPF_PLANIFS on tdDemande.TD_DEMANDEID equals tpfPlanif.TPF_DEMANDE_ORIGINEID
                                where tpfPlanif.TRST_STATUTID == StatusLiteral.Available
                                join truUserDemandeur in _dbContext.TRU_USERS on tdDemande.TRU_DEMANDEURID equals truUserDemandeur.TRU_USERID
                                join truUserDeclarantPlanif in _dbContext.TRU_USERS on tpfPlanif.TRU_DECLARANTID equals truUserDeclarantPlanif.TRU_USERID
                                join tsScenario in _dbContext.TS_SCENARIOS on tdDemande.TS_SCENARIOID equals tsScenario.TS_SCENARIOID
                                where tsScenario.TRST_STATUTID == StatusLiteral.Available
                                join temEtatMaster in _dbContext.TEM_ETAT_MASTERS on teEtat.TEM_ETAT_MASTERID equals temEtatMaster.TEM_ETAT_MASTERID
                                join tcCategorie in _dbContext.TC_CATEGORIES on temEtatMaster.TC_CATEGORIEID equals tcCategorie.TC_CATEGORIEID
                                where temEtatMaster.TRST_STATUTID == StatusLiteral.Available &&
                                      tdDemande.TRST_STATUTID == StatusLiteral.ScheduleModel &&
                                      (teEtat.TRST_STATUTID == StatusLiteral.Available ||
                                       teEtat.TRST_STATUTID == StatusLiteral.Prototype
                                      )
                                      &&
                                      (from temfEtatMasterFerme in _dbContext.TEMF_ETAT_MASTER_FERMES
                                       join tfFerme in _dbContext.TF_FERMES on temfEtatMasterFerme.TF_FERMEID equals tfFerme.TF_FERMEID
                                       where tfFerme.TRST_STATUTID == StatusLiteral.Available
                                       join tpufParalleleuFerme in _dbContext.TPUF_PARALLELEU_FERMES on tfFerme.TF_FERMEID equals tpufParalleleuFerme.TF_FERMEID
                                       where tpufParalleleuFerme.TRST_STATUTID == StatusLiteral.Available
                                       join tpuParalleleu in _dbContext.TPU_PARALLELEUS on tpufParalleleuFerme.TPU_PARALLELEUID equals tpuParalleleu.TPU_PARALLELEUID
                                       where tpuParalleleu.TRST_STATUTID == StatusLiteral.Available
                                       select temfEtatMasterFerme.TEM_ETAT_MASTERID
                                      )
                                      .Contains(temEtatMaster.TEM_ETAT_MASTERID)
                                join truUserRespFonc in _dbContext.TRU_USERS on temEtatMaster.TRU_RESPONSABLE_FONCTIONNELID equals truUserRespFonc.TRU_USERID
                                /* where TD_DEMANDE.TD_DEMANDEID == 84  ----------------TEST */
                                select new ModeleDemandeCalendar
                                {
                                    TD_DEMANDEID = 0, //TD_DEMANDE.TD_DEMANDEID,
                                    TE_ETATID = tdDemande.TE_ETATID,
                                    TS_SCENARIOID = tdDemande.TS_SCENARIOID,
                                    TS_NOM_SCENARIO = tsScenario.TS_NOM_SCENARIO, // take
                                    TE_NOM_ETAT_VERSION = teEtat.TE_FULLNAME, // take
                                    TRU_DEMANDEURID = tdDemande.TRU_DEMANDEURID,
                                    TRU_DECLARANTID = tpfPlanif.TRU_DECLARANTID,
                                    TRU_DEMANDEUR = truUserDemandeur.TRU_FULLNAME, // take
                                    TRU_USER_RESP_FONC = truUserRespFonc.TRU_FULLNAME,
                                    TRU_USER_DECLARANT_PLANIF = truUserDeclarantPlanif.TRU_FULLNAME,
                                    TPF_PLANIFID = tpfPlanif.TPF_PLANIFID,
                                    TPF_CRON = tpfPlanif.TPF_CRON,
                                    TPF_DATE_DEBUT = tpfPlanif.TPF_DATE_DEBUT,
                                    TPF_DATE_FIN = tpfPlanif.TPF_DATE_FIN,
                                    TPF_DEMANDE_ORIGINEID = tpfPlanif.TPF_DEMANDE_ORIGINEID,
                                    TPF_TIMEZONE_INFOID = tpfPlanif.TPF_TIMEZONE_INFOID,
                                    TC_CATEGORIEID = tcCategorie.TC_CATEGORIEID
                                })
            /* ici, pas besoin d'ajouter de prédicats, sinon la suite sera faussée */
            .ToList();

        // 1 - Vue DemandesEtendues (non récurrentes => IS_RECURRENT == false)
        var listDemandeEtenduesExecutionUniques = _dbContext.VDE_DEMANDES_ETENDUES.Where(e =>
                e.TPF_PLANIF_ORIGINEID == null
                && (e.TD_DATE_EXECUTION_SOUHAITEE ?? dtUtcStart.DateTime) >= dtUtcStart.DateTime
                && (e.TD_DATE_EXECUTION_SOUHAITEE ?? dtUtcEnd.DateTime) <= dtUtcEnd.DateTime
            )
            .ToList();

        foreach (var item in listDemandeEtenduesExecutionUniques)
        {
            yield return new()
            {
                IS_RECURRENT = false,
                TD_DEMANDEID = 0, //item.TD_DEMANDEID,
                TE_ETATID = item.TE_ETATID,
                TS_SCENARIOID = item.TS_SCENARIOID,
                TS_NOM_SCENARIO = item.TS_NOM_SCENARIO, // take
                TE_NOM_ETAT_VERSION = item.TE_NOM_ETAT_VERSION, // take
                TRU_DEMANDEURID = item.TRU_DEMANDEURID,
                TRU_DECLARANTID = item.TRU_DEMANDEURID,
                TRU_DEMANDEUR = item.DEMANDEUR, // take
                TRU_USER_RESP_FONC = item.REFERENT,
                TRU_USER_DECLARANT_PLANIF = item.DEMANDEUR,
                TPF_PLANIFID = -1,
                TPF_CRON = null,
                TPF_TIMEZONE_INFOID = null,
                TPF_DATE_DEBUT = default,
                TPF_DATE_FIN = default,
                TPF_DEMANDE_ORIGINEID = -1,
                TD_DATE_PRISE_EN_CHARGE /*THEORIQUE*/ = item.TD_DATE_EXECUTION_SOUHAITEE,
                TD_DUREE_PRODUCTION_REEL = item.TD_DUREE_PRODUCTION_REEL,
                CODE_STATUT_DEMANDE = null, //item.CODE_STATUT_DEMANDE
                TC_CATEGORIEID = item.TC_CATEGORIEID
            };
        }

        // 2 - Données issue du décodage du CRON (récurrentes => IS_RECURRENT == true)
        foreach (var item in listModelDemande)
        {
            // Timezone offset of the planif (for time "dtUtcStart").
            var startDateOffset = TZConvert.GetTimeZoneInfo(item.TPF_TIMEZONE_INFOID).GetUtcOffset(dtUtcStart);

            // Si la date de départ dtUtcStart < X jours de la date de début des CRON, alors on set dtUtcStart comme TPF_DATE_DEBUT, sinon on aura des trous dans la raquette
            if (dtUtcStart.DateTime < DateExtensions.ConvertToUTC(item.TPF_DATE_DEBUT, item.TPF_TIMEZONE_INFOID))
            {
                startDateOffset = TZConvert.GetTimeZoneInfo(item.TPF_TIMEZONE_INFOID).GetUtcOffset(item.TPF_DATE_DEBUT);
                dtUtcStart = new DateTimeOffset(item.TPF_DATE_DEBUT, startDateOffset);
            }

            // Start date is "dtUtcStart" converted from server timezone to planif timezone.
            var debut = dtUtcStart.ToOffset(startDateOffset);

            // If end date exists in db, end date is converted from undefined to planif timezone.
            DateTimeOffset fin;

            if (item.TPF_DATE_FIN.HasValue)
            {
                // Timezone offset of the planif (for time TPF_DATE_FIN).
                var endDateOffset = TZConvert.GetTimeZoneInfo(item.TPF_TIMEZONE_INFOID).GetUtcOffset(item.TPF_DATE_FIN.Value);
                fin = new DateTimeOffset(item.TPF_DATE_FIN.Value, endDateOffset).Truncate(TimeSpan.FromMinutes(1));
            }
            else
            {
                fin = dtUtcEnd;
            }

            // limiter a maxNextOccurencesPerDemande propositions de date de planif par ligne de planification
            var prochainesOccurences = debut <= fin
                ? CronExpression.Parse(item.TPF_CRON)
                    .GetOccurrences(debut, fin, TZConvert.GetTimeZoneInfo(item.TPF_TIMEZONE_INFOID))
                    //?.Take(maxNextOccurencesPerDemande)
                    .Where(e => e != default)
                    .ToList()
                : new List<DateTimeOffset>();

            foreach (var dateProchaineOccurenceUTC in prochainesOccurences
                         .Select(occurence => DateExtensions.ConvertToUTC(occurence.DateTime, item.TPF_TIMEZONE_INFOID))
                         .Where(dateProchaineOccurenceUTC => dateProchaineOccurenceUTC >= dtUtcStart && dateProchaineOccurenceUTC <= dtUtcEnd))
            {
                // on retourne chaque demande au fil de l'eau
                yield return new ModeleDemandeCalendar
                {
                    IS_RECURRENT = true,
                    TD_DEMANDEID = 0, //item.TD_DEMANDEID,
                    TE_ETATID = item.TE_ETATID,
                    TS_SCENARIOID = item.TS_SCENARIOID,
                    TS_NOM_SCENARIO = item.TS_NOM_SCENARIO, // take
                    TE_NOM_ETAT_VERSION = item.TE_NOM_ETAT_VERSION, // take
                    TRU_DEMANDEURID = item.TRU_DEMANDEURID,
                    TRU_DECLARANTID = item.TRU_DECLARANTID,
                    TRU_DEMANDEUR = item.TRU_DEMANDEUR, // take
                    TRU_USER_RESP_FONC = item.TRU_USER_RESP_FONC,
                    TRU_USER_DECLARANT_PLANIF = item.TRU_USER_DECLARANT_PLANIF,
                    TPF_PLANIFID = item.TPF_PLANIFID,
                    TPF_CRON = item.TPF_CRON,
                    TPF_TIMEZONE_INFOID = item.TPF_TIMEZONE_INFOID,
                    TPF_DATE_DEBUT = item.TPF_DATE_DEBUT,
                    TPF_DATE_FIN = item.TPF_DATE_FIN,
                    TPF_DEMANDE_ORIGINEID = item.TPF_DEMANDE_ORIGINEID,
                    TD_DATE_PRISE_EN_CHARGE /*THEORIQUE*/ = dateProchaineOccurenceUTC,
                    TD_DUREE_PRODUCTION_REEL = null,
                    CODE_STATUT_DEMANDE = null, //item.CODE_STATUT_DEMANDE
                    TC_CATEGORIEID = item.TC_CATEGORIEID
                };
            }
        }
    }

    #endregion

    #region PERIODIC TREATMENT (CRON)

    // date dernière exécution de l'analyse des Commandes
    private DateTime _cmdLastChk = DateTime.UtcNow.AddDays(-1);

    /// <summary>
    /// Commandes : Analyse des commandes inexploitées depuis une certaine durée
    /// </summary>
    public async ValueTask CmdCheckLifePhaseEveryDay()
    {
        /* Opération de controle des Commandes présentes dans une phase donnée depuis une certaine durée */
        /* L'analyse des commandes à lieu au mieux une fois par jour ou plus si redémarrage du service */
        /* On expoite les commandes non archivées et dont la phase actuelle est dans cet état depuis plus de x jours, x dépendant de chaque phase */

        var now = DateTime.UtcNow;

        if ((now - _cmdLastChk).Days > 0)
        {
            int nbDestroy = 0;
            int nbArchived = 0;

            /*--------------------- GESTION DES COMMANDES BROUILLON -------------------------*/
            /* Ces commandes vont être physiquement détruites ainsi que les fichiers associés */

            /* les commandes brouillon ne sont pas tracées dans la table de suivi */
            var ordersDraftToDestroy = (
                    from tcmdCommande in _dbContext.TCMD_COMMANDES
                    join tcmdPhPhase in _dbContext.TCMD_PH_PHASES on tcmdCommande.TCMD_PH_PHASEID equals tcmdPhPhase.TCMD_PH_PHASEID
                    where tcmdPhPhase.TCMD_PH_CODE == Phases.Draft
                    select new
                    {
                        tcmdCommande.TCMD_COMMANDEID,
                        tcmdCommande.TCMD_PH_PHASEID,
                        maxdate = tcmdCommande.TCMD_DATE_MODIF,
                        tcmdPhPhase.TCMD_PH_DELAI_RECYCLAGE
                    }
                )
                .AsEnumerable()
                .Where(a => (now - a.maxdate).Days > a.TCMD_PH_DELAI_RECYCLAGE);
            foreach (var item in ordersDraftToDestroy)
            {
                nbDestroy += 1;
                try
                {
                    await _ordersServices.DeleteOrderAsync(item.TCMD_COMMANDEID, null);
                }

                catch (Exception ex)
                {
                    _logger.Error(ex, "< -------- Failed: CmdCheckLifePhase Destroy Draft Orders. {Message}", ex.InnerException?.Message ?? ex.Message);
                }
            }

            /*--------------------- GESTION DES COMMANDES Autre que BROUILLON -------------------------*/
            /* Ces commandes vont être mise en phase "ARCHIVEE" */
            /* attention a bien recupérer la phase ayant la date la plus récente ( a cause des drag & drop on peut trouver pour une cmd une meme phase plusieurs fois a plusieurs date */

            var ordersToArchive = (from tcmdCommande in _dbContext.TCMD_COMMANDES
                                   join tcmdPhPhases in _dbContext.TCMD_PH_PHASES on tcmdCommande.TCMD_PH_PHASEID equals tcmdPhPhases.TCMD_PH_PHASEID
                                   where tcmdPhPhases.TCMD_PH_CODE != Phases.Archived && tcmdPhPhases.TCMD_PH_CODE != Phases.Draft
                                   join tcmdSpSuiviPhase in _dbContext.TCMD_SP_SUIVI_PHASES on new
                                   { aa = tcmdCommande.TCMD_COMMANDEID, bb = tcmdCommande.TCMD_PH_PHASEID } equals new
                                   { aa = tcmdSpSuiviPhase.TCMD_COMMANDEID, bb = tcmdSpSuiviPhase.TCMD_PH_PHASE_APRESID }
                                   group tcmdSpSuiviPhase by new
                                   {
                                       tcmdSpSuiviPhase.TCMD_COMMANDEID,
                                       tcmdSpSuiviPhase.TCMD_PH_PHASE_APRESID,
                                       tcmdPhPhases.TCMD_PH_CODE,
                                       tcmdPhPhases.TCMD_PH_DELAI_RECYCLAGE
                                   }
                                   into g
                                   select new
                                   {
                                       g.Key.TCMD_COMMANDEID,
                                       TCMD_PH_PHASEID = g.Key.TCMD_PH_PHASE_APRESID,
                                       g.Key.TCMD_PH_CODE,
                                       maxdate = g.Max(tcmdSpSuiviPhase => tcmdSpSuiviPhase.TCMD_SP_DATE_MODIF) ?? now,
                                       g.Key.TCMD_PH_DELAI_RECYCLAGE
                                   }
                ).AsEnumerable()
                .Where(a => (now - a.maxdate).Days > a.TCMD_PH_DELAI_RECYCLAGE);

            foreach (var item in ordersToArchive)
            {
                nbArchived++;

                try
                {
                    var reason = (from tcmdRpRaisonPhase in _dbContext.TCMD_RP_RAISON_PHASES
                                  join tcmdPhPhase in _dbContext.TCMD_PH_PHASES on tcmdRpRaisonPhase.TCMD_PH_PHASEID equals tcmdPhPhase.TCMD_PH_PHASEID
                                  where tcmdPhPhase.TCMD_PH_CODE == Phases.Archived
                                  select tcmdRpRaisonPhase).FirstOrDefault();

                    int reasonId = reason!.TCMD_RP_RAISON_PHASEID;

                    // Change phase request.
                    ChangeOrderPhaseArguments args = new()
                    {
                        PhaseCode = Phases.Archived,
                        ReasonId = reasonId,
                        Comment = ""
                    };

                    /* l'utilisateur de la modif n existe pas vu que c est traitement auto */
                    await _ordersServices.ChangeOrderPhaseAsync(item.TCMD_COMMANDEID, null, args);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "< -------- Failed: CmdCheckLifePhase Update Orders to Archive. {Message}", ex.InnerException?.Message ?? ex.Message);
                }
            }

            _cmdLastChk = DateTime.UtcNow;

            if (nbArchived > 0 || nbDestroy > 0)
            {
                _logger.Information($"< -------- Commandes: Destroyed: {nbDestroy}, Archived: {nbArchived}");

                await _trackedEntitiesServices.AddTrackedEntitiesAsync(null, new[] { typeof(TCMD_COMMANDES).FullName },
                    Litterals.Update, _sessionId, "CmdCheckLifePhase");
            }
        }
    }

    /// <summary>
    /// Logs : Suppression des logs de type Information depuis une certaine durée (paramétrable depuis app settings).<br/>
    /// When parameter is missing from app settings, then the default value is 30 (days).
    /// </summary>
    public async ValueTask LogsCheckForCleanupInformation()
    {
        // Default to 30 days when parameters were not found or badly set
        var fromDays = _configuration.GetValue<int>("CronJobManager:LogsCheckForCleanupInformation");
        fromDays = fromDays < 1 ? 30 : fromDays;

        await _dbContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbContextLogs.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbContextLogs.Database.BeginTransactionAsync();

            try
            {
                var sinceDate = DateTime.UtcNow.AddDays(-fromDays);
                sinceDate = new DateTime(sinceDate.Year, sinceDate.Month, sinceDate.Day).AddSeconds(1);

                var logs = _dbContextLogs
                    .Set<TM_LOG_Logs>();

                var datasIdsToRemove = await logs
                    .AsNoTracking()
                    .Where(e => e.Log_CreationDate < sinceDate && e.Log_Type == "Information")
                    .ToListAsync();

                if (datasIdsToRemove.Count > 0)
                {
                    logs.RemoveRange(datasIdsToRemove);
                    await _dbContextLogs.SaveChangesAsync();
                    await dbCTrans.CommitAsync();

                    _logger.Information($"> LogsCheckForCleanupInformation: {datasIdsToRemove.Count} rows removed from db-Logs");
                }
            }
            catch (Exception ex)
            {
                dbCTrans.Rollback();
                _logger.Error(ex, $"< LogsCheckForCleanupInformation: failure");
            }
        });
    }
    #endregion

    #region MAINTENANCE
    /// <summary>
    /// Client can stop current running process
    /// </summary>
    public async ValueTask<bool> StopCmdAsync(string connectionId)
    {
        try
        {
            var cpu = _cpuManager.GetCpuInstance(connectionId);

            if (!string.IsNullOrEmpty(cpu.Id))
            {
                var user = _commonService.GetUserIdAndName();

                _cpuManager.UpdateCpu(cpu);

                await _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnStopCmdAsync", new object[] { cpu, user.userName });

                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "< -------- Failed: StopCmdAsync. {Message}", ex.InnerException?.Message ?? ex.Message);
        }

        return false;
    }
    #endregion    
}