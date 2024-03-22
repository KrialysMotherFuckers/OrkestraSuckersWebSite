using System.ComponentModel.DataAnnotations;

/// <summary>
// vue pour IHM DTF afficher les planifs recurrentes des UTD 
/// </summary>
namespace Krialys.Data.EF.Univers;

public class VPD_PLANIF_DETAILS
{

    [Key]
    [Display(Name = "Display_VPD_PLANIF_DETAILS_TPF_PLANIFID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TPF_PLANIFID { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_TPF_CRON", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TPF_CRON { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_TPF_DATE_DEBUT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime TPF_DATE_DEBUT { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_TPF_DATE_FIN", ResourceType = typeof(Resources.DataAnnotationsResources))]
    [DisplayFormat(DataFormatString = "g")]
    public DateTime? TPF_DATE_FIN { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_TPF_DEMANDE_ORIGINEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TPF_DEMANDE_ORIGINEID { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_TPF_PREREQUIS_DELAI_CHK", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TPF_PREREQUIS_DELAI_CHK { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_TPF_TIMEZONE_INFOID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TPF_TIMEZONE_INFOID { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_PLANIF_STATUTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string PLANIF_STATUTID { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_TRU_DECLARANTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TRU_DECLARANTID { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_DECLARANT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string DECLARANT { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_TD_DEMANDEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TD_DEMANDEID { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_TS_SCENARIOID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TS_SCENARIOID { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_TD_COMMENTAIRE_UTILISATEUR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TD_COMMENTAIRE_UTILISATEUR { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_NB_RESSOURCES", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int NB_RESSOURCES { get; set; }

    [Display(Name = "Display_VPD_PLANIF_DETAILS_SCENARIO_STATUTID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string SCENARIO_STATUTID { get; set; }


}
