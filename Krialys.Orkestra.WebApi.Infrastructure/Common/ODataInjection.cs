using Krialys.Common.Extensions;
using Krialys.Data.EF.RefManager;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.FileStorage;
using Krialys.Data.EF.Logs;
using Krialys.Data.EF.Mso;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.Extensions;
using Krialys.Orkestra.WebApi.Services.System;
using Microsoft.OData.ModelBuilder;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Krialys.Orkestra.WebApi.Services.DI;

public static partial class ODataInjection
{
    public static void Add(IServiceCollection services, IMvcBuilder builder, string dbSlot, string dbType)
    {
        // DI
        services.AddTo(builder, dbSlot, dbType, new ODataConventionModelBuilder()
                .With(o =>
                {
                    o.Namespace = typeof(CommonServices).Namespace;
                    //o.EntitySet<XXX>(nameof(XXX));
                })
                .GetEdmModel());
    }

    public static void AddEtqEdmModel(IServiceCollection services, IMvcBuilder builder, string dbSlot, string dbType)
    {
        // DI
        services.AddTo(builder, dbSlot, dbType, new ODataConventionModelBuilder()
                .With(o =>
                {
                    // => REMOVE from the Model [Column(TypeName = "date")] if you have a Column that is a Date to avoid crash on date conversion!
                    o.Namespace = typeof(TETQ_ETIQUETTES).Namespace;

                    // Name all EntitySet
                    o.EntitySet<TETQ_ETIQUETTES>(nameof(TETQ_ETIQUETTES));
                    o.EntitySet<TOBJE_OBJET_ETIQUETTES>(nameof(TOBJE_OBJET_ETIQUETTES));
                    //o.EntitySet<TPRS_PROCESSUS>(nameof(TPRS_PROCESSUS));
                    //o.EntitySet<TTPR_TYPE_PROCESSUS>(nameof(TTPR_TYPE_PROCESSUS));
                    o.EntitySet<TPRCP_PRC_PERIMETRES>(nameof(TPRCP_PRC_PERIMETRES));
                    o.EntitySet<TOBF_OBJ_FORMATS>(nameof(TOBF_OBJ_FORMATS));
                    o.EntitySet<TOBN_OBJ_NATURES>(nameof(TOBN_OBJ_NATURES));
                    o.EntitySet<TEQC_ETQ_CODIFS>(nameof(TEQC_ETQ_CODIFS));
                    o.EntitySet<TSEQ_SUIVI_EVENEMENT_ETQS>(nameof(TSEQ_SUIVI_EVENEMENT_ETQS));
                    o.EntitySet<TTE_TYPE_EVENEMENTS>(nameof(TTE_TYPE_EVENEMENTS));
                    o.EntitySet<ETQ_TM_AET_Authorization>(nameof(ETQ_TM_AET_Authorization));

                    o.EntitySet<TSR_SUIVI_RESSOURCES>(nameof(TSR_SUIVI_RESSOURCES));
                    o.EntitySet<TTR_TYPE_RESSOURCES>(nameof(TTR_TYPE_RESSOURCES));

                    o.EntitySet<TACT_ACTIONS>(nameof(TACT_ACTIONS));
                    o.EntitySet<TRGL_REGLES>(nameof(TRGL_REGLES));
                    o.EntitySet<TRGLI_REGLES_LIEES>(nameof(TRGLI_REGLES_LIEES));
                    o.EntitySet<TRGLRV_REGLES_VALEURS>(nameof(TRGLRV_REGLES_VALEURS));
                    o.EntitySet<TOBJR_OBJET_REGLES>(nameof(TOBJR_OBJET_REGLES));
                    o.EntitySet<TETQR_ETQ_REGLES>(nameof(TETQR_ETQ_REGLES));

                    o.EntitySet<TTDOM_TYPE_DOMAINES>(nameof(TTDOM_TYPE_DOMAINES));
                    o.EntitySet<TDOM_DOMAINES>(nameof(TDOM_DOMAINES));

                    // HOME
                    o.EntitySet<VACCGET_ACCUEIL_GRAPHE_ETQS>(nameof(VACCGET_ACCUEIL_GRAPHE_ETQS));

                    //o.EntitySet<__EFMigrationsHistory>(nameof(__EFMigrationsHistory));
                })
                .GetEdmModel());
    }

