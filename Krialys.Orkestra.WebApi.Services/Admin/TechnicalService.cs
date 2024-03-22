using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Models.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace Krialys.Orkestra.WebApi.Services.Admin;

public interface ITechnicalService : IScopedService
{
    ValueTask<bool> DatabasePurgeAsync();
    ValueTask<bool> OrkestraNodeWorkerUpdateCheck(Version fileVersion, DateTime fileCreationDateUtc);
    ValueTask<UpdateDetails> OrkestraWebSiteUpdateCheck(Version version);
    ValueTask<FileStream> OrkestraNodeWorkerGetUpdate();
    ValueTask<string> OrkestraNodeWorkerFileNameGet();
}

public class TechnicalService : ILicence, ITechnicalService
{
    private readonly Krialys.Data.EF.Etq.KrialysDbContext _dbEtqContext;
    private readonly Krialys.Data.EF.FileStorage.KrialysDbContext _dbFileStorageContext;
    private readonly Krialys.Data.EF.Logs.KrialysDbContext _dbLogsContext;
    private readonly Krialys.Data.EF.Mso.KrialysDbContext _dbMsoContext;
    private readonly Krialys.Data.EF.RefManager.KrialysDbContext _dbRefManagerContext;
    private readonly Krialys.Data.EF.Univers.KrialysDbContext _dbUniversContext;
    private readonly ILogger<TechnicalService> _iLogger;

    public TechnicalService(
        Krialys.Data.EF.Etq.KrialysDbContext dbEtqContext,
        Krialys.Data.EF.FileStorage.KrialysDbContext dbFileStorageContext,
        Krialys.Data.EF.Logs.KrialysDbContext dbLogsContext,
        Krialys.Data.EF.Mso.KrialysDbContext dbMsoContext,
        Krialys.Data.EF.RefManager.KrialysDbContext dbRefManagerContext,
        Krialys.Data.EF.Univers.KrialysDbContext dbUniversContext,
        ILogger<TechnicalService> iLogger)
    {
        _dbEtqContext = dbEtqContext ?? throw new ArgumentNullException(nameof(dbEtqContext));
        _dbFileStorageContext = dbFileStorageContext ?? throw new ArgumentNullException(nameof(dbFileStorageContext));
        _dbLogsContext = dbLogsContext ?? throw new ArgumentNullException(nameof(dbLogsContext));
        _dbMsoContext = dbMsoContext ?? throw new ArgumentNullException(nameof(dbMsoContext));
        _dbRefManagerContext = dbRefManagerContext ?? throw new ArgumentNullException(nameof(dbRefManagerContext));
        _dbUniversContext = dbUniversContext ?? throw new ArgumentNullException(nameof(dbUniversContext));
        _iLogger = iLogger ?? throw new ArgumentNullException(nameof(iLogger));
    }

