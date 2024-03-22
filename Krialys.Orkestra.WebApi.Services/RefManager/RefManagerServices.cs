using Hangfire;
using Krialys.Common;
using Krialys.Common.Enums;
using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.RefManager;
using Krialys.Data.EF.Univers;
using Krialys.Data.Model;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.System;
using Krialys.Orkestra.WebApi.Services.System.HUB;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Syncfusion.Blazor.Grids;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using static Krialys.Orkestra.Common.Shared.ETQ;

namespace Krialys.Orkestra.WebApi.Services.RefManager;

public class ReferentialTable : ReferentialTableInfo
{
    public ICollection<IDictionary<string, object>> Datas { get; set; }
}

public class ReferentialTableInfo
{
    public IEnumerable<ReferentialTableDataModel> DataModel { get; set; }

    public string LabelCode { get; set; }
}

public class ReferentialTableDataModel
{
    public string ColumnName { get; set; }
    public int ColumnId { get; set; }
    public string DataType { get; set; }
    public ManagedTypes ManagedType { get; set; }

    public bool IsNullable { get; set; }
    public bool IsColumnKey { get; set; }

    public ColumnType ColumnType { get; set; }

    public EditType EditType { get; set; }
}

public interface IRefManagerServices : IScopedService
{
    ValueTask<GdbRequestToHandle> GetGdbRequestTohandle(GdbRequestAction action);
    ValueTask<bool> GdbRequestHandled(GdbRequestHandled requestHandled);

    ValueTask FireAndForgetRefreshData();
    ValueTask<bool> RefreshReferentialData();

    ValueTask<bool> UpdateReferentialData();

    ValueTask<ReferentialTableInfo> GetReferentialTableInfo(int referentialTableId);
    ValueTask<byte[]> GetReferentialTableData(int referentialTableId);

    ValueTask<bool> UpdateReferentialTableData(int referentialTableId, byte[] jsonData, bool approved);
    ValueTask<bool> UpdateReferential(ReferentialTable record);
    ValueTask<int> CloneLabelObjectCodeEntriesAsync(int refTableId, string labelCodeObject);

    ValueTask<bool> ApproveReferential(TM_RFS_ReferentialSettings record);

    ValueTask AddOrUpdateHistory(TM_RFH_ReferentialHistorical newRecord);
}

public class RefManagerServices : IRefManagerServices
{
    private readonly Krialys.Data.EF.RefManager.KrialysDbContext _dbRefContext;
    private readonly Krialys.Data.EF.Univers.KrialysDbContext _dbUniversContext;

    private readonly ITrackedEntitiesServices _iTrackedEntitiesServices;
    private readonly ICpuServices _iCpuServices;
    private readonly IBackgroundJobClient _iBackgroundJobClient;
    private readonly ICommonServices _iCommonServices;
    private readonly IRefManagerRfsTableHelperServices _rfsTableHelper;
    private readonly Serilog.ILogger _iLogger;
    private readonly ICpuConnectionManager _cpuManager;
    private readonly IHubContext<SPUHub> _hubContext;

    private static string[] ToStringArray(string @this)
        => string.IsNullOrEmpty(@this) ? Array.Empty<string>() : @this.Split(',').Select(p => p.Trim()).ToArray();

    public RefManagerServices(
        Krialys.Data.EF.RefManager.KrialysDbContext dbRefContext,
        Krialys.Data.EF.Univers.KrialysDbContext dbUniversContext,
        ITrackedEntitiesServices iTrackedEntitiesServices,
        ICpuServices iCpuServices,
        Serilog.ILogger iLogger,
        IBackgroundJobClient iBackgroundJobClient,
        ICommonServices iCommonServices,
        IRefManagerRfsTableHelperServices rfsTableHelper,
        ICpuConnectionManager cpuManager,
        IHubContext<SPUHub> hubContext)
    {
        _dbRefContext = dbRefContext ?? throw new ArgumentNullException(nameof(dbRefContext));
        _dbUniversContext = dbUniversContext ?? throw new ArgumentNullException(nameof(dbUniversContext));
        _iCpuServices = iCpuServices ?? throw new ArgumentNullException(nameof(iCpuServices));
        _iBackgroundJobClient = iBackgroundJobClient ?? throw new ArgumentNullException(nameof(iBackgroundJobClient));
        _iTrackedEntitiesServices = iTrackedEntitiesServices ?? throw new ArgumentNullException(nameof(iTrackedEntitiesServices));
        _iCommonServices = iCommonServices;
        _rfsTableHelper = rfsTableHelper;
        _cpuManager = cpuManager;
        _hubContext = hubContext;
        _iLogger = iLogger ?? throw new ArgumentNullException(nameof(iLogger));
    }