    public static void AddLogsEdmModel(IServiceCollection services, IMvcBuilder builder, string dbSlot, string dbType)
    {
        // ETL Logs
        services.TryAddScoped<ILogsProcessingUnitServices, LogsProcessingUnitServices>();

        // DI
        services.AddTo(builder, dbSlot, dbType, new ODataConventionModelBuilder()
                .With(o =>
                {
                    // => REMOVE from the Model [Column(TypeName = "date")] if you have a Column that is a Date to avoid crash on date conversion!
                    o.Namespace = typeof(TM_LOG_Logs).Namespace;

                    o.EntitySet<TM_LOG_Logs>(nameof(TM_LOG_Logs));
                })
                .GetEdmModel());
    }

    public static void AddFileStorageEdmModel(IServiceCollection services, IMvcBuilder builder, string dbSlot, string dbType)
    {
        // DI
        services.AddTo(builder, dbSlot, dbType, new ODataConventionModelBuilder()
                .With(o =>
                {
                    // => REMOVE from the Model [Column(TypeName = "date")] if you have a Column that is a Date to avoid crash on date conversion!
                    o.Namespace = typeof(TM_STF_StorageFileRequest).Namespace;

                    o.EntitySet<TR_SCT_StreamCategoryType>(nameof(TR_SCT_StreamCategoryType));
                    o.EntitySet<TM_STF_StorageFileRequest>(nameof(TM_STF_StorageFileRequest));
                })
                .GetEdmModel());
    }

    public static void AddMsoEdmModel(IServiceCollection services, IMvcBuilder builder, string dbSlot, string dbType)
    {
        // DI
        services.AddTo(builder, dbSlot, dbType, new ODataConventionModelBuilder()
                .With(o =>
                {
                    // => REMOVE from the Model [Column(TypeName = "date")] if you have a Column that is a Date to avoid crash on date conversion!
                    o.Namespace = typeof(TRAPL_APPLICATIONS).Namespace;

                    o.EntitySet<TRAPL_APPLICATIONS>(nameof(TRAPL_APPLICATIONS));
                    o.EntitySet<TRA_ATTENDUS>(nameof(TRA_ATTENDUS));
                    o.EntitySet<TRAP_ATTENDUS_PLANIFS>(nameof(TRAP_ATTENDUS_PLANIFS));
                    o.EntitySet<TRC_CRITICITES>(nameof(TRC_CRITICITES));
                    o.EntitySet<TRNF_NATURES_FLUX>(nameof(TRNF_NATURES_FLUX));
                    o.EntitySet<TRNT_NATURES_TRAITEMENTS>(nameof(TRNT_NATURES_TRAITEMENTS));
                    o.EntitySet<TRP_PLANIFS>(nameof(TRP_PLANIFS));
                    o.EntitySet<TRR_RESULTATS>(nameof(TRR_RESULTATS));
                    o.EntitySet<TRTT_TECHNOS_TRAITEMENTS>(nameof(TRTT_TECHNOS_TRAITEMENTS));
                    o.EntitySet<TTL_LOGS>(nameof(TTL_LOGS));
                    o.EntitySet<TRC_CONTRATS>(nameof(TRC_CONTRATS));
                })
                .GetEdmModel());
    }

    public static void AddRefManagerEdmModel(IServiceCollection services, IMvcBuilder builder, string dbSlot, string dbType)
    {
        // DI
        services.AddTo(builder, dbSlot, dbType, new ODataConventionModelBuilder()
                .With(o =>
                {
                    // => REMOVE from the Model [Column(TypeName = "date")] if you have a Column that is a Date to avoid crash on date conversion!
                    o.Namespace = typeof(TM_RFS_ReferentialSettings).Namespace;

                    o.EntitySet<TM_RFS_ReferentialSettings>(nameof(TM_RFS_ReferentialSettings));
                    o.EntitySet<TR_CNX_Connections>(nameof(TR_CNX_Connections));
                })
                .GetEdmModel());
    }

