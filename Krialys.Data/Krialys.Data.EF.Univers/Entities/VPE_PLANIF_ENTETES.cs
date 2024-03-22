using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
// vue pour IHM DTF afficher les UTD ayant au moins une planification récurrente
/// </summary>
namespace Krialys.Data.EF.Univers;

public class VPE_PLANIF_ENTETES
{

    [Key]
    [Display(Name = "Display_VPE_PLANIF_ENTETES_TS_SCENARIOID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TS_SCENARIOID { get; set; }


    [Display(Name = "Display_VPE_PLANIF_ENTETES_CATEGORIE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string CATEGORIE { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_TEM_NOM_ETAT_MASTER", ResourceType = typeof(Resources.DataAnnotationsResources))]

    public string TEM_NOM_ETAT_MASTER { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_TE_NOM_ETAT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TE_NOM_ETAT { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_TE_NOM_ETAT_VERSION", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TE_NOM_ETAT_VERSION { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_VERSION", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string VERSION { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_TE_COMMENTAIRE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TE_COMMENTAIRE { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_STATUT_ETAT_FR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string STATUT_ETAT_FR { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_STATUT_ETAT_EN", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string STATUT_ETAT_EN { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_TS_NOM_SCENARIO", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TS_NOM_SCENARIO { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_TS_DESCR", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string TS_DESCR { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_REFERENT", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string REFERENT { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_REFERENT_TECH", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string REFERENT_TECH { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_TE_ETATID", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public int TE_ETATID { get; set; }

    [Display(Name = "Display_VPE_PLANIF_ENTETES_ELIGIBLE", ResourceType = typeof(Resources.DataAnnotationsResources))]
    public string ELIGIBLE { get; set; }


    [ForeignKey(nameof(TS_SCENARIOID))]
    [InverseProperty(nameof(TS_SCENARIOS.VPE_PLANIF_ENTETE))]
    public virtual TS_SCENARIOS TS_SCENARIO { get; set; }

}
