using Krialys.Data.EF.Univers;
using Krialys.Orkestra.WebApi.Controllers.Common;
using Krialys.Orkestra.WebApi.Services.Common;

// > Implements GenericCRUDController for TBD_BATCH_DEMANDES
namespace Krialys.Orkestra.WebApi.Controllers.UNIVERS;

/// <summary>
/// This class represents the route to be applied
/// </summary>
internal static class Route { internal const string Name = Litterals.UniversRootPath; }

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TBD_BATCH_DEMANDESController : GenericCrudController<TBD_BATCH_DEMANDES, KrialysDbContext>
{
    public TBD_BATCH_DEMANDESController(GenericCrud<TBD_BATCH_DEMANDES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TBS_BATCH_SCENARIOSController : GenericCrudController<TBS_BATCH_SCENARIOS, KrialysDbContext>
{
    public TBS_BATCH_SCENARIOSController(GenericCrud<TBS_BATCH_SCENARIOS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TC_CATEGORIESController : GenericCrudController<TC_CATEGORIES, KrialysDbContext>
{
    public TC_CATEGORIESController(GenericCrud<TC_CATEGORIES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TD_DEMANDESController : GenericCrudController<TD_DEMANDES, KrialysDbContext>
{
    public TD_DEMANDESController(GenericCrud<TD_DEMANDES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TDQ_DEMANDE_QUALIFSController : GenericCrudController<TDQ_DEMANDE_QUALIFS, KrialysDbContext>
{
    public TDQ_DEMANDE_QUALIFSController(GenericCrud<TDQ_DEMANDE_QUALIFS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TE_ETATSController : GenericCrudController<TE_ETATS, KrialysDbContext>
{
    public TE_ETATSController(GenericCrud<TE_ETATS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TEB_ETAT_BATCHSController : GenericCrudController<TEB_ETAT_BATCHS, KrialysDbContext>
{
    public TEB_ETAT_BATCHSController(GenericCrud<TEB_ETAT_BATCHS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TEL_ETAT_LOGICIELSController : GenericCrudController<TEL_ETAT_LOGICIELS, KrialysDbContext>
{
    public TEL_ETAT_LOGICIELSController(GenericCrud<TEL_ETAT_LOGICIELS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TEM_ETAT_MASTERSController : GenericCrudController<TEM_ETAT_MASTERS, KrialysDbContext>
{
    public TEM_ETAT_MASTERSController(GenericCrud<TEM_ETAT_MASTERS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TEMF_ETAT_MASTER_FERMESController : GenericCrudController<TEMF_ETAT_MASTER_FERMES, KrialysDbContext>
{
    public TEMF_ETAT_MASTER_FERMESController(GenericCrud<TEMF_ETAT_MASTER_FERMES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TEP_ETAT_PREREQUISSController : GenericCrudController<TEP_ETAT_PREREQUISS, KrialysDbContext>
{
    public TEP_ETAT_PREREQUISSController(GenericCrud<TEP_ETAT_PREREQUISS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TER_ETAT_RESSOURCESController : GenericCrudController<TER_ETAT_RESSOURCES, KrialysDbContext>
{
    public TER_ETAT_RESSOURCESController(GenericCrud<TER_ETAT_RESSOURCES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TF_FERMESController : GenericCrudController<TF_FERMES, KrialysDbContext>
{
    public TF_FERMESController(GenericCrud<TF_FERMES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TI_INFOSController : GenericCrudController<TI_INFOS, KrialysDbContext>
{
    public TI_INFOSController(GenericCrud<TI_INFOS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TL_LOGICIELSController : GenericCrudController<TL_LOGICIELS, KrialysDbContext>
{
    public TL_LOGICIELSController(GenericCrud<TL_LOGICIELS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TLE_LOGICIEL_EDITEURSController : GenericCrudController<TLE_LOGICIEL_EDITEURS, KrialysDbContext>
{
    public TLE_LOGICIEL_EDITEURSController(GenericCrud<TLE_LOGICIEL_EDITEURS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TLEM_LOGICIEL_EDITEUR_MODELESController : GenericCrudController<TLEM_LOGICIEL_EDITEUR_MODELES, KrialysDbContext>
{
    public TLEM_LOGICIEL_EDITEUR_MODELESController(GenericCrud<TLEM_LOGICIEL_EDITEUR_MODELES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TP_PERIMETRESController : GenericCrudController<TP_PERIMETRES, KrialysDbContext>
{
    public TP_PERIMETRESController(GenericCrud<TP_PERIMETRES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TPD_PREREQUIS_DEMANDESController : GenericCrudController<TPD_PREREQUIS_DEMANDES, KrialysDbContext>
{
    public TPD_PREREQUIS_DEMANDESController(GenericCrud<TPD_PREREQUIS_DEMANDES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TPF_PLANIFSController : GenericCrudController<TPF_PLANIFS, KrialysDbContext>
{
    public TPF_PLANIFSController(GenericCrud<TPF_PLANIFS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TR_WST_WebSite_SettingsController : GenericCrudController<TR_WST_WebSite_Settings, KrialysDbContext>
{
    public TR_WST_WebSite_SettingsController(GenericCrud<TR_WST_WebSite_Settings, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TPR_PROFILSController : GenericCrudController<TPR_PROFILS, KrialysDbContext>
{
    public TPR_PROFILSController(GenericCrud<TPR_PROFILS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TPS_PREREQUIS_SCENARIOSController : GenericCrudController<TPS_PREREQUIS_SCENARIOS, KrialysDbContext>
{
    public TPS_PREREQUIS_SCENARIOSController(GenericCrud<TPS_PREREQUIS_SCENARIOS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TPU_PARALLELEUSController : GenericCrudController<TPU_PARALLELEUS, KrialysDbContext>
{
    public TPU_PARALLELEUSController(GenericCrud<TPU_PARALLELEUS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TPUF_PARALLELEU_FERMESController : GenericCrudController<TPUF_PARALLELEU_FERMES, KrialysDbContext>
{
    public TPUF_PARALLELEU_FERMESController(GenericCrud<TPUF_PARALLELEU_FERMES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TPUP_PARALLELEU_PARAMSController : GenericCrudController<TPUP_PARALLELEU_PARAMS, KrialysDbContext>
{
    public TPUP_PARALLELEU_PARAMSController(GenericCrud<TPUP_PARALLELEU_PARAMS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TQM_QUALIF_MODELESController : GenericCrudController<TQM_QUALIF_MODELES, KrialysDbContext>
{
    public TQM_QUALIF_MODELESController(GenericCrud<TQM_QUALIF_MODELES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRAPLAS_APPLICATIONS_AUTH_SCENARIOSController : GenericCrudController<TRAPLAS_APPLICATIONS_AUTH_SCENARIOS, KrialysDbContext>
{
    public TRAPLAS_APPLICATIONS_AUTH_SCENARIOSController(GenericCrud<TRAPLAS_APPLICATIONS_AUTH_SCENARIOS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRAS_AUTH_SCENARIOSController : GenericCrudController<TRAS_AUTH_SCENARIOS, KrialysDbContext>
{
    public TRAS_AUTH_SCENARIOSController(GenericCrud<TRAS_AUTH_SCENARIOS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRCCL_CATALOG_CLAIMSController : GenericCrudController<TRCCL_CATALOG_CLAIMS, KrialysDbContext>
{
    public TRCCL_CATALOG_CLAIMSController(GenericCrud<TRCCL_CATALOG_CLAIMS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRCL_CLAIMSController : GenericCrudController<TRCL_CLAIMS, KrialysDbContext>
{
    public TRCL_CLAIMSController(GenericCrud<TRCL_CLAIMS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRCLI_CLIENTAPPLICATIONSController : GenericCrudController<TRCLI_CLIENTAPPLICATIONS, KrialysDbContext>
{
    public TRCLI_CLIENTAPPLICATIONSController(GenericCrud<TRCLI_CLIENTAPPLICATIONS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRCLICL_CLIENTAPPLICATIONS_CLAIMSController : GenericCrudController<TRCLICL_CLIENTAPPLICATIONS_CLAIMS, KrialysDbContext>
{
    public TRCLICL_CLIENTAPPLICATIONS_CLAIMSController(GenericCrud<TRCLICL_CLIENTAPPLICATIONS_CLAIMS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRD_RESSOURCE_DEMANDESController : GenericCrudController<TRD_RESSOURCE_DEMANDES, KrialysDbContext>
{
    public TRD_RESSOURCE_DEMANDESController(GenericCrud<TRD_RESSOURCE_DEMANDES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TR_LNG_LanguagesController : GenericCrudController<TR_LNG_Languages, KrialysDbContext>
{
    public TR_LNG_LanguagesController(GenericCrud<TR_LNG_Languages, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRS_RESSOURCE_SCENARIOSController : GenericCrudController<TRS_RESSOURCE_SCENARIOS, KrialysDbContext>
{
    public TRS_RESSOURCE_SCENARIOSController(GenericCrud<TRS_RESSOURCE_SCENARIOS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRST_STATUTSController : GenericCrudController<TRST_STATUTS, KrialysDbContext>
{
    public TRST_STATUTSController(GenericCrud<TRST_STATUTS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRTZ_TZSController : GenericCrudController<TRTZ_TZS, KrialysDbContext>
{
    public TRTZ_TZSController(GenericCrud<TRTZ_TZS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRU_USERSController : GenericCrudController<TRU_USERS, KrialysDbContext>
{
    public TRU_USERSController(GenericCrud<TRU_USERS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TRUCL_USERS_CLAIMSController : GenericCrudController<TRUCL_USERS_CLAIMS, KrialysDbContext>
{
    public TRUCL_USERS_CLAIMSController(GenericCrud<TRUCL_USERS_CLAIMS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TS_SCENARIOSController : GenericCrudController<TS_SCENARIOS, KrialysDbContext>
{
    public TS_SCENARIOSController(GenericCrud<TS_SCENARIOS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TSL_SERVEUR_LOGICIELSController : GenericCrudController<TSL_SERVEUR_LOGICIELS, KrialysDbContext>
{
    public TSL_SERVEUR_LOGICIELSController(GenericCrud<TSL_SERVEUR_LOGICIELS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TSP_SERVEUR_PARAMSController : GenericCrudController<TSP_SERVEUR_PARAMS, KrialysDbContext>
{
    public TSP_SERVEUR_PARAMSController(GenericCrud<TSP_SERVEUR_PARAMS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TSRV_SERVEURSController : GenericCrudController<TSRV_SERVEURS, KrialysDbContext>
{
    public TSRV_SERVEURSController(GenericCrud<TSRV_SERVEURS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TTAU_AUTHENTIFICATIONSController : GenericCrudController<TTAU_AUTHENTIFICATIONS, KrialysDbContext>
{
    public TTAU_AUTHENTIFICATIONSController(GenericCrud<TTAU_AUTHENTIFICATIONS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TDP_DEMANDE_PROCESSController : GenericCrudController<TDP_DEMANDE_PROCESS, KrialysDbContext>
{
    public TDP_DEMANDE_PROCESSController(GenericCrud<TDP_DEMANDE_PROCESS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class VSCU_CTRL_STRUCTURE_UPLOADSController : GenericCrudController<VSCU_CTRL_STRUCTURE_UPLOADS, KrialysDbContext>
{
    public VSCU_CTRL_STRUCTURE_UPLOADSController(GenericCrud<VSCU_CTRL_STRUCTURE_UPLOADS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class VDE_DEMANDES_ETENDUESController : GenericCrudController<VDE_DEMANDES_ETENDUES, KrialysDbContext>
{
    public VDE_DEMANDES_ETENDUESController(GenericCrud<VDE_DEMANDES_ETENDUES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class VDE_DEMANDES_RESSOURCESController : GenericCrudController<VDE_DEMANDES_RESSOURCES, KrialysDbContext>
{
    public VDE_DEMANDES_RESSOURCESController(GenericCrud<VDE_DEMANDES_RESSOURCES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class VPE_PLANIF_ENTETESController : GenericCrudController<VPE_PLANIF_ENTETES, KrialysDbContext>
{
    public VPE_PLANIF_ENTETESController(GenericCrud<VPE_PLANIF_ENTETES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class VPD_PLANIF_DETAILSController : GenericCrudController<VPD_PLANIF_DETAILS, KrialysDbContext>
{
    public VPD_PLANIF_DETAILSController(GenericCrud<VPD_PLANIF_DETAILS, KrialysDbContext> genericService) : base(genericService) { }
}

// ACCUEIL
[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class VACCGD_ACCUEIL_GRAPHE_DEMANDESController : GenericCrudController<VACCGD_ACCUEIL_GRAPHE_DEMANDES, KrialysDbContext>
{
    public VACCGD_ACCUEIL_GRAPHE_DEMANDESController(GenericCrud<VACCGD_ACCUEIL_GRAPHE_DEMANDES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class VACCGQ_ACCUEIL_GRAPHE_QUALITESController : GenericCrudController<VACCGQ_ACCUEIL_GRAPHE_QUALITES, KrialysDbContext>
{
    public VACCGQ_ACCUEIL_GRAPHE_QUALITESController(GenericCrud<VACCGQ_ACCUEIL_GRAPHE_QUALITES, KrialysDbContext> genericService) : base(genericService) { }
}

//Habilitations DTF D
[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TH_HABILITATIONSController : GenericCrudController<TH_HABILITATIONS, KrialysDbContext>
{
    public TH_HABILITATIONSController(GenericCrud<TH_HABILITATIONS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TSG_SCENARIO_GPESController : GenericCrudController<TSG_SCENARIO_GPES, KrialysDbContext>
{
    public TSG_SCENARIO_GPESController(GenericCrud<TSG_SCENARIO_GPES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TSGA_SCENARIO_GPE_ASSOCIESController : GenericCrudController<TSGA_SCENARIO_GPE_ASSOCIES, KrialysDbContext>
{
    public TSGA_SCENARIO_GPE_ASSOCIESController(GenericCrud<TSGA_SCENARIO_GPE_ASSOCIES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TTE_TEAMSController : GenericCrudController<TTE_TEAMS, KrialysDbContext>
{
    public TTE_TEAMSController(GenericCrud<TTE_TEAMS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TUTE_USER_TEAMSController : GenericCrudController<TUTE_USER_TEAMS, KrialysDbContext>
{
    public TUTE_USER_TEAMSController(GenericCrud<TUTE_USER_TEAMS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class VDTFH_HABILITATIONSController : GenericCrudController<VDTFH_HABILITATIONS, KrialysDbContext>
{
    public VDTFH_HABILITATIONSController(GenericCrud<VDTFH_HABILITATIONS, KrialysDbContext> genericService) : base(genericService) { }
}

//Habilitations DTF F
//CMD D
[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TCMD_PH_PHASESController : GenericCrudController<TCMD_PH_PHASES, KrialysDbContext>
{
    public TCMD_PH_PHASESController(GenericCrud<TCMD_PH_PHASES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TCMD_RP_RAISON_PHASESController : GenericCrudController<TCMD_RP_RAISON_PHASES, KrialysDbContext>
{
    public TCMD_RP_RAISON_PHASESController(GenericCrud<TCMD_RP_RAISON_PHASES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TCMD_TD_TYPE_DOCUMENTSController : GenericCrudController<TCMD_TD_TYPE_DOCUMENTS, KrialysDbContext>
{
    public TCMD_TD_TYPE_DOCUMENTSController(GenericCrud<TCMD_TD_TYPE_DOCUMENTS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TCMD_MC_MODE_CREATIONSController : GenericCrudController<TCMD_MC_MODE_CREATIONS, KrialysDbContext>
{
    public TCMD_MC_MODE_CREATIONSController(GenericCrud<TCMD_MC_MODE_CREATIONS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TCMD_COMMANDESController : GenericCrudController<TCMD_COMMANDES, KrialysDbContext>
{
    public TCMD_COMMANDESController(GenericCrud<TCMD_COMMANDES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TCMD_CR_CMD_RAISON_PHASESController : GenericCrudController<TCMD_CR_CMD_RAISON_PHASES, KrialysDbContext>
{
    public TCMD_CR_CMD_RAISON_PHASESController(GenericCrud<TCMD_CR_CMD_RAISON_PHASES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TCMD_DA_DEMANDES_ASSOCIEESController : GenericCrudController<TCMD_DA_DEMANDES_ASSOCIEES, KrialysDbContext>
{
    public TCMD_DA_DEMANDES_ASSOCIEESController(GenericCrud<TCMD_DA_DEMANDES_ASSOCIEES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TCMD_DOC_DOCUMENTSController : GenericCrudController<TCMD_DOC_DOCUMENTS, KrialysDbContext>
{
    public TCMD_DOC_DOCUMENTSController(GenericCrud<TCMD_DOC_DOCUMENTS, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TCMD_SP_SUIVI_PHASESController : GenericCrudController<TCMD_SP_SUIVI_PHASES, KrialysDbContext>
{
    public TCMD_SP_SUIVI_PHASESController(GenericCrud<TCMD_SP_SUIVI_PHASES, KrialysDbContext> genericService) : base(genericService) { }
}

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Route.Name)]
public class TDC_DEMANDES_COMMANDESController : GenericCrudController<TDC_DEMANDES_COMMANDES, KrialysDbContext>
{
    public TDC_DEMANDES_COMMANDESController(GenericCrud<TDC_DEMANDES_COMMANDES, KrialysDbContext> genericService) : base(genericService) { }
}

//[ApiController]
//[Route("[controller]")]
//public class TR_MEL_EMail_TemplatesController : GenericController<TR_MEL_EMail_Templates, KrialysDbContext>
//{
//    public TR_MEL_EMail_TemplatesController(IGenericService<TR_MEL_EMail_Templates, KrialysDbContext> genericService) : base(genericService) { }
//}