    public static void AddUniversEdmModel(IServiceCollection services, IMvcBuilder builder, string dbSlot, string dbType)
    {
        // DI
        services.AddTo(builder, dbSlot, dbType, new ODataConventionModelBuilder()
                .With(o =>
                {
                    // => REMOVE from the Model [Column(TypeName = "date")] if you have a Column that is a Date to avoid crash on date conversion!
                    o.Namespace = typeof(TRCLI_CLIENTAPPLICATIONS).Namespace;

                    // Habilitations
                    o.EntitySet<TRCL_CLAIMS>(nameof(TRCL_CLAIMS));
                    o.EntitySet<TRCLI_CLIENTAPPLICATIONS>(nameof(TRCLI_CLIENTAPPLICATIONS));
                    o.EntitySet<TRCLICL_CLIENTAPPLICATIONS_CLAIMS>(nameof(TRCLICL_CLIENTAPPLICATIONS_CLAIMS));
                    o.EntitySet<TRU_USERS>(nameof(TRU_USERS));
                    o.EntitySet<TRUCL_USERS_CLAIMS>(nameof(TRUCL_USERS_CLAIMS));
                    o.EntitySet<TTAU_AUTHENTIFICATIONS>(nameof(TTAU_AUTHENTIFICATIONS));
                    o.EntitySet<TRCCL_CATALOG_CLAIMS>(nameof(TRCCL_CATALOG_CLAIMS));
                    o.EntitySet<TRAPLAS_APPLICATIONS_AUTH_SCENARIOS>(nameof(TRAPLAS_APPLICATIONS_AUTH_SCENARIOS));
                    o.EntitySet<TRAS_AUTH_SCENARIOS>(nameof(TRAS_AUTH_SCENARIOS));

                    // Orkestra
                    o.EntitySet<TBD_BATCH_DEMANDES>(nameof(TBD_BATCH_DEMANDES));
                    o.EntitySet<TBS_BATCH_SCENARIOS>(nameof(TBS_BATCH_SCENARIOS));
                    o.EntitySet<TC_CATEGORIES>(nameof(TC_CATEGORIES));
                    o.EntitySet<TD_DEMANDES>(nameof(TD_DEMANDES));
                    o.EntitySet<TDQ_DEMANDE_QUALIFS>(nameof(TDQ_DEMANDE_QUALIFS));
                    o.EntitySet<TE_ETATS>(nameof(TE_ETATS));
                    o.EntitySet<TEB_ETAT_BATCHS>(nameof(TEB_ETAT_BATCHS));
                    o.EntitySet<TEL_ETAT_LOGICIELS>(nameof(TEL_ETAT_LOGICIELS));
                    o.EntitySet<TEM_ETAT_MASTERS>(nameof(TEM_ETAT_MASTERS));
                    o.EntitySet<TEMF_ETAT_MASTER_FERMES>(nameof(TEMF_ETAT_MASTER_FERMES));
                    o.EntitySet<TEP_ETAT_PREREQUISS>(nameof(TEP_ETAT_PREREQUISS));
                    o.EntitySet<TER_ETAT_RESSOURCES>(nameof(TER_ETAT_RESSOURCES));
                    o.EntitySet<TF_FERMES>(nameof(TF_FERMES));
                    o.EntitySet<TI_INFOS>(nameof(TI_INFOS));
                    o.EntitySet<TL_LOGICIELS>(nameof(TL_LOGICIELS));
                    o.EntitySet<TLE_LOGICIEL_EDITEURS>(nameof(TLE_LOGICIEL_EDITEURS));
                    o.EntitySet<TLEM_LOGICIEL_EDITEUR_MODELES>(nameof(TLEM_LOGICIEL_EDITEUR_MODELES));
                    o.EntitySet<TP_PERIMETRES>(nameof(TP_PERIMETRES));
                    o.EntitySet<TPD_PREREQUIS_DEMANDES>(nameof(TPD_PREREQUIS_DEMANDES));
                    o.EntitySet<TPF_PLANIFS>(nameof(TPF_PLANIFS));
                    o.EntitySet<TR_WST_WebSite_Settings>(nameof(TR_WST_WebSite_Settings));
                    o.EntitySet<TPR_PROFILS>(nameof(TPR_PROFILS));
                    o.EntitySet<TPS_PREREQUIS_SCENARIOS>(nameof(TPS_PREREQUIS_SCENARIOS));
                    o.EntitySet<TPU_PARALLELEUS>(nameof(TPU_PARALLELEUS));
                    o.EntitySet<TPUF_PARALLELEU_FERMES>(nameof(TPUF_PARALLELEU_FERMES));
                    o.EntitySet<TPUP_PARALLELEU_PARAMS>(nameof(TPUP_PARALLELEU_PARAMS));
                    o.EntitySet<TQM_QUALIF_MODELES>(nameof(TQM_QUALIF_MODELES));
                    o.EntitySet<TRD_RESSOURCE_DEMANDES>(nameof(TRD_RESSOURCE_DEMANDES));
                    o.EntitySet<TR_LNG_Languages>(nameof(TR_LNG_Languages));
                    o.EntitySet<TRS_RESSOURCE_SCENARIOS>(nameof(TRS_RESSOURCE_SCENARIOS));
                    o.EntitySet<TRST_STATUTS>(nameof(TRST_STATUTS));
                    o.EntitySet<TRTZ_TZS>(nameof(TRTZ_TZS));
                    o.EntitySet<TS_SCENARIOS>(nameof(TS_SCENARIOS));
                    o.EntitySet<TSL_SERVEUR_LOGICIELS>(nameof(TSL_SERVEUR_LOGICIELS));
                    o.EntitySet<TSP_SERVEUR_PARAMS>(nameof(TSP_SERVEUR_PARAMS));
                    o.EntitySet<TSRV_SERVEURS>(nameof(TSRV_SERVEURS));
                    o.EntitySet<TDP_DEMANDE_PROCESS>(nameof(TDP_DEMANDE_PROCESS));

                    o.EntitySet<VSCU_CTRL_STRUCTURE_UPLOADS>(nameof(VSCU_CTRL_STRUCTURE_UPLOADS));
                    o.EntitySet<VDE_DEMANDES_ETENDUES>(nameof(VDE_DEMANDES_ETENDUES));
                    o.EntitySet<VDE_DEMANDES_RESSOURCES>(nameof(VDE_DEMANDES_RESSOURCES));

                    o.EntitySet<VPE_PLANIF_ENTETES>(nameof(VPE_PLANIF_ENTETES));
                    o.EntitySet<VPD_PLANIF_DETAILS>(nameof(VPD_PLANIF_DETAILS));

                    // HOME
                    o.EntitySet<VACCGD_ACCUEIL_GRAPHE_DEMANDES>(nameof(VACCGD_ACCUEIL_GRAPHE_DEMANDES));
                    o.EntitySet<VACCGQ_ACCUEIL_GRAPHE_QUALITES>(nameof(VACCGQ_ACCUEIL_GRAPHE_QUALITES));

                    // Habilitations DTF 
                    o.EntitySet<TH_HABILITATIONS>(nameof(TH_HABILITATIONS));
                    o.EntitySet<TSG_SCENARIO_GPES>(nameof(TSG_SCENARIO_GPES));
                    o.EntitySet<TSGA_SCENARIO_GPE_ASSOCIES>(nameof(TSGA_SCENARIO_GPE_ASSOCIES));
                    o.EntitySet<TTE_TEAMS>(nameof(TTE_TEAMS));
                    o.EntitySet<TUTE_USER_TEAMS>(nameof(TUTE_USER_TEAMS));
                    o.EntitySet<VDTFH_HABILITATIONS>(nameof(VDTFH_HABILITATIONS));

                    // CMD
                    o.EntitySet<TCMD_PH_PHASES>(nameof(TCMD_PH_PHASES));
                    o.EntitySet<TCMD_RP_RAISON_PHASES>(nameof(TCMD_RP_RAISON_PHASES));
                    o.EntitySet<TCMD_TD_TYPE_DOCUMENTS>(nameof(TCMD_TD_TYPE_DOCUMENTS));
                    o.EntitySet<TCMD_MC_MODE_CREATIONS>(nameof(TCMD_MC_MODE_CREATIONS));

                    o.EntitySet<TCMD_COMMANDES>(nameof(TCMD_COMMANDES));
                    o.EntitySet<TCMD_CR_CMD_RAISON_PHASES>(nameof(TCMD_CR_CMD_RAISON_PHASES));
                    o.EntitySet<TCMD_DA_DEMANDES_ASSOCIEES>(nameof(TCMD_DA_DEMANDES_ASSOCIEES));
                    o.EntitySet<TCMD_DOC_DOCUMENTS>(nameof(TCMD_DOC_DOCUMENTS));

                    o.EntitySet<TCMD_SP_SUIVI_PHASES>(nameof(TCMD_SP_SUIVI_PHASES));
                    o.EntitySet<TDC_DEMANDES_COMMANDES>(nameof(TDC_DEMANDES_COMMANDES));

                    o.EntitySet<TR_MEL_EMail_Templates>(nameof(TR_MEL_EMail_Templates));

                    // PU
                    o.EntitySet<PARALLELEU>($"${nameof(PARALLELEU)}$");
                })
                .GetEdmModel());
    }
}