    public async ValueTask<bool> DatabasePurgeAsync()
    {
        var result = false;

        try
        {
            new List<string>()
                {
                    "TSR_SUIVI_RESSOURCES",
                    "TSEQ_SUIVI_EVENEMENT_ETQS",
                    "TETQR_ETQ_REGLES",
                    "TOBJR_OBJET_REGLES",
                    "ETQ_TM_AET_Authorization",
                    "TETQ_ETIQUETTES",
                    "TPRCP_PRC_PERIMETRES",
                    "TOBJE_OBJET_ETIQUETTES",
                    "TEQC_ETQ_CODIFS",

                    "TACT_ACTIONS",
                    "TDOM_DOMAINES",
                    "TOBF_OBJ_FORMATS",
                    "TOBN_OBJ_NATURES",
                    "TRGLI_REGLES_LIEES",
                    "TRGLRV_REGLES_VALEURS",
                    "TRGL_REGLES",
                    "TTDOM_TYPE_DOMAINES",
                    "TTE_TYPE_EVENEMENTS",
                    "TTR_TYPE_RESSOURCES"
                }
                //_dbEtqContext.GetAllTableNames()
                .ForEach(x =>
                {
                    switch (x.ToLower())
                    {
                        case "tact_actions":
                        case "tdom_domaines":
                        case "tobf_obj_formats":
                        case "tobn_obj_natures":
                        case "trgl_regles":
                        case "trgli_regles_liees":
                        case "trglrv_regles_valeurs":
                        case "ttdom_type_domaines":
                        case "tte_type_evenements":
                        case "ttr_type_ressources":
                            // Nothing is done....
                            break;

                        default:
                            _dbEtqContext.Database.ExecuteSqlRaw($"DELETE FROM `{x}`; delete from sqlite_sequence where name='{x}';");
                            break;
                    }
                });
            await _dbEtqContext.Database.ExecuteSqlRawAsync("VACUUM;");

            _dbFileStorageContext.GetAllTableNames()
                .ForEach(x =>
                {
                    switch (x.ToLower())
                    {
                        //case "ttl_logs":
                        case "tr_sct_stream_category_type":
                            // Nothing is done....
                            break;

                        default:
                            _dbFileStorageContext.Database.ExecuteSqlRaw($"DELETE FROM `{x}`; delete from sqlite_sequence where name='{x}';");
                            break;
                    }
                });
            await _dbFileStorageContext.Database.ExecuteSqlRawAsync("VACUUM;");

            _dbLogsContext.GetAllTableNames()
                .ForEach(x =>
                {
                    switch (x.ToLower())
                    {
                        //case "ttl_logs":
                        //case "tr_sct_stream_category_type":
                        //    // Nothing is done....
                        //    break;

                        default:
                            _dbLogsContext.Database.ExecuteSqlRaw($"DELETE FROM `{x}`; delete from sqlite_sequence where name='{x}';");
                            break;
                    }
                });
            await _dbLogsContext.Database.ExecuteSqlRawAsync("VACUUM;");

            _dbRefManagerContext.GetAllTableNames()
                .ForEach(x =>
                {
                    switch (x.ToLower())
                    {
                        case "tr_cnx_connections":
                            // Nothing is done....
                            break;

                        default:
                            _dbRefManagerContext.Database.ExecuteSqlRaw($"DELETE FROM `{x}`; delete from sqlite_sequence where name='{x}';");
                            break;
                    }
                });
            await _dbRefManagerContext.Database.ExecuteSqlRawAsync("VACUUM;");

            _dbMsoContext.GetAllTableNames()
                .ForEach(x =>
                {
                    switch (x.ToLower())
                    {
                        //case "ttl_logs":
                        case "trapl_applications":
                        case "trc_criticites":
                        case "trnf_natures_flux":
                        case "trnt_natures_traitements":
                        case "trr_resultats":
                        case "trtt_technos_traitements":
                            // Nothing is done....
                            break;

                        default:
                            _dbMsoContext.Database.ExecuteSqlRaw($"DELETE FROM `{x}`; delete from sqlite_sequence where name='{x}';");
                            break;
                    }
                });
            await _dbMsoContext.Database.ExecuteSqlRawAsync("VACUUM;");

            _dbLogsContext.GetAllTableNames()
                .ForEach(x =>
                {
                    switch (x.ToLower())
                    {
                        case "tm_log_logs":
                            // Nothing is done....
                            break;

                        default:
                            _dbLogsContext.Database.ExecuteSqlRaw($"DELETE FROM `{x}`; delete from sqlite_sequence where name='{x}';");
                            break;
                    }
                });
            await _dbLogsContext.Database.ExecuteSqlRawAsync("VACUUM;");

            new List<string>()
                {
                    "PARALLELEU",
                    "TM_LIC_Licence",
                    "TI_INFOS",
                    "TEMF_ETAT_MASTER_FERMES",
                    "TTAU_AUTHENTIFICATIONS",

                    "TH_HABILITATIONS",
                    "TSGA_SCENARIO_GPE_ASSOCIES",
                    "TSG_SCENARIO_GPES",

                    "TDQ_DEMANDE_QUALIFS",
                    "TDP_DEMANDE_PROCESS",
                    "TBD_BATCH_DEMANDES",
                    "TRD_RESSOURCE_DEMANDES",
                    "TPD_PREREQUIS_DEMANDES",
                    "TPF_PLANIFS",
                    "TD_DEMANDES",

                    "TEL_ETAT_LOGICIELS",

                    "TPS_PREREQUIS_SCENARIOS",
                    "TRS_RESSOURCE_SCENARIOS",
                    "TBS_BATCH_SCENARIOS",
                    "TEP_ETAT_PREREQUISS",
                    "TEB_ETAT_BATCHS",
                    "TER_ETAT_RESSOURCES",
                    "TRAPLAS_APPLICATIONS_AUTH_SCENARIOS",

                    "TUTE_USER_TEAMS",
                    "TTE_TEAMS",

                    "TCMD_CR_CMD_RAISON_PHASES",
                    "TCMD_SP_SUIVI_PHASES",
                    "TCMD_DOC_DOCUMENTS",
                    "TCMD_DA_DEMANDES_ASSOCIEES",
                    "TDC_DEMANDES_COMMANDES",

                    "TCMD_COMMANDES",
                    "TS_SCENARIOS",
                    "TE_ETATS",
                    "TEM_ETAT_MASTERS",
            }
                //_dbUniversContext.GetAllTableNames()
                .ForEach(x =>
                {
                    switch (x.ToLower())
                    {
                        case "tc_categories":
                        case "tcmd_mc_mode_creations":
                        case "tcmd_ph_phases":
                        case "tcmd_rp_raison_phases":
                        case "tcmd_td_type_documents":
                        case "tf_fermes":
                        case "tl_logiciels":
                        case "tle_logiciel_editeurs":
                        case "tlem_logiciel_editeur_modeles":
                        case "tr_mel_email_templates":
                        case "tpm_params":
                        case "tpu_paralleleus":
                        case "tpuf_paralleleu_fermes":
                        case "tpup_paralleleu_params":
                        case "tqm_qualif_modeles":
                        case "tras_auth_scenarios":
                        case "trccl_catalog_claims":
                        case "trcl_claims":
                        case "trcli_clientapplications":
                        case "trclicl_clientapplications_claims":
                        case "tr_lng_languages":
                        case "trst_statuts":
                        case "trtz_tzs":
                        case "tru_users":
                        case "trucl_users_claims":
                        case "tsl_serveur_logiciels":
                        case "tsp_serveur_params":
                        case "tsrv_serveurs":
                        case "tr_wst_website_settings":
                        case "tp_perimetres":
                        case "tpr_profils":
                            // Nothing is done....
                            break;

                        default:

                            if (x.ToLower() == "tcmd_commandes")
                                _dbUniversContext.Database.ExecuteSqlRaw($"DELETE FROM `{x}` where tcmd_origineid > 0;");

                            _dbUniversContext.Database.ExecuteSqlRaw($"DELETE FROM `{x}`; delete from sqlite_sequence where name='{x}';");
                            break;
                    }
                });
            await _dbUniversContext.Database.ExecuteSqlRawAsync("VACUUM;");

            result = true;
        }
        catch (Exception ex)
        {
            _iLogger.LogError(null, ex);
            throw;
        }

        return await ValueTask.FromResult(result);
    }

