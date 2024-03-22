namespace Krialys.Orkestra.Common.Shared;

public static class Univers
{
    #region CALENDAR (DTF)

    /// <summary>
    /// Modèle de Demande (base)
    /// </summary>
    public class ModeleDemandeBase
    {
        public int TD_DEMANDEID { get; set; }
        public int TE_ETATID { get; set; }
        public int? TS_SCENARIOID { get; set; }
        public string TRU_DEMANDEURID { get; set; }
        public int TPF_PLANIFID { get; set; }
        public DateTime TPF_DATE_DEBUT { get; set; }
        public DateTime? TPF_DATE_FIN { get; set; }
        public int TPF_DEMANDE_ORIGINEID { get; set; }

        //specifique IN
        public string TPF_CRON { get; set; }
        public string TPF_TIMEZONE_INFOID { get; set; } // le timezone dans lequel l'heure/minute potentielle a été saisie dans le cron
    }

    /// <summary>
    /// Modèle de Demande
    /// </summary>
    public class ModeleDemande : ModeleDemandeBase
    {
        public string TD_COMMENTAIRE_UTILISATEUR { get; set; }
        public string TD_SEND_MAIL_GESTIONNAIRE { get; set; }
        public string TD_SEND_MAIL_CLIENT { get; set; }
        public int? TPF_PREREQUIS_DELAI_CHK { get; set; }
    }

    /// <summary>
    /// Modèle de Demande (décodage CRON) utilisé pour le calendrier de production
    /// </summary>
    public class ModeleDemandeCalendar : ModeleDemandeBase
    {
        /// <summary>
        /// Eqv. TE_FULLNAME
        /// </summary>
        public string TE_NOM_ETAT_VERSION { get; set; }
        public string TS_NOM_SCENARIO { get; set; }
        public string TRU_DECLARANTID { get; set; }
        public string TRU_DEMANDEUR { get; set; }
        public string TRU_USER_RESP_FONC { get; set; }
        public string TRU_USER_DECLARANT_PLANIF { get; set; }

        /// <summary>
        /// TD_DATE_PRISE_EN_CHARGE 'THEORIQUE'
        /// </summary>
        public DateTime? TD_DATE_PRISE_EN_CHARGE /* THEORIQUE */ { get; set; }
        public bool IS_RECURRENT { get; set; }

        public int? TD_DUREE_PRODUCTION_REEL { get; set; }

        public string CODE_STATUT_DEMANDE { get; set; }

        public string TSRV_NOM { get; set; }

        public int TC_CATEGORIEID { get; set; }
    }

    #endregion
}

#region Order (DTS)
/// <summary>
/// Data needed to change the phase of an order (TCMD_COMMANDES).
/// </summary>
public class ChangeOrderPhaseArguments
{
    // Code of the phase we want to access.
    public string PhaseCode { get; set; }

    // Comment describing the phase change.
    public string Comment { get; set; }

    // Id of the reason why the phase is changed.
    public int ReasonId { get; set; }
}
#endregion

#region ETQ
/// <summary>
/// Data needed to apply authorizations on a label (TETQ_ETIQUETTES).
/// </summary>
public class EtqAuthorizationArguments
{
    /// <summary>
    /// Is label visible by all users?
    /// </summary>
    public bool IsAccessPublic { get; set; }

    /// <summary>
    /// Users for which the authorizations are modified.
    /// </summary>
    public string[] UsersIds { get; set; }

    /// <summary>
    /// If null, don't change authorizations.
    /// If true, give authorization.
    /// If false, revoke authorization.
    /// </summary>
    public bool? Authorize { get; set; }
}

/// <summary>
/// Data used to filter labels (TETQ_ETIQUETTES).
/// </summary>
public class EtqFilters
{
    /// <summary>
    /// Value of searched string.
    /// </summary>
    public string SearchedValue { get; set; }

    /// <summary>
    /// Minimum value for the creation date.
    /// </summary>
    public DateTime? CreationDateMin { get; set; }

    /// <summary>
    /// Maximum value for the creation date.
    /// </summary>
    public DateTime? CreationDateMax { get; set; }

    /// <summary>
    /// Ids of selected modules.
    /// </summary>
    public int[] ModuleIds { get; set; }

    /// <summary>
    /// Id of the selected order.
    /// </summary>
    public int? OrderNumber { get; set; }

    /// <summary>
    /// Ids of selected rule value (TRGLRV_REGLES_VALEURS).
    /// </summary>
    public int[] RuleValueIds { get; set; }

    /// <summary>
    /// Label of the selected action (TACT_LIB).
    /// </summary>
    public string ActionLabel { get; set; }
}
#endregion