    /// <summary>
    /// This function is invoked when the JAR is about to recap the handled informations
    /// </summary>
    /// <param name="requestHandled"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async ValueTask<bool> GdbRequestHandled(GdbRequestHandled requestHandled)
    {
        if (requestHandled == null) throw new ArgumentNullException(nameof(requestHandled));

        var result = true;
        var utcNow = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1)); // don't store milliseconds
        TM_RFH_ReferentialHistorical historical = null;

        _iLogger.Information($"GdbRequestHandled [Start]");

        if (requestHandled.ReferentielInfos.Any())
            await using (var transaction = await _dbRefContext.Database.BeginTransactionAsync())
                try
                {
                    _iLogger.Information($"GdbRequestHandled [Start] [Referentials To Handle : {requestHandled.ReferentielInfos.Count()}]");

                    var refInfo = requestHandled.ReferentielInfos.FirstOrDefault();
                    //requestHandled.ReferentielInfos.ToList().ForEach(refInfo =>
                    {
                        var settings = _dbRefContext.TM_RFS_ReferentialSettings.FirstOrDefault(x => x.Rfs_TableName.ToLower() == refInfo.TableName.ToLower());
                        if (settings != null)
                        {
                            var logMessage = $"GdbRequestHandled [Mode : {requestHandled.RequestAction.ToString()}] [Table : {settings.Rfs_TableName}]";

                            if (!string.IsNullOrEmpty(refInfo.ErrorMessage))
                                logMessage += $"\r\n[Error Message : {refInfo.ErrorMessage}]\r\n";

                            switch (requestHandled.RequestAction)
                            {
                                //Check data validity at first
                                case GdbRequestAction.Read:
                                    if (string.IsNullOrEmpty(refInfo.TableData) || string.IsNullOrEmpty(refInfo.TableMeta))
                                        throw new Exception($"{logMessage} Missing TableData or TableMeta");

                                    var foundRecord = _dbRefContext.TX_RFX_ReferentialSettingsData.FirstOrDefault(x => x.Rfs_id == settings.Rfs_id);
                                    if (foundRecord == null)
                                    {
                                        _dbRefContext.TX_RFX_ReferentialSettingsData.Add(new TX_RFX_ReferentialSettingsData()
                                        {
                                            Rfs_id = settings.Rfs_id,
                                            Rfx_TableData = Encoding.UTF8.GetString(Convert.FromBase64String(refInfo.TableData)),
                                            Rfx_TableMetadata = Encoding.UTF8.GetString(Convert.FromBase64String(refInfo.TableMeta)),
                                            Rfx_TableDataUpdateDate = utcNow,
                                            Rfx_TableDataUpdatedBy = "admin"
                                        });
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(foundRecord.Rfx_TableData))
                                        {
                                            foundRecord.Rfx_TableDataBackup = settings.Rfs_IsBackupNeeded ? foundRecord.Rfx_TableData : null;
                                            foundRecord.Rfx_TableMetadataBackup = settings.Rfs_IsBackupNeeded ? foundRecord.Rfx_TableMetadata : null;
                                            foundRecord.Rfx_TableDataBackupDate = settings.Rfs_IsBackupNeeded ? utcNow : null;
                                            foundRecord.Rfx_TableDataBackupUpdatedBy = settings.Rfs_IsBackupNeeded ? "admin" : null;
                                        }

                                        foundRecord.Rfx_TableData = Encoding.UTF8.GetString(Convert.FromBase64String(refInfo.TableData));
                                        foundRecord.Rfx_TableMetadata = Encoding.UTF8.GetString(Convert.FromBase64String(refInfo.TableMeta));
                                        foundRecord.Rfx_TableDataUpdateDate = utcNow;
                                        foundRecord.Rfx_TableDataUpdatedBy = "admin";

                                        _dbRefContext.TX_RFX_ReferentialSettingsData.Update(foundRecord);
                                    }

                                    settings.Rfs_TableDataNeedToBeRefreshed = false;
                                    settings.Rfs_LastRefreshDate = utcNow;
                                    break;

                                case GdbRequestAction.Write:
                                    settings.Rfs_LabelDataClonedInProgressListJson = null;
                                    settings.Rfs_ScenarioId = null;
                                    settings.Rfs_SendDateToGdb = null;
                                    settings.Rfs_TableDataApprovalDate = null;
                                    settings.Rfs_TableDataApproved = false; // TODO: what to do in case of error?
                                    settings.Rfs_TableDataApprovedBy = null;
                                    settings.Rfs_UpdateDate = utcNow;
                                    settings.Rfs_IsUpdateSentToGdb = false;
                                    settings.Rfs_TableDataToApprouve = false;
                                    settings.Rfs_TableDataNeedToBeRefreshed = false;
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            // 2/2 TODO : retour suite à demandeId + etiquette
                            // Maj histo avec statut à 'terminé (DR ou ER)' + maj table demandes statut 'DR' si pas d'erreur, sinon 'ER' + error message
                            // Mise à jour des datas $XXX vers $YYY en cas de succès
                            historical = _dbRefContext.TM_RFH_ReferentialHistorical.FirstOrDefault(x => x.Rfh_TreatmentId == requestHandled.RequestId);
                            if (historical != null)
                            {
                                historical.Rfh_ErrorMessage = refInfo.ErrorMessage;
                                historical.Rfh_UpdateDate = utcNow;
                                historical.Rfh_StatusCode = string.IsNullOrEmpty(refInfo.ErrorMessage) && historical.Rfh_RequestId != null
                                    ? StatusRefManagerHistoricalLiterals.Succeeded
                                    : StatusRefManagerHistoricalLiterals.Failed;
                                historical.Rfh_ProcessStatus = string.IsNullOrEmpty(refInfo.ErrorMessage) && historical.Rfh_RequestId != null
                                    ? StatusLiteral.RealizedRequest
                                    : StatusLiteral.InError;
                            }

                            _dbRefContext.TM_RFS_ReferentialSettings.Update(settings);
                        }

                        await _dbRefContext.SaveChangesAsync();
                        await transaction.CommitAsync();

                        // Update historical
                        if (historical != null)
                        {
                            await AddOrUpdateHistory(historical);

                            // Only for labels: update matrix old label with new label
                            if (requestHandled.RequestAction == GdbRequestAction.Write && string.IsNullOrEmpty(historical.Rfh_ErrorMessage))
                            {
                                await _rfsTableHelper.UpdateNewLabelMatrix(settings.Rfs_id,
                                   settings.Rfs_LabelCodeFieldName, historical.Rfh_LabelCode, historical.Rfh_LabelCodeGenerated);
                            }

                            // Update statuses/error message TD_DEMANDES
                            var demande = _dbUniversContext.TD_DEMANDES.FirstOrDefault(x => x.TD_DEMANDEID == historical.Rfh_RequestId);
                            if (demande != null)
                            {
                                demande.TRST_STATUTID = historical.Rfh_ProcessStatus;
                                demande.TD_INFO_RETOUR_TRAITEMENT = historical.Rfh_ErrorMessage;
                                demande.TD_DUREE_PRODUCTION_REEL = string.IsNullOrEmpty(historical.Rfh_ErrorMessage) ? 0 : 5;
                                demande.TD_DATE_LIVRAISON = utcNow;
                                demande.TD_DATE_PRISE_EN_CHARGE = utcNow;
                                await _dbUniversContext.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = false;

                    _iLogger.Error(ex, "GdbRequestHandled Error: {Message}", ex.Message);

                    await transaction.RollbackAsync();
                }

        _iLogger.Information($"GdbRequestHandled [End]");

        return await ValueTask.FromResult(result);
    }

    /// <summary>
    /// This function is invoked first by the JAR file to handle a GdbRequestAction action.<br/>
    /// Whatever the action on the tables, only one table will be managed.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public async ValueTask<GdbRequestToHandle> GetGdbRequestTohandle(GdbRequestAction action)
    {
        var historicalTreatmentId = Guid.NewGuid().ToString("N");
        var utcDateNow = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1));

        IEnumerable<ReferentielInfo> requestList =
                    from settings in _dbRefContext.TM_RFS_ReferentialSettings
                    join cnx in _dbRefContext.TR_CNX_Connections on settings.Cnx_Id equals cnx.Cnx_Id
                    join data in _dbRefContext.TX_RFX_ReferentialSettingsData on settings.Rfs_id equals data.Rfs_id
                    where ((action == GdbRequestAction.Read && settings.Rfs_TableDataNeedToBeRefreshed)
                        || (action == GdbRequestAction.Write && settings.Rfs_TableDataApproved && !settings.Rfs_IsUpdateSentToGdb))
                    select new ReferentielInfo()
                    {
                        DatabaseSettings = new DatabaseSettings()
                        {
                            DatabaseType = (DatabaseType)cnx.Cnx_DatabaseType,
                            ServerName = cnx.Cnx_ServerName,
                            Port = cnx.Cnx_Port,
                            DatabaseName = cnx.Cnx_DatabaseName,
                            Login = cnx.Cnx_Login != null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(cnx.Cnx_Login)) : "",
                            Password = cnx.Cnx_Password != null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(cnx.Cnx_Password)) : "",
                        },
                        Typology = settings.Rfs_TableTypology,

                        #region Not serialized but - required for the workflow -
                        TableId = settings.Rfs_id,
                        TableFunctionalName = settings.Rfs_TableFunctionalName,
                        ScenarioId = settings.Rfs_ScenarioId,
                        LabelDataClonedJsonList = settings.Rfs_LabelDataClonedInProgressListJson,
                        ParamLabelObjectCode = settings.Rfs_ParamLabelObjectCode,
                        LabelCodeFieldName = settings.Rfs_LabelCodeFieldName,
                        #endregion

                        TableName = settings.Rfs_TableName,
                        Schema = settings.Rfs_TableSchema,
                        SelectQuery = settings.Rfs_TableQuerySelect,
                        Update = new UpdateInfo
                        {
                            Columns = ToStringArray(settings.Rfs_TableQueryUpdateColumns),
                            PrimaryKeys = ToStringArray(settings.Rfs_TableQueryUpdateKeys),
                        },
                        TableData = (action == GdbRequestAction.Read ? "" : (!string.IsNullOrEmpty(data.Rfx_TableData) ? Convert.ToBase64String(Encoding.UTF8.GetBytes(data.Rfx_TableData)) : "")),
                        TableMeta = (action == GdbRequestAction.Read ? "" : (!string.IsNullOrEmpty(data.Rfx_TableMetadata) ? Convert.ToBase64String(Encoding.UTF8.GetBytes(data.Rfx_TableMetadata)) : "")),
                        MaxRowsToRead = settings.Rfs_TableDataMaxRowsExpectedToReceive ?? 0,
                        SqlCriteria = settings.Rfs_TableQueryCriteria,
                    };

        // Code added to avoid JAVA pure crash when nothing to do!
        if (!requestList.Any())
        {
            // WORKAROUND_TO_AVOID_JAVA_CRASH
            requestList = new[]
            {
                (from settings in _dbRefContext.TM_RFS_ReferentialSettings
                join cnx in _dbRefContext.TR_CNX_Connections on settings.Cnx_Id equals cnx.Cnx_Id
                join data in _dbRefContext.TX_RFX_ReferentialSettingsData on settings.Rfs_id equals data.Rfs_id
                select new ReferentielInfo()
                {
                    DatabaseSettings = new DatabaseSettings()
                    {
                        DatabaseType = (DatabaseType)cnx.Cnx_DatabaseType,
                        ServerName = cnx.Cnx_ServerName,
                        Port = cnx.Cnx_Port,
                        DatabaseName = cnx.Cnx_DatabaseName,
                        Login = cnx.Cnx_Login != null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(cnx.Cnx_Login)) : "",
                        Password = cnx.Cnx_Password != null ? Convert.ToBase64String(Encoding.UTF8.GetBytes(cnx.Cnx_Password)) : "",
                    },
                    Typology = RefManagerTypologyType.None,
                    TableId = 1, // FK
                    TableName = "NO_TABLE",
                    Schema = settings.Rfs_TableSchema,
                    SelectQuery = $"SELECT 1 FROM DUAL", // oracle
                    Update = new UpdateInfo
                    {
                        Columns = Array.Empty<string>(),
                        PrimaryKeys = Array.Empty<string>(),
                    },
                    SqlCriteria = "1=0",
                }).FirstOrDefault()
            };
        }

        // 1/2 Creation demandeId + etiquette : envoi
        // Only in WRITE mode => requestList.First() + join historical
        // Click UI => histo => $XXXX à traiter via RefHistorical : status_code 'pris en compte coté IHM et à traiter par java' + label_code '$XXXX' (cas etq) et null pour les autres
        // On récupère le tableName (id via un champ jsonIgnore à ajouter dans ReferentielInfo) des settings + label code => màj de la table histo avec un nouveau statut 'en cours de traitement java' + filter que les datas à envoyer de $xxx
        // Créer demande + etq avec les datas filtrées sur le $XXX + transfo de $XXX en $YYY 'new code etq' et envoyer à java
        // il manque un champ: rfh_label_ui_code et mettre le request_id des settings dans histo
        if (action == GdbRequestAction.Read)
        {

        }
        else if (action == GdbRequestAction.Write)
        {
            var referentielInfo = requestList.FirstOrDefault();

            var originalLabelCode = referentielInfo.Typology == RefManagerTypologyType.WithLabel && !string.IsNullOrEmpty(referentielInfo.LabelDataClonedJsonList)
                ? JsonSerializer.Deserialize<string[]>(referentielInfo.LabelDataClonedJsonList)[0]
                : null;

            // Create a new request
            var request = await AddRequest(referentielInfo, historicalTreatmentId, utcDateNow);

            // Create a new label (but what TODO in case of error here?)
            var label = referentielInfo.Typology == RefManagerTypologyType.WithLabel && !string.IsNullOrEmpty(referentielInfo.ParamLabelObjectCode)
                ? await AddLabel(request.TD_DEMANDEID, referentielInfo.ParamLabelObjectCode, historicalTreatmentId, simulation: false) // test
                : default;

            // Update table data
            if (!string.IsNullOrEmpty(label.newLabelCode) && !string.IsNullOrEmpty(referentielInfo.LabelCodeFieldName))
            {
                var data = await _rfsTableHelper.GetNewLabelMatrix(referentielInfo.TableId, referentielInfo.LabelCodeFieldName, originalLabelCode, label.newLabelCode);
                if (!string.IsNullOrEmpty(data))
                    referentielInfo.TableData = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
            }

            // Add new entry to the history table
            await AddOrUpdateHistory(new TM_RFH_ReferentialHistorical
            {
                Rfs_id = referentielInfo.TableId, // FK
                Rfh_TreatmentId = historicalTreatmentId,
                Rfh_ProcessStatus = string.IsNullOrEmpty(request.TRST_STATUTID) ? StatusLiteral.InProgress : request.TRST_STATUTID, // TODO => apply translation at runtime Trad.Keys["STATUS:xxx"]
                Rfh_StatusCode = StatusRefManagerHistoricalLiterals.Created, // Specific status for historical. TODO => define a list of all statuses here
                Rfh_RequestId = request.TD_DEMANDEID > 0 ? request.TD_DEMANDEID : null,
                Rfh_LabelCode = originalLabelCode,
                Rfh_LabelCodeGenerated = label.newLabelCode ?? label.errorMessage, // What to do here when there was an error because of a bad parameter ?
                Rfh_ProcessType = $"RefManager-{action}",
                Rfh_TableFunctionalName = referentielInfo.TableFunctionalName,
                Rfh_TableName = referentielInfo.TableName,
                Rfh_UpdateDate = utcDateNow,
            });

            // Swap values since referentielInfo was previously modified
            requestList = new[] { referentielInfo };
        }

        return new GdbRequestToHandle()
        {
            // This value (don't be confused with the requestId) is used/stored as a technical key into the historical table (treatmentId)
            RequestId = historicalTreatmentId,
            ReferentielInfos = requestList,
        };
    }

    /// <summary>
    /// Create a new Label code
    /// </summary>
    /// <param name="requestId">RequestId coming from the TD_DEMANDE previously created</param>
    /// <param name="labelObjectCode">Parameter coming from the settings table</param>
    /// <param name="historicalTreatmentId">UUID</param>
    /// <returns></returns>
    private async ValueTask<(string newLabelCode, string errorMessage)> AddLabel(int requestId, string labelObjectCode, string historicalTreatmentId, bool simulation)
    {
        if (requestId < 1 || string.IsNullOrEmpty(labelObjectCode) || string.IsNullOrEmpty(historicalTreatmentId))
            return (null, null);

        var newLabel = new CalculateEtqInput(
            labelObjectCode,
            default,
            default,
            default,
            requestId,
            historicalTreatmentId,
            simulation // true for test purpose
        );

        var etqOutput = await _iCpuServices.EtqCalculate(guid: historicalTreatmentId, codeEtq: labelObjectCode, demandeid: requestId,
            version: default, codePerimetre: default, valDynPerimetre: default, source: "RefManager-Write", pSimul: simulation);

        return etqOutput.Success
            ? (etqOutput.CodeEtq, null)
            : (null, etqOutput.Message);
    }

    /// <summary>
    /// Create a new Request (DemandeId).
    /// <br/>This function will be invoked from the JAR.
    /// </summary>
    /// <param name="referentielInfo">ReferentielInfo</param>
    /// <param name="historicalTreatmentId">ReferentielInfo's RequestId aka TreatmentId</param>
    /// <param name="utcDateNow">UTC dateTime</param>
    /// <returns>An instance of TD_DEMANDES entity.</returns>
    private async ValueTask<TD_DEMANDES> AddRequest(ReferentielInfo referentielInfo, string historicalTreatmentId, DateTime utcDateNow)
    {
        // TODO :: add check between nb ligne min and max vs these that are added/deleted before sending
        // TODO :: get label data cloned from settings => this is where the $xxxx label code is stored

        var newRequest = new TD_DEMANDES { TD_DEMANDEID = -1 };

        // Id du module d'une UTD aka scenarioId
        var scenarioModuleId = referentielInfo?.ScenarioId;
        if (!scenarioModuleId.HasValue)
        {
            _iLogger.Error("< -------- Failed: CreateNewRequest. {Message}", "ModuleId not found!");
            return newRequest;
        }

        // ID version d'un UTD (TE_ETATID)
        var etatId = _dbUniversContext.TS_SCENARIOS.AsNoTracking()
            .FirstOrDefault(x => x.TS_SCENARIOID == scenarioModuleId.Value && x.TRST_STATUTID == StatusLiteral.Available)!.TE_ETATID;
        if (etatId == 0)
        {
            _iLogger.Error("< -------- Failed: CreateNewRequest. {Message}", "EtatId not found!");
            return newRequest;
        }

        // ID user demandeur
        var demandeurId = _iCommonServices.GetUserIdAndName().userId;

        // Table name
        var gdbTableName = referentielInfo.TableName;

        await _dbUniversContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbUniversContext.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbUniversContext.Database.BeginTransactionAsync();

            try
            {
                var entry = await _dbUniversContext.AddAsync(new TD_DEMANDES
                {
                    TE_ETATID = etatId,
                    TD_DATE_DEMANDE = utcDateNow,
                    TD_DATE_EXECUTION_SOUHAITEE = utcDateNow,
                    TD_COMMENTAIRE_UTILISATEUR = $"Sending data to GDB table: {gdbTableName} ({referentielInfo.Typology})",
                    TRST_STATUTID = StatusLiteral.InProgress, // Statut en cours
                    TRU_DEMANDEURID = demandeurId,
                    TD_GUID = historicalTreatmentId,
                    TS_SCENARIOID = scenarioModuleId,
                });

                await _dbUniversContext.SaveChangesAsync();

                // Track pour aider refresh des interfaces abonnées (iso avec la page des productions des UTDs)
                await _iTrackedEntitiesServices.AddTrackedEntitiesAsync(null, new[] { typeof(VDE_DEMANDES_ETENDUES).FullName },
                    Litterals.Insert, demandeurId, "CreateDemande");

                await dbCTrans.CommitAsync();

                newRequest = entry.Entity;
            }
            catch (Exception ex)
            {
                _iLogger.Error(ex, "< -------- Failed: CreateNewRequest. {Message}", ex.InnerException?.Message ?? ex.Message);
                await dbCTrans.RollbackAsync();
            }
        });

        return newRequest;
    }

    public ValueTask<ReferentialTableInfo> GetReferentialTableInfo(int referentialTableId)
    {
        var result = new ReferentialTableInfo();

        var refSettingsData = _dbRefContext.TX_RFX_ReferentialSettingsData.FirstOrDefault(x => x.Rfs_id == referentialTableId);

        if (refSettingsData != null)
        {
            var referentialTableDataModels = (!string.IsNullOrEmpty(refSettingsData.Rfx_TableMetadata)
                ? JsonSerializer.Deserialize<IEnumerable<ReferentialTableDataModel>>(refSettingsData.Rfx_TableMetadata)
                : Enumerable.Empty<ReferentialTableDataModel>()).ToList();

            // Add GUID key that is not present in the model as of JAR version 1.0
            if (referentialTableDataModels.Any(e => e.ColumnName != JsonExtensions.Id))
            {
                referentialTableDataModels = new List<ReferentialTableDataModel> {
                    new ReferentialTableDataModel
                    {
                        ColumnId = 0,
                        ColumnName = JsonExtensions.Id,
                        ManagedType = ManagedTypes.String,
                        DataType = "VARCHAR2",
                        IsColumnKey = true,
                        IsNullable = false,
                    }
                }.Concat(referentialTableDataModels).ToList();
            }

            // DBMS types decoder to enrich existing metadata properties required from the UI
            foreach (var item in referentialTableDataModels)
            {
                switch (item.DataType.ToUpper())
                {
                    case DbTypes.VarChar2:
                    case DbTypes.NVarChar2:
                    case DbTypes.Char:
                    case DbTypes.Text:
                    case DbTypes.Guid:
                        item.ColumnType = ColumnType.String;
                        item.EditType = EditType.DefaultEdit;
                        item.ManagedType = ManagedTypes.String;
                        break;

                    case DbTypes.Float:
                    case DbTypes.Double:
                    case DbTypes.BinaryFloat:
                    case DbTypes.Number:
                        item.ColumnType = ColumnType.Decimal;
                        item.EditType = EditType.NumericEdit;
                        item.ManagedType = ManagedTypes.Decimal;
                        break;

                    case DbTypes.Long:
                    case DbTypes.BigInt:
                        //case "NUMBER":
                        item.ColumnType = ColumnType.Long;
                        item.EditType = EditType.NumericEdit;
                        item.ManagedType = ManagedTypes.Int64;
                        break;

                    case DbTypes.Int:
                    case DbTypes.Integer:
                        item.ColumnType = ColumnType.Integer;
                        item.EditType = EditType.NumericEdit;
                        item.ManagedType = ManagedTypes.Int32;
                        break;

                    case DbTypes.Bool:
                    case DbTypes.Boolean:
                        item.ColumnType = ColumnType.Boolean;
                        item.EditType = EditType.BooleanEdit;
                        item.ManagedType = ManagedTypes.Boolean;
                        break;

                    case DbTypes.Date:
                    case DbTypes.DateTime:
                    case DbTypes.DateTime2:
                        item.ColumnType = ColumnType.DateTime;
                        item.EditType = EditType.DateTimePickerEdit;
                        item.ManagedType = ManagedTypes.DateTime;
                        break;

                    default:
                        item.ColumnType = ColumnType.None;
                        item.EditType = EditType.DefaultEdit;
                        item.ManagedType = ManagedTypes.String;
                        break;
                }
            }

            result.DataModel = referentialTableDataModels;
        }

        return ValueTask.FromResult(result);
    }

    public ValueTask<byte[]> GetReferentialTableData(int referentialTableId)
    {
        return ValueTask.FromResult(
            ConvertToReferentialTableData(_dbRefContext.TX_RFX_ReferentialSettingsData
                .FirstOrDefault(x => x.Rfs_id == referentialTableId)
                ?.Rfx_TableData, referentialTableId, true)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private byte[] ConvertToReferentialTableData(string jsonTableData, int referentialTableId, bool compressData = false)
    {
        if (string.IsNullOrEmpty(jsonTableData))
            return Array.Empty<byte>();

        IList<IDictionary<string, object>> result = new List<IDictionary<string, object>>();

        foreach (var item in JArray.Parse(jsonTableData)
            .ToDynamicDictionary(GetReferentialTableInfo(referentialTableId)
            .Result
            .DataModel
            .ToDictionary(key => key.ColumnName, value => value.ManagedType)))
        {
            result.Add(item.GetDictionary());
        }

        return compressData
            ? GZipExtensions.Compress(JsonSerializer.SerializeToUtf8Bytes(result))
            : JsonSerializer.SerializeToUtf8Bytes(result);
    }

    public ValueTask<bool> UpdateReferential(ReferentialTable record)
    {
        var result = false;

        return ValueTask.FromResult(result);
    }

    /// <summary>
    /// Update a referential table data
    /// </summary>
    /// <param name="refTableId">referential table id</param>
    /// <param name="refTableData">Compressed byte array (json)</param>
    /// <param name="isDataApproved">Indicate if data are approved (true), else updated (false)</param>
    /// <returns></returns>
    public async ValueTask<bool> UpdateReferentialTableData(int refTableId, byte[] refTableData, bool isDataApproved)
    {
        var result = false;
        var user = _iCommonServices.GetUserIdAndName();
        var utcNow = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1));

        await using (var transaction = await _dbRefContext.Database.BeginTransactionAsync())
            try
            {
                var refSettings = _dbRefContext.TM_RFS_ReferentialSettings.FirstOrDefault(x => x.Rfs_id == refTableId);
                var refSettingsData = _dbRefContext.TX_RFX_ReferentialSettingsData.FirstOrDefault(x => x.Rfs_id == refTableId);

                if (refSettings != null && refSettingsData != null)
                {
                    refSettings.Rfs_TableDataApprovedBy = isDataApproved ? user.userName : null;
                    refSettings.Rfs_TableDataApprovalDate = isDataApproved ? utcNow : null;
                    refSettings.Rfs_TableDataToApprouve = !isDataApproved;
                    refSettings.Rfs_TableDataApproved = isDataApproved;
                    refSettings.Rfs_IsUpdateSentToGdb = !isDataApproved;
                    refSettingsData.Rfx_TableDataUpdatedBy = isDataApproved ? null : user.userName;
                    refSettingsData.Rfx_TableDataUpdateDate = isDataApproved ? null : utcNow;
                    refSettings.Rfs_UpdateBy = isDataApproved ? null : user.userName;
                    refSettings.Rfs_UpdateDate = isDataApproved ? null : utcNow;
                    refSettingsData.Rfx_TableDataBackupDate = isDataApproved ? utcNow : null;
                    refSettings.Rfs_TableDataNeedToBeRefreshed = isDataApproved;

                    // Backup data and metadata? not sure...
                    refSettingsData.Rfx_TableDataBackup = refSettingsData.Rfx_TableData;
                    refSettingsData.Rfx_TableMetadataBackup = refSettingsData.Rfx_TableMetadata;

                    // Update table
                    _dbRefContext.TM_RFS_ReferentialSettings.Update(refSettings);
                    _dbRefContext.TX_RFX_ReferentialSettingsData.Update(refSettingsData);
                }

                await _dbRefContext.SaveChangesAsync();
                await transaction.CommitAsync();
                result = true;
            }
            catch (Exception ex)
            {
                _iLogger.Error(ex, "UpdateReferentialTableData Error: {Message}", ex.Message);
                await transaction.RollbackAsync();
            }

        // Apply CRUD
        if (!isDataApproved && result)
        {
            if (refTableData != null && refTableData.Length > 7)
            {
                var dataToUpdate = JsonSerializer.Deserialize<IEnumerable<IDictionary<string, object>>>(
                    ConvertToReferentialTableData(JsonSerializer.Deserialize<string>(GZipExtensions.Decompress(refTableData[1..])),
                    refTableId));

                foreach (var keyValue in dataToUpdate)
                {
                    switch (refTableData[0])
                    {
                        case 0: // Add
                            result = await _rfsTableHelper.InsertEntryAsync(refTableId, keyValue[JsonExtensions.Id], JsonSerializer.Serialize(keyValue));
                            break;

                        case 1: // Update
                            result = await _rfsTableHelper.UpdateEntryAsync(refTableId, keyValue[JsonExtensions.Id], JsonSerializer.Serialize(keyValue));
                            //var updateWholeMatrix = await _rfsTableHelper.UpdateNewLabelMatrix(7, "CODE_REF", "$2022_TRF_LISTPE_01", "PODZOBY");
                            break;

                        case 2: // Delete
                            result = await _rfsTableHelper.DeleteEntryAsync(refTableId, keyValue[JsonExtensions.Id]);
                            //var test = await RefreshReferentialData();
                            //var test = await UpdateReferentialData();
                            break;
                    }

                    if (!result)
                        break;
                }
            }
        }

        return await ValueTask.FromResult(result);
    }

    /// <summary>
    /// Clone label object code entries.<br/>
    /// Check if labelCode exists and if $labelCode does not exist
    /// </summary>
    /// <param name="refTableId">Table reference Id</param>
    /// <param name="labelCodeObject">Value of the property to look for</param>
    /// <returns>True when entries have been succesfully cloned, false when the labelcode has already been cloned, else return null when Rfs_LabelCodeFieldName is null.</returns>
    public async ValueTask<int> CloneLabelObjectCodeEntriesAsync(int refTableId, string labelCodeObject)
        => await _rfsTableHelper.CloneLabelObjectCodeEntriesAsync(refTableId, labelCodeObject);

    public async ValueTask<bool> ApproveReferential(TM_RFS_ReferentialSettings record)
    {
        var result = false;

        record.Rfs_TableDataToApprouve = true;
        record.Rfs_TableDataApproved = false;
        record.Rfs_TableDataApprovalDate = DateTime.UtcNow;
        record.Rfs_TableDataApprovedBy = null;

        record.Rfs_IsUpdateSentToGdb = false;
        record.Rfs_SendDateToGdb = null;

        return await ValueTask.FromResult(result);
    }

    #region History Management

    /// <summary>
    /// Add a new row or update an existing history row
    /// </summary>
    /// <param name="newRecord"></param>
    /// <returns></returns>
    public async ValueTask AddOrUpdateHistory(TM_RFH_ReferentialHistorical newRecord)
    {
        await _dbRefContext.Database.CreateExecutionStrategy().Execute(async () =>
        {
            await _dbRefContext.Database.OpenConnectionAsync();
            await using var dbCTrans = await _dbRefContext.Database.BeginTransactionAsync();

            try
            {
                // Check if we already have an existing entry, thus means updating
                var existingValues = _dbRefContext.TM_RFH_ReferentialHistorical
                    .OrderBy(x => x.Rfh_id)
                    .Select(x => new { x.Rfs_id, x.Rfh_RequestId, x.Rfh_id, x.Rfh_UpdateBy })
                    .LastOrDefault(x => x.Rfs_id > 0
                        && x.Rfs_id == newRecord.Rfs_id
                        && (x.Rfh_RequestId == null || x.Rfh_RequestId == 0));

                if (existingValues != null)
                {
                    // Come from UI, this is the only way to know who has ordered
                    newRecord.Rfh_UpdateBy ??= existingValues.Rfh_UpdateBy;
                    newRecord.Rfh_id = existingValues.Rfh_id;
                }

                // It does exactly AddOrUpdate based on value of entity PrimaryKey(0 means Add, > 0 means Update)
                _dbRefContext.TM_RFH_ReferentialHistorical.Update(newRecord);
                await _dbRefContext.SaveChangesAsync();
                await dbCTrans.CommitAsync();
            }
            catch (Exception ex)
            {
                _iLogger.Error(ex, "AddOrUpdateHistory Error: {Message}", ex.Message);
                await dbCTrans.RollbackAsync();
            }
        });
    }

    #endregion

    #region Database Settings Management



    #endregion

    #region WorkerNode
    /// <summary>
    /// Refresh datas (read)
    /// </summary>
    /// <returns></returns>
    public ValueTask<bool> RefreshReferentialData()
        => ExecuteRefManagerDataAction(GdbRequestAction.Read);

    /// <summary>
    /// Update datas (write)
    /// </summary>
    /// <returns></returns>
    public ValueTask<bool> UpdateReferentialData()
        => ExecuteRefManagerDataAction(GdbRequestAction.Write);

    /// <summary>
    /// Refresh datas (read) from a croned Job
    /// </summary>
    /// <returns></returns>
    public ValueTask FireAndForgetRefreshData()
    {
        _iBackgroundJobClient.Enqueue(() => RefreshReferentialData());

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Run refManager.
    /// <br/>This is the entry point for running JAR from the WorkerNode.
    /// </summary>
    /// <param name="action">Read or Write mode for running the JAR file</param>
    /// <returns>True if JAR has started, false when error occured</returns>
    private async ValueTask<bool> ExecuteRefManagerDataAction(GdbRequestAction action)
    {
        try
        {
            // Look for a free node capable of running refManager
            var cpus = _cpuManager.GetOnlineConnections().Where(x => x.CanUseRefManagerFeature
                && x.Authorization.IsAuthorized
                && x.Resilience == false);

            var count = cpus?.Count();

            var cpu = cpus?.FirstOrDefault(x => x.IsRefManagerRunning == false);

            if (cpu != null)
            {
                if (cpu.IsRefManagerRunning == false)
                {
                    cpu.IsRefManagerRunning = true;
                    cpu = _cpuManager.UpdateCpu(cpu);
                    await _hubContext.Clients.Client(cpu.Id).SendCoreAsync("OnRefManagerRunJarAsync", new object[] { cpu, action });

                    return true;
                }

                if (count > 1)
                    _iLogger.Warning("< -------- GdbRequest: all the {0} nodes are currently busy", count);
                else if (count == 1)
                    _iLogger.Warning("< -------- GdbRequest: the node {0} is currently busy", cpu.Id);
            }
        }
        catch (Exception ex)
        {
            _iLogger.Error(ex, "< -------- GdbRequest: OnRefManagerRunJarAsync {0}", ex.InnerException?.Message ?? ex.Message);
        }

        return false;
    }
    #endregion
}