    public async ValueTask<string> OrkestraNodeWorkerFileNameGet() => await ValueTask.FromResult(updateFileName);

    public async ValueTask<bool> OrkestraNodeWorkerUpdateCheck(Version fileVersion, DateTime fileCreationDateUtc)
    {
        var result = false;

        var fileUpdatePath = Path.Combine(Globals.AssemblyDirectory, "App_Data", "Update", updateFileName);
        if (!File.Exists(fileUpdatePath)) return result;

        var updateVersionInfo = FileVersionInfo.GetVersionInfo(fileUpdatePath);
        var fileInfo = new FileInfo(fileUpdatePath);

        var updateVersion = new Version(updateVersionInfo.FileVersion);
        if (updateVersionInfo != null && fileInfo != null)
            result = (updateVersion != fileVersion) || (fileInfo.CreationTimeUtc > fileCreationDateUtc);

        return await ValueTask.FromResult(result);
    }

    public ValueTask<FileStream> OrkestraNodeWorkerGetUpdate()
    {
        var fileUpdate = Path.Combine(Globals.AssemblyDirectory, "App_Data", "Update", updateFileName);

        if (File.Exists(fileUpdate))
            return ValueTask.FromResult(new FileStream(fileUpdate, FileMode.Open));

        throw new Exception($"{fileUpdate} to sent not there...");
    }

    public async ValueTask<UpdateDetails> OrkestraWebSiteUpdateCheck(Version version)
    {
        var result = new UpdateDetails();

        var assembly = Assembly.GetEntryAssembly();
        if (assembly != null)
        {
            var currentVersion = assembly.GetName().Version;

            if (currentVersion != version)
            {
                result.UpdateVersionNumber = currentVersion;
                result.IsUpdateAvailable = true;
            }
        }

        return await ValueTask.FromResult(result);
    }

    public async ValueTask<TechnicalDetails> OrkestraTechnicalDetails()
    {
        var result = new TechnicalDetails();

        //var dbInfo = new DatabaseInfo();
        //dbInfo.DbPath = _dbEtqContext.Database.GetConnectionString();
        //result.DatabaseInfoList.Add(dbInfo);

        return await ValueTask.FromResult(result);
    }

}

public static class Extensions
{
    public static List<string> GetAllTableNames(this DbContext dbContext)
        => dbContext.Model
            .GetEntityTypes()
            .Select(t => t.GetTableName())
            .Distinct()
            .ToList();
}
