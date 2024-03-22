using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.System;
using Microsoft.Extensions.Configuration;

namespace Krialys.Orkestra.WebApi.Services.Data;

public interface IVersionServices : IScopedService
{
    ValueTask<bool> Duplicate(int dpuIdToDuplicate);
}

public class VersionServices : IVersionServices
{
    #region Injected services

    private readonly KrialysDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ICommonServices _commonServices;
    private readonly IFileServices _fileServices;

    public VersionServices(KrialysDbContext dbContext, IConfiguration configuration, ICommonServices commonServices, IFileServices fileServices)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _commonServices = commonServices;
        _fileServices = fileServices;
    }

    private class TS_SCENARIOSToDuplicate : TS_SCENARIOS
    {
        public int TS_SCENARIOID_New { get; set; }
    }

    private class TEB_ETAT_BATCHS_ToDuplicate : TEB_ETAT_BATCHS { public int TEB_ETAT_BATCHID_New { get; set; } }
    private class TBS_BATCH_SCENARIOS_ToDuplicate : TBS_BATCH_SCENARIOS { public int TEB_ETAT_BATCHID_New { get; set; } }

    private class TER_ETAT_RESSOURCES_ToDuplicate : TER_ETAT_RESSOURCES { public int TER_ETAT_RESSOURCEID_New { get; set; } }
    private class TRS_RESSOURCE_SCENARIOS_ToDuplicate : TRS_RESSOURCE_SCENARIOS { public int TER_ETAT_RESSOURCEID_New { get; set; } }

    private class TEP_ETAT_PREREQUISS_ToDuplicate : TEP_ETAT_PREREQUISS { public int TEP_ETAT_PREREQUISID_New { get; set; } }
    private class TPS_PREREQUIS_SCENARIOS_ToDuplicate : TPS_PREREQUIS_SCENARIOS { public int TEP_ETAT_PREREQUISID_New { get; set; } }

    public ValueTask<List<TEM_ETAT_MASTERS>> GetAllJobs() => ValueTask.FromResult(_dbContext.TEM_ETAT_MASTERS.ToList());

    public async ValueTask<bool> Duplicate(int dpuIdToDuplicate)
    {
        bool result = false;
        bool canSaveEmptyEnvironement = false;
        bool canSaveResourcesEnvironement = false;
        var newEtatId = 0;
        var toDuplicate = _dbContext.TE_ETATS.FirstOrDefault(x => x.TE_ETATID == dpuIdToDuplicate);
        if (toDuplicate == null) throw new Exception($"Record not found [TE_ETATID: {dpuIdToDuplicate}]");

        var user = _commonServices.GetUserIdAndName();
        using (var transaction = _dbContext.Database.BeginTransaction())
            try
            {
                #region Db Record dulication

                var oldEtatId = toDuplicate.TE_ETATID;
                newEtatId = _dbContext.TE_ETATS.Any() ? _dbContext.TE_ETATS.Max(x => x.TE_ETATID) + 1 : 1;
                toDuplicate.TE_ETATID = newEtatId;
                toDuplicate.TE_DATE_DERNIERE_PRODUCTION = null;
                toDuplicate.TE_DATE_REVISION = DateExtensions.GetUtcNow();
                toDuplicate.TE_GUID = Guid.NewGuid().ToString("N");
                toDuplicate.TRST_STATUTID = StatusLiteral.Draft;
                toDuplicate.TRU_DECLARANTID = user.userId;
                _dbContext.TE_ETATS.Add(toDuplicate);

                var newEtatLogicielsId = _dbContext.TEL_ETAT_LOGICIELS.Any() ? _dbContext.TEL_ETAT_LOGICIELS.Max(x => x.TEL_ETAT_LOGICIELID) : 0;
                _dbContext.TEL_ETAT_LOGICIELS.Where(x => x.TE_ETATID == oldEtatId)
                    .ToList()
                    .ForEach(x =>
                    {
                        x.TEL_ETAT_LOGICIELID = ++newEtatLogicielsId;
                        x.TE_ETATID = newEtatId;
                        _dbContext.TEL_ETAT_LOGICIELS.Add(x);
                    });
                _dbContext.SaveChanges();

                var TS_SCENARIOSToDuplicateList =
                            (
                                from scen in _dbContext.TS_SCENARIOS
                                where scen.TE_ETATID == oldEtatId
                                select new TS_SCENARIOSToDuplicate
                                {
                                    TS_SCENARIOID = scen.TS_SCENARIOID,
                                    TE_ETATID = scen.TE_ETATID,
                                    TS_GUID = scen.TS_GUID,
                                    TRST_STATUTID = scen.TRST_STATUTID,
                                    TS_DESCR = scen.TS_DESCR,
                                    TS_IS_DEFAULT = scen.TS_IS_DEFAULT,
                                    TS_NOM_SCENARIO = scen.TS_NOM_SCENARIO
                                }
                            ).ToList();

                if (TS_SCENARIOSToDuplicateList.Any())
                {
                    #region TS_SCENARIOS Duplication

                    var currScenariosId = _dbContext.TS_SCENARIOS.Any() ? _dbContext.TS_SCENARIOS.Max(x => x.TS_SCENARIOID) : 0;
                    var currHabilitationId = _dbContext.TH_HABILITATIONS.Any() ? _dbContext.TH_HABILITATIONS.Max(x => x.TH_HABILITATIONID) : 0;
                    var currScenGroupId = _dbContext.TSGA_SCENARIO_GPE_ASSOCIES.Any() ? _dbContext.TSGA_SCENARIO_GPE_ASSOCIES.Max(x => x.TSGA_SCENARIO_GPE_ASSOCIEID) : 0;

                    TS_SCENARIOSToDuplicateList.ForEach(scen =>
                    {
                        currScenariosId++;

                        scen.TS_SCENARIOID_New = currScenariosId;

                        _dbContext.TS_SCENARIOS.Add(
                                                new TS_SCENARIOS
                                                {
                                                    TS_SCENARIOID = currScenariosId,
                                                    TE_ETATID = newEtatId,
                                                    TS_GUID = Guid.NewGuid().ToString("N"),
                                                    TRST_STATUTID = StatusLiteral.Draft,
                                                    TS_DESCR = scen.TS_DESCR,
                                                    TS_IS_DEFAULT = scen.TS_IS_DEFAULT,
                                                    TS_NOM_SCENARIO = scen.TS_NOM_SCENARIO
                                                });
                        var res = AddHabilitationToScenario(scen.TS_SCENARIOID, currScenariosId, currHabilitationId, currScenGroupId, user.userId);
                        currHabilitationId = res.CurrHabilitationId;
                        currScenGroupId = res.CurrScenGroupId;
                    });
                    _dbContext.SaveChanges();

                    #endregion

                    #region TEB_ETAT_BATCHS Duplication

                    var TEB_ETAT_BATCHS_ToDuplicateList =
                                (
                                    from batch in _dbContext.TEB_ETAT_BATCHS
                                    where batch.TE_ETATID == oldEtatId //&& batch.TRST_STATUTID == StatusLiteral.Available
                                    select new TEB_ETAT_BATCHS_ToDuplicate
                                    {
                                        TEB_ETAT_BATCHID = batch.TEB_ETAT_BATCHID
                                    }
                                ).ToList();

                    var TBS_BATCH_SCENARIOS_ToDuplicateList =
                                (
                                    from batch in _dbContext.TEB_ETAT_BATCHS
                                    join scenario in _dbContext.TBS_BATCH_SCENARIOS on batch.TEB_ETAT_BATCHID equals scenario.TEB_ETAT_BATCHID
                                    where batch.TE_ETATID == oldEtatId //&& batch.TRST_STATUTID == StatusLiteral.Available
                                    select new TBS_BATCH_SCENARIOS_ToDuplicate
                                    {
                                        TEB_ETAT_BATCHID = scenario.TEB_ETAT_BATCHID,
                                        TS_SCENARIOID = scenario.TS_SCENARIOID,
                                        TBS_ORDRE_EXECUTION = scenario.TBS_ORDRE_EXECUTION
                                    }
                                ).ToList();

                    if (TEB_ETAT_BATCHS_ToDuplicateList.Any())
                    {
                        var currEtatBatchsId = _dbContext.TEB_ETAT_BATCHS.Any() ? _dbContext.TEB_ETAT_BATCHS.Max(x => x.TEB_ETAT_BATCHID) : 0;
                        TEB_ETAT_BATCHS_ToDuplicateList.ForEach(batch =>
                        {
                            var etatBatch = _dbContext.TEB_ETAT_BATCHS.FirstOrDefault(x => x.TEB_ETAT_BATCHID == batch.TEB_ETAT_BATCHID);
                            if (etatBatch != null)
                            {
                                currEtatBatchsId++;
                                TBS_BATCH_SCENARIOS_ToDuplicateList.Where(y => y.TEB_ETAT_BATCHID == batch.TEB_ETAT_BATCHID).ToList().ForEach(x => x.TEB_ETAT_BATCHID_New = currEtatBatchsId);

                                etatBatch.TEB_ETAT_BATCHID = currEtatBatchsId;
                                etatBatch.TE_ETATID = newEtatId;
                                etatBatch.TEB_DATE_CREATION = DateExtensions.GetUtcNow();
                                _dbContext.TEB_ETAT_BATCHS.Add(etatBatch);
                            }
                        });
                        _dbContext.SaveChanges();
                    }

                    if (TBS_BATCH_SCENARIOS_ToDuplicateList.Any())
                    {
                        TBS_BATCH_SCENARIOS_ToDuplicateList.ForEach(batchScen =>
                        {
                            _dbContext.TBS_BATCH_SCENARIOS.Add(new TBS_BATCH_SCENARIOS
                            {
                                TEB_ETAT_BATCHID = batchScen.TEB_ETAT_BATCHID_New,
                                TS_SCENARIOID = TS_SCENARIOSToDuplicateList.FirstOrDefault(x => x.TS_SCENARIOID == batchScen.TS_SCENARIOID).TS_SCENARIOID_New,
                                TBS_ORDRE_EXECUTION = batchScen.TBS_ORDRE_EXECUTION
                            });
                        });
                        _dbContext.SaveChanges();
                    }

                    #endregion

                    #region TER_ETAT_RESSOURCES Duplication

                    var TER_ETAT_RESSOURCES_ToDuplicateList =
                                (
                                    from ress in _dbContext.TER_ETAT_RESSOURCES
                                    where ress.TE_ETATID == oldEtatId //&& batch.TRST_STATUTID == StatusLiteral.Available
                                    select new TER_ETAT_RESSOURCES_ToDuplicate
                                    {
                                        TER_ETAT_RESSOURCEID = ress.TER_ETAT_RESSOURCEID
                                    }
                                ).ToList();

                    var TRS_RESSOURCE_SCENARIOS_ToDuplicateList =
                                (
                                    from res in _dbContext.TER_ETAT_RESSOURCES
                                    join scenario in _dbContext.TRS_RESSOURCE_SCENARIOS on res.TER_ETAT_RESSOURCEID equals scenario.TER_ETAT_RESSOURCEID
                                    where res.TE_ETATID == oldEtatId
                                    select new TRS_RESSOURCE_SCENARIOS_ToDuplicate
                                    {
                                        TER_ETAT_RESSOURCEID = scenario.TER_ETAT_RESSOURCEID,
                                        TS_SCENARIOID = scenario.TS_SCENARIOID,
                                        TRS_COMMENTAIRE = scenario.TRS_COMMENTAIRE,
                                        TRS_FICHIER_OBLIGATOIRE = scenario.TRS_FICHIER_OBLIGATOIRE
                                    }
                                ).ToList();

                    if (TER_ETAT_RESSOURCES_ToDuplicateList.Any())
                    {
                        var currEtatRessourcesId = _dbContext.TER_ETAT_RESSOURCES.Any() ? _dbContext.TER_ETAT_RESSOURCES.Max(x => x.TER_ETAT_RESSOURCEID) : 0;
                        TER_ETAT_RESSOURCES_ToDuplicateList.ForEach(res =>
                        {
                            var etatRes = _dbContext.TER_ETAT_RESSOURCES.FirstOrDefault(x => x.TER_ETAT_RESSOURCEID == res.TER_ETAT_RESSOURCEID);
                            if (etatRes != null)
                            {
                                currEtatRessourcesId++;
                                TRS_RESSOURCE_SCENARIOS_ToDuplicateList.Where(y => y.TER_ETAT_RESSOURCEID == etatRes.TER_ETAT_RESSOURCEID).ToList().ForEach(x => x.TER_ETAT_RESSOURCEID_New = currEtatRessourcesId);

                                etatRes.TER_ETAT_RESSOURCEID = currEtatRessourcesId;
                                etatRes.TE_ETATID = newEtatId;
                                _dbContext.TER_ETAT_RESSOURCES.Add(etatRes);
                            }
                        });
                        _dbContext.SaveChanges();
                    }

                    if (TRS_RESSOURCE_SCENARIOS_ToDuplicateList.Any())
                    {
                        TRS_RESSOURCE_SCENARIOS_ToDuplicateList.ToList().ForEach(ressScen =>
                        {
                            _dbContext.TRS_RESSOURCE_SCENARIOS.Add(new TRS_RESSOURCE_SCENARIOS
                            {
                                TER_ETAT_RESSOURCEID = ressScen.TER_ETAT_RESSOURCEID_New,
                                TS_SCENARIOID = TS_SCENARIOSToDuplicateList.FirstOrDefault(x => x.TS_SCENARIOID == ressScen.TS_SCENARIOID).TS_SCENARIOID_New,
                                TRS_COMMENTAIRE = ressScen.TRS_COMMENTAIRE,
                                TRS_FICHIER_OBLIGATOIRE = ressScen.TRS_FICHIER_OBLIGATOIRE
                            });
                        });
                        _dbContext.SaveChanges();
                    }

                    #endregion

                    #region TEP_ETAT_PREREQUISS Duplication

                    var TEP_ETAT_PREREQUISS_ToDuplicateList =
                                (
                                    from prereq in _dbContext.TEP_ETAT_PREREQUISS
                                    where prereq.TE_ETATID == oldEtatId //&& batch.TRST_STATUTID == StatusLiteral.Available
                                    select new TEP_ETAT_PREREQUISS_ToDuplicate
                                    {
                                        TEP_ETAT_PREREQUISID = prereq.TEP_ETAT_PREREQUISID
                                    }
                                ).ToList();

                    var TPS_PREREQUIS_SCENARIOS_ToDuplicateList =
                                (
                                    from required in _dbContext.TEP_ETAT_PREREQUISS
                                    join scenario in _dbContext.TPS_PREREQUIS_SCENARIOS on required.TEP_ETAT_PREREQUISID equals scenario.TS_SCENARIOID
                                    where required.TE_ETATID == oldEtatId //&& required.TRST_STATUTID == StatusLiteral.Available
                                    select new TPS_PREREQUIS_SCENARIOS_ToDuplicate
                                    {
                                        TEP_ETAT_PREREQUISID = scenario.TEP_ETAT_PREREQUISID,
                                        TS_SCENARIOID = scenario.TS_SCENARIOID,
                                        TPS_NB_FILE_MIN = scenario.TPS_NB_FILE_MIN,
                                        TPS_NB_FILE_MAX = scenario.TPS_NB_FILE_MAX,
                                        TPS_COMMENTAIRE = scenario.TPS_COMMENTAIRE
                                    }
                                ).ToList();

                    if (TEP_ETAT_PREREQUISS_ToDuplicateList.Any())
                    {
                        var currEtatPreRequisId = _dbContext.TEP_ETAT_PREREQUISS.Any() ? _dbContext.TEP_ETAT_PREREQUISS.Max(x => x.TEP_ETAT_PREREQUISID) : 0;
                        TER_ETAT_RESSOURCES_ToDuplicateList.ForEach(etatRes =>
                        {
                            var etatRequired = _dbContext.TEP_ETAT_PREREQUISS.FirstOrDefault(x => x.TEP_ETAT_PREREQUISID == etatRes.TER_ETAT_RESSOURCEID);
                            if (etatRequired != null)
                            {
                                currEtatPreRequisId++;
                                TPS_PREREQUIS_SCENARIOS_ToDuplicateList.Where(y => y.TEP_ETAT_PREREQUISID == etatRes.TER_ETAT_RESSOURCEID).ToList().ForEach(x => x.TEP_ETAT_PREREQUISID_New = currEtatPreRequisId);

                                etatRequired.TEP_ETAT_PREREQUISID = currEtatPreRequisId;
                                etatRequired.TE_ETATID = newEtatId;
                                etatRequired.TEP_DATE_MAJ = DateExtensions.GetUtcNow();
                                _dbContext.TEP_ETAT_PREREQUISS.Add(etatRequired);
                            }
                        });
                        _dbContext.SaveChanges();
                    }

                    if (TPS_PREREQUIS_SCENARIOS_ToDuplicateList.Any())
                    {
                        TPS_PREREQUIS_SCENARIOS_ToDuplicateList.ToList().ForEach(batchScen =>
                        {
                            _dbContext.TPS_PREREQUIS_SCENARIOS.Add(new TPS_PREREQUIS_SCENARIOS
                            {
                                TEP_ETAT_PREREQUISID = batchScen.TEP_ETAT_PREREQUISID_New,
                                TS_SCENARIOID = TS_SCENARIOSToDuplicateList.FirstOrDefault(x => x.TS_SCENARIOID == batchScen.TS_SCENARIOID).TS_SCENARIOID_New,
                                TPS_NB_FILE_MIN = batchScen.TPS_NB_FILE_MIN,
                                TPS_NB_FILE_MAX = batchScen.TPS_NB_FILE_MAX,
                                TPS_COMMENTAIRE = batchScen.TPS_COMMENTAIRE,
                            });
                        });
                        _dbContext.SaveChanges();
                    }

                    #endregion
                }

                #endregion

                #region File & Folder Duplication

                string etatOldFileName = $"E{oldEtatId.ToString().PadLeft(6, '0')}.zip";
                string etatNewFileName = $"E{newEtatId.ToString().PadLeft(6, '0')}.zip";

                //if (!File.Exists(Path.Combine(_configuration.GetValue<string>("ParallelU:PathEnvVierge"), etatOldFileName))) 
                //    throw new Exception($"File {etatOldFileName} not found");

                string etatNewFolderName = $"{newEtatId.ToString().PadLeft(6, '0')}";

                if (File.Exists(Path.Combine(_configuration.GetValue<string>("ParallelU:PathEnvVierge"), etatOldFileName)))
                {
                    File.Copy(
                            Path.Combine(_configuration.GetValue<string>("ParallelU:PathEnvVierge"), etatOldFileName),
                            Path.Combine(_configuration.GetValue<string>("ParallelU:PathEnvVierge"), etatNewFileName),
                            true);

                    // Remove directory
                    var dir = Path.Combine(_configuration.GetValue<string>("ParallelU:PathEnvVierge"), $"E{etatNewFolderName}");
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);

                    canSaveEmptyEnvironement = true;
                }

                if (Directory.Exists(Path.Combine(_configuration.GetValue<string>("ParallelU:PathRessourceModele"), etatOldFileName)))
                {
                    new DirectoryInfo(Path.Combine(_configuration.GetValue<string>("ParallelU:PathRessourceModele"), etatNewFolderName)).Delete(true);

                    new DirectoryInfo(Path.Combine(_configuration.GetValue<string>("ParallelU:PathRessourceModele"), etatOldFileName))
                        .DeepCopy(Path.Combine(_configuration.GetValue<string>("ParallelU:PathRessourceModele"), etatNewFolderName));

                    // TODO: Save new resources files FileStorage db
                    canSaveResourcesEnvironement = true;
                }

                #endregion

                transaction.Commit();
                result = true;
            }
            catch //(Exception ex)
            {
                transaction.Rollback();

                // TODO: Log error in @StdLogException
            }

        // Save new empty environnement to FileStorage db
        if (result && canSaveEmptyEnvironement)
        {
            var envId = $"E{newEtatId.ToString().PadLeft(6, '0')}";
            if (_fileServices.CreateDirectoryBaseandFolders(envId, true, ""))
                result = await _fileServices.EmptyEnvironmentValidation(envId);
        }

        // TODO: Save new resources files FileStorage db
        //if (result && canSaveResourcesEnvironement)
        //    result = await _fileServices...;

        return result;
    }

    private (int CurrHabilitationId, int CurrScenGroupId) AddHabilitationToScenario(int oldScenarioId, int newScenarioId, int currHabilitationId, int currScenGroupId, string userId)
    {
        var now = DateExtensions.GetUtcNow();

        _dbContext.TH_HABILITATIONS.Where(x => x.TS_SCENARIOID == oldScenarioId && x.TRST_STATUTID == StatusLiteral.Available)
            .ToList()
            .ForEach(x =>
            {
                currHabilitationId++;

                _dbContext.TH_HABILITATIONS.Add(
                    new TH_HABILITATIONS
                    {
                        TH_COMMENTAIRE = x.TH_COMMENTAIRE,
                        TH_DATE_INITIALISATION = now,
                        TH_DROIT_CONCERNE = x.TH_DROIT_CONCERNE,
                        TH_EST_HABILITE = x.TH_EST_HABILITE,
                        TH_HABILITATIONID = currHabilitationId,
                        TH_HERITE_HABILITATIONID = x.TH_HERITE_HABILITATIONID,
                        TH_MAJ_DATE = now,
                        TRST_STATUTID = x.TRST_STATUTID,
                        TRU_INITIALISATION_AUTEURID = x.TRU_INITIALISATION_AUTEURID,
                        TRU_MAJ_AUTEURID = userId,
                        TSG_SCENARIO_GPEID = x.TSG_SCENARIO_GPEID,
                        TS_SCENARIOID = newScenarioId,
                        TTE_TEAMID = x.TTE_TEAMID,
                        TRU_USERID = x.TRU_USERID
                    });
            });

        _dbContext.TSGA_SCENARIO_GPE_ASSOCIES.Where(x => x.TS_SCENARIOID == oldScenarioId && x.TRST_STATUTID == StatusLiteral.Available)
            .ToList()
            .ForEach(x =>
            {
                currScenGroupId++;

                _dbContext.TSGA_SCENARIO_GPE_ASSOCIES.Add(
                    new TSGA_SCENARIO_GPE_ASSOCIES
                    {
                        TRST_STATUTID = x.TRST_STATUTID,
                        TSGA_COMMENTAIRE = x.TSGA_COMMENTAIRE,
                        TSGA_DATE_CREATION = now,
                        TSGA_SCENARIO_GPE_ASSOCIEID = currScenGroupId,
                        TSG_SCENARIO_GPEID = x.TSG_SCENARIO_GPEID,
                        TS_SCENARIOID = newScenarioId
                    });
            });

        return (currHabilitationId, currScenGroupId);
    }

    #endregion
}
