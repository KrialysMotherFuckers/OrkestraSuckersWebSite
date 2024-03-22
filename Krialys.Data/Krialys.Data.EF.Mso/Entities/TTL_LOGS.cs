// Attention : ne pas oublier de re/mettre les 'DisplayNameLocalized'
using System.ComponentModel.DataAnnotations;

namespace Krialys.Data.EF.Mso;

public partial class TTL_LOGS
{
    [Key]
    [Display(Name = "Display_TTL_LOGS_TTL_LOGID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TTL_LOGID { get; set; }

    [Display(Name = "Display_TTL_LOGS_TRA_ATTENDUID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TRA_ATTENDUID { get; set; }

    [MaxLength(255)]
    [StringLength(255)]
    [Display(Name = "Display_TTL_LOGS_TRA_CODE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TRA_CODE { get; set; }

    [Display(Name = "Display_TTL_LOGS_TTED_DEMANDEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TTED_DEMANDEID { get; set; }

    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_TTL_LOGS_TTL_DATE_DEBUT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime? TTL_DATE_DEBUT { get; set; }

    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_TTL_LOGS_TTL_DATE_FIN", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime? TTL_DATE_FIN { get; set; }

    [MaxLength(255)]
    [StringLength(255)]
    [Display(Name = "Display_TTL_LOGS_TTL_RESULTAT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TTL_RESULTAT { get; set; }

    [MaxLength(255)]
    [StringLength(255)]
    [Display(Name = "Display_TTL_LOGS_TTL_INFO", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TTL_INFO { get; set; }

    [Display(Name = "Display_TTL_LOGS_TTL_CODE_ANOMALIE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TTL_CODE_ANOMALIE { get; set; }

    [Display(Name = "Display_TTL_LOGS_TTL_GROUPEID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TTL_GROUPEID { get; set; }

    [MaxLength(255)]
    [StringLength(255)]
    [Display(Name = "Display_TTL_LOGS_TTL_FICHIER_SOURCE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TTL_FICHIER_SOURCE { get; set; }

    [Display(Name = "Display_TTL_LOGS_TTL_NB_LIGNES_ENTREE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TTL_NB_LIGNES_ENTREE { get; set; }

    [Display(Name = "Display_TTL_LOGS_TTL_NB_LIGNES_SORTIE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TTL_NB_LIGNES_SORTIE { get; set; }

    [Display(Name = "Display_TTL_LOGS_TTL_TAILLE_FICHIER_ENTREE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TTL_TAILLE_FICHIER_ENTREE { get; set; }

    [Display(Name = "Display_TTL_LOGS_TTL_TAILLE_FICHIER_SORTIE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int? TTL_TAILLE_FICHIER_SORTIE { get; set; }

    [DisplayFormat(DataFormatString = "g")]
    [Display(Name = "Display_TTL_LOGS_TTL_FICHIER_DATE_MODIF", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public DateTime? TTL_FICHIER_DATE_MODIF { get; set; }

    [MaxLength(255)]
    [StringLength(255)]
    [Display(Name = "Display_TTL_LOGS_TTL_FICHIER_ACTEUR_MODIF", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TTL_FICHIER_ACTEUR_MODIF { get; set; }

    [MaxLength(255)]
    [StringLength(255)]
    [Display(Name = "Display_TTL_LOGS_TRC_CONTRAT_CODE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TRC_CONTRAT_CODE { get; set; }

    [MaxLength(4000)]
    [StringLength(4000)]
    [Display(Name = "Display_TTL_LOGS_TTL_DYNAMIC_OBJECT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TTL_DYNAMIC_OBJECT { get; set; }
}