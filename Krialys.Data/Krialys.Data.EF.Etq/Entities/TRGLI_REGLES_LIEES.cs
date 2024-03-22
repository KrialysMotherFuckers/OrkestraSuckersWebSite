using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Krialys.Data.EF.Etq;


[Index(nameof(TRGLRV_REGLES_VALEURID), nameof(TRGLRV_REGLES_VALEURLIEEID), Name = "UK_TRGLI_REGLES_LIEES", IsUnique = true)]

public partial class TRGLI_REGLES_LIEES
{
    public TRGLI_REGLES_LIEES()
    {
    }

    [Key]
    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TRGLI_REGLES_LIEES_TRGL_REGLELIEEID", ResourceType = typeof(DataAnnotationsResources))]

    public int TRGL_REGLELIEEID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TRGLI_REGLES_LIEES_TRGLRV_REGLES_VALEURID", ResourceType = typeof(DataAnnotationsResources))]

    public int TRGLRV_REGLES_VALEURID { get; set; }

    [Required(ErrorMessageResourceName = "REQUIRED", ErrorMessageResourceType = typeof(DataAnnotationsResources))]
    [Display(Name = "Display_TRGLI_REGLES_LIEES_TRGLRV_REGLES_VALEURLIEEID", ResourceType = typeof(DataAnnotationsResources))]

    public int TRGLRV_REGLES_VALEURLIEEID { get; set; }

    //ForeignKey

    [ForeignKey(nameof(TRGLRV_REGLES_VALEURID))]
    [InverseProperty(nameof(TRGLRV_REGLES_VALEURS.TRGLI_REGLES_LIEES_SRC))]
    public virtual TRGLRV_REGLES_VALEURS TRGLRV_REGLES_VALEUR { get; set; }

    [ForeignKey("TRGLRV_REGLES_VALEURLIEEID")]
    [InverseProperty(nameof(TRGLRV_REGLES_VALEURS.TRGLI_REGLES_LIEES_CIBLE))]
    public virtual TRGLRV_REGLES_VALEURS TRGLRV_REGLES_VALEUR_CIBLE { get; set; }

}