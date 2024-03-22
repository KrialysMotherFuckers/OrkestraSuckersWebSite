using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Krialys.Data.EF.Etq;

[Index(nameof(TOBJE_OBJET_ETIQUETTEID), nameof(TRGL_REGLEID), Name = "UK_TOBJR_OBJET_REGLES", IsUnique = true)]

public partial class TOBJR_OBJET_REGLES
{
    public TOBJR_OBJET_REGLES()
    {
    }

    [Key]
    [Display(Name = "Display_TOBJR_OBJET_REGLES_TOBJR_OBJET_REGLEID", ResourceType = typeof(DataAnnotationsResources))]
    public int TOBJR_OBJET_REGLEID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TOBJR_OBJET_REGLES_TOBJE_OBJET_ETIQUETTEID", ResourceType = typeof(DataAnnotationsResources))]
    public int TOBJE_OBJET_ETIQUETTEID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TOBJR_OBJET_REGLES_TRGL_REGLEID", ResourceType = typeof(DataAnnotationsResources))]
    public int TRGL_REGLEID { get; set; }


    // a alimenter avec la valeur par défaut de la regle 

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TOBJR_OBJET_REGLES_TRGLRV_REGLES_VALEURID", ResourceType = typeof(DataAnnotationsResources))]
    public int TRGLRV_REGLES_VALEURID { get; set; }



    [StringLength(1)]
    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TOBJR_OBJET_REGLES_TOBJR_APPLICABLE", ResourceType = typeof(DataAnnotationsResources))]
    public string TOBJR_APPLICABLE { get; set; }

    //on autorise soit null soit une valeur positive
    //"Est attendu soit aucune valeur soit une valeur >0"
    [Range(1, int.MaxValue, ErrorMessageResourceName = "Display_TOBJR_OBJET_REGLES_TOBJR_ECHEANCE_DUREE_RANGE", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TOBJR_OBJET_REGLES_TOBJR_ECHEANCE_DUREE", ResourceType = typeof(DataAnnotationsResources))]
    public int? TOBJR_ECHEANCE_DUREE { get; set; }


    //ForeignKey

    [ForeignKey(nameof(TOBJE_OBJET_ETIQUETTEID))]
    [InverseProperty("TOBJR_OBJET_REGLES")]
    public virtual TOBJE_OBJET_ETIQUETTES TOBJE_OBJET_ETIQUETTE { get; set; }

    [ForeignKey(nameof(TRGL_REGLEID))]
    [InverseProperty("TOBJR_OBJET_REGLES")]
    public virtual TRGL_REGLES TRGL_REGLES { get; set; }


    [ForeignKey(nameof(TRGLRV_REGLES_VALEURID))]
    [InverseProperty(nameof(TRGLRV_REGLES_VALEURS.TOBJR_OBJET_REGLE))]
    public virtual TRGLRV_REGLES_VALEURS TRGLRV_REGLES_VALEUR { get; set; }

}