using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>

/// </summary>
namespace Krialys.Data.EF.Univers;

public class VDE_DEMANDES_ETENDUES
{

    [Key]
    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_DEMANDEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TD_DEMANDEID { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TE_NOM_ETAT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TE_NOM_ETAT { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TE_NOM_ETAT_VERSION", ResourceType = typeof(Resources.DataAnnotationsResources))]

    public string TE_NOM_ETAT_VERSION { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_VERSION", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string VERSION { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TE_COMMENTAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TE_COMMENTAIRE { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TS_NOM_SCENARIO", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TS_NOM_SCENARIO { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TS_DESCR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TS_DESCR { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_CATEGORIE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string CATEGORIE { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TC_CATEGORIEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TC_CATEGORIEID { get; set; }


    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_STATUT_ETAT_FR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string STATUT_ETAT_FR { get; set; }


    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_STATUT_ETAT_EN", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string STATUT_ETAT_EN { get; set; }


    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_COMMENTAIRE_UTILISATEUR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TD_COMMENTAIRE_UTILISATEUR { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_DATE_DEMANDE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime TD_DATE_DEMANDE { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_DATE_DERNIER_DOWNLOAD", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime? TD_DATE_DERNIER_DOWNLOAD { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_DATE_EXECUTION_SOUHAITEE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime? TD_DATE_EXECUTION_SOUHAITEE { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_DATE_LIVRAISON", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime? TD_DATE_LIVRAISON { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_DATE_PRISE_EN_CHARGE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime? TD_DATE_PRISE_EN_CHARGE { get; set; }

    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_DATE_PIVOT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime? TD_DATE_PIVOT { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_DEMANDE_ORIGINEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TD_DEMANDE_ORIGINEID { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_DUREE_PRODUCTION_REEL", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TD_DUREE_PRODUCTION_REEL { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_INFO_RETOUR_TRAITEMENT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TD_INFO_RETOUR_TRAITEMENT { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_QUALIF_BILAN", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TD_QUALIF_BILAN { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_QUALIF_EXIST_FILE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TD_QUALIF_EXIST_FILE { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_QUALIF_FILE_SIZE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TD_QUALIF_FILE_SIZE { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_RESULT_EXIST_FILE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TD_RESULT_EXIST_FILE { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_RESULT_FILE_SIZE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TD_RESULT_FILE_SIZE { get; set; }


    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_SUSPEND_EXECUTION", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TD_SUSPEND_EXECUTION { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_IGNORE_RESULT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TD_IGNORE_RESULT { get; set; }


    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TD_RESULT_NB_DOWNLOAD", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TD_RESULT_NB_DOWNLOAD { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TSRV_SERVEURID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TSRV_SERVEURID { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TSRV_NOM", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TSRV_NOM { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_REFERENT", ResourceType = typeof(Resources.DataAnnotationsResources))]

    public string REFERENT { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_REFERENT_TECH", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string REFERENT_TECH { get; set; }

    //2022.07.26 rendu optionnel pour afficher les demandes annulées suite à passage de Proto a Brouillon
    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TS_SCENARIOID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TS_SCENARIOID { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TE_ETATID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TE_ETATID { get; set; }


    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TPF_PLANIF_ORIGINEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TPF_PLANIF_ORIGINEID { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TRU_DEMANDEURID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TRU_DEMANDEURID { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_DEMANDEUR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string DEMANDEUR { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_CODE_STATUT_DEMANDE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string CODE_STATUT_DEMANDE { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_STATUT_DEMANDE_FR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string STATUT_DEMANDE_FR { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_STATUT_DEMANDE_EN", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string STATUT_DEMANDE_EN { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_NB_RESSOURCES", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int NB_RESSOURCES { get; set; }

    [Display(Name = "Display_VDE_DEMANDES_ETENDUES_TBD_CODE_RETOUR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TBD_CODE_RETOUR { get; set; }  // code retour provenant du bat, renseigné uniquement si erreur


    [ForeignKey(nameof(TS_SCENARIOID))]
    [InverseProperty(nameof(TS_SCENARIOS.VDE_DEMANDES_ETENDUE))]
    public virtual TS_SCENARIOS TS_SCENARIO { get; set; }



    //public string TD_GUID { get; set; }
    //public int TRU_GESTIONNAIRE_VALIDEURID { get; set; }
    //public string TD_SEND_MAIL_CLIENT { get; set; }
    //public string TD_SEND_MAIL_GESTIONNAIRE { get; set; }
    //public string TD_COMMENTAIRE_GESTIONNAIRE { get; set; }

    //[DisplayFormat(DataFormatString = "g")]
    //public DateTime? TD_DATE_AVIS_GESTIONNAIRE { get; set; }
    //public int TD_PREREQUIS_DELAI_CHK { get; set; }

}
